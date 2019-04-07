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

namespace StockManagementSystem
{
    /// <summary>
    /// NnMessage.xaml 的交互逻辑
    /// </summary>
    public partial class NnMessage : Window
    {
        public NnMessage()
        {
            InitializeComponent();
            this.Top = SystemParameters.WorkArea.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
        }

        // 关闭按钮
        private void btclose_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void window_loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = SystemParameters.WorkArea.Height - this.Height;
            animation.Duration = TimeSpan.FromSeconds(0.5);
            this.BeginAnimation(Window.TopProperty, animation);
        }
    }
}
