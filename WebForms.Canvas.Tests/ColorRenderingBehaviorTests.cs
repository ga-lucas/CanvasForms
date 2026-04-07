using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Tests;

/// <summary>
/// Tests to verify color rendering behavior and focus interaction
/// </summary>
public class ColorRenderingBehaviorTests
{
    [Fact]
    public void Button_CustomBackColor_ShouldBePreserved()
    {
        var button = new Button
        {
            BackColor = Color.FromArgb(100, 150, 200),
            Text = "Test",
            Width = 100,
            Height = 30,
            Enabled = true
        };

        // Button should have the custom BackColor set and preserved
        Assert.Equal(Color.FromArgb(100, 150, 200), button.BackColor);
    }

    [Fact]
    public void CheckBox_NonTransparentBackColor_ShouldBePreserved()
    {
        var checkBox = new CheckBox
        {
            BackColor = Color.FromArgb(255, 255, 200),
            Text = "Test CheckBox",
            Width = 150,
            Height = 20,
            Enabled = true
        };

        Assert.Equal(Color.FromArgb(255, 255, 200), checkBox.BackColor);
    }

    [Fact]
    public void RadioButton_NonTransparentBackColor_ShouldBePreserved()
    {
        var radioButton = new RadioButton
        {
            BackColor = Color.FromArgb(200, 220, 255),
            Text = "Test Radio",
            Width = 150,
            Height = 20,
            Enabled = true
        };

        Assert.Equal(Color.FromArgb(200, 220, 255), radioButton.BackColor);
    }

    [Fact]
    public void Form_ShouldRenderBackColor()
    {
        var form = new Form
        {
            BackColor = Color.FromArgb(240, 248, 255),
            Width = 400,
            Height = 300
        };

        var childButton = new Button
        {
            Text = "Child Button",
            Left = 10,
            Top = 10
        };
        form.Controls.Add(childButton);

        Assert.Equal(Color.FromArgb(240, 248, 255), form.BackColor);
        Assert.Equal(1, form.Controls.Count);
    }

    [Fact]
    public void Label_WithCustomColors_ShouldPreserveBoth()
    {
        var label = new Label
        {
            BackColor = Color.FromArgb(255, 248, 220),
            ForeColor = Color.FromArgb(139, 69, 19),
            Text = "Custom Label",
            Width = 100,
            Height = 20
        };

        Assert.Equal(Color.FromArgb(255, 248, 220), label.BackColor);
        Assert.Equal(Color.FromArgb(139, 69, 19), label.ForeColor);
    }

    [Fact]
    public void TextBox_WithCustomColors_ShouldPreserveBoth()
    {
        var textBox = new TextBox
        {
            BackColor = Color.FromArgb(255, 255, 224),
            ForeColor = Color.FromArgb(0, 0, 139),
            Text = "Sample text",
            Width = 200,
            Height = 20,
            Enabled = true
        };

        Assert.Equal(Color.FromArgb(255, 255, 224), textBox.BackColor);
        Assert.Equal(Color.FromArgb(0, 0, 139), textBox.ForeColor);
    }

    [Fact]
    public void PictureBox_ShouldPreserveBackColor()
    {
        var pictureBox = new PictureBox
        {
            BackColor = Color.FromArgb(211, 211, 211),
            Width = 100,
            Height = 100
        };

        Assert.Equal(Color.FromArgb(211, 211, 211), pictureBox.BackColor);
    }

    [Fact]
    public void Button_WithFocus_ShouldHaveFocusedState()
    {
        var button = new Button
        {
            Text = "Focused Button",
            Width = 100,
            Height = 30,
            Enabled = true,
            Visible = true,
            TabStop = true
        };

        // Set focus
        var result = button.Focus();

        Assert.True(result);
        Assert.True(button.Focused);
    }

    [Fact]
    public void CheckBox_WithFocus_ShouldHaveFocusedState()
    {
        var checkBox = new CheckBox
        {
            Text = "Focused CheckBox",
            Width = 150,
            Height = 20,
            Enabled = true,
            Visible = true,
            TabStop = true
        };

        // Set focus
        var result = checkBox.Focus();

        Assert.True(result);
        Assert.True(checkBox.Focused);
    }

    [Fact]
    public void RadioButton_WithFocus_ShouldHaveFocusedState()
    {
        var radioButton = new RadioButton
        {
            Text = "Focused Radio",
            Width = 150,
            Height = 20,
            Enabled = true,
            Visible = true,
            TabStop = true
        };

        // Set focus
        var result = radioButton.Focus();

        Assert.True(result);
        Assert.True(radioButton.Focused);
    }

