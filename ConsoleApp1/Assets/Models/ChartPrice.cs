using System;

namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// チャート用価格情報（テクニカル指標含む）
    /// </summary>
    public class ChartPrice : ICloneable
    {
        public virtual DateTime Date { get; internal set; }
        public virtual double Price { get; internal set; }
        public virtual double Volatility { get; internal set; }
        public virtual double RSIL { get; internal set; }
        public virtual double RSIS { get; internal set; }
        /// <summary>
        /// 単純移動平均値（25日）
        /// </summary>
        public virtual double SMA25 { get; internal set; }
        /// <summary>
        /// 単純移動平均値（75日）
        /// </summary>
        public virtual double SMA75 { get; internal set; }
        /// <summary>
        /// Moving Average Deviation（平均移動乖離）25日
        /// </summary>
        public virtual double MADS { get; internal set; }
        /// <summary>
        /// Moving Average Deviation（平均移動乖離）75日
        /// </summary>
        public virtual double MADL { get; internal set; }
        /// <summary>
        /// SMA25、SMA75の乖離値
        /// </summary>
        public virtual double SMAdev { get; internal set; }

        public object Clone()
        {
            // 浅いコピー（メンバーコピー）を返す
            return this.MemberwiseClone();
        }
        public virtual bool OverboughtIndicator()
        {
            bool result = false;

            // RSIが閾値以下の場合
            if (this.RSIL >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;
            if (this.RSIS >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;

            return result;
        }

        public virtual bool OversoldIndicator()
        {
            bool result = false;

            // RSIが閾値以下の場合
            if (this.RSIL <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;
            if (this.RSIS <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;

            return result;
        }

    }
}