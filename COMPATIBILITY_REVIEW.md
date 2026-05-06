# Canvas.Windows.Forms - Compatibility Review

> **Last updated:** reflects codebase state after the `additional_controls` branch implementation pass.
> Status legend: Good / Partial / Stub / Not started

---

## Summary Table

| Category | Control / Component | Status | Key Gaps |
|----------|---------------------|--------|----------|
| Core | `Control` (base) | Partial | Some render-side APIs are browser-constrained by design; drag-drop wired via HTML5 events |
| Core | `ContainerControl` | Partial | `ActiveControl`, `Validate`, `ValidateChildren` |
| Core | `ScrollableControl` | Partial | Auto-scroll sizing; no physical scrollbar chrome |
| Core | `UserControl` | Stub | Base lifecycle present; composite paint partial |
| Windowing | `Form` | Partial | Chrome, move/resize done; MDI, OwnedForms missing |
| Text | `Label` | Partial | Multi-line, alignment, AutoEllipsis done; Image not rendered |
| Text | `LinkLabel` | Partial | Click/visited + optional browser nav; multi-link partial |
| Text | `TextBox / TextBoxBase` | Partial | Editing, selection, redo, placeholder, autocomplete done; IME absent |
| Text | `MaskedTextBox` | Partial | Mask display + validation; provider/culture hooks thin |
| Text | `RichTextBox` | Stub | Stores RTF; renders as plain text |
| Buttons | `Button / ButtonBase` | Good | Hover/pressed/focus/keyboard; gradient + flat rendering; Image on face; FlatAppearance |
| Buttons | `CheckBox` | Good | Toggle, ThreeState, CheckAlign, Appearance |
| Buttons | `RadioButton` | Good | Mutual exclusion within parent |
| Lists | `ListBox` | Good | SelectionMode, owner-draw, ItemHeight, IntegralHeight, double-click |
| Lists | `CheckedListBox` | Partial | Basic checked-item behavior; CheckOnClick done |
| Lists | `ComboBox` | Partial | Drop-down + selection; autocomplete partial |
| Lists | `ListControl` (base) | Partial | DataSource, DisplayMember, ValueMember wired |
| Collections | `TreeView` | Good | Nodes, expand/collapse, LabelEdit, ToolTipText, BeginUpdate |
| Collections | `ListView` | Good | Details/List/LargeIcon; keyboard nav; EnsureVisible; BeginUpdate |
| Display | `PictureBox` | Partial | URL/Image; SizeMode; LoadAsync; LoadCompleted; ErrorImage |
| Display | `ProgressBar` | Partial | Blocks/continuous/marquee; RightToLeftLayout |
| Display | `MonthCalendar` | Good | SelectionRange; BoldedDates; keyboard/mouse nav |
| Common | `DateTimePicker` | Good | Format/CustomFormat; ShowUpDown/ShowCheckBox; calendar |
| Common | `NumericUpDown / UpDownBase` | Partial | Value clamping, keyboard entry, TextAlign |
| Common | `DomainUpDown` | Partial | Items, Sorted, Wrap, SelectedIndex/SelectedItem |
| Common | `TrackBar` | Partial | H/V; ticks; keyboard/mouse; SetRange |
| Common | `HScrollBar / VScrollBar` | Partial | SmallChange/LargeChange; Scroll/ValueChanged |
| Common | `ImageList` | Stub | API present; image storage stub |
| Common | `Timer` | Good | PeriodicTimer-based; Interval; fires on SynchronizationContext |
| Containers | `Panel / ScrollableControl` | Partial | Child painting + input routing; scroll offset |
| Containers | `GroupBox` | Partial | Border/caption + child routing/clipping |
| Containers | `TabControl` | Partial | Tab strip + page switching |
| Containers | `SplitContainer` | Good | Resizable panes; fixed/min-size; double-click reset |
| Layout | `FlowLayoutPanel` | Good | FlowDirection + wrap/break; SetFlowBreak |
| Layout | `TableLayoutPanel` | Good | Row/column styles + spans; CellBorderStyle; GetControlFromPosition |
| Menus | `MenuStrip` | Partial | Top-level menu bar with dropdowns |
| Menus | `ContextMenuStrip` | Partial | Right-click overlay; Opening/Closing events |
| Menus | `ToolStrip` | Partial | Toolbar with icons, hover, checked state |
| Menus | `StatusStrip / ToolStripStatusLabel` | Partial | Status bar; Spring; BorderSides; SizingGrip |
| Menus | `ToolStripMenuItem` | Partial | Dropdowns, check state, shortcuts, image, enabled |
| Menus | `ToolStripContainer / ToolStripPanel` | Stub | Dockable strip host |
| Dialogs | `OpenFileDialog` | Partial | Host FS + browser upload |
| Dialogs | `SaveFileDialog` | Partial | CreatePrompt, OverwritePrompt, OpenFile() |
| Dialogs | `FolderBrowserDialog` | Partial | SelectedPath, Description, ShowNewFolderButton |
| Dialogs | `ColorDialog` | Partial | Swatch palette + Hex/RGB/HSV inputs |
| Dialogs | `FontDialog` | Partial | Family/style/size; ShowEffects; ShowColor; Apply event |
| Data | `DataGridView` | Partial | IList/BindingSource/DataTable binding; auto-col gen; sort; col types |
| Data | `BindingSource` | Partial | IList/IBindingList; Filter/Sort/Find; server-backed |
| Data | `DataTable` | Stub | Lightweight in-process stub; not full ADO.NET |
| Non-visual | `NotifyIcon` | Partial | Canvas system tray; ContextMenuStrip popup; balloon tips |
| Non-visual | `ToolTip` | Stub | API present; browser title-attr fallback |
| Non-visual | `Clipboard` | Good | SetText/GetText/Async; `navigator.clipboard` bridge; local-cache fallback |
| Graphics | `Graphics` / drawing primitives | Good | Lines, rects, ellipses, arcs, beziers, polygons, round-rects, gradients, paths, dash styles |

