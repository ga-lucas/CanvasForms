using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents the method that will handle events that have no event data
/// </summary>
public delegate void CancelEventHandler(object? sender, CancelEventArgs e);

/// <summary>
/// Provides data for a cancelable event
/// </summary>
public class CancelEventArgs : EventArgs
{
    public bool Cancel { get; set; }

    public CancelEventArgs() { }

    public CancelEventArgs(bool cancel)
    {
        Cancel = cancel;
    }
}

/// <summary>
/// Specifies the reason that a form was closed
/// </summary>
public enum CloseReason
{
    None = 0,
    WindowsShutDown = 1,
    MdiFormClosing = 2,
    UserClosing = 3,
    TaskManagerClosing = 4,
    FormOwnerClosing = 5,
    ApplicationExitCall = 6
}

/// <summary>
/// Provides data for the FormClosing event
/// </summary>
public class FormClosingEventArgs : CancelEventArgs
{
    public CloseReason CloseReason { get; }

    public FormClosingEventArgs(CloseReason closeReason, bool cancel = false)
        : base(cancel)
    {
        CloseReason = closeReason;
    }
}

/// <summary>
/// Provides data for the FormClosed event
/// </summary>
public class FormClosedEventArgs : EventArgs
{
    public CloseReason CloseReason { get; }

    public FormClosedEventArgs(CloseReason closeReason)
    {
        CloseReason = closeReason;
    }
}

/// <summary>
/// Represents the method that handles FormClosing events
/// </summary>
public delegate void FormClosingEventHandler(object? sender, FormClosingEventArgs e);

/// <summary>
/// Represents the method that handles FormClosed events
/// </summary>
public delegate void FormClosedEventHandler(object? sender, FormClosedEventArgs e);

/// <summary>
/// Provides data for the PreviewKeyDown event
/// </summary>
public class PreviewKeyDownEventArgs : EventArgs
{
    public Keys KeyData { get; }
    public Keys KeyCode { get; }
    public Keys Modifiers { get; }
    public bool Alt { get; }
    public bool Control { get; }
    public bool Shift { get; }
    public bool IsInputKey { get; set; }

    public PreviewKeyDownEventArgs(Keys keyData)
    {
        KeyData = keyData;
        // Extract key code (remove modifier bits)
        KeyCode = keyData & (Keys)0xFFFF;
        // Extract modifiers
        Modifiers = keyData & ((Keys)0x10000 | (Keys)0x20000 | (Keys)0x40000);
        Alt = (keyData & (Keys)0x40000) != 0;
        Control = (keyData & (Keys)0x20000) != 0;
        Shift = (keyData & (Keys)0x10000) != 0;
    }
}

public delegate void PreviewKeyDownEventHandler(object? sender, PreviewKeyDownEventArgs e);

/// <summary>
/// Provides data for layout events
/// </summary>
public class LayoutEventArgs : EventArgs
{
    public Control? AffectedControl { get; }
    public string? AffectedProperty { get; }

    public LayoutEventArgs(Control? affectedControl, string? affectedProperty)
    {
        AffectedControl = affectedControl;
        AffectedProperty = affectedProperty;
    }
}

public delegate void LayoutEventHandler(object? sender, LayoutEventArgs e);

/// <summary>
/// Provides data for control add/remove events
/// </summary>
public class ControlEventArgs : EventArgs
{
    public Control? Control { get; }

    public ControlEventArgs(Control? control)
    {
        Control = control;
    }
}

public delegate void ControlEventHandler(object? sender, ControlEventArgs e);

/// <summary>
/// Provides data for drag and drop events
/// </summary>
public class DragEventArgs : EventArgs
{
    public object? Data { get; }
    public int KeyState { get; }
    public int X { get; }
    public int Y { get; }
    public DragDropEffects AllowedEffect { get; }
    public DragDropEffects Effect { get; set; }

    public DragEventArgs(object? data, int keyState, int x, int y, DragDropEffects allowedEffect, DragDropEffects effect)
    {
        Data = data;
        KeyState = keyState;
        X = x;
        Y = y;
        AllowedEffect = allowedEffect;
        Effect = effect;
    }
}

public delegate void DragEventHandler(object? sender, DragEventArgs e);

