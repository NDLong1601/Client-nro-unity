using System;

namespace Nro.UI
{
    /// <summary>
    /// Shared geometry for a fixed-column, vertically scrolled grid. The owner
    /// retains item data and decides how cells are painted or activated.
    /// </summary>
    public sealed class UiGridLayout
    {
        private readonly UiRect _viewport;
        private readonly int _columns;
        private readonly int _inset;
        private readonly int _gap;
        private readonly int _rowStride;
        private readonly int _cellHeight;

        public int CellWidth { get; private set; }

        public UiGridLayout(UiRect viewport, int columns, int inset, int gap, int rowStride, int cellHeight)
        {
            _viewport = viewport;
            _columns = Math.Max(1, columns);
            _inset = Math.Max(0, inset);
            _gap = Math.Max(0, gap);
            _rowStride = Math.Max(1, rowStride);
            _cellHeight = Math.Max(1, cellHeight);
            CellWidth = Math.Max(1,
                (viewport.Width - _inset * 2 - _gap * (_columns - 1)) / _columns);
        }

        public UiRect GetCellBounds(int index, int scrollY)
        {
            if (index < 0 || _viewport.IsEmpty) return UiRect.Empty;
            int column = index % _columns;
            int row = index / _columns;
            return new UiRect(_viewport.X + _inset + column * (CellWidth + _gap),
                _viewport.Y + _inset + row * _rowStride - scrollY,
                CellWidth, _cellHeight);
        }

        public bool IsCellVisible(int index, int scrollY)
        {
            UiRect cell = GetCellBounds(index, scrollY);
            return !cell.IsEmpty && cell.Bottom > _viewport.Y && cell.Y < _viewport.Bottom;
        }

        public int GetClampedColumn(int x)
        {
            int column = (x - _viewport.X - _inset) / Math.Max(1, CellWidth + _gap);
            return Math.Max(0, Math.Min(_columns - 1, column));
        }

        public int GetProportionalColumn(int x)
        {
            int column = (x - _viewport.X) * _columns / Math.Max(1, _viewport.Width);
            return Math.Max(0, Math.Min(_columns - 1, column));
        }

        public int HitTestCell(int x, int y, int itemCount, int scrollY)
        {
            if (!_viewport.Contains(x, y)) return -1;
            for (int index = 0; index < itemCount; index++)
            {
                if (GetCellBounds(index, scrollY).Contains(x, y)) return index;
            }
            return -1;
        }
    }
}
