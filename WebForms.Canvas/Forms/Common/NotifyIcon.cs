using Canvas.Windows.Forms.Services;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms NotifyIcon component (system tray icon).
/// When <see cref="Visible"/> is <see langword="true"/> the icon is added to
/// the canvas system-tray area rendered inside the taskbar.
/// </summary>
public class NotifyIcon : System.ComponentModel.Component
{
    private bool _visible = false;
    private string _text = string.Empty;
    private Icon? _icon;
    private ContextMenuStrip? _contextMenuStrip;
    private BalloonTipIcon _balloonTipIcon = BalloonTipIcon.None;
    private string _balloonTipTitle = string.Empty;
    private string _balloonTipText = string.Empty;

    // ── Events ───────────────────────────────────────────────────────────────
    public event EventHandler?      Click;
    public event MouseEventHandler? MouseClick;
    public event MouseEventHandler? MouseDoubleClick;
    public event EventHandler?      DoubleClick;
    public event MouseEventHandler? MouseDown;
    public event MouseEventHandler? MouseUp;
    public event MouseEventHandler? MouseMove;
    public event EventHandler?      BalloonTipClicked;
    public event EventHandler?      BalloonTipClosed;
    public event EventHandler?      BalloonTipShown;

    // ── Active balloon tip (read by SystemTray.razor) ─────────────────────
    internal BalloonTipInfo? ActiveBalloon { get; private set; }

    // ── Properties ───────────────────────────────────────────────────────────

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            if (_visible)
                NotifyIconRegistry.Instance.Register(this);
            else
                NotifyIconRegistry.Instance.Unregister(this);
        }
    }

    /// <summary>Tooltip text (max 63 chars, matching WinForms).</summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value?.Length > 63 ? value[..63] : value ?? string.Empty;
            if (_visible) NotifyIconRegistry.Instance.NotifyChanged();
        }
    }

    public Icon? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            if (_visible) NotifyIconRegistry.Instance.NotifyChanged();
        }
    }

    public ContextMenuStrip? ContextMenuStrip { get => _contextMenuStrip; set => _contextMenuStrip = value; }

    public BalloonTipIcon BalloonTipIcon { get => _balloonTipIcon; set => _balloonTipIcon = value; }
    public string BalloonTipTitle { get => _balloonTipTitle; set => _balloonTipTitle = value; }
    public string BalloonTipText  { get => _balloonTipText;  set => _balloonTipText  = value; }

    // ── Methods called by SystemTray.razor ────────────────────────────────

    internal void RaiseClick(MouseEventArgs e)
    {
        Click?.Invoke(this, e);
        MouseClick?.Invoke(this, e);
    }

    internal void RaiseDoubleClick(MouseEventArgs e)
    {
        DoubleClick?.Invoke(this, e);
        MouseDoubleClick?.Invoke(this, e);
    }

    internal void RaiseMouseDown(MouseEventArgs e)     => MouseDown?.Invoke(this, e);
    internal void RaiseMouseUp(MouseEventArgs e)       => MouseUp?.Invoke(this, e);
    internal void RaiseMouseMove(MouseEventArgs e)     => MouseMove?.Invoke(this, e);

    internal void RaiseBalloonTipClicked() => BalloonTipClicked?.Invoke(this, EventArgs.Empty);
    internal void RaiseBalloonTipClosed()  => BalloonTipClosed?.Invoke(this, EventArgs.Empty);

    // ── ShowBalloonTip ────────────────────────────────────────────────────

    public void ShowBalloonTip(int timeout) => ShowBalloonTip(timeout, _balloonTipTitle, _balloonTipText, _balloonTipIcon);

    public void ShowBalloonTip(int timeout, string tipTitle, string tipText, BalloonTipIcon tipIcon)
    {
        _balloonTipTitle = tipTitle;
        _balloonTipText  = tipText;
        _balloonTipIcon  = tipIcon;

        ActiveBalloon = new BalloonTipInfo(tipTitle, tipText, tipIcon, timeout > 0 ? timeout : 3000);
        if (_visible) NotifyIconRegistry.Instance.NotifyChanged();
        BalloonTipShown?.Invoke(this, EventArgs.Empty);
    }

    internal void ClearBalloon()
    {
        ActiveBalloon = null;
        BalloonTipClosed?.Invoke(this, EventArgs.Empty);
        if (_visible) NotifyIconRegistry.Instance.NotifyChanged();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _visible)
            NotifyIconRegistry.Instance.Unregister(this);
        base.Dispose(disposing);
    }
}

/// <summary>Carries the active balloon-tip payload for the tray UI.</summary>
internal sealed class BalloonTipInfo
{
    public string Title   { get; }
    public string Text    { get; }
    public BalloonTipIcon Icon { get; }
    public int TimeoutMs  { get; }

    public BalloonTipInfo(string title, string text, BalloonTipIcon icon, int timeoutMs)
    {
        Title     = title;
        Text      = text;
        Icon      = icon;
        TimeoutMs = timeoutMs;
    }
}

public enum BalloonTipIcon { None, Info, Warning, Error }

/// <summary>Stub icon class for API compatibility</summary>
public class Icon : IDisposable
{
    public string? ResourcePath { get; }
    public Icon(string path) => ResourcePath = path;
    public void Dispose() { }
}
