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
            // �K�v�Ȃ��n�������������ɋL�q
        }
    }
}