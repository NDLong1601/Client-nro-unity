using Game1.UI.Adapters;
using Nro.UI;

namespace Game1.UI.Pilots
{
    public static class EnemyListView
    {
        public const int ITEM_HEIGHT = 24;
        public const int AVATAR_COLUMN_WIDTH = 24;

        public static void Paint(
            mGraphics g,
            int xScroll,
            int yScroll,
            int wScroll,
            int hScroll,
            int cmy,
            int selected,
            MyVector vEnemy,
            int currentListLength)
        {
            if (g == null)
            {
                return;
            }

            UiRect viewport = new UiRect(xScroll, yScroll, wScroll, hScroll);
            using (UiRenderState.Push(g, viewport, clip: true, translate: false))
            {
                g.translate(0, -cmy);
                try
                {
                    int effectiveCount = currentListLength;
                    if (vEnemy != null && effectiveCount > vEnemy.size())
                    {
                        effectiveCount = vEnemy.size();
                    }

                    if (effectiveCount <= 0 || vEnemy == null)
                    {
                        int fontH = (mFont.tahoma_7 != null) ? mFont.tahoma_7.getHeight() : 12;
                        if (mFont.tahoma_7_green2 != null && mResources.no_enemy != null)
                        {
                            mFont.tahoma_7_green2.drawString(
                                g,
                                mResources.no_enemy,
                                xScroll + wScroll / 2,
                                yScroll + hScroll / 2 - fontH / 2,
                                2
                            );
                        }
                        return;
                    }

                    UiListRange range = UiListLayout.GetVisibleRange(effectiveCount, ITEM_HEIGHT, cmy, hScroll);
                    if (range.IsEmpty)
                    {
                        return;
                    }

                    int avatarW = AVATAR_COLUMN_WIDTH;
                    int bodyW = wScroll - avatarW;
                    int rowH = ITEM_HEIGHT - 1;

                    for (int i = range.StartIndex; i < range.EndIndex; i++)
                    {
                        InfoItem infoItem = vEnemy.elementAt(i) as InfoItem;
                        if (infoItem == null || infoItem.charInfo == null)
                        {
                            continue;
                        }

                        UiRect rowBounds = UiListLayout.GetRowBounds(viewport, i, ITEM_HEIGHT);
                        int avatarX = rowBounds.X;
                        int rowY = rowBounds.Y;
                        int bodyX = avatarX + avatarW;
                        bool isSelected = (i == selected);

                        g.setColor(isSelected ? 16383818 : 15196114);
                        g.fillRect(bodyX, rowY, bodyW, rowH);

                        g.setColor(isSelected ? 9541120 : 9993045);
                        g.fillRect(avatarX, rowY, avatarW, rowH);

                        if (infoItem.charInfo.headICON != -1)
                        {
                            SmallImage.drawSmallImage(g, infoItem.charInfo.headICON, avatarX, rowY, 0, 0);
                        }
                        else if (GameScr.parts != null && infoItem.charInfo.head >= 0 && infoItem.charInfo.head < GameScr.parts.Length)
                        {
                            Part part = GameScr.parts[infoItem.charInfo.head];
                            if (part != null && part.pi != null && Char.CharInfo != null && Char.CharInfo.Length > 0 &&
                                Char.CharInfo[0] != null && Char.CharInfo[0].Length > 0 && Char.CharInfo[0][0] != null &&
                                Char.CharInfo[0][0].Length > 0)
                            {
                                int frameIdx = Char.CharInfo[0][0][0];
                                if (frameIdx >= 0 && frameIdx < part.pi.Length && part.pi[frameIdx] != null)
                                {
                                    SmallImage.drawSmallImage(
                                        g,
                                        part.pi[frameIdx].id,
                                        avatarX + part.pi[frameIdx].dx,
                                        rowY + 3 + part.pi[frameIdx].dy,
                                        0,
                                        0
                                    );
                                }
                            }
                        }

                        string cName = infoItem.charInfo.cName ?? string.Empty;
                        string infoStr = infoItem.s ?? string.Empty;

                        if (infoItem.isOnline)
                        {
                            if (mFont.tahoma_7b_green != null)
                            {
                                mFont.tahoma_7b_green.drawString(g, cName, bodyX + 5, rowY, 0);
                            }
                            if (mFont.tahoma_7_blue != null)
                            {
                                mFont.tahoma_7_blue.drawString(g, infoStr, bodyX + 5, rowY + 11, 0);
                            }
                        }
                        else
                        {
                            if (mFont.tahoma_7_grey != null)
                            {
                                mFont.tahoma_7_grey.drawString(g, cName, bodyX + 5, rowY, 0);
                                mFont.tahoma_7_grey.drawString(g, infoStr, bodyX + 5, rowY + 11, 0);
                            }
                        }
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
