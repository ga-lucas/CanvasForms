using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class SplitContainerDemoForm : Form
{
    private Label? _status;
    private SplitContainer? _split;

    public SplitContainerDemoForm()
    {
        Text = "SplitContainer Demo";
        Width = 900;
        Height = 600;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
        PerformLayout();
    }

    private void InitializeControls()
    {
        var title = new Label
        {
            Text = "SplitContainer (drag the splitter) - Panels use Dock=Fill",
            Left = 20,
            Top = 20,
            Width = 860,
            Height = 25,
            ForeColor = Color.FromArgb(0, 51, 153),
            BackColor = Color.Transparent
        };
        Controls.Add(title);

        _status = new Label
        {
            Text = "SplitterDistance: (drag to update)",
            Left = 20,
            Top = 520,
            Width = 860,
            Height = 25,
            ForeColor = Color.FromArgb(64, 64, 64),
            BackColor = Color.Transparent
        };
        Controls.Add(_status);

        _split = new SplitContainer
        {
            Left = 20,
            Top = 55,
            Width = 860,
            Height = 455,
            Orientation = Orientation.Vertical,
            SplitterWidth = 6,
            SplitterIncrement = 10,
            SplitterDistance = 300,
            Panel1MinSize = 150,
            Panel2MinSize = 150,
            BorderStyle = BorderStyle.Fixed3D,
            BackColor = Color.White
        };

        _split.SplitterMoving += (_, e) => UpdateStatus(e.SplitX, e.SplitY);
        _split.SplitterMoved += (_, e) => UpdateStatus(e.SplitX, e.SplitY);
        Controls.Add(_split);

        var treeHeader = new Label
        {
            Text = "Panel1: TreeView",
            Dock = DockStyle.Top,
            Height = 22,
            BackColor = Color.FromArgb(250, 250, 250)
        };

        var tree = new TreeView
        {
            Dock = DockStyle.Fill,
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true
        };

        var root = new TreeNode("Solution")
        {
            Nodes =
            {
                new TreeNode("WebForms.Canvas"),
                new TreeNode("WebForms.Canvas.Host"),
                new TreeNode("WebForms.Canvas.Tests")
            }
        };
        tree.Nodes.Add(root);

        _split.Panel1.Controls.Add(tree);
        _split.Panel1.Controls.Add(treeHeader);

        var listHeader = new Label
        {
            Text = "Panel2: ListView (Details)",
            Dock = DockStyle.Top,
            Height = 22,
            BackColor = Color.FromArgb(250, 250, 250)
        };

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };

        list.Columns.Add(new ColumnHeader { Text = "File", Width = 250 });
        list.Columns.Add(new ColumnHeader { Text = "Type", Width = 140 });
        list.Columns.Add(new ColumnHeader { Text = "Status", Width = 120 });

        list.Items.Add(new ListViewItem(new[] { "SplitContainer.cs", "Control", "New" }));
        list.Items.Add(new ListViewItem(new[] { "FormRenderer.razor", "Blazor", "Cursor updated" }));
        list.Items.Add(new ListViewItem(new[] { "SplitContainerLayoutTests.cs", "Tests", "Passing" }));

        _split.Panel2.Controls.Add(list);
        _split.Panel2.Controls.Add(listHeader);

        UpdateStatus(_split.SplitterDistance, 0);
    }

    private void UpdateStatus(int splitX, int splitY)
    {
        if (_status == null || _split == null) return;

        var axisValue = _split.Orientation == Orientation.Vertical ? splitX : splitY;
        _status.Text = $"SplitterDistance: {axisValue} (Increment: {_split.SplitterIncrement})";
    }
}
