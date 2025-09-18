namespace ConsoleApp1.Assets.Calculators
{
    /// <summary>
    /// 計算・集計系ストラテジーのインターフェース
    /// </summary>
    public interface IAssetCalculator
    {
        void UpdateProgress(AssetInfo asset);
        void UpdateDividendPayoutRatio(AssetInfo asset);
        void UpdateFullYearPerformanceForcastSummary(AssetInfo asset);
        void SetupChartPrices(AssetInfo asset);

        double GetDividendPayoutRatio(string adjustedDividendPerShare, string adjustedEarningsPerShare);
        double GetDOE(double dividendPayoutRatio, double roe);
        string GetDividendPerShareIncreased(string adjustedDividendPerShare1, string adjustedDividendPerShare2);
        string GetIncreasedRate(string lastValue, string secondLastValue);
        string GetRevenueIncreasedSummary(string revenue1, string revenue2);
        string GetOrdinaryIncomeIncreasedSummary(string ordinaryIncome1, string ordinaryIncome2);
        string GetDividendPerShareIncreasedSummary(string adjustedDividendPerShare1, string adjustedDividendPerShare2);
        string ConvertToPercentageStringWithSign(double v);
        string ConvertToStringWithSign(double number);
    }
}