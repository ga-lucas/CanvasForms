namespace System.Windows.Forms;

/// <summary>
/// Provides a <see cref="System.Threading.SynchronizationContext"/> for Windows Forms
/// applications.  In CanvasForms there is no Win32 message pump; this subclass
/// delegates to the default thread-pool context so that translated apps that call
/// <c>SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext())</c>
/// still compile and run without error.
/// </summary>
public sealed class WindowsFormsSynchronizationContext : System.Threading.SynchronizationContext
{
    public override void Post(System.Threading.SendOrPostCallback d, object? state)
        => System.Threading.ThreadPool.QueueUserWorkItem(_ => d(state));

    public override void Send(System.Threading.SendOrPostCallback d, object? state)
        => d(state);

    /// <summary>
    /// Installs this context as the current synchronization context for the calling thread.
    /// In CanvasForms this is a no-op — the host manages its own async scheduler.
    /// </summary>
    public static void AutoInstall() { }
}
