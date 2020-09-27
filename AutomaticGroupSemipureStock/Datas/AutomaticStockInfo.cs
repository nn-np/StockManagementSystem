using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticGroupSemipureStock.Datas
{
    /// <summary>
    /// 自动组半纯品信息
    /// </summary>
    class AutomaticStockInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// Work No
        /// </summary>
        public long WorkNo { get; set; }
        public string OperationName { get; set; }
        public string Status { get; set; }
        /// <summary>
        /// 纯化班组
        /// </summary>
        public string PurGroup { get; set; }
        /// <summary>
        /// 纯化人
        /// </summary>
        public string PurUser { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 用户备注
        /// </summary>
        public string UserComments { get; set; }
        /// <summary>
        /// 半纯品的量
        /// </summary>
        public string Quality { get; set; }
        /// <summary>
        /// 库存量
        /// </summary>
        public string StockQuality { get; set; }
        /// <summary>
        /// 半纯品纯度
        /// </summary>
        public double Purity { get; set; }
        /// <summary>
        /// 分析仪器
        /// </summary>
        public string AnalyticalInstruments { get; set; }
        /// <summary>
        /// 库存坐标
        /// </summary>
        public string Coordinate { get; set; }
        /// <summary>
        /// 库存状态
        /// </summary>
        public StockState State { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime AddDate { get; set; }
        /// <summary>
        /// 移除时间
        /// </summary>
        public DateTime RemoveDate { get; set; }
    }

    /// <summary>
    /// 库存状态
    /// </summary>
    public enum StockState
    {
        Empty,      // 空坐标
        Filled,     // 被填充
        Removed,    // 被移除
    }
}
