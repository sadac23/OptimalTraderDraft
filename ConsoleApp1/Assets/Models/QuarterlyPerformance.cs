namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// l”¼Šú‹ÆÑî•ñ
    /// </summary>
    public class QuarterlyPerformance
    {
        /// <summary>
        /// ŒˆZŠú
        /// </summary>
        public string FiscalPeriod { get; set; }
        /// <summary>
        /// ”„ã‚
        /// </summary>
        public string Revenue { get; set; }
        /// <summary>
        /// ‰c‹Æ‰v
        /// </summary>
        public string OperatingProfit { get; set; }
        /// <summary>
        /// Œoí‰v
        /// </summary>
        public double OrdinaryProfit { get; internal set; }
        /// <summary>
        /// ÅI‰v
        /// </summary>
        public string NetProfit { get; internal set; }
        /// <summary>
        /// C³ˆêŠ”‰v
        /// </summary>
        public string AdjustedEarningsPerShare { get; set; }
        /// <summary>
        /// C³ˆêŠ””z
        /// </summary>
        public string AdjustedDividendPerShare { get; internal set; }
        /// <summary>
        /// ”­•\“ú
        /// </summary>
        public string ReleaseDate { get; internal set; }
    }
}