using Game2.UI.Adapters;
using Nro.UI;

namespace Game2.UI.Components
{
    /// <summary>
    /// Paints a screen-positioned equipment layout while retaining server slot
    /// indices. Geometry and selection remain owned by the caller.
    /// </summary>
    public static class UiEquipmentGrid
    {
        public static void Paint(mGraphics g, UiRect viewport, UiRect[] slotBounds,
            int[] visualOrder, Item[] items, int selectedServerSlot, string[] emptyLabels,
            int backgroundColor, int equipmentBorderInset = 2)
        {
            if (g == null || viewport.IsEmpty || slotBounds == null || visualOrder == null) return;

            using (UiRenderState.Push(g, viewport, clip: true))
            {
                for (int visualIndex = 0; visualIndex < visualOrder.Length; visualIndex++)
                {
                    if (visualIndex >= slotBounds.Length) break;
                    int serverSlot = visualOrder[visualIndex];
                    Item item = items != null && serverSlot >= 0 && serverSlot < items.Length
                        ? items[serverSlot] : null;
                    UiRect bounds = slotBounds[visualIndex];
                    UiItemSlot.Paint(g, item, bounds, serverSlot == selectedServerSlot,
                        backgroundColor, equipmentBorderInset);

                    if ((item == null || item.template == null) && emptyLabels != null
                        && serverSlot >= 0 && serverSlot < emptyLabels.Length)
                        UiItemSlot.PaintEmptyLabel(g, bounds, emptyLabels[serverSlot]);
                }
            }
        }
    }
}
