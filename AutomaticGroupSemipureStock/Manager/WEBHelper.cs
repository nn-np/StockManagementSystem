using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutomaticGroupSemipureStock.Manager
{
    class WEBHelper
    {
        /// <summary>
        /// 全局JSON配置
        /// </summary>
        private static readonly JsonSerializerOptions _jso = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        /// <summary>
        /// 使用POST请求数据，无返回值，用于更新与添加，数据以JSON形式附加在Body中
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="jval">附加的数据</param>
        public static WEBMessage HttpPostBody(string url, object jval)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.ContentType = "application/json";// ;charset=utf-8  application/x-www-form-urlencoded
            if (jval != null)
            {
                var vl = JsonSerializer.Serialize(jval, _jso);
                var vs = Encoding.Default.GetBytes(vl);
                using (Stream st = request.GetRequestStream())
                {
                    st.Write(vs, 0, vs.Length);
                }
            }
            WebResponse res = request.GetResponse();
            string val = "";
            using (StreamReader reader = new StreamReader(res.GetResponseStream()))
            {
                val = reader.ReadToEnd();
            }
            return new WEBMessage() { StatusCode = ((HttpWebResponse)res).StatusCode, Message = val };
        }
        /// <summary>
        /// 异步使用POST请求数据，无返回值，用于更新与添加，数据以JSON形式附加在Body中
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="jval">附加的数据</param>
        public static async Task<WEBMessage> HttpPostBodyAsync(string url, object jval)
        {
            return await Task.Run(() => HttpPostBody(url, jval));
        }
        /// <summary>
        /// 使用POST请求数据（只能请求返回值为JSON格式的数据）
        /// </summary>
        /// <typeparam name="T">得到的数据</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="y">请求参数</param>
        /// <returns>请求类型</returns>
        public static T HttpPostJSON<T>(string url, object jval)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.ContentType = "application/json";// ;charset=utf-8
            if (jval != null)
            {
                var vl = JsonSerializer.Serialize(jval, _jso);
                var vs = Encoding.Default.GetBytes(vl);
                using (Stream st = request.GetRequestStream())
                {
                    st.Write(vs, 0, vs.Length);
                }
            }
            WebResponse res = request.GetResponse();
            string val = "";
            using (StreamReader reader = new StreamReader(res.GetResponseStream()))
            {
                val = reader.ReadToEnd();
            }
            request.Abort();
            if (string.IsNullOrEmpty(val)) return default;
            var tt = JsonSerializer.Deserialize<T>(val, _jso);
            return tt;
        }

        /// <summary>
        /// 使用POST请求数据，异步方法（只能请求返回值为JSON格式的数据）
        /// </summary>
        /// <typeparam name="T">得到的数据</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="y">请求参数</param>
        /// <returns>请求类型</returns>
        public static async Task<T> HttpPostJSONAsync<T>(string url, object y)
        {
            return await Task.Run(() =>
            {
                return HttpPostJSON<T>(url, y);
            });
        }

        /// <summary>
        /// 使用GET请求数据（只能请求返回值为JSON格式的数据）
        /// </summary>
        /// <typeparam name="T">得到的数据</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="y">请求参数</param>
        /// <returns>请求类型</returns>
        public static T HttpGetJSON<T>(string url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            WebResponse res = request.GetResponse();
            string val = "";
            using (StreamReader reader = new StreamReader(res.GetResponseStream()))
            {
                val = reader.ReadToEnd();
            }
            request.Abort();
            var tt = JsonSerializer.Deserialize<T>(val, _jso);
            return tt;
        }

        /// <summary>
        /// 使用GET请求数据，异步方法（只能请求返回值为JSON格式的数据）
        /// </summary>
        /// <typeparam name="T">得到的数据</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="y">请求参数</param>
        /// <returns>请求类型</returns>
        public static async Task<T> HttpGetJSONAsync<T>(string url)
        {
            return await Task.Run(() =>
            {
                return HttpGetJSON<T>(url);
            });
        }
    }

    public struct WEBMessage
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
