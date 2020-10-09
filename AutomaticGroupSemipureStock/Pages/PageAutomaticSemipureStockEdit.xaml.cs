using AutomaticGroupSemipureStock.Datas;
using AutomaticGroupSemipureStock.Manager;
using AutomaticGroupSemipureStock.Views;
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
    /// 自动组半纯品库编辑页面
    /// </summary>
    public partial class PageAutomaticSemipureStockEdit : Page, PageAction
    {
        // ---------------数据们-----------------
        private ObservableCollection<AutomaticStockInfo> _MainList;     // 主列表
        private ObservableCollection<AutomaticStockInfo> _CurrentList;  // 当前列表


        // ----------------筛选-------------------
        private NnCheckView CurrentCheckView;// 当前筛选视图
        private HashSet<NnCheckView> CheckViews;

        public PageAutomaticSemipureStockEdit()
        {
            InitializeComponent();
            CheckViews = new HashSet<NnCheckView>();
            Init();
        }

        private async void Init()
        {
            MainWindow.StatusBar(true, "加载数据...");
            try
            {
                List<AutomaticStockInfo> ll = null;
#if (DEBUG)
                ll = await WEBHelper.HttpGetJSONAsync<List<AutomaticStockInfo>>("http://10.11.30.155:5000/api/stockinfo/automaticstockemptcoordinates");
#else
                ll = await WEBHelper.HttpGetJSONAsync<List<AutomaticStockInfo>>("http://10.11.30.155:5004/api/stockinfo/automaticstockemptcoordinates");
#endif
                ll.Sort((x, y) =>
                {
                    if (x == null || y == null) return 0;
                    var v1 = x.Coordinate.Split('-');
                    var v2 = y.Coordinate.Split('-');
                    if (v1 == null || v2 == null || v1.Length != 3 || v2.Length != 3) return 0;
                    if (v1[1] == v2[1])
                    {
                        if (v1[2][0] == v2[2][0])
                        {
                            int.TryParse(v1[2].Substring(1), out int i1);
                            int.TryParse(v2[2].Substring(1), out int i2);
                            return i1 - i2;
                        }
                        return v1[2][0] - v2[2][0];
                    }
                    else
                    {
                        if (v1[1][0] == v2[1][0])
                        {
                            int.TryParse(v1[1].Substring(1), out int i1);
                            int.TryParse(v2[1].Substring(1), out int i2);
                            return i1 - i2;
                        }
                        return v1[1].CompareTo(v2[1]);
                    }
                });
                _MainList = new ObservableCollection<AutomaticStockInfo>();
                foreach (var v in ll)
                {
                    v.Update = UpdateAndSave;
                    _MainList.Add(v);
                }
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

        private async void UpdateAndSave(AutomaticStockInfo obj)
        {
            if (obj.WorkNo > 0)
            {
                obj.State = StockState.Filled;
                obj.AddDate = DateTime.Now;
            }
            try
            {
#if (DEBUG)
                var v = await WEBHelper.HttpPostBodyAsync("http://10.11.30.155:5000/api/stockinfo/AutomaticStockUpdate", obj);
#else
                var v = await WEBHelper.HttpPostBodyAsync("http://10.11.30.155:5004/api/stockinfo/AutomaticStockUpdate", obj);
#endif
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }


        #region 事件
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "back":        // 返回
                    MainWindow.Action?.Invoke("back");
                    break;
                case "opentable":   // 打开表格
                    OpenTable();
                    break;
                default: return;
            }
        }

        private void mSBMain_OnSearching(object sender, Views.SearcherRoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "refresh":         // 刷新
                    _removeFilter();
                    Init();
                    break;
                case "removefilter":    // 移除所有筛选
                    _removeFilter();
                    break;
                default:return;
            }
        }
        #endregion // 事件

        #region 筛选
        /// <summary>
        /// 清除所有筛选
        /// </summary>
        private void _removeFilter()
        {
            _CurrentList = _MainList;
            mDGMain.ItemsSource = _CurrentList;

            foreach (var v in CheckViews)
            {
                if (v.IsChecked) v.RemoveCheck();
            }
        }
        private void NnCheckView_OnCheck(object sender, RoutedEventArgs e)
        {
            var view = sender as NnCheckView;
            if (view == null) return;

            if (!CheckViews.Contains(view))
            {
                CheckViews.Add(view);
            }

            view.CurrentList = _CurrentList;
            if (CurrentCheckView != view)
            {
                view.ReLoadData();
            }
            CurrentCheckView = view;
        }

        private void NnCheckView_StartFilter(object sender, RoutedEventArgs e)
        {
            var nck = sender as NnCheckView;
            if (nck == null) return;
            switch (nck.CheckAction)
            {
                case "screeningok":// 筛选确定
                    break;
                case "clear":// 取消筛选
                    CurrentCheckView = null;
                    break;
                default: return;
            }
            _sereeningOKNew();
        }

        private void _sereeningOKNew()
        {
            var pairs = new Dictionary<string, NnCheckView>();
            foreach (var v in CheckViews)
            {
                if (v.IsChecked && !pairs.ContainsKey(v.Trait))
                {
                    pairs.Add(v.Trait, v);
                }
            }
            if (pairs.Count <= 0)
            {
                _CurrentList = _MainList;
                mDGMain.ItemsSource = _CurrentList;
                return;
            }
            _CurrentList = new ObservableCollection<AutomaticStockInfo>();
            foreach (var v in _MainList)
            {
                if (IsFilterPassed(v, pairs))
                {
                    _CurrentList.Add(v);
                }
            }
            mDGMain.ItemsSource = _CurrentList;
        }

        /// <summary>
        /// 筛选是否通过
        /// </summary>
        private bool IsFilterPassed(object op, Dictionary<string, NnCheckView> pairs)
        {
            foreach (var v in pairs)
            {
                if (!v.Value.CheckPassed(op))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion // 筛选

        /// <summary>
        /// 打开表格
        /// </summary>
        public async void OpenTable()
        {
            if (_CurrentList == null || _CurrentList.Count <= 0) return;

            MainWindow.StatusBar?.Invoke(true, "打开表格...");

            try
            {
                await Task.Run(() =>
                {
                    ExcelManager.OpenTable(_CurrentList);
                });
            }
            catch { MainWindow.StatusBar?.Invoke(false, "打开表格出现错误，请重试!"); return; }

            MainWindow.StatusBar?.Invoke();
        }
    }
}
