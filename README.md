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

See `COMPATIBILITY_REVIEW.md` for a full per-control breakdown, and the test project for property-level tracking:

- `Canvas.Windows.Forms.Tests/PROPERTY_COMPLETENESS.md`
- `Canvas.Windows.Forms.Tests/PROPERTY_FUNCTIONALITY.md`

---

## Limitations

- No HWND / native Windows handles
- No P/Invoke or native Windows APIs
- No Visual Studio WinForms designer support
- Single-threaded (WASM) — `InvokeRequired` always returns `false`
- Local-network scope only (no internet/auth hardening yet)

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
