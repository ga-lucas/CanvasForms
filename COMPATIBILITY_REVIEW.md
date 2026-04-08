# Canvas.Windows.Forms Compatibility Review

This document reviews the compatibility of Canvas.Windows.Forms controls with the Windows Forms SDK API.

## Summary

| Control | Status | Notes |
|---------|--------|-------|
| Control (base) | ⚠️ Partial | Core properties present, many stubs |
| Form | ⚠️ Partial | Basic windowing works, missing advanced features |
| Button | ✅ Good | Core functionality complete |
| CheckBox | ✅ Good | Core functionality complete |
| RadioButton | ✅ Good | Core functionality complete |
| Label | ⚠️ Partial | Basic text, missing advanced features |
| TextBox | ⚠️ Partial | Basic editing works, missing some features |
| ListBox | ⚠️ Partial | Single/multi select works, missing virtual mode |
| ComboBox | ⚠️ Partial | Drop-down works, autocomplete partial |
| CheckedListBox | ⚠️ Partial | Basic functionality |
| PictureBox | ⚠️ Partial | Image display, missing some modes |
| DateTimePicker | ❌ Stub | Needs implementation |

---

## Control (Base Class)

### ✅ Implemented Properties
- `Name`, `Text`, `Tag`
- `Left`, `Top`, `Width`, `Height`
- `Location`, `Size`, `Bounds`, `ClientSize`, `ClientRectangle`
- `BackColor`, `ForeColor`
- `Visible`, `Enabled`, `Focused`
- `Dock`, `Anchor`
- `Parent`, `Controls` (ControlCollection)
- `TabIndex`, `TabStop`
- `Font` (basic)
- `Cursor` (enum only)

### ⚠️ Stub/Partial Properties
```csharp
// Present but non-functional or simplified:
public bool AllowDrop { get; set; }           // No drag-drop in canvas
public IntPtr Handle { get; }                  // Always IntPtr.Zero
public bool DoubleBuffered { get; set; }       // Canvas always double-buffered
public object? ContextMenu { get; set; }       // Not implemented
public object? ContextMenuStrip { get; set; }  // Not implemented
public object? BindingContext { get; set; }    // Data binding not implemented
public ImeMode ImeMode { get; set; }           // No IME support in canvas
```

### ❌ Missing Properties
```csharp
// Not present - need to add as stubs:
public Padding Padding { get; set; }
public Padding Margin { get; set; }
public Point PointToScreen(Point p);
public Point PointToClient(Point p);
public Rectangle RectangleToScreen(Rectangle r);
public Rectangle RectangleToClient(Rectangle r);
public Region Region { get; set; }
public RightToLeft RightToLeft { get; set; }  // Enum exists, property is bool
public AccessibleObject AccessibilityObject { get; }
```

### ✅ Implemented Methods
- `Invalidate()`, `Refresh()`, `Update()`
- `Focus()`, `Select()`
- `BringToFront()`, `SendToBack()`
- `PerformLayout()`, `SuspendLayout()`, `ResumeLayout()`
- `Show()`, `Hide()`
- `Dispose()`
- `OnPaint()`, `OnClick()`, `OnMouseDown/Up/Move()`
- `OnKeyDown/Up/Press()`
- `OnResize()`, `OnLayout()`

### ❌ Missing Methods
```csharp
// Need to add:
public void Invoke(Delegate method);           // Cross-thread (shim - just call directly in WASM)
public void BeginInvoke(Delegate method);      // Cross-thread (shim)
public bool InvokeRequired { get; }            // Always false in WASM
public Graphics CreateGraphics();              // Useful for measurement
public void Scale(SizeF factor);
public void SetBounds(int x, int y, int w, int h);
```

### ✅ Implemented Events
- `Click`, `DoubleClick`
- `MouseDown`, `MouseUp`, `MouseMove`, `MouseEnter`, `MouseLeave`
- `KeyDown`, `KeyUp`, `KeyPress`
- `GotFocus`, `LostFocus`, `Enter`, `Leave`
- `Paint`, `Resize`, `Layout`
- `TextChanged`, `EnabledChanged`, `VisibleChanged`

