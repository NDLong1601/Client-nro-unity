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
        private void ConfigureClanRects()
        {
            _clanChatHeaderRect = _skillListHeaderRect;
            int composerHeight = 32;
            _clanChatComposerRect = new UiRect(_leftColRect.X, _leftColRect.Bottom - composerHeight,
                _leftColRect.Width, composerHeight);
            _clanChatListRect = new UiRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width,
                System.Math.Max(1, _leftColRect.Height - composerHeight - 2));
            _clanShareRect = new UiRect(_clanChatComposerRect.X + 2, _clanChatComposerRect.Y + 3, 31, 25);
            _clanSendRect = new UiRect(_clanChatComposerRect.Right - 39, _clanChatComposerRect.Y + 3, 37, 25);
            EnsureClanChatField();
            _clanChatField.x = _clanShareRect.Right + 3;
            _clanChatField.y = _clanChatComposerRect.Y + 3;
            _clanChatField.width = System.Math.Max(36, _clanSendRect.X - _clanChatField.x - 3);
            _clanChatField.height = 25;

            const int functionGap = 3;
            const int functionHeight = 22;
            int functionWidth = (_rightColRect.Width - functionGap) / 2;
            for (int i = 0; i < _clanFunctionRects.Length; i++)
            {
                int column = i % 2;
                int row = i / 2;
                _clanFunctionRects[i] = new UiRect(_rightColRect.X + column * (functionWidth + functionGap),
                    _rightColRect.Y + row * (functionHeight + functionGap), functionWidth, functionHeight);
            }
            _clanRightHeaderRect = new UiRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, 22);
            int bodyY = _selectedClanView == ClanViewHistory
                ? _clanRightHeaderRect.Bottom + 4
                : _rightColRect.Y + 3 * (functionHeight + functionGap);
            _clanRightBodyRect = new UiRect(_rightColRect.X, bodyY, _rightColRect.Width,
                System.Math.Max(1, _rightColRect.Bottom - bodyY));

            const int sideWidth = 54;
            const int sideHeight = 23;
            const int sideGap = 4;
            int sideX = _frameRect.Right + 3;
            if (sideX + sideWidth > GameCanvas.w - 2) sideX = System.Math.Max(2, GameCanvas.w - sideWidth - 2);
            for (int i = 0; i < _clanSideActionRects.Length; i++)
                _clanSideActionRects[i] = new UiRect(sideX, _frameRect.Y + 4 + i * (sideHeight + sideGap),
                    sideWidth, sideHeight);

            _clanUpgradeButtonRect = new UiRect(_clanRightBodyRect.X + (_clanRightBodyRect.Width - 100) / 2,
                _clanRightBodyRect.Bottom - 43, 100, 38);

            int dialogWidth = System.Math.Min(340, GameCanvas.w - 20);
            int dialogHeight = 122;
            int dialogX = (GameCanvas.w - dialogWidth) / 2;
            int dialogY = (GameCanvas.h - dialogHeight) / 2;
            _clanDialogRect = new UiRect(dialogX, dialogY, dialogWidth, dialogHeight);
            _clanDialogCloseRect = new UiRect(dialogX + dialogWidth - 23, dialogY + 5, 18, 18);
            _clanDialogSubmitRect = new UiRect(dialogX + (dialogWidth - 94) / 2,
                dialogY + dialogHeight - 35, 94, 27);
            if (_clanDialogField != null)
            {
                _clanDialogField.x = dialogX + 25;
                _clanDialogField.y = dialogY + 50;
                _clanDialogField.width = dialogWidth - 50;
                _clanDialogField.height = 28;
            }
        }

        private void EnsureFunctionWorldChatField()
        {
            if (_functionWorldChatField != null) return;
            _functionWorldChatField = new TField();
            _functionWorldChatField.name = "Nhập nội dung cần chat ở đây ...";
            _functionWorldChatField.setIputType(TField.INPUT_TYPE_ANY);
            _functionWorldChatField.setMaxTextLenght(120);
        }

        private void ConfigureFunctionRects()
        {
            const int gap = 3;
            int width = (_leftColRect.Width - gap) / 2;
            int height = System.Math.Max(1, (_leftColRect.Height - gap * 5) / 6);
            for (int i = 0; i < _functionMenuRects.Length; i++)
            {
                int column = i % 2;
                int row = i / 2;
                _functionMenuRects[i] = new UiRect(_leftColRect.X + column * (width + gap),
                    _leftColRect.Y + row * (height + gap), width, height);
            }

            int zoneGap = 3;
            int zoneWidth = (_rightBodyRect.Width - zoneGap * 3) / 4;
            int zoneHeight = System.Math.Max(22, (_rightBodyRect.Height - zoneGap * 4) / 5);
            for (int i = 0; i < _functionZoneRects.Length; i++)
            {
                int column = i % 4;
                int row = i / 4;
                _functionZoneRects[i] = new UiRect(_rightBodyRect.X + column * (zoneWidth + zoneGap),
                    _rightBodyRect.Y + row * (zoneHeight + zoneGap), zoneWidth, zoneHeight);
            }

            int toggleHeight = System.Math.Max(25, (_rightBodyRect.Height - 20) / _functionToggleRects.Length);
            for (int i = 0; i < _functionToggleRects.Length; i++)
                _functionToggleRects[i] = new UiRect(_rightBodyRect.X + 3,
                    _rightBodyRect.Y + 18 + i * toggleHeight, _rightBodyRect.Width - 6, toggleHeight - 3);

            int composerHeight = 31;
            _functionWorldChatComposerRect = new UiRect(_rightBodyRect.X, _rightBodyRect.Bottom - composerHeight,
                _rightBodyRect.Width, composerHeight);
            _functionWorldChatListRect = new UiRect(_rightBodyRect.X, _rightBodyRect.Y, _rightBodyRect.Width,
                System.Math.Max(1, _rightBodyRect.Height - composerHeight - 3));
            _functionWorldChatSendRect = new UiRect(_functionWorldChatComposerRect.Right - 42,
                _functionWorldChatComposerRect.Y + 3, 39, 25);
            EnsureFunctionWorldChatField();
            _functionWorldChatField.x = _functionWorldChatComposerRect.X + 4;
            _functionWorldChatField.y = _functionWorldChatComposerRect.Y + 3;
            _functionWorldChatField.width = System.Math.Max(35, _functionWorldChatSendRect.X - _functionWorldChatField.x - 4);
            _functionWorldChatField.height = 25;
        }

        private void EnterFunctionTab()
        {
            _functionFocusArea = FunctionFocusMenu;
            _functionWorldChatFocused = false;
            _functionWorldChatField?.setFocus(false);
            ConfigureFunctionRects();
            ConfigureScrollAdapters();
        }

        private void EnterClanTab()
        {
            _clanFocusArea = ClanFocusFunctions;
            _selectedClanFunction = GetClanFunctionForView(_selectedClanView);
            _selectedClanRow = 0;
            _clanChatFocused = false;
            EnsureClanChatField();
            ConfigureClanRects();
            RefreshClanData();
        }

        private void RefreshClanData()
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            ClanProgression.requestSnapshot(false);
            ClanProgression.requestBuffSnapshot(false);
            Service.gI().clanTreasuryView();
            ClanValue.requestSnapshot(false);
            ClanAppearance.requestSnapshot(false);
        }

        private static MyVector GetClanMembers()
        {
            if (GameCanvas.panel != null && GameCanvas.panel.myMember != null)
                return GameCanvas.panel.myMember;
            return new MyVector();
        }

        private static Member FindClanMember(int playerId)
        {
            MyVector members = GetClanMembers();
            for (int i = 0; i < members.size(); i++)
            {
                Member member = members.elementAt(i) as Member;
                if (member != null && member.ID == playerId) return member;
            }
            return null;
        }

        private int GetClanContentItemCount()
        {
            if (_selectedClanView == ClanViewMembers) return GetClanMembers().size();
            if (_selectedClanView == ClanViewInfo) return BuildClanInfoLines().Count;
            if (_selectedClanView == ClanViewPotential) return ClanProgression.BRANCH_COUNT;
            if (_selectedClanView == ClanViewHistory) return GetClanContributionLedgerCount();
            if (_selectedClanView == ClanViewTreasury)
            {
                Char me = Char.myCharz();
                int count = me != null && me.arrItemBox != null ? me.arrItemBox.Length : 0;
                return (count + ClanStorageColumns - 1) / ClanStorageColumns;
            }
            return 0;
        }

        private int GetClanContentRowHeight()
        {
            if (_selectedClanView == ClanViewMembers) return ClanMemberRowHeight;
            if (_selectedClanView == ClanViewInfo) return 13;
            if (_selectedClanView == ClanViewPotential) return ClanPotentialRowHeight;
            if (_selectedClanView == ClanViewHistory) return ClanLedgerRowHeight;
            if (_selectedClanView == ClanViewTreasury) return ClanStorageRowHeight;
            return 10;
        }

        private UiRect GetClanScrollableBodyRect()
        {
            if (_selectedClanView == ClanViewInfo)
                return new UiRect(_clanRightBodyRect.X + 3, _clanRightBodyRect.Y + 3,
                    System.Math.Max(1, _clanRightBodyRect.Width - 6),
                    System.Math.Max(1, _clanRightBodyRect.Height - 7));
            if (_selectedClanView != ClanViewPotential) return _clanRightBodyRect;
            const int pointsHeaderHeight = 17;
            return new UiRect(_clanRightBodyRect.X, _clanRightBodyRect.Y + pointsHeaderHeight,
                _clanRightBodyRect.Width,
                System.Math.Max(1, _clanRightBodyRect.Height - pointsHeaderHeight));
        }

        private static int GetClanContributionLedgerCount()
        {
            int count = 0;
            for (int i = 0; i < ClanTreasury.current.ledger.size(); i++)
            {
                ClanLedgerEntry entry = ClanTreasury.current.ledger.elementAt(i) as ClanLedgerEntry;
                if (ClanTreasury.isPlayerContributionEntry(entry)) count++;
            }
            return count;
        }

        private static int GetClanFunctionForView(int view)
        {
            if (view == ClanViewInfo) return 1;
            if (view == ClanViewTreasury || view == ClanViewHistory) return 2;
            if (view == ClanViewPotential) return 3;
            if (view == ClanViewUpgrade) return 5;
            return 0;
        }

        private void SelectClanView(int view)
        {
            _selectedClanView = view;
            _selectedClanFunction = GetClanFunctionForView(view);
            _selectedClanRow = 0;
            _selectedClanStorageSlot = -1;
            ConfigureClanRects();
            _rightScrollAdapter?.Reset();
            if (view == ClanViewInfo) RefreshClanData();
            else if (view == ClanViewPotential || view == ClanViewUpgrade)
                ClanProgression.requestSnapshot(true);
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void OpenClanTreasury()
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            _selectedClanView = ClanViewTreasury;
            _selectedClanFunction = 2;
            _selectedClanRow = 0;
            _selectedClanStorageSlot = -1;
            _clanStorageLoaded = false;
            ConfigureClanRects();
            _rightScrollAdapter?.Reset();
            Service.gI().clanTreasuryView();
            Service.gI().clanItemStorageView();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        public static bool TryConsumeClanStorageOpen()
        {
            if (!_isOpen || _instance == null || _instance._selectedMainTab != 3
                || _instance._selectedClanView != ClanViewTreasury)
                return false;
            _instance._clanStorageLoaded = true;
            _instance._selectedClanStorageSlot = -1;
            _instance._rightScrollAdapter?.Reset();
            _instance.ConfigureScrollAdapters();
            ClanProgression.requestBuffSnapshot(true);
            return true;
        }

        private bool HandleClanPointerInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _clanFunctionRects.Length; i++)
                {
                    UiRect rect = _clanFunctionRects[i];
                    if (_selectedClanView != ClanViewHistory
                        && GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height))
                    {
                        GameCanvas.clearAllPointerEvent();
                        _keyboardFocus = KeyboardFocusContent;
                        _clanFocusArea = ClanFocusFunctions;
                        _selectedClanFunction = i;
                        ActivateClanFunction(i);
                        return true;
                    }
                }
                int sideCount = GetClanSideActionCount();
                for (int i = 0; i < sideCount; i++)
                {
                    UiRect rect = _clanSideActionRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.clearAllPointerEvent();
                    _keyboardFocus = KeyboardFocusContent;
                    _clanFocusArea = ClanFocusSideActions;
                    _selectedClanSideAction = i;
                    ActivateClanSideAction(i);
                    return true;
                }
                if (GameCanvas.isPointer(_clanShareRect.X, _clanShareRect.Y,
                    _clanShareRect.Width, _clanShareRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    Service.gI().shareClanLocation();
                    return true;
                }
                if (GameCanvas.isPointer(_clanSendRect.X, _clanSendRect.Y,
                    _clanSendRect.Width, _clanSendRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SendClanChat();
                    return true;
                }
                if (_clanChatField != null && GameCanvas.isPointer(_clanChatField.x, _clanChatField.y,
                    _clanChatField.width, _clanChatField.height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanFocusArea = ClanFocusChat;
                    _clanChatFocused = true;
                    _clanChatField.setFocusWithKb(true);
                    return true;
                }
                if (_selectedClanView == ClanViewUpgrade
                    && GameCanvas.isPointer(_clanUpgradeButtonRect.X, _clanUpgradeButtonRect.Y,
                        _clanUpgradeButtonRect.Width, _clanUpgradeButtonRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanFocusArea = ClanFocusContent;
                    ActivateClanContent(0);
                    return true;
                }
                if (_selectedClanView == ClanViewPotential)
                {
                    for (int branch = 0; branch < ClanProgression.BRANCH_COUNT; branch++)
                    {
                        UiRect rect = _clanPotentialRects[branch];
                        if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                        GameCanvas.clearAllPointerEvent();
                        _clanFocusArea = ClanFocusContent;
                        _selectedClanRow = branch;
                        ActivateClanContent(branch);
                        return true;
                    }
                }
            }

            if (_leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int chatIndex))
            {
                _clanChatFocused = false;
                _clanChatField?.setFocus(false);
                if (chatIndex >= 0) ActivateClanMessage(chatIndex);
                return true;
            }
            if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out int contentIndex))
            {
                _clanFocusArea = ClanFocusContent;
                if (contentIndex >= 0)
                {
                    if (_selectedClanView == ClanViewTreasury)
                    {
                        UiGridLayout grid = new UiGridLayout(_clanRightBodyRect, ClanStorageColumns,
                            2, 2, ClanStorageRowHeight, ClanStorageRowHeight - 2);
                        int column = grid.GetProportionalColumn(GameCanvas.px);
                        _selectedClanRow = contentIndex;
                        ActivateClanContent(contentIndex * ClanStorageColumns + column);
                    }
                    else
                    {
                        _selectedClanRow = contentIndex;
                        if (_selectedClanView != ClanViewPotential)
                            ActivateClanContent(contentIndex);
                    }
                }
                return true;
            }

            int ascii = GameCanvas.keyAsciiPress;
            if (HandleClanTextBackspace(_clanChatField, _clanChatFocused)) return true;
            if (_clanChatFocused && ascii != 0 && ascii != 10 && ascii != 13)
            {
                _clanChatField?.keyPressed(ascii);
                GameCanvas.keyAsciiPress = 0;
                return true;
            }
            return false;
        }

        private void ActivateClanFunction(int index)
        {
            if (index == 0) SelectClanView(ClanViewMembers);
            else if (index == 1) SelectClanView(ClanViewInfo);
            else if (index == 2) OpenClanTreasury();
            else if (index == 3) SelectClanView(ClanViewPotential);
            else if (index == 4)
            {
                Service.gI().clanMessage(1, null, -1);
                GameScr.info1.addInfo("Đã gửi xin đậu trong bang.", 0);
                SoundMn.gI().panelClick();
            }
            else if (index == 5) SelectClanView(ClanViewUpgrade);
        }

        private void ActivateClanContent(int index)
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            if (_selectedClanView == ClanViewPotential)
            {
                if (!ClanProgression.isReady(me.clan.ID))
                    GameScr.info1.addInfo("Đang tải tiến trình bang.", 0);
                else if (me.role != 0)
                    GameScr.info1.addInfo("Chỉ bang chủ được cộng điểm tiềm năng bang.", 0);
                else if (index >= 0 && index < ClanProgression.BRANCH_COUNT)
                    ClanProgression.allocate(index);
                return;
            }
            if (_selectedClanView == ClanViewUpgrade)
            {
                if (!ClanProgression.isReady(me.clan.ID))
                    GameScr.info1.addInfo("Đang tải tiến trình bang.", 0);
                else if (me.role != 0)
                    GameScr.info1.addInfo("Chỉ bang chủ được nâng cấp bang.", 0);
                else Service.gI().clanProgression(ClanProgression.REQUEST_UPGRADE);
                return;
            }
            if (_selectedClanView == ClanViewTreasury && _clanStorageLoaded
                && me.arrItemBox != null && index >= 0 && index < me.arrItemBox.Length)
            {
                if (me.arrItemBox[index] == null) return;
                if (_selectedClanStorageSlot == index)
                    Service.gI().clanItemStorageUse(index);
                else
                {
                    _selectedClanStorageSlot = index;
                    GameScr.info1.addInfo("Chọn lại để dùng " + me.arrItemBox[index].template.name + ".", 0);
                }
            }
        }

        private void ActivateClanMessage(int index)
        {
            if (index < 0 || index >= ClanMessage.vMessage.size()) return;
            ClanMessage message = ClanMessage.vMessage.elementAt(index) as ClanMessage;
            int visibleOptionCount = GetVisibleClanMessageOptionCount(message);
            if (visibleOptionCount == 0) return;
            if (message.type == 1)
                Service.gI().clanDonate(message.id);
            else if (message.type == 4)
                Service.gI().clanTree(ClanTree.REQUEST_HELP_WATER);
            else if (message.type == 2 && Char.myCharz().role == 0)
            {
                int optionWidth = 37;
                int rowRight = _clanChatListRect.Right - 2;
                int optionStart = rowRight - visibleOptionCount * optionWidth;
                int option = (GameCanvas.px - optionStart) / optionWidth;
                if (option >= 0 && option < visibleOptionCount)
                    Service.gI().joinClan(message.id, (sbyte)(option == 0 ? 1 : 0));
            }
        }

        private void ActivateClanSideAction(int index)
        {
            if (_selectedClanView == ClanViewTreasury || _selectedClanView == ClanViewHistory)
            {
                if (index == 0) OpenClanDialog(ClanInputDepositGold);
                else if (index == 1) OpenClanDialog(ClanInputDepositGem);
                else if (index == 2)
                {
                    _selectedClanView = ClanViewHistory;
                    _selectedClanFunction = 2;
                    ConfigureClanRects();
                    _rightScrollAdapter?.Reset();
                    Service.gI().clanTreasuryLedger(0L);
                    ConfigureScrollAdapters();
                    SoundMn.gI().panelClick();
                }
                else if (index == 3) OpenClanTreasury();
                return;
            }
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            if (me.role == 0)
            {
                if (index == 0) OpenClanDialog(ClanInputSlogan);
                else if (index == 1) OpenClanIconPicker();
                else if (index == 2) Service.gI().leaveClan();
            }
            else if (index == 0) Service.gI().leaveClan();
        }

        private void OpenClanIconPicker()
        {
            Panel panel = GameCanvas.panel;
            Close();
            if (panel != null)
            {
                panel.setTypeMain();
                panel.currentTabIndex = 3;
                panel.setTabClans();
                panel.show();
            }
            Service.gI().getClan(3, -1, null);
        }

        private static bool HandleClanTextBackspace(TField field, bool focused)
        {
            if (!Main.isPC || !focused || field == null || !GameCanvas.keyPressed[14]) return false;
            GameCanvas.keyPressed[14] = false;
            field.keyPressed(-8);
            return true;
        }

        private void SendClanChat()
        {
            string text = _clanChatField != null ? _clanChatField.getText().Trim() : string.Empty;
            if (text.Length == 0) return;
            Service.gI().clanMessage(0, text, -1);
            _clanChatField.setText(string.Empty);
            _clanChatFocused = false;
            _clanChatField.setFocus(false);
            SoundMn.gI().panelClick();
        }

        private void OpenClanDialog(int mode)
        {
            _clanDialogMode = mode;
            _clanDialogFocus = 0;
            _clanDialogField = new TField();
            _clanDialogField.name = mode == ClanInputSlogan ? "Nhập khẩu hiệu ..." : "Nhập tại đây ...";
            _clanDialogField.setIputType(mode == ClanInputSlogan ? TField.INPUT_TYPE_ANY : TField.INPUT_TYPE_NUMERIC);
            _clanDialogField.setMaxTextLenght(mode == ClanInputSlogan ? 80 : 18);
            ConfigureClanRects();
            _clanDialogField.setFocusWithKb(true);
            GameCanvas.keyAsciiPress = 0;
            GameCanvas.clearKeyPressed();
            SoundMn.gI().panelClick();
        }

        private void CloseClanDialog()
        {
            _clanDialogMode = ClanInputNone;
            if (_clanDialogField != null) _clanDialogField.setFocus(false);
            _clanDialogField = null;
            _clanDialogFocus = 0;
        }

        private void HandleClanDialogInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                if (GameCanvas.isPointer(_clanDialogCloseRect.X, _clanDialogCloseRect.Y,
                    _clanDialogCloseRect.Width, _clanDialogCloseRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    CloseClanDialog();
                    return;
                }
                if (_clanDialogField != null && GameCanvas.isPointer(_clanDialogField.x, _clanDialogField.y,
                    _clanDialogField.width, _clanDialogField.height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanDialogFocus = 0;
                    _clanDialogField.setFocusWithKb(true);
                    return;
                }
                if (GameCanvas.isPointer(_clanDialogSubmitRect.X, _clanDialogSubmitRect.Y,
                    _clanDialogSubmitRect.Width, _clanDialogSubmitRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanDialogFocus = 1;
                    SubmitClanDialog();
                    return;
                }
                GameCanvas.clearAllPointerEvent();
            }
            if (GameCanvas.keyAsciiPress == 27 || GameCanvas.keyPressed[12] || GameCanvas.keyPressed[13])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                CloseClanDialog();
                return;
            }
            if (HandleClanTextBackspace(_clanDialogField, _clanDialogFocus == 0)) return;
            if (GameCanvas.keyPressed[22] || GameCanvas.keyPressed[8])
            {
                ConsumeDirectionKeys(22, 8);
                _clanDialogFocus = 1;
                _clanDialogField?.setFocus(false);
                return;
            }
            if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[2])
            {
                ConsumeDirectionKeys(21, 2);
                _clanDialogFocus = 0;
                _clanDialogField?.setFocus(true);
                return;
            }
            if (GameCanvas.keyPressed[Main.isPC ? 25 : 5])
            {
                GameCanvas.clearKeyPressed();
                if (_clanDialogFocus == 0)
                {
                    _clanDialogFocus = 1;
                    _clanDialogField?.setFocus(false);
                }
                else SubmitClanDialog();
                return;
            }
            int ascii = GameCanvas.keyAsciiPress;
            if (_clanDialogFocus == 0 && ascii != 0 && ascii != 10 && ascii != 13)
            {
                _clanDialogField?.keyPressed(ascii);
                GameCanvas.keyAsciiPress = 0;
            }
        }

        private void SubmitClanDialog()
        {
            string value = _clanDialogField != null ? _clanDialogField.getText().Trim() : string.Empty;
            if (_clanDialogMode == ClanInputSlogan)
            {
                if (value.Length == 0)
                {
                    GameScr.info1.addInfo(mResources.clan_slogan_blank, 0);
                    return;
                }
                Service.gI().getClan(4, (sbyte)Char.myCharz().clan.imgID, value);
                CloseClanDialog();
                return;
            }
            if (!long.TryParse(value, out long amount) || amount <= 0L)
            {
                GameScr.info1.addInfo("Số lượng đóng góp không hợp lệ.", 0);
                return;
            }
            sbyte currency = _clanDialogMode == ClanInputDepositGold
                ? ClanTreasury.CURRENCY_GOLD : ClanTreasury.CURRENCY_GEM;
            Service.gI().clanTreasuryDeposit(currency, amount,
                DateTime.UtcNow.Ticks + "-custom-clan-" + Char.myCharz().charID);
            CloseClanDialog();
        }

        private void PaintClanDialog(mGraphics g)
        {
            int accent = GetClanDialogAccent();
            g.setColor(0x17110D, 0.48f);
            g.fillRect(0, 0, GameCanvas.w, GameCanvas.h + 1);
            g.setColor(0x3B2C22);
            g.fillRect(_clanDialogRect.X + 3, _clanDialogRect.Y + 4,
                _clanDialogRect.Width, _clanDialogRect.Height, 9);
            g.setColor(0xEFE4D5);
            g.fillRect(_clanDialogRect.X, _clanDialogRect.Y,
                _clanDialogRect.Width, _clanDialogRect.Height, 9);
            g.setColor(accent);
            g.fillRect(_clanDialogRect.X, _clanDialogRect.Y,
                _clanDialogRect.Width, 29, 9);
            g.setColor(0xFFF2B8);
            g.fillRect(_clanDialogRect.X + 5, _clanDialogRect.Y + 2,
                _clanDialogRect.Width - 10, 1);

            PaintClanDialogIcon(g, accent);
            string title = _clanDialogMode == ClanInputSlogan ? "Đổi khẩu hiệu bang"
                : (_clanDialogMode == ClanInputDepositGold ? "Góp vàng vào bang" : "Góp ngọc vào bang");
            string hint = _clanDialogMode == ClanInputSlogan ? "Tối đa 80 ký tự"
                : "Nhập số lượng muốn chuyển vào ngân quỹ bang";
            mFont.tahoma_7b_white.drawString(g, title, _clanDialogRect.X + 37,
                _clanDialogRect.Y + 8, mFont.LEFT);
            mFont.tahoma_7_grey.drawString(g, hint, _clanDialogRect.X + 25,
                _clanDialogRect.Y + 34, mFont.LEFT);

            g.setColor(0xB6A58D);
            g.fillRect(_clanDialogField.x - 3, _clanDialogField.y - 2,
                _clanDialogField.width + 6, _clanDialogField.height + 4, 6);
            g.setColor(0xFBF8F3);
            g.fillRect(_clanDialogField.x - 2, _clanDialogField.y - 1,
                _clanDialogField.width + 4, _clanDialogField.height + 2, 5);

            g.setColor(0x8C2D25);
            g.fillRect(_clanDialogCloseRect.X, _clanDialogCloseRect.Y,
                _clanDialogCloseRect.Width, _clanDialogCloseRect.Height, 5);
            g.setColor(0xFFD1C7);
            g.drawLine(_clanDialogCloseRect.X + 5, _clanDialogCloseRect.Y + 5,
                _clanDialogCloseRect.X + 12, _clanDialogCloseRect.Y + 12);
            g.drawLine(_clanDialogCloseRect.X + 12, _clanDialogCloseRect.Y + 5,
                _clanDialogCloseRect.X + 5, _clanDialogCloseRect.Y + 12);
            _clanDialogField?.paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            PaintClanDialogActionButton(g, accent,
                _clanDialogMode == ClanInputSlogan ? "Lưu khẩu hiệu" : "Xác nhận góp");
        }

        private int GetClanDialogAccent()
        {
            if (_clanDialogMode == ClanInputDepositGem) return 0x18A66A;
            if (_clanDialogMode == ClanInputSlogan) return 0xD44738;
            return 0xE99A00;
        }

        private void PaintClanDialogIcon(mGraphics g, int accent)
        {
            int centerX = _clanDialogRect.X + 20;
            int centerY = _clanDialogRect.Y + 14;
            Image icon = _clanDialogMode == ClanInputDepositGold ? Panel.imgXu
                : _clanDialogMode == ClanInputDepositGem ? Panel.imgLuong : null;
            if (icon != null)
            {
                g.drawImage(icon, centerX, centerY, mGraphics.HCENTER | mGraphics.VCENTER);
                return;
            }
            g.setColor(0xFFF4D2);
            g.fillRect(centerX - 7, centerY - 5, 14, 10, 3);
            g.fillRect(centerX - 4, centerY + 4, 4, 3, 1);
            g.setColor(accent);
            g.fillRect(centerX - 4, centerY - 2, 8, 1);
            g.fillRect(centerX - 4, centerY + 1, 6, 1);
        }

        private void PaintClanDialogActionButton(mGraphics g, int accent, string text)
        {
            bool focused = _clanDialogFocus == 1;
            bool hovered = Main.isPC && _clanDialogSubmitRect.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse);
            g.setColor(0x765018);
            g.fillRect(_clanDialogSubmitRect.X + 1, _clanDialogSubmitRect.Y + 2,
                _clanDialogSubmitRect.Width, _clanDialogSubmitRect.Height - 1, 5);
            g.setColor(focused || hovered ? LightenClanDialogAccent(accent) : accent);
            g.fillRect(_clanDialogSubmitRect.X, _clanDialogSubmitRect.Y,
                _clanDialogSubmitRect.Width, _clanDialogSubmitRect.Height - 2, 5);
            g.setColor(0xFFF2B8);
            g.fillRect(_clanDialogSubmitRect.X + 3, _clanDialogSubmitRect.Y + 1,
                _clanDialogSubmitRect.Width - 6, 1);
            mFont.tahoma_7b_white.drawString(g, text,
                _clanDialogSubmitRect.X + _clanDialogSubmitRect.Width / 2,
                _clanDialogSubmitRect.Y + 7, mFont.CENTER);
        }

        private static int LightenClanDialogAccent(int color)
        {
            int red = System.Math.Min(255, ((color >> 16) & 0xFF) + 24);
            int green = System.Math.Min(255, ((color >> 8) & 0xFF) + 24);
            int blue = System.Math.Min(255, (color & 0xFF) + 24);
            return red << 16 | green << 8 | blue;
        }

        private void MoveClanHorizontalFocus(int direction)
        {
            if (_clanFocusArea == ClanFocusFunctions)
            {
                int column = _selectedClanFunction % 2;
                if (direction < 0 && column == 0)
                {
                    _keyboardFocus = KeyboardFocusMainTabs;
                    SoundMn.gI().panelClick();
                }
                else if (direction > 0 && column == 1)
                {
                    _clanFocusArea = ClanFocusContent;
                    if (_selectedClanView == ClanViewTreasury && _selectedClanStorageSlot < 0)
                        _selectedClanStorageSlot = 0;
                    SoundMn.gI().panelClick();
                }
                else
                {
                    _selectedClanFunction = System.Math.Max(0,
                        System.Math.Min(_clanFunctionRects.Length - 1, _selectedClanFunction + direction));
                    SoundMn.gI().panelClick();
                }
                return;
            }
            if (_clanFocusArea == ClanFocusContent)
            {
                if (_selectedClanView == ClanViewTreasury)
                {
                    Char me = Char.myCharz();
                    int slotCount = me != null && me.arrItemBox != null ? me.arrItemBox.Length : 0;
                    if (slotCount > 0)
                    {
                        if (_selectedClanStorageSlot < 0) _selectedClanStorageSlot = 0;
                        int column = _selectedClanStorageSlot % ClanStorageColumns;
                        int target = _selectedClanStorageSlot + direction;
                        if (direction < 0 && column == 0) _clanFocusArea = ClanFocusFunctions;
                        else if (direction > 0 && (column == ClanStorageColumns - 1 || target >= slotCount))
                        {
                            if (GetClanSideActionCount() > 0) _clanFocusArea = ClanFocusSideActions;
                        }
                        else
                        {
                            _selectedClanStorageSlot = target;
                            _selectedClanRow = target / ClanStorageColumns;
                            _rightScrollAdapter?.ScrollToIndex(_selectedClanRow);
                        }
                    }
                    else if (direction < 0) _clanFocusArea = ClanFocusFunctions;
                    else if (GetClanSideActionCount() > 0) _clanFocusArea = ClanFocusSideActions;
                }
                else if (direction < 0) _clanFocusArea = ClanFocusFunctions;
                else if (GetClanSideActionCount() > 0) _clanFocusArea = ClanFocusSideActions;
                SoundMn.gI().panelClick();
                return;
            }
            if (_clanFocusArea == ClanFocusSideActions && direction < 0)
            {
                _clanFocusArea = ClanFocusContent;
                SoundMn.gI().panelClick();
            }
        }

        private void MoveClanVerticalSelection(int direction)
        {
            if (_clanFocusArea == ClanFocusFunctions)
            {
                int target = _selectedClanFunction + direction * 2;
                _selectedClanFunction = System.Math.Max(0,
                    System.Math.Min(_clanFunctionRects.Length - 1, target));
            }
            else if (_clanFocusArea == ClanFocusSideActions)
            {
                int count = GetClanSideActionCount();
                if (count > 0) _selectedClanSideAction = (_selectedClanSideAction + direction + count) % count;
            }
            else if (_clanFocusArea == ClanFocusContent)
            {
                if (_selectedClanView == ClanViewTreasury)
                {
                    Char me = Char.myCharz();
                    int slotCount = me != null && me.arrItemBox != null ? me.arrItemBox.Length : 0;
                    if (slotCount > 0)
                    {
                        if (_selectedClanStorageSlot < 0) _selectedClanStorageSlot = 0;
                        int target = _selectedClanStorageSlot + direction * ClanStorageColumns;
                        if (target >= 0 && target < slotCount) _selectedClanStorageSlot = target;
                        _selectedClanRow = _selectedClanStorageSlot / ClanStorageColumns;
                        _rightScrollAdapter?.ScrollToIndex(_selectedClanRow);
                    }
                }
                else
                {
                    int count = GetClanContentItemCount();
                    if (count > 0)
                    {
                        _selectedClanRow = (_selectedClanRow + direction + count) % count;
                        _rightScrollAdapter?.ScrollToIndex(_selectedClanRow);
                    }
                }
            }
            SoundMn.gI().panelClick();
        }

        private void HandleClanConfirm()
        {
            if (_clanChatFocused)
            {
                SendClanChat();
                return;
            }
            if (_clanFocusArea == ClanFocusFunctions) ActivateClanFunction(_selectedClanFunction);
            else if (_clanFocusArea == ClanFocusSideActions) ActivateClanSideAction(_selectedClanSideAction);
            else if (_clanFocusArea == ClanFocusContent)
                ActivateClanContent(_selectedClanView == ClanViewTreasury
                    ? _selectedClanStorageSlot : _selectedClanRow);
        }

    }
}
