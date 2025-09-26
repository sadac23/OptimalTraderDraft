namespace ConsoleApp1.Scraper.Contracts
{
    using System.Threading.Tasks;
    using ConsoleApp1.Assets;

    public interface IAssetScraper
    {
        Task ScrapeAsync(AssetInfo assetInfo, ScrapeTarget target);
    }

    public enum ScrapeTarget
    {
        Dividend,
        Yutai,
        Finance,
        History,
        Top,
        Profile,
        Disclosure
    }
}