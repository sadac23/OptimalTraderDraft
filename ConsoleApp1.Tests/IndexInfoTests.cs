using Moq;
using Moq.Protected; // Protected()拡張メソッド用
using System.Threading.Tasks;
using Xunit;
using ConsoleApp1;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

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

    private IndexInfo indexInfo;
    private MockUpdater updater;
    private MockFormatter formatter;
    private WatchList.WatchStock watchStock;

    public IndexInfoTests()
    {
        // Arrange
        var watchStock = new WatchList.WatchStock
        {
            Code = "9999",
            Classification = "0",
            IsFavorite = "1",
            Memo = "テスト用"
        };
        updater = new MockUpdater();
        formatter = new MockFormatter();

        // MoqでIndexInfoのGetLastHistoryUpdateDayをモック
        var mock = new Mock<IndexInfo>(watchStock, updater, formatter) { CallBase = true };
        mock.Protected()
            .Setup<DateTime>("GetLastHistoryUpdateDay")
            .Returns(new DateTime(2024, 1, 1));

        indexInfo = mock.Object;
    }

    [Fact]
    public void ToOutputString_ReturnsFormattedString()
    {
        indexInfo.Name = "TOPIX";
        indexInfo.Code = "1000";
        indexInfo.Section = "";
        indexInfo.Industry = "";
        //indexInfo.LatestPrice = new ChartPrice { Price = 0, Date = DateTime.Now, RSIS = 0, RSIL = 0 };
        // 他にもToOutputStringで使われるプロパティを適宜初期化

        var result = indexInfo.ToOutputString();
        Assert.Equal("Code:1000, Name:TOPIX", result);
    }

    [Fact]
    public async Task UpdateFromExternalSource_UpdatesStockInfo()
    {
        // Act
        await indexInfo.UpdateFromExternalSource();

        // Assert
        Assert.True(updater.WasCalled);
        Assert.Equal("テストインデックス", indexInfo.Name);
        Assert.Equal("998407", indexInfo.Code);
    }
}