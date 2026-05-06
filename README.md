# CanvasForms

Run WinForms applications in the browser. The UI runs as Blazor WebAssembly on the client, rendered to an HTML `<canvas>`. Server resources (databases, files, etc.) are accessed via a lightweight ASP.NET Core host — no Windows required on the client.

> **Scope:** Local-network / same-machine use (Mode A). Not currently designed for public internet exposure.

---

## How it works

```
┌─────────────────────────────────────────────┐
│  Browser (WASM)                             │
│                                             │
│  Translated WinForms app                   │
│  ↓ uses                                     │
│  Canvas.Windows.Forms (WinForms API shim)   │
│  ↓ renders via                              │
│  HTML <canvas>  ←  FormRenderer / Desktop   │
│                                             │
│  ↕ HTTP/SignalR for data only               │
└─────────────────────────────────────────────┘
         ↕
┌─────────────────────────────────────────────┐
│  Server (ASP.NET Core)                      │
│                                             │
│  - Serves translated app assemblies         │
│  - Manages installed app registry           │
│  - Provides data APIs (DB, files, etc.)     │
└─────────────────────────────────────────────┘
```

WinForms apps are **translated at install time** using the IL Translator — `System.Windows.Forms` references are rewritten to `Canvas.Windows.Forms`. The translated assemblies are then loaded dynamically in the browser via `Assembly.Load(bytes)` and run entirely client-side.

---

## Projects

| Project | Type | Purpose |
|---------|------|---------|
| `Canvas.Windows.Forms` | Razor Class Library (net10.0) | WinForms API shim + canvas renderer. Types live under `System.Windows.Forms`. |
| `Canvas.Windows.Forms.Host` | Blazor WebAssembly (net10.0) | Client app — Desktop surface, OS shell, loads and runs translated apps. |
| `Canvas.Windows.Forms.Host.Server` | ASP.NET Core (net10.0) | Server host — serves the WASM client, manages installed apps, provides data APIs. |
| `Canvas.Windows.Forms.ILTranslator` | Console app (net10.0) | Translates WinForms assemblies to use the Canvas shim via Mono.Cecil IL rewriting. |
| `Canvas.Windows.Forms.RemoteProtocol` | Class Library (net10.0) | Shared types for app metadata and desktop snapshots. |
| `Canvas.Windows.Forms.Tests` | Test project (net10.0) | WinForms API compatibility tracking and unit tests. |

---

## Quick start

### Prerequisites

- .NET SDK **10.0**
- Visual Studio 2026+ (or any editor that supports Blazor WebAssembly)

### Run

1. Set **`Canvas.Windows.Forms.Host.Server`** as the startup project.
2. Run (`F5`).
3. Open `http://localhost:5001` in a browser.

The OS shell launches, opens the demo **WelcomeForm**, and shows a Start menu.

---

## Installing a WinForms app

1. Click **Start → Install App...**
2. Upload the app's `.exe` and `.dll` files.
3. The server translates the assemblies (rewrites `System.Windows.Forms` → `Canvas.Windows.Forms`).
4. The app appears in the Start menu — click to launch it in the browser.

Installed apps are stored in `Canvas.Windows.Forms.Host.Server/.apps/` (excluded from git).

---

## Architecture details

### UI runs in the browser

The `Desktop` Blazor component manages open windows — dragging, resizing, minimize/maximize/close, taskbar, z-order. `FormRenderer` draws each window's chrome and client area to a `<canvas>` element. All window management logic runs as WASM on the client with no server round-trips.

### WinForms API shim

`Canvas.Windows.Forms` implements the `System.Windows.Forms` namespace so that translated apps compile and run without modification:

- `Control`, `Form`, `ContainerControl`, `ScrollableControl`
- `Button`, `CheckBox`, `RadioButton`
- `Label`, `TextBox`, `TextBoxBase`
- `ListBox`, `CheckedListBox`, `ComboBox`, `ListControl`
- `PictureBox`, `DateTimePicker`
- `Padding`, `Anchor`, `Dock`, layout engine
- `FormClosing` / `FormClosed` events with `CloseReason` and cancellation support
- `Control.Invoke` / `BeginInvoke` shims (no-op — WASM is single-threaded)
- `PointToScreen` / `PointToClient` / `RectangleToScreen` / `SetBounds`

