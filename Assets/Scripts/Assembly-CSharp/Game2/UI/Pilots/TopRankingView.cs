using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.Pilots
{
    public static class TopRankingView
    {
        public const int ITEM_HEIGHT = 24;
        public const int AVATAR_COLUMN_WIDTH = 29;

        public static void Paint(
            mGraphics g,
            int xScroll,
            int yScroll,
            int wScroll,
            int hScroll,
            int cmy,
            int selected,
            MyVector vTop,
            int currentListLength,
            int myCharId)
        {
            if (g == null || vTop == null || currentListLength <= 0)
            {
                return;
            }

            UiRect viewport = new UiRect(xScroll, yScroll, wScroll, hScroll);
            using (UiRenderState.Push(g, viewport, clip: true, translate: false))
            {
                g.translate(0, -cmy);
                try
                {
                    UiListRange range = UiListLayout.GetVisibleRange(currentListLength, ITEM_HEIGHT, cmy, hScroll);
                    if (range.IsEmpty)
                    {
                        return;
                    }

                    int avatarW = AVATAR_COLUMN_WIDTH;
                    int bodyW = wScroll - avatarW;
                    int rowH = ITEM_HEIGHT - 1;

                    for (int i = range.StartIndex; i < range.EndIndex; i++)
                    {
                        TopInfo topInfo = (TopInfo)vTop.elementAt(i);
                        if (topInfo == null)
                        {
                            continue;
                        }

                        UiRect rowBounds = UiListLayout.GetRowBounds(viewport, i, ITEM_HEIGHT);
                        int avatarX = rowBounds.X;
                        int rowY = rowBounds.Y;
                        int bodyX = rowBounds.X + avatarW;
                        bool isSelected = (i == selected);

                        UiListRow.PaintRankingBackground(
                            g,
                            rowBounds.X,
                            rowBounds.Y,
                            rowBounds.Width,
                            rowBounds.Height,
                            avatarW,
                            isSelected);

                        if (topInfo.headICON != -1)
                        {
                            SmallImage.drawSmallImage(g, topInfo.headICON, avatarX, rowY, 0, 0);
                        }
                        else if (GameScr.parts != null && topInfo.headID >= 0 && topInfo.headID < GameScr.parts.Length)
                        {
                            Part part = GameScr.parts[topInfo.headID];
                            if (part != null && part.pi != null && part.pi.Length > Char.CharInfo[0][0][0])
                            {
                                SmallImage.drawSmallImage(
                                    g,
                                    part.pi[Char.CharInfo[0][0][0]].id,
                                    avatarX + part.pi[Char.CharInfo[0][0][0]].dx,
                                    rowY + rowH - 1,
                                    0,
                                    mGraphics.BOTTOM | mGraphics.LEFT
                                );
                            }
                        }

                        mFont nameFont = (topInfo.pId == myCharId) ? mFont.tahoma_7b_red : mFont.tahoma_7b_green;
                        nameFont.drawString(g, topInfo.name ?? string.Empty, bodyX + 5, rowY, 0);

                        mFont.tahoma_7_blue.drawString(g, topInfo.info ?? string.Empty, bodyX + bodyW - 5, rowY + 11, 1);
                        mFont.tahoma_7_green2.drawString(g, mResources.rank + ": " + topInfo.rank + string.Empty, bodyX + 5, rowY + 11, 0);
                    }
                }
                finally
                {
                    g.translate(0, cmy);
                }
            }
        }
    }
}
