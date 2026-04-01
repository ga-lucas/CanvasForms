using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Tests;

/// <summary>
/// Integration tests demonstrating the complete color and focus system improvements
/// </summary>
public class ColorAndFocusIntegrationTests
{
    [Fact]
    public void CompleteScenario_FormWithColoredControls_ShouldWorkCorrectly()
    {
        // Create a form with custom background
        var form = new Form
        {
            BackColor = Color.FromArgb(240, 248, 255), // Alice Blue
            Width = 400,
            Height = 300,
            Text = "Test Form"
        };

        // Add a button with custom colors
        var button = new Button
        {
            BackColor = Color.FromArgb(100, 150, 200),
            ForeColor = Color.White,
            Text = "Click Me",
            Left = 10,
            Top = 10,
            Width = 100,
            Height = 30,
            TabIndex = 0
        };

        // Add a checkbox with custom colors
        var checkBox = new CheckBox
        {
            BackColor = Color.FromArgb(255, 255, 200),
            ForeColor = Color.Black,
            Text = "Check This",
            Left = 10,
            Top = 50,
            Width = 150,
            Height = 20,
            TabIndex = 1
        };

        // Add a radio button with custom colors
        var radioButton = new RadioButton
        {
            BackColor = Color.FromArgb(200, 220, 255),
            ForeColor = Color.FromArgb(0, 0, 128),
            Text = "Select This",
            Left = 10,
            Top = 80,
            Width = 150,
            Height = 20,
            TabIndex = 2
        };

        // Add a textbox with custom colors
        var textBox = new TextBox
        {
            BackColor = Color.FromArgb(255, 255, 224),
            ForeColor = Color.FromArgb(0, 0, 139),
            Text = "Type here",
            Left = 10,
            Top = 110,
            Width = 200,
            Height = 20,
            TabIndex = 3
        };

        // Add a label with custom colors
        var label = new Label
        {
            BackColor = Color.FromArgb(255, 248, 220),
            ForeColor = Color.FromArgb(139, 69, 19),
            Text = "Instructions:",
            Left = 10,
            Top = 140,
            Width = 150,
            Height = 20
        };

        // Add all controls to form
        form.Controls.Add(button);
        form.Controls.Add(checkBox);
        form.Controls.Add(radioButton);
        form.Controls.Add(textBox);
        form.Controls.Add(label);

        // Verify all controls were added
        Assert.Equal(5, form.Controls.Count);

        // Verify form background color
        Assert.Equal(Color.FromArgb(240, 248, 255), form.BackColor);

        // Verify all control colors are preserved
        Assert.Equal(Color.FromArgb(100, 150, 200), button.BackColor);
        Assert.Equal(Color.White, button.ForeColor);
        Assert.Equal(Color.FromArgb(255, 255, 200), checkBox.BackColor);
        Assert.Equal(Color.Black, checkBox.ForeColor);
        Assert.Equal(Color.FromArgb(200, 220, 255), radioButton.BackColor);
        Assert.Equal(Color.FromArgb(0, 0, 128), radioButton.ForeColor);
        Assert.Equal(Color.FromArgb(255, 255, 224), textBox.BackColor);
        Assert.Equal(Color.FromArgb(0, 0, 139), textBox.ForeColor);
        Assert.Equal(Color.FromArgb(255, 248, 220), label.BackColor);
        Assert.Equal(Color.FromArgb(139, 69, 19), label.ForeColor);
    }

