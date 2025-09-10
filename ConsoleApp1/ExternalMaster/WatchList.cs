// See https://aka.ms/new-console-template for more information

using ClosedXML.Excel;
using ConsoleApp1.Database;
using DocumentFormat.OpenXml.Office2013.Word;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class WatchList
{
    internal static List<WatchStock> GetWatchStockList(string connectionString)
    {
        string query = "SELECT * FROM watch_list WHERE ifnull(del_flag, 0) = 0";

        List<WatchStock> results = new List<WatchStock>();

        SQLiteConnection connection = DbConnectionFactory.GetConnection();

        using (SQLiteCommand command = new SQLiteCommand(query, connection))
        {
            //                command.Parameters.AddWithValue("@yourValue", "some_value");

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())

                {
                    WatchStock data = new WatchStock
                    {
                        Code = reader.GetString(0)
                    };
                    results.Add(data);
                }
            }
        }

        return results;
    }

    internal static List<WatchStock> LoadXlsx()
    {
        List<WatchStock> results = new List<WatchStock>();

        // 読み込むワークシート名を指定
        string sheetName = "ウォッチリスト";

        // 読み込み開始行のインデックス（1ベース）
        int startRowIndex = 3;

        // Excelファイルを読み込む
        using (var workbook = new XLWorkbook(CommonUtils.Instance.FilepathOfWatchList))
        {
            // 指定したワークシートを取得
            var worksheet = workbook.Worksheet(sheetName);

            if (worksheet != null)
            {
                List<WatchStock> temp = new List<WatchStock>();

                // 指定行から順番にデータを取得
                var rows = worksheet.RowsUsed()
                    .Where(row => row.RowNumber() >= startRowIndex);

                foreach (var row in rows)
                {
                    WatchStock data = new WatchStock
                    {
                        Code = row.Cell(3).Value.ToString(),
                        Name = row.Cell(4).Value.ToString(),
                        Classification = row.Cell(5).Value.ToString(),
                        IsFavorite = row.Cell(6).Value.ToString(),
                        DeleteDate = row.Cell(8).Value.ToString(),
                        Memo = row.Cell(9).Value.ToString(),
                    };
                    temp.Add(data);
                }

                results = temp.OrderBy(p => p.Classification).ThenBy(p => p.Code).ToList();
            }
            else
            {
                Console.WriteLine($"ワークシート '{sheetName}' が見つかりません。");
            }
        }

        return results;
    }

    public class WatchStock
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DeleteDate { get; set; }
        /// <summary>
        /// 1:日本個別、2:日本ETF、3:日本投信、4:米国ETF
        /// </summary>
        public string Classification { get; set; }
        public string IsFavorite { get; set; }
        public string Memo { get; set; }
    }

}