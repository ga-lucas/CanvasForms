using Canvas.Windows.Forms.Host.Server;
using Canvas.Windows.Forms.Host.Server.Hubs;
using Canvas.Windows.Forms.RemoteProtocol;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<AppRuntime>();
builder.Services.AddSingleton<AppManager>();

var app = builder.Build();

app.UseRouting();
app.UseWebSockets();

// UseBlazorFrameworkFiles augments IWebHostEnvironment.WebRootFileProvider so
// that _framework/* resolves to the WASM project's build output.
// It must be called before UseStaticFiles.
app.UseBlazorFrameworkFiles();

// Build a MIME type provider with explicit mappings for all Blazor/WASM types.
// The default FileExtensionContentTypeProvider already maps .js → text/javascript,
// but .wasm, .blat, and .dat are missing — we add them here.
var mimeProvider = new FileExtensionContentTypeProvider();
mimeProvider.Mappings[".wasm"] = "application/wasm";
mimeProvider.Mappings[".blat"] = "application/octet-stream";
mimeProvider.Mappings[".dat"]  = "application/octet-stream";
mimeProvider.Mappings[".dll"]  = "application/octet-stream";

// Pass the augmented WebRootFileProvider explicitly so UseStaticFiles serves
// _framework/* from the WASM project output (not just the server's wwwroot).
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = app.Environment.WebRootFileProvider,
    ContentTypeProvider = mimeProvider,
    OnPrepareResponse = ctx =>
    {
        // In development, disable caching to ensure fresh files on every request
        if (app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers.Pragma = "no-cache";
            ctx.Context.Response.Headers.Expires = "0";
        }
        else
        {
            // In production, use versioned/fingerprinted URLs with long cache
            var path = ctx.File.PhysicalPath;
            if (path?.Contains("_framework") == true)
            {
                ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            }
        }
    }
});

// Wire up desktop change notifications via SignalR
var runtime = app.Services.GetRequiredService<AppRuntime>();
var hubContext = app.Services.GetRequiredService<IHubContext<CanvasHub>>();

runtime.DesktopChanged += async () =>
{
    try
    {
        var snapshot = runtime.GetSnapshot();
        await hubContext.Clients.All.SendAsync("DesktopChanged", snapshot);
    }
    catch { /* ignore broadcast errors */ }
};

// ============ API Endpoints ============

// ============ Host File System Endpoints (for browser-hosted dialogs) ============

