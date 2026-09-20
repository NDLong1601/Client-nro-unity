using Nro.UI;

namespace Game2.UI.Components
{
    public static class UiListRow
    {
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
