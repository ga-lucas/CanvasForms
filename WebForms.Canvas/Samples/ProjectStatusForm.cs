using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Displays the CanvasForms controls roadmap — implementation status for every WinForms control.
/// </summary>
public class ProjectStatusForm : Form
{
    public ProjectStatusForm()
    {
        Text = "Project Status — Controls Roadmap";
        Width = 820;
        Height = 620;
        BackColor = Color.White;
        AllowResize = true;
        AllowMove = true;
        MinimumWidth = 600;
        MinimumHeight = 400;

        InitializeControls();
        PerformLayout();
    }

    private void InitializeControls()
    {
        // Header label
        var header = new Label
        {
            Text = "CanvasForms — WinForms Controls Roadmap",
            Left = 10,
            Top = 10,
            Width = 780,
            Height = 30,
            ForeColor = Color.FromArgb(26, 115, 232),
            BackColor = Color.FromArgb(240, 248, 255),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(header);

        // Legend
        var legend = new Label
        {
            Text = "✅ Good  |  ⚠️ Partial  |  🧩 Stub/Compat  |  🔲 Not started",
            Left = 10,
            Top = 48,
            Width = 780,
            Height = 22,
            ForeColor = Color.FromArgb(80, 80, 80),
            BackColor = Color.FromArgb(255, 255, 224),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(legend);

        // ListView
        var lv = new ListView
        {
            Left = 10,
            Top = 78,
            Width = 780,
            Height = 490,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false
        };

        lv.Columns.Add("Tier", 70);
        lv.Columns.Add("Area", 110);
        lv.Columns.Add("Control", 180);
        lv.Columns.Add("Status", 100);
        lv.Columns.Add("Notes", 300);

        foreach (var entry in GetRoadmapEntries())
        {
            var item = new ListViewItem(entry.Tier);
            item.SubItems.Add(entry.Area);
            item.SubItems.Add(entry.Control);
            item.SubItems.Add(entry.Status);
            item.SubItems.Add(entry.Notes);
            lv.Items.Add(item);
        }

        Controls.Add(lv);
    }

    private record RoadmapEntry(string Tier, string Area, string Control, string Status, string Notes);

    private static IEnumerable<RoadmapEntry> GetRoadmapEntries() =>
    [
        // ── Tier 1 ──────────────────────────────────────────────────────────────
        new("Tier 1", "Windowing",  "Form",                           "⚠️ Partial",      "Chrome, move/resize, min/max/close, start position, key preview."),
        new("Tier 1", "Core",       "Control",                        "⚠️ Partial",      "Full property/event surface; layout, focus, tab navigation."),
        new("Tier 1", "Buttons",    "Button / ButtonBase",            "✅ Good",          "Hover/pressed/focus + click via mouse/keyboard."),
        new("Tier 1", "Buttons",    "CheckBox",                       "✅ Good",          "Toggle behavior + indicator rendering."),
        new("Tier 1", "Buttons",    "RadioButton",                    "✅ Good",          "Mutual exclusivity within parent."),
        new("Tier 1", "Text",       "Label",                          "✅ Good",          "Multi-line, alignment, UseMnemonic, AutoEllipsis, AutoSize, BorderStyle."),
        new("Tier 1", "Text",       "LinkLabel",                      "⚠️ Partial",      "Click/visited + optional browser navigation via LinkUrl."),
        new("Tier 1", "Text",       "TextBox / TextBoxBase",          "✅ Good",          "Editing, selection, shortcuts, redo, word-delete, placeholder, autocomplete."),
        new("Tier 1", "Lists",      "ListBox",                        "✅ Good",          "Selection + navigation; owner-draw, MeasureItem, IntegralHeight."),
        new("Tier 1", "Lists",      "ComboBox",                       "⚠️ Partial",      "Drop-down + selection; autocomplete partial."),
        new("Tier 1", "Containers", "Panel / ScrollableControl",      "⚠️ Partial",      "Child painting + input routing; scroll offset support."),
        new("Tier 1", "Containers", "GroupBox",                       "⚠️ Partial",      "Border/caption + child routing/clipping."),
        new("Tier 1", "Containers", "SplitContainer",                 "✅ Good",          "Resizable pane splitter; fixed/min-size; double-click reset."),
        new("Tier 1", "Layout",     "FlowLayoutPanel",                "✅ Good",          "FlowDirection + wrap/break + SetFlowBreak."),
        new("Tier 1", "Layout",     "TableLayoutPanel",               "✅ Good",          "Row/column styles + spans; CellBorderStyle; GetControlFromPosition."),
        new("Tier 1", "Collections","TreeView",                       "✅ Good",          "Nodes, expand/collapse, selection; LabelEdit; BeginUpdate/EndUpdate."),
        new("Tier 1", "Collections","ListView",                       "✅ Good",          "Details/List/LargeIcon views; keyboard nav; EnsureVisible; BeginUpdate/EndUpdate."),
        new("Tier 1", "Menus",      "MenuStrip",                      "⚠️ Partial",      "Top-level menu bar with dropdowns."),
        new("Tier 1", "Menus",      "ContextMenuStrip",               "⚠️ Partial",      "Right-click overlay menus."),
        new("Tier 1", "Menus",      "ToolStrip",                      "⚠️ Partial",      "Toolbar with icons, hover, checked state."),
        new("Tier 1", "Menus",      "StatusStrip / StatusLabel",      "⚠️ Partial",      "Status bar; Spring, BorderSides, SizingGrip."),
        new("Tier 1", "Display",    "PictureBox",                     "✅ Good",          "URL/Image; Load/LoadAsync; SizeMode; LoadCompleted events."),
        new("Tier 1", "Display",    "ProgressBar",                    "✅ Good",          "Blocks/continuous/marquee; animated MarqueeAnimationSpeed."),
        new("Tier 1", "Common",     "DateTimePicker",                 "✅ Good",          "Format/CustomFormat; ShowUpDown/ShowCheckBox; calendar styling."),
        new("Tier 1", "Common",     "NumericUpDown / UpDownBase",     "✅ Good",          "Spinner UI + value clamping; direct-type keyboard entry; TextAlign."),
        new("Tier 1", "Common",     "MonthCalendar",                  "✅ Good",          "Single-month view; SelectionRange; BoldedDates; keyboard/mouse nav."),
        new("Tier 1", "Common",     "Timer",                          "✅ Good",          "PeriodicTimer-based async loop; fires on captured SynchronizationContext."),
        new("Tier 1", "Data",       "DataGridView",                   "⚠️ Partial",      "IList/BindingSource/DataTable binding; sort; multiple column types."),
        new("Tier 1", "Dialogs",    "OpenFileDialog",                 "⚠️ Partial",      "Host FS + browser upload."),
        new("Tier 1", "Dialogs",    "SaveFileDialog",                 "⚠️ Partial",      "CreatePrompt, OverwritePrompt, OpenFile()."),
        new("Tier 1", "Dialogs",    "FolderBrowserDialog",            "⚠️ Partial",      "SelectedPath, Description, ShowNewFolderButton."),
        new("Tier 1", "Dialogs",    "ColorDialog",                    "⚠️ Partial",      "Swatch palette + Hex/RGB/HSV inputs."),
        new("Tier 1", "Dialogs",    "FontDialog",                     "⚠️ Partial",      "Family/style/size lists; ShowEffects, ShowColor, Apply event."),
        new("Tier 1", "Non-visual", "ToolTip",                        "🧩 Stub",         "API present; rendering may be incomplete."),
        new("Tier 1", "Non-visual", "ErrorProvider",                  "🔲 Not started",  "Standard form validation."),

        // ── Tier 2 ──────────────────────────────────────────────────────────────
        new("Tier 2", "Text",       "MaskedTextBox",                  "⚠️ Partial",      "Masked display + basic validation."),
        new("Tier 2", "Text",       "RichTextBox",                    "⚠️ Partial",      "Stores RTF, renders as plain text."),
        new("Tier 2", "Lists",      "CheckedListBox",                 "⚠️ Partial",      "Basic checked item behavior."),
        new("Tier 2", "Common",     "ImageList",                      "🧩 Stub",         "API present; image storage stub."),
        new("Tier 2", "Containers", "TabControl",                     "⚠️ Partial",      "Tab strip + page switching."),
        new("Tier 2", "Containers", "UserControl",                    "🧩 Stub",         "Base present; composite lifecycle partial."),
        new("Tier 2", "Menus",      "ToolStripMenuItem",              "⚠️ Partial",      "Dropdowns, check state, shortcuts, image, enabled."),
        new("Tier 2", "Menus",      "ToolStripContainer / Panel",     "🧩 Stub",         "Dockable strip host."),
        new("Tier 2", "Data",       "BindingSource",                  "✅ Good",          "IList/IBindingList wrapper; Filter/Sort/Find; server-backed via CanvasDataService."),
        new("Tier 2", "Data",       "DataGridViewColumn types",       "✅ Good",          "TextBox/CheckBox/ComboBox/Button/Image/Link column variants."),
        new("Tier 2", "Data",       "CanvasDataService",              "✅ Good",          "Server-backed ADO.NET; SQLite default; ambient Current accessor."),
        new("Tier 2", "Non-visual", "NotifyIcon",                     "🧩 Stub",         "API present; system tray stub."),

        // ── Tier 3 (not yet started) ─────────────────────────────────────────────
        new("Tier 3", "Menus",      "MainMenu / ContextMenu",         "🔲 Not started",  "Legacy pre-MenuStrip menus."),
        new("Tier 3", "Menus",      "ToolBar",                        "🔲 Not started",  "Legacy pre-ToolStrip toolbar."),
        new("Tier 3", "Input",      "TrackBar",                       "🔲 Not started",  ""),
        new("Tier 3", "Input",      "HScrollBar / VScrollBar",        "🔲 Not started",  "Standalone scroll bars."),
        new("Tier 3", "Input",      "DomainUpDown",                   "🔲 Not started",  ""),
        new("Tier 3", "Data",       "PropertyGrid",                   "🔲 Not started",  ""),
        new("Tier 3", "Data",       "BindingNavigator",               "🔲 Not started",  ""),
        new("Tier 3", "Data",       "DataGrid (legacy)",              "🔲 Not started",  ""),
        new("Tier 3", "Print",      "PrintDialog",                    "🔲 Not started",  ""),
        new("Tier 3", "Print",      "PrintPreviewDialog",             "🔲 Not started",  ""),
        new("Tier 3", "Print",      "PrintDocument",                  "🔲 Not started",  ""),
        new("Tier 3", "Print",      "PrintPreviewControl",            "🔲 Not started",  ""),
        new("Tier 3", "Legacy",     "StatusBar",                      "🔲 Not started",  "Pre-StatusStrip."),
        new("Tier 3", "Other",      "ErrorProvider",                  "🔲 Not started",  ""),
        new("Tier 3", "Other",      "HelpProvider",                   "🔲 Not started",  ""),
        new("Tier 3", "Other",      "WebBrowser / WebView2",          "🔲 Not started",  ""),
        new("Tier 3", "Other",      "Chart",                          "🔲 Not started",  ""),
        new("Tier 3", "Other",      "MDI (MdiClient, MDI Forms)",     "🔲 Not started",  ""),
        new("Tier 3", "Other",      "Screen (multi-monitor)",         "🔲 Not started",  ""),
        new("Tier 3", "Other",      "Clipboard",                      "🔲 Not started",  "JS bridge needed."),
    ];
}
