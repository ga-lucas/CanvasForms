namespace System.Windows.Forms;

// ── Timer ─────────────────────────────────────────────────────────────────────
/// <summary>
/// Implements a timer that raises <see cref="Tick"/> at a fixed <see cref="Interval"/>
/// while <see cref="Enabled"/> is <see langword="true"/>.
///
/// API-compatible with <c>System.Windows.Forms.Timer</c>. Because Blazor WASM runs
/// on a single thread the Tick handler fires on the same synchronisation context that
/// was current when the timer was constructed or started — identical to the WinForms
/// behaviour of dispatching to the UI thread.
/// </summary>
public sealed class Timer : IDisposable
{
    // ── Fields ────────────────────────────────────────────────────────────────

    private int                        _interval     = 100;   // milliseconds
    private bool                       _enabled;
    private bool                       _disposed;
    private CancellationTokenSource?   _cts;
    private readonly SynchronizationContext? _syncCtx;

    // ── Constructors ──────────────────────────────────────────────────────────

    /// <summary>Initialises a new <see cref="Timer"/> with a 100 ms interval.</summary>
    public Timer()
    {
        // Capture the Blazor dispatcher sync context so Tick always runs on the UI thread.
        _syncCtx = SynchronizationContext.Current;
    }

    /// <summary>
    /// Initialises a new <see cref="Timer"/> owned by the specified
    /// <see cref="System.ComponentModel.IContainer"/> (for designer compatibility).
    /// </summary>
    public Timer(System.ComponentModel.IContainer container) : this()
    {
        container?.Add(new TimerComponent(this));
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the time (in milliseconds) between <see cref="Tick"/> events.
    /// Must be greater than zero. Default is 100.
    /// Matches WinForms <c>Timer.Interval</c>.
    /// </summary>
    public int Interval
    {
        get => _interval;
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value),
                "Interval must be greater than zero.");
            if (_interval == value) return;
            _interval = value;

            // Restart with the new interval if currently running.
            if (_enabled)
            {
                StopLoop();
                StartLoop();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the timer is currently running.
    /// Setting to <see langword="true"/> starts the timer; <see langword="false"/> stops it.
    /// Matches WinForms <c>Timer.Enabled</c>.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_disposed) return;
            if (value == _enabled) return;
            _enabled = value;
            if (_enabled)
                StartLoop();
            else
                StopLoop();
        }
    }

    /// <summary>
    /// Gets or sets an arbitrary object associated with this timer.
    /// Matches WinForms <c>Timer.Tag</c>.
    /// </summary>
    public object? Tag { get; set; }

    // ── Events ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Raised on each tick of the timer while <see cref="Enabled"/> is true.
    /// Matches WinForms <c>Timer.Tick</c>.
    /// </summary>
    public event EventHandler? Tick;

    // ── Public methods ────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the timer (sets <see cref="Enabled"/> to <see langword="true"/>).
    /// Matches WinForms <c>Timer.Start()</c>.
    /// </summary>
    public void Start()  => Enabled = true;

    /// <summary>
    /// Stops the timer (sets <see cref="Enabled"/> to <see langword="false"/>).
    /// Matches WinForms <c>Timer.Stop()</c>.
    /// </summary>
    public void Stop()   => Enabled = false;

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <summary>
    /// Stops the timer and releases all resources.
    /// Matches WinForms <c>Timer.Dispose()</c>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _enabled  = false;
        StopLoop();
        Tick = null;
    }

    // ── Private implementation ────────────────────────────────────────────────

    private void StartLoop()
    {
        _cts = new CancellationTokenSource();
        var token    = _cts.Token;
        var interval = _interval;
        var syncCtx  = _syncCtx;

        // Run the periodic loop as a fire-and-forget background Task.
        // In WASM this is cooperative; in server-side Blazor it runs on the thread pool
        // and Posts back to the captured sync context for each Tick.
        _ = Task.Run(async () =>
        {
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));
            try
            {
                while (await periodicTimer.WaitForNextTickAsync(token))
                {
                    if (token.IsCancellationRequested) break;
                    FireTick(syncCtx);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal stop — swallow.
            }
        });
    }

    private void StopLoop()
    {
        var cts = _cts;
        _cts = null;
        cts?.Cancel();
        cts?.Dispose();
    }

    private void FireTick(SynchronizationContext? syncCtx)
    {
        if (syncCtx is not null)
        {
            // Post to UI thread — non-blocking, matches WinForms message-queue dispatch.
            syncCtx.Post(_ => RaiseTick(), null);
        }
        else
        {
            RaiseTick();
        }
    }

    private void RaiseTick()
    {
        if (!_enabled || _disposed) return;
        Tick?.Invoke(this, EventArgs.Empty);
    }

    // ── Designer component wrapper ─────────────────────────────────────────────

    /// <summary>
    /// Thin <see cref="System.ComponentModel.IComponent"/> wrapper so a <see cref="Timer"/>
    /// can be added to an <see cref="System.ComponentModel.IContainer"/> by the designer.
    /// </summary>
    private sealed class TimerComponent : System.ComponentModel.IComponent
    {
        private readonly Timer _timer;
        public TimerComponent(Timer timer) => _timer = timer;
        public System.ComponentModel.ISite? Site { get; set; }
        public event EventHandler? Disposed;
        public void Dispose()
        {
            _timer.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
        }
    }
}
