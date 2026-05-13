# Canvas.Windows.Forms - Compatibility Review

> **Last updated:** reflects codebase state after the `additional_controls` branch implementation pass.
> Status legend: Good / Partial / Stub / Not started

---

## Summary Table

| Category | Control / Component | Status | Key Gaps |
|----------|---------------------|--------|----------|
| Core | `Control` (base) | Partial | Some render-side APIs are browser-constrained by design; drag-drop wired via HTML5 events |
| Core | `ContainerControl` | Partial | `ActiveControl` wired; `Validate()` fires Validating/Validated; `ValidateChildren()`/`ValidateChildren(ValidationConstraints)` walk full tree; `AutoValidate` respected in focus path |
| Core | `ScrollableControl` | Partial | Auto-scroll sizing; no physical scrollbar chrome |
| Core | `UserControl` | Stub | Base lifecycle present; composite paint partial |
| Windowing | `Form` | Partial | Chrome, move/resize done; Icon → browser favicon + Text → browser tab title when active; MDI support: IsMdiContainer, MdiParent, MdiChildren, ActiveMdiChild, ActivateMdiChild, LayoutMdi (Cascade/TileH/TileV), MdiChildActivate event, constrained drag, resize handles (8-dir), z-index management, mouse/keyboard routing to child controls, child Invalidate→re-render wiring; OwnedForms present |
| Text | `Label` | Partial | Multi-line, alignment, AutoEllipsis done; Image/ImageIndex/ImageKey/ImageList rendered with ImageAlign (all 9 alignments); border styles present |
| Text | `LinkLabel` | Partial | `Links` collection with `Add(start, length, data)` overloads; multi-span hit-testing; per-link `Visited`/`Enabled`/`LinkData`; `LinkLabelLinkClickedEventArgs.Link` carries the clicked span; `LinkUrl` legacy mode preserved; browser nav via JS |
| Text | `TextBox / TextBoxBase` | Partial | Editing, selection, redo, placeholder, autocomplete done; IME absent |
| Text | `MaskedTextBox` | Partial | Mask display + per-token input validation; BackSpace/Delete aware; MaskFull/UnmaskedText; provider/culture hooks thin |
| Text | `RichTextBox` | Partial | RTF parsed into styled runs; bold/italic/underline/colour/font-size rendered per-run; SelectionFont/Color/Bold/Italic/Underline; Find(); LoadFile/SaveFile; HTML clipboard round-trip; ScrollToCaret(); ZoomFactor applied to run font sizes |
| Buttons | `Button / ButtonBase` | Good | Hover/pressed/focus/keyboard; gradient + flat rendering; Image on face; FlatAppearance |
| Buttons | `CheckBox` | Good | Toggle, ThreeState, CheckAlign, Appearance |
| Buttons | `RadioButton` | Good | Mutual exclusion within parent |
| Lists | `ListBox` | Good | SelectionMode, owner-draw, ItemHeight, IntegralHeight, double-click |
| Lists | `CheckedListBox` | Partial | Basic checked-item behavior; CheckOnClick done; mouse wheel + first-letter type-ahead added |
| Lists | `ComboBox` | Partial | Drop-down + selection; autocomplete partial; DrawMode (OwnerDrawFixed/Variable) + DrawItem + MeasureItem implemented |
| Lists | `ListControl` (base) | Partial | DataSource, DisplayMember, ValueMember wired |
| Collections | `TreeView` | Good | Nodes, expand/collapse, LabelEdit, ToolTipText, BeginUpdate |
| Collections | `ListView` | Good | Details/List/LargeIcon; keyboard nav; EnsureVisible; BeginUpdate |
| Display | `PictureBox` | Partial | URL/Image; SizeMode (Normal/CenterImage/Zoom/StretchImage — all correctly implemented using natural image dimensions from JS); ImageLocation; LoadAsync; LoadCompleted; ErrorImage |
| Display | `ProgressBar` | Partial | Blocks/continuous/marquee; RightToLeftLayout; SetRange; ShowPercentage overlay |
| Display | `MonthCalendar` | Good | SelectionRange; BoldedDates; keyboard/mouse nav |
| Common | `DateTimePicker` | Good | Format/CustomFormat; ShowUpDown/ShowCheckBox; calendar |
| Common | `NumericUpDown / UpDownBase` | Partial | Value clamping, keyboard entry, TextAlign |
| Common | `DomainUpDown` | Partial | Items, Sorted, Wrap, SelectedIndex/SelectedItem |
| Common | `TrackBar` | Partial | H/V; ticks; keyboard/mouse; SetRange |
| Common | `HScrollBar / VScrollBar` | Partial | SmallChange/LargeChange; Scroll/ValueChanged; mouse wheel; WinForms effective-maximum clamping |
| Common | `ImageList` | Partial | URL/key storage; ImageSize; wired into ListView, TreeView, TabControl |
| Common | `Timer` | Good | PeriodicTimer-based; Interval; fires on SynchronizationContext |
| Containers | `Panel / ScrollableControl` | Partial | Child painting + input routing; scroll offset; AutoSize + AutoSizeMode |
| Containers | `GroupBox` | Partial | Border/caption + child routing/clipping; AutoSize + AutoSizeMode |
| Containers | `TabControl` | Partial | Tab strip + page switching; Ctrl+Tab keyboard nav; TabCount; GetTabRect |
| Containers | `SplitContainer` | Good | Resizable panes; fixed/min-size; double-click reset |
| Layout | `FlowLayoutPanel` | Good | FlowDirection + wrap/break; SetFlowBreak |
| Layout | `TableLayoutPanel` | Good | Row/column styles + spans; CellBorderStyle; GetControlFromPosition |
| Menus | `MenuStrip` | Partial | Top-level menu bar with dropdowns |
| Menus | `ContextMenuStrip` | Partial | Right-click overlay; Opening/Closing events |
| Menus | `ToolStrip` | Partial | Toolbar with icons, hover, checked state |
| Menus | `StatusStrip / ToolStripStatusLabel` | Partial | Status bar; Spring; BorderSides; SizingGrip |
| Menus | `ToolStripMenuItem` | Partial | Dropdowns, check state, shortcuts, image, enabled |
| Menus | `ToolStripContainer / ToolStripPanel` | Partial | Auto-show/hide bands; row layout of child ToolStrips; 3-pass size-from-content layout |
| Menus (legacy) | `MainMenu` | Partial | Wraps MenuStrip; MenuItem collection; Form.Menu property |
| Menus (legacy) | `ContextMenu` | Partial | Wraps ContextMenuStrip; MenuItem collection; Popup event; Control.ContextMenu wired |
| Menus (legacy) | `ToolBar` | Partial | Wraps ToolStrip; ToolBarButton/ButtonClick; Appearance/TextAlign/Wrappable |
| Dialogs | `OpenFileDialog` | Partial | Host FS + browser upload |
| Dialogs | `SaveFileDialog` | Partial | CreatePrompt, OverwritePrompt, OpenFile() |
| Dialogs | `FolderBrowserDialog` | Partial | SelectedPath, Description, ShowNewFolderButton |
| Dialogs | `ColorDialog` | Partial | Swatch palette + Hex/RGB/HSV inputs |
| Dialogs | `FontDialog` | Partial | Family/style/size; ShowEffects; ShowColor; Apply event |
| Data | `DataGridView` | Partial | IList/BindingSource/DataTable binding; auto-col gen; sort; col types; frozen columns; clipboard copy (Ctrl+C); multi-column sort (Ctrl+click header) |
| Data | `PropertyGrid` | Partial | Reflection-based two-column browser; SelectedObject/SelectedObjects; PropertySort; HelpVisible; ToolbarVisible; inline editing; SelectedGridItemChanged; PropertyValueChanged |
| Data | `BindingSource` | Partial | IList/IBindingList; Filter/Sort/Find; server-backed |
| Data | `BindingNavigator` | Partial | ToolStrip-based navigation bar; First/Prev/Next/Last/Add/Delete; **editable PositionItem textbox** (type record number + Enter); count label; bound to BindingSource events |
| Data | `DataTable` | Partial | DataView/DefaultView; DataRowView (ICustomTypeDescriptor); typed RowChanged/RowDeleted/ColumnChanged events; Select(filter, sort); DataSet/DataTableCollection/DataRelation; IListSource; BindingSource wired |
| Non-visual | `NotifyIcon` | Partial | Canvas system tray; ContextMenuStrip popup; balloon tips |
| Non-visual | `ToolTip` | Partial | InitialDelay/AutoPopDelay/ReshowDelay; ShowAlways (form-active gate); balloon + icon title; overlay div in FormRenderer |
| Non-visual | `ErrorProvider` | Partial | SetError/GetError/Clear; red badge overlays positioned right of each control; hover title shows message |
| Non-visual | `Clipboard` | Good | SetText/GetText/Async; `navigator.clipboard` bridge; local-cache fallback |
| Non-visual | `Screen` | Partial | PrimaryScreen/AllScreens; Bounds/WorkingArea from `window.screen`/`window.inner*` via JS; FromControl/FromPoint/FromRectangle; GetWorkingArea/GetBounds overloads; 1920×1080 fallback before first render |
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
- Text (title bar), Icon (property; rendered as browser favicon when form is active)
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

