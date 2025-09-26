using System.Threading.Tasks;
using ConsoleApp1.Assets;
using ConsoleApp1.Scraper.Contracts;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

internal class YahooScraper : IAssetScraper
{
    private readonly IAssetScrapeStrategy _strategy;

    public YahooScraper(IAssetScrapeStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task ScrapeAsync(AssetInfo stockInfo, ScrapeTarget target)
    {
        var logger = CommonUtils.Instance.Logger;
        var url = _strategy.GetUrl(stockInfo, target);
        logger.LogInformation(url);

        string html = string.Empty;
        try
        {
            html = await CommonUtils.Instance.HttpClient.GetStringAsync(url);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning($"ScrapeAsync: 404 Not Found - Code:{stockInfo.Code} URL:{url}");
            return;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, $"ScrapeAsync: HttpRequestException - Code:{stockInfo.Code} URL:{url}");
            return;
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, $"ScrapeAsync: Unexpected error - Code:{stockInfo.Code} URL:{url}");
            return;
        }

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        try
        {
            _strategy.Parse(htmlDocument, stockInfo, target);
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, $"ScrapeAsync: Parse error - Code:{stockInfo.Code} URL:{url}");
        }
    }
}