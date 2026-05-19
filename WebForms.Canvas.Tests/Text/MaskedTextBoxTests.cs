using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class MaskedTextBoxDefaultsTests
{
    [Fact]
    public void DefaultMask_IsEmpty()
    {
        var mtb = new MaskedTextBox();
        Assert.Equal(string.Empty, mtb.Mask);
    }

    [Fact]
    public void DefaultPromptChar_IsUnderscore()
    {
        var mtb = new MaskedTextBox();
        Assert.Equal('_', mtb.PromptChar);
    }

    [Fact]
    public void DefaultPasswordChar_IsNull()
    {
        var mtb = new MaskedTextBox();
        Assert.Equal('\0', mtb.PasswordChar);
    }

    [Fact]
    public void DefaultHidePromptOnLeave_IsFalse()
    {
        var mtb = new MaskedTextBox();
        Assert.False(mtb.HidePromptOnLeave);
    }

    [Fact]
    public void DefaultBeepOnError_IsFalse()
    {
        var mtb = new MaskedTextBox();
        Assert.False(mtb.BeepOnError);
    }

    [Fact]
    public void DefaultUseSystemPasswordChar_IsFalse()
    {
        var mtb = new MaskedTextBox();
        Assert.False(mtb.UseSystemPasswordChar);
    }

    [Fact]
    public void DefaultCutCopyMaskFormat_IsIncludeLiterals()
    {
        var mtb = new MaskedTextBox();
        Assert.Equal(MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat);
    }
}

public class MaskedTextBoxMaskTests
{
    [Fact]
    public void Mask_Set_FiresMaskChanged()
    {
        var mtb = new MaskedTextBox();
        bool fired = false;
        mtb.MaskChanged += (_, _) => fired = true;
        mtb.Mask = "000-0000";
        Assert.True(fired);
    }

    [Fact]
    public void Mask_Set_ClearsText()
    {
        var mtb = new MaskedTextBox { Text = "Hello" };
        mtb.Mask = "00000";
        Assert.Equal(string.Empty, mtb.Text);
    }

    [Fact]
    public void Mask_Set_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.Mask = "(000) 000-0000";
        Assert.Equal("(000) 000-0000", mtb.Mask);
    }

    [Fact]
    public void MaskCompleted_EmptyMask_IsTrue()
    {
        var mtb = new MaskedTextBox();
        Assert.True(mtb.MaskCompleted);
    }

    [Fact]
    public void MaskCompleted_WithMask_NoInput_IsFalse()
    {
        var mtb = new MaskedTextBox { Mask = "000" };
        Assert.False(mtb.MaskCompleted);
    }

    [Fact]
    public void MaskedText_WithMask_ContainsPromptChars()
    {
        var mtb = new MaskedTextBox { Mask = "000" };
        Assert.Contains(mtb.PromptChar, mtb.MaskedText);
    }
}

public class MaskedTextBoxPropertyTests
{
    [Fact]
    public void PromptChar_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.PromptChar = '#';
        Assert.Equal('#', mtb.PromptChar);
    }

    [Fact]
    public void PasswordChar_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.PasswordChar = '*';
        Assert.Equal('*', mtb.PasswordChar);
    }

    [Fact]
    public void HidePromptOnLeave_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.HidePromptOnLeave = true;
        Assert.True(mtb.HidePromptOnLeave);
    }

    [Fact]
    public void BeepOnError_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.BeepOnError = true;
        Assert.True(mtb.BeepOnError);
    }

    [Fact]
    public void CutCopyMaskFormat_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.CutCopyMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        Assert.Equal(MaskFormat.ExcludePromptAndLiterals, mtb.CutCopyMaskFormat);
    }

    [Fact]
    public void UseSystemPasswordChar_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.UseSystemPasswordChar = true;
        Assert.True(mtb.UseSystemPasswordChar);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// MaskedTextBox — TextMaskFormat, UnmaskedText, FormattedText, MaskFull
// ════════════════════════════════════════════════════════════════════════════════
public class MaskedTextBoxFormatTests
{
    [Fact]
    public void UnmaskedText_ReturnsRawEditableChars()
    {
        var mtb = new MaskedTextBox { Mask = "000-00" };
        mtb.Text = "12345";
        Assert.Equal("12345", mtb.UnmaskedText);
    }

    [Fact]
    public void MaskedText_IncludesLiterals()
    {
        var mtb = new MaskedTextBox { Mask = "000-00" };
        mtb.Text = "12345";
        Assert.Contains("-", mtb.MaskedText);
    }

    [Fact]
    public void MaskFull_TrueWhenAllRequiredPositionsFilled()
    {
        var mtb = new MaskedTextBox { Mask = "000" };
        mtb.Text = "123";
        Assert.True(mtb.MaskFull);
    }

    [Fact]
    public void MaskFull_FalseWhenRequiredPositionsNotFilled()
    {
        var mtb = new MaskedTextBox { Mask = "000" };
        mtb.Text = "12";
        Assert.False(mtb.MaskFull);
    }

    [Fact]
    public void TextMaskFormat_DefaultIsIncludeLiterals()
    {
        var mtb = new MaskedTextBox();
        Assert.Equal(MaskFormat.IncludeLiterals, mtb.TextMaskFormat);
    }

    [Fact]
    public void TextMaskFormat_RoundTrips()
    {
        var mtb = new MaskedTextBox();
        mtb.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        Assert.Equal(MaskFormat.ExcludePromptAndLiterals, mtb.TextMaskFormat);
    }

    [Fact]
    public void FormattedText_IncludeLiterals_ContainsLiteralChars()
    {
        var mtb = new MaskedTextBox { Mask = "000-00", TextMaskFormat = MaskFormat.IncludeLiterals };
        mtb.Text = "12345";
        Assert.Contains("-", mtb.FormattedText);
    }

    [Fact]
    public void FormattedText_ExcludePromptAndLiterals_ReturnsRawChars()
    {
        var mtb = new MaskedTextBox { Mask = "000-00", TextMaskFormat = MaskFormat.ExcludePromptAndLiterals };
        mtb.Text = "123";
        Assert.Equal("123", mtb.FormattedText);
    }

    [Fact]
    public void FormattedText_IncludePromptAndLiterals_ContainsPromptChars()
    {
        var mtb = new MaskedTextBox { Mask = "000", TextMaskFormat = MaskFormat.IncludePromptAndLiterals };
        mtb.Text = "1";
        Assert.Contains("_", mtb.FormattedText);
    }

    [Fact]
    public void MaskChanged_EventFires_OnMaskAssignment()
    {
        var mtb = new MaskedTextBox();
        bool fired = false;
        mtb.MaskChanged += (_, _) => fired = true;
        mtb.Mask = "000";
        Assert.True(fired);
    }
}