### Partial
- MDI: `IsMdiContainer`, `MdiParent`, `MdiChildren`, `ActiveMdiChild`, `ActivateMdiChild()`, `LayoutMdi()` (Cascade/TileHorizontal/TileVertical/ArrangeIcons), `MdiChildActivate` event — all implemented via `MdiClientArea` Blazor component; constrained drag (children cannot leave workspace), 8-direction resize handles, z-index layering (active child on top), full mouse/keyboard event routing to child `Form`/`Control` handlers, child `Invalidate()` wired to canvas re-render via `RequestRender` callback; minimized icon strip; **Ctrl+Tab / Ctrl+Shift+Tab** child cycling in `MdiClientArea.OnChildKeyDown`; **`ArrangeIcons`** arranges minimized icon strips in left-to-right slots along the bottom of the MDI client area

### Not implemented
- ~~OwnedForms collection (stub only)~~ → **Implemented**: `AddOwnedForm(Form)` / `RemoveOwnedForm(Form)` are now `public` (matching WinForms API); closing the owner form now closes all owned forms via `CloseReason.FormOwnerClosing`

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
- ImageIndex, ImageKey, ImageList stubs (accepted; rendered via ImageList when set; TextImageRelation layout TBD)

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
- DropDown, DropDownClosed, SelectedIndexChanged, TextChanged, SelectionChangeCommitted
- **Editable `DropDown` input** — `OnKeyPress` routes printable characters and Backspace into `_text`; programmatic `Text` setter syncs internal user-text state
- **`DropDownList` type-ahead** — typing a character selects the first item whose text starts with that character (wraps around)
- **`AutoCompleteMode`** (`Suggest`, `Append`, `SuggestAppend`, `None`) — fully implemented:
  - `Append`: best-matching suffix appended after typed text, rendered with blue selection highlight; advancing through suffix on matching keystrokes
  - `Suggest`: dropdown opens and pre-highlights the first matching item
  - `SuggestAppend`: both behaviours combined
  - Enter commits the suffix / highlighted suggestion; Escape reverts to user-typed text; LostFocus commits
