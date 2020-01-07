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
    }
}
