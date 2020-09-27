using AutomaticGroupSemipureStock.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomaticGroupSemipureStock.Views
{
    /// <summary>
    /// 可批量编辑DataGrid控件
    /// </summary>
    class DataGridCanEdit : DataGrid
    {
        // -----------------搜索相关--------------------
        private string CurrentCellName;
        private string CurrentCellTag;
        private Windows.SearchWindow SearchWindow;
        private DataGridRow _selectedRow;

        public DataGridCanEdit() : base()
        {
            CommandManager.RegisterClassCommandBinding(
               typeof(DataGridCanEdit),
               new CommandBinding(
                   ApplicationCommands.Paste,
                   new ExecutedRoutedEventHandler(ExecutedPaste),
                   new CanExecuteRoutedEventHandler(CanExecutePaste)));

            // 重复
            var RUICRepeat = new RoutedUICommand("Repeat Value", "Repeat", typeof(DataGridCanEdit));
            KeyBinding KBRepeat = new KeyBinding(RUICRepeat, new KeyGesture(Key.D, ModifierKeys.Control));
            // 搜索
            var RUICFind = new RoutedUICommand("Find Value", "Find", typeof(DataGridCanEdit));
            KeyBinding KBFind = new KeyBinding(RUICFind, new KeyGesture(Key.F, ModifierKeys.Control));

            this.InputBindings.Add(KBRepeat);
            this.InputBindings.Add(KBFind);

            CommandManager.RegisterClassCommandBinding(
               typeof(DataGridCanEdit),
               new CommandBinding(
                   RUICRepeat,
                   new ExecutedRoutedEventHandler(ExecutedRepeat),
                   new CanExecuteRoutedEventHandler(CanExecuteRepeat)));

            CommandManager.RegisterClassCommandBinding(
               typeof(DataGridCanEdit),
               new CommandBinding(
                   RUICFind,
                   new ExecutedRoutedEventHandler(ExecutedFind),
                   new CanExecuteRoutedEventHandler(CanExecuteFind)));

            this.CurrentCellChanged += DataGridCanEdit_CurrentCellChanged;

            this.KeyDown += DataGridCanEdit_KeyDown;

            this.MouseDoubleClick += DataGridCanEdit_MouseDoubleClick;
        }

        private void DataGridCanEdit_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CurrentCell == null) return;
            DataGridRow row = ItemContainerGenerator.ContainerFromItem(CurrentCell.Item) as DataGridRow;
            if (row == null) return;
            if (row.DetailsVisibility == Visibility.Visible)
            {
                row.DetailsVisibility = Visibility.Collapsed;
            }
            else
            {
                row.DetailsVisibility = Visibility.Visible;
                _selectedRow = row;
            }
        }

        /// <summary>
        /// CurrentCell改变事件
        /// </summary>
        private void DataGridCanEdit_CurrentCellChanged(object sender, EventArgs e)
        {
            if (this.CurrentCell == null) return;

            var vs = CurrentCell.Column?.Header as NnCheckView;
            if (vs == null) return;
            CurrentCellTag = vs.Trait;
            CurrentCellName = vs.Title;

            if (SearchWindow != null && SearchWindow.IsRunning)
            {
                SearchWindow.TheTitle = vs.Title;
                SearchWindow.CellTag = vs.Trait;
            }
            if (_selectedRow != null && _selectedRow.DetailsVisibility == Visibility.Visible)
            {
                _selectedRow.DetailsVisibility = Visibility.Collapsed;
                _selectedRow = null;
            }
            //this.BeginEdit();
        }

        /// <summary>
        /// KeyDown
        /// </summary>
        private void DataGridCanEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                this.CommitEdit();
                this.Focus();
            }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        private void ExecutedFind(object sender, ExecutedRoutedEventArgs e)
        {
            SearchWindow = Windows.SearchWindow.Search(CurrentCellName, CurrentCellTag, this);
        }
        /// <summary>
        /// 搜索
        /// </summary>
        private void CanExecuteFind(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// 重复
        /// </summary>
        private void CanExecuteRepeat(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        /// <summary>
        /// 重复
        /// </summary>
        private void ExecutedRepeat(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedCells.Count <= 0) return;
            try
            {
                object firstItem = SelectedCells[0].Item;

                HashSet<object> cs = new HashSet<object>();

                Dictionary<DataGridColumn, object> pairs = new Dictionary<DataGridColumn, object>();
                foreach (var v in this.SelectedCells)
                {
                    if (v.Item == firstItem)
                    {
                        var cv = v.Column.Header as NnCheckView;
                        if (cv == null) continue;
                        var obj = v.Item.GetValue(cv.Trait);
                        pairs.Add(v.Column, obj);
                    }
                    else
                    {
                        cs.Add(v.Item);
                    }
                }
                foreach (var v in cs)
                {
                    foreach (var vv in pairs)
                    {
                        vv.Key.OnPastingCellClipboardContent(v, vv.Value);
                    }
                }
            }
            catch { }

            // 确保每次编辑结束之后更新数据
            this.CommitEdit();
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        private void CanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        /// <summary>
        /// 粘贴
        /// </summary>
        private void ExecutedPaste(object sender, ExecutedRoutedEventArgs e)
        {

            try
            {
                List<string[]> rowData = _parseClipboardData();
                if (rowData.Count < 1) return;

                var _currentCell = this.CurrentCell;

                if (_currentCell == null) return;

                int rowIndex = this.Items.IndexOf(_currentCell.Item);
                int columnIndex = _currentCell.Column.DisplayIndex;

                int rind = rowIndex;
                for (int i = 0; rowIndex < this.Items.Count && i < rowData.Count; ++i, ++rowIndex)
                {
                    int columnind = columnIndex;
                    for (int j = 0; j < rowData[i].Length && columnind < this.Columns.Count; ++j, ++columnind)
                    {
                        DataGridColumn colum = this.ColumnFromDisplayIndex(columnind);

                        colum.OnPastingCellClipboardContent(this.Items[rowIndex], rowData[i][j].TrimEnd());
                        if (rowIndex > rind && columnind == columnIndex)
                        {
                            colum.OnPastingCellClipboardContent(this.Items[rowIndex], rowData[i][j].TrimEnd());
                        }
                    }
                }
            }
            catch { }

            // 确保每次编辑结束之后更新数据
            this.CommitEdit();
        }

        // 从剪贴板获取数据
        private List<string[]> _parseClipboardData()
        {
            List<string[]> list = new List<string[]>();
            string text = Clipboard.GetText();
            string[] texts = text.Split('\n');
            foreach (var v in texts)
            {
                list.Add(v?.Split('\t'));
            }
            if (list[list.Count - 1].Length <= 0 || (list[list.Count - 1].Length == 1 && string.IsNullOrEmpty(list[list.Count - 1][0])))
            {
                list.RemoveAt(list.Count - 1);
            }
            return list;
        }
    }
}
