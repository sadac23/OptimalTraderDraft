using Moq;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;
using static StockInfo;

namespace ConsoleApp1.Tests;

[Collection("CommonUtils collection")]
public class IndexInfoTests
{
    // ���b�N�p�N���X
    private class MockUpdater : IExternalSourceUpdatable
    {
        public bool WasCalled { get; private set; } = false;
        public Task UpdateFromExternalSourceAsync(StockInfo stockInfo)
        {
            WasCalled = true;
            // �e�X�g�p�Ƀv���p�e�B��ύX
            stockInfo.Name = "�e�X�g�C���f�b�N�X";
            stockInfo.Code = "998407";
            stockInfo.Classification = "0"; // �w��
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
            Memo = "�e�X�g�p"
        };
        _updater = new MockUpdater();
        _formatter = new MockFormatter();

        // Moq��IndexInfo��GetLastHistoryUpdateDay�����b�N
        var mock = new Mock<IndexInfo>(
            new WatchList.WatchStock
            {
                Code = "9999",
                Classification = "0",
                IsFavorite = "1",
                Memo = "�e�X�g�p"
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
        // ���ɂ�ToOutputString�Ŏg����v���p�e�B��K�X������

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
        Assert.Equal("�e�X�g�C���f�b�N�X", _indexInfo.Name);
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
            Name = "�e�X�g�w��",
            Classification = "0",
            IsFavorite = "1",
            Memo = "����̓e�X�g�p�����ł�"
        })
        {
            // ChartPrices��MockChartPrice�C���X�^���X��ǉ�
            this.ChartPrices = new List<ChartPrice>
            {
                new MockChartPrice()
            };
        }
        public override string Code { get; set; } = "1234";
        public override string Name { get; set; } = "�e�X�g�w��";
        //public override ChartPrice LatestPrice => new MockChartPrice();
        public override bool IsOwnedNow() => false;
        // Memo�v���p�e�B�͕K�v�ɉ����ăI�[�o�[���C�h��
    }

    [Fact]
    public void ToOutputString_FormatsIndexInfoCorrectly()
    {
        // Arrange
        var formatter = new IndexInfo.IndexFormatter();

        // StockInfo�̃��b�N
        var stockInfo = new MockStockInfo();

        // Act
        var result = formatter.ToOutputString(stockInfo);

        // Assert
        Assert.Contains("1234�F�e�X�g�w��", result);
        Assert.Contains("�����F1,234.5�i24/08/01�FS25.12,L60.34�j��", result); // ����OversoldIndicator()��true�̂���
        Assert.Contains("�`���[�g�iRSI�j�F", result); // �`���[�g�s�̌���
        Assert.Contains("�����F\r\n����̓e�X�g�p�����ł�", result); // �����s�̌���
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
                Name = "�_�~�[�C���f�b�N�X",
                Classification = "0",
                IsFavorite = "1",
                Memo = "�e�X�g�p"
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
        // ����
        Assert.Equal("���o���ϊ���", stockInfo.Name);
        // �R�[�h
        Assert.Equal("998407", stockInfo.Code);
        // �敪
        Assert.Equal("0", stockInfo.Classification);

        // �����iScrapedPrices�j��1���ȏ�擾����Ă��邱��
        Assert.NotNull(stockInfo.ScrapedPrices);
        Assert.NotEmpty(stockInfo.ScrapedPrices);

        // 1���ڂ̗����̎�v�v���p�e�B���Ó��Ȓl�ł��邱��
        var price = stockInfo.ScrapedPrices[0];
        Assert.True(price.Date > DateTime.MinValue);
        Assert.True(price.Open >= 0);
        Assert.True(price.High >= 0);
        Assert.True(price.Low >= 0);
        Assert.True(price.Close >= 0);
        Assert.Equal(price.Close, price.AdjustedClose); // �w���̏ꍇ
    }
}