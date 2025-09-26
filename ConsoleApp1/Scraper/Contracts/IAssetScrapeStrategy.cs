namespace ConsoleApp1.Scraper.Contracts
{
    using System.Collections.Generic;
    using ConsoleApp1.Assets;
    using HtmlAgilityPack;

    public interface IAssetScrapeStrategy
    {
        string GetUrl(AssetInfo assetInfo, ScrapeTarget target);
        Dictionary<string, string> GetXPaths(ScrapeTarget target);
        void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target);
    }
}