- **`AutoCompleteSource`** (`ListItems` / `CustomSource`) — `CustomSource` reads `AutoCompleteCustomSource`; all other sources fall back to `ListItems`
- **`FindString(s)` / `FindString(s, startIndex)`** — case-insensitive prefix search through items
- **`FindStringExact(s)` / `FindStringExact(s, startIndex)`** — case-insensitive exact match through items
- **`DrawMode`** (`OwnerDrawFixed` / `OwnerDrawVariable`) — `DrawItem` event raised for each drop-down item and for the selected-item face (with `DrawItemState.ComboBoxEdit`); `MeasureItem` raised per-item for `OwnerDrawVariable` to support variable row heights

### Partial
- AutoCompleteCustomSource population (accepted; content must be filled by caller)

---

## TreeView

### Good
- Nodes (recursive; TreeNode with Text, Tag, ImageIndex, Checked, ToolTipText)
- Expand/Collapse all levels; BeforeExpand, AfterExpand, BeforeCollapse, AfterCollapse
- SelectedNode, BeforeSelect, AfterSelect
- LabelEdit, BeforeLabelEdit, AfterLabelEdit
- CheckBoxes, NodeChecked
- BeginUpdate(), EndUpdate()
- ImageList, ImageIndex, SelectedImageIndex (icons rendered via DrawImage)
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
- SmallImageList, LargeImageList (icons rendered in Details, List, and LargeIcon views via DrawImage)
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
- Multi-column sort: AddSort(col, dir) + Ctrl+click column header appends secondary criteria; sort indicators show ▲1 ▲2 …
- Frozen columns: DataGridViewColumn.Frozen = true pins columns to the left, unaffected by horizontal scroll; separator line drawn
- Copy to clipboard: Ctrl+C / GetClipboardContent() exports tab-separated text honouring ClipboardCopyMode and current selection
- Virtualised row scroll
- AllowUserToAddRows, AllowUserToDeleteRows, ReadOnly
- DefaultCellStyle, column/row-level style overrides
- AutoResizeColumns()

