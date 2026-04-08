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
