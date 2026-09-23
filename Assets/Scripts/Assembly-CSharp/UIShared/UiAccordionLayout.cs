using System;

namespace Nro.UI
{
    /// <summary>
    /// Pixel layout for a single-expanded-row accordion. Painting, pointer input,
    /// and scroll sizing all use the same header and body bounds.
    /// </summary>
    public sealed class UiAccordionLayout
    {
        private const int HorizontalInset = 3;
        private const int TopInset = 3;
        private const int BottomInset = 3;
        private const int HeaderHeight = 26;
        private const int HeaderBodyGap = 2;
        private const int RowGap = 4;
        public const int BodyTextLeftInset = 8;
        public const int BodyTextTopInset = 6;
        public const int BodyLineHeight = 16;
        private const int BodyPadding = 14;

        private readonly UiRect _viewport;
        private readonly int[] _headerOffsets;
        private readonly int _expandedBodyHeight;

        public int Count { get { return _headerOffsets.Length; } }
        public int ExpandedIndex { get; private set; }
        public int ContentHeight { get; private set; }

        public UiAccordionLayout(UiRect viewport, int count, int expandedIndex, int expandedLineCount)
        {
            _viewport = viewport;
            _headerOffsets = new int[Math.Max(0, count)];
            ExpandedIndex = expandedIndex >= 0 && expandedIndex < _headerOffsets.Length ? expandedIndex : -1;
            _expandedBodyHeight = Math.Max(1, expandedLineCount) * BodyLineHeight + BodyPadding;
            if (_headerOffsets.Length == 0) return;

            int offset = TopInset;
            for (int i = 0; i < _headerOffsets.Length; i++)
            {
                _headerOffsets[i] = offset;
                offset += HeaderHeight + RowGap;
                if (i == ExpandedIndex) offset += HeaderBodyGap + _expandedBodyHeight;
            }
            ContentHeight = offset + BottomInset;
        }

        public int GetHeaderOffset(int index)
        {
            return index >= 0 && index < Count ? _headerOffsets[index] : -1;
        }

        public static int GetBodyTextWidth(UiRect viewport)
        {
            return Math.Max(1, viewport.Width - HorizontalInset * 2 - BodyTextLeftInset * 2);
        }

        public UiRect GetHeaderBounds(int index, int scrollY)
        {
            if (index < 0 || index >= Count || _viewport.IsEmpty) return UiRect.Empty;
            return new UiRect(_viewport.X + HorizontalInset,
                _viewport.Y + _headerOffsets[index] - scrollY,
                Math.Max(0, _viewport.Width - HorizontalInset * 2), HeaderHeight);
        }

        public UiRect GetBodyBounds(int index, int scrollY)
        {
            if (index != ExpandedIndex || _viewport.IsEmpty) return UiRect.Empty;
            UiRect header = GetHeaderBounds(index, scrollY);
            return new UiRect(header.X, header.Bottom + HeaderBodyGap,
                header.Width, _expandedBodyHeight);
        }

        public int HitTestHeader(int x, int y, int scrollY)
        {
            if (!_viewport.Contains(x, y)) return -1;
            for (int i = 0; i < Count; i++)
            {
                if (GetHeaderBounds(i, scrollY).Contains(x, y)) return i;
            }
            return -1;
        }
    }
}