#### Partial
- In-cell editing: TextBox column only; CheckBox toggle done
- CellValidating, RowValidating: events fire on selection change; `Cancel = true` blocks move and draws a red inset border on the offending cell; error clears when validation passes

#### Not implemented
- ComboBox column in-cell dropdown UI

#### Implemented (this session)
- Frozen rows (`DataGridViewRow.Frozen`): pinned below column header, unaffected by vertical scroll; two-pass paint, hit-testing, scrollbar thumb, and mouse-wheel clamp all account for frozen zone

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

## BindingNavigator

### Partial — implemented this session
- Inherits from `ToolStrip`; matches WinForms `BindingNavigator : ToolStrip` class hierarchy
- `BindingSource` property — subscribes to `PositionChanged` and `ListChanged` to refresh state
- Default item layout (matches Visual Studio designer output):
  - `MoveFirstItem` (◀◀), `MovePreviousItem` (◀), separator, `PositionItem` (current index), `CountItem` (/ total), separator, `MoveNextItem` (▶), `MoveLastItem` (▶▶), separator, `AddNewItem` (＋), `DeleteItem` (✕)
- All navigation buttons delegate to `BindingSource.MoveFirst/MovePrevious/MoveNext/MoveLast`
- `AddNewItem` calls `BindingSource.Add(null)` + `MoveLast`; `DeleteItem` calls `BindingSource.RemoveAt(Position)`
- `OnAddNew()` / `OnDeleteCurrent()` are `protected virtual` — override to customise
- Buttons auto-enable/disable based on current position (First/Prev disabled at start; Next/Last disabled at end)
- **`PositionItem` is now a `ToolStripTextBox`** — user can type a 1-based record number and press Enter to jump directly to that record; invalid input reverts to actual position

### Not implemented
- (Position field is now editable — previous gap closed)

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

#### Implemented (this session)
- **Mouse wheel**: scroll up/down maps to SmallDecrement/SmallIncrement on the focused scrollbar
- **WinForms effective-maximum clamping**: `Value` is clamped to `Maximum - LargeChange + 1` (matching WinForms `ScrollBar` semantics); `EffectiveMaximum` helper used throughout `RaiseScroll`, keyboard `End` key, and thumb-drag

---

## DomainUpDown

### Partial - DomainUpDown : UpDownBase : ContainerControl hierarchy (matches WinForms)
- Items (DomainUpDownItemCollection: Add, Insert, Remove, Clear, IndexOf, Contains)
- SelectedIndex, SelectedItem
- Sorted, Wrap, TextAlign
- ReadOnly (via UpDownBase)
- SelectedItemChanged event
- Keyboard Up/Down navigation

#### Implemented (this session)
- **Mouse wheel**: scroll up/down cycles through items (respects Wrap)
- **First-letter type-ahead**: pressing a printable character selects the next item whose text starts with that character; search wraps around
- **Home / End**: jump to first / last item

