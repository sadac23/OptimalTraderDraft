namespace ConsoleApp1.Scraper.Contracts
{
    using System.Collections.Generic;
    using ConsoleApp1.Assets;

    public interface IAssetInfoMerger
    {
        AssetInfo Merge(IEnumerable<AssetInfo> sources);
    }
}