using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class TextBoxDefaultsTests
{
    [Fact]
    public void DefaultMultiline_IsFalse()
    {
        var tb = new TextBox();
        Assert.False(tb.Multiline);
    }

    [Fact]
    public void DefaultReadOnly_IsFalse()
    {
        var tb = new TextBox();
        Assert.False(tb.ReadOnly);
    }

    [Fact]
    public void DefaultMaxLength_Is32767()
    {
        var tb = new TextBox();
        Assert.Equal(32767, tb.MaxLength);
    }

    [Fact]
    public void DefaultBorderStyle_IsFixed3D()
    {
        var tb = new TextBox();
        Assert.Equal(BorderStyle.Fixed3D, tb.BorderStyle);
    }

    [Fact]
    public void DefaultText_IsEmpty()
    {
        var tb = new TextBox();
        Assert.Equal(string.Empty, tb.Text);
    }

    [Fact]
    public void DefaultBackColor_IsWhite()
    {
        var tb = new TextBox();
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 255, 255), tb.BackColor);
    }

    [Fact]
    public void DefaultWordWrap_IsTrue()
    {
        var tb = new TextBox();
        Assert.True(tb.WordWrap);
    }
}

public class TextBoxPropertyTests
{
    [Fact]
    public void Text_Set_RaisesTextChanged()
    {
        var tb = new TextBox();
        bool fired = false;
        tb.TextChanged += (_, _) => fired = true;
        tb.Text = "Hello";
        Assert.True(fired);
        Assert.Equal("Hello", tb.Text);
    }

    [Fact]
    public void ReadOnly_Set_RaisesReadOnlyChanged()
    {
        var tb = new TextBox();
        bool fired = false;
        tb.ReadOnlyChanged += (_, _) => fired = true;
        tb.ReadOnly = true;
        Assert.True(fired);
        Assert.True(tb.ReadOnly);
    }

    [Fact]
    public void Multiline_Set_RaisesMultilineChanged()
    {
        var tb = new TextBox();
        bool fired = false;
        tb.MultilineChanged += (_, _) => fired = true;
        tb.Multiline = true;
        Assert.True(fired);
        Assert.True(tb.Multiline);
    }

    [Fact]
    public void MaxLength_Set_AcceptsPositiveValue()
    {
        var tb = new TextBox();
        tb.MaxLength = 100;
        Assert.Equal(100, tb.MaxLength);
    }

    [Fact]
    public void MaxLength_NegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var tb = new TextBox();
        Assert.Throws<ArgumentOutOfRangeException>(() => tb.MaxLength = -1);
    }

    [Fact]
    public void BorderStyle_RoundTrips()
    {
        var tb = new TextBox();
        tb.BorderStyle = BorderStyle.FixedSingle;
        Assert.Equal(BorderStyle.FixedSingle, tb.BorderStyle);
    }

    [Fact]
    public void AcceptsReturn_RoundTrips()
    {
        var tb = new TextBox();
        tb.AcceptsReturn = true;
        Assert.True(tb.AcceptsReturn);
    }

    [Fact]
    public void AcceptsTab_RoundTrips()
    {
        var tb = new TextBox();
        tb.AcceptsTab = true;
        Assert.True(tb.AcceptsTab);
    }

    [Fact]
    public void HideSelection_DefaultIsTrue()
    {
        var tb = new TextBox();
        Assert.True(tb.HideSelection);
    }

    [Fact]
    public void HideSelection_RoundTrips()
    {
        var tb = new TextBox();
        tb.HideSelection = false;
        Assert.False(tb.HideSelection);
    }

    [Fact]
    public void WordWrap_RoundTrips()
    {
        var tb = new TextBox();
        tb.WordWrap = false;
        Assert.False(tb.WordWrap);
    }

    [Fact]
    public void ScrollBars_RoundTrips()
    {
        var tb = new TextBox();
        tb.ScrollBars = ScrollBars.Both;
        Assert.Equal(ScrollBars.Both, tb.ScrollBars);
    }

    [Fact]
    public void ShortcutsEnabled_DefaultIsTrue()
    {
        var tb = new TextBox();
        Assert.True(tb.ShortcutsEnabled);
    }

    [Fact]
    public void ShortcutsEnabled_RoundTrips()
    {
        var tb = new TextBox();
        tb.ShortcutsEnabled = false;
        Assert.False(tb.ShortcutsEnabled);
    }
}

