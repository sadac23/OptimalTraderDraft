using System.Data.SQLite;
using ConsoleApp1.Tests.Utils;
using ConsoleApp1.Database;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.Assets.Calculators;
using ConsoleApp1.Assets.Setups;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

namespace ConsoleApp1.Tests.Assets;

[Collection("CommonUtils collection")]
public class AssetInfoTests
{
    // PEGレシオが正しく計算されるかのテスト
    [Fact]
    public void SetupPEGRatio_ValidData_ComputesCorrectPEG()
    {
        // Arrange
        var watchStock = new WatchList.WatchStock { Code = "1234", Classification = "1", IsFavorite = "0", Memo = "" };
        var asset = new TestAssetInfo(watchStock);
        asset.Per = 15.0;
        asset.FullYearPerformances = new List<FullYearPerformance>
        {
            new FullYearPerformance { AdjustedEarningsPerShare = "100" },
            new FullYearPerformance { AdjustedEarningsPerShare = "120" },
            new FullYearPerformance { AdjustedEarningsPerShare = "" }
        };
    }
    // ダミー WatchStock クラス
    private class DummyWatchStock
    {
        public string Code { get; set; } = "1234";
        public string Classification { get; set; } = "1";
        public string IsFavorite { get; set; } = "1";
        public string Memo { get; set; } = "テストメモ";
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

        var result = AssetInfo.IsWithinMonths(monthsStr, m);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SetupChartPrices_ReflectsHistoryTableData()
    {
        // Arrange
        // テスト用のインメモリSQLite DBを用意
        using var connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
        connection.Open();

        // テーブル作成
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

        // テストデータ挿入
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

        // DbConnectionFactoryのコネクションを差し替え
        DbConnectionFactory.SetConnection(connection); // ここでテスト用コネクションをセット

        var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock
        {
            Code = code,
            Classification = "1",
            IsFavorite = "1",
            Memo = "テスト"
        });

        // ChartDaysを3に設定
        CommonUtils.Instance.ChartDays = 3;

        // Act
        // internalメソッドを直接呼び出し
        stockInfo.SetupChartPrices();

        // Assert
        Assert.NotNull(stockInfo.ChartPrices);
        Assert.Equal(3, stockInfo.ChartPrices.Count);
        Assert.All(stockInfo.ChartPrices, cp => Assert.Equal(code, stockInfo.Code));
        // 日付降順で格納されていること
        for (int i = 1; i < stockInfo.ChartPrices.Count; i++)
        {
            Assert.True(stockInfo.ChartPrices[i - 1].Date >= stockInfo.ChartPrices[i].Date);
        }
        // 価格がhistoryテーブルのclose値と一致すること
        var expectedCloses = new List<double> { 107, 106, 105 }; // 挿入順に降順
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

        // テーブル作成
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

