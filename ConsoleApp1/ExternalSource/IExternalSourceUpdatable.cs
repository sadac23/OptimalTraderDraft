using ConsoleApp1.Assets;

namespace ConsoleApp1.ExternalSource;

public interface IExternalSourceUpdatable
{
    Task UpdateFromExternalSourceAsync(AssetInfo stockInfo);
}