using Microsoft.AspNetCore.SignalR;
using Canvas.Windows.Forms.RemoteProtocol;

namespace Canvas.Windows.Forms.Host.Server.Hubs;

/// <summary>
/// SignalR hub for canvas rendering and desktop state.
/// </summary>
public sealed class CanvasHub : Hub
{
    private readonly AppRuntime _runtime;
    private readonly ILogger<CanvasHub> _logger;

    public CanvasHub(AppRuntime runtime, ILogger<CanvasHub> logger)
    {
        _runtime = runtime;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Gets the current desktop state.
    /// </summary>
    public DesktopSnapshot GetDesktop()
    {
        return _runtime.GetSnapshot();
    }

    /// <summary>
    /// Renders the current form.
    /// </summary>
    public RenderFrame Render()
    {
        return _runtime.Render();
    }

    /// <summary>
    /// Renders a specific form by ID.
    /// </summary>
    public RenderFrame RenderForm(string formId)
    {
        // For now we only have one form, but this supports future multi-form
        return _runtime.Render();
    }

    /// <summary>
    /// Moves a form to a new position.
    /// </summary>
    public void MoveForm(string formId, int left, int top)
    {
        _runtime.MoveForm(formId, left, top);
    }

    /// <summary>
    /// Resizes a form.
    /// </summary>
    public void ResizeForm(string formId, int left, int top, int width, int height)
    {
        _runtime.ResizeForm(formId, left, top, width, height);
    }

    /// <summary>
    /// Minimizes a form.
    /// </summary>
    public void MinimizeForm(string formId)
    {
        _runtime.MinimizeForm(formId);
    }

    /// <summary>
    /// Maximizes or restores a form.
    /// </summary>
    public void MaximizeForm(string formId, int desktopWidth, int desktopHeight)
    {
        _runtime.MaximizeForm(formId, desktopWidth, desktopHeight);
    }

    /// <summary>
    /// Activates (focuses) a form.
    /// </summary>
    public void ActivateForm(string formId)
    {
        _runtime.ActivateForm(formId);
    }

    /// <summary>
    /// Closes a form.
    /// </summary>
    public void CloseForm(string formId)
    {
        _runtime.CloseForm(formId);
    }

    /// <summary>
    /// Sends a mouse event to a form.
    /// </summary>
    public async Task MouseEvent(string formId, string eventType, int x, int y, int button)
    {
        _runtime.SendMouseEvent(eventType, x, y, button);

        // Send updated render after input
        var frame = _runtime.Render();
        await Clients.Caller.SendAsync("RenderFrame", frame);
    }

    /// <summary>
    /// Sends a keyboard event to the current app.
    /// </summary>
    public async Task KeyEvent(string eventType, int keyCode, bool alt, bool ctrl, bool shift, char keyChar)
    {
        _runtime.SendKeyEvent(eventType, keyCode, alt, ctrl, shift, keyChar);

        // Send updated render after input
        var frame = _runtime.Render();
        await Clients.Caller.SendAsync("RenderFrame", frame);
    }

    /// <summary>
    /// Pings the server (keepalive).
    /// </summary>
    public Task Ping() => Task.CompletedTask;
}
