using Microsoft.JSInterop;

namespace System.Windows.Forms;

/// <summary>
/// Represents a label that can display one or more hyperlinks within its text.
/// </summary>
public class LinkLabel : Label
{
    // ── Inner types ───────────────────────────────────────────────────────────

    /// <summary>Represents a single hyperlink span within a <see cref="LinkLabel"/>.</summary>
    public class Link
    {
        internal LinkLabel? Owner;

        public Link() { }
        public Link(int start, int length) { Start = start; Length = length; }
        public Link(int start, int length, object? linkData) { Start = start; Length = length; LinkData = linkData; }

        /// <summary>Zero-based character index where the link starts.</summary>
        public int Start { get; set; }

        /// <summary>Number of characters covered by the link (0 = whole text).</summary>
        public int Length { get; set; }

        /// <summary>Application-defined data attached to this link.</summary>
        public object? LinkData { get; set; }

        private bool _visited;
        public bool Visited
        {
            get => _visited;
            set { _visited = value; Owner?.Invalidate(); }
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set { _enabled = value; Owner?.Invalidate(); }
        }

        public string Description { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        internal int EffectiveEnd(int textLen) => Length == 0 ? textLen : Start + Length;
    }

    /// <summary>Ordered collection of <see cref="Link"/> objects owned by a <see cref="LinkLabel"/>.</summary>
    public sealed class LinkCollection : IEnumerable<Link>
    {
        private readonly LinkLabel _owner;
        private readonly List<Link> _links = new();

        internal LinkCollection(LinkLabel owner) => _owner = owner;

        public int Count => _links.Count;
        public Link this[int index] => _links[index];

        public Link Add(object? linkData)
            => AddCore(new Link(0, 0, linkData));

        public Link Add(int start, int length)
            => AddCore(new Link(start, length));

        public Link Add(int start, int length, object? linkData)
            => AddCore(new Link(start, length, linkData));

        private Link AddCore(Link lk)
        {
            lk.Owner = _owner;
            _links.Add(lk);
            _owner.Invalidate();
            return lk;
        }

        public void Remove(Link link)   { _links.Remove(link);    _owner.Invalidate(); }
        public void RemoveAt(int index) { _links.RemoveAt(index); _owner.Invalidate(); }
        public void Clear()             { _links.Clear();         _owner.Invalidate(); }
        public bool Contains(Link link) => _links.Contains(link);
        public int  IndexOf(Link link)  => _links.IndexOf(link);

        public IEnumerator<Link> GetEnumerator() => _links.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _links.GetEnumerator();
    }

    // ── Fields ────────────────────────────────────────────────────────────────

    private readonly LinkCollection _links;
    private string _linkUrl = string.Empty;
    private Link? _hoveredLink;
    private Link? _activeLink;

    // ── Constructor ───────────────────────────────────────────────────────────

