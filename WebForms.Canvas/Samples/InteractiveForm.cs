using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class InteractiveForm : Form
{
    private readonly List<Point> _clickPoints = new();
    private Point _currentMousePos = new(0, 0);
    private string _lastKey = "";
    private int _mouseDownCount = 0;
    private int _doubleClickCount = 0;

    public InteractiveForm()
    {
        Text = "Interactive Form - Click, Drag, Type!";
        Width = 600;
        Height = 500;
        BackColor = Color.White;
        AllowResize = true;
        AllowMove = true;
        MinimumWidth = 300;
        MinimumHeight = 200;

        // Subscribe to events
        Paint += OnFormPaint;
        MouseClick += OnFormMouseClick;
        MouseMove += OnFormMouseMove;
        MouseDown += OnFormMouseDown;
        MouseDoubleClick += OnFormMouseDoubleClick;
        KeyPress += OnFormKeyPress;
        KeyDown += OnFormKeyDown;
    }

    private void OnFormPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);

        // Draw instructions
        using var instructionBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
        g.DrawString("🖱️ Click anywhere to draw circles", "Arial", 14, instructionBrush, 10, 10);
        g.DrawString("🖱️ Double-click to clear all circles", "Arial", 14, instructionBrush, 10, 30);
        g.DrawString("⌨️ Type keys to see them displayed", "Arial", 14, instructionBrush, 10, 50);
        g.DrawString("↕️ Drag the title bar to move", "Arial", 14, instructionBrush, 10, 70);
        g.DrawString("↔️ Drag corners/edges to resize", "Arial", 14, instructionBrush, 10, 90);

        //// Draw all clicked points as circles
        using var circleBrush = new SolidBrush(Color.FromArgb(100, 74, 144, 226));
        using var circlePen = new Pen(Color.FromArgb(74, 144, 226), 2);
        foreach(var point in _clickPoints)
        {
            g.FillEllipse(circleBrush, point.X - 15, point.Y - 15, 30, 30);
            g.DrawEllipse(circlePen, point.X - 15, point.Y - 15, 30, 30);
        }

        // Draw current mouse position
        using var crosshairPen = new Pen(Color.Red, 1);
        if(_currentMousePos.X > 0 || _currentMousePos.Y > 0)
        {
            g.DrawLine(crosshairPen, _currentMousePos.X - 10, _currentMousePos.Y, _currentMousePos.X + 10, _currentMousePos.Y);
            g.DrawLine(crosshairPen, _currentMousePos.X, _currentMousePos.Y - 10, _currentMousePos.X, _currentMousePos.Y + 10);

            using var posBrush = new SolidBrush(Color.Black);
            g.DrawString($"({_currentMousePos.X}, {_currentMousePos.Y})", "Arial", 10, posBrush, _currentMousePos.X + 15, _currentMousePos.Y - 5);
        }

        // Draw statistics
        using var statsBrush = new SolidBrush(Color.Black);
        var statsY = Height - 80;
        g.DrawString($"Circles drawn: {_clickPoints.Count}", "Arial", 12, statsBrush, 10, statsY);
        g.DrawString($"Mouse clicks: {_mouseDownCount}", "Arial", 12, statsBrush, 10, statsY + 20);
        g.DrawString($"Double clicks: {_doubleClickCount}", "Arial", 12, statsBrush, 10, statsY + 40);
        g.DrawString($"Last key: {_lastKey}", "Arial", 12, statsBrush, 10, statsY + 60);
    }

    private void OnFormMouseClick(object? sender, MouseEventArgs e)
    {
        // Add a new point where the user clicked
        _clickPoints.Add(new Point(e.X, e.Y));
        Invalidate();
    }

    private void OnFormMouseMove(object? sender, MouseEventArgs e)
    {
        // Track current mouse position
        _currentMousePos = new Point(e.X, e.Y);
        Invalidate();
    }

    private void OnFormMouseDown(object? sender, MouseEventArgs e)
    {
        _mouseDownCount++;
        Invalidate();
    }

    private void OnFormMouseDoubleClick(object? sender, MouseEventArgs e)
    {
        // Clear all circles on double-click
        _clickPoints.Clear();
        _doubleClickCount++;
        Invalidate();
    }

    private void OnFormKeyPress(object? sender, KeyPressEventArgs e)
    {
        _lastKey = $"'{e.KeyChar}' (KeyPress)";
        Invalidate();
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        var modifiers = new List<string>();
        if (e.Control) modifiers.Add("Ctrl");
        if (e.Alt) modifiers.Add("Alt");
        if (e.Shift) modifiers.Add("Shift");

        var modifierStr = modifiers.Count > 0 ? string.Join("+", modifiers) + "+" : "";
        _lastKey = $"{modifierStr}{e.KeyCode}";

        // Clear circles on 'C' key
        if (e.KeyCode == Keys.C)
        {
            _clickPoints.Clear();
        }

        Invalidate();
    }
}
