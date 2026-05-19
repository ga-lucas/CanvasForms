using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

/// <summary>
/// Integration tests demonstrating the complete color and focus system.
/// </summary>
public class ColorAndFocusIntegrationTests
{
    [Fact]
    public void CompleteScenario_FormWithColoredControls_ShouldWorkCorrectly()
    {
        var form = new Form { BackColor = System.Drawing.Color.FromArgb(240, 248, 255), Width = 400, Height = 300, Text = "Test Form" };

        var button      = new Button      { BackColor = System.Drawing.Color.FromArgb(100, 150, 200), ForeColor = System.Drawing.Color.White,              Text = "Click Me",    Left = 10, Top = 10,  Width = 100, Height = 30, TabIndex = 0 };
        var checkBox    = new CheckBox    { BackColor = System.Drawing.Color.FromArgb(255, 255, 200), ForeColor = System.Drawing.Color.Black,              Text = "Check This",  Left = 10, Top = 50,  Width = 150, Height = 20, TabIndex = 1 };
        var radioButton = new RadioButton { BackColor = System.Drawing.Color.FromArgb(200, 220, 255), ForeColor = System.Drawing.Color.FromArgb(0, 0, 128), Text = "Select This", Left = 10, Top = 80,  Width = 150, Height = 20, TabIndex = 2 };
        var textBox     = new TextBox     { BackColor = System.Drawing.Color.FromArgb(255, 255, 224), ForeColor = System.Drawing.Color.FromArgb(0, 0, 139), Text = "Type here",   Left = 10, Top = 110, Width = 200, Height = 20, TabIndex = 3 };
        var label       = new Label       { BackColor = System.Drawing.Color.FromArgb(255, 248, 220), ForeColor = System.Drawing.Color.FromArgb(139, 69, 19), Text = "Instructions:", Left = 10, Top = 140, Width = 150, Height = 20 };

        form.Controls.Add(button); form.Controls.Add(checkBox); form.Controls.Add(radioButton); form.Controls.Add(textBox); form.Controls.Add(label);

        Assert.Equal(5, form.Controls.Count);
        Assert.Equal(System.Drawing.Color.FromArgb(240, 248, 255), form.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(100, 150, 200), button.BackColor);
        Assert.Equal(System.Drawing.Color.White, button.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 200), checkBox.BackColor);
        Assert.Equal(System.Drawing.Color.Black, checkBox.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(200, 220, 255), radioButton.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(0, 0, 128), radioButton.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 224), textBox.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(0, 0, 139), textBox.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(255, 248, 220), label.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(139, 69, 19), label.ForeColor);
    }

    [Fact]
    public void CompleteScenario_TabNavigationWithFocus_ShouldWork()
    {
        var form = new TestControl { Visible = true, Enabled = true };

        var button      = new Button      { Text = "Button",      Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var checkBox    = new CheckBox    { Text = "CheckBox",    Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };
        var radioButton = new RadioButton { Text = "RadioButton", Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };
        var textBox     = new TextBox     { Text = "TextBox",     Visible = true, Enabled = true, TabStop = true, TabIndex = 3 };

        form.Controls.Add(button); form.Controls.Add(checkBox); form.Controls.Add(radioButton); form.Controls.Add(textBox);

        button.Focus();
        Assert.True(button.Focused); Assert.False(checkBox.Focused); Assert.False(radioButton.Focused); Assert.False(textBox.Focused);

        form.SelectNextControl(button, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.False(button.Focused); Assert.True(checkBox.Focused);

        form.SelectNextControl(checkBox, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.True(radioButton.Focused);

        form.SelectNextControl(radioButton, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.True(textBox.Focused);

        form.SelectNextControl(textBox, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.True(button.Focused);
    }

    [Fact]
    public void CompleteScenario_ControlCollectionLinqOperations_ShouldWork()
    {
        var form = new Form();
        form.Controls.Add(new Button      { Name = "btn1", Text = "Button 1" });
        form.Controls.Add(new Button      { Name = "btn2", Text = "Button 2" });
        form.Controls.Add(new CheckBox    { Name = "chk1", Text = "CheckBox" });
        form.Controls.Add(new RadioButton { Name = "rad1", Text = "RadioButton" });

        var allControls = form.Controls.Cast<Control>().ToList();
        Assert.Equal(4, allControls.Count);

        var buttons = form.Controls.Cast<Control>().Where(c => c is Button).ToList();
        Assert.Equal(2, buttons.Count);

        var names = form.Controls.Cast<Control>().Select(c => c.Name).ToList();
        Assert.Contains("btn1", names); Assert.Contains("btn2", names); Assert.Contains("chk1", names); Assert.Contains("rad1", names);

        Assert.True(form.Controls.Cast<Control>().Any(c => c is CheckBox));
        Assert.Equal(2, form.Controls.OfType<Button>().Count());
    }

    [Fact]
    public void CompleteScenario_ThemeableControlSet_ShouldMaintainIndependentColors()
    {
        var darkButton   = new Button   { BackColor = System.Drawing.Color.FromArgb(45, 45, 48),   ForeColor = System.Drawing.Color.FromArgb(241, 241, 241) };
        var darkCheckBox = new CheckBox { BackColor = System.Drawing.Color.FromArgb(45, 45, 48),   ForeColor = System.Drawing.Color.FromArgb(241, 241, 241) };
        var lightButton  = new Button   { BackColor = System.Drawing.Color.FromArgb(240, 240, 240), ForeColor = System.Drawing.Color.FromArgb(30, 30, 30)    };
        var lightCheckBox = new CheckBox { BackColor = System.Drawing.Color.FromArgb(255, 255, 255), ForeColor = System.Drawing.Color.FromArgb(0, 0, 0)      };

        Assert.Equal(System.Drawing.Color.FromArgb(45, 45, 48),   darkButton.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(241, 241, 241), darkButton.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(45, 45, 48),   darkCheckBox.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(241, 241, 241), darkCheckBox.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), lightButton.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(30, 30, 30),   lightButton.ForeColor);
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 255), lightCheckBox.BackColor);
        Assert.Equal(System.Drawing.Color.FromArgb(0, 0, 0),      lightCheckBox.ForeColor);
    }

    [Fact]
    public void CompleteScenario_MixedEnabledDisabledControls_ShouldHandleFocusCorrectly()
    {
        var form    = new TestControl { Visible = true, Enabled = true };
        var button1 = new Button { Visible = true, Enabled = true,  TabStop = true, TabIndex = 0 };
        var button2 = new Button { Visible = true, Enabled = false, TabStop = true, TabIndex = 1 };
        var button3 = new Button { Visible = true, Enabled = true,  TabStop = true, TabIndex = 2 };
        form.Controls.Add(button1); form.Controls.Add(button2); form.Controls.Add(button3);

        button1.Focus();
        Assert.True(button1.Focused);

        Assert.False(button2.Focus());
        Assert.False(button2.Focused);
        Assert.True(button1.Focused);

        form.SelectNextControl(button1, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.False(button1.Focused); Assert.False(button2.Focused); Assert.True(button3.Focused);
    }

    private class TestControl : Control { }
}
