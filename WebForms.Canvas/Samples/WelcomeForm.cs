using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Samples;

public class WelcomeForm : Form
{
    // References to other forms (lazy-initialized)
    private DockingDemoForm? _dockingDemoForm;
    private ControlsDemoForm? _controlsDemoForm;
    private InteractiveForm? _interactiveForm;
    private SampleDrawingForm? _sampleDrawingForm;

    // Reference to parent form list (to add new forms)
    private List<Form>? _parentFormList;
    private Action? _onFormsChanged;

    // UI Controls
    private Label? _titleLabel;
    private Label? _infoLabel;
    private Button? _btnDockingDemo;
    private Button? _btnControlsDemo;
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

    /// <summary>
    /// Set reference to parent form list for adding new forms
    /// </summary>
    public void SetParentFormList(List<Form> parentFormList)
    {
        _parentFormList = parentFormList;
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
        _btnDockingDemo.Click += BtnDockingDemo_Click;
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
        _btnControlsDemo.Click += BtnControlsDemo_Click;
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
        _btnInteractive.Click += BtnInteractive_Click;
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
        _btnDrawingSample.Click += BtnDrawingSample_Click;
        Controls.Add(_btnDrawingSample);

        // Re-enable Paint event for features list at bottom
        Paint += OnFormPaint;
    }

    private void BtnDockingDemo_Click(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Docking Demo button clicked!");

        // Lazy initialization - create form on first click
        if (_dockingDemoForm == null)
        {
            System.Diagnostics.Debug.WriteLine("Creating new DockingDemoForm");
            _dockingDemoForm = new DockingDemoForm
            {
                Left = 50,
                Top = 50,
                Width = 600,
                Height = 500,
                Visible = false, // Start hidden
                OnContainerChanged = this.OnContainerChanged // Pass through the callback
            };
            _parentFormList?.Add(_dockingDemoForm);
            System.Diagnostics.Debug.WriteLine($"Added to list. Total forms: {_parentFormList?.Count}");
        }

        if (!_dockingDemoForm.Visible)
        {
            System.Diagnostics.Debug.WriteLine("Showing DockingDemoForm");
            _dockingDemoForm.Show();
            _dockingDemoForm.BringToFront();

            // Notify parent that forms changed
            System.Diagnostics.Debug.WriteLine("Notifying parent of change");
            _onFormsChanged?.Invoke();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Bringing DockingDemoForm to front");
            _dockingDemoForm.BringToFront();
        }
    }

    private void BtnControlsDemo_Click(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Controls Demo button clicked!");

        // Lazy initialization
        if (_controlsDemoForm == null)
        {
            System.Diagnostics.Debug.WriteLine("Creating new ControlsDemoForm");
            _controlsDemoForm = new ControlsDemoForm
            {
                Left = 700,
                Top = 50,
                Width = 400,
                Height = 550,
                Visible = false // Start hidden
            };
            _parentFormList?.Add(_controlsDemoForm);
        }

        if (!_controlsDemoForm.Visible)
        {
            System.Diagnostics.Debug.WriteLine("Showing ControlsDemoForm");
            _controlsDemoForm.Show();
            _controlsDemoForm.BringToFront();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Bringing ControlsDemoForm to front");
            _controlsDemoForm.BringToFront();
        }
    }

    private void BtnInteractive_Click(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Interactive button clicked!");

        // Lazy initialization
        if (_interactiveForm == null)
        {
            System.Diagnostics.Debug.WriteLine("Creating new InteractiveForm");
            _interactiveForm = new InteractiveForm
            {
                Left = 50,
                Top = 580,
                Width = 500,
                Height = 400,
                Visible = false // Start hidden
            };
            _parentFormList?.Add(_interactiveForm);
        }

        if (!_interactiveForm.Visible)
        {
            System.Diagnostics.Debug.WriteLine("Showing InteractiveForm");
            _interactiveForm.Show();
            _interactiveForm.BringToFront();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Bringing InteractiveForm to front");
            _interactiveForm.BringToFront();
        }
    }

    private void BtnDrawingSample_Click(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Drawing Sample button clicked!");

        // Lazy initialization
        if (_sampleDrawingForm == null)
        {
            System.Diagnostics.Debug.WriteLine("Creating new SampleDrawingForm");
            _sampleDrawingForm = new SampleDrawingForm
            {
                Left = 600,
                Top = 580,
                Width = 600,
                Height = 400,
                Visible = false // Start hidden
            };
            _parentFormList?.Add(_sampleDrawingForm);
        }

        if (!_sampleDrawingForm.Visible)
        {
            System.Diagnostics.Debug.WriteLine("Showing SampleDrawingForm");
            _sampleDrawingForm.Show();
            _sampleDrawingForm.BringToFront();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Bringing SampleDrawingForm to front");
            _sampleDrawingForm.BringToFront();
        }
    }

    private void OnFormPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        int y = 320;
        int lineHeight = 16;

        // Features header
        using var headerBrush = new SolidBrush(Color.FromArgb(60, 60, 60));
        g.DrawString("✅ Features Implemented:", "Arial", 12, headerBrush, 20, y);
        y += 20;

        // Features list (condensed)
        using var featureBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
        string[] features = new[]
        {
            "Drawing: Primitives, Fills, Pens, Brushes, Colors, Text",
            "Events: Mouse (click, dblclick, move), Keyboard, Touch",
            "Forms: Dragging, Resizing, Min/Max/Close, Taskbar",
            "Layout: Docking (Top/Bottom/Left/Right/Fill), Anchoring",
            "Controls: Button, Label, TextBox, CheckBox, RadioButton, PictureBox",
            "Window Manager: Multiple windows, Age-based ordering"
        };

        foreach (var feature in features)
        {
            if (y + lineHeight > Height - 20) break;
            g.DrawString($"• {feature}", "Arial", 9, featureBrush, 30, y);
            y += lineHeight;
        }

        // Instructions at bottom
        if (y + 40 < Height - 10)
        {
            y = Height - 50;
            using var tipBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
            g.DrawString("💡 Tip: Click buttons above to explore. Resize this form to see anchoring!", 
                "Arial", 9, tipBrush, 20, y);
        }
    }
}