---

## Control (Base Class)

### Implemented
- Name, Text, Tag, Site
- Left, Top, Width, Height, Location, Size, Bounds, ClientSize, ClientRectangle
- BackColor, ForeColor (System.Drawing.Color)
- Visible, Enabled, Focused, ContainsFocus
- Dock, Anchor
- Parent, Controls (ControlCollection with Add/Remove/Clear events)
- TabIndex, TabStop
- Font (Family, Size, Style; DefaultFont = 12pt Segoe UI)
- Cursor, Region, BackgroundImage, BackgroundImageLayout
- Padding, Margin (real System.Windows.Forms.Padding struct with per-edge and aggregate semantics)
- MinimumSize, MaximumSize, PreferredSize, GetPreferredSize()
- AllowDrop (property; no actual drag-drop in canvas)
- RightToLeft (enum), UseWaitCursor
- InvokeRequired (always false in WASM), Invoke(), BeginInvoke()
- PointToScreen(), PointToClient(), RectangleToScreen(), RectangleToClient()
- FindForm(), GetContainerControl()
- SetBounds(), Scale()
- CreateGraphics(), Invalidate(), Refresh(), Update()
- Focus(), Select(), SelectNextControl()
- BringToFront(), SendToBack()
- PerformLayout(), SuspendLayout(), ResumeLayout()
- Show(), Hide(), Dispose()
- Property-change events: VisibleChanged, EnabledChanged, LocationChanged, Move, Resize, SizeChanged, BackColorChanged, ForeColorChanged, FontChanged, TabIndexChanged, TabStopChanged, CursorChanged, DockChanged, RegionChanged, BackgroundImageChanged, BackgroundImageLayoutChanged
- Input events: Click, DoubleClick, MouseDown, MouseUp, MouseMove, MouseEnter, MouseLeave, MouseWheel
- Keyboard: KeyDown, KeyUp, KeyPress
- Focus: GotFocus, LostFocus, Enter, Leave
- Layout/paint: Paint, Resize, Layout
- Drag-and-drop: DragEnter, DragDrop, DragOver, DragLeave, GiveFeedback, QueryContinueDrag (events, On* raisers, DoDragDrop)
- AllowDrop (controls whether a control is a valid drop target)
- DragDropManager: static session manager bridges WinForms DoDragDrop → HTML5 drag events

