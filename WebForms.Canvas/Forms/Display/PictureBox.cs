using WebForms.Canvas.Drawing;
using Microsoft.JSInterop;

namespace System.Windows.Forms;

public class PictureBox : Control
{
    private string _imageUrl = string.Empty;
    private PictureBoxSizeMode _sizeMode = PictureBoxSizeMode.Normal;
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

        // Draw background
        g.FillRectangle(new SolidBrush(BackColor), new Rectangle(0, 0, Width, Height));

        // Draw border
        g.DrawRectangle(new Pen(Color.FromArgb(172, 172, 172)), new Rectangle(0, 0, Width, Height));

        // Draw image if URL is set
        if (!string.IsNullOrEmpty(_imageUrl))
        {
            var imageRect = CalculateImageRectangle();
            g.DrawImage(_imageUrl, imageRect);
        }

        // Draw focus rectangle if focused
        if (Focused && Enabled)
        {
            var focusRect = new Rectangle(2, 2, Width - 4, Height - 4);
            using var focusPen = new Pen(Color.Black);
            g.DrawRectangle(focusPen, focusRect);
        }

        base.OnPaint(e);
    }

    private Rectangle CalculateImageRectangle()
    {
        // For now, we'll use simple size modes
        // In a full implementation, we'd need to know the actual image dimensions
        switch (_sizeMode)
        {
            case PictureBoxSizeMode.Normal:
                // Draw at original size from top-left
                return new Rectangle(1, 1, Width - 2, Height - 2);

            case PictureBoxSizeMode.StretchImage:
                // Stretch to fill entire control
                return new Rectangle(1, 1, Width - 2, Height - 2);

            case PictureBoxSizeMode.CenterImage:
                // Center the image (for now, just center within bounds)
                return new Rectangle(1, 1, Width - 2, Height - 2);

            case PictureBoxSizeMode.Zoom:
                // Maintain aspect ratio and fit within bounds
                // For now, same as stretch (would need actual image dimensions)
                return new Rectangle(1, 1, Width - 2, Height - 2);

            default:
                return new Rectangle(1, 1, Width - 2, Height - 2);
        }
    }

    protected internal override void OnGotFocus(EventArgs e)
    {
        Invalidate();
        base.OnGotFocus(e);
    }

    protected internal override void OnLostFocus(EventArgs e)
    {
        Invalidate();
        base.OnLostFocus(e);
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
