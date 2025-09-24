using System;
using Xunit;

namespace ConsoleApp1.Tests.Utils;

[Collection("CommonUtils collection")]
public class CommonUtilsTests
{
    [Fact]
    public void �����Ȃ炻�̓��t���Ԃ�()
    {
        // 2025/9/24�i���j�͕���
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 24);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 24), result);
    }

    [Fact]
    public void �y�j���Ȃ�O�c�Ɠ����Ԃ�()
    {
        // 2025/9/27�i�y�j�͓y�j��
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 27);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 26), result); // ���j��
    }

    [Fact]
    public void ���j���Ȃ�O�c�Ɠ����Ԃ�()
    {
        // 2025/9/28�i���j�͓��j��
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 28);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 26), result); // ���j��
    }

    [Fact]
    public void �j���Ȃ�O�c�Ɠ����Ԃ�()
    {
        // 2025/9/23�i�΁j�͏H���̓��i�j���j
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 23);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 22), result); // �O���i���j���j
    }
}