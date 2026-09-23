using System;
using Nro.UI;

namespace Game1.UI.Components
{
    public sealed class UiSkillPanelStyle
    {
        public int HeaderFill = 0xD83A2F;
        public int HeaderBorder = 0x7B241D;
        public int HeaderHighlight = 0xF07D66;
        public mFont HeaderFont = mFont.tahoma_7b_white;
        public int ListFill = 0xDED1BB;
        public int DetailFill = 0xDED1BB;
        public int ColumnBorder = 0x9A896F;

    }

    /// <summary>
    /// Shared two-column skill panel chrome. Skill data and row/detail content
    /// remain owned by the screen and are supplied through paint callbacks.
    /// </summary>
    public static class UiSkillPanel
    {
        private static readonly UiSkillPanelStyle DefaultStyle = new UiSkillPanelStyle();

        public static void PaintHeader(mGraphics g, UiRect bounds, string text,
            UiSkillPanelStyle style = null)
        {
            if (g == null || bounds.IsEmpty) return;
            style = style ?? DefaultStyle;
            UiMenuTheme.PaintHeader(g, bounds, text, style.HeaderFill, style.HeaderBorder,
                style.HeaderHighlight, style.HeaderFont ?? mFont.tahoma_7b_white);
        }

        public static void PaintListColumn(mGraphics g, UiRect bounds, UiScrollList rows,
            Action<mGraphics, int, UiRect> paintRow, UiSkillPanelStyle style = null)
        {
            if (g == null || bounds.IsEmpty) return;
            style = style ?? DefaultStyle;
            PaintColumnSurface(g, bounds, style.ListFill, style.ColumnBorder);
            if (rows != null) rows.Paint(g, paintRow);
        }

        public static void PaintDetailColumn(mGraphics g, UiRect bounds,
            Action<mGraphics> paintContent, UiSkillPanelStyle style = null)
        {
            if (g == null || bounds.IsEmpty) return;
            style = style ?? DefaultStyle;
            PaintColumnSurface(g, bounds, style.DetailFill, style.ColumnBorder);
            paintContent?.Invoke(g);
        }

        private static void PaintColumnSurface(mGraphics g, UiRect bounds, int fill, int border)
        {
            UiMenuTheme.PaintSurface(g, bounds, fill, border);
        }
    }
}
