using System;

namespace Nro.UI
{
    public struct UiListRange : IEquatable<UiListRange>
    {
        public static readonly UiListRange Empty = new UiListRange(0, 0);

        public int StartIndex { get; }
        public int EndIndex { get; }

        public int Count => System.Math.Max(0, EndIndex - StartIndex);

        public bool IsEmpty => Count <= 0;

        public UiListRange(int startIndex, int endIndex)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            if (endIndex < startIndex)
            {
                endIndex = startIndex;
            }
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public bool Contains(int index)
        {
            return index >= StartIndex && index < EndIndex;
        }

        public bool Equals(UiListRange other)
        {
            return StartIndex == other.StartIndex && EndIndex == other.EndIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is UiListRange other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StartIndex * 397) ^ EndIndex;
            }
        }

        public static bool operator ==(UiListRange left, UiListRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UiListRange left, UiListRange right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("[{0}..{1}) (Count={2})", StartIndex, EndIndex, Count);
        }
    }
}