### Partial / Browser-constrained
- Handle (IntPtr): always IntPtr.Zero - no HWND in canvas
- DoubleBuffered: accepted; canvas is inherently double-buffered
- ImeMode: property present; no IME support in canvas
- CreateParams: stub; not used for canvas rendering
- WndProc(): not applicable
- AccessibilityObject: stub object returned

### Not implemented
- QueryAccessibilityHelp

---

## Form

### Implemented
- Text (title bar), Icon (property; not rendered as favicon)
- WindowState (Normal / Minimized / Maximized) + WindowStateChanged
- FormBorderStyle (affects chrome rendering)
- FormStartPosition (Manual, CenterScreen, CenterParent, WindowsDefaultLocation)
- CenterToScreen(), CenterToParent(), SetDesktopLocation(), SetDesktopBounds()
- MinimumSize, MaximumSize
- AcceptButton, CancelButton (IButtonControl)
- KeyPreview + form-level KeyDown/KeyUp/KeyPress
- Owner, ShowDialog() (modal block via FormManager)
- DialogResult (set by AcceptButton/CancelButton or directly)
- FormClosing, FormClosed (with CancelEventArgs / FormClosedEventArgs)
- Close(), Activate(), Focus()
- Shown, ResizeBegin, ResizeEnd, Move lifecycle events
- Load raised via FormRenderer.razor
- Tab navigation (SelectNextControl, GetNextControl)
- TopMost (via ZIndex), ShowInTaskbar, Opacity (CSS opacity)
- Controls, ActiveControl (focus tracking with proper old/new state propagation)
- EnsureTitleBarVisible() (canvas extension for viewport-clamp)

### Partial
- MainMenuStrip: property wired; rendered in-form, not OS chrome
- ControlBox, MinimizeBox, MaximizeBox: properties present; chrome rendering simplified
- AutoScroll, AutoScrollPosition: via ScrollableControl; partial

### Not implemented
- MDI (MdiParent, MdiChildren, IsMdiContainer, tile/cascade)
- OwnedForms collection

---

## Button / ButtonBase

### Good
- Text, Enabled, Visible, FlatStyle (Standard, Flat, Popup, System)
- Click event, PerformClick()
- Visual states: Normal, Hover, Pressed, Focused, Disabled
- Gradient fill (Standard/System) or flat solid fill (Flat/Popup) + text centering via TextMeasurementService
- DialogResult
- TextAlign, ImageAlign, TextImageRelation (all layout modes)
- Keyboard activation (Space/Enter)
- **Image** property rendered on button face; position controlled by `ImageAlign` and `TextImageRelation`
- **FlatAppearance** — `MouseOverBackColor`, `MouseDownBackColor`, `BorderColor`, `BorderSize`, `CheckedBackColor` all honoured in Flat/Popup rendering
- ImageIndex, ImageKey, ImageList stubs (accepted; not yet composited via ImageList)

---

## TextBox / TextBoxBase

### Implemented
- Text, MaxLength, ReadOnly, Multiline, WordWrap
- PasswordChar, UseSystemPasswordChar
- SelectionStart, SelectionLength, SelectedText
- Select(), SelectAll(), Clear(), AppendText()
- Copy(), Cut(), Paste(), Undo(), Redo() — **Copy/Cut write to real browser clipboard via `navigator.clipboard`; Paste refreshes from real clipboard before inserting**
- TextAlign (Left/Center/Right), CharacterCasing
- Lines (multiline split/join)
- ScrollBars (visual only)
- PlaceholderText
- Keyboard: Ctrl+A/C/X/V/Z/Y, Shift+arrows, word-delete (Ctrl+Backspace/Delete)
- AutoCompleteMode, AutoCompleteSource, AutoCompleteCustomSource
- AcceptsReturn, AcceptsTab
- GetCharIndexFromPosition(), GetPositionFromCharIndex() (approximate)