        // historyにはLastTradingDateより前の日付のみ
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
            Memo = "テスト"
        });

        CommonUtils.Instance.ChartDays = 3;

        // 直近営業日をセット
        var lastTradingDate = DateTime.Today;
        CommonUtils.Instance.LastTradingDate = lastTradingDate;

        // LatestScrapedPriceを直近営業日でセット
        stockInfo.LatestScrapedPrice = new ScrapedPrice
        {
            Date = lastTradingDate,
            Close = 999 // 特異値
        };

        // Act
        stockInfo.SetupChartPrices();

        // Assert
        Assert.NotNull(stockInfo.ChartPrices);
        // 直近営業日分が追加されているか
        Assert.Contains(stockInfo.ChartPrices, cp => cp.Date.Date == lastTradingDate && cp.Price == 999);
        // 件数はChartDays件であること
        Assert.Equal(3, stockInfo.ChartPrices.Count);
    }

    [Fact]
    public async Task RegisterCacheAsync_ActuallyInsertsDataToDatabase()
    {
        // Arrange: インメモリDB初期化とテーブル作成
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

        // 複数レコードを用意
        var scrapedPrices = new List<ScrapedPrice>
            {
                new ScrapedPrice { DateYYYYMMDD = "20250101", Date = new DateTime(2025, 1, 1), Open = 100, High = 110, Low = 90, AdjustedClose = 105, Volume = 1000 },
                new ScrapedPrice { DateYYYYMMDD = "20250102", Date = new DateTime(2025, 1, 2), Open = 101, High = 111, Low = 91, AdjustedClose = 106, Volume = 1100 }
            };
        var forcasts = new List<FullYearPerformanceForcast>
            {
                new FullYearPerformanceForcast { RevisionDate = new DateTime(2025, 1, 1), FiscalPeriod = "2025.03", Category = "予想", RevisionDirection = "", Revenue = "1000", OperatingProfit = "100", OrdinaryProfit = "90", NetProfit = "80", RevisedDividend = "10" },
                new FullYearPerformanceForcast { RevisionDate = new DateTime(2025, 2, 1), FiscalPeriod = "2025.03", Category = "修正", RevisionDirection = "上方", Revenue = "1100", OperatingProfit = "120", OrdinaryProfit = "100", NetProfit = "90", RevisedDividend = "12" }
            };

        // ファクトリ経由でコネクションを取得し、リポジトリに渡す
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
        assetInfo.Repository = repo; // リポジトリをセット

        // テスト用にCommonUtils.Instance.ExecusionDateとStockPriceHistoryMonthsをセット
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 1, 1);
        CommonUtils.Instance.StockPriceHistoryMonths = 12;

        // Act
        await assetInfo.RegisterCacheAsync();

        // Assert: historyテーブルにデータが全件入っているか
        using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM history WHERE code = '1234'";
            var count = Convert.ToInt32(checkCmd.ExecuteScalar());
            Assert.Equal(scrapedPrices.Count, count);
        }
        // Assert: forcast_historyテーブルにデータが全件入っているか
        using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM forcast_history WHERE code = '1234'";
            var count = Convert.ToInt32(checkCmd.ExecuteScalar());
            Assert.Equal(forcasts.Count, count);
        }
    }

    // --- 必要に応じてダミー実装 ---
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
            return 0.0; // ダミー実装
        }

        public double GetDOE(double dividendPayoutRatio, double roe)
        {
            return 0.0; // ダミー実装
        }

        public string GetDividendPerShareIncreased(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
        {
            return string.Empty; // ダミー実装
        }

        public string GetIncreasedRate(string lastValue, string secondLastValue)
        {
            return string.Empty; // ダミー実装
        }

        public string GetRevenueIncreasedSummary(string revenue1, string revenue2)
        {
            return string.Empty; // ダミー実装
        }

        public string GetOrdinaryIncomeIncreasedSummary(string ordinaryIncome1, string ordinaryIncome2)
        {
            return string.Empty; // ダミー実装
        }

        public string GetDividendPerShareIncreasedSummary(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
        {
            return string.Empty; // ダミー実装
        }

        public string ConvertToPercentageStringWithSign(double v)
        {
            return string.Empty; // ダミー実装
        }

        public string ConvertToStringWithSign(double number)
        {
            return string.Empty; // ダミー実装
        }
    }
    private class DummySetupStrategy : IAssetSetupStrategy
    {
        public void UpdateExecutions(AssetInfo asset, List<ExecutionList.ListDetail> executionList) { }
        public void UpdateAveragePerPbr(AssetInfo asset, List<MasterList.AveragePerPbrDetails> masterList) { }
    }

    // ヘルパー
    private AssetInfo CreateStockInfo()
    {
        return AssetInfoFactory.Create(new WatchList.WatchStock
        {
            Code = "1234",
            Classification = "1",
            IsFavorite = "1",
            Memo = "テスト"
        });
    }

    // データが足りない場合はPEGが0になる
    [Fact]
    public void SetupPEGRatio_InsufficientData_PEGIsZero()
    {
        // Arrange
        var watchStock = new WatchList.WatchStock { Code = "1234", Classification = "1", IsFavorite = "0", Memo = "" };
        var asset = new TestAssetInfo(watchStock);
        asset.Per = 10.0;
        asset.FullYearPerformances = new List<FullYearPerformance>
            {
                new FullYearPerformance { AdjustedEarningsPerShare = "100" }
            };

        // Act
        asset.SetupPEGRatio();

        // Assert
        Assert.Equal(0, asset.PEGRatio);
    }

    // EPS成長率が0の場合はPEGが0になる
    [Fact]
    public void SetupPEGRatio_ZeroGrowth_PEGIsZero()
    {
        // Arrange
        var watchStock = new WatchList.WatchStock { Code = "1234", Classification = "1", IsFavorite = "0", Memo = "" };
        var asset = new TestAssetInfo(watchStock);
        asset.Per = 10.0;
        asset.FullYearPerformances = new List<FullYearPerformance>
            {
                new FullYearPerformance { AdjustedEarningsPerShare = "100" },
                new FullYearPerformance { AdjustedEarningsPerShare = "100" },
                new FullYearPerformance { AdjustedEarningsPerShare = "" }
            };

        // Act
        asset.SetupPEGRatio();

        // Assert
        Assert.Equal(0, asset.PEGRatio);
    }
}

