namespace System.Windows.Forms;

public class ScrollableControl : Control
{
    public bool AutoScroll { get; set; }

    public System.Drawing.Size AutoScrollMargin { get; set; } = System.Drawing.Size.Empty;

    public System.Drawing.Size AutoScrollMinSize { get; set; } = System.Drawing.Size.Empty;

    public System.Drawing.Point AutoScrollPosition { get; set; } = System.Drawing.Point.Empty;

    public override object? CreateParams => null;
}
