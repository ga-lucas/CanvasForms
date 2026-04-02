# Windows Forms Controls Implementation Strategy

## Executive Summary

With the complete Control base class now in place, we can implement Windows Forms controls **10x more efficiently** using a layered, template-based approach. This document provides a strategic roadmap for implementing controls to enable compiled Windows Forms apps to run in the canvas environment.

## Current State Analysis

### ✅ Already Implemented (Partial)
- **Button** - Good implementation with hover/press states
- **TextBox** - Partial implementation
- **Label** - Basic implementation
- **CheckBox** - Basic implementation  
- **RadioButton** - Basic implementation
- **PictureBox** - Basic implementation

### 🎯 Foundation Complete
- **Control** - 100% API complete with all events/methods
- **Form** - Full implementation with windowing
- **Graphics** - Canvas rendering system
- **Event System** - Complete mouse/keyboard/focus handling

## New Efficient Implementation Strategy

### 1. Create Control Templates

Instead of reimplementing everything, create **base templates** that common controls inherit from:

#### ButtonBase Template
```csharp
public abstract class ButtonBase : Control
{
    // Shared button functionality
    protected bool IsPressed { get; set; }
    protected bool IsHovered { get; set; }
    protected bool IsDefault { get; set; }

    public FlatStyle FlatStyle { get; set; } = FlatStyle.Standard;
    public ContentAlignment TextAlign { get; set; } = ContentAlignment.MiddleCenter;
    public Image? Image { get; set; }
    public ContentAlignment ImageAlign { get; set; } = ContentAlignment.MiddleCenter;

    protected ButtonBase()
    {
        SetStyle(ControlStyles.Selectable | ControlStyles.StandardClick, true);
    }

    // Common button event handling
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
        {
            IsPressed = true;
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (IsPressed && e.Button == MouseButtons.Left)
        {
            IsPressed = false;
            Invalidate();
            if (ClientRectangle.Contains(e.X, e.Y))
            {
                OnClick(EventArgs.Empty);
            }
        }
        base.OnMouseUp(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        IsHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        IsHovered = false;
        IsPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            IsPressed = true;
            Invalidate();
        }
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            IsPressed = false;
            OnClick(EventArgs.Empty);
            Invalidate();
        }
        base.OnKeyUp(e);
    }
}
```

Then controls become **trivial**:

```csharp
public class Button : ButtonBase
{
    public Button()
    {
        Width = 75;
        Height = 23;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var state = GetVisualState();

        // Use shared renderer
        ButtonRenderer.DrawButton(g, ClientRectangle, Text, Font, 
            state, FlatStyle, BackColor, ForeColor);

        base.OnPaint(e);
    }
}

public class CheckBox : ButtonBase
{
    public bool Checked { get; set; }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        OnCheckedChanged(EventArgs.Empty);
        base.OnClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        CheckBoxRenderer.DrawCheckBox(e.Graphics, ClientRectangle, 
            Text, Font, Checked, GetVisualState());
        base.OnPaint(e);
    }
}
```

#### TextBoxBase Template
```csharp
public abstract class TextBoxBase : Control
{
    protected string _text = "";
    protected int _selectionStart = 0;
    protected int _selectionLength = 0;
    protected int _scrollOffset = 0;

    public virtual int SelectionStart { get; set; }
    public virtual int SelectionLength { get; set; }
    public virtual string SelectedText { get; set; }

    public bool ReadOnly { get; set; }
    public int MaxLength { get; set; } = 32767;
    public bool Multiline { get; set; }
    public char PasswordChar { get; set; }

    // Common text editing logic
    protected virtual void HandleKeyPress(char c) { }
    protected virtual void HandleBackspace() { }
    protected virtual void HandleDelete() { }
    protected virtual void Select(int start, int length) { }
}
```

### 2. Create Rendering Helpers

Centralize rendering logic in **static renderer classes**:

```csharp
public static class ButtonRenderer
{
    public static void DrawButton(Graphics g, Rectangle bounds, string text,
        Font font, ButtonState state, FlatStyle style, 
        Color backColor, Color foreColor)
    {
        // Centralized button rendering
        switch (style)
        {
            case FlatStyle.Flat:
                DrawFlatButton(g, bounds, text, font, state, backColor, foreColor);
                break;
            case FlatStyle.Popup:
                DrawPopupButton(g, bounds, text, font, state, backColor, foreColor);
                break;
            default:
                DrawStandardButton(g, bounds, text, font, state, backColor, foreColor);
                break;
        }
    }

    private static void DrawStandardButton(/*...*/) { /* ... */ }
    private static void DrawFlatButton(/*...*/) { /* ... */ }
    // etc.
}

public static class TextBoxRenderer { /* ... */ }
public static class CheckBoxRenderer { /* ... */ }
public static class ComboBoxRenderer { /* ... */ }
```

