namespace System.Windows.Forms;

// ──────────────────────────────────────────────────────────────────────────────
//  ICanvasDataService
//
//  Server-side data access abstraction exposed to canvas-layer WinForms code.
//  The canvas project (WASM-capable RCL) only holds this interface + the ambient
//  static accessor — no ADO.NET package is referenced here.  The server project
//  provides the concrete implementation and assigns CanvasDataService.Current
//  at startup (the same pattern as HostFileSystem.Current).
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Provides server-side data access to canvas-rendered WinForms apps.
/// Both native apps and translated (ILTranslated) apps consume this interface
/// through the <see cref="CanvasDataService.Current"/> ambient accessor.
/// </summary>
public interface ICanvasDataService
{
    // ── DataTable fill (WinForms DataAdapter pattern) ─────────────────────────

    /// <summary>
    /// Executes <paramref name="sql"/> against the default connection and fills
    /// <paramref name="table"/> with the result set.  Column schema is inferred
    /// automatically; existing rows are cleared first.
    /// </summary>
    void Fill(DataTable table, string sql, params (string name, object? value)[] parameters);

    /// <summary>
    /// Asynchronously executes <paramref name="sql"/> and fills <paramref name="table"/>.
    /// </summary>
    Task FillAsync(DataTable table, string sql, params (string name, object? value)[] parameters);

    // ── Named connection support ──────────────────────────────────────────────

    /// <summary>
    /// Returns a scoped <see cref="ICanvasDbContext"/> for the named connection
    /// string key.  Use this when you need multiple data sources or explicit
    /// transaction control.
    /// </summary>
    ICanvasDbContext OpenContext(string connectionName = "Default");

    // ── Schema helpers ────────────────────────────────────────────────────────

    /// <summary>Returns the names of all tables in the default database.</summary>
    IReadOnlyList<string> GetTableNames(string connectionName = "Default");

    /// <summary>Returns column metadata for <paramref name="tableName"/>.</summary>
    IReadOnlyList<CanvasColumnInfo> GetColumns(string tableName, string connectionName = "Default");
}

// ──────────────────────────────────────────────────────────────────────────────
//  ICanvasDbContext — scoped ADO.NET-style context
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A scoped data context returned by <see cref="ICanvasDataService.OpenContext"/>.
/// Wraps a single database connection and provides DataTable-fill + scalar helpers.
/// Dispose to release the underlying connection.
/// </summary>
public interface ICanvasDbContext : IDisposable
{
    void Fill(DataTable table, string sql, params (string name, object? value)[] parameters);
    Task FillAsync(DataTable table, string sql, params (string name, object? value)[] parameters);

    int Execute(string sql, params (string name, object? value)[] parameters);
    Task<int> ExecuteAsync(string sql, params (string name, object? value)[] parameters);

    object? Scalar(string sql, params (string name, object? value)[] parameters);
    Task<object?> ScalarAsync(string sql, params (string name, object? value)[] parameters);

    void BeginTransaction();
    void Commit();
    void Rollback();
}

// ──────────────────────────────────────────────────────────────────────────────
//  CanvasColumnInfo — lightweight column metadata
// ──────────────────────────────────────────────────────────────────────────────

public sealed record CanvasColumnInfo(
    string Name,
    string DataTypeName,
    bool IsNullable,
    int? MaxLength);

// ──────────────────────────────────────────────────────────────────────────────
//  CanvasDataService — ambient accessor (mirrors HostFileSystem.Current)
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Ambient accessor for the server-backed data service.
/// <para>
/// The server assigns <see cref="Current"/> during startup.  Both native apps
/// and translated (ILTranslated) apps read it through this static property —
/// no constructor injection is needed, so translated apps require zero code
/// changes to access server-side data sources.
/// </para>
/// <para>
/// Apps can register additional named connections at runtime using one of the
/// <c>RegisterProvider</c> overloads.  The server resolves the actual
/// <c>DbConnection</c> factory; the canvas layer only passes strings or opaque
/// factory objects so that the WASM-capable RCL has no ADO.NET dependency.
/// </para>
/// <example>
/// <code>
/// // Works in native and translated apps:
/// var table = new DataTable();
/// CanvasDataService.Current?.Fill(table, "SELECT * FROM Products");
/// dataGridView1.DataSource = table;
///
/// // Register a named provider (string-based — no ADO.NET types needed):
/// CanvasDataService.RegisterProvider("Sales", "Sqlite", "Data Source=sales.db");
///
/// // Open a named context:
/// using var ctx = CanvasDataService.Current?.OpenContext("Sales");
/// ctx?.Fill(table, "SELECT * FROM Orders");
/// </code>
/// </example>
/// </summary>
public static class CanvasDataService
{
    /// <summary>
    /// The active server-side data service.  Set by the server host at startup.
    /// Null when running outside a host that provides data services (e.g., pure
    /// browser-only execution without a data backend).
    /// </summary>
    public static ICanvasDataService? Current { get; set; }

    // ── Dynamic registration API ──────────────────────────────────────────────

    /// <summary>
    /// Registers a named connection using a provider name string and connection
    /// string.  The server resolves the appropriate <c>DbConnection</c> factory.
    /// <para>
    /// Supported provider names: <c>"Sqlite"</c>, <c>"SqlServer"</c>,
    /// <c>"Postgres"</c> (case-insensitive).
    /// </para>
    /// </summary>
    /// <param name="name">Connection name, e.g. <c>"Sales"</c> or <c>"Default"</c>.</param>
    /// <param name="providerName">Provider identifier string.</param>
    /// <param name="connectionString">ADO.NET connection string for the provider.</param>
    public static void RegisterProvider(string name, string providerName, string connectionString)
        => (Current as ICanvasDataServiceRegistrar)?.RegisterProvider(name, providerName, connectionString);

    /// <summary>
    /// Registers a named connection using an opaque factory object.
    /// For native apps only: pass a <c>Func&lt;DbConnection&gt;</c> boxed as <c>object</c>.
    /// The server-side implementation casts it back to the correct delegate type.
    /// </summary>
    /// <param name="name">Connection name.</param>
    /// <param name="factory">A <c>Func&lt;DbConnection&gt;</c> boxed as <c>object</c>.</param>
    public static void RegisterProvider(string name, object factory)
        => (Current as ICanvasDataServiceRegistrar)?.RegisterProvider(name, factory);

    /// <summary>
    /// Removes a previously registered named connection.
    /// The built-in <c>"Default"</c> host connection cannot be removed.
    /// </summary>
    public static void UnregisterProvider(string name)
        => (Current as ICanvasDataServiceRegistrar)?.UnregisterProvider(name);

    /// <summary>
    /// Returns the names of all currently registered connections.
    /// </summary>
    public static IReadOnlyList<string> GetRegisteredProviders()
        => (Current as ICanvasDataServiceRegistrar)?.GetRegisteredProviders()
           ?? Array.Empty<string>();
}

// ──────────────────────────────────────────────────────────────────────────────
//  ICanvasDataServiceRegistrar — implemented server-side, called via ambient cast
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Extended interface implemented by the server-side data service to support
/// runtime provider registration.  The canvas layer calls it by casting
/// <see cref="CanvasDataService.Current"/> to this interface internally.
/// App code should use the static helpers on <see cref="CanvasDataService"/>
/// rather than this interface directly.
/// </summary>
public interface ICanvasDataServiceRegistrar
{
    void RegisterProvider(string name, string providerName, string connectionString);
    void RegisterProvider(string name, object factory);
    void UnregisterProvider(string name);
    IReadOnlyList<string> GetRegisteredProviders();
    void ClearAppProviders();
}
