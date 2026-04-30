using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demonstrates MenuStrip, ToolStrip and ContextMenuStrip with small inline SVG icons.
/// </summary>
public class MenuDemoForm : Form
{
    // ── Server-side icon helpers ───────────────────────────────────────────────
    // Icons live in wwwroot/images/ and are served by the host at /images/*.svg.
    // When a translated WinForms app is loaded, its images would be in the same
    // location alongside the host application.

    private static Image Icon(string filename)
        => new Image { Source = $"/images/{filename}", Width = 16, Height = 16 };

    private static readonly Image IconNew     = Icon("new.svg");
    private static readonly Image IconOpen    = Icon("open.svg");
    private static readonly Image IconSave    = Icon("save.svg");
    private static readonly Image IconCut     = Icon("cut.svg");
    private static readonly Image IconCopy    = Icon("copy.svg");
    private static readonly Image IconPaste   = Icon("paste.svg");
    private static readonly Image IconUndo    = Icon("undo.svg");
    private static readonly Image IconRedo    = Icon("redo.svg");
    private static readonly Image IconBold    = Icon("bold.svg");
    private static readonly Image IconItalic  = Icon("italic.svg");
    private static readonly Image IconZoomIn  = Icon("zoom-in.svg");
    private static readonly Image IconZoomOut = Icon("zoom-out.svg");
    private static readonly Image IconInfo    = Icon("info.svg");
    private static readonly Image IconWarn    = Icon("warn.svg");
    private static readonly Image IconGrid    = Icon("grid.svg");

    // ── State ──────────────────────────────────────────────────────────────────
    private ListBox?              _log;
    private ToolStripStatusLabel? _statusLabel;
    private bool      _boldOn;
    private bool      _italicOn;
    private bool      _gridOn;
    private int       _zoom = 100;

    // ── Constructor ────────────────────────────────────────────────────────────
    public MenuDemoForm()
    {
        Text = "Menus & ToolStrip Demo";
        Width  = 720;
        Height = 560;
        BackColor = Color.White;
        AllowResize = true;

        BuildMenuStrip();
        BuildToolStrip();
        BuildBody();
        BuildContextMenu();

        Log("Form ready. Right-click the log area for a ContextMenuStrip.");
    }

    // ── MenuStrip ─────────────────────────────────────────────────────────────
    private void BuildMenuStrip()
    {
        var menu = new MenuStrip();

        // ── File ──────────────────────────────────────────────────────────────
        var fileMenu = new ToolStripMenuItem("File");

        var newItem  = new ToolStripMenuItem("New",  IconNew,  (s, e) => Log("File → New"));
        var openItem = new ToolStripMenuItem("Open…", IconOpen, (s, e) => Log("File → Open…"));
        var saveItem = new ToolStripMenuItem("Save",  IconSave, (s, e) => Log("File → Save"));
        saveItem.ShortcutKeys = Keys.Control | Keys.S;
        saveItem.ShortcutKeyDisplayString = "Ctrl+S";

        var recentMenu = new ToolStripMenuItem("Recent Files");
        recentMenu.DropDownItems.Add("demo1.txt",  null, (s, e) => Log("Recent → demo1.txt"));
        recentMenu.DropDownItems.Add("report.cs",  null, (s, e) => Log("Recent → report.cs"));
        recentMenu.DropDownItems.Add("notes.md",   null, (s, e) => Log("Recent → notes.md"));

        fileMenu.DropDownItems.Add(newItem);
        fileMenu.DropDownItems.Add(openItem);
        fileMenu.DropDownItems.Add(saveItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(recentMenu);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("Exit", null, (s, e) => Close()));

        // ── Edit ──────────────────────────────────────────────────────────────
        var editMenu = new ToolStripMenuItem("Edit");

        var undoItem  = new ToolStripMenuItem("Undo",  IconUndo,  (s, e) => Log("Edit → Undo"));
        var redoItem  = new ToolStripMenuItem("Redo",  IconRedo,  (s, e) => Log("Edit → Redo"));
        undoItem.ShortcutKeyDisplayString = "Ctrl+Z";
        redoItem.ShortcutKeyDisplayString = "Ctrl+Y";
        undoItem.Enabled = false; // demo stub

        var cutItem   = new ToolStripMenuItem("Cut",   IconCut,   (s, e) => Log("Edit → Cut"));
        var copyItem  = new ToolStripMenuItem("Copy",  IconCopy,  (s, e) => Log("Edit → Copy"));
        var pasteItem = new ToolStripMenuItem("Paste", IconPaste, (s, e) => Log("Edit → Paste"));

        editMenu.DropDownItems.Add(undoItem);
        editMenu.DropDownItems.Add(redoItem);
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(cutItem);
        editMenu.DropDownItems.Add(copyItem);
        editMenu.DropDownItems.Add(pasteItem);

        // ── View ──────────────────────────────────────────────────────────────
        var viewMenu = new ToolStripMenuItem("View");

        var boldItem   = new ToolStripMenuItem("Bold",        IconBold,    (s, e) => ToggleBold())   { CheckOnClick = true };
        var italicItem = new ToolStripMenuItem("Italic",      IconItalic,  (s, e) => ToggleItalic()) { CheckOnClick = true };
        var gridItem   = new ToolStripMenuItem("Show Grid",   IconGrid,    (s, e) => ToggleGrid())   { CheckOnClick = true };

        var zoomMenu = new ToolStripMenuItem("Zoom");
        foreach (var pct in new[] { 75, 100, 125, 150 })
        {
            var label = $"{pct}%";
            zoomMenu.DropDownItems.Add(new ToolStripMenuItem(label, null, (s, e) => SetZoom(int.Parse(label[..^1]))));
        }
        zoomMenu.DropDownItems.Add(new ToolStripSeparator());
        zoomMenu.DropDownItems.Add(new ToolStripMenuItem("Zoom In",  IconZoomIn,  (s, e) => SetZoom(_zoom + 25)));
        zoomMenu.DropDownItems.Add(new ToolStripMenuItem("Zoom Out", IconZoomOut, (s, e) => SetZoom(_zoom - 25)));

        viewMenu.DropDownItems.Add(boldItem);
        viewMenu.DropDownItems.Add(italicItem);
        viewMenu.DropDownItems.Add(gridItem);
        viewMenu.DropDownItems.Add(new ToolStripSeparator());
        viewMenu.DropDownItems.Add(zoomMenu);

        // ── Help ──────────────────────────────────────────────────────────────
        var helpMenu = new ToolStripMenuItem("Help");
        helpMenu.DropDownItems.Add(new ToolStripMenuItem("Documentation…", IconInfo, (s, e) => Log("Help → Documentation")));
        helpMenu.DropDownItems.Add(new ToolStripMenuItem("Report Issue…",  IconWarn, (s, e) => Log("Help → Report Issue")));
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add(new ToolStripMenuItem("About",          null,     (s, e) => ShowAbout()));

        menu.Items.Add(fileMenu);
        menu.Items.Add(editMenu);
        menu.Items.Add(viewMenu);
        menu.Items.Add(helpMenu);

        Controls.Add(menu);
    }

