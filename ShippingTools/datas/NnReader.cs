using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingTools
{
    class NnReader
    {
        private static NnReader mReader;
        private static readonly object locker = new object();

        private OleDbConnection mConnection;
        public NnReader Instence
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
                if (mReader.mConnection.State != System.Data.ConnectionState.Open)
                {
                    try
                    {

                    }
                    catch { }
                }
                return mReader;
            }
        }

        private NnReader()
        {
            string path;
#if (DEBUG)
            path = ConfigurationManager.AppSettings["dbpath_d"];
#else
            path = ConfigurationManager.AppSettings["dbpath"];
#endif

            NnMessage.ShowMessage("数据库错误！", true);
        }
    }
}
