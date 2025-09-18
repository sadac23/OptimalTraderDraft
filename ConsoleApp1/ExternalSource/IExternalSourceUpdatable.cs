namespace ConsoleApp1.ExternalSource;

public interface IExternalSourceUpdatable
{
    Task UpdateFromExternalSourceAsync(StockInfo stockInfo);
}