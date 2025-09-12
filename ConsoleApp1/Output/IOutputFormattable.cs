using ConsoleApp1.Assets;

namespace ConsoleApp1.Output;

public interface IOutputFormattable
{
    string ToOutputString(AssetInfo stockInfo);
}