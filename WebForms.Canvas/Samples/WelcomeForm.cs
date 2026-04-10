using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class WelcomeForm : Form
{
    // UI Controls
    private Label? _titleLabel;
    private Label? _infoLabel;
    private Button? _btnDockingDemo;
    private Button? _btnInteractive;
    private Button? _btnDrawingSample;

    public WelcomeForm()
    {
        Text = "Welcome - Windows Forms Canvas Clone";
        Width = 700;
        Height = 650;
        BackColor = Color.White;
        AllowResize = true;
        AllowMove = true;
        MinimumWidth = 500;
        MinimumHeight = 400;

        InitializeControls();

        // Force layout after initialization
        PerformLayout();
    }

    private void InitializeControls()
    {
        // Title Label
        _titleLabel = new Label
        {
            Text = "Windows Forms Canvas Clone",
            Left = 10,
            Top = 10,
            Width = 680,
            Height = 35,
            ForeColor = Color.FromArgb(26, 115, 232),
            BackColor = Color.FromArgb(240, 248, 255),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(_titleLabel);

        // Info Label (no newlines, simpler text)
        _infoLabel = new Label
        {
            Text = "HTML canvas-based Windows Forms implementation with full window management.",
            Left = 20,
            Top = 55,
            Width = 660,
            Height = 25,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.FromArgb(255, 255, 224),
            TextAlign = ContentAlignment.TopLeft
        };
        Controls.Add(_infoLabel);

        // Features label
        var featuresLabel = new Label
        {
            Text = "Features: Docking & Anchoring, Taskbar, Min/Max/Close, Drag & Resize",
            Left = 20,
            Top = 90,
            Width = 660,
            Height = 25,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.FromArgb(255, 255, 224)
        };
        Controls.Add(featuresLabel);

        // Demo Buttons Section Label
        var demosLabel = new Label
        {
            Text = "Click to open demo forms:",
            Left = 20,
            Top = 130,
            Width = 660,
            Height = 25,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.White
        };
        Controls.Add(demosLabel);

        // Row 1
        var btnInputControls = new Button
        {
            Text = "Input Controls",
            Left = 40,
            Top = 170,
            Width = 200,
            Height = 50
        };
        btnInputControls.Click += (s, e) =>
        {
           Canvas.Windows.Forms.CanvasApplication.FormManager?.ShowOrCreateForm<DemoInputControlsForm>();
        };
        Controls.Add(btnInputControls);

        var btnSelectionControls = new Button
        {
            Text = "Selection Controls",
            Left = 250,
            Top = 170,
            Width = 200,
            Height = 50
        };
        btnSelectionControls.Click += (s, e) =>
        {
          Canvas.Windows.Forms.CanvasApplication.FormManager?.ShowOrCreateForm<DemoSelectionControlsForm>();
        };
        Controls.Add(btnSelectionControls);

        var btnCollectionControls = new Button
        {
            Text = "TreeView & ListView",
            Left = 460,
            Top = 170,
            Width = 200,
            Height = 50
        };
        btnCollectionControls.Click += (s, e) =>
        {
           Canvas.Windows.Forms.CanvasApplication.FormManager?.ShowOrCreateForm<DemoCollectionControlsForm>();
        };
        Controls.Add(btnCollectionControls);

        // Row 2
        _btnDockingDemo = new Button
        {
            Text = "Docking & Anchoring",
            Left = 40,
            Top = 240,
            Width = 200,
            Height = 50
        };
        _btnDockingDemo.Click += (s, e) =>
        {
           Canvas.Windows.Forms.CanvasApplication.FormManager?.ShowOrCreateForm<DockingDemoForm>();
        };
        Controls.Add(_btnDockingDemo);

        _btnInteractive = new Button
        {
            Text = "Interactive Form",
            Left = 250,
            Top = 240,
            Width = 200,
            Height = 50
        };
        _btnInteractive.Click += (s, e) =>
        {
           Canvas.Windows.Forms.CanvasApplication.FormManager?.ShowOrCreateForm<InteractiveForm>();
        };
        Controls.Add(_btnInteractive);

        _btnDrawingSample = new Button
        {
            Text = "Drawing Sample",
            Left = 460,
            Top = 240,
            Width = 200,
            Height = 50
        };
        _btnDrawingSample.Click += (s, e) =>
        {
         Canvas.Windows.Forms.CanvasApplication.FormManager?.ShowOrCreateForm<SampleDrawingForm>();
        };
        Controls.Add(_btnDrawingSample);

        // Links section
        var linksLabel = new Label
        {
            Text = "Links:",
            Left = 20,
            Top = 310,
            Width = 660,
            Height = 25,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.White
        };
        Controls.Add(linksLabel);

        // GitHub link
        var githubLink = new LinkLabel
        {
            Text = "View on GitHub",
            LinkUrl = "https://github.com/ga-lucas/CanvasForms",
            Left = 40,
            Top = 340,
            Width = 150,
            Height = 20
        };
        githubLink.LinkClicked += (s, e) =>
        {
            // Optional: show a message when link is clicked
            // The URL will be opened automatically
        };
        Controls.Add(githubLink);

        // Documentation link
        var docsLink = new LinkLabel
        {
            Text = "Documentation",
            LinkUrl = "https://docs.microsoft.com/en-us/dotnet/desktop/winforms/",
            Left = 200,
            Top = 340,
            Width = 150,
            Height = 20
        };
        Controls.Add(docsLink);

        // Example link
        var exampleLink = new LinkLabel
        {
            Text = "WinForms Examples",
            LinkUrl = "https://github.com/dotnet/winforms",
            Left = 360,
            Top = 340,
            Width = 150,
            Height = 20
        };
        Controls.Add(exampleLink);
    }
}
