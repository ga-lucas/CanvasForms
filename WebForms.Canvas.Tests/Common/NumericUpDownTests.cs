using System;
using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// NumericUpDown — general + Hexadecimal mode
// ════════════════════════════════════════════════════════════════════════════════
public class NumericUpDownTests
{
    // ── Defaults ──────────────────────────────────────────────────────────────

    [Fact]
    public void DefaultValue_IsZero()
    {
        var n = new NumericUpDown();
        Assert.Equal(0m, n.Value);
    }

    [Fact]
    public void DefaultMinimum_IsZero() => Assert.Equal(0m, new NumericUpDown().Minimum);

    [Fact]
    public void DefaultMaximum_Is100() => Assert.Equal(100m, new NumericUpDown().Maximum);

    // ── Value clamping ────────────────────────────────────────────────────────

    [Fact]
    public void Value_ClampsToMinimum()
    {
        var n = new NumericUpDown { Minimum = 5, Maximum = 50, Value = 0 };
        Assert.Equal(5m, n.Value);
    }

    [Fact]
    public void Value_ClampsToMaximum()
    {
        var n = new NumericUpDown { Minimum = 0, Maximum = 10, Value = 200 };
        Assert.Equal(10m, n.Value);
    }

    [Fact]
    public void ValueChanged_FiresOnChange()
    {
        var n = new NumericUpDown();
        bool fired = false;
        n.ValueChanged += (_, _) => fired = true;
        n.Value = 5;
        Assert.True(fired);
    }

    [Fact]
    public void ValueChanged_DoesNotFireIfValueUnchanged()
    {
        var n = new NumericUpDown { Value = 5 };
        int count = 0;
        n.ValueChanged += (_, _) => count++;
        n.Value = 5;
        Assert.Equal(0, count);
    }

    // ── UpButton / DownButton ─────────────────────────────────────────────────

    [Fact]
    public void UpButton_IncrementsValue()
    {
        var n = new NumericUpDown { Value = 3, Increment = 2 };
        n.UpButton();
        Assert.Equal(5m, n.Value);
    }

    [Fact]
    public void DownButton_DecrementsValue()
    {
        var n = new NumericUpDown { Value = 3, Increment = 2 };
        n.DownButton();
        Assert.Equal(1m, n.Value);
    }

    [Fact]
    public void UpButton_DoesNotExceedMaximum()
    {
        var n = new NumericUpDown { Maximum = 10, Value = 9, Increment = 5 };
        n.UpButton();
        Assert.Equal(10m, n.Value);
    }

    [Fact]
    public void DownButton_DoesNotGoBelowMinimum()
    {
        var n = new NumericUpDown { Minimum = 0, Value = 1, Increment = 5 };
        n.DownButton();
        Assert.Equal(0m, n.Value);
    }

    // ── Hexadecimal display ───────────────────────────────────────────────────

    [Fact]
    public void Text_InHexMode_ReturnsUppercaseHex()
    {
        var n = new NumericUpDown { Hexadecimal = true, Maximum = 1000, Value = 255 };
        Assert.Equal("FF", n.Text);
    }

    [Fact]
    public void Text_InHexMode_ZeroDisplaysAsZero()
    {
        var n = new NumericUpDown { Hexadecimal = true, Value = 0 };
        Assert.Equal("0", n.Text);
    }

    [Fact]
    public void Text_Setter_InHexMode_ParsesHexString()
    {
        var n = new NumericUpDown { Hexadecimal = true, Maximum = 1000 };
        n.Text = "1F";
        Assert.Equal(31m, n.Value);
    }

    [Fact]
    public void Text_Setter_InHexMode_ParsesUppercaseHex()
    {
        var n = new NumericUpDown { Hexadecimal = true, Maximum = 10000 };
        n.Text = "FF";
        Assert.Equal(255m, n.Value);
    }

    [Fact]
    public void Hexadecimal_False_UsesDecimalFormat()
    {
        var n = new NumericUpDown { Hexadecimal = false, Maximum = 1000, Value = 255 };
        Assert.Equal("255", n.Text);
    }

    // ── ThousandsSeparator / DecimalPlaces ────────────────────────────────────

    [Fact]
    public void ThousandsSeparator_FormatsLargeNumbers()
    {
        var n = new NumericUpDown { Maximum = 100000, Value = 12345, ThousandsSeparator = true, DecimalPlaces = 0 };
        Assert.Contains(",", n.Text);
    }

    [Fact]
    public void DecimalPlaces_IncludesDecimalInDisplay()
    {
        var n = new NumericUpDown { DecimalPlaces = 2, Value = 3 };
        Assert.Contains(".", n.Text);
    }
}
