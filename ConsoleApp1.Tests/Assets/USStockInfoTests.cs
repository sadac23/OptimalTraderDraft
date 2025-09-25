using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.Assets.Calculators;
using ConsoleApp1.Assets.Setups;
using ConsoleApp1.Output;
using ConsoleApp1.Assets.Alerting;

namespace ConsoleApp1.Tests.Assets;

[Collection("CommonUtils collection")]
public class USStockInfoTests
{
    private USStockInfo CreateUSStockInfo()
    {
        var watchStock = new WatchList.WatchStock
        {
            Code = "AAPL",
            Name = "Apple Inc.",
            Classification = "5", // 米国個別株
            IsFavorite = "1",
            Memo = "Test memo"
        };

        // Factory経由で生成
        var assetInfo = AssetInfoFactory.Create(watchStock);
        return Assert.IsType<USStockInfo>(assetInfo);
    }

    [Fact]
    public void CanSetAndGet_Properties()
    {
        var info = CreateUSStockInfo();
        info.Name = "Microsoft";
        info.Code = "MSFT";
        info.Classification = "5";
        info.Roe = 15.2;
        info.Per = 30.1;
        info.Pbr = 8.5;
        info.IsFavorite = true;
        info.Memo = "US tech giant";
        info.Section = "NASDAQ";
        info.Industry = "Technology";
        info.AveragePer = 25.0;
        info.AveragePbr = 7.0;
        info.EarningsPeriod = "2024/12";
        info.Doe = 0.05;
        info.PEGRatio = 1.5;

        Assert.Equal("Microsoft", info.Name);
        Assert.Equal("MSFT", info.Code);
        Assert.Equal("5", info.Classification);
        Assert.Equal(15.2, info.Roe);
        Assert.Equal(30.1, info.Per);
        Assert.Equal(8.5, info.Pbr);
        Assert.True(info.IsFavorite);
        Assert.Equal("US tech giant", info.Memo);
        Assert.Equal("NASDAQ", info.Section);
        Assert.Equal("Technology", info.Industry);
        Assert.Equal(25.0, info.AveragePer);
        Assert.Equal(7.0, info.AveragePbr);
        Assert.Equal("2024/12", info.EarningsPeriod);
        Assert.Equal(0.05, info.Doe);
        Assert.Equal(1.5, info.PEGRatio);
    }

    [Fact]
    public void Formatter_ToOutputString_ContainsKeyInfo()
    {
        var info = CreateUSStockInfo();
        info.Name = "Apple Inc.";
        info.Code = "AAPL";
        info.Section = "NASDAQ";
        info.Industry = "Technology";
        info.Memo = "Test memo";
        info.ChartPrices = new List<ChartPrice>
        {
            new ChartPrice { Date = DateTime.Today, Price = 200, Volatility = 0.02, RSIS = 60, RSIL = 55 }
        };
        // Executionsをnull参照防止のため初期化
        info.Executions = new List<ExecutionList.Execution>();

        var formatter = new USStockFormatter();
        var output = formatter.ToOutputString(info);

        Assert.Contains("AAPL", output);
        Assert.Contains("Apple Inc.", output);
        Assert.Contains("NASDAQ", output);
        Assert.Contains("Technology", output);
        Assert.Contains("Test memo", output);
        Assert.Contains("株価", output);
        Assert.Contains("チャート", output);
    }