---

## Form

### ✅ Implemented
- `Text` (title bar)
- `WindowState` (Normal, Minimized, Maximized)
- `FormBorderStyle` (partial)
- `StartPosition` (partial)
- `MinimumSize`, `MaximumSize`
- `FormClosing`, `FormClosed` events (with CancelEventArgs)
- `Close()` method
- `Show()`, `ShowDialog()` (non-blocking in canvas)

### ⚠️ Partial/Stub
```csharp
public DialogResult DialogResult { get; set; }  // Partial
public Form? Owner { get; set; }                 // Stub
public bool TopMost { get; set; }                // Via ZIndex
public double Opacity { get; set; }              // Not rendered
public bool ShowInTaskbar { get; set; }          // Always true
public Icon Icon { get; set; }                   // Not rendered
public MenuStrip MainMenuStrip { get; set; }     // Not implemented
```

### ❌ Missing
```csharp
public bool Modal { get; }
public Form[] OwnedForms { get; }
public IButtonControl AcceptButton { get; set; }
public IButtonControl CancelButton { get; set; }
public bool ControlBox { get; set; }
public bool MinimizeBox { get; set; }
public bool MaximizeBox { get; set; }
```

---

## Button

### ✅ Fully Compatible
- `Text`, `Enabled`, `Visible`
- `Click` event
- `PerformClick()` method
- Visual states (Normal, Hover, Pressed, Disabled)
- `FlatStyle` (partial - affects appearance)
- `DialogResult`

### ⚠️ Partial
```csharp
public Image Image { get; set; }                // Not rendered
public ContentAlignment ImageAlign { get; set; } // Stub
public ContentAlignment TextAlign { get; set; }  // Partial
public FlatStyle FlatStyle { get; set; }         // Simplified
```

---

## TextBox

### ✅ Implemented
- `Text`, `MaxLength`
- `ReadOnly`, `Multiline`
- `PasswordChar`, `UseSystemPasswordChar`
- `SelectionStart`, `SelectionLength`
- `SelectedText`, `Select()`, `SelectAll()`
- `TextAlign` (Left, Center, Right)
- `CharacterCasing` (Normal, Upper, Lower)
- Basic keyboard input (typing, backspace, delete, arrow keys)
- Basic clipboard (Ctrl+C/V/X/A)

### ⚠️ Partial
```csharp
public AutoCompleteMode AutoCompleteMode { get; set; }      // Partial implementation
public AutoCompleteSource AutoCompleteSource { get; set; }  // Partial
public string[] AutoCompleteCustomSource { get; set; }      // Works
public bool AcceptsReturn { get; set; }                      // For multiline
public bool AcceptsTab { get; set; }                         // Stub
public ScrollBars ScrollBars { get; set; }                   // Visual only
```

### ❌ Missing
```csharp
public string[] Lines { get; set; }              // Need for multiline
public void AppendText(string text);
public void Clear();
public void Copy(); public void Cut(); public void Paste();  // Methods exist but may need review
public int GetCharIndexFromPosition(Point pt);
public Point GetPositionFromCharIndex(int index);
```

---

## ListBox

### ✅ Implemented
- `Items` collection (Add, Remove, Insert, Clear)
- `SelectedIndex`, `SelectedItem`
- `SelectedIndices`, `SelectedItems` (multi-select)
- `SelectionMode` (One, MultiSimple, MultiExtended)
- Visual scrolling
- Mouse selection
- `SelectedIndexChanged` event

### ⚠️ Partial
```csharp
public bool Sorted { get; set; }                 // Property exists, sorting not auto
public DrawMode DrawMode { get; set; }           // Stub (OwnerDraw not implemented)
public int ItemHeight { get; set; }              // Fixed height
public bool IntegralHeight { get; set; }         // Stub
```