### 3. Create Control Generators

Use **source generators** or **T4 templates** for repetitive controls:

```csharp
// Template file: Control.tt
<#@ template language="C#" #>
<#@ parameter name="ControlName" type="System.String" #>
<#@ parameter name="BaseClass" type="System.String" #>
<#@ parameter name="DefaultWidth" type="System.Int32" #>
<#@ parameter name="DefaultHeight" type="System.Int32" #>

namespace WebForms.Canvas.Forms;

public class <#= ControlName #> : <#= BaseClass #>
{
    public <#= ControlName #>()
    {
        Width = <#= DefaultWidth #>;
        Height = <#= DefaultHeight #>;
    }

    // Auto-generated properties from Windows Forms API
<# foreach (var prop in GetProperties(ControlName)) { #>
    public <#= prop.Type #> <#= prop.Name #> { get; set; }
<# } #>
}
```

## Recommended Implementation Priority

### Phase 1: Core Controls (1-2 weeks)
These enable 80% of Windows Forms apps:

1. **Panel** ⭐ - Container control
   ```csharp
   public class Panel : ScrollableControl
   {
       // Just needs border styles + auto-scroll
   }
   ```

2. **GroupBox** ⭐ - Visual grouping
   ```csharp
   public class GroupBox : Panel
   {
       // Panel + title bar rendering
   }
   ```

3. **ListBox** ⭐ - Item selection
4. **ComboBox** ⭐ - Dropdown selection
5. **DateTimePicker** ⭐ - Date input
6. **NumericUpDown** ⭐ - Number input

### Phase 2: Data Controls (2-3 weeks)
For data-heavy apps:

7. **DataGridView** ⭐⭐⭐ - Most complex but most important
8. **TreeView** - Hierarchical data
9. **ListView** - Multi-column lists
10. **ProgressBar** - Progress indication
11. **TrackBar** - Slider control

### Phase 3: Menus & Toolbars (1-2 weeks)
12. **MenuStrip**
13. **ToolStrip**
14. **StatusStrip**
15. **ContextMenuStrip**

### Phase 4: Containers (1 week)
16. **TabControl**
17. **SplitContainer**
18. **FlowLayoutPanel**
19. **TableLayoutPanel**

### Phase 5: Advanced (2-3 weeks)
20. **RichTextBox**
21. **WebBrowser** (iframe wrapper)
22. **PrintPreviewControl**

## Simplified Implementation Pattern

### Step-by-Step for Each Control

1. **Define the class** (5 minutes)
   ```csharp
   public class MyControl : ButtonBase  // Or appropriate base
   {
       public MyControl() 
       {
           // Set defaults
       }
   }
   ```

2. **Add properties** (10 minutes)
   - Copy from System.Windows.Forms API
   - Use auto-properties initially
   - Add change notifications where needed

3. **Override OnPaint** (20-30 minutes)
   ```csharp
   protected override void OnPaint(PaintEventArgs e)
   {
       MyControlRenderer.Draw(e.Graphics, this);
       base.OnPaint(e);
   }
   ```

4. **Handle events** (10-20 minutes)
   - Override relevant On* methods from Control base
   - Most logic already in base classes

5. **Test** (10 minutes)
   - Create sample form
   - Verify rendering and interaction

**Total per control: 1-2 hours** (vs 8-10 hours before)

## Implementation Accelerators

### 1. API Compatibility Analyzer

Create a tool to compare your controls to System.Windows.Forms:

```csharp
public class ControlAPIAnalyzer
{
    public static Report CompareToWindowsForms(Type canvasControl)
    {
        var wfType = Type.GetType($"System.Windows.Forms.{canvasControl.Name}");

        var missing = new List<string>();
        foreach (var prop in wfType.GetProperties())
        {
            if (canvasControl.GetProperty(prop.Name) == null)
                missing.Add($"Property: {prop.Name}");
        }

        // Same for methods, events

        return new Report { MissingMembers = missing };
    }
}
```

### 2. Renderer Library

Create a shared rendering library:

```csharp
public static class ControlRenderers
{
    public static class Borders
    {
        public static void DrawNone(Graphics g, Rectangle r) { }
        public static void DrawFixed3D(Graphics g, Rectangle r) { }
        public static void DrawFixedSingle(Graphics g, Rectangle r) { }
    }

    public static class Text
    {
        public static void DrawCentered(Graphics g, string text, Font font, 
            Brush brush, Rectangle bounds) { }
        public static void DrawAligned(Graphics g, string text, Font font,
            Brush brush, Rectangle bounds, ContentAlignment align) { }
    }

    public static class Focus
    {
        public static void DrawFocusRectangle(Graphics g, Rectangle r) { }
    }
}
```

