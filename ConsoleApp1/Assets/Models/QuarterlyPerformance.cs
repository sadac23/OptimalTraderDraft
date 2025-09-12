namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// �l�����Ɛя��
    /// </summary>
    public class QuarterlyPerformance
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
        public double OrdinaryProfit { get; internal set; }
        /// <summary>
        /// �ŏI�v
        /// </summary>
        public string NetProfit { get; internal set; }
        /// <summary>
        /// �C���ꊔ�v
        /// </summary>
        public string AdjustedEarningsPerShare { get; set; }
        /// <summary>
        /// �C���ꊔ�z
        /// </summary>
        public string AdjustedDividendPerShare { get; internal set; }
        /// <summary>
        /// ���\��
        /// </summary>
        public string ReleaseDate { get; internal set; }
    }
}