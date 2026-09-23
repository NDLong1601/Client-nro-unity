using System;
using System.Collections.Generic;
using Game1.Assets.src.g;
using Game1.UI.Adapters;
using Game1.UI.Components;
using Nro.UI;

namespace Game1.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private void PaintInventoryTabContent(mGraphics g)
        {
            PaintInventoryLeftTabs(g);
            PaintInventoryBagTabs(g);
            if (_selectedInventoryLeftTab == 0) PaintEquipmentSlots(g);
            else PaintPlayerInformation(g);
            if (ModFunc.isInventory) PaintInventoryGrid(g);
            else PaintInventoryList(g);
            PaintInventoryItemDetail(g);
        }

        private void PaintInventoryLeftTabs(mGraphics g)
        {
            _inventoryLeftTabBar.SetSelectedIndex(_selectedInventoryLeftTab);
            _inventoryLeftTabBar.FocusedIndex = _inventoryFocusArea == InventoryFocusLeft
                ? _selectedInventoryLeftTab : -1;
            _inventoryLeftTabBar.PaintRaised(g);
        }

        private void PaintInventoryBagTabs(mGraphics g)
        {
            _inventoryBagTabBar.SetSelectedIndex(_selectedInventoryBagTab);
            _inventoryBagTabBar.FocusedIndex = _inventoryFocusArea == InventoryFocusBagTabs
                ? _selectedInventoryBagTab : -1;
            _inventoryBagTabBar.PaintRaised(g);
        }

        private void ConfigureInventoryTabBars()
        {
            _inventoryLeftTabBar.ButtonStyle = UiMenuTheme.ButtonStyle;
            _inventoryLeftTabBar.TabClicked = tab =>
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusLeft;
                SelectInventoryLeftTab(tab);
            };
            _inventoryLeftTabBar.Configure(
                new UiRect(_inventoryLeftTab0Rect.X, _inventoryLeftTab0Rect.Y,
                    _inventoryLeftTab1Rect.X + _inventoryLeftTab1Rect.Width - _inventoryLeftTab0Rect.X,
                    _inventoryLeftTab0Rect.Height),
                new string[] { "Trang bị", "Thông tin" }, UiTabOrientation.Horizontal,
                _selectedInventoryLeftTab, gap: 4);

            _inventoryBagTabBar.ButtonStyle = UiMenuTheme.ButtonStyle;
            _inventoryBagTabBar.TabClicked = tab =>
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusBagTabs;
                SelectInventoryBagTab(tab);
            };
            _inventoryBagTabBar.Configure(
                new UiRect(_inventoryBagTab0Rect.X, _inventoryBagTab0Rect.Y,
                    _inventoryBagTab1Rect.X + _inventoryBagTab1Rect.Width - _inventoryBagTab0Rect.X,
                    _inventoryBagTab0Rect.Height),
                new string[] { "Tab 1", "Tab 2" }, UiTabOrientation.Horizontal,
                _selectedInventoryBagTab, gap: 4);
        }

        private void PaintEquipmentSlots(mGraphics g)
        {
            UiMenuTheme.PaintSurface(g, _leftColRect);

            Char me = Char.myCharz();
            Item[] body = me != null && me.arrItemBody != null ? me.arrItemBody : new Item[0];
            if (me != null)
            {
                using (UiRenderState.Push(g, _leftColRect, clip: true))
                {
                    int characterBottom = _equipmentSlotRects[6].Y - 3;
                    me.paintCharBody(g, _leftColRect.X + _leftColRect.Width / 2, characterBottom, 1, 0, true);
                }
            }
            UiEquipmentGrid.Paint(g, _leftColRect, _equipmentSlotRects, EquipmentVisualOrder,
                body, _selectedInventoryBodySlot, EquipmentSlotLabels, 0xB7A489,
                InventoryEquipmentBorderInset);
        }

        private void PaintPlayerInformation(mGraphics g)
        {
            UiMenuTheme.PaintSurface(g, _leftColRect);

            Char me = Char.myCharz();
            if (me == null) return;
            using (UiRenderState.Push(g, _leftColRect, clip: true))
            {
                int scrollY = _leftScrollAdapter != null ? _leftScrollAdapter.ScrollY : 0;
                int x = _leftColRect.X + 5;
                int y = _leftColRect.Y + 4 - scrollY;
                int avatarWidth = System.Math.Min(62, System.Math.Max(48, _leftColRect.Width / 3));
                UiRect profileAvatarRect = new UiRect(x, y + 2, avatarWidth, PlayerProfileHeaderHeight - 8);
                using (UiRenderState.Push(g, profileAvatarRect, clip: true))
                {
                    SmallImage.drawSmallImage(g, me.avatarz(), profileAvatarRect.X + profileAvatarRect.Width / 2,
                        profileAvatarRect.Bottom - 2, 0, mGraphics.HCENTER | mGraphics.BOTTOM);
                }

                int textX = profileAvatarRect.Right + 6;
                int textWidth = System.Math.Max(45, _leftColRect.Right - textX - 5);
                mFont.tahoma_7_orange.drawString(g, TruncateString(mFont.tahoma_7_orange, me.cName, textWidth), textX, y + 7, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, "Tộc : " + GetRaceName(me.cgender), textX, y + 25, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, TruncateString(mFont.tahoma_7b_dark, me.getStrLevel(), textWidth), textX, y + 42, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, "Sức mạnh :", textX, y + 59, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, NinjaUtil.getMoneys(me.cPower), textX, y + 74, mFont.LEFT);

                int dividerY = y + PlayerProfileHeaderHeight - 4;
                g.setColor(0xB58C58);
                g.drawLine(_leftColRect.X + 3, dividerY, _leftColRect.X + _leftColRect.Width - 3, dividerY);
                mFont.tahoma_7_red.drawString(g, "Thông tin player", x, dividerY + 4, mFont.LEFT);
                int lineY = dividerY + 19;
                int lineWidth = _leftColRect.Width - 10;
                List<string> infoLines = BuildPlayerInformationLines(me);
                for (int i = 0; i < infoLines.Count; i++)
                {
                    string line = infoLines[i];
                    if (string.IsNullOrEmpty(line))
                    {
                        lineY += PlayerInfoLineHeight;
                        continue;
                    }
                    mFont font = line == "Chỉ số từ trang bị:" ? mFont.tahoma_7_red : mFont.tahoma_7b_dark;
                    string[] wrapped = font.splitFontArray(line, lineWidth);
                    for (int j = 0; j < wrapped.Length; j++)
                    {
                        font.drawString(g, wrapped[j], x, lineY, mFont.LEFT);
                        lineY += PlayerInfoLineHeight;
                    }
                }
            }
        }

        private static List<string> BuildPlayerInformationLines(Char character)
        {
            List<string> lines = new List<string>();
            if (character == null) return lines;
            lines.Add("Tộc: " + GetRaceName(character.cgender));
            lines.Add("HP: " + NinjaUtil.getMoneys(character.cHP) + " / " + NinjaUtil.getMoneys(character.cHPFull));
            lines.Add("KI: " + NinjaUtil.getMoneys(character.cMP) + " / " + NinjaUtil.getMoneys(character.cMPFull));
            lines.Add("Sức đánh: " + NinjaUtil.getMoneys(character.cDamFull));
            lines.Add("Giáp: " + NinjaUtil.getMoneys(character.cDefull));
            lines.Add("Chí mạng: " + character.cCriticalFull + "% / SĐCM: " + (100 + SumEquippedOption(character, 5)) + "%");
            if (character.tlDef > 0) lines.Add("Giảm sát thương: " + character.tlDef + "%");
            if (character.tlPst > 0) lines.Add("Phản sát thương: " + character.tlPst + "%");
            if (character.tlNeDon > 0) lines.Add("Né đòn: " + character.tlNeDon + "%");
            if (character.tlHutHp > 0) lines.Add("Hút HP: " + character.tlHutHp + "%");
            if (character.tlHutMp > 0) lines.Add("Hút KI: " + character.tlHutMp + "%");
            if (character.tileGiamTDHS > 0) lines.Add("Giảm TDHS: " + character.tileGiamTDHS + "%");
            if (character.timeGiamTDHS > 0) lines.Add("Giảm TDHS: " + character.timeGiamTDHS + " giây");
            if (character.khangTDHS) lines.Add("Kháng TDHS: Có");
            if (character.isKhongLanh) lines.Add("Kháng lạnh: Có");
            if (character.wearingVoHinh) lines.Add("Vô hình: Có");
            if (character.teleport) lines.Add("Dịch chuyển: Có");

            Dictionary<int, int> optionTotals = new Dictionary<int, int>();
            Dictionary<int, ItemOptionTemplate> optionTemplates = new Dictionary<int, ItemOptionTemplate>();
            List<int> optionOrder = new List<int>();
            if (character.arrItemBody != null)
            {
                for (int i = 0; i < character.arrItemBody.Length; i++)
                {
                    Item item = character.arrItemBody[i];
                    if (item == null || item.itemOption == null) continue;
                    for (int j = 0; j < item.itemOption.Length; j++)
                    {
                        ItemOption option = item.itemOption[j];
                        if (option == null || option.optionTemplate == null || !option.IsValidOption()
                            || string.IsNullOrEmpty(option.optionTemplate.name)) continue;
                        int id = option.optionTemplate.id;
                        if (!optionTotals.ContainsKey(id))
                        {
                            optionTotals.Add(id, 0);
                            optionTemplates.Add(id, option.optionTemplate);
                            optionOrder.Add(id);
                        }
                        optionTotals[id] += option.param;
                    }
                }
            }

            lines.Add(string.Empty);
            lines.Add("Chỉ số từ trang bị:");
            if (optionOrder.Count == 0)
            {
                lines.Add("Chưa trang bị vật phẩm có chỉ số.");
            }
            else
            {
                for (int i = 0; i < optionOrder.Count; i++)
                {
                    int id = optionOrder[i];
                    ItemOptionTemplate template = optionTemplates[id];
                    string optionText = template.name.StartsWith("$")
                        ? NinjaUtil.Replace(template.name, "$", string.Empty)
                        : NinjaUtil.Replace(template.name, "#", optionTotals[id] + string.Empty);
                    lines.Add("- " + optionText);
                }
            }
            return lines;
        }

        private static string GetRaceName(int gender)
        {
            if (gender == 1) return "Namek";
            if (gender == 2) return "Xayda";
            return "Trái Đất";
        }

        private static int SumEquippedOption(Char character, int optionId)
        {
            int total = 0;
            if (character == null || character.arrItemBody == null) return total;
            for (int i = 0; i < character.arrItemBody.Length; i++)
            {
                Item item = character.arrItemBody[i];
                if (item == null || item.itemOption == null) continue;
                for (int j = 0; j < item.itemOption.Length; j++)
                {
                    ItemOption option = item.itemOption[j];
                    if (option != null && option.optionTemplate != null && option.optionTemplate.id == optionId) total += option.param;
                }
            }
            return total;
        }

        private void PaintInventoryGrid(mGraphics g)
        {
            PaintInventoryBagBackground(g);
            Char me = Char.myCharz();
            Item[] bag = me != null && me.arrItemBag != null ? me.arrItemBag : new Item[0];
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bag.Length);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bag.Length);
            UiGridLayout grid = new UiGridLayout(_rightBodyRect, InventoryGridColumns,
                2, 2, InventoryGridRowHeight, InventoryGridRowHeight - 2);
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;

            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                for (int localIndex = 0; localIndex < count; localIndex++)
                {
                    if (!grid.IsCellVisible(localIndex, scrollY)) continue;
                    int bagIndex = start + localIndex;
                    Item item = bagIndex < bag.Length ? bag[bagIndex] : null;
                    bool selected = bagIndex == _selectedInventoryBagSlot;
                    UiRect rect = grid.GetCellBounds(localIndex, scrollY);
                    UiItemSlot.Paint(g, item, rect, selected, 0xB7A489, InventoryEquipmentBorderInset);
                }
            }
        }

        private void PaintInventoryList(mGraphics g)
        {
            PaintInventoryBagBackground(g);
            Char me = Char.myCharz();
            Item[] bag = me != null && me.arrItemBag != null ? me.arrItemBag : new Item[0];
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bag.Length);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bag.Length);
            if (_rightScrollAdapter == null) return;
            const int iconWidth = 29;

            _rightScrollAdapter.Paint(g, (graphics, localIndex, rowRect) =>
            {
                if (localIndex >= count) return;
                int bagIndex = start + localIndex;
                Item item = bagIndex < bag.Length ? bag[bagIndex] : null;
                bool selected = bagIndex == _selectedInventoryBagSlot;
                UiRect rowVisualRect = new UiRect(rowRect.X, rowRect.Y, rowRect.Width,
                    System.Math.Max(0, rowRect.Height - 1));
                UiMenuTheme.PaintCard(graphics, rowVisualRect,
                    selected ? 0xFFF0B0 : 0xF6F3EE,
                    selected ? 0xF8CD63 : 0xB9AA93);
                graphics.setColor(selected ? 0xD7A900 : 0xB7A489);
                graphics.fillRect(rowVisualRect.X, rowVisualRect.Y, iconWidth,
                    System.Math.Max(1, rowVisualRect.Height - 1), 4);
                if (item == null || item.template == null) return;

                UiRect iconRect = new UiRect(rowVisualRect.X, rowVisualRect.Y, iconWidth, rowVisualRect.Height);
                UiItemSlot.Paint(graphics, item, iconRect, selected, 0xB7A489, InventoryEquipmentBorderInset);

                int textX = rowVisualRect.X + iconWidth + 5;
                int textWidth = rowVisualRect.Width - iconWidth - 9;
                string name = item.template.name + GetUpgradeSuffix(item);
                mFont.tahoma_7b_dark.drawString(graphics, TruncateString(mFont.tahoma_7b_dark, name, textWidth), textX, rowVisualRect.Y + 3, mFont.LEFT);
                string summary = GetItemOptionSummary(item);
                if (!string.IsNullOrEmpty(summary))
                    mFont.tahoma_7_blue.drawString(graphics, TruncateString(mFont.tahoma_7_blue, summary, textWidth), textX, rowVisualRect.Y + 17, mFont.LEFT);
                if (selected) UiItemSlot.PaintFocusFrame(graphics, rowVisualRect);
            });
        }

        private void PaintInventoryBagBackground(mGraphics g)
        {
            UiMenuTheme.PaintSurface(g, _rightBodyRect);
        }

        private void PaintInventoryItemDetail(mGraphics g)
        {
            if (!_showInventoryDetail) return;
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            if (item == null || item.template == null) return;
            ConfigureInventoryDetailRects();
            UiRect rect = _inventoryDetailRect;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            g.setColor(0x35281C, 0.35f);
            g.fillRect(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
            g.setColor(0xF5F1EA);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(0xC9B89F);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);

            using (UiRenderState.Push(g, rect, clip: true))
            {
                const int headerHeight = 39;
                g.setColor(0xE8DDCC);
                g.fillRect(rect.X + 1, rect.Y + 1, rect.Width - 2, headerHeight - 1);
                int iconWidth = System.Math.Min(34, rect.Width / 4);
                UiRect iconRect = new UiRect(rect.X + 4, rect.Y + 3, iconWidth, 32);
                UiItemSlot.Paint(g, item, iconRect, false, 0xB7A489, InventoryEquipmentBorderInset);
                int textX = iconRect.Right + 6;
                int textWidth = System.Math.Max(30, rect.Right - textX - 4);
                int y = rect.Y + 5;
                mFont.tahoma_7b_dark.drawString(g,
                    TruncateString(mFont.tahoma_7b_dark, item.template.name + GetUpgradeSuffix(item), textWidth),
                    textX, y, mFont.LEFT);
                y += 15;
                if (item.quantity > 1)
                {
                    mFont.tahoma_7_green2.drawString(g, "Số lượng: " + item.quantity, textX, y, mFont.LEFT);
                }

                int headerDividerY = rect.Y + headerHeight;
                g.setColor(0xC9B89F);
                g.drawLine(rect.X + 4, headerDividerY, rect.Right - 4, headerDividerY);
                GetCrystalStarSlots(item, out _, out int maxStarSlots);
                int starHeight = maxStarSlots > 0 ? (maxStarSlots > 5 ? 29 : 18) : 0;
                bool hasFooter = item.template.strRequire > 1 || !string.IsNullOrEmpty(item.template.description);
                int footerHeight = hasFooter ? 20 : 0;
                int optionsBottom = rect.Bottom - 3 - starHeight - footerHeight;
                y = headerDividerY + 4;
                int optionTextX = rect.X + 7;
                int optionTextWidth = System.Math.Max(30, rect.Width - 14);

                if (item.itemOption != null)
                {
                    for (int i = 0; i < item.itemOption.Length && y < optionsBottom; i++)
                    {
                        ItemOption option = item.itemOption[i];
                        if (option == null || option.optionTemplate == null || !option.IsValidOption()) continue;
                        int id = option.optionTemplate.id;
                        if (id == Item.OPT_LVITEM || id == Item.OPT_STARSLOT || id == Item.OPT_MAXSTARSLOT) continue;
                        string optionText = option.getOptionString();
                        if (string.IsNullOrEmpty(optionText)) continue;
                        string[] wrapped = mFont.tahoma_7_green2.splitFontArray(optionText, optionTextWidth);
                        for (int j = 0; j < wrapped.Length && y < optionsBottom; j++)
                        {
                            mFont.tahoma_7_green2.drawString(g, wrapped[j], optionTextX, y, mFont.LEFT);
                            y += 13;
                        }
                    }
                }

                int footerY = rect.Bottom - starHeight - footerHeight;
                if (hasFooter)
                {
                    g.setColor(0xB58C58);
                    g.drawLine(rect.X + 5, footerY, rect.Right - 5, footerY);
                    if (item.template.strRequire > 1)
                    {
                        mFont requirementFont = item.template.strRequire > Char.myCharz().cPower
                            ? mFont.tahoma_7_red : mFont.tahoma_7_grey;
                        requirementFont.drawString(g, "Sức mạnh yêu cầu: " + item.template.strRequire,
                            rect.X + rect.Width / 2, footerY + 4, mFont.CENTER);
                    }
                    else
                    {
                        mFont.tahoma_7_grey.drawString(g,
                            TruncateString(mFont.tahoma_7_grey, item.template.description, rect.Width - 10),
                            rect.X + rect.Width / 2, footerY + 4, mFont.CENTER);
                    }
                }
                if (starHeight > 0)
                {
                    int starY = rect.Bottom - starHeight;
                    g.setColor(0xB58C58);
                    g.drawLine(rect.X + 5, starY, rect.Right - 5, starY);
                    PaintCrystalStars(g, item, new UiRect(rect.X + 5, starY + 2, rect.Width - 10, starHeight - 2));
                }
            }

            int actionCount = GetInventoryActionCount(item, fromBody);
            for (int i = 0; i < actionCount; i++)
            {
                UiRect actionRect = _inventoryActionRects[i];
                bool focused = _inventoryFocusArea == InventoryFocusActions && _selectedInventoryAction == i;
                PaintInventoryActionButton(g, actionRect, GetInventoryActionLabel(item, fromBody, i), focused);
            }
        }

        private static void GetCrystalStarSlots(Item item, out int filledSlots, out int maxSlots)
        {
            filledSlots = 0;
            maxSlots = 0;
            if (item == null || item.itemOption == null) return;
            for (int i = 0; i < item.itemOption.Length; i++)
            {
                ItemOption option = item.itemOption[i];
                if (option == null || option.optionTemplate == null) continue;
                if (option.optionTemplate.id == Item.OPT_STARSLOT) filledSlots = System.Math.Max(0, option.param);
                else if (option.optionTemplate.id == Item.OPT_MAXSTARSLOT) maxSlots = System.Math.Max(0, option.param);
            }
            if (filledSlots > maxSlots) filledSlots = maxSlots;
        }

        private static void PaintCrystalStars(mGraphics g, Item item, UiRect rect)
        {
            GetCrystalStarSlots(item, out int filledSlots, out int maxSlots);
            if (maxSlots <= 0) return;
            int topCount = maxSlots > 5 ? (maxSlots + 1) / 2 : maxSlots;
            int bottomCount = maxSlots - topCount;
            int spacing = System.Math.Min(20, System.Math.Max(9, (rect.Width - 4) / System.Math.Max(1, topCount)));
            for (int i = 0; i < maxSlots; i++)
            {
                bool bottomRow = i >= topCount;
                int rowIndex = bottomRow ? i - topCount : i;
                int rowCount = bottomRow ? bottomCount : topCount;
                int x = rect.X + rect.Width / 2 - (rowCount - 1) * spacing / 2 + rowIndex * spacing;
                int y = rect.Y + (bottomRow ? 18 : 7);
                Image starImage = i < filledSlots
                    ? (i >= ChatPopup.numSlot && Panel.imgStar8 != null ? Panel.imgStar8 : Panel.imgStar)
                    : Panel.imgMaxStar;
                if (starImage != null) g.drawImage(starImage, x, y, mGraphics.HCENTER | mGraphics.VCENTER);
                else
                {
                    mFont starFont = i < filledSlots ? mFont.tahoma_7b_yellow : mFont.tahoma_7b_dark;
                    starFont.drawString(g, "*", x, y - 5, mFont.CENTER);
                }
            }
        }

        private static void PaintInventoryActionButton(mGraphics g, UiRect rect, string label, bool focused)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            UiMenuTheme.PaintButton(g, rect, label, focused);
        }

        private static int GetItemUpgradeLevel(Item item)
        {
            return UiItemSlot.GetUpgradeLevel(item);
        }

        private static string GetUpgradeSuffix(Item item)
        {
            int upgrade = GetItemUpgradeLevel(item);
            return upgrade > 0 ? " [+" + upgrade + "]" : string.Empty;
        }

        private static string GetItemOptionSummary(Item item)
        {
            if (item == null || item.itemOption == null) return string.Empty;
            for (int i = 0; i < item.itemOption.Length; i++)
            {
                ItemOption option = item.itemOption[i];
                if (option == null || option.optionTemplate == null) continue;
                int id = option.optionTemplate.id;
                if (id == 72 || id == 102 || id == 107) continue;
                return option.getOptionString();
            }
            return string.Empty;
        }

    }
}
