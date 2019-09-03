using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data
{
    class NotQCOrder
    {
        private string boxno;
        /// <summary>
        /// 盒号
        /// </summary>
        public string BoxNo { get => boxno ?? ""; set => boxno = value; }
        private string workno;
        /// <summary>
        /// WorkNo
        /// </summary>
        public string WorkNo { get => workno ?? ""; set => workno = value; }
        private string orderid;
        /// <summary>
        /// Order ID
        /// </summary>
        public string OrderId { get => orderid ?? ""; set => orderid = value; }
        private string quality;
        /// <summary>
        /// 质量
        /// </summary>
        public string Quality { get => quality ?? ""; set => quality = value; }
        private string coordinate;
        /// <summary>
        /// 坐标
        /// </summary>
        public string Coordinate { get => coordinate ?? ""; set => coordinate = value; }
        /// <summary>
        /// 真实坐标
        /// </summary>
        public string RCoordinate { get => BoxNo + "/" + Coordinate; }
        private string comments;

        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get => comments ?? ""; set => comments = value; }
        private string removeWorkNo;
        /// <summary>
        /// 取走WorkNo
        /// </summary>
        public string RemoveWorkNo { get => removeWorkNo ?? ""; set => removeWorkNo = value; }
        /// <summary>
        /// 添加日期
        /// </summary>
        public DateTime DateAdd { get; set; }
        /// <summary>
        /// 移除日期
        /// </summary>
        public DateTime DateRemove { get; set; }
        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsAvailable { get => !string.IsNullOrWhiteSpace(BoxNo) && !string.IsNullOrWhiteSpace(Coordinate); }
        public NotQCState State
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RemoveWorkNo))
                    return NotQCState.Insert;
                return NotQCState.Remove;
            }
        }

        public NotQCOrder(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return;
            string[] strs = v.Split('\t');
            if (strs.Length > 0)
                BoxNo = strs[0];
            if (strs.Length > 1)
                WorkNo = strs[1].TrimEnd();
            if (strs.Length > 2)
                OrderId = strs[2].TrimEnd();
            if (strs.Length > 3)
                Quality = strs[3].TrimEnd();
            if (strs.Length > 4)
                Coordinate = strs[4].TrimEnd();
            if (strs.Length > 5)
                Comments = strs[5].TrimEnd();
            if (strs.Length > 6)
                RemoveWorkNo = strs[6].TrimEnd();
        }

        public NotQCOrder()
        {
            
        }

        public void InitNotQCOrderByDB(OleDbDataReader reader)
        {
            DateAdd = NnStockManager.GetDateTiemFromDb(reader, "DateAdd");
            DateRemove = NnStockManager.GetDateTiemFromDb(reader, "DateRemove");
            WorkNo = NnStockManager.GetStringFromDb(reader, "WorkNo");
            OrderId = NnStockManager.GetStringFromDb(reader, "OrderId");
            Quality = NnStockManager.GetStringFromDb(reader, "Quality");
            string str = NnStockManager.GetStringFromDb(reader, "Coordinate");
            int index = str.IndexOf('/');
            if (index >= 0)
            {
                BoxNo = str.Substring(0, index);
                Coordinate = str.Substring(index + 1, str.Length - index - 1);
            }
            Comments = NnStockManager.GetStringFromDb(reader, "Comments");
            RemoveWorkNo = NnStockManager.GetStringFromDb(reader, "WorkNoRemove");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateAdd.ToShortDateString()).Append(',').Append(BoxNo).Append(',').Append(WorkNo).Append(',')
                .Append(OrderId).Append(',').Append(Quality).Append(',').Append(Coordinate).Append(',').Append(Comments).Append(',')
                .Append(DateRemove.ToShortDateString()).Append(',').Append(RemoveWorkNo).Append('\n');
            return sb.ToString();
        }

        public enum NotQCState
        {
            Insert,
            Remove,
        }
    }
}
