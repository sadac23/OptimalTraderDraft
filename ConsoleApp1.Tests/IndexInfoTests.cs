using System.Threading.Tasks;
using Xunit;
using ConsoleApp1;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

namespace ConsoleApp1.Tests;

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
            stockInfo.Code = "9999";
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

    [Fact]
    public async Task UpdateFromExternalSource_UpdatesStockInfo()
    {
        // Arrange
        var updater = new MockUpdater();
        var formatter = new MockFormatter();
        var indexInfo = new IndexInfo(updater, formatter);

        // Act
        await indexInfo.UpdateFromExternalSource();

        // Assert
        Assert.True(updater.WasCalled);
        Assert.Equal("テストインデックス", indexInfo.Name);
        Assert.Equal("9999", indexInfo.Code);
    }

    [Fact]
    public void ToOutputString_ReturnsFormattedString()
    {
        // Arrange
        var updater = new MockUpdater();
        var formatter = new MockFormatter();
        var indexInfo = new IndexInfo(updater, formatter)
        {
            Name = "TOPIX",
            Code = "1000"
        };

        // Act
        var result = indexInfo.ToOutputString();

        // Assert
        Assert.Equal("Code:1000, Name:TOPIX", result);
    }
}