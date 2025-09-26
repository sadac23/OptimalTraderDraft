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

            // Strategy�𖾎��I�Ɏw��i��F���{���pStrategy�B�K�v�ɉ����đ���Strategy�ɐ؂�ւ��j
            var yahooStrategy = new JapaneseStockYahooScrapeStrategy();
            var scraper = new YahooScraper(yahooStrategy);

            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1234" });
            var from = DateTime.Now.AddDays(-10);
            var to = DateTime.Now;

            // Act & Assert
            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.History);
            // ��O���������Ȃ����Ƃ݂̂��m�F
        }

        [Theory]
        [InlineData("998407", "0")] // �w���i��: ���o���ϊ����j
        [InlineData("7203", "1")] // ���{�ʊ��i��: �g���^�����ԁj   
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
                "2" => new JapaneseStockYahooScrapeStrategy(), // ETF�����{���헪�őΉ����Ă���ꍇ
                _ => throw new ArgumentException("�s���ȕ���")
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

                // �w���̏ꍇ��Volume=0, AdjustedClose=Close
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
            // ��O���������Ȃ����Ƃ݂̂��m�F
        }

        [Fact]
        public async Task ScrapeTop_DoesNotThrowException()
        {
            // �_�~�[CommonUtils���Z�b�g���A�K�v�ȃv���p�e�B��ݒ�
            var dummyUtils = new DummyCommonUtils();
            dummyUtils.Logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<Program>();
            // dummyUtils.HttpClient = new System.Net.Http.HttpClient(); // �� ���̍s���폜

            CommonUtils.SetInstanceForTest(dummyUtils);

            var scraper = new YahooScraper(new JapaneseStockYahooScrapeStrategy());
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1489" });
            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);
            // ��O���������Ȃ����Ƃ݂̂��m�F
        }

        // ETF�p
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

            // LatestScrapedPrice�̊e�����o�̌���
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
            // ETF���ʂ̑��̌���...
        }

        // �ʊ��p
        [Theory]
        [InlineData("7203", "1")] // �g���^������
        [InlineData("6503", "1")] // �O�H�d�@
        public async Task ScrapeTop_ValidatesAllScrapedProperties_ForJapaneseIndividualStocks(string code, string classification)
        {
            var scraper = new YahooScraper(new JapaneseStockYahooScrapeStrategy());
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);

            Assert.False(string.IsNullOrEmpty(stockInfo.Name));
            Assert.False(string.IsNullOrEmpty(stockInfo.Code));
            Assert.False(string.IsNullOrEmpty(stockInfo.Classification));
            Assert.NotNull(stockInfo.LatestScrapedPrice);

            // LatestScrapedPrice�̊e�����o�̌���
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
            // �ʊ����ʂ̑��̌���...
        }

        // �w���p
        [Theory]
        [InlineData("998407", "0")] // �w��
        public async Task ScrapeTop_ValidatesAllScrapedProperties_ForIndex(string code, string classification)
        {
            var scraper = new YahooScraper(new JapaneseStockYahooScrapeStrategy());
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeAsync(stockInfo, ScrapeTarget.Top);

            Assert.False(string.IsNullOrEmpty(stockInfo.Name));
            Assert.False(string.IsNullOrEmpty(stockInfo.Code));
            Assert.False(string.IsNullOrEmpty(stockInfo.Classification));
            Assert.NotNull(stockInfo.LatestScrapedPrice);

            // LatestScrapedPrice�̊e�����o�̌���
            Assert.True(stockInfo.LatestScrapedPrice.Date == CommonUtils.Instance.LastTradingDate);
            Assert.False(string.IsNullOrEmpty(stockInfo.LatestScrapedPrice.DateYYYYMMDD));
            Assert.True(stockInfo.LatestScrapedPrice.Open > 0);
            Assert.True(stockInfo.LatestScrapedPrice.High > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Low > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Close > 0);
            Assert.True(stockInfo.LatestScrapedPrice.Volume >= 0);
            Assert.True(stockInfo.LatestScrapedPrice.AdjustedClose >= 0);

            // �w�����ʂ̑��̌���...
        }
    }
}