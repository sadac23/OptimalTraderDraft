using System;
using System.Collections.Generic;
using Xunit;
using ConsoleApp1;
using ConsoleApp1.Tests; // DummyCommonUtilsを参照するため追加

namespace ConsoleApp1.Tests
{
    public class StockInfoTests
    {
        // ダミー WatchStock クラス
        private class DummyWatchStock
        {
            public string Code { get; set; } = "1234";
            public string Classification { get; set; } = "1";
            public string IsFavorite { get; set; } = "1";
            public string Memo { get; set; } = "テストメモ";
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            var watchStock = new DummyWatchStock();
            var stockInfo = new StockInfo(new WatchList.WatchStock
            {
                Code = watchStock.Code,
                Classification = watchStock.Classification,
                IsFavorite = watchStock.IsFavorite,
                Memo = watchStock.Memo
            });

            Assert.Equal("1234", stockInfo.Code);
            Assert.Equal("1", stockInfo.Classification);
            Assert.True(stockInfo.IsFavorite);
            Assert.Equal("テストメモ", stockInfo.Memo);
            Assert.NotNull(stockInfo.ScrapedPrices);
            Assert.NotNull(stockInfo.FullYearPerformances);
            Assert.NotNull(stockInfo.FullYearProfits);
            Assert.NotNull(stockInfo.QuarterlyPerformances);
            Assert.NotNull(stockInfo.FullYearPerformancesForcasts);
            Assert.NotNull(stockInfo.ChartPrices);
            Assert.NotNull(stockInfo.Disclosures);
        }

        [Fact]
        public void IsPERUndervalued_ReturnsTrue_WhenPERIsLow()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.Per = 10;
            typeof(StockInfo)
                .GetProperty("AveragePer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                ?.SetValue(stockInfo, 15d);

            Assert.True(stockInfo.IsPERUndervalued());
        }

        [Fact]
        public void IsHighYield_ReturnsTrue_WhenYieldIsHigh()
        {
            CommonUtils.SetInstanceForTest(new DummyCommonUtils());
            var stockInfo = CreateStockInfo();
            stockInfo.DividendYield = 0.03;
            stockInfo.ShareholderBenefitYield = 0.02;

            Assert.True(stockInfo.IsHighYield());
        }

        [Fact]
        public void IsOwnedNow_ReturnsTrue_WhenBuyExecutionExists()
        {
            CommonUtils.SetInstanceForTest(new DummyCommonUtils());
            var stockInfo = CreateStockInfo();
            stockInfo.Executions = new List<ExecutionList.Execution>
            {
                new ExecutionList.Execution
                {
                    BuyOrSell = "買",
                    HasSellExecuted = false
                }
            };

            Assert.True(stockInfo.IsOwnedNow());
        }

        // ヘルパー
        private StockInfo CreateStockInfo()
        {
            return new StockInfo(new WatchList.WatchStock
            {
                Code = "1234",
                Classification = "1",
                IsFavorite = "1",
                Memo = "テスト"
            });
        }

        // DummyCommonUtilsの定義は削除
    }
}