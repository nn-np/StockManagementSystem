using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AutomaticGroupSemipureStock.Views
{
    /// <summary>
    /// 筛选控件
    /// </summary>
    public partial class NnCheckView : UserControl, INotifyPropertyChanged
    {
        // ---------------属性-----------------
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(NnCheckView));
        /// <summary>
        /// 表头
        /// </summary>
        public string Title { get { return GetValue(TitleProperty) as string; } set { SetValue(TitleProperty, value); } }

        public static readonly DependencyProperty TraitProperty = DependencyProperty.Register("Trait", typeof(string), typeof(NnCheckView));
        /// <summary>
        /// 筛选属性特征
        /// </summary>
        public string Trait { get { return GetValue(TraitProperty) as string; } set { SetValue(TraitProperty, value); Checker = new NnCheck() { Name = value }; } }

        public static readonly DependencyProperty ConverterProperty = DependencyProperty.Register("Converter", typeof(IValueConverter), typeof(NnCheckView));
        /// <summary>
        /// 数据转换
        /// </summary>
        public IValueConverter Converter { get { return GetValue(ConverterProperty) as IValueConverter; } set { SetValue(ConverterProperty, value); } }
        // ---------------事件-----------------
        public static readonly RoutedEvent OnCheckEvent = EventManager.RegisterRoutedEvent("OnCheck", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NnCheckView));
        public event RoutedEventHandler OnCheck { add { AddHandler(OnCheckEvent, value); } remove { RemoveHandler(OnCheckEvent, value); } }
        public static readonly RoutedEvent StartFilterEvent = EventManager.RegisterRoutedEvent("StartFilter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NnCheckView));
        public event RoutedEventHandler StartFilter { add { AddHandler(StartFilterEvent, value); } remove { RemoveHandler(StartFilterEvent, value); } }
        // ----------------数据----------------
        public System.Collections.IEnumerable CurrentList { get; set; }
        // ----------------常规属性----------------
        public NnCheck Checker { get; set; }

        public string CheckAction;
        private bool ischekced;
        public bool IsChecked { get => ischekced; set { ischekced = value; _initButton(); } }

        /// <summary>
        /// 是否加载了数据
        /// </summary>
        public bool IsLoad { get => Checker != null && Checker.Loaded; }

        private DateType DateType;// 日期范围类别

        private CheckType checktype;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 筛选类别
        /// </summary>
        public CheckType CheckType
        {
            get => checktype;
            set
            {
                checktype = value;
                switch (value)
                {
                    case CheckType.Normal:
                        mGDPopNum.Visibility = Visibility.Collapsed;
                        mGDPopDate.Visibility = Visibility.Collapsed;
                        break;
                    case CheckType.Number:
                        mGDPopNum.Visibility = Visibility.Visible;
                        mGDPopDate.Visibility = Visibility.Collapsed;
                        break;
                    case CheckType.Date:
                        mGDPopNum.Visibility = Visibility.Collapsed;
                        mGDPopDate.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private double numstart;
        public double NumStart { get => numstart; set { numstart = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NumStart")); } }
        private double numend;
        public double NumEnd { get => numend; set { numend = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NumEnd")); } }

        /// <summary>
        /// 是否是百分比
        /// </summary>
        public bool IsPercentage { get; set; }
        private bool isPriority;
        /// <summary>
        /// 是否优先（对优先筛选可能做特别处理）
        /// </summary>
        public bool IsPriority { get => isPriority; set { isPriority = value; _initButton(); } }
        /// <summary>
        /// 检测是否通过
        /// </summary>
        public Func<object, bool> CheckPassed { get; internal set; }

        public NnCheckView()
        {
            InitializeComponent();
            CheckPassed = CheckPassedNormal;
        }

        public void ReLoadData()
        {
            Checker = new NnCheck() { Name = Trait };
            Checker.LoadData(CurrentList, CheckType, Converter);
        }

        /// <summary>
        /// 初始化按钮
        /// </summary>
        private void _initButton()
        {
            if (isPriority)
            {
                (mBTMain.Content as ContentControl).Template = ischekced ? Resources["bt_screening4"] as ControlTemplate : Resources["bt_screening3"] as ControlTemplate;
            }
            else
            {
                (mBTMain.Content as ContentControl).Template = ischekced ? Resources["bt_screening2"] as ControlTemplate : Resources["bt_screening1"] as ControlTemplate;
            }
        }

        #region 事件
        // -------------事件-----------------

        /// <summary>
        /// 筛选
        /// </summary>
        private void Button_Click_Filter(object sender, RoutedEventArgs e)
        {
            mPop.IsOpen = !mPop.IsOpen;
            if (!mPop.IsOpen) return;

            RoutedEventArgs args = new RoutedEventArgs(OnCheckEvent, this);
            RaiseEvent(args);

            if (CurrentList == null) return;

            if (!IsLoad)
            {
                Checker = new NnCheck() { Name = Trait };
                Checker.LoadData(CurrentList, CheckType, this.Converter);
            }
            mLBScreening.ItemsSource = Checker.Items;
            e.Handled = true;
        }

        /// <summary>
        /// 筛选确认
        /// </summary>
        private void Button_Click_FilterConfirm(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "clear":// 清除筛选（部分）
                    CheckAction = "clear";
                    RemoveCheck();
                    break;
                case "screeningok":// 筛选确定
                    CheckAction = "screeningok";
                    _initIsPassed();
                    break;
                case "screeningcancel":// 取消筛选
                    mPop.IsOpen = false;
                    return;
                default: return;
            }
            IsChecked = !Checker.IsAllChecked;
            RoutedEventArgs args = new RoutedEventArgs(StartFilterEvent, this);
            RaiseEvent(args);
            mPop.IsOpen = false;
        }

        private void _initIsPassed()
        {
            if (CheckType == CheckType.Date && DateType != DateType.Null)
            {
                CheckPassed = CheckPassedDate;
                return;
            }
            else if (CheckType == CheckType.Number && (NumStart != 0 || NumEnd != 0))
            {
                CheckPassed = CheckPassedNumber;
                return;
            }
            CheckPassed = CheckPassedNormal;
        }

        /// <summary>
        /// 筛选项目选择
        /// </summary>
        private void mLBScreening_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            NnCheckItem ic = e.AddedItems[0] as NnCheckItem;
            if (ic == null) return;
            ic.IsChecked = !ic.IsChecked;
            (sender as ListBox).SelectedIndex = -1;
        }

        private void mDPStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Checker.Items[0].IsChecked) Checker.Items[0].IsChecked = false;
            IsChecked = !Checker.IsAllChecked;
            if ((((Control)sender).Tag as string) == "start")
            {
                if (DateType != DateType.End)
                {
                    DateType = DateType.Start;
                    return;
                }
            }
            else
            {
                if (DateType != DateType.Start)
                {
                    DateType = DateType.End;
                    return;
                }
            }
            DateType = DateType.All;
        }
        #endregion

        /// <summary>
        /// 常规检测
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        internal bool CheckPassedNormal(object op)
        {
            var obj = Checker.GetValue(op);
            if (obj != null && this.Converter != null)
                obj = this.Converter.Convert(obj, typeof(string), null, null);

            return Checker.CheckPassed(obj?.ToString());
        }
        /// <summary>
        /// 数字检测
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        internal bool CheckPassedNumber(object op)
        {
            var obj = Checker.GetValue(op);
            var tp = obj.GetType();
            double d;
            if (tp != typeof(int) && tp != typeof(long) && tp != typeof(float) && tp != typeof(double))
            {
                if (obj != null && this.Converter != null)
                    obj = this.Converter.Convert(obj, typeof(string), null, null);

                string str = obj?.ToString();
                if (IsPercentage && str != null && str.Length > 0)
                {
                    str = str.Substring(0, str.Length - 1);
                }
                bool b = double.TryParse(str, out d);
                if (!b) return Checker.CheckPassed(str);
            }
            else
            {
                d = Convert.ToDouble(obj);
            }
            if (NumStart != 0 && NumEnd != 0)
            {
                return d >= NumStart && d <= NumEnd;
            }
            else if (NumStart != 0)
            {
                return d >= NumStart;
            }
            return d <= NumEnd;
        }
        /// <summary>
        /// 日期检测
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        internal bool CheckPassedDate(object op)
        {
            var obj = Checker.GetValue(op);

            DateTime dt;
            if (obj.GetType() != typeof(DateTime))
            {
                if (obj != null && this.Converter != null)
                    obj = this.Converter.Convert(obj, typeof(string), null, null);
                string str = obj?.ToString();
                bool b = DateTime.TryParse(str, out dt);
                if (!b) return Checker.CheckPassed(str);
            }
            else
            {
                dt = (DateTime)obj;
            }

            if (DateType == DateType.All)
            {
                return mDPStart.SelectedDate != null && mDPEnd.SelectedDate != null && dt >= mDPStart.SelectedDate && dt <= mDPEnd.SelectedDate;
            }
            else if (DateType == DateType.Start)
            {
                return mDPStart.SelectedDate != null && dt >= mDPStart.SelectedDate;
            }
            else
            {
                return mDPEnd.SelectedDate != null && dt <= mDPEnd.SelectedDate;
            }
        }

        private void mTBNumStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Checker == null || Checker.Items == null) return;
            if (Checker.Items[0].IsChecked) Checker.Items[0].IsChecked = false;
            IsChecked = !Checker.IsAllChecked;
        }
        /// <summary>
        /// 清除筛选
        /// </summary>
        internal void RemoveCheck()
        {
            mDPEnd.SelectedDate = mDPStart.SelectedDate = null;
            DateType = DateType.Null;
            NumStart = NumEnd = 0;
            IsChecked = false;
            Checker.IsAllChecked = true;
            if (!Checker.Items[0].IsChecked) Checker.Items[0].IsChecked = true;
        }

        internal void InitValue(List<string> items)
        {
            foreach (var v in Checker.Items)
            {
                if (v.Name == "所有") continue;
                if (!items.Contains(v.Name))
                {
                    v.IsChecked = false;
                }
            }
            IsChecked = !Checker.IsAllChecked;
        }
    }

    public class NnCheck
    {
        /// <summary>
        /// 是否选中所有
        /// </summary>
        public bool IsAllChecked { get; set; } = true;
        /// <summary>
        /// 是否已经加载数据
        /// </summary>
        public bool Loaded { get; internal set; }
        private Dictionary<string, NnCheckItem> selectedPairs;
        public Dictionary<string, NnCheckItem> SelectedItems { get { if (selectedPairs == null) selectedPairs = new Dictionary<string, NnCheckItem>(); return selectedPairs; } set { selectedPairs = value; } }
        /// <summary>
        /// Name 属性的Path
        /// </summary>
        public string Name { get; set; }
        private Dictionary<string, NnCheckItem> pairs;
        public Dictionary<string, NnCheckItem> ItemsDictionary { get { if (pairs == null) pairs = new Dictionary<string, NnCheckItem>(); return pairs; } set => pairs = value; }
        private List<NnCheckItem> items;
        public List<NnCheckItem> Items
        {
            get
            {
                if (items == null || items.Count != pairs.Count)
                {
                    items = new List<NnCheckItem>();
                    foreach (var v in ItemsDictionary)
                    {
                        items.Add(v.Value);
                    }
                    if (CheckType != CheckType.Normal)
                    {
                        SortItems(items);
                    }
                }
                return items;
            }
            set => items = value;
        }

        /// <summary>
        /// 筛选类别
        /// </summary>
        public CheckType CheckType { get; set; }

        public bool Add(string name)
        {
            if (!pairs.ContainsKey(name))
            {
                NnCheckItem nci = new NnCheckItem() { Name = name, IsChecked = true, OnCheckChanged = ItemCheckChanged };
                pairs.Add(name, nci);
                SelectedItems.Add(nci.Name, nci);
                ++ChekcedCount;
            }
            return pairs.Count <= 10000;
        }

        public NnCheck()
        {
            pairs = new Dictionary<string, NnCheckItem>();
            NnCheckItem nci = new NnCheckItem() { Name = "所有", IsChecked = true, OnCheckChanged = AllChecked };
            pairs.Add(nci.Name, nci);
        }
        /// <summary>
        /// 所有被选中
        /// </summary>
        /// <param name="obj"></param>
        private void AllChecked(NnCheckItem obj)
        {
            IsAllChecked = obj.IsChecked;
            if (!IsAllChecked)
            {
                SelectedItems.Clear();
            }
            foreach (var v in Items)
            {
                if (v == obj) continue;
                v.IsCheckedPrivate = obj.IsChecked;
                if (obj.IsChecked)
                {
                    if (!SelectedItems.ContainsKey(v.Name))
                        SelectedItems.Add(v.Name, v);
                }
                else
                {
                    if (SelectedItems.ContainsKey(v.Name))
                        SelectedItems.Remove(v.Name);
                }
            }
            ChekcedCount = obj.IsChecked ? items.Count - 1 : 0;
        }

        /// <summary>
        /// Item选择事件被改变
        /// </summary>
        private void ItemCheckChanged(NnCheckItem obj)
        {
            if (obj.IsChecked)
            {
                SelectedItems.Add(obj.Name, obj);
            }
            else
            {
                SelectedItems.Remove(obj.Name);
            }
            ChekcedCount += obj.IsChecked ? 1 : -1;
            IsAllChecked = ChekcedCount >= (Items.Count - 1);
            if (IsAllChecked != items[0].IsCheckedPrivate)
            {
                items[0].IsCheckedPrivate = IsAllChecked;
            }
        }
        /// <summary>
        /// 被选中的数量
        /// </summary>
        private int ChekcedCount;

        /// <summary>
        /// 加载数据初始化筛选
        /// </summary>
        internal void LoadData(System.Collections.IEnumerable listOrders, CheckType checkType = CheckType.Normal, IValueConverter converter = null)
        {
            if (listOrders == null) return;
            CheckType = checkType;
            Loaded = true;
            bool isSpace = false;// 是否含有空白字符
            foreach (var v in listOrders)
            {
                var obj = GetValue(v);
                if (converter != null)
                {
                    obj = converter.Convert(obj, typeof(string), null, null);
                }
                string s = obj?.ToString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    isSpace = true;
                    continue;
                }
                bool b = this.Add(s);
            }
            if (isSpace)
            {
                this.Add("（空白）");
            }
        }

        private void SortItems(List<NnCheckItem> items)
        {
            if (CheckType == CheckType.Date)
            {
                items.Sort((x, y) =>
                {
                    if (x.Name == "所有") return -1;
                    if (x.Name == "（空白）") return 1;
                    if (y.Name == "所有") return 1;
                    if (y.Name == "（空白）") return -1;
                    bool b1 = DateTime.TryParse(x.Name, out DateTime d1);
                    bool b2 = DateTime.TryParse(y.Name, out DateTime d2);
                    if (b1 && b2)
                    {
                        return (int)(d1 - d2).TotalSeconds;
                    }
                    else if (!b1 && !b2)
                    {
                        return x.Name.CompareTo(y.Name);
                    }
                    else if (b1)
                    {
                        return 1;
                    }
                    return -1;
                });
            }
            else if (CheckType == CheckType.Number)
            {
                items.Sort((x, y) =>
                {
                    if (x.Name == "所有") return -1;
                    if (x.Name == "（空白）") return 1;
                    if (y.Name == "所有") return 1;
                    if (y.Name == "（空白）") return -1;
                    bool b1 = double.TryParse(x.Name, out double d1);
                    bool b2 = double.TryParse(y.Name, out double d2);
                    if (b1 && b2)
                    {
                        return (int)(d1 - d2);
                    }
                    else if (!b1 && !b2)
                    {
                        return x.Name.CompareTo(y.Name);
                    }
                    else if (b1)
                    {
                        return 1;
                    }
                    return -1;
                });
            }
        }

        internal bool CheckPassed(string op)
        {
            // 其他情况
            if (string.IsNullOrWhiteSpace(op)) return SelectedItems.ContainsKey("（空白）");
            return SelectedItems.ContainsKey(op);
        }

        public object GetValue(object op)
        {
            string[] names = Name.Split('.');
            object obj = null;
            if (names.Length == 1)
            {
                obj = op.GetType().GetProperty(Name).GetValue(op);
            }
            else
            {
                var v1 = op.GetType().GetProperty(names[0])?.GetValue(op);
                if (v1 != null)
                    obj = v1.GetType().GetProperty(names[1])?.GetValue(v1);
            }
            return obj;
        }
    }
    public class NnCheckItem : INotifyPropertyChanged
    {
        public Action<NnCheckItem> OnCheckChanged { get; set; }
        private bool isChecked;
        public bool IsCheckedPrivate { get => isChecked; set { isChecked = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked")); } }
        public bool IsChecked { get => isChecked; set { isChecked = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked")); OnCheckChanged?.Invoke(this); } }
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// 筛选类别
    /// </summary>
    public enum CheckType
    {
        Normal,
        Date,
        Number,
    }

    enum DateType
    {
        Null,
        Start,
        End,
        All
    }
}
