using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a label that displays a hyperlink
/// </summary>
public class LinkLabel : Label
{
    private bool _isHovered = false;
    private bool _isVisited = false;

    public LinkLabel()
    {
        ForeColor = Color.FromArgb(0, 0, 255);
        Cursor = Cursor.Hand;
        TabStop = true;
    }

    public Color LinkColor { get; set; } = Color.FromArgb(0, 0, 255);
    public Color VisitedLinkColor { get; set; } = Color.FromArgb(128, 0, 128);
    public Color ActiveLinkColor { get; set; } = Color.FromArgb(255, 0, 0);
    public Color DisabledLinkColor { get; set; } = Color.FromArgb(133, 133, 133);
    public LinkBehavior LinkBehavior { get; set; } = LinkBehavior.SystemDefault;
    public bool LinkVisited
    {
        get => _isVisited;
        set { _isVisited = value; Invalidate(); }
    }

    public event LinkLabelLinkClickedEventHandler? LinkClicked;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        DrawControlBackground(g);

        if (!string.IsNullOrEmpty(Text))
        {
            Color linkColor;
            if (!Enabled)
                linkColor = DisabledLinkColor;
            else if (_isHovered)
                linkColor = ActiveLinkColor;
            else if (_isVisited)
                linkColor = VisitedLinkColor;
            else
                linkColor = LinkColor;

            var (x, y) = GetTextPosition();
            using var brush = new SolidBrush(linkColor);
            g.DrawString(Text, "Arial", 12, brush, x, y);

            bool showUnderline = LinkBehavior == LinkBehavior.AlwaysUnderline
                || (LinkBehavior == LinkBehavior.SystemDefault || LinkBehavior == LinkBehavior.HoverUnderline && _isHovered);
            if (showUnderline)
            {
                var textWidth = Text.Length * 7;
                using var underlinePen = new Pen(linkColor, 1);
                g.DrawLine(underlinePen, x, y + 14, x + textWidth, y + 14);
            }
        }

        DrawFocusRect(g, new Rectangle(0, 0, Width - 1, Height - 1));
    }

    protected internal override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
            Focus();
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
        {
            _isVisited = true;
            LinkClicked?.Invoke(this, new LinkLabelLinkClickedEventArgs(e.Button));
            Invalidate();
        }
        base.OnMouseUp(e);
    }

    protected internal override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); }
    protected internal override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); }
}

public enum LinkBehavior { SystemDefault, AlwaysUnderline, HoverUnderline, NeverUnderline }

public delegate void LinkLabelLinkClickedEventHandler(object? sender, LinkLabelLinkClickedEventArgs e);

public class LinkLabelLinkClickedEventArgs : EventArgs
{
    public MouseButtons Button { get; }
    public LinkLabelLinkClickedEventArgs(MouseButtons button) => Button = button;
}
