using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class ProgressBarDefaultsTests
{
    [Fact]
    public void DefaultMinimum_IsZero()
    {
        var pb = new ProgressBar();
        Assert.Equal(0, pb.Minimum);
    }

    [Fact]
    public void DefaultMaximum_Is100()
    {
        var pb = new ProgressBar();
        Assert.Equal(100, pb.Maximum);
    }

    [Fact]
    public void DefaultValue_IsZero()
    {
        var pb = new ProgressBar();
        Assert.Equal(0, pb.Value);
    }

    [Fact]
    public void DefaultStep_Is10()
    {
        var pb = new ProgressBar();
        Assert.Equal(10, pb.Step);
    }

    [Fact]
    public void DefaultStyle_IsBlocks()
    {
        var pb = new ProgressBar();
        Assert.Equal(ProgressBarStyle.Blocks, pb.Style);
    }

    [Fact]
    public void DefaultTabStop_IsFalse()
    {
        var pb = new ProgressBar();
        Assert.False(pb.TabStop);
    }
}

public class ProgressBarValueTests
{
    [Fact]
    public void Value_CanBeSetWithinRange()
    {
        var pb = new ProgressBar();
        pb.Value = 50;
        Assert.Equal(50, pb.Value);
    }

    [Fact]
    public void Value_Clamps_BelowMinimum()
    {
        var pb = new ProgressBar();
        pb.Value = -10;
        Assert.Equal(0, pb.Value);
    }

    [Fact]
    public void Value_Clamps_AboveMaximum()
    {
        var pb = new ProgressBar();
        pb.Value = 200;
        Assert.Equal(100, pb.Value);
    }

    [Fact]
    public void Value_AtMinimum_IsAccepted()
    {
        var pb = new ProgressBar();
        pb.Value = pb.Minimum;
        Assert.Equal(pb.Minimum, pb.Value);
    }

    [Fact]
    public void Value_AtMaximum_IsAccepted()
    {
        var pb = new ProgressBar();
        pb.Value = pb.Maximum;
        Assert.Equal(pb.Maximum, pb.Value);
    }
}

public class ProgressBarRangeTests
{
    [Fact]
    public void Minimum_Set_ClampsValueIfNeeded()
    {
        var pb = new ProgressBar();
        pb.Value = 10;
        pb.Minimum = 20; // value below new minimum
        Assert.Equal(20, pb.Value);
    }

    [Fact]
    public void Maximum_Set_ClampsValueIfNeeded()
    {
        var pb = new ProgressBar();
        pb.Value = 80;
        pb.Maximum = 50; // value above new maximum
        Assert.Equal(50, pb.Value);
    }

    [Fact]
    public void SetRange_UpdatesMinAndMax()
    {
        var pb = new ProgressBar();
        pb.SetRange(10, 200);
        Assert.Equal(10, pb.Minimum);
        Assert.Equal(200, pb.Maximum);
    }

    [Fact]
    public void SetRange_ClampsExistingValue()
    {
        var pb = new ProgressBar();
        pb.Value = 100;
        pb.SetRange(0, 50);
        Assert.Equal(50, pb.Value);
    }

    [Fact]
    public void SetRange_MinGreaterThanMax_ThrowsArgumentException()
    {
        var pb = new ProgressBar();
        Assert.Throws<ArgumentException>(() => pb.SetRange(100, 50));
    }
}

public class ProgressBarStepTests
{
    [Fact]
    public void PerformStep_IncrementsValueByStep()
    {
        var pb = new ProgressBar { Value = 0, Step = 10 };
        pb.PerformStep();
        Assert.Equal(10, pb.Value);
    }

    [Fact]
    public void PerformStep_DoesNotExceedMaximum()
    {
        var pb = new ProgressBar { Value = 95, Step = 10, Maximum = 100 };
        pb.PerformStep();
        Assert.Equal(100, pb.Value);
    }

    [Fact]
    public void Increment_IncrementsValueByAmount()
    {
        var pb = new ProgressBar { Value = 20 };
        pb.Increment(15);
        Assert.Equal(35, pb.Value);
    }

    [Fact]
    public void Increment_DoesNotExceedMaximum()
    {
        var pb = new ProgressBar { Value = 90 };
        pb.Increment(50);
        Assert.Equal(100, pb.Value);
    }

    [Fact]
    public void Step_RoundTrips()
    {
        var pb = new ProgressBar();
        pb.Step = 5;
        Assert.Equal(5, pb.Step);
    }
}

public class ProgressBarStyleTests
{
    [Fact]
    public void Style_Continuous_RoundTrips()
    {
        var pb = new ProgressBar();
        pb.Style = ProgressBarStyle.Continuous;
        Assert.Equal(ProgressBarStyle.Continuous, pb.Style);
    }

    [Fact]
    public void Style_Marquee_RoundTrips()
    {
        var pb = new ProgressBar();
        pb.Style = ProgressBarStyle.Marquee;
        Assert.Equal(ProgressBarStyle.Marquee, pb.Style);
        pb.Style = ProgressBarStyle.Blocks; // restore to stop timer
    }

    [Fact]
    public void MarqueeAnimationSpeed_DefaultIs100()
    {
        var pb = new ProgressBar();
        Assert.Equal(100, pb.MarqueeAnimationSpeed);
    }

    [Fact]
    public void MarqueeAnimationSpeed_CannotBeNegative()
    {
        var pb = new ProgressBar();
        pb.MarqueeAnimationSpeed = -50;
        Assert.Equal(0, pb.MarqueeAnimationSpeed);
    }

    [Fact]
    public void RightToLeftLayout_DefaultIsFalse()
    {
        var pb = new ProgressBar();
        Assert.False(pb.RightToLeftLayout);
    }

    [Fact]
    public void RightToLeftLayout_RoundTrips()
    {
        var pb = new ProgressBar();
        pb.RightToLeftLayout = true;
        Assert.True(pb.RightToLeftLayout);
    }
}
