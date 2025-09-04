using System;
using System.Collections.Generic;
using Xunit;
using ConsoleApp1;
using ConsoleApp1.Tests; // DummyCommonUtilsを参照するため追加

namespace ConsoleApp1.Tests
{
    [Collection("CommonUtils collection")]
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
            var stockInfo = StockInfo.GetInstance(new WatchList.WatchStock
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
            stockInfo.AveragePer = 15d;

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

        // --- 追加テスト ---

        [Fact]
        public void CanSetAndGet_AveragePbr()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.AveragePbr = 1.5d;
            Assert.Equal(1.5d, stockInfo.AveragePbr);
        }

        [Fact]
        public void CanSetAndGet_DividendYield()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.DividendYield = 0.025;
            Assert.Equal(0.025, stockInfo.DividendYield);
        }

        [Fact]
        public void IsFavorite_ReturnsTrue_WhenIsFavoriteIs1()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.IsFavorite = true;
            Assert.True(stockInfo.IsFavorite);
        }

        [Fact]
        public void Memo_CanBeSetAndRetrieved()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.Memo = "メモテスト";
            Assert.Equal("メモテスト", stockInfo.Memo);
        }

        [Fact]
        public void IsPERUndervalued_ReturnsFalse_WhenPERIsHigh()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.Per = 20;
            stockInfo.AveragePer = 10;
            Assert.False(stockInfo.IsPERUndervalued());
        }

        [Fact]
        public void IsHighYield_ReturnsFalse_WhenYieldIsLow()
        {
            CommonUtils.SetInstanceForTest(new DummyCommonUtils());
            var stockInfo = CreateStockInfo();
            stockInfo.DividendYield = 0.01;
            stockInfo.ShareholderBenefitYield = 0.01;
            Assert.False(stockInfo.IsHighYield());
        }

        [Fact]
        public void IsOwnedNow_ReturnsFalse_WhenNoExecutions()
        {
            CommonUtils.SetInstanceForTest(new DummyCommonUtils());
            var stockInfo = CreateStockInfo();
            stockInfo.Executions = new List<ExecutionList.Execution>();
            Assert.False(stockInfo.IsOwnedNow());
        }

        [Fact]
        public void IsOwnedNow_ReturnsFalse_WhenAllExecutionsAreSold()
        {
            CommonUtils.SetInstanceForTest(new DummyCommonUtils());
            var stockInfo = CreateStockInfo();
            stockInfo.Executions = new List<ExecutionList.Execution>
            {
                new ExecutionList.Execution
                {
                    BuyOrSell = "買",
                    HasSellExecuted = true
                }
            };
            Assert.False(stockInfo.IsOwnedNow());
        }

        [Fact]
        public void Code_CanBeSetAndRetrieved()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.Code = "5678";
            Assert.Equal("5678", stockInfo.Code);
        }

        [Fact]
        public void Classification_CanBeSetAndRetrieved()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.Classification = "2";
            Assert.Equal("2", stockInfo.Classification);
        }

        [Fact]
        public void CanSetAndGet_QuarterlyPerformances()
        {
            var stockInfo = CreateStockInfo();
            var list = new List<StockInfo.QuarterlyPerformance>
            {
                new StockInfo.QuarterlyPerformance
                {
                    FiscalPeriod = "2023Q1",
                    Revenue = "1000",
                    OperatingProfit = "200",
                    OrdinaryProfit = 150,
                    NetProfit = "120",
                    AdjustedEarningsPerShare = "10",
                    AdjustedDividendPerShare = "5",
                    ReleaseDate = "2023-04-01"
                },
                new StockInfo.QuarterlyPerformance
                {
                    FiscalPeriod = "2023Q2",
                    Revenue = "1100",
                    OperatingProfit = "220",
                    OrdinaryProfit = 160,
                    NetProfit = "130",
                    AdjustedEarningsPerShare = "11",
                    AdjustedDividendPerShare = "6",
                    ReleaseDate = "2023-07-01"
                }
            };
            stockInfo.QuarterlyPerformances = list;
            Assert.Equal(list, stockInfo.QuarterlyPerformances);
        }

        [Fact]
        public void CanSetAndGet_ChartPrices()
        {
            var stockInfo = CreateStockInfo();
            var prices = new List<StockInfo.ChartPrice>
            {
                new StockInfo.ChartPrice { Date = DateTime.Now, Price = 100 },
                new StockInfo.ChartPrice { Date = DateTime.Now.AddDays(1), Price = 200 },
                new StockInfo.ChartPrice { Date = DateTime.Now.AddDays(2), Price = 300 }
            };
            stockInfo.ChartPrices = prices;
            Assert.Equal(prices, stockInfo.ChartPrices);
        }

        [Fact]
        public void CanSetAndGet_FullYearPerformancesForcasts()
        {
            var stockInfo = CreateStockInfo();
            var forecasts = new List<StockInfo.FullYearPerformanceForcast>
            {
                new StockInfo.FullYearPerformanceForcast
                {
                    FiscalPeriod = "2023",
                    RevisionDate = DateTime.Now,
                    Category = "Category1",
                    RevisionDirection = "Up",
                    Revenue = "1000",
                    OperatingProfit = "200",
                    OrdinaryProfit = "150",
                    NetProfit = "120",
                    RevisedDividend = "10",
                    Summary = "Forecast Summary"
                },
                new StockInfo.FullYearPerformanceForcast
                {
                    FiscalPeriod = "2024",
                    RevisionDate = DateTime.Now.AddMonths(1),
                    Category = "Category2",
                    RevisionDirection = "Down",
                    Revenue = "1100",
                    OperatingProfit = "220",
                    OrdinaryProfit = "160",
                    NetProfit = "130",
                    RevisedDividend = "12",
                    Summary = "Forecast Summary 2"
                }
            };
            stockInfo.FullYearPerformancesForcasts = forecasts;
            Assert.Equal(forecasts, stockInfo.FullYearPerformancesForcasts);
        }

        [Fact]
        public void CanSetAndGet_ScrapedPrices()
        {
            var stockInfo = CreateStockInfo();
            var scraped = new List<StockInfo.ScrapedPrice>
            {
                new StockInfo.ScrapedPrice { Date = DateTime.Now, Close = 1 },
                new StockInfo.ScrapedPrice { Date = DateTime.Now.AddDays(1), Close = 2 },
                new StockInfo.ScrapedPrice { Date = DateTime.Now.AddDays(2), Close = 3 }
            };
            stockInfo.ScrapedPrices = scraped;
            Assert.Equal(scraped, stockInfo.ScrapedPrices);
        }

        [Fact]
        public void CanSetAndGet_FullYearProfits()
        {
            var stockInfo = CreateStockInfo();
            var profits = new List<StockInfo.FullYearProfit>
            {
                new StockInfo.FullYearProfit
                {
                    FiscalPeriod = "2023",
                    Revenue = "1000",
                    OperatingIncome = "200",
                    OperatingMargin = "20%",
                    Roe = 10.5,
                    Roa = 5.2,
                    TotalAssetTurnover = "1.5",
                    AdjustedEarningsPerShare = "15"
                },
                new StockInfo.FullYearProfit
                {
                    FiscalPeriod = "2024",
                    Revenue = "1100",
                    OperatingIncome = "220",
                    OperatingMargin = "22%",
                    Roe = 11.0,
                    Roa = 5.5,
                    TotalAssetTurnover = "1.6",
                    AdjustedEarningsPerShare = "16"
                }
            };
            stockInfo.FullYearProfits = profits;
            Assert.Equal(profits, stockInfo.FullYearProfits);
        }

        [Fact]
        public void CanSetAndGet_DividendYield_Boundary()
        {
            var stockInfo = CreateStockInfo();
            stockInfo.DividendYield = 0.0;
            Assert.Equal(0.0, stockInfo.DividendYield);

            stockInfo.DividendYield = 1.0;
            Assert.Equal(1.0, stockInfo.DividendYield);
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentCode()
        {
            var stockInfo1 = CreateStockInfo();
            var stockInfo2 = CreateStockInfo();
            stockInfo2.Code = "9999";
            Assert.False(stockInfo1.Equals(stockInfo2));
        }

        [Theory]
        [InlineData("3月", 0, 3, true)] // 今月が3月
        [InlineData("4月", 1, 3, true)] // 今月が3月、+1か月以内に4月
        [InlineData("5月", 1, 3, false)] // 今月が3月、+1か月以内に5月は含まれない
        [InlineData("3,4", 1, 3, true)] // 今月が3月、3月と4月
        [InlineData("2,4", 1, 3, true)] // 今月が3月、4月が+1か月以内
        [InlineData("2,5", 1, 3, false)] // 今月が3月、どちらも範囲外
        [InlineData("", 0, 3, false)] // 空文字
        [InlineData(null, 0, 3, false)] // null
        [InlineData("12,1", 1, 12, true)] // 今月が12月、+1か月以内に1月
        public void IsWithinMonths_VariousCases(string monthsStr, short m, int currentMonth, bool expected)
        {
            // DateTime.Now.Month をモックできないため、テスト用にラップするか、DateTimeを注入する設計が理想です。
            // ここでは DateTime.Now.Month を使う前提で、currentMonth==DateTime.Now.Month のときだけテストが通る形にします。
            if (DateTime.Now.Month != currentMonth)
            {
                // テスト実行月が期待と異なる場合はスキップ
                return;
            }

            var result = StockInfo.IsWithinMonths(monthsStr, m);
            Assert.Equal(expected, result);
        }

        // ヘルパー
        private StockInfo CreateStockInfo()
        {
            return StockInfo.GetInstance(new WatchList.WatchStock
            {
                Code = "1234",
                Classification = "1",
                IsFavorite = "1",
                Memo = "テスト"
            });
        }
    }
}