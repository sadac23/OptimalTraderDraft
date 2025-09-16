// See https://aka.ms/new-console-template for more information
using System.Text;
using ConsoleApp1.Assets;
using ConsoleApp1.Assets.Calculators;
using ConsoleApp1.Assets.Repositories;
using ConsoleApp1.ExternalSource;
using ConsoleApp1.Output;

public class IndexInfo : AssetInfo
{

    // Repository対応の新コンストラクタ（推奨）
    public IndexInfo(
        WatchList.WatchStock watchStock,
        IExternalSourceUpdatable updater,
        IOutputFormattable formatter,
        IAssetRepository repository,
        IAssetJudgementStrategy judgementStrategy)
        : base(watchStock, updater, formatter, repository, judgementStrategy)
    {
        // 必要に応じて初期化処理を追加
    }

    /// <summary>
    /// インデックス用の外部情報取得処理
    /// </summary>
    internal override async Task UpdateFromExternalSource()
    {
        if (_updater != null)
        {
            await _updater.UpdateFromExternalSourceAsync(this);
        }
        // 必要に応じて追加の処理をここに記述
    }

    /// <summary>
    /// インデックス種別用の出力処理
    /// </summary>
    internal override string ToOutputString()
    {
        if (_formatter != null)
        {
            return _formatter.ToOutputString(this);
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
            var lastUpdateDay = stockInfo.GetLastHistoryUpdateDay();

            if (CommonUtils.Instance.LastTradingDate > lastUpdateDay)
            {
                await yahooScraper.ScrapeHistory(stockInfo, lastUpdateDay, CommonUtils.Instance.ExecusionDate);
            }
        });
        tasks.Add(yahooHistory);

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
            sb.AppendLine($"メモ：");
            sb.AppendLine(stockInfo.Memo);
        }

        return sb.ToString();
    }
}
