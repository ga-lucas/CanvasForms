using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class FlowLayoutDemoForm : Form
{
    public FlowLayoutDemoForm()
    {
        Text = "FlowLayoutPanel Demo";
        Width = 760;
        Height = 520;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
        PerformLayout();
    }

    private void InitializeControls()
    {
        var header = new Label
        {
            Text = "FlowLayoutPanel: WrapContents + FlowBreak + embedded controls",
            Dock = DockStyle.Top,
            Height = 34,
            BackColor = Color.FromArgb(230, 240, 255),
            ForeColor = Color.FromArgb(26, 115, 232),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(header);

        var footer = new Label
        {
            Text = "Tip: Resize the form. The FlowBreak after the GroupBox forces the next item to start a new row.",
            Dock = DockStyle.Bottom,
            Height = 34,
            BackColor = Color.FromArgb(255, 255, 224),
            ForeColor = Color.FromArgb(70, 70, 70),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(footer);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Size(10, 10)
        };
        Controls.Add(flow);

        var intro = new Label
        {
            Text = "These items are placed by FlowLayoutPanel.",
            Width = 260,
            Height = 24,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(60, 60, 60),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Size(6, 6)
        };
        flow.Controls.Add(intro);

        var gb = new GroupBox
        {
            Text = "Embedded group",
            Width = 320,
            Height = 160,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(60, 60, 60),
            Margin = new Size(6, 6)
        };

        var txt = new TextBox
        {
            Left = 12,
            Top = 28,
            Width = 290,
            Text = "TextBox inside GroupBox"
        };
        gb.Controls.Add(txt);

        var chk = new CheckBox
        {
            Left = 12,
            Top = 62,
            Width = 290,
            Text = "CheckBox inside GroupBox",
            Checked = true
        };
        gb.Controls.Add(chk);

        var combo = new ComboBox
        {
            Left = 12,
            Top = 94,
            Width = 290,
            Text = "ComboBox inside GroupBox"
        };
        combo.Items.Add("LeftToRight");
        combo.Items.Add("TopDown");
        combo.Items.Add("RightToLeft");
        combo.Items.Add("BottomUp");
        combo.SelectedIndex = 0;
        combo.SelectedIndexChanged += (s, e) =>
        {
            switch (combo.SelectedIndex)
            {
                case 1:
                    flow.FlowDirection = FlowDirection.TopDown;
                    break;
                case 2:
                    flow.FlowDirection = FlowDirection.RightToLeft;
                    break;
                case 3:
                    flow.FlowDirection = FlowDirection.BottomUp;
                    break;
                default:
                    flow.FlowDirection = FlowDirection.LeftToRight;
                    break;
            }
            flow.PerformLayout();
        };
        gb.Controls.Add(combo);

        flow.Controls.Add(gb);
        FlowLayoutPanel.SetFlowBreak(gb, true);

        var btnWrap = new Button
        {
           Text = "WrapContents = True",
            Width = 180,
            Height = 36,
            Margin = new Size(6, 6)
        };
        btnWrap.Click += (s, e) =>
        {
            flow.WrapContents = !flow.WrapContents;
            btnWrap.Text = $"WrapContents = {flow.WrapContents}";
           flow.PerformLayout();
            flow.Invalidate();
        };
        flow.Controls.Add(btnWrap);

        var btnBreak = new Button
        {
          Text = "FlowBreak = False",
            Width = 180,
            Height = 36,
            Margin = new Size(6, 6)
        };
        btnBreak.Click += (s, e) =>
        {
            FlowLayoutPanel.SetFlowBreak(btnBreak, !FlowLayoutPanel.GetFlowBreak(btnBreak));
            btnBreak.Text = $"FlowBreak = {FlowLayoutPanel.GetFlowBreak(btnBreak)}";
           flow.PerformLayout();
            flow.Invalidate();
        };
        flow.Controls.Add(btnBreak);

        var info = new Label
        {
            Text = "A plain label after the buttons.",
            Width = 260,
            Height = 24,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(60, 60, 60),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Size(6, 6)
        };
        flow.Controls.Add(info);
    }
}
