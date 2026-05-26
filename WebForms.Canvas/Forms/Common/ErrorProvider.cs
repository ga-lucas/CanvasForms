
namespace System.Windows.Forms;

// ── ErrorProviderEntry ────────────────────────────────────────────────────────
/// <summary>
/// A single error annotation tracked by the registry.
/// Position is computed live at render time (not cached) so that controls inside
/// layout panels that reflow after <see cref="ErrorProvider.SetError"/> was called
/// still get their icon in the right place.
/// </summary>
public sealed class ErrorProviderEntry
{
    /// <summary>The control this entry is attached to.</summary>
    public Control Control { get; }
    /// <summary>The error message shown in the hover tooltip.</summary>
    public string Message { get; internal set; }
    /// <summary>Unique DOM id for the badge element.</summary>
    public string Id { get; }
    /// <summary>Whether the icon should blink (driven by <see cref="ErrorProvider.BlinkStyle"/>).</summary>
    public bool Blink { get; internal set; }
    /// <summary>
    /// Full animation period in milliseconds (= <c>BlinkRate * 2</c>).
    /// Only meaningful when <see cref="Blink"/> is <c>true</c>.
    /// </summary>
    public int BlinkPeriodMs { get; internal set; }
    /// <summary>
    /// CSS iteration count string: <c>"infinite"</c> for AlwaysBlink,
    /// or a finite number string (e.g. <c>"5"</c>) for BlinkIfDifferentError.
    /// </summary>
    public string BlinkIterations { get; internal set; } = "infinite";

    internal ErrorProviderEntry(Control control, string message, string id, bool blink,
                                 int blinkPeriodMs, string blinkIterations)
    {
        Control         = control;
        Message         = message;
        Id              = id;
        Blink           = blink;
        BlinkPeriodMs   = blinkPeriodMs;
        BlinkIterations = blinkIterations;
    }

    /// <summary>
    /// Computes the form-relative position of the badge icon at the moment of the call.
    /// Walking the parent chain each render ensures correctness after layout changes.
    /// </summary>
    public (int x, int y) ComputePosition()
    {
        const int IconSize = 16;
        var x = Control.Left + Control.Width + 2;
        var y = Control.Top  + Math.Max(0, (Control.Height - IconSize) / 2);
        var parent = Control.Parent;
        while (parent != null && parent is not Form)
        {
            x += parent.Left;
            y += parent.Top;
            parent = parent.Parent;
        }
        return (x, y);
    }
}

// ── ErrorProviderRegistry ─────────────────────────────────────────────────────
/// <summary>
/// Static registry that <see cref="FormRenderer"/> polls to render error-icon
/// overlays.  Updated exclusively by <see cref="ErrorProvider"/> instances.
/// </summary>
public static class ErrorProviderRegistry
{
    // control → entry (one entry per control regardless of how many ErrorProviders are used)
    private static readonly Dictionary<Control, ErrorProviderEntry> _entries = new();

    /// <summary>Snapshot of all active error entries (safe to iterate in Razor markup).</summary>
    public static IReadOnlyCollection<ErrorProviderEntry> Entries => _entries.Values;

    /// <summary>Raised when entries are added, changed, or removed.</summary>
    public static event EventHandler? Changed;

    internal static void Set(Control control, string message, bool blink = false,
                              int blinkPeriodMs = 500, string blinkIterations = "infinite")
    {
        var id = $"ep_{control.GetHashCode():x8}";

        if (string.IsNullOrEmpty(message))
        {
            if (_entries.Remove(control))
                Changed?.Invoke(null, EventArgs.Empty);
        }
        else
        {
            if (_entries.TryGetValue(control, out var existing))
            {
                existing.Message        = message;
                existing.Blink          = blink;
                existing.BlinkPeriodMs  = blinkPeriodMs;
                existing.BlinkIterations = blinkIterations;
            }
            else
            {
                _entries[control] = new ErrorProviderEntry(control, message, id, blink, blinkPeriodMs, blinkIterations);
            }
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }

    internal static void Remove(Control control)
    {
        if (_entries.Remove(control))
            Changed?.Invoke(null, EventArgs.Empty);
    }

    internal static void RemoveAll(IEnumerable<Control> controls)
    {
        var changed = false;
        foreach (var c in controls)
            changed |= _entries.Remove(c);
        if (changed)
            Changed?.Invoke(null, EventArgs.Empty);
    }

    }

// ── ErrorProvider ─────────────────────────────────────────────────────────────
/// <summary>
/// WinForms-compatible ErrorProvider component.
///
/// Usage:
/// <code>
/// var ep = new ErrorProvider();
/// ep.SetError(textBox1, "Value is required.");
/// ep.SetError(textBox1, "");  // clears the error
/// </code>
///
/// Rendering is performed by the canvas <c>FormRenderer</c> via
/// <see cref="ErrorProviderRegistry"/>.  A small red ⊗ badge appears to the right
/// of each control that has an error; hovering it reveals the message text.
/// </summary>
public class ErrorProvider : System.ComponentModel.Component
{
    public ErrorProvider() { }

