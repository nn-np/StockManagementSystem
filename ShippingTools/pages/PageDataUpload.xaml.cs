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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShippingTools.pages
{
    /// <summary>
    /// 数据上传
    /// </summary>
    public partial class PageDataUpload : Page
    {

        public delegate void NnAction(string v);
        public NnAction NAction { get; set; }

        public PageDataUpload()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "submit":// 提交标签数据
                    Thread t = new Thread(_submit);
                    t.IsBackground = true;
                    t.Start(mTBMain.Text);
                    NavigationService.GoBack();
                    break;
                case "cancel":
                    NavigationService.GoBack();
                    break;
                default: break;
            }
        }
        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="o"></param>
        private void _submit(object o)
        {
            string value = o as string;
            if (string.IsNullOrEmpty(value))
            {
                NnMessage.ShowMessage("上传数据为空！");
                return;
            }
            Dispatcher.Invoke(() => ((Application.Current.MainWindow) as MainWindow)?.StatusBarState(true, "正在上传..."));

            List<Order> list = _getOrders(value);
            string erririnfo = _upload(list);

            if (!string.IsNullOrWhiteSpace(erririnfo))
            {
                WarnWindow.ShowMessage(erririnfo);
            }

            Dispatcher.Invoke(() =>
            {
                ((Application.Current.MainWindow) as MainWindow)?.StatusBarState(false, "标签已上传.");
                NAction?.Invoke("init");
            });
        }

        private List<Order> _getOrders(string value)
        {
            List<Order> list = new List<Order>();
            string[] vs = value.Split('\n');
            foreach(var v in vs)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                Order od = new Order();
                od.InitByString(v);
                list.Add(od);
            }
            return list;
        }

        private string _upload(List<Order> list)
        {
            List<Order> ll = NnReader.Instance.UploadPyrolysisLabel(list);
            StringBuilder sb = new StringBuilder();
            foreach (var v in ll)
            {
                if (!v.IsValid || string.IsNullOrEmpty(v.OrderId))
                {
                    sb.Append(v.OriginalValue).Append("  数据无效，检查数据是否完整！\n");
                    continue;
                }
                else
                {
                    sb.Append(v.OriginalValue).Append("  上传失败！\n");
                }
            }
            return sb.ToString();
        }

        private void MTBMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mTBMain.Clear();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            mTBMain.Focus();
        }
    }
}
