//#define NOTQC
using StockManagementSystem.data;
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
using System.Windows.Shapes;

namespace StockManagementSystem
{
    /// <summary>
    /// 开始窗口
    /// </summary>
    public partial class Start : Window
    {

        private PageLogin mLogin;// 登陆页
        private NnStockManager manager;

        public Start()
        {
            InitializeComponent();


            if (!App.IsNotUpdate)
                Update();

#if (CUSTOMER)
            MainWindow window = new MainWindow();
            Application.Current.MainWindow = window;
            window.Show();
            this.Close();
#else

            if (ConfigurationManager.AppSettings["isSkip"] == "True")
            {
                MainWindow mw = new MainWindow();
                Application.Current.MainWindow = mw;
                mw.Show();
                this.Close();
                return;
            }

            mLogin = new PageLogin(this);
            mFrame.Content = mLogin;
            manager = new NnStockManager(ShowMessage);
#endif
        }

        // 不用跳过
        public Start(bool isSkip)
        {
            InitializeComponent();

            mLogin = new PageLogin(this);
            mFrame.Content = mLogin;
            manager = new NnStockManager(ShowMessage);
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        private void Update()
        {
            string appname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string basepath = @"\\c11w16fs01.genscript.com\int_17005\化学部\内部\自动生产组--卜宗磊负责\原仪器组文件\3  班\个人\徐世宁\软件测试\" + appname;

            string versionold = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            int v1 = GetVersion(versionold);
            int i2 = versionold.LastIndexOf('.') + 1;
            int.TryParse(versionold.Substring(i2, versionold.Length - i2), out int v2);// 现在版本号
            if (v2 >= v1) return;


            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            string str = $"\"{basepath};{appname}\"";
            Console.WriteLine(str);
            process.StartInfo.FileName = "NnUpdater.exe";
            process.StartInfo.Arguments = str;
#if (!DEBUG)
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
#endif
            Dispatcher.Invoke(() => Application.Current.MainWindow.Close());

            process.Start();
        }

        /// <summary>
        /// 获取最新版本号
        /// </summary>
        internal int GetVersion(string versionold)
        {
            int i = -1;
            try
            {
                System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient();
                c.Connect("10.11.30.155", 5016);
                c.Client.Send(Encoding.Default.GetBytes($"StockManagementSystem;{versionold}"));
                byte[] buffer = new byte[1024];
                int count = c.Client.Receive(buffer);
                string str = Encoding.Default.GetString(buffer, 0, count);
                int.TryParse(str, out i);
            }
            catch { }
            return i;
        }

        public NnStockManager StockManager { get => manager; }

        public void ShowMessage(string message, bool isError = false)
        {
            this.Dispatcher.Invoke(() => new NnMessage(message, isError).Show());
            Console.WriteLine(message);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "close":// 关闭
                    this.Close();
                    break;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        /// <summary>
        /// 切换用户
        /// </summary>
        public static void ChangeUser()
        {
            Start st = new Start(false);
            Application.Current.MainWindow = st;
            st.Show();
        }
    }
}
