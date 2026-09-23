using Nro.UI;

namespace Game2.UI.Components
{
    /// <summary>
    /// Shared item-cell renderer for equipment, bag, and storage slots.
    /// The owner supplies item data, bounds, and selection state.
    /// </summary>
    public static class UiItemSlot
    {
        public static void Paint(mGraphics g, Item item, UiRect bounds, bool focused,
            int backgroundColor, int equipmentBorderInset = 2)
        {
            if (g == null || bounds.IsEmpty) return;

            g.setColor(backgroundColor);
            g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            if (GameCanvas.panel != null)
                GameCanvas.panel.paintInventoryGridEffect(g, item, bounds.X, bounds.Y,
                    bounds.Width, bounds.Height, includeUpgradeLevel: false, includeCrystalStars: false);

            g.setColor(0xDFD2BC, 0.85f);
            g.drawRect(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            if (GetUpgradeLevel(item) > 0)
                Panel.paintEquipmentCellFrame(g, item, bounds.X, bounds.Y, bounds.Width,
                    bounds.Height, isSelected: false, inset: equipmentBorderInset);

            if (item != null && item.template != null)
            {
                SmallImage.drawSmallImage(g, item.template.iconID,
                    bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, 0, 3);
                if (GameCanvas.panel != null)
                    GameCanvas.panel.paintInventoryGridItemMarkers(g, item, bounds.X, bounds.Y,
                        bounds.Width, bounds.Height, paintCrystalSlotBorder: false);
                PaintItemMarkers(g, item, bounds);
            }

            if (focused) PaintFocusFrame(g, bounds);
        }

        public static void PaintEmptyLabel(mGraphics g, UiRect bounds, string label,
            mFont font = null, int lineHeight = 8)
        {
            if (g == null || bounds.IsEmpty || string.IsNullOrEmpty(label)) return;
            font = font ?? mFont.tahoma_7_white;
            string[] lines = label.Split('\n');
            int y = bounds.Y + (bounds.Height - lines.Length * lineHeight) / 2;
            for (int i = 0; i < lines.Length; i++)
                font.drawString(g, lines[i], bounds.X + bounds.Width / 2,
                    y + i * lineHeight, mFont.CENTER);
        }

        public static int GetUpgradeLevel(Item item)
        {
            if (item == null || item.itemOption == null) return 0;
            for (int i = 0; i < item.itemOption.Length; i++)
            {
                ItemOption option = item.itemOption[i];
                if (option != null && option.optionTemplate != null && option.optionTemplate.id == 72)
                    return option.param;
            }
            return 0;
        }

        public static void PaintFocusFrame(mGraphics g, UiRect bounds)
        {
            float glow = GameCanvas.gameTick % 20 < 10 ? 0.95f : 0.65f;
            g.setColor(0xFFF1A0, glow);
            g.drawRect(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            g.setColor(0xF3A000, 0.9f);
            g.drawRect(bounds.X + 1, bounds.Y + 1, System.Math.Max(1, bounds.Width - 3),
                System.Math.Max(1, bounds.Height - 3));
        }

        private static void PaintItemMarkers(mGraphics g, Item item, UiRect bounds)
        {
            int upgrade = GetUpgradeLevel(item);
            if (upgrade > 0)
                mFont.tahoma_7_yellow.drawString(g, "+" + upgrade, bounds.X + 2, bounds.Y + 1, mFont.LEFT);
            if (item.quantity > 1)
                mFont.tahoma_7_yellow.drawString(g, item.quantity.ToString(), bounds.Right - 2,
                    bounds.Bottom - mFont.tahoma_7_yellow.getHeight(), mFont.RIGHT);
        }
    }
}