### ❌ Missing
```csharp
public int TopIndex { get; set; }                // Scroll position
public void SetSelected(int index, bool value);
public int FindString(string s);
public int FindStringExact(string s);
public Rectangle GetItemRectangle(int index);
```

---

## ComboBox

### ✅ Implemented
- `Items` collection
- `SelectedIndex`, `SelectedItem`, `Text`
- `DropDownStyle` (DropDown, DropDownList, Simple)
- `DroppedDown` property
- Drop-down rendering and interaction
- `DropDown`, `DropDownClosed` events
- `SelectedIndexChanged` event

### ⚠️ Partial
```csharp
public int DropDownWidth { get; set; }
public int DropDownHeight { get; set; }
public int MaxDropDownItems { get; set; }
public AutoCompleteMode AutoCompleteMode { get; set; }
```

### ❌ Missing
```csharp
public ComboBox.ObjectCollection AutoCompleteCustomSource { get; }
public int MaxLength { get; set; }
public bool DroppedDown { get; set; }  // Exists but may need work
public int FindString(string s);
public int FindStringExact(string s);
```

---

## CheckBox / RadioButton

### ✅ Fully Compatible
- `Checked`, `CheckState` (CheckBox)
- `CheckedChanged` event
- Visual states
- Auto-grouping (RadioButton within container)

### ⚠️ Partial
```csharp
public bool ThreeState { get; set; }             // CheckBox only, stub
public CheckState CheckState { get; set; }       // Partial
public ContentAlignment CheckAlign { get; set; } // Stub
public Appearance Appearance { get; set; }       // Stub
```

---

## Label

### ✅ Implemented
- `Text`, `TextAlign`
- `AutoSize`
- Basic rendering

### ⚠️ Partial
```csharp
public bool UseMnemonic { get; set; }            // Stub (no accelerator keys)
public BorderStyle BorderStyle { get; set; }     // Stub
public Image Image { get; set; }                 // Not rendered
public FlatStyle FlatStyle { get; set; }         // Stub
```

---

## PictureBox

### ✅ Implemented
- `Image` property (via URL)
- `SizeMode` (Normal, StretchImage, AutoSize, CenterImage, Zoom)
- Basic image rendering

### ❌ Missing
```csharp
public Image ErrorImage { get; set; }
public Image InitialImage { get; set; }
public bool WaitOnLoad { get; set; }
public void Load(string url);
public event AsyncCompletedEventHandler LoadCompleted;
```

---

## Recommended Actions

### Priority 1: Add Missing Shims for Compatibility
```csharp
// Control.cs - add these as no-op or simple implementations:
public bool InvokeRequired => false;
public object Invoke(Delegate method) => method.DynamicInvoke();
public IAsyncResult BeginInvoke(Delegate method) { method.DynamicInvoke(); return null!; }
public Padding Padding { get; set; } = Padding.Empty;
public Padding Margin { get; set; } = Padding.Empty;
```

### Priority 2: Fix RightToLeft Property Type
```csharp
// Currently: public bool RightToLeft { get; set; }
// Should be: public RightToLeft RightToLeft { get; set; }
```

### Priority 3: Add Missing Form Properties
```csharp
public IButtonControl? AcceptButton { get; set; }
public IButtonControl? CancelButton { get; set; }
public bool ControlBox { get; set; } = true;
public bool MinimizeBox { get; set; } = true;
public bool MaximizeBox { get; set; } = true;
```

### Priority 4: Add Missing Collection Methods
```csharp
// ListBox, ComboBox
public int FindString(string s);
public int FindStringExact(string s);
```

### Priority 5: Implement DateTimePicker
Currently a stub - needs calendar dropdown implementation.

---

## Notes for IL Translator

The translator should handle these patterns:
1. `Control.Invoke()` calls → direct calls (single-threaded in WASM)
2. `MessageBox.Show()` → Canvas MessageBox implementation
3. `Application.DoEvents()` → no-op (event loop is browser)
4. `Thread.Sleep()` → `await Task.Delay()` if possible, or no-op
5. Native interop (`DllImport`) → stub or remove
