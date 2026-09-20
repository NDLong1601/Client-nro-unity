using System;
using Game2.UI.Adapters;
using Nro.UI;

namespace Game2.UI.Components
{
    public enum UiFrameStyle
    {
        Simple = 0,
        PopupYellow = 1,
        PopupGreen = 2
    }

    public static class UiFrame
    {
        public static void Paint(mGraphics g, UiRect bounds, UiFrameStyle style = UiFrameStyle.Simple)
        {
            if (g == null || bounds.IsEmpty) return;

            using (UiRenderState.Push(g, bounds, clip: false))
            {
                int x = bounds.X;
                int y = bounds.Y;
                int w = bounds.Width;
                int h = bounds.Height;

                if (style == UiFrameStyle.PopupYellow && PopUp.imgPopUp != null)
                {
                    PopUp.paintPopUp(g, x, y, w, h, 0, true);
                    return;
                }
                if (style == UiFrameStyle.PopupGreen && PopUp.imgPopUp2 != null)
                {
                    PopUp.paintPopUp(g, x, y, w, h, 1, true);
                    return;
                }

                // Simple style: solid bordered frame using color tokens
                // Outer border
                g.setColor(0x2B1F14);
                g.drawRect(x, y, w - 1, h - 1);
                // Background fill
                g.setColor(UiColorTokens.RowNormal);
                g.fillRect(x + 1, y + 1, w - 2, h - 2);
                // Inner top highlight line
                g.setColor(UiColorTokens.RowSelected);
                g.drawRect(x + 1, y + 1, w - 3, 1);
            }
        }
    }
}