    /// <summary>
    /// Initialises a new <see cref="ErrorProvider"/> owned by the specified container (for designer compatibility).
    /// </summary>
    public ErrorProvider(System.ComponentModel.IContainer container) : this()
    {
        container?.Add(this);
    }

    private readonly Dictionary<Control, string> _errors = new();
    private ContainerControl? _containerControl;
    private int _blinkRate  = 250;
    private ErrorBlinkStyle _blinkStyle = ErrorBlinkStyle.BlinkIfDifferentError;

    // ── Properties (WinForms API) ─────────────────────────────────────────────

    /// <summary>The container (form) this provider is scoped to.  Optional.</summary>
    public ContainerControl? ContainerControl
    {
        get => _containerControl;
        set => _containerControl = value;
    }

    /// <summary>Blink rate in ms (accepted; no actual blinking in canvas — animations are CSS).</summary>
    public int BlinkRate  { get => _blinkRate;  set => _blinkRate  = Math.Max(0, value); }

    /// <summary>When the error icon blinks (accepted; not differentiated in canvas).</summary>
    public ErrorBlinkStyle BlinkStyle { get => _blinkStyle; set => _blinkStyle = value; }

    /// <summary>Custom icon (ResourcePath accepted; canvas renders a standard red badge regardless).</summary>
    public object? Icon { get; set; }

    // ── Core API ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets the error description for a control.  Passing an empty or null string
    /// clears any existing error for that control.
    /// </summary>
    public void SetError(Control control, string? value)
    {
        if (control == null) return;

        var msg = value ?? string.Empty;

        // Determine whether this call should trigger a blink.
        bool isDifferentError = _errors.TryGetValue(control, out var prev) && prev != msg && !string.IsNullOrEmpty(msg);
        bool blink = _blinkStyle switch
        {
            ErrorBlinkStyle.AlwaysBlink          => !string.IsNullOrEmpty(msg),
            ErrorBlinkStyle.BlinkIfDifferentError => isDifferentError,
            _                                    => false   // NeverBlink
        };

        // BlinkRate is the half-period (ms per visible/invisible phase), so the full CSS period = BlinkRate * 2.
        // Minimum 100ms to avoid seizure-risk and degenerate values.
        int periodMs = Math.Max(100, _blinkRate) * 2;

        // AlwaysBlink keeps blinking forever; BlinkIfDifferentError blinks 5 times then stops.
        string iterations = _blinkStyle == ErrorBlinkStyle.AlwaysBlink ? "infinite" : "5";

        _errors[control] = msg;
        ErrorProviderRegistry.Set(control, msg, blink, periodMs, iterations);
    }

    /// <summary>Returns the current error description for <paramref name="control"/>.</summary>
    public string GetError(Control control)
        => _errors.TryGetValue(control, out var msg) ? msg : string.Empty;

    /// <summary>Clears all errors managed by this provider.</summary>
    public void Clear()
    {
        ErrorProviderRegistry.RemoveAll(_errors.Keys);
        _errors.Clear();
    }

    // ── DataSource / BindingSource integration ────────────────────────────────

    /// <summary>Data source for automatic validation error display (stub — accepted, not yet auto-wired).</summary>
    public object? DataSource { get; set; }

    /// <summary>Data member used when DataSource is set (stub).</summary>
    public string DataMember { get; set; } = string.Empty;

    // ── Binding helper ────────────────────────────────────────────────────────

    /// <summary>
    /// Hooks <see cref="Control.Validated"/> on <paramref name="control"/> so that
    /// a validation delegate can call <see cref="SetError"/> in response.
    /// This mirrors the designer pattern of wiring Validated events at design time.
    /// </summary>
    public void BindValidation(Control control, CancelEventHandler validateHandler)
    {
        if (control == null) return;
        control.Validating += validateHandler;
    }

    // ── IExtenderProvider-style helpers ──────────────────────────────────────

    /// <summary>Returns true when <paramref name="control"/> currently has an error.</summary>
    public bool HasError(Control control)
        => _errors.TryGetValue(control, out var m) && !string.IsNullOrEmpty(m);

    // ── Dispose ───────────────────────────────────────────────────────────────

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Clear();
        base.Dispose(disposing);
    }
}

// ── Enums ─────────────────────────────────────────────────────────────────────
public enum ErrorBlinkStyle
{
    /// <summary>Always blink when an error is set.</summary>
    AlwaysBlink,
    /// <summary>Blink only when a new (different) error replaces an existing one.</summary>
    BlinkIfDifferentError,
    /// <summary>Never blink.</summary>
    NeverBlink
}
