using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Tests;

/// <summary>
/// Tests for focus management functionality
/// </summary>
public class FocusManagementTests
{
    [Fact]
    public void Focus_OnFocusableControl_ShouldSucceed()
    {
        var control = new TestControl
        {
            Visible = true,
            Enabled = true,
            TabStop = true
        };

        var result = control.Focus();

        Assert.True(result);
        Assert.True(control.Focused);
    }

    [Fact]
    public void Focus_OnDisabledControl_ShouldFail()
    {
        var control = new TestControl
        {
            Visible = true,
            Enabled = false,
            TabStop = true
        };

        var result = control.Focus();

        Assert.False(result);
        Assert.False(control.Focused);
    }

    [Fact]
    public void Focus_OnInvisibleControl_ShouldFail()
    {
        var control = new TestControl
        {
            Visible = false,
            Enabled = true,
            TabStop = true
        };

        var result = control.Focus();

        Assert.False(result);
        Assert.False(control.Focused);
    }

    [Fact]
    public void Focus_OnControlWithTabStopFalse_ShouldFail()
    {
        var control = new TestControl
        {
            Visible = true,
            Enabled = true,
            TabStop = false
        };

        var result = control.Focus();

        Assert.False(result);
        Assert.False(control.Focused);
    }

    [Fact]
    public void Focus_ShouldRaiseFocusEvents()
    {
        var control = new TestControl { Visible = true, Enabled = true, TabStop = true };

        bool enterRaised = false;
        bool gotFocusRaised = false;

        control.Enter += (s, e) => enterRaised = true;
        control.GotFocus += (s, e) => gotFocusRaised = true;

        control.Focus();

        Assert.True(enterRaised);
        Assert.True(gotFocusRaised);
    }

    [Fact]
    public void Focus_ShouldRemoveFocusFromPreviousControl()
    {
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true };
        var form = new TestControl { Visible = true, Enabled = true };

        form.Controls.Add(control1);
        form.Controls.Add(control2);

        control1.Focus();
        Assert.True(control1.Focused);
        Assert.False(control2.Focused);

