
namespace System.Windows.Forms;

// ── ToolTipRegistry ───────────────────────────────────────────────────────────
/// <summary>
/// Singleton state bag read by <see cref="FormRenderer"/> to render the
/// active tooltip overlay div.  Updated exclusively by <see cref="ToolTip"/>.
/// </summary>
public static class ToolTipRegistry
{
    /// <summary>Text currently being displayed (empty = nothing visible).</summary>
    public static string Text      { get; internal set; } = string.Empty;
    /// <summary>Optional title shown above the text.</summary>
    public static string Title     { get; internal set; } = string.Empty;
    /// <summary>Icon badge rendered alongside the title.</summary>
    public static ToolTipIcon Icon { get; internal set; } = ToolTipIcon.None;
    /// <summary>Whether to render a balloon-style border.</summary>
    public static bool IsBalloon   { get; internal set; }
    /// <summary>Left position in form-relative px (set by ToolTip.Show).</summary>
    public static int X            { get; internal set; }
    /// <summary>Top position in form-relative px.</summary>
    public static int Y            { get; internal set; }

    /// <summary>True when a tooltip is currently visible.</summary>
    public static bool IsVisible   => !string.IsNullOrEmpty(Text);

    /// <summary>Fired when the registry state changes so FormRenderer can call StateHasChanged.</summary>
    public static event EventHandler? Changed;

    internal static void Show(string text, string title, ToolTipIcon icon, bool balloon, int x, int y)
    {
        Text      = text;
        Title     = title;
        Icon      = icon;
        IsBalloon = balloon;
        X         = x;
        Y         = y;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    internal static void Hide()
    {
        if (!IsVisible) return;
        Text = string.Empty;
        Changed?.Invoke(null, EventArgs.Empty);
    }
}

// ── ToolTip ───────────────────────────────────────────────────────────────────
/// <summary>
/// WinForms-compatible ToolTip component.  Call <see cref="SetToolTip"/> to
/// associate tooltip text with a control.  When the user hovers over the
/// control the tooltip appears after <see cref="InitialDelay"/> ms and hides
/// after <see cref="AutoPopDelay"/> ms.
///
/// Rendering is done by the canvas <c>FormRenderer</c> component via
/// <see cref="ToolTipRegistry"/>.
/// </summary>
public class ToolTip : System.ComponentModel.Component
{
    // Value: (caption, per-control AutoPopDelay; -1 = use global)
    private readonly Dictionary<Control, (string Caption, int PopDelay)> _toolTips = new();
    private bool   _active        = true;
    private int    _autoPopDelay  = 5000;
    private int    _initialDelay  = 500;
    private int    _reshowDelay   = 100;
    private bool   _showAlways    = false;
    private bool   _isBalloon     = false;
    private ToolTipIcon _icon     = ToolTipIcon.None;
    private string  _title        = string.Empty;

    // Cancellation token source for the current pending show/hide timer.
    private CancellationTokenSource? _cts;

    // Timestamp of the last time a tooltip was hidden (used to select ReshowDelay vs InitialDelay).
    private DateTime _lastHideTime = DateTime.MinValue;

    // ── Public properties (match WinForms ToolTip) ────────────────────────────
    public bool        Active        { get => _active;       set => _active       = value; }
    public int         AutoPopDelay  { get => _autoPopDelay; set => _autoPopDelay = value; }
    public int         InitialDelay  { get => _initialDelay; set => _initialDelay = value; }
    public int         ReshowDelay   { get => _reshowDelay;  set => _reshowDelay  = value; }
    public bool        ShowAlways    { get => _showAlways;   set => _showAlways   = value; }
    public bool        IsBalloon     { get => _isBalloon;    set => _isBalloon    = value; }
    public ToolTipIcon ToolTipIcon   { get => _icon;         set => _icon         = value; }
    public string      ToolTipTitle  { get => _title;        set => _title        = value; }

    /// <summary>Background colour of the tooltip window. Accepted; not yet propagated to canvas rendering.</summary>
    public Color BackColor  { get; set; } = Color.Empty;
    /// <summary>Foreground (text) colour of the tooltip. Accepted; not yet propagated to canvas rendering.</summary>
    public Color ForeColor  { get; set; } = Color.Empty;
    /// <summary>When true, ampersands (&amp;) are stripped from the tooltip text (accepted; always stripped in canvas).</summary>
    public bool  StripAmpersands { get; set; } = true;
    /// <summary>When true (default), the tooltip fades in/out. Accepted; no-op in the canvas layer.</summary>
    public bool  UseFading  { get; set; } = true;
    /// <summary>When true (default), the tooltip slides in. Accepted; no-op in the canvas layer.</summary>
    public bool  UseAnimation { get; set; } = true;

    // ── Association ───────────────────────────────────────────────────────────

    /// <summary>
    /// Associates <paramref name="caption"/> with <paramref name="control"/>.
    /// Passing an empty/null string removes the association.
    /// Hooks <see cref="Control.MouseEnter"/> and <see cref="Control.MouseLeave"/>
    /// on first association.
    /// </summary>
    public void SetToolTip(Control control, string caption)
        => SetToolTip(control, caption, -1);

    /// <summary>
    /// Associates <paramref name="caption"/> with <paramref name="control"/> and
    /// overrides <see cref="AutoPopDelay"/> for this specific control.
    /// Pass <paramref name="autoPopDelay"/> = -1 to use the global setting.
    /// </summary>
    public void SetToolTip(Control control, string caption, int autoPopDelay)
    {
        if (control == null) return;

        var alreadyHooked = _toolTips.ContainsKey(control);

        if (string.IsNullOrEmpty(caption))
        {
            _toolTips.Remove(control);
            if (alreadyHooked)
                UnhookControl(control);
        }
        else
        {
            _toolTips[control] = (caption, autoPopDelay);
            if (!alreadyHooked)
                HookControl(control);
        }
    }

