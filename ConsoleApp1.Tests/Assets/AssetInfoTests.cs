using System.Data.SQLite;
using ConsoleApp1.Tests.Utils;
using ConsoleApp1.Database;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;

namespace ConsoleApp1.Tests.Assets;

[Collection("CommonUtils collection")]
public class AssetInfoTests
{
    // PEG���V�I���������v�Z����邩�̃e�X�g
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
        // EPS������ = ((120-100)/100)*100 = 20%, PEG = 15/20 = 0.75
        Assert.Equal(0.75, asset.PEGRatio, 2);
    }

    // �f�[�^������Ȃ��ꍇ��PEG��0�ɂȂ�
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

    // EPS��������0�̏ꍇ��PEG��0�ɂȂ�
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
            Name = "�e�X�g����",
            Classification = "1",
            IsFavorite = "1",
            Memo = "�������e"
        })
        {
            Code = "1234",
            Name = "�e�X�g����",
            Section = "���؂o",
            Industry = "���E�ʐM",
            DividendYield = 0.025,
            DividendPayoutRatio = 0.3,
            Doe = 0.05,
            DividendRecordDateMonth = "6��",
            ShareholderBenefitYield = 0.01,
            ShareholderBenefitsDetails = "QUO�J�[�h",
            NumberOfSharesRequiredForBenefits = "100",
            ShareholderBenefitRecordMonth = "6��",
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
            PressReleaseDate = "2024�N6��1��",
            Executions = new List<ExecutionList.Execution>
        {
            new ExecutionList.Execution
            {
                BuyOrSell = "��",
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
        Assert.Contains("1234�F�e�X�g����", output);
        Assert.Contains("�����F1,000.0", output);
        Assert.Contains("�s��/�Ǝ�F���؂o/���E�ʐM", output);
        Assert.Contains("�z�������F2.50%", output);
        Assert.Contains("�D�җ����F1.00%", output);
        Assert.Contains("ROE�F8.5", output);
        Assert.Contains("PER�F15.2", output);
        Assert.Contains("PEG���V�I�F1.20", output);
        Assert.Contains("PBR�F1.1", output);
        Assert.Contains("�c�Ɨ��v���F12.00%", output);
        Assert.Contains("�M�p�{���F2.5", output);
        Assert.Contains("�o�����F20000", output);
        Assert.Contains("�����F", output);
        Assert.Contains("�������e", output);
    }
}

// �C��: TestAssetInfo �N���X�� LatestPrice �v���p�e�B�̂����܂������������邽�߁A�v���p�e�B����ύX  
public class TestAssetInfo : AssetInfo
{
    public TestAssetInfo(WatchList.WatchStock stock) : base(stock) { }
}

