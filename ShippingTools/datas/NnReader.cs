using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShippingTools
{
    class NnReader
    {
        private static NnReader mReader;
        private static readonly object locker = new object();

        private MySqlConnection mConnection;
        public static NnReader Instance
        {
            get
            {
                if (mReader == null)
                {
                    lock (locker)
                    {
                        if (mReader == null)
                        {
                            mReader = new NnReader();
                        }
                    }
                }
                else if (mReader.mConnection.State != System.Data.ConnectionState.Open)
                {
                    try
                    {
                        mReader.mConnection.Open();
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); NnMessage.ShowMessage("数据库错误！", true); }
                }
                return mReader;
            }
        }

        private NnReader()
        {
            try
            {
                string key = ConfigurationManager.AppSettings["dbkey"];
#if (DEBUG)
                string connstring = $"{ConfigurationManager.AppSettings["dbpath_d"]}{(string.IsNullOrEmpty(key) ? "" : "password=" + NnDecrypt(key))}";
#else
                string connstring = $"{ConfigurationManager.AppSettings["dbpath"]}{(string.IsNullOrEmpty(key) ? "" : "password=" + NnDecrypt(key))}";
#endif
                mConnection = new MySqlConnection(connstring);
                mConnection.Open();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); NnMessage.ShowMessage("数据库错误！", true); }
        }
        // ------------数据处理------------
        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal List<Order> UploadPyrolysisLabel(List<Order> list)
        {
            List<Order> ll = new List<Order>();
            List<Order> lFreeze = new List<Order>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand("INSERT INTO weighingtool (SerialNumber,WorkNo,OrderId,Quality,Mw,Exterior,LotNo,PackagingRequirements,Purity,DataCollation,Label,DateLabel,Release,DateTelease,Comments) VALUES(@v1,@v2,@v3,@v4,@v5,@v6,@v7,@v8,@v9,@v10,@v11,@v12,@v13,@v14,@v15)", mConnection))
                {
                    foreach (var v in list)
                    {
                        if (v.IsFreeze)
                        {
                            lFreeze.Add(v);
                            continue;
                        }
                        cmd.Parameters.Clear();
                        int i = 0;
                        foreach (var v2 in v.GetObjects())
                        {
                            cmd.Parameters.AddWithValue($"v{++i}", v2);
                        }
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e) { ll.Add(v); Console.WriteLine(e.ToString()); }
                    }
                    cmd.CommandText = "INSERT INTO weighingtoolfreeze (SerialNumber,WorkNo,OrderId,Quality,Mw,Exterior,LotNo,PackagingRequirements,Purity,DataCollation,Label,DateLabel,Release,DateTelease,Comments) VALUES(@v1,@v2,@v3,@v4,@v5,@v6,@v7,@v8,@v9,@v10,@v11,@v12,@v13,@v14,@v15)";
                    foreach(var v in lFreeze)
                    {
                        cmd.Parameters.Clear();
                        int i = 0;
                        foreach (var v2 in v.GetObjects())
                        {
                            cmd.Parameters.AddWithValue($"v{++i}", v2);
                        }
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e) { ll.Add(v); Console.WriteLine(e.ToString()); }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
            return ll;
        }

        // ------------功能-------------
        public static string NnDecrypt(string decryptStr)
        {
            byte[] decrypt = Convert.FromBase64String(decryptStr);
            using (RijndaelManaged rDel = new RijndaelManaged())
            {
                rDel.Key = Encoding.UTF8.GetBytes("3456gh.dfr4u.pbcl_ser-215gfodcxs");
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform transform = rDel.CreateDecryptor())
                {
                    byte[] result = transform.TransformFinalBlock(decrypt, 0, decrypt.Length);
                    return Encoding.UTF8.GetString(result);
                }
            }
        }
    }
}