    /// <summary>Returns the tooltip text for a control (empty string if none).</summary>
    public string GetToolTip(Control control)
        => _toolTips.TryGetValue(control, out var entry) ? entry.Caption : string.Empty;

    /// <summary>Removes all tooltip associations.</summary>
    public void RemoveAll()
    {
        foreach (var c in _toolTips.Keys.ToList())
            UnhookControl(c);
        _toolTips.Clear();
    }

    // ── Manual Show / Hide ────────────────────────────────────────────────────

    /// <summary>
    /// Immediately shows the specified text near <paramref name="control"/>.
    /// </summary>
    public void Show(string text, Control control)
        => ShowImmediate(text, control, _autoPopDelay);

    /// <summary>Shows for <paramref name="duration"/> ms.</summary>
    public void Show(string text, Control control, int duration)
        => ShowImmediate(text, control, duration);

    /// <summary>Shows at an explicit form-relative offset.</summary>
    public void Show(string text, Control control, Point point)
        => ShowAt(text, GetFormX(control) + point.X, GetFormY(control) + point.Y, _autoPopDelay);

    /// <summary>Shows at an explicit form-relative offset for <paramref name="duration"/> ms.</summary>
    public void Show(string text, Control control, Point point, int duration)
        => ShowAt(text, GetFormX(control) + point.X, GetFormY(control) + point.Y, duration);

    /// <summary>Hides any tooltip that was shown for <paramref name="control"/>.</summary>
    public void Hide(Control control) => CancelAndHide();

    // ── Internals ─────────────────────────────────────────────────────────────

    private void HookControl(Control control)
    {
        control.MouseEnter += OnControlMouseEnter;
        control.MouseLeave += OnControlMouseLeave;
        control.MouseDown  += OnControlMouseDown;
    }

    private void UnhookControl(Control control)
    {
        control.MouseEnter -= OnControlMouseEnter;
        control.MouseLeave -= OnControlMouseLeave;
        control.MouseDown  -= OnControlMouseDown;
    }

    private void OnControlMouseEnter(object? sender, EventArgs e)
    {
        if (!_active || sender is not Control c) return;
        if (!_toolTips.TryGetValue(c, out var entry)) return;
        var tip = entry.Caption;

        // ShowAlways = false (default): suppress tooltip when the parent form is not active.
        if (!_showAlways)
        {
            var rootForm = GetRootForm(c);
            if (rootForm != null && Canvas.Windows.Forms.CanvasApplication.FormManager?.ActiveForm != rootForm)
                return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // Use ReshowDelay if a tooltip was recently dismissed; otherwise use the full InitialDelay.
        var elapsed = (DateTime.UtcNow - _lastHideTime).TotalMilliseconds;
        var delay    = (elapsed < _autoPopDelay) ? _reshowDelay : _initialDelay;

        // Per-control AutoPopDelay takes priority over the global setting.
        var duration = entry.PopDelay >= 0 ? entry.PopDelay : _autoPopDelay;

        // Compute position below the control (form-relative)
        var x = GetFormX(c);
        var y = GetFormY(c) + c.Height + 2;

        _ = Task.Run(async () =>
        {
            await Task.Delay(delay, token);
            if (token.IsCancellationRequested) return;
            _lastHideTime = DateTime.MinValue;   // reset — tooltip is now visible
            ToolTipRegistry.Show(tip, _title, _icon, _isBalloon, x, y);

            await Task.Delay(duration, token);
            if (token.IsCancellationRequested) return;
            _lastHideTime = DateTime.UtcNow;
            ToolTipRegistry.Hide();
        }, token);
    }

    private void OnControlMouseLeave(object? sender, EventArgs e)  => CancelAndHide();
    private void OnControlMouseDown(object? sender, MouseEventArgs e) => CancelAndHide();

    private void CancelAndHide()
    {
        _cts?.Cancel();
        _cts = null;
        if (ToolTipRegistry.IsVisible)
        {
            _lastHideTime = DateTime.UtcNow;
            ToolTipRegistry.Hide();
        }
    }

    private void ShowImmediate(string text, Control control, int duration)
    {
        var x = GetFormX(control);
        var y = GetFormY(control) + control.Height + 2;
        ShowAt(text, x, y, duration);
    }

    private void ShowAt(string text, int x, int y, int duration)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        ToolTipRegistry.Show(text, _title, _icon, _isBalloon, x, y);
        _ = Task.Run(async () =>
        {
            await Task.Delay(duration, token);
            if (token.IsCancellationRequested) return;
            ToolTipRegistry.Hide();
        }, token);
    }

    // Walk the parent chain to find the root Form that owns this control.
    private static Form? GetRootForm(Control c)
    {
        Control? p = c.Parent;
        while (p != null)
        {
            if (p is Form f) return f;
            p = p.Parent;
        }
        return c as Form;
    }

    // Walk the parent chain to compute the form-relative X/Y of a control.
    private static int GetFormX(Control c)
    {
        var x = c.Left;
        var p = c.Parent;
        while (p != null && p is not Form) { x += p.Left; p = p.Parent; }
        return x;
    }

    private static int GetFormY(Control c)
    {
        var y = c.Top;
        var p = c.Parent;
        while (p != null && p is not Form) { y += p.Top; p = p.Parent; }
        return y;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CancelAndHide();
            RemoveAll();
        }
        base.Dispose(disposing);
    }
}

public enum ToolTipIcon { None, Info, Warning, Error }