    // ── ToolStrip ─────────────────────────────────────────────────────────────
    private void BuildToolStrip()
    {
        var bar = new ToolStrip
        {
            Top  = 24,   // below the MenuStrip
            Left = 0,
            Width  = Width,
            Height = 28,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            BackColor = Color.FromArgb(245, 245, 245)
        };

        ToolStripButton Btn(string tip, Image icon, Action act)
        {
            var b = new ToolStripButton(tip, icon, (s, e) => { act(); Log($"ToolStrip → {tip}"); }) { ToolTipText = tip };
            return b;
        }

        // New / Open / Save cluster
        bar.Items.Add(Btn("New",  IconNew,  () => { }));
        bar.Items.Add(Btn("Open", IconOpen, () => { }));
        bar.Items.Add(Btn("Save", IconSave, () => { }));
        bar.Items.Add(new ToolStripSeparator());

        // Edit cluster
        bar.Items.Add(Btn("Cut",   IconCut,   () => { }));
        bar.Items.Add(Btn("Copy",  IconCopy,  () => { }));
        bar.Items.Add(Btn("Paste", IconPaste, () => { }));
        bar.Items.Add(new ToolStripSeparator());

        // Undo / Redo
        var undoBtn = Btn("Undo", IconUndo, () => { });
        undoBtn.Enabled = false;
        bar.Items.Add(undoBtn);
        bar.Items.Add(Btn("Redo", IconRedo, () => { }));
        bar.Items.Add(new ToolStripSeparator());

        // Format toggles
        var boldBtn = new ToolStripButton("Bold", IconBold, (s, e) => ToggleBold()) { ToolTipText = "Toggle Bold", CheckOnClick = true };
        var italicBtn = new ToolStripButton("Italic", IconItalic, (s, e) => ToggleItalic()) { ToolTipText = "Toggle Italic", CheckOnClick = true };
        bar.Items.Add(boldBtn);
        bar.Items.Add(italicBtn);
        bar.Items.Add(new ToolStripSeparator());

        // Zoom combo
        var zoomCombo = new ToolStripComboBox("zoom");
        foreach (var z in new[] { "75%", "100%", "125%", "150%" }) zoomCombo.Items.Add(z);
        zoomCombo.SelectedIndex = 1;
        bar.Items.Add(new ToolStripLabel("Zoom: "));
        bar.Items.Add(zoomCombo);

        Controls.Add(bar);
    }

