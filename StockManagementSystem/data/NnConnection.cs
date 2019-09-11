using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace data
{
    /// <summary>
    /// 用于获取配置文件
    /// </summary>
    class NnConnection
    {

        // 获得Md5字符串
        public static string GetMD5String(string str)
        {
            using(MD5 md5 = new MD5Cng())
            {
                byte[] values = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToString(values).Replace("-", "");
            }
        }

        public static string NnDecrypt(string decryptStr)
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
