using System.Net.Http;

namespace Canvas.Windows.Forms;

public sealed record HostFileSystemEntry(
    string Name,
    string FullPath,
    bool IsDirectory,
    long? Size);

public interface IHostFileSystem
{
    Task<string[]> GetRootsAsync(CancellationToken cancellationToken = default);
    Task<HostFileSystemEntry[]> ListAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default);
}

public interface IHostFileUpload
{
    /// <summary>
    /// Opens the browser's file picker, uploads selected files to the host/server, and returns the server JSON response.
    /// </summary>
    Task<string> UploadFromBrowserAsync(bool multiple, string accept, CancellationToken cancellationToken = default);
}

/// <summary>
/// Host file-system abstraction for browser/WASM execution.
/// The UI runs in the browser, but filesystem access is proxied to the host/server.
/// </summary>
public static class HostFileSystem
{
    public static IHostFileSystem? Current { get; set; }
}
