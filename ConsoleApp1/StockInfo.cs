// See https://aka.ms/new-console-template for more information
internal class StockInfo
{
    public StockInfo(string code, string classification)
    {
        Code = code;
        Classification = classification;
        Prices = new List<StockInfo.Price>();
    }
    public string Code { get; set; }
    public string Name { get; set; }
    public List<Price> Prices { get; set; }
    public string Classification { get; set; }
    public double Roe { get; internal set; }
    public string Per { get; internal set; }
    public string Pbr { get; internal set; }
    public string DividendYield { get; internal set; }
    public string MarginBalanceRatio { get; internal set; }
    public string MarketCap { get; internal set; }

    public class Price
    {
        public DateTime Date { get; set; }
        public string DateYYYYMMDD { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }
}