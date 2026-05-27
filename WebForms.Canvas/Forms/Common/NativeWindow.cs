namespace System.Windows.Forms;

/// <summary>
/// Provides a low-level encapsulation of a window handle and a window procedure.
/// In CanvasForms there are no Win32 window handles; this stub exists so that
/// translated assemblies that subclass <c>NativeWindow</c> compile without modification.
/// All methods are safe no-ops or return sentinel values.
/// </summary>
public class NativeWindow : MarshalByRefObject, IWin32Window
{
    /// <summary>Always returns <see cref="IntPtr.Zero"/> — no HWND exists in CanvasForms.</summary>
    public IntPtr Handle => IntPtr.Zero;

    /// <summary>Creates a window with the specified creation parameters (no-op).</summary>
    public virtual void CreateHandle(CreateParams cp) { }

    /// <summary>Destroys the window (no-op).</summary>
    public virtual void DestroyHandle() { }

    /// <summary>Assigns a handle to this window (no-op).</summary>
    public void AssignHandle(IntPtr handle) { }

    /// <summary>Releases the handle (no-op).</summary>
    public virtual void ReleaseHandle() { }

    /// <summary>
    /// Invokes the default window procedure with the specified message.
    /// In CanvasForms no message processing occurs.
    /// </summary>
    protected virtual void WndProc(ref Message m) { }

    /// <summary>Processes the message (no-op).</summary>
    public void DefWndProc(ref Message m) { }

    /// <summary>
    /// Associates a <see cref="NativeWindow"/> with the given handle.
    /// Always returns <c>null</c> in CanvasForms.
    /// </summary>
    public static NativeWindow? FromHandle(IntPtr handle) => null;
}

/// <summary>
/// Contains parameters for creating a window (stub).
/// Provided so code that instantiates <see cref="CreateParams"/> compiles.
/// </summary>
public class CreateParams
{
    public string? Caption  { get; set; }
    public string? ClassName { get; set; }
    public int Style        { get; set; }
    public int ExStyle      { get; set; }
    public int X            { get; set; }
    public int Y            { get; set; }
    public int Width        { get; set; }
    public int Height       { get; set; }
    public IntPtr Parent    { get; set; }
    public int Param        { get; set; }
    public object? Param2   { get; set; }
}
