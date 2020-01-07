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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShippingTools
{
    /// <summary>
    /// NnMessage.xaml 的交互逻辑
    /// </summary>
    public partial class NnMessage : Window
    {
        private static NnMessage mMessage;

        /// <summary>
        /// 消息弹出窗口
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="isError">消息级别，错误或警告</param>
        private NnMessage()
        {
            InitializeComponent();
            this.Top = SystemParameters.WorkArea.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
        }

        private void _showMessage(string message, bool isError = false)
        {
            tb_bottom.Text = DateTime.Now.ToShortTimeString();
            tb_top.Text = "提示：";
            if (isError)
            {
                tb_top.Foreground = new SolidColorBrush(Color.FromRgb(0xCA, 0x51, 0x00));
            }
            tb_center.Text = message;
            if (!isError)
            {
                Thread t = new Thread(_autoClose);
                t.IsBackground = true;
                t.Start();
            }
        }

        private void _autoClose()
        {
            Thread.Sleep(4500);
            this.Dispatcher.Invoke(() => this.Close());
        }

        // 关闭按钮
        private void btclose_click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void window_loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = SystemParameters.WorkArea.Height - this.Height;
            animation.Duration = TimeSpan.FromSeconds(0.5);
            this.BeginAnimation(Window.TopProperty, animation);
        }

        public static void ShowMessage(string message, bool isError = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isError)
                {
                    if (mMessage == null) mMessage = new NnMessage();
                    mMessage._showMessage(message, isError);
                    mMessage.Show();
                    return;
                }
                NnMessage m = new NnMessage();
                m._showMessage(message, isError);
                m.Show();
            });
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