---

## NumericUpDown / UpDownBase

### Partial
- Value, Minimum, Maximum, Increment
- DecimalPlaces, ThousandsSeparator, Hexadecimal
- TextAlign, ReadOnly, InterceptArrowKeys
- Direct keyboard entry with buffer; Enter to commit
- UpButton(), DownButton()
- ValueChanged event

#### Implemented (this session)
- **Mouse wheel**: scroll up/down increments/decrements `Value`
- **Live typing buffer display**: text area shows the in-progress typed string rather than the committed value while editing
- **SelectAll on focus**: entire text is highlighted when the control gains focus (blue overlay); first keystroke replaces
- **Ctrl+A**: programmatic select-all mirrors WinForms `SelectAll()`
- **Escape**: discards the current typing buffer and reverts display to committed value
- **PageUp / PageDown**: large-step navigation by `Increment × 10`; commits any active typing buffer first
- **`Text` property**: string get/set wrapper around `Value` for designer compatibility
- **`OnPaint` selection highlight**: semi-transparent blue overlay drawn over the text area when all-selected

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

## ToolTip

### Partial — canvas overlay approach

#### Implemented
- `SetToolTip(control, text)` / `GetToolTip(control)` / `RemoveAll()` — associate text with controls
- `MouseEnter` on a registered control starts the `InitialDelay` timer; the tooltip appears automatically
- `MouseLeave` / `MouseDown` cancel the pending timer and hide the tooltip
- `AutoPopDelay` — tooltip is auto-hidden after this many ms once it appears
- `ReshowDelay` property present (used when re-entering a control before AutoPopDelay expires — accepted, not yet differentiated from InitialDelay)
- `Show(text, control)` / `Show(text, control, duration)` / `Show(text, control, point)` / `Show(text, control, point, duration)` — manual show with optional position and duration
- `Hide(control)` — manual hide
- `Active` — when `false`, tooltips are suppressed
- `IsBalloon` — renders with rounded 8px border instead of 3px
- `ToolTipTitle` + `ToolTipIcon` (None / Info / Warning / Error) — renders bold title row with colour-coded badge above the tip text
- Rendered as an absolutely-positioned `<div>` overlay in `FormRenderer.razor` (pointer-events: none), z-index 99999
- `ToolTipRegistry` (static) — change-event bus so the renderer re-renders on show/hide transitions
- `Dispose()` unregisters all hooks and cancels pending timers
- `ReshowDelay` — when a tooltip was recently dismissed (within `AutoPopDelay` ms), the next hover uses `ReshowDelay` instead of the full `InitialDelay`
- `ShowAlways` — when `false` (default) tooltips are suppressed if the control's parent form is not the active form; when `true` tooltips always show regardless of form focus

#### Not implemented
- System-level OS tooltip for controls outside the canvas (e.g. `NotifyIcon` text uses its own tooltip)
- Per-control `AutoPopDelay` override (global setting only)

---

## ErrorProvider

### Partial — canvas overlay approach

#### Implemented
- `SetError(control, message)` — sets or clears the error for a control; empty/null clears
- `GetError(control)` — returns the current error string for a control
- `Clear()` — clears all errors managed by this provider
- `HasError(control)` — convenience predicate
- `BlinkRate` (int, ms) — accepted; no actual blinking in canvas
- `BlinkStyle` (`AlwaysBlink`, `BlinkIfDifferentError`, `NeverBlink`) — accepted
- `ContainerControl` — property accepted
- `Icon` — property accepted (custom icon path stored; canvas renders standard red badge)
- `DataSource` / `DataMember` — properties accepted (auto-wiring not yet implemented)
- `BindValidation(control, handler)` — convenience hook that wires `control.Validating`
- `Dispose()` — clears all errors from the registry
- **`ErrorProviderRegistry`** (static) — change-event bus; holds all active `ErrorProviderEntry` records (form-relative X/Y, message, DOM id)
- **Rendering** — `FormRenderer` iterates `ErrorProviderRegistry.Entries` and renders a red `!` circle badge (16 × 16 px, z-index 99998) positioned to the right of each affected control; browser-native `title` attribute provides the hover message
- **Blinking** — `BlinkRate` drives animation period (`BlinkRate × 2` ms per CSS cycle); `AlwaysBlink` → `infinite` iterations; `BlinkIfDifferentError` → 5 iterations; `NeverBlink` → no animation; CSS keyframe uses abrupt step-start toggle matching WinForms behaviour

