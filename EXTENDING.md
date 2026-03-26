# Extending WebForms.Canvas

This guide explains how to add new drawing primitives and features to the WebForms.Canvas library.

## Adding a New Drawing Primitive

To add a new drawing command (e.g., drawing a polygon), follow these steps:

### 1. Create a Drawing Command Class

In `WebForms.Canvas\Drawing\DrawingCommands.cs`, add a new command class:

```csharp
public class DrawPolygonCommand : DrawingCommand
{
    public Pen Pen { get; }
    public Point[] Points { get; }

    public DrawPolygonCommand(Pen pen, Point[] points)
    {
        Pen = pen;
        Points = points;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ctx.strokeStyle = '{Pen.Color.ToRgbaString()}';");
        sb.AppendLine($"ctx.lineWidth = {Pen.Width};");
        sb.AppendLine("ctx.beginPath();");

        if (Points.Length > 0)
        {
            sb.AppendLine($"ctx.moveTo({Points[0].X}, {Points[0].Y});");
            for (int i = 1; i < Points.Length; i++)
            {
                sb.AppendLine($"ctx.lineTo({Points[i].X}, {Points[i].Y});");
            }
            sb.AppendLine("ctx.closePath();");
        }

        sb.AppendLine("ctx.stroke();");
        return sb.ToString();
    }
}
```

### 2. Add Methods to Graphics Class

In `WebForms.Canvas\Drawing\Graphics.cs`, add methods to use your new command:

```csharp
public void DrawPolygon(Pen pen, Point[] points)
{
    _commands.Add(new DrawPolygonCommand(pen, points));
}

public void FillPolygon(Brush brush, Point[] points)
{
    _commands.Add(new FillPolygonCommand(brush, points));
}
```

### 3. Test Your New Feature

Create a test in your sample form:

```csharp
private void OnPaint(object sender, PaintEventArgs e)
{
    var g = e.Graphics;
    using var pen = new Pen(Color.Blue, 2);

    var points = new Point[]
    {
        new Point(100, 50),
        new Point(150, 100),
        new Point(125, 150),
        new Point(75, 150),
        new Point(50, 100)
    };

    g.DrawPolygon(pen, points);
}
```

## Adding Event Support

To add mouse or keyboard events:

### 1. Define Event Arguments

Create a new file `WebForms.Canvas\Forms\MouseEventArgs.cs`:

```csharp
using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class MouseEventArgs : EventArgs
{
    public int X { get; }
    public int Y { get; }
    public MouseButtons Button { get; }

    public MouseEventArgs(int x, int y, MouseButtons button)
    {
        X = x;
        Y = y;
        Button = button;
    }

    public Point Location => new(X, Y);
}

public enum MouseButtons
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 4
}

public delegate void MouseEventHandler(object sender, MouseEventArgs e);
```

### 2. Add Events to Control Class

In `WebForms.Canvas\Forms\Control.cs`:

```csharp
public event MouseEventHandler? MouseClick;
public event MouseEventHandler? MouseMove;
public event MouseEventHandler? MouseDown;
public event MouseEventHandler? MouseUp;

protected internal virtual void OnMouseClick(MouseEventArgs e)
{
    MouseClick?.Invoke(this, e);
}

protected internal virtual void OnMouseMove(MouseEventArgs e)
{
    MouseMove?.Invoke(this, e);
}
```

### 3. Wire Up Events in FormRenderer

In `WebForms.Canvas\Components\FormRenderer.razor`:

```razor
<canvas @ref="_canvasRef" 
        width="@Form.Width" 
        height="@Form.Height"
        @onclick="HandleClick"
        @onmousemove="HandleMouseMove"
        style="display: block; background: @Form.BackColor.ToHexString();">
</canvas>

@code {
    private void HandleClick(MouseEventArgs e)
    {
        if (Form == null) return;

        var args = new Forms.MouseEventArgs(
            (int)e.OffsetX, 
            (int)e.OffsetY, 
            Forms.MouseButtons.Left
        );

        Form.OnMouseClick(args);
    }

    private void HandleMouseMove(MouseEventArgs e)
    {
        if (Form == null) return;

        var args = new Forms.MouseEventArgs(
            (int)e.OffsetX, 
            (int)e.OffsetY, 
            Forms.MouseButtons.None
        );

        Form.OnMouseMove(args);
    }
}
```

## Adding New Controls

To add a control (e.g., Button):

### 1. Create the Control Class

Create `WebForms.Canvas\Forms\Controls\Button.cs`:

