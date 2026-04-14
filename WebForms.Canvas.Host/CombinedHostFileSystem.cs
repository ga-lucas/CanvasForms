using Canvas.Windows.Forms;

namespace Canvas.Windows.Forms.Host;

internal sealed class CombinedHostFileSystem : IHostFileSystem, IHostFileUpload
{
    private readonly IHostFileSystem _fs;
    private readonly IHostFileUpload _upload;

    public CombinedHostFileSystem(IHostFileSystem fs, IHostFileUpload upload)
    {
        _fs = fs;
        _upload = upload;
    }

    public Task<string[]> GetRootsAsync(CancellationToken cancellationToken = default) =>
        _fs.GetRootsAsync(cancellationToken);

    public Task<HostFileSystemEntry[]> ListAsync(string path, CancellationToken cancellationToken = default) =>
        _fs.ListAsync(path, cancellationToken);

    public Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default) =>
        _fs.DirectoryExistsAsync(path, cancellationToken);

    public Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default) =>
        _fs.FileExistsAsync(path, cancellationToken);

    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default) =>
        _fs.OpenReadAsync(path, cancellationToken);

    public Task<string> UploadFromBrowserAsync(bool multiple, string accept, CancellationToken cancellationToken = default) =>
        _upload.UploadFromBrowserAsync(multiple, accept, cancellationToken);
}
