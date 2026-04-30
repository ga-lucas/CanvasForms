using Canvas.Windows.Forms;
using CanvasFont      = Canvas.Windows.Forms.Drawing.Font;
using CanvasFontStyle = Canvas.Windows.Forms.Drawing.FontStyle;

namespace System.Windows.Forms;

/// <summary>
/// Prompts the user to select a font, font size, and font style.
/// Matches the WinForms <c>FontDialog</c> API surface.
/// </summary>
public class FontDialog : CommonDialog
{
    // ── WinForms-compatible properties ────────────────────────────────────────

    /// <summary>Gets or sets the selected font.</summary>
    public CanvasFont Font { get; set; } = new CanvasFont("Arial", 12f);

    /// <summary>Gets or sets the selected color (shown only when <see cref="ShowColor"/> is true).</summary>
    public Drawing.Color Color { get; set; } = Drawing.Color.Black;

    /// <summary>Gets or sets whether the Effects group box (strikethrough/underline/color) is shown.</summary>
    public bool ShowEffects { get; set; } = true;

    /// <summary>Gets or sets whether the color selector is shown in the Effects group.</summary>
    public bool ShowColor { get; set; } = false;

    /// <summary>Gets or sets whether the Apply button is shown.</summary>
    public bool ShowApplyButton { get; set; } = false;

    /// <summary>Gets or sets whether the help button is shown. Stub.</summary>
    public bool ShowHelp { get; set; } = false;

    /// <summary>Gets or sets whether only fixed-pitch fonts are listed. Stub.</summary>
    public bool FixedPitchOnly { get; set; } = false;

    /// <summary>Gets or sets whether only TrueType fonts are listed. Stub.</summary>
    public bool FontMustExist { get; set; } = false;

    /// <summary>Gets or sets the minimum selectable font size.</summary>
    public int MinSize { get; set; } = 1;

    /// <summary>Gets or sets the maximum selectable font size (0 = no limit).</summary>
    public int MaxSize { get; set; } = 0;

    /// <summary>Gets or sets whether scalable fonts are allowed. Stub.</summary>
    public bool AllowScriptChange { get; set; } = true;

    /// <summary>Gets or sets whether vector fonts are shown. Stub.</summary>
    public bool AllowVectorFonts { get; set; } = true;

    /// <summary>Gets or sets whether vertical fonts are shown. Stub.</summary>
    public bool AllowVerticalFonts { get; set; } = true;

    /// <summary>Raised when the user clicks the Apply button.</summary>
    public event EventHandler? Apply;

    // ── CommonDialog overrides ────────────────────────────────────────────────

    public override void Reset()
    {
        Font            = new CanvasFont("Arial", 12f);
        Color           = Drawing.Color.Black;
        ShowEffects     = true;
        ShowColor       = false;
        ShowApplyButton = false;
        ShowHelp        = false;
        FixedPitchOnly  = false;
        FontMustExist   = false;
        MinSize         = 1;
        MaxSize         = 0;
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
            onAccept: (f, c) => { Font = f; if (ShowColor) Color = c; accepted = true; tcs.TrySetResult(DialogResult.OK); },
            onApply:  (f, c) => Apply?.Invoke(this, EventArgs.Empty),
            onCancel: ()     => tcs.TrySetResult(DialogResult.Cancel));

