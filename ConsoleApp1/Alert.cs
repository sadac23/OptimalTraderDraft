// See https://aka.ms/new-console-template for more information


using DocumentFormat.OpenXml.Wordprocessing;
using System.Configuration;
using System.Globalization;
using System.Runtime.ConstrainedExecution;

internal class Alert
{
    public Alert(List<Analyzer.AnalysisResult> results)
    {
        AnalysisResults = results;
    }

    public List<Analyzer.AnalysisResult> AnalysisResults { get; }

    internal void SaveFile(string alertFilePath)
    {
        //1928：積水ハウス(株)（建設業）☆
        //株価：1234（2024/11/29）
        //市場：東証プライム,名証プレミア
        //配当利回り：3.64％（50%,5月,11月）
        //優待利回り：3.64％（QUOカード,100株,5月,11月）
        //通期予想：増収増益増配（+50%,+50%,+50）
        //時価総額：2兆3,470億円
        //ROE：9.99→9.99→10.71
        //PER：11.0倍（14.2）
        //PBR：1.18倍（1.1）
        //信用倍率：8.58倍
        //自己資本比率：40.0%
        //約定履歴：
        //買：2024/12/04：2068*300：-10.40%
        //売：2024/12/05：2068*100：-10.40%
        //買：2024/12/06：2060*100：-10.40%
        //変動履歴：
        //2024/10/04：3951：-10.40% (8)
        //2024/09/27：4119：-10.40% (9)
        //2024/09/20：3959：-10.40% (10)
        //決算発表日：
        //次回の決算発表日は未定です。
        //メモ：
        //ほげほげほげほげほげ。

        using (StreamWriter writer = new StreamWriter(alertFilePath))
        {
            // ファイルヘッダー
            writer.WriteLine(DateTime.Today.ToString("yyyyMMdd"));

            foreach (Analyzer.AnalysisResult r in AnalysisResults)
            {
                if (r.ShouldAlert())
                {
                    short count = 0;
                    string s = string.Empty;

                    writer.WriteLine("");
                    writer.WriteLine($"{r.StockInfo.Code}：{r.StockInfo.Name}（{r.StockInfo.Industry}）{(r.StockInfo.IsFavorite ? "☆" : "")}");
                    writer.WriteLine($"株価：{r.StockInfo.LatestPrice}（{r.StockInfo.LatestPriceDate.ToString("yyyy/MM/dd")}）");
                    writer.WriteLine($"市場：{r.StockInfo.Section}");
                    writer.WriteLine($"配当利回り：{ConvertToPercentage(r.StockInfo.DividendYield)}（{ConvertToPercentage( r.StockInfo.DividendPayoutRatio)},{r.StockInfo.DividendRecordDateMonth}）");
                    if (!string.IsNullOrEmpty(r.StockInfo.ShareholderBenefitsDetails))
                        writer.WriteLine($"優待利回り：{ConvertToPercentage(r.StockInfo.ShareholderBenefitYield)}（{r.StockInfo.ShareholderBenefitsDetails},{r.StockInfo.NumberOfSharesRequiredForBenefits},{r.StockInfo.ShareholderBenefitRecordDateMonth}）");
                    writer.WriteLine($"通期予想：{r.StockInfo.FullYearPerformanceForcastSummary}");
                    writer.WriteLine($"時価総額：{ConvertToYenNotation(r.StockInfo.MarketCap)}");

                    count = 0;
                    s = string.Empty;
                    foreach (StockInfo.FullYearProfit p in r.StockInfo.FullYearProfits)
                    {
                        if (count > 0) s += "→";
                        s += p.Roe;
                        count++;
                    }
                    if (!string.IsNullOrEmpty(s)) writer.WriteLine($"ROE：{s}");

                    writer.WriteLine($"PER：{ConvertToMultiplierString(r.StockInfo.Per)}（{r.StockInfo.AveragePer}）");
                    writer.WriteLine($"PBR：{ConvertToMultiplierString(r.StockInfo.Pbr)}（{r.StockInfo.AveragePbr}）");
                    writer.WriteLine($"信用倍率：{r.StockInfo.MarginBalanceRatio}");
                    writer.WriteLine($"自己資本比率：{r.StockInfo.EquityRatio}");

                    count = 0;
                    foreach (ExecutionList.Execution e in r.StockInfo.Executions)
                    {
                        if (count == 0) writer.WriteLine($"約定履歴：");
                        writer.WriteLine($"{e.BuyOrSell}：{e.Date.ToString("yyyy/MM/dd")}：{e.Price}*{e.Quantity}：{ConvertToPercentage((r.StockInfo.Prices[0].Close / e.Price) - 1)}");
                        count++;
                    }

                    count = 0;
                    foreach(Analyzer.AnalysisResult.PriceVolatility p in r.PriceVolatilities)
                    {
                        if (p.ShouldAlert)
                        {
                            if (count == 0) writer.WriteLine($"変動履歴：");
                            writer.WriteLine($"{p.VolatilityRateIndex1Date.ToString("yyyy/MM/dd")}：{p.VolatilityRateIndex1}：{ConvertToPercentage(p.VolatilityRate)}({p.VolatilityTerm})");
                            count++;
                        }
                    }

                    writer.WriteLine($"決算発表日：");
                    writer.WriteLine(r.StockInfo.PressReleaseDate);

                    if (string.IsNullOrEmpty(r.StockInfo.Memo))
                    {
                        //メモ：
                        writer.WriteLine($"メモ：");
                        writer.WriteLine(r.StockInfo.Memo);
                    }
                }
            }
        }
    }

    private string ConvertToMultiplierString(double value)
    {
        // 小数点以下2桁までの文字列に変換し、"倍"を追加
        return value.ToString("F2", CultureInfo.InvariantCulture) + "倍";
    }

    private string ConvertToPercentage(double value)
    {
        // パーセント形式の文字列に変換
        return (value * 100).ToString("F2", CultureInfo.InvariantCulture) + "%";
    }

    private string ConvertToYenNotation(double value)
    {
        if (value >= 1_000_000_000_000)
        {
            double trillions = Math.Floor(value / 1_000_000_000_000);
            double billions = (value % 1_000_000_000_000) / 100_000_000;
            return $"{trillions.ToString("N0", CultureInfo.InvariantCulture)}兆{billions.ToString("N0", CultureInfo.InvariantCulture)}億円";
        }
        else if (value >= 100_000_000)
        {
            double billions = value / 100_000_000;
            return $"{billions.ToString("N0", CultureInfo.InvariantCulture)}億円";
        }
        else if (value >= 10_000)
        {
            return $"{value.ToString("N0", CultureInfo.InvariantCulture)}円";
        }
        else
        {
            return value.ToString("N0", CultureInfo.InvariantCulture) + "円";
        }
    }

    
}