using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demonstrates TrackBar (horizontal &amp; vertical), HScrollBar, VScrollBar and DomainUpDown.
/// All controls show their current value in a live feedback label.
/// </summary>
public class DemoSliderSpinnerForm : Form
{
    // Live feedback labels
    private Label _lblHTrackValue  = null!;
    private Label _lblVTrackValue  = null!;
    private Label _lblHScrollValue = null!;
    private Label _lblVScrollValue = null!;
    private Label _lblDomainValue  = null!;

    // Colour preview panel driven by the three sliders (R/G/B)
    private Panel _colorPreview = null!;
    private TrackBar _tbRed   = null!;
    private TrackBar _tbGreen = null!;
    private TrackBar _tbBlue  = null!;
    private Label _lblColorHex = null!;

    public DemoSliderSpinnerForm()
    {
        Text          = "Sliders & Spinners Demo";
        Width         = 820;
        Height        = 680;
        BackColor     = Color.FromArgb(245, 245, 245);
        AllowResize   = true;
        AllowMove     = true;
        MinimumWidth  = 600;
        MinimumHeight = 500;

        InitializeControls();
    }

    private void InitializeControls()
    {
        // ── Title ────────────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Sliders & Spinners",
            Left      = 10, Top = 10, Width = 790, Height = 30,
            Font      = new Font("Arial", 16),
            ForeColor = Color.FromArgb(0, 51, 153),
            TextAlign = ContentAlignment.TopCenter
        });

        // ════════════════════════════════════════════════════════════════════
        // LEFT COLUMN — TrackBars (horizontal)
        // ════════════════════════════════════════════════════════════════════
        const int lx = 20;
        int y = 55;

        // Section header
        Controls.Add(SectionLabel("TrackBar — Horizontal", lx, y)); y += 25;

        var tbH = new TrackBar
        {
            Left = lx, Top = y, Width = 360, Height = 45,
            Minimum = 0, Maximum = 100, Value = 40,
            TickFrequency = 10, TickStyle = TickStyle.BottomRight,
            Orientation = Orientation.Horizontal
        };
        _lblHTrackValue = ValueLabel($"Value: {tbH.Value}", lx + 370, y + 12);
        tbH.ValueChanged += (s, e) => _lblHTrackValue.Text = $"Value: {tbH.Value}";
        Controls.Add(tbH);
        Controls.Add(_lblHTrackValue);
        y += 60;

        // Tick style none
        Controls.Add(SectionLabel("TrackBar — No Ticks", lx, y)); y += 25;
        var tbNoTick = new TrackBar
        {
            Left = lx, Top = y, Width = 360, Height = 45,
            Minimum = 0, Maximum = 200, Value = 75,
            TickStyle = TickStyle.None,
            Orientation = Orientation.Horizontal
        };
        var lblNoTick = ValueLabel($"Value: {tbNoTick.Value}", lx + 370, y + 12);
        tbNoTick.ValueChanged += (s, e) => lblNoTick.Text = $"Value: {tbNoTick.Value}";
        Controls.Add(tbNoTick);
        Controls.Add(lblNoTick);
        y += 60;

        // ── RGB colour picker using three TrackBars ───────────────────────
        Controls.Add(SectionLabel("RGB Colour Mixer (three TrackBars)", lx, y)); y += 25;

        Controls.Add(new Label { Text = "R:", Left = lx, Top = y + 12, Width = 20, Height = 20, ForeColor = Color.DarkRed });
        _tbRed = new TrackBar { Left = lx + 22, Top = y, Width = 310, Height = 45, Minimum = 0, Maximum = 255, Value = 100, TickStyle = TickStyle.None };
        Controls.Add(_tbRed); y += 50;

        Controls.Add(new Label { Text = "G:", Left = lx, Top = y + 12, Width = 20, Height = 20, ForeColor = Color.DarkGreen });
        _tbGreen = new TrackBar { Left = lx + 22, Top = y, Width = 310, Height = 45, Minimum = 0, Maximum = 255, Value = 150, TickStyle = TickStyle.None };
        Controls.Add(_tbGreen); y += 50;

        Controls.Add(new Label { Text = "B:", Left = lx, Top = y + 12, Width = 20, Height = 20, ForeColor = Color.DarkBlue });
        _tbBlue = new TrackBar { Left = lx + 22, Top = y, Width = 310, Height = 45, Minimum = 0, Maximum = 255, Value = 200, TickStyle = TickStyle.None };
        Controls.Add(_tbBlue); y += 55;

        _colorPreview = new Panel { Left = lx, Top = y, Width = 120, Height = 50, BackColor = Color.FromArgb(100, 150, 200) };
        _lblColorHex  = new Label  { Text = "#6496C8", Left = lx + 130, Top = y + 15, Width = 120, Height = 22, ForeColor = Color.DimGray };
        Controls.Add(_colorPreview);
        Controls.Add(_lblColorHex);

        _tbRed.ValueChanged   += (s, e) => UpdateColorPreview();
        _tbGreen.ValueChanged += (s, e) => UpdateColorPreview();
        _tbBlue.ValueChanged  += (s, e) => UpdateColorPreview();
        y += 65;

        // ── DomainUpDown ──────────────────────────────────────────────────
        Controls.Add(SectionLabel("DomainUpDown", lx, y)); y += 25;

        var dud = new DomainUpDown
        {
            Left = lx, Top = y, Width = 200, Height = 24,
            Wrap = true
        };
        string[] planets = { "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };
        foreach (var p in planets) dud.Items.Add(p);
        dud.SelectedIndex = 2; // Earth

        _lblDomainValue = ValueLabel($"Selected: {dud.SelectedItem}", lx + 215, y + 2);
        dud.SelectedItemChanged += (s, e) => _lblDomainValue.Text = $"Selected: {dud.SelectedItem}";
        Controls.Add(dud);
        Controls.Add(_lblDomainValue);
        y += 40;

        // Sorted DomainUpDown
        Controls.Add(new Label { Text = "(Sorted, Wrap=false):", Left = lx, Top = y, Width = 160, Height = 20, ForeColor = Color.Gray });
        var dudSorted = new DomainUpDown
        {
            Left = lx + 165, Top = y, Width = 200, Height = 24,
            Sorted = true, Wrap = false
        };
        string[] fruits = { "Banana", "Apple", "Cherry", "Date", "Fig", "Grape" };
        foreach (var f in fruits) dudSorted.Items.Add(f);
        dudSorted.SelectedIndex = 0;
        Controls.Add(dudSorted);

        // ════════════════════════════════════════════════════════════════════
        // RIGHT COLUMN — TrackBar vertical + ScrollBars
        // ════════════════════════════════════════════════════════════════════
        const int rx = 460;
        int ry = 55;

        // Vertical TrackBar
        Controls.Add(SectionLabel("TrackBar — Vertical", rx, ry)); ry += 25;

        var tbV = new TrackBar
        {
            Left = rx, Top = ry, Width = 45, Height = 200,
            Minimum = 0, Maximum = 10, Value = 5,
            TickFrequency = 1,
            Orientation = Orientation.Vertical
        };
        _lblVTrackValue = ValueLabel($"Value: {tbV.Value}", rx + 55, ry + 80);
        tbV.ValueChanged += (s, e) => _lblVTrackValue.Text = $"Value: {tbV.Value}";
        Controls.Add(tbV);
        Controls.Add(_lblVTrackValue);
        ry += 215;

        // HScrollBar
        Controls.Add(SectionLabel("HScrollBar", rx, ry)); ry += 25;

        var hsb = new HScrollBar
        {
            Left = rx, Top = ry, Width = 320, Height = 20,
            Minimum = 0, Maximum = 50, Value = 10,
            SmallChange = 1, LargeChange = 5
        };
        _lblHScrollValue = ValueLabel($"Value: {hsb.Value}", rx, ry + 26);
        hsb.Scroll += (s, e) => _lblHScrollValue.Text = $"Value: {hsb.Value}";
        Controls.Add(hsb);
        Controls.Add(_lblHScrollValue);
        ry += 55;

        // VScrollBar
        Controls.Add(SectionLabel("VScrollBar", rx, ry)); ry += 25;

        var vsb = new VScrollBar
        {
            Left = rx, Top = ry, Width = 20, Height = 150,
            Minimum = 0, Maximum = 20, Value = 8,
            SmallChange = 1, LargeChange = 4
        };
        _lblVScrollValue = ValueLabel($"Value: {vsb.Value}", rx + 30, ry + 55);
        vsb.Scroll += (s, e) => _lblVScrollValue.Text = $"Value: {vsb.Value}";
        Controls.Add(vsb);
        Controls.Add(_lblVScrollValue);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void UpdateColorPreview()
    {
        var c = Color.FromArgb(_tbRed.Value, _tbGreen.Value, _tbBlue.Value);
        _colorPreview.BackColor = c;
        _lblColorHex.Text = $"#{_tbRed.Value:X2}{_tbGreen.Value:X2}{_tbBlue.Value:X2}";
    }

    private static Label SectionLabel(string text, int x, int y) => new Label
    {
        Text      = text,
        Left      = x, Top = y, Width = 340, Height = 20,
        ForeColor = Color.FromArgb(0, 80, 160),
        Font      = new Font("Arial", 10)
    };

    private static Label ValueLabel(string text, int x, int y) => new Label
    {
        Text      = text,
        Left      = x, Top = y, Width = 160, Height = 22,
        ForeColor = Color.DimGray
    };
}
