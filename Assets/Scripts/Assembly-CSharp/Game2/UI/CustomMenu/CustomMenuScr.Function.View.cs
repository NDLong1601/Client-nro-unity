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
        private static void PaintFunctionHeader(mGraphics g, UiRect rect, string text)
        {
            UiMenuTheme.PaintHeader(g, rect, text);
        }

        private void PaintFunctionTabContent(mGraphics g)
        {
            PaintFunctionHeader(g, _skillListHeaderRect, "Chức năng");
            PaintFunctionHeader(g, _skillDetailHeaderRect, GetFunctionDetailTitle());
            PaintFunctionMenu(g);

            if (_functionView == FunctionViewNotifications) PaintFunctionNotification(g);
            else if (_functionView == FunctionViewZones) PaintFunctionZones(g);
            else if (_functionView == FunctionViewFlags) PaintFunctionFlags(g);
            else if (_functionView == FunctionViewActivityOverview || _functionView == FunctionViewActivityDaily
                || _functionView == FunctionViewActivityWeekly || _functionView == FunctionViewActivitySources)
                PaintFunctionActivity(g);
            else if (_functionView == FunctionViewWorldChat) PaintFunctionWorldChat(g);
            else if (_functionView == FunctionViewToggles) PaintFunctionToggles(g);
            else if (_functionView == FunctionViewAccount) PaintFunctionAccount(g);
            else if (_functionView == FunctionViewSettings) PaintFunctionSettings(g);
            else if (_functionView == FunctionViewHistory) PaintFunctionHistory(g);
            else if (_functionView == FunctionViewChangeAccount) PaintFunctionChangeAccount(g);
            else PaintFunctionDefault(g);

            PaintFunctionSideCard(g);

            _rightScrollAdapter?.PaintScrollbar(g, GetFunctionScrollableViewport());
        }

        private string GetFunctionDetailTitle()
        {
            if (_functionView == FunctionViewActivityDaily) return "Mốc ngày";
            if (_functionView == FunctionViewActivityWeekly) return "Mốc tuần";
            if (_functionView == FunctionViewActivitySources) return "Nguồn điểm";
            return "Chi tiết chức năng";
        }

        private void PaintFunctionMenu(mGraphics g)
        {
            UiMenuTheme.PaintSurface(g, _leftColRect);
            for (int i = 0; i < FunctionNames.Length; i++)
            {
                bool active = i == _selectedFunction;
                bool focused = _functionFocusArea == FunctionFocusMenu && i == _selectedFunction;
                PaintFunctionMenuButton(g, _functionMenuRects[i], FunctionNames[i], active, focused);
            }
        }

        private static void PaintFunctionMenuButton(mGraphics g, UiRect rect, string text, bool active, bool focused)
        {
            UiMenuTheme.PaintButton(g, rect, text, active, focused);
        }

        private void PaintFunctionSideCard(mGraphics g)
        {
            int cardW = 78;
            int cardX = _frameRect.Right + 4;
            int cardY = _frameRect.Y + 2;
            if (cardX + cardW > GameCanvas.w) return;

            if (_functionView == FunctionViewZones)
            {
                int cardH = 80;
                UiMenuTheme.PaintSurface(g, new UiRect(cardX, cardY, cardW, cardH),
                    0xEAA000, 0x8C560A);

                g.setColor(0x56CE16);
                g.fillRect(cardX + cardW / 2 - 5, cardY + 7, 10, 10, 5);

                string zoneStr = "Khu " + (TileMap.zoneID >= 0 ? TileMap.zoneID.ToString() : "1");
                mFont.tahoma_7b_dark.drawString(g, zoneStr, cardX + cardW / 2, cardY + 23, mFont.CENTER);

                string mapStr = !string.IsNullOrEmpty(TileMap.mapName) ? TileMap.mapName : "Bản đồ";
                mFont.tahoma_7b_dark.drawString(g, TruncateString(mFont.tahoma_7b_dark, mapStr, cardW - 6), cardX + cardW / 2, cardY + 41, mFont.CENTER);

                GameScr game = GameScr.gI();
                string plyStr = "0/15";
                if (game.zones != null && game.numPlayer != null && game.maxPlayer != null)
                {
                    for (int i = 0; i < game.zones.Length; i++)
                    {
                        if (game.zones[i] == TileMap.zoneID && i < game.numPlayer.Length && i < game.maxPlayer.Length)
                        {
                            plyStr = game.numPlayer[i] + "/" + game.maxPlayer[i];
                            break;
                        }
                    }
                }
                mFont.tahoma_7b_dark.drawString(g, plyStr, cardX + cardW / 2, cardY + 59, mFont.CENTER);
            }
            else if (_functionView == FunctionViewFlags)
            {
                int cardH = 64;
                UiMenuTheme.PaintSurface(g, new UiRect(cardX, cardY, cardW, cardH),
                    0xEAA000, 0x8C560A);

                mFont.tahoma_7b_dark.drawString(g, "Đổi cờ", cardX + cardW / 2, cardY + 7, mFont.CENTER);
                g.setColor(0xC88300);
                g.drawLine(cardX + 6, cardY + 22, cardX + cardW - 6, cardY + 22);

                int flagCount = 0;
                if (GameScr.vCharInMap != null)
                {
                    for (int i = 0; i < GameScr.vCharInMap.size(); i++)
                    {
                        Char c = GameScr.vCharInMap.elementAt(i) as Char;
                        if (c != null && c.cFlag > 0) flagCount++;
                    }
                }
                mFont.tahoma_7b_dark.drawString(g, "Số người cờ:", cardX + cardW / 2, cardY + 27, mFont.CENTER);
                mFont.tahoma_7b_dark.drawString(g, flagCount.ToString(), cardX + cardW / 2, cardY + 43, mFont.CENTER);
            }
            else if (_functionView == FunctionViewActivityOverview || _functionView == FunctionViewActivityDaily
                || _functionView == FunctionViewActivityWeekly || _functionView == FunctionViewActivitySources)
            {
                int cardH = 82;
                UiMenuTheme.PaintSurface(g, new UiRect(cardX, cardY, cardW, cardH),
                    0xEAA000, 0x8C560A);

                mFont.tahoma_7b_dark.drawString(g, "Năng động", cardX + cardW / 2, cardY + 6, mFont.CENTER);
                g.setColor(0xC88300);
                g.drawLine(cardX + 6, cardY + 21, cardX + cardW - 6, cardY + 21);

                ActivityScreen act = ActivityScreen.gI();
                int dailyPts = act.getPanelTierProgress(false);
                int weeklyPts = act.getPanelTierProgress(true);
                if (Char.myCharz() != null && Char.myCharz().cNangdong > dailyPts)
                {
                    dailyPts = (int)Char.myCharz().cNangdong;
                }

                mFont.tahoma_7b_dark.drawString(g, "Hôm nay:", cardX + cardW / 2, cardY + 25, mFont.CENTER);
                mFont.tahoma_7b_dark.drawString(g, dailyPts + "/100", cardX + cardW / 2, cardY + 39, mFont.CENTER);

                mFont.tahoma_7b_dark.drawString(g, "Tuần:", cardX + cardW / 2, cardY + 53, mFont.CENTER);
                mFont.tahoma_7b_dark.drawString(g, weeklyPts.ToString(), cardX + cardW / 2, cardY + 67, mFont.CENTER);
            }
        }

        private void PaintFunctionDefault(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
        }

        private void PaintFunctionNotification(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            UiAccordionLayout layout = BuildFunctionNotificationLayout(out string[] expandedLines);
            if (layout.Count == 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa có thông báo.", _rightBodyRect.X + _rightBodyRect.Width / 2,
                    _rightBodyRect.Y + 12, mFont.CENTER);
                return;
            }
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                for (int i = 0; i < layout.Count; i++)
                {
                    GameInfo info = Panel.vGameInfo.elementAt(i) as GameInfo;
                    if (info == null) continue;

                    bool isSelected = i == layout.ExpandedIndex;
                    UiRect headerRect = layout.GetHeaderBounds(i, scrollY);
                    UiMenuTheme.PaintButton(g, headerRect, info.main ?? string.Empty, isSelected);

                    if (isSelected)
                    {
                        UiRect boxRect = layout.GetBodyBounds(i, scrollY);
                        UiMenuTheme.PaintSurface(g, boxRect, 0xFAF5EB);

                        for (int lineIdx = 0; lineIdx < expandedLines.Length; lineIdx++)
                        {
                            mFont.tahoma_7b_dark.drawString(g, expandedLines[lineIdx],
                                boxRect.X + UiAccordionLayout.BodyTextLeftInset,
                                boxRect.Y + UiAccordionLayout.BodyTextTopInset
                                    + lineIdx * UiAccordionLayout.BodyLineHeight, mFont.LEFT);
                        }
                    }
                }
            }
        }

        private void PaintFunctionZones(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            GameScr game = GameScr.gI();
            int[] zones = game.zones;
            int[] states = game.pts;
            int[] players = game.numPlayer;
            int[] capacity = game.maxPlayer;
            int count = zones != null ? zones.Length : 0;
            if (count == 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải danh sách khu...", _rightBodyRect.X + _rightBodyRect.Width / 2,
                    _rightBodyRect.Y + 12, mFont.CENTER);
                return;
            }

            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            UiGridLayout grid = new UiGridLayout(_rightBodyRect, 4, 3, 3, 29, 26);

            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                for (int i = 0; i < count; i++)
                {
                    if (!grid.IsCellVisible(i, scrollY)) continue;
                    UiRect cell = grid.GetCellBounds(i, scrollY);
                    int x = cell.X;
                    int y = cell.Y;
                    int cellW = cell.Width;
                    int cellH = cell.Height;

                    int state = states != null && i < states.Length ? states[i] : 1;
                    int color = state == 0 ? 0x50B60E : (state == 2 ? 0xF53B30 : 0xF5E829);

                    bool isSelected = i == _selectedFunctionRow || (zones[i] == TileMap.zoneID);
                    g.setColor(isSelected ? 0xFFF7A0 : 0x795C2B);
                    g.fillRect(x, y, cellW, cellH, 4);

                    g.setColor(color);
                    g.fillRect(x + 1, y + 1, cellW - 2, cellH - 2, 3);

                    mFont.tahoma_7b_dark.drawString(g, zones[i].ToString(), x + cellW / 2, y + 2, mFont.CENTER);

                    int current = players != null && i < players.Length ? players[i] : 0;
                    int maximum = capacity != null && i < capacity.Length ? capacity[i] : 0;
                    mFont.tahoma_7b_dark.drawString(g, current + "/" + maximum, x + cellW / 2, y + 13, mFont.CENTER);
                }
            }
        }

        private void PaintFunctionFlags(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            MyVector flags = GameCanvas.panel != null ? GameCanvas.panel.vFlag : null;
            int count = flags != null ? flags.size() : 0;
            if (count == 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải danh sách cờ...", _rightBodyRect.X + _rightBodyRect.Width / 2,
                    _rightBodyRect.Y + 12, mFont.CENTER);
                return;
            }
            _rightScrollAdapter?.Paint(g, (graphics, i, bounds) =>
            {
                    if (i >= count) return;
                    Item flag = flags.elementAt(i) as Item;

                    UiRect row = new UiRect(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, 36);
                    bool isSelected = i == _selectedFunctionRow || (flag != null && flag.template != null && flag.template.id == Char.myCharz().cFlag);

                    UiMenuTheme.PaintCard(g, row,
                        isSelected ? 0xFFF0B0 : 0xF6F3EE,
                        isSelected ? 0xF8CD63 : 0xB9AA93);

                    UiRect icon = new UiRect(row.X + 3, row.Y + 3, 30, row.Height - 6);
                    g.setColor(0xC8B08A);
                    g.fillRect(icon.X, icon.Y, icon.Width, icon.Height, 3);

                    if (flag != null && flag.template != null)
                    {
                        SmallImage.drawSmallImage(g, flag.template.iconID, icon.X + icon.Width / 2,
                            icon.Y + icon.Height / 2, 0, 3);
                        mFont.tahoma_7b_dark.drawString(g, flag.template.name, icon.Right + 6, row.Y + 4, mFont.LEFT);
                        string option = GetItemOptionSummary(flag);
                        if (!string.IsNullOrEmpty(option))
                        {
                            mFont.tahoma_7b_dark.drawString(g, TruncateString(mFont.tahoma_7b_dark, option, row.Width - icon.Width - 14),
                                icon.Right + 6, row.Y + 18, mFont.LEFT);
                        }
                    }
                    else
                    {
                        mFont.tahoma_7_grey.drawString(g, "Không có dữ liệu cờ", icon.Right + 6, row.Y + 10, mFont.LEFT);
                    }
            });
        }

        private void PaintFunctionActivity(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            ActivityScreen dashboard = ActivityScreen.gI();
            if (_functionView == FunctionViewActivityOverview)
            {
                string[] options = new string[] { "Mốc ngày", "Mốc tuần", "Nguồn điểm" };
                int cardW = _rightBodyRect.Width - 16;
                int cardH = 34;
                int startY = _rightBodyRect.Y + 12;
                int gap = 8;
                for (int i = 0; i < options.Length; i++)
                {
                    UiRect row = new UiRect(_rightBodyRect.X + 8, startY + i * (cardH + gap), cardW, cardH);
                    bool isHovered = (Main.isPC && row.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse)) || (i == _selectedFunctionRow);

                    UiMenuTheme.PaintCard(g, row,
                        isHovered ? 0xFFF0B0 : 0xF6F3EE,
                        isHovered ? 0xF8CD63 : 0xB9AA93);

                    mFont.tahoma_7b_dark.drawString(g, options[i], row.X + row.Width / 2, row.Y + 9, mFont.CENTER);
                }
                return;
            }
            if (dashboard.isPanelLoading())
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải tiến độ Năng động...", _rightBodyRect.X + _rightBodyRect.Width / 2,
                    _rightBodyRect.Y + 12, mFont.CENTER);
                return;
            }
            int count = GetFunctionScrollableItemCount();
            if (count == 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa có dữ liệu để hiển thị.", _rightBodyRect.X + _rightBodyRect.Width / 2,
                    _rightBodyRect.Y + 12, mFont.CENTER);
                return;
            }
            bool weekly = _functionView == FunctionViewActivityWeekly;
            bool sources = _functionView == FunctionViewActivitySources;
            _rightScrollAdapter?.Paint(g, (graphics, i, bounds) =>
            {
                if (i >= count) return;
                UiRect row = new UiRect(bounds.X + 3, bounds.Y + 2, bounds.Width - 7,
                    bounds.Height - 4);
                int state = sources ? (dashboard.isPanelSourceEnabled(i) ? 1 : 3) : dashboard.getPanelTierState(weekly, i);
                UiMenuTheme.PaintCard(graphics, row,
                    i == _selectedFunctionRow ? 0xFFF0B0 : 0xF6F3EE,
                    state == 1 ? 0x5ABD1B : 0xB9AA93);
                string title = sources ? dashboard.getPanelSourceTitle(i) : dashboard.getPanelTierTitle(weekly, i);
                string status = sources ? dashboard.getPanelSourceStatus(i) : dashboard.getPanelTierStatus(weekly, i);
                string detail = sources ? dashboard.getPanelSourceDetail(i) : dashboard.getPanelTierDetail(weekly, i);
                mFont.tahoma_7b_dark.drawString(graphics, title, row.X + 5, row.Y + 3, mFont.LEFT);
                (state == 1 ? mFont.tahoma_7b_green : state == 2 ? mFont.tahoma_7_green : mFont.tahoma_7b_red)
                    .drawString(graphics, status, row.Right - 4, row.Y + 3, mFont.RIGHT);
                mFont.tahoma_7_grey.drawString(graphics, TruncateString(mFont.tahoma_7_grey, detail, row.Width - 10),
                    row.X + 5, row.Y + 16, mFont.LEFT);
                int progress = sources ? dashboard.getPanelSourceProgress(i) : dashboard.getPanelTierProgress(weekly);
                int target = sources ? dashboard.getPanelSourceCap(i) : dashboard.getPanelTierTarget(weekly, i);
                UiProgressBar.PaintFlat(graphics, new UiRect(row.X + 5, row.Bottom - 9, row.Width - 10, 6),
                    progress, target, 0xD8DFC4, state == 1 ? 0x4DBD18 : 0x2EA52A);
            });
        }

        private void PaintFunctionAccount(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            string[] accOptions = new string[] { mResources.inventory_Pass, mResources.friend, mResources.enemy, mResources.msg, mResources.charger };
            int cardW = _rightBodyRect.Width - 16;
            int cardH = 26;
            int startY = _rightBodyRect.Y + 6;
            for (int i = 0; i < accOptions.Length; i++)
            {
                UiRect row = new UiRect(_rightBodyRect.X + 8, startY + i * (cardH + 4), cardW, cardH);
                bool isHovered = Main.isPC && row.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse);
                UiMenuTheme.PaintCard(g, row,
                    isHovered ? 0xFFF0B0 : 0xF6F3EE,
                    isHovered ? 0xF8CD63 : 0xB9AA93);
                mFont.tahoma_7b_dark.drawString(g, accOptions[i], row.X + row.Width / 2, row.Y + 6, mFont.CENTER);
            }
        }

        private void PaintFunctionSettings(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            string[] optLabels = new string[]
            {
                "Âm thanh: " + (GameCanvas.isPlaySound ? "Bật" : "Tắt"),
                "Tăng VGA: " + (Panel.graphics == 1 ? "Bật" : "Tắt"),
                "Bàn phím ảo: " + (GameScr.isAnalog != 0 ? "Bật" : "Tắt"),
                "Màn hình: " + (mGraphics.zoomLevel <= 1 ? "x2" : "x1")
            };
            int cardW = _rightBodyRect.Width - 16;
            int cardH = 28;
            int startY = _rightBodyRect.Y + 10;
            for (int i = 0; i < optLabels.Length; i++)
            {
                UiRect row = new UiRect(_rightBodyRect.X + 8, startY + i * (cardH + 6), cardW, cardH);
                bool isHovered = Main.isPC && row.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse);
                UiMenuTheme.PaintCard(g, row,
                    isHovered ? 0xFFF0B0 : 0xF6F3EE,
                    isHovered ? 0xF8CD63 : 0xB9AA93);
                mFont.tahoma_7b_dark.drawString(g, optLabels[i], row.X + row.Width / 2, row.Y + 7, mFont.CENTER);
            }
        }

        private void PaintFunctionHistory(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            MyVector entries = GameCanvas.panel != null ? GameCanvas.panel.getPKHistoryEntries() : null;
            if (entries == null || entries.size() == 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa có lịch sử giao đấu.", _rightBodyRect.X + _rightBodyRect.Width / 2,
                    _rightBodyRect.Y + 16, mFont.CENTER);
                return;
            }
            _rightScrollAdapter?.Paint(g, (graphics, i, bounds) =>
            {
                    if (i >= entries.size()) return;
                    PKHistoryEntry entry = entries.elementAt(i) as PKHistoryEntry;
                    if (entry == null) return;
                    UiRect row = new UiRect(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, 33);
                    UiMenuTheme.PaintCard(g, row);
                    mFont.tahoma_7b_dark.drawString(g, entry.opponentName ?? "Đối thủ", row.X + 6, row.Y + 3, mFont.LEFT);
                    mFont resultFont = entry.won ? mFont.tahoma_7b_green : mFont.tahoma_7b_red;
                    resultFont.drawString(g, entry.won ? "Thắng" : "Thua", row.Right - 6, row.Y + 3, mFont.RIGHT);
                    long elapsed = mSystem.currentTimeMillis() / 1000L - entry.completedAt;
                    if (elapsed < 0L) elapsed = 0L;
                    string timeStr = NinjaUtil.getTimeAgo(elapsed) + " " + mResources.ago;
                    mFont.tahoma_7_grey.drawString(g, timeStr, row.X + 6, row.Y + 17, mFont.LEFT);
            });
        }

        private void PaintFunctionChangeAccount(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            UiRect box = new UiRect(_rightBodyRect.X + 12, _rightBodyRect.Y + 24, _rightBodyRect.Width - 24, 80);
            UiMenuTheme.PaintCard(g, box);

            mFont.tahoma_7b_dark.drawString(g, "Bạn có muốn đổi tài khoản?", box.X + box.Width / 2, box.Y + 12, mFont.CENTER);

            UiRect okBtn = new UiRect(box.X + 10, box.Bottom - 30, (box.Width - 28) / 2, 22);
            UiRect cancelBtn = new UiRect(okBtn.Right + 8, box.Bottom - 30, okBtn.Width, 22);

            PaintFunctionMenuButton(g, okBtn, "Đồng ý", true, false);
            PaintFunctionMenuButton(g, cancelBtn, "Hủy", false, false);
        }

        private void PaintFunctionWorldChat(mGraphics g)
        {
            PaintClanSurface(g, _functionWorldChatListRect, 0xDED1BB);
            int count = GameScr.vChatVip.size();
            if (count == 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa có lịch sử chat thế giới.",
                    _functionWorldChatListRect.X + _functionWorldChatListRect.Width / 2,
                    _functionWorldChatListRect.Y + 12, mFont.CENTER);
            }
            _rightScrollAdapter?.Paint(g, (graphics, i, bounds) =>
            {
                if (i >= count) return;
                string text = GameScr.vChatVip.elementAt(i) as string ?? string.Empty;
                UiRect row = new UiRect(bounds.X + 3, bounds.Y + 2, bounds.Width - 7,
                    bounds.Height - 3);
                UiMenuTheme.PaintCard(graphics, row,
                    i == _selectedFunctionRow ? 0xFFF0B0 : 0xF6F3EE,
                    i == _selectedFunctionRow ? 0xF8CD63 : 0xB9AA93);
                mFont.tahoma_7b_dark.drawString(graphics,
                    TruncateString(mFont.tahoma_7b_dark, text, row.Width - 10),
                    row.X + 5, row.Y + 4, mFont.LEFT);
            });
            PaintClanSurface(g, _functionWorldChatComposerRect, 0xEDE5D9);
            _functionWorldChatField?.paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            PaintClanButton(g, _functionWorldChatSendRect, "Gửi\n5 ngọc", false,
                _functionFocusArea == FunctionFocusContent && _functionWorldChatFocused);
        }

        private void PaintFunctionToggles(mGraphics g)
        {
            PaintClanSurface(g, _rightBodyRect, 0xDED1BB);
            mFont.tahoma_7b_dark.drawString(g, "Chức năng:", _rightBodyRect.X + 5, _rightBodyRect.Y + 4, mFont.LEFT);
            for (int i = 0; i < _functionToggleRects.Length; i++)
            {
                UiRect rect = _functionToggleRects[i];
                if (i == 3)
                    mFont.tahoma_7b_dark.drawString(g, "Tự động:", rect.X + 2, rect.Y - 11, mFont.LEFT);
                PaintFunctionToggleRow(g, rect, GetFunctionToggleLabel(i), IsFunctionSettingEnabled(i),
                    _functionFocusArea == FunctionFocusContent && _selectedFunctionRow == i);
            }
        }

        private static void PaintFunctionToggleRow(mGraphics g, UiRect rect, string label, bool enabled, bool focused)
        {
            UiMenuTheme.PaintCard(g, rect,
                focused ? 0xFFF0B0 : 0xF6F3EE,
                focused ? 0xF8CD63 : 0xB9AA93);
            mFont.tahoma_7b_dark.drawString(g, label, rect.X + 7, rect.Y + 6, mFont.LEFT);
            int switchWidth = 46;
            int switchHeight = 18;
            int switchX = rect.Right - switchWidth - 8;
            int switchY = rect.Y + (rect.Height - switchHeight) / 2;
            g.setColor(enabled ? 0x50B60E : 0xB9A68A);
            g.fillRect(switchX, switchY, switchWidth, switchHeight, 8);
            g.setColor(0xF7F6EE);
            int knobX = enabled ? switchX + switchWidth - switchHeight + 1 : switchX + 1;
            g.fillRect(knobX, switchY + 2, switchHeight - 4, switchHeight - 4, 7);
        }

        private static string GetFunctionToggleLabel(int index)
        {
            if (index == 0) return "FPS Cao";
            if (index == 1) return "Nhân vật trong khu";
            if (index == 2) return "Thông tin bản thân";
            if (index == 3) return "Auto Pha lê hóa";
            return "Auto Đăng nhập";
        }

        private static bool IsFunctionSettingEnabled(int index)
        {
            if (index == 0) return ModFunc.GI().isHighFps;
            if (index == 1) return ModFunc.GI().showCharsInMap;
            if (index == 2) return ModFunc.GI().showInfoMe;
            if (index == 3) return ModFunc.GI().isAutoPhaLe;
            return ModFunc.isAutoLogin;
        }

    }
}
