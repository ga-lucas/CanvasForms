using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Samples;

public class DockingDemoForm : Form
{
    private Label? _topLabel;
    private Label? _bottomLabel;
    private Label? _leftLabel;
    private Label? _rightLabel;
    private TextBox? _fillTextBox;
    private Button? _anchoredButton1;
    private Button? _anchoredButton2;
    private Button? _anchoredButton3;
    private Label? _infoLabel;

    public DockingDemoForm()
    {
        Text = "Docking & Anchoring Demo";
        Width = 600;
        Height = 500;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
    }

    private void InitializeControls()
    {
        // Top docked label
        _topLabel = new Label
        {
            Text = "🔝 Docked to Top (Dock = DockStyle.Top)",
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.FromArgb(173, 216, 230),
            ForeColor = Color.FromArgb(0, 0, 139),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_topLabel);

        // Bottom docked label
        _bottomLabel = new Label
        {
            Text = "🔽 Docked to Bottom (Dock = DockStyle.Bottom)",
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.FromArgb(255, 218, 185),
            ForeColor = Color.FromArgb(139, 69, 19),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_bottomLabel);

        // Left docked label
        _leftLabel = new Label
        {
            Text = "◀ Left",
            Dock = DockStyle.Left,
            Width = 80,
            BackColor = Color.FromArgb(144, 238, 144),
            ForeColor = Color.FromArgb(0, 100, 0),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_leftLabel);

        // Right docked label
        _rightLabel = new Label
        {
            Text = "Right ▶",
            Dock = DockStyle.Right,
            Width = 80,
            BackColor = Color.FromArgb(255, 182, 193),
            ForeColor = Color.FromArgb(139, 0, 0),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_rightLabel);

        // Info label in the fill area (anchored to all sides)
        _infoLabel = new Label
        {
            Text = "ℹ️ Info: Resize the form to see docking and anchoring in action!",
            Left = 90,
            Top = 40,
            Width = 400,
            Height = 40,
            BackColor = Color.FromArgb(255, 255, 224),
            ForeColor = Color.FromArgb(70, 70, 70),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_infoLabel);

        // Fill textbox (demonstrates Dock.Fill in remaining space)
        _fillTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = "This TextBox has Dock = DockStyle.Fill. It fills all remaining space after other docked controls are positioned. Try resizing the form!"
        };
        Controls.Add(_fillTextBox);

        // Anchored buttons - demonstrate different anchor combinations

        // Button anchored to Top-Left (default - stays in place)
        _anchoredButton1 = new Button
        {
            Text = "Top-Left Anchor",
            Left = 90,
            Top = 90,
            Width = 120,
            Height = 30,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        _anchoredButton1.Click += (s, e) => ShowAnchorInfo("Top-Left", "Stays in fixed position relative to top-left corner");
        Controls.Add(_anchoredButton1);

        // Button anchored to Top-Right (moves with right edge)
        _anchoredButton2 = new Button
        {
            Text = "Top-Right Anchor",
            Left = 370,
            Top = 90,
            Width = 120,
            Height = 30,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _anchoredButton2.Click += (s, e) => ShowAnchorInfo("Top-Right", "Moves to stay same distance from right edge");
        Controls.Add(_anchoredButton2);

        // Button anchored to all sides (stretches)
        _anchoredButton3 = new Button
        {
            Text = "All Sides Anchor (Stretches)",
            Left = 90,
            Top = 130,
            Width = 400,
            Height = 30,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        _anchoredButton3.Click += (s, e) => ShowAnchorInfo("All Sides", "Stretches to maintain distance from all edges");
        Controls.Add(_anchoredButton3);
    }

    private void ShowAnchorInfo(string anchorType, string description)
    {
        if (_infoLabel != null)
        {
            _infoLabel.Text = $"ℹ️ {anchorType}: {description}";
            _infoLabel.BackColor = Color.FromArgb(200, 255, 200);
        }
    }
}
