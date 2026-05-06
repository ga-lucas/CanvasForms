using System.Windows.Forms;

namespace Canvas.Windows.Forms.Services;

/// <summary>
/// Singleton service that tracks all visible <see cref="NotifyIcon"/> instances
/// and notifies the UI when the tray icon set changes.
/// </summary>
public class NotifyIconRegistry
{
    /// <summary>Process-wide singleton — accessible from <see cref="NotifyIcon"/> without DI.</summary>
    public static readonly NotifyIconRegistry Instance = new();

    private readonly List<NotifyIcon> _icons = new();
    private Action? _onChanged;

    /// <summary>Subscribes the Desktop/SystemTray component for state-change notifications.</summary>
    public void SetChangeCallback(Action callback) => _onChanged = callback;

    /// <summary>All currently registered (visible) tray icons.</summary>
    public IReadOnlyList<NotifyIcon> Icons => _icons;

    internal void Register(NotifyIcon icon)
    {
        if (!_icons.Contains(icon))
        {
            _icons.Add(icon);
            _onChanged?.Invoke();
        }
    }

    internal void Unregister(NotifyIcon icon)
    {
        if (_icons.Remove(icon))
            _onChanged?.Invoke();
    }

    internal void NotifyChanged() => _onChanged?.Invoke();
}
