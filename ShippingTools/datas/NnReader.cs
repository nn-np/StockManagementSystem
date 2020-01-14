using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShippingTools
{
    class NnReader
    {
        private static NnReader mReader;
        private static readonly object locker = new object();

        private MySqlConnection mConnection;
        public static NnReader Instance
        {
            get
            {
                if (mReader == null)
                {
                    lock (locker)
                    {
                        if (mReader == null)
                        {
                            mReader = new NnReader();
                        }
                    }
                }
                else if (mReader.mConnection.State != System.Data.ConnectionState.Open)
                {
                    try
                    {
                        mReader.mConnection.Open();
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); NnMessage.ShowMessage("数据库错误！", true); }
                }
                return mReader;
            }
        }

        private NnReader()
        {
            try
            {
                string key = ConfigurationManager.AppSettings["dbkey"];
#if (DEBUG)
                string connstring = $"{ConfigurationManager.AppSettings["dbpath_d"]}{(string.IsNullOrEmpty(key) ? "" : "password=" + NnDecrypt(key))}";
#else
                string connstring = $"{ConfigurationManager.AppSettings["dbpath"]}{(string.IsNullOrEmpty(key) ? "" : "password=" + NnDecrypt(key))}";
#endif
                mConnection = new MySqlConnection(connstring);
                mConnection.Open();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); NnMessage.ShowMessage("数据库错误！", true); }
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

        internal List<Order> UploadPyrolysisLabel(List<Order> list)
        {
            throw new NotImplementedException();
        }
    }
}
