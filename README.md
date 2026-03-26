# WebForms.Canvas - HTML Canvas-based Windows Forms Clone

A Blazor WebAssembly implementation of Windows Forms drawing capabilities using HTML Canvas, designed to eventually run Windows Forms executables in the browser.

## Overview

This project provides a canvas-based clone of Windows Forms that can render graphics primitives in a web browser. The initial implementation focuses on drawing capabilities, with plans to support running Windows Forms executables (excluding P/Invoke and Windows-specific system calls).

## Architecture

### Core Components

1. **Drawing Primitives** (`WebForms.Canvas.Drawing`)
   - `Color`: RGBA color representation with common named colors
   - `Point`, `PointF`: 2D coordinate structures
   - `Size`, `SizeF`: Dimension structures
   - `Rectangle`, `RectangleF`: Rectangular region structures
   - `Pen`: Line drawing tool with color and width
   - `Brush`: Fill tool (currently supports `SolidBrush`)
   - `Graphics`: Main drawing surface with command buffering

2. **Forms System** (`WebForms.Canvas.Forms`)
   - `Control`: Base class for all visual elements
   - `Form`: Top-level window container
   - `PaintEventArgs`: Event arguments for paint operations
   - Paint event system for custom drawing

3. **Rendering** (`WebForms.Canvas.Components`)
   - `FormRenderer`: Blazor component that renders forms to HTML Canvas
   - JavaScript interop for canvas drawing commands

## Currently Implemented Features

✅ **Drawing Primitives**
- Lines (`DrawLine`)
- Rectangles (`DrawRectangle`, `FillRectangle`)
- Ellipses/Circles (`DrawEllipse`, `FillEllipse`)
- Text (`DrawString`)

✅ **Graphics System**
- Pen and Brush abstractions
- Color management with RGBA support
- Command buffering for efficient rendering
- Paint event system

✅ **Form Management**
- Basic form display
- Background color support
- Show/Close operations
- Title bar with close button

## Usage Example

```csharp
using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

public class MyDrawingForm : Form
{
    public MyDrawingForm()
    {
        Text = "My Drawing";
        Width = 800;
        Height = 600;
        BackColor = Color.White;

        Paint += OnPaint;
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw a red rectangle
        using var pen = new Pen(Color.Red, 2);
        g.DrawRectangle(pen, 50, 50, 200, 100);

        // Fill a blue circle
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 300, 50, 100, 100);

        // Draw text
        g.DrawString("Hello World!", "Arial", 24, 
            new SolidBrush(Color.Black), 50, 200);
    }
}
```

In your Blazor page:

```razor
@page "/"
@using WebForms.Canvas.Components

<FormRenderer Form="@myForm" />

@code {
    private MyDrawingForm myForm = new();

    protected override void OnInitialized()
    {
        myForm.Show();
    }
}
```

## Roadmap

### Phase 1: Drawing (Current)
- ✅ Basic shapes (lines, rectangles, ellipses)
- ✅ Fill operations
- ✅ Text rendering
- ✅ Color support

### Phase 2: Interaction
- ⬜ Mouse events (click, move, drag)
- ⬜ Keyboard events
- ⬜ Focus management

### Phase 3: Controls
- ⬜ Button
- ⬜ Label
- ⬜ TextBox
- ⬜ CheckBox
- ⬜ RadioButton
- ⬜ ListBox
- ⬜ ComboBox
- ⬜ PictureBox

### Phase 4: Advanced Drawing
- ⬜ Images and bitmaps
- ⬜ Paths and polygons
- ⬜ Gradients
- ⬜ Transformations (rotate, scale, translate)
- ⬜ Clipping regions

### Phase 5: IL Execution
- ⬜ IL bytecode interpreter
- ⬜ Assembly loading and reflection
- ⬜ Windows Forms API compatibility layer
- ⬜ Executable execution engine

## Project Structure

```
WebForms.Canvas/              # Core library (Razor Class Library)
├── Drawing/                  # Drawing primitives and graphics
│   ├── Color.cs
│   ├── Point.cs
│   ├── Size.cs
│   ├── Rectangle.cs
│   ├── Pen.cs
│   ├── Brush.cs
│   ├── Graphics.cs
│   └── DrawingCommands.cs
├── Forms/                    # Form system
│   ├── Control.cs
│   ├── Form.cs
│   └── PaintEventArgs.cs
├── Components/               # Blazor components
│   └── FormRenderer.razor
├── Samples/                  # Example forms
│   └── SampleDrawingForm.cs
└── wwwroot/
    └── canvas-renderer.js    # JavaScript interop

WebForms.Canvas.Host/         # Blazor WebAssembly host
└── Pages/
    └── Home.razor            # Demo page
```

## Technical Details

### Command-Based Rendering

The graphics system uses a command pattern where drawing operations are buffered as commands and then serialized to JavaScript for execution on the canvas. This approach:

- Allows for batched rendering
- Provides a clean separation between .NET and JavaScript
- Enables future optimization opportunities (command merging, caching, etc.)

### JavaScript Interop

Drawing commands are converted to JavaScript code and executed via `IJSRuntime`:

```csharp
var jsCommands = string.Join("\n", commands.Select(c => c.ToJavaScript()));
await JSRuntime.InvokeVoidAsync("renderCanvas", _canvasRef, jsCommands);
```

Example generated JavaScript:
```javascript
ctx.strokeStyle = 'rgba(255,0,0,1)';
ctx.lineWidth = 2;
ctx.strokeRect(50, 50, 200, 100);
```

## Limitations

Current limitations that will be addressed in future phases:

1. **No P/Invoke support**: Native Windows API calls cannot be executed
2. **No system-specific features**: Clipboard, file dialogs, etc. require browser APIs
3. **Single-threaded**: Browser limitations apply
4. **No GDI+ features**: Advanced graphics features not yet implemented
5. **No designer support**: Forms must be created programmatically

## Building and Running

1. Clone the repository
2. Open the solution in Visual Studio 2026 or later
3. Build the solution (Ctrl+Shift+B)
4. Run the WebForms.Canvas.Host project (F5)

The demo page will show a sample form with various drawing primitives.

## Contributing

This project is in early development. Contributions are welcome for:

- Additional drawing primitives
- Event system implementation
- Control implementations
- Performance optimizations
- Documentation improvements

## License

[Your License Here]

## Future Vision

The ultimate goal is to create a system that can load and execute Windows Forms `.exe` files directly in the browser, providing a web-based Windows Forms runtime. This would enable:

- Legacy application modernization without rewriting
- Cross-platform Windows Forms apps via web browsers
- Gradual migration paths for existing WinForms codebases
- Educational tools for learning Windows Forms programming
