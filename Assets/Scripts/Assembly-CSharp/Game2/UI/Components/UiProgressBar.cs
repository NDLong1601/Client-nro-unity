using Nro.UI;

namespace Game2.UI.Components
{
    public static class UiProgressBar
    {
        public static void PaintFlat(mGraphics g, UiRect bounds, int value, int maximum,
            int trackColor, int fillColor)
        {
            if (g == null || bounds.IsEmpty) return;
            g.setColor(trackColor);
            g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            int width = System.Math.Max(0, System.Math.Min(bounds.Width,
                (int)((long)System.Math.Max(0, value) * bounds.Width / System.Math.Max(1, maximum))));
            if (width <= 0) return;
            g.setColor(fillColor);
            g.fillRect(bounds.X, bounds.Y, width, bounds.Height);
        }

        public static void PaintInset(mGraphics g, UiRect bounds, long value, long maximum,
            int trackColor, int fillColor)
        {
            if (g == null || bounds.IsEmpty) return;
            g.setColor(trackColor);
            g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height, 3);
            if (maximum <= 0L || value <= 0L) return;
            long clamped = System.Math.Min(value, maximum);
            int width = (int)(clamped * System.Math.Max(1, bounds.Width - 2) / maximum);
            g.setColor(fillColor);
            g.fillRect(bounds.X + 1, bounds.Y + 1, System.Math.Max(1, width),
                System.Math.Max(1, bounds.Height - 2), 2);
        }
    }
}
