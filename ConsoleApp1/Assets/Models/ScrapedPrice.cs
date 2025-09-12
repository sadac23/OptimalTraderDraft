using System;

namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// 日次価格情報（株価履歴）
    /// </summary>
    public class ScrapedPrice
    {
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 日付文字列
        /// </summary>
        public string DateYYYYMMDD { get; set; }
        /// <summary>
        /// 始値
        /// </summary>
        public double Open { get; set; }
        /// <summary>
        /// 高値
        /// </summary>
        public double High { get; set; }
        /// <summary>
        /// 安値
        /// </summary>
        public double Low { get; set; }
        /// <summary>
        /// 終値
        /// </summary>
        public double Close { get; set; }
        /// <summary>
        /// 出来高
        /// </summary>
        public double Volume { get; set; }
        /// <summary>
        /// 調整後終値
        /// </summary>
        public double AdjustedClose { get; internal set; }
    }
}