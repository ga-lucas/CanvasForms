using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms ToolTip component
/// </summary>
public class ToolTip : System.ComponentModel.Component
{
    private readonly Dictionary<Control, string> _toolTips = new();
    private bool _active = true;
    private int _autoPopDelay = 5000;
    private int _initialDelay = 500;
    private int _reshowDelay = 100;
    private bool _showAlways = false;
    private bool _isBalloon = false;
    private ToolTipIcon _toolTipIcon = ToolTipIcon.None;
    private string _toolTipTitle = string.Empty;

    public bool Active { get => _active; set => _active = value; }
    public int AutoPopDelay { get => _autoPopDelay; set => _autoPopDelay = value; }
    public int InitialDelay { get => _initialDelay; set => _initialDelay = value; }
    public int ReshowDelay { get => _reshowDelay; set => _reshowDelay = value; }
    public bool ShowAlways { get => _showAlways; set => _showAlways = value; }
    public bool IsBalloon { get => _isBalloon; set => _isBalloon = value; }
    public ToolTipIcon ToolTipIcon { get => _toolTipIcon; set => _toolTipIcon = value; }
    public string ToolTipTitle { get => _toolTipTitle; set => _toolTipTitle = value; }

    /// <summary>
    /// Assigns a tooltip string to a control
    /// </summary>
    public void SetToolTip(Control control, string caption)
    {
        if (control == null) return;
        if (string.IsNullOrEmpty(caption))
            _toolTips.Remove(control);
        else
            _toolTips[control] = caption;
    }

    /// <summary>
    /// Gets the tooltip string for a control
    /// </summary>
    public string GetToolTip(Control control)
    {
        return _toolTips.TryGetValue(control, out var tip) ? tip : string.Empty;
    }

    /// <summary>
    /// Removes the tooltip for a control
    /// </summary>
    public void RemoveAll() => _toolTips.Clear();

    /// <summary>
    /// Shows the specified tooltip text for a control (rendering is handled externally)
    /// </summary>
    public void Show(string text, Control control) { /* Rendering stub */ }

    public void Show(string text, Control control, int duration) { /* Rendering stub */ }

    public void Hide(Control control) { /* Rendering stub */ }
}

public enum ToolTipIcon { None, Info, Warning, Error }
