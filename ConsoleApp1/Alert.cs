﻿// See https://aka.ms/new-console-template for more information


using DocumentFormat.OpenXml.Wordprocessing;
using System.Configuration;
using System.Runtime.ConstrainedExecution;

internal class Alert
{
    public Alert()
    {
    }

    public Alert(List<Analyzer.AnalysisResult> results)
    {
        AnalysisResults = results;
    }

    public List<Analyzer.AnalysisResult> AnalysisResults { get; }

    internal void SaveFile(string alertFilePath)
    {
        //1928：積水ハウス(株)
        //利回り：3.64％
        //通期予想：増収増益増配
        //時価総額：2兆3,470億円
        //ROE：10.71
        //PER：11.0倍
        //PBR：1.18倍
        //信用倍率：8.58倍
        //自己資本比率：40.0%
        //約定履歴：
        //買：2024/12/04：2068*300
        //売：2024/12/05：2068*100
        //買：2024/12/06：2060*100
        //変動履歴：
        //-10.40% (8)：3951→3540(2024/10/04→2024/11/29)
        //-14.06% (9)：4119→3540(2024/09/27→2024/11/29)
        //-10.58% (10)：3959→3540(2024/09/20→2024/11/29)

        using (StreamWriter writer = new StreamWriter(alertFilePath))
        {
            // ファイルヘッダー
            writer.WriteLine(DateTime.Today.ToString("yyyyMMdd"));

            foreach (Analyzer.AnalysisResult r in AnalysisResults)
            {
                if (r.ShouldAlert())
                {
                    writer.WriteLine("");
                    writer.WriteLine($"{r.StockInfo.Code}：{r.StockInfo.Name}");
                    writer.WriteLine($"利回り：{r.StockInfo.DividendYield}");
                    writer.WriteLine($"通期予想：{r.StockInfo.FullYearPerformanceForcastSummary}");
                    writer.WriteLine($"時価総額：{r.StockInfo.MarketCap}");
                    writer.WriteLine($"ROE：{r.StockInfo.Roe}");
                    writer.WriteLine($"PER：{r.StockInfo.Per}");
                    writer.WriteLine($"PBR：{r.StockInfo.Pbr}");
                    writer.WriteLine($"信用倍率：{r.StockInfo.MarginBalanceRatio}");
                    writer.WriteLine($"自己資本比率：40.0%");

                    short count = 0;
                    foreach (ExecutionList.Execution e in r.StockInfo.Executions)
                    {
                        if (count == 0) writer.WriteLine($"約定履歴：");
                        writer.WriteLine($"{e.BuyOrSell}：{e.Date.ToString("yyyy/MM/dd")}：{e.Price}*{e.Quantity}");
                        count++;
                    }

                    count = 0;
                    foreach(Analyzer.AnalysisResult.PriceVolatility p in r.PriceVolatilities)
                    {
                        if (p.ShouldAlert)
                        {
                            if (count == 0) writer.WriteLine($"変動履歴：");
                            writer.WriteLine(
                                $"{ConvertToPercetage(p.VolatilityRate)}({p.VolatilityTerm})" +
                                $"：{p.VolatilityRateIndex1}→{p.VolatilityRateIndex2}" +
                                $"({p.VolatilityRateIndex1Date.ToString("yyyy/MM/dd")}→{p.VolatilityRateIndex2Date.ToString("yyyy/MM/dd")})"
                                );
                            count++;
                        }
                    }
                }
            }
        }
    }
    internal string ConvertToPercetage(double v)
    {
        // パーセント形式の文字列に変換
        return (v * 100).ToString("F2") + "%";
    }

}