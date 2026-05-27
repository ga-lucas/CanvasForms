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
    public static readonly Cursor Arrow = new Cursor { Name = "default" };
    public static readonly Cursor AppStarting = new Cursor { Name = "progress" };
    public static readonly Cursor UpArrow = new Cursor { Name = "n-resize" };
    public static readonly Cursor PanEast = new Cursor { Name = "e-resize" };
    public static readonly Cursor PanNE = new Cursor { Name = "ne-resize" };
    public static readonly Cursor PanNorth = new Cursor { Name = "n-resize" };
    public static readonly Cursor PanNW = new Cursor { Name = "nw-resize" };
    public static readonly Cursor PanSE = new Cursor { Name = "se-resize" };
    public static readonly Cursor PanSouth = new Cursor { Name = "s-resize" };
    public static readonly Cursor PanSW = new Cursor { Name = "sw-resize" };
    public static readonly Cursor PanWest = new Cursor { Name = "w-resize" };

    public override string ToString() => $"[Cursor: {Name}]";
}

/// <summary>
/// Provides a set of standard cursor objects for use in WinForms API compatibility.
/// All members delegate to the corresponding <see cref="Cursor"/> static instances.
/// </summary>
public static class Cursors
{
    public static Cursor Default      => Cursor.Default;
    public static Cursor Arrow        => Cursor.Arrow;
    public static Cursor Hand         => Cursor.Hand;
    public static Cursor IBeam        => Cursor.IBeam;
    public static Cursor Cross        => Cursor.Cross;
    public static Cursor WaitCursor   => Cursor.WaitCursor;
    public static Cursor Help         => Cursor.Help;
    public static Cursor HSplit       => Cursor.HSplit;
    public static Cursor VSplit       => Cursor.VSplit;
    public static Cursor NoMove2D     => Cursor.NoMove2D;
    public static Cursor SizeAll      => Cursor.SizeAll;
    public static Cursor SizeNESW     => Cursor.SizeNESW;
    public static Cursor SizeNS       => Cursor.SizeNS;
    public static Cursor SizeNWSE     => Cursor.SizeNWSE;
    public static Cursor SizeWE       => Cursor.SizeWE;
    public static Cursor No           => Cursor.No;
    public static Cursor AppStarting  => Cursor.AppStarting;
    public static Cursor UpArrow      => Cursor.UpArrow;
    public static Cursor PanEast      => Cursor.PanEast;
    public static Cursor PanNE        => Cursor.PanNE;
    public static Cursor PanNorth     => Cursor.PanNorth;
    public static Cursor PanNW        => Cursor.PanNW;
    public static Cursor PanSE        => Cursor.PanSE;
    public static Cursor PanSouth     => Cursor.PanSouth;
    public static Cursor PanSW        => Cursor.PanSW;
    public static Cursor PanWest      => Cursor.PanWest;
}

