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
        private void PaintClanTabContent(mGraphics g)
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null)
            {
                g.setColor(0xDFD2BC);
                g.fillRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);
                mFont.tahoma_7b_dark.drawString(g, "Bạn chưa tham gia bang hội.",
                    _contentRect.X + _contentRect.Width / 2, _contentRect.Y + _contentRect.Height / 2 - 8,
                    mFont.CENTER);
                mFont.tahoma_7_grey.drawString(g, "Hãy dùng menu Bang hội chính để tìm hoặc tạo bang.",
                    _contentRect.X + _contentRect.Width / 2, _contentRect.Y + _contentRect.Height / 2 + 10,
                    mFont.CENTER);
                return;
            }

            PaintClanChatColumn(g);
            if (_selectedClanView == ClanViewHistory)
            {
                PaintClanHeader(g, _clanRightHeaderRect, "Lịch sử cống hiến", true);
                PaintClanHistory(g);
            }
            else
            {
                PaintClanFunctionTabs(g);
                if (_selectedClanView == ClanViewMembers) PaintClanMembers(g);
                else if (_selectedClanView == ClanViewInfo) PaintClanInfo(g);
                else if (_selectedClanView == ClanViewTreasury) PaintClanTreasury(g);
                else if (_selectedClanView == ClanViewPotential) PaintClanPotential(g);
                else if (_selectedClanView == ClanViewUpgrade) PaintClanUpgrade(g);
            }
            _leftScrollAdapter?.PaintScrollbar(g, _clanChatListRect);
            _rightScrollAdapter?.PaintScrollbar(g, GetClanScrollableBodyRect());
            PaintClanSideActions(g);
        }

        private void PaintClanChatColumn(mGraphics g)
        {
            PaintClanHeader(g, _clanChatHeaderRect, "Chat bang", true);
            PaintClanSurface(g, _clanChatListRect, 0xDED1BB);
            PaintClanChatMessages(g);

            PaintClanSurface(g, _clanChatComposerRect, 0xEDE5D9);
            PaintClanButton(g, _clanShareRect, "Vị trí", false, _clanFocusArea == ClanFocusChat && !_clanChatFocused);
            _clanChatField?.paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            PaintClanButton(g, _clanSendRect, "Gửi", false, _clanFocusArea == ClanFocusChat && _clanChatFocused);
        }

        private void PaintClanChatMessages(mGraphics g)
        {
            if (_leftScrollAdapter == null) return;
            _leftScrollAdapter.Paint(g, (graphics, index, rowBounds) =>
            {
                ClanMessage message = ClanMessage.vMessage.elementAt(index) as ClanMessage;
                if (message == null) return;
                UiRect rowRect = new UiRect(rowBounds.X + 2, rowBounds.Y + 1,
                    rowBounds.Width - 7, ClanChatRowHeight - 3);
                graphics.setColor(0xB9AA93);
                graphics.fillRect(rowRect.X + 1, rowRect.Y + 1, rowRect.Width, rowRect.Height, 4);
                graphics.setColor(0xF8F6F2);
                graphics.fillRect(rowRect.X, rowRect.Y, rowRect.Width, System.Math.Max(1, rowRect.Height - 1), 4);
                graphics.setColor(0xFFFFFF);
                graphics.fillRect(rowRect.X + 3, rowRect.Y + 1, System.Math.Max(1, rowRect.Width - 6), 1);

                UiRect avatarRect = new UiRect(rowRect.X + 3, rowRect.Y + 3, 37, rowRect.Height - 6);
                graphics.setColor(0xB9A68A);
                graphics.fillRect(avatarRect.X, avatarRect.Y, avatarRect.Width, avatarRect.Height, 4);
                PaintClanMemberAvatar(graphics, FindClanMember(message.playerId), avatarRect);

                int textX = avatarRect.Right + 5;
                int visibleOptionCount = GetVisibleClanMessageOptionCount(message);
                int rightReserve = System.Math.Min(80, visibleOptionCount * 38);
                int textWidth = System.Math.Max(35, rowRect.Right - textX - 4 - rightReserve);
                mFont nameFont = message.role == 0 ? mFont.tahoma_7b_red
                    : message.role == 1 ? mFont.tahoma_7b_green : mFont.tahoma_7b_blue;
                nameFont.drawString(graphics,
                    TruncateString(nameFont, message.playerName ?? string.Empty, textWidth),
                    textX, rowRect.Y + 3, mFont.LEFT);
                string body = GetClanMessageText(message);
                string[] lines = mFont.tahoma_7b_dark.splitFontArray(body, textWidth);
                for (int line = 0; line < lines.Length && line < 2; line++)
                    mFont.tahoma_7b_dark.drawString(graphics, lines[line], textX,
                        rowRect.Y + 15 + line * 11, mFont.LEFT);
                if (message.time > 0L)
                {
                    int ago = (int)System.Math.Max(0L, mSystem.currentTimeMillis() / 1000L - message.time);
                    mFont.tahoma_7_grey.drawString(graphics, NinjaUtil.getTimeAgo(ago) + " " + mResources.ago,
                        rowRect.Right - 4, rowRect.Bottom - 12, mFont.RIGHT);
                }
                PaintClanMessageOptions(graphics, message, rowRect);
            });
        }

        private static string GetClanMessageText(ClanMessage message)
        {
            if (message == null) return string.Empty;
            if (message.type == 1)
                return mResources.request_pea + " (" + message.recieve + "/" + message.maxCap + ")";
            if (message.type == 2) return mResources.request_join_clan;
            if (message.chat == null || message.chat.Length == 0) return string.Empty;
            return string.Join(" ", message.chat);
        }

        private void PaintClanMessageOptions(mGraphics g, ClanMessage message, UiRect rowRect)
        {
            int visibleOptionCount = GetVisibleClanMessageOptionCount(message);
            if (visibleOptionCount == 0) return;
            int buttonWidth = 37;
            int startX = rowRect.Right - 3 - visibleOptionCount * buttonWidth;
            for (int i = 0; i < visibleOptionCount; i++)
            {
                UiRect rect = new UiRect(startX + i * buttonWidth, rowRect.Y + 4, buttonWidth - 2, 22);
                UiActionButton.PaintFlat(g, rect,
                    TruncateString(mFont.tahoma_7b_dark, message.option[i], rect.Width - 3),
                    false, ClanMessageOptionButtonStyle);
            }
        }

        private static bool IsOwnClanMessage(ClanMessage message)
        {
            Char me = Char.myCharz();
            if (message == null || me == null) return false;
            if (message.playerId == me.charID) return true;
            return !string.IsNullOrEmpty(me.cName)
                && string.Equals(message.playerName, me.cName, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetVisibleClanMessageOptionCount(ClanMessage message)
        {
            if (message == null || message.option == null || message.option.Length == 0) return 0;
            if (message.type == 1 && (IsOwnClanMessage(message) || message.recieve >= message.maxCap)) return 0;
            if (message.type == 4 && IsOwnClanMessage(message)) return 0;
            Char me = Char.myCharz();
            if (message.type == 2 && (me == null || me.role != 0 || IsOwnClanMessage(message))) return 0;
            return message.option.Length;
        }

        private static void PaintClanMemberAvatar(mGraphics g, Member member, UiRect rect)
        {
            if (member == null) return;
            using (UiRenderState.Push(g, rect, clip: true))
            {
                if (member.headICON >= 0)
                {
                    SmallImage.drawSmallImage(g, member.headICON, rect.X + rect.Width / 2,
                        rect.Y + rect.Height / 2, 0, StaticObj.VCENTER_HCENTER);
                    return;
                }
                int head = member.head;
                if (Char.myCharz() != null && member.ID == Char.myCharz().charID) head = Char.myCharz().head;
                if (GameScr.parts == null || head < 0 || head >= GameScr.parts.Length || GameScr.parts[head] == null)
                    return;
                Part part = GameScr.parts[head];
                int frame = Char.CharInfo[0][0][0];
                if (part.pi == null || frame < 0 || frame >= part.pi.Length || part.pi[frame] == null) return;
                SmallImage.drawSmallImage(g, part.pi[frame].id,
                    rect.X + rect.Width / 2 + Char.CharInfo[0][0][1] + part.pi[frame].dx - 3,
                    rect.Bottom, 0, mGraphics.LEFT | mGraphics.BOTTOM);
            }
        }

        private void PaintClanFunctionTabs(mGraphics g)
        {
            for (int i = 0; i < _clanFunctionRects.Length; i++)
            {
                bool active = i == GetClanFunctionForView(_selectedClanView);
                bool focused = _clanFocusArea == ClanFocusFunctions && i == _selectedClanFunction;
                PaintClanButton(g, _clanFunctionRects[i], ClanFunctionNames[i], active, focused);
            }
        }

        private static void PaintClanHeader(mGraphics g, UiRect rect, string text, bool red)
        {
            UiMenuTheme.PaintHeader(g, rect, text, red);
        }

        private static void PaintClanSurface(mGraphics g, UiRect rect, int fill)
        {
            UiMenuTheme.PaintSurface(g, rect, fill);
        }

        private static void PaintClanButton(mGraphics g, UiRect rect, string text, bool active, bool focused)
        {
            UiMenuTheme.PaintButton(g, rect, text, active, focused);
        }

        private void PaintClanMembers(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xDED1BB);
            MyVector members = GetClanMembers();
            _rightScrollAdapter?.Paint(g, (graphics, i, bounds) =>
            {
                    if (i >= members.size()) return;
                    Member member = members.elementAt(i) as Member;
                    if (member == null) return;
                    int y = bounds.Y;
                    bool focused = _clanFocusArea == ClanFocusContent && _selectedClanRow == i;
                    UiRect card = new UiRect(bounds.X + 3, y + 2,
                        bounds.Width - 9, ClanMemberRowHeight - 4);
                    UiMenuTheme.PaintCard(g, card,
                        focused ? 0xFFF0B0 : 0xF6F3EE, focused ? 0xF8CD63 : 0xB9AA93);
                    UiRect avatar = new UiRect(card.X + 2, card.Y + 1, 28, card.Height - 2);
                    g.setColor(0xB7A489);
                    g.fillRect(avatar.X, avatar.Y, avatar.Width, avatar.Height, 3);
                    PaintClanMemberAvatar(g, member, avatar);
                    int textX = avatar.Right + 4;
                    mFont nameFont = member.role == 0 ? mFont.tahoma_7b_red
                        : member.role == 1 ? mFont.tahoma_7b_green : mFont.tahoma_7b_blue;
                    nameFont.drawString(g, TruncateString(nameFont, member.name ?? string.Empty,
                        _clanRightBodyRect.Width - 74), textX, y + 2, mFont.LEFT);
                    mFont.tahoma_7_grey.drawString(g, mResources.power + ": " + (member.powerPoint ?? "0"),
                        textX, y + 14, mFont.LEFT);
                    mFont.tahoma_7b_dark.drawString(g, member.clanPoint.ToString(),
                        card.Right - 15, y + 7, mFont.RIGHT);
                    SmallImage.drawSmallImage(g, 7223, card.Right - 7,
                        y + ClanMemberRowHeight / 2, 0, StaticObj.VCENTER_HCENTER);
            });
        }

        private static List<string> BuildClanInfoLines()
        {
            List<string> lines = new List<string>();
            Clan clan = Char.myCharz() != null ? Char.myCharz().clan : null;
            if (clan == null) return lines;
            lines.Add("Thông tin chung:");
            lines.Add("- Tên: " + (clan.name ?? string.Empty));
            lines.Add("- Khẩu hiệu: " + (clan.slogan ?? string.Empty));
            lines.Add("- Thành viên: " + clan.currMember + "/" + clan.maxMember);
            lines.Add("- Bang chủ: " + (clan.leaderName ?? string.Empty));
            lines.Add("Clan Value:");
            if (!ClanValue.isReady(clan.ID)) lines.Add("- Đang tải giá trị bang...");
            else if (!ClanValue.current.enabled) lines.Add("- Tính năng đang tạm khóa");
            else
            {
                lines.Add("- Tổng: " + Res.formatNumber(ClanValue.current.totalValue));
                lines.Add("- Cấp bang: " + Res.formatNumber(ClanValue.current.clanLevelScore));
                lines.Add("- Tiềm năng đã dùng: " + Res.formatNumber(ClanValue.current.spentPotentialScore));
                lines.Add("- Cấp cây: " + Res.formatNumber(ClanValue.current.treeLevelScore));
                lines.Add("- Thành tích: " + Res.formatNumber(ClanValue.current.achievementScore));
                lines.Add("- Hoạt động tuần: " + Res.formatNumber(ClanValue.current.weeklyActivityScore));
            }
            lines.Add("Diện mạo Cây bang:");
            if (!ClanAppearance.isReady(clan.ID)) lines.Add("- Đang tải diện mạo...");
            else if (!ClanAppearance.current.enabled) lines.Add("- Tính năng đang tạm khóa");
            else
            {
                string appearanceTitle = !string.IsNullOrEmpty(ClanAppearance.current.title)
                    ? ClanAppearance.current.title : ClanAppearance.current.tierName;
                lines.Add("- Bậc hiện tại: " + appearanceTitle);
                if (ClanAppearance.current.nextTierId >= 0)
                {
                    lines.Add("- Kế tiếp: " + ClanAppearance.current.nextTierName);
                    lines.Add("- Còn cấp bang/cây: " + ClanAppearance.current.remainingClanLevels
                        + "/" + ClanAppearance.current.remainingTreeLevels);
                    lines.Add("- Còn Clan Value: " + Res.formatNumber(ClanAppearance.current.remainingClanValue));
                }
                else lines.Add("- Đã đạt bậc cao nhất");
            }
            lines.Add("Tài sản:");
            lines.Add("- Capsule bang: " + Res.formatNumber(ClanTreasury.current.capsule));
            lines.Add("- Vàng bang: " + Res.formatNumber(ClanTreasury.current.gold));
            lines.Add("- Ngọc bang: " + Res.formatNumber(ClanTreasury.current.gem));
            lines.Add("- Cống hiến: " + Res.formatNumber(ClanTreasury.current.contribution));
            lines.Add("Thuộc tính:");
            if (!ClanProgression.isReady(clan.ID)) lines.Add("- Đang tải chỉ số tiềm năng bang...");
            else
            {
                for (int branch = 0; branch < ClanProgression.BRANCH_COUNT; branch++)
                {
                    lines.Add("- " + ClanPotentialNames[branch] + ": +"
                        + ClanProgression.effectPercentText(branch) + "%");
                }
            }
            lines.Add("Buff bang:");
            for (int line = 0; line < ClanProgression.BUFF_COUNT; line++)
                lines.Add("- " + ClanProgression.buffStatusText(GetClanBuffDisplayType(line)));
            return lines;
        }

        private void PaintClanInfo(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xE8DDCC);
            UiRect viewport = GetClanScrollableBodyRect();
            List<string> lines = BuildClanInfoLines();
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            bool inBuffSection = false;
            int buffLine = 0;
            using (UiRenderState.Push(g, viewport, clip: true))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    int y = viewport.Y + i * 13 - scrollY;
                    if (y + 13 <= viewport.Y || y >= viewport.Bottom) continue;
                    string line = lines[i];
                    bool heading = line.EndsWith(":") && !line.StartsWith("-");
                    if (heading)
                    {
                        inBuffSection = line == "Buff bang:";
                        mFont.tahoma_7b_yellow.drawString(g,
                            TruncateString(mFont.tahoma_7b_yellow, line, viewport.Width - 8),
                            viewport.X + 3, y, mFont.LEFT);
                        continue;
                    }
                    if (inBuffSection && buffLine < ClanProgression.BUFF_COUNT)
                    {
                        int type = GetClanBuffDisplayType(buffLine++);
                        mFont buffFont = GetClanBuffFont(type);
                        string status = TruncateString(buffFont, line, viewport.Width - 8);
                        PaintClanBuffStatus(g, viewport, y, status, buffFont, type,
                            GetClanBuffColor(type), ClanProgression.isBuffActive(type));
                        continue;
                    }
                    mFont font = line.StartsWith("- Tổng:") ? mFont.tahoma_7b_yellow : mFont.tahoma_7b_dark;
                    string display = TruncateString(font, line, viewport.Width - 8);
                    if (line.StartsWith("- Bậc hiện tại:") && ClanAppearance.current.enabled)
                        font.drawStringColor(g, display, viewport.X + 3, y,
                            mFont.LEFT, ClanAppearance.current.accentRgb);
                    else font.drawString(g, display, viewport.X + 3, y, mFont.LEFT);
                }
            }
        }

        private static void PaintClanBuffStatus(mGraphics g, UiRect viewport, int y,
            string status, mFont font, int type, int color, bool active)
        {
            int textX = viewport.X + 3;
            if (active && NeedsClanBuffContrastBackground(type))
            {
                int chipWidth = System.Math.Min(viewport.Width - 5, font.getWidth(status) + 6);
                g.setColor(0x554335);
                g.fillRect(textX - 2, y, chipWidth, 12, 3);
            }
            if (active) font.drawStringColor(g, status, textX, y, mFont.LEFT, color);
            else font.drawString(g, status, textX, y, mFont.LEFT);
        }

        private static bool NeedsClanBuffContrastBackground(int type)
        {
            return type == 2 || type == 5;
        }

        private static int GetClanBuffDisplayType(int line)
        {
            if (line < 3) return line;
            if (line == 3) return 4;
            if (line == 4) return 3;
            return 5;
        }

        private static int GetClanBuffColor(int type)
        {
            if (type == 0) return 0xE00000;
            if (type == 1) return 0x0031E0;
            if (type == 2) return 0xFFFFFF;
            if (type == 3) return 0xDE00BA;
            if (type == 4) return 0x078700;
            return 0xFFF500;
        }

        private static mFont GetClanBuffFont(int type)
        {
            if (!ClanProgression.isBuffActive(type)) return mFont.tahoma_7b_dark;
            if (type == 0) return mFont.tahoma_7_red;
            if (type == 1) return mFont.tahoma_7_blue;
            if (type == 2) return mFont.tahoma_7_white;
            if (type == 3) return mFont.tahoma_7_blue;
            if (type == 4) return mFont.tahoma_7_green;
            return mFont.tahoma_7_yellow;
        }

        private void PaintClanPotential(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xDED1BB);
            Char me = Char.myCharz();
            if (me == null || me.clan == null || !ClanProgression.isReady(me.clan.ID))
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải tiến trình bang...",
                    _clanRightBodyRect.X + _clanRightBodyRect.Width / 2,
                    _clanRightBodyRect.Y + _clanRightBodyRect.Height / 2, mFont.CENTER);
                return;
            }
            mFont.tahoma_7_grey.drawString(g,
                "Số điểm còn lại: " + ClanProgression.current.unspentPoints,
                _clanRightBodyRect.X + 7, _clanRightBodyRect.Y + 3, mFont.LEFT);
            _rightScrollAdapter?.Paint(g, (graphics, branch, bounds) =>
            {
                    if (branch >= ClanProgression.BRANCH_COUNT) return;
                    int y = bounds.Y;
                    bool focused = _clanFocusArea == ClanFocusContent && _selectedClanRow == branch;
                    UiRect card = new UiRect(bounds.X + 3, y + 2,
                        bounds.Width - 9, ClanPotentialRowHeight - 4);
                    UiMenuTheme.PaintCard(g, card,
                        focused ? 0xFFF0B0 : 0xEFE5D6, focused ? 0xF8CD63 : 0xB9AA93);
                    mFont.tahoma_7b_dark.drawString(g, ClanPotentialNames[branch],
                        card.X + 5, y + 3, mFont.LEFT);
                    mFont.tahoma_7_grey.drawString(g, ClanProgression.effectText(branch),
                        card.X + 5, y + 17, mFont.LEFT);
                    UiRect valueRect = new UiRect(card.Right - 75, y + 4, 39, 25);
                    g.setColor(0xF4EFE8);
                    g.fillRect(valueRect.X, valueRect.Y, valueRect.Width, valueRect.Height, 3);
                    mFont.tahoma_7b_dark.drawString(g, ClanProgression.rank(branch).ToString(),
                        valueRect.X + valueRect.Width / 2, valueRect.Y + 7, mFont.CENTER);
                    UiRect plusRect = new UiRect(card.Right - 29, y + 4, 27, 25);
                    _clanPotentialRects[branch] = plusRect;
                    bool canAdd = me.role == 0 && ClanProgression.current.unspentPoints > 0
                        && ClanProgression.rank(branch) < 20;
                    g.setColor(canAdd ? 0xFF4A17 : 0x9B8F82);
                    g.fillRect(plusRect.X + 10, plusRect.Y + 2, 7, 21);
                    g.fillRect(plusRect.X + 3, plusRect.Y + 9, 21, 7);
            });
        }

        private void PaintClanTreasury(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xC4B397);
            if (!_clanStorageLoaded || Char.myCharz() == null || Char.myCharz().arrItemBox == null)
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải kho bang...",
                    _clanRightBodyRect.X + _clanRightBodyRect.Width / 2,
                    _clanRightBodyRect.Y + _clanRightBodyRect.Height / 2, mFont.CENTER);
                return;
            }
            Item[] storage = Char.myCharz().arrItemBox;
            UiGridLayout grid = new UiGridLayout(_clanRightBodyRect, ClanStorageColumns,
                2, 2, ClanStorageRowHeight, ClanStorageRowHeight - 2);
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            using (UiRenderState.Push(g, _clanRightBodyRect, clip: true))
            {
                for (int slot = 0; slot < storage.Length; slot++)
                {
                    if (!grid.IsCellVisible(slot, scrollY)) continue;
                    UiItemSlot.Paint(g, storage[slot], grid.GetCellBounds(slot, scrollY),
                        slot == _selectedClanStorageSlot, 0xB7A489, InventoryEquipmentBorderInset);
                }
            }
        }

        private void PaintClanUpgrade(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xE8DDCC);
            Char me = Char.myCharz();
            int x = _clanRightBodyRect.X + 10;
            int y = _clanRightBodyRect.Y + 7;
            if (me == null || me.clan == null || !ClanProgression.isReady(me.clan.ID))
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải tiến trình bang...", x, y, mFont.LEFT);
                return;
            }
            ClanProgression progression = ClanProgression.current;
            mFont.tahoma_7b_dark.drawString(g, "Cấp bang: " + progression.level, x, y, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, "EXP: " + Res.formatNumber(progression.exp) + "/"
                + Res.formatNumber(progression.expRequired), x, y += 15, mFont.LEFT);
            UiProgressBar.PaintInset(g, new UiRect(x, y + 13, _clanRightBodyRect.Width - 20, 6),
                progression.exp, progression.expRequired, 0x9A896F, 0x65C70D);
            y += 20;
            mFont.tahoma_7_green2.drawString(g, "Capsule bang: " + Res.formatNumber(ClanTreasury.current.capsule)
                + "/" + Res.formatNumber(progression.capsuleRequired), x, y, mFont.LEFT);
            mFont.tahoma_7_yellow.drawString(g, "Vàng: " + Res.formatNumber(ClanTreasury.current.gold)
                + "/" + Res.formatNumber(progression.goldRequired), x, y += 14, mFont.LEFT);
            mFont.tahoma_7_green2.drawString(g, "Ngọc: " + Res.formatNumber(ClanTreasury.current.gem)
                + "/" + Res.formatNumber(progression.gemRequired), x, y += 14, mFont.LEFT);
            PaintClanButton(g, _clanUpgradeButtonRect, "Nâng cấp", true,
                _clanFocusArea == ClanFocusContent);
        }

        private void PaintClanHistory(mGraphics g)
        {
            UiRect historyBody = _clanRightBodyRect;
            PaintClanSurface(g, historyBody, 0xDED1BB);
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            int visibleIndex = 0;
            using (UiRenderState.Push(g, historyBody, clip: true))
            {
                for (int i = 0; i < ClanTreasury.current.ledger.size(); i++)
                {
                    ClanLedgerEntry entry = ClanTreasury.current.ledger.elementAt(i) as ClanLedgerEntry;
                    if (!ClanTreasury.isPlayerContributionEntry(entry)) continue;
                    int y = historyBody.Y + visibleIndex * ClanLedgerRowHeight - scrollY;
                    visibleIndex++;
                    if (y + ClanLedgerRowHeight <= historyBody.Y || y >= historyBody.Bottom) continue;
                    UiRect card = new UiRect(historyBody.X + 3, y + 2,
                        historyBody.Width - 9, ClanLedgerRowHeight - 4);
                    g.setColor(0xB9AA93);
                    g.fillRect(card.X, card.Y + 1, card.Width, card.Height, 4);
                    g.setColor(0xF8F6F2);
                    g.fillRect(card.X, card.Y, card.Width, System.Math.Max(1, card.Height - 1), 4);
                    Member actor = FindClanMember((int)entry.actorId);
                    UiRect avatar = new UiRect(card.X + 2, card.Y + 1, 32, card.Height - 2);
                    g.setColor(0xB7A489);
                    g.fillRect(avatar.X, avatar.Y, avatar.Width, avatar.Height, 3);
                    PaintClanMemberAvatar(g, actor, avatar);
                    bool gem = entry.currencyType == ClanTreasury.CURRENCY_GEM;
                    int textX = avatar.Right + 5;
                    mFont.tahoma_7b_dark.drawString(g,
                        TruncateString(mFont.tahoma_7b_dark, (entry.actorName ?? "Người chơi")
                            + " đã góp " + (gem ? "ngọc" : "vàng"), historyBody.Width - 91),
                        textX, y + 4, mFont.LEFT);
                    mFont.tahoma_7_grey.drawString(g, FormatClanLedgerDate(entry.createdAt),
                        textX, y + 18, mFont.LEFT);
                    mFont amountFont = gem ? mFont.tahoma_7b_green2 : mFont.tahoma_7b_yellow;
                    amountFont.drawString(g, "+" + Res.formatNumber(entry.amount),
                        card.Right - 16, y + 10, mFont.RIGHT);
                    Image icon = gem ? Panel.imgLuong : Panel.imgXu;
                    if (icon != null) g.drawImage(icon, card.Right - 7,
                        y + ClanLedgerRowHeight / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                }
            }
            if (visibleIndex == 0)
                mFont.tahoma_7_grey.drawString(g, "Chưa có đóng góp vàng hoặc ngọc.",
                    historyBody.X + historyBody.Width / 2, historyBody.Y + historyBody.Height / 2,
                    mFont.CENTER);
        }

        private static string FormatClanLedgerDate(long seconds)
        {
            try
            {
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(seconds).AddHours(7.0);
                return date.ToString("HH'h'mm - dd/MM/yyyy");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private int GetClanSideActionCount()
        {
            if (_selectedClanView == ClanViewTreasury) return 3;
            if (_selectedClanView == ClanViewHistory) return 4;
            if (_selectedClanView == ClanViewMembers)
                return Char.myCharz() != null && Char.myCharz().role == 0 ? 3 : 1;
            return 0;
        }

        private string GetClanSideActionLabel(int index)
        {
            if (_selectedClanView == ClanViewTreasury || _selectedClanView == ClanViewHistory)
            {
                if (index == 0) return "Góp vàng";
                if (index == 1) return "Góp ngọc";
                if (index == 2) return "Lịch sử";
                return "Quay lại";
            }
            if (Char.myCharz() != null && Char.myCharz().role == 0)
            {
                if (index == 0) return "Khẩu hiệu";
                if (index == 1) return "Biểu tượng";
                return "Rời bang";
            }
            return "Rời bang";
        }

        private void PaintClanSideActions(mGraphics g)
        {
            int count = GetClanSideActionCount();
            for (int i = 0; i < count; i++)
                PaintClanButton(g, _clanSideActionRects[i], GetClanSideActionLabel(i),
                    _selectedClanView == ClanViewHistory && i == 2,
                    _clanFocusArea == ClanFocusSideActions && _selectedClanSideAction == i);
        }

    }
}
