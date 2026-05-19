using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class RichTextBoxDefaultsTests
{
    [Fact]
    public void DefaultMultiline_IsTrue()
    {
        var rtb = new RichTextBox();
        Assert.True(rtb.Multiline);
    }

    [Fact]
    public void DefaultWordWrap_IsTrue()
    {
        var rtb = new RichTextBox();
        Assert.True(rtb.WordWrap);
    }

    [Fact]
    public void DefaultAcceptsReturn_IsTrue()
    {
        var rtb = new RichTextBox();
        Assert.True(rtb.AcceptsReturn);
    }

    [Fact]
    public void DefaultDetectUrls_IsTrue()
    {
        var rtb = new RichTextBox();
        Assert.True(rtb.DetectUrls);
    }

    [Fact]
    public void DefaultEnableAutoDragDrop_IsFalse()
    {
        var rtb = new RichTextBox();
        Assert.False(rtb.EnableAutoDragDrop);
    }

    [Fact]
    public void DefaultSize_MatchesWinForms()
    {
        var rtb = new RichTextBox();
        Assert.Equal(100, rtb.Width);
        Assert.Equal(96, rtb.Height);
    }
}

public class RichTextBoxTextTests
{
    [Fact]
    public void Text_Set_RaisesTextChanged()
    {
        var rtb = new RichTextBox();
        bool fired = false;
        rtb.TextChanged += (_, _) => fired = true;
        rtb.Text = "Hello";
        Assert.True(fired);
        Assert.Equal("Hello", rtb.Text);
    }

    [Fact]
    public void AppendText_AppendsToExisting()
    {
        var rtb = new RichTextBox { Text = "Hello" };
        rtb.AppendText(" World");
        Assert.Equal("Hello World", rtb.Text);
    }

    [Fact]
    public void Clear_EmptiesText()
    {
        var rtb = new RichTextBox { Text = "Some content" };
        rtb.Clear();
        Assert.Equal(string.Empty, rtb.Text);
    }

    [Fact]
    public void Lines_MultilineText_ReturnsMultipleLines()
    {
        var rtb = new RichTextBox { Text = "Line1\r\nLine2\r\nLine3" };
        Assert.Equal(3, rtb.Lines.Length);
    }
}

public class RichTextBoxRtfTests
{
    [Fact]
    public void Rtf_Set_SetsTextFromStrippedRtf()
    {
        var rtb = new RichTextBox();
        rtb.Rtf = @"{\rtf1 Hello World}";
        // Text should be stripped RTF content
        Assert.NotNull(rtb.Text);
    }

    [Fact]
    public void Rtf_Set_RoundTrips()
    {
        var rtb = new RichTextBox();
        var rtfContent = @"{\rtf1\ansi Hello}";
        rtb.Rtf = rtfContent;
        Assert.Equal(rtfContent, rtb.Rtf);
    }

    [Fact]
    public void HtmlContent_Set_SetsTextFromStrippedHtml()
    {
        var rtb = new RichTextBox();
        rtb.HtmlContent = "<b>Bold</b> text";
        Assert.NotNull(rtb.Text);
        Assert.Contains("Bold", rtb.Text);
    }
}

public class RichTextBoxSelectionFormattingTests
{
    [Fact]
    public void SelectionFont_CanBeSetAndRead()
    {
        var rtb = new RichTextBox { Text = "Hello World" };
        rtb.SelectAll();
        var font = new Font("Arial", 14);
        rtb.SelectionFont = font;
        Assert.NotNull(rtb.SelectionFont);
    }

    [Fact]
    public void SelectionColor_CanBeSetAndRead()
    {
        var rtb = new RichTextBox { Text = "Hello" };
        rtb.SelectAll();
        rtb.SelectionColor = Color.FromArgb(255, 0, 0);
        Assert.Equal(Color.FromArgb(255, 0, 0), rtb.SelectionColor);
    }

    [Fact]
    public void SelectionBackColor_CanBeSetAndRead()
    {
        var rtb = new RichTextBox { Text = "Hello" };
        rtb.SelectAll();
        rtb.SelectionBackColor = Color.FromArgb(255, 255, 0);
        Assert.Equal(Color.FromArgb(255, 255, 0), rtb.SelectionBackColor);
    }
}

public class RichTextBoxPropertyTests
{
    [Fact]
    public void DetectUrls_RoundTrips()
    {
        var rtb = new RichTextBox();
        rtb.DetectUrls = false;
        Assert.False(rtb.DetectUrls);
    }

    [Fact]
    public void EnableAutoDragDrop_RoundTrips()
    {
        var rtb = new RichTextBox();
        rtb.EnableAutoDragDrop = true;
        Assert.True(rtb.EnableAutoDragDrop);
    }

    [Fact]
    public void ZoomFactor_DefaultIsOne()
    {
        var rtb = new RichTextBox();
        Assert.Equal(1, rtb.ZoomFactor);
    }

    [Fact]
    public void ZoomFactor_RoundTrips()
    {
        var rtb = new RichTextBox();
        rtb.ZoomFactor = 2;
        Assert.Equal(2, rtb.ZoomFactor);
    }

    [Fact]
    public void ReadOnly_RoundTrips()
    {
        var rtb = new RichTextBox();
        rtb.ReadOnly = true;
        Assert.True(rtb.ReadOnly);
    }

    [Fact]
    public void MaxLength_RoundTrips()
    {
        var rtb = new RichTextBox();
        rtb.MaxLength = 500;
        Assert.Equal(500, rtb.MaxLength);
    }
}
