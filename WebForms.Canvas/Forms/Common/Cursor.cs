namespace System.Windows.Forms;

public class Cursor
{
    public string Name { get; set; } = "default";

    public static readonly Cursor Default = new Cursor { Name = "default" };
    public static readonly Cursor Hand = new Cursor { Name = "pointer" };
    public static readonly Cursor IBeam = new Cursor { Name = "text" };
    public static readonly Cursor Cross = new Cursor { Name = "crosshair" };
    public static readonly Cursor WaitCursor = new Cursor { Name = "wait" };
    public static readonly Cursor Help = new Cursor { Name = "help" };
    public static readonly Cursor HSplit = new Cursor { Name = "row-resize" };
    public static readonly Cursor VSplit = new Cursor { Name = "col-resize" };
    public static readonly Cursor NoMove2D = new Cursor { Name = "move" };
    public static readonly Cursor SizeAll = new Cursor { Name = "move" };
    public static readonly Cursor SizeNESW = new Cursor { Name = "nesw-resize" };
    public static readonly Cursor SizeNS = new Cursor { Name = "ns-resize" };
    public static readonly Cursor SizeNWSE = new Cursor { Name = "nwse-resize" };
    public static readonly Cursor SizeWE = new Cursor { Name = "ew-resize" };
    public static readonly Cursor No = new Cursor { Name = "not-allowed" };
}
