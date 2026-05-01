using System.Text.Json;
using System.Text.Json.Serialization;

namespace Canvas.Windows.Forms.Host.Server.Data;

// ──────────────────────────────────────────────────────────────────────────────
//  canvas-connections.json schema
//
//  Place this file alongside your translated app's main assembly (or anywhere
//  in the app directory).  The server auto-loads it before launching the app.
//
//  Example:
//  {
//    "connections": [
//      {
//        "name": "Default",
//        "provider": "Sqlite",
//        "connectionString": "Data Source=app.db"
//      },
//      {
//        "name": "Reporting",
//        "provider": "SqlServer",
//        "connectionString": "Server=localhost;Database=Reports;Trusted_Connection=true"
//      }
//    ]
//  }
//
//  Supported providers: Sqlite, SqlServer (MsSql/SqlClient), Postgres (PostgreSQL/Npgsql), MySQL (MariaDB)
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Root model for <c>canvas-connections.json</c>.
/// </summary>
public sealed class CanvasConnectionsConfig
{
    [JsonPropertyName("connections")]
    public List<CanvasConnectionEntry> Connections { get; set; } = [];

    // ── JSON file names searched (in priority order) ──────────────────────────

    public static readonly string[] FileNames =
    [
        "canvas-connections.json",
        "canvas-connections.local.json"   // local override, not committed
    ];

    // ── Loader ────────────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>
    /// Attempts to load connection config from the given directory.
    /// Returns <c>null</c> if no config file is found.
    /// Merges all found files (local overrides base).
    /// </summary>
    public static CanvasConnectionsConfig? TryLoad(string directory, ILogger? logger = null)
    {
        CanvasConnectionsConfig? merged = null;

        foreach (var fileName in FileNames)
        {
            var path = Path.Combine(directory, fileName);
            if (!File.Exists(path)) continue;

            try
            {
                var json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<CanvasConnectionsConfig>(json, _jsonOptions);
                if (config == null) continue;

                logger?.LogInformation("canvas-connections: loaded {File} ({Count} connection(s))",
                    path, config.Connections.Count);

                if (merged == null)
                {
                    merged = config;
                }
                else
                {
                    // local file overrides base: upsert by name
                    foreach (var entry in config.Connections)
                    {
                        var existing = merged.Connections
                            .FindIndex(c => string.Equals(c.Name, entry.Name, StringComparison.OrdinalIgnoreCase));
                        if (existing >= 0)
                            merged.Connections[existing] = entry;
                        else
                            merged.Connections.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "canvas-connections: failed to parse {File}", path);
            }
        }

        return merged;
    }

    /// <summary>
    /// Applies all entries in this config to the given data service.
    /// </summary>
    public void Apply(ServerCanvasDataService service, CanvasProviderResolver resolver, ILogger? logger = null)
    {
        foreach (var entry in Connections)
        {
            if (string.IsNullOrWhiteSpace(entry.Provider) || string.IsNullOrWhiteSpace(entry.ConnectionString))
            {
                logger?.LogWarning("canvas-connections: skipping entry '{Name}' — missing provider or connectionString", entry.Name);
                continue;
            }

            try
            {
                service.RegisterProvider(entry.Name, entry.Provider, entry.ConnectionString);
                logger?.LogInformation("canvas-connections: registered '{Name}' ({Provider})", entry.Name, entry.Provider);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "canvas-connections: failed to register '{Name}'", entry.Name);
            }
        }
    }
}

/// <summary>
/// One connection entry in <c>canvas-connections.json</c>.
/// </summary>
public sealed class CanvasConnectionEntry
{
    /// <summary>Connection name used in <c>OpenContext("name")</c>.  Defaults to <c>"Default"</c>.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Default";

    /// <summary>Provider identifier: <c>Sqlite</c>, <c>SqlServer</c>, <c>Postgres</c>, <c>MySQL</c>.</summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "Sqlite";

    /// <summary>ADO.NET connection string passed directly to the provider.</summary>
    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; set; } = string.Empty;
}