### Partial
- IME / composition input: no support in canvas
- HideSelection: property present; no distinct rendering
- Physical scroll position (GetFirstVisibleLine())

---

## ListBox

### Good
- Items (Add, Insert, Remove, Clear, AddRange, Contains, IndexOf)
- SelectedIndex, SelectedItem, SelectedIndices, SelectedItems
- SelectionMode (One, MultiSimple, MultiExtended, None)
- Sorted (auto-sorts on add)
- Mouse + keyboard selection (arrows, Shift, Ctrl, Space)
- DrawMode, MeasureItem, DrawItem (owner-draw)
- ItemHeight, IntegralHeight
- DoubleClick / MouseDoubleClick
- SelectedIndexChanged event
- FindString(), FindStringExact()
- GetItemRectangle(), GetItemText()
- TopIndex, EnsureVisible()

---

## ComboBox

### Implemented
- Items, SelectedIndex, SelectedItem, SelectedValue, Text
- DropDownStyle (DropDown, DropDownList, Simple)
- DroppedDown, MaxDropDownItems, DropDownWidth, DropDownHeight
- DisplayMember, ValueMember, DataSource
- DropDown, DropDownClosed, SelectedIndexChanged, TextChanged

### Partial
- AutoCompleteMode / AutoCompleteSource / AutoCompleteCustomSource: partial
- DrawMode (OwnerDraw not implemented)
- FindString(), FindStringExact(): stub

---

## TreeView

### Good
- Nodes (recursive; TreeNode with Text, Tag, ImageIndex, Checked, ToolTipText)
- Expand/Collapse all levels; BeforeExpand, AfterExpand, BeforeCollapse, AfterCollapse
- SelectedNode, BeforeSelect, AfterSelect
- LabelEdit, BeforeLabelEdit, AfterLabelEdit
- CheckBoxes, NodeChecked
- BeginUpdate(), EndUpdate()
- ImageList, ImageIndex, SelectedImageIndex
- Keyboard navigation (arrows, +/-, *, Home, End)

---

## ListView

### Good
- Items (ListViewItem with SubItems, ImageIndex, Checked, Tag)
- Columns (ColumnHeader with Text, Width, TextAlign)
- View (Details, List, LargeIcon, SmallIcon, Tile)
- SelectedItems, SelectedIndices, CheckedItems
- MultiSelect, CheckBoxes, FullRowSelect, GridLines
- ColumnClick, ItemActivate, SelectedIndexChanged, ItemChecked
- Sorting, ListViewItemSorter
- SmallImageList, LargeImageList
- BeginUpdate(), EndUpdate(), EnsureVisible()
- Keyboard navigation

---

## DataGridView

### Partial - substantial coverage

#### Implemented
- DataSource binding: IList<T>, BindingSource, DataTable
- Auto-column generation from reflected properties
- Column types: TextBox, CheckBox, Button, ComboBox, Image, Link
- Columns / Rows collections with typed access
- Row selection (single/multi), SelectedRows, CurrentRow
- CellClick, CellDoubleClick, SelectionChanged, RowEnter, CellValueChanged
- Sort(), SortCompare
- Virtualised row scroll
- AllowUserToAddRows, AllowUserToDeleteRows, ReadOnly
- DefaultCellStyle, column/row-level style overrides
- AutoResizeColumns()

#### Partial
- In-cell editing: TextBox column only; CheckBox toggle done
- CellValidating, RowValidating: events present, no built-in UI feedback

#### Not implemented
- Frozen columns/rows
- ComboBox column in-cell dropdown UI
- Copy to clipboard
- Multi-column sort

---

## BindingSource

### Partial - well-covered
- DataSource, DataMember
- Current, Position, navigation (MoveFirst, MoveLast, MoveNext, MovePrevious)
- Count, List, Add(), Remove(), RemoveAt()
- Filter (LINQ predicate on IList<T>)
- Sort (property name + direction)
- Find(property, key)
- ListChanged, CurrentChanged, PositionChanged
- Server-backed via CanvasDataService / ICanvasDataService

---

## NotifyIcon (System Tray)

### Partial - functional canvas tray

