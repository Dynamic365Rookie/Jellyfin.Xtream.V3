using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Jellyfin.Xtream.V3.Utilities;

/// <summary>
/// Generates the plugin logo PNG.
/// </summary>
public static class LogoGenerator
{
    public static void GenerateLogo(string outputPath)
    {
        const int size = 256;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var center = size / 2;
        var radius = size / 2 - 12;

        // Gradient colors
        var cyan = Color.FromArgb(0, 212, 255);
        var orange = Color.FromArgb(255, 107, 53);

        // Draw outer circle border (Cyan)
        using (var pen = new Pen(cyan, 3))
        {
            g.DrawEllipse(pen, center - radius, center - radius, radius * 2, radius * 2);
        }

        // Draw inner circle with gradient fill
        var rect = new Rectangle(center - radius + 2, center - radius + 2, radius * 2 - 4, radius * 2 - 4);
        using (var brush = new LinearGradientBrush(rect,
            Color.FromArgb(30, 0, 212, 255),
            Color.FromArgb(30, 255, 107, 53), 45f))
        {
            g.FillEllipse(brush, rect);
        }

        // Draw wave patterns
        var waveY = center;
        var waveHeight = 15;

        // Wave 1 & 2 (Cyan)
        using (var pen = new Pen(cyan, 3) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            for (int wave = 0; wave < 2; wave++)
            {
                var startX = center - 80 + (wave * 30);
                var path = new GraphicsPath();
                path.AddBezier(startX, waveY, startX + 10, waveY - waveHeight,
                    startX + 20, waveY + waveHeight, startX + 30, waveY);
                g.DrawPath(pen, path);
            }
        }

        // Wave 3 (Orange)
        using (var pen = new Pen(orange, 3) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            var startX = center - 20;
            var path = new GraphicsPath();
            path.AddBezier(startX, waveY, startX + 10, waveY - waveHeight,
                startX + 20, waveY + waveHeight, startX + 30, waveY);
            g.DrawPath(pen, path);
        }

        // Play button (Orange triangle)
        var playPoints = new[]
        {
            new PointF(center + 40, waveY - 20),
            new PointF(center + 40, waveY + 20),
            new PointF(center + 65, waveY)
        };
        using (var brush = new SolidBrush(orange))
        {
            g.FillPolygon(brush, playPoints);
        }

        bitmap.Save(outputPath);
        Console.WriteLine($"Logo generated: {outputPath}");
    }
}
