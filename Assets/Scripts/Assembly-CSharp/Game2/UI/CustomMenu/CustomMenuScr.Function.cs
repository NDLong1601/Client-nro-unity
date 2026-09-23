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
        public static void OnWorldChat(string text)
        {
            if (string.IsNullOrEmpty(text) || _instance == null) return;
            if (_instance._selectedMainTab == 4 && _instance._functionView == FunctionViewWorldChat)
            {
                _instance.ConfigureScrollAdapters();
            }
        }

        public static bool TryConsumeFunctionFlagOpen()
        {
            if (!_isOpen || _instance == null || _instance._selectedMainTab != 4) return false;
            _instance._selectedFunction = FunctionChangeFlag;
            _instance._functionView = FunctionViewFlags;
            _instance._functionFocusArea = FunctionFocusContent;
            _instance._selectedFunctionRow = 0;
            _instance.ConfigureFunctionRects();
            _instance.ConfigureScrollAdapters();
            return true;
        }

        private UiRect GetFunctionScrollableViewport()
        {
            return _functionView == FunctionViewWorldChat ? _functionWorldChatListRect : _rightBodyRect;
        }

        private int GetFunctionScrollableItemCount()
        {
            if (_functionView == FunctionViewNotifications) return Panel.vGameInfo.size();
            if (_functionView == FunctionViewZones) return GameScr.gI().zones != null ? (GameScr.gI().zones.Length + 3) / 4 : 0;
            if (_functionView == FunctionViewFlags) return GameCanvas.panel != null && GameCanvas.panel.vFlag != null ? GameCanvas.panel.vFlag.size() : 0;
            if (_functionView == FunctionViewActivityDaily) return ActivityScreen.gI().getPanelTierCount(false);
            if (_functionView == FunctionViewActivityWeekly) return ActivityScreen.gI().getPanelTierCount(true);
            if (_functionView == FunctionViewActivitySources) return ActivityScreen.gI().getPanelSourceCount();
            if (_functionView == FunctionViewWorldChat) return GameScr.vChatVip.size();
            if (_functionView == FunctionViewHistory) return GameCanvas.panel != null && GameCanvas.panel.getPKHistoryEntries() != null ? GameCanvas.panel.getPKHistoryEntries().size() : 0;
            return 0;
        }

        private int GetFunctionScrollableRowHeight()
        {
            if (_functionView == FunctionViewNotifications) return 36;
            if (_functionView == FunctionViewZones) return 29;
            if (_functionView == FunctionViewFlags) return 39;
            if (_functionView == FunctionViewActivityDaily || _functionView == FunctionViewActivityWeekly
                || _functionView == FunctionViewActivitySources) return 45;
            if (_functionView == FunctionViewWorldChat) return 25;
            if (_functionView == FunctionViewHistory) return 36;
            return 1;
        }

        private int GetFunctionNotificationTotalHeight()
        {
            return BuildFunctionNotificationLayout(out _).ContentHeight;
        }

        private UiAccordionLayout BuildFunctionNotificationLayout(out string[] expandedLines)
        {
            expandedLines = new string[0];
            int count = Panel.vGameInfo != null ? Panel.vGameInfo.size() : 0;
            int expandedIndex = _selectedFunctionRow;
            if (expandedIndex >= 0 && expandedIndex < count)
            {
                GameInfo info = Panel.vGameInfo.elementAt(expandedIndex) as GameInfo;
                if (info != null)
                {
                    expandedLines = mFont.tahoma_7b_dark.splitFontArray(info.content ?? string.Empty,
                        UiAccordionLayout.GetBodyTextWidth(_rightBodyRect)) ?? new string[0];
                }
                else expandedIndex = -1;
            }
            return new UiAccordionLayout(_rightBodyRect, count, expandedIndex, expandedLines.Length);
        }

        private bool HandleFunctionPointerInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _functionMenuRects.Length; i++)
                {
                    UiRect rect = _functionMenuRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.clearAllPointerEvent();
                    _functionFocusArea = FunctionFocusMenu;
                    _selectedFunction = i;
                    ActivateFunction(i);
                    return true;
                }
                if (_functionView == FunctionViewNotifications)
                {
                    int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
                    UiAccordionLayout layout = BuildFunctionNotificationLayout(out _);
                    int index = layout.HitTestHeader(GameCanvas.px, GameCanvas.py, scrollY);
                    if (index >= 0 && Panel.vGameInfo.elementAt(index) is GameInfo)
                    {
                        GameCanvas.clearAllPointerEvent();
                        _functionFocusArea = FunctionFocusContent;
                        SelectFunctionNotification(index);
                        return true;
                    }
                }
                else if (_functionView == FunctionViewZones)
                {
                    int count = GameScr.gI().zones != null ? GameScr.gI().zones.Length : 0;
                    int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
                    UiGridLayout grid = new UiGridLayout(_rightBodyRect, 4, 3, 3, 29, 26);
                    int index = grid.HitTestCell(GameCanvas.px, GameCanvas.py, count, scrollY);
                    if (index >= 0)
                    {
                        GameCanvas.clearAllPointerEvent();
                        _functionFocusArea = FunctionFocusContent;
                        SelectFunctionZone(index);
                        return true;
                    }
                }
                else if (_functionView == FunctionViewFlags)
                {
                    int count = GameCanvas.panel != null && GameCanvas.panel.vFlag != null ? GameCanvas.panel.vFlag.size() : 0;
                    int rowH = 36;
                    int gap = 3;
                    int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
                    for (int i = 0; i < count; i++)
                    {
                        int y = _rightBodyRect.Y + 3 + i * (rowH + gap) - scrollY;
                        if (GameCanvas.isPointer(_rightBodyRect.X + 3, y, _rightBodyRect.Width - 6, rowH))
                        {
                            GameCanvas.clearAllPointerEvent();
                            _functionFocusArea = FunctionFocusContent;
                            SelectFunctionFlag(i);
                            return true;
                        }
                    }
                }
                else if (_functionView == FunctionViewActivityOverview)
                {
                    int cardW = _rightBodyRect.Width - 16;
                    int cardH = 34;
                    int startY = _rightBodyRect.Y + 12;
                    int gap = 8;
                    for (int i = 0; i < 3; i++)
                    {
                        int y = startY + i * (cardH + gap);
                        if (GameCanvas.isPointer(_rightBodyRect.X + 8, y, cardW, cardH))
                        {
                            GameCanvas.clearAllPointerEvent();
                            _functionFocusArea = FunctionFocusContent;
                            _selectedFunctionRow = i;
                            ActivateFunctionActivityAction(i);
                            return true;
                        }
                    }
                }
                else if (_functionView == FunctionViewDisciple)
                {
                    if (Char.myCharz() != null && Char.myCharz().havePet && Char.myPetz() != null)
                    {
                        int infoH = 88;
                        int btnW = (_rightBodyRect.Width - 12 - 8) / 3;
                        int btnH = 22;
                        int startY = _rightBodyRect.Y + 4 + infoH + 6;
                        for (int i = 0; i < 5; i++)
                        {
                            int col = i % 3;
                            int row = i / 3;
                            int bx = _rightBodyRect.X + 4 + col * (btnW + 4);
                            int by = startY + row * (btnH + 4);
                            if (GameCanvas.isPointer(bx, by, btnW, btnH))
                            {
                                GameCanvas.clearAllPointerEvent();
                                _functionFocusArea = FunctionFocusContent;
                                if (i == 4)
                                {
                                    GameCanvas.startYesNoDlg(mResources.sure_fusion, new Command(mResources.YES, 888351), new Command(mResources.NO, 2001));
                                }
                                else
                                {
                                    Service.gI().petStatus((sbyte)i);
                                    Char.myPetz().petStatus = (sbyte)i;
                                }
                                SoundMn.gI().panelClick();
                                return true;
                            }
                        }
                    }
                }
                else if (_functionView == FunctionViewAccount)
                {
                    int cardW = _rightBodyRect.Width - 16;
                    int cardH = 26;
                    int startY = _rightBodyRect.Y + 6;
                    for (int i = 0; i < 5; i++)
                    {
                        int y = startY + i * (cardH + 4);
                        if (GameCanvas.isPointer(_rightBodyRect.X + 8, y, cardW, cardH))
                        {
                            GameCanvas.clearAllPointerEvent();
                            _functionFocusArea = FunctionFocusContent;
                            if (i == 0) { GameCanvas.panel.setTypeAccount(); GameCanvas.panel.show(); }
                            else if (i == 1) { GameCanvas.panel.setTypeFriend(); GameCanvas.panel.show(); }
                            else if (i == 2) { GameCanvas.panel.setTypeEnemy(); GameCanvas.panel.show(); }
                            else if (i == 3) { GameCanvas.panel.setTypeMessage(); GameCanvas.panel.show(); }
                            else if (i == 4) { GameCanvas.panel.setTypeAccount(); GameCanvas.panel.show(); }
                            SoundMn.gI().panelClick();
                            return true;
                        }
                    }
                }
                else if (_functionView == FunctionViewSettings)
                {
                    int cardW = _rightBodyRect.Width - 16;
                    int cardH = 28;
                    int startY = _rightBodyRect.Y + 10;
                    for (int i = 0; i < 4; i++)
                    {
                        int y = startY + i * (cardH + 6);
                        if (GameCanvas.isPointer(_rightBodyRect.X + 8, y, cardW, cardH))
                        {
                            GameCanvas.clearAllPointerEvent();
                            _functionFocusArea = FunctionFocusContent;
                            if (i == 0) GameCanvas.isPlaySound = !GameCanvas.isPlaySound;
                            else if (i == 1) Panel.graphics = (Panel.graphics == 0 ? 1 : 0);
                            else if (i == 2) SoundMn.gI().analogToolOption();
                            else if (i == 3) mGraphics.zoomLevel = (mGraphics.zoomLevel <= 1 ? 2 : 1);
                            SoundMn.gI().panelClick();
                            return true;
                        }
                    }
                }
                else if (_functionView == FunctionViewChangeAccount)
                {
                    UiRect box = new UiRect(_rightBodyRect.X + 12, _rightBodyRect.Y + 24, _rightBodyRect.Width - 24, 80);
                    UiRect okBtn = new UiRect(box.X + 10, box.Bottom - 30, (box.Width - 28) / 2, 22);
                    UiRect cancelBtn = new UiRect(okBtn.Right + 8, box.Bottom - 30, okBtn.Width, 22);
                    if (GameCanvas.isPointer(okBtn.X, okBtn.Y, okBtn.Width, okBtn.Height))
                    {
                        GameCanvas.clearAllPointerEvent();
                        GameCanvas.loginScr.backToRegister();
                        SoundMn.gI().panelClick();
                        return true;
                    }
                    if (GameCanvas.isPointer(cancelBtn.X, cancelBtn.Y, cancelBtn.Width, cancelBtn.Height))
                    {
                        GameCanvas.clearAllPointerEvent();
                        _functionView = FunctionViewDefault;
                        SoundMn.gI().panelClick();
                        return true;
                    }
                }
                else if (_functionView == FunctionViewWorldChat)
                {
                    if (GameCanvas.isPointer(_functionWorldChatSendRect.X, _functionWorldChatSendRect.Y,
                        _functionWorldChatSendRect.Width, _functionWorldChatSendRect.Height))
                    {
                        GameCanvas.clearAllPointerEvent();
                        SendFunctionWorldChat();
                        return true;
                    }
                    if (_functionWorldChatField != null && GameCanvas.isPointer(_functionWorldChatField.x,
                        _functionWorldChatField.y, _functionWorldChatField.width, _functionWorldChatField.height))
                    {
                        GameCanvas.clearAllPointerEvent();
                        _functionFocusArea = FunctionFocusContent;
                        _functionWorldChatFocused = true;
                        _functionWorldChatField.setFocusWithKb(true);
                        return true;
                    }
                }
                else if (_functionView == FunctionViewToggles)
                {
                    for (int i = 0; i < _functionToggleRects.Length; i++)
                    {
                        UiRect rect = _functionToggleRects[i];
                        if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                        GameCanvas.clearAllPointerEvent();
                        _functionFocusArea = FunctionFocusContent;
                        _selectedFunctionRow = i;
                        ToggleFunctionSetting(i);
                        return true;
                    }
                }
            }

            if (_functionView == FunctionViewNotifications || _functionView == FunctionViewZones || _functionView == FunctionViewFlags
                || _functionView == FunctionViewActivityDaily || _functionView == FunctionViewActivityWeekly
                || _functionView == FunctionViewActivitySources || _functionView == FunctionViewWorldChat || _functionView == FunctionViewHistory)
            {
                if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out int row))
                {
                    _functionFocusArea = FunctionFocusContent;
                    // Notification scroll rows represent pixels, not notification indices.
                    // Header selection is handled by the layout hit test above.
                    if (row >= 0 && _functionView != FunctionViewNotifications)
                    {
                        _selectedFunctionRow = row;
                        if (_functionView == FunctionViewZones) SelectFunctionZone(row);
                        else if (_functionView == FunctionViewFlags) SelectFunctionFlag(row);
                        else if (_functionView != FunctionViewWorldChat && _functionView != FunctionViewHistory) ActivateFunctionActivityAction(row);
                    }
                    return true;
                }
            }

            if (HandleFunctionTextBackspace()) return true;
            int ascii = GameCanvas.keyAsciiPress;
            if (_functionWorldChatFocused && ascii != 0 && ascii != 10 && ascii != 13)
            {
                _functionWorldChatField?.keyPressed(ascii);
                GameCanvas.keyAsciiPress = 0;
                return true;
            }
            return false;
        }

        private void ActivateFunction(int index)
        {
            _selectedFunction = System.Math.Max(0, System.Math.Min(FunctionNames.Length - 1, index));
            _selectedFunctionRow = 0;
            _rightScrollAdapter?.Reset();
            _functionWorldChatFocused = false;
            _functionWorldChatField?.setFocus(false);
            if (index == FunctionNotification)
            {
                _functionView = FunctionViewNotifications;
                _selectedFunctionRow = -1;
            }
            else if (index == FunctionChangeZone)
            {
                _functionView = FunctionViewZones;
                ModFunc.GI().userOpenZones = false;
                Service.gI().openUIZone();
            }
            else if (index == FunctionDisciple)
            {
                _functionView = FunctionViewDisciple;
                Service.gI().petInfo();
            }
            else if (index == FunctionChangeFlag)
            {
                _functionView = FunctionViewFlags;
                Service.gI().getFlag(0, -1);
            }
            else if (index == FunctionActivity)
            {
                _functionView = FunctionViewActivityOverview;
                ActivityScreen.gI().requestPanelData();
            }
            else if (index == FunctionCollection)
            {
                _functionView = FunctionViewCollection;
                Service.gI().SendRada(0, -1);
                CostumeCollectionScr.gI().requestOpen();
            }
            else if (index == FunctionWorldChat) _functionView = FunctionViewWorldChat;
            else if (index == FunctionMod) _functionView = FunctionViewToggles;
            else if (index == FunctionAccount) _functionView = FunctionViewAccount;
            else if (index == FunctionSettings) _functionView = FunctionViewSettings;
            else if (index == FunctionHistory)
            {
                _functionView = FunctionViewHistory;
                Service.gI().requestPKHistory();
            }
            else if (index == FunctionChangeAccount) _functionView = FunctionViewChangeAccount;
            else _functionView = FunctionViewDefault;
            ConfigureFunctionRects();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void SelectFunctionNotification(int index)
        {
            if (index < 0 || index >= Panel.vGameInfo.size()) return;
            if (_selectedFunctionRow == index)
            {
                _selectedFunctionRow = -1;
            }
            else
            {
                GameInfo info = Panel.vGameInfo.elementAt(index) as GameInfo;
                if (info == null) return;
                _selectedFunctionRow = index;
                info.hasRead = true;
                Rms.saveRMSInt(info.id + string.Empty, 1);
            }
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void SelectFunctionZone(int index)
        {
            GameScr game = GameScr.gI();
            if (game.zones == null || index < 0 || index >= game.zones.Length || game.zones[index] < 0) return;
            _selectedFunctionRow = index;
            Service.gI().requestChangeZone(game.zones[index], -1);
            Service.gI().openUIZone();
            SoundMn.gI().panelClick();
        }

        private void SelectFunctionFlag(int index)
        {
            int count = GameCanvas.panel != null && GameCanvas.panel.vFlag != null ? GameCanvas.panel.vFlag.size() : 0;
            if (index < 0 || index >= count) return;
            _selectedFunctionRow = index;
            Service.gI().getFlag(1, (sbyte)index);
            SoundMn.gI().panelClick();
        }

        private void ActivateFunctionActivityAction(int index)
        {
            if (_functionView == FunctionViewActivityOverview)
            {
                _functionView = index == 0 ? FunctionViewActivityDaily
                    : index == 1 ? FunctionViewActivityWeekly : FunctionViewActivitySources;
                _selectedFunctionRow = 0;
                ConfigureScrollAdapters();
                SoundMn.gI().panelClick();
                return;
            }
            if (_functionView == FunctionViewActivityDaily)
                ActivityScreen.gI().claimPanelTier(false, index);
            else if (_functionView == FunctionViewActivityWeekly)
                ActivityScreen.gI().claimPanelTier(true, index);
        }

        private void SendFunctionWorldChat()
        {
            string text = _functionWorldChatField != null ? _functionWorldChatField.getText().Trim() : string.Empty;
            if (text.Length == 0) return;
            if (Char.myCharz() == null || Char.myCharz().checkLuong() < 5)
            {
                GameCanvas.startOKDlg(mResources.not_enough_luong_world_channel);
                return;
            }
            Service.gI().chatGlobal(text);
            _functionWorldChatField.setText(string.Empty);
            _functionWorldChatFocused = false;
            _functionWorldChatField.setFocus(false);
            SoundMn.gI().panelClick();
        }

        private bool HandleFunctionTextBackspace()
        {
            if (!Main.isPC || !_functionWorldChatFocused || _functionWorldChatField == null || !GameCanvas.keyPressed[14]) return false;
            GameCanvas.keyPressed[14] = false;
            _functionWorldChatField.keyPressed(-8);
            return true;
        }

        private void ToggleFunctionSetting(int index)
        {
            if (index == 0)
            {
                ModFunc.GI().isHighFps = !ModFunc.GI().isHighFps;
                ModFunc.GI().ChangeFPSTarget();
            }
            else if (index == 1) ModFunc.GI().showCharsInMap = !ModFunc.GI().showCharsInMap;
            else if (index == 2) ModFunc.GI().showInfoMe = !ModFunc.GI().showInfoMe;
            else if (index == 3)
            {
                ModFunc.GI().isAutoPhaLe = !ModFunc.GI().isAutoPhaLe;
                if (ModFunc.GI().isAutoPhaLe) new System.Threading.Thread(ModFunc.GI().AutoPhaLe).Start();
            }
            else if (ModFunc.isAutoLogin)
            {
                ModFunc.isAutoLogin = false;
                ModFunc.autoLogin = null;
            }
            else
            {
                ModFunc.isAutoLogin = true;
                ModFunc.autoLogin = new AutoLogin
                {
                    accAutoLogin = GameCanvas.loginScr != null ? GameCanvas.loginScr.tfUser.getText() : string.Empty
                };
            }
            GameScr.info1.addInfo("Đã " + (IsFunctionSettingEnabled(index) ? "bật " : "tắt ") + GetFunctionToggleLabel(index), 0);
            SoundMn.gI().panelClick();
        }

        private void MoveFunctionHorizontalFocus(int direction)
        {
            if (_functionFocusArea == FunctionFocusMenu)
            {
                if (direction < 0) _keyboardFocus = KeyboardFocusMainTabs;
                else _functionFocusArea = FunctionFocusContent;
                SoundMn.gI().panelClick();
                return;
            }
            if (direction < 0)
            {
                _functionWorldChatFocused = false;
                _functionWorldChatField?.setFocus(false);
                _functionFocusArea = FunctionFocusMenu;
                SoundMn.gI().panelClick();
            }
        }

        private void MoveFunctionVerticalSelection(int direction)
        {
            if (_functionFocusArea == FunctionFocusMenu)
            {
                if (_selectedFunction < 0)
                {
                    _selectedFunction = direction > 0 ? 0 : FunctionNames.Length - 2;
                    SoundMn.gI().panelClick();
                    return;
                }
                _selectedFunction = (_selectedFunction + direction * 2 + FunctionNames.Length) % FunctionNames.Length;
                SoundMn.gI().panelClick();
                return;
            }
            int count = _functionView == FunctionViewActivityOverview ? 3
                : _functionView == FunctionViewZones ? (GameScr.gI().zones != null ? GameScr.gI().zones.Length : 0)
                : _functionView == FunctionViewToggles ? _functionToggleRects.Length
                : _functionView == FunctionViewDisciple ? 5
                : _functionView == FunctionViewAccount ? 5
                : _functionView == FunctionViewSettings ? 4
                : _functionView == FunctionViewChangeAccount ? 2
                : GetFunctionScrollableItemCount();
            if (count <= 0) return;
            int step = _functionView == FunctionViewZones ? 4 : 1;
            _selectedFunctionRow = (_selectedFunctionRow + direction * step + count * 2) % count;
            if (_functionView == FunctionViewNotifications)
            {
                ConfigureScrollAdapters();
                int headerOffset = BuildFunctionNotificationLayout(out _).GetHeaderOffset(_selectedFunctionRow);
                _rightScrollAdapter?.ScrollToIndex(System.Math.Max(0, headerOffset));
            }
            else _rightScrollAdapter?.ScrollToIndex(_selectedFunctionRow);
            SoundMn.gI().panelClick();
        }

        private void HandleFunctionConfirm()
        {
            if (_functionFocusArea == FunctionFocusMenu)
            {
                if (_selectedFunction < 0)
                {
                    _selectedFunction = FunctionNotification;
                    ActivateFunction(_selectedFunction);
                    return;
                }
                MoveFunctionHorizontalFocus(1);
                return;
            }
            if (_functionView == FunctionViewNotifications)
                SelectFunctionNotification(_selectedFunctionRow < 0 ? 0 : _selectedFunctionRow);
            else if (_functionView == FunctionViewZones) SelectFunctionZone(_selectedFunctionRow);
            else if (_functionView == FunctionViewFlags) SelectFunctionFlag(_selectedFunctionRow);
            else if (_functionView == FunctionViewActivityOverview || _functionView == FunctionViewActivityDaily
                || _functionView == FunctionViewActivityWeekly || _functionView == FunctionViewActivitySources)
                ActivateFunctionActivityAction(_selectedFunctionRow);
            else if (_functionView == FunctionViewToggles) ToggleFunctionSetting(_selectedFunctionRow);
            else if (_functionView == FunctionViewDisciple)
            {
                if (Char.myCharz() != null && Char.myCharz().havePet && Char.myPetz() != null && _selectedFunctionRow >= 0 && _selectedFunctionRow < 5)
                {
                    if (_selectedFunctionRow == 4) GameCanvas.startYesNoDlg(mResources.sure_fusion, new Command(mResources.YES, 888351), new Command(mResources.NO, 2001));
                    else { Service.gI().petStatus((sbyte)_selectedFunctionRow); Char.myPetz().petStatus = (sbyte)_selectedFunctionRow; }
                    SoundMn.gI().panelClick();
                }
            }
            else if (_functionView == FunctionViewAccount)
            {
                if (_selectedFunctionRow == 0) { GameCanvas.panel.setTypeAccount(); GameCanvas.panel.show(); }
                else if (_selectedFunctionRow == 1) { GameCanvas.panel.setTypeFriend(); GameCanvas.panel.show(); }
                else if (_selectedFunctionRow == 2) { GameCanvas.panel.setTypeEnemy(); GameCanvas.panel.show(); }
                else if (_selectedFunctionRow == 3) { GameCanvas.panel.setTypeMessage(); GameCanvas.panel.show(); }
                else if (_selectedFunctionRow == 4) { GameCanvas.panel.setTypeAccount(); GameCanvas.panel.show(); }
                SoundMn.gI().panelClick();
            }
            else if (_functionView == FunctionViewSettings)
            {
                if (_selectedFunctionRow == 0) GameCanvas.isPlaySound = !GameCanvas.isPlaySound;
                else if (_selectedFunctionRow == 1) Panel.graphics = (Panel.graphics == 0 ? 1 : 0);
                else if (_selectedFunctionRow == 2) SoundMn.gI().analogToolOption();
                else if (_selectedFunctionRow == 3) mGraphics.zoomLevel = (mGraphics.zoomLevel <= 1 ? 2 : 1);
                SoundMn.gI().panelClick();
            }
            else if (_functionView == FunctionViewChangeAccount)
            {
                if (_selectedFunctionRow == 0) GameCanvas.loginScr.backToRegister();
                else _functionView = FunctionViewDefault;
                SoundMn.gI().panelClick();
            }
            else if (_functionView == FunctionViewWorldChat)
            {
                if (_functionWorldChatFocused) SendFunctionWorldChat();
                else
                {
                    _functionWorldChatFocused = true;
                    _functionWorldChatField?.setFocusWithKb(true);
                }
            }
        }
    }
}
