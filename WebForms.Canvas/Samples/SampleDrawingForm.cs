using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class SampleDrawingForm : Form
{
    public SampleDrawingForm()
    {
        Text = "Drawing Sample - Windows Forms Canvas Clone";
        Width = 800;
        Height = 600;
        BackColor = Color.White;

        // Subscribe to Paint event
        Paint += OnFormPaint;
    }

    private void OnFormPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw some shapes
        using var redPen = new Pen(Color.Red, 2);
        using var bluePen = new Pen(Color.Blue, 3);
        using var greenBrush = new SolidBrush(Color.Green);
        using var yellowBrush = new SolidBrush(Color.Yellow);

        // Draw lines
        g.DrawLine(redPen, 10, 10, 200, 10);
        g.DrawLine(bluePen, 10, 30, 200, 150);

        // Draw rectangles
        g.DrawRectangle(redPen, 220, 10, 150, 100);
        g.FillRectangle(greenBrush, 400, 10, 150, 100);

        // Draw ellipses/circles
        g.DrawEllipse(bluePen, 220, 130, 150, 100);
        g.FillEllipse(yellowBrush, 400, 130, 150, 150);

        // Draw text
        g.DrawString("Hello, Windows Forms Canvas!", "Arial", 24, new SolidBrush(Color.Black), 50, 300);
        g.DrawString("Drawing primitives work!", "Arial", 18, new SolidBrush(Color.Blue), 50, 340);

        // Draw a simple scene
        DrawHouse(g);
    }

    private void DrawHouse(Graphics g)
    {
        // House body
        using var brownBrush = new SolidBrush(Color.FromArgb(139, 69, 19));
        g.FillRectangle(brownBrush, 100, 400, 200, 150);

        // Roof
        using var redBrush = new SolidBrush(Color.Red);
        using var blackPen = new Pen(Color.Black, 2);

        g.DrawLine(blackPen, 100, 400, 200, 350); // Left roof
        g.DrawLine(blackPen, 200, 350, 300, 400); // Right roof
        g.DrawLine(blackPen, 100, 400, 300, 400); // Bottom roof line

        // Door
        using var darkBrownBrush = new SolidBrush(Color.FromArgb(101, 67, 33));
        g.FillRectangle(darkBrownBrush, 160, 470, 60, 80);

        // Window
        using var cyanBrush = new SolidBrush(Color.FromArgb(173, 216, 230));
        g.FillRectangle(cyanBrush, 220, 430, 50, 50);
        g.DrawRectangle(blackPen, 220, 430, 50, 50);
        g.DrawLine(blackPen, 245, 430, 245, 480); // Window cross
        g.DrawLine(blackPen, 220, 455, 270, 455);

        // Sun
        using var yellowBrush2 = new SolidBrush(Color.Yellow);
        g.FillEllipse(yellowBrush2, 600, 370, 80, 80);
    }
}
