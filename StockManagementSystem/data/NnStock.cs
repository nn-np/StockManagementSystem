﻿using System;
using System.Data.OleDb;
using System.Linq;
/**
 * 多肽类
 * crude为 -2，desalt为 -1
 * nnns
 */
namespace StockManagementSystem.data
{
    public class NnStock
    {
        // -------属性------
        private double purity;// 纯度
        private double quality;// 质量
        private double mw;// 分子量
        private string coordinate;// 坐标

        public NnStock(string orderId = "", long workNo = -1)
        {
            this.OrderId = orderId;
            this.WorkNo = workNo;
        }

        // 这个通过一个包括所有信息的字符串构建NnStock
        public NnStock(string allInfo)
        {
            if (string.IsNullOrWhiteSpace(allInfo)) return;
            OriginalString = allInfo.Trim();
            string[] strs = allInfo.Split('\t');
            if (strs.Length < 5) return;
            WorkNoString = strs[0].TrimEnd();
            OrderId = strs[1].TrimEnd();
            QualitySum = strs[2].TrimEnd();
            Cause = strs[3].TrimEnd();
            Coordinate = strs[4].TrimEnd();
            if (strs.Length > 5)
                PurityString = strs[5].TrimEnd();
            if (strs.Length > 6)
                MwString = strs[6].TrimEnd();
            if (strs.Length > 7)
                Comments = strs[7].TrimEnd();
        }
        private string orderid;
        public string OrderId { get => orderid ?? ""; set => orderid = value; }
        public long WorkNo { get; set; }// workNo
        public string WorkNoString
        {
            get => WorkNo.ToString();
            set
            {
                long no = 0;
                if (long.TryParse(value, out no))
                    WorkNo = no;
                else
                    WorkNo = -1;
            }
        }
        /// <summary>
        /// 原始数据
        /// </summary>
        public string OriginalString { get; set; }
        public double Quality { get => quality; set => quality = value; }
        public string QualityString { get => $"{quality}mg"; set => quality = getMaxValue(value); }// 这个是得到字符串中的最大值作为质量
        public string QualitySum { get => $"{quality}"; set => quality = getSumValue(value); }// 这个是得到字符串中数字的和作为质量

        // 分子量
        public double Mw { get => mw; set => mw = value; }

        public string MwString { get => mw.ToString(); set => mw = getMaxValue(value); }

        // 判断一条数据是否有效，必须要有坐标，如果没有原因，则必须要有orderId或者workNo二者之一
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsAvailable { get => !string.IsNullOrWhiteSpace(Coordinate) && Coordinate.Contains('-'); }
        //public bool IsAvailable{ get => !string.IsNullOrWhiteSpace(Coordinate) && (!string.IsNullOrEmpty(Cause) || ((OrderId ?? "").Contains('-')) || WorkNo > 0); }
        //public bool IsAvailable { get => string.IsNullOrEmpty(Cause) ? ((OrderId ?? "").Contains('-') || WorkNo > 0) && (Coordinate ?? "").Contains('-') : (Coordinate ?? "").Contains('-'); }

