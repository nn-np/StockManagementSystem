using data;
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

namespace StockManagementSystem
{
    /// <summary>
    /// Start.xaml 的交互逻辑
    /// </summary>
    public partial class Start : Window
    {

        private PageLogin mLogin;// 登陆页
        private NnStockManager manager;

        public Start()
        {
            InitializeComponent();
            mLogin = new PageLogin(this);
            mFrame.Content = mLogin;
            manager = new NnStockManager(ShowMessage);
        }

        public NnStockManager StockManager { get => manager; }

        public void ShowMessage(string message, bool isError = false)
        {
            this.Dispatcher.Invoke(() => new NnMessage(message, isError).Show());
            Console.WriteLine(message);
        }
    }
}
