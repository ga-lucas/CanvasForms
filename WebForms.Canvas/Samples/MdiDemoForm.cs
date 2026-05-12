using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demonstrates MDI (Multiple Document Interface) support.
/// This is the MDI parent form; child document windows are spawned from its toolbar.
/// </summary>
public class MdiDemoForm : Form
{
    private int _childIndex = 0;
    private MenuStrip _menu = null!;
    private StatusStrip _status = null!;
    private ToolStripStatusLabel _statusLabel = null!;

    public MdiDemoForm()
    {
        Text             = "MDI Demo — Multiple Document Interface";
        Width            = 900;
        Height           = 620;
        IsMdiContainer   = true;
        BackColor        = Color.FromArgb(100, 100, 120);

        BuildMenu();
        BuildStatus();

        // Spawn a couple of children so the user sees something immediately
        SpawnChild("Document 1", Color.FromArgb(255, 252, 240));
        SpawnChild("Document 2", Color.FromArgb(240, 255, 245));

        PerformLayout();
    }

    // ── Menu ─────────────────────────────────────────────────────────────────

    private void BuildMenu()
    {
        _menu = new MenuStrip();

        // File menu
        var fileItem = new ToolStripMenuItem("File");
        var newChild  = new ToolStripMenuItem("New Child Window");
        var separator = new ToolStripMenuItem("-");
        var closeAll  = new ToolStripMenuItem("Close All Children");
        var exitItem  = new ToolStripMenuItem("Close MDI Demo");

        newChild.Click  += (s, e) => SpawnChild();
        closeAll.Click  += (s, e) => CloseAllChildren();
        exitItem.Click  += (s, e) => Close();

        fileItem.DropDownItems.Add(newChild);
        fileItem.DropDownItems.Add(new ToolStripSeparator());
        fileItem.DropDownItems.Add(closeAll);
        fileItem.DropDownItems.Add(new ToolStripSeparator());
        fileItem.DropDownItems.Add(exitItem);

        // Window menu
        var windowItem = new ToolStripMenuItem("Window");
        var cascade    = new ToolStripMenuItem("Cascade");
        var tileH      = new ToolStripMenuItem("Tile Horizontal");
        var tileV      = new ToolStripMenuItem("Tile Vertical");

        cascade.Click += (s, e) => { LayoutMdi(MdiLayout.Cascade);        UpdateStatus(); };
        tileH.Click   += (s, e) => { LayoutMdi(MdiLayout.TileHorizontal); UpdateStatus(); };
        tileV.Click   += (s, e) => { LayoutMdi(MdiLayout.TileVertical);   UpdateStatus(); };

        windowItem.DropDownItems.Add(cascade);
        windowItem.DropDownItems.Add(tileH);
        windowItem.DropDownItems.Add(tileV);

        _menu.Items.Add(fileItem);
        _menu.Items.Add(windowItem);
        MainMenuStrip = _menu;
        Controls.Add(_menu);
    }

    // ── Status bar ───────────────────────────────────────────────────────────

    private void BuildStatus()
    {
        _statusLabel = new ToolStripStatusLabel("Ready")
        {
            Spring    = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _status = new StatusStrip();
        _status.Items.Add(_statusLabel);
        Controls.Add(_status);
    }

    private void UpdateStatus()
    {
        var count  = MdiChildren.Length;
        var active = ActiveMdiChild?.Text ?? "none";
        _statusLabel.Text = $"{count} child window{(count == 1 ? "" : "s")} open  |  Active: {active}";
    }

    // ── Child factory ────────────────────────────────────────────────────────

    private void SpawnChild(string? title = null, Color? backColor = null)
    {
        _childIndex++;
        var child = new MdiChildDocument(
            title    ?? $"Document {_childIndex}",
            backColor ?? RandomPastel(_childIndex))
        {
            MdiParent = this
        };

        MdiChildActivate += (s, e) => UpdateStatus();
        child.FormClosed  += (s, e) => UpdateStatus();

        UpdateStatus();
    }

    private void CloseAllChildren()
    {
        foreach (var c in MdiChildren.ToArray())
            c.Close();
        UpdateStatus();
    }

    private static Color RandomPastel(int index)
    {
        // Cycle through a handful of pleasant pastel colours
        Color[] palette =
        [
            Color.FromArgb(255, 252, 240),
            Color.FromArgb(240, 255, 245),
            Color.FromArgb(240, 245, 255),
            Color.FromArgb(255, 240, 252),
            Color.FromArgb(245, 255, 240),
            Color.FromArgb(255, 248, 240),
        ];
        return palette[(index - 1) % palette.Length];
    }
}

// ── MDI child document window ─────────────────────────────────────────────────

/// <summary>
/// A simple MDI child window showing a text-editor-style surface
/// with some demo content painted onto its canvas.
/// </summary>
internal class MdiChildDocument : Form
{
    private readonly TextBox _editor;

    public MdiChildDocument(string title, Color back)
    {
        Text      = title;
        Width     = 400;
        Height    = 300;
        BackColor = back;

        // Editable text area filling the client area
        _editor = new TextBox
        {
            Multiline  = true,
            ScrollBars = ScrollBars.Both,
            Dock       = DockStyle.Fill,
            BackColor  = back,
            Text       = $"This is {title}.\r\n\r\nYou can type here.\r\nTry opening more windows and using Window → Cascade / Tile.",
            Font       = new Font("Consolas", 10)
        };
        Controls.Add(_editor);

        PerformLayout();
    }
}
