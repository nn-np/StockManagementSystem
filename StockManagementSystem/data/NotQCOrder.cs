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
        private string comments;

        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get => comments ?? ""; set => comments = value; }
        private string cause;
        /// <summary>
        /// 原因
        /// </summary>
        public string Cause { get => cause ?? ""; set => cause = value; }
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
        public bool IsAvailable { get => !string.IsNullOrWhiteSpace(Coordinate) && Coordinate.Contains('-'); }
        public NotQCState State
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Cause))
                    return NotQCState.Insert;
                return NotQCState.Remove;
            }
        }

        public NotQCOrder(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return;
            string[] strs = v.Split('\t');
            if (strs.Length < 5) return;
            WorkNo = strs[0].TrimEnd();
            OrderId = strs[1].TrimEnd();
            Quality = strs[2].TrimEnd();
            Cause = strs[3].TrimEnd();
            Coordinate = strs[4].TrimEnd();
            if (strs.Length > 7)
                Comments = strs[7].TrimEnd();
        }

        public NotQCOrder() { }

        public void InitNotQCOrderByDB(OleDbDataReader reader)
        {
            DateAdd = NnStockManager.GetDateTiemFromDb(reader, "DateAdd");
            DateRemove = NnStockManager.GetDateTiemFromDb(reader, "DateRemove");
            WorkNo = NnStockManager.GetStringFromDb(reader, "WorkNo");
            OrderId = NnStockManager.GetStringFromDb(reader, "OrderId");
            Quality = NnStockManager.GetStringFromDb(reader, "Quality");
            Coordinate = NnStockManager.GetStringFromDb(reader, "Coordinate");
            Comments = NnStockManager.GetStringFromDb(reader, "Comments");
            Cause= NnStockManager.GetStringFromDb(reader, "Cause");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool isremove = State == NotQCState.Remove;
            sb.Append(DateAdd.ToShortDateString()).Append(',').Append(WorkNo).Append(',')
                .Append(OrderId).Append(',').Append(Quality).Append(',').Append(Coordinate).Append(',').Append(Comments).Append(',')
                .Append(isremove ? (DateRemove.ToShortDateString() + ',') : "").Append(isremove ? Cause : "").Append('\n');
            return sb.ToString();
        }

        public enum NotQCState
        {
            Insert,
            Remove,
        }
    }
}
