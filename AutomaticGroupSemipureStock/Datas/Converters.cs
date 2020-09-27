using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AutomaticGroupSemipureStock.Datas
{
    /// <summary>
    /// 日期转换
    /// </summary>
    public class DateConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            var v = (DateTime)value;
            if (v == DateTime.MinValue) return "";
            return v.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime.TryParse(value?.ToString(), out DateTime dt);
            return dt;
        }
    }

    /// <summary>
    /// 纯度转换
    /// </summary>
    public class PurityConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double val = (double)value;
                if (val == 0) return "";
                if (val == -1) return "Desalt";
                if (val == -2) return "Crude";
                return val.ToString("p");
            }
            catch { }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string str = value.ToString();
                double.TryParse(str, out double d);
                return d;
            }
            catch { }
            return 0;
        }
    }
}
