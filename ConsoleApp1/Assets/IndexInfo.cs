// See https://aka.ms/new-console-template for more information
using System.Text;
using ConsoleApp1.Assets;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

public class IndexInfo : AssetInfo
{
    public IndexInfo(
        WatchList.WatchStock watchStock,
        IExternalSourceUpdatable updater,
        IOutputFormattable formatter)
        : base(watchStock, updater, formatter)
    {
    }

    /// <summary>
    /// インデックス用の外部情報取得処理
    /// </summary>
    internal override async Task UpdateFromExternalSource()
    {
        if (updater != null)
        {
            await updater.UpdateFromExternalSourceAsync(this);
        }
        // 必要に応じて追加の処理をここに記述
    }

    /// <summary>
    /// インデックス種別用の出力処理
    /// </summary>
    internal override string ToOutputString()
    {
        if (formatter != null)
        {
            return formatter.ToOutputString(this);
        }
        // 必要に応じて追加の処理をここに記述

        return string.Empty;
    }
}
// インデックス種別用の外部情報取得処理
public class IndexUpdater : IExternalSourceUpdatable
{
    public async Task UpdateFromExternalSourceAsync(AssetInfo stockInfo)
    {
        List<Task> tasks = new List<Task>();

        var yahooScraper = new YahooScraper();

        Task yahooTop = Task.Run(async () =>
        {
            await yahooScraper.ScrapeTop(stockInfo);
        });
        tasks.Add(yahooTop);

        Task yahooHistory = Task.Run(async () =>
        {
            // 履歴更新の最終日を取得（なければ基準開始日を取得）
            var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();

            // 最終更新後に直近営業日がある場合は履歴取得
            if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
            {
                await yahooScraper.ScrapeHistory(stockInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate);
            }
        });
        tasks.Add(yahooHistory);

        // タスクの実行待ち
        await Task.WhenAll(tasks);
    }
}

// インデックス種別用の出力処理
public class IndexFormatter : IOutputFormattable
{
    public string ToOutputString(AssetInfo stockInfo)
    {
        StringBuilder sb = new StringBuilder();

        var mark = CommonUtils.Instance.BadgeString.ShouldWatch;
        var count = 0;
        var s = string.Empty;

        sb.AppendLine($"{stockInfo.Code}：{stockInfo.Name}");

        sb.AppendLine($"株価：{stockInfo.LatestPrice.Price.ToString("N1")}" +
            $"（{stockInfo.LatestPrice.Date.ToString("yy/MM/dd")}" +
            $"：S{stockInfo.LatestPrice.RSIS.ToString("N2")}" +
            $",L{stockInfo.LatestPrice.RSIL.ToString("N2")}" +
            $"）{(stockInfo.LatestPrice.OversoldIndicator() || (stockInfo.IsOwnedNow() && stockInfo.LatestPrice.OverboughtIndicator()) ? mark : string.Empty)}");

        //チャート：
        sb.AppendLine($"チャート（RSI）：");
        foreach (var p in stockInfo.ChartPrices)
        {
            sb.AppendLine(
                $"{p.Date.ToString("MM/dd")}" +
                $"：{p.Price.ToString("N1")}" +
                $"：{CommonUtils.Instance.ConvertToPercentage(p.Volatility, true)}" +
                $"（S{p.RSIS.ToString("N2")}" +
                $",L{p.RSIL.ToString("N2")}）" +
                $"{(p.OversoldIndicator() ? mark : string.Empty)}" +
                $"");
        }

        if (!string.IsNullOrEmpty(stockInfo.Memo))
        {
            //メモ：
            sb.AppendLine($"メモ：");
            sb.AppendLine(stockInfo.Memo);
        }

        return sb.ToString();
    }
}
