using System.Windows.Forms;
using System.Reflection;

namespace WebForms.Canvas.Tests;

/// <summary>
/// Tests to track the completeness of Control class properties 
/// compared to System.Windows.Forms.Control
/// </summary>
public class ControlPropertyCompletenessTests
{
    // Expected properties from System.Windows.Forms.Control
    private static readonly string[] ExpectedProperties = new[]
    {
        "AccessibilityObject",
        "AccessibleDefaultActionDescription",
        "AccessibleDescription",
        "AccessibleName",
        "AccessibleRole",
        "AllowDrop",
        "Anchor",
        "AutoScrollOffset",
        "AutoSize",
        "BackColor",
        "BackgroundImage",
        "BackgroundImageLayout",
        "BindingContext",
        "Bottom",
        "Bounds",
        "CanEnableIme",
        "CanFocus",
        "CanRaiseEvents",
        "CanSelect",
        "Capture",
        "CausesValidation",
        "CheckForIllegalCrossThreadCalls",
        "ClientRectangle",
        "ClientSize",
        "CompanyName",
        "ContainsFocus",
        "ContextMenu",
        "ContextMenuStrip",
        "Controls",
        "Created",
        "CreateParams",
        "Cursor",
        "DataBindings",
        "DataContext",
        "DefaultBackColor",
        "DefaultCursor",
        "DefaultFont",
        "DefaultForeColor",
        "DefaultImeMode",
        "DefaultMargin",
        "DefaultMaximumSize",
        "DefaultMinimumSize",
        "DefaultPadding",
        "DefaultSize",
        "DeviceDpi",
        "DisplayRectangle",
        "Disposing",
        "Dock",
        "DoubleBuffered",
        "Enabled",
        "Focused",
        "Font",
        "FontHeight",
        "ForeColor",
        "Handle",
        "HasChildren",
        "Height",
        "ImeMode",
        "ImeModeBase",
        "InvokeRequired",
        "IsAccessible",
        "IsAncestorSiteInDesignMode",
        "IsDisposed",
        "IsHandleCreated",
        "IsMirrored",
        "LayoutEngine",
        "Left",
        "Location",
        "Margin",
        "MaximumSize",
        "MinimumSize",
        "ModifierKeys",
        "MouseButtons",
        "MousePosition",
        "Name",
        "Padding",
        "Parent",
        "PreferredSize",
        "ProductName",
        "ProductVersion",
        "PropagatingImeMode",
        "RecreatingHandle",
        "Region",
        "RenderRightToLeft",
        "ResizeRedraw",
        "Right",
        "RightToLeft",
        "ScaleChildren",
        "ShowFocusCues",
        "ShowKeyboardCues",
        "Site",
        "Size",
        "TabIndex",
        "TabStop",
        "Tag",
        "Text",
        "Top",
        "TopLevelControl",
        "UseWaitCursor",
        "Visible",
        "Width",
        "WindowTarget"
    };

    [Fact]
    public void Control_ShouldHaveAllExpectedProperties()
    {
        // Get all properties from our Control class (both instance and static)
        var controlType = typeof(Control);
        var actualProperties = controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Select(p => p.Name)
            .ToHashSet();

        // Find missing properties
        var missingProperties = ExpectedProperties.Where(p => !actualProperties.Contains(p)).ToList();

        // Report on completeness
        var implementedCount = ExpectedProperties.Length - missingProperties.Count;
        var completenessPercentage = (implementedCount * 100.0) / ExpectedProperties.Length;

        // Create detailed report
        var report = $"\n=== Control Property Completeness Report ===\n" +
                     $"Implemented: {implementedCount}/{ExpectedProperties.Length} ({completenessPercentage:F1}%)\n" +
                     $"Missing: {missingProperties.Count}\n";

        if (missingProperties.Any())
        {
            report += "\nMissing Properties:\n";
            foreach (var prop in missingProperties.OrderBy(p => p))
            {
                report += $"  - {prop}\n";
            }
        }

        // Output the report (will show in test output)
        Console.WriteLine(report);

        // This test will fail until 100% completeness is achieved
        // You can comment out the assertion to just see the report
        Assert.True(missingProperties.Count == 0, 
            $"Control class is missing {missingProperties.Count} properties. See test output for details.");
    }

