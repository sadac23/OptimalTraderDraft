namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// �ʊ����v���
    /// </summary>
    public class FullYearProfit
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
        public string OperatingIncome { get; set; }
        /// <summary>
        /// ����c�Ɨ��v��
        /// </summary>
        public string OperatingMargin { get; set; }
        /// <summary>
        /// �q�n�d
        /// </summary>
        public double Roe { get; set; }
        /// <summary>
        /// �q�n�`
        /// </summary>
        public double Roa { get; set; }
        /// <summary>
        /// �����Y��]��
        /// </summary>
        public string TotalAssetTurnover { get; set; }
        /// <summary>
        /// �C��1���v
        /// </summary>
        public string AdjustedEarningsPerShare { get; set; }
    }
}