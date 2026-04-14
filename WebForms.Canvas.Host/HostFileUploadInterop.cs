using Canvas.Windows.Forms;
using Microsoft.JSInterop;

namespace Canvas.Windows.Forms.Host;

internal sealed class HostFileUploadInterop : IHostFileUpload
{
    private readonly IJSRuntime _js;

    public HostFileUploadInterop(IJSRuntime js)
    {
        _js = js;
    }

    public Task<string> UploadFromBrowserAsync(bool multiple, string accept, CancellationToken cancellationToken = default)
    {
        // hostfs-upload.js is loaded globally by the host index.html.
        return _js.InvokeAsync<string>("hostFsUpload.uploadFiles", cancellationToken, multiple, accept).AsTask();
    }
}
