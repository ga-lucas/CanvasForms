# Canvas.Windows.Forms

WinForms-style UI framework rendered to an HTML `<canvas>` using Blazor WebAssembly. The goal is **maximum compatibility with the WinForms API surface/behavior**, while mapping rendering and input to the browser.

## What this repo contains

- **Canvas.Windows.Forms** (`net10.0` Razor Class Library): the WinForms-like API (types live under `System.Windows.Forms`) plus a lightweight drawing layer (`Canvas.Windows.Forms.Drawing`).
- **Canvas.Windows.Forms.Host** (`net10.0` Blazor WebAssembly): a runnable demo host that boots a desktop-like surface and shows sample forms.
- **Canvas.Windows.Forms.Tests** (`net10.0`): tests and documentation tracking WinForms API completeness (especially `Control`).

## Quick start

### Prerequisites

- .NET SDK **10.0**
- Visual Studio 2026+ (or any editor that can build/run Blazor WebAssembly)

### Run the demo

1. Open the solution.
2. Set **Canvas.Windows.Forms.Host** as the startup project.
3. Run (F5).

The host renders a **Desktop** surface and opens `WelcomeForm`.

## How it works (high level)

### Desktop + FormManager

The `Desktop` component hosts a `FormManager` that tracks open forms, z-order, activation, and taskbar buttons.

### Rendering pipeline

- `FormRenderer` draws the window chrome (title bar, border, min/max/close buttons) and then renders the client area.
- User code draws via `Paint` handlers using `Canvas.Windows.Forms.Drawing.Graphics`.
- Drawing commands are sent to JavaScript and executed on the canvas.

### Input

Mouse, keyboard, and touch events are captured by Blazor and translated into WinForms-style events on `Form`/`Control`.

## Usage

### Add the desktop surface (Blazor)

In a page like `Pages/Home.razor`:

```razor
@page "/"
@using Canvas.Windows.Forms
@using Canvas.Windows.Forms.Components
@using Canvas.Windows.Forms.Samples
@using System.Windows.Forms

<Desktop @ref="_desktop" TaskbarHeight="32" />

@code {
    private Desktop? _desktop;

    protected override void OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _desktop != null)
        {
            var mainForm = new WelcomeForm();
            _desktop.FormManager.MainForm = mainForm;
            Application.Run(mainForm);
        }

        return Task.CompletedTask;
    }
}
```

### Create a form and draw

```csharp
using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

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

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        using var pen = new Pen(Color.Red, 2);
        g.DrawRectangle(pen, 50, 50, 200, 100);
    }
}
```

### Open forms (WinForms-style)

```csharp
Application.FormManager?.ShowOrCreateForm<ControlsDemoForm>();
```

More details: `APPLICATION_FORMMANAGER.md`.

## Implemented features (selected)

### Windowing / desktop

- Desktop surface with taskbar buttons
- Multiple windows with z-ordering / activation
- Drag to move, resize handles
- Minimize / maximize / close
- Min/Max size constraints

### Controls

Controls live in `WebForms.Canvas/Forms/...` (project: `Canvas.Windows.Forms`).

Status legend:

- ✅ **Good**: usable for typical demos/apps.
- ⚠️ **Partial**: core behavior exists, but missing WinForms features and/or rendering fidelity.
- 🧩 **Stub/Compatibility**: API exists primarily for porting; limited behavior.

