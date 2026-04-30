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

Drawing commands are buffered and dispatched to the JS renderer via Blazor JS interop.

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
- `NotifyIcon`

### Not implemented yet (common WinForms controls)

This list is not exhaustive; it’s meant to highlight frequently used WinForms controls that are not currently present in this repo.

**Menus / toolbars**
- `MainMenu`, `ContextMenu`
- `MenuStrip`, `ContextMenuStrip`, `ToolStrip`, `StatusStrip`

**Data / inspection**
- `DataGridView`
- `PropertyGrid`

**Value/input**
- `TrackBar`
- `ScrollBar` (`HScrollBar`, `VScrollBar`)
- `DomainUpDown`

**Rich UI controls**
- `ListView` / `TreeView` advanced features (icons, in-place label editing, full keyboard/mouse parity, virtualization)
- `ListView` groups, image lists, and owner-draw modes

**Dialogs**
- `OpenFileDialog`, `SaveFileDialog`, `FolderBrowserDialog`
- `ColorDialog`, `FontDialog`, `PrintDialog`

**Other common controls**
- `WebBrowser`
- `Chart`

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
| ⚠️ | `TextBox` / `TextBoxBase` | Editing, selection, shortcuts; autocomplete evolving |
| ⚠️ | `Label` | Multi-line, alignment, approximate measurement |
| ⚠️ | `ComboBox` | Drop-down + selection; autocomplete partial |
| ⚠️ | `ListBox` | Selection + navigation; advanced modes missing |
| ⚠️ | `Panel` / `ScrollableControl` | Child painting, input routing, scroll offset |
| ⚠️ | `GroupBox` | Border/caption + child routing |
| ⚠️ | `TabControl` | Tab strip + page switching |
| ⚠️ | `MenuStrip` | Top-level menu bar with dropdowns |
| ⚠️ | `ContextMenuStrip` | Right-click overlay menus |
| ⚠️ | `ToolStrip` | Toolbar with icons, hover, checked state |
| ⚠️ | `StatusStrip` / `ToolStripStatusLabel` | Status bar; Spring, BorderSides, SizingGrip |
| ⚠️ | `SplitContainer` | Resizable pane splitter |
| ⚠️ | `FlowLayoutPanel` | FlowDirection + wrap/break |
| ⚠️ | `TableLayoutPanel` | Row/column styles + spans |
| ⚠️ | `DateTimePicker` | Simplified text + drop-down calendar |
| ⚠️ | `NumericUpDown` | Spinner UI + value clamping |
| ⚠️ | `PictureBox` | URL-based image loading |
| ⚠️ | `ProgressBar` | Blocks/continuous/marquee |
| ⚠️ | `TreeView` | Nodes, expand/collapse, selection |
| ⚠️ | `ListView` | Details view + columns; growing |
| ⚠️ | `OpenFileDialog` | Host FS + browser upload |
| 🧩 | `ToolTip` | API present; rendering may be incomplete |
| 🔲 | **`DataGridView`** | ⭐ Highest-impact missing control; used in nearly every business app |
| ✅ | `Timer` | `PeriodicTimer`-based async loop; `Interval`, `Enabled`, `Start()`, `Stop()`, `Tick`, `Tag`, `IContainer` ctor; fires on captured `SynchronizationContext` |
| 🔲 | **`ErrorProvider`** | Standard form validation; common in data-entry forms |
| ⚠️ | `SaveFileDialog` | Inherits full FileDialog UI; `CreatePrompt`, `OverwritePrompt`, `OpenFile()` |
| ⚠️ | `FolderBrowserDialog` | `SelectedPath`, `Description`, `RootFolder`, `ShowNewFolderButton`, `InitialDirectory`; host FS aware |
| ⚠️ | `ColorDialog` | Swatch palette + Hex/RGB/HSV inputs; `Color`, `AllowFullOpen`, `CustomColors`, `FullOpen` |
| ⚠️ | `FontDialog` | Family/style/size lists; `ShowEffects`, `ShowColor`, `MinSize`/`MaxSize`, `Apply` event |

### Tier 2 — Medium priority

| Status | Control | Notes |
|--------|---------|-------|
| ⚠️ | `RichTextBox` | Stores RTF, renders as plain text |
| ⚠️ | `MaskedTextBox` | Masked display + basic validation |
| ⚠️ | `CheckedListBox` | Basic checked item behaviour |
| ⚠️ | `MonthCalendar` | Single-month view + keyboard/mouse |
| 🧩 | `NotifyIcon` | API present; system tray stub |
| 🧩 | `UserControl` | Base present; full composite lifecycle partial |
| 🧩 | `ToolStripMenuItem` | Dropdowns, check state, shortcuts |
| 🧩 | `ToolStripContainer` / `ToolStripPanel` | Dockable strip host |
| 🔲 | **`PropertyGrid`** | Common in tools and settings panels |
| 🔲 | **`TrackBar`** | Slider; common in settings/media UIs |
| 🔲 | **`HScrollBar` / `VScrollBar`** | Standalone scrollbars used in legacy apps |
| 🔲 | **`DomainUpDown`** | Text-based up-down; pair to `NumericUpDown` |
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
| 🔲 | **`BindingSource`** | Data-binding plumbing; used with `DataGridView` |
| 🔲 | **`BindingNavigator`** | Record-navigation bar; paired with `BindingSource` |
| 🔲 | **`StatusBar`** (legacy) | Pre-`StatusStrip`; thin wrapper for translator compat |
| 🔲 | **`ToolBar`** (legacy) | Pre-`ToolStrip` |
| 🔲 | **`MainMenu`** (legacy) | Pre-`MenuStrip` |
| 🔲 | **`ContextMenu`** (legacy) | Pre-`ContextMenuStrip` |
| 🔲 | **`Splitter`** (legacy) | Pre-`SplitContainer` |
| 🔲 | **`PrintPreviewControl`** | Embedded (non-dialog) print preview |
| 🔲 | **`Screen`** | Multi-monitor info; `Screen.PrimaryScreen` stub needed |
| 🔲 | **`Clipboard`** | Cut/Copy/Paste; requires JS bridge |
| 🔲 | **`WebBrowser` / WebView2** | Embedded web content; stub for compatibility |
| 🔲 | **MDI (`MdiClient`, MDI Forms)** | MDI window management; enterprise apps |
| 🔲 | **`DataGridViewColumn` types** | TextBox/CheckBox/ComboBox/Button/Image/Link column variants |

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
