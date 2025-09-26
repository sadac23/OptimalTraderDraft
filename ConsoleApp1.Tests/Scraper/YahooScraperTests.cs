using ConsoleApp1.Tests.Utils;
using ConsoleApp1.Assets;
using ConsoleApp1.Scraper.Contracts;
using ConsoleApp1.Scraper.Strategies;

namespace ConsoleApp1.Tests.Scraper
{
    [Collection("CommonUtils collection")]
    public class YahooScraperTests
    {
        [Fact]
        public async Task ScrapeHistory_DoesNotThrowException()
        {
            // Arrange
            var dummyUtils = new DummyCommonUtils();
            dummyUtils.Logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<Program>();
            CommonUtils.SetInstanceForTest(dummyUtils);

            // Strategyを明示的に指定（例：日本株用Strategy。必要に応じて他のStrategyに切り替え可）
            var yahooStrategy = new JapaneseStockYahooScrapeStrategy();
            var scraper = new YahooScraper(yahooStrategy);

            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1234" });
            var from = DateTime.Now.AddDays(-10);
            var to = DateTime.Now;

            // Act & Assert
            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.History);
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
            dummyUtils.Logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<Program>();
            CommonUtils.SetInstanceForTest(dummyUtils);

            IAssetScrapeStrategy strategy = classification switch
            {
                "0" => new IndexYahooScrapeStrategy(),
                "1" => new JapaneseStockYahooScrapeStrategy(),
                "2" => new JapaneseStockYahooScrapeStrategy(), // ETFも日本株戦略で対応している場合
                _ => throw new ArgumentException("不正な分類")
            };
            var scraper = new YahooScraper(strategy);

            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });
            var from = DateTime.Now.AddDays(-30);
            var to = DateTime.Now;

            // Act
            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.History);

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

                // 指数の場合はVolume=0, AdjustedClose=Close
                if (classification == "0")
                {
                    Assert.Equal(0, price.Volume);
                    Assert.Equal(price.Close, price.AdjustedClose);
                }
            }
        }

        [Fact]
        public async Task ScrapeProfile_DoesNotThrowException()
        {
            var yahooStrategy = new JapaneseStockYahooScrapeStrategy();
            var scraper = new YahooScraper(yahooStrategy);

            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1234" });
            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Profile);
            // 例外が発生しないことのみを確認
        }

        [Fact]
        public async Task ScrapeTop_DoesNotThrowException()
        {
            // ダミーCommonUtilsをセットし、必要なプロパティを設定
            var dummyUtils = new DummyCommonUtils();
            dummyUtils.Logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<Program>();
            // dummyUtils.HttpClient = new System.Net.Http.HttpClient(); // ← この行を削除

            CommonUtils.SetInstanceForTest(dummyUtils);

            var scraper = new YahooScraper(new JapaneseStockYahooScrapeStrategy());
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1489" });
            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);
            // 例外が発生しないことのみを確認
        }

        // ETF用
        [Theory]
        [InlineData("1489", "2")] // ETF
        public async Task ScrapeTop_ValidatesAllScrapedProperties_ForETF(string code, string classification)
        {
            var scraper = new YahooScraper(new JapaneseStockYahooScrapeStrategy());
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);

            Assert.False(string.IsNullOrEmpty(stockInfo.Name));
            Assert.False(string.IsNullOrEmpty(stockInfo.Code));
            Assert.False(string.IsNullOrEmpty(stockInfo.Classification));
            Assert.NotNull(stockInfo.LatestScrapedPrice);

            // LatestScrapedPriceの各メンバの検証
            Assert.True(stockInfo.LatestScrapedPrice.Date == CommonUtils.Instance.LastTradingDate);
            Assert.False(string.IsNullOrEmpty(stockInfo.LatestScrapedPrice.DateYYYYMMDD));
            Assert.True(stockInfo.LatestScrapedPrice.Open > 0);
            Assert.True(stockInfo.LatestScrapedPrice.High > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Low > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Close > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Volume >= 0);
            Assert.True(stockInfo.LatestScrapedPrice.AdjustedClose >= 0);

            Assert.False(string.IsNullOrEmpty(stockInfo.EarningsPeriod));
            Assert.False(string.IsNullOrEmpty(stockInfo.FundManagementCompany));
            Assert.True(stockInfo.TrustFeeRate >= 0);
            // ETF共通の他の検証...
        }

        // 個別株用
        [Theory]
        [InlineData("7203", "1")] // トヨタ自動車
        [InlineData("6503", "1")] // 三菱電機
        public async Task ScrapeTop_ValidatesAllScrapedProperties_ForJapaneseIndividualStocks(string code, string classification)
        {
            var scraper = new YahooScraper(new JapaneseStockYahooScrapeStrategy());
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);

            Assert.False(string.IsNullOrEmpty(stockInfo.Name));
            Assert.False(string.IsNullOrEmpty(stockInfo.Code));
            Assert.False(string.IsNullOrEmpty(stockInfo.Classification));
            Assert.NotNull(stockInfo.LatestScrapedPrice);

            // LatestScrapedPriceの各メンバの検証
            Assert.True(stockInfo.LatestScrapedPrice.Date == CommonUtils.Instance.LastTradingDate);
            Assert.False(string.IsNullOrEmpty(stockInfo.LatestScrapedPrice.DateYYYYMMDD));
            Assert.True(stockInfo.LatestScrapedPrice.Open > 0);
            Assert.True(stockInfo.LatestScrapedPrice.High > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Low > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Close > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Volume >= 0);
            Assert.True(stockInfo.LatestScrapedPrice.AdjustedClose >= 0);

            Assert.False(string.IsNullOrEmpty(stockInfo.PressReleaseDate));
            Assert.NotNull(stockInfo.LatestTradingVolume);
            Assert.NotNull(stockInfo.MarginBuyBalance);
            Assert.NotNull(stockInfo.MarginSellBalance);
            Assert.NotNull(stockInfo.MarginBalanceDate);
            // 個別株共通の他の検証...
        }

        // 指数用
        [Theory]
        [InlineData("998407", "0")] // 指数
        public async Task ScrapeTop_ValidatesAllScrapedProperties_ForIndex(string code, string classification)
        {
            var scraper = new YahooScraper(new JapaneseStockYahooScrapeStrategy());
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);

            Assert.False(string.IsNullOrEmpty(stockInfo.Name));
            Assert.False(string.IsNullOrEmpty(stockInfo.Code));
            Assert.False(string.IsNullOrEmpty(stockInfo.Classification));
            Assert.NotNull(stockInfo.LatestScrapedPrice);

            // LatestScrapedPriceの各メンバの検証
            Assert.True(stockInfo.LatestScrapedPrice.Date == CommonUtils.Instance.LastTradingDate);
            Assert.False(string.IsNullOrEmpty(stockInfo.LatestScrapedPrice.DateYYYYMMDD));
            Assert.True(stockInfo.LatestScrapedPrice.Open > 0);
            Assert.True(stockInfo.LatestScrapedPrice.High > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Low > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Close > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Volume >= 0);
            Assert.True(stockInfo.LatestScrapedPrice.AdjustedClose >= 0);

            // 指数共通の他の検証...
        }
    }
}