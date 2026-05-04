using System.Windows.Forms;
using Canvas.Windows.Forms.Drawing;

namespace Canvas.Windows.Forms.Samples;

public class SampleDrawingForm : Form
{
    public SampleDrawingForm()
    {
        Text = "Drawing Sample - Windows Forms Canvas Clone";
        Width = 900;
        Height = 700;
        BackColor = Color.White;

        Paint += OnFormPaint;
    }

    private void OnFormPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        // ── Basic primitives ──────────────────────────────────────────────────
        using var redPen   = new Pen(Color.Red, 2);
        using var bluePen  = new Pen(Color.Blue, 3);
        using var blackPen = new Pen(Color.Black, 1);
        using var greenBrush  = new SolidBrush(Color.Green);
        using var yellowBrush = new SolidBrush(Color.Yellow);

        g.DrawLine(redPen, 10, 10, 200, 10);
        g.DrawLine(bluePen, 10, 30, 200, 150);
        g.DrawRectangle(redPen, 220, 10, 150, 100);
        g.FillRectangle(greenBrush, 400, 10, 150, 100);
        g.DrawEllipse(bluePen, 220, 130, 150, 100);
        g.FillEllipse(yellowBrush, 400, 130, 150, 100);

        g.DrawString("Hello, Windows Forms Canvas!", "Arial", 20, new SolidBrush(Color.Black), 10, 250);

        // ── RoundRect ─────────────────────────────────────────────────────────
        using var roundPen   = new Pen(Color.DarkBlue, 2);
        using var roundBrush = new SolidBrush(Color.FromArgb(100, 100, 200));
        g.DrawRoundRect(roundPen, 10, 290, 160, 60, 12);
        g.FillRoundRect(roundBrush, 190, 290, 160, 60, 16);

        // ── Arc ───────────────────────────────────────────────────────────────
        using var arcPen = new Pen(Color.Purple, 3);
        g.DrawArc(arcPen, 370, 280, 120, 90, 0, 270);      // 270° arc

        // ── Bezier ────────────────────────────────────────────────────────────
        using var bezierPen = new Pen(Color.Teal, 2);
        g.DrawBezier(bezierPen, 510, 290, 560, 250, 610, 380, 650, 290);

        // ── LinearGradientBrush on rectangle ─────────────────────────────────
        using var lgBrush = new LinearGradientBrush(
            new Rectangle(10, 380, 200, 60),
            Color.FromArgb(255, 80, 120),
            Color.FromArgb(80, 80, 255),
            LinearGradientMode.Horizontal);
        g.FillRectangle(lgBrush, 10, 380, 200, 60);
        g.DrawString("Linear gradient", "Arial", 13, new SolidBrush(Color.White), 20, 400);

        // ── LinearGradientBrush on FillRoundRect ──────────────────────────────
        using var lgRound = new LinearGradientBrush(
            new Point(230, 380), new Point(230, 440),
            Color.Orange, Color.DeepPink);
        g.FillRoundRect(lgRound, 230, 380, 160, 60, 14);

        // ── RadialGradientBrush on ellipse ────────────────────────────────────
        using var rgBrush = new RadialGradientBrush(
            new Point(480, 410), 50,
            Color.White, Color.DarkOrange);
        g.FillEllipse(rgBrush, 430, 380, 100, 60);

        // ── Polygon ───────────────────────────────────────────────────────────
        Point[] star = ComputeStar(620, 410, 45, 20, 5);
        using var starBrush = new LinearGradientBrush(
            new Point(575, 365), new Point(665, 455),
            Color.Yellow, Color.OrangeRed);
        g.FillPolygon(starBrush, star);
        g.DrawPolygon(new Pen(Color.DarkRed, 1), star);

        // ── GraphicsPath: rounded arrow ───────────────────────────────────────
        using var pathPen   = new Pen(Color.DarkGreen, 2);
        using var pathBrush = new SolidBrush(Color.FromArgb(180, 220, 160));
        var arrow = BuildArrowPath(760, 380, 100, 60);
        g.FillPath(pathBrush, arrow);
        g.DrawPath(pathPen, arrow);

