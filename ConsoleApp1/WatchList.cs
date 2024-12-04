// See https://aka.ms/new-console-template for more information

using ClosedXML.Excel;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    internal static List<ExecutionStock> GetXlsxExecutionStockList(string? xlsxExecutionFilePath)
    {
        List<ExecutionStock> results = new List<ExecutionStock>();

        // 読み込むワークシート名を指定
        string sheetName = "約定履歴";

        // 読み込み開始行のインデックス（1ベース）
        int startRowIndex = 5;

        // Excelファイルを読み込む
        using (var workbook = new XLWorkbook(xlsxExecutionFilePath))
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
                    ExecutionStock data = new ExecutionStock
                    {
                        No = row.Cell(2).Value.ToString(),
                        PurchaseDate = row.Cell(3).Value.ToString(),
                        Code = row.Cell(4).Value.ToString(),
                        Name = row.Cell(5).Value.ToString(),
                        PurchasePrice = row.Cell(6).Value.ToString(),
                        SaleDate = row.Cell(11).Value.ToString(),
                        SalePrice = row.Cell(12).Value.ToString(),
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
                    WatchStock data = new WatchStock
                    {
                        Code = row.Cell(3).Value.ToString(),
                        Name = row.Cell(4).Value.ToString(),
                        Classification = row.Cell(5).Value.ToString(),
                        DeleteDate = row.Cell(7).Value.ToString(),
                        Memo = row.Cell(8).Value.ToString(),
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

    internal static List<WatchStock> GetXlsxWatchStockList(string? xlsxFilePath, List<ExecutionStock> executionList)
    {
        List<WatchStock> watchStocks = GetXlsxWatchStockList(xlsxFilePath);

        foreach (ExecutionStock executionStock in executionList)
        {
            // ウォッチリストに存在していないか
            if (!watchStocks.Any(watchStocks => watchStocks.Code == executionStock.Code))
            {
                // 所有しているか
                if (executionStock.Code != string.Empty 
                    && executionStock.PurchaseDate != string.Empty 
                    && executionStock.SaleDate == string.Empty)
                {
                    var w = new WatchStock()
                    {
                        Code = executionStock.Code,
                        Name = executionStock.Name,
                    };

                    watchStocks.Add(w);
                }
            }
        }

        return watchStocks;
    }

    internal class WatchStock
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DeleteDate { get; set; }
        public string Classification { get; set; }
        public string Memo { get; internal set; }
    }

    internal class ExecutionStock
    {
        public string No { get; internal set; }
        public string PurchaseDate { get; internal set; }
        public string Code { get; internal set; }
        public string Name { get; internal set; }
        public string PurchasePrice { get; internal set; }
        public string SaleDate { get; internal set; }
        public string SalePrice { get; internal set; }
    }
}