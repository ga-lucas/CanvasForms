using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Canvas.Windows.Forms.Host.Server.Data;

/// <summary>
/// Manages the default SQLite database used by <see cref="ServerCanvasDataService"/>.
/// <para>
/// On first startup the database is created at <c>.data/canvas.db</c> (relative to
/// the server's content root) and seeded with demo tables.  Translated apps can
/// create their own tables via <see cref="ICanvasDbContext.Execute"/>.
/// </para>
/// </summary>
public sealed class SqliteDataConnection
{
    private readonly string _dbPath;

    public SqliteDataConnection(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, ".data");
        Directory.CreateDirectory(dataDir);
        _dbPath = Path.Combine(dataDir, "canvas.db");
    }

    /// <summary>Full path to the SQLite database file.</summary>
    public string DbPath => _dbPath;

    /// <summary>
    /// Creates a new open <see cref="DbConnection"/> to the SQLite database.
    /// The caller is responsible for disposal.
    /// </summary>
    public DbConnection CreateConnection()
    {
        var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        return conn;
    }

    /// <summary>
    /// Ensures the database exists and contains the demo seed data.
    /// Called once at application startup.
    /// </summary>
    public void EnsureSeeded()
    {
        using var conn = (SqliteConnection)CreateConnection();
        using var cmd = conn.CreateCommand();

        // ── Products table ──────────────────────────────────────────────────
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Products (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT    NOT NULL,
                Category    TEXT    NOT NULL,
                Price       REAL    NOT NULL,
                Stock       INTEGER NOT NULL,
                Available   INTEGER NOT NULL DEFAULT 1
            )
            """;
        cmd.ExecuteNonQuery();

        // ── Customers table ─────────────────────────────────────────────────
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Customers (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName   TEXT NOT NULL,
                LastName    TEXT NOT NULL,
                Email       TEXT,
                City        TEXT,
                Country     TEXT
            )
            """;
        cmd.ExecuteNonQuery();

        // ── Orders table ────────────────────────────────────────────────────
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Orders (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId  INTEGER NOT NULL,
                ProductId   INTEGER NOT NULL,
                Quantity    INTEGER NOT NULL,
                OrderDate   TEXT    NOT NULL,
                Total       REAL    NOT NULL
            )
            """;
        cmd.ExecuteNonQuery();

        // Seed only when empty
        cmd.CommandText = "SELECT COUNT(*) FROM Products";
        var count = (long)(cmd.ExecuteScalar() ?? 0L);
        if (count > 0) return;

        SeedProducts(conn);
        SeedCustomers(conn);
        SeedOrders(conn);
    }

    // ── Seed helpers ────────────────────────────────────────────────────────

    private static void SeedProducts(SqliteConnection conn)
    {
        var products = new[]
        {
            ("Widget Pro",      "Widgets",    19.99,  150, 1),
            ("Gadget Plus",     "Gadgets",    34.50,   80, 1),
            ("Doohickey Deluxe","Parts",       9.95,  500, 1),
            ("Thingamajig",     "Parts",      14.75,  200, 1),
            ("Whatchamacallit", "Gadgets",    49.99,   45, 1),
            ("Gizmo Standard",  "Widgets",    24.00,  120, 1),
            ("Flibbertigibbet", "Misc",        5.00, 1000, 0),
            ("Contraption XL",  "Gadgets",    89.99,   20, 1),
            ("Apparatus Mini",  "Parts",      12.50,  300, 1),
            ("Device Alpha",    "Widgets",    59.00,   60, 1),
        };

        foreach (var (name, cat, price, stock, avail) in products)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Products(Name,Category,Price,Stock,Available) VALUES(@n,@c,@p,@s,@a)";
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@c", cat);
            cmd.Parameters.AddWithValue("@p", price);
            cmd.Parameters.AddWithValue("@s", stock);
            cmd.Parameters.AddWithValue("@a", avail);
            cmd.ExecuteNonQuery();
        }
    }

    private static void SeedCustomers(SqliteConnection conn)
    {
        var customers = new[]
        {
            ("Alice", "Smith",   "alice@example.com",  "New York",  "USA"),
            ("Bob",   "Jones",   "bob@example.com",    "London",    "UK"),
            ("Carol", "Williams","carol@example.com",  "Sydney",    "Australia"),
            ("David", "Brown",   "david@example.com",  "Toronto",   "Canada"),
            ("Eva",   "Davis",   "eva@example.com",    "Berlin",    "Germany"),
        };

        foreach (var (first, last, email, city, country) in customers)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Customers(FirstName,LastName,Email,City,Country) VALUES(@f,@l,@e,@c,@co)";
            cmd.Parameters.AddWithValue("@f", first);
            cmd.Parameters.AddWithValue("@l", last);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@c", city);
            cmd.Parameters.AddWithValue("@co", country);
            cmd.ExecuteNonQuery();
        }
    }

    private static void SeedOrders(SqliteConnection conn)
    {
        var orders = new[]
        {
            (1, 1, 3, "2024-01-10",  59.97),
            (2, 2, 1, "2024-01-12",  34.50),
            (3, 5, 2, "2024-01-15",  69.00),
            (4, 3, 4, "2024-02-01",  44.25),
            (5, 1, 8, "2024-02-14",  89.99),
            (1, 6, 2, "2024-03-05",  48.00),
            (2, 9, 5, "2024-03-20",  62.50),
            (5, 10,1, "2024-04-01",  59.00),
        };

        foreach (var (cid, pid, qty, date, total) in orders)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Orders(CustomerId,ProductId,Quantity,OrderDate,Total) VALUES(@c,@p,@q,@d,@t)";
            cmd.Parameters.AddWithValue("@c", cid);
            cmd.Parameters.AddWithValue("@p", pid);
            cmd.Parameters.AddWithValue("@q", qty);
            cmd.Parameters.AddWithValue("@d", date);
            cmd.Parameters.AddWithValue("@t", total);
            cmd.ExecuteNonQuery();
        }
    }
}
