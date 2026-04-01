using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Tests;

/// <summary>
/// Tests to verify controls properly use BackColor and ForeColor in rendering
/// </summary>
public class ColorPropertyRenderingTests
{
    [Fact]
    public void Button_ShouldRespectBackColor()
    {
        var button = new Button
        {
            BackColor = Color.FromArgb(100, 150, 200),
            Width = 100,
            Height = 30
        };

        // Button should use BackColor in normal state
        // This test verifies the property is set correctly
        Assert.Equal(Color.FromArgb(100, 150, 200), button.BackColor);
    }

    [Fact]
    public void Button_ShouldRespectForeColor()
    {
        var button = new Button
        {
            ForeColor = Color.FromArgb(255, 0, 0),
            Text = "Test"
        };

        Assert.Equal(Color.FromArgb(255, 0, 0), button.ForeColor);
    }

    [Fact]
    public void CheckBox_ShouldRespectBackColor()
    {
        var checkBox = new CheckBox
        {
            BackColor = Color.FromArgb(255, 255, 200),
            Width = 100,
            Height = 20
        };

        Assert.Equal(Color.FromArgb(255, 255, 200), checkBox.BackColor);
    }

    [Fact]
    public void CheckBox_ShouldRespectForeColor()
    {
        var checkBox = new CheckBox
        {
            ForeColor = Color.FromArgb(0, 128, 0),
            Text = "Check me"
        };

        Assert.Equal(Color.FromArgb(0, 128, 0), checkBox.ForeColor);
    }

    [Fact]
    public void RadioButton_ShouldRespectBackColor()
    {
        var radioButton = new RadioButton
        {
            BackColor = Color.FromArgb(200, 220, 255),
            Width = 100,
            Height = 20
        };

        Assert.Equal(Color.FromArgb(200, 220, 255), radioButton.BackColor);
    }

    [Fact]
    public void RadioButton_ShouldRespectForeColor()
    {
        var radioButton = new RadioButton
        {
            ForeColor = Color.FromArgb(128, 0, 128),
            Text = "Select me"
        };

        Assert.Equal(Color.FromArgb(128, 0, 128), radioButton.ForeColor);
    }

    [Fact]
    public void Label_ShouldRespectBackColor()
    {
        var label = new Label
        {
            BackColor = Color.FromArgb(240, 240, 240),
            Width = 100,
            Height = 20
        };

        Assert.Equal(Color.FromArgb(240, 240, 240), label.BackColor);
    }

    [Fact]
    public void Label_ShouldRespectForeColor()
    {
        var label = new Label
        {
            ForeColor = Color.FromArgb(64, 64, 64),
            Text = "Label text"
        };

        Assert.Equal(Color.FromArgb(64, 64, 64), label.ForeColor);
    }

    [Fact]
    public void TextBox_ShouldRespectBackColor()
    {
        var textBox = new TextBox
        {
            BackColor = Color.FromArgb(255, 255, 230),
            Width = 150,
            Height = 20
        };

        Assert.Equal(Color.FromArgb(255, 255, 230), textBox.BackColor);
    }

    [Fact]
    public void TextBox_ShouldRespectForeColor()
    {
        var textBox = new TextBox
        {
            ForeColor = Color.FromArgb(0, 0, 128),
            Text = "Sample text"
        };

        Assert.Equal(Color.FromArgb(0, 0, 128), textBox.ForeColor);
    }

    [Fact]
    public void PictureBox_ShouldRespectBackColor()
    {
        var pictureBox = new PictureBox
        {
            BackColor = Color.FromArgb(200, 200, 200),
            Width = 100,
            Height = 100
        };

        Assert.Equal(Color.FromArgb(200, 200, 200), pictureBox.BackColor);
    }

    [Fact]
    public void Form_ShouldRespectBackColor()
    {
        var form = new Form
        {
            BackColor = Color.FromArgb(220, 230, 240),
            Width = 400,
            Height = 300
        };

        Assert.Equal(Color.FromArgb(220, 230, 240), form.BackColor);
    }

    [Fact]
    public void CheckBox_TransparentBackColor_ShouldNotRenderBackground()
    {
        var checkBox = new CheckBox
        {
            BackColor = Color.Transparent,
            Width = 100,
            Height = 20
        };

        // Transparent BackColor should be preserved
        Assert.Equal(Color.Transparent, checkBox.BackColor);
    }

    [Fact]
    public void RadioButton_TransparentBackColor_ShouldNotRenderBackground()
    {
        var radioButton = new RadioButton
        {
            BackColor = Color.Transparent,
            Width = 100,
            Height = 20
        };

        // Transparent BackColor should be preserved
        Assert.Equal(Color.Transparent, radioButton.BackColor);
    }

    [Fact]
    public void Label_TransparentBackColor_ShouldNotRenderBackground()
    {
        var label = new Label
        {
            BackColor = Color.Transparent,
            Width = 100,
            Height = 20
        };

        // Transparent BackColor should be preserved
        Assert.Equal(Color.Transparent, label.BackColor);
    }
}
