namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms NotifyIcon component (system tray icon)
/// This is a canvas-compatible stub — actual OS tray integration is not available
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

    public event EventHandler? Click;
    public event MouseEventHandler? MouseClick;
    public event MouseEventHandler? MouseDoubleClick;
    public event EventHandler? DoubleClick;
    public event EventHandler? BalloonTipClicked;
    public event EventHandler? BalloonTipClosed;
    public event EventHandler? BalloonTipShown;

    public bool Visible
    {
        get => _visible;
        set { _visible = value; }
    }

    /// <summary>
    /// Tooltip text shown when hovering over the tray icon (max 63 chars in WinForms)
    /// </summary>
    public string Text
    {
        get => _text;
        set => _text = value?.Length > 63 ? value.Substring(0, 63) : value ?? string.Empty;
    }

    public Icon? Icon { get => _icon; set => _icon = value; }

    public ContextMenuStrip? ContextMenuStrip { get => _contextMenuStrip; set => _contextMenuStrip = value; }

    public BalloonTipIcon BalloonTipIcon { get => _balloonTipIcon; set => _balloonTipIcon = value; }
    public string BalloonTipTitle { get => _balloonTipTitle; set => _balloonTipTitle = value; }
    public string BalloonTipText { get => _balloonTipText; set => _balloonTipText = value; }

    /// <summary>
    /// Shows a balloon tooltip from the tray icon (stub)
    /// </summary>
    public void ShowBalloonTip(int timeout) { BalloonTipShown?.Invoke(this, EventArgs.Empty); }
    public void ShowBalloonTip(int timeout, string tipTitle, string tipText, BalloonTipIcon tipIcon)
    {
        _balloonTipTitle = tipTitle;
        _balloonTipText = tipText;
        _balloonTipIcon = tipIcon;
        ShowBalloonTip(timeout);
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

/// <summary>Stub ContextMenuStrip for API compatibility</summary>
public class ContextMenuStrip : System.ComponentModel.Component { }