        control2.Focus();
        Assert.False(control1.Focused);
        Assert.True(control2.Focused);
    }

    [Fact]
    public void Focus_ShouldRaiseLostFocusOnPreviousControl()
    {
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true };
        var form = new TestControl { Visible = true, Enabled = true };

        form.Controls.Add(control1);
        form.Controls.Add(control2);

        bool leaveCalled = false;
        bool lostFocusCalled = false;

        control1.Leave += (s, e) => leaveCalled = true;
        control1.LostFocus += (s, e) => lostFocusCalled = true;

        control1.Focus();
        control2.Focus();

        Assert.True(leaveCalled);
        Assert.True(lostFocusCalled);
    }

    [Fact]
    public void Select_ShouldCallFocus()
    {
        var control = new TestControl { Visible = true, Enabled = true, TabStop = true };

        control.Select();

        Assert.True(control.Focused);
    }

    [Fact]
    public void ContainsFocus_ShouldReturnTrueWhenControlIsFocused()
    {
        var control = new TestControl { Visible = true, Enabled = true, TabStop = true };

        control.Focus();

        Assert.True(control.ContainsFocus);
    }

    [Fact]
    public void ContainsFocus_ShouldReturnTrueWhenChildIsFocused()
    {
        var parent = new TestControl { Visible = true, Enabled = true };
        var child = new TestControl { Visible = true, Enabled = true, TabStop = true };

        parent.Controls.Add(child);
        child.Focus();

        Assert.True(parent.ContainsFocus);
        Assert.True(child.ContainsFocus);
    }

    [Fact]
    public void ContainsFocus_ShouldReturnFalseWhenNoFocus()
    {
        var control = new TestControl { Visible = true, Enabled = true, TabStop = true };

        Assert.False(control.ContainsFocus);
    }

    [Fact]
    public void SelectNextControl_ShouldMoveToNextControl()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        control1.Focus();
        var result = form.SelectNextControl(control1, forward: true, tabStopOnly: true, nested: false, wrap: true);

        Assert.True(result);
        Assert.False(control1.Focused);
        Assert.True(control2.Focused);
        Assert.False(control3.Focused);
    }

    [Fact]
    public void SelectNextControl_Backward_ShouldMoveToPreviousControl()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        control3.Focus();
        var result = form.SelectNextControl(control3, forward: false, tabStopOnly: true, nested: false, wrap: true);

        Assert.True(result);
        Assert.False(control1.Focused);
        Assert.True(control2.Focused);
        Assert.False(control3.Focused);
    }

    [Fact]
    public void SelectNextControl_WithWrap_ShouldWrapAround()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);

        control2.Focus();
        var result = form.SelectNextControl(control2, forward: true, tabStopOnly: true, nested: false, wrap: true);

        Assert.True(result);
        Assert.True(control1.Focused);
        Assert.False(control2.Focused);
    }

    [Fact]
    public void SelectNextControl_WithoutWrap_ShouldNotWrapAround()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 1 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);

        control2.Focus();
        var result = form.SelectNextControl(control2, forward: true, tabStopOnly: true, nested: false, wrap: false);

        Assert.False(result);
        Assert.False(control1.Focused);
        Assert.True(control2.Focused); // Should stay on current control
    }

    [Fact]
    public void SelectNextControl_ShouldSkipDisabledControls()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = false, TabStop = true, TabIndex = 1 };
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        control1.Focus();
        var result = form.SelectNextControl(control1, forward: true, tabStopOnly: true, nested: false, wrap: true);

        Assert.True(result);
        Assert.False(control1.Focused);
        Assert.False(control2.Focused); // Skipped because disabled
        Assert.True(control3.Focused);
    }

    [Fact]
    public void SelectNextControl_ShouldSkipInvisibleControls()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = false, Enabled = true, TabStop = true, TabIndex = 1 };
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        control1.Focus();
        var result = form.SelectNextControl(control1, forward: true, tabStopOnly: true, nested: false, wrap: true);

        Assert.True(result);
        Assert.False(control1.Focused);
        Assert.False(control2.Focused); // Skipped because invisible
        Assert.True(control3.Focused);
    }

    [Fact]
    public void SelectNextControl_WithTabStopFalse_ShouldBeSkippedWhenTabStopOnly()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = false, TabIndex = 1 };
        var control3 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(control2);
        form.Controls.Add(control3);

        control1.Focus();
        var result = form.SelectNextControl(control1, forward: true, tabStopOnly: true, nested: false, wrap: true);

        Assert.True(result);
        Assert.False(control1.Focused);
        Assert.False(control2.Focused); // Skipped because TabStop is false
        Assert.True(control3.Focused);
    }

    [Fact]
    public void SelectNextControl_WithNested_ShouldIncludeNestedControls()
    {
        var form = new TestControl { Visible = true, Enabled = true };
        var panel = new TestControl { Visible = true, Enabled = true, TabIndex = 1 };
        var control1 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var nestedControl = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 0 };
        var control2 = new TestControl { Visible = true, Enabled = true, TabStop = true, TabIndex = 2 };

        form.Controls.Add(control1);
        form.Controls.Add(panel);
        panel.Controls.Add(nestedControl);
        form.Controls.Add(control2);

        control1.Focus();
        var result = form.SelectNextControl(control1, forward: true, tabStopOnly: true, nested: true, wrap: true);

        // With nested=true, should navigate into child controls
        // The result should be true if any focusable control was found
        Assert.True(result);
        // Either the nested control or the next control in tab order should be focused
        Assert.True(nestedControl.Focused || panel.Controls.Cast<Control>().Any(c => c.Focused) || control2.Focused);
    }

    [Fact]
    public void CanFocus_ShouldConsiderVisibleEnabledAndTabStop()
    {
        var control = new TestControl { Visible = true, Enabled = true, TabStop = true };
        Assert.True(control.CanFocus);

        control.Visible = false;
        Assert.False(control.CanFocus);

        control.Visible = true;
        control.Enabled = false;
        Assert.False(control.CanFocus);

        control.Enabled = true;
        control.TabStop = false;
        Assert.False(control.CanFocus);
    }

    [Fact]
    public void CanSelect_ShouldMatchCanFocus()
    {
        var control = new TestControl { Visible = true, Enabled = true, TabStop = true };
        Assert.Equal(control.CanFocus, control.CanSelect);

        control.Enabled = false;
        Assert.Equal(control.CanFocus, control.CanSelect);
    }

    // Helper test control
    private class TestControl : Control
    {
    }
}
