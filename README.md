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
- `Padding`, `Anchor`, `Dock`, layout engine — **Padding now insets docking/anchoring `clientRect` and `DisplayRectangle` (WinForms parity)**
- `FormClosing` / `FormClosed` events with `CloseReason` and cancellation support
- `Control.Invoke(Delegate)`, `Invoke(Action)`, `Invoke<T>(Func<T>)`, `BeginInvoke` (posts via `SynchronizationContext` on Blazor Server; sync fallback on WASM), `EndInvoke`
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
- `TrackBar`
- `DomainUpDown`
- `ScrollBar` (`HScrollBar`, `VScrollBar`)
- `PropertyGrid`

**Menus / toolbars**
- `MenuStrip`, `ContextMenuStrip`
- `ToolStrip`, `ToolStripMenuItem`, `ToolStripContainer`
- `StatusStrip`
- `MainMenu`, `ContextMenu`, `ToolBar` (legacy wrappers)

**Non-visual / helper components**
- `ToolTip`
- `Timer`
- `ImageList`
- `ErrorProvider`
- `NotifyIcon`
- `BindingSource`, `BindingNavigator`
- `Clipboard`
- `Screen`

**Web / browser**
- `WebBrowser` / WebView2

**Dialogs**
- `OpenFileDialog`, `SaveFileDialog`, `FolderBrowserDialog`
- `ColorDialog`, `FontDialog`

**Data**
- `DataGridView`
- `DataTable`
- `CanvasDataService`

### Not yet implemented (common WinForms controls)

