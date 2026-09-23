using System;
using System.Collections.Generic;
using Game2.Assets.src.g;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private static int GetInventoryTabStart(int tab, int bagLength)
        {
            int firstTabCount = (bagLength + 1) / 2;
            return tab <= 0 ? 0 : firstTabCount;
        }

        private static int GetInventoryTabItemCount(int tab, int bagLength)
        {
            int start = GetInventoryTabStart(tab, bagLength);
            int end = tab <= 0 ? (bagLength + 1) / 2 : bagLength;
            return System.Math.Max(0, end - start);
        }

        private int GetInventoryBagRowCount()
        {
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int itemCount = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            return ModFunc.isInventory ? (itemCount + InventoryGridColumns - 1) / InventoryGridColumns : itemCount;
        }

        private int GetInventoryInfoContentHeight()
        {
            Char me = Char.myCharz();
            if (me == null) return _leftColRect.Height;
            int lineCount = 0;
            int width = System.Math.Max(40, _leftColRect.Width - 10);
            List<string> lines = BuildPlayerInformationLines(me);
            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) lineCount++;
                else lineCount += System.Math.Max(1, mFont.tahoma_7b_dark.splitFontArray(lines[i], width).Length);
            }
            return PlayerProfileHeaderHeight + 19 + lineCount * PlayerInfoLineHeight + 6;
        }

        private void ConfigureEquipmentSlotRects()
        {
            const int gap = 2;
            const int columns = 5;
            const int rows = 5;
            int cellWidth = System.Math.Max(20, (_leftColRect.Width - gap * (columns + 1)) / columns);
            int cellHeight = System.Math.Max(20, (_leftColRect.Height - gap * (rows + 1)) / rows);
            int gridWidth = cellWidth * columns + gap * (columns - 1);
            int gridHeight = cellHeight * rows + gap * (rows - 1);
            int gridX = _leftColRect.X + (_leftColRect.Width - gridWidth) / 2;
            int gridY = _leftColRect.Y + (_leftColRect.Height - gridHeight) / 2;
            int rightX = gridX + (columns - 1) * (cellWidth + gap);

            for (int row = 0; row < 3; row++)
            {
                int y = gridY + row * (cellHeight + gap);
                _equipmentSlotRects[row] = new UiRect(gridX, y, cellWidth, cellHeight);
                _equipmentSlotRects[row + 3] = new UiRect(rightX, y, cellWidth, cellHeight);
            }

            for (int index = 0; index < 10; index++)
            {
                int column = index % columns;
                int row = index / columns + 3;
                int x = gridX + column * (cellWidth + gap);
                int y = gridY + row * (cellHeight + gap);
                _equipmentSlotRects[index + 6] = new UiRect(x, y, cellWidth, cellHeight);
            }
        }

        private void ConfigureInventoryDetailRects()
        {
            _inventoryDetailRect = UiRect.Empty;
            for (int i = 0; i < _inventoryActionRects.Length; i++) _inventoryActionRects[i] = UiRect.Empty;
            if (!_showInventoryDetail) return;

            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            if (item == null || item.template == null) return;

            const int gap = 3;
            const int buttonHeight = 26;
            if (fromBody)
            {
                int detailHeight = System.Math.Min(126, _leftColRect.Height - 8);
                _inventoryDetailRect = new UiRect(_leftColRect.X + 2, _leftColRect.Bottom - detailHeight,
                    _leftColRect.Width - 4, detailHeight);
                int buttonWidth = System.Math.Max(48, (_rightBodyRect.Width - gap * 3) / 2);
                int buttonX = _rightBodyRect.X + gap;
                int buttonY = _inventoryDetailRect.Y;
                _inventoryActionRects[0] = new UiRect(buttonX, buttonY, buttonWidth, buttonHeight);
                _inventoryActionRects[1] = new UiRect(buttonX, buttonY + buttonHeight + gap, buttonWidth, buttonHeight);
                return;
            }

            int detailInset = ModFunc.isInventory
                ? System.Math.Max(28, (_rightBodyRect.Width - 12) / InventoryGridColumns + gap)
                : 31;
            int actionAreaHeight = buttonHeight * 2 + gap * 3;
            int detailHeightBag = System.Math.Min(112, System.Math.Max(72, _rightBodyRect.Height - actionAreaHeight));
            _inventoryDetailRect = new UiRect(_rightBodyRect.X + detailInset, _rightBodyRect.Y,
                _rightBodyRect.Width - detailInset, detailHeightBag);

            int actionY = _inventoryDetailRect.Bottom + gap;
            int actionWidth = (_rightBodyRect.Width - gap * 3) / 2;
            for (int i = 0; i < _inventoryActionRects.Length; i++)
            {
                int column = i % 2;
                int row = i / 2;
                _inventoryActionRects[i] = new UiRect(_rightBodyRect.X + gap + column * (actionWidth + gap),
                    actionY + row * (buttonHeight + gap), actionWidth, buttonHeight);
            }
        }

        private bool HandleInventoryPointerInput()
        {
            ConfigureInventoryDetailRects();
            Item selectedItem = GetSelectedInventoryItem(out bool selectedFromBody, out _);
            int actionCount = GetInventoryActionCount(selectedItem, selectedFromBody);
            if (_showInventoryDetail && selectedItem != null && GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < actionCount; i++)
                {
                    UiRect actionRect = _inventoryActionRects[i];
                    if (!GameCanvas.isPointer(actionRect.X, actionRect.Y, actionRect.Width, actionRect.Height)) continue;
                    _keyboardFocus = KeyboardFocusContent;
                    _inventoryFocusArea = InventoryFocusActions;
                    _selectedInventoryAction = i;
                    GameCanvas.isPointerJustRelease = false;
                    PerformInventoryAction(i);
                    return true;
                }
                if (_inventoryDetailRect.Width > 0 && GameCanvas.isPointer(_inventoryDetailRect.X, _inventoryDetailRect.Y,
                    _inventoryDetailRect.Width, _inventoryDetailRect.Height))
                {
                    GameCanvas.isPointerJustRelease = false;
                    return true;
                }
            }

            bool isInventoryListDragging = (_leftScrollAdapter != null && _leftScrollAdapter.IsDragging)
                || (_rightScrollAdapter != null && _rightScrollAdapter.IsDragging);
            if (_inventoryLeftTabBar.UpdateInput(_inputContext, isInventoryListDragging)
                || _inventoryBagTabBar.UpdateInput(_inputContext, isInventoryListDragging))
                return true;

            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryLeftTab0Rect.X, _inventoryLeftTab0Rect.Y,
                _inventoryLeftTab0Rect.Width, _inventoryLeftTab0Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusLeft;
                SelectInventoryLeftTab(0);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryLeftTab1Rect.X, _inventoryLeftTab1Rect.Y,
                _inventoryLeftTab1Rect.Width, _inventoryLeftTab1Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusLeft;
                SelectInventoryLeftTab(1);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryBagTab0Rect.X, _inventoryBagTab0Rect.Y,
                _inventoryBagTab0Rect.Width, _inventoryBagTab0Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusBagTabs;
                SelectInventoryBagTab(0);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryBagTab1Rect.X, _inventoryBagTab1Rect.Y,
                _inventoryBagTab1Rect.Width, _inventoryBagTab1Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusBagTabs;
                SelectInventoryBagTab(1);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }

            if (_selectedInventoryLeftTab == 0 && GameCanvas.isPointerJustRelease)
            {
                for (int visualIndex = 0; visualIndex < _equipmentSlotRects.Length; visualIndex++)
                {
                    UiRect rect = _equipmentSlotRects[visualIndex];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    SelectInventoryBodyItem(EquipmentVisualOrder[visualIndex]);
                    GameCanvas.isPointerJustRelease = false;
                    return true;
                }
            }
            else if (_selectedInventoryLeftTab == 1 && _leftScrollAdapter != null)
            {
                if (_leftScrollAdapter.UpdateKey(_inputContext, out _)) return true;
                if (_leftScrollAdapter.IsDragging) return true;
            }

            if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out int clickedRow))
            {
                SelectInventoryBagItem(clickedRow);
                return true;
            }
            return _rightScrollAdapter != null && _rightScrollAdapter.IsDragging;
        }

        private void SelectInventoryLeftTab(int tab)
        {
            if (tab < 0) tab = 0;
            if (tab > 1) tab = 1;
            if (_selectedInventoryLeftTab == tab)
            {
                CloseInventoryDetail();
                return;
            }
            CloseInventoryDetail();
            _selectedInventoryLeftTab = tab;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
            _leftScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void SelectInventoryBagTab(int tab)
        {
            if (tab < 0) tab = 0;
            if (tab > 1) tab = 1;
            if (_selectedInventoryBagTab == tab)
            {
                CloseInventoryDetail();
                return;
            }
            CloseInventoryDetail();
            _selectedInventoryBagTab = tab;
            _selectedInventoryBagSlot = -1;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryLeftSelection(int direction)
        {
            if (_selectedInventoryLeftTab != 0) return;
            CloseInventoryDetail();
            int visualIndex = System.Array.IndexOf(EquipmentVisualOrder, _selectedInventoryBodySlot);
            if (visualIndex < 0) visualIndex = direction > 0 ? 0 : InventoryEquipmentSlotCount - 1;
            else visualIndex = (visualIndex + direction + InventoryEquipmentSlotCount) % InventoryEquipmentSlotCount;
            _selectedInventoryBodySlot = EquipmentVisualOrder[visualIndex];
            _selectedInventoryBagSlot = -1;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryHorizontalSelection(int direction)
        {
            CloseInventoryDetail();
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (count <= 0) return;
            int localIndex = _selectedInventoryBagSlot - start;
            if (localIndex < 0 || localIndex >= count)
            {
                localIndex = direction > 0 ? 0 : count - 1;
            }
            else
            {
                int currentRow = localIndex / InventoryGridColumns;
                int currentColumn = localIndex % InventoryGridColumns;
                int targetColumn = currentColumn + direction;
                if (targetColumn < 0 || targetColumn >= InventoryGridColumns) return;
                int target = localIndex + direction;
                int targetRow = target / InventoryGridColumns;
                if (target < 0 || target >= count || targetRow != currentRow) return;
                localIndex = target;
            }
            _selectedInventoryBagSlot = start + localIndex;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            _rightScrollAdapter?.ScrollToIndex(localIndex / InventoryGridColumns);
            ConfigureInventoryDetailRects();
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryVerticalSelection(int direction)
        {
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (count <= 0)
            {
                if (direction < 0) FocusInventoryBagTabs();
                return;
            }

            int localIndex = _selectedInventoryBagSlot - start;
            if (localIndex < 0 || localIndex >= count)
            {
                if (direction < 0) FocusInventoryBagTabs();
                else SetInventoryBagSelection(start, 0);
                return;
            }

            bool isFirstRow = ModFunc.isInventory
                ? localIndex < InventoryGridColumns
                : !ModFunc.isInventory && localIndex == 0;
            if (direction < 0 && isFirstRow)
            {
                FocusInventoryBagTabs();
                return;
            }

            int step = ModFunc.isInventory ? InventoryGridColumns : 1;
            int target = localIndex + direction * step;
            if (target < 0 || target >= count) return;
            SetInventoryBagSelection(start, target);
        }

        private void SetInventoryBagSelection(int start, int localIndex)
        {
            CloseInventoryDetail();
            _selectedInventoryBagSlot = start + localIndex;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            _inventoryFocusArea = InventoryFocusBagItems;
            _rightScrollAdapter?.ScrollToIndex(ModFunc.isInventory ? localIndex / InventoryGridColumns : localIndex);
            ConfigureInventoryDetailRects();
            SoundMn.gI().panelClick();
        }

        private void FocusInventoryBagTabs()
        {
            CloseInventoryDetail();
            _inventoryFocusArea = InventoryFocusBagTabs;
            SoundMn.gI().panelClick();
        }

        private void FocusInventoryBagItems()
        {
            CloseInventoryDetail();
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (count <= 0) return;
            int localIndex = _selectedInventoryBagSlot - start;
            if (localIndex < 0 || localIndex >= count) localIndex = 0;
            SetInventoryBagSelection(start, localIndex);
        }

        private void SelectInventoryBagItem(int clickedRow)
        {
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int localIndex = clickedRow;
            if (ModFunc.isInventory)
            {
                UiGridLayout grid = new UiGridLayout(_rightBodyRect, InventoryGridColumns,
                    2, 2, InventoryGridRowHeight, InventoryGridRowHeight - 2);
                int column = grid.GetClampedColumn(GameCanvas.px);
                localIndex = clickedRow * InventoryGridColumns + column;
            }
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (localIndex < 0 || localIndex >= count) return;
            int bagIndex = start + localIndex;
            bool openDetail = _selectedInventoryBagSlot == bagIndex && _selectedInventoryBodySlot < 0;
            CloseInventoryDetail();
            _selectedInventoryBagSlot = bagIndex;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            _inventoryFocusArea = InventoryFocusBagItems;
            if (openDetail) OpenInventoryDetail();
            SoundMn.gI().panelClick();
        }

        private void SelectInventoryBodyItem(int slot)
        {
            bool openDetail = _selectedInventoryBodySlot == slot && _selectedInventoryBagSlot < 0;
            CloseInventoryDetail();
            _selectedInventoryBodySlot = slot;
            _selectedInventoryBagSlot = -1;
            _selectedInventoryAction = 0;
            _inventoryFocusArea = InventoryFocusLeft;
            if (openDetail) OpenInventoryDetail();
            SoundMn.gI().panelClick();
        }

        private void OpenInventoryDetail()
        {
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            if (GetInventoryActionCount(item, fromBody) <= 0) return;
            _showInventoryDetail = true;
            _inventoryFocusArea = InventoryFocusActions;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
        }

        private void CloseInventoryDetail()
        {
            _showInventoryDetail = false;
            _inventoryDetailRect = UiRect.Empty;
            for (int i = 0; i < _inventoryActionRects.Length; i++) _inventoryActionRects[i] = UiRect.Empty;
            if (_inventoryFocusArea == InventoryFocusActions)
                _inventoryFocusArea = _selectedInventoryBodySlot >= 0 ? InventoryFocusLeft : InventoryFocusBagItems;
        }

        private void HandleInventoryConfirm()
        {
            if (_inventoryFocusArea == InventoryFocusBagTabs)
            {
                FocusInventoryBagItems();
                return;
            }
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            int actionCount = GetInventoryActionCount(item, fromBody);
            if (actionCount <= 0) return;
            if (!_showInventoryDetail || _inventoryFocusArea != InventoryFocusActions)
            {
                OpenInventoryDetail();
                SoundMn.gI().panelClick();
                return;
            }
            PerformInventoryAction(_selectedInventoryAction);
        }

        private void MoveInventoryActionFocus(int direction)
        {
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            int count = GetInventoryActionCount(item, fromBody);
            if (count <= 0) return;
            int target = _selectedInventoryAction + direction;
            if (fromBody || target < 0 || target >= count || target / 2 != _selectedInventoryAction / 2) return;
            _selectedInventoryAction = target;
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryActionVertical(int direction)
        {
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            int count = GetInventoryActionCount(item, fromBody);
            if (count <= 0) return;
            int step = fromBody ? 1 : 2;
            int target = _selectedInventoryAction + direction * step;
            if (target < 0 || target >= count) return;
            _selectedInventoryAction = target;
            SoundMn.gI().panelClick();
        }

        private Item GetSelectedInventoryItem(out bool fromBody, out int index)
        {
            fromBody = false;
            index = -1;
            Char me = Char.myCharz();
            if (me == null) return null;
            if (_selectedInventoryBodySlot >= 0)
            {
                fromBody = true;
                index = _selectedInventoryBodySlot;
                return me.arrItemBody != null && index < me.arrItemBody.Length ? me.arrItemBody[index] : null;
            }
            index = _selectedInventoryBagSlot;
            return me.arrItemBag != null && index >= 0 && index < me.arrItemBag.Length ? me.arrItemBag[index] : null;
        }

        private static bool CanUseAutoItem(Item item)
        {
            return item != null && item.template != null
                && (item.template.type == 29 || item.template.type == 33 || item.template.id == 380 || item.quantity >= 2);
        }

        private static bool IsAutoItem(Item item)
        {
            return item != null && item.template != null && ModFunc.GI().listItemAuto.Exists(
                autoItem => autoItem.id == item.template.id);
        }

        private static int GetInventoryActionCount(Item item, bool fromBody)
        {
            if (item == null || item.template == null) return 0;
            if (fromBody) return 2;
            return CanUseAutoItem(item) ? 4 : 3;
        }

        private static string GetInventoryActionLabel(Item item, bool fromBody, int action)
        {
            if (fromBody)
            {
                if (action == 0) return "Lấy ra";
                if (action == 1) return "Bỏ ra";
                return string.Empty;
            }
            if (action == 0) return "Sử dụng";
            if (action == 1) return "Sử dụng\ncho đệ tử";
            if (action == 2) return "Bỏ ra";
            if (action == 3) return IsAutoItem(item) ? "Xóa Auto Item" : "Auto Item";
            return string.Empty;
        }

        private void PerformInventoryAction(int action)
        {
            Char me = Char.myCharz();
            if (me == null || me.statusMe == 14)
            {
                GameCanvas.startOKDlg(mResources.can_not_do_when_die);
                return;
            }
            Item item = GetSelectedInventoryItem(out bool fromBody, out int index);
            int actionCount = GetInventoryActionCount(item, fromBody);
            if (item == null || index < 0 || action < 0 || action >= actionCount) return;
            bool actionPerformed = false;

            if (fromBody)
            {
                if (action == 0)
                {
                    Service.gI().getItem(InventoryBodyToBag, (sbyte)index);
                    actionPerformed = true;
                }
                else if (action == 1)
                {
                    Service.gI().useItem(1, InventoryWhereBody, (sbyte)index, -1);
                    actionPerformed = true;
                }
            }
            else if (action == 0)
            {
                if (item.isTypeBody()) Service.gI().getItem(InventoryBagToBody, (sbyte)index);
                else Service.gI().useItem(0, InventoryWhereBag, (sbyte)index, -1);
                actionPerformed = true;
                if (item.template.id == 193 || item.template.id == 194) Close();
            }
            else if (action == 1)
            {
                if (me.havePet)
                {
                    Service.gI().getItem(InventoryBagToPet, (sbyte)index);
                    actionPerformed = true;
                }
                else if (me.havePet2)
                {
                    Service.gI().getItem(InventoryBagToPet2, (sbyte)index);
                    actionPerformed = true;
                }
                else GameScr.info1.addInfo("Bạn chưa có đệ tử.", 0);
            }
            else if (action == 2)
            {
                Service.gI().useItem(1, InventoryWhereBag, (sbyte)index, -1);
                actionPerformed = true;
            }
            else if (action == 3)
            {
                ModFunc.GI().perform(IsAutoItem(item) ? 501 : 500, item);
                actionPerformed = true;
            }
            if (actionPerformed) CloseInventoryDetail();
            SoundMn.gI().panelClick();
        }

        private void EnsureClanChatField()
        {
            if (_clanChatField != null) return;
            _clanChatField = new TField();
            _clanChatField.name = "Soạn tin nhắn ...";
            _clanChatField.setIputType(TField.INPUT_TYPE_ANY);
            _clanChatField.setMaxTextLenght(120);
        }

    }
}
