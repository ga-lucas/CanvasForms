# Quick Start Guide

This guide will help you create your first custom Windows Forms canvas application.

## Step 1: Create a Custom Form

Create a new class that inherits from `Form`:

```csharp
using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace YourNamespace;

public class MyFirstForm : Form
{
    private int clickCount = 0;

    public MyFirstForm()
    {
        // Set form properties
        Text = "My First Form";
        Width = 600;
        Height = 400;
        BackColor = Color.FromArgb(240, 240, 240);

        // Subscribe to Paint event
        Paint += OnPaint;
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        // Your drawing code here
        using var pen = new Pen(Color.Blue, 2);
        using var brush = new SolidBrush(Color.Red);

        g.DrawRectangle(pen, 50, 50, 200, 100);
        g.FillEllipse(brush, 300, 50, 100, 100);
        g.DrawString("Hello, Canvas!", "Arial", 20, 
            new SolidBrush(Color.Black), 50, 200);
    }
}
```

## Step 2: Display Your Form in Blazor

In your Blazor page (e.g., `Home.razor`):

```razor
@page "/"
@using WebForms.Canvas.Components
@using YourNamespace

<h1>My Canvas Application</h1>

<FormRenderer Form="@myForm" />

@code {
    private MyFirstForm myForm = new();

    protected override void OnInitialized()
    {
        myForm.Show();
    }
}
```

## Step 3: Run Your Application

1. Press F5 in Visual Studio
2. Your browser will open showing your custom form
3. The canvas will render all your drawing commands

## Common Drawing Operations

### Drawing Lines

```csharp
using var pen = new Pen(Color.Red, 2);
g.DrawLine(pen, x1, y1, x2, y2);

// Or with Point structs
g.DrawLine(pen, new Point(10, 10), new Point(100, 100));
```

### Drawing Rectangles

```csharp
// Outline
using var pen = new Pen(Color.Blue, 1);
g.DrawRectangle(pen, x, y, width, height);

// Filled
using var brush = new SolidBrush(Color.Green);
g.FillRectangle(brush, x, y, width, height);
```

### Drawing Circles/Ellipses

```csharp
// Outline
using var pen = new Pen(Color.Purple, 2);
g.DrawEllipse(pen, x, y, width, height);

// Filled
using var brush = new SolidBrush(Color.Yellow);
g.FillEllipse(brush, x, y, width, height);

// Perfect circle: make width == height
g.FillEllipse(brush, 100, 100, 50, 50);
```

### Drawing Text

```csharp
using var brush = new SolidBrush(Color.Black);
g.DrawString(
    "Your text here",      // Text to draw
    "Arial",               // Font family
    16,                    // Font size
    brush,                 // Brush for color
    x,                     // X position
    y                      // Y position
);
```

## Using Colors

### Predefined Colors

```csharp
Color.Black
Color.White
Color.Red
Color.Green
Color.Blue
Color.Yellow
Color.Transparent
```

### Custom Colors

```csharp
// RGB (alpha = 255)
Color.FromArgb(128, 0, 255)

// ARGB (with transparency)
Color.FromArgb(128, 255, 0, 0) // Semi-transparent red

// From hex (in code)
Color.FromArgb(0xFF, 0x45, 0x67, 0x89)
```

## Animation Example

To create animations, use `Invalidate()` to trigger redraws:

```csharp
public class AnimatedForm : Form
{
    private int x = 0;
    private System.Threading.Timer? timer;

    public AnimatedForm()
    {
        Text = "Animation Demo";
        Width = 400;
        Height = 300;

        Paint += OnPaint;

        // Start animation timer
        timer = new System.Threading.Timer(_ => 
        {
            x = (x + 5) % Width;
            Invalidate(); // Trigger redraw
        }, null, 0, 50); // 50ms = ~20 FPS
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);

        using var brush = new SolidBrush(Color.Red);
        g.FillEllipse(brush, x, 100, 50, 50);
    }
}
```

## Interactive Form Example

Handle mouse events (when implemented):

```csharp
public class InteractiveForm : Form
{
    private List<Point> clickPoints = new();

    public InteractiveForm()
    {
        Text = "Click to Draw";
        Width = 600;
        Height = 400;

        Paint += OnPaint;
        // MouseClick += OnMouseClick; // Coming in Phase 2
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);

        using var brush = new SolidBrush(Color.Blue);
        foreach (var point in clickPoints)
        {
            g.FillEllipse(brush, point.X - 5, point.Y - 5, 10, 10);
        }
    }

    // private void OnMouseClick(object sender, MouseEventArgs e)
    // {
    //     clickPoints.Add(new Point(e.X, e.Y));
    //     Invalidate();
    // }
}
```

