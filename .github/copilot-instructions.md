# Copilot Instructions

## Project Guidelines
- Build a cross-platform WinForms “emulator”/compatibility layer that can run as many existing compiled WinForms apps as possible, rendering UI through HTML canvas in the browser while executing app logic via Blazor Server/host services, with no dependency on Windows.
- For this workspace, prioritize maximum compatibility with the WinForms SDK API/behavior, even if it introduces breaking changes to existing code.
- Prioritize implementing behavior as closely as possible to real WinForms across all controls (compatibility over preserving existing behavior).
- Change `CanvasForms` `Control.BackColor` and `Control.ForeColor` (and default colors) to use `System.Drawing.Color` for WinForms API compatibility with translated designer apps; keep rendering conversion internally via existing `Canvas.Windows.Forms.Drawing.Color` implicit conversions. Focus on cross-platform compatibility via `System.Drawing.Primitives` (avoid `System.Drawing.Common`/GDI+).
- The project scope is Mode A (host-run logic + canvas-rendered UI), intended for same-machine/local-network development use only for now; do not design for internet exposure. Security concerns can be scoped accordingly (localhost/limited origins acceptable).
- Evolve this repository as a proof of concept (PoC) to gradually increase WinForms compatibility for translated (ILTranslator) WinForms apps (designer-generated). Ensure that apps utilize the host/server for storage.

## Known Issues
- Currently experiencing a runtime launch failure when a translated app sets `Form.BackColor`. Investigate and resolve this issue.
- Custom drawing support is desired; refer to `SampleDrawingForm` for implementation guidance.