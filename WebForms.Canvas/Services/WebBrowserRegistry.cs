using System.Windows.Forms;

namespace Canvas.Windows.Forms.Services;

/// <summary>
/// Tracks all active <see cref="WebBrowser"/> instances so that
/// <see cref="Canvas.Windows.Forms.Components.FormRenderer"/> can render
/// the corresponding iframe overlays outside the canvas element.
/// </summary>
public class WebBrowserRegistry
{
    /// <summary>Process-wide singleton — accessible from <see cref="WebBrowser"/> without DI.</summary>
    public static readonly WebBrowserRegistry Instance = new();

    // Maps each WebBrowser to the Form that owns it.
    private readonly List<WebBrowser> _browsers = new();
    private Action? _onChanged;

    /// <summary>Subscribes the FormRenderer component for state-change notifications.</summary>
    public void SetChangeCallback(Action callback) => _onChanged = callback;

    /// <summary>All currently registered browser controls.</summary>
    public IReadOnlyList<WebBrowser> Browsers => _browsers;

    internal void Register(WebBrowser browser)
    {
        if (!_browsers.Contains(browser))
        {
            _browsers.Add(browser);
            _onChanged?.Invoke();
        }
    }

    internal void Unregister(WebBrowser browser)
    {
        if (_browsers.Remove(browser))
            _onChanged?.Invoke();
    }

    internal void NotifyChanged() => _onChanged?.Invoke();
}
