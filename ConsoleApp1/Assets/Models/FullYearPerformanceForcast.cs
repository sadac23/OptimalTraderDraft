using System;

namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// ’ÊŠú‹ÆÑ—\‘zi‚¨‚æ‚Ñ—\‘z—š—ğj
    /// </summary>
    public class FullYearPerformanceForcast : ICloneable
    {
        /// <summary>
        /// ŒˆZŠú
        /// </summary>
        public string FiscalPeriod { get; internal set; }
        /// <summary>
        /// C³“ú
        /// </summary>
        public DateTime RevisionDate { get; internal set; }
        /// <summary>
        /// ‹æ•ª
        /// </summary>
        public string Category { get; internal set; }
        /// <summary>
        /// C³•ûŒü
        /// </summary>
        public string RevisionDirection { get; internal set; }
        /// <summary>
        /// ”„ã‚
        /// </summary>
        public string Revenue { get; internal set; }
        /// <summary>
        /// ‰c‹Æ‰v
        /// </summary>
        public string OperatingProfit { get; internal set; }
        /// <summary>
        /// Œoí‰v
        /// </summary>
        public string OrdinaryProfit { get; internal set; }
        /// <summary>
        /// ÅI‰v
        /// </summary>
        public string NetProfit { get; internal set; }
        /// <summary>
        /// C³”z“–
        /// </summary>
        public string RevisedDividend { get; internal set; }
        /// <summary>
        /// —\‘zŠT—v
        /// </summary>
        public string Summary { get; internal set; }
        /// <summary>
        /// ‘O‰ñ‚Ì—\‘z
        /// </summary>
        public FullYearPerformanceForcast PreviousForcast { get; internal set; }

        public object Clone()
        {
            // ó‚¢ƒRƒs[iƒƒ“ƒo[ƒRƒs[j‚ğ•Ô‚·
            return this.MemberwiseClone();
        }
        /// <summary>
        /// ‰º•ûC³‚ğŠÜ‚Ş‚©H
        /// </summary>
        internal virtual bool HasDownwardRevision()
        {
            bool result = false;

            if (this.Category != CommonUtils.Instance.ForecastCategoryString.Initial
                && this.Category != CommonUtils.Instance.ForecastCategoryString.Final)
            {
                // ”„ã
                if (CommonUtils.Instance.GetDouble(this.Revenue) < CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // Œoí—˜‰v
                if (CommonUtils.Instance.GetDouble(this.OrdinaryProfit) < CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryProfit)) result = true;
                // ”z“–
                if (CommonUtils.Instance.GetDouble(this.RevisedDividend) < CommonUtils.Instance.GetDouble(this.PreviousForcast.RevisedDividend)) result = true;
            }

            return result;
        }

        /// <summary>
        /// ã•ûC³‚ğŠÜ‚Ş‚©H
        /// </summary>
        internal virtual bool HasUpwardRevision()
        {
            bool result = false;

            if (this.PreviousForcast == null) return result;

            if (this.Category != CommonUtils.Instance.ForecastCategoryString.Initial)
            {
                // ”„ã
                if (CommonUtils.Instance.GetDouble(this.Revenue) > CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // Œoí—˜‰v
                if (CommonUtils.Instance.GetDouble(this.OrdinaryProfit) > CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryProfit)) result = true;
                // ”z“–
                if (CommonUtils.Instance.GetDouble(this.RevisedDividend) > CommonUtils.Instance.GetDouble(this.PreviousForcast.RevisedDividend)) result = true;
            }

            return result;
        }

    }
}