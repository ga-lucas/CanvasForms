# Development Workflow & Caching Guide

## ✅ Fixed Issues

The project is now configured to handle development builds without aggressive caching:

### Configuration Changes

1. **Debug vs Release builds**:
   - **Debug**: No fingerprinting, relies on HTTP `no-cache` headers
   - **Release**: Full fingerprinting with cache-friendly headers

2. **HTTP Headers** (automatic):
   - Development: `Cache-Control: no-cache, no-store, must-revalidate`
   - Production: `Cache-Control: public, max-age=31536000, immutable`

3. **HTML meta tags**: Prevent HTML page caching

## 🚀 Normal Development Workflow

After the initial clean/rebuild, you should be able to:

1. **Make code changes** to any .cs file
2. **Just build** (no clean needed):
   ```powershell
   dotnet build Canvas.Windows.Forms.Host.Server
   ```
   Or use the quick rebuild script:
   ```powershell
   .\rebuild-dev.ps1
   ```

3. **Refresh the browser** (F5 or Ctrl+R)
   - No hard refresh needed
   - No manual cache clearing needed

## 🔧 Troubleshooting

### If changes don't appear:

1. **Check you're in Debug mode** (not Release)
2. **Use DevTools "Disable cache"**:
   - Press F12 to open DevTools
   - Go to Network tab
   - Check "Disable cache" (while DevTools is open)

3. **Hard refresh once**: Ctrl+Shift+R (Cmd+Shift+R on Mac)

4. **Clear browser cache** (one-time):
   - Chrome/Edge: Ctrl+Shift+Delete → Clear cached images and files
   - Firefox: Ctrl+Shift+Delete → Cached Web Content

### If you still see old code:

```powershell
# Full clean and rebuild
dotnet clean Canvas.Windows.Forms.Host.Server
dotnet build Canvas.Windows.Forms.Host.Server
```

Then clear browser cache once.

## 🎯 Known Warnings (Safe to Ignore)

### CSS Hot Reload Warning
```
CSS Hot Reload ignoring http://localhost:5001/css/bootstrap/bootstrap.min.css 
because it was inaccessible or had more than 7000 rules.
```

**This is normal** - Bootstrap has >7000 CSS rules, which exceeds the hot reload limit. 
It doesn't affect functionality.

## 📁 Quick Reference

### Build Commands

| Command | Purpose |
|---------|---------|
| `.\rebuild-dev.ps1` | Quick rebuild for development |
| `dotnet build Canvas.Windows.Forms.Host.Server` | Build server (includes WASM client) |
| `dotnet run --project Canvas.Windows.Forms.Host.Server` | Run development server |
| `dotnet clean && dotnet build` | Full clean rebuild (rarely needed) |

### Browser Shortcuts

| Shortcut | Action |
|----------|--------|
| F5 | Normal refresh |
| Ctrl+R | Reload page |
| Ctrl+Shift+R | Hard refresh (bypass cache) |
| Ctrl+Shift+Delete | Clear browser cache |
| F12 → Network → "Disable cache" | Disable cache while DevTools open |

## 🏗️ How It Works

### Development (Debug Configuration)

1. Files are served **without fingerprinting**
2. HTTP headers force browsers to check for updates: `no-cache`
3. Blazor boot files are regenerated on every build: `BlazorCacheBootResources=false`
4. No query parameters or hash suffixes on file names

### Production (Release Configuration)

1. Files get **content-based hash fingerprints** (e.g., `app.a3b5c7d9.js`)
2. HTTP headers allow aggressive caching: `max-age=31536000, immutable`
3. Boot files are cached for performance
4. Browser automatically fetches new files when fingerprint changes

## 🐛 Debugging Cache Issues

If you suspect caching problems:

1. **Check the configuration**:
   ```powershell
   dotnet build -c Debug  # Should NOT have fingerprinting
   dotnet build -c Release  # SHOULD have fingerprinting
   ```

2. **Inspect HTTP headers** (F12 → Network tab → click a file):
   - Debug should show: `Cache-Control: no-cache, no-store`
   - Release should show: `Cache-Control: public, max-age=31536000`

3. **Check file names** in browser DevTools Network tab:
   - Debug: `Canvas.Windows.Forms.wasm`
   - Release: `Canvas.Windows.Forms.a3b5c7d9.wasm`

## 📝 Notes

- The project uses .NET 10 full release (not preview)
- Blazor WebAssembly is hosted on ASP.NET Core server
- Static files are served from the server's augmented WebRootFileProvider
- SignalR is used for desktop change notifications

---

**Last updated**: Based on .NET 10 release configuration
