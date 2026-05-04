using System.Collections.Concurrent;
using System.Data.Common;
using System.Windows.Forms;
// Aliases to resolve ambiguities between System.Data and System.Windows.Forms re-exports
using WfDataTable = System.Windows.Forms.DataTable;
using WfDataRow   = System.Windows.Forms.DataRow;
using DBNull      = System.DBNull;

namespace Canvas.Windows.Forms.Host.Server.Data;

/// <summary>
/// Server-side implementation of <see cref="ICanvasDataService"/>.
/// Holds a live <see cref="ConcurrentDictionary{TKey,TValue}"/> of named
/// connection factories so that apps can register providers at runtime via
/// <see cref="ICanvasDataServiceRegistrar"/> (surfaced through
/// <see cref="CanvasDataService.RegisterProvider"/>).
/// </summary>
public sealed class ServerCanvasDataService : ICanvasDataService, ICanvasDataServiceRegistrar
{
    // ── Connection registry ───────────────────────────────────────────────────

    // "host" entries survive app restarts; "app" entries are cleared on Stop().
    private readonly ConcurrentDictionary<string, Func<DbConnection>> _hostFactories = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Func<DbConnection>> _appFactories  = new(StringComparer.OrdinalIgnoreCase);

    private readonly CanvasProviderResolver _resolver;

    public ServerCanvasDataService(CanvasProviderResolver resolver)
    {
        _resolver = resolver;
    }

    // ── Host-level registration (called from Program.cs / DI setup) ───────────

    /// <summary>
    /// Registers a host-level named connection that survives app restarts.
    /// Typically called once at server startup for the "Default" SQLite DB.
    /// </summary>
    public void RegisterHostProvider(string name, Func<DbConnection> factory)
        => _hostFactories[name] = factory;

    // ── ICanvasDataServiceRegistrar (called by app code via CanvasDataService) ─

    public void RegisterProvider(string name, string providerName, string connectionString)
    {
        var factory = _resolver.Resolve(providerName, connectionString);
        _appFactories[name] = factory;
    }

    public void RegisterProvider(string name, object factory)
    {
        if (factory is Func<DbConnection> typed)
            _appFactories[name] = typed;
        else
            throw new ArgumentException(
                $"factory must be a Func<DbConnection>, got {factory?.GetType().FullName ?? "null"}.",
                nameof(factory));
    }

    public void UnregisterProvider(string name)
    {
        if (string.Equals(name, "Default", StringComparison.OrdinalIgnoreCase) && _hostFactories.ContainsKey("Default"))
            throw new InvalidOperationException("Cannot remove the built-in 'Default' host connection.");
        _appFactories.TryRemove(name, out _);
    }

