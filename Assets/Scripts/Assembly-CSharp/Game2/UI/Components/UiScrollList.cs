using System;
using Game2.UI.Adapters;
using Nro.UI;

namespace Game2.UI.Components
{
    /// <summary>
    /// Uniform-row scrolling viewport. The owner retains row data and supplies
    /// a painter, while this component owns clipping, row bounds, and scroll input.
    /// </summary>
    public sealed class UiScrollList
    {
        private readonly ScrollViewAdapter _adapter = new ScrollViewAdapter();
        private int _rowHeight = 1;
        private int _spacing;

        public UiRect Viewport { get; private set; }
        public int ItemCount { get; private set; }
        public int ItemSize { get { return _rowHeight + _spacing; } }
        public int RowHeight { get { return _rowHeight; } }
        public int Spacing { get { return _spacing; } }
        public int ScrollY { get { return _adapter.ScrollY; } }
        public int ScrollLimit { get { return _adapter.ScrollLimit; } }
        public bool IsDragging { get { return _adapter.IsDragging; } }
        public int SelectedIndex { get { return _adapter.SelectedIndex; } }

        public void Configure(UiRect viewport, int itemCount, int rowHeight, int spacing = 0)
        {
            Viewport = viewport;
            ItemCount = System.Math.Max(0, itemCount);
            _rowHeight = System.Math.Max(1, rowHeight);
            _spacing = System.Math.Max(0, spacing);
            _adapter.Configure(viewport, ItemCount, _rowHeight + _spacing);
        }

        public void Update()
        {
            _adapter.Update();
        }

        public bool UpdateInput(UiInputContext input, out int clickedIndex)
        {
            return _adapter.UpdateKey(input, out clickedIndex);
        }

        public bool UpdateKey(UiInputContext input, out int clickedIndex)
        {
            return UpdateInput(input, out clickedIndex);
        }

        public bool ScrollByWheel(int wheelDelta)
        {
            return _adapter.ScrollByWheel(wheelDelta);
        }

        public void ScrollToIndex(int index)
        {
            _adapter.ScrollToIndex(index);
        }

        public UiRect GetItemBounds(int index)
        {
            if (index < 0 || index >= ItemCount || Viewport.IsEmpty) return UiRect.Empty;
            int stride = _rowHeight + _spacing;
            int y = Viewport.Y + index * stride - ScrollY;
            return new UiRect(Viewport.X, y, Viewport.Width, _rowHeight);
        }

        public bool IsItemVisible(int index)
        {
            UiRect item = GetItemBounds(index);
            return !item.IsEmpty && item.Bottom > Viewport.Y && item.Y < Viewport.Bottom;
        }

        public void Paint(mGraphics g, Action<mGraphics, int, UiRect> paintItem)
        {
            if (g == null || paintItem == null || ItemCount <= 0 || Viewport.IsEmpty) return;

            using (UiRenderState.Push(g, Viewport, clip: true))
            {
                int stride = _rowHeight + _spacing;
                int firstIndex = System.Math.Max(0, (ScrollY - _rowHeight) / stride);
                int lastIndex = System.Math.Min(ItemCount - 1,
                    (ScrollY + Viewport.Height + stride - 1) / stride);
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    UiRect item = GetItemBounds(i);
                    if (item.Bottom <= Viewport.Y || item.Y >= Viewport.Bottom) continue;
                    paintItem(g, i, item);
                }
            }
        }

        public void PaintScrollbar(mGraphics g, UiRect viewport)
        {
            if (g == null || ScrollLimit <= 0 || viewport.IsEmpty) return;
            int trackHeight = System.Math.Max(1, viewport.Height - 8);
            int totalHeight = viewport.Height + ScrollLimit;
            int thumbHeight = System.Math.Max(12,
                viewport.Height * trackHeight / System.Math.Max(1, totalHeight));
            thumbHeight = System.Math.Min(trackHeight, thumbHeight);
            int travel = trackHeight - thumbHeight;
            int thumbY = viewport.Y + 4 + ScrollY * travel / System.Math.Max(1, ScrollLimit);
            int x = viewport.Right - 4;
            g.setColor(0xB9AA92);
            g.fillRect(x, viewport.Y + 4, 2, trackHeight, 2);
            g.setColor(0xE89A08);
            g.fillRect(x - 1, thumbY, 4, thumbHeight, 3);
            g.setColor(0xFFE17A);
            g.fillRect(x, thumbY + 1, 1, System.Math.Max(1, thumbHeight - 2));
        }

        public void Reset()
        {
            _adapter.Reset();
            Viewport = UiRect.Empty;
            ItemCount = 0;
            _rowHeight = 1;
            _spacing = 0;
        }
    }
}
