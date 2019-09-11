using data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StockManagementSystem
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    // <!--#FFCA5100 #FF007ACC-->
    public partial class MainWindow : Window
    {
        private NnStockManager m_manager;

        private string submitStr;// 用于提交或者搜索的字符串

        public bool IsPassed { get; internal set; }

        public MainWindow(bool isPassed = false)
        {
            InitializeComponent();
            IsPassed = isPassed;
            new Thread(init).Start();// 开始初始化
            if (!IsPassed)
            {
                mBTSubmit.Visibility = Visibility.Collapsed;
                Run run = new Run("多肽库存服务");
                run.FontSize = 24;
                mTBTitle.Text = "";
                mTBTitle.Inlines.Add(run);
                Run run2 = new Run("输入[Order ID]、[Wo No.]或[坐标]进行搜索");
                run2.FontSize = 16;
                run2.FontWeight = FontWeights.Thin;
                mTBTitle.Inlines.Add(new LineBreak());
                mTBTitle.Inlines.Add(run2);

                mBTSearchNotQC.Visibility = Visibility.Visible;
                mBTSearchStock.Visibility = Visibility.Visible;
                mBTSearch.Visibility = Visibility.Collapsed;
                mBTSubmit.Visibility = Visibility.Collapsed;
                mBTCoordinate.Visibility = Visibility.Collapsed;
            }
            _statusBarState("正在初始化...", true);
        }

        private void _showMessage(string message,bool isError = false)
        {
            this.Dispatcher.Invoke(() => new NnMessage(message, isError).Show());
            Console.WriteLine(message);
        }

        private void init()
        {
            m_manager = new NnStockManager(_showMessage);
            if (!m_manager.IsValid)
            {
                _statusBarState("初始化失败！", true);
                return;
            }
            _statusBarState("就绪", false);
        }

        // 搜索按钮
        private void click_search(object sender, RoutedEventArgs e)
        {
            submitStr = m_tb.Text;
            if (m_manager == null || string.IsNullOrWhiteSpace(submitStr))
                return;
            if (!m_manager.IsValid)
            {
                _showMessage("初始化失败，无法搜索！", false);
                return;
            }
            new Thread(_search).Start();
            _statusBarState("正在搜索...", true);
        }

        private void _search()
        {
            string str = m_manager.Search(submitStr);
            submitStr = "";
            this.Dispatcher.Invoke(new TextBoxUpdate(_textBoxUpdate), "\n\n------------\n"+str);
            _statusBarState("就绪", false);
        }

        // 提交库存按钮
        private void click_submit(object sender, RoutedEventArgs e)
        {
#if (!DEBUG)
            if (m_manager == null) return;
            submitStr = m_tb.Text;
            if (string.IsNullOrWhiteSpace(submitStr))
            {
                _showMessage("提交数据为空！", false);
                return;
            }
            if (!m_manager.IsValid)
            {
                _showMessage("初始化失败，无法提交！", false);
                return;
            }
#else
            submitStr = m_tb.Text;
#endif
            submitStr += submitStr.EndsWith("\n") ? "" : "\n";
            new Thread(_submit).Start();
            _statusBarState("正在提交...", true);
        }

        private void _submit()
        {
            StringBuilder errorstr = new StringBuilder();
            TextBoxUpdate update = new TextBoxUpdate(_textBoxUpdate);
            this.Dispatcher.Invoke(update, "\n\n--------------\n");
            int counts = 0, successcount = 0;
            string[] values = submitStr.Split('\n');
            foreach(var v in values)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                ++counts;
                NnStock stock = new NnStock(v);
                if (stock.IsAvailable)
                {
                    int count = m_manager.Submit(stock);
                    string showStr = _getSubmitFeedback(v, stock, count,errorstr);
                    if (count > 0) ++successcount;
                    this.Dispatcher.Invoke(update, showStr);
                }
                else
                {
                    this.Dispatcher.Invoke(update, v.TrimEnd() + "\t---\t数据无效\n");
                    errorstr.Append(v.TrimEnd()).Append("\t---\t数据无效\n");
                }
            }
            this.Dispatcher.Invoke(update, $"--------------\n总计/成功/失败  {counts}/{successcount}/{counts - successcount}（条） nnns\n");
            submitStr = "";
            _statusBarState("就绪", false);

            if (!string.IsNullOrWhiteSpace(errorstr.ToString()))
            {
                WarnWindow.ShowMessage(errorstr.ToString());
            }

            // 每次更新结束后检查是否需要备份数据库
            _checkBackup();
        }

        private string _getSubmitFeedback(string subStr,NnStock stock,int count, StringBuilder errorstr)
        {
            string showStr = subStr.TrimEnd() + "\t---\t";
            switch (stock.State)
            {
                case NnStock.StockState.Insert:
                    showStr += "添加";
                    break;
                case NnStock.StockState.Update:
                    showStr += "更新";
                    break;
                case NnStock.StockState.Delete:
                    showStr += "移除";
                    break;
            }
            if (count > 0)
            {
                showStr += "成功\n";
            }
            else
            {
                switch (stock.State)
                {
                    case NnStock.StockState.Insert:
                        showStr += "失败，记录已存在\n";
                        break;
                    case NnStock.StockState.Update:
                        showStr += "失败，记录不存在\n";
                        break;
                    case NnStock.StockState.Delete:
                        showStr += "失败，记录不存在\n";
                        break;
                }
                errorstr.Append(showStr);
            }
            return showStr;
        }

        private void _checkBackup()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (configuration.AppSettings.Settings["backuptime"] != null)
            {
                long ticks;
                if(long.TryParse(configuration.AppSettings.Settings["backuptime"].Value,out ticks))
                {
                    DateTime bktime = new DateTime(ticks);
                    if (_daySpan(bktime, DateTime.Now) < 1)
                        return;
                }
            }
            new Thread(new ParameterizedThreadStart(_backup)).Start(configuration);
        }

        private void _backup(object o)
        {
            Configuration configuration = o as Configuration;
            try
            {
                string path = Environment.CurrentDirectory + @"\" + DateTime.Now.Ticks + ".nnbkp";

                File.Copy(m_manager.DatabasePath, path);

                if (configuration.AppSettings.Settings["backuptime"] == null)
                    configuration.AppSettings.Settings.Add("backuptime", DateTime.Now.Ticks.ToString());
                else
                    configuration.AppSettings.Settings["backuptime"].Value = DateTime.Now.Ticks.ToString();
                configuration.Save();
                DirectoryInfo info = new DirectoryInfo(Environment.CurrentDirectory);
                foreach (FileInfo finfo in info.GetFiles())
                {
                    if (finfo.Extension == ".nnbkp")
                    {
                        long ticks;
                        if (!long.TryParse(System.IO.Path.GetFileNameWithoutExtension(finfo.Name), out ticks))
                            continue;
                        DateTime tm = new DateTime(ticks);
                        if (_daySpan(tm, DateTime.Now) > 7)
                        {
                            try
                            {
                                finfo.Delete();
                            }
                            catch { }
                        }
                    }
                }
                _showMessage("数据已备份！", false);
            }
            catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        private int _daySpan(DateTime t1,DateTime t2)
        {
            TimeSpan ts1 = new TimeSpan(t1.Ticks);
            TimeSpan ts2 = new TimeSpan(t2.Ticks);
            TimeSpan t = ts2.Subtract(ts1).Duration();
            return t.Days;
        }
        
        // 更新主textBox
        delegate void TextBoxUpdate(string value);
        private void _statusBarState(string value,bool isWarning)
        {
            this.Dispatcher.Invoke(() =>
            {
                st_tbstate.Text = value;
                st_bar.Background = isWarning ? new SolidColorBrush(Color.FromRgb(0xCA, 0x51, 0x00))
                    : new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
            });
        }

        private void _textBoxUpdate(string value)
        {
            m_tb.AppendText(value);
            m_tb.ScrollToEnd();
        }

        // 导出坐标按钮
        private void click_coordinate(object sender, RoutedEventArgs e)
        {
            if (m_manager == null) return;
            if (!m_manager.IsValid)
            {
                _showMessage("初始化失败，无法导出！", false);
                return;
            }
            new Thread(m_manager.OutputCoordinate).Start();
        }

        // 窗口开始活动
        private void m_activited(object sender, EventArgs e)
        {
            m_tb.Focus();
        }

        // textchanged
        private void m_textcanged(object sender, TextChangedEventArgs e)
        {
            string str = m_tb.Text.Trim() + "\r\n";
            int i = str.IndexOf("\r\n"), j = 0;
            int count = 0;
            while (i > 0)
            {
                if (i - j > 1)
                    ++count;
                j = i + 1;
                i = str.IndexOf('\n', j);
            }
            st_tbcount.Text = count + "条";
        }

        // 输入框被双击
        private void tb_doubleclick(object sender, MouseButtonEventArgs e)
        {
            m_tb.Clear();
        }

        /// <summary>
        /// 搜索半纯品
        /// </summary>
        private void click_searchNotQc(object sender, RoutedEventArgs e)
        {
            new Thread(_searchNotQC).Start(m_tb.Text);
        }

        private void _searchNotQC(object o)
        {
            string str = o as string;
            if (string.IsNullOrWhiteSpace(str)) return;
            _statusBarState("正在搜索...", true);
            m_manager.SearchNotQCData(str);
            _statusBarState("就绪", false);
        }
    }
}
