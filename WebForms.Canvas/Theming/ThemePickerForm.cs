using System.Windows.Forms;
using Canvas.Windows.Forms.Drawing;
using Canvas.Windows.Forms.Theming;
using SDC = System.Drawing.Color;

namespace Canvas.Windows.Forms.Theming;

/// <summary>
/// A dialog form that lets the user browse and apply built-in CanvasForms themes.
/// </summary>
public sealed class ThemePickerForm : Form
{
    private readonly ListBox _themeList;
    private readonly Panel   _swatchPanel;
    private readonly Label   _previewLabel;
    private readonly Button  _applyButton;
    private readonly Button  _closeButton;
    private readonly Label   _titleLabel;

    private string _selectedTheme = CanvasThemeRegistry.Classic;

    public ThemePickerForm()
    {
        Text            = "Choose Theme";
        Width           = 440;
        Height          = 340;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;

        // ── Title label ───────────────────────────────────────────────────────
        _titleLabel = new Label
        {
            Text     = "Select a theme:",
            Left     = 12,
            Top      = 12,
            Width    = 200,
            Height   = 20,
            AutoSize = false
        };

        // ── Theme list ────────────────────────────────────────────────────────
        _themeList = new ListBox
        {
            Left          = 12,
            Top           = 36,
            Width         = 180,
            Height        = 200,
            SelectionMode = SelectionMode.One
        };

        foreach (var name in CanvasThemeRegistry.BuiltInThemes)
            _themeList.Items.Add(name);

        // Pre-select the currently active theme name if known.
        var currentName = DetectCurrentThemeName();
        var idx = _themeList.Items.IndexOf(currentName);
        _themeList.SelectedIndex = idx >= 0 ? idx : 0;
        _selectedTheme = currentName;

        _themeList.SelectedIndexChanged += OnThemeListSelectionChanged;

        // ── Swatch panel ──────────────────────────────────────────────────────
        _swatchPanel = new Panel
        {
            Left   = 204,
            Top    = 36,
            Width  = 212,
            Height = 200
        };
        _swatchPanel.Paint += OnSwatchPaint;

        // ── Preview label (below swatch) ──────────────────────────────────────
        _previewLabel = new Label
        {
            Text      = currentName,
            Left      = 204,
            Top       = 242,
            Width     = 212,
            Height    = 20,
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // ── Buttons ───────────────────────────────────────────────────────────
        _applyButton = new Button
        {
            Text   = "Apply",
            Left   = 204,
            Top    = 268,
            Width  = 100,
            Height = 28
        };
        _applyButton.Click += OnApplyClick;

        _closeButton = new Button
        {
            Text   = "Close",
            Left   = 316,
            Top    = 268,
            Width  = 100,
            Height = 28
        };
        _closeButton.Click += OnCloseClick;

        // ── Layout ────────────────────────────────────────────────────────────
        Controls.Add(_titleLabel);
        Controls.Add(_themeList);
        Controls.Add(_swatchPanel);
        Controls.Add(_previewLabel);
        Controls.Add(_applyButton);
        Controls.Add(_closeButton);
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnThemeListSelectionChanged(object? sender, EventArgs e)
    {
        if (_themeList.SelectedItem is string name)
        {
            _selectedTheme      = name;
            _previewLabel.Text  = name;
            _swatchPanel.Invalidate();
        }
    }

    private void OnApplyClick(object? sender, EventArgs e)
    {
        CanvasThemeRegistry.Apply(_selectedTheme);
    }

    private void OnCloseClick(object? sender, EventArgs e)
    {
        Close();
    }

    // ── Swatch painting ───────────────────────────────────────────────────────

    private void OnSwatchPaint(object? sender, PaintEventArgs e)
    {
        var g    = e.Graphics;
        var info = BuildSwatchInfo(_selectedTheme);
        int y    = 4;

        foreach (var (label, back, fore) in info)
        {
            // Color swatch block
            using var backBrush = new SolidBrush(Color.FromArgb(back.A, back.R, back.G, back.B));
            g.FillRectangle(backBrush, 0, y, 30, 18);
            using var borderPen = new Pen(Color.FromArgb(80, 0, 0, 0), 1);
            g.DrawRectangle(borderPen, 0, y, 30, 18);

            // Label
            using var textBrush = new SolidBrush(Color.FromArgb(fore.A, fore.R, fore.G, fore.B));
            g.DrawString(label, "Arial", 11, textBrush, 36, y + 2);

            y += 24;
        }
    }

    /// <summary>Builds a small set of representative swatch rows for a named theme.</summary>
    private static List<(string label, SDC back, SDC fore)> BuildSwatchInfo(string themeName)
    {
        var t = CanvasThemeRegistry.Peek(themeName) ?? CanvasTheme.Current;

        return new List<(string, SDC, SDC)>
        {
            ("Control",    t.ControlBackColor,          t.ControlForeColor),
            ("Window",     t.WindowBackColor,           t.WindowForeColor),
            ("Title bar",  t.TitleBarGradientBottom,    SDC.FromArgb(255,255,255)),
            ("Button",     t.ButtonBackColor,           t.ButtonForeColor),
            ("Selection",  t.SelectionBackColor,        t.SelectionForeColor),
            ("Desktop",    t.DesktopBackColor,          t.ControlForeColor),
            ("Taskbar",    t.TaskbarGradientBottom,     t.TaskbarButtonInactiveForeColor),
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string DetectCurrentThemeName()
    {
        // Best-effort: compare desktop back color as a distinguishing heuristic.
        var t = CanvasTheme.Current;
        if (t.DesktopBackColor == SDC.FromArgb(0x1A, 0x1A, 0x2E)) return CanvasThemeRegistry.Dark;
        if (t.DesktopBackColor == SDC.FromArgb(0xD6, 0xE4, 0xF0)) return CanvasThemeRegistry.Light;
        return CanvasThemeRegistry.Classic;
    }
}