#### Implemented
- Visible registers/unregisters icon in the canvas system-tray (right of taskbar)
- Text (tooltip; max 63 chars per WinForms spec)
- Icon (ResourcePath rendered as img; fallback SVG with app initial letter)
- ContextMenuStrip - right-click opens HTML overlay menu (items, separators, enabled/disabled, hover)
- Click, MouseClick, DoubleClick, MouseDoubleClick events
- ShowBalloonTip() - animated toast (bottom-right, above taskbar) with auto-dismiss, close button
- BalloonTipIcon rendering (Info/Warning/Error coloured badge)
- BalloonTipClicked, BalloonTipClosed, BalloonTipShown events
- Dispose() auto-unregisters from tray
- Canvas tray also includes a live clock (HH:mm + date) for visual parity

#### Partial
- Icon image: URL/path only; no GDI System.Drawing.Icon handle rendering

---

## TrackBar

### Partial - TrackBar : Control hierarchy (matches WinForms)
- Minimum, Maximum, Value, SmallChange, LargeChange, TickFrequency
- Orientation (Horizontal / Vertical)
- TickStyle (None, TopLeft, BottomRight, Both)
- AutoSize, SetRange()
- ValueChanged, Scroll events
- Mouse click/drag on thumb and track; keyboard (arrows, PgUp/Dn, Home, End)

---

## HScrollBar / VScrollBar

### Partial - ScrollBar : Control hierarchy (matches WinForms)
- Minimum, Maximum, Value, SmallChange, LargeChange
- Scroll (ScrollEventArgs with ScrollEventType, ScrollOrientation), ValueChanged
- Arrow buttons, track page-scroll, thumb drag, keyboard navigation
- HScrollBar (default 200x17) and VScrollBar (default 17x200)

---

## DomainUpDown

### Partial - DomainUpDown : UpDownBase : ContainerControl hierarchy (matches WinForms)
- Items (DomainUpDownItemCollection: Add, Insert, Remove, Clear, IndexOf, Contains)
- SelectedIndex, SelectedItem
- Sorted, Wrap, TextAlign
- ReadOnly (via UpDownBase)
- SelectedItemChanged event
- Keyboard Up/Down navigation

---

## NumericUpDown / UpDownBase

### Partial
- Value, Minimum, Maximum, Increment
- DecimalPlaces, ThousandsSeparator, Hexadecimal
- TextAlign, ReadOnly, InterceptArrowKeys
- Direct keyboard entry with buffer; Enter to commit
- UpButton(), DownButton()
- ValueChanged event

---

## Timer

### Good
- Interval, Enabled, Start(), Stop()
- Tick event fired on captured SynchronizationContext
- Tag, IContainer constructor
- Backed by PeriodicTimer (safe on WASM UI thread)

---

## Clipboard

### Good
- `Clipboard.SetText(string)` / `SetText(string, TextDataFormat)` — writes to local cache **and** the real browser clipboard via `navigator.clipboard.writeText`
- `Clipboard.GetText()` / `GetText(TextDataFormat)` — synchronous read from local cache
- `Clipboard.GetTextAsync()` — async read from real browser clipboard; falls back to cache if `clipboard-read` permission is denied
- `Clipboard.SetTextAsync(string)` — awaitable write to real clipboard
- `Clipboard.ContainsText()` — checks local cache
- `Clipboard.Clear()` — clears local cache and real clipboard
- `TextBoxBase` Ctrl+C / Ctrl+X write via `Clipboard.SetText` (real clipboard)
- `TextBoxBase` Ctrl+V triggers async clipboard refresh then pastes
- `TextDataFormat` enum (Text, UnicodeText, Rtf, Html, CommaSeparatedValue) — only plain text transported

### Partial
- Non-text formats (`SetDataObject`, `GetDataObject`, `IDataObject`) — not implemented; canvas layer is text-only
- `clipboard-read` requires browser permission prompt on non-localhost origins

---

## SplitContainer

### Good
- Panel1, Panel2, SplitterDistance, SplitterWidth

- IsSplitterFixed, SplitterMoved, SplitterMoving
- Double-click to reset splitter

