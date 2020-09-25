using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagementSystem.data
{
    class StockInfos
    {
        private List<OrderInfo> mNormalList;// 正常
        private List<OrderInfo> mRemoveList;// 已移除


        /// <summary>
        /// 数据数量
        /// </summary>
        public int Count { get => mNormalList.Count + mRemoveList.Count + 5; }

        /// <summary>
        /// 数据数量(正常)
        /// </summary>
        public int CountNormal { get => mNormalList.Count + 2; }

        public StockInfos()
        {
            mNormalList = new List<OrderInfo>();
            mRemoveList = new List<OrderInfo>();
        }

        public void Add(OrderInfo info)
        {
            switch (info.SearchState)
            {
                case SearchState.Deleted:
                    mRemoveList.Add(info);
                    break;
                default:
                    mNormalList.Add(info);
                    break;
            }
        }

        internal object[][] GetValues()
        {
            object[][] vs = new object[Count][];
            vs[0] = new object[] { "搜索关键字","日期","WO号","Order ID","质量","坐标","备注"};
            int rindex = 1;
            foreach(var v in mNormalList)
            {
                vs[rindex] = v.GetObejcts();
                ++rindex;
            }

            rindex += 2;
            vs[rindex++] = new object[] { "已被移除项目：" };
            vs[rindex++] = new object[] { "搜索关键字", "日期", "WO号", "Order ID", "质量", "坐标", "备注", "取走日期", "原因" };
            foreach(var v in mRemoveList)
            {
                vs[rindex] = v.GetObjectsRemove();
                ++rindex;
            }
            return vs;
        }
    }

    class StockSearcher
    {
        private List<NnStock> mNormalList;// 正常
        private List<NnStock> mLList;// 临时坐标
        private List<NnStock> mRemoveList;// 已移除

        /// <summary>
        /// 数据数量
        /// </summary>
        public int Count { get => mNormalList.Count + mLList.Count + mRemoveList.Count + 5; }

        /// <summary>
        /// 数据数量
        /// </summary>
        public int CountNormal { get => mNormalList.Count + mLList.Count + 2; }


        public StockSearcher()
        {
            mNormalList = new List<NnStock>();
            mLList = new List<NnStock>();
            mRemoveList = new List<NnStock>();
        }

        public void Add(NnStock stock)
        {
            switch (stock.StockSearchState)
            {
                case SearchState.None:// 没有结果
                    mNormalList.Add(stock);
                    break;
                case SearchState.Normal:// 正常
                    mNormalList.Add(stock);
                    break;
                case SearchState.Temporary:// 临时坐标
                    mLList.Add(stock);
                    break;
                case SearchState.Deleted:// 已移除
                    mRemoveList.Add(stock);
                    break;
                default:
                    mNormalList.Add(stock);
                    break;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("KeyWord,AddDate,WorkNo,OrderID,Qualit,Coordinate,Purity,Mw,备注,\n");
            foreach (var v in mNormalList)
            {
                if (v.StockSearchState == SearchState.None)
                {
                    sb.Append(v.OriginalString.Replace(",", "")).Append(",,无记录！,\n");
                }
                else
                {
                    sb.Append(v.OriginalString.Replace(",", "")).Append(",").Append(v.ToString()).Append(",\n");
                }
            }
            foreach (var v in mLList)
            {
                sb.Append(v.OriginalString.Replace(",", "")).Append(",").Append(v.ToString()).Append(",\n");
            }
            sb.Append("\n\n已被移除项目：\nKeyWord,RemoveDate,AddDate,WorkNo,OrderID,Qualit,Coordinate,Purity,Mw,备注,Cause,\n");
            foreach (var v in mRemoveList)
            {
                sb.Append(v.OriginalString.Replace(",", "")).Append(",").Append(v.DateRemove.ToShortDateString()).Append(',').Append(v.ToString()).Append(v.Cause).Append('\n');
            }
            return sb.ToString();
        }

        internal object[][] GetValues()
        {
            object[][] vs = new object[Count][];
            vs[0] = new object[] { "搜索关键字", "添加日期", "WorkNo", "OrderID", "质量", "坐标", "纯度", "Mw", "备注" };
            int rindex = 1;
            foreach (var v in mNormalList)
            {
                if (v.StockSearchState == SearchState.None)
                {
                    vs[rindex] = new object[] { v.OriginalString, null, "无记录！" };
                }
                else
                {
                    vs[rindex] = v.GetValue();
                }
                ++rindex;
            }
            foreach (var v in mLList)
            {
                vs[rindex] = v.GetValue();

                ++rindex;
            }
            rindex += 2;
            vs[rindex++] = new object[] { "已被移除项目：" };
            vs[rindex++] = new object[] { "搜索关键字", "移除日期", "添加日期", "WorkNo", "OrderID", "质量", "坐标", "纯度", "Mw", "备注", "原因" };
            foreach (var v in mRemoveList)
            {
                vs[rindex] = v.GetValueRemove();
                ++rindex;
            }
            return vs;
        }
    }

    /// <summary>
    /// 搜索数据
    /// </summary>
    class SearchItem
    {
        private string vl;
        /// <summary>
        /// 搜索字符窜
        /// </summary>
        public string Value
        {
            get => vl;
            set
            {
                vl = value?.Trim();
                if (vl != null && !vl.Contains('-'))// 很大概率是WorkNo
                {
                    bool b = long.TryParse(vl, out long l);
                    if (!b) SearchType = SearchType.Other;
                    else SearchType = SearchType.WorkNo;
                }
                else
                {
                    SearchType = SearchType.CoordinateAndOrderID;
                }
            }
        }
        /// <summary>
        /// 搜索类型
        /// </summary>
        public SearchType SearchType { get; set; }

        /// <summary>
        /// 从字符串中获取搜索信息
        /// </summary>
        public static List<SearchItem> GetSearchItems(string value)
        {
            List<SearchItem> list = new List<SearchItem>();

            var vs = value.Split('\n');
            foreach (var v in vs)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                var vv = v.Trim();
                SearchItem item = new SearchItem() { Value = vv };
                list.Add(item);
            }
            return list;
        }
    }

    enum SearchType
    {
        /// <summary>
        /// WorkNo
        /// </summary>
        WorkNo,
        /// <summary>
        /// 坐标和OrderID
        /// </summary>
        CoordinateAndOrderID,
        /// <summary>
        /// 无效数据
        /// </summary>
        Other,
    }
}
