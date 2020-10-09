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
    /// 搜索页面
    /// </summary>
    public partial class PageSearch : Page
    {
        // ---------------数据们-----------------
        private ObservableCollection<AutomaticStockInfo> _MainList;     // 主列表
        private ObservableCollection<AutomaticStockInfo> _CurrentList;  // 当前列表

        // ----------------筛选-------------------
        private NnCheckView CurrentCheckView;// 当前筛选视图
        private HashSet<NnCheckView> CheckViews;

        public PageSearch(string obj)
        {
            InitializeComponent();
            mSBMain.Text = obj;
            CheckViews = new HashSet<NnCheckView>();
            Init(obj);
        }

        private async void Init(string obj)
        {
            List<SearchValue> ll = GetSerachValue(obj);
            MainWindow.StatusBar(true, "加载数据...");
            try
            {
#if (DEBUG)
                _MainList = await WEBHelper.HttpGetJSONAsync<ObservableCollection<AutomaticStockInfo>>("http://10.11.30.155:5000/api/stockinfo/AutomaticStockSearch",ll);
#else
                _MainList = await WEBHelper.HttpGetJSONAsync<ObservableCollection<AutomaticStockInfo>>("http://10.11.30.155:5004/api/stockinfo/AutomaticStockSearch",ll);
#endif
                foreach (var v in _MainList)
                {
                    v.Update = UpdateAndSave;
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

        /// <summary>
        /// 获取要搜索的字符
        /// </summary>
        private List<SearchValue> GetSerachValue(string obj)
        {
            if (string.IsNullOrWhiteSpace(obj)) return null;
            List<SearchValue> ll = new List<SearchValue>();
            var vs = obj.Split('\n');
            foreach(var v in vs)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                var vv = v.Trim();
                bool b = long.TryParse(vv, out long l);
                if (b)
                {
                    ll.Add(new SearchValue() { Type = SearchType.WorkNo, Value = vv });
                }
                else
                {
                    ll.Add(new SearchValue() { Type = SearchType.Coordinate, Value = vv });
                }
            }

            return ll;
        }

        private async void UpdateAndSave(AutomaticStockInfo obj)
        {
            try
            {
#if (DEBUG)
                var v = await WEBHelper.HttpPostBodyAsync("http://10.11.30.155:5000/api/stockinfo/AutomaticStockUpdate", obj);
#else
                var v = await WEBHelper.HttpPostBodyAsync("http://10.11.30.155:5004/api/stockinfo/AutomaticStockUpdate", obj);
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                MainWindow.StatusBar(false, $"数据更新出现错误：{e.Message},ID：{obj.ID}");
            }
        }

        #region 事件
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "opentable":// 打开表格
                    OpenTable();
                    break;
                case "backprevious":// 返回
                    MainWindow.Action?.Invoke("backprevious");
                    break;
                default:return;
            }
        }

        private async void OpenTable()
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "removefilter":// 清除所有筛选
                    _removeFilter();
                    break;
                default:return;
            }
        }
        /// <summary>
        /// 搜索
        /// </summary>
        private void mSBMain_OnSearching(object sender, Views.SearcherRoutedEventArgs e)
        {
            Init(e.SearchValue);
        }
        #endregion// 事件

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
    }

    /// <summary>
    /// 搜索内容
    /// </summary>
    struct SearchValue
    {
        public string Value { get; set; }
        public SearchType Type { get; set; }
    }

    /// <summary>
    /// 搜索类型
    /// </summary>
    enum SearchType
    {
        WorkNo,
        Coordinate
    }
}