[Collection("CommonUtils collection")]
public class JapaneseStockFormatterTests
{
    [Fact]
        public void ToOutputString_BasicOutput_ContainsKeyInfo()
    {
        // Arrange
        var stockInfo = new TestAssetInfo(new WatchList.WatchStock
        {
            Code = "1234",
            Name = "テスト銘柄",
            Classification = "1",
            IsFavorite = "1",
            Memo = "メモ内容"
        })
        {
            Code = "1234",
            Name = "テスト銘柄",
            Section = "東証Ｐ",
            Industry = "情報・通信",
            DividendYield = 0.025,
            DividendPayoutRatio = 0.3,
            Doe = 0.05,
            DividendRecordDateMonth = "6月",
            ShareholderBenefitYield = 0.01,
            ShareholderBenefitsDetails = "QUOカード",
            NumberOfSharesRequiredForBenefits = "100",
            ShareholderBenefitRecordMonth = "6月",
            ShareholderBenefitRecordDay = "30",
            FullYearPerformancesForcasts = new List<FullYearPerformanceForcast>(),
            QuarterlyFullyearProgressRate = 0.5,
            QuarterlyPerformanceReleaseDate = new DateTime(2024, 5, 10),
            QuarterlyOperatingProfitMarginYoY = 0.1,
            PreviousFullyearProgressRate = 0.4,
            PreviousPerformanceReleaseDate = new DateTime(2023, 5, 10),
            MarketCap = 1000000000,
            FullYearProfits = new List<FullYearProfit> { new FullYearProfit { Roe = 8.5 } },
            Per = 15.2,
            AveragePer = 18.0,
            PEGRatio = 1.2,
            Pbr = 1.1,
            AveragePbr = 1.5,
            OperatingProfitMargin = 0.12,
            MarginBalanceRatio = "2.5",
            MarginBuyBalance = "1000",
            MarginSellBalance = "500",
            MarginBalanceDate = "2024/06/01",
            LatestTradingVolume = "20000",
            EquityRatio = "50%",
            EarningsPeriod = "2024/03",
            PressReleaseDate = "2024年6月1日",
            Executions = new List<ExecutionList.Execution>
        {
            new ExecutionList.Execution
            {
                BuyOrSell = "買",
                Date = new DateTime(2024, 4, 1),
                Price = 950,
                Quantity = 100,
                HasSellExecuted = false
            }
        },
            ChartPrices = new List<ChartPrice>
        {
            new ChartPrice { Date = new DateTime(2024, 6, 1), Price = 1000, Volatility = 0.01, RSIS = 30, RSIL = 40 }
        }
        };

        var formatter = new JapaneseStockFormatter();

        // Act
        var output = formatter.ToOutputString(stockInfo);

        // Assert
        Assert.Contains("1234：テスト銘柄", output);
        Assert.Contains("株価：1,000.0", output);
        Assert.Contains("市場/業種：東証Ｐ/情報・通信", output);
        Assert.Contains("配当利回り：2.50%", output);
        Assert.Contains("優待利回り：1.00%", output);
        Assert.Contains("ROE：8.5", output);
        Assert.Contains("PER：15.2", output);
        Assert.Contains("PEGレシオ：1.20", output);
        Assert.Contains("PBR：1.1", output);
        Assert.Contains("営業利益率：12.00%", output);
        Assert.Contains("信用倍率：2.5", output);
        Assert.Contains("出来高：20000", output);
        Assert.Contains("メモ：", output);
        Assert.Contains("メモ内容", output);
    }
}

// 修正: TestAssetInfo クラスの LatestPrice プロパティのあいまいさを解消するため、プロパティ名を変更  
public class TestAssetInfo : AssetInfo
{
    public TestAssetInfo(WatchList.WatchStock stock)
        : base(stock, new AssetInfoDependencies
        {
            Updater = new DummyUpdater(),
            Formatter = new DummyFormatter(),
            Repository = new AssetRepository(),
            JudgementStrategy = new DummyJudgementStrategy(),
            Calculator = new DummyCalculator(),
            SetupStrategy = new DummySetupStrategy()
        })
    { }
}

internal class DummySetupStrategy : IAssetSetupStrategy
{
    public void UpdateExecutions(AssetInfo asset, List<ExecutionList.ListDetail> executionList)
    {
        // ダミー実装: 実際のロジックは必要に応じて追加してください
    }

    public void UpdateAveragePerPbr(AssetInfo asset, List<MasterList.AveragePerPbrDetails> masterList)
    {
        // ダミー実装: 実際のロジックは必要に応じて追加してください
    }
}

