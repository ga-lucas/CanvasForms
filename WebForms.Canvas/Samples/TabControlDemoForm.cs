using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class TabControlDemoForm : Form
{
    public TabControlDemoForm()
    {
        Text = "TabControl Demo";
        Width = 700;
        Height = 520;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
        PerformLayout();
    }

    private void InitializeControls()
    {
        var title = new Label
        {
            Text = "TabControl - basic WinForms-like behavior (click tabs / use Left/Right) ",
            Left = 20,
            Top = 20,
            Width = 640,
            Height = 25,
            ForeColor = Color.FromArgb(0, 51, 153),
            BackColor = Color.Transparent
        };
        Controls.Add(title);

        var tabs = new TabControl
        {
            Left = 20,
            Top = 55,
            Width = 640,
            Height = 380,
            HotTrack = true,
        };

        var ownerDrawToggle = new CheckBox
        {
            Text = "OwnerDrawFixed",
            Left = 20,
            Top = 445,
            Width = 140,
            Height = 20,
            Checked = false,
            TabStop = true,
            TabIndex = 10
        };

        var page1 = new TabPage("General");
        var page2 = new TabPage("Details");
        var page3 = new TabPage("About");

        page1.Controls.Add(new Label { Text = "Name:", Left = 20, Top = 20, Width = 60, Height = 20 });
        page1.Controls.Add(new TextBox { Left = 90, Top = 18, Width = 200, Height = 22, TabIndex = 0, TabStop = true });
        page1.Controls.Add(new CheckBox { Text = "Enabled", Left = 20, Top = 55, Width = 120, Height = 20, TabIndex = 1, TabStop = true });

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        list.Columns.Add(new ColumnHeader { Text = "Key", Width = 160 });
        list.Columns.Add(new ColumnHeader { Text = "Value", Width = 260 });
        list.Items.Add(new ListViewItem(new[] { "OS", "Browser" }));
        list.Items.Add(new ListViewItem(new[] { "Renderer", "Canvas" }));
        page2.Controls.Add(list);

        page3.Controls.Add(new Label
        {
            Text = "CanvasForms TabControl demo",
            Left = 20,
            Top = 20,
            Width = 400,
            Height = 20
        });

        tabs.TabPages.AddRange(new[] { page1, page2, page3 });

        tabs.DrawItem += (_, e) =>
        {
            // Minimal owner-draw demo: render tab header background + text.
            // (TabControl still draws its own border/selection visuals.)
            e.BackColor = (e.State & DrawItemState.Selected) != 0
                ? Color.FromArgb(255, 255, 255)
                : Color.FromArgb(240, 240, 240);

            e.ForeColor = (e.State & DrawItemState.Selected) != 0
                ? Color.FromArgb(0, 0, 0)
                : Color.FromArgb(60, 60, 60);

            e.DrawBackground();

            var tabText = tabs.TabPages[e.Index].Text ?? string.Empty;
            using var tb = new SolidBrush(e.ForeColor);
            e.Graphics.DrawString(tabText, e.Font, tb, e.Bounds.X + 8, e.Bounds.Y + 5);
        };

        ownerDrawToggle.CheckedChanged += (_, __) =>
        {
            tabs.DrawMode = ownerDrawToggle.Checked ? TabDrawMode.OwnerDrawFixed : TabDrawMode.Normal;
            tabs.Invalidate();
        };

        tabs.SelectedIndexChanged += (_, __) =>
        {
            // placeholder hook
        };

        Controls.Add(tabs);

        Controls.Add(ownerDrawToggle);

        var hint = new Label
        {
            Text = "Tip: use mouse to select tabs; Left/Right arrow changes selection when focused.",
            Left = 180,
            Top = 445,
            Width = 480,
            Height = 25,
            ForeColor = Color.Gray,
            BackColor = Color.Transparent
        };
        Controls.Add(hint);
    }
}
