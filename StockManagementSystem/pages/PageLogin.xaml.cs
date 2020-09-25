using StockManagementSystem.data;
using StockManagementSystem.pages;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace StockManagementSystem
{
    /// <summary>
    /// 登录页面
    /// </summary>
    public partial class PageLogin : Page
    {
        private Window mParent;
        private PageNewUser mUser;
        private NnStockManager mManager;

        public PageLogin(Window window)
        {
            InitializeComponent();

            userName.Text = ConfigurationManager.AppSettings["lastUser"];
            mParent = window;
            mCBType.ItemsSource = new string[] { "库存", "半纯品", "树脂肽" };
            int.TryParse(ConfigurationManager.AppSettings["loginType"], out int ind);
            mCBType.SelectedIndex = ind;
        }

        // 新建账号
        private void click_new(object sender, RoutedEventArgs e)
        {
            if (!((Start)mParent).StockManager.IsValid)
            {
                mTBWorring.Text = "初始化失败，无法新建账号！";
                return;
            }
            if (mUser == null)
                mUser = new PageNewUser(mParent);
            ((Start)mParent).mFrame.Content = mUser;
        }

        // 跳过
        private void click_skip(object sender, RoutedEventArgs e)
        {
            data.Tools.SetConfiguration("isSkip", isSkip.IsChecked.ToString());

            MainWindow window = new MainWindow();
            Application.Current.MainWindow = window;
            window.Show();
            mParent.Close();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "login":
                    _login();
                    break;
            }
        }
        /// <summary>
        /// 登录
        /// </summary>
        private void _login()
        {
            if (mManager == null)
                mManager = ((Start)mParent).StockManager;
            if (mManager == null || !mManager.IsValid)
            {
                mTBWorring.Text = "初始化失败，无法登陆！";
                return;
            }
            if (string.IsNullOrWhiteSpace(userName.Text) || string.IsNullOrWhiteSpace(password.Password))
            {
                mTBWorring.Text = "用户名或密码不能为空！";
                return;
            }
            if (mManager.IsPassed(userName.Text, NnConnection.GetMD5String(password.Password)))
            {
                Window window = null;
                int i = mCBType.SelectedIndex;
                i = i < 0 ? 0 : i;
                switch (i)
                {
                    case 0:// 库存
                        window = new MainWindow(true);
                        break;
                    case 1:// 半纯品
                        window = new NotQCWindow();
                        break;
                    case 2:// 树脂肽
                        window = new ResinWindow();
                        break;
                    default:
                        window = new MainWindow(true);
                        break;
                }
                // 保存登录类别
                data.Tools.SetConfiguration("loginType", mCBType.SelectedIndex.ToString());
                data.Tools.SetConfiguration("lastUser", userName.Text);
                data.Tools.SetConfiguration("isSkip", isSkip.IsChecked.ToString());
                // 打登录后窗口
                Application.Current.MainWindow = window;
                window.Show();
                mParent.Close();
            }
            mTBWorring.Text = "用户名或密码错误，请重新输入！";
            password.Password = "";
        }
    }
}
