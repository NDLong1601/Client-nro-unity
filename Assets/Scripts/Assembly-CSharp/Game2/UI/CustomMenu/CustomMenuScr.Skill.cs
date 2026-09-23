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
        private static int GetSkillRowCount()
        {
            Char me = Char.myCharz();
            int templateCount = me != null && me.nClass != null && me.nClass.skillTemplates != null
                ? me.nClass.skillTemplates.Length
                : 0;
            return SkillTemplateStartRow + templateCount;
        }

        private void ConfigureSkillActionRects()
        {
            int margin = 7;
            int gap = 6;
            int buttonHeight = 29;
            int buttonWidth = (_rightBodyRect.Width - margin * 2 - gap) / 2;
            int secondRowY = _rightBodyRect.Y + _rightBodyRect.Height - margin - buttonHeight;
            int firstRowY = secondRowY - gap - buttonHeight;
            int leftX = _rightBodyRect.X + margin;
            int rightX = leftX + buttonWidth + gap;

            _potentialButtonRects[0] = new UiRect(leftX, firstRowY, buttonWidth, buttonHeight);
            _potentialButtonRects[1] = new UiRect(rightX, firstRowY, buttonWidth, buttonHeight);
            _potentialButtonRects[2] = new UiRect(leftX, secondRowY, buttonWidth, buttonHeight);
            _potentialButtonRects[3] = new UiRect(rightX, secondRowY, buttonWidth, buttonHeight);
            _assignSkillButtonRect = new UiRect(leftX, secondRowY, _rightBodyRect.Width - margin * 2, buttonHeight);

            int keyGap = 2;
            int keyWidth = 70;
            int keyHeight = System.Math.Max(22, (_frameRect.Height - 4 - keyGap * (SkillKeyButtonCount - 1)) / SkillKeyButtonCount);
            int keyX = _frameRect.X + _frameRect.Width + 4;
            if (keyX + keyWidth > GameCanvas.w - 2) keyX = System.Math.Max(2, GameCanvas.w - keyWidth - 2);
            int keyY = _frameRect.Y + 2;
            for (int i = 0; i < SkillKeyButtonCount; i++)
                _skillKeyButtonRects[i] = new UiRect(keyX, keyY + i * (keyHeight + keyGap), keyWidth, keyHeight);

            int sideHeight = 28;
            _intrinsicSideButtonRects[0] = new UiRect(keyX, keyY, keyWidth, sideHeight);
            _intrinsicSideButtonRects[1] = new UiRect(keyX, keyY + sideHeight + 4, keyWidth, sideHeight);

            int dialogWidth = System.Math.Min(290, GameCanvas.w - 20);
            int dialogHeight = 112;
            int dialogX = (GameCanvas.w - dialogWidth) / 2;
            int dialogY = (GameCanvas.h - dialogHeight) / 2;
            _intrinsicInputDialogRect = new UiRect(dialogX, dialogY, dialogWidth, dialogHeight);
            _intrinsicInputCloseRect = new UiRect(dialogX + dialogWidth - 19, dialogY - 4, 20, 20);
            int inputWidth = dialogWidth - 54;
            _intrinsicNormalButtonRect = new UiRect(dialogX + 25, dialogY + dialogHeight - 34, 78, 27);
            _intrinsicVipButtonRect = new UiRect(dialogX + dialogWidth - 103, dialogY + dialogHeight - 34, 78, 27);
            if (_intrinsicInputField != null)
            {
                _intrinsicInputField.x = dialogX + 27;
                _intrinsicInputField.y = dialogY + 29;
                _intrinsicInputField.width = inputWidth;
                _intrinsicInputField.height = 27;
            }
        }

        private bool HandleSkillPointerInput()
        {
            if ((_showIntrinsicList || _showIntrinsicConfirmation) && GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _intrinsicSideButtonRects.Length; i++)
                {
                    if (i == 1 && (!_showIntrinsicList || _selectedIntrinsicListIndex < 0)) continue;
                    UiRect sideRect = _intrinsicSideButtonRects[i];
                    if (!GameCanvas.isPointer(sideRect.X, sideRect.Y, sideRect.Width, sideRect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedIntrinsicSideAction = i;
                    _skillFocusArea = SkillFocusIntrinsicSide;
                    if (i == 0 && _showIntrinsicConfirmation) ReturnToIntrinsicMain();
                    else if (i == 0) ExitIntrinsicListView();
                    else OpenIntrinsicInput();
                    return true;
                }
            }

            if (_showIntrinsicList && _rightScrollAdapter != null
                && _rightScrollAdapter.UpdateKey(_inputContext, out int intrinsicIndex))
            {
                if (intrinsicIndex >= 0 && intrinsicIndex < GetIntrinsicListCount())
                {
                    _selectedIntrinsicListIndex = intrinsicIndex;
                    _skillFocusArea = SkillFocusDetail;
                    _selectedIntrinsicSideAction = 1;
                    SoundMn.gI().panelClick();
                }
                return true;
            }

            if (_showSkillKeyPicker && GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _skillKeyButtonRects.Length; i++)
                {
                    UiRect rect = _skillKeyButtonRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedSkillKeyIndex = i;
                    AssignSelectedSkillToKey(i);
                    return true;
                }
            }

            if (_leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int clickedIndex))
            {
                if (clickedIndex >= 0 && clickedIndex < GetSkillRowCount())
                {
                    _selectedSkillRow = clickedIndex;
                    _showSkillKeyPicker = false;
                    ExitIntrinsicListView(false);
                    _skillFocusArea = SkillFocusList;
                    _selectedPotentialAction = GetFirstVisiblePotentialAction();
                    RequestSelectedIntrinsicInfo();
                    _keyboardFocus = KeyboardFocusContent;
                    SoundMn.gI().panelClick();
                }
                return true;
            }

            if (GameCanvas.isPointerJustRelease && _selectedSkillRow >= 0 && _selectedSkillRow < PotentialStatRowCount)
            {
                for (int i = 0; i < _potentialButtonRects.Length; i++)
                {
                    if (!IsPotentialActionVisible(i)) continue;
                    UiRect rect = _potentialButtonRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedPotentialAction = i;
                    _skillFocusArea = SkillFocusDetail;
                    if (i == 3) OpenAutoPotentialInput();
                    else IncreaseSelectedPotential(GetPotentialBatch(i));
                    return true;
                }
            }

            if (GameCanvas.isPointerJustRelease && _selectedSkillRow == IntrinsicRowIndex)
            {
                EnsureDefaultIntrinsicActions();
                for (int i = 0; i < _intrinsicActionCount; i++)
                {
                    UiRect rect = GetIntrinsicActionRect(i);
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedIntrinsicAction = i;
                    _skillFocusArea = SkillFocusDetail;
                    PerformIntrinsicAction(i);
                    return true;
                }
            }

            if (GameCanvas.isPointerJustRelease && _selectedSkillRow >= SkillTemplateStartRow
                && GameCanvas.isPointer(_assignSkillButtonRect.X, _assignSkillButtonRect.Y, _assignSkillButtonRect.Width, _assignSkillButtonRect.Height))
            {
                GameCanvas.isPointerJustRelease = false;
                _skillFocusArea = SkillFocusDetail;
                OpenSkillKeyPicker();
                return true;
            }

            if (_showSkillKeyPicker && GameCanvas.isPointerJustRelease)
            {
                _showSkillKeyPicker = false;
                _skillFocusArea = SkillFocusDetail;
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            return false;
        }

        private static long GetPotentialIncreaseValue(Char me, int row)
        {
            if (me == null) return 0L;
            if (row == 0) return me.hpFrom1000TiemNang;
            if (row == 1) return me.mpFrom1000TiemNang;
            if (row == 2) return me.damFrom1000TiemNang;
            if (row == 3) return me.defFrom1000TiemNang;
            if (row == 4) return me.criticalFrom1000Tiemnang;
            return 0L;
        }

        private void RequestSelectedIntrinsicInfo()
        {
            if (_selectedSkillRow != IntrinsicRowIndex)
            {
                ExitIntrinsicListView(false);
                _showIntrinsicConfirmation = false;
                _expectingIntrinsicConfirmation = false;
                return;
            }
            _showIntrinsicConfirmation = false;
            _expectingIntrinsicConfirmation = false;
            _waitingIntrinsicMenu = true;
            _selectedIntrinsicAction = -1;
            EnsureDefaultIntrinsicActions();
            Service.gI().speacialSkill(0);
        }

        private void EnsureDefaultIntrinsicActions()
        {
            if (_intrinsicActionCount > 0 || string.IsNullOrEmpty(Panel.specialInfo)) return;
            _intrinsicActionCount = DefaultIntrinsicActionLabels.Length;
            for (int i = 0; i < _intrinsicActionCount; i++)
            {
                _intrinsicActionLabels[i] = DefaultIntrinsicActionLabels[i];
                _intrinsicActionServerIndices[i] = DefaultIntrinsicActionServerIndices[i];
            }
        }

        private bool CaptureIntrinsicDialog(string text)
        {
            if (!_waitingIntrinsicMenu || _selectedSkillRow != IntrinsicRowIndex) return false;
            _waitingIntrinsicMenu = false;
            _intrinsicDialogText = text ?? string.Empty;
            _intrinsicActionCount = 0;
            string listLabel = null;
            int listServerIndex = -1;

            MyVector menuItems = GameCanvas.menu != null ? GameCanvas.menu.menuItems : null;
            if (menuItems != null)
            {
                int count = System.Math.Min(MaxIntrinsicActionCount, menuItems.size());
                for (int i = 0; i < count; i++)
                {
                    Command command = menuItems.elementAt(i) as Command;
                    if (command == null || string.IsNullOrEmpty(command.caption)) continue;
                    string compact = command.caption.Replace("\r", " ").Replace("\n", " ").Trim();
                    if (compact.IndexOf("Từ chối", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    if (IsIntrinsicListAction(compact))
                    {
                        listLabel = "Danh sách nội tại";
                        listServerIndex = i;
                        continue;
                    }
                    if (_intrinsicActionCount >= _intrinsicActionLabels.Length - 1) continue;
                    _intrinsicActionLabels[_intrinsicActionCount] = FormatIntrinsicActionLabel(compact);
                    _intrinsicActionServerIndices[_intrinsicActionCount] = i;
                    _intrinsicActionCount++;
                }
            }
            if (listServerIndex >= 0 && _intrinsicActionCount < _intrinsicActionLabels.Length)
            {
                _intrinsicActionLabels[_intrinsicActionCount] = listLabel;
                _intrinsicActionServerIndices[_intrinsicActionCount] = listServerIndex;
                _intrinsicActionCount++;
            }
            EnsureDefaultIntrinsicActions();
            _showIntrinsicConfirmation = _expectingIntrinsicConfirmation && _intrinsicActionCount == 1;
            _expectingIntrinsicConfirmation = false;
            _selectedIntrinsicAction = _intrinsicActionCount > 0 ? 0 : -1;

            if (Char.chatPopup != null)
            {
                Effect2.vEffect2.removeElement(Char.chatPopup);
                Char.chatPopup = null;
            }
            GameCanvas.menu?.doCloseMenu();
            return true;
        }

        private static string FormatIntrinsicActionLabel(string label)
        {
            string compact = label.Replace("\r", " ").Replace("\n", " ").Trim();
            if (IsIntrinsicListAction(compact)) return "Danh sách nội tại";
            return compact;
        }

        private static bool IsIntrinsicListAction(string label)
        {
            return label.IndexOf("Xem tất cả", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("Danh sách", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void PerformIntrinsicAction(int actionIndex)
        {
            if (ModFunc.GI().IsAutoIntrinsicRunning)
            {
                GameScr.info1.addInfo("Đang tự động mở nội tại.", 0);
                return;
            }
            EnsureDefaultIntrinsicActions();
            if (actionIndex < 0 || actionIndex >= _intrinsicActionCount) return;
            string label = _intrinsicActionLabels[actionIndex] ?? string.Empty;
            int serverIndex = _intrinsicActionServerIndices[actionIndex];
            if (serverIndex < 0) return;
            if (_showIntrinsicConfirmation)
            {
                Service.gI().confirmMenu(IntrinsicNpcId, (sbyte)serverIndex);
                _showIntrinsicConfirmation = false;
                _waitingIntrinsicMenu = true;
                _expectingIntrinsicConfirmation = false;
                Service.gI().speacialSkill(0);
                SoundMn.gI().panelClick();
                return;
            }
            bool opensIntrinsicList = IsIntrinsicListAction(label);
            if (opensIntrinsicList)
            {
                _waitingIntrinsicList = true;
                _selectedIntrinsicListIndex = -1;
            }
            else
            {
                _waitingIntrinsicMenu = true;
                _expectingIntrinsicConfirmation = true;
            }
            Service.gI().confirmMenu(IntrinsicNpcId, (sbyte)serverIndex);
            if (opensIntrinsicList && HasIntrinsicListData()) EnterIntrinsicListView();
            SoundMn.gI().panelClick();
        }

        private void ReturnToIntrinsicMain()
        {
            _showIntrinsicConfirmation = false;
            _selectedIntrinsicSideAction = 0;
            _skillFocusArea = SkillFocusDetail;
            _waitingIntrinsicMenu = true;
            _expectingIntrinsicConfirmation = false;
            Service.gI().speacialSkill(0);
            SoundMn.gI().panelClick();
        }

        private UiRect GetIntrinsicActionRect(int actionIndex)
        {
            if (_showIntrinsicConfirmation) return _assignSkillButtonRect;
            return actionIndex == _intrinsicActionCount - 1 && IsIntrinsicListAction(_intrinsicActionLabels[actionIndex] ?? string.Empty)
                ? _assignSkillButtonRect
                : _potentialButtonRects[actionIndex];
        }

        private static int GetIntrinsicListCount()
        {
            Char me = Char.myCharz();
            return me != null && me.infoSpeacialSkill != null && me.infoSpeacialSkill.Length > 0
                && me.infoSpeacialSkill[0] != null ? me.infoSpeacialSkill[0].Length : 0;
        }

        private static bool HasIntrinsicListData()
        {
            return GetIntrinsicListCount() > 0;
        }

        private void EnterIntrinsicListView()
        {
            _waitingIntrinsicList = false;
            _showIntrinsicList = true;
            _showIntrinsicConfirmation = false;
            _showSkillKeyPicker = false;
            _selectedIntrinsicListIndex = -1;
            _selectedIntrinsicSideAction = 0;
            _skillFocusArea = SkillFocusDetail;
            GameCanvas.panel?.hide();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
        }

        private void ExitIntrinsicListView(bool playSound = true)
        {
            bool wasVisible = _showIntrinsicList || _showIntrinsicInput;
            _waitingIntrinsicList = false;
            _showIntrinsicList = false;
            CloseIntrinsicInput();
            _selectedIntrinsicListIndex = -1;
            _selectedIntrinsicSideAction = 0;
            if (_selectedSkillRow == IntrinsicRowIndex) _skillFocusArea = SkillFocusDetail;
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            if (playSound && wasVisible) SoundMn.gI().panelClick();
        }

        private string GetSelectedIntrinsicInfo()
        {
            Char me = Char.myCharz();
            if (me == null || me.infoSpeacialSkill == null || me.infoSpeacialSkill.Length == 0
                || me.infoSpeacialSkill[0] == null || _selectedIntrinsicListIndex < 0
                || _selectedIntrinsicListIndex >= me.infoSpeacialSkill[0].Length) return string.Empty;
            return me.infoSpeacialSkill[0][_selectedIntrinsicListIndex] ?? string.Empty;
        }

        private void OpenIntrinsicInput()
        {
            string selectedInfo = GetSelectedIntrinsicInfo();
            if (string.IsNullOrEmpty(selectedInfo)) return;
            _showIntrinsicInput = true;
            _intrinsicInputFocus = 0;
            _intrinsicInputField = new TField();
            _intrinsicInputField.name = "Nhập chỉ số mong muốn....";
            _intrinsicInputField.setIputType(TField.INPUT_TYPE_NUMERIC);
            _intrinsicInputField.setMaxTextLenght(3);
            ConfigureSkillActionRects();
            _intrinsicInputField.setFocusWithKb(true);
            GameCanvas.keyAsciiPress = 0;
            GameCanvas.clearKeyPressed();
            SoundMn.gI().panelClick();
        }

        private void CloseIntrinsicInput()
        {
            _showIntrinsicInput = false;
            if (_intrinsicInputField != null) _intrinsicInputField.setFocus(false);
            _intrinsicInputField = null;
            _intrinsicInputFocus = 0;
        }

        private void SetIntrinsicInputFocus(int focus)
        {
            _intrinsicInputFocus = focus;
            if (_intrinsicInputField != null) _intrinsicInputField.setFocus(focus == 0);
            SoundMn.gI().panelClick();
        }

        private void HandleIntrinsicInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                if (GameCanvas.isPointer(_intrinsicInputCloseRect.X, _intrinsicInputCloseRect.Y,
                    _intrinsicInputCloseRect.Width, _intrinsicInputCloseRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    CloseIntrinsicInput();
                    return;
                }
                if (_intrinsicInputField != null && GameCanvas.isPointer(_intrinsicInputField.x, _intrinsicInputField.y,
                    _intrinsicInputField.width, _intrinsicInputField.height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SetIntrinsicInputFocus(0);
                    _intrinsicInputField.setFocusWithKb(true);
                    return;
                }
                if (GameCanvas.isPointer(_intrinsicNormalButtonRect.X, _intrinsicNormalButtonRect.Y,
                    _intrinsicNormalButtonRect.Width, _intrinsicNormalButtonRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SetIntrinsicInputFocus(1);
                    SubmitIntrinsicInput(false);
                    return;
                }
                if (GameCanvas.isPointer(_intrinsicVipButtonRect.X, _intrinsicVipButtonRect.Y,
                    _intrinsicVipButtonRect.Width, _intrinsicVipButtonRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SetIntrinsicInputFocus(2);
                    SubmitIntrinsicInput(true);
                    return;
                }
                GameCanvas.clearAllPointerEvent();
            }

            if (GameCanvas.keyAsciiPress == 27 || GameCanvas.keyPressed[12] || GameCanvas.keyPressed[13])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                CloseIntrinsicInput();
                return;
            }

            if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[2])
            {
                ConsumeDirectionKeys(21, 2);
                SetIntrinsicInputFocus(0);
                return;
            }
            if (GameCanvas.keyPressed[22] || GameCanvas.keyPressed[8])
            {
                ConsumeDirectionKeys(22, 8);
                SetIntrinsicInputFocus(_intrinsicInputFocus == 2 ? 2 : 1);
                return;
            }
            if (GameCanvas.keyPressed[23] || GameCanvas.keyPressed[4])
            {
                ConsumeDirectionKeys(23, 4);
                if (_intrinsicInputFocus > 0) SetIntrinsicInputFocus(1);
                return;
            }
            if (GameCanvas.keyPressed[24] || GameCanvas.keyPressed[6])
            {
                ConsumeDirectionKeys(24, 6);
                if (_intrinsicInputFocus > 0) SetIntrinsicInputFocus(2);
                return;
            }

            if (GameCanvas.keyPressed[Main.isPC ? 25 : 5])
            {
                GameCanvas.keyPressed[25] = false;
                GameCanvas.keyPressed[15] = false;
                GameCanvas.keyPressed[5] = false;
                if (_intrinsicInputFocus == 0) SetIntrinsicInputFocus(1);
                else SubmitIntrinsicInput(_intrinsicInputFocus == 2);
                return;
            }

            int ascii = GameCanvas.keyAsciiPress;
            if (_intrinsicInputFocus == 0 && ascii != 0 && ascii != 10 && ascii != 13)
            {
                _intrinsicInputField?.keyPressed(ascii);
                GameCanvas.keyAsciiPress = 0;
            }
        }

        private void SubmitIntrinsicInput(bool vip)
        {
            string value = _intrinsicInputField != null ? _intrinsicInputField.getText() : string.Empty;
            if (!int.TryParse(value, out int target) || target <= 0)
            {
                GameScr.info1.addInfo("Chỉ số đã nhập không hợp lệ.", 0);
                return;
            }
            string selectedInfo = GetSelectedIntrinsicInfo();
            if (string.IsNullOrEmpty(selectedInfo)) return;
            ModFunc.GI().curSelectIntrinsic = selectedInfo;
            ModFunc.GI().SetAutoIntrinsic(target, vip);
            CloseIntrinsicInput();
            ExitIntrinsicListView(false);
        }

        private static long GetPotentialCurrentValue(Char me, int row)
        {
            if (me == null) return 0L;
            if (row == 0) return me.cHPGoc;
            if (row == 1) return me.cMPGoc;
            if (row == 2) return me.cDamGoc;
            if (row == 3) return me.cDefGoc;
            if (row == 4) return me.cCriticalGoc;
            return 0L;
        }

        private static long GetPotentialCost(Char me, int row)
        {
            if (me == null) return 0L;
            if (row == 0) return me.cHPGoc + 1000L;
            if (row == 1) return me.cMPGoc + 1000L;
            if (row == 2) return me.cDamGoc * (long)me.expForOneAdd;
            if (row == 3) return 500000L + me.cDefGoc * 100000L;
            if (row == 4 && Panel.t_tiemnang != null && Panel.t_tiemnang.Length > 0)
            {
                int level = System.Math.Max(0, System.Math.Min(me.cCriticalGoc, Panel.t_tiemnang.Length - 1));
                return Panel.t_tiemnang[level];
            }
            return 0L;
        }

        private static long GetPotentialBatchCost(Char me, int row, int batch)
        {
            if (me == null || batch <= 0) return 0L;
            if (row == 0) return batch * (2L * (me.cHPGoc + 1000L) + (batch - 1L) * 20L) / 2L;
            if (row == 1) return batch * (2L * (me.cMPGoc + 1000L) + (batch - 1L) * 20L) / 2L;
            if (row == 2) return batch * (2L * me.cDamGoc + batch - 1L) / 2L * me.expForOneAdd;
            if (row == 3) return batch * (2L * (me.cDefGoc + 5L) + batch - 1L) / 2L * 100000L;
            if (row == 4 && Panel.t_tiemnang != null && Panel.t_tiemnang.Length > 0)
            {
                long total = 0L;
                for (int i = 0; i < batch; i++)
                {
                    int level = System.Math.Max(0, System.Math.Min(me.cCriticalGoc + i, Panel.t_tiemnang.Length - 1));
                    total += Panel.t_tiemnang[level];
                }
                return total;
            }
            return 0L;
        }

        private static int GetPotentialBatch(int actionIndex)
        {
            return actionIndex == 0 ? 1 : (actionIndex == 1 ? 10 : 100);
        }

        private bool IsPotentialActionVisible(int actionIndex)
        {
            Char me = Char.myCharz();
            if (actionIndex < 0 || actionIndex >= _potentialButtonRects.Length || me == null
                || _selectedSkillRow < 0 || _selectedSkillRow >= PotentialStatRowCount) return false;
            // The original panel only supports a single critical upgrade per request.
            if (_selectedSkillRow == 4 && actionIndex != 0) return false;
            int batch = actionIndex == 3 ? 1 : GetPotentialBatch(actionIndex);
            long required = GetPotentialBatchCost(me, _selectedSkillRow, batch);
            return required > 0L && me.cTiemNang >= required;
        }

        private int GetFirstVisiblePotentialAction()
        {
            for (int i = 0; i < _potentialButtonRects.Length; i++)
                if (IsPotentialActionVisible(i)) return i;
            return -1;
        }

        private void IncreaseSelectedPotential(int batch)
        {
            Char me = Char.myCharz();
            if (me == null || _selectedSkillRow < 0 || _selectedSkillRow >= PotentialStatRowCount) return;
            if (me.statusMe == 14)
            {
                GameCanvas.startOKDlg(mResources.can_not_do_when_die);
                return;
            }

            long required = GetPotentialBatchCost(me, _selectedSkillRow, batch);
            if (required <= 0L || me.cTiemNang < required)
            {
                GameCanvas.startOKDlg("Không đủ tiềm năng. Cần " + NinjaUtil.getMoneys(required) + ".", isError: false);
                return;
            }

            Service.gI().upPotential(false, _selectedSkillRow, batch);
            SoundMn.gI().panelClick();
        }

        private void OpenAutoPotentialInput()
        {
            if (_selectedSkillRow < 0 || _selectedSkillRow >= PotentialStatRowCount || !IsPotentialActionVisible(3)) return;
            _showSkillKeyPicker = false;
            Close();
            ModFunc.GI().perform(100, _selectedSkillRow + "-False");
        }

        private SkillTemplate GetSelectedSkillTemplate()
        {
            int templateIndex = _selectedSkillRow - SkillTemplateStartRow;
            Char me = Char.myCharz();
            if (templateIndex < 0 || me == null || me.nClass == null || me.nClass.skillTemplates == null
                || templateIndex >= me.nClass.skillTemplates.Length) return null;
            return me.nClass.skillTemplates[templateIndex];
        }

        private Skill GetSelectedSkill()
        {
            SkillTemplate template = GetSelectedSkillTemplate();
            Char me = Char.myCharz();
            return template != null && me != null ? me.getSkill(template) : null;
        }

        private void OpenSkillKeyPicker()
        {
            Skill skill = GetSelectedSkill();
            if (skill == null)
            {
                GameCanvas.startOKDlg("Kỹ năng chưa được học.", isError: false);
                return;
            }
            bool useTouchSlots = GameCanvas.isTouch && !Main.isPC;
            Skill[] slots = useTouchSlots ? GameScr.onScreenSkill : GameScr.keySkill;
            _selectedSkillKeyIndex = 0;
            if (slots != null)
            {
                int limit = System.Math.Min(SkillKeyButtonCount, slots.Length);
                for (int i = 0; i < limit; i++)
                {
                    if (slots[i] == null || slots[i].template == null || slots[i].template.id != skill.template.id) continue;
                    _selectedSkillKeyIndex = i;
                    break;
                }
            }
            _showSkillKeyPicker = true;
            _skillFocusArea = SkillFocusKeys;
            SoundMn.gI().panelClick();
        }

        private void AssignSelectedSkillToKey(int keyIndex)
        {
            Skill skill = GetSelectedSkill();
            if (skill == null) return;
            bool useTouchSlots = GameCanvas.isTouch && !Main.isPC;
            Skill[] slots = useTouchSlots ? GameScr.onScreenSkill : GameScr.keySkill;
            if (slots == null || keyIndex < 0 || keyIndex >= slots.Length) return;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].template != null && slots[i].template.id == skill.template.id)
                    slots[i] = null;
            }
            slots[keyIndex] = skill;
            if (useTouchSlots) GameScr.gI().saveonScreenSkillToRMS();
            else GameScr.gI().saveKeySkillToRMS();
            _showSkillKeyPicker = false;
            _skillFocusArea = SkillFocusDetail;
            SoundMn.gI().panelClick();
        }

    }
}
