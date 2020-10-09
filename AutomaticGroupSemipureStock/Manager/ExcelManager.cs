using AutomaticGroupSemipureStock.Datas;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticGroupSemipureStock.Manager
{
    class ExcelManager
    {
        /// <summary>
        /// 打开表格
        /// </summary>
        /// <param name="currentList"></param>
        internal static void OpenTable(ObservableCollection<AutomaticStockInfo> currentList)
        {
            NnExcel ne = new NnExcel();
            ne.Visible = true;

            object[][] vs = new object[currentList.Count+1][];
            vs[0] = new object[] { "ID", "WO NO", "Operation Name", "Status", "Purity Group", "Purity User", "Comments", "Comments", "半纯品量", "库存量", "半纯品纯度", "QC/LC2030分析", "库存坐标", "添加日期", "取出日期" };
            int ind = 0;
            foreach(var v in currentList)
            {
                vs[++ind] = new object[] { v.ID, v.WorkNo, v.OperationName, v.Status, v.PurGroup, v.PurUser, v.Comments, v.UserComments, v.Quality, v.StockQuality, v.Purity, v.AnalyticalInstruments, v.Coordinate, v.AddDate.ToShortDateString(), v.RemoveDate.ToShortDateString() };
            }
            ne.AddValue(vs, 1, 15);
            ne.SetColor(1, 1, 1, 15, NnExcel.ColorType.Blue);
            ne.AutoFit(4, 15);
        }
    }

    class NnExcel : IDisposable
    {
        private Application app;
        private Workbook book;

        private bool IsFirst = true;// 是否是第一次运行
        public bool Visible { get => app.Visible; set => app.Visible = value; }
        public Application App { get => app; }

        public Worksheet CurrentSheet { get; set; }
        public NnExcel()
        {
            app = new Application();
        }

        public void Dispose()
        {
            try
            {
                if (book != null)
                {
                    book.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(book);
                }
                if (app != null)
                {
                    app.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
                }
            }
            catch { }
        }

        public void AddValue(object[][] values, int rindex, int cs)
        {
            try
            {
                if (book == null) book = app.Workbooks.Add();
                Microsoft.Office.Interop.Excel.Worksheet sheet = book.Worksheets[1];
                CurrentSheet = sheet;

                object[,] vs = new object[values.Length, cs];
                for (int i = 0; i < vs.Length && i < values.Length; ++i)
                {
                    if (values[i] == null) continue;
                    for (int j = 0; j < values[i].Length && j < cs; ++j)
                    {
                        vs[i, j] = values[i][j];
                    }
                }

                Microsoft.Office.Interop.Excel.Range rg = sheet.Range[sheet.Cells[rindex, 1], sheet.Cells[values.Length + rindex - 1, cs]];
                rg.Value = vs;

                IsFirst = false;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public void AddValueWithName(object[,] vs, int rindex, string name)
        {
            try
            {
                var sheet = _initSheet(name);

                Range rg = sheet.Range[sheet.Cells[rindex, 1], sheet.Cells[vs.GetLength(0) + rindex - 1, vs.GetLength(1)]];
                rg.Value = vs;

                IsFirst = false;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        private Worksheet _initSheet(string name = null)
        {
            if (book == null) book = app.Workbooks.Add();
            Worksheet sheet;
            if (IsFirst)
                sheet = book.Worksheets[1];
            else
                sheet = book.Worksheets.Add(Type.Missing, CurrentSheet);
            CurrentSheet = sheet;
            if (!string.IsNullOrEmpty(name))
                sheet.Name = name;

            return sheet;
        }

        public void AddValueWithName(object[][] values, int rindex, int cs, string name)
        {
            try
            {
                if (book == null) book = app.Workbooks.Add();
                Microsoft.Office.Interop.Excel.Worksheet sheet;
                if (IsFirst)
                    sheet = book.Worksheets[1];
                else
                    sheet = book.Worksheets.Add(Type.Missing, CurrentSheet);
                CurrentSheet = sheet;
                sheet.Name = name;

                object[,] vs = new object[values.Length, cs];
                for (int i = 0; i < vs.Length && i < values.Length; ++i)
                {
                    if (values[i] == null) continue;
                    for (int j = 0; j < values[i].Length && j < cs; ++j)
                    {
                        vs[i, j] = values[i][j];
                    }
                }

                Microsoft.Office.Interop.Excel.Range rg = sheet.Range[sheet.Cells[rindex, 1], sheet.Cells[values.Length + rindex - 1, cs]];
                rg.Value = vs;

                IsFirst = false;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        internal void AddValueWithName(Worksheet st, object[][] values, int rindex, int cs, string name)
        {
            try
            {
                if (book == null) book = app.Workbooks.Add();
                st.Copy(Type.Missing, st);
                Microsoft.Office.Interop.Excel.Worksheet sheet = book.Worksheets[2];
                CurrentSheet = sheet;
                sheet.Name = name;

                object[,] vs = new object[values.Length, cs];
                for (int i = 0; i < vs.Length && i < values.Length; ++i)
                {
                    if (values[i] == null) continue;
                    for (int j = 0; j < values[i].Length && j < cs; ++j)
                    {
                        vs[i, j] = values[i][j];
                    }
                }

                Microsoft.Office.Interop.Excel.Range rg = sheet.Range[sheet.Cells[rindex, 1], sheet.Cells[values.Length + rindex - 1, cs]];
                rg.Value = vs;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        /// <summary>
        /// 自动列宽
        /// </summary>
        public void AutoFit(params int[] vs)
        {
            if (CurrentSheet == null) return;
            Range rg1 = CurrentSheet.UsedRange;
            foreach (var v in vs)
            {
                Range r = rg1.Range[rg1.Cells[1, v], rg1.Cells[rg1.Rows.Count, v]];
                r.Columns.AutoFit();
            }
        }

        public void SetColor(int x, int y, int x1, int y1, ColorType ct = ColorType.Blue)
        {
            Range rg1 = CurrentSheet.Range[CurrentSheet.Cells[x, y], CurrentSheet.Cells[x1, y1]];
            switch (ct)
            {
                case ColorType.Blue:
                    rg1.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(142, 169, 219));
                    break;
                case ColorType.Orange:
                    rg1.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(255, 217, 102));
                    break;
                case ColorType.Gree:
                    rg1.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(0xA9, 0xD0, 0x8E));
                    break;
            }
        }

        /// <summary>
        /// 颜色类别
        /// </summary>
        public enum ColorType
        {
            /// <summary>
            /// 蓝色
            /// </summary>
            Blue,
            /// <summary>
            /// 橙色
            /// </summary>
            Orange,
            /// <summary>
            /// 绿色
            /// </summary>
            Gree,
        }
    }
}
