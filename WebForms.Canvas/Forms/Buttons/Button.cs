
namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms button control
/// </summary>
public class Button : ButtonBase
{
    public Button()
    {
        Width = 75;
        Height = 23;
        BackColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonBackColor;
        ForeColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonForeColor;
        Text = "Button";
    }

    private const int CornerRadius = 4;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);
        var state = GetButtonState();

        bool isFlat   = FlatStyle == FlatStyle.Flat;
        bool isPopup  = FlatStyle == FlatStyle.Popup;
        bool useFlatPath = isFlat || isPopup;

        // ── Background & border ───────────────────────────────────────────────
        if (useFlatPath)
        {
            _PaintFlatBackground(g, bounds, state, isFlat);
        }
        else
        {
            _PaintStandardBackground(g, bounds, state);
        }

        // ── Image ─────────────────────────────────────────────────────────────
        Rectangle textRect  = bounds;
        Rectangle imageRect = Rectangle.Empty;

        var effectiveImage = EffectiveImage;
        if (effectiveImage != null)
        {
            imageRect = _CalcImageRect(bounds, effectiveImage);

            if (TextImageRelation == TextImageRelation.ImageBeforeText)
            {
                int imgRight = imageRect.Right + 2;
                textRect = new Rectangle(imgRight, bounds.Y, bounds.Right - imgRight, bounds.Height);
            }
            else if (TextImageRelation == TextImageRelation.TextBeforeImage)
            {
                // measure text width to know how wide the text area is
                var ms = FindForm()?.TextMeasurementService;
                int fontSize = Font != null ? (int)Font.Size : 12;
                string fontFamily = Font?.Family ?? "Arial";
                int tw = ms?.MeasureTextEstimate(Text, fontFamily, fontSize) ?? (Text.Length * 7);
                textRect = new Rectangle(bounds.X + 2, bounds.Y, tw + 4, bounds.Height);
                int imgLeft = textRect.Right + 2;
                imageRect = new Rectangle(imgLeft, imageRect.Y, imageRect.Width, imageRect.Height);
            }
            else if (TextImageRelation == TextImageRelation.ImageAboveText)
            {
                int imgBottom = imageRect.Bottom + 2;
                textRect = new Rectangle(bounds.X, imgBottom, bounds.Width, bounds.Bottom - imgBottom);
            }
            else if (TextImageRelation == TextImageRelation.TextAboveImage)
            {
                var ms = FindForm()?.TextMeasurementService;
                int fontSize = Font != null ? (int)Font.Size : 12;
                string fontFamily = Font?.Family ?? "Arial";
                int th = ms?.GetFontHeightEstimate(fontFamily, fontSize) ?? 14;
                textRect = new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, th + 2);
                imageRect = new Rectangle(imageRect.X, textRect.Bottom + 2, imageRect.Width, imageRect.Height);
            }

            if (!imageRect.IsEmpty)
                g.DrawImage(effectiveImage, imageRect.X, imageRect.Y, imageRect.Width, imageRect.Height);
        }

        // ── Text ─────────────────────────────────────────────────────────────
        if (!string.IsNullOrEmpty(Text))
        {
            var textColor = Enabled
                ? (Color)ForeColor
                : (Color)Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonDisabledForeColor;

            var measureService = FindForm()?.TextMeasurementService;
            int fontSize   = Font != null ? (int)Font.Size : 12;
            string family  = Font?.Family ?? "Arial";
            int textWidth  = measureService?.MeasureTextEstimate(Text, family, fontSize) ?? (Text.Length * 7);
            int textHeight = measureService?.GetFontHeightEstimate(family, fontSize) ?? 14;
            int textX = textRect.X + (textRect.Width  - textWidth)  / 2;
            int textY = textRect.Y + (textRect.Height - textHeight) / 2;

            using var textBrush = new SolidBrush(textColor);
            g.DrawString(Text, family, fontSize, textBrush, textX, textY);
        }

        // ── Focus rectangle ───────────────────────────────────────────────────
        if (Focused && Enabled)
        {
            var focusRect = new Rectangle(3, 3, Width - 6, Height - 6);
            using var focusPen = new Pen(Canvas.Windows.Forms.Theming.CanvasTheme.Current.FocusRectColor) { DashStyle = DashStyle.Dot };
            g.DrawRoundRect(focusPen, focusRect, CornerRadius - 1);
        }

        base.OnPaint(e);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void _PaintStandardBackground(Graphics g, Rectangle bounds, ButtonState state)
    {
        Color colorTop, colorBottom, borderColor;
        switch (state)
        {
            case ButtonState.Disabled:
                colorTop    = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonBackColor;
                colorBottom = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonBackColor;
                borderColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.DisabledBorderColor;
                break;
            case ButtonState.Pushed:
                colorTop    = DarkenColor(BackColor, 0.18f);
                colorBottom = DarkenColor(BackColor, 0.08f);
                borderColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonBorderPressed;
                break;
            case ButtonState.Hot:
                colorTop    = LightenColor(BackColor, 0.22f);
                colorBottom = LightenColor(BackColor, 0.08f);
                borderColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonBorderHover;
                break;
            default:
                colorTop    = LightenColor(BackColor, 0.10f);
                colorBottom = DarkenColor(BackColor, 0.05f);
                borderColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonBorderNormal;
                break;
        }

        var topLeft    = new Point(bounds.Left, bounds.Top);
        var bottomLeft = new Point(bounds.Left, bounds.Bottom);
        using var bgBrush = new LinearGradientBrush(topLeft, bottomLeft, colorTop, colorBottom);
        g.FillRoundRect(bgBrush, bounds, CornerRadius);

        using var borderPen = new Pen(borderColor);
        g.DrawRoundRect(borderPen, bounds, CornerRadius);
    }

    private void _PaintFlatBackground(Graphics g, Rectangle bounds, ButtonState state, bool isFlat)
    {
        var fa = FlatAppearance;

        // Resolve background fill
        Color fillColor;
        if (state == ButtonState.Pushed && !fa.MouseDownBackColor.IsEmpty)
            fillColor = fa.MouseDownBackColor;
        else if (state == ButtonState.Hot && !fa.MouseOverBackColor.IsEmpty)
            fillColor = fa.MouseOverBackColor;
        else if (state == ButtonState.Hot && !isFlat)
            // Popup: show border + slight highlight on hover
            fillColor = LightenColor(BackColor, 0.12f);
        else if (state == ButtonState.Disabled)
            fillColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ButtonBackColor;
        else if (state == ButtonState.Hot)
            fillColor = LightenColor(BackColor, 0.12f);
        else if (state == ButtonState.Pushed)
            fillColor = DarkenColor(BackColor, 0.12f);
        else
            fillColor = BackColor;

        using var bgBrush = new SolidBrush(fillColor);
        g.FillRoundRect(bgBrush, bounds, 2);

        // Resolve border
        bool drawBorder = isFlat
            || state == ButtonState.Hot || state == ButtonState.Pushed || Focused;

        if (drawBorder && fa.BorderSize > 0)
        {
            Color bc = !fa.BorderColor.IsEmpty
                ? fa.BorderColor
                : (state == ButtonState.Disabled
                    ? (Color)Canvas.Windows.Forms.Theming.CanvasTheme.Current.DisabledBorderColor
                    : Canvas.Windows.Forms.Theming.CanvasTheme.Current.FocusRectColor);

            using var borderPen = new Pen(bc, fa.BorderSize);
            g.DrawRoundRect(borderPen, bounds, 2);
        }
    }

    /// <summary>
    /// Calculates the image destination rectangle inside the button bounds,
    /// honouring <see cref="ButtonBase.ImageAlign"/>.
    /// </summary>
    private Rectangle _CalcImageRect(Rectangle bounds, Image img)
    {
        int imgW = img.Width  > 0 ? img.Width  : 16;
        int imgH = img.Height > 0 ? img.Height : 16;

        int x, y;
        switch (ImageAlign)
        {
            case ContentAlignment.TopLeft:    x = 4;                          y = 4;                          break;
            case ContentAlignment.TopCenter:  x = (bounds.Width - imgW) / 2;  y = 4;                          break;
            case ContentAlignment.TopRight:   x = bounds.Width - imgW - 4;    y = 4;                          break;
            case ContentAlignment.MiddleLeft: x = 4;                          y = (bounds.Height - imgH) / 2;  break;
            case ContentAlignment.MiddleRight:x = bounds.Width - imgW - 4;    y = (bounds.Height - imgH) / 2;  break;
            case ContentAlignment.BottomLeft: x = 4;                          y = bounds.Height - imgH - 4;    break;
            case ContentAlignment.BottomCenter:x= (bounds.Width - imgW) / 2;  y = bounds.Height - imgH - 4;   break;
            case ContentAlignment.BottomRight:x = bounds.Width - imgW - 4;    y = bounds.Height - imgH - 4;    break;
            default: // MiddleCenter / Overlay
                x = (bounds.Width  - imgW) / 2;
                y = (bounds.Height - imgH) / 2;
                break;
        }
        return new Rectangle(bounds.X + x, bounds.Y + y, imgW, imgH);
    }
}
