using System;

namespace Nro.UI
{
    public static class UiListLayout
    {
        public static int CalculateMaxScroll(int itemCount, int itemHeight, int viewportHeight)
        {
            if (itemCount <= 0 || itemHeight <= 0 || viewportHeight <= 0)
            {
                return 0;
            }
            long totalHeight = (long)itemCount * itemHeight;
            if (totalHeight <= viewportHeight)
            {
                return 0;
            }
            long maxScroll = totalHeight - viewportHeight;
            return maxScroll > int.MaxValue ? int.MaxValue : (int)maxScroll;
        }

        public static UiListRange GetVisibleRange(int itemCount, int itemHeight, int scrollY, int viewportHeight)
        {
            if (itemCount <= 0 || itemHeight <= 0 || viewportHeight <= 0)
            {
                return UiListRange.Empty;
            }

            int startIndex = scrollY <= 0 ? 0 : scrollY / itemHeight;
            if (startIndex >= itemCount)
            {
                return UiListRange.Empty;
            }

            long viewBottom = (long)scrollY + viewportHeight;
            if (viewBottom <= 0)
            {
                return UiListRange.Empty;
            }

            long lastVisibleIndex = (viewBottom - 1) / itemHeight;
            if (lastVisibleIndex < 0)
            {
                return UiListRange.Empty;
            }

            int endIndex = (int)System.Math.Min((long)itemCount, lastVisibleIndex + 1);
            if (endIndex <= startIndex)
            {
                return UiListRange.Empty;
            }

            return new UiListRange(startIndex, endIndex);
        }

        public static bool IsRowVisible(int index, int itemCount, int itemHeight, int scrollY, int viewportHeight)
        {
            if (index < 0 || index >= itemCount || itemHeight <= 0 || viewportHeight <= 0)
            {
                return false;
            }

            long rowTop = (long)index * itemHeight;
            long rowBottom = rowTop + itemHeight;
            long viewTop = scrollY;
            long viewBottom = (long)scrollY + viewportHeight;

            return rowBottom > viewTop && rowTop < viewBottom;
        }

        public static UiRect GetRowBounds(UiRect viewport, int index, int itemHeight)
        {
            return new UiRect(viewport.X, viewport.Y + index * itemHeight, viewport.Width, itemHeight);
        }

        public static UiRect GetRowBounds(int x, int y, int width, int index, int itemHeight)
        {
            return new UiRect(x, y + index * itemHeight, width, itemHeight);
        }
    }
}