        form.FormClosed += (_, __) =>
        {
            if (!tcs.Task.IsCompleted) tcs.TrySetResult(DialogResult.Cancel);
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

    private static readonly string[] CommonFamilies =
    {
        "Arial", "Arial Black", "Calibri", "Cambria", "Comic Sans MS", "Consolas",
        "Courier New", "Georgia", "Impact", "Lucida Console", "Segoe UI", "Tahoma",
        "Times New Roman", "Trebuchet MS", "Verdana",
    };

    private static readonly int[] CommonSizes =
    {
        6, 7, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72
    };

    private Form BuildForm(
        Action<CanvasFont, Drawing.Color> onAccept,
        Action<CanvasFont, Drawing.Color> onApply,
        Action onCancel)
    {
        const int W   = 500;
        const int H   = 400;
        const int Pad = 12;

        var dialog = new Form
        {
            Text        = "Font",
            Width       = W,
            Height      = H,
            Left        = 130,
            Top         = 90,
            AllowResize = false,
        };

        int innerW = W - Pad * 2 - 16;

        // Column widths
        int familyW = (int)(innerW * 0.50);
        int styleW  = (int)(innerW * 0.28);
        int sizeW   = innerW - familyW - styleW - 8;

        // ── Row 0: column headers ─────────────────────────────────────────────
        var familyLabel = new Label { Text = "Font:",       Left = Pad,                     Top = Pad,      Width = familyW, Height = 18 };
        var styleLabel  = new Label { Text = "Font style:", Left = Pad + familyW + 4,       Top = Pad,      Width = styleW,  Height = 18 };
        var sizeLabel   = new Label { Text = "Size:",       Left = Pad + familyW + styleW + 8, Top = Pad,  Width = sizeW,   Height = 18 };

        // ── Row 1: text boxes (type-ahead) ────────────────────────────────────
        int row1 = Pad + 20;
        var familyBox = new TextBox { Left = Pad,                        Top = row1, Width = familyW, Height = 24, Text = Font.Family };
        var styleBox  = new TextBox { Left = Pad + familyW + 4,          Top = row1, Width = styleW,  Height = 24, Text = FontStyleName(Font.Style) };
        var sizeBox   = new TextBox { Left = Pad + familyW + styleW + 8, Top = row1, Width = sizeW,   Height = 24, Text = ((int)Font.Size).ToString() };

        // ── Row 2: list boxes ─────────────────────────────────────────────────
        int row2 = row1 + 28;
        int listH = 120;
        var familyList = new ListBox { Left = Pad,                        Top = row2, Width = familyW, Height = listH };
        var styleList  = new ListBox { Left = Pad + familyW + 4,          Top = row2, Width = styleW,  Height = listH };
        var sizeList   = new ListBox { Left = Pad + familyW + styleW + 8, Top = row2, Width = sizeW,   Height = listH };

        foreach (var f in CommonFamilies) familyList.Items.Add(f);
        foreach (var s in new[] { "Regular", "Italic", "Bold", "Bold Italic" }) styleList.Items.Add(s);

        int effectsTop = row2 + listH + 10;

        // ── Effects ───────────────────────────────────────────────────────────
        var strikeBox     = new CheckBox { Text = "Strikeout", Left = Pad,      Top = effectsTop, Width = 100, Height = 22, Checked = Font.Style.HasFlag(CanvasFontStyle.Strikeout), Visible = ShowEffects };
        var underlineBox  = new CheckBox { Text = "Underline", Left = Pad + 108, Top = effectsTop, Width = 100, Height = 22, Checked = Font.Style.HasFlag(CanvasFontStyle.Underline), Visible = ShowEffects };

        // ── Preview panel ─────────────────────────────────────────────────────
        int previewTop = effectsTop + (ShowEffects ? 30 : 0);
        var previewLabel = new Label { Text = "Sample",      Left = Pad, Top = previewTop,     Width = 50,     Height = 18 };
        var previewPanel = new Panel
        {
            Left      = Pad,
            Top       = previewTop + 20,
            Width     = innerW,
            Height    = 46,
            BackColor = Drawing.Color.White,
        };
        var previewText = new Label
        {
            Text      = "AaBbYyZz",
            Left      = 8,
            Top       = 4,
            Width     = innerW - 16,
            Height    = 36,
            BackColor = Drawing.Color.White,
        };
        previewPanel.Controls.Add(previewText);

        // ── Button row ────────────────────────────────────────────────────────
        int btnTop  = H - 50;
        int btnW    = 76;
        int btnRight = Pad + innerW;
        var cancelButton = new Button { Text = "Cancel", Left = btnRight - btnW,         Top = btnTop, Width = btnW, Height = 28 };
        var okButton     = new Button { Text = "OK",     Left = btnRight - btnW * 2 - 4, Top = btnTop, Width = btnW, Height = 28 };
        var applyButton  = new Button { Text = "Apply",  Left = btnRight - btnW * 3 - 8, Top = btnTop, Width = btnW, Height = 28, Visible = ShowApplyButton };

        dialog.Controls.Add(familyLabel); dialog.Controls.Add(styleLabel); dialog.Controls.Add(sizeLabel);
        dialog.Controls.Add(familyBox);   dialog.Controls.Add(styleBox);   dialog.Controls.Add(sizeBox);
        dialog.Controls.Add(familyList);  dialog.Controls.Add(styleList);  dialog.Controls.Add(sizeList);
        dialog.Controls.Add(strikeBox);   dialog.Controls.Add(underlineBox);
        dialog.Controls.Add(previewLabel); dialog.Controls.Add(previewPanel);
        dialog.Controls.Add(okButton);    dialog.Controls.Add(cancelButton); dialog.Controls.Add(applyButton);

        // Select current values in lists
        SelectListItem(familyList, Font.Family);
        SelectListItem(styleList,  FontStyleName(Font.Style));
        var sizeItems = BuildSizeList();
        foreach (var s in sizeItems) sizeList.Items.Add(s.ToString());
        SelectListItem(sizeList, ((int)Font.Size).ToString());

        Drawing.Color currentColor = Color;

        CanvasFont CurrentFont()
        {
            var family  = familyBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(family)) family = "Arial";
            var styleName = styleBox.Text.Trim();
            var style   = ParseFontStyle(styleName, strikeBox.Checked, underlineBox.Checked);
            if (!float.TryParse(sizeBox.Text, out float sz)) sz = 12;
            sz = Math.Max(MinSize, MaxSize > 0 ? Math.Min(sz, MaxSize) : sz);
            return new CanvasFont(family, sz, style);
        }

        void UpdatePreview()
        {
            var f = CurrentFont();
            // Apply font to preview label — canvas rendering uses Control.Font already
            previewText.Font = f;

            // Build a descriptive sample that shows the style choices clearly
            var extras = new List<string>();
            if (f.Style.HasFlag(CanvasFontStyle.Bold))      extras.Add("Bold");
            if (f.Style.HasFlag(CanvasFontStyle.Italic))    extras.Add("Italic");
            if (f.Style.HasFlag(CanvasFontStyle.Strikeout)) extras.Add("Strike");
            if (f.Style.HasFlag(CanvasFontStyle.Underline)) extras.Add("Underline");
            var styleDesc = extras.Count > 0 ? string.Join(", ", extras) : "Regular";
            previewText.Text = $"AaBbYyZz  —  {f.Family} {(int)f.Size}pt {styleDesc}";
            previewPanel.Invalidate();
            dialog.Invalidate();
        }

        // Sync list → textbox
        familyList.SelectedIndexChanged += (_, __) =>
        {
            if (familyList.SelectedItem is string s) { familyBox.Text = s; UpdatePreview(); }
        };
        styleList.SelectedIndexChanged += (_, __) =>
        {
            if (styleList.SelectedItem is string s) { styleBox.Text = s; UpdatePreview(); }
        };
        sizeList.SelectedIndexChanged += (_, __) =>
        {
            if (sizeList.SelectedItem is string s) { sizeBox.Text = s; UpdatePreview(); }
        };

        // Sync textbox → list
        familyBox.KeyDown    += (_, e) => { if (e.KeyCode == Keys.Enter) { SelectListItem(familyList, familyBox.Text); UpdatePreview(); } };
        sizeBox.KeyDown      += (_, e) => { if (e.KeyCode == Keys.Enter) UpdatePreview(); };
        strikeBox.CheckedChanged    += (_, __) => UpdatePreview();
        underlineBox.CheckedChanged += (_, __) => UpdatePreview();

        okButton.Click     += (_, __) => onAccept(CurrentFont(), currentColor);
        applyButton.Click  += (_, __) => onApply(CurrentFont(),  currentColor);
        cancelButton.Click += (_, __) => { onCancel(); dialog.Close(); };

        UpdatePreview();
        return dialog;
    }

