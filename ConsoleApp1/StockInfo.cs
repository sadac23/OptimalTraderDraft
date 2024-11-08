// See https://aka.ms/new-console-template for more information
internal class StockInfo
{
    public StockInfo(string code)
    {
        Code = code;
        Prices = new List<StockInfo.Price>();
    }
    public string Code { get; set; }
    public string Name { get; set; }
    public List<Price> Prices { get; set; }

    public class Price
    {
        public string Date { get; set; }
        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string Close { get; set; }
        public string Volume { get; set; }
    }
}