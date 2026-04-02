using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Samples;

public class WelcomeForm : Form
{
    // UI Controls
    private Label? _titleLabel;
    private Label? _infoLabel;
    private Button? _btnDockingDemo;
    private Button? _btnControlsDemo;
    private Button? _btnInteractive;
    private Button? _btnDrawingSample;
    private Button? _btnListControl;

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

        // Docking Demo Button
        _btnDockingDemo = new Button
        {
            Text = "Docking & Anchoring",
            Left = 40,
            Top = 170,
            Width = 280,
            Height = 50
        };
        _btnDockingDemo.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<DockingDemoForm>();
        };
        Controls.Add(_btnDockingDemo);

        // Controls Demo Button
        _btnControlsDemo = new Button
        {
            Text = "Controls Demo",
            Left = 360,
            Top = 170,
            Width = 280,
            Height = 50
        };
        _btnControlsDemo.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<ControlsDemoForm>();
        };
        Controls.Add(_btnControlsDemo);

        // Interactive Form Button
        _btnInteractive = new Button
        {
            Text = "Interactive Form",
            Left = 40,
            Top = 240,
            Width = 280,
            Height = 50
        };
        _btnInteractive.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<InteractiveForm>();
        };
        Controls.Add(_btnInteractive);

        // Drawing Sample Button
        _btnDrawingSample = new Button
        {
            Text = "Drawing Sample",
            Left = 360,
            Top = 240,
            Width = 280,
            Height = 50
        };
        _btnDrawingSample.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<SampleDrawingForm>();
        };
        Controls.Add(_btnDrawingSample);


        // ListControl
        _btnListControl = new Button
        {
            Text = "ListBox Demo",
            Left = 40,
            Top = 310,
            Width = 280,
            Height = 50
        };
        _btnListControl.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<ListBoxDemoForm>();
        };
        Controls.Add(_btnListControl);
    }
}
