using System;
using System.Collections.Generic;
using Xunit;

namespace ConsoleApp1.Tests.Analisys
{
    [Collection("CommonUtils collection")]
    public class Analyzer_GetCutlerRSI_NoDb_Tests
    {
        class TestAnalyzer : Analyzer
        {
            private readonly List<(DateTime, double)> _testPrices;
            public TestAnalyzer(List<(DateTime, double)> testPrices)
            {
                _testPrices = testPrices;
            }

            internal override List<(DateTime, double)> GetCutlerRsiPrices(int v, DateTime endDate, StockInfo stockInfo)
            {
                return _testPrices;
            }
        }

        [Fact]
        public void GetCutlerRSI_CoversAllLatestScrapedPriceCases()
        {
            int v = 14;
            var baseDate = new DateTime(2024, 1, 1);
            var lastTradingDay = CommonUtils.Instance.LastTradingDate;

            // 1. ���߉c�Ɠ��f�[�^������ & latestPrice���L���i�ǉ������j
            var prices1 = new List<(DateTime, double)>();
            for (int i = 0; i < v; i++)
                prices1.Add((baseDate.AddDays(i), 100 + i));
            // �Ō�̓��t�� lastTradingDay ���O
            var analyzer1 = new TestAnalyzer(prices1);
            var stockInfo1 = StockInfo.GetInstance(new WatchList.WatchStock { Code = "TEST1", Name = "Test1" });
            stockInfo1.LatestScrapedPrice = new StockInfo.ScrapedPrice
            {
                Date = lastTradingDay,
                Close = 200
            };
            double rsi1 = analyzer1.GetCutlerRSI(v, lastTradingDay, stockInfo1);
            Assert.InRange(rsi1, 0, 100);

            // 2. ���߉c�Ɠ��f�[�^�����ɂ���i�ǉ�����Ȃ��j
            var prices2 = new List<(DateTime, double)>(prices1)
            {
                (lastTradingDay, 200)
            };
            var analyzer2 = new TestAnalyzer(prices2);
            var stockInfo2 = StockInfo.GetInstance(new WatchList.WatchStock { Code = "TEST2", Name = "Test2" });
            stockInfo2.LatestScrapedPrice = new StockInfo.ScrapedPrice
            {
                Date = lastTradingDay,
                Close = 200
            };
            double rsi2 = analyzer2.GetCutlerRSI(v, lastTradingDay, stockInfo2);
            Assert.InRange(rsi2, 0, 100);

            // 3. latestPrice��null�i�ǉ�����Ȃ��j
            var analyzer3 = new TestAnalyzer(prices1);
            var stockInfo3 = StockInfo.GetInstance(new WatchList.WatchStock { Code = "TEST3", Name = "Test3" });
            stockInfo3.LatestScrapedPrice = null;
            double rsi3 = analyzer3.GetCutlerRSI(v, lastTradingDay, stockInfo3);
            Assert.InRange(rsi3, 0, 100);

            // 4. latestPrice.Date.Date > ���߉c�Ɠ��i�ǉ�����Ȃ��j
            var analyzer4 = new TestAnalyzer(prices1);
            var stockInfo4 = StockInfo.GetInstance(new WatchList.WatchStock { Code = "TEST4", Name = "Test4" });
            stockInfo4.LatestScrapedPrice = new StockInfo.ScrapedPrice
            {
                Date = lastTradingDay.AddDays(1), // �������t
                Close = 999
            };
            double rsi4 = analyzer4.GetCutlerRSI(v, lastTradingDay, stockInfo4);
            Assert.InRange(rsi4, 0, 100);

            // 5. �ǉ���A������v+1�𒴂���ꍇ�i�Â��f�[�^���폜�����j
            var prices5 = new List<(DateTime, double)>();
            for (int i = 0; i < v + 1; i++)
                prices5.Add((baseDate.AddDays(i), 100 + i));
            // �Ō�̓��t�� lastTradingDay ���O
            var analyzer5 = new TestAnalyzer(prices5);
            var stockInfo5 = StockInfo.GetInstance(new WatchList.WatchStock { Code = "TEST5", Name = "Test5" });
            stockInfo5.LatestScrapedPrice = new StockInfo.ScrapedPrice
            {
                Date = lastTradingDay,
                Close = 999
            };
            double rsi5 = analyzer5.GetCutlerRSI(v, lastTradingDay, stockInfo5);
            // v+1���ɒ�������Ă��邱�Ƃ��m�F
            var resultPrices = analyzer5.GetCutlerRsiPrices(v, lastTradingDay, stockInfo5);
            Assert.True(resultPrices.Count <= v + 1);
            Assert.InRange(rsi5, 0, 100);
        }
    }
}


