using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace data
{
    // 这个类是服务性质的，我认为这个类不需要检查异常，让外面调用者检查并判断
    // 6B99EA9FBBD04700F4C0FCD4DA705623
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
            client.Close();
            return Encoding.Default.GetString(buffer, 0, len);
        }

        // 获得Md5字符串
        public static string GetMD5String(string str)
        {
            using(MD5 md5 = new MD5Cng())
            {
                byte[] values = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToString(values).Replace("-", "");
            }
        }

        static string NnDecrypt(string decryptStr)
        {
            byte[] decrypt = Convert.FromBase64String(decryptStr);
            using (RijndaelManaged rDel = new RijndaelManaged())
            {
                rDel.Key = Encoding.UTF8.GetBytes("3456gh.dfr4u.pbcl_ser-215gfodcxs");
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform transform = rDel.CreateDecryptor())
                {
                    byte[] result = transform.TransformFinalBlock(decrypt, 0, decrypt.Length);
                    return Encoding.UTF8.GetString(result);
                }
            }
        }
    }
}
