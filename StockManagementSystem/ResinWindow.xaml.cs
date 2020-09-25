using StockManagementSystem.data;
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
    /// 树脂肽页面
    /// </summary>
    public partial class ResinWindow : Window
    {
        private NnStockManager mManager;

        public bool IsPassed { get; internal set; }

        public ResinWindow()
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

        private void _showMessage(string message, bool isError = false)
        {
            this.Dispatcher.Invoke(() => new NnMessage(message, isError).Show());
            Console.WriteLine(message);
        }

        /// <summary>
        /// 搜索库存
        /// </summary>
        private void click_search(object sender, RoutedEventArgs e)
        {
            if (mManager == null || string.IsNullOrWhiteSpace(m_tb.Text))
                return;
            if (!mManager.IsValid)
            {
                _showMessage("初始化失败，无法搜索！", false);
                return;
            }
            new Thread(_search).Start(m_tb.Text.Trim());
            _statusBarState("正在搜索...", true);
        }

        /// <summary>
        /// 搜索库存
        /// </summary>
        private void _search(object o)
        {
            string value = o as string;
            if (value == null) return;

            _updateTBMain("\n\n-----开始搜索-------\n");

            string str = mManager.SearchAll(value);

            _updateTBMain("\n------搜索结束，搜索结果已在Excel表格中打开------" + str);

            _statusBarState("就绪", false);
        }

        // 提交库存按钮
        private void click_submit(object sender, RoutedEventArgs e)
        {
            if (mManager == null || string.IsNullOrWhiteSpace(m_tb.Text)) return;
            new Thread(_submit).Start(m_tb.Text);
            _statusBarState("正在提交...", true);
        }

        // 提交
        private void _submit(object o)
        {
            string str = o as string;
            if (str == null) return;

            StringBuilder errorstr = new StringBuilder();

            _updateTBMain("\n\n--------------\n");

            int counts = 0, successcount = 0;
            string[] values = str.Split('\n');
            foreach (var v in values)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                ++counts;
                OrderInfo order = new OrderInfo(v);
                if (order.IsAvailable)
                {
                    int count = mManager.SubmitResinOrder(order);
                    string showStr = _getSubmitFeedback(v, order, count, errorstr);
                    if (count > 0) ++successcount;
                    _updateTBMain(showStr);
                }
                else
                {
                    _updateTBMain(v.TrimEnd() + "\t---\t数据无效\n");
                    errorstr.Append(v.TrimEnd()).Append("\t---\t数据无效\n");
                }
            }
            _updateTBMain($"--------------\n总计/成功/失败  {counts}/{successcount}/{counts - successcount}（条） nnns\n");
            _statusBarState("就绪", false);

            if (!string.IsNullOrWhiteSpace(errorstr.ToString()))
            {
                WarnWindow.ShowMessage(errorstr.ToString());
            }
        }

        private string _getSubmitFeedback(string subStr, OrderInfo order, int count, StringBuilder errorstr)
        {
            string showStr = subStr.TrimEnd() + "\t---\t";
            switch (order.State)
            {
                case OrderInfo.OrderInfoState.Insert:
                    showStr += "添加";
                    break;
                case OrderInfo.OrderInfoState.Remove:
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
                    case OrderInfo.OrderInfoState.Insert:
                        showStr += "失败，记录已存在\n";
                        break;
                    case OrderInfo.OrderInfoState.Remove:
                        showStr += "失败，记录不存在\n";
                        break;
                }
                errorstr.Append(showStr);
            }
            return showStr;
        }

        private void _statusBarState(string value = "就绪", bool isWarning = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                st_tbstate.Text = value;
                st_bar.Background = isWarning ? new SolidColorBrush(Color.FromRgb(0xCA, 0x51, 0x00))
                    : new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
            });
        }

        private void _updateTBMain(string str)
        {
            Dispatcher.Invoke(() =>
            {
                m_tb.AppendText(str);
                m_tb.ScrollToEnd();
            });
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "changeuser":// 切换用户
                    Start.ChangeUser();
                    this.Close();
                    break;
            }
        }
    }
}