#### Not implemented
- Auto-validation wired from `DataSource` / `DataMember` (stub — must call `SetError` manually)
- `IExtenderProvider` compile-time property extension (designer-only concept; not applicable)

---

## Legacy Menu / Toolbar Controls

### Partial — implemented for translator compatibility

#### MenuItem
- Wraps `ToolStripMenuItem`; `Text`, `Enabled`, `Visible`, `Checked`, `Shortcut`, `ShowShortcut`
- `MenuItems` collection (nested sub-items added to `ToolStripMenuItem.DropDownItems`)
- `Click`, `Popup`, `Select` events; `PerformClick()`; `RadioCheck`, `OwnerDraw` stubs
- `Shortcut` legacy enum — values match `Keys` int representation (safe cast)

#### MainMenu
- Wraps `MenuStrip`; `MenuItems` collection adds to `MenuStrip.Items`
- `Form.Menu` property: sets `MainMenuStrip`, adds wrapped `MenuStrip` to `Form.Controls`
- `RightToLeft` forwarded to inner `MenuStrip`

#### ContextMenu
- Wraps `ContextMenuStrip`; `MenuItems` collection adds to `ContextMenuStrip.Items`
- `Control.ContextMenu` setter wires `value._strip` to `ContextMenuStrip`
- `Popup` event raised before `Show(control, pos)` (matches WinForms behavior)
- `Show(Control, Point)` delegates to `ContextMenuStrip.Show`

#### ToolBarButton
- Wraps `ToolStripButton`; `Text`, `ToolTipText`, `Enabled`, `Visible`, `Pushed`, `Image`, `ImageIndex`, `Name`, `Tag`
- `Style` enum: `PushButton`, `ToggleButton`, `Separator` (inserted as `ToolStripSeparator`), `DropDownButton`
- `DropDownMenu` property (`Menu` interface — accepts `MainMenu` or `ContextMenu`)
- `Click` event forwarded from inner button

#### ToolBar
- Extends `Control`, wraps `ToolStrip` internally
- `Buttons` collection; `Appearance`, `TextAlign`, `ShowToolTips`, `Wrappable`, `ButtonSize`, `ImageSize`, `ImageList`
- `ButtonClick` event (`ToolBarButtonClickEventArgs.Button`)
- `OnPaint` syncs inner `ToolStrip` bounds and delegates paint

### Not implemented
- `DropDownButton` arrow + menu display (stub property only)
- Owner-draw events (`DrawItem` on `ToolBar`)

---

## SplitContainer

### Good
- Panel1, Panel2, SplitterDistance, SplitterWidth

- IsSplitterFixed, SplitterMoved, SplitterMoving
- Double-click to reset splitter

---

## GroupBox

### Partial
- FlatStyle (Standard / Flat / System) + etched / flat border rendering
- Caption text with gap in border
- Child mouse routing, clipping

#### Implemented (this session)
- **`AutoSize`**: when enabled, the GroupBox resizes to wrap all visible children; width/height padded with 8px margins and caption height clearance
- **`AutoSizeMode`** (`GrowOnly` / `GrowAndShrink`): controls whether the auto-sized box can shrink below its current explicit size

---

## Panel

### Partial
- BorderStyle (None / FixedSingle / Fixed3D)
- Child painting + input routing; scroll offset via ScrollableControl

#### Implemented (this session)
- **`AutoSize`**: when enabled, the Panel resizes to wrap all visible children; accounts for border width and Padding on all sides
- **`AutoSizeMode`** (`GrowOnly` / `GrowAndShrink`): controls whether the panel can shrink below its current explicit size

---

## RichTextBox (container section)