Controls below have **no source file** in the repo yet. Everything else either has at least a stub or partial implementation — check the [Controls roadmap](#controls-roadmap) table for its current status.

**Data / inspection**

**Print**
- `PrintDialog`, `PrintPreviewDialog`, `PrintDocument`, `PrintPreviewControl`

**Other**
- `HelpProvider`
- `Chart`

Controls live in `WebForms.Canvas/Forms/...` (project: `Canvas.Windows.Forms`).
See [`COMPATIBILITY_REVIEW.md`](COMPATIBILITY_REVIEW.md) for a full per-control breakdown of implemented APIs, known gaps, and session notes.

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

## Theming

CanvasForms ships a token-based theming system that controls every colour used by the desktop shell, window chrome, and canvas-rendered controls.

### Built-in themes

| Theme | Description |
|-------|-------------|
| **Classic** | Default blue-gradient chrome — mirrors the CanvasForms default palette |
| **Light** | Clean white/grey palette inspired by modern Windows light mode |
| **Dark** | Dark-surface palette inspired by modern Windows dark mode |

### External theme files

The three built-in themes are stored as editable JSON files in the server project, under `Canvas.Windows.Forms.Host.Server/themes/`.  
Edit any file and restart the server — no recompile required.

| File | Theme |
|------|-------|
| `themes/classic.json` | Classic |
| `themes/light.json` | Light |
| `themes/dark.json` | Dark |

The server exposes the files through two minimal API endpoints:

```
GET /api/themes          → string[]   (list of available theme names)
GET /api/themes/{name}   → JSON text  (raw token object for that theme)
```

At startup the Blazor shell fetches all available themes and registers them with `CanvasThemeRegistry.Register(name, json)`.  
If the server is unreachable the registry falls back to embedded compile-time copies of the same JSON.

### Adding a custom theme

1. Drop a new `.json` file in `Canvas.Windows.Forms.Host.Server/themes/` (same token keys as the built-ins).
2. Restart the server — the file is automatically discovered and listed by `/api/themes`.
3. The theme name (capitalised file stem) will appear in the **Settings → Theme…** picker.

You can also register a theme in code at startup:

```csharp
CanvasThemeRegistry.Register("MyTheme", myThemeJson);
```

Or assign a hand-crafted `CanvasTheme` directly:

```csharp
CanvasTheme.Current = new CanvasTheme { DesktopBackColor = Color.MidnightBlue, ... };
```

### Rounded window corners

Set `windowCornerRadius` in your theme JSON to control how much window corners are rounded (pixels, default `0` for Classic, `8` for Light/Dark):

```json
{ "windowCornerRadius": 12, ... }
```

### How it works

1. `CanvasTheme` — holds all named `System.Drawing.Color` tokens (title bar gradients, taskbar colours, button states, desktop background, `WindowCornerRadius`, etc.). Setting `CanvasTheme.Current` raises the static `ThemeChanged` event.
2. `CanvasThemeRegistry` — seeds from embedded JSON fallbacks at startup, then accepts `Register(name, json)` calls; exposes `Apply(name)` to switch themes.
3. `CanvasThemeLoader` — parses JSON into `CanvasTheme` instances; supports non-mutating `LoadFromJsonWithoutApplying()` for preview swatches.
4. `ThemePickerForm` — a canvas WinForms dialog (opens from **Start → Settings → Theme…**) that lists available themes, shows colour swatches, and calls `CanvasThemeRegistry.Apply()` on confirm.
5. `Desktop.razor` — subscribes to `CanvasTheme.ThemeChanged`; on change it pushes updated tokens to the JS renderer via `applyCanvasTheme(tokens)` and re-renders the Blazor shell.

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
| ⚠️ | `ComboBox` | Editable DropDown input; DropDownList type-ahead; AutoCompleteMode (Suggest/Append/SuggestAppend); FindString/FindStringExact |
| ✅ | `ListBox` | Selection + navigation; owner-draw, MeasureItem, ItemHeight, IntegralHeight, double-click; **DataSource/DisplayMember/ValueMember/SelectedValue binding**; IBindingList change refresh |
| ⚠️ | `Panel` / `ScrollableControl` | Child painting, input routing, scroll offset; AutoSize + AutoSizeMode |
| ⚠️ | `GroupBox` | Border/caption + child routing; AutoSize + AutoSizeMode |
| ⚠️ | `TabControl` | Tab strip + page switching; TabCount; GetTabRect(index); Ctrl+Tab keyboard navigation; `ShowToolTips`; `RowCount`; `TabPage.ToolTipText` |
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
| ✅ | **`MessageBox`** | All `Show` overloads (text/caption/buttons/icon/defaultButton/options/owner); `ShowAsync`; `MessageBoxButtons`/`MessageBoxIcon`/`MessageBoxDefaultButton`/`MessageBoxOptions` enums; SignalR broadcast to browser; `CanvasApplication.MessageBoxHandler` hook |
| ✅ | `TreeView` | Nodes, expand/collapse, selection; LabelEdit; ToolTipText; BeginUpdate/EndUpdate; CheckBoxes + `AfterCheck`/`BeforeCheck` events; Space-key toggle |
| ✅ | `ListView` | Details/List/LargeIcon views; keyboard nav; EnsureVisible; BeginUpdate/EndUpdate; `Groups`/`ShowGroups`; `ListViewGroup`/`ListViewGroupCollection`; `ListViewItem.Group` |
| ⚠️ | `OpenFileDialog` | Host FS + browser upload; `DereferenceLinks`, `SupportMultiDottedExtensions`, `ShowHelp` |
| ⚠️ | `ToolTip` | InitialDelay/AutoPopDelay hover timer; balloon + icon title; canvas overlay div; **per-control AutoPopDelay override** via `SetToolTip(control, text, delay)` |
| ⚠️ | **`DataGridView`** | In-process `DataSource` binding (IList, BindingSource, DataTable); auto-column gen; virtualised scroll; row selection; single/multi-column sort (Ctrl+click header, ▲1 ▲2 indicators); frozen columns (pin columns to left, unaffected by horizontal scroll); **frozen rows** (`DataGridViewRow.Frozen`, pinned below header, unaffected by vertical scroll); **`CellValidating` / `RowValidating`** — fire on selection change; `Cancel = true` blocks move and draws red inset border on the failing cell; clears when validation passes; Ctrl+C clipboard export (tab-separated, respects `ClipboardCopyMode`); column types: TextBox/CheckBox/Button/ComboBox/Image/Link; **`DataGridViewComboBoxColumn` in-cell dropdown** — dropdown arrow button, double-click or F2 opens overlay; Up/Down/Enter/Escape keyboard nav; hover highlight; `Items`/`DataSource` supported; `RowsRemoved`, `UserAddedRow`, `UserDeletingRow`, `UserDeletedRow`, `DefaultValuesNeeded` events added |
| ✅ | `Timer` | `PeriodicTimer`-based async loop; `Interval`, `Enabled`, `Start()`, `Stop()`, `Tick`, `Tag`, `IContainer` ctor; fires on captured `SynchronizationContext` |
| ⚠️ | **`ErrorProvider`** | SetError/GetError/Clear; red badge overlays; hover title tooltip; BlinkRate/BlinkStyle; ContainerControl |
| ⚠️ | `SaveFileDialog` | Inherits full FileDialog UI; `CreatePrompt`, `OverwritePrompt`, `OpenFile()` |
| ⚠️ | `FolderBrowserDialog` | `SelectedPath`, `Description`, `RootFolder`, `ShowNewFolderButton`, `InitialDirectory`; host FS aware |
| ⚠️ | `ColorDialog` | Swatch palette + Hex/RGB/HSV inputs; `Color`, `AllowFullOpen`, `CustomColors`, `FullOpen` |
| ⚠️ | `FontDialog` | Family/style/size lists; `ShowEffects`, `ShowColor`, `MinSize`/`MaxSize`, `Apply` event |

### Tier 2 — Medium priority

| Status | Control | Notes |
|--------|---------|-------|
| ⚠️ | `RichTextBox` | RTF parsed into styled runs; bold/italic/underline/colour/font-size; SelectionFont/Color/Bold/Italic/Underline; Find(); LoadFile/SaveFile; HTML clipboard; ScrollToCaret(); ZoomFactor; `LinkClicked`/`Protected`/`VScroll`/`HScroll` events |
| ⚠️ | `LinkLabel` | Links collection; multi-span hit testing; LinkClicked event; ActiveLinkColor/LinkColor/VisitedLinkColor; LinkBehavior; visited-state tracking |
| ⚠️ | `MaskedTextBox` | Masked display + per-token input validation; `MaskFull`; `MaskInputRejected`; Backspace/Delete mask-aware |
| ⚠️ | `CheckedListBox` | Checked item behaviour; ThreeState; ItemCheck/ItemChecked events; keyboard nav (Space/arrows/Home/End/PageUp/PageDown); mouse wheel scrolling; first-letter type-ahead |
| ✅ | `MonthCalendar` | Single-month view; SelectionRange; BoldedDates; keyboard/mouse nav |
| ⚠️ | `NotifyIcon` | Canvas system tray in taskbar; ContextMenuStrip popup; balloon tips; Click/DoubleClick; **MouseDown/MouseUp/MouseMove** |
| ⚠️ | `UserControl` | `Load` event; `AutoSize`/`AutoSizeMode`; `BorderStyle` painted (None/FixedSingle/Fixed3D); `OnCreateControl`/`CreateControl` lifecycle; `AutoScaleDimensions` designer support |
| ⚠️ | `ToolStripMenuItem` | Dropdowns, check state, shortcuts, image, enabled |
| ⚠️ | `ToolStripContainer` / `ToolStripPanel` | Auto-show/hide bands; row layout |
| ⚠️ | **`PropertyGrid`** | Reflection-based two-column property browser; `SelectedObject`/`SelectedObjects`; `PropertySort`; `HelpVisible`; `ToolbarVisible`; inline editing; `SelectedGridItemChanged`; `PropertyValueChanged`; **nested object expansion** (sub-properties up to depth 2); **read-only greying**; **bold non-default values**; **enum/bool dropdown overlay** (dropdown arrow button, Enter/F2/click opens list, Up/Down/Enter/Escape nav, Space toggles bool) |
| ⚠️ | **`TrackBar`** | Slider; Horizontal/Vertical; tick marks; keyboard/mouse; SetRange |
| ⚠️ | **`HScrollBar` / `VScrollBar`** | Standalone scrollbars; SmallChange/LargeChange; Scroll/ValueChanged events; mouse wheel; WinForms effective-maximum clamping (Maximum − LargeChange + 1) |
| ⚠️ | **`DomainUpDown`** | String-list up-down; Sorted/Wrap; SelectedItem/SelectedIndex; mouse wheel scrolling; first-letter type-ahead; Home/End keyboard navigation |
| ⚠️ | **`HelpProvider`** | F1 help integration; per-control HelpString/HelpKeyword; browser tab for URLs; JS alert for text |
| ⚠️ | **`ToolStripProgressBar`** | Hosted progress bar in ToolStrip/StatusStrip; Value/Min/Max/Step; inline canvas rendering; default Width=75; layout-aware width from inner ProgressBar |
| ⚠️ | **`ToolStripSplitButton`** | Split-action toolbar button; face click + dropdown; `DropDownClosed`/`DropDownOpened`/`DropDownOpening` events; hosted ProgressBar(75px)/TextBox(100px)/ComboBox(100px) rendering with layout-aware widths |
| ⚠️ | **`PrintDialog`** | Canvas dialog (printer/copies/range); calls `PrintDocument.Print()` on OK; submits `PrintJob` to `IHostPrintService` |
| 🧩 | **`PrintPreviewDialog`** | Placeholder preview; `PrintPreviewControl` renders stub page |
| ⚠️ | **`PrintDocument`** | `Print()` drives `PrintPage` events with `PrintGraphics` capture surface; multi-page; submits `PrintJob` to `IHostPrintService` |

### Tier 3 — Lower priority / legacy compat

| Status | Control | Notes |
|--------|---------|-------|
| ⚠️ | **`DataGrid`** (legacy) | Subclasses DataGridView; TableStyles; DataGridTableStyle/ColumnStyle; CaptionText; DataGridCell |
| ✅ | **`BindingSource`** | IList/IBindingList wrapper; `Current`/`Position`; functional `Filter` (equality predicate) + `Sort` (multi-column ASC/DESC) + `Find`; DataView delegates to native RowFilter/Sort; server-backed via `CanvasDataService` |
| ⚠️ | **`BindingNavigator`** | Record-navigation bar; First/Prev/Next/Last/Add/Delete; bound to `BindingSource.Position`; **editable `PositionItem` textbox** (type 1-based record number + Enter to jump); `CountItem` label |
| ⚠️ | **`StatusBar`** (legacy) | Panels; ShowPanels; SizingGrip; spring sizing; OwnerDraw; DrawItem event |
| ⚠️ | **`ToolBar`** (legacy) | Pre-`ToolStrip`; wraps `ToolStrip`; `ToolBarButton` / `ButtonClick`; `Appearance`, `TextAlign`, `ImageList`, `Wrappable`; **`DropDownButton` style** with live arrow+menu via `ToolStripDropDownButton`; `DropDownMenu` accepts `ContextMenu`/`MainMenu`; **`DrawItem` owner-draw** |
| ⚠️ | **`MainMenu`** (legacy) | Pre-`MenuStrip`; `MenuItem` collection; `Form.Menu` property; `MenuItem`: **`Click`/`Popup`/`Select` events**; **`RadioCheck`** mutual-exclusion; `PerformClick()` |
| ⚠️ | **`ContextMenu`** (legacy) | Pre-`ContextMenuStrip`; `MenuItem` collection; `Popup` event; `Control.ContextMenu` wires to `ContextMenuStrip` |
| ⚠️ | **`Splitter`** (legacy) | Docking drag-resize; MinSize/MinExtra; SplitterMoving/SplitterMoved; cursor follows dock |
| 🧩 | **`PrintPreviewControl`** | Embedded print-preview stub; renders placeholder page; full WinForms API surface |
| ⚠️ | **`Screen`** | `PrimaryScreen`/`AllScreens`; `Bounds` from `window.screen`; `WorkingArea` from `window.innerWidth/Height`; `FromControl`/`FromPoint`/`GetWorkingArea`/`GetBounds`; JS interop via `getScreenInfo`; 1920×1080 fallback; no multi-monitor |
| ⚠️ | **`Clipboard`** | SetText/GetText (plain + HTML formats); async SetTextAsync/GetTextAsync/SetHtmlAsync/GetHtmlAsync; SetDataObject/GetDataObject; ContainsText/ContainsData; Clear; JS interop bridge |
| ⚠️ | **`WebBrowser` / WebView2** | iframe overlay; Navigate, GoBack/Forward, Stop, Refresh, DocumentText, ExecuteScriptAsync, events; cross-origin DOM access blocked by browser sandbox |
| ⚠️ | **MDI (`MdiClient`, MDI Forms)** | `IsMdiContainer`, `MdiParent`, `MdiChildren`, `ActiveMdiChild`, `ActivateMdiChild`, `LayoutMdi` (Cascade/TileH/TileV/ArrangeIcons), `MdiChildActivate`; `MdiClientArea` Blazor component renders child windows with title bar, min/max/restore/close, and minimize strip; Ctrl+Tab / Ctrl+Shift+Tab child cycling; `ArrangeIcons` slots minimized children along bottom |
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
| `WebForms.Canvas/Theming/CanvasTheme.cs` | Theme token model — all named colour properties |
| `WebForms.Canvas/Theming/CanvasThemeRegistry.cs` | Built-in theme registry (Classic / Light / Dark) + `Apply()` |
| `WebForms.Canvas/Theming/ThemePickerForm.cs` | Theme picker dialog (canvas WinForms Form) |

---

## License

No license file is currently included in this repository.