[Flags]
public enum DragDropEffects
{
    None = 0,
    Copy = 1,
    Move = 2,
    Link = 4,
    Scroll = -2147483648,
    All = Copy | Move | Link | Scroll
}

/// <summary>
/// Provides data for the GiveFeedback event
/// </summary>
public class GiveFeedbackEventArgs : EventArgs
{
    public DragDropEffects Effect { get; }
    public bool UseDefaultCursors { get; set; }

    public GiveFeedbackEventArgs(DragDropEffects effect, bool useDefaultCursors)
    {
        Effect = effect;
        UseDefaultCursors = useDefaultCursors;
    }
}

public delegate void GiveFeedbackEventHandler(object? sender, GiveFeedbackEventArgs e);

/// <summary>
/// Provides data for the QueryContinueDrag event
/// </summary>
public class QueryContinueDragEventArgs : EventArgs
{
    public int KeyState { get; }
    public bool EscapePressed { get; }
    public DragAction Action { get; set; }

    public QueryContinueDragEventArgs(int keyState, bool escapePressed, DragAction action)
    {
        KeyState = keyState;
        EscapePressed = escapePressed;
        Action = action;
    }
}

public delegate void QueryContinueDragEventHandler(object? sender, QueryContinueDragEventArgs e);

public enum DragAction
{
    Continue,
    Drop,
    Cancel
}

/// <summary>
/// Provides data for the HelpRequested event
/// </summary>
public class HelpEventArgs : EventArgs
{
    public Point MousePos { get; }
    public bool Handled { get; set; }

    public HelpEventArgs(Point mousePos)
    {
        MousePos = mousePos;
    }
}

public delegate void HelpEventHandler(object? sender, HelpEventArgs e);

/// <summary>
/// Provides data for the ChangeUICues event
/// </summary>
public class UICuesEventArgs : EventArgs
{
    public UICues Changed { get; }
    public UICues ChangeFocus => Changed & UICues.ChangeFocus;
    public UICues ChangeKeyboard => Changed & UICues.ChangeKeyboard;
    public bool ShowFocus => (Changed & UICues.ShowFocus) != 0;
    public bool ShowKeyboard => (Changed & UICues.ShowKeyboard) != 0;

    public UICuesEventArgs(UICues uicues)
    {
        Changed = uicues;
    }
}

public delegate void UICuesEventHandler(object? sender, UICuesEventArgs e);

[Flags]
public enum UICues
{
    None = 0,
    ShowFocus = 1,
    ShowKeyboard = 2,
    Shown = ShowFocus | ShowKeyboard,
    ChangeFocus = 4,
    ChangeKeyboard = 8,
    Changed = ChangeFocus | ChangeKeyboard
}

/// <summary>
/// Provides data for the QueryAccessibilityHelp event
/// </summary>
public class QueryAccessibilityHelpEventArgs : EventArgs
{
    public string? HelpNamespace { get; set; }
    public string? HelpString { get; set; }
    public string? HelpKeyword { get; set; }

    public QueryAccessibilityHelpEventArgs()
    {
    }

    public QueryAccessibilityHelpEventArgs(string? helpNamespace, string? helpString, string? helpKeyword)
    {
        HelpNamespace = helpNamespace;
        HelpString = helpString;
        HelpKeyword = helpKeyword;
    }
}

public delegate void QueryAccessibilityHelpEventHandler(object? sender, QueryAccessibilityHelpEventArgs e);

/// <summary>
/// Provides data for the Invalidated event
/// </summary>
public class InvalidateEventArgs : EventArgs
{
    public Rectangle InvalidRect { get; }

    public InvalidateEventArgs(Rectangle invalidRect)
    {
        InvalidRect = invalidRect;
    }
}

public delegate void InvalidateEventHandler(object? sender, InvalidateEventArgs e);

/// <summary>
/// Represents a Windows message
/// </summary>
public struct Message
{
    public IntPtr HWnd { get; set; }
    public int Msg { get; set; }
    public IntPtr WParam { get; set; }
    public IntPtr LParam { get; set; }
    public IntPtr Result { get; set; }
}

/// <summary>
/// Represents a bitmap image (stub)
/// </summary>
public class Bitmap
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Bitmap(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
