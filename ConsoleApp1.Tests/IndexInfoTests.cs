using Moq;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using static StockInfo;

namespace ConsoleApp1.Tests;

[Collection("CommonUtils collection")]
public class IndexInfoTests
{
    // モック用クラス
    private class MockUpdater : IExternalSourceUpdatable
    {
        public bool WasCalled { get; private set; } = false;
        public Task UpdateFromExternalSourceAsync(StockInfo stockInfo)
        {
            WasCalled = true;
            // テスト用にプロパティを変更
            stockInfo.Name = "テストインデックス";
            stockInfo.Code = "998407";
            stockInfo.Classification = "0"; // 指数
            return Task.CompletedTask;
        }
    }

    private class MockFormatter : IOutputFormattable
    {
        public string ToOutputString(StockInfo stockInfo)
        {
            return $"Code:{stockInfo.Code}, Name:{stockInfo.Name}";
        }
    }

    private IndexInfo _indexInfo;
    private MockUpdater _updater;
    private MockFormatter _formatter;
    private WatchList.WatchStock _watchStock;

    public IndexInfoTests()
    {
        // Arrange
        _watchStock = new WatchList.WatchStock
        {
            Code = "9999",
            Classification = "0",
            IsFavorite = "1",
            Memo = "テスト用"
        };
        _updater = new MockUpdater();
        _formatter = new MockFormatter();

        // MoqでIndexInfoのGetLastHistoryUpdateDayをモック
        var mock = new Mock<IndexInfo>(
            new WatchList.WatchStock
            {
                Code = "9999",
                Classification = "0",
                IsFavorite = "1",
                Memo = "テスト用"
            },
            new IndexInfo.IndexUpdater(),
            new IndexInfo.IndexFormatter()
        ) { CallBase = true };
        mock.Setup(x => x.GetLastHistoryUpdateDay())
            .Returns(new DateTime(2024, 1, 1));

        _indexInfo = mock.Object;
    }

    [Fact]
    public void ToOutputString_ReturnsFormattedString()
    {
        _indexInfo.Name = "TOPIX";
        _indexInfo.Code = "1000";
        _indexInfo.Section = "";
        _indexInfo.Industry = "";
        //indexInfo.LatestPrice = new ChartPrice { Price = 0, Date = DateTime.Now, RSIS = 0, RSIL = 0 };
        // 他にもToOutputStringで使われるプロパティを適宜初期化

        var result = _indexInfo.ToOutputString();
        Assert.Equal("Code:1000, Name:TOPIX", result);
    }

    [Fact]
    public async Task UpdateFromExternalSource_UpdatesStockInfo()
    {
        // Act
        await _indexInfo.UpdateFromExternalSource();

        // Assert
        Assert.True(_updater.WasCalled);
        Assert.Equal("テストインデックス", _indexInfo.Name);
        Assert.Equal("998407", _indexInfo.Code);
    }
}

[Collection("CommonUtils collection")]
public class IndexFormatterTests
{
    private class MockChartPrice : ChartPrice
    {
        public override double Price => 1234.5;
        public override DateTime Date => new DateTime(2024, 8, 1);
        public override double RSIS => 25.12;
        public override double RSIL => 60.34;
        public override bool OversoldIndicator() => true;
        public override bool OverboughtIndicator() => false;
    }

    private class MockStockInfo : StockInfo
    {
        public MockStockInfo() : base(new WatchList.WatchStock
        {
            Code = "1234",
            Name = "テスト指数",
            Classification = "0",
            IsFavorite = "1",
            Memo = "これはテスト用メモです"
        })
        {
            // ChartPricesにMockChartPriceインスタンスを追加
            this.ChartPrices = new List<ChartPrice>
            {
                new MockChartPrice()
            };
        }
        public override string Code { get; set; } = "1234";
        public override string Name { get; set; } = "テスト指数";
        //public override ChartPrice LatestPrice => new MockChartPrice();
        public override bool IsOwnedNow() => false;
        // Memoプロパティは必要に応じてオーバーライド可
    }

    [Fact]
    public void ToOutputString_FormatsIndexInfoCorrectly()
    {
        // Arrange
        var formatter = new IndexInfo.IndexFormatter();

        // StockInfoのモック
        var stockInfo = new MockStockInfo();

        // Act
        var result = formatter.ToOutputString(stockInfo);

        // Assert
        Assert.Contains("1234：テスト指数", result);
        Assert.Contains("株価：1,234.5（24/08/01：S25.12,L60.34）★", result); // ★はOversoldIndicator()がtrueのため
        Assert.Contains("チャート（RSI）：", result); // チャート行の検証
        Assert.Contains("メモ：\r\nこれはテスト用メモです", result); // メモ行の検証
    }
}

[Collection("CommonUtils collection")]
public class IndexUpdaterTests
{
    [Fact]
    public async Task UpdateFromExternalSourceAsync_UpdatesStockInfoFromHtml()
    {
        // Arrange
        var mock = new Mock<IndexInfo>(
            new WatchList.WatchStock
            {
                Code = "998407",
                Name = "ダミーインデックス",
                Classification = "0",
                IsFavorite = "1",
                Memo = "テスト用"
            },
            new IndexInfo.IndexUpdater(),
            new IndexInfo.IndexFormatter()
        ) { CallBase = true };
        mock.Setup(x => x.GetLastHistoryUpdateDay())
            .Returns(new DateTime(2024, 1, 1));

        var updater = new IndexInfo.IndexUpdater();
        IndexInfo stockInfo = mock.Object;

        // Act
        await updater.UpdateFromExternalSourceAsync(stockInfo);

        // Assert
        // 名称
        Assert.Equal("日経平均株価", stockInfo.Name);
        // コード
        Assert.Equal("998407", stockInfo.Code);
        // 区分
        Assert.Equal("0", stockInfo.Classification);

        // 履歴（ScrapedPrices）が1件以上取得されていること
        Assert.NotNull(stockInfo.ScrapedPrices);
        Assert.NotEmpty(stockInfo.ScrapedPrices);

        // 1件目の履歴の主要プロパティが妥当な値であること
        var price = stockInfo.ScrapedPrices[0];
        Assert.True(price.Date > DateTime.MinValue);
        Assert.True(price.Open >= 0);
        Assert.True(price.High >= 0);
        Assert.True(price.Low >= 0);
        Assert.True(price.Close >= 0);
        Assert.Equal(price.Close, price.AdjustedClose); // 指数の場合
    }
}