### 3. Property Bag Pattern

For complex controls, use a property bag:

```csharp
public class DataGridView : Control
{
    // Instead of implementing 200+ properties manually
    private PropertyBag _properties = new();

    public DataGridViewCellStyle DefaultCellStyle
    {
        get => _properties.Get<DataGridViewCellStyle>();
        set => _properties.Set(value);
    }

    // PropertyBag handles change notifications, defaults, etc.
}
```

## Code Generation Template

### Auto-Generate Control Stubs

```bash
# PowerShell script to generate control stubs
param([string]$ControlName, [string]$BaseClass = "Control")

$template = @"
using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

/// <summary>
/// Represents a Windows Forms $ControlName control
/// </summary>
public class $ControlName : $BaseClass
{
    public $ControlName()
    {
        // TODO: Set default size
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // TODO: Implement rendering
        base.OnPaint(e);
    }
}
"@

$template | Out-File "Forms\$ControlName.cs"
```

## Testing Strategy

### Unit Tests Per Control
```csharp
[TestFixture]
public class ButtonTests
{
    [Test]
    public void Button_Click_RaisesEvent()
    {
        var button = new Button();
        bool clicked = false;
        button.Click += (s, e) => clicked = true;

        button.PerformClick();  // Simulates click

        Assert.IsTrue(clicked);
    }

    [Test]
    public void Button_Disabled_DoesNotClick()
    {
        var button = new Button { Enabled = false };
        bool clicked = false;
        button.Click += (s, e) => clicked = true;

        button.PerformClick();

        Assert.IsFalse(clicked);
    }
}
```

### Integration Tests
```csharp
[Test]
public void Form_WithControls_RendersCorrectly()
{
    var form = new Form();
    form.Controls.Add(new Button { Text = "OK" });
    form.Controls.Add(new TextBox { Text = "Hello" });
    form.Controls.Add(new Label { Text = "Name:" });

    // Verify all controls render
    Assert.AreEqual(3, form.Controls.Count);
}
```

## API Coverage Goals

### Minimum Viable Product (MVP)
- **Properties**: 60% coverage
- **Methods**: 40% coverage  
- **Events**: 80% coverage (already have from Control)

### Full Coverage
- **Properties**: 90% coverage
- **Methods**: 70% coverage
- **Events**: 100% coverage

## Estimated Timeline

### With New Strategy

| Phase | Controls | Time | People |
|-------|----------|------|--------|
| Phase 1 | Core 6 controls | 2 weeks | 1-2 |
| Phase 2 | Data controls | 3 weeks | 2-3 |
| Phase 3 | Menus & toolbars | 2 weeks | 1-2 |
| Phase 4 | Containers | 1 week | 1 |
| Phase 5 | Advanced | 3 weeks | 2-3 |
| **Total** | **~25 controls** | **11 weeks** | **2-3 avg** |

### Old Approach (for comparison)
- Same controls: 6-9 months
- More developers needed

### Efficiency Gain: **60-80% faster**

## Migration Guide for Windows Forms Apps

Once controls are implemented, migration is straightforward:

```csharp
// Windows Forms app
namespace MyWinFormsApp
{
    public class MainForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button btnOK;
        // ...
    }
}

// Canvas app - just change the using!
namespace MyCanvasApp
{
    using WebForms.Canvas.Forms;  // ← Only change

    public class MainForm : Form
    {
        private Button btnOK;
        // Everything else stays the same!
        // ...
    }
}
```

## Recommended Next Steps

### Immediate (This Week)
1. ✅ Create `ButtonBase` class
2. ✅ Create `TextBoxBase` class
3. ✅ Create renderer helpers
4. ✅ Refactor existing Button/CheckBox/RadioButton to use ButtonBase

### Short Term (Next 2 Weeks)
5. ✅ Implement Panel
6. ✅ Implement ListBox
7. ✅ Implement ComboBox
8. ✅ Create control generator scripts

### Medium Term (Next Month)
9. ✅ Implement DataGridView (basic)
10. ✅ Implement MenuStrip
11. ✅ Implement TabControl

### Long Term (Next Quarter)
12. ✅ Full DataGridView
13. ✅ All Phase 1-4 controls
14. ✅ Sample apps demonstrating migration

## Success Metrics

- ✅ Can run simple Windows Forms app without modification (except using statement)
- ✅ 90% of common controls available
- ✅ Performance acceptable (60 FPS rendering)
- ✅ Developer experience matches Windows Forms

## Conclusion

With the complete Control base class, implementing controls is now:
- **10x faster** - Template-based approach
- **More consistent** - Shared base classes and renderers
- **Higher quality** - Centralized, tested code
- **Easier to maintain** - Less duplication

**The goal of emulating compiled Windows Forms apps is now achievable in 2-3 months with this strategy!**
