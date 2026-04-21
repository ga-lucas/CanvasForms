using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class TableLayoutDemoForm : Form
{
    private Panel? _anchoredPanel;
    private Label? _anchoredInfo;
    private Button? _anchoredButton;

    public TableLayoutDemoForm()
    {
        Text = "TableLayoutPanel Demo";
        Width = 820;
        Height = 560;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
        PerformLayout();

        // After the initial layout pass, position the anchored demo controls relative to the
        // actual Panel size, then let anchoring keep them in place on subsequent resizes.
        ApplyAnchoredDemoLayout();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyAnchoredDemoLayout();
    }

    private void ApplyAnchoredDemoLayout()
    {
        if (_anchoredPanel == null || _anchoredInfo == null || _anchoredButton == null) return;
        if (_anchoredPanel.Width <= 0 || _anchoredPanel.Height <= 0) return;

        // Top label stretches left/right.
        _anchoredInfo.Left = 10;
        _anchoredInfo.Top = 10;
        _anchoredInfo.Width = Math.Max(0, _anchoredPanel.Width - 20);

        // Bottom-right button sits with a small padding.
        const int pad = 10;
        _anchoredButton.Left = Math.Max(0, _anchoredPanel.Width - _anchoredButton.Width - pad);
        _anchoredButton.Top = Math.Max(0, _anchoredPanel.Height - _anchoredButton.Height - pad);

        // Reset anchor baselines now that the parent has a stable size.
        _anchoredInfo.OriginalBoundsSet = false;
        _anchoredButton.OriginalBoundsSet = false;
        _anchoredPanel.PerformLayout();
    }

    private void InitializeControls()
    {
        var header = new Label
        {
            Text = "TableLayoutPanel: AutoSize tracks, spans, Dock/Anchor within cells",
            Dock = DockStyle.Top,
            Height = 34,
            BackColor = Color.FromArgb(230, 240, 255),
            ForeColor = Color.FromArgb(26, 115, 232),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(header);

        var footer = new Label
        {
            Text = "Tip: Resize the form. The bottom row uses Percent sizing; the top row mixes Absolute/AutoSize/Percent.",
            Dock = DockStyle.Bottom,
            Height = 34,
            BackColor = Color.FromArgb(255, 255, 224),
            ForeColor = Color.FromArgb(70, 70, 70),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(footer);

        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Size(10, 10),
            ColumnCount = 3,
            RowCount = 3
        };
        Controls.Add(tlp);

        tlp.SuspendLayout();

        // Columns: Absolute | AutoSize | Percent
        tlp.ColumnStyles[0].SizeType = SizeType.Absolute;
        tlp.ColumnStyles[0].Size = 220;
        tlp.ColumnStyles[1].SizeType = SizeType.AutoSize;
        tlp.ColumnStyles[1].Size = 0;
        tlp.ColumnStyles[2].SizeType = SizeType.Percent;
        tlp.ColumnStyles[2].Size = 100;

        // Rows: AutoSize | Absolute | Percent
        tlp.RowStyles[0].SizeType = SizeType.AutoSize;
        tlp.RowStyles[0].Size = 0;
        tlp.RowStyles[1].SizeType = SizeType.Absolute;
        tlp.RowStyles[1].Size = 160;
        tlp.RowStyles[2].SizeType = SizeType.Percent;
        tlp.RowStyles[2].Size = 100;

        var leftGroup = new GroupBox
        {
            Text = "Embedded",
            Dock = DockStyle.Fill,
            Margin = new Size(6, 6)
        };

        var leftText = new TextBox
        {
            Left = 12,
            Top = 28,
            Width = 180,
            Text = "Inside GroupBox"
        };
        leftGroup.Controls.Add(leftText);

        var leftCheck = new CheckBox
        {
            Left = 12,
            Top = 62,
            Width = 180,
            Text = "Check me",
            Checked = true
        };
        leftGroup.Controls.Add(leftCheck);

        tlp.Controls.Add(leftGroup);
        tlp.SetCellPosition(leftGroup, new TableLayoutPanelCellPosition(0, 0));
        TableLayoutPanel.SetRowSpan(leftGroup, 2);

        var autosizeLabel = new Label
        {
            Text = "AutoSize col\n(prefers width)",
            Width = 140,
            Height = 40,
            BackColor = Color.FromArgb(255, 255, 224),
            ForeColor = Color.FromArgb(60, 60, 60),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Size(6, 6)
        };
        tlp.Controls.Add(autosizeLabel);
        tlp.SetCellPosition(autosizeLabel, new TableLayoutPanelCellPosition(1, 0));

        var anchoredPanel = new Panel
        {
            BackColor = Color.FromArgb(245, 245, 245),
            Dock = DockStyle.Fill,
            Margin = new Size(6, 6)
        };
        _anchoredPanel = anchoredPanel;
        tlp.Controls.Add(anchoredPanel);
        tlp.SetCellPosition(anchoredPanel, new TableLayoutPanelCellPosition(2, 0));

        var anchoredInfo = new Label
        {
            Text = "Anchor Right|Bottom",
            Left = 10,
            Top = 10,
            Width = 160,
            Height = 24,
            BackColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _anchoredInfo = anchoredInfo;
        anchoredPanel.Controls.Add(anchoredInfo);

        var anchoredButton = new Button
        {
            Text = "Bottom-Right",
            Width = 120,
            Height = 30,
            Left = 10,
            Top = 10,
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        _anchoredButton = anchoredButton;
        anchoredButton.Click += (s, e) =>
        {
            anchoredInfo.Text = "Clicked at " + DateTime.Now.ToLongTimeString();
        };
        anchoredPanel.Controls.Add(anchoredButton);

        // Middle row: a spanning TextBox across col 1 & 2
        var spanningText = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = "This TextBox spans 2 columns (ColumnSpan=2) and is Dock=Fill",
            Margin = new Size(6, 6)
        };
        tlp.Controls.Add(spanningText);
        tlp.SetCellPosition(spanningText, new TableLayoutPanelCellPosition(1, 1));
        TableLayoutPanel.SetColumnSpan(spanningText, 2);

        // Bottom row: FlowLayoutPanel embedded inside a TableLayoutPanel cell
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Size(6, 6),
            WrapContents = true
        };
        var one = new Button { Text = "One", Width = 70, Height = 30, Margin = new Size(4, 4) };
        var two = new Button { Text = "Two", Width = 70, Height = 30, Margin = new Size(4, 4) };
        var three = new Button { Text = "Three", Width = 70, Height = 30, Margin = new Size(4, 4) };

        one.Click += (s, e) => footer.Text = "Clicked: One @ " + DateTime.Now.ToLongTimeString();
        two.Click += (s, e) => footer.Text = "Clicked: Two @ " + DateTime.Now.ToLongTimeString();
        three.Click += (s, e) => footer.Text = "Clicked: Three @ " + DateTime.Now.ToLongTimeString();

        flow.Controls.Add(one);
        flow.Controls.Add(two);
        FlowLayoutPanel.SetFlowBreak(two, true);
        flow.Controls.Add(three);

        tlp.Controls.Add(flow);
        tlp.SetCellPosition(flow, new TableLayoutPanelCellPosition(0, 2));
        TableLayoutPanel.SetColumnSpan(flow, 3);

        tlp.ResumeLayout(false);
    }
}
