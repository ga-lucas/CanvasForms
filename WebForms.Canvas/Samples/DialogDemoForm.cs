using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demo form for all common dialogs: Open/Save file, folder browser, color, font.
/// </summary>
public class DialogDemoForm : Form
{
    private ListBox? _log;
    private Label?   _resultLabel;

    public DialogDemoForm()
    {
        Text          = "Dialog Demos";
        Width         = 560;
        Height        = 500;
        AllowResize   = true;
        MinimumWidth  = 420;
        MinimumHeight = 360;
        BackColor     = Color.White;

        BuildUI();
    }

    private void BuildUI()
    {
        const int Pad  = 14;
        const int BtnH = 34;
        const int BtnW = 190;
        const int Gap  = 8;

        // ── Title ─────────────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "Common Dialog Demos",
            Left      = Pad, Top = Pad,
            Width     = Width - Pad * 2 - 16,
            Height    = 26,
            ForeColor = Color.FromArgb(26, 115, 232),
            BackColor = Color.FromArgb(240, 248, 255),
            TextAlign = ContentAlignment.TopCenter,
            Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        });

        int y = Pad + 34;

        // ── Button column (left) ──────────────────────────────────────────────
        Button MakeBtn(string text, int top)
        {
            var btn = new Button
            {
                Text   = text,
                Left   = Pad,
                Top    = top,
                Width  = BtnW,
                Height = BtnH,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
            };
            Controls.Add(btn);
            return btn;
        }

        var btnOpen   = MakeBtn("📂  Open File Dialog",  y);                        y += BtnH + Gap;
        var btnSave   = MakeBtn("💾  Save File Dialog",   y);                        y += BtnH + Gap;
        var btnFolder = MakeBtn("📁  Folder Browser",     y);                        y += BtnH + Gap;
        var btnColor  = MakeBtn("🎨  Color Dialog",       y);                        y += BtnH + Gap;
        var btnFont   = MakeBtn("🔤  Font Dialog",        y);                        y += BtnH + Gap;

        // ── Result panel (right / bottom) ────────────────────────────────────
        _resultLabel = new Label
        {
            Text      = "Result will appear here.",
            Left      = Pad + BtnW + Pad,
            Top       = Pad + 34,
            Width     = Width - (Pad + BtnW + Pad + Pad + 16),
            Height    = (BtnH + Gap) * 5 - Gap,
            BackColor = Color.FromArgb(248, 248, 248),
            ForeColor = Color.FromArgb(40, 40, 40),
            Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            TextAlign = ContentAlignment.TopLeft,
        };
        Controls.Add(_resultLabel);

        // ── Log list ─────────────────────────────────────────────────────────
        _log = new ListBox
        {
            Left   = Pad,
            Top    = y + 8,
            Width  = Width - Pad * 2 - 16,
            Height = Height - y - 50,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
        };
        Controls.Add(_log);

        // ── Wire buttons ─────────────────────────────────────────────────────
        btnOpen.Click += async (_, __) =>
        {
            var dlg = new OpenFileDialog
            {
                Title       = "Open a file",
                Filter      = "All supported|*.txt;*.cs;*.json;*.xml;*.md;*.png;*.jpg|Text|*.txt;*.cs;*.json|Images|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = true,
                EnableUpload = CanvasFormsOptions.EnableFileDialogUpload,
            };
            var result = await dlg.ShowDialogAsync();
            ShowResult("OpenFileDialog", result,
                result == DialogResult.OK
                    ? string.Join("\n", dlg.FileNames.Select(f => $"  {f}"))
                    : null);
        };

        btnSave.Click += async (_, __) =>
        {
            var dlg = new SaveFileDialog
            {
                Title           = "Save as…",
                Filter          = "Text file|*.txt|CSV|*.csv|All files (*.*)|*.*",
                DefaultExt      = "txt",
                OverwritePrompt = true,
            };
            var result = await dlg.ShowDialogAsync();
            ShowResult("SaveFileDialog", result,
                result == DialogResult.OK ? dlg.FileName : null);
        };

        btnFolder.Click += async (_, __) =>
        {
            var dlg = new FolderBrowserDialog
            {
                Description         = "Select an output folder",
                ShowNewFolderButton = true,
            };
            var result = await dlg.ShowDialogAsync();
            ShowResult("FolderBrowserDialog", result,
                result == DialogResult.OK ? dlg.SelectedPath : null);
        };

        btnColor.Click += async (_, __) =>
        {
            var dlg = new ColorDialog
            {
                Color        = Color.FromArgb(70, 130, 180),
                AllowFullOpen = true,
            };
            var result = await dlg.ShowDialogAsync();
            if (result == DialogResult.OK)
            {
                var c = dlg.Color;
                ShowResult("ColorDialog", result, $"R={c.R}  G={c.G}  B={c.B}  #{c.R:X2}{c.G:X2}{c.B:X2}");
                if (_resultLabel != null)
                    _resultLabel.BackColor = Color.FromArgb(c.R, c.G, c.B);
            }
            else
            {
                ShowResult("ColorDialog", result, null);
            }
        };

        btnFont.Click += async (_, __) =>
        {
            var dlg = new FontDialog
            {
                ShowEffects = true,
                ShowColor   = false,
                MinSize     = 6,
                MaxSize     = 72,
            };
            var result = await dlg.ShowDialogAsync();
            ShowResult("FontDialog", result,
                result == DialogResult.OK
                    ? $"Family: {dlg.Font.Family}\nSize: {dlg.Font.Size}pt\nStyle: {dlg.Font.Style}"
                    : null);
        };
    }

    private void ShowResult(string dialogName, DialogResult result, string? detail)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var status    = result == DialogResult.OK ? "OK" : result.ToString();
        var line      = $"[{timestamp}] {dialogName} → {status}";
        if (!string.IsNullOrWhiteSpace(detail)) line += $"\n{detail}";

        if (_resultLabel != null)
        {
            _resultLabel.Text      = line;
            _resultLabel.BackColor = result == DialogResult.OK
                ? Color.FromArgb(230, 255, 230)
                : Color.FromArgb(248, 248, 248);
        }

        _log?.Items.Add($"[{timestamp}] {dialogName}: {status}");
        if (_log is { Items.Count: > 0 })
            _log.SelectedIndex = _log.Items.Count - 1;

        Invalidate();
    }
}