    // ダミー依存クラス
    private class DummyRepository : IAssetRepository
    {
        public Task<List<ScrapedPrice>> LoadHistoryAsync(string code) => Task.FromResult(new List<ScrapedPrice>());
        public Task SaveHistoryAsync(string code, List<ScrapedPrice> prices) => Task.CompletedTask;
        public Task DeleteHistoryAsync(string code, DateTime targetDate) => Task.CompletedTask;
        public Task RegisterHistoryAsync(string code, List<ScrapedPrice> prices) => Task.CompletedTask;
        public Task DeleteOldHistoryAsync(string code, DateTime deleteBefore) => Task.CompletedTask;
        public Task RegisterForcastHistoryAsync(string code, List<FullYearPerformanceForcast> forcasts) => Task.CompletedTask;
        public List<FullYearPerformanceForcast> GetPreviousForcasts(string code, string fiscalPeriod) => new List<FullYearPerformanceForcast>();
        public DateTime GetLastHistoryUpdateDay(string code) => DateTime.MinValue;
        public List<Dictionary<string, object>> GetChartPriceRows(string code, int limit) => new List<Dictionary<string, object>>();
    }
    private class DummyJudgementStrategy : IAssetJudgementStrategy
    {
        public bool IsPERUndervalued(AssetInfo asset, bool isLenient = false) => false;
        public bool IsPBRUndervalued(AssetInfo asset, bool isLenient = false) => false;
        public bool IsROEAboveThreshold(AssetInfo asset) => false;
        public bool IsAnnualProgressOnTrack(AssetInfo asset) => false;
        public bool IsHighYield(AssetInfo asset) => false;
        public bool IsHighMarketCap(AssetInfo asset) => false;
        public bool IsCloseToDividendRecordDate(AssetInfo asset) => false;
        public bool IsCloseToShareholderBenefitRecordDate(AssetInfo asset) => false;
        public bool IsCloseToQuarterEnd(AssetInfo asset) => false;
        public bool IsAfterQuarterEnd(AssetInfo asset) => false;
        public bool IsQuarterEnd(AssetInfo asset) => false;
        public bool IsJustSold(AssetInfo asset) => false;
        public bool IsOwnedNow(AssetInfo asset) => false;
        public bool IsGoldenCrossPossible(AssetInfo asset) => false;
        public bool HasRecentStockSplitOccurred(AssetInfo asset) => false;
        public bool ShouldAverageDown(AssetInfo asset, ExecutionList.Execution e) => false;
        public bool IsGranvilleCase1Matched(AssetInfo asset) => false;
        public bool IsGranvilleCase2Matched(AssetInfo asset) => false;
        public bool HasDisclosure(AssetInfo asset) => false;
        public bool IsRecordDate(AssetInfo asset) => false;
        public bool IsAfterRecordDate(AssetInfo asset) => false;
        public bool IsCloseToRecordDate(AssetInfo asset) => false;
        public bool ExtractAndValidateDateWithinOneMonth(AssetInfo asset) => false;
    }
    private class DummyCalculator : IAssetCalculator
    {
        public void UpdateProgress(AssetInfo asset) { }
        public void UpdateDividendPayoutRatio(AssetInfo asset) { }
        public void UpdateFullYearPerformanceForcastSummary(AssetInfo asset) { }
        public void SetupChartPrices(AssetInfo asset) { }

        public double GetDividendPayoutRatio(string adjustedDividendPerShare, string adjustedEarningsPerShare)
        {
            return 0.0; // Dummy implementation
        }

        public double GetDOE(double dividendPayoutRatio, double roe)
        {
            return 0.0; // Dummy implementation
        }

        public string GetDividendPerShareIncreased(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
        {
            return string.Empty; // Dummy implementation
        }

        public string GetIncreasedRate(string lastValue, string secondLastValue)
        {
            return string.Empty; // Dummy implementation
        }

        public string GetRevenueIncreasedSummary(string revenue1, string revenue2)
        {
            return string.Empty; // Dummy implementation
        }

        public string GetOrdinaryIncomeIncreasedSummary(string ordinaryIncome1, string ordinaryIncome2)
        {
            return string.Empty; // Dummy implementation
        }

        public string GetDividendPerShareIncreasedSummary(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
        {
            return string.Empty; // Dummy implementation
        }

        public string ConvertToPercentageStringWithSign(double v)
        {
            return string.Empty; // Dummy implementation
        }

        public string ConvertToStringWithSign(double number)
        {
            return string.Empty; // Dummy implementation
        }
    }
    private class DummySetupStrategy : IAssetSetupStrategy
    {
        public void UpdateExecutions(AssetInfo asset, List<ExecutionList.ListDetail> executionList) { }
        public void UpdateAveragePerPbr(AssetInfo asset, List<MasterList.AveragePerPbrDetails> masterList) { }
    }
    private class DummyAlertEvaluator : IAlertEvaluator
    {
        public bool ShouldAlert(AssetInfo asset) => false;
    }
}