### Partial
- `Rtf`, `HtmlContent`, styled-run rendering (bold/italic/underline/colour/font-size)
- `SelectionFont`, `SelectionColor`, `SelectionBold`, `SelectionItalic`, `SelectionUnderline`
- `Find()` / `LoadFile()` / `SaveFile()`
- HTML clipboard round-trip (Copy/Paste)
- `ScrollToCaret()` — stub previously; now functional (see Text section above)

#### Implemented (this session)
- **`ZoomFactor`**: each RTF run's rendered font size is multiplied by `ZoomFactor` in `DrawRtfRuns`; `Invalidate()` is called on assignment
- **`ScrollToCaret()`**: estimates the line index of the current caret position by counting newlines, converts to pixel offset, and adjusts `_scrollOffsetY` to bring the line into the visible text bounds; clamps to `>= 0`

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

## CheckedListBox

### Partial
- Items, GetItemChecked/SetItemChecked, GetItemCheckState/SetItemCheckState
- CheckOnClick, ThreeState
- CheckedIndices, CheckedItems collections
- ItemCheck event (with ItemCheckEventArgs.NewValue / CurrentValue)
- Mouse click on checkbox area toggles state; selection tracking

#### Implemented (this session)
- **Mouse wheel**: scrolls the visible item list up/down (speed scales with `e.Delta`)
- **First-letter type-ahead**: pressing a printable character moves selection to the next item whose text starts with that character; wraps around

---

## ProgressBar

### Partial
- Minimum, Maximum, Value, Step
- PerformStep(), Increment()
- Style (Blocks / Continuous / Marquee); MarqueeAnimationSpeed
- RightToLeftLayout
- Gradient + rounded-rect rendering for all three styles

#### Implemented (this session)
- **`SetRange(min, max)`**: sets Minimum and Maximum in one call, matching WinForms API; clamps current Value
- **`ShowPercentage`**: when `true`, draws the integer percentage text centred over the bar track (non-WinForms extension; useful for translated apps that previously used label overlays)

---

## TabControl

### Partial
- TabPages collection; Add, Remove, Insert
- SelectedIndex, SelectedTab
- Alignment (Top / Bottom / Left / Right)
- Appearance, SizeMode, Multiline, HotTrack
- ImageList / per-tab ImageIndex/ImageKey
- Selecting / Selected / Deselecting / Deselected / SelectedIndexChanged events
- Header scroll buttons for single-row overflow
- DrawMode (Normal / OwnerDrawFixed) + DrawItem

#### Implemented (this session)
- **`TabCount`** property: returns `TabPages.Count` (matches WinForms public API)
- **`GetTabRect(index)`**: returns the bounding `Rectangle` of the header tab at the given index; builds header rects on demand if stale

---

## PropertyGrid

### Partial — implemented

#### Core
- `SelectedObject` / `SelectedObjects` — sets the reflected target; rebuilds row list on assignment
- `PropertySort`: `Alphabetical`, `Categorized`, `CategorizedAlphabetical`, `NoSort`
- `HelpVisible` — toggles the 56px description panel at the bottom
- `ToolbarVisible` — toggles the 24px toolbar with categorised/alphabetical sort buttons

#### Reflection / row building
- Reads all public instance readable properties via `System.Reflection`
- Respects `[Category]` and `[Description]` attributes from `System.ComponentModel`
- Groups into collapsible category rows when `PropertySort` is categorized
- Alphabetical ordering applied within each category when `CategorizedAlphabetical`

#### Rendering (OnPaint)
- Two-column layout: name column | value column, separated by a draggable splitter
- Category rows highlighted with distinct background; expand/collapse `+`/`−` box
- Selected row highlighted with system blue; value text in blue tint for property rows
- Inline editor overlay (white box + blue border) drawn on the selected value cell while editing
- Caret rendering inside inline editor
- Description panel shows property name (bold) and `[Description]` text

#### Interaction
- Mouse click on row → selects it; click on value column → starts inline edit
- Click on category expand/collapse box → toggles `Expanded`
- Splitter drag → adjusts name/value column split
- Mouse wheel → scrolls the grid
- Keyboard: `Up`/`Down`/`PageUp`/`PageDown`/`Home`/`End` navigate; `F2`/`Enter` → start edit; `Escape` → cancel; `Left`/`Right` → expand/collapse category; printable chars → start edit immediately

