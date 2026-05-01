using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Canvas.Windows.Forms.Host.Server.Data;

/// <summary>
/// Resolves a provider name string + connection string into a
/// <see cref="DbConnection"/> factory.
/// <para>
/// Built-in providers (case-insensitive):
/// <list type="bullet">
///   <item><c>"Sqlite"</c> — Microsoft.Data.Sqlite (always available)</item>
///   <item><c>"SqlServer"</c> — Microsoft.Data.SqlClient (if package is referenced)</item>
///   <item><c>"Postgres"</c> / <c>"PostgreSQL"</c> — Npgsql (if package is referenced)</item>
/// </list>
/// Additional providers can be registered at startup via <see cref="Register"/>.
/// </para>
/// </summary>
public sealed class CanvasProviderResolver
{
    // name → factory-builder: given a connection string, returns a Func<DbConnection>
    private readonly Dictionary<string, Func<string, Func<DbConnection>>> _builders =
        new(StringComparer.OrdinalIgnoreCase);

    public CanvasProviderResolver()
    {
        // ── Built-in: SQLite ─────────────────────────────────────────────────
        _builders["Sqlite"] = cs => () => new SqliteConnection(cs);

        // ── Built-in: SQL Server (reflection — optional package) ─────────────
        _builders["SqlServer"]  = cs => ReflectionFactory("Microsoft.Data.SqlClient", "Microsoft.Data.SqlClient.SqlConnection", cs);
        _builders["MsSql"]      = _builders["SqlServer"];
        _builders["SqlClient"]  = _builders["SqlServer"];

        // ── Built-in: PostgreSQL (reflection — optional package) ─────────────
        _builders["Postgres"]   = cs => ReflectionFactory("Npgsql", "Npgsql.NpgsqlConnection", cs);
        _builders["PostgreSQL"] = _builders["Postgres"];
        _builders["Npgsql"]     = _builders["Postgres"];

        // ── Built-in: MySQL (reflection — optional package) ──────────────────
        _builders["MySQL"]      = cs => ReflectionFactory("MySqlConnector", "MySqlConnector.MySqlConnection", cs);
        _builders["MariaDB"]    = _builders["MySQL"];
    }

    /// <summary>
    /// Registers a custom provider.
    /// </summary>
    /// <param name="providerName">Case-insensitive provider key, e.g. <c>"MyDb"</c>.</param>
    /// <param name="builder">
    /// Delegate that takes a connection string and returns a factory
    /// <c>Func&lt;DbConnection&gt;</c>.
    /// </param>
    public void Register(string providerName, Func<string, Func<DbConnection>> builder)
        => _builders[providerName] = builder;

    /// <summary>
    /// Resolves a provider name + connection string into a connection factory.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the provider name is unknown or the required package is not loaded.
    /// </exception>
    public Func<DbConnection> Resolve(string providerName, string connectionString)
    {
        if (!_builders.TryGetValue(providerName, out var builder))
        {
            var known = string.Join(", ", _builders.Keys.Order());
            throw new InvalidOperationException(
                $"Unknown data provider '{providerName}'. " +
                $"Known providers: {known}. " +
                $"Register additional providers via CanvasProviderResolver.Register().");
        }

        return builder(connectionString);
    }

    // ── Reflection-based factory for optional packages ────────────────────────

    private static Func<DbConnection> ReflectionFactory(
        string assemblyName, string typeName, string connectionString)
    {
        return () =>
        {
            Type? type = null;

            // Try already-loaded assemblies first
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(typeName);
                if (type != null) break;
            }

            // Fallback: try to load by assembly name
            if (type == null)
            {
                try
                {
                    var asm = System.Reflection.Assembly.Load(assemblyName);
                    type = asm.GetType(typeName);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Could not load ADO.NET provider '{typeName}' from assembly '{assemblyName}'. " +
                        $"Add a NuGet package reference for this provider. Inner: {ex.Message}", ex);
                }
            }

            if (type == null)
                throw new InvalidOperationException(
                    $"Type '{typeName}' not found in assembly '{assemblyName}'.");

            var conn = (DbConnection)Activator.CreateInstance(type, connectionString)!;
            return conn;
        };
    }
}
