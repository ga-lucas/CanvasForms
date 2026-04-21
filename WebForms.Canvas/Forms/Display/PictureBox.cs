using Canvas.Windows.Forms.Drawing;
using Microsoft.JSInterop;

namespace System.Windows.Forms;

public class PictureBox : Control
{
    private string _imageUrl = string.Empty;
    private PictureBoxSizeMode _sizeMode = PictureBoxSizeMode.Normal;
    private BorderStyle _borderStyle = BorderStyle.None;
    private bool _imageLoaded = false;

    public PictureBox()
    {
        Width = 100;
        Height = 100;
        BackColor = Color.FromArgb(240, 240, 240);
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
    /// Preload the image into the browser cache
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
                await form.TextMeasurementService.JSRuntime.InvokeVoidAsync(
                    "preloadImage", _imageUrl);
                _imageLoaded = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to preload image {_imageUrl}: {ex.Message}");
        }
    }

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
        var contentWidth = Math.Max(0, Width - (inset * 2));
        var contentHeight = Math.Max(0, Height - (inset * 2));

        // For now, we'll use simple size modes
        // In a full implementation, we'd need to know the actual image dimensions
        switch (_sizeMode)
        {
            case PictureBoxSizeMode.Normal:
                // Draw at original size from top-left
                return new Rectangle(inset, inset, contentWidth, contentHeight);

            case PictureBoxSizeMode.StretchImage:
                // Stretch to fill entire control
                return new Rectangle(inset, inset, contentWidth, contentHeight);

            case PictureBoxSizeMode.CenterImage:
                // Center the image (for now, just center within bounds)
                return new Rectangle(inset, inset, contentWidth, contentHeight);

            case PictureBoxSizeMode.Zoom:
                // Maintain aspect ratio and fit within bounds
                // For now, same as stretch (would need actual image dimensions)
                return new Rectangle(inset, inset, contentWidth, contentHeight);

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