    [Fact]
    public void PictureBox_WithFocus_ShouldHaveFocusedState()
    {
        var pictureBox = new PictureBox
        {
            Width = 100,
            Height = 100,
            Enabled = true,
            Visible = true,
            TabStop = true
        };

        // Set focus
        var result = pictureBox.Focus();

        Assert.True(result);
        Assert.True(pictureBox.Focused);
    }

    [Fact]
    public void Button_Disabled_ShouldNotAcceptFocus()
    {
        var button = new Button
        {
            Text = "Disabled",
            Width = 100,
            Height = 30,
            Enabled = false,
            Visible = true,
            TabStop = true
        };

        var result = button.Focus();

        Assert.False(result);
        Assert.False(button.Focused);
    }

    [Fact]
    public void CheckBox_Disabled_ShouldNotAcceptFocus()
    {
        var checkBox = new CheckBox
        {
            Text = "Disabled",
            Width = 150,
            Height = 20,
            Enabled = false,
            Visible = true,
            TabStop = true,
            Checked = true
        };

        var result = checkBox.Focus();

        Assert.False(result);
        Assert.False(checkBox.Focused);
        Assert.True(checkBox.Checked);
    }

    [Fact]
    public void TextBox_Disabled_ShouldNotAcceptFocus()
    {
        var textBox = new TextBox
        {
            Text = "Disabled text",
            Width = 200,
            Height = 20,
            Enabled = false,
            Visible = true,
            TabStop = true
        };

        var result = textBox.Focus();

        Assert.False(result);
        Assert.False(textBox.Focused);
    }

    [Fact]
    public void MultipleControls_WithDifferentColors_ShouldMaintainIndependentColors()
    {
        var button = new Button { BackColor = Color.FromArgb(255, 0, 0), ForeColor = Color.White };
        var checkBox = new CheckBox { BackColor = Color.FromArgb(0, 255, 0), ForeColor = Color.Black };
        var label = new Label { BackColor = Color.FromArgb(0, 0, 255), ForeColor = Color.White };

        Assert.Equal(Color.FromArgb(255, 0, 0), button.BackColor);
        Assert.Equal(Color.White, button.ForeColor);
        Assert.Equal(Color.FromArgb(0, 255, 0), checkBox.BackColor);
        Assert.Equal(Color.Black, checkBox.ForeColor);
        Assert.Equal(Color.FromArgb(0, 0, 255), label.BackColor);
        Assert.Equal(Color.White, label.ForeColor);
    }

    [Fact]
    public void Button_LosesFocus_ShouldNoLongerBeFocused()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var button1 = new Button { Visible = true, Enabled = true, TabStop = true };
        var button2 = new Button { Visible = true, Enabled = true, TabStop = true };

        form.Controls.Add(button1);
        form.Controls.Add(button2);

        button1.Focus();
        Assert.True(button1.Focused);
        Assert.False(button2.Focused);

        button2.Focus();
        Assert.False(button1.Focused);
        Assert.True(button2.Focused);
    }

    [Fact]
    public void CheckBox_LosesFocus_ShouldNoLongerBeFocused()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var checkBox1 = new CheckBox { Visible = true, Enabled = true, TabStop = true };
        var checkBox2 = new CheckBox { Visible = true, Enabled = true, TabStop = true };

        form.Controls.Add(checkBox1);
        form.Controls.Add(checkBox2);

        checkBox1.Focus();
        Assert.True(checkBox1.Focused);
        Assert.False(checkBox2.Focused);

        checkBox2.Focus();
        Assert.False(checkBox1.Focused);
        Assert.True(checkBox2.Focused);
    }

    [Fact]
    public void RadioButton_LosesFocus_ShouldNoLongerBeFocused()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var radio1 = new RadioButton { Visible = true, Enabled = true, TabStop = true };
        var radio2 = new RadioButton { Visible = true, Enabled = true, TabStop = true };

        form.Controls.Add(radio1);
        form.Controls.Add(radio2);

        radio1.Focus();
        Assert.True(radio1.Focused);
        Assert.False(radio2.Focused);

        radio2.Focus();
        Assert.False(radio1.Focused);
        Assert.True(radio2.Focused);
    }

    // Helper test control
    private class TestControl : Control
    {
    }
}

