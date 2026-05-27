namespace System.Windows.Forms;

/// <summary>
/// Executes an operation on a separate thread and provides events for completion/progress.
/// In CanvasForms the worker runs on a <see cref="System.Threading.Tasks.Task"/> so that
/// the browser (single-threaded) host doesn't deadlock.
/// </summary>
public class BackgroundWorker : System.ComponentModel.Component
{
    private System.Threading.CancellationTokenSource? _cts;
    private bool _isBusy;
    private bool _cancellationPending;

    // ── Options ───────────────────────────────────────────────────────────────
    public bool WorkerReportsProgress { get; set; } = false;
    public bool WorkerSupportsCancellation { get; set; } = false;

    // ── State ─────────────────────────────────────────────────────────────────
    public bool IsBusy => _isBusy;
    public bool CancellationPending => _cancellationPending;

    // ── Events ────────────────────────────────────────────────────────────────
    public event DoWorkEventHandler? DoWork;
    public event ProgressChangedEventHandler? ProgressChanged;
    public event RunWorkerCompletedEventHandler? RunWorkerCompleted;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Starts execution of the background operation.</summary>
    public void RunWorkerAsync() => RunWorkerAsync(null);

    /// <summary>Starts execution of the background operation with the given argument.</summary>
    public void RunWorkerAsync(object? argument)
    {
        if (_isBusy) throw new InvalidOperationException("BackgroundWorker is already running.");

        _isBusy = true;
        _cancellationPending = false;
        _cts = WorkerSupportsCancellation ? new System.Threading.CancellationTokenSource() : null;

        Task.Run(() =>
        {
            object? result = null;
            Exception? error = null;
            bool cancelled = false;

            try
            {
                var args = new DoWorkEventArgs(argument);
                DoWork?.Invoke(this, args);
                result = args.Result;
                cancelled = args.Cancel;
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                _isBusy = false;
                _cancellationPending = false;
                var completedArgs = new RunWorkerCompletedEventArgs(result, error, cancelled);
                RunWorkerCompleted?.Invoke(this, completedArgs);
            }
        });
    }

    /// <summary>Requests cancellation of the pending background operation.</summary>
    public void CancelAsync()
    {
        if (!WorkerSupportsCancellation)
            throw new InvalidOperationException("This BackgroundWorker does not support cancellation.");
        _cancellationPending = true;
        _cts?.Cancel();
    }

    /// <summary>Raises a <see cref="ProgressChanged"/> event.</summary>
    public void ReportProgress(int percentProgress) => ReportProgress(percentProgress, null);

    /// <summary>Raises a <see cref="ProgressChanged"/> event with user state.</summary>
    public void ReportProgress(int percentProgress, object? userState)
    {
        if (!WorkerReportsProgress)
            throw new InvalidOperationException("This BackgroundWorker does not support progress reporting.");
        ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentProgress, userState));
    }
}

// ── Event argument types ──────────────────────────────────────────────────────

public delegate void DoWorkEventHandler(object? sender, DoWorkEventArgs e);
public delegate void RunWorkerCompletedEventHandler(object? sender, RunWorkerCompletedEventArgs e);
public delegate void ProgressChangedEventHandler(object? sender, ProgressChangedEventArgs e);

public class DoWorkEventArgs : CancelEventArgs
{
    public DoWorkEventArgs(object? argument) { Argument = argument; }
    public object? Argument { get; }
    public object? Result   { get; set; }
}

public class RunWorkerCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{
    public RunWorkerCompletedEventArgs(object? result, Exception? error, bool cancelled)
        : base(error, cancelled, null)
    {
        _result = result;
    }

    private readonly object? _result;

    public new object? Result
    {
        get
        {
            if (Error is not null) throw Error;
            if (Cancelled) throw new InvalidOperationException("Operation was cancelled.");
            return _result;
        }
    }
}

public class ProgressChangedEventArgs : EventArgs
{
    public ProgressChangedEventArgs(int progressPercentage, object? userState)
    {
        ProgressPercentage = progressPercentage;
        UserState = userState;
    }

    public int ProgressPercentage { get; }
    public object? UserState { get; }
}
