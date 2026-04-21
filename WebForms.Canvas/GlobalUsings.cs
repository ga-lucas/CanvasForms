// ---------------------------------------------------------------------------
// Global using aliases for the Canvas.Windows.Forms platform project.
//
// WHY THIS EXISTS
// ---------------
// This project hosts two Color types simultaneously:
//
//   System.Drawing.Color      – the real WinForms public API surface.
//                               BackColor/ForeColor/DefaultBackColor use this
//                               so translated apps compile without changes.
//
//   Canvas.Windows.Forms.     – internal rendering type. Used only inside
//   Drawing.Color               the platform's own OnPaint / Graphics code.
//                               Implicit conversions to/from System.Drawing.Color
//                               are defined on this type (Color.cs).
//
// The bare 'using Canvas.Windows.Forms.Drawing' is kept as a global so that
// Graphics, Pen, Rectangle, Size, Font, SolidBrush, SizeF, Point etc. are
// all available everywhere without per-file imports.
//
// The ONLY conflicting type is Color (both namespaces define it).
// That is resolved by the CanvasColor alias below: rendering code uses
// CanvasColor.FromArgb(...) and the public API uses System.Drawing.Color.
// ---------------------------------------------------------------------------

global using Canvas.Windows.Forms.Drawing;
global using CanvasColor = Canvas.Windows.Forms.Drawing.Color;