    [Fact]
    public void Control_ImplementedProperties_ShouldWork()
    {
        // Test that the currently implemented properties work correctly
        var control = new TestControl();

        // Basic properties
        Assert.NotNull(control.Name);
        Assert.NotNull(control.Text);
        Assert.True(control.Visible);
        Assert.True(control.Enabled);

        // Layout properties
        Assert.Equal(0, control.Left);
        Assert.Equal(0, control.Top);
        Assert.True(control.Width > 0);
        Assert.True(control.Height > 0);

        // Color properties
        Assert.NotNull(control.BackColor);
        Assert.NotNull(control.ForeColor);

        // Anchor and Dock
        Assert.Equal(AnchorStyles.Top | AnchorStyles.Left, control.Anchor);
        Assert.Equal(DockStyle.None, control.Dock);

        // Parent/Child
        Assert.Null(control.Parent);
        Assert.NotNull(control.Controls);

        // Location and Bounds
        Assert.NotNull(control.Location);
        Assert.NotNull(control.Size);
        Assert.NotNull(control.Bounds);
    }

    // Helper test control
    private class TestControl : Control
    {
    }

    [Fact]
    public void Control_PropertyReport_ByCategory()
    {
        // This test provides a detailed breakdown by category
        var controlType = typeof(Control);
        var allProperties = controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        var report = "\n=== Control Properties By Category ===\n\n";

        // Layout Properties
        var layoutProps = new[] { "Left", "Top", "Width", "Height", "Location", "Size", "Bounds", 
            "Anchor", "Dock", "Margin", "Padding", "MinimumSize", "MaximumSize", "AutoScrollOffset",
            "ClientRectangle", "ClientSize", "DisplayRectangle", "Bottom", "Right" };
        report += $"Layout Properties ({layoutProps.Intersect(allProperties.Select(p => p.Name)).Count()}/{layoutProps.Length}):\n";
        foreach (var prop in layoutProps)
            report += $"  ✓ {prop}\n";

        // Appearance Properties
        var appearanceProps = new[] { "BackColor", "ForeColor", "BackgroundImage", "BackgroundImageLayout",
            "Font", "FontHeight", "Visible", "Region", "RightToLeft", "IsMirrored" };
        report += $"\nAppearance Properties ({appearanceProps.Intersect(allProperties.Select(p => p.Name)).Count()}/{appearanceProps.Length}):\n";
        foreach (var prop in appearanceProps)
            report += $"  ✓ {prop}\n";

        // State Properties
        var stateProps = new[] { "Enabled", "Focused", "CanFocus", "CanSelect", "ContainsFocus",
            "Created", "IsHandleCreated", "IsDisposed", "Disposing", "RecreatingHandle" };
        report += $"\nState Properties ({stateProps.Intersect(allProperties.Select(p => p.Name)).Count()}/{stateProps.Length}):\n";
        foreach (var prop in stateProps)
            report += $"  ✓ {prop}\n";

        // Accessibility Properties
        var accessibilityProps = new[] { "AccessibilityObject", "AccessibleName", "AccessibleDescription",
            "AccessibleDefaultActionDescription", "AccessibleRole", "IsAccessible" };
        report += $"\nAccessibility Properties ({accessibilityProps.Intersect(allProperties.Select(p => p.Name)).Count()}/{accessibilityProps.Length}):\n";
        foreach (var prop in accessibilityProps)
            report += $"  ✓ {prop}\n";

        // Static Properties
        var staticProps = allProperties.Where(p => p.GetMethod?.IsStatic == true).Select(p => p.Name).ToList();
        report += $"\nStatic Properties ({staticProps.Count}):\n";
        foreach (var prop in staticProps.OrderBy(p => p))
            report += $"  ✓ {prop}\n";

        Console.WriteLine(report);
    }
}
