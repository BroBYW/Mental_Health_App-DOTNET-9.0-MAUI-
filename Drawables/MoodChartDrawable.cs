using Microsoft.Maui.Graphics;
using PROJECT.Models;
using System.Collections.Generic;
using System.Linq;

namespace PROJECT.Drawables
{
    public class MoodChartDrawable : IDrawable
    {
        public IList<MoodPoint> Points { get; set; } = new List<MoodPoint>();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.FillColor = Colors.Transparent;
            canvas.FillRectangle(dirtyRect);

            // Axes
            canvas.StrokeSize = 2;
            canvas.StrokeColor = Color.FromArgb("#203229");
            float left = 40f, bottom = dirtyRect.Height - 40f, top = 40f, right = dirtyRect.Width - 20f;
            canvas.DrawLine(left, bottom, right, bottom); // X
            canvas.DrawLine(left, bottom, left, top);     // Y

            if (!Points.Any()) { canvas.RestoreState(); return; }

            // Map value (1..5) to y position (higher = better at top)
            float yScale = (bottom - top) / 4f; // steps between 1..5
            float xStep = (right - left) / (Points.Count - 1);

            // Polyline
            var path = new PathF();
            for (int i = 0; i < Points.Count; i++)
            {
                var pt = Points[i];
                float x = left + xStep * i;
                float y = bottom - (pt.Value - 1) * yScale;
                if (i == 0) path.MoveTo(x, y); else path.LineTo(x, y);
            }
            canvas.StrokeColor = Color.FromArgb("#2E7D32");
            canvas.StrokeSize = 3;
            canvas.DrawPath(path);

            // Points
            foreach (var (pt, i) in Points.Select((p, i) => (p, i)))
            {
                float x = left + xStep * i;
                float y = bottom - (pt.Value - 1) * yScale;
                canvas.FillColor = Color.FromArgb("#62C370");
                canvas.FillCircle(x, y, 5);
                canvas.StrokeColor = Colors.White;
                canvas.DrawCircle(x, y, 5);
            }

            canvas.RestoreState();
        }
    }
}