    public IReadOnlyList<string> GetRegisteredProviders()
        => _hostFactories.Keys.Concat(_appFactories.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>
    /// Clears all app-registered providers.  Called by <c>AppRuntime.Stop()</c>
    /// so the next app starts with a clean slate (host providers remain).
    /// </summary>
    public void ClearAppProviders() => _appFactories.Clear();

    // ── Factory resolution (app overrides host) ───────────────────────────────

    private Func<DbConnection> GetFactory(string connectionName)
    {
        // App-registered providers take precedence over host providers
        if (_appFactories.TryGetValue(connectionName, out var appFactory))
            return appFactory;
        if (_hostFactories.TryGetValue(connectionName, out var hostFactory))
            return hostFactory;

        var available = GetRegisteredProviders();
        throw new InvalidOperationException(
            $"No data connection named '{connectionName}' is registered. " +
            $"Available: {string.Join(", ", available)}");
    }

    // ── ICanvasDataService ────────────────────────────────────────────────────

    public void Fill(WfDataTable table, string sql, params (string name, object? value)[] parameters)
    {
        using var ctx = OpenContext();
        ctx.Fill(table, sql, parameters);
    }

    public async Task FillAsync(WfDataTable table, string sql, params (string name, object? value)[] parameters)
    {
        await using var ctx = (ServerDbContext)OpenContext();
        await ctx.FillAsync(table, sql, parameters);
    }

    public ICanvasDbContext OpenContext(string connectionName = "Default")
    {
        var factory = GetFactory(connectionName);
        var conn = factory();
        conn.Open();
        return new ServerDbContext(conn);
    }

    public IReadOnlyList<string> GetTableNames(string connectionName = "Default")
    {
        using var ctx = (ServerDbContext)OpenContext(connectionName);
        var table = new WfDataTable();
        ctx.Fill(table, "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name");
        return table.Rows.Cast<WfDataRow>().Select(r => r[0]?.ToString() ?? string.Empty).ToList();
    }

    public IReadOnlyList<CanvasColumnInfo> GetColumns(string tableName, string connectionName = "Default")
    {
        using var ctx = (ServerDbContext)OpenContext(connectionName);
        var table = new WfDataTable();
        ctx.Fill(table, $"PRAGMA table_info(\"{tableName}\")");
        return table.Rows.Cast<WfDataRow>().Select(r => new CanvasColumnInfo(
            r["name"]?.ToString() ?? "",
            r["type"]?.ToString() ?? "",
            r["notnull"]?.ToString() == "0",
            null)).ToList();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
//  ServerDbContext — ICanvasDbContext wrapping a single DbConnection
// ──────────────────────────────────────────────────────────────────────────────

internal sealed class ServerDbContext : ICanvasDbContext, IAsyncDisposable
{
    private readonly DbConnection _conn;
    private DbTransaction? _tx;

    internal ServerDbContext(DbConnection conn)
    {
        _conn = conn;
    }

    // ── Fill ──────────────────────────────────────────────────────────────────

    public void Fill(WfDataTable table, string sql, params (string name, object? value)[] parameters)
    {
        table.Rows.Clear();

        using var cmd = BuildCommand(sql, parameters);
        using var reader = cmd.ExecuteReader();

        // Build columns from schema on first fill
        if (table.Columns.Count == 0)
            BuildColumns(table, reader);

        while (reader.Read())
        {
            var row = table.NewRow();
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            table.Rows.Add(row);
        }

        table.AcceptChanges();
    }

    public async Task FillAsync(WfDataTable table, string sql, params (string name, object? value)[] parameters)
    {
        table.Rows.Clear();

        await using var cmd = BuildCommand(sql, parameters);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (table.Columns.Count == 0)
            BuildColumns(table, reader);

        while (await reader.ReadAsync())
        {
            var row = table.NewRow();
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            table.Rows.Add(row);
        }

        table.AcceptChanges();
    }

    // ── Execute / Scalar ──────────────────────────────────────────────────────

    public int Execute(string sql, params (string name, object? value)[] parameters)
    {
        using var cmd = BuildCommand(sql, parameters);
        return cmd.ExecuteNonQuery();
    }

    public async Task<int> ExecuteAsync(string sql, params (string name, object? value)[] parameters)
    {
        await using var cmd = BuildCommand(sql, parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    public object? Scalar(string sql, params (string name, object? value)[] parameters)
    {
        using var cmd = BuildCommand(sql, parameters);
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? null : result;
    }

    public async Task<object?> ScalarAsync(string sql, params (string name, object? value)[] parameters)
    {
        await using var cmd = BuildCommand(sql, parameters);
        var result = await cmd.ExecuteScalarAsync();
        return result == DBNull.Value ? null : result;
    }

    // ── Transactions ──────────────────────────────────────────────────────────

    public void BeginTransaction() => _tx = _conn.BeginTransaction();
    public void Commit() { _tx?.Commit(); _tx = null; }
    public void Rollback() { _tx?.Rollback(); _tx = null; }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private DbCommand BuildCommand(string sql, (string name, object? value)[] parameters)
    {
        var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        if (_tx != null) cmd.Transaction = _tx;
        foreach (var (name, value) in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        return cmd;
    }

    private static void BuildColumns(WfDataTable table, DbDataReader reader)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var col = table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            col.AllowDBNull = true;
        }
    }

    // ── Disposal ──────────────────────────────────────────────────────────────

    public void Dispose()
    {
        _tx?.Dispose();
        _conn.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_tx != null) await _tx.DisposeAsync();
        await _conn.DisposeAsync();
    }
}