### IL Translator

`Canvas.Windows.Forms.ILTranslator` rewrites assemblies at the IL level using Mono.Cecil:

```
input.dll  →  [rewrite System.Windows.Forms → Canvas.Windows.Forms]  →  output.dll
```

Usage:
```
Canvas.Windows.Forms.ILTranslator <input-assembly> <output-assembly>
```

The server runs this automatically when an app is installed via the UI.

### Drawing pipeline

```
Paint event  →  Graphics commands  →  canvas-renderer.js  →  HTMLCanvasElement
```

Drawing commands are buffered as typed `DrawingCommand` objects and dispatched to the JS renderer via Blazor JS interop. Gradient brushes are serialised as compact token strings (`LG:…` / `RG:…`) that the JS side resolves into `CanvasGradient` objects at paint time. `GraphicsPath` segments are serialised as a flat numeric array and replayed natively using the Canvas 2D path API.

Current opcodes: `StrokeLine`, `StrokeRect`, `FillRect`, `StrokeEllipse`, `FillEllipse`, `DrawText`, `Clear`, `Save`/`Restore`, `ClipRect`, `DrawImage`, `StrokeRoundRect`, `FillRoundRect`, `DrawArc`, `DrawBezier`, `DrawPolygon`/`FillPolygon`, `DrawPath`/`FillPath`.

---

## Creating a form

```csharp
using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

public class MyForm : Form
{
    public MyForm()
    {
        Text = "Hello CanvasForms";
        Width = 600;
        Height = 400;

        var btn = new Button { Text = "Click me", Left = 20, Top = 20 };
        btn.Click += (s, e) => btn.Text = "Clicked!";
        Controls.Add(btn);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using var pen = new Pen(Color.SteelBlue, 2);
        e.Graphics.DrawRectangle(pen, 20, 60, 200, 100);
    }
}
```

Launching from the OS page:

```csharp
Application.Run(new MyForm());
```

Handling close cancellation:

```csharp
FormClosing += (s, e) =>
{
    if (hasUnsavedChanges)
        e.Cancel = true; // prevents close
};
```

---

## WinForms compatibility

The goal is maximum API-surface compatibility. Some members are stubs (e.g. `Handle`, `AllowDrop`, IME) — they exist so translated apps compile, but have no browser equivalent.

### Implemented controls (source present)

The following WinForms controls/types currently exist under `WebForms.Canvas/Forms/*` (project: `Canvas.Windows.Forms`). Many are **partial** implementations (see the per-control status table below).

**Windowing**
- `Form`

**Core / base types**
- `Control`
- `ContainerControl`
- `ScrollableControl`

**Buttons**
- `Button` (`ButtonBase`)
- `CheckBox` (`ToggleButtonBase`)
- `RadioButton`

**Text / input**
- `Label`
- `LinkLabel`
- `TextBox` (`TextBoxBase`)
- `MaskedTextBox`
- `RichTextBox`

**Lists / hierarchy**
- `ListBox`, `CheckedListBox`
- `ComboBox` (`ListControl`)
- `ListView`
- `TreeView`

**Containers / layout**
- `Panel`
- `GroupBox`
- `TabControl`
- `SplitContainer`
- `TableLayoutPanel`
- `FlowLayoutPanel`

**Display / common controls**
- `PictureBox`
- `ProgressBar`
- `DateTimePicker`
- `MonthCalendar`
- `NumericUpDown` (`UpDownBase`)

**Non-visual / helper components (currently stubs/compat)**
- `ToolTip`

### Not yet implemented (common WinForms controls)

