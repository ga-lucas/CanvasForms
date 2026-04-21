using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demonstrates hierarchical/collection controls: TreeView and ListView side-by-side
/// </summary>
public class DemoCollectionControlsForm : Form
{
    private Label? _treeOutput;
    private Label? _listViewOutput;

    public DemoCollectionControlsForm()
    {
        Text = "TreeView & ListView Demo";
        Width = 850;
        Height = 580;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
    }

    private void InitializeControls()
    {
        const int leftCol = 20;
        const int rightCol = 430;
        int y = 20;

        // ═══ Title ═══
        var title = new Label
        {
            Text = "Collection Controls",
            Left = 20, Top = y, Width = 810, Height = 30,
            Font = new Font("Arial", 16),
            ForeColor = Color.FromArgb(0, 51, 153),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(title);
        y += 40;

        // ═══ LEFT: TreeView ═══
        var lblTree = new Label
        {
            Text = "TreeView (Expand/Collapse with mouse or arrows):",
            Left = leftCol, Top = y, Width = 380, Height = 20
        };
        Controls.Add(lblTree);
        y += 25;

        var treeView = new TreeView
        {
            Left = leftCol, Top = y, Width = 380, Height = 350,
            ShowLines = true,
            ShowRootLines = true,
            ShowPlusMinus = true
        };

        // Build sample tree
        var root1 = new TreeNode("Documents");
        root1.Nodes.Add(new TreeNode("Reports", new[]
        {
            new TreeNode("Q1 Report.pdf"),
            new TreeNode("Q2 Report.pdf"),
            new TreeNode("Annual Summary.docx")
        }));
        root1.Nodes.Add(new TreeNode("Presentations", new[]
        {
            new TreeNode("Product Launch.pptx"),
            new TreeNode("Team Meeting.pptx")
        }));
        treeView.Nodes.Add(root1);

        var root2 = new TreeNode("Projects");
        root2.Nodes.Add(new TreeNode("WebApp", new[]
        {
            new TreeNode("src"),
            new TreeNode("tests"),
            new TreeNode("README.md")
        }));
        root2.Nodes.Add(new TreeNode("MobileApp", new[]
        {
            new TreeNode("iOS"),
            new TreeNode("Android")
        }));
        treeView.Nodes.Add(root2);

        var root3 = new TreeNode("Media");
        root3.Nodes.Add(new TreeNode("Photos", new[]
        {
            new TreeNode("Vacation 2024"),
            new TreeNode("Family")
        }));
        root3.Nodes.Add(new TreeNode("Videos"));
        treeView.Nodes.Add(root3);

        treeView.AfterSelect += (s, e) =>
        {
            if (_treeOutput != null && e.Node != null)
                _treeOutput.Text = $"Selected: {e.Node.Text} (Path: {e.Node.FullPath})";
        };

        Controls.Add(treeView);
        y += 355;

        _treeOutput = new Label
        {
            Text = "Selected: (none)",
            Left = leftCol, Top = y, Width = 380, Height = 40,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(_treeOutput);

        // ═══ RIGHT: ListView ═══
        y = 60;
        var lblListView = new Label
        {
            Text = "ListView (Details View):",
            Left = rightCol, Top = y, Width = 380, Height = 20
        };
        Controls.Add(lblListView);
        y += 25;

        var listView = new ListView
        {
            Left = rightCol, Top = y, Width = 390, Height = 350,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };

        // Add columns
        listView.Columns.Add(new ColumnHeader { Text = "Name", Width = 150 });
        listView.Columns.Add(new ColumnHeader { Text = "Type", Width = 100 });
        listView.Columns.Add(new ColumnHeader { Text = "Size", Width = 80 });

        // Add items
        listView.Items.Add(new ListViewItem(new[] { "Project.sln", "Solution", "12 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "Program.cs", "C# File", "4 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "README.md", "Markdown", "2 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "app.config", "Config", "1 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "styles.css", "CSS", "8 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "index.html", "HTML", "3 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "script.js", "JavaScript", "6 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "package.json", "JSON", "1 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "logo.png", "Image", "45 KB" }));
        listView.Items.Add(new ListViewItem(new[] { "data.xml", "XML", "7 KB" }));

        listView.SelectedIndexChanged += (s, e) =>
        {
            if (_listViewOutput != null && listView.SelectedIndices.Count > 0)
            {
                var item = listView.Items[listView.SelectedIndices[0]];
                // SubItems[0] = Type (second column), SubItems[1] = Size (third column)
                _listViewOutput.Text = $"Selected: {item.Text} ({item.SubItems[0].Text}, {item.SubItems[1].Text})";
            }
        };

        Controls.Add(listView);
        y += 355;

        _listViewOutput = new Label
        {
            Text = "Selected: (none)",
            Left = rightCol, Top = y, Width = 380, Height = 40,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(_listViewOutput);

        // ═══ Instructions ═══
        y += 45;
        var instructions = new Label
        {
            Text = "TreeView: Click +/- or use arrow keys. ListView: Use arrow keys, Home/End, Page Up/Down to navigate.",
            Left = 20, Top = y, Width = 810, Height = 40,
            ForeColor = Color.Gray
        };
        Controls.Add(instructions);
    }
}
