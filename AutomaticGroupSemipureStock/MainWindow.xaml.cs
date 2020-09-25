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

namespace AutomaticGroupSemipureStock
{
    /// <summary>
    /// 自动组半纯品库
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void TheStatusBarState(bool isWarning = false, string left = "就绪", string right = "", double progress = 0, bool isComplate = true, string message = null);
        public static TheStatusBarState StatusBar { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            StatusBar = StatusBarState;
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
        }
    }
}
