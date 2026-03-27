using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class Form : Control
{
    private static int _nextZIndex = 1;
    private const int TitleBarHeight = 32; // Height of the title bar
    private Control? _focusedControl;

    public bool AllowResize { get; set; } = true;
    public bool AllowMove { get; set; } = true;
    public int MinimumWidth { get; set; } = 100;
    public int MinimumHeight { get; set; } = 50;
    public int MaximumWidth { get; set; } = 0; // 0 = no limit
    public int MaximumHeight { get; set; } = 0; // 0 = no limit

    // Z-order for stacking
    public int ZIndex { get; set; } = 0;

    // Focused control for keyboard input
    public Control? FocusedControl
    {
        get => _focusedControl;
        set
        {
            if (_focusedControl != value)
            {
                _focusedControl = value;
                Invalidate();
            }
        }
    }

    // Client area dimensions (excluding title bar)
    public int ClientWidth => Width;
    public int ClientHeight => Math.Max(0, Height - TitleBarHeight);

    public Form()
    {
        Text = "Form";
        Width = 800;
        Height = 600;
        Left = 50;
        Top = 50;
        BackColor = Color.FromArgb(240, 240, 240);
        ZIndex = _nextZIndex++;
    }

    public void BringToFront()
    {
        ZIndex = _nextZIndex++;
        Invalidate();
    }

    public Graphics CreateGraphics()
    {
        return new Graphics(ClientWidth, ClientHeight);
    }

    public void Show()
    {
        Visible = true;
        Invalidate();
    }

    public void Close()
    {
        Visible = false;
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        // Let user code handle Paint event first
        base.OnPaint(e);

        // Then render child controls
        foreach (var control in Controls)
        {
            if (!control.Visible) continue;

            // Save current graphics state
            var g = e.Graphics;

            // Translate graphics to control position
            g.TranslateTransform(control.Left, control.Top);

            // Create paint args for the control
            var controlPaintArgs = new PaintEventArgs(
                g,
                new Rectangle(0, 0, control.Width, control.Height)
            );

            // Let control paint itself
            control.OnPaint(controlPaintArgs);

            // Restore graphics state
            g.TranslateTransform(-control.Left, -control.Top);
        }
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        // Check if any child control was clicked
        foreach (var control in Controls)
        {
            if (!control.Visible || !control.Enabled) continue;

            if (HitTest(control, e.X, e.Y))
            {
                // Set focus to this control
                FocusedControl = control;

                var controlArgs = new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - control.Left,
                    e.Y - control.Top
                );
                control.OnMouseDown(controlArgs);
                return; // Don't propagate to form
            }
        }

        // Clicked on form background - clear focus
        FocusedControl = null;

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        // Check if any child control should receive mouse up
        foreach (var control in Controls)
        {
            if (!control.Visible || !control.Enabled) continue;

            if (HitTest(control, e.X, e.Y))
            {
                var controlArgs = new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - control.Left,
                    e.Y - control.Top
                );
                control.OnMouseUp(controlArgs);
                return;
            }
        }

        base.OnMouseUp(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        // Check if any child control should receive mouse move
        foreach (var control in Controls)
        {
            if (!control.Visible) continue;

            if (HitTest(control, e.X, e.Y))
            {
                var controlArgs = new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - control.Left,
                    e.Y - control.Top
                );
                control.OnMouseMove(controlArgs);
                return;
            }
        }

        base.OnMouseMove(e);
    }

    private bool HitTest(Control control, int x, int y)
    {
        return x >= control.Left && x < control.Left + control.Width &&
               y >= control.Top && y < control.Top + control.Height;
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        // Route keyboard events to focused control
        if (FocusedControl != null && FocusedControl.Enabled)
        {
            FocusedControl.OnKeyDown(e);
        }
        else
        {
            base.OnKeyDown(e);
        }
    }

    protected internal override void OnKeyUp(KeyEventArgs e)
    {
        // Route keyboard events to focused control
        if (FocusedControl != null && FocusedControl.Enabled)
        {
            FocusedControl.OnKeyUp(e);
        }
        else
        {
            base.OnKeyUp(e);
        }
    }

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        // Route keyboard events to focused control
        if (FocusedControl != null && FocusedControl.Enabled)
        {
            FocusedControl.OnKeyPress(e);
        }
        else
        {
            base.OnKeyPress(e);
        }
    }
}
