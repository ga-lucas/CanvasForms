# Maximize/Restore Features

## Recent Fixes

### Immediate Rendering on Double-Click Maximize (Fixed)
**Issue:** When double-clicking to maximize, the form would move but not resize/change icon until clicked again.

**Root Cause:** The `ToggleMaximize()` method wasn't forcing an immediate re-render after changing window state. Blazor's state management wasn't picking up the changes immediately.

**Solution:** 
- Made `ToggleMaximize()` async
- Added multiple `StateHasChanged()` calls with `Task.Yield()` between them
- This forces Blazor to process the state changes on the UI thread
- Added explicit `await RenderForm()` call after state change
- Ensures immediate visual update of size and icon

```csharp
// Force multiple state updates to ensure immediate visual update
StateHasChanged();
await Task.Yield(); // Let Blazor process the state change
StateHasChanged();
await RenderForm();
StateHasChanged();
```

### Windows-Style Drag from Maximize (Fixed)
**Issue:** When dragging a maximized window, it would stay at full size instead of restoring to previous size.

**Root Cause:** The drag operation didn't detect maximized state and restore the window first. Additionally, Blazor wasn't immediately updating the UI to reflect the size change.

**Solution:** Enhanced `StartDrag()` to:
1. Detect if window is maximized when drag starts
2. Automatically restore the window to previous size
3. Calculate mouse position ratio to keep cursor under the same relative spot
4. Position the restored window so it "sticks" to the cursor
5. Force multiple state updates with `Task.Yield()` to ensure Blazor processes changes
6. Trigger explicit rendering
7. This matches Windows desktop behavior perfectly

```csharp
// If window is maximized, restore it first (Windows-style behavior)
if (Form.WindowState == FormWindowState.Maximized)
{
    // Calculate where the mouse is relative to the window width
    var mouseXRatio = e.OffsetX / Form.Width;

    // Restore the window
    Form.Restore();

    // Trigger layout update after restore
    Form.PerformLayout();

    // Adjust position so mouse stays under same relative position
    _formStartLeft = (int)(e.ClientX - (Form.Width * mouseXRatio));
    _formStartTop = 0;

    // Set the form to the calculated position immediately
    Form.Left = _formStartLeft;
    Form.Top = _formStartTop;

    // Force multiple state updates to ensure Blazor picks up the changes
    StateHasChanged();
    await Task.Yield(); // Yield to allow Blazor to process
    StateHasChanged();
    await RenderForm();
    StateHasChanged();
}
```

---

## Overview
Enhanced the window management system with two key features:
1. **Dynamic Maximize Button Icon** - Shows different icons for maximize vs. restore states
2. **Title Bar Double-Click** - Toggles maximize/restore on double-click

## Feature 1: Dynamic Maximize Button Icon

### Visual Design
The maximize button now displays context-aware icons:

**Normal State (Not Maximized):**
```
┌─────┐
│     │  Single square outline
│     │  Represents "maximize window"
└─────┘
```

**Maximized State:**
```
┌───┐
│ ┌─┼─┐  Two overlapping squares
└─┼─┘ │  Represents "restore window"
  └───┘
```

### Implementation Details

**JavaScript (`canvas-renderer.js`):**
- Updated `renderFormCanvas()` signature to accept `isMaximized` boolean parameter
- Modified maximize button rendering logic:
  ```javascript
  if (isMaximized) {
      // Draw two overlapping squares (restore icon)
      ctx.strokeRect(maximizeButtonX + 7, buttonY + 5, buttonSize - 12, buttonSize - 12);
      ctx.strokeRect(maximizeButtonX + 5, buttonY + 7, buttonSize - 12, buttonSize - 12);
  } else {
      // Draw single square (maximize icon)
      ctx.strokeRect(maximizeButtonX + 5, buttonY + 5, buttonSize - 10, buttonSize - 10);
  }
  ```

**FormRenderer.razor:**
- Updated `RenderForm()` to pass window state:
  ```csharp
  await JSRuntime.InvokeVoidAsync("renderFormCanvas", _canvasRef, 
      // ... other parameters ...
      Form.WindowState == FormWindowState.Maximized);
  ```

### User Experience
- Visual feedback clearly indicates current window state
- Matches standard desktop windowing system conventions
- Icon automatically updates when:
  - Maximize button is clicked
  - Form is maximized/restored programmatically
  - Title bar is double-clicked (see Feature 2)

## Feature 2: Title Bar Double-Click to Maximize/Restore

