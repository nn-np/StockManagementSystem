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
using System.Windows.Shapes;

namespace AutomaticGroupSemipureStock.Windows
{
    /// <summary>
    /// 搜索窗口页面
    /// </summary>
    public partial class SearchWindow : Window
    {
        private static SearchWindow mWindow;
        private string title;
        public string TheTitle { get => title; set { title = value; mTBTitle.Text = string.IsNullOrWhiteSpace(value) ? "查找内容：" : $"在[{value}]中查找："; } }
        public bool IsRunning { get; set; }

        public string CellTag { get; set; }// 字符Tag
        public DataGrid mDGMain { get; set; }// 表格

        private int SearchIndex = 0;
        public SearchWindow(string title = null)
        {
            InitializeComponent();
            TheTitle = title;
            IsRunning = true;
            init();
        }

        private void init()
        {
            mTBTitle.Text = string.IsNullOrWhiteSpace(TheTitle) ? "查找内容：" : $"在[{TheTitle}]中查找：";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mWindow.IsRunning = false;
            mWindow = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Control)sender).Tag)
            {
                case "search":
                    Search();
                    break;
                default: return;
            }
        }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(mTBContent.Text)) return;
            string str = mTBContent.Text.ToLower();
            for (int i = SearchIndex; i < mDGMain.Items.Count; ++i)
            {
                if (CellTag == null) break;
                object obj = mDGMain.Items[i];
                var val = obj.GetType().GetProperty(CellTag)?.GetValue(obj)?.ToString();
                bool b;
                if (mCBFCM.IsChecked == true)
                    b = val.ToLower() == str;
                else
                    b = val.ToLower().Contains(str);
                if (val != null && b)
                {
                    mDGMain.ScrollIntoView(obj);
                    mDGMain.SelectedIndex = i;
                    mDGMain.Focus();
                    SearchIndex = i + 1;
                    return;
                }
                SearchIndex = (i + 1) < mDGMain.Items.Count ? i + 1 : 0;
            }
            SearchIndex = 0;
            WarnWindow.ShowMessage("没有查找到此内容！");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mTBContent.Focus();
        }

        public static SearchWindow Search(string value, string currentCellTag, DataGrid dg)
        {
            if (mWindow == null)
            {
                mWindow = new SearchWindow(value);
                mWindow.Owner = Application.Current.MainWindow;
                mWindow.Show();
            }
            else
            {
                mWindow.Activate();
            }
            mWindow.IsRunning = true;
            mWindow.CellTag = currentCellTag;
            mWindow.mDGMain = dg;

            return mWindow;
        }
    }
}
