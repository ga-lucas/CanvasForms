using Canvas.Windows.Forms;

namespace System.Windows.Forms;

// ── Supporting enums ──────────────────────────────────────────────────────────

public enum MessageBoxButtons
{
    OK                = 0,
    OKCancel          = 1,
    AbortRetryIgnore  = 2,
    YesNoCancel       = 3,
    YesNo             = 4,
    RetryCancel       = 5
}

public enum MessageBoxIcon
{
    None        = 0,
    Error       = 16,
    Hand        = 16,
    Stop        = 16,
    Question    = 32,
    Exclamation = 48,
    Warning     = 48,
    Asterisk    = 64,
    Information = 64
}

public enum MessageBoxDefaultButton
{
    Button1 = 0,
    Button2 = 256,
    Button3 = 512
}

public enum MessageBoxOptions
{
    None              = 0,
    ServiceNotification = 0x00200000,
    DefaultDesktopOnly  = 0x00020000,
    RightAlign          = 0x00080000,
    RtlReading          = 0x00100000
}

// ── MessageBox ────────────────────────────────────────────────────────────────

/// <summary>
/// WinForms-compatible <see cref="MessageBox"/> surface.
/// In the canvas/Blazor host, messages are forwarded to
/// <see cref="CanvasApplication.ShowMessageBox"/> (a browser-side modal).
/// All synchronous <c>Show</c> overloads return <see cref="DialogResult.OK"/>
/// (or the appropriate affirmative result) immediately when no async host is
/// available — matching the WinForms API signature.
/// </summary>
public static class MessageBox
{
    // ── Minimal one-arg overloads ─────────────────────────────────────────────

    public static DialogResult Show(string? text)
        => Show(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    public static DialogResult Show(IWin32Window? owner, string? text)
        => Show(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    // ── Two-arg ───────────────────────────────────────────────────────────────

    public static DialogResult Show(string? text, string? caption)
        => Show(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    public static DialogResult Show(IWin32Window? owner, string? text, string? caption)
        => Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    // ── Three-arg ─────────────────────────────────────────────────────────────

    public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons)
        => Show(null, text, caption, buttons, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    public static DialogResult Show(IWin32Window? owner, string? text, string? caption, MessageBoxButtons buttons)
        => Show(owner, text, caption, buttons, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    // ── Four-arg ──────────────────────────────────────────────────────────────

    public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        => Show(null, text, caption, buttons, icon,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    public static DialogResult Show(IWin32Window? owner, string? text, string? caption,
                                    MessageBoxButtons buttons, MessageBoxIcon icon)
        => Show(owner, text, caption, buttons, icon,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.None);

    // ── Five-arg ──────────────────────────────────────────────────────────────

    public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons,
                                    MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        => Show(null, text, caption, buttons, icon, defaultButton, MessageBoxOptions.None);

    public static DialogResult Show(IWin32Window? owner, string? text, string? caption,
                                    MessageBoxButtons buttons, MessageBoxIcon icon,
                                    MessageBoxDefaultButton defaultButton)
        => Show(owner, text, caption, buttons, icon, defaultButton, MessageBoxOptions.None);

    // ── Full overload (all args) ──────────────────────────────────────────────

    public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons,
                                    MessageBoxIcon icon, MessageBoxDefaultButton defaultButton,
                                    MessageBoxOptions options)
        => Show(null, text, caption, buttons, icon, defaultButton, options);

    /// <summary>
    /// Core implementation. Forwards to <see cref="CanvasApplication.ShowMessageBox"/>
    /// when a handler has been registered; otherwise returns the affirmative default result.
    /// </summary>
    public static DialogResult Show(IWin32Window? owner, string? text, string? caption,
                                    MessageBoxButtons buttons, MessageBoxIcon icon,
                                    MessageBoxDefaultButton defaultButton,
                                    MessageBoxOptions options)
    {
        // Delegate to the host when a handler is available (e.g. JS interop alert/confirm).
        if (Canvas.Windows.Forms.CanvasApplication.MessageBoxHandler != null)
        {
            return Canvas.Windows.Forms.CanvasApplication.MessageBoxHandler(
                owner, text ?? string.Empty, caption ?? string.Empty,
                buttons, icon, defaultButton, options);
        }

        // Fallback: return the affirmative default for the given button set.
        return DefaultResult(buttons);
    }

    // ── Async variant (non-blocking, for use from async Blazor code) ──────────

    /// <summary>
    /// Async version of <see cref="Show(string?)"/> for use from Blazor/async contexts.
    /// Awaits the host handler when available.
    /// </summary>
    public static Task<DialogResult> ShowAsync(string? text, string? caption = null,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        MessageBoxIcon icon = MessageBoxIcon.None,
        IWin32Window? owner = null)
    {
        if (Canvas.Windows.Forms.CanvasApplication.AsyncMessageBoxHandler != null)
        {
            return Canvas.Windows.Forms.CanvasApplication.AsyncMessageBoxHandler(
                owner, text ?? string.Empty, caption ?? string.Empty, buttons, icon);
        }

        return Task.FromResult(DefaultResult(buttons));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns the default (affirmative) <see cref="DialogResult"/> for a given button set.</summary>
    public static DialogResult DefaultResultFor(MessageBoxButtons buttons) => buttons switch
    {
        MessageBoxButtons.OK             => DialogResult.OK,
        MessageBoxButtons.OKCancel       => DialogResult.OK,
        MessageBoxButtons.YesNo          => DialogResult.Yes,
        MessageBoxButtons.YesNoCancel    => DialogResult.Yes,
        MessageBoxButtons.AbortRetryIgnore => DialogResult.Abort,
        MessageBoxButtons.RetryCancel    => DialogResult.Retry,
        _                                => DialogResult.OK
    };

    private static DialogResult DefaultResult(MessageBoxButtons buttons) => DefaultResultFor(buttons);
}
