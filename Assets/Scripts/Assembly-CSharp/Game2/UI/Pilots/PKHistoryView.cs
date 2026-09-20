using System;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.Pilots
{
    public static class PKHistoryView
    {
        public const int ITEM_HEIGHT = 34;

        public static void Paint(
            mGraphics g,
            int xScroll,
            int yScroll,
            int wScroll,
            int hScroll,
            int cmy,
            int selected,
            bool isLoading,
            MyVector entries,
            Image imgWin,
            Image imgLose,
            string myCharName)
        {
            if (g == null)
            {
                return;
            }

            UiRect viewport = new UiRect(xScroll, yScroll, wScroll, hScroll);
            using (UiRenderState.Push(g, viewport, clip: true, translate: false))
            {
                if (isLoading)
                {
                    mFont.tahoma_7_grey.drawString(g, "Đang tải...", xScroll + wScroll / 2, yScroll + 12, mFont.CENTER);
                    return;
                }

                if (entries == null || entries.size() == 0)
                {
                    mFont.tahoma_7_grey.drawString(g, "Chưa có lịch sử thách đấu", xScroll + wScroll / 2, yScroll + 12, mFont.CENTER);
                    return;
                }

                g.translate(0, -cmy);
                try
                {
                    int count = entries.size();
                    UiListRange range = UiListLayout.GetVisibleRange(count, ITEM_HEIGHT, cmy, hScroll);
                    if (range.IsEmpty)
                    {
                        return;
                    }

                    long nowSec = mSystem.currentTimeMillis() / 1000L;
                    string myName = myCharName ?? string.Empty;

                    for (int i = range.StartIndex; i < range.EndIndex; i++)
                    {
                        PKHistoryEntry entry = (PKHistoryEntry)entries.elementAt(i);
                        if (entry == null)
                        {
                            continue;
                        }

                        UiRect rowBounds = UiListLayout.GetRowBounds(viewport, i, ITEM_HEIGHT);
                        int y = rowBounds.Y;

                        UiListRow.PaintStandardBackground(
                            g,
                            rowBounds.X,
                            rowBounds.Y,
                            rowBounds.Width,
                            rowBounds.Height,
                            i == selected);

                        mFont.tahoma_7b_dark.drawString(g, myName, xScroll + 8, y + 7, mFont.LEFT);

                        Image resultIcon = entry.won ? imgWin : imgLose;
                        if (resultIcon != null)
                        {
                            g.drawImage(resultIcon, xScroll + wScroll / 2, y + 11, mGraphics.HCENTER | mGraphics.VCENTER);
                        }
                        else
                        {
                            mFont resultFont = entry.won ? mFont.tahoma_7b_green : mFont.tahoma_7b_red;
                            resultFont.drawString(g, entry.won ? "WIN" : "LOSE", xScroll + wScroll / 2, y + 7, mFont.CENTER);
                        }

                        mFont.tahoma_7b_dark.drawString(g, entry.opponentName ?? string.Empty, xScroll + wScroll - 8, y + 7, mFont.RIGHT);

                        long elapsed = nowSec - entry.completedAt;
                        if (elapsed < 0L)
                        {
                            elapsed = 0L;
                        }
                        mFont.tahoma_7_grey.drawString(g, NinjaUtil.getTimeAgo(elapsed) + " " + mResources.ago, xScroll + wScroll - 8, y + 20, mFont.RIGHT);
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
