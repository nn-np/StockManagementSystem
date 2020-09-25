using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagementSystem.data
{
    /// <summary>
    /// 通用工具类
    /// </summary>
    class Tools
    {
        /// <summary>
        /// 设置配置文件
        /// </summary>
        public static void SetConfiguration(string key, string value)
        {
            try
            {
                Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (cfg.AppSettings.Settings[key] == null)
                {
                    cfg.AppSettings.Settings.Add(key, value);
                }
                else
                {
                    cfg.AppSettings.Settings[key].Value = value;
                }
                cfg.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch { }
        }
    }
}