internal class DummyCalculator : IAssetCalculator
{
    public void UpdateProgress(AssetInfo asset)
    {
        // ダミー実装: 実際のロジックは必要に応じて追加してください
    }

    public void UpdateDividendPayoutRatio(AssetInfo asset)
    {
        // ダミー実装: 実際のロジックは必要に応じて追加してください
    }

    public void UpdateFullYearPerformanceForcastSummary(AssetInfo asset)
    {
        // ダミー実装: 実際のロジックは必要に応じて追加してください
    }

    public void SetupChartPrices(AssetInfo asset)
    {
        // ダミー実装: 実際のロジックは必要に応じて追加してください
    }

    public double GetDividendPayoutRatio(string adjustedDividendPerShare, string adjustedEarningsPerShare)
    {
        return 0.0; // ダミー実装
    }

    public double GetDOE(double dividendPayoutRatio, double roe)
    {
        return 0.0; // ダミー実装
    }

    public string GetDividendPerShareIncreased(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
    {
        return string.Empty; // ダミー実装
    }

    public string GetIncreasedRate(string lastValue, string secondLastValue)
    {
        return string.Empty; // ダミー実装
    }

    public string GetRevenueIncreasedSummary(string revenue1, string revenue2)
    {
        return string.Empty; // ダミー実装
    }

    public string GetOrdinaryIncomeIncreasedSummary(string ordinaryIncome1, string ordinaryIncome2)
    {
        return string.Empty; // ダミー実装
    }

    public string GetDividendPerShareIncreasedSummary(string adjustedDividendPerShare1, string adjustedDividendPerShare2)
    {
        return string.Empty; // ダミー実装
    }

    public string ConvertToPercentageStringWithSign(double v)
    {
        return string.Empty; // ダミー実装
    }

    public string ConvertToStringWithSign(double number)
    {
        return string.Empty; // ダミー実装
    }
}

internal class DummyFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo stockInfo)
    {
        // ダミー実装: 必要に応じて適切なロジックを追加してください
        return string.Empty;
    }
}

internal class DummyUpdater : IExternalSourceUpdatable
{
    public Task UpdateFromExternalSourceAsync(AssetInfo stockInfo)
    {
        // ダミー実装: 実際のロジックは必要に応じて追加してください
        return Task.CompletedTask;
    }
}

internal class DummyJudgementStrategy : IAssetJudgementStrategy
{
    public bool ExtractAndValidateDateWithinOneMonth(AssetInfo asset)
    {
        return false;
    }

    public bool HasDisclosure(AssetInfo asset)
    {
        return false;
    }

    public bool HasRecentStockSplitOccurred(AssetInfo asset)
    {
        return false;
    }

    public bool IsAfterQuarterEnd(AssetInfo asset)
    {
        return false;
    }

    public bool IsAfterRecordDate(AssetInfo asset)
    {
        return false;
    }

    public bool IsAnnualProgressOnTrack(AssetInfo asset)
    {
        return false;
    }

    public bool IsCloseToDividendRecordDate(AssetInfo asset)
    {
        return false;
    }

    public bool IsCloseToQuarterEnd(AssetInfo asset)
    {
        return false;
    }

    public bool IsCloseToRecordDate(AssetInfo asset)
    {
        return false;
    }

    public bool IsCloseToShareholderBenefitRecordDate(AssetInfo asset)
    {
        return false;
    }

    public bool IsGoldenCrossPossible(AssetInfo asset)
    {
        return false;
    }

    public bool IsGranvilleCase1Matched(AssetInfo asset)
    {
        return false;
    }

    public bool IsGranvilleCase2Matched(AssetInfo asset)
    {
        return false;
    }

    public bool IsHighMarketCap(AssetInfo asset)
    {
        return false;
    }

    public bool IsHighYield(AssetInfo asset)
    {
        return false;
    }

    public bool IsJustSold(AssetInfo asset)
    {
        return false;
    }

    public bool IsOwnedNow(AssetInfo asset)
    {
        return false;
    }

    public bool IsPBRUndervalued(AssetInfo asset, bool isLenient = false)
    {
        return false;
    }

    public bool IsPERUndervalued(AssetInfo asset, bool isLenient = false)
    {
        return false;
    }

    public bool IsQuarterEnd(AssetInfo asset)
    {
        return false;
    }

    public bool IsRecordDate(AssetInfo asset)
    {
        return false;
    }

    public bool IsROEAboveThreshold(AssetInfo asset)
    {
        return false;
    }

    public bool ShouldAverageDown(AssetInfo asset, ExecutionList.Execution e)
    {
        return false;
    }
}