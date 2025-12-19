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
            canvas.Antialias = true;


            // 1. Define margins (Increased top margin slightly to accommodate title if needed)
            float left = 100f, bottom = dirtyRect.Height - 40f, top = 40f, right = dirtyRect.Width - 20f;

            // 2. Draw Subtle Grid Lines (Dashed)
            canvas.StrokeSize = 1;
            canvas.StrokeColor = Color.FromArgb("#E0E0E0");
            canvas.StrokeDashPattern = new float[] { 4, 4 };

            float yScale = (bottom - top) / 4f;

            // Draw horizontal grid lines
            for (int i = 0; i < 5; i++)
            {
                float y = bottom - i * yScale;
                canvas.DrawLine(left, y, right, y);
            }

            canvas.StrokeDashPattern = null;

            // 3. Draw Y-Axis Labels
            canvas.FontSize = 14;
            canvas.FontColor = Colors.Black;
            string[] moodLabels = { "☹️ Awful", "🙁 Bad", "😐 Meh", "🙂 Good", "😀 Rad" };

            for (int i = 0; i < moodLabels.Length; i++)
            {
                float y = bottom - i * yScale;
                canvas.DrawString(moodLabels[i], 0, y - 10, left - 10, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
            }

            if (!Points.Any()) { canvas.RestoreState(); return; }

            // 4. Calculate Coordinates
            float xStep = (right - left) / (Points.Count - 1);
            var path = new PathF();
            var coordinates = new List<PointF>();

            for (int i = 0; i < Points.Count; i++)
            {
                float x = left + xStep * i;
                float y = bottom - (Points[i].Value - 1) * yScale;
                coordinates.Add(new PointF(x, y));
            }

            // 5. Draw Gradient Area
            var areaPath = new PathF();
            areaPath.MoveTo(coordinates[0].X, bottom);
            foreach (var p in coordinates) areaPath.LineTo(p);
            areaPath.LineTo(coordinates.Last().X, bottom);
            areaPath.Close();

            var gradient = new LinearGradientPaint
            {
                StartColor = Color.FromArgb("#8062C370"),
                EndColor = Color.FromArgb("#0062C370"),
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };

            canvas.SetFillPaint(gradient, dirtyRect);
            canvas.FillPath(areaPath);

            // 6. Draw Main Line
            path.MoveTo(coordinates[0]);
            foreach (var p in coordinates.Skip(1)) path.LineTo(p);

            canvas.StrokeColor = Color.FromArgb("#2E7D32");
            canvas.StrokeSize = 4;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.DrawPath(path);

            // 7. Draw Decorative Points
            canvas.FontSize = 12;
            canvas.FontColor = Colors.Gray;

            for (int i = 0; i < coordinates.Count; i++)
            {
                var pt = coordinates[i];

                // Outer Glow
                canvas.FillColor = Color.FromArgb("#4062C370");
                canvas.FillCircle(pt.X, pt.Y, 10);

                // Inner Dot
                canvas.FillColor = Colors.White;
                canvas.FillCircle(pt.X, pt.Y, 5);
                canvas.StrokeColor = Color.FromArgb("#2E7D32");
                canvas.StrokeSize = 2;
                canvas.DrawCircle(pt.X, pt.Y, 5);
            }

            canvas.RestoreState();
        }
    }
}