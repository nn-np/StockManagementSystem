using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace data
{
    /// <summary>
    /// 用于获取配置文件
    /// </summary>
    class NnConnection
    {
        private TcpClient m_client;
        private NetworkStream m_stream;
        /// <summary>
        /// 通过网路获取配置文件，使用这个方法的时候注意异常处理
        /// </summary>
        public NnConnection()
        {
            m_client = new TcpClient("47.105.178.132", 9012);
        }

        public void Close()
        {
            m_stream.Close();
            m_client.Dispose();
            m_client.Close();
        }

        public string GetString(string instructions)
        {
            if (m_stream == null) m_stream = m_client.GetStream();
            byte[] buffer = new byte[1024];
            m_client.Client.Send(Encoding.Default.GetBytes(instructions));
            int len = m_stream.Read(buffer, 0, buffer.Length);
            return Encoding.Default.GetString(buffer, 0, len);
        }
    }
}
