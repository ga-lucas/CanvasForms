using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Samples;

public class WelcomeForm : Form
{
    public WelcomeForm()
    {
        Text = "Welcome - Windows Forms Canvas Clone";
        Width = 700;
        Height = 600;
        BackColor = Color.White;
        AllowResize = true;
        AllowMove = true;
        MinimumWidth = 500;
        MinimumHeight = 400;

        Paint += OnFormPaint;
    }

    private void OnFormPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);

        int y = 10;
        int lineHeight = 20;

        // Title
        using var titleBrush = new SolidBrush(Color.FromArgb(26, 115, 232));
        g.DrawString("Windows Forms Canvas Clone", "Arial", 18, titleBrush, 10, y);
        y += 30;

        // Description
        using var textBrush = new SolidBrush(Color.Black);
        g.DrawString("HTML canvas-based implementation of Windows Forms with window management.", "Arial", 12, textBrush, 10, y);
        y += 30;

        // Info box
        using var infoBg = new SolidBrush(Color.FromArgb(227, 242, 253));
        using var infoBorder = new Pen(Color.FromArgb(33, 150, 243), 1);
        g.FillRectangle(infoBg, 10, y, Width - 40, 40);
        g.DrawRectangle(infoBorder, 10, y, Width - 40, 40);

        using var infoBrush = new SolidBrush(Color.FromArgb(13, 71, 161));
        g.DrawString("💡 Features: Taskbar switching • Min/Max/Close buttons • Drag to move • Resize", "Arial", 11, infoBrush, 20, y + 12);
        y += 55;

        // Features header
        using var headerBrush = new SolidBrush(Color.FromArgb(60, 60, 60));
        g.DrawString("✅ Features Implemented:", "Arial", 14, headerBrush, 10, y);
        y += 25;

        // Features list
        using var featureBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
        string[] features = new[]
        {
            "✅ Basic drawing primitives (lines, rectangles, ellipses)",
            "✅ Filled shapes support",
            "✅ Pen and Brush abstractions",
            "✅ Color management (RGBA support)",
            "✅ Text rendering",
            "✅ Graphics command buffering",
            "✅ Paint event system",
            "✅ Mouse events (click, double-click, move, down, up, enter, leave)",
            "✅ Keyboard events (keydown, keyup, keypress)",
            "✅ Touch events support",
            "✅ Form dragging (drag title bar to move)",
            "✅ Form resizing (drag edges/corners)",
            "✅ Control hierarchy (parent/child controls)",
            "✅ Button control with hover/click states",
            "✅ Label control with text alignment",
            "✅ TextBox control with keyboard input & scrolling",
            "✅ CheckBox control",
            "✅ RadioButton control with auto-grouping",
            "✅ PictureBox control with image loading",
            "✅ 🆕 Window Manager with Taskbar",
            "✅ 🆕 Window min/max/restore buttons",
            "✅ 🆕 Taskbar switching (ordered by age)",
            "✅ 🆕 Full viewport desktop (no scrolling)"
        };

        foreach (var feature in features)
        {
            if (y + lineHeight > Height - 60) break; // Don't overflow
            g.DrawString(feature, "Arial", 10, featureBrush, 20, y);
            y += lineHeight;
        }

        // Try it out section (if space available)
        if (y + 80 < Height - 20)
        {
            y += 10;
            g.DrawString("🎮 Try it out:", "Arial", 14, headerBrush, 10, y);
            y += 20;

            string[] tips = new[]
            {
                "• Click minimize (—), maximize (□), or close (×) buttons",
                "• Click taskbar buttons to switch/restore windows",
                "• Drag title bars to move windows around",
                "• Drag edges/corners to resize windows"
            };

            foreach (var tip in tips)
            {
                if (y + lineHeight > Height - 20) break;
                g.DrawString(tip, "Arial", 10, featureBrush, 20, y);
                y += lineHeight;
            }
        }
    }
}