---

## FlowLayoutPanel

### Good
- FlowDirection (LeftToRight, RightToLeft, TopDown, BottomUp)
- WrapContents, SetFlowBreak(), GetFlowBreak()
- Correct per-edge Padding and child Margin semantics

---

## TableLayoutPanel

### Good
- RowStyles, ColumnStyles (Absolute, Percent, AutoSize)
- Row/column spans (SetRowSpan, SetColumnSpan)
- CellBorderStyle
- GetControlFromPosition(), GetPositionFromControl()
- Anchor/Dock within cells; correct Padding/Margin semantics

---

## Dialogs

| Dialog | Status | Notes |
|--------|--------|-------|
| OpenFileDialog | Partial | Host FS enumeration + browser file input upload bridge |
| SaveFileDialog | Partial | CreatePrompt, OverwritePrompt, OpenFile(), host FS write |
| FolderBrowserDialog | Partial | SelectedPath, Description, RootFolder, ShowNewFolderButton |
| ColorDialog | Partial | Swatch palette + Hex/RGB/HSV inputs; AllowFullOpen, CustomColors |
| FontDialog | Partial | Family/style/size lists; ShowEffects; ShowColor; MinSize/MaxSize; Apply |
| MessageBox | Good | Show() overloads; MessageBoxButtons, MessageBoxIcon, DialogResult |

---

## Graphics / Drawing

### Good
- Primitives: Lines, rectangles, ellipses, arcs, Bezier curves, polygons
- Filled shapes: FillRectangle, FillEllipse, FillPolygon
- Round-rects: DrawRoundRect, FillRoundRect with corner radius
- Paths: GraphicsPath with AddLine, AddArc, AddBezier, AddRectangle, AddEllipse, AddPolygon, CloseFigure; drawn via DrawPath / FillPath
- Brushes: SolidBrush, LinearGradientBrush (two-stop + InterpolationColors; LinearGradientMode), RadialGradientBrush
- Pens: DashStyle (Solid, Dash, Dot, DashDot, DashDotDot, Custom); Width
- Text: DrawString with StringFormat (alignment, trimming); Bold/Italic/Underline/Strikeout
- Images: DrawImage (URL-based)
- Transform/clip: TranslateTransform, SetClip, ResetTransform (basic)
- Command-buffered rendering dispatched via Blazor interop to canvas-renderer.js

### Partial
- MeasureString: approximate (font metrics table); TextMeasurementService used for precise centering via JS
- GraphicsPath.Widen(), IsVisible(): not implemented

---

## IL Translator Compatibility Notes

The translator runs two passes over a compiled WinForms assembly:

**Pass 1 — Assembly reference retargeting** (original behaviour)  
Rewrites assembly identity references from `System.Windows.Forms` / `System.Windows.Forms.Primitives` / `WebForms.Canvas` → `Canvas.Windows.Forms`.

**Pass 2 — IL call-site rewrites** (new; handles WASM constraints)  
Scans every method body and patches specific call patterns that break under WASM's single-thread model.

| Pattern | IL Rewrite applied |
|---------|--------------------|
| `Control.Invoke(delegate)` | Direct call — `InvokeRequired` is always false in WASM |
| `Control.BeginInvoke(delegate)` | `Task.Run` shim |
| `Application.DoEvents()` | No-op (browser owns event loop) |
| `Application.Run(form)` | Replaced by `FormManager.LaunchForm<T>()` |
| `Thread.Sleep(n)` | `await Task.Delay(n)` where possible; otherwise no-op |
| `MessageBox.Show(...)` | Canvas MessageBox — HTML modal |
| Native `[DllImport]` | Remove or stub — no P/Invoke in WASM |
| `new Icon(path)` | `ResourcePath` stored; rendered via `<img>` tag |
| `System.Drawing.Color` | Implicit conversion to/from `Canvas.Windows.Forms.Drawing.Color` |
| `System.Drawing.Size` | Implicit conversion to/from canvas `Size` |
| `System.Drawing.Point` | Implicit conversion to/from canvas `Point` |
| **`control.DoDragDrop(data, effects)`** — return value used | **✅ Rewritten:** `pop` (discard immediate `None`) + `call DragDropManager::get_LastResult` inserted after the call, so the variable receiving the result gets the real drop effect once `FormRenderer.HandleDrop` fires. Void-context calls (result discarded) are left unchanged. |

