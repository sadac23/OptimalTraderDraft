using System.Collections.Generic;
using Xunit;

namespace ConsoleApp1.Tests.Analisys
{
    public class Analyzer_CalcCutlerRSI_Tests
    {
        [Fact]
        public void CalcCutlerRSI_ReturnsValueInRange()
        {
            int v = 14;
            var prices = new List<double> { 100, 102, 101, 103, 105, 104, 106, 108, 107, 110, 112, 111, 113, 115, 114 };
            double rsi = Analyzer.CalcCutlerRSI(v, prices);
            Assert.InRange(rsi, 0, 100);
        }

        [Fact]
        public void CalcCutlerRSI_ReturnsZero_WhenNoData()
        {
            int v = 14;
            var prices = new List<double>();
            double rsi = Analyzer.CalcCutlerRSI(v, prices);
            Assert.Equal(0, rsi);
        }

        [Fact]
        public void CalcCutlerRSI_ReturnsZero_WhenOnlyOneData()
        {
            int v = 14;
            var prices = new List<double> { 100 };
            double rsi = Analyzer.CalcCutlerRSI(v, prices);
            Assert.Equal(0, rsi);
        }
    }
}

