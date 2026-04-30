using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

using WinTimer = System.Windows.Forms.Timer;

namespace Canvas.Windows.Forms.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// Timer
// ════════════════════════════════════════════════════════════════════════════════
public class TimerTests
{
    // ── Construction & defaults ───────────────────────────────────────────────

    [Fact]
    public void DefaultInterval_Is100()
    {
        using var t = new WinTimer();
        Assert.Equal(100, t.Interval);
    }

    [Fact]
    public void DefaultEnabled_IsFalse()
    {
        using var t = new WinTimer();
        Assert.False(t.Enabled);
    }

    [Fact]
    public void DefaultTag_IsNull()
    {
        using var t = new WinTimer();
        Assert.Null(t.Tag);
    }

    // ── Interval ─────────────────────────────────────────────────────────────

    [Fact]
    public void Interval_RoundTrips()
    {
        using var t = new WinTimer { Interval = 250 };
        Assert.Equal(250, t.Interval);
    }

    [Fact]
    public void Interval_Zero_Throws()
    {
        using var t = new WinTimer();
        var ex = Record.Exception(() => { t.Interval = 0; });
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }

    [Fact]
    public void Interval_Negative_Throws()
    {
        using var t = new WinTimer();
        var ex = Record.Exception(() => { t.Interval = -1; });
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }

    // ── Enabled / Start / Stop ────────────────────────────────────────────────

    [Fact]
    public void Start_SetsEnabled_True()
    {
        using var t = new WinTimer();
        t.Start();
        Assert.True(t.Enabled);
        t.Stop();
    }

    [Fact]
    public void Stop_SetsEnabled_False()
    {
        using var t = new WinTimer();
        t.Start();
        t.Stop();
        Assert.False(t.Enabled);
    }

    [Fact]
    public void Enabled_SetFalse_DoesNotThrowWhenAlreadyStopped()
    {
        using var t = new WinTimer();
        t.Enabled = false; // idempotent — no exception
        Assert.False(t.Enabled);
    }

    [Fact]
    public void Enabled_SetTrue_ThenFalse_RoundTrips()
    {
        using var t = new WinTimer { Interval = 500 };
        t.Enabled = true;
        Assert.True(t.Enabled);
        t.Enabled = false;
        Assert.False(t.Enabled);
    }

    // ── Tag ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Tag_RoundTrips()
    {
        using var t = new WinTimer { Tag = "hello" };
        Assert.Equal("hello", t.Tag);
    }

    // ── Tick fires ────────────────────────────────────────────────────────────

    [Fact(Timeout = 3000)]
    public async Task Tick_FiresAtLeastOnce_WhenEnabled()
    {
        using var t   = new WinTimer { Interval = 50 };
        var tcs       = new TaskCompletionSource<bool>();
        t.Tick       += (_, __) => tcs.TrySetResult(true);
        t.Start();
        var fired = await tcs.Task;
        t.Stop();
        Assert.True(fired);
    }

    [Fact(Timeout = 2000)]
    public async Task Tick_DoesNotFire_AfterStop()
    {
        using var t = new WinTimer { Interval = 30 };
        int count   = 0;
        t.Tick     += (_, __) => Interlocked.Increment(ref count);
        t.Start();
        await Task.Delay(80);
        t.Stop();
        int snapshot = count;
        await Task.Delay(100);           // wait longer than one interval after stop
        Assert.Equal(snapshot, count);   // no new ticks after Stop
    }

    [Fact(Timeout = 2000)]
    public async Task Tick_DoesNotFire_BeforeStart()
    {
        using var t = new WinTimer { Interval = 30 };
        int count   = 0;
        t.Tick     += (_, __) => Interlocked.Increment(ref count);
        await Task.Delay(100);           // wait without starting
        Assert.Equal(0, count);
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var t = new WinTimer { Interval = 50 };
        t.Start();
        t.Dispose(); // should not throw
    }

    [Fact]
    public void Dispose_TwiceDoesNotThrow()
    {
        var t = new WinTimer();
        t.Dispose();
        t.Dispose(); // idempotent
    }

    [Fact]
    public void Enabled_AfterDispose_RemainsDisabled()
    {
        var t = new WinTimer();
        t.Start();
        t.Dispose();
        Assert.False(t.Enabled);
    }

    [Fact(Timeout = 1000)]
    public async Task Tick_DoesNotFire_AfterDispose()
    {
        var t   = new WinTimer { Interval = 30 };
        int count = 0;
        t.Tick += (_, __) => Interlocked.Increment(ref count);
        t.Start();
        await Task.Delay(60);
        t.Dispose();
        int snapshot = count;
        await Task.Delay(100);
        Assert.Equal(snapshot, count);
    }

    // ── Interval change while running ─────────────────────────────────────────

    [Fact]
    public void Interval_CanBeChanged_WhileRunning_WithoutThrowing()
    {
        using var t = new WinTimer { Interval = 100 };
        t.Start();
        t.Interval = 200; // should restart cleanly, not throw
        t.Stop();
    }

    // ── IContainer constructor ────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithContainer_AddsComponent()
    {
        var container = new System.ComponentModel.Container();
        using var t   = new WinTimer(container);
        Assert.Single(container.Components);
    }
}
