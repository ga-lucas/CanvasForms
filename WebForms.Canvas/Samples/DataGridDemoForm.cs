using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demo form showcasing DataGridView with multiple column types and data binding patterns.
/// </summary>
public class DataGridDemoForm : Form
{
    public DataGridDemoForm()
    {
        Text = "DataGridView Demo";
        Width = 820;
        Height = 540;
        AllowResize = true;
        MinimumWidth = 500;
        MinimumHeight = 360;
        BackColor = Color.White;
        BuildUI();
    }

    private void BuildUI()
    {
        const int Pad = 10;

        // ── Toolbar ────────────────────────────────────────────────────────────
        var toolbar = new Panel
        {
            Left = 0, Top = 0, Width = Width, Height = 36,
            BackColor = Color.FromArgb(240, 240, 240),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        Button MakeToolBtn(string text, int left) => new Button
        {
            Text = text, Left = left, Top = 4,
            Width = 110, Height = 28,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
        };

        var btnList = MakeToolBtn("List Source", Pad);
        var btnTable = MakeToolBtn("DataTable", Pad + 120);
        var btnBinding = MakeToolBtn("BindingSource", Pad + 240);
        var btnClear = MakeToolBtn("Clear", Pad + 360);
        toolbar.Controls.Add(btnList);
        toolbar.Controls.Add(btnTable);
        toolbar.Controls.Add(btnBinding);
        toolbar.Controls.Add(btnClear);
        Controls.Add(toolbar);

        // ── Status label ───────────────────────────────────────────────────────
        var statusLabel = new Label
        {
            Text = "Select a data source above",
            Left = Pad, Top = Height - 32,
            Width = Width - Pad * 2 - 20, Height = 20,
            ForeColor = Color.Gray,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };
        Controls.Add(statusLabel);

        // ── DataGridView ───────────────────────────────────────────────────────
        var grid = new DataGridView
        {
            Left = Pad, Top = 44,
            Width = Width - Pad * 2 - 16,
            Height = Height - 44 - 40,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = true,
        };
        Controls.Add(grid);

        // ── Selection feedback ─────────────────────────────────────────────────
        grid.SelectionChanged += (_, _) =>
        {
            if (grid.SelectedRowIndex.HasValue)
                statusLabel.Text = $"Row {grid.SelectedRowIndex.Value} selected";
        };

        grid.CellClick += (_, e) =>
        {
            statusLabel.Text = $"Cell [{e.RowIndex}, {e.ColumnIndex}] clicked";
        };

        // ── Button handlers ────────────────────────────────────────────────────

        btnList.Click += (_, _) =>
        {
            grid.Columns.Clear();
            grid.AutoGenerateColumns = true;
            grid.DataSource = BuildListSource();
            statusLabel.Text = $"Loaded {grid.RowCount} rows from List<T>";
        };

        btnTable.Click += (_, _) =>
        {
            grid.Columns.Clear();
            grid.AutoGenerateColumns = true;
            grid.DataSource = BuildDataTableSource();
            statusLabel.Text = $"Loaded {grid.RowCount} rows from DataTable";
        };

        btnBinding.Click += (_, _) =>
        {
            grid.Columns.Clear();
            // Explicit columns for BindingSource demo
            grid.AutoGenerateColumns = false;
            AddExplicitColumns(grid);
            var bs = new BindingSource();
            bs.DataSource = BuildListSource();
            grid.DataSource = bs;
            statusLabel.Text = $"Loaded {grid.RowCount} rows via BindingSource";
        };

        btnClear.Click += (_, _) =>
        {
            grid.Columns.Clear();
            grid.DataSource = null;
            statusLabel.Text = "Cleared";
        };
    }

    // ── Sample data factories ──────────────────────────────────────────────────

    private static List<ProductRow> BuildListSource()
    {
        return new List<ProductRow>
        {
            new(1,  "Widget A",       "Electronics",  9.99m,   true,  42),
            new(2,  "Gadget B",       "Electronics",  24.50m,  true,  15),
            new(3,  "Sprocket C",     "Hardware",     3.75m,   false, 200),
            new(4,  "Doohickey D",    "Misc",         14.00m,  true,  8),
            new(5,  "Thingamajig E",  "Hardware",     6.25m,   false, 77),
            new(6,  "Gizmo F",        "Electronics",  49.99m,  true,  3),
            new(7,  "Part G",         "Hardware",     1.10m,   true,  500),
            new(8,  "Component H",    "Electronics",  18.00m,  false, 22),
            new(9,  "Unit I",         "Misc",         0.99m,   true,  999),
            new(10, "Module J",       "Electronics",  99.00m,  true,  1),
        };
    }

    private static DataTable BuildDataTableSource()
    {
        var dt = new DataTable("Inventory");
        dt.Columns.Add("ID",       typeof(int));
        dt.Columns.Add("Name",     typeof(string));
        dt.Columns.Add("Category", typeof(string));
        dt.Columns.Add("Price",    typeof(decimal));
        dt.Columns.Add("InStock",  typeof(bool));
        dt.Columns.Add("Qty",      typeof(int));

        var products = BuildListSource();
        foreach (var p in products)
            dt.Rows.Add(p.Id, p.Name, p.Category, p.Price, p.InStock, p.Qty);

        return dt;
    }

    private static void AddExplicitColumns(DataGridView grid)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Id",       HeaderText = "ID",       DataPropertyName = "Id",       Width = 40  });
        grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Name",     HeaderText = "Name",     DataPropertyName = "Name",     Width = 130 });
        grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Category", HeaderText = "Category", DataPropertyName = "Category", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Price",    HeaderText = "Price",    DataPropertyName = "Price",    Width = 80,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }});
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "InStock",  HeaderText = "In Stock", DataPropertyName = "InStock",  Width = 70  });
        grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Qty",      HeaderText = "Qty",      DataPropertyName = "Qty",      Width = 55  });
    }

    // ── Strongly-typed row record ──────────────────────────────────────────────

    private record ProductRow(int Id, string Name, string Category, decimal Price, bool InStock, int Qty);
}
