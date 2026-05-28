using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class WelcomeForm : Form
{
    public WelcomeForm()
    {
        Text = "Welcome - Windows Forms Canvas Clone";
        Width = 700;
        Height = 660;
        BackColor = Color.White;
        AllowResize = true;
        AllowMove = true;
        MinimumWidth = 500;
        MinimumHeight = 400;

        InitializeControls();
        PerformLayout();
    }

    private static void Launch<T>() where T : Form, new()
        => CanvasApplication.FormManager?.ShowOrCreateForm<T>();

    private void InitializeControls()
    {
        const int lx  = 40;          // left x of button grid
        const int col2 = 250;         // col 2 x
        const int col3 = 460;         // col 3 x
        const int bw  = 200;          // button width
        const int bh  = 50;           // button height
        const int rh  = 60;           // row height (button + gap)

        // ── Header ────────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Windows Forms Canvas Clone",
            Left      = 10, Top = 10, Width = 680, Height = 35,
            ForeColor = Color.FromArgb(26, 115, 232),
            BackColor = Color.FromArgb(240, 248, 255),
            TextAlign = ContentAlignment.TopCenter
        });

        Controls.Add(new Label
        {
            Text      = "HTML canvas-based Windows Forms implementation with full window management.",
            Left      = 20, Top = 52, Width = 660, Height = 22,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.FromArgb(255, 255, 224)
        });

        Controls.Add(new Label
        {
            Text      = "Features: Docking & Anchoring, Taskbar, Min/Max/Close, Drag & Resize",
            Left      = 20, Top = 81, Width = 660, Height = 22,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.FromArgb(255, 255, 224)
        });

        Controls.Add(new Label
        {
            Text      = "Click to open demo forms:",
            Left      = 20, Top = 112, Width = 660, Height = 20,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.White
        });

        // ── Project Status — full-width, bold ────────────────────────────
        var btnStatus = new Button
        {
            Text   = "📊  Project Status",
            Left   = lx, Top = 138, Width = 620, Height = 44,
            Font   = new Font("Arial", 12),
            ForeColor = Color.FromArgb(0, 51, 153),
            BackColor = Color.FromArgb(232, 240, 255)
        };
        btnStatus.Click += (s, e) => Launch<ProjectStatusForm>();
        Controls.Add(btnStatus);

        // ── Demo button grid (3 columns, 6 rows) ─────────────────────────
        int y = 192;   // first row top

        Button Btn(string text, int x, int top)
        {
            var b = new Button { Text = text, Left = x, Top = top, Width = bw, Height = bh };
            Controls.Add(b);
            return b;
        }

        // Row 1
        Btn("Input Controls",      lx,   y).Click += (s, e) => Launch<DemoInputControlsForm>();
        Btn("Selection Controls",  col2, y).Click += (s, e) => Launch<DemoSelectionControlsForm>();
        Btn("TreeView & ListView", col3, y).Click += (s, e) => Launch<DemoCollectionControlsForm>();
        y += rh;

        // Row 2
        Btn("Docking & Anchoring", lx,   y).Click += (s, e) => Launch<DockingDemoForm>();
        Btn("FlowLayoutPanel",     col2, y).Click += (s, e) => Launch<FlowLayoutDemoForm>();
        Btn("TableLayoutPanel",    col3, y).Click += (s, e) => Launch<TableLayoutDemoForm>();
        y += rh;

        // Row 3
        Btn("SplitContainer",      lx,   y).Click += (s, e) => Launch<SplitContainerDemoForm>();
        Btn("Interactive Form",    col2, y).Click += (s, e) => Launch<InteractiveForm>();
        Btn("Drawing Sample",      col3, y).Click += (s, e) => Launch<SampleDrawingForm>();
        y += rh;

        // Row 4
        Btn("TabControl",          lx,   y).Click += (s, e) => Launch<TabControlDemoForm>();
        Btn("Dialog Demos",        col2, y).Click += (s, e) => Launch<DialogDemoForm>();
        Btn("Menus & ToolStrip",   col3, y).Click += (s, e) => Launch<MenuDemoForm>();
        y += rh;

        // Row 5
        Btn("DataGridView",        lx,   y).Click += (s, e) => Launch<DataGridDemoForm>();
        Btn("Server Data (ADO.NET)", col2, y).Click += (s, e) => Launch<ServerDataDemoForm>();
        Btn("Sliders & Spinners",  col3, y).Click += (s, e) => Launch<DemoSliderSpinnerForm>();
        y += rh;

        // Row 6
        Btn("WebBrowser & NotifyIcon", lx,   y).Click += (s, e) => Launch<DemoWebNotifyForm>();
        Btn("MDI Demo",                col2, y).Click += (s, e) => Launch<MdiDemoForm>();
        Btn("Charts Demo",             col3, y).Click += (s, e) => Launch<ChartDemoForm>();
        y += rh;

        // ── Links ────────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Links:",
            Left      = 20, Top = y + 4, Width = 660, Height = 20,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.White
        });
        y += 28;

        Controls.Add(new LinkLabel { Text = "View on GitHub",    LinkUrl = "https://github.com/ga-lucas/CanvasForms",                          Left = 40,  Top = y, Width = 160, Height = 20 });
        Controls.Add(new LinkLabel { Text = "Documentation",     LinkUrl = "https://docs.microsoft.com/en-us/dotnet/desktop/winforms/",        Left = 210, Top = y, Width = 150, Height = 20 });
        Controls.Add(new LinkLabel { Text = "WinForms Examples", LinkUrl = "https://github.com/dotnet/winforms",                               Left = 370, Top = y, Width = 150, Height = 20 });
    }
}


