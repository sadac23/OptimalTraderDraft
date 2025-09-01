using System;
using Xunit;

namespace ConsoleApp1.Tests
{
    public class CommonUtilsFixture : IDisposable
    {
        public CommonUtilsFixture()
        {
            CommonUtils.SetInstanceForTest(new DummyCommonUtils());
        }

        public void Dispose()
        {
            // 必要なら後始末処理をここに記述
        }
    }
}