using Microsoft.JSInterop;

namespace System.Windows.Forms;

public class PictureBox : Control
{
    private string _imageUrl = string.Empty;
    private PictureBoxSizeMode _sizeMode = PictureBoxSizeMode.Normal;
    private BorderStyle _borderStyle = BorderStyle.None;
    private bool _imageLoaded = false;
    private int _naturalWidth = 0;
    private int _naturalHeight = 0;

    public event EventHandler? LoadCompleted;
#pragma warning disable CS0067
    public event EventHandler? LoadProgressChanged;
#pragma warning restore CS0067

    public PictureBox()
    {
        Width = 100;
        Height = 100;
        BackColor = Color.FromArgb(240, 240, 240);
    }

    /// <summary>
    /// Gets or sets the image by URL (WinForms compat: maps to ImageUrl)
    /// </summary>
    public string? Image
    {
        get => string.IsNullOrEmpty(_imageUrl) ? null : _imageUrl;
        set => ImageUrl = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the path or URL of the image to display.
    /// Equivalent to <see cref="ImageUrl"/>; provided for WinForms designer compatibility.
    /// </summary>
    public string? ImageLocation
    {
        get => string.IsNullOrEmpty(_imageUrl) ? null : _imageUrl;
        set => ImageUrl = value ?? string.Empty;
    }

    /// <summary>
    /// Placeholder shown while image loads (stub — renders nothing extra)
    /// </summary>
    public string? InitialImage { get; set; }

    /// <summary>
    /// Image shown on load error (stub — renders nothing extra)
    /// </summary>
    public string? ErrorImage { get; set; }

    /// <summary>
    /// Loads an image from the given URL (synchronous alias for setting ImageUrl)
    /// </summary>
    public void Load(string url)
    {
        ImageUrl = url;
        LoadCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Asynchronously loads an image from the given URL
    /// </summary>
    public async Task LoadAsync(string url)
    {
        ImageUrl = url;
        await PreloadImageAsync();
        LoadCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets the image URL to display in the PictureBox
    /// </summary>
    public string ImageUrl
    {
        get => _imageUrl;
        set
        {
            if (_imageUrl != value)
            {
                _imageUrl = value;
                _imageLoaded = false; // Reset loaded flag when URL changes
                _naturalWidth = 0;
                _naturalHeight = 0;

                // Preload image asynchronously if we have a URL
                if (!string.IsNullOrEmpty(_imageUrl))
                {
                    _ = PreloadImageAsync();
                }

                Invalidate();
            }
        }
    }

    /// <summary>
    /// Preload the image into the browser cache and capture its natural dimensions.
    /// </summary>
    private async Task PreloadImageAsync()
    {
        if (string.IsNullOrEmpty(_imageUrl) || _imageLoaded)
            return;

        try
        {
            // Get the form's JS runtime for image preloading
            var form = GetParentForm();
            if (form?.TextMeasurementService?.JSRuntime != null)
            {
                var js = form.TextMeasurementService.JSRuntime;
                await js.InvokeVoidAsync("preloadImage", _imageUrl);
                _imageLoaded = true;

                // Fetch the natural dimensions so SizeMode calculations are accurate.
                try
                {
                    var size = await js.InvokeAsync<ImageSizeResult>("getImageSize", _imageUrl);
                    if (size.Width > 0 && size.Height > 0)
                    {
                        _naturalWidth  = size.Width;
                        _naturalHeight = size.Height;
                        Invalidate();
                    }
                }
                catch { /* dimension fetch failure is non-fatal */ }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to preload image {_imageUrl}: {ex.Message}");
        }
    }

    private record ImageSizeResult(int Width, int Height);

    /// <summary>
    /// Get the parent Form
    /// </summary>
    private Form? GetParentForm()
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is Form form)
                return form;
            parent = parent.Parent;
        }
        return null;
    }

    /// <summary>
    /// Gets or sets how the image is displayed in the PictureBox
    /// </summary>
    public PictureBoxSizeMode SizeMode
    {
        get => _sizeMode;
        set
        {
            if (_sizeMode != value)
            {
                _sizeMode = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the border style of the PictureBox
    /// </summary>
    public BorderStyle BorderStyle
    {
        get => _borderStyle;
        set
        {
            if (_borderStyle != value)
            {
                _borderStyle = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Called when the control is added to a parent
    /// </summary>
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);

        // Trigger preload if we have an image URL but haven't loaded yet
        if (!string.IsNullOrEmpty(_imageUrl) && !_imageLoaded)
        {
            _ = PreloadImageAsync();
        }
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        DrawControlBackground(g);

        // Draw border based on BorderStyle
        if (_borderStyle == BorderStyle.FixedSingle)
        {
            g.DrawRectangle(new Pen(Color.FromArgb(172, 172, 172)), new Rectangle(0, 0, Width, Height));
        }
        else if (_borderStyle == BorderStyle.Fixed3D)
        {
            // Draw a simple 3D-style border (inset effect)
            var darkGray = new Pen(Color.FromArgb(128, 128, 128));
            var lightGray = new Pen(Color.FromArgb(223, 223, 223));

            // Top and left - dark
            g.DrawLine(darkGray, 0, 0, Width - 1, 0);
            g.DrawLine(darkGray, 0, 0, 0, Height - 1);

            // Bottom and right - light
            g.DrawLine(lightGray, Width - 1, 0, Width - 1, Height - 1);
            g.DrawLine(lightGray, 0, Height - 1, Width - 1, Height - 1);
        }

        if (!string.IsNullOrEmpty(_imageUrl))
        {
            var imageRect = CalculateImageRectangle();
            g.DrawImage(_imageUrl, imageRect);
        }

        DrawFocusRect(g);

        base.OnPaint(e);
    }

    private Rectangle CalculateImageRectangle()
    {
        // Account for border insets
        var inset = _borderStyle == BorderStyle.None ? 0 : (_borderStyle == BorderStyle.Fixed3D ? 2 : 1);
        var contentWidth  = Math.Max(0, Width  - inset * 2);
        var contentHeight = Math.Max(0, Height - inset * 2);

        // If natural dimensions are unknown, fall back to StretchImage so something renders.
        int natW = _naturalWidth  > 0 ? _naturalWidth  : contentWidth;
        int natH = _naturalHeight > 0 ? _naturalHeight : contentHeight;

        switch (_sizeMode)
        {
            case PictureBoxSizeMode.Normal:
                // Draw at natural size from top-left; clip if larger than control.
                return new Rectangle(inset, inset,
                    Math.Min(natW, contentWidth),
                    Math.Min(natH, contentHeight));

            case PictureBoxSizeMode.StretchImage:
                // Stretch to fill entire content area.
                return new Rectangle(inset, inset, contentWidth, contentHeight);

            case PictureBoxSizeMode.CenterImage:
            {
                // Center at natural size; clip if larger than control.
                int drawW = Math.Min(natW, contentWidth);
                int drawH = Math.Min(natH, contentHeight);
                int x = inset + (contentWidth  - drawW) / 2;
                int y = inset + (contentHeight - drawH) / 2;
                return new Rectangle(x, y, drawW, drawH);
            }

            case PictureBoxSizeMode.Zoom:
            {
                // Fit within content area while preserving aspect ratio.
                double scaleX = (double)contentWidth  / natW;
                double scaleY = (double)contentHeight / natH;
                double scale  = Math.Min(scaleX, scaleY);
                int drawW = Math.Max(1, (int)Math.Round(natW * scale));
                int drawH = Math.Max(1, (int)Math.Round(natH * scale));
                int x = inset + (contentWidth  - drawW) / 2;
                int y = inset + (contentHeight - drawH) / 2;
                return new Rectangle(x, y, drawW, drawH);
            }

            default:
                return new Rectangle(inset, inset, contentWidth, contentHeight);
        }
    }

    }

    /// <summary>
    /// Specifies how an image is positioned within a PictureBox
    /// </summary>
    public enum PictureBoxSizeMode
{
    /// <summary>
    /// The image is placed in the upper-left corner, and clipped if larger than the control
    /// </summary>
    Normal,

    /// <summary>
    /// The image is stretched or shrunk to fit the control
    /// </summary>
    StretchImage,

    /// <summary>
    /// The image is centered in the control
    /// </summary>
    CenterImage,

    /// <summary>
    /// The image is sized to fit the control while maintaining aspect ratio
    /// </summary>
    Zoom
}
