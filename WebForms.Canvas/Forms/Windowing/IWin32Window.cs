namespace System.Windows.Forms;

/// <summary>
/// Minimal IWin32Window abstraction for WinForms API compatibility.
/// Canvas.Windows.Forms does not use native OS window handles, but many APIs accept an owner.
/// </summary>
public interface IWin32Window
{
    IntPtr Handle { get; }
}
