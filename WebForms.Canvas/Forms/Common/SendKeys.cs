namespace System.Windows.Forms;

/// <summary>
/// Provides methods for sending keystrokes to the active application.
/// In CanvasForms keyboard input originates from the browser and cannot be synthesized
/// programmatically.  All methods are safe no-ops provided for API compatibility.
/// </summary>
public static class SendKeys
{
    /// <summary>Sends keystrokes to the active application (no-op in CanvasForms).</summary>
    public static void Send(string keys) { }

    /// <summary>
    /// Sends keystrokes to the active application and waits for the messages to be
    /// processed (no-op in CanvasForms).
    /// </summary>
    public static void SendWait(string keys) { }

    /// <summary>Processes all Windows messages currently in the message queue (no-op).</summary>
    public static void Flush() { }
}
