# Form Title Bar Visibility Feature

## Recent Enhancements

### Maximize/Restore Icon Toggle (New)
**Feature:** The maximize button now displays different icons based on the form's window state:
- **Maximized:** Shows a "restore" icon (two overlapping squares)
- **Normal:** Shows a "maximize" icon (single square)

**Implementation:** 
- Updated `renderFormCanvas()` JavaScript function to accept `isMaximized` parameter
- Modified icon rendering logic to draw different icons based on state
- FormRenderer now passes `Form.WindowState == FormWindowState.Maximized` to the render function

### Title Bar Double-Click (New)
**Feature:** Double-clicking on the title bar now toggles between maximized and normal states.

**Implementation:**
- Enhanced `HandleDoubleClick()` to detect clicks on the title bar area
- Excludes clicks on window control buttons (close, minimize, maximize)
- Calls `ToggleMaximize()` when title bar is double-clicked
- Maintains existing double-click behavior for client area

---

## Recent Fixes

### Maximize Offset Bug (Fixed)
**Issue:** When maximizing a form, it had an offset (approximately 50px from the top), not properly filling the desktop area.

**Root Cause:** The `Maximize()` method was setting `Top = taskbarHeight`, but form positions are relative to the desktop area (which already starts below the taskbar), not the full viewport.

**Solution:** Changed `Top = taskbarHeight` to `Top = 0` in the `Maximize()` method. Also updated `ToggleMaximize()` to use actual viewport dimensions instead of hardcoded values.

### Bottom Drag-Off Bug (Fixed)
**Issue:** Forms could be dragged off the bottom of the screen, making the title bar inaccessible.

**Root Cause:** The `maxTop` calculation didn't account for the taskbar height when constraining the form position.

**Solution:** Changed the calculation from `maxTop = viewportHeight - TitleBarHeight` to `maxTop = viewportHeight - taskbarHeight - TitleBarHeight`.

---

## Overview
This feature ensures that form title bars remain visible and accessible at all times, even when forms are moved, resized, or when the browser viewport changes.

## Implementation Details

### 1. Form Class (`WebForms.Canvas\Forms\Form.cs`)
Added a new public method `EnsureTitleBarVisible()` that:
- Ensures the form's top position is not negative (stays within desktop area)
- Ensures the title bar doesn't extend below the bottom of the viewport (accounting for taskbar height)
- Keeps at least 50 pixels of the form visible from the left side for grab access
- Positions forms that are too wide to fit as far left as possible
- Only applies to forms in `Normal` state (not minimized or maximized)

**Key Fix:** The bottom constraint now correctly calculates `maxTop = viewportHeight - taskbarHeight - TitleBarHeight` to account for the fact that form positions are relative to the desktop area (below the taskbar), not the full viewport.

**Method Signature:**
```csharp
public void EnsureTitleBarVisible(int viewportWidth, int viewportHeight, int taskbarHeight)
```

### 2. FormRenderer Component (`WebForms.Canvas\Components\FormRenderer.razor`)
Enhanced to:
- Accept viewport dimensions and taskbar height as parameters
- Call `EnsureTitleBarVisible()` when dragging or resizing is finished
- Provide a `OnViewportChanged()` JSInvokable method for viewport resize events
- Track viewport size changes from JavaScript

**New Parameters:**
- `ViewportWidth` - Current viewport width
- `ViewportHeight` - Current viewport height  
- `TaskbarHeight` - Height of the taskbar

### 3. Desktop Component (`WebForms.Canvas\Components\Desktop.razor`)
Enhanced to:
- Track viewport dimensions
- Set up viewport resize listeners on initialization
- Pass viewport dimensions to all FormRenderer instances
- Automatically adjust all visible forms when viewport size changes
- Implements `IDisposable` for proper cleanup

**New Features:**
- `OnViewportResize()` - JSInvokable method called when browser window is resized
- Automatic form repositioning on viewport changes
- Debounced resize handling (100ms delay)

### 4. JavaScript (`WebForms.Canvas\wwwroot\canvas-renderer.js`)
Added new function:
- `setupViewportTracking(dotNetRef)` - Sets up viewport resize listener
  - Gets initial viewport dimensions
  - Registers global resize event handler
  - Debounces resize events (100ms)
  - Invokes C# callback on viewport changes

## Behavior

### When Moving a Form
1. User drags form by title bar
2. User releases mouse button
3. `OnGlobalMouseUp()` is called
4. `EnsureFormTitleBarVisible()` is invoked
5. Form position is adjusted if title bar is not fully visible

### When Resizing a Form
1. User drags resize handle
2. Form dimensions change during drag
3. User releases mouse button
4. `OnGlobalMouseUp()` is called
5. `EnsureFormTitleBarVisible()` is invoked
6. Form is repositioned if title bar went off-screen

### When Viewport Changes
1. Browser window is resized
2. JavaScript resize event fires (debounced)
3. `OnViewportResize()` is invoked in Desktop component
4. All visible normal-state forms are checked
5. Each form's `EnsureTitleBarVisible()` is called
6. Forms are repositioned as needed

## Constraints

The implementation ensures:
- **Minimum visibility**: At least 50 pixels of the form remain visible from the left
- **Title bar always visible**: The entire title bar height (32px) is kept in viewport
- **Bottom constraint**: Forms cannot be dragged so far down that the title bar goes off the bottom
- **Top constraint**: Forms cannot move above the desktop area (Top >= 0)
- **Wide forms**: Forms wider than viewport are positioned flush left (Left = 0)
- **Only normal windows**: Minimized and maximized forms are not affected

**Important:** The form's `Top` and `Left` positions are relative to the desktop area (which starts below the taskbar), not the full viewport. This is accounted for in the constraint calculations.

## Testing

To test this feature:
1. Create and show a form
2. Move the form near or beyond viewport edges
3. Release the mouse - form should snap back to keep title bar visible
4. Resize the browser window - forms should automatically reposition
5. Try with forms wider than the viewport - they should align to the left edge

## Example Code

```csharp
var form = new Form
{
    Text = "Test Form",
    Width = 800,
    Height = 600,
    Left = 50,
    Top = 50
};
form.Show();

// When moved off-screen or viewport resizes,
// the form will automatically reposition to keep title bar visible
```

## Benefits

1. **Accessibility**: Users can always access the title bar to move, minimize, maximize, or close forms
2. **Usability**: Forms never become "lost" off-screen
3. **Responsive**: Automatically adapts to browser window resizing
4. **Intuitive**: Behavior matches desktop windowing systems
5. **Non-intrusive**: Only activates when needed (after moves/resizes/viewport changes)