#### Value editing
- `CommitEdit` converts the text buffer to the property's declared type using `TypeConverter` with numeric/bool/enum fast paths
- Calls `prop.SetValue` on the owner object; fires `PropertyValueChanged`
- Conversion failure silently restores the old display value
- Read-only properties (`CanWrite == false`) are not editable

#### Events
- `SelectedGridItemChanged` (`SelectedGridItemChangedEventArgs`: `OldSelection`, `NewSelection`)
- `PropertyValueChanged` (`PropertyValueChangedEventArgs`: `ChangedItem`, `OldValue`)

#### Public API
- `Refresh()` — rebuilds and repaints
- `CollapseAllGridItems()` / `ExpandAllGridItems()`
- `SelectedGridItem` — get/set; fires `SelectedGridItemChanged`
- `GridItem`: `Label`, `Value`, `PropertyInfo`, `IsCategory`, `Category`, `Description`, `Expandable`, `Expanded`, `Children`, `Parent`, `IsReadOnly`, `IsNonDefault`, `Depth`
- `GridItemType` enum: `Property`, `Category`, `ArrayValue`, `Root`
- `PropertySort` enum: `NoSort`, `Alphabetical`, `Categorized`, `CategorizedAlphabetical`
- **Read-only property greying** — properties with no public setter or `[ReadOnly(true)]` render with grey name text
- **Bold non-default values** — property names rendered bold when value differs from `[DefaultValue]` attribute (or from `default(T)` for value types)
- **Nested object expansion** — complex-type property values show a [+]/[−] expand box; sub-properties render as indented child rows (up to depth 2); toggled on click; `CollapseAllGridItems`/`ExpandAllGridItems` recurse into sub-items

### Not implemented
- `UITypeEditor` drop-down / modal editors (colour picker, enum drop-down, etc.)
- Custom `TypeConverter` descriptions in the drop-down

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


---

## Screen

### Partial — single virtual browser screen

#### Implemented
- `Screen.PrimaryScreen` — returns the one virtual browser screen
- `Screen.AllScreens` — single-element array containing `PrimaryScreen`
- `Bounds` — `System.Drawing.Rectangle(0, 0, window.screen.width, window.screen.height)`
- `WorkingArea` — `System.Drawing.Rectangle(0, 0, window.innerWidth, window.innerHeight)` (viewport after browser chrome)
- `BitsPerPixel` — from `window.screen.colorDepth`
- `Primary` — always `true`; `DeviceName` — always `\\\\.\\DISPLAY1`
- `Screen.FromControl(control)` / `FromPoint(pt)` / `FromRectangle(rect)` — all return `PrimaryScreen`
- `Screen.GetWorkingArea(point/rect/control)` — returns `PrimaryScreen.WorkingArea`
- `Screen.GetBounds(point/rect/control)` — returns `PrimaryScreen.Bounds`
- JS interop via `window.getScreenInfo` called by `FormRenderer` on first render; 1920x1080 fallback before first render

#### Not implemented
- Multi-monitor support (browser exposes only one screen)
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
- `Screen`: one virtual browser screen; `PrimaryScreen.Bounds` = `window.screen` dimensions; `WorkingArea` = `window.innerWidth/Height`; no multi-monitor support
- MDI: constrained drag/resize, z-index, mouse/keyboard routing, and child invalidation re-render all implemented (see `Form` section); Ctrl+Tab child cycling and `ArrangeIcons` now implemented
- No `PrintDocument` / print preview (no printer access from WASM)
- `DoDragDrop` return value is async — the IL translator patches call-sites automatically (see above)
- `WebBrowser` / `WebView2`: cross-origin `Document` DOM access blocked by browser sandbox; `GoBack`/`GoForward` only work for same-origin history entries

---

For per-property tracking see `Canvas.Windows.Forms.Tests/PROPERTY_COMPLETENESS.md` and `CONTROLS_IMPLEMENTATION_STRATEGY.md`.