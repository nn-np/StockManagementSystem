using data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private NnConnection m_connection;
        private string submitStr;// 用于提交或者搜索的字符串
        public MainWindow()
        {
            InitializeComponent();
            new Thread(init).Start();// 开始初始化
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
                this.Dispatcher.Invoke(new StatusBarUpdate(_statusBarState), "初始化失败！", true);
                return;
            }
            this.Dispatcher.Invoke(new StatusBarUpdate(_statusBarState), "就绪", false);
            try
            {
                m_connection = new NnConnection();
            }
            catch { }
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
            submitStr += submitStr.EndsWith("\n") ? "" : "\n";
            new Thread(_search).Start();
            _statusBarState("正在搜索...", true);
        }

        private void _search()
        {
            string str = m_manager.Search(submitStr);
            submitStr = "";
            this.Dispatcher.Invoke(new TextBoxUpdate(_textBoxUpdate), "\n\n------------\n"+str);
            this.Dispatcher.Invoke(new StatusBarUpdate(_statusBarState), "就绪", false);
        }

        // 提交库存按钮
        private void click_submit(object sender, RoutedEventArgs e)
        {
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
            submitStr += submitStr.EndsWith("\n") ? "" : "\n";
            new Thread(_submit).Start();
            _statusBarState("正在提交...", true);
        }

        private void _submit()
        {
            TextBoxUpdate update = new TextBoxUpdate(_textBoxUpdate);
            this.Dispatcher.Invoke(update, "\n\n--------------\n");
            int i = 0, j = 0;
            int counts = 0, successcount = 0;
            while (true)
            {
                ++counts;
                i = submitStr.IndexOf('\n', j);
                if (i < 0) break;
                string subStr = submitStr.Substring(j, i - j);
                NnStock stock = new NnStock(subStr);
                if (stock.IsAvailable)
                {
                    int count = m_manager.Submit(stock);
                    bool isAdd = string.IsNullOrEmpty(stock.Cause);
                    string showStr = subStr.TrimEnd() + "\t---\t" + (isAdd ? "添加" : "移除");
                    if (count > 0)
                    {
                        showStr += "成功\n";
                        ++successcount;
                    }
                    else
                        showStr += (isAdd ? "失败，记录已存在\n" : "失败，记录不存在\n");
                    this.Dispatcher.Invoke(update, showStr);
                }
                else
                    this.Dispatcher.Invoke(update, subStr.TrimEnd() + "\t---\t数据无效\n");
                j = i + 1;
            }
            this.Dispatcher.Invoke(update, $"--------------\n总计/成功/失败  {counts - 1}/{successcount}/{counts - successcount - 1}（条） nnns\n");
            submitStr = "";
            this.Dispatcher.Invoke(new StatusBarUpdate(_statusBarState), "就绪", false);
        }
        // 更新状态栏
        delegate void StatusBarUpdate(string value, bool isError);
        // 更新主textBox
        delegate void TextBoxUpdate(string value);
        private void _statusBarState(string value,bool isWarning)
        {
            st_tbstate.Text = value;
            st_bar.Background = isWarning ? new SolidColorBrush(Color.FromRgb(0xCA, 0x51, 0x00))
                : new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
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
    }
}
