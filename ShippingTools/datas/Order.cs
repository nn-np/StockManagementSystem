using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingTools
{
    /// <summary>
    /// 发货订单
    /// </summary>
    class Order
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        private string serialNumber;
        /// <summary>
        /// 序号
        /// </summary>
        public string SerialNumber { get => serialNumber ?? ""; set => serialNumber = value; }
        private string workno;
        /// <summary>
        /// WorkNo
        /// </summary>
        public string WorkNo { get => workno ?? ""; set => workno = value; }
        private string orderId;
        /// <summary>
        /// OrderID
        /// </summary>
        public string OrderId { get => orderId ?? ""; set => orderId = value; }
        private string quality;
        /// <summary>
        /// 质量
        /// </summary>
        public string Quality { get => quality ?? ""; set => quality = value; }
        private string mw;
        /// <summary>
        /// 分子量
        /// </summary>
        public string Mw { get => mw ?? ""; set => mw = value; }
        private string exterior;
        /// <summary>
        /// 外观
        /// </summary>
        public string Exterior { get => exterior ?? ""; set => exterior = value; }
        private string lotno;
        /// <summary>
        /// Lot No
        /// </summary>
        public string LotNo { get => lotno ?? ""; set => lotno = value; }
        private string packagingRequirements;
        /// <summary>
        /// 分装要求
        /// </summary>
        public string PackagingRequirements { get => packagingRequirements ?? ""; set => packagingRequirements = value; }
        private string purity;
        /// <summary>
        /// 纯度
        /// </summary>
        public string Purity { get => purity ?? ""; set => purity = value; }
        private string dataCollation;
        /// <summary>
        /// 数据整理
        /// </summary>
        public string DataCollation { get => dataCollation ?? ""; set => dataCollation = value; }
        private string label;
        /// <summary>
        /// 贴标
        /// </summary>
        public string Label { get => label ?? ""; set => label = value; }
        private DateTime dateLabel;
        /// <summary>
        /// 贴标日期
        /// </summary>
        public string DateLabel { get => dateLabel.ToShortDateString(); set { DateTime.TryParse(value, out DateTime dt); dateLabel = dt; } }


        private string release;
        /// <summary>
        /// 放行
        /// </summary>
        public string Release { get => release ?? ""; set => release = value; }
        private DateTime dateRelease;
        /// <summary>
        /// 放行日期
        /// </summary>
        public string DateTelease { get => dateRelease.ToShortDateString(); set { DateTime.TryParse(value, out DateTime dt); dateRelease = dt; } }
        private string comments;

        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get => comments ?? ""; set => comments = value; }
        /// <summary>
        /// 上传原始数据
        /// </summary>
        public string OriginalValue { get; internal set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// 是否冻干
        /// </summary>
        public bool IsFreeze { get => SerialNumber.Contains("冻干"); }

        internal void InitByString(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return;
            OriginalValue = v;
            string[] vs = v.Split('\t');
            if (vs.Length < 9) return;
            IsValid = true;
            SerialNumber = vs[0].Trim();
            WorkNo = vs[1].Trim();
            OrderId = vs[2].Trim();
            Quality = vs[3].Trim();
            Mw = vs[4].Trim();
            Exterior = vs[5].Trim();
            LotNo = vs[6].Trim();
            PackagingRequirements = vs[7].Trim();
            Purity = vs[8].Trim();
            DataCollation = _getValueFromList(vs, 9);
            Label = _getValueFromList(vs, 10);
            Release = _getValueFromList(vs, 11);
            Comments = _getValueFromList(vs, 12);
        }

        private string _getValueFromList(string[] vs, int v)
        {
            if (vs.Length > v)
            {
                return vs[v].Trim();
            }
            return "";
        }


        internal object[] GetObjects()
        {
            return new object[] { SerialNumber, WorkNo, OrderId, Quality, Mw, Exterior, LotNo, PackagingRequirements, Purity, DataCollation, Label, dateLabel.ToString("yyyy-MM-dd HH:mm:ss"), Release, dateRelease.ToString("yyyy-MM-dd HH:mm:ss"), Comments };
        }
    }
}
