// See https://aka.ms/new-console-template for more information

using ClosedXML.Excel;
using System.Data.SQLite;

internal class WatchList
{
    internal static List<WatchStock> GetWatchStockList(string connectionString)
    {
        string query = "SELECT * FROM watch_list WHERE ifnull(del_flag, 0) = 0";

        List<WatchStock> results = new List<WatchStock>();

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

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
        }
        return results;
    }

    internal static List<WatchStock> GetXlsxWatchStockList(string filePath)
    {
        List<WatchStock> results = new List<WatchStock>();

        // 読み込むワークシート名を指定
        string sheetName = "ウォッチリスト";

        // 読み込み開始行のインデックス（1ベース）
        int startRowIndex = 3;

        // Excelファイルを読み込む
        using (var workbook = new XLWorkbook(filePath))
        {
            // 指定したワークシートを取得
            var worksheet = workbook.Worksheet(sheetName);

            if (worksheet != null)
            {
                // 指定行から順番にデータを取得
                var rows = worksheet.RowsUsed()
                    .Where(row => row.RowNumber() >= startRowIndex);

                foreach (var row in rows)
                {
                    //foreach (var cell in row.Cells())
                    //{
                    //    Console.Write(cell.Value + "\t");
                    //}
                    //Console.WriteLine();

                    WatchStock data = new WatchStock
                    {
                        Code = row.Cell(3).Value.ToString(),
                        Name = row.Cell(4).Value.ToString(),
                        Classification = row.Cell(5).Value.ToString(),
                        DeleteDate = row.Cell(7).Value.ToString(),
                    };
                    results.Add(data);
                }
            }
            else
            {
                Console.WriteLine($"ワークシート '{sheetName}' が見つかりません。");
            }
        }

        return results;
    }

    internal class WatchStock
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DeleteDate { get; set; }
        public string Classification { get; set; }
    }
}