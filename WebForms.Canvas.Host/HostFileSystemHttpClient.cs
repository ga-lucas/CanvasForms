using System.Net.Http.Json;
using Canvas.Windows.Forms;

namespace Canvas.Windows.Forms.Host;

internal sealed class HostFileSystemHttpClient : IHostFileSystem
{
    private readonly HttpClient _http;

    public HostFileSystemHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string[]> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<string[]>("api/hostfs/roots", cancellationToken)
            ?? Array.Empty<string>();
    }

    public async Task<HostFileSystemEntry[]> ListAsync(string path, CancellationToken cancellationToken = default)
    {
        var url = $"api/hostfs/list?path={Uri.EscapeDataString(path ?? string.Empty)}";
        return await _http.GetFromJsonAsync<HostFileSystemEntry[]>(url, cancellationToken)
            ?? Array.Empty<HostFileSystemEntry>();
    }

    public async Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var url = $"api/hostfs/dir-exists?path={Uri.EscapeDataString(path ?? string.Empty)}";
        return await _http.GetFromJsonAsync<bool>(url, cancellationToken);
    }

    public async Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var url = $"api/hostfs/file-exists?path={Uri.EscapeDataString(path ?? string.Empty)}";
        return await _http.GetFromJsonAsync<bool>(url, cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var url = $"api/hostfs/openread?path={Uri.EscapeDataString(path ?? string.Empty)}";
        var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
