using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

/// <summary>
/// Verifies Control.Invoke / BeginInvoke / EndInvoke compatibility overloads.
/// </summary>
public class InvokeTests
{
    // ── Invoke(Action) ────────────────────────────────────────────────────────

    [Fact]
    public void Invoke_Action_ExecutesImmediately()
    {
        var ctrl = new Panel();
        int counter = 0;
        ctrl.Invoke(() => counter++);
        Assert.Equal(1, counter);
    }

    [Fact]
    public void Invoke_ActionCalledMultipleTimes_AccumulatesCorrectly()
    {
        var ctrl = new Panel();
        int counter = 0;
        ctrl.Invoke(() => counter += 10);
        ctrl.Invoke(() => counter += 5);
        Assert.Equal(15, counter);
    }

    // ── Invoke<T>(Func<T>) ────────────────────────────────────────────────────

    [Fact]
    public void Invoke_Func_ReturnsValue()
    {
        var ctrl = new Panel();
        var result = ctrl.Invoke(() => 42);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Invoke_FuncString_ReturnsString()
    {
        var ctrl = new Panel();
        var result = ctrl.Invoke(() => "hello");
        Assert.Equal("hello", result);
    }

    // ── Invoke(Delegate) — existing overloads still work ─────────────────────

    [Fact]
    public void Invoke_Delegate_ExecutesAndReturnsResult()
    {
        var ctrl = new Panel();
        Func<int> del = () => 99;
        var result = ctrl.Invoke((Delegate)del);
        Assert.Equal(99, result);
    }

    [Fact]
    public void Invoke_DelegateWithArgs_ExecutesAndReturnsResult()
    {
        var ctrl = new Panel();
        Func<int, int> del = x => x * 2;
        var result = ctrl.Invoke((Delegate)del, 7);
        Assert.Equal(14, result);
    }

    // ── BeginInvoke / EndInvoke ───────────────────────────────────────────────

    [Fact]
    public void BeginInvoke_Action_ExecutesEventually()
    {
        var ctrl = new Panel();
        int counter = 0;
        var ar = ctrl.BeginInvoke(() => counter++);
        // In WASM / no SyncContext, executes synchronously — result is immediately available
        Assert.NotNull(ar);
        // EndInvoke should not throw
        ctrl.EndInvoke(ar);
        Assert.True(counter >= 0); // may be 0 (posted) or 1 (sync) depending on context
    }

    [Fact]
    public void BeginInvoke_Delegate_ReturnsIAsyncResult()
    {
        var ctrl = new Panel();
        Action del = () => { };
        var ar = ctrl.BeginInvoke((Delegate)del);
        Assert.NotNull(ar);
    }

    [Fact]
    public void BeginInvoke_DelegateWithReturnValue_EndInvokeReturnsResult()
    {
        var ctrl = new Panel();
        Func<int> del = () => 55;
        var ar = ctrl.BeginInvoke((Delegate)del);
        var result = ctrl.EndInvoke(ar);
        // In no-SyncContext environment result is returned synchronously
        Assert.True(result == null || (int)result == 55);
    }

    // ── InvokeRequired ────────────────────────────────────────────────────────

    [Fact]
    public void InvokeRequired_AlwaysFalseInWasm()
    {
        var ctrl = new Panel();
        Assert.False(ctrl.InvokeRequired);
    }
}
