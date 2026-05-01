using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demo form that binds a DataGridView to server-backed SQLite data via
/// <see cref="CanvasDataService.Current"/>.  Shows the three common tables
/// (Products, Customers, Orders) and demonstrates an inline INSERT.
/// </summary>
public class ServerDataDemoForm : Form
{
    private DataGridView _grid = null!;
    private Label _statusLabel = null!;
    private Label _dbPathLabel = null!;

    public ServerDataDemoForm()
    {
        Text = "Server Data Demo (ADO.NET / SQLite)";
        Width = 860;
        Height = 560;
        AllowResize = true;
        MinimumWidth = 600;
        MinimumHeight = 400;
        BackColor = Color.White;
        BuildUI();
    }

    private void BuildUI()
    {
        const int Pad = 10;

        // ── Toolbar ────────────────────────────────────────────────────────────
        var toolbar = new Panel
        {
            Left = 0, Top = 0, Width = Width, Height = 42,
            BackColor = Color.FromArgb(240, 240, 240),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        Button MakeBtn(string text, int left) => new Button
        {
            Text = text, Left = left, Top = 6, Width = 110, Height = 28,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
        };

        var btnProducts  = MakeBtn("Products",  Pad);
        var btnCustomers = MakeBtn("Customers", Pad + 120);
        var btnOrders    = MakeBtn("Orders",    Pad + 240);
        var btnJoin      = MakeBtn("Join Query", Pad + 360);
        var btnAddRow    = MakeBtn("Add Product", Pad + 480);
        var btnRefresh   = MakeBtn("Refresh",   Pad + 600);
        var btnRegister  = MakeBtn("Register DB", Pad + 720);

        toolbar.Controls.Add(btnProducts);
        toolbar.Controls.Add(btnCustomers);
        toolbar.Controls.Add(btnOrders);
        toolbar.Controls.Add(btnJoin);
        toolbar.Controls.Add(btnAddRow);
        toolbar.Controls.Add(btnRefresh);
        toolbar.Controls.Add(btnRegister);
        Controls.Add(toolbar);

        // ── DB path note ───────────────────────────────────────────────────────
        _dbPathLabel = new Label
        {
            Text = CanvasDataService.Current != null
                ? "Connected to server SQLite database"
                : "⚠ No data service available (server not running?)",
            Left = Pad, Top = 48,
            Width = Width - Pad * 2, Height = 18,
            ForeColor = CanvasDataService.Current != null ? Color.FromArgb(0, 128, 0) : Color.Red,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };
        Controls.Add(_dbPathLabel);

        // ── DataGridView ───────────────────────────────────────────────────────
        _grid = new DataGridView
        {
            Left = Pad, Top = 72,
            Width = Width - Pad * 2, Height = Height - 72 - 36 - Pad,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        };
        Controls.Add(_grid);

        // ── Status bar ────────────────────────────────────────────────────────
        _statusLabel = new Label
        {
            Text = "Click a button to load data from the server database",
            Left = Pad, Top = Height - 30,
            Width = Width - Pad * 2, Height = 20,
            ForeColor = Color.Gray,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };
        Controls.Add(_statusLabel);

        // ── Button events ─────────────────────────────────────────────────────
        btnProducts.Click  += (_, _) => LoadTable("Products",  "SELECT * FROM Products");
        btnCustomers.Click += (_, _) => LoadTable("Customers", "SELECT * FROM Customers");
        btnOrders.Click    += (_, _) => LoadTable("Orders",    "SELECT * FROM Orders ORDER BY OrderDate DESC");
        btnJoin.Click      += (_, _) => LoadTable("Orders + Details",
            """
            SELECT o.Id, c.FirstName || ' ' || c.LastName AS Customer,
                   p.Name AS Product, o.Quantity, o.OrderDate, o.Total
            FROM   Orders o
            JOIN   Customers c ON c.Id = o.CustomerId
            JOIN   Products  p ON p.Id = o.ProductId
            ORDER  BY o.OrderDate DESC
            """);
        btnAddRow.Click  += OnAddRow;
        btnRefresh.Click += (_, _) =>
        {
            if (_currentSql != null)
                LoadTable(_currentTitle!, _currentSql);
        };
        btnRegister.Click += OnRegisterSecondDb;

        // Load products by default
        Load += (_, _) => LoadTable("Products", "SELECT * FROM Products");
    }

    private string? _currentSql;
    private string? _currentTitle;

    private void OnAddRow(object? sender, EventArgs e)
    {
        var svc = CanvasDataService.Current;
        if (svc == null) { SetStatus("No data service.", isError: true); return; }

        try
        {
            using var ctx = svc.OpenContext();
            int n = (int)(long)(ctx.Scalar("SELECT COUNT(*) FROM Products") ?? 0L);
            ctx.Execute(
                "INSERT INTO Products(Name,Category,Price,Stock,Available) VALUES(@n,@c,@p,@s,@a)",
                ("@n", $"New Product {n + 1}"),
                ("@c", "Demo"),
                ("@p", 0.99),
                ("@s", 1),
                ("@a", 1));
            LoadTable("Products", "SELECT * FROM Products");
            SetStatus($"Inserted 'New Product {n + 1}' — total rows now {n + 1}.");
        }
        catch (Exception ex)
        {
            SetStatus($"Insert failed: {ex.Message}", isError: true);
        }
    }

    private void SetStatus(string text, bool isError = false)
    {
        _statusLabel.Text = text;
        _statusLabel.ForeColor = isError ? Color.Red : Color.Gray;
    }

    // ── Runtime registration demo ─────────────────────────────────────────────

    private bool _secondDbRegistered = false;

    private void OnRegisterSecondDb(object? sender, EventArgs e)
    {
        var svc = CanvasDataService.Current;
        if (svc == null) { SetStatus("No data service.", isError: true); return; }

        if (!_secondDbRegistered)
        {
            try
            {
                // Dynamically register a second in-memory SQLite database named "Demo2".
                // In a real app this could be "SqlServer", "Postgres", or any other provider.
                // The connection string is passed as a plain string — no ADO.NET types needed here.
                CanvasDataService.RegisterProvider("Demo2", "Sqlite", "Data Source=:memory:");

                // Seed the in-memory DB via OpenContext
                using var ctx = svc.OpenContext("Demo2");
                ctx.Execute("CREATE TABLE Notes (Id INTEGER PRIMARY KEY, Text TEXT, Priority INTEGER)");
                ctx.Execute("INSERT INTO Notes VALUES(1,'Buy groceries',2)");
                ctx.Execute("INSERT INTO Notes VALUES(2,'Fix bug #42',1)");
                ctx.Execute("INSERT INTO Notes VALUES(3,'Read book',3)");

                _secondDbRegistered = true;
                var providers = CanvasDataService.GetRegisteredProviders();
                SetStatus($"Registered 'Demo2' (in-memory SQLite). Active providers: {string.Join(", ", providers)}");
            }
            catch (Exception ex)
            {
                SetStatus($"Registration failed: {ex.Message}", isError: true);
                return;
            }
        }

        // Query the second DB
        LoadTable("Demo2 → Notes", "SELECT * FROM Notes ORDER BY Priority", connectionName: "Demo2");
    }

    private void LoadTable(string title, string sql, string connectionName = "Default")
    {
        _currentSql = sql;
        _currentTitle = title;

        var svc = CanvasDataService.Current;
        if (svc == null) { SetStatus("No data service available.", isError: true); return; }

        try
        {
            var table = new DataTable(title);
            using var ctx = svc.OpenContext(connectionName);
            ctx.Fill(table, sql);
            _grid.DataSource = table;
            SetStatus($"{title}: {table.Rows.Count} rows  [connection: {connectionName}]");
        }
        catch (Exception ex)
        {
            SetStatus($"Error loading {title}: {ex.Message}", isError: true);
        }
    }
}
