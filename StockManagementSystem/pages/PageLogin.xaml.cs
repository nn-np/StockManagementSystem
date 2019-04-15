using data;
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
    /// PageLogin.xaml 的交互逻辑
    /// </summary>
    public partial class PageLogin : Page
    {
        private Window mParent;
        private PageNewUser mUser;
        private NnStockManager mManager;

        public PageLogin(Window window)
        {
            InitializeComponent();
            userName.Text = _getStringFromConfiguration("lastUser");
            mParent = window;
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
            if (isSkip.IsChecked != null && (bool)isSkip.IsChecked)
                _updateConfiguration("isSkip", true.ToString());

            MainWindow window = new MainWindow();
            Application.Current.MainWindow = window;
            window.Show();
            mParent.Close();
        }

        // 更新配置文件
        private void _updateConfiguration(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (configuration.AppSettings.Settings[key] != null)
                configuration.AppSettings.Settings[key].Value = value;
            else
                configuration.AppSettings.Settings.Add(key, value);
            configuration.Save();
        }

        // 从配文件获取字符串
        private string _getStringFromConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        // 登陆
        private void click_login(object sender, RoutedEventArgs e)
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
                _updateConfiguration("lastUser", userName.Text);
                MainWindow window = new MainWindow(true);
                Application.Current.MainWindow = window;
                window.Show();
                mParent.Close();
            }
            mTBWorring.Text = "用户名或密码错误，请重新输入！";
            password.Password = "";
        }
    }
}
