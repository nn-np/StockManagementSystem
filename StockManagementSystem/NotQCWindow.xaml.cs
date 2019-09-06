using data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StockManagementSystem
{
    /// <summary>
    /// 未QC库数据管理
    /// </summary>
    public partial class NotQCWindow : Window
    {
        private NnStockManager mManager;
        public NotQCWindow()
        {
            InitializeComponent();
            mManager = new NnStockManager(_showMessage);
            if (!mManager.IsValid)
            {
                _statusBarState("初始化失败！", true);
                return;
            }
            _statusBarState("就绪", false);
        }

        private void click_search(object sender, RoutedEventArgs e)
        {
            new Thread(_search).Start(mTBMain.Text);
        }

        private void _search(object o)
        {
            string str = o as string;
            if (string.IsNullOrWhiteSpace(str)) return;
            _statusBarState("正在搜索...", true);
            mManager.SearchNotQCData(str);

            _statusBarState("就绪", false);
        }

        private void click_alldata(object sender, RoutedEventArgs e)
        {
            _statusBarState("正在读取...", true);
            new Thread(_output).Start();
        }

        private void _output()
        {
            mManager.AllNotQCData();

            _statusBarState("就绪", false);
        }

        private void click_submit(object sender, RoutedEventArgs e)
        {
            if (mManager == null || string.IsNullOrWhiteSpace(mTBMain.Text)) return;
            new Thread(_submit).Start(mTBMain.Text);
            _statusBarState("正在提交...", true);
        }

        private void _submit(object o)
        {
            _updateTBMain("\n\n--------------\n");
            string value = o as string;
            if (value == null) return;

            int counts = 0, successcount = 0;
            string[] values = value.Split('\n');
            foreach (var v in values)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                ++counts;
                NotQCOrder order = new NotQCOrder(v);
                if (order.IsAvailable)
                {
                    int count = mManager.SubmitNotQCOrder(order);
                    string showStr = _getSubmitFeedback(v, order, count);
                    if (count > 0) ++successcount;
                    _updateTBMain(showStr);
                }
                else
                    _updateTBMain(v.TrimEnd() + "\t---\t数据无效\n");
            }
            _updateTBMain($"--------------\n总计/成功/失败  {counts}/{successcount}/{counts - successcount}（条） nnns\n");
            _statusBarState("就绪", false);
        }

        private string _getSubmitFeedback(string subStr, NotQCOrder order, int count)
        {
            string showStr = subStr.TrimEnd() + "\t---\t";
            switch (order.State)
            {
                case NotQCOrder.NotQCState.Insert:
                    showStr += "添加";
                    break;
                case NotQCOrder.NotQCState.Remove:
                    showStr += "移除";
                    break;
            }
            if (count > 0)
            {
                showStr += "成功\n";
            }
            else
            {
                switch (order.State)
                {
                    case NotQCOrder.NotQCState.Insert:
                        showStr += "添加失败，记录已存在\n";
                        break;
                    case NotQCOrder.NotQCState.Remove:
                        showStr += "失败，记录不存在\n";
                        break;
                }
            }
            return showStr;
        }

        private void _updateTBMain(string str)
        {
            Dispatcher.Invoke(() =>
            {
                mTBMain.AppendText(str);
                mTBMain.ScrollToEnd();
            });
        }

        private void _statusBarState(string value, bool isWarning)
        {
            this.Dispatcher.Invoke(() =>
            {
                st_tbstate.Text = value;
                mSBMain.Background = isWarning ? new SolidColorBrush(Color.FromRgb(0xCA, 0x51, 0x00))
                    : new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
            });
        }

        private void _showMessage(string message, bool isError = false)
        {
            this.Dispatcher.Invoke(() => new NnMessage(message, isError).Show());
            Console.WriteLine(message);
        }

        // textchanged
        private void m_textcanged(object sender, TextChangedEventArgs e)
        {
            string str = mTBMain.Text.Trim() + "\r\n";
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

        // 窗口开始活动
        private void m_activited(object sender, EventArgs e)
        {
            mTBMain.Focus();
        }

        // 输入框被双击
        private void tb_doubleclick(object sender, MouseButtonEventArgs e)
        {
            mTBMain.Clear();
        }
        /// <summary>
        /// 坐标
        /// </summary>
        private void click_coordinate(object sender, RoutedEventArgs e)
        {
            new Thread(_outputCoordinate).Start();
        }

        private void _outputCoordinate()
        {
            _statusBarState("正在读取...", true);
            mManager.OutputNotQCCoordinate();
            _statusBarState("就绪", false);
        }
    }
}
