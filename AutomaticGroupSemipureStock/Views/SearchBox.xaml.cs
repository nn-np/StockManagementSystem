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

namespace AutomaticGroupSemipureStock.Views
{
    /// <summary>
    /// 搜索框
    /// </summary>
    public partial class SearchBox : UserControl
    {
        // ---------------属性-----------------
        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string), typeof(SearchBox));
        /// <summary>
        /// 提示字符
        /// </summary>
        public string HintText { get { return GetValue(HintTextProperty) as string; } set { SetValue(HintTextProperty, value); } }

        // ------------------事件---------------------
        public static readonly RoutedEvent OnSearchingEvent = EventManager.RegisterRoutedEvent("OnSearching", RoutingStrategy.Bubble, typeof(EventHandler<SearcherRoutedEventArgs>), typeof(SearchBox));
        public event RoutedEventHandler OnSearching { add { AddHandler(OnSearchingEvent, value); } remove { RemoveHandler(OnSearchingEvent, value); } }

        public string Text { set => mTBMain.Text = value; }

        public SearchBox()
        {
            InitializeComponent();
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mTBMain.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearcherRoutedEventArgs args = new SearcherRoutedEventArgs(OnSearchingEvent, this);
            args.SearchValue = mTBMain.Text;
            RaiseEvent(args);
        }
    }

    public class SearcherRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 搜索信息
        /// </summary>
        public string SearchValue { get; set; }

        public SearcherRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }

    }
}
