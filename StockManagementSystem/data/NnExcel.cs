using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagementSystem.data
{
    class NnExcel : IDisposable
    {
        private Microsoft.Office.Interop.Excel.Application app;
        private Microsoft.Office.Interop.Excel.Workbook book;

        private bool IsFirst = true;// 是否是第一次运行
        public bool Visible { get => app.Visible; set => app.Visible = value; }
        public Microsoft.Office.Interop.Excel.Application App { get => app; }

        public Microsoft.Office.Interop.Excel.Worksheet CurrentSheet { get; set; }

        public Microsoft.Office.Interop.Excel.Workbook Workbook { get => book; }
        public NnExcel()
        {
            app = new Microsoft.Office.Interop.Excel.Application();
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

        /// <summary>
        /// 自动列宽
        /// </summary>
        public void AutoFit(int[] vs)
        {
            if (CurrentSheet == null) return;
            Microsoft.Office.Interop.Excel.Range rg1 = CurrentSheet.UsedRange;
            foreach (var v in vs)
            {
                Microsoft.Office.Interop.Excel.Range r = rg1.Range[rg1.Cells[1, v], rg1.Cells[rg1.Rows.Count, v]];
                r.Columns.AutoFit();
            }
        }

        public void SetColor(NPoint p1, NPoint p2, ColorType ct)
        {
            Microsoft.Office.Interop.Excel.Range rg1 = CurrentSheet.Range[CurrentSheet.Cells[p1.X, p1.Y], CurrentSheet.Cells[p2.X, p2.Y]];
            if (ct == ColorType.Blue)
            {
                rg1.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(142, 169, 219));
            }
            else
            {
                rg1.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 217, 102));
            }
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
        }
    }

    struct NPoint
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y { get; set; }

        public static NPoint GetPoint(int x, int y)
        {
            return new NPoint() { X = x, Y = y };
        }
    }
}
