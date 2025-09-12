namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// �ʊ��Ɛя��
    /// </summary>
    public class FullYearPerformance
    {
        /// <summary>
        /// ���Z��
        /// </summary>
        public string FiscalPeriod { get; set; }
        /// <summary>
        /// ���㍂
        /// </summary>
        public string Revenue { get; set; }
        /// <summary>
        /// �c�Ɖv
        /// </summary>
        public string OperatingProfit { get; set; }
        /// <summary>
        /// �o��v
        /// </summary>
        public string OrdinaryProfit { get; set; }
        /// <summary>
        /// �ŏI�v
        /// </summary>
        public string NetProft { get; set; }
        /// <summary>
        /// �C��1���v
        /// </summary>
        public string AdjustedEarningsPerShare { get; set; }
        /// <summary>
        /// �C��1���z
        /// </summary>
        public string AdjustedDividendPerShare { get; set; }
        /// <summary>
        /// ���\��
        /// </summary>
        public string AnnouncementDate { get; set; }
    }
}