using Canvas.Windows.Forms;
using Microsoft.JSInterop;

namespace System.Windows.Forms;

/// <summary>
/// Designer-time interface allowing a component to provide extended properties
/// to controls. Canvas provides a runtime-only stub.
/// </summary>
public interface IExtenderProvider
{
    bool CanExtend(object extendee);
}

/// <summary>
/// Provides pop-up or online Help for controls. Implements the WinForms
/// <c>HelpProvider</c> API (F1 integration, per-control help text / keyword / URL).
/// Canvas implementation: F1 on a control with a help URL opens the browser tab via JS;
/// F1 with help text shows a MessageBox; F1 with a keyword is treated as text.
/// </summary>
public class HelpProvider : System.ComponentModel.Component, IExtenderProvider
{
    public HelpProvider() { }

    /// <summary>
    /// Initialises a new <see cref="HelpProvider"/> owned by the specified container (for designer compatibility).
    /// </summary>
    public HelpProvider(System.ComponentModel.IContainer container) : this()
    {
        container?.Add(this);
    }

    // ── Per-control registrations ─────────────────────────────────────────────

    private readonly Dictionary<Control, string>  _helpString  = new();
    private readonly Dictionary<Control, string>  _helpKeyword = new();
    private readonly Dictionary<Control, HelpNavigator> _helpNavigator = new();
    private readonly Dictionary<Control, bool>    _showHelp    = new();
    private readonly HashSet<Control>             _hooked      = new();

    // ── HelpNamespace ────────────────────────────────────────────────────────

    private string? _helpNamespace;

    /// <summary>
    /// Base URL or .chm path of the help file.
    /// In canvas, this is treated as a URL prefix opened in the browser.
    /// </summary>
    public string? HelpNamespace
    {
        get => _helpNamespace;
        set => _helpNamespace = value;
    }

    // ── IExtenderProvider ─────────────────────────────────────────────────────

    public bool CanExtend(object extendee) => extendee is Control;

    // ── SetHelpString / GetHelpString ─────────────────────────────────────────

    public void SetHelpString(Control ctl, string? helpString)
    {
        if (helpString is null || helpString.Length == 0)
            _helpString.Remove(ctl);
        else
            _helpString[ctl] = helpString;
        HookControl(ctl);
    }

    public string GetHelpString(Control ctl)
        => _helpString.TryGetValue(ctl, out var s) ? s : string.Empty;

    // ── SetHelpKeyword / GetHelpKeyword ───────────────────────────────────────

    public void SetHelpKeyword(Control ctl, string? keyword)
    {
        if (keyword is null || keyword.Length == 0)
            _helpKeyword.Remove(ctl);
        else
            _helpKeyword[ctl] = keyword;
        HookControl(ctl);
    }

    public string GetHelpKeyword(Control ctl)
        => _helpKeyword.TryGetValue(ctl, out var k) ? k : string.Empty;

    // ── SetHelpNavigator / GetHelpNavigator ───────────────────────────────────

    public void SetHelpNavigator(Control ctl, HelpNavigator navigator)
    {
        _helpNavigator[ctl] = navigator;
        HookControl(ctl);
    }

    public HelpNavigator GetHelpNavigator(Control ctl)
        => _helpNavigator.TryGetValue(ctl, out var n) ? n : HelpNavigator.AssociateIndex;

    // ── SetShowHelp / GetShowHelp ─────────────────────────────────────────────

    public void SetShowHelp(Control ctl, bool value)
    {
        _showHelp[ctl] = value;
        HookControl(ctl);
    }

    public bool GetShowHelp(Control ctl)
        => _showHelp.TryGetValue(ctl, out var v) ? v : HasAnyHelp(ctl);

    // ── Help() overloads ──────────────────────────────────────────────────────

