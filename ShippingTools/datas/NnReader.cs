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
            if (path == null)
            {
                path = @"";
            }
            int index = 12;
            while (index < 21)
            {
                try
                {
                    mConnection = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.{index.ToString()}.0;Data Source={path}");
                    mConnection.Open();
                    return;
                }
                catch (Exception e) { ++index; Console.WriteLine(e.ToString()); }
            }
            NnMessage.ShowMessage("数据库错误！", true);
        }
    }
}
