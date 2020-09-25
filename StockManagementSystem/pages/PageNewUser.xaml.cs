using StockManagementSystem.data;
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

namespace StockManagementSystem.pages
{
    /// <summary>
    /// PageNewUser.xaml 的交互逻辑
    /// </summary>
    public partial class PageNewUser : Page
    {
        private Window parent;
        public PageNewUser(Window window)
        {
            InitializeComponent();
            parent = window;
        }

        // 提交
        private void click_newuser(object sender, RoutedEventArgs e)
        {
            mTBWorring.Text = "";
            NnStockManager manager = ((Start)parent).StockManager;
            if(NnConnection.GetMD5String(mPBAmid.Password)!= "6B99EA9FBBD04700F4C0FCD4DA705623")
            {
                mTBWorring.Text = "管理员密码错误！";
                return;
            }
            if (string.IsNullOrWhiteSpace(mTBUsername.Text))
            {
                mTBWorring.Text = "用户名不能为空！";
                return;
            }
            if (mPBPassword.Password != mPBRePassword.Password)
            {
                mTBWorring.Text = "两次密码输入不一致！";
                return;
            }
            if(manager.AddUser(mTBUsername.Text.Trim(), NnConnection.GetMD5String(mPBRePassword.Password)) > 0)
            {
                MainWindow window = new MainWindow(true);
                Application.Current.MainWindow = window;
                window.Show();
                ((Start)parent).ShowMessage("用户添加成功，并已使用此用户登陆。", false);
                parent.Close();
            }
            else
            {
                mTBWorring.Text = "此用户已存在！";
            }
        }

        // 取消
        private void click_cancel(object sender, RoutedEventArgs e)
        {
            NavigationService.GetNavigationService(this).GoBack();
        }
    }
}
