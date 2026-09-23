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
        public override void paint(mGraphics g)
        {
            g.translate(-g.getTranslateX(), -g.getTranslateY());
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);

            // 1. Drop shadow for window
            g.setColor(0x000000);
            g.fillRect(_frameRect.X + 3, _frameRect.Y + 3, _frameRect.Width, _frameRect.Height);

            // 2. Main Frame Background
            g.setColor(0xECE0CF); // Warm cream fill
            g.fillRect(_frameRect.X, _frameRect.Y, _frameRect.Width, _frameRect.Height);

            // Frame border
            g.setColor(0x5C4638);
            g.drawRect(_frameRect.X, _frameRect.Y, _frameRect.Width, _frameRect.Height);

            // 3. Left Vertical Tab Bar
            PaintVerticalTabBar(g);

            // 4. Footer Bar
            PaintFooterBar(g);

            // 5. Content Area
            if (_selectedMainTab == 0)
            {
                PaintTaskTabContent(g);
            }
            else if (_selectedMainTab == 1)
            {
                PaintInventoryTabContent(g);
            }
            else if (_selectedMainTab == 2)
            {
                PaintSkillTabContent(g);
            }
            else if (_selectedMainTab == 3)
            {
                PaintClanTabContent(g);
            }
            else if (_selectedMainTab == 4)
            {
                PaintFunctionTabContent(g);
            }
            else if (_selectedMainTab == 5)
            {
                PaintDiscipleTabContent(g);
            }
            else if (_selectedMainTab == 6)
            {
                PaintFriendTabContent(g);
            }
            else
            {
                PaintEmptyTabContent(g);
            }
            if (_showIntrinsicInput) PaintIntrinsicInput(g);
            if (_clanDialogMode != ClanInputNone) PaintClanDialog(g);
        }

        private void PaintVerticalTabBar(mGraphics g)
        {
            g.setColor(0xEAE5DC);
            g.fillRect(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height);
            g.setColor(0x3E3A34);
            g.drawRect(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height);

            _mainTabScrollAdapter.Paint(g, (graphics, i, bounds) =>
            {
                int ty = bounds.Y;
                int tabH = bounds.Height;
                bool isSelected = (i == _selectedMainTab);
                if (isSelected)
                {
                    g.setColor(0xFFCB35);
                    g.fillRect(_tabBarRect.X + 1, ty + 1, _tabBarRect.Width - 1, tabH - 1);
                    if (_keyboardFocus == KeyboardFocusMainTabs)
                    {
                        g.setColor(0xFFF2A8);
                        g.drawRect(_tabBarRect.X + 2, ty + 2, _tabBarRect.Width - 5, tabH - 5);
                    }
                }
                g.setColor(0x766B5D);
                g.drawLine(_tabBarRect.X, ty + tabH, _tabBarRect.X + _tabBarRect.Width, ty + tabH);
                Image icon = _mainTabIcons != null && i < _mainTabIcons.Length ? _mainTabIcons[i] : null;
                if (icon != null)
                {
                    int sourceW = mGraphics.getImageWidth(icon);
                    int sourceH = mGraphics.getImageHeight(icon);
                    int width = System.Math.Min(42, 38 * sourceW / System.Math.Max(1, sourceH));
                    int height = System.Math.Min(38, 42 * sourceH / System.Math.Max(1, sourceW));
                    g.drawImageScaleInClip(icon, _tabBarRect.X + (_tabBarRect.Width - width) / 2,
                        ty + (tabH - height) / 2, width, height);
                }
            });
            _mainTabScrollAdapter.PaintScrollbar(g, _tabBarRect);
        }

        private void PaintFooterBar(mGraphics g)
        {
            g.setColor(0xE9E4DA);
            g.fillRect(_footerRect.X, _footerRect.Y, _footerRect.Width, _footerRect.Height);
            g.setColor(0x7B6F60);
            g.drawRect(_footerRect.X, _footerRect.Y, _footerRect.Width, _footerRect.Height);

            Char me = Char.myCharz();
            long xu = me != null ? me.xu : 0L;
            int luong = me != null ? me.luong : 0;
            int luongKhoa = me != null ? me.luongKhoa : 0;
            int usableW = _footerRect.Width - 28;
            PaintCurrency(g, _footerRect.X + usableW / 6, Panel.imgXu, NinjaUtil.getMoneys(xu));
            PaintCurrency(g, _footerRect.X + usableW / 2, Panel.imgLuong, NinjaUtil.getMoneys(luong));
            PaintCurrency(g, _footerRect.X + usableW * 5 / 6, Panel.imgLuongKhoa, NinjaUtil.getMoneys(luongKhoa));

            g.setColor(0xF7C400);
            g.fillRect(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height);
            g.setColor(0xE59600);
            g.drawRect(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height);
            g.setColor(0xE35A24);
            g.drawLine(_closeBtnRect.X + 5, _closeBtnRect.Y + 5, _closeBtnRect.X + 17, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 6, _closeBtnRect.Y + 5, _closeBtnRect.X + 18, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 17, _closeBtnRect.Y + 5, _closeBtnRect.X + 5, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 18, _closeBtnRect.Y + 5, _closeBtnRect.X + 6, _closeBtnRect.Y + 17);
        }

        private void PaintCurrency(mGraphics g, int centerX, Image icon, string value)
        {
            int textW = mFont.tahoma_7_orange.getWidth(value);
            int iconW = icon != null ? icon.getWidth() : 14;
            int startX = centerX - (iconW + 5 + textW) / 2;
            if (icon != null) g.drawImage(icon, startX + iconW / 2, _footerRect.Y + _footerRect.Height / 2, mGraphics.HCENTER | mGraphics.VCENTER);
            mFont.tahoma_7_orange.drawString(g, value, startX + iconW + 5, _footerRect.Y + 7, mFont.LEFT);
        }

        private void PaintEmptyTabContent(mGraphics g)
        {
            int midX = _contentRect.X + _contentRect.Width / 2;
            int midY = _contentRect.Y + _contentRect.Height / 2;

            g.setColor(0xDFD2BC);
            g.fillRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);
            g.setColor(0xC4B79B);
            g.drawRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);

            string tabName = MainTabNames[_selectedMainTab];
            mFont.tahoma_7b_dark.drawString(g, tabName, midX, midY - 14, mFont.CENTER);
            mFont.tahoma_7_grey.drawString(g, "Chức năng đang được phát triển...", midX, midY + 4, mFont.CENTER);
        }

        private int DrawWrappedText(mGraphics g, mFont font, string text, int x, int y, int width)
        {
            string[] lines = font.splitFontArray(text ?? string.Empty, width);
            for (int i = 0; i < lines.Length; i++)
            {
                font.drawString(g, lines[i], x, y, mFont.LEFT);
                y += 14;
            }
            return y;
        }

        private string TruncateString(mFont font, string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (font.getWidth(text) <= maxWidth) return text;
            string s = text;
            while (s.Length > 0 && font.getWidth(s + "...") > maxWidth)
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s + "...";
        }

        // Icons
        private void DrawBullIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x8D6E63);
            g.fillRect(x + 4, y + 6, 12, 10);
            g.setColor(0xD7CCC8);
            g.fillRect(x + 2, y + 2, 4, 5);
            g.fillRect(x + 14, y + 2, 4, 5);
            g.setColor(0xBCAAA4);
            g.fillRect(x + 6, y + 11, 8, 5);
            g.setColor(0x3E2723);
            g.fillRect(x + 6, y + 8, 2, 2);
            g.fillRect(x + 12, y + 8, 2, 2);
        }

        private void DrawKanaoIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x9B59B6);
            g.fillRect(x + 3, y + 4, 6, 6);
            g.fillRect(x + 11, y + 4, 6, 6);
            g.fillRect(x + 4, y + 10, 5, 5);
            g.fillRect(x + 11, y + 10, 5, 5);
            g.setColor(0x2C3E50);
            g.fillRect(x + 9, y + 3, 2, 12);
        }

        private void DrawFishIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x3498DB);
            g.fillRect(x + 4, y + 6, 10, 6);
            g.setColor(0x2980B9);
            g.fillRect(x + 2, y + 7, 2, 4);
            g.fillRect(x + 14, y + 4, 3, 10);
            g.setColor(0xFFFFFF);
            g.fillRect(x + 5, y + 7, 2, 2);
        }

        private void DrawCheckmark(mGraphics g, int x, int y, int size)
        {
            g.setColor(0x27AE60);
            int cx = x + 3;
            int cy = y + size / 2;
            for (int i = 0; i < 3; i++)
            {
                g.drawLine(cx + i, cy + i, cx + i + 1, cy + i + 1);
            }
            for (int i = 0; i < 7; i++)
            {
                g.drawLine(cx + 3 + i, cy + 3 - i, cx + 4 + i, cy + 3 - i);
            }
        }

        private void DrawLock(mGraphics g, int x, int y, int w, int h)
        {
            g.setColor(0x7F8C8D);
            g.drawRect(x + 2, y + 1, w - 4, h / 2);
            g.fillRect(x, y + h / 2, w, h / 2);
            g.setColor(0x2C3E50);
            g.fillRect(x + w / 2 - 1, y + h / 2 + 2, 2, 3);
        }

        private void DrawBulletPoint(mGraphics g, int x, int y, int color)
        {
            g.setColor(color);
            g.fillRect(x, y, 3, 3);
        }

        private void DrawScrollIcon(mGraphics g, int x, int y)
        {
            g.setColor(0xF9E79F);
            g.fillRect(x + 5, y + 3, 16, 18);
            g.setColor(0xD4AC0D);
            g.drawRect(x + 5, y + 3, 16, 18);
            g.setColor(0xB7950B);
            g.fillRect(x + 3, y + 2, 4, 20);
            g.fillRect(x + 19, y + 2, 4, 20);
            g.setColor(0x7D6608);
            g.drawLine(x + 8, y + 8, x + 18, y + 8);
            g.drawLine(x + 8, y + 12, x + 18, y + 12);
            g.drawLine(x + 8, y + 16, x + 15, y + 16);
        }

        private void DrawBackpackIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x8B4513);
            g.fillRect(x + 4, y + 6, 18, 15);
            g.setColor(0x5C2D0C);
            g.drawRect(x + 4, y + 6, 18, 15);
            g.setColor(0xA0522D);
            g.fillRect(x + 4, y + 4, 18, 6);
            g.setColor(0xF1C40F);
            g.fillRect(x + 11, y + 9, 4, 3);
            g.setColor(0x5C2D0C);
            g.drawLine(x + 9, y + 3, x + 17, y + 3);
            g.drawLine(x + 9, y + 3, x + 9, y + 5);
            g.drawLine(x + 17, y + 3, x + 17, y + 5);
        }

        private void DrawBookIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x2471A3);
            g.fillRect(x + 5, y + 3, 16, 18);
            g.setColor(0x1B4F72);
            g.drawRect(x + 5, y + 3, 16, 18);
            g.setColor(0x5499C7);
            g.fillRect(x + 3, y + 3, 4, 18);
            g.setColor(0xF1C40F);
            g.fillRect(x + 10, y + 8, 7, 7);
            g.setColor(0x2471A3);
            g.fillRect(x + 12, y + 10, 3, 3);
        }

        private void DrawShieldIcon(mGraphics g, int x, int y)
        {
            g.setColor(0xC0392B);
            g.fillRect(x + 5, y + 4, 16, 12);
            for (int i = 0; i < 6; i++)
            {
                g.drawLine(x + 5 + i, y + 16 + i, x + 20 - i, y + 16 + i);
            }
            g.setColor(0x78281F);
            g.drawRect(x + 5, y + 4, 16, 12);
            g.setColor(0xF1C40F);
            g.fillRect(x + 10, y + 8, 6, 6);
        }

        private void DrawGearIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x7F8C8D);
            g.fillRect(x + 7, y + 7, 12, 12);
            g.setColor(0x34495E);
            g.drawRect(x + 7, y + 7, 12, 12);
            g.setColor(0xD5C7B0);
            g.fillRect(x + 11, y + 11, 4, 4);
            g.setColor(0x7F8C8D);
            g.fillRect(x + 11, y + 4, 4, 3);
            g.fillRect(x + 11, y + 19, 4, 3);
            g.fillRect(x + 4, y + 11, 3, 4);
            g.fillRect(x + 19, y + 11, 3, 4);
        }
    }
}
