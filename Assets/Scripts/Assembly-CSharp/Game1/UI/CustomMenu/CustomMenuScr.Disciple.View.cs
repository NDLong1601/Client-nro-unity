using System;
using Game1.Assets.src.g;
using Game1.UI.Adapters;
using Game1.UI.Components;
using Nro.UI;

namespace Game1.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private void PaintDiscipleTabContent(mGraphics g)
        {
            _discipleLeftTabBar.SetSelectedIndex(_discipleLeftTab);
            _discipleRightTabBar.SetSelectedIndex(_discipleRightTab);
            _discipleLeftTabBar.FocusedIndex = _keyboardFocus == KeyboardFocusContent && _discipleFocusArea == 0
                ? _discipleLeftTab : -1;
            _discipleRightTabBar.FocusedIndex = _keyboardFocus == KeyboardFocusContent && _discipleFocusArea == 1
                ? _discipleRightTab : -1;
            _discipleLeftTabBar.PaintRaised(g);
            _discipleRightTabBar.PaintRaised(g);
            UiMenuTheme.PaintSurface(g, _leftColRect);
            UiMenuTheme.PaintSurface(g, _rightBodyRect);

            Char pet = GetDisciple();
            if (pet == null)
            {
                mFont.tahoma_7b_dark.drawString(g, "Bạn chưa có đệ tử.",
                    _contentRect.X + _contentRect.Width / 2, _contentRect.Y + 82, mFont.CENTER);
                return;
            }
            if (_discipleLeftTab == 0) PaintDiscipleEquipment(g, pet);
            else PaintDiscipleInformation(g, pet);
            if (_discipleRightTab == 0) PaintDiscipleSkills(g, pet);
            else PaintDiscipleStatuses(g, pet);
            _leftScrollAdapter?.PaintScrollbar(g, _leftColRect);
            _rightScrollAdapter?.PaintScrollbar(g, _rightBodyRect);
        }

        private void PaintDiscipleEquipment(mGraphics g, Char pet)
        {
            if (pet.arrItemBody == null) return;
            _leftScrollAdapter?.Paint(g, (graphics, slot, bounds) =>
            {
                Item item = slot < pet.arrItemBody.Length ? pet.arrItemBody[slot] : null;
                UiRect row = new UiRect(bounds.X + 2, bounds.Y + 1, bounds.Width - 5, bounds.Height - 2);
                UiMenuTheme.PaintCard(graphics, row);
                UiRect icon = new UiRect(row.X, row.Y, 28, row.Height - 1);
                UiItemSlot.Paint(graphics, item, icon, slot == _selectedDiscipleEquipmentSlot,
                    0xB7A489, InventoryEquipmentBorderInset);
                if (item == null || item.template == null) return;
                int textX = icon.Right + 5;
                int textWidth = row.Right - textX - 3;
                mFont.tahoma_7b_dark.drawString(graphics,
                    TruncateString(mFont.tahoma_7b_dark, item.template.name + GetUpgradeSuffix(item), textWidth),
                    textX, row.Y + 2, mFont.LEFT);
                string summary = GetItemOptionSummary(item);
                if (!string.IsNullOrEmpty(summary))
                    mFont.tahoma_7_blue.drawString(graphics,
                        TruncateString(mFont.tahoma_7_blue, summary, textWidth),
                        textX, row.Y + 16, mFont.LEFT);
            });
        }

        private void PaintDiscipleInformation(mGraphics g, Char pet)
        {
            UiRect portraitCard = new UiRect(_leftColRect.X + 3, _leftColRect.Y + 2,
                _leftColRect.Width - 6, 86);
            UiMenuTheme.PaintCard(g, portraitCard);
            UiRect portrait = new UiRect(portraitCard.X + 3, portraitCard.Y + 3, 66, portraitCard.Height - 6);
            g.setColor(0xB7A489);
            g.fillRect(portrait.X, portrait.Y, portrait.Width, portrait.Height, 4);
            using (UiRenderState.Push(g, portrait, clip: true))
                SmallImage.drawSmallImage(g, pet.avatarz(), portrait.X + portrait.Width / 2,
                    portrait.Bottom - 2, 0, mGraphics.HCENTER | mGraphics.BOTTOM);

            int textX = portrait.Right + 7;
            int textWidth = portraitCard.Right - textX - 3;
            mFont.tahoma_7b_dark.drawString(g,
                TruncateString(mFont.tahoma_7b_dark, pet.cName ?? "Đệ tử", textWidth),
                textX, portraitCard.Y + 6, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, "Sức mạnh:", textX, portraitCard.Y + 20, mFont.LEFT);
            mFont.tahoma_7_orange.drawString(g,
                TruncateString(mFont.tahoma_7_orange, NinjaUtil.getMoneys(pet.cPower), textWidth),
                textX, portraitCard.Y + 32, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g,
                TruncateString(mFont.tahoma_7b_dark, pet.currStrLevel ?? string.Empty, textWidth),
                textX, portraitCard.Y + 46, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, "Thể lực:", textX, portraitCard.Y + 60, mFont.LEFT);
            UiProgressBar.PaintFlat(g, new UiRect(textX, portraitCard.Bottom - 10,
                System.Math.Max(10, textWidth - 2), 6), pet.cStamina, pet.cMaxStamina, 0xB7A489, 0x5279EE);

            UiRect stats = new UiRect(portraitCard.X, portraitCard.Bottom + 3,
                portraitCard.Width, System.Math.Max(1, _leftColRect.Bottom - portraitCard.Bottom - 7));
            UiMenuTheme.PaintCard(g, stats);
            string[] lines =
            {
                "HP: " + NinjaUtil.getMoneys(pet.cHP) + "/" + NinjaUtil.getMoneys(pet.cHPFull),
                "KI: " + NinjaUtil.getMoneys(pet.cMP) + "/" + NinjaUtil.getMoneys(pet.cMPFull),
                "Sức đánh: " + NinjaUtil.getMoneys(pet.cDamFull),
                "Giáp: " + NinjaUtil.getMoneys(pet.cDefull),
                "Chí mạng: " + pet.cCriticalFull + "%",
                "Tiềm năng: " + NinjaUtil.getMoneys(pet.cTiemNang)
            };
            using (UiRenderState.Push(g, stats, clip: true))
            {
                for (int i = 0; i < lines.Length; i++)
                    mFont.tahoma_7b_dark.drawString(g,
                        TruncateString(mFont.tahoma_7b_dark, lines[i], stats.Width - 10),
                        stats.X + 5, stats.Y + 4 + i * 14, mFont.LEFT);
            }
        }

        private void PaintDiscipleSkills(mGraphics g, Char pet)
        {
            int learnedCount = pet.arrPetSkill != null ? pet.arrPetSkill.Length : 0;
            _rightScrollAdapter?.Paint(g, (graphics, rowIndex, bounds) =>
            {
                bool isPotential = rowIndex < 5;
                bool selected = _discipleFocusArea == 1 && rowIndex == _selectedDiscipleRow;
                int textX = bounds.X + 30;
                int textWidth = bounds.Width - 35;
                graphics.setColor(isPotential
                    ? (selected ? 16383818 : 15196114)
                    : (selected ? 16776068 : 16765060));
                graphics.fillRect(textX, bounds.Y, bounds.Width - 30, bounds.Height - 1);
                if (GameScr.imgSkill != null)
                    graphics.drawImage(GameScr.imgSkill, bounds.X, bounds.Y, 0);
                if (!isPotential && GameScr.imgSkill2 != null)
                    graphics.drawImage(GameScr.imgSkill2, bounds.X, bounds.Y, 0);

                if (isPotential)
                {
                    int potentialType = rowIndex;
                    SmallImage.drawSmallImage(graphics, PotentialIcons[potentialType],
                        bounds.X + 4, bounds.Y + 4, 0, 0);
                    string value = GetDisciplePotentialValue(pet, potentialType);
                    string statName = potentialType == 0 ? mResources.HP
                        : potentialType == 1 ? mResources.KI
                        : potentialType == 2 ? mResources.hit_point
                        : potentialType == 3 ? mResources.armor : mResources.critical;
                    mFont.tahoma_7b_blue.drawString(graphics,
                        TruncateString(mFont.tahoma_7b_blue,
                            statName + " " + mResources.root + ": " + value, textWidth),
                        textX + 5, bounds.Y + 3, mFont.LEFT);
                    long cost = GetDisciplePotentialCost(pet, potentialType);
                    string costText = potentialType == 4 ? Res.formatNumber2(cost) : NinjaUtil.getMoneys(cost);
                    string subtitle = costText + " " + mResources.potential + ": "
                        + mResources.increase + " " + (potentialType < 2 ? 20 : 1);
                    mFont.tahoma_7_green2.drawString(graphics,
                        TruncateString(mFont.tahoma_7_green2, subtitle, textWidth),
                        textX + 5, bounds.Y + 15, mFont.LEFT);
                }
                else
                {
                    int skillIndex = rowIndex - 5;
                    if (skillIndex >= learnedCount) return;
                    Skill skill = pet.arrPetSkill[skillIndex];
                    if (skill != null && skill.template != null)
                        SmallImage.drawSmallImage(graphics, skill.template.iconId,
                            bounds.X + 4, bounds.Y + 4, 0, 0);
                    if (skill == null || skill.template == null)
                    {
                        mFont.tahoma_7_green2.drawString(graphics,
                            skill != null && !string.IsNullOrEmpty(skill.moreInfo)
                                ? TruncateString(mFont.tahoma_7_green2, skill.moreInfo, textWidth)
                                : "Chưa học kỹ năng", textX + 5, bounds.Y + 3, mFont.LEFT);
                        mFont.tahoma_7_green2.drawString(graphics,
                            mResources.level + ": 0", textX + 5, bounds.Y + 15, mFont.LEFT);
                        if (GameScr.efs != null && GameScr.efs.Length > 98
                            && GameScr.efs[98] != null && GameScr.efs[98].arrEfInfo != null
                            && GameScr.efs[98].arrEfInfo.Length > 0)
                            SmallImage.drawSmallImage(graphics, GameScr.efs[98].arrEfInfo[0].idImg,
                                bounds.X + 8, bounds.Y + 7, 0, 0);
                        return;
                    }
                    mFont.tahoma_7_blue.drawString(graphics,
                        TruncateString(mFont.tahoma_7_blue, skill.template.name, textWidth),
                        textX + 5, bounds.Y + 3, mFont.LEFT);
                    mFont.tahoma_7_green2.drawString(graphics,
                        mResources.level + ": " + skill.getDisplayLevel(), textX + 5, bounds.Y + 15, mFont.LEFT);
                }
            });
        }

        private static string GetDisciplePotentialValue(Char pet, int type)
        {
            if (type == 0) return NinjaUtil.getMoneys(pet.cHPGoc);
            if (type == 1) return NinjaUtil.getMoneys(pet.cMPGoc);
            if (type == 2) return NinjaUtil.getMoneys(pet.cDamGoc);
            if (type == 3) return NinjaUtil.getMoneys(pet.cDefGoc);
            return pet.cCriticalGoc + "%";
        }

        private void PaintDiscipleStatuses(mGraphics g, Char pet)
        {
            _rightScrollAdapter?.Paint(g, (graphics, index, bounds) =>
            {
                UiRect button = new UiRect(bounds.X + 3, bounds.Y + 2, bounds.Width - 7, bounds.Height - 3);
                UiMenuTheme.PaintButton(graphics, button, DiscipleStatusNames[index],
                    pet.petStatus == DiscipleStatusIds[index],
                    _discipleFocusArea == 1 && _selectedDiscipleRow == index);
            });
        }
    }
}
