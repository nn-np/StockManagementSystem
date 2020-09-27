using AutomaticGroupSemipureStock.Datas;
using AutomaticGroupSemipureStock.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AutomaticGroupSemipureStock.Pages
{
    /// <summary>
    /// 自动组半纯品库页面
    /// </summary>
    public partial class PageAutomaticSemipureStock : Page
    {
        private ObservableCollection<AutomaticStockInfo> _EmptyList;    // 空坐标的列表
        private ObservableCollection<AutomaticStockInfo> _FilledList;   // 被填充的列表
        private ObservableCollection<AutomaticStockInfo> _MainList;     // 主列表
        private ObservableCollection<AutomaticStockInfo> _CurrentList;  // 当前列表
        public PageAutomaticSemipureStock()
        {
            InitializeComponent();
            Init();
        }

        private async void Init()
        {
            MainWindow.StatusBar(true, "加载数据...");
            try
            {
#if (DEBUG)
                _FilledList = await WEBHelper.HttpGetJSONAsync<ObservableCollection<AutomaticStockInfo>>("http://10.11.30.155:5000/api/stockinfo/AutomaticStockInfos");
#else
                _FilledList = await WEBHelper.HttpGetJSONAsync<ObservableCollection<AutomaticStockInfo>>("http://10.11.30.155:5004/api/stockinfo/AutomaticStockInfos");
#endif
                _MainList = _FilledList;
                _CurrentList = _MainList;
                mDGMain.ItemsSource = _CurrentList;
            }
            catch (Exception e)
            {
                MainWindow.StatusBar(false, $"数据加载出现错误：{e.Message}");
                return;
            }
            MainWindow.StatusBar();
        }

#region 事件
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "edit":        // 开始编辑
                    StartEdit();
                    break;
                case "opentable":   // 打开表格
                    break;
                default:return;
            }
        }

        private async void StartEdit()
        {
            MainWindow.StatusBar(true, "加载数据...");
            try
            {
#if (DEBUG)
                _EmptyList = await WEBHelper.HttpGetJSONAsync<ObservableCollection<AutomaticStockInfo>>("http://10.11.30.155:5000/api/stockinfo/emptcoordinates");
#else
                _EmptyList = await WEBHelper.HttpGetJSONAsync<ObservableCollection<AutomaticStockInfo>>("http://10.11.30.155:5004/api/stockinfo/emptcoordinates");
#endif
                _MainList = _EmptyList;
                _CurrentList = _MainList;
                mDGMain.ItemsSource = _CurrentList;
            }
            catch (Exception e)
            {
                MainWindow.StatusBar(false, $"数据加载出现错误：{e.Message}");
                return;
            }
            MainWindow.StatusBar();
        }
        #endregion // 事件

        #region 筛选
        private void NnCheckView_OnCheck(object sender, RoutedEventArgs e)
        {

        }

        private void NnCheckView_StartFilter(object sender, RoutedEventArgs e)
        {

        }
#endregion // 筛选
    }
}
