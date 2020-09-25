using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows;

namespace StockManagementSystem
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static bool IsNotUpdate { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "donotupdate")
                {
                    IsNotUpdate = true;
                    return;
                }
                else
                {
                    AllocConsole();
                }
            }
        }
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
            e.Handled = true;
        }
    }
}
