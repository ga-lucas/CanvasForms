using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// UserControl — lifecycle, border, AutoScaleDimensions
// ════════════════════════════════════════════════════════════════════════════════
public class UserControlTests
{
    // ── Defaults ──────────────────────────────────────────────────────────────

    [Fact]
    public void BorderStyle_DefaultsToNone()
    {
        var uc = new UserControl();
        Assert.Equal(BorderStyle.None, uc.BorderStyle);
    }

    [Fact]
    public void AutoScaleDimensions_DefaultIsNonZero()
    {
        var uc = new UserControl();
        Assert.True(uc.AutoScaleDimensions.Width  > 0);
        Assert.True(uc.AutoScaleDimensions.Height > 0);
    }

    // ── BorderStyle round-trip ────────────────────────────────────────────────

    [Fact]
    public void BorderStyle_FixedSingle_RoundTrips()
    {
        var uc = new UserControl { BorderStyle = BorderStyle.FixedSingle };
        Assert.Equal(BorderStyle.FixedSingle, uc.BorderStyle);
    }

    [Fact]
    public void BorderStyle_Fixed3D_RoundTrips()
    {
        var uc = new UserControl { BorderStyle = BorderStyle.Fixed3D };
        Assert.Equal(BorderStyle.Fixed3D, uc.BorderStyle);
    }

    // ── AutoScaleDimensions assignment (designer pattern) ─────────────────────

    [Fact]
    public void AutoScaleDimensions_CanBeSetByDesigner()
    {
        var uc = new UserControl();
        uc.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
        Assert.Equal(7f,  uc.AutoScaleDimensions.Width);
        Assert.Equal(15f, uc.AutoScaleDimensions.Height);
    }

    // ── CreateControl lifecycle ───────────────────────────────────────────────

    [Fact]
    public void CreateControl_FiresOnCreateControlOnce()
    {
        var uc = new TrackingUserControl();
        Assert.Equal(0, uc.CreateControlCount);

        uc.CreateControl();
        Assert.Equal(1, uc.CreateControlCount);

        // Second call must be idempotent
        uc.CreateControl();
        Assert.Equal(1, uc.CreateControlCount);
    }

    [Fact]
    public void OnCreateControl_IsVirtualAndOverridable()
    {
        var uc = new TrackingUserControl();
        uc.CreateControl();
        Assert.True(uc.OnCreateControlCalled);
    }

    // ── Load event ────────────────────────────────────────────────────────────

    [Fact]
    public void RaiseLoad_FiresLoadEvent()
    {
        var uc = new UserControl();
        bool fired = false;
        uc.Load += (_, _) => fired = true;
        uc.RaiseLoad();
        Assert.True(fired);
    }

    [Fact]
    public void RaiseLoad_PassesSenderAsUserControl()
    {
        var uc = new UserControl();
        object? sender = null;
        uc.Load += (s, _) => sender = s;
        uc.RaiseLoad();
        Assert.Same(uc, sender);
    }

    // ── AutoSize ──────────────────────────────────────────────────────────────

    [Fact]
    public void AutoSize_DefaultsFalse()
    {
        var uc = new UserControl();
        Assert.False(uc.AutoSize);
    }

    [Fact]
    public void AutoSizeMode_DefaultsGrowOnly()
    {
        var uc = new UserControl();
        Assert.Equal(AutoSizeMode.GrowOnly, uc.AutoSizeMode);
    }

    [Fact]
    public void AutoSize_CanBeSetTrue()
    {
        var uc = new UserControl { AutoSize = true };
        Assert.True(uc.AutoSize);
    }

    // ── Child controls ────────────────────────────────────────────────────────

    [Fact]
    public void Controls_CanAddChildrenToUserControl()
    {
        var uc  = new UserControl();
        var btn = new Button();
        uc.Controls.Add(btn);
        Assert.Single(uc.Controls);
        Assert.Same(btn, uc.Controls[0]);
    }

    // ── Helper subclass ───────────────────────────────────────────────────────

    private sealed class TrackingUserControl : UserControl
    {
        public int  CreateControlCount    { get; private set; }
        public bool OnCreateControlCalled { get; private set; }

        protected override void OnCreateControl()
        {
            CreateControlCount++;
            OnCreateControlCalled = true;
            base.OnCreateControl();
        }
    }
}
