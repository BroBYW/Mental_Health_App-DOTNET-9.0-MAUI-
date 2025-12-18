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
            canvas.Antialias = true; // IMPORTANT: Makes lines smooth

            // 1. Define margins
            float left = 100f, bottom = dirtyRect.Height - 40f, top = 40f, right = dirtyRect.Width - 20f;

            // 2. Draw Subtle Grid Lines (Dashed)
            canvas.StrokeSize = 1;
            canvas.StrokeColor = Color.FromArgb("#E0E0E0"); // Very light gray
            canvas.StrokeDashPattern = new float[] { 4, 4 }; // Dashed pattern

            float yScale = (bottom - top) / 4f;

            // Draw horizontal grid lines for each mood
            for (int i = 0; i < 5; i++)
            {
                float y = bottom - i * yScale;
                canvas.DrawLine(left, y, right, y);
            }

            // Reset Stroke for main drawing
            canvas.StrokeDashPattern = null;

            // 3. Draw Y-Axis Labels (Emoji + Text)
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

            // Store points for reuse
            var coordinates = new List<PointF>();
            for (int i = 0; i < Points.Count; i++)
            {
                float x = left + xStep * i;
                float y = bottom - (Points[i].Value - 1) * yScale;
                coordinates.Add(new PointF(x, y));
            }

            // 5. Draw Gradient Area (Under the line)
            // Create a closed path starting from bottom-left, going up to points, and down to bottom-right
            var areaPath = new PathF();
            areaPath.MoveTo(coordinates[0].X, bottom); // Start at bottom-left
            foreach (var p in coordinates) areaPath.LineTo(p); // Trace points
            areaPath.LineTo(coordinates.Last().X, bottom); // Drop to bottom-right
            areaPath.Close(); // Close back to start

            // Define Gradient (Green -> Transparent)
            var gradient = new LinearGradientPaint
            {
                StartColor = Color.FromArgb("#8062C370"), // Semi-transparent Green
                EndColor = Color.FromArgb("#0062C370"),   // Fully transparent
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1) // Vertical gradient
            };

            canvas.SetFillPaint(gradient, dirtyRect);
            canvas.FillPath(areaPath);

            // 6. Draw the Main Line (Thicker & Smooth)
            path.MoveTo(coordinates[0]);
            foreach (var p in coordinates.Skip(1)) path.LineTo(p);

            canvas.StrokeColor = Color.FromArgb("#2E7D32"); // Dark Green
            canvas.StrokeSize = 4; // Thicker line
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.DrawPath(path);

            // 7. Draw Decorative Points
            canvas.FontSize = 12;
            canvas.FontColor = Colors.Gray;

            for (int i = 0; i < coordinates.Count; i++)
            {
                var pt = coordinates[i];
                var date = Points[i].Day;

                // Outer Glow (Halo)
                canvas.FillColor = Color.FromArgb("#4062C370"); // Very transparent green
                canvas.FillCircle(pt.X, pt.Y, 10);

                // Inner Dot
                canvas.FillColor = Colors.White;
                canvas.FillCircle(pt.X, pt.Y, 5);
                canvas.StrokeColor = Color.FromArgb("#2E7D32");
                canvas.StrokeSize = 2;
                canvas.DrawCircle(pt.X, pt.Y, 5);

                // Date Label
                string dateLabel = date.ToString("dd/MM");
                canvas.DrawString(dateLabel, pt.X - 25, bottom + 10, 50, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
            }

            canvas.RestoreState();
        }
    }
}