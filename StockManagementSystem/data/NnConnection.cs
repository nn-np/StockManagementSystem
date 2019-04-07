using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace data
{
    // 这个类是服务性质的，我认为这个类不需要检查异常，让外面调用者检查并判断
    /// <summary>
    /// 用于获取配置文件
    /// </summary>
    class NnConnection
    {
        /// <summary>
        /// 通过网路获取配置文件，使用这个方法的时候注意异常处理
        /// </summary>
        public static string GetString(string instructions)
        {
            TcpClient client = new TcpClient("47.105.178.132", 9012);
            byte[] buffer = new byte[1024];
            client.Client.Send(Encoding.Default.GetBytes(instructions));
            int len = client.Client.Receive(buffer);
            client.Dispose();
            client.Close();
            return Encoding.Default.GetString(buffer, 0, len);
        }
    }
}
