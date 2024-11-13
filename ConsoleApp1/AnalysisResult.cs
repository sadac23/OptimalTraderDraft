// See https://aka.ms/new-console-template for more information
internal class AnalysisResult
{
    public string Code { get; set; }
    public string DateString { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; }
    public double VolatilitySum { get; set; }
    public int VolatilityTerm { get; set; }
    public double LeverageRatio { get; set; }
    public double MarketCap { get; set; }
    public double Roe { get; set; }
    public double EquityRatio { get; set; }
    public double RevenueProfitDividend { get; set; }
    public string MinkabuAnalysis { get; set; }
}