using ShippingTools.pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ShippingTools
{
    /// <summary>
    /// 发货工具界面
    /// </summary>
    public partial class MainWindow : Window
    {
        private Page mPage;
        private Template mTemplate;
        public MainWindow()
        {
            InitializeComponent();

            init();
            _initPosition();
        }

        private void init()
        {
            List<Template> list = new List<Template>();
            list.Add(new ShippingTools.Template() { Title = "所有数据" });
            list.Add(new ShippingTools.Template() { Title = "数据上传" });

            mLBMain.ItemsSource = list;
            mLBMain.SelectedIndex = 0;
        }

        private void _initPosition()
        {
            try
            {
                Rect bounds = Properties.Settings.Default.MainWindowPosition;
                if (bounds.Width <= 0) return;
                Top = bounds.Top;
                Left = bounds.Left;
                if (SizeToContent == SizeToContent.Manual)
                {
                    Width = bounds.Width;
                    Height = bounds.Height;
                }
                WindowState = Properties.Settings.Default.MainWindowState;
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.MainWindowPosition = RestoreBounds;
            Properties.Settings.Default.MainWindowState = WindowState;
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// 状态栏设置
        /// </summary>
        /// <param name="isWarning">是否警告（警告为红色）</param>
        /// <param name="left">左边显示的文字</param>
        /// <param name="right">中间显示的文字</param>
        /// <param name="progress">进度条显示数值</param>
        /// <param name="isComplate">是否完成（完成关闭进度条的显示）</param>
        /// <param name="message">要弹出的消息</param>
        public void StatusBarState(bool isWarning = false, string left = "就绪", string right = "", double progress = 0, bool isComplate = true, string message = null)
        {
            Dispatcher.Invoke(() =>
            {
                if (isComplate)
                {
                    sbProgress.Visibility = Visibility.Collapsed;
                    sbCenter.Text = "";
                }
                else
                {
                    sbProgress.Visibility = Visibility.Visible;
                    sbProgress.Value = progress * 100;
                    sbCenter.Text = progress.ToString("p");
                }
                sbLeft.Text = left;
                sbRight.Text = right;
                mStatusBar.Background = isWarning ? new SolidColorBrush(Color.FromRgb(0xCA, 0x51, 0x00))
                    : new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
            });
        }

        private void mLBMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0 || e.AddedItems[0] == null) return;
            mTemplate = e.AddedItems[0] as Template;
            if (mTemplate == null) return;
            switch (mTemplate.Title)
            {
                case "所有数据":
                    mPage = new PageValue();
                    break;
                case "数据上传":
                    mPage = new PageDataUpload() { NAction = _action };
                    break;
                default: return;
            }
            mFrame.Content = mPage;
            if (mFrame.CanGoBack)
            {
                mFrame.RemoveBackEntry();
            }
        }

        private void _action(string v)
        {
            switch (v)
            {
                case "init":
                    init();
                    break;
                default:return;
            }
        }
    }
}
