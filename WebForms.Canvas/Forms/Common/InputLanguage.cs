namespace System.Windows.Forms;

/// <summary>
/// Represents the input language of the current thread.
/// In CanvasForms the browser controls the input language; this class is
/// provided for API compatibility only.
/// </summary>
public sealed class InputLanguage
{
    private InputLanguage() { }

    /// <summary>Returns the default input language for the current session (stub).</summary>
    public static InputLanguage DefaultInputLanguage { get; } = new InputLanguage();

    /// <summary>Returns the currently selected input language (stub — same as default).</summary>
    public static InputLanguage CurrentInputLanguage
    {
        get => DefaultInputLanguage;
        set { /* no-op */ }
    }

    /// <summary>Returns the collection of installed input languages (stub — contains only the default).</summary>
    public static InputLanguageCollection InstalledInputLanguages { get; }
        = new InputLanguageCollection(new[] { DefaultInputLanguage });

    /// <summary>Returns the culture associated with this input language (invariant in stub).</summary>
    public System.Globalization.CultureInfo Culture => System.Globalization.CultureInfo.InvariantCulture;

    /// <summary>Returns the name of the input language layout.</summary>
    public string LayoutName => "US";

    /// <summary>Returns the handle for the input locale identifier.</summary>
    public IntPtr Handle => IntPtr.Zero;

    public override string ToString() => LayoutName;
}

/// <summary>
/// A read-only collection of <see cref="InputLanguage"/> objects.
/// </summary>
public sealed class InputLanguageCollection : System.Collections.ObjectModel.ReadOnlyCollection<InputLanguage>
{
    internal InputLanguageCollection(IList<InputLanguage> list) : base(list) { }
}

/// <summary>
/// Provides data for the <see cref="Form.InputLanguageChanged"/> and
/// <see cref="Form.InputLanguageChanging"/> events.
/// </summary>
public class InputLanguageChangedEventArgs : EventArgs
{
    public InputLanguage InputLanguage { get; }
    public byte CharSet { get; }
    public InputLanguageChangedEventArgs(InputLanguage inputLanguage, byte charSet)
    {
        InputLanguage = inputLanguage;
        CharSet = charSet;
    }
}

/// <summary>
/// Provides data for the <see cref="Form.InputLanguageChanging"/> event.
/// </summary>
public class InputLanguageChangingEventArgs : System.ComponentModel.CancelEventArgs
{
    public InputLanguage InputLanguage { get; }
    public byte CharSet { get; }
    public InputLanguageChangingEventArgs(InputLanguage inputLanguage, byte charSet)
    {
        InputLanguage = inputLanguage;
        CharSet = charSet;
    }
}

/// <summary>Delegate for input language change events.</summary>
public delegate void InputLanguageChangedEventHandler(object sender, InputLanguageChangedEventArgs e);

/// <summary>Delegate for input language changing events.</summary>
public delegate void InputLanguageChangingEventHandler(object sender, InputLanguageChangingEventArgs e);
