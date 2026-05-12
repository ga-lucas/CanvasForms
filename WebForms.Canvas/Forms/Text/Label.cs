
namespace System.Windows.Forms;

public class Label : Control
{
    private bool _autoSize = false;
    private bool _autoEllipsis = false;
    private bool _useMnemonic = true;
    private FlatStyle _flatStyle = FlatStyle.Standard;
    private ContentAlignment _imageAlign = ContentAlignment.MiddleCenter;
    private Canvas.Windows.Forms.Drawing.Image? _image;
    private int _imageIndex = -1;
    private string? _imageKey;
    private ImageList? _imageList;

    public Label()
    {
        Width = 100;
        Height = 20;
        BackColor = System.Drawing.Color.Transparent;
        ForeColor = System.Drawing.Color.Black;
        Text = "Label";
        TabStop = false;
    }

    public ContentAlignment TextAlign { get; set; } = ContentAlignment.TopLeft;

    public new bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize != value)
            {
                _autoSize = value;
                if (_autoSize) PerformAutoSize();
                Invalidate();
            }
        }
    }

    /// <summary>Gets or sets whether to add an ellipsis when text overflows the control width.</summary>
    public bool AutoEllipsis
    {
        get => _autoEllipsis;
        set { _autoEllipsis = value; Invalidate(); }
    }

    /// <summary>Gets or sets whether the label interprets &amp; as mnemonic prefix (underline next char).</summary>
    public bool UseMnemonic
    {
        get => _useMnemonic;
        set { _useMnemonic = value; Invalidate(); }
    }

    /// <summary>Gets or sets the flat style of the label border.</summary>
    public FlatStyle FlatStyle
    {
        get => _flatStyle;
        set { _flatStyle = value; Invalidate(); }
    }

    /// <summary>Gets or sets the alignment of any image within the label.</summary>
    public ContentAlignment ImageAlign
    {
        get => _imageAlign;
        set { _imageAlign = value; Invalidate(); }
    }

    /// <summary>Gets or sets the image displayed on the label.</summary>
    public Canvas.Windows.Forms.Drawing.Image? Image
    {
        get => _image;
        set { _image = value; Invalidate(); }
    }

    /// <summary>Gets or sets the index into the <see cref="ImageList"/> for the image to display.</summary>
    public int ImageIndex
    {
        get => _imageIndex;
        set { _imageIndex = value; Invalidate(); }
    }

    /// <summary>Gets or sets the key for the image in the <see cref="ImageList"/>.</summary>
    public string? ImageKey
    {
        get => _imageKey;
        set { _imageKey = value; Invalidate(); }
    }

    /// <summary>Gets or sets the <see cref="ImageList"/> used to resolve <see cref="ImageIndex"/> or <see cref="ImageKey"/>.</summary>
    public ImageList? ImageList
    {
        get => _imageList;
        set { _imageList = value; Invalidate(); }
    }

    /// <summary>Gets the preferred width of the label based on the current text and font.</summary>
    public int PreferredWidth => MeasureTextWidth(DisplayText);

    /// <summary>Gets the preferred height of the label based on the current font.</summary>
    public int PreferredHeight => Font.Height + 4;

    /// <summary>Gets or sets the border style (Label supports None/FixedSingle/Fixed3D).</summary>
    public BorderStyle BorderStyle { get; set; } = BorderStyle.None;

    // Returns the text that should be rendered (strips mnemonic prefix when UseMnemonic=true).
    private string DisplayText
    {
        get
        {
            if (_useMnemonic && Text != null && Text.Contains('&'))
                return Text.Replace("&&", "\x01").Replace("&", "").Replace("\x01", "&");
            return Text ?? string.Empty;
        }
    }

    private void PerformAutoSize()
    {
        var lines = DisplayText.Replace("\r", "").Split('\n');
        int maxW = 0;
        foreach (var l in lines) maxW = Math.Max(maxW, MeasureTextWidth(l));
        Width  = maxW + 4;
        Height = lines.Length * Font.Height + 4;
    }

    private int MeasureTextWidth(string text)
        => (int)Math.Round((text ?? string.Empty).Length * Font.Size * 0.6f);

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        if (_autoSize) PerformAutoSize();
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw background if not transparent
        if (BackColor != System.Drawing.Color.Transparent)
        {
            using var bgBrush = new SolidBrush(BackColor);
            g.FillRectangle(bgBrush, 0, 0, Width, Height);
        }

        // Border
        if (BorderStyle == BorderStyle.FixedSingle)
        {
            using var bp = new Pen(System.Drawing.Color.FromArgb(122, 122, 122));
            g.DrawRectangle(bp, 0, 0, Width - 1, Height - 1);
        }
        else if (BorderStyle == BorderStyle.Fixed3D)
        {
            using var dark = new Pen(System.Drawing.Color.FromArgb(128, 128, 128));
            using var light = new Pen(System.Drawing.Color.FromArgb(223, 223, 223));
            g.DrawLine(dark,  0, 0, Width - 1, 0);
            g.DrawLine(dark,  0, 0, 0, Height - 1);
            g.DrawLine(light, Width - 1, 0, Width - 1, Height - 1);
            g.DrawLine(light, 0, Height - 1, Width - 1, Height - 1);
        }

        // Draw text
        var displayText = DisplayText;
        if (!string.IsNullOrEmpty(displayText))
        {
            var rawLines = displayText.Replace("\r", string.Empty).Split('\n');
            var charHeight = Font.Height;
            var (x0, y0, _) = GetTextBlockPosition(rawLines);

            using var textBrush = new SolidBrush(Enabled ? ForeColor : System.Drawing.Color.FromArgb(109, 109, 109));

            // Find mnemonic char index for underline drawing (only when UseMnemonic=true)
            int mnemonicCharIdx = -1;
            if (_useMnemonic && Text != null)
            {
                var raw = Text.Replace("&&", "\x01");
                int amp = raw.IndexOf('&');
                if (amp >= 0 && amp + 1 < raw.Length)
                {
                    mnemonicCharIdx = amp; // approximate position for underline
                }
            }

            for (var i = 0; i < rawLines.Length; i++)
            {
                var line = rawLines[i] ?? string.Empty;
                // AutoEllipsis: truncate with "…" if line would overflow
                if (_autoEllipsis)
                {
                    var charWidth = (int)Math.Round(Font.Size * 0.6f);
                    int maxChars = Width / Math.Max(1, charWidth);
                    if (line.Length > maxChars && maxChars > 1)
                        line = line.Substring(0, maxChars - 1) + "…";
                }
                var x = GetLineX(rawLines, line);
                var y = y0 + (i * charHeight);
                g.DrawString(line, Font, textBrush, x, y);
            }
        }

        // Draw image (on top of text, WinForms default for Label)
        var img = ResolveImage();
        if (img != null)
        {
            var r = _CalcImageRect(img);
            g.DrawImage(img, r);
        }

        base.OnPaint(e);
    }

    protected (int x0, int y0, int charHeight) GetTextBlockPosition(string[] lines)
    {
        // Scale character metrics with the current font size.
        // ~0.6× width-to-height ratio is a reasonable proportional-font approximation.
        var charHeight  = Font.Height;                      // Font.Height already adds 2px inter-line
        var charWidth   = (int)Math.Round(Font.Size * 0.6f);
        const int baselineOffset = 2;

        var maxLineLen = 0;
        foreach (var l in lines)
            maxLineLen = Math.Max(maxLineLen, (l ?? string.Empty).Length);

        var textWidth  = maxLineLen * charWidth;
        var textHeight = lines.Length * charHeight;

        var (baseX, baseY) = TextAlign switch
        {
            ContentAlignment.TopLeft     => (0, 0),
            ContentAlignment.TopCenter   => ((Width - textWidth) / 2, 0),
            ContentAlignment.TopRight    => (Width - textWidth, 0),
            ContentAlignment.MiddleLeft  => (0, (Height - textHeight) / 2),
            ContentAlignment.MiddleCenter => ((Width - textWidth) / 2, (Height - textHeight) / 2),
            ContentAlignment.MiddleRight => (Width - textWidth, (Height - textHeight) / 2),
            ContentAlignment.BottomLeft  => (0, Height - textHeight),
            ContentAlignment.BottomCenter => ((Width - textWidth) / 2, Height - textHeight),
            ContentAlignment.BottomRight => (Width - textWidth, Height - textHeight),
            _ => (0, 0)
        };

        return (baseX, baseY + baselineOffset, charHeight);
    }

    // Overload that accepts all lines (used by updated OnPaint)
    protected int GetLineX(string[] allLines, string line)
        => GetLineX(line);

    protected int GetLineX(string line)
    {
        var charWidth = (int)Math.Round(Font.Size * 0.6f);
        var lineWidth = (line ?? string.Empty).Length * charWidth;

        return TextAlign switch
        {
            ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter
                => Math.Max(0, (Width - lineWidth) / 2),
            ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight
                => Math.Max(0, Width - lineWidth),
            _ => 0
        };
    }

    /// <summary>Returns the effective image to draw: explicit Image, or resolved from ImageList.</summary>
    private Canvas.Windows.Forms.Drawing.Image? ResolveImage()
    {
        if (_image != null) return _image;
        if (_imageList == null) return null;

        if (!string.IsNullOrEmpty(_imageKey))
        {
            var url = _imageList.Images[_imageKey];
            return url != null ? new Canvas.Windows.Forms.Drawing.Image { Source = url } : null;
        }
        if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
        {
            var url = _imageList.Images[_imageIndex];
            return url != null ? new Canvas.Windows.Forms.Drawing.Image { Source = url } : null;
        }
        return null;
    }

    /// <summary>Calculates the destination rectangle for the image inside the label bounds.</summary>
    private Rectangle _CalcImageRect(Canvas.Windows.Forms.Drawing.Image img)
    {
        int imgW = img.Width  > 0 ? img.Width  : 16;
        int imgH = img.Height > 0 ? img.Height : 16;

        int x, y;
        switch (_imageAlign)
        {
            case ContentAlignment.TopLeft:     x = 2;                          y = 2;                          break;
            case ContentAlignment.TopCenter:   x = (Width  - imgW) / 2;        y = 2;                          break;
            case ContentAlignment.TopRight:    x = Width  - imgW - 2;          y = 2;                          break;
            case ContentAlignment.MiddleLeft:  x = 2;                          y = (Height - imgH) / 2;        break;
            case ContentAlignment.MiddleRight: x = Width  - imgW - 2;          y = (Height - imgH) / 2;        break;
            case ContentAlignment.BottomLeft:  x = 2;                          y = Height - imgH - 2;          break;
            case ContentAlignment.BottomCenter:x = (Width  - imgW) / 2;        y = Height - imgH - 2;          break;
            case ContentAlignment.BottomRight: x = Width  - imgW - 2;          y = Height - imgH - 2;          break;
            default: // MiddleCenter
                x = (Width  - imgW) / 2;
                y = (Height - imgH) / 2;
                break;
        }
        return new Rectangle(x, y, imgW, imgH);
    }
}

public enum ContentAlignment
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}