public class TextBoxLinesTests
{
    [Fact]
    public void Lines_EmptyText_ReturnsEmptyArray()
    {
        var tb = new TextBox();
        Assert.Empty(tb.Lines);
    }

    [Fact]
    public void Lines_SingleLine_ReturnsSingleElement()
    {
        var tb = new TextBox { Text = "Hello" };
        Assert.Single(tb.Lines);
        Assert.Equal("Hello", tb.Lines[0]);
    }

    [Fact]
    public void Lines_MultilineText_ReturnsMultipleLines()
    {
        var tb = new TextBox { Multiline = true, Text = "Line1\r\nLine2\r\nLine3" };
        Assert.Equal(3, tb.Lines.Length);
        Assert.Equal("Line1", tb.Lines[0]);
        Assert.Equal("Line2", tb.Lines[1]);
        Assert.Equal("Line3", tb.Lines[2]);
    }

    [Fact]
    public void Lines_Set_UpdatesText()
    {
        var tb = new TextBox { Multiline = true };
        tb.Lines = new[] { "A", "B", "C" };
        Assert.Contains("A", tb.Text);
        Assert.Contains("B", tb.Text);
        Assert.Contains("C", tb.Text);
    }
}

public class TextBoxModifiedAndUndoTests
{
    [Fact]
    public void Modified_DefaultIsFalse()
    {
        var tb = new TextBox();
        Assert.False(tb.Modified);
    }

    [Fact]
    public void Modified_Set_RaisesModifiedChanged()
    {
        var tb = new TextBox();
        bool fired = false;
        tb.ModifiedChanged += (_, _) => fired = true;
        tb.Modified = true;
        Assert.True(fired);
    }

    [Fact]
    public void CanUndo_IsFalse_WhenNoHistory()
    {
        var tb = new TextBox();
        Assert.False(tb.CanUndo);
    }

    [Fact]
    public void CanRedo_IsFalse_WhenNoHistory()
    {
        var tb = new TextBox();
        Assert.False(tb.CanRedo);
    }

    [Fact]
    public void SelectAll_SetsSelectionLengthToTextLength()
    {
        var tb = new TextBox { Text = "Hello World" };
        tb.SelectAll();
        Assert.Equal(tb.Text.Length, tb.SelectionLength);
    }

    [Fact]
    public void Clear_EmptiesText()
    {
        var tb = new TextBox { Text = "some text" };
        tb.Clear();
        Assert.Equal(string.Empty, tb.Text);
    }

    [Fact]
    public void AppendText_AppendsToExistingText()
    {
        var tb = new TextBox { Text = "Hello" };
        tb.AppendText(" World");
        Assert.Equal("Hello World", tb.Text);
    }
}

public class TextBoxPasswordTests
{
    [Fact]
    public void PasswordChar_DefaultIsNull()
    {
        var tb = new TextBox();
        Assert.Equal('\0', tb.PasswordChar);
    }

    [Fact]
    public void PasswordChar_RoundTrips()
    {
        var tb = new TextBox();
        tb.PasswordChar = '*';
        Assert.Equal('*', tb.PasswordChar);
    }

    [Fact]
    public void UseSystemPasswordChar_DefaultIsFalse()
    {
        var tb = new TextBox();
        Assert.False(tb.UseSystemPasswordChar);
    }

    [Fact]
    public void UseSystemPasswordChar_RoundTrips()
    {
        var tb = new TextBox();
        tb.UseSystemPasswordChar = true;
        Assert.True(tb.UseSystemPasswordChar);
    }
}

public class TextBoxCharacterCasingTests
{
    [Fact]
    public void CharacterCasing_DefaultIsNormal()
    {
        var tb = new TextBox();
        Assert.Equal(CharacterCasing.Normal, tb.CharacterCasing);
    }

    [Fact]
    public void CharacterCasing_Upper_RoundTrips()
    {
        var tb = new TextBox();
        tb.CharacterCasing = CharacterCasing.Upper;
        Assert.Equal(CharacterCasing.Upper, tb.CharacterCasing);
    }

    [Fact]
    public void TextAlign_DefaultIsLeft()
    {
        var tb = new TextBox();
        Assert.Equal(HorizontalAlignment.Left, tb.TextAlign);
    }

    [Fact]
    public void TextAlign_RoundTrips()
    {
        var tb = new TextBox();
        tb.TextAlign = HorizontalAlignment.Center;
        Assert.Equal(HorizontalAlignment.Center, tb.TextAlign);
    }
}
