using System.Windows.Forms;

namespace Canvas.Windows.Forms.Tests;

/// <summary>
/// Tests for Tab key navigation functionality
/// </summary>
public class TabKeyNavigationTests
{
    [Fact]
    public void TabKey_ShouldMoveToNextControl()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        // Focus first control
        control1.Focus();
        Assert.True(control1.Focused);

        // Simulate Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        control1.SimulateKeyDown(keyArgs);

        // Should move to control2
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.True(control2.Focused);
        Assert.False(control3.Focused);
    }

    [Fact]
    public void ShiftTabKey_ShouldMoveToPreviousControl()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        // Focus third control
        control3.Focus();
        Assert.True(control3.Focused);

        // Simulate Shift+Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: true);
        control3.SimulateKeyDown(keyArgs);

        // Should move to control2
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.True(control2.Focused);
        Assert.False(control3.Focused);
    }

    [Fact]
    public void TabKey_AtLastControl_ShouldWrapToFirst()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);

        // Focus last control
        control2.Focus();
        Assert.True(control2.Focused);

        // Simulate Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        control2.SimulateKeyDown(keyArgs);

        // Should wrap to control1
        Assert.True(keyArgs.Handled);
        Assert.True(control1.Focused);
        Assert.False(control2.Focused);
    }

    [Fact]
    public void ShiftTabKey_AtFirstControl_ShouldWrapToLast()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);

        // Focus first control
        control1.Focus();
        Assert.True(control1.Focused);

        // Simulate Shift+Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: true);
        control1.SimulateKeyDown(keyArgs);

        // Should wrap to control2
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.True(control2.Focused);
    }

    [Fact]
    public void TabKey_ShouldSkipDisabledControls()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = false, TabStop = true, TabIndex = 1 }; // Disabled
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        // Focus first control
        control1.Focus();
        Assert.True(control1.Focused);

        // Simulate Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        control1.SimulateKeyDown(keyArgs);

        // Should skip disabled control2 and move to control3
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.False(control2.Focused);
        Assert.True(control3.Focused);
    }

    [Fact]
    public void TabKey_ShouldSkipInvisibleControls()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = false, Enabled = true, TabStop = true, TabIndex = 1 }; // Invisible
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        // Focus first control
        control1.Focus();
        Assert.True(control1.Focused);

        // Simulate Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        control1.SimulateKeyDown(keyArgs);

        // Should skip invisible control2 and move to control3
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.False(control2.Focused);
        Assert.True(control3.Focused);
    }

    [Fact]
    public void TabKey_ShouldSkipControlsWithTabStopFalse()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = false, TabIndex = 1 }; // TabStop = false
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        // Focus first control
        control1.Focus();
        Assert.True(control1.Focused);

        // Simulate Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        control1.SimulateKeyDown(keyArgs);

        // Should skip control2 (TabStop=false) and move to control3
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.False(control2.Focused);
        Assert.True(control3.Focused);
    }

    [Fact]
    public void TabKey_WithNestedControls_ShouldNavigateIntoChildren()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var panel = new TestControl { Visible = true, Enabled = true, TabIndex = 1 }; // Container
        var nestedControl = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(panel);
        panel.Controls.Add(nestedControl);
        form.Controls.Add(control2);

        // Focus first control
        control1.Focus();
        Assert.True(control1.Focused);

        // Simulate Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        control1.SimulateKeyDown(keyArgs);

        // Should navigate into panel and focus nestedControl
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.True(nestedControl.Focused);
        Assert.False(control2.Focused);
    }

    [Fact]
    public void TabKey_FromNestedControl_ShouldContinueToNextTopLevel()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var panel = new TestControl { Visible = true, Enabled = true, TabIndex = 1 }; // Container
        var nestedControl = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(panel);
        panel.Controls.Add(nestedControl);
        form.Controls.Add(control2);

        // Focus nested control
        nestedControl.Focus();
        Assert.True(nestedControl.Focused);

        // Simulate Tab key press
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        nestedControl.SimulateKeyDown(keyArgs);

        // Should move to control2
        Assert.True(keyArgs.Handled);
        Assert.False(control1.Focused);
        Assert.False(nestedControl.Focused);
        Assert.True(control2.Focused);
    }

    [Fact]
    public void NonTabKey_ShouldNotBeHandled()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };

        form.Controls.Add(control1);
        control1.Focus();

        // Simulate Enter key press
        var keyArgs = new KeyEventArgs(Keys.Enter, shift: false);
        control1.SimulateKeyDown(keyArgs);

        // Enter key should not be handled by tab navigation
        Assert.False(keyArgs.Handled);
        Assert.True(control1.Focused);
    }

    [Fact]
    public void TabKey_WithFormFocusedControl_ShouldNavigateCorrectly()
    {
        var form = new TestForm
        {
            Width = 400,
            Height = 300
        };

        var button1 = new Button
        {
            Text = "Button 1",
            TabIndex = 0,
            TabStop = true,
            Visible = true,
            Enabled = true
        };

        var button2 = new Button
        {
            Text = "Button 2",
            TabIndex = 1,
            TabStop = true,
            Visible = true,
            Enabled = true
        };

        form.Controls.Add(button1);
        form.Controls.Add(button2);

        // Focus first button
        button1.Focus();
        form.FocusedControl = button1;
        Assert.True(button1.Focused);

        // Simulate Tab key press through Form's KeyDown routing
        var keyArgs = new KeyEventArgs(Keys.Tab, shift: false);
        form.SimulateKeyDown(keyArgs);

        // Should move to button2
        Assert.True(keyArgs.Handled);
        Assert.False(button1.Focused);
        Assert.True(button2.Focused);
    }

    // Helper test control that exposes OnKeyDown for testing
    private class TestControl : Control
    {
        public void SimulateKeyDown(KeyEventArgs e) => OnKeyDown(e);
    }

    // Helper form that exposes OnKeyDown for testing
    private class TestForm : Form
    {
        public void SimulateKeyDown(KeyEventArgs e) => OnKeyDown(e);
    }
}
