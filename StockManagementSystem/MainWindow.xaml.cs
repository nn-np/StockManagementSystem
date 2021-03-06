﻿using StockManagementSystem.data;
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
    /// 库存管理
    /// </summary>
    // <!--#FFCA5100 #FF007ACC-->
    public partial class MainWindow : Window
    {
        private NnStockManager m_manager;

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

                //mBTSearch.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// 搜索库存
        /// </summary>
        private void click_search(object sender, RoutedEventArgs e)
        {
            if (m_manager == null || string.IsNullOrWhiteSpace(m_tb.Text))
                return;
            if (!m_manager.IsValid)
            {
                _showMessage("初始化失败，无法搜索！", false);
                return;
            }

            new Thread(_search).Start(m_tb.Text.Trim());
            _statusBarState("正在搜索...", true);
        }

        private void _updateTBMain(string str)
        {
            Dispatcher.Invoke(() =>
            {
                m_tb.AppendText(str);
                m_tb.ScrollToEnd();
            });
        }

        /// <summary>
        /// 搜索库存
        /// </summary>
        private void _search(object o)
        {
            string value = o as string;
            if (value == null) return;

            _updateTBMain("\n\n-----开始搜索-------\n");

            string str = m_manager.SearchAll(value);

            _updateTBMain("\n------搜索结束，搜索结果已在Excel表格中打开------" + str);

            _statusBarState("就绪", false);
        }

        // 提交库存按钮
        private void click_submit(object sender, RoutedEventArgs e)
        {
            string str = m_tb.Text;
#if (!DEBUG)
            if (m_manager == null) return;
            if (string.IsNullOrWhiteSpace(str)|| !m_manager.IsValid)
            {
                _showMessage(string.IsNullOrWhiteSpace(str) ? "提交数据为空！":"初始化失败，无法提交！", false);
                return;
            }
#endif
            new Thread(_submit).Start(str);
            _statusBarState("正在提交...", true);
        }

        private void _submit(object o)
        {
            string str = o as string;
            if (str == null) return;

            StringBuilder errorstr = new StringBuilder();
            TextBoxUpdate update = new TextBoxUpdate(_textBoxUpdate);
            this.Dispatcher.Invoke(update, "\n\n--------------\n");
            int counts = 0, successcount = 0;
            var stocks = _getStocks(str);
            foreach(var v in stocks)
            {
                ++counts;
                if (v.IsAvailable)
                {
                    int count = m_manager.Submit(v);
                    string showStr = _getSubmitFeedback(v.OriginalString, v, count,errorstr);
                    if (count > 0) ++successcount;
                    this.Dispatcher.Invoke(update, showStr);
                }
                else
                {
                    this.Dispatcher.Invoke(update, v.OriginalString + "\t---\t数据无效\n");
                    errorstr.Append(v.OriginalString).Append("\t---\t数据无效\n");
                }
            }
            this.Dispatcher.Invoke(update, $"--------------\n总计/成功/失败  {counts}/{successcount}/{counts - successcount}（条） nnns\n");

            _statusBarState("就绪", false);

            if (!string.IsNullOrWhiteSpace(errorstr.ToString()))
            {
                WarnWindow.ShowMessage(errorstr.ToString());
            }

            // 每次更新结束后检查是否需要备份数据库
            _checkBackup();
        }

        private List<NnStock> _getStocks(string str)
        {
            List<NnStock> list = new List<NnStock>();
            string[] strs = str.Split('\n');
            foreach (var v in strs)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                NnStock stock = new NnStock(v);
                list.Add(stock);
            }
            return list;
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
