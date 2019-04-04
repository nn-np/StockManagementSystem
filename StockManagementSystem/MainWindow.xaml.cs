using data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Regex m_regex = new Regex(@"\n");
        public MainWindow()
        {
            InitializeComponent();
        }

        // 搜索按钮
        private void click_search(object sender, RoutedEventArgs e)
        {

        }

        // 提交库存按钮
        private void click_submit(object sender, RoutedEventArgs e)
        {
            new NnConnection();
        }

        private void click_coordinate(object sender, RoutedEventArgs e)
        {

        }

        // 窗口开始活动
        private void m_activited(object sender, EventArgs e)
        {
            m_tb.Focus();
        }

        // textchanged
        private void m_textcanged(object sender, TextChangedEventArgs e) => st_tbcount.Text = (m_regex.Matches(m_tb.Text.TrimEnd()).Count + (string.IsNullOrWhiteSpace(m_tb.Text) ? 0 : 1)) + "条";
    }
}
