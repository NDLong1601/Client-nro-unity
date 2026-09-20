using System;

namespace Nro.UI
{
    public struct UiRect : IEquatable<UiRect>
    {
        public static readonly UiRect Empty = new UiRect(0, 0, 0, 0);

        private readonly int x;
        private readonly int y;
        private readonly int width;
        private readonly int height;

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public int Left
        {
            get { return x; }
        }

        public int Top
        {
            get { return y; }
        }

        public int Right
        {
            get { return x + width; }
        }

        public int Bottom
        {
            get { return y + height; }
        }

        public bool IsEmpty
        {
            get { return width <= 0 || height <= 0; }
        }

        public UiRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public bool Contains(int px, int py)
        {
            if (IsEmpty)
            {
                return false;
            }
            return px >= x && px < x + width && py >= y && py < y + height;
        }

        public UiRect Intersect(UiRect other)
        {
            if (IsEmpty || other.IsEmpty)
            {
                return Empty;
            }

            int newLeft = Math.Max(Left, other.Left);
            int newTop = Math.Max(Top, other.Top);
            int newRight = Math.Min(Right, other.Right);
            int newBottom = Math.Min(Bottom, other.Bottom);

            if (newRight <= newLeft || newBottom <= newTop)
            {
                return Empty;
            }

            return new UiRect(newLeft, newTop, newRight - newLeft, newBottom - newTop);
        }

        public UiRect Inset(int dx, int dy)
        {
            return Inset(dx, dy, dx, dy);
        }

        public UiRect Inset(int left, int top, int right, int bottom)
        {
            int newX = x + left;
            int newY = y + top;
            int newW = width - left - right;
            int newH = height - top - bottom;
            if (newW < 0)
            {
                newW = 0;
            }
            if (newH < 0)
            {
                newH = 0;
            }
            return new UiRect(newX, newY, newW, newH);
        }

        public bool Equals(UiRect other)
        {
            return x == other.x && y == other.y && width == other.width && height == other.height;
        }

        public override bool Equals(object obj)
        {
            return obj is UiRect && Equals((UiRect)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = x;
                hash = (hash * 397) ^ y;
                hash = (hash * 397) ^ width;
                hash = (hash * 397) ^ height;
                return hash;
            }
        }

        public static bool operator ==(UiRect left, UiRect right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UiRect left, UiRect right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("UiRect({0}, {1}, {2}, {3})", x, y, width, height);
        }
    }
}
