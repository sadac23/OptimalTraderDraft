using Xunit;

namespace ConsoleApp1.Tests.Utils
{
    [CollectionDefinition("CommonUtils collection")]
    public class CommonUtilsCollection : ICollectionFixture<CommonUtilsFixture>
    {
        // このクラスには実装は不要です。属性のみでOKです。
    }
}