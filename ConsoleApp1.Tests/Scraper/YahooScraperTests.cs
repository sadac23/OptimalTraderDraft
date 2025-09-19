using ConsoleApp1.Tests.Utils;
using ConsoleApp1.Assets;

namespace ConsoleApp1.Tests.Scraper
{
    [Collection("CommonUtils collection")]
    public class YahooScraperTests
    {
        [Fact]
        public async Task ScrapeHistory_DoesNotThrowException()
        {
            var scraper = new YahooScraper();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1234" });
            var from = DateTime.Now.AddDays(-10);
            var to = DateTime.Now;
            await scraper.ScrapeHistory(stockInfo, from, to);
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

            var scraper = new YahooScraper();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });
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
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1234" });
            await scraper.ScrapeProfile(stockInfo);
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

            var scraper = new YahooScraper();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1489" });
            await scraper.ScrapeTop(stockInfo);
            // ��O���������Ȃ����Ƃ݂̂��m�F
        }

        // ETF�p
        [Theory]
        [InlineData("1489", "2")] // ETF
        public async Task ScrapeTop_ValidatesAllScrapedProperties_ForETF(string code, string classification)
        {
            var scraper = new YahooScraper();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeTop(stockInfo);

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
            var scraper = new YahooScraper();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeTop(stockInfo);

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
            var scraper = new YahooScraper();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = code, Classification = classification });

            await scraper.ScrapeTop(stockInfo);

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

        [Fact]
        public async Task ScrapeDisclosure_DoesNotThrowException()
        {
            var scraper = new YahooScraper();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock { Code = "1234" });
            await scraper.ScrapeDisclosure(stockInfo);
            // ��O���������Ȃ����Ƃ݂̂��m�F
        }

        [Fact]
        public void GetDate_ParsesVariousFormats()
        {
            var scraper = new YahooScraper();
            var date = scraper.GetType().GetMethod("GetDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(date.Invoke(scraper, new object[] { "2024�N6��1��" }));
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
            var result = (DateTime)method.Invoke(scraper, new object[] { now, "6������" });
            Assert.Equal(new DateTime(2024, 6, 30), result);
        }

        [Fact]
        public void ParseMonth_ValidAndInvalid()
        {
            var scraper = new YahooScraper();
            var method = scraper.GetType().GetMethod("ParseMonth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.Equal(6, method.Invoke(scraper, new object[] { "6������" }));
            Assert.Equal(-1, method.Invoke(scraper, new object[] { "�s���ȕ�����" }));
        }
    }
}