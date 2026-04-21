using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// A form that displays text files or images based on file extension.
/// </summary>
public class FileViewerForm : Form
{
    private readonly string _filePath;
    private readonly string _extension;
    private TextBox? _textBox;
    private PictureBox? _pictureBox;
    private Label? _statusLabel;

    public FileViewerForm(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _extension = Path.GetExtension(_filePath).ToLowerInvariant();

        Text = $"File Viewer - {Path.GetFileName(_filePath)}";
        Width = 800;
        Height = 600;
        BackColor = Color.White;
        AllowResize = true;
        AllowMove = true;
        MinimumWidth = 400;
        MinimumHeight = 300;

        InitializeControls();
        LoadFile();

        PerformLayout();
    }

    private void InitializeControls()
    {
        // Status label at the bottom
        _statusLabel = new Label
        {
            Text = $"File: {_filePath}",
            Left = 10,
            Top = Height - 60,
            Width = Width - 40,
            Height = 25,
            ForeColor = Color.FromArgb(60, 60, 60),
            BackColor = Color.FromArgb(240, 240, 240),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        Controls.Add(_statusLabel);

        // Decide which control to show based on file extension
        if (IsTextFile(_extension))
        {
            CreateTextViewer();
        }
        else if (IsImageFile(_extension))
        {
            CreateImageViewer();
        }
        else
        {
            CreateUnsupportedViewer();
        }
    }

    private void CreateTextViewer()
    {
        _textBox = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            Left = 10,
            Top = 10,
            Width = Width - 40,
            Height = Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Consolas", 9)
        };
        Controls.Add(_textBox);
    }

    private void CreateImageViewer()
    {
        _pictureBox = new PictureBox
        {
            Left = 10,
            Top = 10,
            Width = Width - 40,
            Height = Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(245, 245, 245)
        };
        Controls.Add(_pictureBox);
    }

    private void CreateUnsupportedViewer()
    {
        var label = new Label
        {
            Text = $"Unsupported file type: {_extension}\n\nSupported types:\n- Text: .txt, .cs, .json, .xml, .md, .log\n- Images: .png, .jpg, .jpeg, .gif, .bmp",
            Left = 10,
            Top = 10,
            Width = Width - 40,
            Height = Height - 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ForeColor = Color.FromArgb(120, 120, 120),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(label);
    }

    private async void LoadFile()
    {
        try
        {
            if (_textBox != null)
            {
                await LoadTextFile();
            }
            else if (_pictureBox != null)
            {
                await LoadImageFile();
            }
        }
        catch (Exception ex)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = $"Error loading file: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
        }
    }

    private async Task LoadTextFile()
    {
        if (_textBox == null) return;

        try
        {
            // Read file from host filesystem
            var content = await File.ReadAllTextAsync(_filePath);
            _textBox.Text = content;

            if (_statusLabel != null)
            {
                var lines = content.Split('\n').Length;
                var chars = content.Length;
                _statusLabel.Text = $"File: {_filePath} | {lines} lines, {chars} characters";
            }
        }
        catch (Exception ex)
        {
            _textBox.Text = $"Error reading file: {ex.Message}";
            _textBox.ForeColor = Color.Red;
        }
    }

    private async Task LoadImageFile()
    {
        if (_pictureBox == null) return;

        try
        {
            // Read image file from host filesystem
            var bytes = await File.ReadAllBytesAsync(_filePath);

            if (_statusLabel != null)
            {
                _statusLabel.Text = $"File: {_filePath} | {bytes.Length / 1024.0:F2} KB";
            }

            // Convert to base64 data URL for browser display
            var base64 = Convert.ToBase64String(bytes);
            var mimeType = GetMimeType(_extension);
            var dataUrl = $"data:{mimeType};base64,{base64}";

            _pictureBox.ImageUrl = dataUrl;
        }
        catch (Exception ex)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = $"Error loading image: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
        }
    }

    private static bool IsTextFile(string extension)
    {
        return extension switch
        {
            ".txt" => true,
            ".cs" => true,
            ".json" => true,
            ".xml" => true,
            ".md" => true,
            ".log" => true,
            ".html" => true,
            ".css" => true,
            ".js" => true,
            ".config" => true,
            ".ini" => true,
            ".yml" => true,
            ".yaml" => true,
            _ => false
        };
    }

    private static bool IsImageFile(string extension)
    {
        return extension switch
        {
            ".png" => true,
            ".jpg" => true,
            ".jpeg" => true,
            ".gif" => true,
            ".bmp" => true,
            ".svg" => true,
            ".webp" => true,
            _ => false
        };
    }

    private static string GetMimeType(string extension)
    {
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
