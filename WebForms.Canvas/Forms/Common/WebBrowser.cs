using Canvas.Windows.Forms.Services;
using Microsoft.JSInterop;

namespace System.Windows.Forms;

/// <summary>
/// Canvas-hosted Web browser control.
/// Renders as an absolutely-positioned &lt;iframe&gt; overlay on top of the form canvas,
/// tracked to the control's position and size inside its parent form.
/// </summary>
/// <remarks>
/// Platform notes (browser / WASM):
/// <list type="bullet">
///   <item>Cross-origin navigation is allowed but DOM access (<see cref="Document"/>) is blocked by the browser sandbox.</item>
///   <item><see cref="GoBack"/> and <see cref="GoForward"/> require the iframe's <c>allow-same-origin</c> sandbox attribute.</item>
///   <item>Setting <see cref="DocumentText"/> uses the iframe's <c>srcdoc</c> attribute.</item>
/// </list>
/// </remarks>
public class WebBrowser : Control
{
    private Uri? _url;
    private string? _documentText;
    private bool _scriptEnabled = true;
    private string _iframeId = $"wb-{Guid.NewGuid():N}";

    // ── Internal: JS interop handle supplied by FormRenderer ─────────────────

    internal IJSRuntime? JSRuntime { get; set; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the URL currently displayed in the browser.</summary>
    public Uri? Url
    {
        get => _url;
        set
        {
            if (_url == value) return;
            _url = value;
            _documentText = null;
            WebBrowserRegistry.Instance.NotifyChanged();
            OnNavigated(new WebBrowserNavigatedEventArgs(_url));
        }
    }

    /// <summary>Gets or sets an HTML string to display (sets <c>srcdoc</c>). Clears <see cref="Url"/>.</summary>
    public string? DocumentText
    {
        get => _documentText;
        set
        {
            if (_documentText == value) return;
            _documentText = value;
            _url = null;
            WebBrowserRegistry.Instance.NotifyChanged();
        }
    }

    /// <summary>
    /// Gets the unique DOM id used for the rendered &lt;iframe&gt; element.
    /// FormRenderer uses this to locate the element for JS calls.
    /// </summary>
    public string IframeId => _iframeId;

    /// <summary>Gets the <see cref="Form"/> that directly contains this control.</summary>
    public Form? OwnerForm => FindForm();

    /// <summary>Whether script execution is allowed in the iframe. Default: <see langword="true"/>.</summary>
    public bool ScriptEnabled
    {
        get => _scriptEnabled;
        set { _scriptEnabled = value; WebBrowserRegistry.Instance.NotifyChanged(); }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    public event EventHandler<WebBrowserNavigatingEventArgs>? Navigating;
    public event EventHandler<WebBrowserNavigatedEventArgs>? Navigated;
    public event WebBrowserDocumentCompletedEventHandler? DocumentCompleted;
    public event EventHandler? CanGoBackChanged;
    public event EventHandler? CanGoForwardChanged;

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>Navigates the browser to the specified URL string.</summary>
    public void Navigate(string urlString)
    {
        var args = new WebBrowserNavigatingEventArgs(urlString);
        OnNavigating(args);
        if (args.Cancel) return;

        _url = new Uri(urlString, UriKind.RelativeOrAbsolute);
        _documentText = null;
        WebBrowserRegistry.Instance.NotifyChanged();
        OnNavigated(new WebBrowserNavigatedEventArgs(_url));
    }

    /// <summary>Navigates the browser to the specified URI.</summary>
    public void Navigate(Uri url)
    {
        var args = new WebBrowserNavigatingEventArgs(url.ToString());
        OnNavigating(args);
        if (args.Cancel) return;

        _url = url;
        _documentText = null;
        WebBrowserRegistry.Instance.NotifyChanged();
        OnNavigated(new WebBrowserNavigatedEventArgs(_url));
    }

    /// <summary>
    /// Navigates back in the iframe's session history.
    /// Requires a JS runtime; no-op if unavailable.
    /// </summary>
    public void GoBack() => FireAndForget(InvokeNavigationJSAsync("canvasWebBrowserGoBack"));

    /// <summary>
    /// Navigates forward in the iframe's session history.
    /// Requires a JS runtime; no-op if unavailable.
    /// </summary>
    public void GoForward() => FireAndForget(InvokeNavigationJSAsync("canvasWebBrowserGoForward"));

    /// <summary>Stops the current load.</summary>
    public void Stop() => FireAndForget(InvokeNavigationJSAsync("canvasWebBrowserStop"));

    /// <summary>Reloads the current page.</summary>
    public new void Refresh() => FireAndForget(InvokeNavigationJSAsync("canvasWebBrowserRefresh"));

    /// <summary>
    /// Executes a JavaScript expression inside the iframe and returns the result as a string.
    /// Works only for same-origin content.
    /// </summary>
    public async Task<string?> ExecuteScriptAsync(string script)
    {
        if (JSRuntime is null) return null;
        try
        {
            return await JSRuntime.InvokeAsync<string?>("canvasWebBrowserExecScript", _iframeId, script);
        }
        catch
        {
            return null;
        }
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WebBrowserRegistry.Instance.Register(this);
    }

    protected override void Dispose(bool disposing)
    {
        WebBrowserRegistry.Instance.Unregister(this);
        base.Dispose(disposing);
    }

    // We also need to register when added to a parent control's collection
    // (HandleCreated is a stub in this canvas runtime).
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        WebBrowserRegistry.Instance.Register(this);
    }

    // ── Virtual event raisers ─────────────────────────────────────────────────

    protected virtual void OnNavigating(WebBrowserNavigatingEventArgs e) => Navigating?.Invoke(this, e);
    protected virtual void OnNavigated(WebBrowserNavigatedEventArgs e) => Navigated?.Invoke(this, e);
    protected virtual void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e) => DocumentCompleted?.Invoke(this, e);

    // Called by FormRenderer after the iframe fires onload
    internal void RaiseDocumentCompleted()
    {
        OnDocumentCompleted(new WebBrowserDocumentCompletedEventArgs(_url));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task InvokeNavigationJSAsync(string funcName)
    {
        if (JSRuntime is null) return;
        try { await JSRuntime.InvokeVoidAsync(funcName, _iframeId); }
        catch { /* best effort */ }
    }

    private static void FireAndForget(Task t) => t.ContinueWith(_ => { }, TaskContinuationOptions.None);
}

// ── WebView2 alias ────────────────────────────────────────────────────────────

/// <summary>
/// Canvas-hosted WebView2-style control — implemented as a subclass of <see cref="WebBrowser"/>
/// since both render as an iframe overlay. Use <see cref="WebBrowser"/> for maximum compatibility.
/// </summary>
public class WebView2 : WebBrowser { }

// ── Supporting event arg types ────────────────────────────────────────────────

public class WebBrowserNavigatingEventArgs : CancelEventArgs
{
    public string Url { get; }
    public WebBrowserNavigatingEventArgs(string url) => Url = url;
}

public class WebBrowserNavigatedEventArgs : EventArgs
{
    public Uri? Url { get; }
    public WebBrowserNavigatedEventArgs(Uri? url) => Url = url;
}

public class WebBrowserDocumentCompletedEventArgs : EventArgs
{
    public Uri? Url { get; }
    public WebBrowserDocumentCompletedEventArgs(Uri? url) => Url = url;
}

public delegate void WebBrowserDocumentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs e);
