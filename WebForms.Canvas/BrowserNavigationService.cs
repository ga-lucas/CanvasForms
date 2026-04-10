using Microsoft.JSInterop;

namespace Canvas.Windows.Forms;

/// <summary>
/// Provides browser navigation services for controls that need to open URLs
/// </summary>
public static class BrowserNavigationService
{
    /// <summary>
    /// Gets or sets the IJSRuntime instance for JavaScript interop
    /// This is set by the FormManager or host component during initialization
    /// </summary>
    public static IJSRuntime? JSRuntime { get; set; }

    /// <summary>
    /// Opens a URL in a new browser window or tab
    /// </summary>
    /// <param name="url">The URL to open</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task OpenUrlAsync(string url)
    {
        if (JSRuntime == null)
        {
            throw new InvalidOperationException("BrowserNavigationService.JSRuntime has not been initialized");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            await JSRuntime.InvokeVoidAsync("open", url, "_blank");
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            System.Diagnostics.Debug.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }

    /// <summary>
    /// Opens a URL in a new browser window with specific window features
    /// </summary>
    /// <param name="url">The URL to open</param>
    /// <param name="windowName">The name of the window (use "_blank" for a new tab/window)</param>
    /// <param name="windowFeatures">Optional window features (e.g., "width=800,height=600")</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task OpenUrlAsync(string url, string windowName, string? windowFeatures = null)
    {
        if (JSRuntime == null)
        {
            throw new InvalidOperationException("BrowserNavigationService.JSRuntime has not been initialized");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            if (string.IsNullOrEmpty(windowFeatures))
            {
                await JSRuntime.InvokeVoidAsync("open", url, windowName);
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("open", url, windowName, windowFeatures);
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            System.Diagnostics.Debug.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }
}