```csharp
using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms.Controls;

public class Button : Control
{
    public string Text { get; set; } = "Button";
    public event EventHandler? Click;

    public Button()
    {
        Width = 100;
        Height = 30;
        BackColor = Color.FromArgb(225, 225, 225);

        MouseClick += (s, e) => Click?.Invoke(this, EventArgs.Empty);
        Paint += OnButtonPaint;
    }

    private void OnButtonPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw button background
        using var backBrush = new SolidBrush(BackColor);
        g.FillRectangle(backBrush, 0, 0, Width, Height);

        // Draw border
        using var borderPen = new Pen(Color.FromArgb(112, 112, 112), 1);
        g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        // Draw text (centered)
        using var textBrush = new SolidBrush(Color.Black);
        g.DrawString(Text, "Arial", 12, textBrush, 10, Height / 2 - 6);
    }
}
```

### 2. Support Child Controls

Modify `Control.cs` to support parent-child relationships:

```csharp
public abstract class Control
{
    private readonly List<Control> _controls = new();

    public Control? Parent { get; set; }
    public int Left { get; set; }
    public int Top { get; set; }

    public IReadOnlyList<Control> Controls => _controls;

    public void Add(Control control)
    {
        control.Parent = this;
        _controls.Add(control);
    }
}
```

### 3. Update Rendering to Support Children

In `FormRenderer.razor`, modify the rendering logic to recursively render child controls.

## Adding Image Support

To add image/bitmap support:

### 1. Create Image Classes

```csharp
public class Image : IDisposable
{
    public int Width { get; }
    public int Height { get; }
    public string DataUrl { get; }

    public Image(string dataUrl, int width, int height)
    {
        DataUrl = dataUrl;
        Width = width;
        Height = height;
    }

    public static Image FromFile(string path)
    {
        // Load image and convert to data URL
        // This would require JavaScript interop
        throw new NotImplementedException();
    }

    public void Dispose() { }
}
```

### 2. Add DrawImage Commands

```csharp
public class DrawImageCommand : DrawingCommand
{
    public Image Image { get; }
    public int X { get; }
    public int Y { get; }

    public DrawImageCommand(Image image, int x, int y)
    {
        Image = image;
        X = x;
        Y = y;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"var img = new Image();");
        sb.AppendLine($"img.src = '{Image.DataUrl}';");
        sb.AppendLine($"ctx.drawImage(img, {X}, {Y});");
        return sb.ToString();
    }
}
```

### 3. Add to Graphics

```csharp
public void DrawImage(Image image, int x, int y)
{
    _commands.Add(new DrawImageCommand(image, x, y));
}

public void DrawImage(Image image, Point location)
{
    DrawImage(image, location.X, location.Y);
}
```

## Performance Optimization Tips

1. **Command Batching**: Group similar commands together
2. **Dirty Regions**: Only redraw changed portions of the canvas
3. **Double Buffering**: Render to an off-screen canvas first
4. **Request Animation Frame**: Use RAF for smoother animations

```csharp
// Example: Dirty region tracking
public class Graphics
{
    private Rectangle _dirtyRegion;

    public void InvalidateRegion(Rectangle region)
    {
        _dirtyRegion = Rectangle.Union(_dirtyRegion, region);
    }
}
```

## JavaScript Interop Best Practices

1. **Minimize Calls**: Batch operations into single JS invocations
2. **Use One-Way Calls**: Prefer `InvokeVoidAsync` over `InvokeAsync`
3. **Handle Errors**: Wrap JS in try-catch blocks
4. **Type Safety**: Use strongly-typed parameters

```javascript
// Good: Batch operations
window.renderCanvas = (canvas, commands) => {
    const ctx = canvas.getContext('2d');
    eval(commands); // Execute all commands at once
};

// Bad: Multiple calls for each operation
window.drawLine = (canvas, x1, y1, x2, y2) => { /* ... */ };
window.drawRect = (canvas, x, y, w, h) => { /* ... */ };
```

## Testing Your Extensions

1. Create unit tests for command generation
2. Test JavaScript output for correctness
3. Verify rendering in the browser
4. Test event handling and interaction
5. Check memory leaks with Dispose patterns

```csharp
[Fact]
public void DrawLine_GeneratesCorrectJavaScript()
{
    var graphics = new Graphics(800, 600);
    var pen = new Pen(Color.Red, 2);

    graphics.DrawLine(pen, 10, 10, 100, 100);

    var commands = graphics.GetCommands();
    var js = commands.First().ToJavaScript();

    Assert.Contains("ctx.strokeStyle = 'rgba(255,0,0,1)';", js);
    Assert.Contains("ctx.lineWidth = 2;", js);
    Assert.Contains("ctx.moveTo(10, 10);", js);
    Assert.Contains("ctx.lineTo(100, 100);", js);
}
```

## Next Steps

- Review the existing code in the `Drawing` and `Forms` namespaces
- Look at `SampleDrawingForm.cs` for usage examples
- Experiment with the canvas API documentation: https://developer.mozilla.org/en-US/docs/Web/API/Canvas_API
- Consider contributing your extensions back to the project!