### Behavior
Double-clicking on the title bar toggles between:
- **Normal → Maximized:** Form expands to fill desktop area
- **Maximized → Normal:** Form restores to previous size and position

### Implementation Details

**FormRenderer.razor - HandleDoubleClick():**
```csharp
private void HandleDoubleClick(BlazorMouseEventArgs e)
{
    if (Form == null) return;

    var x = (int)e.OffsetX;
    var y = (int)e.OffsetY;

    // Check if double-click is on title bar
    if (y < TitleBarHeight + BorderWidth && y >= BorderWidth)
    {
        // Get window control button rectangles
        var closeButtonRect = GetCloseButtonRect();
        var maximizeButtonRect = GetMaximizeButtonRect();
        var minimizeButtonRect = GetMinimizeButtonRect();

        // Only toggle if NOT clicking on control buttons
        if (!IsPointInRect(x, y, closeButtonRect) &&
            !IsPointInRect(x, y, maximizeButtonRect) &&
            !IsPointInRect(x, y, minimizeButtonRect))
        {
            ToggleMaximize();
            return;
        }
    }

    // ... handle client area double-clicks ...
}
```

### Smart Hit Testing
The implementation uses precise hit testing to:
- ✅ **Allow:** Double-click on empty title bar area
- ❌ **Prevent:** Double-click on close button
- ❌ **Prevent:** Double-click on maximize button
- ❌ **Prevent:** Double-click on minimize button

This ensures that:
- Users can't accidentally maximize when trying to close
- Double-clicking buttons performs their primary action only
- Maximum usable title bar area for double-click gesture

### User Experience Benefits
1. **Faster Workflow:** No need to precisely click the small maximize button
2. **Familiar Behavior:** Matches Windows, macOS, and Linux desktop conventions
3. **Large Hit Target:** Entire title bar is a target (except buttons)
4. **Intuitive:** Users familiar with desktop apps expect this behavior

## Integration with Existing Features

### Title Bar Visibility
Both features work seamlessly with the title bar visibility constraints:
- Forms can be maximized even when partially off-screen
- Restoring from maximized returns to constrained position
- Title bar remains accessible after restore

### Window State Management
- Properly updates `Form.WindowState` property
- Triggers `WindowStateChanged` event
- Maintains form bounds history for restore operation

## Testing Scenarios

### Test 1: Icon Toggle on Button Click
1. Create a form in normal state
2. Click maximize button
3. ✓ Icon should change to restore (two squares)
4. Click again
5. ✓ Icon should change to maximize (single square)

### Test 2: Icon Toggle on Double-Click
1. Create a form in normal state
2. Double-click title bar
3. ✓ Form maximizes and icon changes to restore
4. Double-click title bar again
5. ✓ Form restores and icon changes to maximize

### Test 3: Button Exclusion
1. Create a form in normal state
2. Double-click on close button
3. ✓ Form should close (not maximize)
4. Create another form
5. Double-click on minimize button
6. ✓ Form should minimize (not maximize)

### Test 4: State Consistency
1. Maximize form via button
2. Restore via title bar double-click
3. ✓ Form should return to previous size/position
4. Maximize via title bar double-click
5. Restore via button
6. ✓ Form should return to same position

## Files Modified

1. **`WebForms.Canvas\wwwroot\canvas-renderer.js`**
   - Added `isMaximized` parameter to `renderFormCanvas()`
   - Implemented conditional icon rendering for maximize/restore

2. **`WebForms.Canvas\Components\FormRenderer.razor`**
   - Updated `RenderForm()` to pass maximized state
   - Enhanced `HandleDoubleClick()` with title bar detection
   - Added smart hit testing to exclude control buttons

3. **`FEATURE_TITLE_BAR_VISIBILITY.md`**
   - Added "Recent Enhancements" section
   - Documented both new features

## Code Example

```csharp
// Create a form
var form = new Form
{
    Text = "My Form",
    Width = 800,
    Height = 600
};
form.Show();

// Users can now:
// 1. Double-click title bar to maximize
// 2. See the icon change to "restore"
// 3. Double-click again to restore
// 4. See the icon change back to "maximize"
// 5. All while maintaining title bar visibility constraints
```

## Future Enhancements

Potential improvements:
- Animate the maximize/restore transition
- Add keyboard shortcuts (F11, Alt+F10, etc.)
- Support snap-to-edge gestures (Windows Aero Snap-like)
- Add maximize to different monitors in multi-monitor setups
