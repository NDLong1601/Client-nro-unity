using System;
using Game1.UI.Adapters;
using Nro.UI;

namespace Game1.UI.Components
{
    public enum UiFrameStyle
    {
        Simple = 0,
        PopupYellow = 1,
        PopupGreen = 2
    }

    public static class UiFrame
    {
        public static void PaintSurface(mGraphics g, UiRect bounds, int fillColor, int borderColor,
            int cornerRadius = 0, int highlightColor = -1)
        {
            if (g == null || bounds.IsEmpty) return;

            using (UiRenderState.Push(g, bounds, clip: false))
            {
                if (borderColor >= 0)
                {
                    g.setColor(borderColor);
                    if (cornerRadius > 0)
                        g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height, cornerRadius);
                    else
                        g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }

                int inset = borderColor >= 0 ? 1 : 0;
                int innerWidth = bounds.Width - inset * 2;
                int innerHeight = bounds.Height - inset * 2;
                if (innerWidth <= 0 || innerHeight <= 0) return;

                g.setColor(fillColor);
                if (cornerRadius > 0)
                    g.fillRect(bounds.X + inset, bounds.Y + inset, innerWidth, innerHeight,
                        System.Math.Max(1, cornerRadius - inset));
                else
                    g.fillRect(bounds.X + inset, bounds.Y + inset, innerWidth, innerHeight);

                if (highlightColor >= 0 && innerWidth > 2)
                {
                    g.setColor(highlightColor);
                    g.fillRect(bounds.X + inset + 1, bounds.Y + inset, innerWidth - 2, 1);
                }
            }
        }

        public static void PaintInsetCard(mGraphics g, UiRect bounds, int outerColor,
            int innerColor, int outerRadius = 4, int innerRadius = 4)
        {
            if (g == null || bounds.IsEmpty) return;
            g.setColor(outerColor);
            g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height, outerRadius);
            if (bounds.Width <= 2 || bounds.Height <= 2) return;
            g.setColor(innerColor);
            g.fillRect(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, innerRadius);
        }

        public static void PaintHeader(mGraphics g, UiRect bounds, string text, int fillColor,
            int borderColor, mFont font = null, int cornerRadius = 4, int textTopOffset = -1)
        {
            if (g == null || bounds.IsEmpty) return;

            PaintSurface(g, bounds, fillColor, borderColor, cornerRadius);
            (font ?? mFont.tahoma_7b_white).drawString(g, text ?? string.Empty,
                bounds.X + bounds.Width / 2,
                textTopOffset >= 0 ? bounds.Y + textTopOffset
                    : bounds.Y + (bounds.Height - (font ?? mFont.tahoma_7b_white).getHeight()) / 2,
                mFont.CENTER);
        }

        public static void PaintRaisedSurface(mGraphics g, UiRect bounds, int fillColor,
            int shadowColor = 0x9A896F, int highlightColor = 0xF8F1E6)
        {
            if (g == null || bounds.IsEmpty) return;
            g.setColor(shadowColor);
            g.fillRect(bounds.X + 1, bounds.Y + 1, System.Math.Max(1, bounds.Width - 1),
                System.Math.Max(1, bounds.Height - 1), 5);
            g.setColor(fillColor);
            g.fillRect(bounds.X, bounds.Y, System.Math.Max(1, bounds.Width - 1),
                System.Math.Max(1, bounds.Height - 2), 5);
            g.setColor(highlightColor);
            g.fillRect(bounds.X + 3, bounds.Y + 1, System.Math.Max(1, bounds.Width - 7), 1);
        }

        public static void PaintRaisedHeader(mGraphics g, UiRect bounds, string text,
            int fillColor, int shadowColor, int highlightColor, mFont font)
        {
            if (g == null || bounds.IsEmpty) return;
            g.setColor(shadowColor);
            g.fillRect(bounds.X + 1, bounds.Y + 2, System.Math.Max(1, bounds.Width - 1),
                System.Math.Max(1, bounds.Height - 1), 4);
            g.setColor(fillColor);
            g.fillRect(bounds.X, bounds.Y, bounds.Width,
                System.Math.Max(1, bounds.Height - 2), 4);
            g.setColor(highlightColor);
            g.fillRect(bounds.X + 3, bounds.Y + 1, System.Math.Max(1, bounds.Width - 6), 1);
            (font ?? mFont.tahoma_7b_white).drawString(g, text ?? string.Empty,
                bounds.X + bounds.Width / 2, bounds.Y + 5, mFont.CENTER);
        }

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
