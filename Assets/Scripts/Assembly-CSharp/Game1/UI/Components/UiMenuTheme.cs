using Nro.UI;

namespace Game1.UI.Components
{
    /// <summary>
    /// The raised amber/green menu chrome shared by all CustomMenu tabs.
    /// </summary>
    public static class UiMenuTheme
    {
        public static readonly UiActionButtonStyle ButtonStyle = new UiActionButtonStyle
        {
            NormalFill = 0xE99A00,
            HoveredFill = 0xF8B315,
            SelectedFill = 0x64C70D,
            SelectedHoveredFill = 0x79DB20,
            Border = 0x765018,
            SelectedBorder = 0x765018,
            RaisedShadowColor = 0x765018,
            SelectedRaisedShadowColor = 0x765018,
            Highlight = 0xFFE075,
            SelectedHighlight = 0xD5FF70,
            CornerRadius = 4,
            DrawFocusBorder = true
        };

        public static void PaintSurface(mGraphics g, UiRect bounds, int fill = 0xDED1BB,
            int shadow = 0x9A896F)
        {
            UiFrame.PaintRaisedSurface(g, bounds, fill, shadow);
        }

        public static void PaintCard(mGraphics g, UiRect bounds, int fill = 0xF6F3EE,
            int shadow = 0xB9AA93)
        {
            if (g == null || bounds.IsEmpty) return;
            g.setColor(shadow);
            g.fillRect(bounds.X, bounds.Y + 1, bounds.Width, bounds.Height, 4);
            g.setColor(fill);
            g.fillRect(bounds.X, bounds.Y, bounds.Width,
                System.Math.Max(1, bounds.Height - 1), 4);
        }

        public static void PaintHeader(mGraphics g, UiRect bounds, string text, bool red = true)
        {
            PaintHeader(g, bounds, text,
                red ? 0xD83A2F : 0xE99A00, red ? 0x7B241D : 0x765018,
                red ? 0xF07D66 : 0xF6C13A,
                red ? mFont.tahoma_7b_white : mFont.tahoma_7b_dark);
        }

        public static void PaintHeader(mGraphics g, UiRect bounds, string text,
            int fill, int shadow, int highlight, mFont font)
        {
            UiFrame.PaintRaisedHeader(g, bounds, text, fill, shadow, highlight, font);
        }

        public static void PaintButton(mGraphics g, UiRect bounds, string text,
            bool selected, bool focused = false)
        {
            UiActionButton.PaintRaised(g, bounds, text, selected, focused, ButtonStyle);
        }
    }
}
