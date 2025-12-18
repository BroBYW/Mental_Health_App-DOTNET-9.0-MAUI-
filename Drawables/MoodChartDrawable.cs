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

            // 1. Define margins and drawing area
            // Increased left margin to 60 to fit text like "Awful"
            float left = 60f, bottom = dirtyRect.Height - 40f, top = 40f, right = dirtyRect.Width - 20f;

            // 2. Draw Axes Lines
            canvas.StrokeSize = 2;
            canvas.StrokeColor = Color.FromArgb("#203229");
            canvas.DrawLine(left, bottom, right, bottom); // X-Axis
            canvas.DrawLine(left, bottom, left, top);     // Y-Axis

            // 3. Draw Y-Axis Labels (Moods)
            canvas.FontSize = 12; // Adjust font size as needed
            canvas.FontColor = Colors.Gray;

            string[] moodLabels = { "Awful", "Bad", "Meh", "Good", "Rad" };
            float yScale = (bottom - top) / 4f; // Steps between 1..5

            for (int i = 0; i < moodLabels.Length; i++)
            {
                // Value is i+1 (1 to 5)
                float y = bottom - i * yScale;

                // Draw label to the left of the Y-axis
                // x=0, width=left-5 creates a text box ending just before the axis line
                canvas.DrawString(moodLabels[i], 0, y - 10, left - 5, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
            }

            if (!Points.Any()) { canvas.RestoreState(); return; }

            float xStep = (right - left) / (Points.Count - 1);

            // 4. Draw Chart Path (Polyline)
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

            // 5. Draw Points and X-Axis Labels (Dates)
            foreach (var (pt, i) in Points.Select((p, i) => (p, i)))
            {
                float x = left + xStep * i;
                float y = bottom - (pt.Value - 1) * yScale;

                // Draw Point
                canvas.FillColor = Color.FromArgb("#62C370");
                canvas.FillCircle(x, y, 5);
                canvas.StrokeColor = Colors.White;
                canvas.DrawCircle(x, y, 5);

                // Draw X-Axis Label (Date) below the axis
                // Assuming pt.Day is a DateTime. Adjust format string as preferred (e.g., "dd/MM")
                string dateLabel = pt.Day.ToString("dd/MM");

                // Draw text centered under the specific X point
                canvas.DrawString(dateLabel, x - 20, bottom + 5, 40, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
            }

            canvas.RestoreState();
        }
    }
}