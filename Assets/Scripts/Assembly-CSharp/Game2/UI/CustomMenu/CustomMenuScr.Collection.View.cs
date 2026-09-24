using System;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private static Image _collectionDragonBallLogo;
        private static Image _collectionOnePieceLogo;
        private static Image _collectionDemonSlayerLogo;
        private static Image _collectionPreviewBackground;
        private static bool _collectionPreviewBackgroundLoaded;
        private static FrameImage _collectionCardFrames;
        private static Image[] _collectionRankImages;
        private static Image _collectionProgressTrack;
        private static Image _collectionProgressFill;

        private void PaintCollectionTabContent(mGraphics g)
        {
            UiMenuTheme.PaintHeader(g, _collectionMenuHeaderRect, "Sưu tầm");
            UiMenuTheme.PaintHeader(g, _collectionHeaderRect,
                _collectionCategory == CollectionAchievements ? "Thành tựu sưu tập" : "Bộ sưu tập");
            g.setColor(0xB49A7C);
            g.drawLine(_collectionMenuHeaderRect.Right + 4, _contentRect.Y + 5,
                _collectionMenuHeaderRect.Right + 4, _contentRect.Bottom - 4);
            for (int i = 0; i < _collectionCategoryRects.Length; i++)
                UiMenuTheme.PaintButton(g, _collectionCategoryRects[i], CollectionCategoryNames[i],
                    i == _collectionCategory,
                    _keyboardFocus == KeyboardFocusContent && _collectionFocusArea == CollectionFocusCategory
                        && i == _collectionCategory);

            UiMenuTheme.PaintSurface(g, _collectionListRect, 0xDECCB5);
            UiMenuTheme.PaintSurface(g, _collectionDetailRect, 0xF1EBE0);
            if (GetCollectionItemCount() == 0)
            {
                string message = IsCollectionLoading() ? "Đang tải dữ liệu..." : "Chưa có dữ liệu";
                mFont.tahoma_7_grey.drawString(g, message,
                    _collectionListRect.X + _collectionListRect.Width / 2,
                    _collectionListRect.Y + 12, mFont.CENTER);
            }
            else if (_collectionCategory == CollectionAchievements)
                _leftScrollAdapter?.Paint(g, PaintCollectionAchievementRow);
            else
                _leftScrollAdapter?.Paint(g, PaintCollectionGridRow);
            _leftScrollAdapter?.PaintScrollbar(g, _collectionListRect);
            PaintCollectionDetail(g);
        }

        private bool IsCollectionLoading()
        {
            if (_collectionCategory == CollectionCostumes || _collectionCategory == CollectionAchievements)
                return CostumeCollectionScr.gI().IsLoading;
            return _collectionCategory == CollectionCards ? _collectionAwaitingCards : _collectionAwaitingFish;
        }

        private void PaintCollectionGridRow(mGraphics g, int row, UiRect bounds)
        {
            int cellW = System.Math.Max(12, (bounds.Width - 6) / CollectionGridColumns);
            for (int column = 0; column < CollectionGridColumns; column++)
            {
                int index = row * CollectionGridColumns + column;
                if (index >= GetCollectionItemCount()) break;
                object item = GetCollectionItem(index);
                bool owned = IsCollectionOwned(item);
                bool selected = index == _collectionSelected[_collectionCategory];
                UiRect cell = new UiRect(bounds.X + 3 + column * cellW, bounds.Y + 2,
                    cellW - 2, CollectionGridRowHeight - 3);
                Info_RadaScr radarCard = item as Info_RadaScr;
                if (radarCard != null && PaintCollectionRadarGridCell(g, cell, radarCard, owned, selected))
                    continue;
                g.setColor(selected ? 0xFFC83C : (owned ? 0xD8AF69 : 0xB8A58C));
                g.fillRect(cell.X, cell.Y, cell.Width, cell.Height, 4);
                g.setColor(owned ? 0xF7F2E9 : 0xB6A58E);
                g.fillRect(cell.X + 1, cell.Y + 1, cell.Width - 2, cell.Height - 2, 3);
                using (UiRenderState.Push(g, cell, clip: true))
                {
                    int iconId = GetCollectionIconId(item);
                    if (iconId >= 0)
                    {
                        if (owned)
                            SmallImage.drawSmallImage(g, iconId, cell.X + cell.Width / 2,
                                cell.Y + cell.Height / 2, 0, mGraphics.HCENTER | mGraphics.VCENTER);
                        else
                            SmallImage.drawSmallImageGray(g, iconId, cell.X + cell.Width / 2,
                                cell.Y + cell.Height / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                    }
                }
                if (!owned) PaintCollectionLock(g, cell.Right - 7, cell.Bottom - 7);
                if (selected)
                {
                    g.setColor(0xA76713);
                    g.drawRect(cell.X, cell.Y, cell.Width, cell.Height);
                }
            }
        }

        private static void EnsureCollectionRadarArt()
        {
            if (_collectionCardFrames == null)
                _collectionCardFrames = new FrameImage(GameCanvas.loadImage("/radar/17.png"), 28, 28);
            if (_collectionProgressTrack == null)
                _collectionProgressTrack = GameCanvas.loadImage("/radar/22.png");
            if (_collectionProgressFill == null)
                _collectionProgressFill = GameCanvas.loadImage("/radar/19.png");
            if (_collectionRankImages != null) return;
            _collectionRankImages = new Image[7];
            for (int i = 0; i < _collectionRankImages.Length; i++)
                _collectionRankImages[i] = GameCanvas.loadImage("/radar/" + (i + 7) + ".png");
        }

        private bool PaintCollectionRadarGridCell(mGraphics g, UiRect cell, Info_RadaScr card,
            bool owned, bool selected)
        {
            EnsureCollectionRadarArt();
            if (card.rank < 0 || card.rank >= 7 || _collectionCardFrames.imgFrame == null)
                return false;
            int border = selected ? 2 : 1;
            UiRect face = cell.Inset(border, border);
            g.setColor(selected ? 0xFFE363 : owned ? 0xBDA47C : 0x8F8173);
            g.fillRect(cell.X, cell.Y, cell.Width, cell.Height, 4);
            using (UiRenderState.Push(g, cell, clip: true))
            {
                g.drawRegionScaleInClip(_collectionCardFrames.imgFrame,
                    0, card.rank * 28, 28, 28,
                    face.X, face.Y, face.Width, face.Height);
                if (!owned)
                    g.fillRect(face.X, face.Y, face.Width, face.Height, 0, 55);
                if (card.idIcon >= 0)
                {
                    if (owned)
                        SmallImage.drawSmallImage(g, card.idIcon,
                            cell.X + cell.Width / 2, cell.Y + cell.Height / 2,
                            0, mGraphics.HCENTER | mGraphics.VCENTER);
                    else
                        SmallImage.drawSmallImageGray(g, card.idIcon,
                            cell.X + cell.Width / 2, cell.Y + cell.Height / 2,
                            mGraphics.HCENTER | mGraphics.VCENTER);
                }
            }
            if (!owned) PaintCollectionLock(g, cell.Right - 7, cell.Bottom - 7);
            return true;
        }

        private object GetCollectionItem(int index)
        {
            MyVector entries = _collectionCategory == CollectionCostumes ? CostumeCollectionScr.gI().Entries
                : _collectionCategory == CollectionCards ? _collectionCards : _collectionFish;
            return entries != null && index >= 0 && index < entries.size() ? entries.elementAt(index) : null;
        }

        private static bool IsCollectionOwned(object item)
        {
            CostumeCollectionEntry costume = item as CostumeCollectionEntry;
            if (costume != null) return costume.unlocked;
            Info_RadaScr card = item as Info_RadaScr;
            return card != null && card.level > 0;
        }

        private static int GetCollectionIconId(object item)
        {
            CostumeCollectionEntry costume = item as CostumeCollectionEntry;
            if (costume != null) return costume.iconId;
            Info_RadaScr card = item as Info_RadaScr;
            return card != null ? card.idIcon : -1;
        }

        private void PaintCollectionLock(mGraphics g, int centerX, int centerY)
        {
            Image icon = _statusIcons != null && _statusIcons.Length > 1 ? _statusIcons[1] : null;
            if (icon != null)
                g.drawImageScaleInClip(icon, centerX - 6, centerY - 6, 12, 12);
            else
            {
                g.setColor(0x6F4A1F);
                g.fillRect(centerX - 4, centerY - 2, 8, 7, 2);
                g.drawRect(centerX - 3, centerY - 5, 6, 5);
            }
        }

        private void PaintCollectionAchievementRow(mGraphics g, int row, UiRect bounds)
        {
            MyVector achievements = CostumeCollectionScr.gI().Achievements;
            if (row < 0 || row >= achievements.size()) return;
            CostumeCollectionAchievement achievement = achievements.elementAt(row) as CostumeCollectionAchievement;
            if (achievement == null) return;
            bool selected = row == _collectionSelected[CollectionAchievements];
            bool claimable = achievement.completed && !achievement.claimed;
            UiRect card = new UiRect(bounds.X + 3, bounds.Y + 2, bounds.Width - 7, bounds.Height - 3);
            UiMenuTheme.PaintCard(g, card,
                selected ? 0xFFCF42 : (achievement.claimed ? 0x2C9827 : 0xFFFDF9),
                selected ? 0xC78816 : 0xAA9880);
            mFont titleFont = achievement.claimed ? mFont.tahoma_7b_white : mFont.tahoma_7b_dark;
            string progress = achievement.progress + "/" + achievement.target;
            int titleWidth = System.Math.Max(15, card.Width - titleFont.getWidth(progress) - 14);
            titleFont.drawString(g, TruncateString(titleFont, achievement.name, titleWidth),
                card.X + 6, card.Y + 6, mFont.LEFT);
            titleFont.drawString(g, progress, card.Right - 5, card.Y + 6, mFont.RIGHT);
            mFont statusFont = achievement.claimed ? mFont.tahoma_7b_white
                : claimable ? mFont.tahoma_7_green : mFont.tahoma_7_grey;
            string status = achievement.claimed ? "Đã nhận thưởng"
                : claimable ? "Đã hoàn thành - Có thể nhận thưởng" : "Chưa hoàn thành";
            statusFont.drawString(g, TruncateString(statusFont, status, card.Width - 12),
                card.X + 6, card.Y + 20, mFont.LEFT);
        }

        private void PaintCollectionDetail(mGraphics g)
        {
            object selected = GetCollectionSelectedItem();
            if (selected == null)
            {
                mFont.tahoma_7_grey.drawString(g,
                    IsCollectionLoading() ? "Đang tải..." : "Chọn một mục",
                    _collectionDetailRect.X + _collectionDetailRect.Width / 2,
                    _collectionDetailRect.Y + 18, mFont.CENTER);
                return;
            }
            UiRect preview = new UiRect(_collectionDetailRect.X + 4, _collectionDetailRect.Y + 4,
                _collectionDetailRect.Width - 8, 69);
            bool illustratedPreview = _collectionCategory != CollectionAchievements;
            if (illustratedPreview)
            {
                g.setColor(0xB39B7A);
                g.fillRect(preview.X, preview.Y, preview.Width, preview.Height, 4);
                g.setColor(0x125326);
                g.fillRect(preview.X + 1, preview.Y + 1,
                    preview.Width - 2, preview.Height - 2, 3);
            }
            else
            {
                g.setColor(0xF1EBE0);
                g.fillRect(preview.X, preview.Y, preview.Width, preview.Height, 4);
                g.setColor(0xB39B7A);
                g.drawRect(preview.X, preview.Y, preview.Width, preview.Height);
            }
            using (UiRenderState.Push(g, preview, clip: true))
            {
                if (illustratedPreview)
                    PaintCollectionPreviewBackground(g, preview);
                if (_collectionCategory == CollectionAchievements)
                    PaintCollectionAchievementLogo(g, preview,
                        (CostumeCollectionAchievement)selected);
                else if (_collectionCategory == CollectionCostumes)
                    PaintCollectionCostumePreview(g, preview, (CostumeCollectionEntry)selected);
                else
                    PaintCollectionRadarPreview(g, preview, (Info_RadaScr)selected);
            }
            string name = _collectionCategory == CollectionAchievements
                ? ((CostumeCollectionAchievement)selected).name
                : _collectionCategory == CollectionCostumes
                    ? ((CostumeCollectionEntry)selected).name : ((Info_RadaScr)selected).name;
            UiRect nameRect = new UiRect(preview.X, preview.Bottom + 1, preview.Width, 15);
            g.setColor(0xFFCC36);
            g.fillRect(nameRect.X, nameRect.Y, nameRect.Width, nameRect.Height, 4);
            mFont.tahoma_7b_dark.drawString(g,
                TruncateString(mFont.tahoma_7b_dark, name ?? string.Empty, nameRect.Width - 6),
                nameRect.X + nameRect.Width / 2, nameRect.Y + 3, mFont.CENTER);

            using (UiRenderState.Push(g, _collectionInfoRect, clip: true))
            {
                g.translate(0, -(_rightScrollAdapter?.ScrollY ?? 0));
                RenderCollectionInfo(g);
            }
            _rightScrollAdapter?.PaintScrollbar(g, _collectionInfoRect);
            if (_collectionCategory == CollectionAchievements)
            {
                CostumeCollectionAchievement achievement = (CostumeCollectionAchievement)selected;
                UiMenuTheme.PaintButton(g, _collectionClaimRect,
                    achievement.claimed ? "Đã nhận" : _collectionClaimingId >= 0 ? "Đang nhận..."
                        : achievement.completed ? "Nhận thưởng" : "Chưa hoàn thành",
                    CanClaimCollectionAchievement(),
                    _collectionFocusArea == CollectionFocusDetail && CanClaimCollectionAchievement());
            }
        }

        private void PaintCollectionCostumePreview(mGraphics g, UiRect rect, CostumeCollectionEntry entry)
        {
            Char me = Char.myCharz();
            int head = entry.headPart >= 0 ? entry.headPart : me != null ? me.head : -1;
            int body = entry.bodyPart >= 0 ? entry.bodyPart : me != null ? me.body : -1;
            int leg = entry.legPart >= 0 ? entry.legPart : me != null ? me.leg : -1;
            bool valid = (entry.headPart >= 0 || entry.bodyPart >= 0 || entry.legPart >= 0)
                && me != null && GameScr.parts != null && head >= 0 && body >= 0 && leg >= 0
                && head < GameScr.parts.Length && body < GameScr.parts.Length && leg < GameScr.parts.Length
                && GameScr.parts[head] != null && GameScr.parts[body] != null && GameScr.parts[leg] != null
                && !entry.previewFailed;
            if (valid)
            {
                if (entry.preview == null || entry.preview.head != head
                    || entry.preview.body != body || entry.preview.leg != leg)
                    entry.preview = new Char { head = head, body = body, leg = leg, bag = -1,
                        cgender = me.cgender };
                try
                {
                    entry.preview.paintCharBody(g, rect.X + rect.Width / 2, rect.Bottom - 5,
                        1, GameCanvas.gameTick / 8 % 2, true);
                }
                catch (Exception)
                {
                    entry.previewFailed = true;
                }
            }
            if ((!valid || entry.previewFailed) && entry.iconId >= 0)
                SmallImage.drawSmallImage(g, entry.iconId, rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2, 0, mGraphics.HCENTER | mGraphics.VCENTER);
            if (!entry.unlocked)
            {
                g.fillRect(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2, 0, 55);
                PaintCollectionLock(g, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            }
        }

        private void PaintCollectionPreviewBackground(mGraphics g, UiRect rect)
        {
            if (!_collectionPreviewBackgroundLoaded)
            {
                _collectionPreviewBackground = GameCanvas.loadImage(
                    "/custom_menu/collection_radar_background.png");
                _collectionPreviewBackgroundLoaded = true;
            }
            if (_collectionPreviewBackground != null)
            {
                UiRect inner = rect.Inset(2, 2);
                int imageW = _collectionPreviewBackground.getWidth();
                int imageH = _collectionPreviewBackground.getHeight();
                if (!inner.IsEmpty && imageW > 0 && imageH > 0)
                {
                    int drawW = inner.Width;
                    int drawH = (int)(((long)imageH * drawW + imageW - 1) / imageW);
                    if (drawH < inner.Height)
                    {
                        drawH = inner.Height;
                        drawW = (int)(((long)imageW * drawH + imageH - 1) / imageH);
                    }
                    using (UiRenderState.Push(g, inner, clip: true))
                        g.drawImageScaleInClip(_collectionPreviewBackground,
                            inner.X + (inner.Width - drawW) / 2,
                            inner.Y + (inner.Height - drawH) / 2, drawW, drawH);
                    g.setColor(0x125326);
                    g.fillRect(inner.X, inner.Y, 2, 1);
                    g.fillRect(inner.X, inner.Y + 1, 1, 1);
                    g.fillRect(inner.Right - 2, inner.Y, 2, 1);
                    g.fillRect(inner.Right - 1, inner.Y + 1, 1, 1);
                    g.fillRect(inner.X, inner.Bottom - 1, 2, 1);
                    g.fillRect(inner.X, inner.Bottom - 2, 1, 1);
                    g.fillRect(inner.Right - 2, inner.Bottom - 1, 2, 1);
                    g.fillRect(inner.Right - 1, inner.Bottom - 2, 1, 1);
                }
            }
            else
            {
                for (int x = rect.X + 9; x < rect.Right; x += 11)
                {
                    g.setColor(0x1D6A31);
                    g.drawLine(x, rect.Y, x, rect.Bottom);
                }
                for (int y = rect.Y + 7; y < rect.Bottom; y += 11)
                {
                    g.setColor(0x1D6A31);
                    g.drawLine(rect.X, y, rect.Right, y);
                }
            }
        }

        private void PaintCollectionRadarPreview(mGraphics g, UiRect rect, Info_RadaScr card)
        {
            if (card != null)
            {
                try { card.paintInfo(g, rect.X + rect.Width / 2, rect.Bottom - 8); }
                catch (Exception) { /* Preview data may still be loading. */ }
                if (card.level == 0)
                {
                    g.fillRect(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2, 0, 65);
                    PaintCollectionLock(g, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                }
                EnsureCollectionRadarArt();
                if (_collectionCategory == CollectionCards)
                    PaintCollectionCardProgress(g, rect, card);
                if (card.rank >= 0 && card.rank < _collectionRankImages.Length)
                {
                    Image rankImage = _collectionRankImages[card.rank];
                    if (rankImage != null)
                        g.drawImage(rankImage, rect.Right - rankImage.getWidth() - 4,
                            rect.Y + 4, 0);
                }
            }
        }

        private static void PaintCollectionCardProgress(mGraphics g, UiRect rect, Info_RadaScr card)
        {
            if (_collectionProgressTrack == null || _collectionProgressFill == null) return;
            int barX = rect.X + 6;
            int barY = rect.Y + 4;
            int barW = 7;
            int barH = rect.Height - 8;
            g.drawImageScaleInClip(_collectionProgressTrack, barX, barY, barW, barH);
            if (card.max_amount <= 0) return;
            int amount = System.Math.Max(0, System.Math.Min((int)card.amount, (int)card.max_amount));
            int filledHeight = amount * barH / card.max_amount;
            if (filledHeight <= 0) return;
            UiRect filled = new UiRect(barX, barY + barH - filledHeight, barW, filledHeight);
            using (UiRenderState.Push(g, filled, clip: true))
                g.drawImageScaleInClip(_collectionProgressFill, barX, barY, barW, barH);
        }

        private void PaintCollectionAchievementLogo(mGraphics g, UiRect rect,
            CostumeCollectionAchievement achievement)
        {
            g.setColor(0xF1EBE0);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            string name = achievement.name ?? string.Empty;
            bool onePiece = name.IndexOf("Hải Tặc", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("OnePiece", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("One Piece", StringComparison.OrdinalIgnoreCase) >= 0;
            bool demonSlayer = name.IndexOf("Demon Slayer", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Kimetsu", StringComparison.OrdinalIgnoreCase) >= 0;
            Image logo;
            if (onePiece)
            {
                if (_collectionOnePieceLogo == null)
                    _collectionOnePieceLogo = GameCanvas.LoadImageFromRoot(
                        "/x4/custom_menu/collection_onepiece.png");
                logo = _collectionOnePieceLogo;
            }
            else if (demonSlayer)
            {
                if (_collectionDemonSlayerLogo == null)
                    _collectionDemonSlayerLogo = GameCanvas.LoadImageFromRoot(
                        "/x4/custom_menu/collection_demonslayer.png");
                logo = _collectionDemonSlayerLogo;
            }
            else
            {
                if (_collectionDragonBallLogo == null)
                    _collectionDragonBallLogo = GameCanvas.LoadImageFromRoot(
                        "/x4/custom_menu/collection_dragonball.png");
                logo = _collectionDragonBallLogo;
            }
            if (logo == null || logo.texture == null) return;
            logo.texture.filterMode = UnityEngine.FilterMode.Trilinear;
            if (!onePiece && !demonSlayer)
            {
                g.drawImageScaleInClip(logo, rect.X + 5, rect.Y + 7,
                    rect.Width - 10, rect.Height - 14);
                return;
            }

            int sourceX = 0;
            int sourceY = 0;
            int sourceW = logo.getWidth();
            int sourceH = logo.getHeight();
            if (onePiece)
            {
                // The artwork fills only the left half of the supplied PNG.
                sourceY = sourceH * 210 / 843;
                sourceW = sourceW * 630 / 1264;
                sourceH = sourceH * 420 / 843;
            }
            else if (demonSlayer)
            {
                // Keep the circular mark and remove the broad transparent side margins.
                sourceX = sourceW * 837 / 3840;
                sourceY = sourceH * 61 / 2160;
                sourceW = sourceW * 2166 / 3840;
                sourceH = sourceH * 2038 / 2160;
            }
            if (sourceW <= 0 || sourceH <= 0) return;
            int drawW = rect.Width - 12;
            int drawH = drawW * sourceH / sourceW;
            if (drawH > rect.Height - 4)
            {
                drawH = rect.Height - 4;
                drawW = drawH * sourceW / sourceH;
            }
            g.drawRegionScaleInClip(logo, sourceX, sourceY, sourceW, sourceH,
                rect.X + (rect.Width - drawW) / 2,
                rect.Y + (rect.Height - drawH) / 2, drawW, drawH);
        }

        private int GetCollectionInfoHeight()
        {
            if (_collectionInfoRect.IsEmpty || GetCollectionSelectedItem() == null) return 0;
            return System.Math.Max(1, RenderCollectionInfo(null) - _collectionInfoRect.Y + 4);
        }

        private int RenderCollectionInfo(mGraphics g)
        {
            object selected = GetCollectionSelectedItem();
            if (selected == null) return _collectionInfoRect.Y;
            int x = _collectionInfoRect.X + 1;
            int width = System.Math.Max(20, _collectionInfoRect.Width - 3);
            int y = _collectionInfoRect.Y + 2;
            CostumeCollectionEntry costume = selected as CostumeCollectionEntry;
            if (costume != null)
            {
                y = PaintCollectionText(g, mFont.tahoma_7b_dark,
                    costume.rarity + (costume.unlocked ? " - Đã sở hữu" : " - Chưa sở hữu"), x, y, width);
                y = PaintCollectionText(g, mFont.tahoma_7_blue, costume.description, x, y + 2, width);
                y = PaintCollectionText(g, mFont.tahoma_7_blue,
                    costume.headPart >= 0 && costume.bodyPart >= 0 && costume.legPart >= 0
                        ? "Loại: Cải trang toàn thân" : "Loại: Cải trang từng phần", x, y + 2, width);
                y = PaintCollectionText(g, mFont.tahoma_7b_dark, "Thông số:", x, y + 3, width);
                for (int i = 0; costume.options != null && i < costume.options.size(); i++)
                    y = PaintCollectionText(g, mFont.tahoma_7_green,
                        "• " + (string)costume.options.elementAt(i), x, y, width);
                if (!costume.unlocked)
                    y = PaintCollectionText(g, mFont.tahoma_7_blue, costume.saleInfo, x, y + 3, width);
            }
            else if (selected is Info_RadaScr)
            {
                Info_RadaScr card = (Info_RadaScr)selected;
                y = PaintCollectionText(g, mFont.tahoma_7b_dark,
                    (card.level > 0 ? "Đã sở hữu" : "Chưa sở hữu") + " - Cấp " + card.level,
                    x, y, width);
                y = PaintCollectionText(g, mFont.tahoma_7, card.info, x, y + 3, width);
                if (_collectionCategory == CollectionCards)
                {
                    y = PaintCollectionText(g, mFont.tahoma_7_green,
                        "Mảnh: " + card.amount + "/" + card.max_amount, x, y + 3, width);
                    for (int i = 0; card.itemOption != null && i < card.itemOption.Length; i++)
                    {
                        ItemOption option = card.itemOption[i];
                        if (option == null) continue;
                        string optionText = option.getOptionString();
                        if (!string.IsNullOrEmpty(optionText))
                            y = PaintCollectionText(g, mFont.tahoma_7_green,
                                "• " + optionText, x, y, width);
                    }
                }
            }
            else if (selected is CostumeCollectionAchievement)
            {
                CostumeCollectionAchievement achievement = (CostumeCollectionAchievement)selected;
                y = PaintCollectionText(g, mFont.tahoma_7,
                    achievement.description, x, y, width);
                y = PaintCollectionText(g, mFont.tahoma_7b_dark,
                    "Tiến độ: " + achievement.progress + "/" + achievement.target, x, y + 3, width);
                y = PaintCollectionText(g, mFont.tahoma_7b_dark, "----- Phần thưởng -----",
                    x, y + 5, width);
                if (achievement.rewardGold > 0)
                    y = PaintCollectionText(g, mFont.tahoma_7_green,
                        "Vàng x" + NinjaUtil.getMoneys(achievement.rewardGold), x, y, width);
                if (achievement.rewardGem > 0)
                    y = PaintCollectionText(g, mFont.tahoma_7_green,
                        "Ngọc x" + NinjaUtil.getMoneys(achievement.rewardGem), x, y, width);
                for (int i = 0; achievement.rewardItems != null && i < achievement.rewardItems.size(); i++)
                {
                    CostumeCollectionRewardItem reward = achievement.rewardItems.elementAt(i)
                        as CostumeCollectionRewardItem;
                    if (reward != null)
                        y = PaintCollectionText(g, mFont.tahoma_7_green,
                            reward.name + " x" + reward.quantity, x, y, width);
                }
                if (achievement.conditionType == 1)
                {
                    y = PaintCollectionText(g, mFont.tahoma_7b_dark,
                        "----- Mục tiêu -----", x, y + 4, width);
                    for (int i = 0; achievement.requiredCostumes != null
                        && i < achievement.requiredCostumes.size(); i++)
                    {
                        CostumeCollectionTargetItem target = achievement.requiredCostumes.elementAt(i)
                            as CostumeCollectionTargetItem;
                        if (target == null) continue;
                        int targetY = y + 2;
                        if (g != null)
                        {
                            Image tick = _statusIcons != null && _statusIcons.Length > 0
                                ? _statusIcons[0] : null;
                            if (target.owned && tick != null)
                                g.drawImageScaleInClip(tick, x, targetY, 10, 10);
                            else
                            {
                                g.setColor(target.owned ? 0x228B28 : 0x938675);
                                g.drawRect(x, targetY, 9, 9);
                            }
                        }
                        y = PaintCollectionText(g, target.owned ? mFont.tahoma_7_green : mFont.tahoma_7_grey,
                            target.name, x + 14, targetY, width - 14);
                    }
                }
            }
            return y;
        }

        private static int PaintCollectionText(mGraphics g, mFont font, string text,
            int x, int y, int width)
        {
            if (string.IsNullOrEmpty(text)) return y;
            string[] paragraphs = text.Replace("\r", string.Empty).Split('\n');
            foreach (string paragraph in paragraphs)
            {
                string[] lines = font.splitFontArray(paragraph, width);
                foreach (string line in lines)
                {
                    if (g != null) font.drawString(g, line, x, y, mFont.LEFT);
                    y += 10;
                }
            }
            return y;
        }
    }
}
