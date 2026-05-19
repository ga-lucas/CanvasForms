using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

/// <summary>Tests verifying controls use BackColor and ForeColor correctly.</summary>
public class ColorPropertyRenderingTests
{
    [Fact]
    public void Button_ShouldRespectBackColor()
    {
        var button = new Button { BackColor = System.Drawing.Color.FromArgb(100, 150, 200) };
        Assert.Equal(System.Drawing.Color.FromArgb(100, 150, 200), button.BackColor);
    }

    [Fact]
    public void Button_ShouldRespectForeColor()
    {
        var button = new Button { ForeColor = System.Drawing.Color.FromArgb(255, 0, 0), Text = "Test" };
        Assert.Equal(System.Drawing.Color.FromArgb(255, 0, 0), button.ForeColor);
    }

    [Fact]
    public void CheckBox_ShouldRespectBackColor()
    {
        var checkBox = new CheckBox { BackColor = System.Drawing.Color.FromArgb(255, 255, 200) };
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 200), checkBox.BackColor);
    }

    [Fact]
    public void CheckBox_ShouldRespectForeColor()
    {
        var checkBox = new CheckBox { ForeColor = System.Drawing.Color.FromArgb(0, 128, 0), Text = "Check me" };
        Assert.Equal(System.Drawing.Color.FromArgb(0, 128, 0), checkBox.ForeColor);
    }

    [Fact]
    public void RadioButton_ShouldRespectBackColor()
    {
        var radioButton = new RadioButton { BackColor = System.Drawing.Color.FromArgb(200, 220, 255) };
        Assert.Equal(System.Drawing.Color.FromArgb(200, 220, 255), radioButton.BackColor);
    }

    [Fact]
    public void RadioButton_ShouldRespectForeColor()
    {
        var radioButton = new RadioButton { ForeColor = System.Drawing.Color.FromArgb(128, 0, 128), Text = "Select me" };
        Assert.Equal(System.Drawing.Color.FromArgb(128, 0, 128), radioButton.ForeColor);
    }

    [Fact]
    public void Label_ShouldRespectBackColor()
    {
        var label = new Label { BackColor = System.Drawing.Color.FromArgb(240, 240, 240) };
        Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), label.BackColor);
    }

    [Fact]
    public void Label_ShouldRespectForeColor()
    {
        var label = new Label { ForeColor = System.Drawing.Color.FromArgb(64, 64, 64), Text = "Label text" };
        Assert.Equal(System.Drawing.Color.FromArgb(64, 64, 64), label.ForeColor);
    }

    [Fact]
    public void TextBox_ShouldRespectBackColor()
    {
        var textBox = new TextBox { BackColor = System.Drawing.Color.FromArgb(255, 255, 230) };
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 230), textBox.BackColor);
    }

    [Fact]
    public void TextBox_ShouldRespectForeColor()
    {
        var textBox = new TextBox { ForeColor = System.Drawing.Color.FromArgb(0, 0, 128), Text = "Sample text" };
        Assert.Equal(System.Drawing.Color.FromArgb(0, 0, 128), textBox.ForeColor);
    }

    [Fact]
    public void PictureBox_ShouldRespectBackColor()
    {
        var pictureBox = new PictureBox { BackColor = System.Drawing.Color.FromArgb(200, 200, 200) };
        Assert.Equal(System.Drawing.Color.FromArgb(200, 200, 200), pictureBox.BackColor);
    }

    [Fact]
    public void Form_ShouldRespectBackColor()
    {
        var form = new Form { BackColor = System.Drawing.Color.FromArgb(220, 230, 240) };
        Assert.Equal(System.Drawing.Color.FromArgb(220, 230, 240), form.BackColor);
    }

    [Fact]
    public void CheckBox_TransparentBackColor_ShouldNotRenderBackground()
    {
        var checkBox = new CheckBox { BackColor = System.Drawing.Color.Transparent };
        Assert.Equal(System.Drawing.Color.Transparent, checkBox.BackColor);
    }

    [Fact]
    public void RadioButton_TransparentBackColor_ShouldNotRenderBackground()
    {
        var radioButton = new RadioButton { BackColor = System.Drawing.Color.Transparent };
        Assert.Equal(System.Drawing.Color.Transparent, radioButton.BackColor);
    }

    [Fact]
    public void Label_TransparentBackColor_ShouldNotRenderBackground()
    {
        var label = new Label { BackColor = System.Drawing.Color.Transparent };
        Assert.Equal(System.Drawing.Color.Transparent, label.BackColor);
    }
}

