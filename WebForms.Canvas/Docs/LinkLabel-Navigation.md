# LinkLabel URL Navigation

## Overview

The `LinkLabel` control now supports automatic browser navigation when clicked. Simply set the `LinkUrl` property to any valid URL, and clicking the link will open it in a new browser window/tab.

## Basic Usage

### Example 1: Simple URL Link

```csharp
var linkLabel = new LinkLabel
{
    Text = "Visit GitHub",
    LinkUrl = "https://github.com",
    Location = new Point(10, 10),
    AutoSize = true
};
form.Controls.Add(linkLabel);
```

When the user clicks "Visit GitHub", a new browser tab will open to https://github.com.

### Example 2: With LinkClicked Event

```csharp
var linkLabel = new LinkLabel
{
    Text = "Documentation",
    LinkUrl = "https://docs.microsoft.com",
    Location = new Point(10, 40)
};

linkLabel.LinkClicked += (sender, e) =>
{
    MessageBox.Show("Opening documentation in new window...");
};

form.Controls.Add(linkLabel);
```

The `LinkClicked` event fires **after** the URL is opened, allowing you to perform additional actions.

### Example 3: Programmatic Navigation

```csharp
var linkLabel = new LinkLabel
{
    Text = "Click me",
    Location = new Point(10, 70)
};

// Set URL later
linkLabel.LinkUrl = "https://example.com";

// Or handle navigation manually in the event
linkLabel.LinkClicked += async (sender, e) =>
{
    // Custom logic
    if (ShouldNavigate())
    {
        await BrowserNavigationService.OpenUrlAsync("https://custom-url.com");
    }
};

form.Controls.Add(linkLabel);
```

## Properties

### `LinkUrl` (string)
- Gets or sets the URL to navigate to when clicked
- Default: empty string (no automatic navigation)
- Example: `"https://github.com/ga-lucas/CanvasForms"`

### Existing Properties
All standard LinkLabel properties are still available:
- `LinkColor` - Color of unvisited links (default: blue)
- `VisitedLinkColor` - Color after clicking (default: purple)
- `ActiveLinkColor` - Color while hovering (default: red)
- `LinkBehavior` - Underline behavior
- `LinkVisited` - Whether the link has been clicked

## Advanced: BrowserNavigationService

For more control over navigation, use the `BrowserNavigationService` directly:

```csharp
using Canvas.Windows.Forms;

// Open URL in new tab/window
await BrowserNavigationService.OpenUrlAsync("https://example.com");

// Open with specific window features
await BrowserNavigationService.OpenUrlAsync(
    "https://example.com",
    "_blank",
    "width=800,height=600,menubar=no,toolbar=no"
);
```

## Notes

- URLs are opened in a **new browser tab/window** (uses `window.open(..., "_blank")`)
- The navigation is **asynchronous** and non-blocking
- If JavaScript interop is unavailable, navigation fails silently
- The `LinkVisited` property automatically updates when clicked
- Works in both standalone mode and hosted environments

## Migration from WinForms

Standard WinForms code using `System.Diagnostics.Process.Start()` needs to be updated:

### Before (WinForms):
```csharp
linkLabel.LinkClicked += (s, e) =>
{
    System.Diagnostics.Process.Start("https://example.com");
};
```

### After (CanvasForms):
```csharp
linkLabel.LinkUrl = "https://example.com";
// LinkClicked event is optional
```

Or use the service directly:
```csharp
linkLabel.LinkClicked += async (s, e) =>
{
    await BrowserNavigationService.OpenUrlAsync("https://example.com");
};
```
