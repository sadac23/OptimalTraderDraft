using System;
using Xunit;

namespace ConsoleApp1.Tests.Utils;

[Collection("CommonUtils collection")]
public class CommonUtilsTests
{
    [Fact]
    public void •½“ú‚È‚ç‚»‚Ì“ú•t‚ª•Ô‚é()
    {
        // 2025/9/24i…j‚Í•½“ú
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 24);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 24), result);
    }

    [Fact]
    public void “y—j“ú‚È‚ç‘O‰c‹Æ“ú‚ª•Ô‚é()
    {
        // 2025/9/27i“yj‚Í“y—j“ú
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 27);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 26), result); // ‹à—j“ú
    }

    [Fact]
    public void “ú—j“ú‚È‚ç‘O‰c‹Æ“ú‚ª•Ô‚é()
    {
        // 2025/9/28i“új‚Í“ú—j“ú
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 28);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 26), result); // ‹à—j“ú
    }

    [Fact]
    public void j“ú‚È‚ç‘O‰c‹Æ“ú‚ª•Ô‚é()
    {
        // 2025/9/23i‰Îj‚ÍH•ª‚Ì“úij“új
        CommonUtils.Instance.ExecusionDate = new DateTime(2025, 9, 23);

        var result = CommonUtils.Instance.GetLastTradingDay();
        Assert.Equal(new DateTime(2025, 9, 22), result); // ‘O“úiŒ—j“új
    }
}