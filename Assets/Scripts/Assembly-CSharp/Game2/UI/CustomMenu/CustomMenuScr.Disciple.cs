using System;
using Game2.Assets.src.g;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        // The visual order follows the reference; these are the original Panel status IDs.
        private static readonly sbyte[] DiscipleStatusIds = { 0, 2, 1, 3, 4, 5 };
        private static readonly string[] DiscipleStatusNames =
            { "Đi theo", "Tấn công", "Bảo vệ", "Về nhà", "Hợp thể", "Hợp thể vĩnh viễn" };
        private const long DiscipleInfoRefreshIntervalMs = 3000L;
        private long _lastDiscipleInfoRefresh;

        private void ConfigureDiscipleTabBars()
        {
            _discipleLeftTabBar.ButtonStyle = UiMenuTheme.ButtonStyle;
            _discipleLeftTabBar.TabClicked = index => SelectDiscipleLeftTab(index);
            _discipleLeftTabBar.Configure(new UiRect(_inventoryLeftTab0Rect.X, _inventoryLeftTab0Rect.Y,
                _inventoryLeftTab1Rect.Right - _inventoryLeftTab0Rect.X, _inventoryLeftTab0Rect.Height),
                new string[] { "Hành trang", "Thông tin" }, UiTabOrientation.Horizontal, _discipleLeftTab, 4);

            _discipleRightTabBar.ButtonStyle = UiMenuTheme.ButtonStyle;
            _discipleRightTabBar.TabClicked = index => SelectDiscipleRightTab(index);
            _discipleRightTabBar.Configure(new UiRect(_inventoryBagTab0Rect.X, _inventoryBagTab0Rect.Y,
                _inventoryBagTab1Rect.Right - _inventoryBagTab0Rect.X, _inventoryBagTab0Rect.Height),
                new string[] { "Kỹ năng", "Trạng thái" }, UiTabOrientation.Horizontal, _discipleRightTab, 4);
        }

        private void EnterDiscipleTab()
        {
            _discipleFocusArea = 0;
            _selectedDiscipleRow = 0;
            _lastDiscipleInfoRefresh = mSystem.currentTimeMillis();
            if (Char.myCharz() != null && Char.myCharz().havePet) Service.gI().petInfo();
        }

        private void RefreshDiscipleInfo()
        {
            Char me = Char.myCharz();
            if (me == null || !me.havePet) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastDiscipleInfoRefresh < DiscipleInfoRefreshIntervalMs) return;

            _lastDiscipleInfoRefresh = now;
            Service.gI().petInfo();
        }

        private Char GetDisciple()
        {
            return Char.myCharz() != null && Char.myCharz().havePet ? Char.myPetz() : null;
        }

        private int GetDiscipleEquipmentCount()
        {
            Char pet = GetDisciple();
            return pet != null && pet.arrItemBody != null ? pet.arrItemBody.Length : 0;
        }

        private int GetDiscipleSkillRowCount()
        {
            Char pet = GetDisciple();
            return pet != null && pet.arrPetSkill != null ? pet.arrPetSkill.Length + 5 : 5;
        }

        private int GetDiscipleStatusCount()
        {
            Char me = Char.myCharz();
            return me != null && me.cgender == 1 ? 6 : 5;
        }

        private void SelectDiscipleLeftTab(int index)
        {
            _discipleLeftTab = index;
            _discipleFocusArea = 0;
            _selectedDiscipleEquipmentSlot = -1;
            _leftScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void SelectDiscipleRightTab(int index)
        {
            _discipleRightTab = index;
            _discipleFocusArea = 1;
            _selectedDiscipleRow = 0;
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private bool HandleDisciplePointerInput()
        {
            if (_discipleLeftTabBar.UpdateInput(_inputContext)) return true;
            if (_discipleRightTabBar.UpdateInput(_inputContext)) return true;
            if (GetDisciple() == null) return false;

            if (_discipleLeftTab == 0 && _leftScrollAdapter != null
                && _leftScrollAdapter.UpdateKey(_inputContext, out int equipmentSlot))
            {
                if (equipmentSlot >= 0)
                {
                    _discipleFocusArea = 0;
                    _selectedDiscipleEquipmentSlot = equipmentSlot;
                    ActivateDiscipleEquipment(equipmentSlot);
                }
                return true;
            }
            if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out int row))
            {
                if (row >= 0)
                {
                    _discipleFocusArea = 1;
                    _selectedDiscipleRow = row;
                    ActivateDiscipleRightRow(row);
                }
                return true;
            }
            return false;
        }

        private void MoveDiscipleHorizontalFocus(int direction)
        {
            if (direction < 0)
            {
                if (_discipleFocusArea == 1) _discipleFocusArea = 0;
                else _keyboardFocus = KeyboardFocusMainTabs;
            }
            else if (_discipleFocusArea == 0) _discipleFocusArea = 1;
            else SelectDiscipleRightTab((_discipleRightTab + 1) % 2);
            SoundMn.gI().panelClick();
        }

        private void MoveDiscipleVerticalSelection(int direction)
        {
            if (_discipleFocusArea == 0)
            {
                if (_discipleLeftTab == 0 && GetDiscipleEquipmentCount() > 0)
                {
                    if (direction < 0 && _selectedDiscipleEquipmentSlot <= 0)
                    {
                        SelectDiscipleLeftTab(1);
                        return;
                    }
                    _selectedDiscipleEquipmentSlot = _selectedDiscipleEquipmentSlot < 0 ? 0
                        : (_selectedDiscipleEquipmentSlot + direction + GetDiscipleEquipmentCount())
                            % GetDiscipleEquipmentCount();
                    _leftScrollAdapter?.ScrollToIndex(_selectedDiscipleEquipmentSlot);
                }
                else SelectDiscipleLeftTab((_discipleLeftTab + direction + 2) % 2);
            }
            else
            {
                int count = _discipleRightTab == 0 ? GetDiscipleSkillRowCount() : GetDiscipleStatusCount();
                if (count > 0)
                {
                    _selectedDiscipleRow = (_selectedDiscipleRow + direction + count) % count;
                    _rightScrollAdapter?.ScrollToIndex(_selectedDiscipleRow);
                }
            }
            SoundMn.gI().panelClick();
        }

        private void HandleDiscipleConfirm()
        {
            if (_discipleFocusArea == 0)
            {
                if (_discipleLeftTab == 0 && _selectedDiscipleEquipmentSlot >= 0)
                    ActivateDiscipleEquipment(_selectedDiscipleEquipmentSlot);
                else SelectDiscipleLeftTab((_discipleLeftTab + 1) % 2);
            }
            else ActivateDiscipleRightRow(_selectedDiscipleRow);
        }

        private void ActivateDiscipleEquipment(int slot)
        {
            Char pet = GetDisciple();
            if (pet == null || pet.arrItemBody == null || slot < 0 || slot >= pet.arrItemBody.Length) return;
            Item item = pet.arrItemBody[slot];
            if (item == null || item.template == null) return;
            string details = item.template.name + GetUpgradeSuffix(item);
            if (item.itemOption != null)
            {
                for (int i = 0; i < item.itemOption.Length; i++)
                {
                    ItemOption option = item.itemOption[i];
                    if (option != null && option.optionTemplate != null && option.IsValidOption())
                        details += "\n" + option.getOptionString();
                }
            }
            GameCanvas.startYesNoDlg(details + "\nCất vào hành trang?",
                new Command(mResources.YES, () => { Service.gI().getItem(7, (sbyte)slot); GameCanvas.endDlg(); }),
                new Command(mResources.NO, () => GameCanvas.endDlg()));
        }

        private void ActivateDiscipleRightRow(int row)
        {
            Char pet = GetDisciple();
            if (pet == null) return;
            if (_discipleRightTab == 1)
            {
                if (row < 0 || row >= GetDiscipleStatusCount()) return;
                sbyte status = DiscipleStatusIds[row];
                if (status == 5)
                    GameCanvas.startYesNoDlg(mResources.sure_fusion,
                        new Command(mResources.YES, 888351), new Command(mResources.NO, 2001));
                else
                {
                    Service.gI().petStatus(status);
                    if (status < 4) pet.petStatus = status;
                }
                SoundMn.gI().panelClick();
                return;
            }

            int potentialType = row;
            if (potentialType < 0 || potentialType >= 5) return;
            long cost = GetDisciplePotentialCost(pet, potentialType);
            if (pet.cTiemNang < cost)
            {
                GameCanvas.startOKDlg(mResources.not_enough_potential_point1
                    + NinjaUtil.getMoneys(pet.cTiemNang) + mResources.not_enough_potential_point2
                    + NinjaUtil.getMoneys(cost), isError: false);
                return;
            }
            string stat = PotentialNames[potentialType];
            GameCanvas.startYesNoDlg("Tăng " + stat + " cho đệ tử?\nTiềm năng: " + NinjaUtil.getMoneys(cost),
                new Command(mResources.YES, () => { Service.gI().upPotential(true, potentialType, 1); GameCanvas.endDlg(); }),
                new Command(mResources.NO, () => GameCanvas.endDlg()));
        }

        private static long GetDisciplePotentialCost(Char pet, int type)
        {
            if (type == 0) return pet.cHPGoc + 1000L;
            if (type == 1) return pet.cMPGoc + 1000L;
            if (type == 2) return pet.cDamGoc * 100L;
            if (type == 3) return (pet.cDefGoc + 5L) * 100000L;
            if (Panel.t_tiemnang == null || Panel.t_tiemnang.Length == 0) return long.MaxValue;
            return Panel.t_tiemnang[System.Math.Max(0,
                System.Math.Min(pet.cCriticalGoc, Panel.t_tiemnang.Length - 1))];
        }
    }
}