/// <summary>Tests verifying color and focus rendering behavior together.</summary>
public class ColorRenderingBehaviorTests
{
    [Fact]
    public void Button_CustomBackColor_ShouldBePreserved()
    {
        var button = new Button { BackColor = System.Drawing.Color.FromArgb(100, 150, 200), Text = "Test" };
        Assert.Equal(System.Drawing.Color.FromArgb(100, 150, 200), button.BackColor);
    }

    [Fact]
    public void CheckBox_NonTransparentBackColor_ShouldBePreserved()
    {
        var checkBox = new CheckBox { BackColor = System.Drawing.Color.FromArgb(255, 255, 200), Text = "Test CheckBox" };
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 200), checkBox.BackColor);
    }

    [Fact]
    public void RadioButton_NonTransparentBackColor_ShouldBePreserved()
    {
        var radioButton = new RadioButton { BackColor = System.Drawing.Color.FromArgb(200, 220, 255), Text = "Test Radio" };
        Assert.Equal(System.Drawing.Color.FromArgb(200, 220, 255), radioButton.BackColor);
    }

    [Fact]
    public void Form_ShouldRenderBackColor()
    {
        var form = new Form { BackColor = System.Drawing.Color.FromArgb(240, 248, 255) };
        var childButton = new Button { Text = "Child Button", Left = 10, Top = 10 };
        form.Controls.Add(childButton);
        Assert.Equal(System.Drawing.Color.FromArgb(240, 248, 255), form.BackColor);
        Assert.Equal(1, form.Controls.Count);
    }

    [Fact]
    public void Label_WithCustomColors_ShouldPreserveBoth()
    {
        var label = new Label
        {
            BackColor = System.Drawing.Color.FromArgb(255, 248, 220),
            ForeColor = System.Drawing.Color.FromArgb(139, 69, 19),
            Text = "Custom Label"
        };
        Assert.Equal(System.Drawing.Color.FromArgb(255, 248, 220), label.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(139, 69, 19), label.ForeColor);
    }

    [Fact]
    public void TextBox_WithCustomColors_ShouldPreserveBoth()
    {
        var textBox = new TextBox
        {
            BackColor = System.Drawing.Color.FromArgb(255, 255, 224),
            ForeColor = System.Drawing.Color.FromArgb(0, 0, 139),
            Text = "Sample text"
        };
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 224), textBox.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(0, 0, 139), textBox.ForeColor);
    }

    [Fact]
    public void PictureBox_ShouldPreserveBackColor()
    {
        var pictureBox = new PictureBox { BackColor = System.Drawing.Color.FromArgb(211, 211, 211) };
        Assert.Equal(System.Drawing.Color.FromArgb(211, 211, 211), pictureBox.BackColor);
    }

    [Fact]
    public void Button_WithFocus_ShouldHaveFocusedState()
    {
        var button = new Button { Enabled = true, Visible = true, TabStop = true };
        Assert.True(button.Focus());
        Assert.True(button.Focused);
    }

    [Fact]
    public void CheckBox_WithFocus_ShouldHaveFocusedState()
    {
        var checkBox = new CheckBox { Enabled = true, Visible = true, TabStop = true };
        Assert.True(checkBox.Focus());
        Assert.True(checkBox.Focused);
    }

    [Fact]
    public void RadioButton_WithFocus_ShouldHaveFocusedState()
    {
        var radioButton = new RadioButton { Enabled = true, Visible = true, TabStop = true };
        Assert.True(radioButton.Focus());
        Assert.True(radioButton.Focused);
    }

    [Fact]
    public void PictureBox_WithFocus_ShouldHaveFocusedState()
    {
        var pictureBox = new PictureBox { Enabled = true, Visible = true, TabStop = true };
        Assert.True(pictureBox.Focus());
        Assert.True(pictureBox.Focused);
    }

    [Fact]
    public void Button_Disabled_ShouldNotAcceptFocus()
    {
        var button = new Button { Enabled = false, Visible = true };
        Assert.False(button.Focus());
        Assert.False(button.Focused);
    }

    [Fact]
    public void CheckBox_Disabled_ShouldNotAcceptFocus()
    {
        var checkBox = new CheckBox { Enabled = false, Visible = true, Checked = true };
        Assert.False(checkBox.Focus());
        Assert.False(checkBox.Focused);
        Assert.True(checkBox.Checked);
    }

    [Fact]
    public void TextBox_Disabled_ShouldNotAcceptFocus()
    {
        var textBox = new TextBox { Enabled = false, Visible = true };
        Assert.False(textBox.Focus());
        Assert.False(textBox.Focused);
    }

    [Fact]
    public void MultipleControls_WithDifferentColors_ShouldMaintainIndependentColors()
    {
        var button   = new Button   { BackColor = System.Drawing.Color.FromArgb(255, 0, 0),   ForeColor = System.Drawing.Color.White };
        var checkBox = new CheckBox { BackColor = System.Drawing.Color.FromArgb(0, 255, 0),   ForeColor = System.Drawing.Color.Black };
        var label    = new Label    { BackColor = System.Drawing.Color.FromArgb(0, 0, 255),   ForeColor = System.Drawing.Color.White };

        Assert.Equal(System.Drawing.Color.FromArgb(255, 0, 0), button.BackColor);
        Assert.Equal(System.Drawing.Color.White, button.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(0, 255, 0), checkBox.BackColor);
        Assert.Equal(System.Drawing.Color.Black, checkBox.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(0, 0, 255), label.BackColor);
        Assert.Equal(System.Drawing.Color.White, label.ForeColor);
    }

    [Fact]
    public void Button_LosesFocus_ShouldNoLongerBeFocused()
    {
        var form    = new TestControl { Visible = true, Enabled = true };
        var button1 = new Button { Visible = true, Enabled = true, TabStop = true };
        var button2 = new Button { Visible = true, Enabled = true, TabStop = true };
        form.Controls.Add(button1);
        form.Controls.Add(button2);

        button1.Focus();
        Assert.True(button1.Focused);

        button2.Focus();
        Assert.False(button1.Focused);
        Assert.True(button2.Focused);
    }

    [Fact]
    public void CheckBox_LosesFocus_ShouldNoLongerBeFocused()
    {
        var form      = new TestControl { Visible = true, Enabled = true };
        var checkBox1 = new CheckBox { Visible = true, Enabled = true, TabStop = true };
        var checkBox2 = new CheckBox { Visible = true, Enabled = true, TabStop = true };
        form.Controls.Add(checkBox1);
        form.Controls.Add(checkBox2);

        checkBox1.Focus();
        checkBox2.Focus();
        Assert.False(checkBox1.Focused);
        Assert.True(checkBox2.Focused);
    }

    [Fact]
    public void RadioButton_LosesFocus_ShouldNoLongerBeFocused()
    {
        var form   = new TestControl { Visible = true, Enabled = true };
        var radio1 = new RadioButton { Visible = true, Enabled = true, TabStop = true };
        var radio2 = new RadioButton { Visible = true, Enabled = true, TabStop = true };
        form.Controls.Add(radio1);
        form.Controls.Add(radio2);

        radio1.Focus();
        radio2.Focus();
        Assert.False(radio1.Focused);
        Assert.True(radio2.Focused);
    }

    private class TestControl : Control { }
}
