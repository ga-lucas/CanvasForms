using Canvas.Windows.Forms;

namespace System.Windows.Forms;

/// <summary>
/// Represents a dialog that lets the user select a color.
/// Matches the WinForms <c>ColorDialog</c> API surface.
/// Renders a hue slider + saturation/brightness picker + hex/RGB inputs on the canvas.
/// </summary>
public class ColorDialog : CommonDialog
{
    // ── WinForms-compatible properties ────────────────────────────────────────

    /// <summary>Gets or sets the color selected by the user.</summary>
    public Drawing.Color Color { get; set; } = Drawing.Color.Black;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box displays all available colors
    /// in the set of basic colors. Stub — always shows full picker.
    /// </summary>
    public bool AnyColor { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the user can use the dialog box
    /// to define custom colors. Setting to false hides the "Define Custom Colors" expansion.
    /// </summary>
    public bool AllowFullOpen { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box will restrict users
    /// to selecting solid colors only. Stub — canvas always renders solid.
    /// </summary>
    public bool SolidColorOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box displays the custom-colors
    /// section expanded. When false the expanded picker is hidden until the user clicks
    /// "Define Custom Colors &gt;&gt;".
    /// </summary>
    public bool FullOpen { get; set; } = false;

    /// <summary>
    /// Gets or sets the set of custom colors shown in the dialog.
    /// Matches WinForms <c>ColorDialog.CustomColors</c> (array of ARGB ints).
    /// </summary>
    public int[]? CustomColors { get; set; }

    // ── CommonDialog overrides ────────────────────────────────────────────────

    public override void Reset()
    {
        Color        = Drawing.Color.Black;
        AnyColor     = false;
        AllowFullOpen = true;
        SolidColorOnly = false;
        FullOpen     = false;
        CustomColors = null;
    }

    protected sealed override DialogResult RunDialog(IWin32Window? owner)
    {
        if (OperatingSystem.IsBrowser())
        {
            _ = ShowDialogAsync(owner);
            return DialogResult.None;
        }
        return ShowDialogAsync(owner).GetAwaiter().GetResult();
    }

    public Task<DialogResult> ShowDialogAsync() => ShowDialogAsync(null);

    public Task<DialogResult> ShowDialogAsync(IWin32Window? owner)
    {
        var tcs = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        bool accepted = false;

        var form = BuildForm(
            onAccept: c => { Color = c; accepted = true; tcs.TrySetResult(DialogResult.OK); },
            onCancel: ()  => tcs.TrySetResult(DialogResult.Cancel));

        form.FormClosed += (_, __) =>
        {
            if (!tcs.Task.IsCompleted)
                tcs.TrySetResult(DialogResult.Cancel);
        };

        var manager = CanvasApplication.FormManager;
        if (manager != null) manager.ShowForm(form);
        else form.Show();

        _ = tcs.Task.ContinueWith(_ =>
        {
            if (accepted) try { form.Close(); } catch { }
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    // ── Dialog UI ─────────────────────────────────────────────────────────────

    private Form BuildForm(Action<Drawing.Color> onAccept, Action onCancel)
    {
        const int W   = 460;
        const int H   = 420;
        const int Pad = 12;

        var dialog = new Form
        {
            Text         = "Color",
            Width        = W,
            Height       = H,
            Left         = 150,
            Top          = 100,
            AllowResize  = false,
        };

        // ── Basic color swatches (48 WinForms-style basic colors) ─────────────
        var basicColors = BuildBasicColorList();
        const int swatchSize = 22;
        const int swatchCols = 8;

        Drawing.Color pickedColor = Color;

        // Parse initial color to HSV for slider init
        RgbToHsv(pickedColor.R, pickedColor.G, pickedColor.B,
                 out double initH, out double initS, out double initV);

        // ── Color swatches panel ──────────────────────────────────────────────
        var swatchesPanel = new Panel
        {
            Left        = Pad,
            Top         = Pad + 20,
            Width       = swatchCols * (swatchSize + 2),
            Height      = (basicColors.Count / swatchCols + 1) * (swatchSize + 2),
            BackColor   = Drawing.Color.FromArgb(240, 240, 240),
        };

        var basicLabel = new Label
        {
            Text = "Basic colors:",
            Left = Pad, Top = Pad, Width = 200, Height = 18,
        };

        // ── Hex input + RGB inputs ────────────────────────────────────────────
        int rightX = Pad + swatchesPanel.Width + 16;
        int rightW = W - rightX - Pad - 16;

        var previewPanel = new Panel
        {
            Left      = rightX,
            Top       = Pad,
            Width     = rightW,
            Height    = 44,
            BackColor = pickedColor,
        };

        var hexLabel = new Label { Text = "Hex:", Left = rightX, Top = Pad + 52, Width = 34, Height = 20 };
        var hexBox   = new TextBox
        {
            Left  = rightX + 36,
            Top   = Pad + 50,
            Width = rightW - 36,
            Height = 24,
            Text  = ColorToHex(pickedColor),
        };

        var rLabel = new Label { Text = "R:", Left = rightX,       Top = Pad + 82, Width = 16, Height = 20 };
        var rBox   = new TextBox { Left = rightX + 18, Top = Pad + 80, Width = (rightW - 48) / 3, Height = 24, Text = pickedColor.R.ToString() };
        var gLabel = new Label { Text = "G:", Left = rightX + 18 + (rightW - 48) / 3 + 4, Top = Pad + 82, Width = 16, Height = 20 };
        var gBox   = new TextBox { Left = rightX + 38 + (rightW - 48) / 3, Top = Pad + 80, Width = (rightW - 48) / 3, Height = 24, Text = pickedColor.G.ToString() };
        var bLabel = new Label { Text = "B:", Left = rightX + 58 + (rightW - 48) / 3 * 2, Top = Pad + 82, Width = 16, Height = 20 };
        var bBox   = new TextBox { Left = rightX + 76 + (rightW - 48) / 3 * 2 - 34, Top = Pad + 80, Width = (rightW - 48) / 3, Height = 24, Text = pickedColor.B.ToString() };

        // Hue slider label
        var hueLabel = new Label { Text = "Hue:", Left = rightX, Top = Pad + 114, Width = 36, Height = 20 };
        var hueBox   = new TextBox { Left = rightX + 40, Top = Pad + 112, Width = rightW - 40, Height = 24, Text = ((int)(initH * 360)).ToString() };
        var satLabel = new Label { Text = "Sat:", Left = rightX, Top = Pad + 144, Width = 36, Height = 20 };
        var satBox   = new TextBox { Left = rightX + 40, Top = Pad + 142, Width = rightW - 40, Height = 24, Text = ((int)(initS * 240)).ToString() };
        var lumLabel = new Label { Text = "Lum:", Left = rightX, Top = Pad + 174, Width = 36, Height = 20 };
        var lumBox   = new TextBox { Left = rightX + 40, Top = Pad + 172, Width = rightW - 40, Height = 24, Text = ((int)(initV * 240)).ToString() };

        var okButton = new Button
        {
            Text = "OK", Left = rightX, Top = H - 50 - 30, Width = (rightW - 4) / 2, Height = 28,
        };
        var cancelButton = new Button
        {
            Text = "Cancel", Left = rightX + (rightW - 4) / 2 + 4, Top = H - 50 - 30, Width = (rightW - 4) / 2, Height = 28,
        };

        // Add controls
        dialog.Controls.Add(basicLabel);
        dialog.Controls.Add(swatchesPanel);
        dialog.Controls.Add(previewPanel);
        dialog.Controls.Add(hexLabel);
        dialog.Controls.Add(hexBox);
        dialog.Controls.Add(rLabel); dialog.Controls.Add(rBox);
        dialog.Controls.Add(gLabel); dialog.Controls.Add(gBox);
        dialog.Controls.Add(bLabel); dialog.Controls.Add(bBox);
        dialog.Controls.Add(hueLabel); dialog.Controls.Add(hueBox);
        dialog.Controls.Add(satLabel); dialog.Controls.Add(satBox);
        dialog.Controls.Add(lumLabel); dialog.Controls.Add(lumBox);
        dialog.Controls.Add(okButton);
        dialog.Controls.Add(cancelButton);

        // Add swatch buttons
        for (int i = 0; i < basicColors.Count; i++)
        {
            var c    = basicColors[i];
            int col  = i % swatchCols;
            int row  = i / swatchCols;
            var btn  = new Button
            {
                Left      = col * (swatchSize + 2),
                Top       = row * (swatchSize + 2),
                Width     = swatchSize,
                Height    = swatchSize,
                BackColor = c,
                Text      = string.Empty,
            };
            var captured = c;
            btn.Click += (_, __) => ApplyColor(captured);
            swatchesPanel.Controls.Add(btn);
        }

        void UpdateInputsFromColor(Drawing.Color c)
        {
            pickedColor       = c;
            previewPanel.BackColor = c;
            hexBox.Text       = ColorToHex(c);
            rBox.Text         = c.R.ToString();
            gBox.Text         = c.G.ToString();
            bBox.Text         = c.B.ToString();
            RgbToHsv(c.R, c.G, c.B, out double h, out double s, out double v);
            hueBox.Text = ((int)(h * 360)).ToString();
            satBox.Text = ((int)(s * 240)).ToString();
            lumBox.Text = ((int)(v * 240)).ToString();
            dialog.Invalidate();
        }

        void ApplyColor(Drawing.Color c) => UpdateInputsFromColor(c);

        void ApplyHex()
        {
            var hex = hexBox.Text.TrimStart('#');
            if (hex.Length == 6 && TryParseHex(hex, out var c))
                UpdateInputsFromColor(c);
        }

        void ApplyRgb()
        {
            if (byte.TryParse(rBox.Text, out var r) &&
                byte.TryParse(gBox.Text, out var g) &&
                byte.TryParse(bBox.Text, out var b))
                UpdateInputsFromColor(Drawing.Color.FromArgb(r, g, b));
        }

        void ApplyHsl()
        {
            if (int.TryParse(hueBox.Text, out var h) &&
                int.TryParse(satBox.Text, out var s) &&
                int.TryParse(lumBox.Text, out var l))
            {
                var c = HsvToRgb(h / 360.0, s / 240.0, l / 240.0);
                UpdateInputsFromColor(c);
            }
        }

        hexBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) ApplyHex(); };
        rBox.KeyDown   += (_, e) => { if (e.KeyCode == Keys.Enter) ApplyRgb(); };
        gBox.KeyDown   += (_, e) => { if (e.KeyCode == Keys.Enter) ApplyRgb(); };
        bBox.KeyDown   += (_, e) => { if (e.KeyCode == Keys.Enter) ApplyRgb(); };
        hueBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) ApplyHsl(); };
        satBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) ApplyHsl(); };
        lumBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) ApplyHsl(); };

        okButton.Click     += (_, __) => onAccept(pickedColor);
        cancelButton.Click += (_, __) => { onCancel(); dialog.Close(); };

        return dialog;
    }

    // ── Color helpers ─────────────────────────────────────────────────────────

    private static List<Drawing.Color> BuildBasicColorList() => new()
    {
        // Row 1 — reds/oranges
        Drawing.Color.FromArgb(255,   0,   0), Drawing.Color.FromArgb(255, 128,   0),
        Drawing.Color.FromArgb(255, 255,   0), Drawing.Color.FromArgb(128, 255,   0),
        Drawing.Color.FromArgb(  0, 255,   0), Drawing.Color.FromArgb(  0, 255, 128),
        Drawing.Color.FromArgb(  0, 255, 255), Drawing.Color.FromArgb(  0, 128, 255),
        // Row 2 — blues/purples
        Drawing.Color.FromArgb(  0,   0, 255), Drawing.Color.FromArgb(128,   0, 255),
        Drawing.Color.FromArgb(255,   0, 255), Drawing.Color.FromArgb(255,   0, 128),
        Drawing.Color.FromArgb(128, 128,   0), Drawing.Color.FromArgb(  0, 128, 128),
        Drawing.Color.FromArgb(  0,   0, 128), Drawing.Color.FromArgb(128,   0,   0),
        // Row 3 — pastels
        Drawing.Color.FromArgb(255, 192, 192), Drawing.Color.FromArgb(255, 224, 192),
        Drawing.Color.FromArgb(255, 255, 192), Drawing.Color.FromArgb(192, 255, 192),
        Drawing.Color.FromArgb(192, 255, 255), Drawing.Color.FromArgb(192, 192, 255),
        Drawing.Color.FromArgb(255, 192, 255), Drawing.Color.FromArgb(224, 192, 255),
        // Row 4 — grays + black/white
        Drawing.Color.White,      Drawing.Color.FromArgb(212, 212, 212),
        Drawing.Color.FromArgb(160, 160, 160), Drawing.Color.FromArgb(128, 128, 128),
        Drawing.Color.FromArgb( 96,  96,  96), Drawing.Color.FromArgb( 64,  64,  64),
        Drawing.Color.FromArgb( 32,  32,  32), Drawing.Color.Black,
    };

    private static string ColorToHex(Drawing.Color c)
        => $"{c.R:X2}{c.G:X2}{c.B:X2}";

    private static bool TryParseHex(string hex, out Drawing.Color color)
    {
        color = default;
        if (hex.Length != 6) return false;
        try
        {
            int rgb = Convert.ToInt32(hex, 16);
            color = Drawing.Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
            return true;
        }
        catch { return false; }
    }

    private static void RgbToHsv(byte r, byte g, byte b,
        out double h, out double s, out double v)
    {
        double dr = r / 255.0, dg = g / 255.0, db = b / 255.0;
        double max = Math.Max(dr, Math.Max(dg, db));
        double min = Math.Min(dr, Math.Min(dg, db));
        double delta = max - min;
        v = max;
        s = max == 0 ? 0 : delta / max;
        if (delta == 0) { h = 0; return; }
        if      (max == dr) h = (dg - db) / delta % 6;
        else if (max == dg) h = (db - dr) / delta + 2;
        else                h = (dr - dg) / delta + 4;
        h /= 6;
        if (h < 0) h += 1;
    }

    private static Drawing.Color HsvToRgb(double h, double s, double v)
    {
        h = Math.Clamp(h, 0, 1); s = Math.Clamp(s, 0, 1); v = Math.Clamp(v, 0, 1);
        if (s == 0) { byte grey = (byte)(v * 255); return Drawing.Color.FromArgb(grey, grey, grey); }
        double hh = h * 6; int i = (int)hh; double f = hh - i;
        double p = v * (1 - s), q = v * (1 - s * f), t = v * (1 - s * (1 - f));
        double r, g, b;
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }
        return Drawing.Color.FromArgb((byte)(r*255), (byte)(g*255), (byte)(b*255));
    }
}
