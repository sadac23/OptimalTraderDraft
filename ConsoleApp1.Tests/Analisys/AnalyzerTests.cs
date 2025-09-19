using System.Data.SQLite;
using ConsoleApp1.Database;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Models;

namespace ConsoleApp1.Tests.Analisys
{
    [Collection("CommonUtils collection")]
    public class AnalyzerTests
    {
        [Fact]
        public void GetCutlerRsiPrices_AddsLatestScrapedPrice_WhenNotInHistory()
        {
            // Arrange
            using var connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            connection.Open();

            // �e�[�u���쐬
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

            // �e�X�g�f�[�^�}���i���߉c�Ɠ����O�̓��t�̂݁j
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

            // DbConnectionFactory�̃R�l�N�V�����������ւ�
            DbConnectionFactory.SetConnection(connection);

            var stockInfo = AssetInfoFactory.Create(new WatchList.WatchStock
            {
                Code = code,
                Classification = "1",
                IsFavorite = "1",
                Memo = "�e�X�g"
            });

            // ���߉c�Ɠ����e�X�g�p�ɐݒ�
            var lastTradingDate = DateTime.Today;
            CommonUtils.Instance.LastTradingDate = lastTradingDate;

            // LatestScrapedPrice��history�ɑ��݂��Ȃ����t�ŃZ�b�g
            stockInfo.LatestScrapedPrice = new ScrapedPrice
            {
                Date = lastTradingDate,
                Close = 999 // �e�X�g�p�̓��ْl
            };

            var analyzer = new Analyzer();

            // Act
            var result = analyzer.GetCutlerRsiPrices(2, lastTradingDate, stockInfo);

            // Assert
            // ���߉c�Ɠ������ǉ�����Ă��邩
            Assert.Contains(result, tuple => tuple.Item1.Date == lastTradingDate && tuple.Item2 == 999);
            // ������v+1���ł��邱��
            Assert.Equal(3, result.Count);
        }
    }
}