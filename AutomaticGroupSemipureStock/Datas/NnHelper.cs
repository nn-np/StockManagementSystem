using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticGroupSemipureStock.Datas
{
    public static class NnHelper
    {
        /// <summary>
        /// 获取值
        /// </summary>
        public static object GetValue(this object op, string name)
        {
            string[] names = name.Split('.');
            object obj = null;
            if (names.Length == 1)
            {
                obj = op.GetType().GetProperty(name)?.GetValue(op);
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
}
