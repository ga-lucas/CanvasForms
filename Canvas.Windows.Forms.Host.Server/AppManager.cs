using Canvas.Windows.Forms.RemoteProtocol;
using Mono.Cecil;

namespace Canvas.Windows.Forms.Host.Server;

/// <summary>
/// Manages installed (uploaded + translated) apps.
/// </summary>
public sealed class AppManager
{
    private readonly ILogger<AppManager> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly Dictionary<string, InstalledApp> _apps = new();
    private readonly object _lock = new();

    public AppManager(ILogger<AppManager> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;

        // Ensure apps directory exists
        Directory.CreateDirectory(AppsDirectory);

        // Load existing apps
        LoadInstalledApps();
    }

    private string AppsDirectory => Path.Combine(_env.ContentRootPath, ".apps");

    /// <summary>
    /// Lists all installed apps.
    /// </summary>
    public IReadOnlyList<InstalledApp> List()
    {
        lock (_lock)
        {
            return _apps.Values.OrderByDescending(a => a.UploadedAtUtc).ToList();
        }
    }

    /// <summary>
    /// Gets an installed app by ID.
    /// </summary>
    public InstalledApp? Get(string appId)
    {
        lock (_lock)
        {
            return _apps.TryGetValue(appId, out var app) ? app : null;
        }
    }

    /// <summary>
    /// Installs (uploads and translates) a new app.
    /// </summary>
    public async Task<InstalledApp> InstallAsync(string name, IFormFileCollection files)
    {
        var appId = Guid.NewGuid().ToString("N")[..8];
        var appDir = Path.Combine(AppsDirectory, appId);
        var uploadDir = Path.Combine(appDir, "uploaded");
        var translatedDir = Path.Combine(appDir, "translated");

        Directory.CreateDirectory(uploadDir);
        Directory.CreateDirectory(translatedDir);

        _logger.LogInformation("Installing app {AppId} ({Name}) with {Count} files", appId, name, files.Count);

        // Save uploaded files
        var savedFiles = new List<string>();
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(fileName)) continue;

            // Only allow certain extensions
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".exe" && ext != ".dll" && ext != ".pdb" && ext != ".json" && ext != ".config")
            {
                _logger.LogWarning("Skipping file with disallowed extension: {FileName}", fileName);
                continue;
            }

            var destPath = Path.Combine(uploadDir, fileName);
            await using var stream = File.Create(destPath);
            await file.CopyToAsync(stream);
            savedFiles.Add(destPath);
        }

        if (savedFiles.Count == 0)
        {
            Directory.Delete(appDir, recursive: true);
            throw new InvalidOperationException("No valid files uploaded");
        }

        // Translate managed assemblies
        string? entryAssemblyPath = null;
        var exeBaseNames = savedFiles
            .Where(p => p.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            .Select(p => Path.GetFileNameWithoutExtension(p))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var path in savedFiles)
        {
            var fileName = Path.GetFileName(path);
            var outPath = Path.Combine(translatedDir, fileName);

            if (IsManagedAssembly(path))
            {
                TranslateAssembly(path, outPath);

                // Determine entry assembly (prefer .dll matching .exe name)
                if (entryAssemblyPath == null)
                {
                    if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                        exeBaseNames.Contains(Path.GetFileNameWithoutExtension(path)))
                    {
                        entryAssemblyPath = outPath;
                    }
                    else if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        entryAssemblyPath = outPath; // First .dll as fallback
                    }
                }
            }
            else
            {
                // Copy non-managed files (config, native apphost, etc.)
                File.Copy(path, outPath, overwrite: true);
            }
        }

        if (entryAssemblyPath == null)
        {
            Directory.Delete(appDir, recursive: true);
            throw new InvalidOperationException("No managed entry assembly found");
        }

        var app = new InstalledApp(
            AppId: appId,
            Name: name,
            EntryAssemblyPath: entryAssemblyPath,
            UploadedAtUtc: DateTime.UtcNow);

        lock (_lock)
        {
            _apps[appId] = app;
        }

        SaveManifest(app, appDir);

        _logger.LogInformation("Installed app {AppId}: {EntryPath}", appId, entryAssemblyPath);
        return app;
    }

    /// <summary>
    /// Uninstalls an app.
    /// </summary>
    public bool Uninstall(string appId)
    {
        lock (_lock)
        {
            if (!_apps.Remove(appId))
            {
                return false;
            }
        }

        var appDir = Path.Combine(AppsDirectory, appId);
        if (Directory.Exists(appDir))
        {
            try
            {
                Directory.Delete(appDir, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete app directory: {Dir}", appDir);
            }
        }

        _logger.LogInformation("Uninstalled app {AppId}", appId);
        return true;
    }

    private bool IsManagedAssembly(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);

            // Check for MZ header
            if (reader.ReadUInt16() != 0x5A4D) return false;

            // Check for PE header
            stream.Position = 0x3C;
            var peOffset = reader.ReadInt32();
            stream.Position = peOffset;
            if (reader.ReadUInt32() != 0x00004550) return false;

            // Check for CLI header
            stream.Position = peOffset + 24; // Optional header offset
            var magic = reader.ReadUInt16();
            var cliHeaderOffset = magic == 0x20b ? peOffset + 248 : peOffset + 232; // PE32+ vs PE32
            stream.Position = cliHeaderOffset;
            var cliRva = reader.ReadUInt32();

            return cliRva != 0;
        }
        catch
        {
            return false;
        }
    }

    private void TranslateAssembly(string inputPath, string outputPath)
    {
        _logger.LogDebug("Translating {Input} -> {Output}", inputPath, outputPath);

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(inputPath)!);
        resolver.AddSearchDirectory(AppContext.BaseDirectory);

        var readerParams = new ReaderParameters
        {
            ReadWrite = false,
            ReadSymbols = File.Exists(Path.ChangeExtension(inputPath, ".pdb")),
            AssemblyResolver = resolver
        };

        using var module = ModuleDefinition.ReadModule(inputPath, readerParams);

        // Retarget System.Windows.Forms references to Canvas.Windows.Forms
        foreach (var reference in module.AssemblyReferences)
        {
            if (reference.Name == "System.Windows.Forms" ||
                reference.Name == "System.Windows.Forms.Primitives")
            {
                reference.Name = "Canvas.Windows.Forms";
            }
        }

        var writerParams = new WriterParameters
        {
            WriteSymbols = readerParams.ReadSymbols
        };

        module.Write(outputPath, writerParams);
    }

    private void SaveManifest(InstalledApp app, string appDir)
    {
        var manifestPath = Path.Combine(appDir, "manifest.json");
        var json = System.Text.Json.JsonSerializer.Serialize(app);
        File.WriteAllText(manifestPath, json);
    }

    private void LoadInstalledApps()
    {
        if (!Directory.Exists(AppsDirectory)) return;

        foreach (var appDir in Directory.GetDirectories(AppsDirectory))
        {
            var manifestPath = Path.Combine(appDir, "manifest.json");
            if (!File.Exists(manifestPath)) continue;

            try
            {
                var json = File.ReadAllText(manifestPath);
                var app = System.Text.Json.JsonSerializer.Deserialize<InstalledApp>(json);
                if (app != null && File.Exists(app.EntryAssemblyPath))
                {
                    _apps[app.AppId] = app;
                    _logger.LogInformation("Loaded installed app: {AppId} ({Name})", app.AppId, app.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load app manifest: {Path}", manifestPath);
            }
        }
    }
}