    public LinkLabel()
    {
        _links = new LinkCollection(this);
        ForeColor = Color.Black;
        Cursor = Cursor.Hand;
        TabStop = true;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    public LinkCollection Links => _links;

    public Color LinkColor        { get; set; } = Color.FromArgb(0, 0, 255);
    public Color VisitedLinkColor { get; set; } = Color.FromArgb(128, 0, 128);
    public Color ActiveLinkColor  { get; set; } = Color.FromArgb(255, 0, 0);
    public Color DisabledLinkColor { get; set; } = Color.FromArgb(133, 133, 133);
    public LinkBehavior LinkBehavior { get; set; } = LinkBehavior.SystemDefault;

    public bool LinkVisited
    {
        get => _links.Count > 0 && _links[0].Visited;
        set { EnsureDefaultLink(); _links[0].Visited = value; }
    }

    /// <summary>Legacy single-link URL. Setting this wires the first link to open the URL on click.</summary>
    public string LinkUrl
    {
        get => _linkUrl;
        set { _linkUrl = value ?? string.Empty; EnsureDefaultLink(); }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    public event LinkLabelLinkClickedEventHandler? LinkClicked;

    // ── Painting ──────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        DrawControlBackground(g);

        var text = Text ?? string.Empty;
        if (text.Length > 0)
            DrawSpans(g, text);

        DrawFocusRect(g, new Rectangle(0, 0, Width - 1, Height - 1));
    }

    private void DrawSpans(Graphics g, string text)
    {
        var runs = BuildRuns(text);
        var lines = text.Replace("\r", "").Split('\n');
        var (x0, y0, charHeight) = GetTextBlockPosition(lines);
        float charW = Font.Size * 0.6f;

        bool underlineAlways = LinkBehavior == LinkBehavior.AlwaysUnderline || LinkBehavior == LinkBehavior.SystemDefault;
        bool underlineHover  = LinkBehavior == LinkBehavior.HoverUnderline;

        int globalChar = 0;
        for (int li = 0; li < lines.Length; li++)
        {
            var line = lines[li];
            float y  = y0 + li * charHeight;
            float x  = x0 + GetLineX(line);
            int lineStart = globalChar;
            int lineEnd   = globalChar + line.Length;

            int pos = lineStart;
            while (pos < lineEnd)
            {
                var run = GetRunAt(runs, pos);
                int segEnd = Math.Min(lineEnd, run.End);
                var segment = text.Substring(pos, segEnd - pos);
                float segX = x + (pos - lineStart) * charW;

                Color color = run.IsLink && run.Link != null
                    ? GetLinkColor(run.Link)
                    : (Enabled ? ForeColor : DisabledLinkColor);

                g.DrawString(segment, Font, color, (int)segX, (int)y);

                if (run.IsLink)
                {
                    bool doLine = underlineAlways || (underlineHover && run.Link == _hoveredLink);
                    if (doLine)
                    {
                        float segW = segment.Length * charW;
                        using var pen = new Pen(color, 1);
                        g.DrawLine(pen, (int)segX, (int)(y + charHeight - 2), (int)(segX + segW), (int)(y + charHeight - 2));
                    }
                }

                pos = segEnd;
            }

            globalChar += line.Length + 1; // +1 for \n
        }
    }

    private Color GetLinkColor(Link lk)
    {
        if (!lk.Enabled || !Enabled) return DisabledLinkColor;
        if (lk == _activeLink || lk == _hoveredLink) return ActiveLinkColor;
        if (lk.Visited) return VisitedLinkColor;
        return LinkColor;
    }

    // ── Run helpers ───────────────────────────────────────────────────────────

    private record Run(int Start, int End, bool IsLink, Link? Link);

    private List<Run> BuildRuns(string text)
    {
        var result = new List<Run>();
        if (_links.Count == 0)
        {
            result.Add(new Run(0, text.Length, true, null));
            return result;
        }

        var sorted = _links.OrderBy(l => l.Start).ToList();
        int pos = 0;
        foreach (var lk in sorted)
        {
            int s = Math.Clamp(lk.Start, 0, text.Length);
            int e = Math.Clamp(lk.EffectiveEnd(text.Length), s, text.Length);
            if (s > pos) result.Add(new Run(pos, s, false, null));
            if (e > s)   result.Add(new Run(s, e, true, lk));
            pos = e;
        }
        if (pos < text.Length)
            result.Add(new Run(pos, text.Length, false, null));
        return result;
    }

    private static Run GetRunAt(List<Run> runs, int idx)
    {
        foreach (var r in runs)
            if (idx >= r.Start && idx < r.End) return r;
        return runs.Count > 0 ? runs[^1] : new Run(idx, idx + 1, false, null);
    }

    // ── Hit testing ───────────────────────────────────────────────────────────

    private Link? HitTestLink(int mouseX, int mouseY)
    {
        var text = Text ?? string.Empty;
        if (text.Length == 0 || _links.Count == 0) return null;

        var lines = text.Replace("\r", "").Split('\n');
        var (x0, y0, charHeight) = GetTextBlockPosition(lines);
        float charW = Font.Size * 0.6f;

        int globalChar = 0;
        for (int li = 0; li < lines.Length; li++)
        {
            var line = lines[li];
            float y = y0 + li * charHeight;
            if (mouseY >= y && mouseY < y + charHeight)
            {
                float x = x0 + GetLineX(line);
                if (mouseX >= x)
                {
                    int col = (int)((mouseX - x) / charW);
                    int charIdx = globalChar + Math.Clamp(col, 0, Math.Max(0, line.Length - 1));
                    foreach (var lk in _links)
                    {
                        int s = lk.Start;
                        int e = lk.EffectiveEnd(text.Length);
                        if (charIdx >= s && charIdx < e) return lk;
                    }
                }
            }
            globalChar += line.Length + 1;
        }
        return null;
    }

    // ── Mouse events ─────────────────────────────────────────────────────────

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        var hit = HitTestLink(e.X, e.Y);
        if (hit != _hoveredLink) { _hoveredLink = hit; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseEnter(EventArgs e) { Invalidate(); base.OnMouseEnter(e); }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        _hoveredLink = null;
        _activeLink  = null;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
        {
            Focus();
            _activeLink = HitTestLink(e.X, e.Y);
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
        {
            var clicked = _activeLink ?? HitTestLink(e.X, e.Y);
            _activeLink = null;

            if (clicked != null)
            {
                clicked.Visited = true;
                var url = (clicked.LinkData as string) ?? _linkUrl;
                if (!string.IsNullOrEmpty(url)) _ = NavigateToUrlAsync(url);
                LinkClicked?.Invoke(this, new LinkLabelLinkClickedEventArgs(e.Button, clicked));
            }
            else if (_links.Count == 0)
            {
                // Legacy: no Links defined — whole label is a link
                if (!string.IsNullOrEmpty(_linkUrl)) _ = NavigateToUrlAsync(_linkUrl);
                LinkClicked?.Invoke(this, new LinkLabelLinkClickedEventArgs(e.Button, null));
            }

            Invalidate();
        }
        base.OnMouseUp(e);
    }

    protected internal override void OnGotFocus(EventArgs e)  { Invalidate(); base.OnGotFocus(e); }
    protected internal override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void EnsureDefaultLink()
    {
        if (_links.Count == 0)
            _links.Add(0, 0);
    }

    private async Task NavigateToUrlAsync(string url)
    {
        try
        {
            var js = Canvas.Windows.Forms.BrowserNavigationService.JSRuntime;
            if (js != null)
                await js.InvokeVoidAsync("open", url, "_blank");
        }
        catch { /* JS interop unavailable */ }
    }
}

// ── Enums / delegates / event-args ────────────────────────────────────────────

public enum LinkBehavior { SystemDefault, AlwaysUnderline, HoverUnderline, NeverUnderline }

public delegate void LinkLabelLinkClickedEventHandler(object? sender, LinkLabelLinkClickedEventArgs e);

/// <summary>Provides data for the <see cref="LinkLabel.LinkClicked"/> event.</summary>
public class LinkLabelLinkClickedEventArgs : EventArgs
{
    public LinkLabelLinkClickedEventArgs(MouseButtons button, LinkLabel.Link? link)
    {
        Button = button;
        Link   = link;
    }

    public MouseButtons Button { get; }

    /// <summary>The <see cref="LinkLabel.Link"/> that was clicked, or <c>null</c> in legacy whole-label mode.</summary>
    public LinkLabel.Link? Link { get; }
}
