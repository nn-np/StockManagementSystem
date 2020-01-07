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
        }

        private void init()
        {
            List<Template> list = new List<Template>();
            list.Add(new ShippingTools.Template() { Title = "所有数据" });

            mLBMain.ItemsSource = list;
            mLBMain.SelectedIndex = 0;
            _initPosition();
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
                default: return;
            }
            mFrame.Content = mPage;
            if (mFrame.CanGoBack)
            {
                mFrame.RemoveBackEntry();
            }
        }
    }
}