// List roots (drives on Windows, '/' on Linux/macOS)
app.MapGet("/api/hostfs/roots", () =>
{
    try
    {
        if (OperatingSystem.IsWindows())
        {
            var roots = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => d.RootDirectory.FullName)
                .ToArray();
            return Results.Ok(roots);
        }

        return Results.Ok(new[] { Path.GetPathRoot(Environment.CurrentDirectory) ?? "/" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Check existence of a directory
app.MapGet("/api/hostfs/dir-exists", (string path) =>
{
    try
    {
        return Results.Ok(Directory.Exists(path));
    }
    catch
    {
        return Results.Ok(false);
    }
});

// Check existence of a file
app.MapGet("/api/hostfs/file-exists", (string path) =>
{
    try
    {
        return Results.Ok(File.Exists(path));
    }
    catch
    {
        return Results.Ok(false);
    }
});

// List directory entries
app.MapGet("/api/hostfs/list", (string path) =>
{
    try
    {
        if (!Directory.Exists(path))
        {
            return Results.Ok(Array.Empty<object>());
        }

        var dirs = Directory.EnumerateDirectories(path)
            .Select(d => new
            {
                name = Path.GetFileName(d),
                fullPath = d,
                isDirectory = true,
                size = (long?)null
            });

        var files = Directory.EnumerateFiles(path)
            .Select(f =>
            {
                long? size = null;
                try { size = new FileInfo(f).Length; } catch { }
                return new
                {
                    name = Path.GetFileName(f),
                    fullPath = f,
                    isDirectory = false,
                    size
                };
            });

        return Results.Ok(dirs.Concat(files).ToArray());
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Download file contents (for OpenFileDialog.OpenFile in browser)
app.MapGet("/api/hostfs/openread", (string path) =>
{
    try
    {
        if (!File.Exists(path))
        {
            return Results.NotFound();
        }

        var fileName = Path.GetFileName(path);
        return Results.File(File.OpenRead(path), "application/octet-stream", fileName);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Upload one or more files to the host temp folder (browser upload feature)
app.MapPost("/api/hostfs/upload", async (HttpRequest request, IWebHostEnvironment env) =>
{
    try
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest("Expected multipart/form-data");
        }

        var form = await request.ReadFormAsync();
        if (form.Files.Count == 0)
        {
            return Results.BadRequest("No files uploaded");
        }

        var baseDir = Path.Combine(env.ContentRootPath, ".apps", "_uploads");
        Directory.CreateDirectory(baseDir);

        var uploadId = Guid.NewGuid().ToString("N");
        var uploadDir = Path.Combine(baseDir, uploadId);
        Directory.CreateDirectory(uploadDir);

        var saved = new List<object>();
        foreach (var file in form.Files)
        {
            if (file.Length == 0) continue;

            // Prevent traversal, keep just the filename.
            var safeName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(safeName)) continue;

            var destPath = Path.Combine(uploadDir, safeName);
            await using var fs = File.Create(destPath);
            await using var src = file.OpenReadStream();
            await src.CopyToAsync(fs, request.HttpContext.RequestAborted);

            saved.Add(new
            {
                name = safeName,
                fullPath = destPath,
                size = new FileInfo(destPath).Length
            });
        }

        return Results.Ok(new
        {
            uploadId,
            directory = uploadDir,
            files = saved
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Status endpoint
app.MapGet("/api/status", (AppRuntime runtime, AppManager manager) => new
{
    running = runtime.IsRunning,
    currentAppId = runtime.CurrentAppId,
    isNativeApp = runtime.IsNativeApp,
    installedApps = manager.List().Count
});

// List installed apps
app.MapGet("/api/apps", (AppManager manager) => manager.List());

// Get specific app
app.MapGet("/api/apps/{appId}", (string appId, AppManager manager) =>
{
    var app = manager.Get(appId);
    return app != null ? Results.Ok(app) : Results.NotFound();
});

// Install (upload + translate) an app
app.MapPost("/api/apps", async (HttpRequest request, AppManager manager) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart/form-data");
    }

    var form = await request.ReadFormAsync();
    if (form.Files.Count == 0)
    {
        return Results.BadRequest("No files uploaded");
    }

    var name = form["name"].FirstOrDefault() ?? "Unnamed App";

    try
    {
        var app = await manager.InstallAsync(name, form.Files);
        return Results.Ok(app);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Uninstall an app
app.MapDelete("/api/apps/{appId}", (string appId, AppManager manager, AppRuntime runtime) =>
{
    // Stop if it's the running app
    if (runtime.CurrentAppId == appId)
    {
        runtime.Stop();
    }

    return manager.Uninstall(appId)
        ? Results.NoContent()
        : Results.NotFound();
});

// Run an installed app
app.MapPost("/api/apps/{appId}/run", (string appId, AppManager manager, AppRuntime runtime) =>
{
    var app = manager.Get(appId);
    if (app == null)
    {
        return Results.NotFound();
    }

    try
    {
        runtime.RunTranslated(appId, app.EntryAssemblyPath);
        return Results.Ok(new { status = "running", appId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Run the demo (native) app
app.MapPost("/api/demo/run", (AppRuntime runtime) =>
{
    try
    {
        runtime.RunNative("demo", () => new Canvas.Windows.Forms.Samples.InteractiveForm());
        return Results.Ok(new { status = "running", appId = "demo" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Stop the current app
app.MapPost("/api/stop", (AppRuntime runtime) =>
{
    runtime.Stop();
    return Results.Ok(new { status = "stopped" });
});

// Get list of files for an installed app (for WASM loading)
app.MapGet("/api/apps/{appId}/files", (string appId, AppManager manager, IWebHostEnvironment env) =>
{
    var appInfo = manager.Get(appId);
    if (appInfo == null)
    {
        return Results.NotFound();
    }

    var appDir = Path.Combine(env.ContentRootPath, ".apps", appId, "translated");
    if (!Directory.Exists(appDir))
    {
        return Results.NotFound("Translated files not found");
    }

    var files = Directory.GetFiles(appDir, "*.dll")
        .Select(f => Path.GetFileName(f))
        .ToList();

    var entryAssembly = Path.GetFileName(appInfo.EntryAssemblyPath);

    return Results.Ok(new
    {
        appId,
        entryAssembly,
        files
    });
});

// Serve a translated assembly file
app.MapGet("/api/apps/{appId}/files/{fileName}", (string appId, string fileName, AppManager manager, IWebHostEnvironment env) =>
{
    var appInfo = manager.Get(appId);
    if (appInfo == null)
    {
        return Results.NotFound();
    }

    // Security: only allow .dll files, prevent directory traversal
    if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
        fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
    {
        return Results.BadRequest("Invalid file name");
    }

    var filePath = Path.Combine(env.ContentRootPath, ".apps", appId, "translated", fileName);
    if (!File.Exists(filePath))
    {
        return Results.NotFound();
    }

    var bytes = File.ReadAllBytes(filePath);
    return Results.File(bytes, "application/octet-stream", fileName);
});

// SignalR hub for canvas rendering
app.MapHub<CanvasHub>("/hub");

// Fallback to index.html for client-side routing.
// Important: MapFallbackToFile uses default static file options, which can bypass our
// development no-cache headers and leave stale fingerprinted framework scripts in cache.
// Serve index.html ourselves in Development with explicit no-cache headers.
app.MapFallback(async context =>
{
    if (app.Environment.IsDevelopment())
    {
        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
    }

    var fileInfo = app.Environment.WebRootFileProvider.GetFileInfo("index.html");
    if (!fileInfo.Exists)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    context.Response.ContentLength = fileInfo.Length;

    await using var stream = fileInfo.CreateReadStream();
    await stream.CopyToAsync(context.Response.Body, context.RequestAborted);
});

app.Run();
