using System;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleApp1.Tests
{
    public class YahooScraperTests
    {
        [Fact]
        public async Task ScrapeHistory_DoesNotThrowException()
        {
            var scraper = new YahooScraper();
            var stockInfo = new StockInfo(new WatchList.WatchStock { Code = "1234" });
            var from = DateTime.Now.AddDays(-10);
            var to = DateTime.Now;
            await scraper.ScrapeHistory(stockInfo, from, to);
            // 例外が発生しないことのみを確認
        }

        [Theory]
        [InlineData("998407", "0")] // 指数（例: 日経平均株価）
        [InlineData("7203", "1")] // 日本個別株（例: トヨタ自動車）
        [InlineData("1489", "2")] // ETF
        public async Task ScrapeHistory_SetsScrapedPrices(string code, string classification)
        {
            // Arrange
            var dummyUtils = new DummyCommonUtils();
            dummyUtils.Logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ConsoleApp1.Program>();
            CommonUtils.SetInstanceForTest(dummyUtils);

            var scraper = new YahooScraper();
            var stockInfo = new StockInfo(new WatchList.WatchStock { Code = code, Classification = classification });
            var from = DateTime.Now.AddDays(-30);
            var to = DateTime.Now;

            // Act
            await scraper.ScrapeHistory(stockInfo, from, to);

            // Assert
            Assert.NotNull(stockInfo.ScrapedPrices);
            Assert.NotEmpty(stockInfo.ScrapedPrices);

            foreach (var price in stockInfo.ScrapedPrices)
            {
                Assert.True(price.Date > DateTime.MinValue);
                Assert.False(string.IsNullOrEmpty(price.DateYYYYMMDD));
                Assert.True(price.Open >= 0);
                Assert.True(price.High >= 0);
                Assert.True(price.Low >= 0);
                Assert.True(price.Close >= 0);
                Assert.True(price.Volume >= 0);
                Assert.True(price.AdjustedClose >= 0);
            }
        }

        [Fact]
        public async Task ScrapeProfile_DoesNotThrowException()
        {
            var scraper = new YahooScraper();
            var stockInfo = new StockInfo(new WatchList.WatchStock { Code = "1234" });
            await scraper.ScrapeProfile(stockInfo);
            // 例外が発生しないことのみを確認
        }

        [Fact]
        public async Task ScrapeTop_DoesNotThrowException()
        {
            // ダミーCommonUtilsをセットし、必要なプロパティを設定
            var dummyUtils = new DummyCommonUtils();
            dummyUtils.Logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ConsoleApp1.Program>();
            // dummyUtils.HttpClient = new System.Net.Http.HttpClient(); // ← この行を削除

            CommonUtils.SetInstanceForTest(dummyUtils);

            var scraper = new YahooScraper();
            var stockInfo = new StockInfo(new WatchList.WatchStock { Code = "1489" });
            await scraper.ScrapeTop(stockInfo);
            // 例外が発生しないことのみを確認
        }

        [Theory]
        [InlineData("1489", "2")] // ETF
        [InlineData("7203", "1")] // 日本個別株（例: トヨタ自動車）
        [InlineData("998407", "0")] // 指数（例: 日経平均株価）
        public async Task ScrapeTop_SetsStockInfoProperties(string code, string classification)
        {
            // ダミーCommonUtilsをセットし、必要なプロパティを設定
            var dummyUtils = new DummyCommonUtils();
            dummyUtils.Logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ConsoleApp1.Program>();
            // dummyUtils.HttpClient = new System.Net.Http.HttpClient(); // ← この行を削除

            CommonUtils.SetInstanceForTest(dummyUtils);

            var scraper = new YahooScraper();
            var stockInfo = new StockInfo(new WatchList.WatchStock { Code = code, Classification = classification });
            await scraper.ScrapeTop(stockInfo);

            // 取得結果の検証（値は実際のページ内容に依存）
            Assert.False(string.IsNullOrEmpty(stockInfo.Name));

            // ETF（classification "2"）または指数（classification "0"）以外のみアサート
            if (stockInfo.Classification != CommonUtils.Instance.Classification.JapaneseETFs
                && stockInfo.Classification != CommonUtils.Instance.Classification.Index)
            {
                Assert.NotNull(stockInfo.PressReleaseDate);
                Assert.NotNull(stockInfo.LatestTradingVolume);
            }

            // 指数（classification "0"）以外はアサートする
            if (stockInfo.Classification != CommonUtils.Instance.Classification.Index)
            {
                Assert.NotNull(stockInfo.MarginBuyBalance);
                Assert.NotNull(stockInfo.MarginSellBalance);
                Assert.NotNull(stockInfo.MarginBalanceDate);
            }

            // ETFの場合の追加情報
            if (stockInfo.Classification == CommonUtils.Instance.Classification.JapaneseETFs)
            {
                Assert.NotNull(stockInfo.EarningsPeriod);
                Assert.NotNull(stockInfo.FundManagementCompany);
                // TrustFeeRateはdouble型なので0以上であることを確認
                Assert.True(stockInfo.TrustFeeRate >= 0);
            }
        }

        [Fact]
        public async Task ScrapeDisclosure_DoesNotThrowException()
        {
            var scraper = new YahooScraper();
            var stockInfo = new StockInfo(new WatchList.WatchStock { Code = "1234" });
            await scraper.ScrapeDisclosure(stockInfo);
            // 例外が発生しないことのみを確認
        }

        [Fact]
        public void GetDate_ParsesVariousFormats()
        {
            var scraper = new YahooScraper();
            var date = scraper.GetType().GetMethod("GetDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(date.Invoke(scraper, new object[] { "2024年6月1日" }));
        }

        [Fact]
        public void ConvertToDouble_ParsesPercentString()
        {
            var scraper = new YahooScraper();
            var method = scraper.GetType().GetMethod("ConvertToDouble", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (double)method.Invoke(scraper, new object[] { "3.5%" });
            Assert.Equal(0.035, result, 3);
        }

        [Fact]
        public void ConvertToDatetime_ParsesDateAndTime()
        {
            var scraper = new YahooScraper();
            var method = scraper.GetType().GetMethod("ConvertToDatetime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (DateTime)method.Invoke(scraper, new object[] { "6/1" });
            Assert.Equal(DateTime.Now.Year, result.Year);
        }

        [Fact]
        public void GetNextClosingDate_ReturnsNextDate()
        {
            var scraper = new YahooScraper();
            var method = scraper.GetType().GetMethod("GetNextClosingDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var now = new DateTime(2024, 5, 1);
            var result = (DateTime)method.Invoke(scraper, new object[] { now, "6月末日" });
            Assert.Equal(new DateTime(2024, 6, 30), result);
        }

        [Fact]
        public void ParseMonth_ValidAndInvalid()
        {
            var scraper = new YahooScraper();
            var method = scraper.GetType().GetMethod("ParseMonth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.Equal(6, method.Invoke(scraper, new object[] { "6月末日" }));
            Assert.Equal(-1, method.Invoke(scraper, new object[] { "不正な文字列" }));
        }
    }
}