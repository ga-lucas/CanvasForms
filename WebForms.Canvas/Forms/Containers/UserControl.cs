namespace System.Windows.Forms;

/// <summary>
/// Base class for all user-defined composite controls.
/// Designer-generated code always subclasses UserControl; it must be present for
/// translated apps to compile and run without changes.
/// </summary>
public class UserControl : ContainerControl
{
    public UserControl()
    {
        IsMouseRoutingContainer = true;
        AutoScaleMode = AutoScaleMode.Font;
    }

    // ── Load event ────────────────────────────────────────────────────────────
    // Fired once after the control is fully initialised (mirrors Form.Load).

    public event EventHandler? Load;

    protected virtual void OnLoad(EventArgs e) => Load?.Invoke(this, e);

    /// <summary>
    /// Called by the hosting infrastructure after the control tree is ready.
    /// Mirrors the WinForms pattern used by Form.OnLoad.
    /// </summary>
    public void RaiseLoad() => OnLoad(EventArgs.Empty);

    // ── AutoSize ──────────────────────────────────────────────────────────────

    private bool _autoSize;
    private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowOnly;

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

    public AutoSizeMode AutoSizeMode
    {
        get => _autoSizeMode;
        set
        {
            if (_autoSizeMode != value)
            {
                _autoSizeMode = value;
                if (_autoSize) PerformAutoSize();
            }
        }
    }

    private System.Drawing.Size _minimumSize = System.Drawing.Size.Empty;
    private System.Drawing.Size _maximumSize = System.Drawing.Size.Empty;

    public new System.Drawing.Size MinimumSize
    {
        get => _minimumSize;
        set { _minimumSize = value; if (_autoSize) PerformAutoSize(); }
    }

    public new System.Drawing.Size MaximumSize
    {
        get => _maximumSize;
        set { _maximumSize = value; if (_autoSize) PerformAutoSize(); }
    }

    private void PerformAutoSize()
    {
        // Calculate the bounding box of all children.
        var right = 0;
        var bottom = 0;
        foreach (Control c in Controls)
        {
            if (!c.Visible) continue;
            right  = Math.Max(right,  c.Left + c.Width);
            bottom = Math.Max(bottom, c.Top  + c.Height);
        }

        var preferred = new System.Drawing.Size(right, bottom);

        // Apply min/max constraints.
        if (_minimumSize.Width  > 0) preferred.Width  = Math.Max(preferred.Width,  _minimumSize.Width);
        if (_minimumSize.Height > 0) preferred.Height = Math.Max(preferred.Height, _minimumSize.Height);
        if (_maximumSize.Width  > 0) preferred.Width  = Math.Min(preferred.Width,  _maximumSize.Width);
        if (_maximumSize.Height > 0) preferred.Height = Math.Min(preferred.Height, _maximumSize.Height);

        // GrowOnly: never shrink below the current design-time size.
        if (_autoSizeMode == AutoSizeMode.GrowOnly)
        {
            preferred.Width  = Math.Max(preferred.Width,  Width);
            preferred.Height = Math.Max(preferred.Height, Height);
        }

        Width  = preferred.Width;
        Height = preferred.Height;
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        DrawControlBackground(e.Graphics);

        if (BorderStyle == BorderStyle.FixedSingle)
        {
            using var pen = new Pen(Color.FromArgb(122, 122, 122));
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
        else if (BorderStyle == BorderStyle.Fixed3D)
        {
            using var darkPen  = new Pen(Color.FromArgb(100, 100, 100));
            using var lightPen = new Pen(Color.FromArgb(220, 220, 220));
            e.Graphics.DrawLine(darkPen,  0, 0, Width - 2, 0);
            e.Graphics.DrawLine(darkPen,  0, 0, 0, Height - 2);
            e.Graphics.DrawLine(lightPen, Width - 1, 0, Width - 1, Height - 1);
            e.Graphics.DrawLine(lightPen, 0, Height - 1, Width - 1, Height - 1);
        }

        base.OnPaint(e);
    }

    // ── Create-control lifecycle ──────────────────────────────────────────────

    private bool _createControlFired;

    /// <summary>
    /// Called once when the control's handle is first created (after child controls are added).
    /// Translated designer code overrides this to run post-init logic.
    /// </summary>
    protected virtual void OnCreateControl() { }

    /// <summary>
    /// Forces handle creation and fires <see cref="OnCreateControl"/> the first time it is called.
    /// </summary>
    public void CreateControl()
    {
        if (_createControlFired) return;
        _createControlFired = true;
        OnCreateControl();
    }

    // ── AutoScale designer support ────────────────────────────────────────────

    /// <summary>
    /// Read/write to satisfy <c>InitializeComponent()</c> designer-generated assignments.
    /// The canvas layer does not perform DPI scaling at the control level.
    /// </summary>
    public System.Drawing.SizeF AutoScaleDimensions { get; set; } = new System.Drawing.SizeF(6f, 13f);

    /// <summary>Required by the Windows Form Designer — override to add child controls.</summary>
    protected virtual void InitializeComponent() { }

    public BorderStyle BorderStyle { get; set; } = BorderStyle.None;
}

public enum AutoSizeMode
{
    GrowAndShrink = 0,
    GrowOnly = 1,
}
