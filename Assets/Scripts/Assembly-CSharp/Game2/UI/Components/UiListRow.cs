using Nro.UI;

namespace Game2.UI.Components
{
    public sealed class UiListRowStyle
    {
        public int NormalFill = UiColorTokens.RowNormal;
        public int HoveredFill = UiColorTokens.RowSelected;
        public int PressedFill = UiColorTokens.RowSelected;
        public int SelectedFill = UiColorTokens.RowSelected;
        public int FocusedFill = UiColorTokens.RowSelected;
        public int DisabledFill = UiColorTokens.RowNormal;
        public int Divider = UiColorTokens.RowDivider;
        public int FocusBorder = UiColorTokens.HeaderDivider;
        public bool DrawDivider = true;
        public bool DrawFocusBorder = true;
    }

    public static class UiListRow
    {
        private static readonly UiListRowStyle DefaultStyle = new UiListRowStyle();

        public static void PaintCardBackground(mGraphics g, UiRect bounds, int fillColor,
            int borderColor, int cornerRadius = 0, int borderInset = 0)
        {
            if (g == null || bounds.IsEmpty) return;
            g.setColor(fillColor);
            if (cornerRadius > 0)
                g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height, cornerRadius);
            else
                g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            g.setColor(borderColor);
            g.drawRect(bounds.X, bounds.Y, bounds.Width - borderInset, bounds.Height - borderInset);
        }

        public static void PaintBackground(mGraphics g, UiRect bounds, UiVisualState state,
            UiListRowStyle style = null)
        {
            if (g == null || bounds.IsEmpty) return;
            style = style ?? DefaultStyle;

            int fill = style.NormalFill;
            if ((state & UiVisualState.Disabled) != 0) fill = style.DisabledFill;
            else if ((state & UiVisualState.Pressed) != 0) fill = style.PressedFill;
            else if ((state & UiVisualState.Selected) != 0) fill = style.SelectedFill;
            else if ((state & UiVisualState.Hovered) != 0) fill = style.HoveredFill;
            else if ((state & UiVisualState.Focused) != 0) fill = style.FocusedFill;

            g.setColor(fill);
            g.fillRect(bounds.X, bounds.Y, bounds.Width, System.Math.Max(0, bounds.Height - 1));
            if (style.DrawDivider && bounds.Height > 0)
            {
                g.setColor(style.Divider);
                g.fillRect(bounds.X, bounds.Bottom - 1, bounds.Width, 1);
            }
            if ((state & UiVisualState.Focused) != 0 && (state & UiVisualState.Disabled) == 0
                && style.DrawFocusBorder)
            {
                g.setColor(style.FocusBorder);
                g.drawRect(bounds.X, bounds.Y, System.Math.Max(0, bounds.Width - 1),
                    System.Math.Max(0, bounds.Height - 2));
            }
        }

        public static void PaintStandardBackground(mGraphics g, int x, int y, int width, int height, bool isSelected)
        {
            if (g == null)
            {
                return;
            }
            g.setColor(isSelected ? UiColorTokens.RowSelected : UiColorTokens.RowNormal);
            g.fillRect(x, y, width, height - 1);
            g.setColor(UiColorTokens.RowDivider);
            g.fillRect(x, y + height - 1, width, 1);
        }

        public static void PaintRankingBackground(mGraphics g, int x, int y, int width, int height, int avatarWidth, bool isSelected)
        {
            if (g == null)
            {
                return;
            }
            int bodyX = x + avatarWidth;
            int bodyW = width - avatarWidth;
            int rowH = height - 1;

            g.setColor(isSelected ? UiColorTokens.RowSelected : UiColorTokens.RowNormal);
            g.fillRect(bodyX, y, bodyW, rowH, 5);

            g.setColor(isSelected ? 9541120 : 9993045);
            g.fillRect(x, y, avatarWidth, rowH, 5);
        }
    }
}
