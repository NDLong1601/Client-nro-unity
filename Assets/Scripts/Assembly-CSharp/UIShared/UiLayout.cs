using System;

namespace Nro.UI
{
    public static class UiLayout
    {
        public static UiRect Inset(UiRect rect, int dx, int dy)
        {
            return rect.Inset(dx, dy);
        }

        public static UiRect Inset(UiRect rect, int left, int top, int right, int bottom)
        {
            return rect.Inset(left, top, right, bottom);
        }

        public static UiRect StackHorizontal(UiRect container, int index, int count, int gap = 0)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be greater than zero.");
            }
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException("index", "Index is out of range.");
            }
            if (gap < 0)
            {
                throw new ArgumentOutOfRangeException("gap", "Gap cannot be negative.");
            }

            long totalGap = (long)(count - 1) * gap;
            if (totalGap > Math.Max(0, container.Width))
            {
                throw new ArgumentOutOfRangeException("gap", "Total gap cannot exceed the container width.");
            }

            int totalItemSpace = Math.Max(0, container.Width - (int)totalGap);
            int baseSize = totalItemSpace / count;
            int remainder = totalItemSpace % count;

            int itemSize = baseSize + (index < remainder ? 1 : 0);
            int offset = index * baseSize + Math.Min(index, remainder) + index * gap;
            int x = container.X + offset;

            return new UiRect(x, container.Y, itemSize, container.Height);
        }

        public static UiRect StackVertical(UiRect container, int index, int count, int gap = 0)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be greater than zero.");
            }
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException("index", "Index is out of range.");
            }
            if (gap < 0)
            {
                throw new ArgumentOutOfRangeException("gap", "Gap cannot be negative.");
            }

            long totalGap = (long)(count - 1) * gap;
            if (totalGap > Math.Max(0, container.Height))
            {
                throw new ArgumentOutOfRangeException("gap", "Total gap cannot exceed the container height.");
            }

            int totalItemSpace = Math.Max(0, container.Height - (int)totalGap);
            int baseSize = totalItemSpace / count;
            int remainder = totalItemSpace % count;

            int itemSize = baseSize + (index < remainder ? 1 : 0);
            int offset = index * baseSize + Math.Min(index, remainder) + index * gap;
            int y = container.Y + offset;

            return new UiRect(container.X, y, container.Width, itemSize);
        }
    }
}
