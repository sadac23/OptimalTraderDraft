using System.Data.SQLite;
using ConsoleApp1.Tests.Utils;
using ConsoleApp1.Database;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;

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

        // Act
        asset.SetupPEGRatio();

        // Assert
        // EPS成長率 = ((120-100)/100)*100 = 20%, PEG = 15/20 = 0.75
        Assert.Equal(0.75, asset.PEGRatio, 2);
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
    public TestAssetInfo(WatchList.WatchStock stock) : base(stock) { }
}

