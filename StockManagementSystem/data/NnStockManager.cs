using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace data
{
    /// <summary>
    /// 用于对数据库的操作以及维护等工作
    /// </summary>
    public class NnStockManager
    {
        private OleDbConnection m_connection;// 数据库
        private Configuration m_configuration;// 配置文件读写
        private StreamWriter m_writer;// 用于写入数据到文件
        string m_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\nnns\";

        private int initcount = 0;

        //弹出通知窗口（通知窗口有两种，一种自动消失（表示提示,isWarning为false），一种需要手动关闭（表示信息重要，必须引起客户重视，isWarning为true））
        public delegate void NnShowMessage(string message, bool isWarning);

        public NnShowMessage ShowMessage { get; set; }

        /// <summary>
        /// NnStockManager是否初始化正常并且可用
        /// </summary>
        public bool IsValid { get; set; }

        public NnStockManager(NnShowMessage nnShow)
        {
            ShowMessage += nnShow;
            init();
        }

        /// <summary>
        /// 数据库文件所在路径
        /// </summary>
        public string DatabasePath { get; set; }

        // 验证密码
        public bool IsPassed(string name, string md5)
        {
            string sql = $"select * from _user where _user='{name}'";
            try
            {
                using(OleDbDataReader reader = _executeReader(sql))
                {
                    if (reader.Read())
                    {
                        return md5 == (reader.GetString(reader.GetOrdinal("passwors")));
                    }
                }
            }
            catch { }
            return false;
        }

        // 添加用户
        public int AddUser(string name,string md5)
        {
            string sql = $"insert into _user (_user,password) values('{name}','{md5}')";
            try
            {
                return _exeCuteNonQuery(sql);
            }
            catch { return 0; }
        }

        /// <summary>
        /// 导出所有的可用坐标
        /// </summary>
        /// <returns></returns>
        public void OutputCoordinate()
        {
            StringBuilder sb = new StringBuilder();
            string sql = $"select * from coordinate";
            try
            {
                using (OleDbDataReader reader = _executeReader(sql))
                {
                    while (reader.Read())
                    {
                        try
                        {
                            string plate = reader["plate"] as string;
                            string coo = reader["coo"] as string;
                            int i = coo.IndexOf(','), j = 0;
                            while (i > 0)
                            {
                                sb.Append(plate).Append('-').Append(coo.Substring(j, i - j)).Append(",\n");
                                j = i + 1;
                                i = coo.IndexOf(',', j);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
            _writeToFileAndOpen(sb.ToString());
        }

        // 用于写入文件并且打开文件
        private void _writeToFileAndOpen(string str)
        {
            try
            {
                string path = m_path + DateTime.Now.Ticks + ".csv";
                DirectoryInfo dir = new DirectoryInfo(m_path);
                if (!dir.Exists) dir.Create();
                m_writer = new StreamWriter(path, false, Encoding.UTF8);

                m_writer.Write(str);
                m_writer.Flush();
                m_writer.Close();
                System.Diagnostics.Process.Start(path);

            }
            catch { if (ShowMessage != null) ShowMessage("文件写入失败", true); }
        }

        // 用于搜索
        internal string Search(string str)
        {
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            sb1.Append("KeyWord,AddDate,WorkNo,OrderID,Qualit,Coordinate,Purity,Mw,\n");
            sb2.Append("\n\n已被移除项目：\nKeyWord,RemoveDate,AddDate,WorkNo,OrderID,Qualit,Coordinate,Purity,Mw,Cause,\n");
            str += str.EndsWith("\n") ? "" : "\n";
            int i = str.IndexOf('\n'), j = 0;
            while (i > 0)
            {
                string s = str.Substring(j, i - j);
                _getSearchString(s, sb1, sb2);
                j = i + 1;
                i = str.IndexOf('\n', j);
            }
            string result = sb1.Append(sb2).ToString();
            _writeToFileAndOpen(result);
            return result.Replace(',', '\t');
        }

        private void _getSearchString(string s, StringBuilder sb1, StringBuilder sb2)
        {
            s = s.Trim();
            int flg = s.IndexOf('-');// flg小于0则认为是workNo，在(0,6]之间为坐标，大于6为OrderId
            if (flg < 0)// 如果是workNo
            {
                sb1.Append(_getStringFromCoordinate(s, "stock_new", "workNo"));
                sb2.Append(_getStringFromCoordinate(s, "stock_old", "workNo"));
            }else if (flg < 7)// 如果是坐标
            {
                sb1.Append(_getStringFromCoordinate("'" + s + "'", "stock_new", "coordinate"));
            }
            else// 如果是orderId
            {
                sb1.Append(_getStringFromCoordinate("'" + s + "'", "stock_new", "orderId"));
                sb2.Append(_getStringFromCoordinate("'" + s + "'", "stock_old", "orderId"));
            }
        }

        private string _getStringFromCoordinate(string s,string stockname, string key)
        {
            StringBuilder sb = new StringBuilder();
            bool isNew = stockname == "stock_new";// 标明这个是在新表里查还是旧表里查
            string sql = $"select * from {stockname} where {key}={s}";
            try
            {
                using (OleDbDataReader reader = _executeReader(sql))
                {
                    while (reader.Read())
                    {
                        NnStock stock = _getNnStcokFromReader(reader, isNew);
                        sb.Append(s.Replace("'", "")).Append(',');
                        if (!isNew)
                            sb.Append(stock.DateRemove.ToShortDateString()).Append(',');
                        sb.Append(stock.ToString());
                        if (!isNew)
                            sb.Append(stock.Cause).Append(',');
                        sb.Append("\n");
                    }
                }
            }
            catch { }
            if (isNew && sb.Length < 2) sb.Append(s.Replace("'", "")).Append(",,无记录！,").Append('\n');// 如果没有记录则添加提示字段
            return sb.ToString();
        }

        private NnStock _getNnStcokFromReader(OleDbDataReader reader,bool isNew)
        {
            NnStock stock = new NnStock();
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal("orderId");
                if (!reader.IsDBNull(ordinal))
                    stock.OrderId = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("workNo");
                if (!reader.IsDBNull(ordinal))
                    stock.WorkNo = reader.GetInt32(ordinal);
                ordinal = reader.GetOrdinal("quality");
                if (!reader.IsDBNull(ordinal))
                    stock.Quality = reader.GetDouble(ordinal);
                ordinal = reader.GetOrdinal("purity");
                if (!reader.IsDBNull(ordinal))
                    stock.Purity = reader.GetDouble(ordinal);
                ordinal = reader.GetOrdinal("mw");
                if (!reader.IsDBNull(ordinal))
                    stock.Mw = (double)reader["mw"];
                if (isNew)
                {
                    ordinal = reader.GetOrdinal("_date");
                    if (!reader.IsDBNull(ordinal))
                        stock.DateAdd = reader.GetDateTime(ordinal);
                    ordinal = reader.GetOrdinal("coordinate");
                    if (!reader.IsDBNull(ordinal))
                        stock.Coordinate = reader.GetString(ordinal);
                }
                else
                {
                    ordinal = reader.GetOrdinal("dateAdd");
                    if (!reader.IsDBNull(ordinal))
                        stock.DateAdd = reader.GetDateTime(ordinal);
                    ordinal = reader.GetOrdinal("dateRemove");
                    if (!reader.IsDBNull(ordinal))
                        stock.DateRemove = reader.GetDateTime(ordinal);
                    ordinal = reader.GetOrdinal("cause");
                    if (!reader.IsDBNull(ordinal))
                        stock.Cause = reader.GetString(ordinal);
                }
            }
            catch { }
            return stock;
        }

        // 删除自己创建的无用文件
        private void _deleteOtherFile()
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(m_path);
                foreach (FileInfo finfo in info.GetFiles())
                {
                    try
                    {
                        finfo.Delete();
                    }
                    catch { }
                }
            }
            catch { }
        }

        /// <summary>
        /// 将更改提交到数据库
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public int Submit(NnStock stock)
        {
            switch (stock.State)
            {
                case NnStock.StockState.Insert:// 添加
                return _addIntoDatabase(stock);
                case NnStock.StockState.Update:// 更新
                    return _updateForDatabase(stock);
                case NnStock.StockState.Delete:// 移除
                return _removeFromDatabase(stock);
                default:return 0;
            }
        }

        // 更新数据库数据
        private int _updateForDatabase(NnStock stock)
        {
            int count = 0;
            try
            {
                string sql = $"select * from stock_new where coordinate='{stock.Coordinate}'";
                using (OleDbDataReader reader = _executeReader(sql))
                {
                    if (reader.Read())
                    {
                        NnStock sk = _getNnStcokFromReader(reader, true);
                        sk.Purity = stock.Purity;
                        sk.Mw = stock.Mw;
                        if (stock.Quality > 0)
                            sk.Quality = stock.Quality;
                        sql = $"update stock_new set quality={sk.Quality},purity={sk.Purity},mw={sk.Mw} where coordiante='{sk.Coordinate}'";
                        return _exeCuteNonQuery(sql);
                    }
                    else return 0;
                }
            }
            catch { }
            return count;
        }

        // 加入数据库
        private int _addIntoDatabase(NnStock stock)
        {
            string sql = $"insert into stock_new ([_date],workNo,orderId,quality,coordinate,purity,mw) " +
                $"values('{DateTime.Now}',{stock.WorkNo},'{stock.OrderId}',{stock.Quality},'{stock.Coordinate}',{stock.Purity},{stock.Mw})";
            int count = 0;
            try
            {
                count = _exeCuteNonQuery(sql);
                _updateCoordinate(stock.Coordinate, true);
            }
            catch (Exception e){ Console.WriteLine(e.ToString()); }
            return count;
        }

        // 从数据库移除
        private int _removeFromDatabase(NnStock stock)
        {
            int count = 0;
            try
            {
                string sql = $"select * from stock_new where coordinate='{stock.Coordinate}'";
                using (OleDbDataReader reader = _executeReader(sql))
                {
                    if (reader.Read())
                    {
                        NnStock sk = _getNnStcokFromReader(reader, true);
                        sql = $"insert into stock_old (dateAdd,dateRemove,workNo,orderId,quality,purity,mw,cause) " +
                            $"values('{sk.DateAdd}','{DateTime.Now}',{sk.WorkNo},'{sk.OrderId}',{sk.Quality},{sk.Purity},{sk.Mw},'{stock.Cause}')";

                        count = _exeCuteNonQuery(sql);
                        if (count > 0)
                        {
                            sql = $"delete from stock_new where coordinate='{stock.Coordinate}'";
                            int i = _exeCuteNonQuery(sql);
                            _updateCoordinate(stock.Coordinate, false);
                        }
                    }
                    else
                        return 0;
                }
            }
            catch(Exception e){ Console.WriteLine(e.ToString()); return 0; }
            return count;
        }
        
        /// <summary>
        /// 更新数据库坐标
        /// </summary>
        /// <param name="coordinate">坐标</param>
        /// <param name="isRemove">是否移除坐标</param>
        private void _updateCoordinate(string coordinate,bool isRemove)
        {
            int index = coordinate.IndexOf('-');
            if (index < 0) return;
            string plate = coordinate.Substring(0, index);
            string place = coordinate.Substring(index + 1, coordinate.Length - index - 1);
            string sql = $"select * from coordinate where plate = '{plate}'";
            using (OleDbDataReader reader = _executeReader(sql))
            {
                if (reader.Read())
                {
                    string newplace;
                    if (isRemove)
                        newplace = (reader["coo"] as string).Replace(place + ",", "");
                    else
                        newplace = _getNewPlace(reader["coo"] as string, place);
                    _exeCuteNonQuery($"update coordinate set coo = '{newplace}' where plate = '{plate}'");
                }
                else if (!isRemove)
                {
                    _exeCuteNonQuery($"insert into coordinate (plate,coo) values('{plate}','{place},')");
                }

            }
        }

        // 获取排序后的新字符串，注意，这里的排序有瑕疵（根据字符串排序，以后改进）
        private string _getNewPlace(string v, string place)
        {
            StringBuilder buder = new StringBuilder();
            bool isInserted = false;
            int i, j = 0;
            while (true)
            {
                i = v.IndexOf(',', j);
                if (i < 0) break;
                string str = v.Substring(j, i - j);
                if (!isInserted && place.CompareTo(str) < 0)
                {
                    buder.Append(place).Append(',');
                    isInserted = true;
                }
                buder.Append(str).Append(',');
                j = i + 1;
            }
            if (!isInserted)
                buder.Append(place).Append(',');
            return buder.ToString();
        }

        // 执行数据库操作
        private int _exeCuteNonQuery(string sql)
        {
            using(OleDbCommand cmd = m_connection.CreateCommand())
            {
                cmd.CommandText = sql;
                return cmd.ExecuteNonQuery();
            }
        }

        // 从数据库读取数据
        private OleDbDataReader _executeReader(string sql)
        {
            using(OleDbCommand cmd = m_connection.CreateCommand())
            {
                cmd.CommandText = sql;
                return cmd.ExecuteReader();
            }
        }

        private void init()
        {
            ++initcount;
            IsValid = true;
            m_configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string connectionStr = null;
            // 如果第一次初始化失败，第二次初始化时直接从网络获取数据库连接字符串
            if (initcount < 2 && m_configuration.AppSettings.Settings["stock_connection"] != null)
            {
                connectionStr = m_configuration.AppSettings.Settings["stock_connection"].Value;
            }
            else
            {
                connectionStr = GetStringFromService("stock_connection");
                if (connectionStr == null)
                {
                    if (ShowMessage != null)
                        ShowMessage("配置文件错误！", true);
                    IsValid = false;
                    return;
                }
            }
            try
            {
                m_connection = new OleDbConnection(connectionStr);
                m_connection.Open();
                int i = connectionStr.IndexOf("Data Source=") + 12;
                int len = connectionStr.IndexOf(";", i) - i;
                len = len > 0 ? len : connectionStr.Length - i;
                DatabasePath = connectionStr.Substring(i, len);
            }
            catch
            {
                if (initcount < 2)
                    init();
                else
                {
                    if (ShowMessage != null)
                        ShowMessage("数据库连接错误！建议检查数据库文件是否被改动。", true);
                    IsValid = false;
                    return;
                }
            }
        }

        // 从网络获取配置信息
        private string GetStringFromService(string v)
        {
            try
            {
                string result = NnConnection.GetString(v);
                if (result == null) return null;
                if (m_configuration.AppSettings.Settings[v] == null)
                    m_configuration.AppSettings.Settings.Add(v, result);
                else
                    m_configuration.AppSettings.Settings[v].Value = result;
                m_configuration.Save();
                return result;
            }
            catch { Console.WriteLine("获取网络数据错误"); return null; }
        }

        ~NnStockManager()
        {
            _deleteOtherFile();
            try
            {
                m_connection.Dispose();
                m_connection.Close();
            }
            catch { }
        }
    }
}
