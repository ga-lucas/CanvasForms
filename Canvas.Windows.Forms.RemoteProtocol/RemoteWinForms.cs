namespace Canvas.Windows.Forms.RemoteProtocol;

public sealed record FormSnapshot(
    string Id,
    string Text,
    int Left,
    int Top,
    int Width,
    int Height,
    int ZIndex,
    bool Visible,
    bool IsMinimized,
    bool IsMaximized,
    string BackColorHex);

public sealed record RenderFrame(
    string FormId,
    int BorderWidth,
    int TitleBarHeightWithBorder,
    int ClientWidth,
    int ClientHeight,
    object[][] Commands);

public sealed record DesktopSnapshot(
    FormSnapshot[] Forms,
    string? ActiveFormId);

public sealed record UploadedApp(
    string AppId,
    string EntryAssemblyPath,
    string[] Files);

public sealed record InstalledApp(
    string AppId,
    string Name,
    string EntryAssemblyPath,
    DateTime UploadedAtUtc);
