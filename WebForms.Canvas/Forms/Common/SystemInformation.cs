namespace System.Windows.Forms;

/// <summary>
/// Provides information about the current system environment.
/// In CanvasForms most values return sensible defaults because the app runs in a
/// browser and has no access to OS system metrics.
/// Types here use <c>Canvas.Windows.Forms.Drawing</c> types (Size, Rectangle, Point)
/// available via global usings.
/// </summary>
public static class SystemInformation
{
    // ── Mouse ─────────────────────────────────────────────────────────────────
    public static int MouseButtons => 2;
    public static bool MousePresent => true;
    public static int MouseWheelScrollLines => 3;
    public static int MouseWheelScrollDelta => 120;
    public static int DoubleClickTime => 500;
    public static Size DoubleClickSize => new Size(4, 4);
    public static Size DragSize => new Size(4, 4);

    // ── Screen / display ──────────────────────────────────────────────────────
    public static Size PrimaryMonitorSize => new Size(1920, 1080);
    public static int VirtualScreenWidth => 1920;
    public static int VirtualScreenHeight => 1080;
    public static Rectangle VirtualScreen => new Rectangle(0, 0, VirtualScreenWidth, VirtualScreenHeight);
    public static Rectangle WorkingArea => new Rectangle(0, 0, VirtualScreenWidth, VirtualScreenHeight);
    public static int MonitorCount => 1;

    // ── UI metrics ────────────────────────────────────────────────────────────
    public static Size SmallIconSize => new Size(16, 16);
    public static Size IconSize => new Size(32, 32);
    public static int BorderSize => 1;
    public static int FixedFrameBorderSize => 3;
    public static Size FrameBorderSize => new Size(4, 4);
    public static int MenuHeight => 19;
    public static Size CursorSize => new Size(32, 32);
    public static int CaptionHeight => 23;
    public static int ToolWindowCaptionHeight => 15;
    public static Size ScrollBarSize => new Size(17, 17);
    public static int HorizontalScrollBarHeight => 17;
    public static int VerticalScrollBarWidth => 17;
    public static int HorizontalScrollBarThumbWidth => 17;
    public static int VerticalScrollBarThumbHeight => 17;

    // ── Environment ───────────────────────────────────────────────────────────
    public static bool HighContrast => false;
    public static bool NativeMouseWheelSupport => true;
    public static bool UserInteractive => true;
    public static bool TerminalServerSession => false;
    public static bool Network => true;
    public static bool DragFullWindows => true;

    // ── Keyboard ──────────────────────────────────────────────────────────────
    public static int KeyboardSpeed => 15;
    public static int KeyboardDelay => 1;
}
