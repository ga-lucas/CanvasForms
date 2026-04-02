using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Samples;

/// <summary>
/// Refactored WelcomeForm using the Application/FormManager pattern
/// No more manual form tracking - just use Application methods!
/// </summary>
public class WelcomeFormRefactored : Form
{
    public WelcomeFormRefactored()
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
        PerformLayout();
    }

    private void InitializeControls()
    {
        // Title Label
        var titleLabel = new Label
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
        Controls.Add(titleLabel);

        // Info Label
        var infoLabel = new Label
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
        Controls.Add(infoLabel);

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
            ForeColor = Color.FromArgb(40, 40, 40),
            Font = new Font("Arial", 11)
        };
        Controls.Add(demosLabel);

        // ========== REFACTORED: No more manual form management! ==========

        // Button 1: Docking Demo - using ShowOrCreateForm<T>()
        var btnDockingDemo = new Button
        {
            Text = "Docking & Anchoring Demo",
            Left = 30,
            Top = 165,
            Width = 200,
            Height = 35,
            BackColor = Color.FromArgb(230, 240, 255)
        };
        btnDockingDemo.Click += (s, e) =>
        {
            // Simple! Just one line to show or create the form
            Application.FormManager?.ShowOrCreateForm<DockingDemoForm>();
        };
        Controls.Add(btnDockingDemo);

        // Button 2: Controls Demo
        var btnControlsDemo = new Button
        {
            Text = "Controls Demo",
            Left = 30,
            Top = 210,
            Width = 200,
            Height = 35,
            BackColor = Color.FromArgb(230, 255, 240)
        };
        btnControlsDemo.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<ControlsDemoForm>();
        };
        Controls.Add(btnControlsDemo);

        // Button 3: Interactive Form
        var btnInteractive = new Button
        {
            Text = "Interactive Form",
            Left = 30,
            Top = 255,
            Width = 200,
            Height = 35,
            BackColor = Color.FromArgb(255, 240, 230)
        };
        btnInteractive.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<InteractiveForm>();
        };
        Controls.Add(btnInteractive);

        // Button 4: Drawing Sample
        var btnDrawingSample = new Button
        {
            Text = "Drawing Sample",
            Left = 250,
            Top = 165,
            Width = 200,
            Height = 35,
            BackColor = Color.FromArgb(255, 230, 255)
        };
        btnDrawingSample.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<SampleDrawingForm>();
        };
        Controls.Add(btnDrawingSample);

        // Button 5: ListBox Demo (NEW!)
        var btnListBoxDemo = new Button
        {
            Text = "ListBox Demo",
            Left = 250,
            Top = 210,
            Width = 200,
            Height = 35,
            BackColor = Color.FromArgb(240, 255, 240)
        };
        btnListBoxDemo.Click += (s, e) =>
        {
            Application.FormManager?.ShowOrCreateForm<ListBoxDemoForm>();
        };
        Controls.Add(btnListBoxDemo);

        // Application Exit Button
        var btnExit = new Button
        {
            Text = "Exit Application",
            Left = 470,
            Top = 165,
            Width = 200,
            Height = 35,
            BackColor = Color.FromArgb(255, 220, 220)
        };
        btnExit.Click += (s, e) =>
        {
            // Close all forms and exit
            Application.Exit();
        };
        Controls.Add(btnExit);

        // Subscribe to Paint for custom rendering
        Paint += OnFormPaint;
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

        // Features list
        using var featureBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
        string[] features = new[]
        {
            "Drawing: Primitives, Fills, Pens, Brushes, Colors, Text",
            "Events: Mouse (click, dblclick, move), Keyboard, Touch",
            "Forms: Dragging, Resizing, Min/Max/Close, Taskbar",
            "Layout: Docking (Top/Bottom/Left/Right/Fill), Anchoring",
            "Controls: Button, Label, TextBox, CheckBox, RadioButton, ListBox",
            "Window Manager: Multiple windows, Application lifecycle",
            "✨ NEW: Application.FormManager for centralized form control!"
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
            g.DrawString("💡 Tip: No more manual form tracking! Use Application.FormManager.ShowOrCreateForm<T>()", 
                "Arial", 9, tipBrush, 20, y);
        }
    }
}
