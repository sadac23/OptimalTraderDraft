using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;

namespace ConsoleApp1.Tests.Assets;

[Collection("CommonUtils collection")]
public class IndexInfoTests
{
    private IndexInfo _indexInfo;

    public IndexInfoTests()
    {
        // AssetInfoFactory�o�R��IndexInfo�𐶐�
        var watchStock = new WatchList.WatchStock
        {
            Code = "998407",
            Classification = CommonUtils.Instance.Classification.Indexs,
            IsFavorite = "1",
            Memo = "�e�X�g�p"
        };
        _indexInfo = (IndexInfo)AssetInfoFactory.Create(watchStock);
        // �K�v�Ȃ�GetLastHistoryUpdateDay�̃��b�N���͕ʓr�H�v���K�v
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
        Assert.Contains("1000�FTOPIX", result);
    }

    [Fact]
    public async Task UpdateFromExternalSource_UpdatesStockInfo()
    {
        // Act
        await _indexInfo.UpdateFromExternalSource();

        // Assert
        // ����
        Assert.Equal("���o���ϊ���", _indexInfo.Name);
        // �R�[�h
        Assert.Equal("998407", _indexInfo.Code);
        // �敪
        Assert.Equal("0", _indexInfo.Classification);

        // �����iScrapedPrices�j��1���ȏ�擾����Ă��邱��
        Assert.NotNull(_indexInfo.ScrapedPrices);
        Assert.NotEmpty(_indexInfo.ScrapedPrices);

        // 1���ڂ̗����̎�v�v���p�e�B���Ó��Ȓl�ł��邱��
        var price = _indexInfo.ScrapedPrices[0];
        Assert.True(price.Date > DateTime.MinValue);
        Assert.True(price.Open >= 0);
        Assert.True(price.High >= 0);
        Assert.True(price.Low >= 0);
        Assert.True(price.Close >= 0);
        Assert.Equal(price.Close, price.AdjustedClose); // �w���̏ꍇ
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

    private class MockStockInfo : AssetInfo
    {
        public MockStockInfo()
            : base((WatchList.WatchStock)null, null) // base�Ăяo���̓_�~�[
        {
            var watchStock = new WatchList.WatchStock
            {
                Code = "1234",
                Name = "�e�X�g�w��",
                Classification = CommonUtils.Instance.Classification.Indexs,
                IsFavorite = "1",
                Memo = "����̓e�X�g�p�����ł�"
            };
            var info = (IndexInfo)AssetInfoFactory.Create(watchStock);
            // �K�v�ȃv���p�e�B��this�ɃR�s�[
            this.Code = info.Code;
            this.Name = info.Name;
            // ...���v���p�e�B���K�v�ɉ�����
            this.ChartPrices = new List<ChartPrice>
            {
                new MockChartPrice()
            };
        }

        public override string Code { get; set; } = "1234";
        public override string Name { get; set; } = "�e�X�g�w��";
        public override bool IsOwnedNow() => false;
    }

    [Fact]
    public void ToOutputString_FormatsIndexInfoCorrectly()
    {
        // Arrange
        var formatter = new IndexFormatter();

        // IndexInfo��Factory�o�R�Ő���
        var watchStock = new WatchList.WatchStock
        {
            Code = "1234",
            Name = "�e�X�g�w��",
            Classification = CommonUtils.Instance.Classification.Indexs,
            IsFavorite = "1",
            Memo = "����̓e�X�g�p�����ł�"
        };
        var stockInfo = (IndexInfo)AssetInfoFactory.Create(watchStock);

        // �K�v�ȃv���p�e�B���Z�b�g
        stockInfo.Name = watchStock.Name;
        stockInfo.ChartPrices = new List<ChartPrice>
        {
            new MockChartPrice() // ����͊����̃e�X�g�pChartPrice�h���N���X
        };

        // Act
        var result = formatter.ToOutputString(stockInfo);

        // Assert
        Assert.Contains("1234�F�e�X�g�w��", result);
        Assert.Contains("�����F1,234.5�i24/08/01�FS25.12,L60.34�j��", result);
        Assert.Contains("�`���[�g�iRSI�j�F", result);
        Assert.Contains("�����F\r\n����̓e�X�g�p�����ł�", result);
    }
}