    // ── Font helpers ──────────────────────────────────────────────────────────

    private static string FontStyleName(CanvasFontStyle style)
    {
        bool bold   = style.HasFlag(CanvasFontStyle.Bold);
        bool italic = style.HasFlag(CanvasFontStyle.Italic);
        return (bold, italic) switch
        {
            (true,  true)  => "Bold Italic",
            (true,  false) => "Bold",
            (false, true)  => "Italic",
            _              => "Regular",
        };
    }

    private static CanvasFontStyle ParseFontStyle(string name, bool strikeout, bool underline)
    {
        var style = name.Trim() switch
        {
            "Bold"        => CanvasFontStyle.Bold,
            "Italic"      => CanvasFontStyle.Italic,
            "Bold Italic" => CanvasFontStyle.Bold | CanvasFontStyle.Italic,
            _             => CanvasFontStyle.Regular,
        };
        if (strikeout)  style |= CanvasFontStyle.Strikeout;
        if (underline)  style |= CanvasFontStyle.Underline;
        return style;
    }

    private static void SelectListItem(ListBox list, string value)
    {
        for (int i = 0; i < list.Items.Count; i++)
        {
            if (string.Equals(list.Items[i]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                list.SelectedIndex = i;
                return;
            }
        }
    }

    private List<int> BuildSizeList()
    {
        var sizes = CommonSizes.Where(s => s >= MinSize && (MaxSize == 0 || s <= MaxSize)).ToList();
        if (!sizes.Contains((int)Font.Size)) sizes.Add((int)Font.Size);
        sizes.Sort();
        return sizes;
    }
}