        // ── GraphicsPath: wave curve ──────────────────────────────────────────
        using var wavePen = new Pen(Color.Navy, 2);
        var wave = BuildWavePath(10, 490, 860);
        g.DrawPath(wavePen, wave);

        // ── House scene (preserved from original) ─────────────────────────────
        DrawHouse(g);

        g.DrawString("RoundRect  |  Arc  |  Bezier  |  Gradient  |  Polygon  |  GraphicsPath",
            "Arial", 12, new SolidBrush(Color.Gray), 10, 560);
    }

    private static GraphicsPath BuildArrowPath(int x, int y, int w, int h)
    {
        var path = new GraphicsPath();
        // Arrow body + head
        int bw = w * 6 / 10, bh = h / 2, bx = x, by = y + h / 4;
        path.AddRectangle(new Rectangle(bx, by, bw, bh));
        // Arrow head triangle
        path.AddLines(new[]
        {
            new Point(x + bw, y),
            new Point(x + w,  y + h / 2),
            new Point(x + bw, y + h),
        });
        path.CloseFigure();
        return path;
    }

    private static GraphicsPath BuildWavePath(int x, int y, int totalWidth)
    {
        var path = new GraphicsPath();
        int waveCount = 6;
        int segW = totalWidth / (waveCount * 2);
        path.AddLine(x, y, x, y);  // move to start
        for (int i = 0; i < waveCount; i++)
        {
            int sx = x + i * segW * 2;
            // Up arc
            path.AddBezier(
                new Point(sx, y),
                new Point(sx + segW / 2, y - 25),
                new Point(sx + segW - segW / 2, y - 25),
                new Point(sx + segW, y));
            // Down arc
            path.AddBezier(
                new Point(sx + segW, y),
                new Point(sx + segW + segW / 2, y + 25),
                new Point(sx + segW * 2 - segW / 2, y + 25),
                new Point(sx + segW * 2, y));
        }
        return path;
    }

    private static Point[] ComputeStar(int cx, int cy, int outerR, int innerR, int points)
    {
        var pts = new Point[points * 2];
        for (int i = 0; i < points * 2; i++)
        {
            double angle = Math.PI / points * i - Math.PI / 2;
            double r = (i % 2 == 0) ? outerR : innerR;
            pts[i] = new Point(cx + (int)(r * Math.Cos(angle)), cy + (int)(r * Math.Sin(angle)));
        }
        return pts;
    }

    private static void DrawHouse(Graphics g)
    {
        using var brownBrush    = new SolidBrush(Color.FromArgb(139, 69, 19));
        using var darkBrownBrush= new SolidBrush(Color.FromArgb(101, 67, 33));
        using var cyanBrush     = new SolidBrush(Color.FromArgb(173, 216, 230));
        using var yellowBrush2  = new SolidBrush(Color.Yellow);
        using var blackPen      = new Pen(Color.Black, 2);

        // House body with vertical gradient
        using var wallGrad = new LinearGradientBrush(
            new Rectangle(100, 580, 200, 100),
            Color.FromArgb(180, 100, 40), Color.FromArgb(139, 69, 19),
            LinearGradientMode.Vertical);
        g.FillRectangle(wallGrad, 100, 580, 200, 100);

        // Roof
        g.DrawLine(blackPen, 100, 580, 200, 540);
        g.DrawLine(blackPen, 200, 540, 300, 580);
        g.DrawLine(blackPen, 100, 580, 300, 580);

        // Door (rounded top)
        g.FillRoundRect(darkBrownBrush, 160, 620, 60, 60, 8);
        g.DrawRoundRect(blackPen, 160, 620, 60, 60, 8);

        // Window
        g.FillRectangle(cyanBrush, 220, 590, 50, 50);
        g.DrawRectangle(blackPen, 220, 590, 50, 50);
        g.DrawLine(blackPen, 245, 590, 245, 640);
        g.DrawLine(blackPen, 220, 615, 270, 615);

        // Sun with radial gradient
        using var sunGrad = new RadialGradientBrush(new Point(660, 555), 40, Color.Yellow, Color.OrangeRed);
        g.FillEllipse(sunGrad, 620, 530, 80, 50);
    }
}