    /// <summary>Shows help for the specified control (called programmatically or from F1).</summary>
    public void ShowHelp(Control ctl)
    {
        if (!GetShowHelp(ctl)) return;

        // URL: namespace + keyword takes priority
        string url = BuildUrl(ctl);
        if (!string.IsNullOrEmpty(url))
        {
            Help.ShowHelp(ctl, url);
            return;
        }

        string text = GetHelpString(ctl);
        if (!string.IsNullOrEmpty(text))
        {
            BrowserNavigationService.ShowAlert(text);
            return;
        }

        if (!string.IsNullOrEmpty(_helpNamespace))
            Help.ShowHelp(ctl, _helpNamespace);
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var ctl in _hooked)
                UnhookControl(ctl);
            _hooked.Clear();
            _helpString.Clear();
            _helpKeyword.Clear();
            _helpNavigator.Clear();
            _showHelp.Clear();
        }
        base.Dispose(disposing);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private bool HasAnyHelp(Control ctl)
        => _helpString.ContainsKey(ctl) || _helpKeyword.ContainsKey(ctl) || _helpNavigator.ContainsKey(ctl);

    private string BuildUrl(Control ctl)
    {
        string ns = _helpNamespace ?? string.Empty;
        string kw = GetHelpKeyword(ctl);
        if (!string.IsNullOrEmpty(ns) && !string.IsNullOrEmpty(kw))
            return $"{ns.TrimEnd('/', '\\')}/#{kw}";
        if (!string.IsNullOrEmpty(ns))
            return ns;
        return string.Empty;
    }

    private void HookControl(Control ctl)
    {
        if (_hooked.Contains(ctl)) return;
        _hooked.Add(ctl);
        ctl.KeyDown += OnControlKeyDown;
        ctl.HelpRequested += OnHelpRequested;
    }

    private void UnhookControl(Control ctl)
    {
        ctl.KeyDown -= OnControlKeyDown;
        ctl.HelpRequested -= OnHelpRequested;
    }

    private void OnControlKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F1 && sender is Control ctl)
        {
            e.Handled = true;
            ShowHelp(ctl);
        }
    }

    private void OnHelpRequested(object? sender, HelpEventArgs e)
    {
        if (sender is Control ctl)
        {
            e.Handled = true;
            ShowHelp(ctl);
        }
    }
}

// ── HelpNavigator enum ────────────────────────────────────────────────────────

/// <summary>Specifies the command to use to display the Help file.</summary>
public enum HelpNavigator
{
    AssociateIndex = unchecked((int)0x80000003),
    Find           = unchecked((int)0x80000005),
    Index          = unchecked((int)0x80000002),
    KeywordIndex   = unchecked((int)0x80000006),
    TableOfContents = unchecked((int)0x80000001),
    Topic          = unchecked((int)0x80000004),
}

// ── Help static class ─────────────────────────────────────────────────────────

/// <summary>
/// Static helper that opens help content.
/// Canvas implementation opens the URL in a new browser tab via JS interop;
/// CHM paths show a MessageBox.
/// </summary>
public static class Help
{
    public static void ShowHelp(Control? parent, string? url)
    {
        if (string.IsNullOrEmpty(url)) return;

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            // Open URL in a new browser tab via JS interop
            try
            {
                var js = BrowserNavigationService.JSRuntime;
                if (js is not null)
                    _ = js.InvokeVoidAsync("open", url, "_blank");
            }
            catch { /* JS interop may not be available during unit tests */ }
        }
        else
        {
            // CHM / local file — show a message (browser cannot open local CHM)
            BrowserNavigationService.ShowAlert($"Help: {url}");
        }
    }

    public static void ShowHelp(Control? parent, string? url, HelpNavigator navigator)
        => ShowHelp(parent, url);

    public static void ShowHelp(Control? parent, string? url, string? keyword)
    {
        if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(keyword))
            ShowHelp(parent, $"{url.TrimEnd('/', '\\')}/#{keyword}");
        else
            ShowHelp(parent, url);
    }

    public static void ShowHelp(Control? parent, string? url, HelpNavigator navigator, string? keyword)
        => ShowHelp(parent, url, keyword);

    public static void ShowHelpIndex(Control? parent, string? url)
        => ShowHelp(parent, url);

    public static void ShowPopup(Control? parent, string caption, Point location)
        => BrowserNavigationService.ShowAlert(caption);
}


