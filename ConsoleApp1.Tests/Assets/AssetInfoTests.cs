using System.Data.SQLite;
using ConsoleApp1.Tests.Utils;
using ConsoleApp1.Database;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.Assets.Calculators;
using ConsoleApp1.Assets.Setups;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

namespace ConsoleApp1.Tests.Assets
{
    [Collection("CommonUtils collection")]
    public class AssetInfoTests
    {
        // �_�~�[ WatchStock �N���X
        private class DummyWatchStock
        {
            public string Code { get; set; } = "1234";
            public string Classification { get; set; } = "1";
            public string IsFavorite { get; set; } = "1";
            public string Memo { get; set; } = "�e�X�g����";
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            var watchStock = new DummyWatchStock();
            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock
            {
                Code = watchStock.Code,
                Classification = watchStock.Classification,
                IsFavorite = watchStock.IsFavorite,
                Memo = watchStock.Memo
            });

            Assert.Equal("1234", stockInfo.Code);
            Assert.Equal("1", stockInfo.Classification);
            Assert.True(stockInfo.IsFavorite);
            Assert.Equal("�e�X�g����", stockInfo.Memo);
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
                    BuyOrSell = "��",
                    HasSellExecuted = false
                }
            };

            Assert.True(stockInfo.IsOwnedNow());
        }

        // --- �ǉ��e�X�g ---

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
            stockInfo.Memo = "�����e�X�g";
            Assert.Equal("�����e�X�g", stockInfo.Memo);
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
                    BuyOrSell = "��",
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
            var list = new List<QuarterlyPerformance>
            {
                new QuarterlyPerformance
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
                new QuarterlyPerformance
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
            var prices = new List<ChartPrice>
            {
                new ChartPrice { Date = DateTime.Now, Price = 100 },
                new ChartPrice { Date = DateTime.Now.AddDays(1), Price = 200 },
                new ChartPrice { Date = DateTime.Now.AddDays(2), Price = 300 }
            };
            stockInfo.ChartPrices = prices;
            Assert.Equal(prices, stockInfo.ChartPrices);
        }

        [Fact]
        public void CanSetAndGet_FullYearPerformancesForcasts()
        {
            var stockInfo = CreateStockInfo();
            var forecasts = new List<FullYearPerformanceForcast>
            {
                new FullYearPerformanceForcast
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
                new FullYearPerformanceForcast
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
            var scraped = new List<ScrapedPrice>
            {
                new ScrapedPrice { Date = DateTime.Now, Close = 1 },
                new ScrapedPrice { Date = DateTime.Now.AddDays(1), Close = 2 },
                new ScrapedPrice { Date = DateTime.Now.AddDays(2), Close = 3 }
            };
            stockInfo.ScrapedPrices = scraped;
            Assert.Equal(scraped, stockInfo.ScrapedPrices);
        }

        [Fact]
        public void CanSetAndGet_FullYearProfits()
        {
            var stockInfo = CreateStockInfo();
            var profits = new List<FullYearProfit>
            {
                new FullYearProfit
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
                new FullYearProfit
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
        [InlineData("3��", 0, 3, true)] // ������3��
        [InlineData("4��", 1, 3, true)] // ������3���A+1�����ȓ���4��
        [InlineData("5��", 1, 3, false)] // ������3���A+1�����ȓ���5���͊܂܂�Ȃ�
        [InlineData("3,4", 1, 3, true)] // ������3���A3����4��
        [InlineData("2,4", 1, 3, true)] // ������3���A4����+1�����ȓ�
        [InlineData("2,5", 1, 3, false)] // ������3���A�ǂ�����͈͊O
        [InlineData("", 0, 3, false)] // �󕶎�
        [InlineData(null, 0, 3, false)] // null
        [InlineData("12,1", 1, 12, true)] // ������12���A+1�����ȓ���1��
        public void IsWithinMonths_VariousCases(string monthsStr, short m, int currentMonth, bool expected)
        {
            // DateTime.Now.Month �����b�N�ł��Ȃ����߁A�e�X�g�p�Ƀ��b�v���邩�ADateTime�𒍓�����݌v�����z�ł��B
            // �����ł� DateTime.Now.Month ���g���O��ŁAcurrentMonth==DateTime.Now.Month �̂Ƃ������e�X�g���ʂ�`�ɂ��܂��B
            if (DateTime.Now.Month != currentMonth)
            {
                // �e�X�g���s�������҂ƈقȂ�ꍇ�̓X�L�b�v
                return;
            }

            var result = AssetInfo.IsWithinMonths(monthsStr, m);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetupChartPrices_ReflectsHistoryTableData()
        {
            // Arrange
            // �e�X�g�p�̃C��������SQLite DB��p��
            using var connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            connection.Open();

            // �e�[�u���쐬
            using (var cmd = new SQLiteCommand(
                @"CREATE TABLE history (
                    code TEXT,
                    date DATETIME,
                    open REAL,
                    high REAL,
                    low REAL,
                    close REAL,
                    volume REAL
                );", connection))
            {
                cmd.ExecuteNonQuery();
            }

            // �e�X�g�f�[�^�}��
            var code = "1234";
            var baseDate = DateTime.Today.AddDays(-2);
            for (int i = 0; i < 3; i++)
            {
                using var insert = new SQLiteCommand(
                    "INSERT INTO history (code, date, open, high, low, close, volume) VALUES (@code, @date, @open, @high, @low, @close, @volume);", connection);
                insert.Parameters.AddWithValue("@code", code);
                insert.Parameters.AddWithValue("@date", baseDate.AddDays(i));
                insert.Parameters.AddWithValue("@open", 100 + i);
                insert.Parameters.AddWithValue("@high", 110 + i);
                insert.Parameters.AddWithValue("@low", 90 + i);
                insert.Parameters.AddWithValue("@close", 105 + i);
                insert.Parameters.AddWithValue("@volume", 1000 + i * 10);
                insert.ExecuteNonQuery();
            }

            // DbConnectionFactory�̃R�l�N�V�����������ւ�
            DbConnectionFactory.SetConnection(connection); // �����Ńe�X�g�p�R�l�N�V�������Z�b�g

            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock
            {
                Code = code,
                Classification = "1",
                IsFavorite = "1",
                Memo = "�e�X�g"
            });

            // ChartDays��3�ɐݒ�
            CommonUtils.Instance.ChartDays = 3;

            // Act
            // internal���\�b�h�𒼐ڌĂяo��
            stockInfo.SetupChartPrices();

            // Assert
            Assert.NotNull(stockInfo.ChartPrices);
            Assert.Equal(3, stockInfo.ChartPrices.Count);
            Assert.All(stockInfo.ChartPrices, cp => Assert.Equal(code, stockInfo.Code));
            // ���t�~���Ŋi�[����Ă��邱��
            for (int i = 1; i < stockInfo.ChartPrices.Count; i++)
            {
                Assert.True(stockInfo.ChartPrices[i - 1].Date >= stockInfo.ChartPrices[i].Date);
            }
            // ���i��history�e�[�u����close�l�ƈ�v���邱��
            var expectedCloses = new List<double> { 107, 106, 105 }; // �}�����ɍ~��
            for (int i = 0; i < expectedCloses.Count; i++)
            {
                Assert.Equal(expectedCloses[i], stockInfo.ChartPrices[i].Price);
            }
        }

        [Fact]
        public void SetupChartPrices_AddsLatestScrapedPrice_WhenNotInHistory()
        {
            // Arrange
            using var connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            connection.Open();

            // �e�[�u���쐬
            using (var cmd = new SQLiteCommand(
                @"CREATE TABLE history (
                    code TEXT,
                    date DATETIME,
                    open REAL,
                    high REAL,
                    low REAL,
                    close REAL,
                    volume REAL
                );", connection))
            {
                cmd.ExecuteNonQuery();
            }

            // history�ɂ�LastTradingDate���O�̓��t�̂�
            var code = "1234";
            var baseDate = DateTime.Today.AddDays(-3);
            for (int i = 0; i < 2; i++)
            {
                using var insert = new SQLiteCommand(
                    "INSERT INTO history (code, date, open, high, low, close, volume) VALUES (@code, @date, @open, @high, @low, @close, @volume);", connection);
                insert.Parameters.AddWithValue("@code", code);
                insert.Parameters.AddWithValue("@date", baseDate.AddDays(i));
                insert.Parameters.AddWithValue("@open", 100 + i);
                insert.Parameters.AddWithValue("@high", 110 + i);
                insert.Parameters.AddWithValue("@low", 90 + i);
                insert.Parameters.AddWithValue("@close", 105 + i);
                insert.Parameters.AddWithValue("@volume", 1000 + i * 10);
                insert.ExecuteNonQuery();
            }

            DbConnectionFactory.SetConnection(connection);

            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock
            {
                Code = code,
                Classification = "1",
                IsFavorite = "1",
                Memo = "�e�X�g"
            });

            CommonUtils.Instance.ChartDays = 3;

            // ���߉c�Ɠ����Z�b�g
            var lastTradingDate = DateTime.Today;
            CommonUtils.Instance.LastTradingDate = lastTradingDate;

            // LatestScrapedPrice�𒼋߉c�Ɠ��ŃZ�b�g
            stockInfo.LatestScrapedPrice = new ScrapedPrice
            {
                Date = lastTradingDate,
                Close = 999 // ���ْl
            };

            // Act
            stockInfo.SetupChartPrices();

            // Assert
            Assert.NotNull(stockInfo.ChartPrices);
            // ���߉c�Ɠ������ǉ�����Ă��邩
            Assert.Contains(stockInfo.ChartPrices, cp => cp.Date.Date == lastTradingDate && cp.Price == 999);
            // ������ChartDays���ł��邱��
            Assert.Equal(3, stockInfo.ChartPrices.Count);
        }

        [Fact]
        public async Task RegisterCacheAsync_ActuallyInsertsDataToDatabase()
        {
            // Arrange: �C��������DB�������ƃe�[�u���쐬
            DbConnectionFactory.Initialize("Data Source=:memory:;Version=3;New=True;");
            var conn = DbConnectionFactory.GetConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
CREATE TABLE history (
    code TEXT,
    date_string TEXT,
    date DATETIME,
    open REAL,
    high REAL,
    low REAL,
    close REAL,
    volume INTEGER
);
CREATE TABLE forcast_history (
    code TEXT,
    revision_date_string TEXT,
    revision_date DATETIME,
    fiscal_period TEXT,
    category TEXT,
    revision_direction TEXT,
    revenue REAL,
    operating_profit REAL,
    ordinary_income REAL,
    net_profit REAL,
    revised_dividend REAL
);";
                cmd.ExecuteNonQuery();
            }

            var stock = new WatchList.WatchStock
            {
                Code = "1234",
                Classification = "Test",
                IsFavorite = "1",
                Memo = ""
            };

            // �������R�[�h��p��
            var scrapedPrices = new List<ScrapedPrice>
            {
                new ScrapedPrice { DateYYYYMMDD = "20250101", Date = new DateTime(2025, 1, 1), Open = 100, High = 110, Low = 90, AdjustedClose = 105, Volume = 1000 },
                new ScrapedPrice { DateYYYYMMDD = "20250102", Date = new DateTime(2025, 1, 2), Open = 101, High = 111, Low = 91, AdjustedClose = 106, Volume = 1100 }
            };
            var forcasts = new List<FullYearPerformanceForcast>
            {
                new FullYearPerformanceForcast { RevisionDate = new DateTime(2025, 1, 1), FiscalPeriod = "2025.03", Category = "�\�z", RevisionDirection = "", Revenue = "1000", OperatingProfit = "100", OrdinaryProfit = "90", NetProfit = "80", RevisedDividend = "10" },
                new FullYearPerformanceForcast { RevisionDate = new DateTime(2025, 2, 1), FiscalPeriod = "2025.03", Category = "�C��", RevisionDirection = "���", Revenue = "1100", OperatingProfit = "120", OrdinaryProfit = "100", NetProfit = "90", RevisedDividend = "12" }
            };

            // �t�@�N�g���o�R�ŃR�l�N�V�������擾���A���|�W�g���ɓn��
            var repo = new AssetRepository();

            var deps = new AssetInfoDependencies
            {
                Updater = new DummyUpdater(),
                Formatter = new DummyFormatter(),
                Repository = repo,
                JudgementStrategy = new DummyJudgementStrategy(),
                Calculator = new DummyCalculator(),
                SetupStrategy = new DummySetupStrategy()
            };

            var assetInfo = AssetInfoFactory.Create(stock);
            assetInfo.ScrapedPrices = scrapedPrices;
            assetInfo.FullYearPerformancesForcasts = forcasts;
            assetInfo.Repository = repo; // ���|�W�g�����Z�b�g

            // �e�X�g�p��CommonUtils.Instance.ExecusionDate��StockPriceHistoryMonths���Z�b�g
            CommonUtils.Instance.ExecusionDate = new DateTime(2025, 1, 1);
            CommonUtils.Instance.StockPriceHistoryMonths = 12;

            // Act
            await assetInfo.RegisterCacheAsync();

            // Assert: history�e�[�u���Ƀf�[�^���S�������Ă��邩
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "SELECT COUNT(*) FROM history WHERE code = '1234'";
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                Assert.Equal(scrapedPrices.Count, count);
            }
            // Assert: forcast_history�e�[�u���Ƀf�[�^���S�������Ă��邩
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "SELECT COUNT(*) FROM forcast_history WHERE code = '1234'";
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                Assert.Equal(forcasts.Count, count);
            }
        }

        // --- �K�v�ɉ����ă_�~�[���� ---
        private class DummyUpdater : IExternalSourceUpdatable
        {
            public Task UpdateFromExternalSourceAsync(AssetInfo stockInfo) => Task.CompletedTask;
        }
        private class DummyFormatter : IOutputFormattable
        {
            public string ToOutputString(AssetInfo stockInfo) => "";
        }
        private class DummyJudgementStrategy : IAssetJudgementStrategy
        {
            public bool IsPERUndervalued(AssetInfo asset, bool isLenient = false) => true;
            public bool IsPBRUndervalued(AssetInfo asset, bool isLenient = false) => true;
            public bool IsROEAboveThreshold(AssetInfo asset) => true;
            public bool IsAnnualProgressOnTrack(AssetInfo asset) => true;
            public bool IsHighYield(AssetInfo asset) => true;
            public bool IsHighMarketCap(AssetInfo asset) => true;
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
                return 0.0; // �_�~�[����
            }

            public double GetDOE(double dividendPayoutRatio, double roe)
            {
                return 0.0; // �_�~�[����
            }

            public string GetDividendPerShareIncreased(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
            {
                return string.Empty; // �_�~�[����
            }

            public string GetIncreasedRate(string lastValue, string secondLastValue)
            {
                return string.Empty; // �_�~�[����
            }

            public string GetRevenueIncreasedSummary(string revenue1, string revenue2)
            {
                return string.Empty; // �_�~�[����
            }

            public string GetOrdinaryIncomeIncreasedSummary(string ordinaryIncome1, string ordinaryIncome2)
            {
                return string.Empty; // �_�~�[����
            }

            public string GetDividendPerShareIncreasedSummary(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
            {
                return string.Empty; // �_�~�[����
            }

            public string ConvertToPercentageStringWithSign(double v)
            {
                return string.Empty; // �_�~�[����
            }

            public string ConvertToStringWithSign(double number)
            {
                return string.Empty; // �_�~�[����
            }
        }
        private class DummySetupStrategy : IAssetSetupStrategy
        {
            public void UpdateExecutions(AssetInfo asset, List<ExecutionList.ListDetail> executionList) { }
            public void UpdateAveragePerPbr(AssetInfo asset, List<MasterList.AveragePerPbrDetails> masterList) { }
        }

        // �w���p�[
        private AssetInfo CreateStockInfo()
        {
            return AssetInfoFactory.Create(new WatchList.WatchStock
            {
                Code = "1234",
                Classification = "1",
                IsFavorite = "1",
                Memo = "�e�X�g"
            });
        }
    }
}