| Area | Control | Status | Notes |
|------|---------|--------|-------|
| Windowing | `Form` | ⚠️ Partial | Window chrome, move/resize, min/max/close are implemented. |
| Core | `Control` | ⚠️ Partial | API surface is prioritized (see tests); many members are compatibility-oriented in a canvas environment. |
| Text | `Label` | ⚠️ Partial | Basic multi-line + alignment, approximate measurement. |
| Text | `LinkLabel` | ⚠️ Partial | Click/visited + optional browser navigation via `LinkUrl`. |
| Text | `TextBox` / `TextBoxBase` | ⚠️ Partial | Basic editing, selection, shortcuts; autocomplete support is evolving. |
| Text | `MaskedTextBox` | ⚠️ Partial | Masked display + basic validation. |
| Text | `RichTextBox` | 🧩 Stub/Compatibility | Stores RTF, renders as plain text. |
| Buttons | `Button` / `ButtonBase` | ✅ Good | Hover/pressed/focus states + click via mouse/keyboard. |
| Buttons | `CheckBox` | ✅ Good | Toggle behavior + indicator rendering. |
| Buttons | `RadioButton` | ✅ Good | Mutual exclusivity within parent. |
| Lists | `ListControl` | ⚠️ Partial | Base type for list-like controls. |
| Lists | `ListBox` | ⚠️ Partial | Selection + basic navigation; missing advanced modes. |
| Lists | `CheckedListBox` | ⚠️ Partial | Basic checked item behavior. |
| Lists | `ComboBox` | ⚠️ Partial | Drop-down + selection; autocomplete support is partial. |
| Collections | `TreeView` | ⚠️ Partial | Nodes + expand/collapse + selection. |
| Collections | `ListView` | ⚠️ Partial | Details view + columns/items; feature coverage still growing. |
| Display | `PictureBox` | ⚠️ Partial | URL-based image loading (see `WebForms.Canvas/Docs/PictureBox.md`). |
| Display | `ProgressBar` | ⚠️ Partial | Blocks/continuous/marquee-style rendering (simplified). |
| Display | `MonthCalendar` | ⚠️ Partial | Single-month view + basic keyboard/mouse navigation. |
| Common | `DateTimePicker` | ⚠️ Partial | Simplified text rendering + drop-down calendar. |
| Common | `NumericUpDown` / `UpDownBase` | ⚠️ Partial | Spinner UI + value clamping/events; missing WinForms edge cases. |
| Containers | `Panel` / `ScrollableControl` | ⚠️ Partial | Child painting + input routing; supports scroll offset behavior used by nested controls. |
| Containers | `GroupBox` | ⚠️ Partial | Border/caption + child routing/clipping. |
| Layout | `FlowLayoutPanel` | ⚠️ Partial | FlowDirection + wrap/flow-break behavior. |
| Layout | `TableLayoutPanel` | ⚠️ Partial | Row/column styles + spans; anchors/dock within cells. |

Non-visual WinForms components (API-oriented):

- `ToolTip` (stub)
- `NotifyIcon` (stub)

### Layout

- Docking and anchoring (`Dock`, `Anchor`)

### Drawing

- Shapes (lines/rectangles/ellipses)
- Fill operations
- Text rendering
- Command-buffered rendering to JS canvas

## WinForms compatibility notes

This project prioritizes matching the **WinForms SDK API surface**. Some APIs exist primarily for compatibility in a browser/canvas environment.

The test project tracks `Control` property parity:

- ✅ **102/102 `Control` properties implemented** (API completeness)
- ⚠️ Not all properties are fully functional yet (some are stubs by design)

See:

- `Canvas.Windows.Forms.Tests/README.md`
- `Canvas.Windows.Forms.Tests/PROPERTY_COMPLETENESS.md`
- `Canvas.Windows.Forms.Tests/PROPERTY_FUNCTIONALITY.md`

### Implemented vs partially implemented controls

The control list above is the authoritative snapshot of which WinForms controls currently exist in the codebase.

For a more detailed narrative review (including gaps and missing members), see `COMPATIBILITY_REVIEW.md` and `CONTROLS_IMPLEMENTATION_STRATEGY.md`.

## Limitations (current)

- No HWND/real window handles (`Handle` and related APIs are compatibility-oriented)
- No P/Invoke / native Windows APIs
- No Visual Studio WinForms designer
- Browser runtime constraints (single-threaded UI model)

## Repo docs

- `APPLICATION_FORMMANAGER.md` – Application + FormManager model
- `EXTENDING.md` – extending drawing primitives and features
- `WebForms.Canvas/Docs/PictureBox.md` – PictureBox specifics

## Roadmap (short)

- Focus management and tab order
- Continue aligning control behavior with WinForms
- More controls and richer drawing primitives

## License

No license file is currently included in the repository.
