# Canvas.Windows.Forms Host Server

This is the unified "OS" for running Canvas.Windows.Forms applications.

## Architecture

```
Canvas.Windows.Forms.Host.Server (ASP.NET Core)
├── Hosts the Blazor WASM client
├── AppRuntime - Runs one app at a time (native or translated)
├── AppManager - Install/uninstall translated apps
├── SignalR Hub - Real-time UI streaming and input
└── IL Translation - Uses Mono.Cecil to retarget assemblies

Canvas.Windows.Forms.Host (Blazor WASM Client)
├── OS.razor - Main "desktop" UI
├── Canvas rendering via SignalR
└── Input forwarding to server
```

## Running

```bash
dotnet run --project Canvas.Windows.Forms.Host.Server
```

Then open https://localhost:7001 in your browser.

## Visual Studio (F5) Notes

This solution hosts a standalone Blazor WebAssembly client. In development, the browser can cache older `/_framework/*` resources while the build produces new (fingerprinted) file names, which can cause F5 runs to fail with missing resource errors.

Mitigations in this repo:

- The WASM host enables .NET 10 HTML asset placeholder replacement (`OverrideHtmlAssetPlaceholders`) and uses the fingerprinted `blazor.webassembly#[.{fingerprint}].js` pattern.
- The server disables HTTP caching in Development.
- The server disables Visual Studio's fast up-to-date check in Debug to reduce stale static-web-assets issues.

If you still see `404`/missing requests under `/_framework/`:

1. Hard refresh the page (Ctrl+F5) or clear site data for `localhost`.
2. Rebuild the WebAssembly + server projects (the root `rebuild-dev.ps1` script is a quick way to do this).

## Features

### Native Apps
- Built directly with Canvas.Windows.Forms
- Run with `POST /api/demo/run`
- Example: `InteractiveForm` demo

### Translated Apps (Uploaded)
- Standard WinForms apps (.exe/.dll)
- Upload via the "Install App..." menu
- IL is rewritten to target Canvas.Windows.Forms
- Dependencies uploaded together

## Windows Forms Control Support

This repo is a WinForms compatibility layer. The goal is API/behavior compatibility first; visual fidelity and edge-case behavior may vary.

### Implemented (at least basic rendering + input)

| Area | Controls |
|------|----------|
| Windowing | `Form` |
| Base | `Control`, `ContainerControl`, `ScrollableControl` |
| Layout / containers | `Panel`, `GroupBox`, `TabControl`, `SplitContainer`, `TableLayoutPanel`, `FlowLayoutPanel` |
| Buttons | `Button`, `CheckBox`, `RadioButton` |
| Text | `Label`, `LinkLabel`, `TextBox`, `MaskedTextBox`, `RichTextBox` |
| Lists / hierarchy | `ListBox`, `CheckedListBox`, `ComboBox`, `ListView`, `TreeView` |
| Date / value | `DateTimePicker`, `MonthCalendar`, `NumericUpDown` |
| Display | `PictureBox`, `ProgressBar` |
| Non-visual / helpers | `ToolTip`, `NotifyIcon` |

### Not implemented yet (common WinForms controls)

This list is not exhaustive, but covers the most commonly requested controls that are not currently present in `WebForms.Canvas/Forms/*`.

| Area | Controls |
|------|----------|
| Menus / toolbars | `MenuStrip`, `ContextMenuStrip`, `ToolStrip`, `StatusStrip` |
| Data | `DataGridView`, `PropertyGrid` |
| Input | `TrackBar`, `ScrollBar` (`HScrollBar`, `VScrollBar`), `DomainUpDown` |
| Display | `ImageList` (full integration), `ListView`/`TreeView` icons |
| Dialogs | `OpenFileDialog`, `SaveFileDialog`, `FolderBrowserDialog`, `ColorDialog`, `FontDialog` |

Notes:
- `ListView` / `TreeView` exist, but some WinForms features are not yet supported (for example: icons, virtualization, in-place label editing, and full keyboard/mouse parity).

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/status` | Current runtime status |
| GET | `/api/apps` | List installed apps |
| POST | `/api/apps` | Upload & install app (multipart form) |
| DELETE | `/api/apps/{id}` | Uninstall app |
| POST | `/api/apps/{id}/run` | Run installed app |
| POST | `/api/demo/run` | Run native demo app |
| POST | `/api/stop` | Stop current app |

## SignalR Hub (`/hub`)

| Method | Description |
|--------|-------------|
| `GetDesktop()` | Get current desktop snapshot |
| `Render()` | Get render frame for current form |
| `MouseEvent(type, x, y, button)` | Send mouse input |
| `KeyEvent(type, keyCode, alt, ctrl, shift, char)` | Send keyboard input |

### Server-to-Client Events
- `DesktopChanged(snapshot)` - Desktop state changed
- `RenderFrame(frame)` - New render frame available
