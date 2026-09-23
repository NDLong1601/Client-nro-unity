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
        private bool HandleKeyboardNavigation()
        {
            if (GameCanvas.keyPressed[23] || GameCanvas.keyPressed[4])
            {
                ConsumeDirectionKeys(23, 4);
                MoveHorizontalFocus(-1);
                return true;
            }
            if (GameCanvas.keyPressed[24] || GameCanvas.keyPressed[6])
            {
                ConsumeDirectionKeys(24, 6);
                MoveHorizontalFocus(1);
                return true;
            }
            if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[2])
            {
                ConsumeDirectionKeys(21, 2);
                MoveVerticalSelection(-1);
                return true;
            }
            if (GameCanvas.keyPressed[22] || GameCanvas.keyPressed[8])
            {
                ConsumeDirectionKeys(22, 8);
                MoveVerticalSelection(1);
                return true;
            }
            if ((_selectedMainTab == 1 || _selectedMainTab == 2 || _selectedMainTab == 3 || _selectedMainTab == 4 || _selectedMainTab == 5 || _selectedMainTab == 6)
                && GameCanvas.keyPressed[Main.isPC ? 25 : 5])
            {
                GameCanvas.keyPressed[25] = false;
                GameCanvas.keyPressed[15] = false;
                GameCanvas.keyPressed[5] = false;
                GameCanvas.keyHold[25] = false;
                GameCanvas.keyHold[15] = false;
                GameCanvas.keyHold[5] = false;
                if (_keyboardFocus == KeyboardFocusMainTabs) MoveHorizontalFocus(1);
                else if (_selectedMainTab == 1) HandleInventoryConfirm();
                else if (_selectedMainTab == 3) HandleClanConfirm();
                else if (_selectedMainTab == 4) HandleFunctionConfirm();
                else if (_selectedMainTab == 5) HandleDiscipleConfirm();
                else if (_selectedMainTab == 6) HandleFriendConfirm();
                else HandleSkillConfirm();
                return true;
            }
            return false;
        }

        private static void ConsumeDirectionKeys(int arrowKey, int legacyKey)
        {
            GameCanvas.keyPressed[arrowKey] = false;
            GameCanvas.keyPressed[legacyKey] = false;
            GameCanvas.keyHold[arrowKey] = false;
            GameCanvas.keyHold[legacyKey] = false;
        }

        private void MoveHorizontalFocus(int direction)
        {
            if (_keyboardFocus == KeyboardFocusMainTabs)
            {
                if (direction <= 0) return;
                _keyboardFocus = KeyboardFocusContent;
                if (_selectedMainTab == 0)
                {
                    _selectedSubTab = 0;
                    RefreshKeyboardPage();
                }
                else if (_selectedMainTab == 1)
                {
                    _inventoryFocusArea = InventoryFocusLeft;
                    if (_selectedInventoryLeftTab == 0 && _selectedInventoryBodySlot < 0)
                        _selectedInventoryBodySlot = EquipmentVisualOrder[0];
                }
                else if (_selectedMainTab == 2 && _selectedSkillRow < 0 && GetSkillRowCount() > 0)
                {
                    _selectedSkillRow = 0;
                    _skillFocusArea = SkillFocusList;
                    _selectedPotentialAction = GetFirstVisiblePotentialAction();
                    RefreshKeyboardPage();
                }
                else if (_selectedMainTab == 3)
                {
                    _clanFocusArea = ClanFocusFunctions;
                    _selectedClanFunction = GetClanFunctionForView(_selectedClanView);
                }
                else if (_selectedMainTab == 5)
                {
                    _discipleFocusArea = 0;
                }
                else if (_selectedMainTab == 6)
                {
                    _friendFocusArea = FriendFocusList;
                }
                SoundMn.gI().panelClick();
                return;
            }

            if (_selectedMainTab == 1)
            {
                if (_inventoryFocusArea == InventoryFocusActions)
                {
                    MoveInventoryActionFocus(direction);
                    return;
                }
                if (_inventoryFocusArea == InventoryFocusBagItems)
                {
                    if (ModFunc.isInventory) MoveInventoryHorizontalSelection(direction);
                    else if (direction < 0)
                    {
                        _inventoryFocusArea = InventoryFocusLeft;
                        SoundMn.gI().panelClick();
                    }
                    return;
                }
                if (_inventoryFocusArea == InventoryFocusBagTabs)
                {
                    if (direction < 0 && _selectedInventoryBagTab > 0) SelectInventoryBagTab(_selectedInventoryBagTab - 1);
                    else if (direction > 0 && _selectedInventoryBagTab < 1) SelectInventoryBagTab(_selectedInventoryBagTab + 1);
                    else if (direction < 0)
                    {
                        _inventoryFocusArea = InventoryFocusLeft;
                        SoundMn.gI().panelClick();
                    }
                    return;
                }
                if (direction < 0)
                {
                    if (_selectedInventoryLeftTab > 0) SelectInventoryLeftTab(_selectedInventoryLeftTab - 1);
                    else
                    {
                        _keyboardFocus = KeyboardFocusMainTabs;
                        SoundMn.gI().panelClick();
                    }
                }
                else if (_selectedInventoryLeftTab < 1) SelectInventoryLeftTab(_selectedInventoryLeftTab + 1);
                else
                {
                    _inventoryFocusArea = InventoryFocusBagTabs;
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (_selectedMainTab == 2)
            {
                if (direction < 0)
                {
                    if (_skillFocusArea == SkillFocusKeys)
                    {
                        _showSkillKeyPicker = false;
                        _skillFocusArea = SkillFocusDetail;
                    }
                    else if (_skillFocusArea == SkillFocusIntrinsicSide)
                    {
                        _skillFocusArea = SkillFocusDetail;
                    }
                    else if (_skillFocusArea == SkillFocusDetail)
                    {
                        _skillFocusArea = SkillFocusList;
                    }
                    else
                    {
                        _keyboardFocus = KeyboardFocusMainTabs;
                    }
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusList)
                {
                    if (_selectedSkillRow >= 0 && _selectedSkillRow < PotentialStatRowCount)
                    {
                        _selectedPotentialAction = GetFirstVisiblePotentialAction();
                        if (_selectedPotentialAction < 0) return;
                        _skillFocusArea = SkillFocusDetail;
                        SoundMn.gI().panelClick();
                    }
                    else if (_selectedSkillRow == IntrinsicRowIndex)
                    {
                        EnsureDefaultIntrinsicActions();
                        if (_intrinsicActionCount <= 0) return;
                        if (_selectedIntrinsicAction < 0) _selectedIntrinsicAction = 0;
                        _skillFocusArea = SkillFocusDetail;
                        SoundMn.gI().panelClick();
                    }
                    else if (_selectedSkillRow >= SkillTemplateStartRow && GetSelectedSkill() != null)
                    {
                        _skillFocusArea = SkillFocusDetail;
                        SoundMn.gI().panelClick();
                    }
                }
                else if (_skillFocusArea == SkillFocusDetail && (_showIntrinsicList || _showIntrinsicConfirmation))
                {
                    _selectedIntrinsicSideAction = _showIntrinsicList && _selectedIntrinsicListIndex >= 0 ? 1 : 0;
                    _skillFocusArea = SkillFocusIntrinsicSide;
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusDetail && _selectedSkillRow >= SkillTemplateStartRow)
                {
                    OpenSkillKeyPicker();
                }
                return;
            }

            if (_selectedMainTab == 3)
            {
                MoveClanHorizontalFocus(direction);
                return;
            }

            if (_selectedMainTab == 4)
            {
                MoveFunctionHorizontalFocus(direction);
                return;
            }

            if (_selectedMainTab == 5)
            {
                MoveDiscipleHorizontalFocus(direction);
                return;
            }

            if (_selectedMainTab == 6)
            {
                MoveFriendHorizontalFocus(direction);
                return;
            }

            if (_selectedMainTab != 0)
            {
                if (direction < 0)
                {
                    _keyboardFocus = KeyboardFocusMainTabs;
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (direction > 0)
            {
                if (_selectedSubTab < 1)
                {
                    _selectedSubTab++;
                    if (!allOtherQuests[_selectedOtherCategoryIndex].hasQuest) SelectFirstAvailableOtherQuest();
                    RefreshKeyboardPage();
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (_selectedSubTab > 0)
            {
                _selectedSubTab--;
                RefreshKeyboardPage();
            }
            else
            {
                _keyboardFocus = KeyboardFocusMainTabs;
            }
            SoundMn.gI().panelClick();
        }

        private void MoveVerticalSelection(int direction)
        {
            if (_keyboardFocus == KeyboardFocusMainTabs)
            {
                CloseInventoryDetail();
                if (_selectedMainTab == 3)
                {
                    _clanChatFocused = false;
                    _clanChatField?.setFocus(false);
                }
                if (_selectedMainTab == 6) LeaveFriendTab();
                _selectedMainTab = (_selectedMainTab + direction + MainTabCount) % MainTabCount;
                _mainTabScrollAdapter.ScrollToIndex(_selectedMainTab);
                if (_selectedMainTab == 0) _selectedSubTab = 0;
                _showSkillKeyPicker = false;
                _skillFocusArea = SkillFocusList;
                _selectedPotentialAction = -1;
                _selectedIntrinsicAction = -1;
                if (_selectedMainTab == 3) EnterClanTab();
                if (_selectedMainTab == 4) EnterFunctionTab();
                if (_selectedMainTab == 5) EnterDiscipleTab();
                if (_selectedMainTab == 6) EnterFriendTab();
                RefreshKeyboardPage();
                SoundMn.gI().panelClick();
                return;
            }

            if (_selectedMainTab == 1)
            {
                if (_inventoryFocusArea == InventoryFocusBagTabs)
                {
                    if (direction > 0) FocusInventoryBagItems();
                }
                else if (_inventoryFocusArea == InventoryFocusBagItems) MoveInventoryVerticalSelection(direction);
                else if (_inventoryFocusArea == InventoryFocusActions) MoveInventoryActionVertical(direction);
                else MoveInventoryLeftSelection(direction);
                return;
            }

            if (_selectedMainTab == 2)
            {
                if (_showIntrinsicList && _skillFocusArea == SkillFocusDetail)
                    MoveIntrinsicListSelection(direction);
                else if (_showIntrinsicList && _skillFocusArea == SkillFocusIntrinsicSide)
                {
                    if (_selectedIntrinsicListIndex < 0) _selectedIntrinsicSideAction = 0;
                    else _selectedIntrinsicSideAction = _selectedIntrinsicSideAction == 0 ? 1 : 0;
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusDetail && _selectedSkillRow < PotentialStatRowCount)
                    MovePotentialAction(direction);
                else if (_skillFocusArea == SkillFocusDetail && _selectedSkillRow == IntrinsicRowIndex)
                    MoveIntrinsicAction(direction);
                else if (_skillFocusArea == SkillFocusKeys)
                {
                    _selectedSkillKeyIndex = (_selectedSkillKeyIndex + direction + SkillKeyButtonCount) % SkillKeyButtonCount;
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusList)
                    MoveRowSelection(direction);
                return;
            }

            if (_selectedMainTab == 3)
            {
                MoveClanVerticalSelection(direction);
                return;
            }

            if (_selectedMainTab == 4)
            {
                MoveFunctionVerticalSelection(direction);
                return;
            }

            if (_selectedMainTab == 5)
            {
                MoveDiscipleVerticalSelection(direction);
                return;
            }

            if (_selectedMainTab == 6)
            {
                MoveFriendVerticalSelection(direction);
                return;
            }

            MoveRowSelection(direction);
        }

        private void HandleSkillConfirm()
        {
            if (_skillFocusArea == SkillFocusList)
            {
                MoveHorizontalFocus(1);
                return;
            }
            if (_skillFocusArea == SkillFocusKeys)
            {
                AssignSelectedSkillToKey(_selectedSkillKeyIndex);
                return;
            }
            if (_showIntrinsicList && _skillFocusArea == SkillFocusIntrinsicSide)
            {
                if (_selectedIntrinsicSideAction == 0) ExitIntrinsicListView();
                else OpenIntrinsicInput();
                return;
            }
            if (_showIntrinsicConfirmation && _skillFocusArea == SkillFocusIntrinsicSide)
            {
                ReturnToIntrinsicMain();
                return;
            }
            if (_skillFocusArea != SkillFocusDetail) return;
            if (_showIntrinsicList)
            {
                if (_selectedIntrinsicListIndex >= 0)
                {
                    _selectedIntrinsicSideAction = 1;
                    _skillFocusArea = SkillFocusIntrinsicSide;
                    SoundMn.gI().panelClick();
                }
                return;
            }
            if (_selectedSkillRow < PotentialStatRowCount)
            {
                if (!IsPotentialActionVisible(_selectedPotentialAction)) return;
                if (_selectedPotentialAction == 3) OpenAutoPotentialInput();
                else IncreaseSelectedPotential(GetPotentialBatch(_selectedPotentialAction));
                return;
            }
            if (_selectedSkillRow == IntrinsicRowIndex)
            {
                PerformIntrinsicAction(_selectedIntrinsicAction);
                return;
            }
            if (_selectedSkillRow >= SkillTemplateStartRow) OpenSkillKeyPicker();
        }

        private void MovePotentialAction(int direction)
        {
            int start = _selectedPotentialAction;
            for (int step = 1; step <= _potentialButtonRects.Length; step++)
            {
                int candidate = (start + direction * step + _potentialButtonRects.Length * 2) % _potentialButtonRects.Length;
                if (!IsPotentialActionVisible(candidate)) continue;
                _selectedPotentialAction = candidate;
                SoundMn.gI().panelClick();
                return;
            }
        }

        private void MoveIntrinsicAction(int direction)
        {
            EnsureDefaultIntrinsicActions();
            if (_intrinsicActionCount <= 0) return;
            if (_selectedIntrinsicAction < 0) _selectedIntrinsicAction = 0;
            else _selectedIntrinsicAction = (_selectedIntrinsicAction + direction + _intrinsicActionCount) % _intrinsicActionCount;
            SoundMn.gI().panelClick();
        }

        private void MoveIntrinsicListSelection(int direction)
        {
            int count = GetIntrinsicListCount();
            if (count <= 0) return;
            if (_selectedIntrinsicListIndex < 0)
                _selectedIntrinsicListIndex = direction > 0 ? 0 : count - 1;
            else
                _selectedIntrinsicListIndex = System.Math.Max(0,
                    System.Math.Min(count - 1, _selectedIntrinsicListIndex + direction));
            _rightScrollAdapter?.ScrollToIndex(_selectedIntrinsicListIndex);
            SoundMn.gI().panelClick();
        }

        private void RefreshKeyboardPage()
        {
            _leftScrollAdapter?.Reset();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            if (_selectedMainTab == 0 && _selectedSubTab == 0)
                _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
            else if (_selectedMainTab == 2 && _selectedSkillRow >= 0)
                _leftScrollAdapter?.ScrollToIndex(_selectedSkillRow);
        }

        private void MoveRowSelection(int direction)
        {
            if (_selectedMainTab == 2)
            {
                int rowCount = GetSkillRowCount();
                if (rowCount <= 0) return;
                if (_selectedSkillRow < 0)
                    _selectedSkillRow = direction > 0 ? 0 : rowCount - 1;
                else
                    _selectedSkillRow = (_selectedSkillRow + direction + rowCount) % rowCount;
                _showSkillKeyPicker = false;
                ExitIntrinsicListView(false);
                _skillFocusArea = SkillFocusList;
                _selectedPotentialAction = GetFirstVisiblePotentialAction();
                RequestSelectedIntrinsicInfo();
                _leftScrollAdapter?.ScrollToIndex(_selectedSkillRow);
                SoundMn.gI().panelClick();
                return;
            }

            if (_selectedMainTab != 0) return;

            if (_selectedSubTab == 0)
            {
                int rowCount = GetMainTaskRowCount();
                if (rowCount <= 0) return;
                _selectedTaskPosition = (_selectedTaskPosition + direction + rowCount) % rowCount;
                _rightScrollAdapter?.Reset();
                ConfigureScrollAdapters();
                _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
                SoundMn.gI().panelClick();
                return;
            }

            List<int> visibleQuests = new List<int>();
            for (int i = 0; i < OtherQuestNavigationOrder.Length; i++)
            {
                int questIndex = OtherQuestNavigationOrder[i];
                if (allOtherQuests[questIndex].hasQuest) visibleQuests.Add(questIndex);
            }
            if (visibleQuests.Count == 0) return;

            int currentPosition = visibleQuests.IndexOf(_selectedOtherCategoryIndex);
            int nextPosition = currentPosition < 0
                ? (direction > 0 ? 0 : visibleQuests.Count - 1)
                : (currentPosition + direction + visibleQuests.Count) % visibleQuests.Count;
            _selectedOtherCategoryIndex = visibleQuests[nextPosition];
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();

            UiRect selectedRect = _otherQuestCardRects[_selectedOtherCategoryIndex];
            if (selectedRect.Width > 0 && _leftScrollAdapter != null)
            {
                int contentOffset = selectedRect.Y - _leftColRect.Y + _leftScrollAdapter.ScrollY;
                _leftScrollAdapter.ScrollToIndex(contentOffset / 10);
            }
            SoundMn.gI().panelClick();
        }

    }
}