    // ── Body ──────────────────────────────────────────────────────────────────
    private void BuildBody()
    {
        // Description label
        var desc = new Label
        {
            Text = "This demo shows MenuStrip (File/Edit/View/Help), a ToolStrip, and a ContextMenuStrip (right-click the log).",
            Left = 8, Top = 58, Width = Width - 24, Height = 36,
            ForeColor = Color.FromArgb(50, 50, 50),
            BackColor = Color.FromArgb(255, 255, 224),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        Controls.Add(desc);

        // Log / output ListBox
        _log = new ListBox
        {
            Left = 8, Top = 100, Width = Width - 24, Height = Height - 165,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
            BackColor = Color.FromArgb(250, 250, 250)
        };
        Controls.Add(_log);

        // Status bar at the bottom using StatusStrip
        var statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel
        {
            Spring      = true,
            Text        = "Ready",
            Alignment   = ToolStripItemAlignment.Left,
            BorderSides = ToolStripStatusLabelBorderSides.None
        };
        var zoomLabel = new ToolStripStatusLabel { Text = "Zoom: 100%", Alignment = ToolStripItemAlignment.Right };
        statusStrip.Items.Add(_statusLabel);
        statusStrip.Items.Add(new ToolStripSeparator());
        statusStrip.Items.Add(zoomLabel);
        Controls.Add(statusStrip);
    }

    // ── ContextMenuStrip ──────────────────────────────────────────────────────
    private void BuildContextMenu()
    {
        var ctx = new ContextMenuStrip();
        ctx.Opening += (s, e) => Log("ContextMenuStrip opening…");
        ctx.Closed  += (s, e) => Log("ContextMenuStrip closed.");

        ctx.Items.Add(new ToolStripMenuItem("Copy Log",    IconCopy, (s, e) => Log("Context → Copy Log")));
        ctx.Items.Add(new ToolStripMenuItem("Clear Log",   null,     (s, e) => ClearLog()));
        ctx.Items.Add(new ToolStripSeparator());

        var fontMenu = new ToolStripMenuItem("Font");
        fontMenu.DropDownItems.Add(new ToolStripMenuItem("Bold",   IconBold,   (s, e) => ToggleBold())   { CheckOnClick = true });
        fontMenu.DropDownItems.Add(new ToolStripMenuItem("Italic", IconItalic, (s, e) => ToggleItalic()) { CheckOnClick = true });
        ctx.Items.Add(fontMenu);

        ctx.Items.Add(new ToolStripSeparator());
        ctx.Items.Add(new ToolStripMenuItem("Zoom In",  IconZoomIn,  (s, e) => SetZoom(_zoom + 25)));
        ctx.Items.Add(new ToolStripMenuItem("Zoom Out", IconZoomOut, (s, e) => SetZoom(_zoom - 25)));
        ctx.Items.Add(new ToolStripSeparator());
        ctx.Items.Add(new ToolStripMenuItem("Properties…", IconInfo, (s, e) => ShowAbout()));

        if (_log is not null)
            _log.ContextMenuStrip = ctx;
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void ToggleBold()
    {
        _boldOn = !_boldOn;
        Log($"Bold {(_boldOn ? "ON" : "OFF")}");
        UpdateStatus();
    }

    private void ToggleItalic()
    {
        _italicOn = !_italicOn;
        Log($"Italic {(_italicOn ? "ON" : "OFF")}");
        UpdateStatus();
    }

    private void ToggleGrid()
    {
        _gridOn = !_gridOn;
        Log($"Grid {(_gridOn ? "shown" : "hidden")}");
        UpdateStatus();
    }

    private void SetZoom(int pct)
    {
        _zoom = Math.Clamp(pct, 25, 400);
        Log($"Zoom set to {_zoom}%");
        UpdateStatus();
    }

    private void ClearLog()
    {
        _log?.Items.Clear();
        Log("Log cleared.");
    }

    private void ShowAbout()
    {
        Log("About: MenuDemoForm — MenuStrip + ToolStrip + ContextMenuStrip demo for CanvasForms.");
    }

    private void Log(string message)
    {
        _log?.Items.Add($"[{DateTime.Now:HH:mm:ss.ff}] {message}");
        if (_log is not null && _log.Items.Count > 0)
            _log.SelectedIndex = _log.Items.Count - 1;
        UpdateStatus(message);
    }

    private void UpdateStatus(string? msg = null)
    {
        if (_statusLabel is null) return;
        var parts = new List<string>();
        if (_boldOn)   parts.Add("Bold");
        if (_italicOn) parts.Add("Italic");
        if (_gridOn)   parts.Add("Grid");
        parts.Add($"Zoom: {_zoom}%");
        if (msg is not null) parts.Insert(0, msg);
        _statusLabel.Text = string.Join("  |  ", parts);
        _statusLabel.Owner?.Invalidate();
    }
}