Controls below have **no source file** in the repo yet. Everything else either has at least a stub or partial implementation — check the [Controls roadmap](#controls-roadmap) table for its current status.

**Menus / toolbars (legacy)**
- `MainMenu`, `ContextMenu` (pre-`MenuStrip` legacy)
- `ToolBar` (pre-`ToolStrip` legacy)

**Data / inspection**
- `PropertyGrid`
- `BindingNavigator`

**Value/input**

**Print**
- `PrintDialog`, `PrintPreviewDialog`, `PrintDocument`, `PrintPreviewControl`

**Other**
- `ErrorProvider`
- `HelpProvider`
- `WebBrowser` / WebView2
- `Chart`
- MDI (`MdiClient`, MDI Forms)
- `Clipboard` (JS bridge needed)

Controls live in `WebForms.Canvas/Forms/...` (project: `Canvas.Windows.Forms`).
See `COMPATIBILITY_REVIEW.md` for a full per-control breakdown, and the test project for property-level tracking:

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
| Text | `RichTextBox` | ⚠️ Partial | RTF parsed into styled runs; bold/italic/underline/colour/font-size per run; SelectionFont/Color/Bold/Italic/Underline; Find(); LoadFile/SaveFile; HTML clipboard. |
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
| Common | `ImageList` | ⚠️ Partial | URL/key storage; ImageSize; wired into ListView, TreeView, TabControl. |
| Common | `Timer` | ✅ Good | `PeriodicTimer`-based async loop; fires on captured `SynchronizationContext`. |
| Containers | `Panel` / `ScrollableControl` | ⚠️ Partial | Child painting + input routing; supports scroll offset behavior used by nested controls. |
| Containers | `GroupBox` | ⚠️ Partial | Border/caption + child routing/clipping. |
| Containers | `TabControl` | ⚠️ Partial | Tab strip + page switching. |
| Containers | `SplitContainer` | ⚠️ Partial | Resizable pane splitter. |
| Containers | `UserControl` | 🧩 Stub/Compatibility | Base present; full composite lifecycle partial. |
| Layout | `FlowLayoutPanel` | ⚠️ Partial | FlowDirection + wrap/flow-break behavior. |
| Layout | `TableLayoutPanel` | ⚠️ Partial | Row/column styles + spans; anchors/dock within cells. |
| Menus | `MenuStrip` | ⚠️ Partial | Top-level menu bar with dropdowns. |
| Menus | `ContextMenuStrip` | ⚠️ Partial | Right-click overlay menus. |
| Menus | `ToolStrip` | ⚠️ Partial | Toolbar with icons, hover, checked state. |
| Menus | `StatusStrip` / `ToolStripStatusLabel` | ⚠️ Partial | Status bar; Spring, BorderSides, SizingGrip. |
| Menus | `ToolStripMenuItem` | 🧩 Stub/Compatibility | Dropdowns, check state, shortcuts. |
| Menus | `ToolStripContainer` / `ToolStripPanel` | ⚠️ Partial | Auto-show/hide bands; row layout of child ToolStrips; content panel fills remainder. |
| Dialogs | `OpenFileDialog` | ⚠️ Partial | Host FS + browser upload. |
| Dialogs | `SaveFileDialog` | ⚠️ Partial | `CreatePrompt`, `OverwritePrompt`, `OpenFile()`. |
| Dialogs | `FolderBrowserDialog` | ⚠️ Partial | `SelectedPath`, `Description`, `ShowNewFolderButton`; host FS aware. |
| Dialogs | `ColorDialog` | ⚠️ Partial | Swatch palette + Hex/RGB/HSV inputs. |
| Dialogs | `FontDialog` | ⚠️ Partial | Family/style/size lists; `ShowEffects`, `ShowColor`, `Apply` event. |
| Data | `DataGridView` | ⚠️ Partial | `IList`/`BindingSource`/`DataTable` binding; auto-column gen; sort; frozen columns; clipboard copy (Ctrl+C); multi-column sort (Ctrl+click header). |
| Data | `DataTable` | ⚠️ Partial | DataView/DefaultView; DataRowView; typed RowChanged/ColumnChanged events; Select(filter, sort); DataSet/DataRelation; IListSource; BindingSource wired. |
| Data | `BindingSource` | ⚠️ Partial | `IList`/`IBindingList`/`DataTable`/`DataSet` wrapper; `Current`/`Position` navigation; server-backed via `CanvasDataService`. |
| Non-visual | `ToolTip` | 🧩 Stub/Compatibility | API present; rendering may be incomplete. |
| Non-visual | `NotifyIcon` | ⚠️ Partial | Canvas system tray: icon in taskbar, ContextMenuStrip popup, balloon tips, Click/DoubleClick events. |
### Layout

- Docking and anchoring (`Dock`, `Anchor`)

### Drawing

#### Primitives
- Lines, rectangles, ellipses — stroke and fill
- **Rounded rectangles** — `DrawRoundRect` / `FillRoundRect` with corner radius
- **Arcs** — `DrawArc(pen, x, y, w, h, startAngle, sweepAngle)`
- **Bezier curves** — `DrawBezier(pen, p1, c1, c2, p2)`
- **Polygons** — `DrawPolygon` / `FillPolygon`
- Text rendering with Bold/Italic/Underline/Strikeout

#### Brushes
| Brush | Description |
|---|---|
| `SolidBrush` | Flat colour fill |
| `LinearGradientBrush` | Two-colour gradient between two points or across a rectangle; supports `LinearGradientMode` (Horizontal/Vertical/Diagonal) and custom `InterpolationColors` stops |
| `RadialGradientBrush` | Radial glow from a centre point (canvas extension) |

#### GraphicsPath
`GraphicsPath` accumulates path segments and is drawn/filled via `g.DrawPath` / `g.FillPath`:

```csharp
var path = new GraphicsPath();
path.AddArc(10, 10, 80, 80, 0, 180);
path.AddLine(90, 50, 150, 50);
path.AddBezier(new Point(150,50), new Point(180,10), new Point(220,90), new Point(250,50));
path.CloseFigure();

g.FillPath(new LinearGradientBrush(new Point(10,10), new Point(250,90),
    Color.SkyBlue, Color.Navy), path);
g.DrawPath(new Pen(Color.DarkBlue, 2), path);
```

Supported segment types: `AddLine`, `AddLines`, `AddBezier`, `AddArc`, `AddRectangle`, `AddEllipse`, `AddPolygon`, `CloseFigure`.

All gradient brushes also work with `FillRectangle`, `FillEllipse`, and `FillRoundRect`.

#### Command-buffered rendering
Drawing calls are accumulated as typed command objects and dispatched once per frame to the JS canvas renderer via Blazor interop — no `eval` or string building at runtime.

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

---

## Controls roadmap

Status legend: ✅ Good &nbsp;|&nbsp; ⚠️ Partial &nbsp;|&nbsp; 🧩 Stub &nbsp;|&nbsp; 🔲 Not started

Items are ordered by estimated prevalence in designer-generated / translated WinForms apps.

### Tier 1 — High priority

| Status | Control | Notes |
|--------|---------|-------|
| ✅ | `Button` / `ButtonBase` | Hover, pressed, focus, keyboard |
| ✅ | `CheckBox` | Toggle + indicator |
| ✅ | `RadioButton` | Mutual exclusion within parent |
| ✅ | `TextBox` / `TextBoxBase` | Editing, selection, shortcuts, redo, word-delete, placeholder, autocomplete |
| ✅ | `Label` | Multi-line, alignment, UseMnemonic, AutoEllipsis, AutoSize, BorderStyle, FlatStyle |
| ⚠️ | `ComboBox` | Drop-down + selection; autocomplete partial |
| ✅ | `ListBox` | Selection + navigation; owner-draw, MeasureItem, ItemHeight, IntegralHeight, double-click |
| ⚠️ | `Panel` / `ScrollableControl` | Child painting, input routing, scroll offset |
| ⚠️ | `GroupBox` | Border/caption + child routing |
| ⚠️ | `TabControl` | Tab strip + page switching |
| ⚠️ | `MenuStrip` | Top-level menu bar with dropdowns |
| ⚠️ | `ContextMenuStrip` | Right-click overlay menus |
| ⚠️ | `ToolStrip` | Toolbar with icons, hover, checked state |
| ⚠️ | `StatusStrip` / `ToolStripStatusLabel` | Status bar; Spring, BorderSides, SizingGrip |
| ✅ | `SplitContainer` | Resizable pane splitter; fixed/min-size; double-click reset |
| ✅ | `FlowLayoutPanel` | FlowDirection + wrap/break + SetFlowBreak |
| ✅ | `TableLayoutPanel` | Row/column styles + spans; CellBorderStyle; GetControlFromPosition |
| ✅ | `DateTimePicker` | Format/CustomFormat; ShowUpDown/ShowCheckBox; calendar styling properties |
| ✅ | `NumericUpDown` | Spinner UI + value clamping; direct-type keyboard entry; TextAlign |
| ✅ | `PictureBox` | URL/Image; Load/LoadAsync; SizeMode; LoadCompleted/LoadProgressChanged events |
| ✅ | `ProgressBar` | Blocks/continuous/marquee; animated MarqueeAnimationSpeed; RightToLeftLayout |
| ✅ | `TreeView` | Nodes, expand/collapse, selection; LabelEdit; ToolTipText; BeginUpdate/EndUpdate |
| ✅ | `ListView` | Details/List/LargeIcon views; keyboard nav; EnsureVisible; BeginUpdate/EndUpdate |
| ⚠️ | `OpenFileDialog` | Host FS + browser upload |
| ⚠️ | `ToolTip` | InitialDelay/AutoPopDelay hover timer; balloon + icon title; canvas overlay div |
| ⚠️ | **`DataGridView`** | In-process `DataSource` binding (IList, BindingSource, DataTable); auto-column gen; virtualised scroll; row selection; single/multi-column sort (Ctrl+click header, ▲1 ▲2 indicators); frozen columns (pin columns to left, unaffected by horizontal scroll); Ctrl+C clipboard export (tab-separated, respects `ClipboardCopyMode`); column types: TextBox/CheckBox/Button/ComboBox/Image/Link |
| ✅ | `Timer` | `PeriodicTimer`-based async loop; `Interval`, `Enabled`, `Start()`, `Stop()`, `Tick`, `Tag`, `IContainer` ctor; fires on captured `SynchronizationContext` |
| ⚠️ | **`ErrorProvider`** | SetError/GetError/Clear; red badge overlays; hover title tooltip; BlinkRate/BlinkStyle; ContainerControl |
| ⚠️ | `SaveFileDialog` | Inherits full FileDialog UI; `CreatePrompt`, `OverwritePrompt`, `OpenFile()` |
| ⚠️ | `FolderBrowserDialog` | `SelectedPath`, `Description`, `RootFolder`, `ShowNewFolderButton`, `InitialDirectory`; host FS aware |
| ⚠️ | `ColorDialog` | Swatch palette + Hex/RGB/HSV inputs; `Color`, `AllowFullOpen`, `CustomColors`, `FullOpen` |
| ⚠️ | `FontDialog` | Family/style/size lists; `ShowEffects`, `ShowColor`, `MinSize`/`MaxSize`, `Apply` event |

### Tier 2 — Medium priority

| Status | Control | Notes |
|--------|---------|-------|
| ⚠️ | `RichTextBox` | RTF parsed into styled runs; bold/italic/underline/colour/font-size; SelectionFont/Color; Find(); LoadFile/SaveFile; HTML clipboard |
| ⚠️ | `MaskedTextBox` | Masked display + basic validation |
| ⚠️ | `CheckedListBox` | Basic checked item behaviour |
| ✅ | `MonthCalendar` | Single-month view; SelectionRange; BoldedDates; keyboard/mouse nav |
| ⚠️ | `NotifyIcon` | Canvas system tray in taskbar; ContextMenuStrip popup; balloon tips; Click/DoubleClick |
| 🧩 | `UserControl` | Base present; full composite lifecycle partial |
| ⚠️ | `ToolStripMenuItem` | Dropdowns, check state, shortcuts, image, enabled |
| ⚠️ | `ToolStripContainer` / `ToolStripPanel` | Auto-show/hide bands; row layout |
| 🔲 | **`PropertyGrid`** | Common in tools and settings panels |
| ⚠️ | **`TrackBar`** | Slider; Horizontal/Vertical; tick marks; keyboard/mouse; SetRange |
| ⚠️ | **`HScrollBar` / `VScrollBar`** | Standalone scrollbars; SmallChange/LargeChange; Scroll/ValueChanged events |
| ⚠️ | **`DomainUpDown`** | String-list up-down; Sorted/Wrap; SelectedItem/SelectedIndex; pair to NumericUpDown |
| 🔲 | **`HelpProvider`** | F1 help integration |
| 🔲 | **`ToolStripProgressBar`** | Common in status strips for background tasks |
| 🔲 | **`ToolStripSplitButton`** | Split-action toolbar button |
| 🔲 | **`PrintDialog`** | Print workflow; business-app compat |
| 🔲 | **`PrintPreviewDialog`** | Paired with `PrintDialog` |
| 🔲 | **`PrintDocument`** | Underlying print model |

### Tier 3 — Lower priority / legacy compat

| Status | Control | Notes |
|--------|---------|-------|
| 🔲 | **`DataGrid`** (legacy) | Older apps use instead of `DataGridView` |
| ✅ | **`BindingSource`** | IList/IBindingList wrapper; `Current`/`Position`; `Filter`/`Sort`/`Find`; server-backed via `CanvasDataService` |
| 🔲 | **`BindingNavigator`** | Record-navigation bar; paired with `BindingSource` |
| 🔲 | **`StatusBar`** (legacy) | Pre-`StatusStrip`; thin wrapper for translator compat |
| 🔲 | **`ToolBar`** (legacy) | Pre-`ToolStrip` |
| 🔲 | **`MainMenu`** (legacy) | Pre-`MenuStrip` |
| 🔲 | **`ContextMenu`** (legacy) | Pre-`ContextMenuStrip` |
| 🔲 | **`Splitter`** (legacy) | Pre-`SplitContainer` |
| 🔲 | **`PrintPreviewControl`** | Embedded (non-dialog) print preview |
| ⚠️ | **`Screen`** | `PrimaryScreen`/`AllScreens`; `Bounds` from `window.screen`; `WorkingArea` from `window.innerWidth/Height`; `FromControl`/`FromPoint`/`GetWorkingArea`/`GetBounds`; JS interop via `getScreenInfo`; 1920×1080 fallback; no multi-monitor |
| 🔲 | **`Clipboard`** | Cut/Copy/Paste; requires JS bridge |
| ⚠️ | **`WebBrowser` / WebView2** | iframe overlay; Navigate, GoBack/Forward, Stop, Refresh, DocumentText, ExecuteScriptAsync, events; cross-origin DOM access blocked by browser sandbox |
| 🔲 | **MDI (`MdiClient`, MDI Forms)** | MDI window management; enterprise apps |
| ✅ | **`DataGridViewColumn` types** | TextBox/CheckBox/ComboBox/Button/Image/Link column variants; `DataGridViewCellStyle`; `DataGridViewRow`/`DataGridViewCell` model |
| ✅ | **`CanvasDataService`** | Server-backed ADO.NET provider; `ICanvasDataService.Fill(DataTable, sql)`; SQLite default; ambient `CanvasDataService.Current` accessor for native and translated apps |

> **Updating this table:** when a control reaches ✅/⚠️/🧩 status, update its row here. The Tier 1 table drives the PoC roadmap; Tier 3 is tracked for completeness.

## Limitations (current)

---

## Repo docs

| File | Contents |
|------|----------|
| `COMPATIBILITY_REVIEW.md` | Per-control WinForms API compatibility review |
| `APPLICATION_FORMMANAGER.md` | `Application` + `FormManager` model |
| `EXTENDING.md` | Extending drawing primitives and controls |
| `WebForms.Canvas/Docs/PictureBox.md` | `PictureBox` specifics |

---

## License

No license file is currently included in this repository.