    [Fact]
    public void CompleteScenario_TabNavigationWithFocus_ShouldWork()
    {
        var form = new TestControl { Visible = true, Enabled = true };

        var button = new Button
        {
            Text = "Button",
            Visible = true,
            Enabled = true,
            TabStop = true,
            TabIndex = 0
        };

        var checkBox = new CheckBox
        {
            Text = "CheckBox",
            Visible = true,
            Enabled = true,
            TabStop = true,
            TabIndex = 1
        };

        var radioButton = new RadioButton
        {
            Text = "RadioButton",
            Visible = true,
            Enabled = true,
            TabStop = true,
            TabIndex = 2
        };

        var textBox = new TextBox
        {
            Text = "TextBox",
            Visible = true,
            Enabled = true,
            TabStop = true,
            TabIndex = 3
        };

        form.Controls.Add(button);
        form.Controls.Add(checkBox);
        form.Controls.Add(radioButton);
        form.Controls.Add(textBox);

        // Initial focus on button
        button.Focus();
        Assert.True(button.Focused);
        Assert.False(checkBox.Focused);
        Assert.False(radioButton.Focused);
        Assert.False(textBox.Focused);

        // Move to checkbox
        form.SelectNextControl(button, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.False(button.Focused);
        Assert.True(checkBox.Focused);
        Assert.False(radioButton.Focused);
        Assert.False(textBox.Focused);

        // Move to radio button
        form.SelectNextControl(checkBox, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.False(button.Focused);
        Assert.False(checkBox.Focused);
        Assert.True(radioButton.Focused);
        Assert.False(textBox.Focused);

        // Move to textbox
        form.SelectNextControl(radioButton, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.False(button.Focused);
        Assert.False(checkBox.Focused);
        Assert.False(radioButton.Focused);
        Assert.True(textBox.Focused);

        // Wrap back to button
        form.SelectNextControl(textBox, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.True(button.Focused);
        Assert.False(checkBox.Focused);
        Assert.False(radioButton.Focused);
        Assert.False(textBox.Focused);
    }

    [Fact]
    public void CompleteScenario_ControlCollectionLinqOperations_ShouldWork()
    {
        var form = new Form();

        var button1 = new Button { Text = "Button 1", Name = "btn1" };
        var button2 = new Button { Text = "Button 2", Name = "btn2" };
        var checkBox = new CheckBox { Text = "CheckBox", Name = "chk1" };
        var radioButton = new RadioButton { Text = "RadioButton", Name = "rad1" };

        form.Controls.Add(button1);
        form.Controls.Add(button2);
        form.Controls.Add(checkBox);
        form.Controls.Add(radioButton);

        // Test Cast<Control>() - this was the original issue!
        var allControls = form.Controls.Cast<Control>().ToList();
        Assert.Equal(4, allControls.Count);

        // Test LINQ Where
        var buttons = form.Controls.Cast<Control>().Where(c => c is Button).ToList();
        Assert.Equal(2, buttons.Count);

        // Test LINQ Select
        var names = form.Controls.Cast<Control>().Select(c => c.Name).ToList();
        Assert.Contains("btn1", names);
        Assert.Contains("btn2", names);
        Assert.Contains("chk1", names);
        Assert.Contains("rad1", names);

        // Test LINQ Any
        var hasCheckBox = form.Controls.Cast<Control>().Any(c => c is CheckBox);
        Assert.True(hasCheckBox);

        // Test LINQ OfType
        var onlyButtons = form.Controls.OfType<Button>().ToList();
        Assert.Equal(2, onlyButtons.Count);
    }

    [Fact]
    public void CompleteScenario_ThemeableControlSet_ShouldMaintainIndependentColors()
    {
        // Simulate a "dark theme" set of controls
        var darkButton = new Button
        {
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.FromArgb(241, 241, 241),
            Text = "Dark Button"
        };

        var darkCheckBox = new CheckBox
        {
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.FromArgb(241, 241, 241),
            Text = "Dark CheckBox"
        };

        // Simulate a "light theme" set of controls
        var lightButton = new Button
        {
            BackColor = Color.FromArgb(240, 240, 240),
            ForeColor = Color.FromArgb(30, 30, 30),
            Text = "Light Button"
        };

        var lightCheckBox = new CheckBox
        {
            BackColor = Color.FromArgb(255, 255, 255),
            ForeColor = Color.FromArgb(0, 0, 0),
            Text = "Light CheckBox"
        };

        // Verify dark theme colors
        Assert.Equal(Color.FromArgb(45, 45, 48), darkButton.BackColor);
        Assert.Equal(Color.FromArgb(241, 241, 241), darkButton.ForeColor);
        Assert.Equal(Color.FromArgb(45, 45, 48), darkCheckBox.BackColor);
        Assert.Equal(Color.FromArgb(241, 241, 241), darkCheckBox.ForeColor);

        // Verify light theme colors
        Assert.Equal(Color.FromArgb(240, 240, 240), lightButton.BackColor);
        Assert.Equal(Color.FromArgb(30, 30, 30), lightButton.ForeColor);
        Assert.Equal(Color.FromArgb(255, 255, 255), lightCheckBox.BackColor);
        Assert.Equal(Color.FromArgb(0, 0, 0), lightCheckBox.ForeColor);
    }

    [Fact]
    public void CompleteScenario_MixedEnabledDisabledControls_ShouldHandleFocusCorrectly()
    {
        var form = new TestControl { Visible = true, Enabled = true };

        var button1 = new Button
        {
            Text = "Enabled",
            Visible = true,
            Enabled = true,
            TabStop = true,
            TabIndex = 0
        };

        var button2 = new Button
        {
            Text = "Disabled",
            Visible = true,
            Enabled = false, // Disabled
            TabStop = true,
            TabIndex = 1
        };

        var button3 = new Button
        {
            Text = "Enabled",
            Visible = true,
            Enabled = true,
            TabStop = true,
            TabIndex = 2
        };

        form.Controls.Add(button1);
        form.Controls.Add(button2);
        form.Controls.Add(button3);

        // Focus first enabled button
        button1.Focus();
        Assert.True(button1.Focused);

        // Try to focus disabled button (should fail)
        var result = button2.Focus();
        Assert.False(result);
        Assert.False(button2.Focused);
        Assert.True(button1.Focused); // First button should still have focus

        // Navigate forward - should skip disabled button
        form.SelectNextControl(button1, forward: true, tabStopOnly: true, nested: false, wrap: true);
        Assert.False(button1.Focused);
        Assert.False(button2.Focused);
        Assert.True(button3.Focused); // Should jump to button3
    }

    // Helper test control
    private class TestControl : Control
    {
    }
}
