using Microsoft.JSInterop;

namespace System.Windows.Forms;

/// <summary>
/// WinForms-compatible <c>System.Windows.Forms.Screen</c> shim.
///
/// In the browser-canvas runtime there is one virtual "screen" that maps to the
/// browser's <c>window.screen</c> / <c>window.inner*</c> dimensions.
/// <list type="bullet">
///   <item><see cref="Bounds"/> — full screen pixel dimensions (<c>window.screen.width/height</c>).</item>
///   <item><see cref="WorkingArea"/> — viewport area after browser chrome (<c>window.innerWidth/Height</c>).</item>
///   <item><see cref="Primary"/> — always <c>true</c>; one virtual screen in the browser.</item>
/// </list>
/// Call <see cref="RefreshAsync"/> (done automatically by <c>FormRenderer</c> on first render) to
/// populate live dimensions from JS; before that a reasonable 1920×1080 fallback is used.
/// </summary>
public sealed class Screen
{
    // ── Static state ──────────────────────────────────────────────────────────

    internal static IJSRuntime? _jsRuntime;

    // Cached info; updated by RefreshAsync / UpdateFromInfo.
    private static ScreenInfo _info = new()
    {
        ScreenWidth  = 1920,
        ScreenHeight = 1080,
        WorkingWidth  = 1920,
        WorkingHeight = 1040,
        DevicePixelRatio = 1.0,
        ColorDepth = 32
    };

    private static Screen _primary = BuildFromInfo(_info);

    // ── Instance ──────────────────────────────────────────────────────────────

    private Screen(System.Drawing.Rectangle bounds, System.Drawing.Rectangle workingArea, string deviceName, int bitsPerPixel, bool primary)
    {
        Bounds      = bounds;
        WorkingArea = workingArea;
        DeviceName  = deviceName;
        BitsPerPixel = bitsPerPixel;
        Primary     = primary;
    }

    /// <summary>Full screen bounds in pixels (origin always 0,0 in browser).</summary>
    public System.Drawing.Rectangle Bounds { get; }

    /// <summary>Usable working area — viewport after browser chrome.</summary>
    public System.Drawing.Rectangle WorkingArea { get; }

    /// <summary>Device name — always <c>"\\\\.\\DISPLAY1"</c> in browser context.</summary>
    public string DeviceName { get; }

    /// <summary>Colour depth in bits per pixel.</summary>
    public int BitsPerPixel { get; }

    /// <summary>Always <c>true</c>; there is one virtual screen in the browser.</summary>
    public bool Primary { get; }

    // ── Static API (WinForms compatible) ─────────────────────────────────────

    /// <summary>Returns the primary (and only) virtual browser screen.</summary>
    public static Screen PrimaryScreen => _primary;

    /// <summary>All screens — a single-element array in the browser.</summary>
    public static Screen[] AllScreens => new[] { _primary };

    /// <summary>Returns the screen that contains the given point (always primary).</summary>
    public static Screen FromPoint(System.Drawing.Point pt) => _primary;

    /// <summary>Returns the screen that contains the given rectangle (always primary).</summary>
    public static Screen FromRectangle(System.Drawing.Rectangle r) => _primary;

    /// <summary>Returns the screen that contains the given control (always primary).</summary>
    public static Screen FromControl(Control control) => _primary;

    /// <summary>Returns the working area of the screen that contains <paramref name="pt"/>.</summary>
    public static System.Drawing.Rectangle GetWorkingArea(System.Drawing.Point pt) => _primary.WorkingArea;

    /// <summary>Returns the working area of the screen that contains <paramref name="r"/>.</summary>
    public static System.Drawing.Rectangle GetWorkingArea(System.Drawing.Rectangle r) => _primary.WorkingArea;

    /// <summary>Returns the working area of the screen that contains <paramref name="control"/>.</summary>
    public static System.Drawing.Rectangle GetWorkingArea(Control control) => _primary.WorkingArea;

    /// <summary>Returns the bounds of the screen that contains <paramref name="pt"/>.</summary>
    public static System.Drawing.Rectangle GetBounds(System.Drawing.Point pt) => _primary.Bounds;

    /// <summary>Returns the bounds of the screen that contains <paramref name="r"/>.</summary>
    public static System.Drawing.Rectangle GetBounds(System.Drawing.Rectangle r) => _primary.Bounds;

    /// <summary>Returns the bounds of the screen that contains <paramref name="control"/>.</summary>
    public static System.Drawing.Rectangle GetBounds(Control control) => _primary.Bounds;

    // ── JS interop refresh ───────────────────────────────────────────────────

    /// <summary>
    /// Reads live screen/viewport dimensions from <c>window.getScreenInfo</c> and
    /// updates <see cref="PrimaryScreen"/>. Called by <c>FormRenderer</c> on first render.
    /// </summary>
    internal static async Task RefreshAsync()
    {
        if (_jsRuntime is null) return;
        try
        {
            var info = await _jsRuntime.InvokeAsync<ScreenInfo>("getScreenInfo");
            UpdateFromInfo(info);
        }
        catch
        {
            // JS not yet ready or permission denied — keep fallback values.
        }
    }

    /// <summary>
    /// Synchronously update from a <see cref="ScreenInfo"/> value returned by JS interop
    /// (e.g. when called from a synchronous Blazor lifecycle method).
    /// </summary>
    private static void UpdateFromInfo(ScreenInfo info)
    {
        _info    = info;
        _primary = BuildFromInfo(info);
    }

    private static Screen BuildFromInfo(ScreenInfo info) =>
        new Screen(
            bounds:      new System.Drawing.Rectangle(0, 0, info.ScreenWidth, info.ScreenHeight),
            workingArea: new System.Drawing.Rectangle(0, 0, info.WorkingWidth, info.WorkingHeight),
            deviceName:  "\\\\.\\DISPLAY1",
            bitsPerPixel: info.ColorDepth,
            primary:     true);

    // ── JS DTO ───────────────────────────────────────────────────────────────

    private record ScreenInfo
    {
        public int    ScreenWidth       { get; init; }
        public int    ScreenHeight      { get; init; }
        public int    WorkingWidth      { get; init; }
        public int    WorkingHeight     { get; init; }
        public double DevicePixelRatio  { get; init; }
        public int    ColorDepth        { get; init; }
    }
}
