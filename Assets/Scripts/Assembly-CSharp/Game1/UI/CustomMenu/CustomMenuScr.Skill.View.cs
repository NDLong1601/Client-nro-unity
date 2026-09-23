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
        private void PaintSkillTabContent(mGraphics g)
        {
            PaintSkillHeaders(g);
            PaintSkillListColumn(g);
            PaintSkillDetailColumn(g);
            if (_showSkillKeyPicker) PaintSkillKeyPicker(g);
            if (_showIntrinsicList || _showIntrinsicConfirmation) PaintIntrinsicSideButtons(g);
        }

        private void PaintSkillHeaders(mGraphics g)
        {
            UiSkillPanel.PaintHeader(g, _skillListHeaderRect,
                "Tiềm năng : " + NinjaUtil.getMoneys(Char.myCharz() != null ? Char.myCharz().cTiemNang : 0L));
            UiSkillPanel.PaintHeader(g, _skillDetailHeaderRect,
                _showIntrinsicList ? "Danh sách nội tại" : "Chi tiết");
        }

        private void PaintSkillListColumn(mGraphics g)
        {
            UiSkillPanel.PaintListColumn(g, _leftColRect, _leftScrollAdapter,
                (graphics, row, bounds) => PaintSkillListRow(graphics, row, bounds.Y));
        }

        private void PaintSkillListRow(mGraphics g, int row, int y)
        {
            bool selected = row == _selectedSkillRow;
            UiMenuTheme.PaintCard(g,
                new UiRect(_leftColRect.X + 1, y, _leftColRect.Width - 2, SkillRowHeight - 1),
                selected ? 0xFFF0B0 : 0xF6F3EE,
                selected ? 0xF8CD63 : 0xB9AA93);

            if (GameScr.imgSkill != null) g.drawImage(GameScr.imgSkill, _leftColRect.X + 3, y + 3, 0);
            if (row < PotentialStatRowCount) PaintPotentialListRow(g, row, y);
            else if (row == IntrinsicRowIndex) PaintIntrinsicListRow(g, y);
            else PaintTemplateSkillListRow(g, row - SkillTemplateStartRow, y);
        }

        private void PaintPotentialListRow(mGraphics g, int row, int y)
        {
            Char me = Char.myCharz();
            if (me == null) return;
            SmallImage.drawSmallImage(g, PotentialIcons[row], _leftColRect.X + 7, y + 7, 0, 0);

            int textX = _leftColRect.X + 41;
            int textWidth = _leftColRect.Width - 46;
            string valueSuffix = row == 4 ? "%" : string.Empty;
            string title = PotentialNames[row] + " : " + NinjaUtil.getMoneys(GetPotentialCurrentValue(me, row)) + valueSuffix;
            string subtitle = NinjaUtil.getMoneys(GetPotentialCost(me, row)) + " tiềm năng : Tăng "
                + GetPotentialIncreaseValue(me, row) + valueSuffix;
            mFont.tahoma_7b_blue.drawString(g, TruncateString(mFont.tahoma_7b_blue, title, textWidth), textX, y + 4, mFont.LEFT);
            mFont.tahoma_7_grey.drawString(g, TruncateString(mFont.tahoma_7_grey, subtitle, textWidth), textX, y + 19, mFont.LEFT);
        }

        private void PaintIntrinsicListRow(mGraphics g, int y)
        {
            if (Panel.specialInfo != null && Panel.spearcialImage >= 0)
                SmallImage.drawSmallImage(g, Panel.spearcialImage, _leftColRect.X + 7, y + 7, 0, 0);
            int textX = _leftColRect.X + 41;
            int textWidth = _leftColRect.Width - 46;
            mFont.tahoma_7b_blue.drawString(g, "Nội tại", textX, y + 4, mFont.LEFT);
            string summary = string.IsNullOrEmpty(Panel.specialInfo) ? "Chưa mở nội tại" : Panel.specialInfo;
            mFont.tahoma_7_grey.drawString(g, TruncateString(mFont.tahoma_7_grey, summary, textWidth), textX, y + 19, mFont.LEFT);
        }

        private void PaintTemplateSkillListRow(mGraphics g, int templateIndex, int y)
        {
            Char me = Char.myCharz();
            if (me == null || me.nClass == null || me.nClass.skillTemplates == null || templateIndex >= me.nClass.skillTemplates.Length) return;
            SkillTemplate template = me.nClass.skillTemplates[templateIndex];
            if (template == null) return;
            Skill skill = me.getSkill(template);
            SmallImage.drawSmallImage(g, template.iconId, _leftColRect.X + 7, y + 7, 0, 0);

            int textX = _leftColRect.X + 41;
            int textRight = _leftColRect.X + _leftColRect.Width - 5;
            int nameWidth = _leftColRect.Width - 85;
            if (skill != null)
            {
                mFont.tahoma_7b_blue.drawString(g, TruncateString(mFont.tahoma_7b_blue, template.name, nameWidth), textX, y + 4, mFont.LEFT);
                mFont.tahoma_7_blue.drawString(g, "Cấp " + skill.getDisplayLevel(), textRight, y + 4, mFont.RIGHT);
                int barWidth = System.Math.Min(54, _leftColRect.Width - 50);
                int progress = skill.curExp;
                if (progress < 0) progress = 0;
                if (progress > 1000) progress = 1000;
                UiProgressBar.PaintFlat(g, new UiRect(textX, y + 22, barWidth, 7),
                    progress, 1000, 0xC9D9AA, 0x00BE69);
            }
            else
            {
                mFont.tahoma_7b_green.drawString(g, TruncateString(mFont.tahoma_7b_green, template.name, _leftColRect.Width - 48), textX, y + 4, mFont.LEFT);
                Skill firstLevel = template.skills != null && template.skills.Length > 0 ? template.skills[0] : null;
                string requirement = firstLevel != null ? "Cần " + NinjaUtil.getMoneys(firstLevel.powRequire) + " tiềm năng để học" : "Chưa học";
                mFont.tahoma_7_grey.drawString(g, TruncateString(mFont.tahoma_7_grey, requirement, _leftColRect.Width - 46), textX, y + 19, mFont.LEFT);
            }
        }

        private void PaintSkillDetailColumn(mGraphics g)
        {
            UiSkillPanel.PaintDetailColumn(g, _rightBodyRect, PaintSkillDetailContent);
        }

        private void PaintSkillDetailContent(mGraphics g)
        {
            if (_showIntrinsicList)
            {
                PaintIntrinsicList(g);
                return;
            }
            if (_selectedSkillRow < 0) return;
            if (_selectedSkillRow < PotentialStatRowCount) PaintPotentialDetail(g);
            else if (_selectedSkillRow == IntrinsicRowIndex) PaintIntrinsicDetail(g);
            else PaintTemplateSkillDetail(g);
        }

        private void PaintPotentialDetail(mGraphics g)
        {
            Char me = Char.myCharz();
            if (me == null) return;
            long increase = GetPotentialIncreaseValue(me, _selectedSkillRow);
            int action = IsPotentialActionVisible(_selectedPotentialAction) ? _selectedPotentialAction : GetFirstVisiblePotentialAction();
            string suffix = _selectedSkillRow == 4 ? "%" : string.Empty;
            string detail;
            if (action < 0)
            {
                detail = "Chưa đủ tiềm năng để tăng " + PotentialNames[_selectedSkillRow] + ".";
            }
            else if (action == 3)
            {
                detail = "Tự động tăng " + PotentialNames[_selectedSkillRow] + " theo số lượng đã nhập.";
            }
            else
            {
                int batch = GetPotentialBatch(action);
                detail = "Sử dụng " + NinjaUtil.getMoneys(GetPotentialBatchCost(me, _selectedSkillRow, batch))
                    + " tiềm năng để nâng " + increase * batch + suffix + " " + PotentialNames[_selectedSkillRow] + ".";
            }
            DrawWrappedText(g, mFont.tahoma_7b_dark, detail, _rightBodyRect.X + 12, _rightBodyRect.Y + 8, _rightBodyRect.Width - 24);

            for (int i = 0; i < _potentialButtonRects.Length; i++)
            {
                if (!IsPotentialActionVisible(i)) continue;
                string label = i == 3 ? "Tăng\ntự động" : "Tăng " + increase * GetPotentialBatch(i) + suffix;
                PaintSkillActionButton(g, _potentialButtonRects[i], label, _skillFocusArea == SkillFocusDetail && i == action);
            }
        }

        private void PaintIntrinsicDetail(mGraphics g)
        {
            EnsureDefaultIntrinsicActions();
            int centerX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            mFont.tahoma_7b_dark.drawString(g, "Nội tại", centerX, _rightBodyRect.Y + 8, mFont.CENTER);
            if (Panel.specialInfo != null && Panel.spearcialImage >= 0)
                SmallImage.drawSmallImage(g, Panel.spearcialImage, centerX, _rightBodyRect.Y + 35, 0, 3);
            string detail = !string.IsNullOrEmpty(Panel.specialInfo)
                ? Panel.specialInfo
                : (!string.IsNullOrEmpty(_intrinsicDialogText) ? _intrinsicDialogText : "Chưa mở nội tại.");
            DrawWrappedText(g, mFont.tahoma_7_green2, detail, _rightBodyRect.X + 12, _rightBodyRect.Y + 58, _rightBodyRect.Width - 24);

            if (_intrinsicActionCount <= 0) return;
            int dividerY = GetIntrinsicActionRect(0).Y - 7;
            g.setColor(0xB29468);
            g.drawLine(_rightBodyRect.X + 10, dividerY, _rightBodyRect.X + _rightBodyRect.Width - 10, dividerY);
            for (int i = 0; i < _intrinsicActionCount; i++)
            {
                bool focused = _skillFocusArea == SkillFocusDetail && i == _selectedIntrinsicAction;
                PaintSkillActionButton(g, GetIntrinsicActionRect(i), _intrinsicActionLabels[i], focused);
            }
        }

        private void PaintIntrinsicList(mGraphics g)
        {
            Char me = Char.myCharz();
            int count = GetIntrinsicListCount();
            if (me == null || count <= 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa có danh sách nội tại.",
                    _rightBodyRect.X + _rightBodyRect.Width / 2, _rightBodyRect.Y + 12, mFont.CENTER);
                return;
            }

            _rightScrollAdapter?.Paint(g, (graphics, i, bounds) =>
            {
                    if (i >= count) return;
                    int rowY = bounds.Y;
                    bool selected = i == _selectedIntrinsicListIndex;
                    UiMenuTheme.PaintCard(g,
                        new UiRect(_rightBodyRect.X + 1, rowY, _rightBodyRect.Width - 2, IntrinsicListRowHeight - 1),
                        selected ? 0xFFF0B0 : 0xF6F3EE,
                        selected ? 0xF8CD63 : 0xB9AA93);

                    PaintIntrinsicIconFrame(g, _rightBodyRect.X + 3, rowY + 2, selected);
                    if (me.imgSpeacialSkill != null && me.imgSpeacialSkill.Length > 0
                        && me.imgSpeacialSkill[0] != null && i < me.imgSpeacialSkill[0].Length)
                        SmallImage.drawSmallImage(g, me.imgSpeacialSkill[0][i], _rightBodyRect.X + 18,
                            rowY + IntrinsicListRowHeight / 2, 0, 3);

                    string info = me.infoSpeacialSkill[0][i] ?? string.Empty;
                    string[] lines = mFont.tahoma_7_grey.splitFontArray(info, _rightBodyRect.Width - 46);
                    int textX = _rightBodyRect.X + 36;
                    if (lines.Length > 0)
                        mFont.tahoma_7_blue.drawString(g, lines[0], textX, rowY + 4, mFont.LEFT);
                    if (lines.Length > 1)
                        mFont.tahoma_7_grey.drawString(g, lines[1], textX, rowY + 19, mFont.LEFT);
            });
        }

        private static void PaintIntrinsicIconFrame(mGraphics g, int x, int y, bool selected)
        {
            g.setColor(selected ? 0x8B42F4 : 0xE68A00);
            g.fillRect(x, y, 31, 31);
            g.setColor(0xFFD15A);
            g.fillRect(x + 2, y + 2, 27, 27);
            g.setColor(0xE7F4F4);
            g.fillRect(x + 4, y + 4, 23, 23);
            g.setColor(0x9B6500);
            g.drawRect(x, y, 30, 30);
        }

        private void PaintIntrinsicSideButtons(mGraphics g)
        {
            bool backFocused = _skillFocusArea == SkillFocusIntrinsicSide && _selectedIntrinsicSideAction == 0;
            PaintSkillActionButton(g, _intrinsicSideButtonRects[0], "Quay lại", backFocused);
            if (!_showIntrinsicList || _selectedIntrinsicListIndex < 0) return;
            bool selectFocused = _skillFocusArea == SkillFocusIntrinsicSide && _selectedIntrinsicSideAction == 1;
            PaintSkillActionButton(g, _intrinsicSideButtonRects[1], "Chọn chỉ số", selectFocused);
        }

        private void PaintIntrinsicInput(mGraphics g)
        {
            UiMenuTheme.PaintSurface(g, _intrinsicInputDialogRect, 0xEDE5D9);
            UiMenuTheme.PaintHeader(g, new UiRect(_intrinsicInputDialogRect.X + 2,
                _intrinsicInputDialogRect.Y + 1, _intrinsicInputDialogRect.Width - 4, 23),
                "Nhập chỉ số");

            g.setColor(0xF7C400);
            g.fillRect(_intrinsicInputCloseRect.X, _intrinsicInputCloseRect.Y,
                _intrinsicInputCloseRect.Width, _intrinsicInputCloseRect.Height);
            g.setColor(0xE59600);
            g.drawRect(_intrinsicInputCloseRect.X, _intrinsicInputCloseRect.Y,
                _intrinsicInputCloseRect.Width, _intrinsicInputCloseRect.Height);
            g.setColor(0xE35A24);
            g.drawLine(_intrinsicInputCloseRect.X + 5, _intrinsicInputCloseRect.Y + 5,
                _intrinsicInputCloseRect.X + 15, _intrinsicInputCloseRect.Y + 15);
            g.drawLine(_intrinsicInputCloseRect.X + 15, _intrinsicInputCloseRect.Y + 5,
                _intrinsicInputCloseRect.X + 5, _intrinsicInputCloseRect.Y + 15);

            _intrinsicInputField?.paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            PaintSkillActionButton(g, _intrinsicNormalButtonRect, "Mở thường", _intrinsicInputFocus == 1);
            PaintSkillActionButton(g, _intrinsicVipButtonRect, "Mở VIP", _intrinsicInputFocus == 2);
        }

        private void PaintTemplateSkillDetail(mGraphics g)
        {
            SkillTemplate template = GetSelectedSkillTemplate();
            if (template == null) return;
            Skill skill = GetSelectedSkill();
            int centerX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            int y = _rightBodyRect.Y + 7;
            mFont.tahoma_7b_dark.drawString(g, template.name, centerX, y, mFont.CENTER);
            y += 15;

            if (template.description != null)
            {
                for (int i = 0; i < template.description.Length; i++)
                {
                    string[] lines = mFont.tahoma_7_green2.splitFontArray(template.description[i] ?? string.Empty, _rightBodyRect.Width - 24);
                    for (int j = 0; j < lines.Length; j++)
                    {
                        mFont.tahoma_7_green2.drawString(g, lines[j], centerX, y, mFont.CENTER);
                        y += 13;
                    }
                }
            }

            y += 4;
            g.setColor(0xB29468);
            g.drawLine(_rightBodyRect.X + 10, y, _rightBodyRect.X + _rightBodyRect.Width - 10, y);
            y += 8;

            if (skill == null)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa học", centerX, y, mFont.CENTER);
                Skill firstLevel = template.skills != null && template.skills.Length > 0 ? template.skills[0] : null;
                if (firstLevel != null)
                    mFont.tahoma_7_blue.drawString(g, "Cần " + NinjaUtil.getMoneys(firstLevel.powRequire) + " tiềm năng", centerX, y + 15, mFont.CENTER);
                return;
            }

            mFont.tahoma_7_blue.drawString(g, "Cấp độ: " + skill.getDisplayLevel(), centerX, y, mFont.CENTER);
            y += 14;
            string damageInfo = NinjaUtil.Replace(template.damInfo ?? string.Empty, "#", skill.damage + string.Empty);
            mFont.tahoma_7_blue.drawString(g, damageInfo, centerX, y, mFont.CENTER);
            y += 14;
            mFont.tahoma_7_blue.drawString(g, "KI tiêu hao: " + skill.manaUse + (template.manaUseType == 1 ? "%" : string.Empty), centerX, y, mFont.CENTER);
            y += 14;
            mFont.tahoma_7_blue.drawString(g, "Hồi chiêu: " + skill.strTimeReplay() + "s", centerX, y, mFont.CENTER);

            int dividerY = _assignSkillButtonRect.Y - 7;
            g.setColor(0xB29468);
            g.drawLine(_rightBodyRect.X + 10, dividerY, _rightBodyRect.X + _rightBodyRect.Width - 10, dividerY);
            PaintSkillActionButton(g, _assignSkillButtonRect, "Gán phím ô", _showSkillKeyPicker || _skillFocusArea == SkillFocusDetail);
        }

        private static void PaintSkillActionButton(mGraphics g, UiRect rect, string label, bool active)
        {
            UiMenuTheme.PaintButton(g, rect, label, active);
        }

        private void PaintSkillKeyPicker(mGraphics g)
        {
            Skill selectedSkill = GetSelectedSkill();
            if (selectedSkill == null) return;
            bool useTouchSlots = GameCanvas.isTouch && !Main.isPC;
            Skill[] slots = useTouchSlots ? GameScr.onScreenSkill : GameScr.keySkill;
            for (int i = 0; i < _skillKeyButtonRects.Length; i++)
            {
                bool assigned = slots != null && i < slots.Length && slots[i] != null && slots[i].template != null
                    && slots[i].template.id == selectedSkill.template.id;
                bool focused = _skillFocusArea == SkillFocusKeys && i == _selectedSkillKeyIndex;
                PaintSkillActionButton(g, _skillKeyButtonRects[i], "Phím " + (i + 1), assigned || focused);
                if (focused)
                {
                    g.setColor(0xFFFFFF);
                    g.drawRect(_skillKeyButtonRects[i].X + 2, _skillKeyButtonRects[i].Y + 2,
                        _skillKeyButtonRects[i].Width - 4, _skillKeyButtonRects[i].Height - 4);
                }
            }
        }

    }
}
