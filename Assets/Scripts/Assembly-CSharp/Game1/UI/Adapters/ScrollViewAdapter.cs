using System;
using Nro.UI;

namespace Game1.UI.Adapters
{
    public class ScrollViewAdapter
    {
        private Scroll _scroll = new Scroll();

        public UiRect Viewport { get; private set; }
        public int ItemCount { get; private set; }
        public int ItemSize { get; private set; }
        public int ScrollY => _scroll.cmy;
        public int ScrollLimit => _scroll.cmyLim;
        public bool IsDragging => _scroll.pointerIsDowning;
        public int SelectedIndex { get; private set; } = -1;

        public void Configure(UiRect viewport, int itemCount, int itemSize)
        {
            Viewport = viewport;
            ItemCount = System.Math.Max(0, itemCount);
            ItemSize = System.Math.Max(1, itemSize);

            _scroll.setStyle(ItemCount, ItemSize, viewport.X, viewport.Y, viewport.Width, viewport.Height, true, 1);

            // Clamp positions and selection when list shrinks
            if (ItemCount == 0)
            {
                _scroll.cmy = 0;
                _scroll.cmtoY = 0;
                _scroll.cmyLim = 0;
                SelectedIndex = -1;
            }
            else
            {
                if (_scroll.cmy > _scroll.cmyLim) _scroll.cmy = _scroll.cmyLim;
                if (_scroll.cmy < 0) _scroll.cmy = 0;
                if (_scroll.cmtoY > _scroll.cmyLim) _scroll.cmtoY = _scroll.cmyLim;
                if (_scroll.cmtoY < 0) _scroll.cmtoY = 0;
                if (SelectedIndex >= ItemCount) SelectedIndex = ItemCount - 1;
            }
        }

        public void Update()
        {
            _scroll.updatecm();
        }

        public bool ScrollByWheel(int wheelDelta)
        {
            if (wheelDelta == 0 || ItemCount <= 0 || _scroll.cmyLim <= 0)
            {
                return false;
            }

            int step = System.Math.Max(12, System.Math.Min(24, ItemSize));
            int target = _scroll.cmtoY - wheelDelta * step;
            target = System.Math.Max(0, System.Math.Min(_scroll.cmyLim, target));
            if (target == _scroll.cmtoY)
            {
                return false;
            }

            // Only move the target. Scroll.updatecm() eases the visible position
            // toward it so a wheel notch does not snap the list abruptly.
            _scroll.cmtoY = target;
            return true;
        }

        public void ScrollToIndex(int index)
        {
            if (ItemCount <= 0) return;
            if (index < 0) index = 0;
            if (index >= ItemCount) index = ItemCount - 1;
            int target = index * ItemSize - (Viewport.Height - ItemSize) / 2;
            if (target < 0) target = 0;
            if (target > _scroll.cmyLim) target = _scroll.cmyLim;
            _scroll.cmy = target;
            _scroll.cmtoY = target;
            SelectedIndex = index;
        }

        public bool UpdateKey(UiInputContext input, out int clickedIndex)
        {
            clickedIndex = -1;
            if (ItemCount == 0 || input == null || input.IsModalBlocked)
            {
                return false;
            }

            // Scroll.updateKey() internally consumes GameCanvas.isPointerJustRelease
            ScrollResult res = _scroll.updateKey();
            if (res == null)
            {
                return false;
            }

            if (res.selected >= 0 && res.selected < ItemCount)
            {
                SelectedIndex = res.selected;
            }

            if (res.isFinish && res.selected >= 0 && res.selected < ItemCount)
            {
                clickedIndex = res.selected;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            // Legacy Scroll.clear() does not release pointerIsDowning or its private
            // gesture/inertia state, so a fresh instance is required at lifecycle reset.
            _scroll = new Scroll();
            Viewport = UiRect.Empty;
            ItemCount = 0;
            ItemSize = 1;
            SelectedIndex = -1;
        }
    }
}
