using Xunit;

namespace ConsoleApp1.Tests
{
    [CollectionDefinition("CommonUtils collection")]
    public class CommonUtilsCollection : ICollectionFixture<CommonUtilsFixture>
    {
        // ���̃N���X�ɂ͎����͕s�v�ł��B�����݂̂�OK�ł��B
    }
}