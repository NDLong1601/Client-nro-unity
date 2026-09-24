using System;
using Game1.UI.Components;
using Nro.UI;

namespace Game1.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private const int CollectionMainTab = 7;
        private const int CollectionCostumes = 0;
        private const int CollectionCards = 1;
        private const int CollectionFish = 2;
        private const int CollectionAchievements = 3;
        private const int CollectionFocusCategory = 0;
        private const int CollectionFocusList = 1;
        private const int CollectionFocusDetail = 2;
        private const int CollectionFishFirstId = 2124;
        private const int CollectionFishLastId = 2138;
        private const int CollectionGridColumns = 4;
        private const int CollectionGridRowHeight = 36;
        private const int CollectionAchievementRowHeight = 39;

        private static readonly string[] CollectionCategoryNames =
            { "Trang phục", "Sổ sưu tầm", "Sổ tay cá", "Thành tựu" };

        private int _collectionCategory;
        private int _collectionFocusArea = CollectionFocusCategory;
        private readonly int[] _collectionSelected = new int[4];
        private MyVector _collectionCards;
        private MyVector _collectionFish;
        private bool _collectionAwaitingCards;
        private bool _collectionAwaitingFish;
        private long _collectionRequestUtcTicks;
        private int _collectionClaimingId = -1;
        private long _collectionClaimUtcTicks;
        private UiRect _collectionMenuHeaderRect;
        private UiRect _collectionHeaderRect;
        private UiRect _collectionListRect;
        private UiRect _collectionDetailRect;
        private UiRect _collectionInfoRect;
        private UiRect _collectionClaimRect;
        private readonly UiRect[] _collectionCategoryRects = new UiRect[4];

        private void ConfigureCollectionRects()
        {
            int x = _contentRect.X + 4;
            int y = _contentRect.Y + 4;
            int menuW = System.Math.Min(94, System.Math.Max(76, _contentRect.Width / 4));
            int gap = 8;
            int bodyX = x + menuW + gap;
            int bodyW = System.Math.Max(90, _contentRect.Right - bodyX - 4);
            int listW = System.Math.Max(78, bodyW * 55 / 100);
            int detailW = bodyW - listW - 3;
            if (detailW < 64)
            {
                detailW = 64;
                listW = bodyW - detailW - 3;
            }
            _collectionMenuHeaderRect = new UiRect(x, y, menuW, 22);
            _collectionHeaderRect = new UiRect(bodyX, y, bodyW, 22);
            for (int i = 0; i < _collectionCategoryRects.Length; i++)
                _collectionCategoryRects[i] = new UiRect(x, y + 26 + i * 29, menuW, 27);
            int bodyY = y + 26;
            int bodyH = _contentRect.Bottom - bodyY - 3;
            _collectionListRect = new UiRect(bodyX, bodyY, listW, bodyH);
            _collectionDetailRect = new UiRect(bodyX + listW + 3, bodyY, detailW, bodyH);
            _collectionInfoRect = new UiRect(_collectionDetailRect.X + 4,
                _collectionDetailRect.Y + 91, _collectionDetailRect.Width - 8,
                System.Math.Max(10, _collectionDetailRect.Height
                    - (_collectionCategory == CollectionAchievements ? 123 : 96)));
            _collectionClaimRect = new UiRect(_collectionDetailRect.X + 8,
                _collectionDetailRect.Bottom - 27, _collectionDetailRect.Width - 16, 23);
        }

        private void EnterCollectionTab()
        {
            _collectionFocusArea = CollectionFocusCategory;
            bool pending = (_collectionAwaitingCards || _collectionAwaitingFish)
                && DateTime.UtcNow.Ticks - _collectionRequestUtcTicks < TimeSpan.TicksPerSecond * 20;
            if (!pending)
            {
                _collectionCards = null;
                _collectionFish = null;
                _collectionAwaitingCards = true;
                _collectionAwaitingFish = true;
                _collectionRequestUtcTicks = DateTime.UtcNow.Ticks;
                _collectionClaimingId = -1;
                _collectionClaimUtcTicks = 0;
                CostumeCollectionScr.gI().requestData();
                Service.gI().SendRada(0, -1);
                Service.gI().SendRada(42, -1);
            }
            ConfigureScrollAdapters();
        }

        private void SwitchToCollectionTab()
        {
            _selectedMainTab = CollectionMainTab;
            _mainTabScrollAdapter.ScrollToIndex(CollectionMainTab);
            _keyboardFocus = KeyboardFocusContent;
            _leftScrollAdapter?.Reset();
            _rightScrollAdapter?.Reset();
            EnterCollectionTab();
            SoundMn.gI().panelClick();
        }

        // The two existing command-127 catalogs have no subtype in their envelope.
        // Fish cards use the dedicated fishing item ID range; radar cards do not.
        public static bool ReceiveCollectionRadar(MyVector cards)
        {
            if (_instance == null || cards == null) return false;
            if (DateTime.UtcNow.Ticks - _instance._collectionRequestUtcTicks > TimeSpan.TicksPerSecond * 20)
            {
                _instance._collectionAwaitingCards = false;
                _instance._collectionAwaitingFish = false;
                return false;
            }
            bool fish = cards.size() > 0;
            for (int i = 0; i < cards.size() && fish; i++)
            {
                Info_RadaScr card = cards.elementAt(i) as Info_RadaScr;
                fish = card != null && card.id >= CollectionFishFirstId && card.id <= CollectionFishLastId;
            }
            if (fish && _instance._collectionAwaitingFish)
            {
                _instance._collectionFish = cards;
                _instance._collectionAwaitingFish = false;
            }
            else if (!fish && _instance._collectionAwaitingCards)
            {
                _instance._collectionCards = cards;
                _instance._collectionAwaitingCards = false;
            }
            else return false;

            if (_isOpen && _instance._selectedMainTab == CollectionMainTab)
                _instance.ConfigureScrollAdapters();
            return true;
        }

        public static void OnCollectionAchievementsReceived()
        {
            if (_instance == null) return;
            _instance._collectionClaimingId = -1;
            _instance._collectionClaimUtcTicks = 0;
            if (_isOpen && _instance._selectedMainTab == CollectionMainTab)
                _instance.ConfigureScrollAdapters();
        }

        private int GetCollectionItemCount()
        {
            switch (_collectionCategory)
            {
                case CollectionCostumes: return CostumeCollectionScr.gI().Entries.size();
                case CollectionCards: return _collectionCards != null ? _collectionCards.size() : 0;
                case CollectionFish: return _collectionFish != null ? _collectionFish.size() : 0;
                default: return CostumeCollectionScr.gI().Achievements.size();
            }
        }

        private object GetCollectionSelectedItem()
        {
            MyVector entries;
            switch (_collectionCategory)
            {
                case CollectionCostumes: entries = CostumeCollectionScr.gI().Entries; break;
                case CollectionCards: entries = _collectionCards; break;
                case CollectionFish: entries = _collectionFish; break;
                default: entries = CostumeCollectionScr.gI().Achievements; break;
            }
            int index = _collectionSelected[_collectionCategory];
            return entries != null && index >= 0 && index < entries.size() ? entries.elementAt(index) : null;
        }

        private void ConfigureCollectionScrollAdapters()
        {
            int count = GetCollectionItemCount();
            int rows = _collectionCategory == CollectionAchievements
                ? count : (count + CollectionGridColumns - 1) / CollectionGridColumns;
            _leftScrollAdapter.Configure(_collectionListRect, rows,
                _collectionCategory == CollectionAchievements ? CollectionAchievementRowHeight : CollectionGridRowHeight);
            _rightScrollAdapter.Configure(_collectionInfoRect, GetCollectionInfoHeight(), 1);
        }

        private void UpdateCollectionScrollAdapters()
        {
            if (DateTime.UtcNow.Ticks - _collectionRequestUtcTicks > TimeSpan.TicksPerSecond * 20)
            {
                _collectionAwaitingCards = false;
                _collectionAwaitingFish = false;
            }
            if (_collectionClaimingId >= 0 && DateTime.UtcNow.Ticks - _collectionClaimUtcTicks
                > TimeSpan.TicksPerSecond * 10)
                _collectionClaimingId = -1;
            int count = GetCollectionItemCount();
            int rows = _collectionCategory == CollectionAchievements
                ? count : (count + CollectionGridColumns - 1) / CollectionGridColumns;
            if (_collectionSelected[_collectionCategory] >= count)
                _collectionSelected[_collectionCategory] = System.Math.Max(0, count - 1);
            if (_leftScrollAdapter == null || _rightScrollAdapter == null) return;
            if (_leftScrollAdapter.ItemCount != rows || _rightScrollAdapter.ItemCount != GetCollectionInfoHeight())
                ConfigureScrollAdapters();
        }

        private void SelectCollectionCategory(int category)
        {
            if (category < 0 || category >= CollectionCategoryNames.Length) return;
            _collectionCategory = category;
            _collectionFocusArea = CollectionFocusCategory;
            _keyboardFocus = KeyboardFocusContent;
            ConfigureCollectionRects();
            _leftScrollAdapter?.Reset();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void SelectCollectionItem(int index)
        {
            if (index < 0 || index >= GetCollectionItemCount()) return;
            _collectionSelected[_collectionCategory] = index;
            _collectionFocusArea = CollectionFocusList;
            _keyboardFocus = KeyboardFocusContent;
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private bool HandleCollectionPointerInput()
        {
            bool leftWasDragging = _leftScrollAdapter != null && _leftScrollAdapter.IsDragging;
            if (_leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int clickedRow))
            {
                int index = GetCollectionPointerIndex(clickedRow, GameCanvas.px, GameCanvas.py);
                if (index >= 0) SelectCollectionItem(index);
                return true;
            }
            if (leftWasDragging) return true;

            bool rightWasDragging = _rightScrollAdapter != null && _rightScrollAdapter.IsDragging;
            if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out _))
                return true;
            if (rightWasDragging) return true;

            if (GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _collectionCategoryRects.Length; i++)
                {
                    UiRect rect = _collectionCategoryRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.clearAllPointerEvent();
                    SelectCollectionCategory(i);
                    return true;
                }
                if (_collectionCategory == CollectionAchievements && CanClaimCollectionAchievement()
                    && GameCanvas.isPointer(_collectionClaimRect.X, _collectionClaimRect.Y,
                        _collectionClaimRect.Width, _collectionClaimRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    ClaimCollectionAchievement();
                    return true;
                }
            }
            return false;
        }

        private int GetCollectionPointerIndex(int row, int x, int y)
        {
            if (!_collectionListRect.Contains(x, y)) return -1;
            int index = _collectionCategory == CollectionAchievements ? row
                : row * CollectionGridColumns + System.Math.Min(CollectionGridColumns - 1,
                    System.Math.Max(0, (x - _collectionListRect.X - 3) * CollectionGridColumns
                        / System.Math.Max(1, _collectionListRect.Width - 6)));
            return index >= 0 && index < GetCollectionItemCount() ? index : -1;
        }

        private void MoveCollectionHorizontalFocus(int direction)
        {
            if (_collectionFocusArea == CollectionFocusList
                && _collectionCategory != CollectionAchievements && GetCollectionItemCount() > 0)
            {
                int selected = _collectionSelected[_collectionCategory];
                int rowStart = selected / CollectionGridColumns * CollectionGridColumns;
                int rowEnd = System.Math.Min(GetCollectionItemCount() - 1,
                    rowStart + CollectionGridColumns - 1);
                int next = selected + direction;
                if (next >= rowStart && next <= rowEnd)
                {
                    SelectCollectionItem(next);
                    return;
                }
            }

            if (direction < 0)
            {
                if (_collectionFocusArea == CollectionFocusCategory) _keyboardFocus = KeyboardFocusMainTabs;
                else _collectionFocusArea--;
            }
            else if (_collectionFocusArea < CollectionFocusDetail) _collectionFocusArea++;
            SoundMn.gI().panelClick();
        }

        private void MoveCollectionVerticalSelection(int direction)
        {
            if (_collectionFocusArea == CollectionFocusCategory)
            {
                SelectCollectionCategory((_collectionCategory + direction + CollectionCategoryNames.Length)
                    % CollectionCategoryNames.Length);
                return;
            }
            if (_collectionFocusArea == CollectionFocusDetail)
            {
                _rightScrollAdapter?.ScrollByWheel(direction < 0 ? 1 : -1);
                return;
            }
            int count = GetCollectionItemCount();
            if (count == 0) return;
            int step = _collectionCategory == CollectionAchievements ? 1 : CollectionGridColumns;
            int next = System.Math.Max(0, System.Math.Min(count - 1, _collectionSelected[_collectionCategory] + direction * step));
            SelectCollectionItem(next);
            _leftScrollAdapter?.ScrollToIndex(_collectionCategory == CollectionAchievements
                ? next : next / CollectionGridColumns);
        }

        private void HandleCollectionConfirm()
        {
            if (_collectionFocusArea == CollectionFocusCategory)
                _collectionFocusArea = CollectionFocusList;
            else if (_collectionFocusArea == CollectionFocusList)
                _collectionFocusArea = CollectionFocusDetail;
            else if (CanClaimCollectionAchievement())
                ClaimCollectionAchievement();
            SoundMn.gI().panelClick();
        }

        private bool CanClaimCollectionAchievement()
        {
            CostumeCollectionAchievement achievement = GetCollectionSelectedItem() as CostumeCollectionAchievement;
            return _collectionCategory == CollectionAchievements && achievement != null
                && achievement.completed && !achievement.claimed && _collectionClaimingId < 0;
        }

        private void ClaimCollectionAchievement()
        {
            if (!CanClaimCollectionAchievement()) return;
            CostumeCollectionAchievement achievement = (CostumeCollectionAchievement)GetCollectionSelectedItem();
            _collectionClaimingId = achievement.id;
            _collectionClaimUtcTicks = DateTime.UtcNow.Ticks;
            Service.gI().SendRada(46, achievement.id);
        }

        private static Info_RadaScr FindCollectionRadarCard(int id)
        {
            if (_instance == null) return null;
            Info_RadaScr card = Info_RadaScr.GetInfo(_instance._collectionCards, id);
            return card ?? Info_RadaScr.GetInfo(_instance._collectionFish, id);
        }

        public static void OnCollectionRadarUse(int id, sbyte use)
        {
            FindCollectionRadarCard(id)?.SetUse(use);
        }

        public static void OnCollectionRadarLevel(int id, sbyte level)
        {
            FindCollectionRadarCard(id)?.SetLevel(level);
        }

        public static void OnCollectionRadarAmount(int id, sbyte amount, sbyte maxAmount)
        {
            FindCollectionRadarCard(id)?.SetAmount(amount, maxAmount);
        }
    }
}
