using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Base class for up-down (spinner) controls
/// </summary>
public abstract class UpDownBase : ContainerControl
{
    protected bool _isUpHovered = false;
    protected bool _isDownHovered = false;
    protected const int ButtonWidth = 16;

    public BorderStyle BorderStyle { get; set; } = BorderStyle.Fixed3D;
    public bool InterceptArrowKeys { get; set; } = true;
    public LeftRightAlignment UpDownAlign { get; set; } = LeftRightAlignment.Right;
    public bool ReadOnly { get; set; } = false;

    protected UpDownBase()
    {
        Width = 100;
        Height = 23;
        BackColor = System.Drawing.Color.White;
        ForeColor = System.Drawing.Color.Black;
        TabStop = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, true);
    }

    public abstract void UpButton();
    public abstract void DownButton();
    protected abstract string GetValueText();

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        var bgColor = Enabled ? BackColor : System.Drawing.Color.FromArgb(240, 240, 240);
        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Border
        var borderColor = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
        using var borderPen = new Pen(borderColor);
        g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        // Value text
        var textColor = Enabled ? ForeColor : System.Drawing.Color.FromArgb(109, 109, 109);
        using var textBrush = new SolidBrush(textColor);
        var text = GetValueText();
        int textX = UpDownAlign == LeftRightAlignment.Right ? 4 : ButtonWidth + 4;
        g.DrawString(text, "Arial", 12, textBrush, textX, (Height - 14) / 2);

        // Spinner buttons (right side for Right alignment, left for Left)
        int btnX = UpDownAlign == LeftRightAlignment.Right ? Width - ButtonWidth : 0;

        // Divider line
        using var dividerPen = new Pen(Color.FromArgb(188, 188, 188));
        g.DrawLine(dividerPen, btnX, 0, btnX, Height);

        int halfH = Height / 2;

        // Up button
        var upBg = _isUpHovered && Enabled ? System.Drawing.Color.FromArgb(229, 241, 251) : bgColor;
        using var upBrush = new SolidBrush(upBg);
        g.FillRectangle(upBrush, btnX, 0, ButtonWidth, halfH);

        // Down button
        var downBg = _isDownHovered && Enabled ? System.Drawing.Color.FromArgb(229, 241, 251) : bgColor;
        using var downBrush = new SolidBrush(downBg);
        g.FillRectangle(downBrush, btnX, halfH, ButtonWidth, Height - halfH);

        // Separator between up/down
        g.DrawLine(dividerPen, btnX, halfH, btnX + ButtonWidth, halfH);

        // Arrows
        var arrowColor = Enabled ? Color.FromArgb(100, 100, 100) : Color.FromArgb(170, 170, 170);
        using var arrowPen = new Pen(arrowColor);
        int cx = btnX + ButtonWidth / 2;
        // Up arrow ▲
        int uy = halfH / 2;
        g.DrawLine(arrowPen, cx - 3, uy + 2, cx, uy - 2);
        g.DrawLine(arrowPen, cx, uy - 2, cx + 3, uy + 2);
        // Down arrow ▼
        int dy = halfH + halfH / 2;
        g.DrawLine(arrowPen, cx - 3, dy - 2, cx, dy + 2);
        g.DrawLine(arrowPen, cx, dy + 2, cx + 3, dy - 2);

        // Focus rectangle on the text portion
        if (Focused && Enabled)
        {
            int textAreaW = Width - ButtonWidth - 5;
            DrawFocusRect(g, new Rectangle(2, 2, textAreaW, Height - 5));
        }

        base.OnPaint(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        int btnX = UpDownAlign == LeftRightAlignment.Right ? Width - ButtonWidth : 0;
        bool overButtons = e.X >= btnX && e.X <= btnX + ButtonWidth;
        bool newUpHover = overButtons && e.Y < Height / 2;
        bool newDownHover = overButtons && e.Y >= Height / 2;
        if (newUpHover != _isUpHovered || newDownHover != _isDownHovered)
        {
            _isUpHovered = newUpHover;
            _isDownHovered = newDownHover;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_isUpHovered || _isDownHovered)
        {
            _isUpHovered = false;
            _isDownHovered = false;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled || e.Button != MouseButtons.Left) { base.OnMouseDown(e); return; }
        Focus();
        int btnX = UpDownAlign == LeftRightAlignment.Right ? Width - ButtonWidth : 0;
        if (e.X >= btnX && e.X <= btnX + ButtonWidth)
        {
            if (e.Y < Height / 2) UpButton();
            else DownButton();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (InterceptArrowKeys && Enabled)
        {
            if (e.KeyCode == Keys.Up) { UpButton(); e.Handled = true; return; }
            if (e.KeyCode == Keys.Down) { DownButton(); e.Handled = true; return; }
        }
        base.OnKeyDown(e);
    }
}