### DoDragDrop rewrite — detailed rationale

WASM runs on a single thread.  Real WinForms blocks the calling thread inside `DoDragDrop` until the drag ends.  In WASM the method must return immediately (otherwise the UI thread is starved and the drop can never fire).  `DoDragDrop` therefore returns `DragDropEffects.None` right away and stores the real result in `DragDropManager.LastResult` when `FormRenderer.HandleDrop` completes.

The IL rewrite transparently patches compiled apps so that:

```csharp
// Original C# — blocks and returns real effect:
var effect = control.DoDragDrop(data, DragDropEffects.Copy);
if (effect == DragDropEffects.Copy) { ... }

// Rewritten IL equivalent — non-blocking, reads LastResult:
control.DoDragDrop(data, DragDropEffects.Copy); // starts session, returns None
var effect = DragDropManager.LastResult;         // real result after drop
if (effect == DragDropEffects.Copy) { ... }
```

Apps that don't use the return value are unaffected.

---

## WebBrowser / WebView2

**Status: ⚠️ Partial**

Implemented as an absolutely-positioned `<iframe>` overlay rendered by `FormRenderer` outside the canvas element, tracked to the control's `Left`/`Top`/`Width`/`Height` within its parent form.

| Feature | Status | Notes |
|---------|--------|-------|
| `Navigate(string url)` / `Navigate(Uri)` | ✅ | Sets `src`; raises `Navigating` / `Navigated` |
| `DocumentText` (set HTML string) | ✅ | Sets `srcdoc` |
| `DocumentCompleted` event | ✅ | Fires on iframe `onload` |
| `GoBack()` / `GoForward()` | ⚠️ | Same-origin history only; JS `contentWindow.history` |
| `Stop()` / `Refresh()` | ✅ | Via JS `contentWindow.stop()` / `location.reload()` |
| `ExecuteScriptAsync(js)` | ⚠️ | Same-origin content only; returns `string?` |
| `ScriptEnabled` | ✅ | Controls `allow-scripts` in `sandbox` attribute |
| `Url` property | ✅ | |
| `Visible` | ✅ | `display: none` hides the iframe |
| Cross-origin navigation | ⚠️ | Allowed but DOM access / script execution blocked by browser |
| `Document` object model | ❌ | Blocked cross-origin; same-origin possible via `ExecuteScriptAsync` |
| `WebView2` | ✅ | Aliased as a subclass of `WebBrowser` |

**Architecture note:** Since canvas is a flat pixel buffer, browser-hosted content cannot be painted into it. The iframe sits as a sibling element in the DOM, z-indexed above the canvas, and follows the form's position/size on every Blazor render cycle.

---

## Known Platform Constraints

These gaps are architectural — they require OS integration unavailable in a browser canvas:

- No P/Invoke or native handles (`Handle`, `HWND`, `WndProc`)
- No IME for CJK / complex-script input
- No actual system tray — canvas tray is a visual simulation inside the page
- Clipboard JS bridge implemented: `Clipboard.SetText`/`GetText`/`SetTextAsync`/`GetTextAsync` use `navigator.clipboard` with local-cache fallback; `clipboard-read` permission required for cross-app paste (auto-granted on localhost)
- No multi-monitor (`Screen` class is a stub)
- No MDI (Multiple Document Interface)
- No `PrintDocument` / print preview (no printer access from WASM)
- `DoDragDrop` return value is async — the IL translator patches call-sites automatically (see above)
- `WebBrowser` / `WebView2`: cross-origin `Document` DOM access blocked by browser sandbox; `GoBack`/`GoForward` only work for same-origin history entries

---

For per-property tracking see `Canvas.Windows.Forms.Tests/PROPERTY_COMPLETENESS.md` and `CONTROLS_IMPLEMENTATION_STRATEGY.md`.