## Creating Complex Drawings

Break down complex drawings into methods:

```csharp
public class ComplexDrawingForm : Form
{
    public ComplexDrawingForm()
    {
        Text = "Complex Drawing";
        Width = 800;
        Height = 600;
        Paint += OnPaint;
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.White);

        DrawSky(g);
        DrawGround(g);
        DrawHouse(g, 100, 300);
        DrawTree(g, 500, 250);
        DrawSun(g, 650, 100);
    }

    private void DrawSky(Graphics g)
    {
        using var brush = new SolidBrush(Color.FromArgb(135, 206, 235));
        g.FillRectangle(brush, 0, 0, Width, Height / 2);
    }

    private void DrawGround(Graphics g)
    {
        using var brush = new SolidBrush(Color.FromArgb(34, 139, 34));
        g.FillRectangle(brush, 0, Height / 2, Width, Height / 2);
    }

    private void DrawHouse(Graphics g, int x, int y)
    {
        // House body
        using var houseBrush = new SolidBrush(Color.FromArgb(139, 69, 19));
        g.FillRectangle(houseBrush, x, y, 150, 100);

        // Roof - draw lines to form triangle
        using var roofPen = new Pen(Color.FromArgb(178, 34, 34), 3);
        g.DrawLine(roofPen, x, y, x + 75, y - 50);
        g.DrawLine(roofPen, x + 75, y - 50, x + 150, y);
    }

    private void DrawTree(Graphics g, int x, int y)
    {
        // Trunk
        using var trunkBrush = new SolidBrush(Color.FromArgb(101, 67, 33));
        g.FillRectangle(trunkBrush, x, y, 30, 80);

        // Leaves
        using var leavesBrush = new SolidBrush(Color.FromArgb(0, 128, 0));
        g.FillEllipse(leavesBrush, x - 30, y - 60, 90, 90);
    }

    private void DrawSun(Graphics g, int x, int y)
    {
        using var sunBrush = new SolidBrush(Color.Yellow);
        g.FillEllipse(sunBrush, x - 40, y - 40, 80, 80);
    }
}
```

## Tips and Best Practices

1. **Use `using` statements**: Always dispose of Pens and Brushes
   ```csharp
   using var pen = new Pen(Color.Red, 2);
   // pen is automatically disposed
   ```

2. **Clear the background**: Start each paint with `g.Clear(BackColor)`

3. **Keep Paint fast**: Avoid heavy calculations in OnPaint
   ```csharp
   // BAD: Calculating in OnPaint
   private void OnPaint(object sender, PaintEventArgs e)
   {
       var data = ExpensiveCalculation(); // Called every frame!
       DrawData(e.Graphics, data);
   }

   // GOOD: Pre-calculate
   private object? cachedData;

   private void CalculateData()
   {
       cachedData = ExpensiveCalculation();
       Invalidate();
   }

   private void OnPaint(object sender, PaintEventArgs e)
   {
       if (cachedData != null)
           DrawData(e.Graphics, cachedData);
   }
   ```

4. **Use meaningful variable names**: Make your code readable
   ```csharp
   // BAD
   g.FillRectangle(b, 10, 20, 30, 40);

   // GOOD
   using var skyBrush = new SolidBrush(Color.Blue);
   g.FillRectangle(skyBrush, skyX, skyY, skyWidth, skyHeight);
   ```

5. **Test incrementally**: Add one shape at a time and test

## Troubleshooting

### Form doesn't appear
- Did you call `myForm.Show()` in `OnInitialized()`?
- Is the `FormRenderer` component in your Blazor page?

### Drawing commands don't appear
- Check if you subscribed to the Paint event
- Verify the coordinates are within the form bounds
- Ensure you're not clearing after drawing

### Colors look wrong
- Remember RGB order: Red, Green, Blue
- Check alpha channel (0 = transparent, 255 = opaque)
- Use `Color.FromArgb(r, g, b)` for RGB without alpha

### Animation is choppy
- Reduce timer frequency (increase interval)
- Minimize work in OnPaint
- Consider dirty region optimization (future feature)

## Next Steps

- Explore the `SampleDrawingForm.cs` for more examples
- Read `EXTENDING.md` to learn how to add new features
- Check the main `README.md` for architecture details
- Experiment with different shapes and colors!

## Need Help?

- Review the sample forms in `WebForms.Canvas\Samples\`
- Check the documentation in `README.md` and `EXTENDING.md`
- Look at the source code for built-in primitives
- Open an issue on GitHub (if applicable)

Happy drawing! 🎨
