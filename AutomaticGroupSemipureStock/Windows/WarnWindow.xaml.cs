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
using System.Windows.Shapes;

namespace AutomaticGroupSemipureStock.Windows
{
    /// <summary>
    /// 提示窗口
    /// </summary>
    public partial class WarnWindow : Window
    {
        public WarnWindow(string value)
        {
            InitializeComponent();
            mTBMain.Text = value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "btok":
                    DialogResult = true;
                    break;
                default: break;
            }
        }

        public static void ShowMessage(string str)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WarnWindow window = new WarnWindow(str);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            });
        }
    }
}