            // 这条数据的状态（需要更新，插入还是删除）
         public StockState State
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Cause))
                {
                    if (string.IsNullOrWhiteSpace(OrderId))
                        return StockState.Update;
                    return StockState.Insert;
                }
                return StockState.Delete;
            }
        }
        private SearchState searchstate;
        /// <summary>
        /// 搜索状态
        /// </summary>
        public SearchState StockSearchState
        {
            get
            {
                if (searchstate != SearchState.Uninitialized) return searchstate;
                if (string.IsNullOrWhiteSpace(Cause))
                {
                    if (Coordinate.StartsWith("L-"))
                        return SearchState.Temporary;
                    return SearchState.Normal;
                }
                return SearchState.Deleted;
            }
            set => searchstate = value;
        }

        public double Purity { get => purity; set => purity = value; }
        public string PurityString
        {
            get
            {
                switch (purity)
                {
                    case -1: return "Desalt";
                    case -2: return "Crude";
                    default: return $"{(purity < 1 ? purity * 100 : purity)}%";
                }
            }
            set
            {
                value = value == null ? "" : value.ToLower();
                if (value == "desalt") purity = -1;
                else if (value == "crude") purity = -2;
                else purity = getMaxValue(value);
            }
        }
        // 坐标
        public string Coordinate { get => coordinate ?? ""; set => coordinate = (value ?? "").ToUpper(); }

        // 添加日期
        public DateTime DateAdd { get; set; }

        // 移除日期
        public DateTime DateRemove { get; set; }

        // 原因
        public string Cause { get; set; }

        private string comments;
        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get => comments ?? ""; set => comments = value; }

        public override string ToString()
        {
            return $"{DateAdd.ToShortDateString()},{WorkNo},{OrderId},{QualityString},{Coordinate},{PurityString},{MwString},{Comments},";
        }

        public object[] GetValue()
        {
            return new object[] { OriginalString, DateAdd.ToShortDateString(), WorkNo, OrderId, QualityString, Coordinate, PurityString, MwString, Comments };
        }

        internal object[] GetValueRemove()
        {
            return new object[] { OriginalString, DateRemove.ToShortDateString(), DateAdd.ToShortDateString(), WorkNo, OrderId, QualityString, Coordinate, PurityString, MwString, Comments, Cause };
        }

        // -------方法-------
        // 获取字符串中所有数字的和
        private double getSumValue(string str)
        {
            str += '\0';
            double value = 0;
            int index = 0;
            for (int len = 0; len < str.Length; ++len)
            {
                char c = str[len];
                if ((c < '0' || c > '9') && c != '.')
                {
                    if (len > index)
                    {
                        double d = 0;
                        value += (double.TryParse(str.Substring(index, len - index), out d)) ? d : 0;
                    }
                    index = len + 1;
                }
            }
            return value;
        }
        // 获得字符串中最大的数字
        private double getMaxValue(string str)
        {
            str += '\0';
            double value = 0;
            int index = 0;
            for (int len = 0; len < str.Length; ++len)
            {
                char c = str[len];
                if ((c < '0' || c > '9') && c != '.')
                {
                    if (len > index)
                    {
                        double d = 0;
                        value = (double.TryParse(str.Substring(index, len - index), out d) && d > value) ? d : value;
                    }
                    index = len + 1;
                }
            }
            return value;
        }

        public enum StockState
        {
            Insert,
            Delete,
            Update
        }
        /// <summary>
        /// 根据数据库初始化新的Stock
        /// </summary>
        internal void InitStockNewByDb(OleDbDataReader reader)
        {
            _initStockByDb(reader);
            DateAdd = NnStockManager.GetDateTiemFromDb(reader, "_date");
            Coordinate = NnStockManager.GetStringFromDb(reader, "coordinate");
            Comments = NnStockManager.GetStringFromDb(reader, "comments");
        }
        /// <summary>
        /// 根据数据库初始化已移除的Stock
        /// </summary>
        internal void InitStockOldByDb(OleDbDataReader reader)
        {
            _initStockByDb(reader);
            DateAdd = NnStockManager.GetDateTiemFromDb(reader, "dateAdd");
            DateRemove = NnStockManager.GetDateTiemFromDb(reader, "dateRemove");
            Cause = NnStockManager.GetStringFromDb(reader, "cause");
        }

        private void _initStockByDb(OleDbDataReader reader)
        {
            OrderId = NnStockManager.GetStringFromDb(reader, "orderId");
            WorkNo = NnStockManager.GetIntFromDb(reader, "workNo");
            Quality = NnStockManager.GetDoubleFromDb(reader, "quality");
            Purity = NnStockManager.GetDoubleFromDb(reader, "purity");
            Mw = NnStockManager.GetDoubleFromDb(reader, "mw");
        }
    }
    /// <summary>
    /// 搜索类型
    /// </summary>
    public enum SearchState
    {
        Uninitialized,// 未初始化
        Normal,
        Temporary,
        Deleted,
        None,// 没有
    }
}
