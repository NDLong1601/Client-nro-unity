using System;
using Nro.UI;

namespace Game1.UI.Components
{
    [Flags]
    public enum UiVisualState
    {
        Normal = 0,
        Hovered = 1,
        Pressed = 2,
        Selected = 4,
        Focused = 8,
        Disabled = 16
    }

    public sealed class UiActionButtonStyle
    {
        public int NormalFill = 0xEAA000;
        public int HoveredFill = 0xF5AC16;
        public int PressedFill = 0xD88E00;
        public int SelectedFill = 0x4FB310;
        public int SelectedHoveredFill = -1;
        public int SelectedBorder = -1;
        public int SelectedHighlight = -1;
        public int DisabledFill = 0xB9A68A;
        public int RaisedShadowColor = -1;
        public int SelectedRaisedShadowColor = -1;
        public int ShadowOffsetX = 1;
        public int ShadowOffsetY = 2;
        public int ShadowWidthInset = 1;
        public int ShadowHeightInset = 1;
        public int FillHeightInset = 2;
        public int FocusWidthInset = 3;
        public int FocusHeightInset = 5;
        public int TextLineHeight = 12;
        public int TextTopPadding = 3;
        public int TextHeight = 9;
        public int Border = 0x7A4C08;
        public int FocusBorder = 0xFFF2A8;
        public int Highlight = 0xFFE075;
        public int CornerRadius = 4;
        public int Padding = 6;
        public int IconGap = 4;
        public int TextAlignment = mFont.CENTER;
        public bool DrawHighlight = true;
        public bool DrawFocusBorder = true;
        public mFont NormalFont = mFont.tahoma_7b_dark;
        public mFont SelectedFont = mFont.tahoma_7b_dark;
        public mFont DisabledFont = mFont.tahoma_7_grey;

        public static UiActionButtonStyle Amber
        {
            get { return new UiActionButtonStyle(); }
        }

        public static UiActionButtonStyle FlatTab
        {
            get
            {
                return new UiActionButtonStyle
                {
                    NormalFill = 0xE9E1D5,
                    HoveredFill = 0xF0E9DE,
                    PressedFill = 0xC8BBAA,
                    SelectedFill = 0xD93A2E,
                    DisabledFill = 0xCEC5B9,
                    Border = 0xB7A080,
                    FocusBorder = 0xFFF2A8,
                    Highlight = 0xFFF4E2,
                    CornerRadius = 0,
                    DrawHighlight = false,
                    NormalFont = mFont.tahoma_7_orange,
                    SelectedFont = mFont.tahoma_7b_white
                };
            }
        }
    }

    /// <summary>
    /// Reusable action button for custom UI. Unlike UiButton, it does not mutate
    /// legacy Command bounds or invoke Command.performAction().
    /// </summary>
    public sealed class UiActionButton
    {
        private const int DragThreshold = 10;
        private int _downX;
        private int _downY;

        public UiRect Bounds { get; set; }
        public string Text { get; set; }
        public Image Icon { get; set; }
        public Action Clicked { get; set; }
        public UiActionButtonStyle Style { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool Selected { get; set; }
        public bool Focused { get; set; }
        public bool IsPressed { get; private set; }

        public UiActionButton(UiRect bounds, string text, Action clicked = null,
            UiActionButtonStyle style = null)
        {
            Bounds = bounds;
            Text = text ?? string.Empty;
            Clicked = clicked;
            Style = style ?? UiActionButtonStyle.Amber;
        }

        public static void PaintRaised(mGraphics g, UiRect bounds, string text, bool selected,
            bool focused, UiActionButtonStyle style)
        {
            if (g == null || bounds.IsEmpty) return;
            style = style ?? UiActionButtonStyle.Amber;
            bool hovered = Main.isPC && bounds.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse);
            bool highlighted = focused || hovered;
            int fill = selected
                ? (highlighted && style.SelectedHoveredFill >= 0 ? style.SelectedHoveredFill : style.SelectedFill)
                : (highlighted ? style.HoveredFill : style.NormalFill);
            int highlight = selected && style.SelectedHighlight >= 0 ? style.SelectedHighlight : style.Highlight;

            int shadowColor = style.RaisedShadowColor >= 0 ? style.RaisedShadowColor : style.Border;
            if (selected && style.SelectedRaisedShadowColor >= 0)
                shadowColor = style.SelectedRaisedShadowColor;
            g.setColor(shadowColor);
            g.fillRect(bounds.X + style.ShadowOffsetX, bounds.Y + style.ShadowOffsetY,
                System.Math.Max(1, bounds.Width - style.ShadowWidthInset),
                System.Math.Max(1, bounds.Height - style.ShadowHeightInset), style.CornerRadius);

            g.setColor(fill);
            g.fillRect(bounds.X, bounds.Y, bounds.Width,
                System.Math.Max(1, bounds.Height - style.FillHeightInset), style.CornerRadius);
            if (highlight >= 0)
            {
                g.setColor(highlight);
                g.fillRect(bounds.X + 2, bounds.Y + 1, System.Math.Max(1, bounds.Width - 4), 1);
            }
            if (focused && style.DrawFocusBorder)
            {
                g.setColor(style.FocusBorder);
                g.drawRect(bounds.X + 1, bounds.Y + 1,
                    System.Math.Max(1, bounds.Width - style.FocusWidthInset),
                    System.Math.Max(1, bounds.Height - style.FocusHeightInset));
            }

            mFont font = style.NormalFont ?? mFont.tahoma_7b_dark;
            string[] lines = (text ?? string.Empty).Split('\n');
            int lineHeight = System.Math.Max(1, style.TextLineHeight);
            int textHeight = font.getHeight() + (lines.Length - 1) * lineHeight;
            int textY = bounds.Y + (bounds.Height - textHeight) / 2 - 1;
            for (int i = 0; i < lines.Length; i++)
                font.drawString(g, lines[i], bounds.X + bounds.Width / 2,
                    textY + i * lineHeight, mFont.CENTER);
        }

        public static void PaintFlat(mGraphics g, UiRect bounds, string text, bool selected,
            UiActionButtonStyle style)
        {
            if (g == null || bounds.IsEmpty) return;
            style = style ?? UiActionButtonStyle.Amber;
            g.setColor(selected ? style.SelectedFill : style.NormalFill);
            g.fillRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            g.setColor(selected && style.SelectedBorder >= 0 ? style.SelectedBorder : style.Border);
            g.drawRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);

            mFont font = style.NormalFont ?? mFont.tahoma_7b_dark;
            string[] lines = (text ?? string.Empty).Split('\n');
            if (lines.Length == 1)
            {
                font.drawString(g, lines[0], bounds.X + bounds.Width / 2,
                    bounds.Y + (bounds.Height - style.TextHeight) / 2, mFont.CENTER);
                return;
            }
            int lineHeight = System.Math.Max(1, style.TextLineHeight);
            for (int i = 0; i < lines.Length; i++)
                font.drawString(g, lines[i], bounds.X + bounds.Width / 2,
                    bounds.Y + style.TextTopPadding + i * lineHeight, mFont.CENTER);
        }

        public bool UpdateInput(UiInputContext input, bool isParentDragging = false)
        {
            if (!Visible || !Enabled || input == null || input.IsModalBlocked || isParentDragging)
            {
                CancelPress();
                return false;
            }

            int pointerX = input.PointerX;
            int pointerY = input.PointerY;
            if (input.IsPointerJustDown && Bounds.Contains(pointerX, pointerY))
            {
                IsPressed = true;
                _downX = pointerX;
                _downY = pointerY;
                input.ConsumePointer();
                return true;
            }

            if (!IsPressed) return false;

            if (input.IsPointerDown)
            {
                if (System.Math.Abs(pointerX - _downX) > DragThreshold
                    || System.Math.Abs(pointerY - _downY) > DragThreshold)
                {
                    CancelPress();
                    return false;
                }
                return true;
            }

            if (input.IsPointerJustRelease)
            {
                bool releasedInside = Bounds.Contains(pointerX, pointerY);
                IsPressed = false;
                input.ConsumePointer();
                if (releasedInside) Clicked?.Invoke();
                return true;
            }

            return true;
        }

        public void Paint(mGraphics g)
        {
            if (!Visible || g == null || Bounds.IsEmpty) return;
            UiActionButtonStyle style = Style ?? UiActionButtonStyle.Amber;
            bool hovered = Main.isPC && Bounds.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse);
            UiVisualState state = UiVisualState.Normal;
            if (!Enabled) state |= UiVisualState.Disabled;
            else
            {
                if (IsPressed) state |= UiVisualState.Pressed;
                if (Selected) state |= UiVisualState.Selected;
                if (Focused) state |= UiVisualState.Focused;
                if (hovered) state |= UiVisualState.Hovered;
            }

            int fillColor = style.NormalFill;
            if ((state & UiVisualState.Disabled) != 0) fillColor = style.DisabledFill;
            else if ((state & UiVisualState.Pressed) != 0) fillColor = style.PressedFill;
            else if ((state & UiVisualState.Selected) != 0) fillColor = style.SelectedFill;
            else if ((state & (UiVisualState.Hovered | UiVisualState.Focused)) != 0) fillColor = style.HoveredFill;

            int inset = style.Border >= 0 ? 1 : 0;
            int cornerRadius = System.Math.Max(0, style.CornerRadius);
            if (inset > 0)
            {
                g.setColor(style.Border);
                g.fillRect(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, cornerRadius);
            }
            int innerWidth = Bounds.Width - inset * 2;
            int innerHeight = Bounds.Height - inset * 2;
            if (innerWidth <= 0 || innerHeight <= 0) return;

            g.setColor(fillColor);
            g.fillRect(Bounds.X + inset, Bounds.Y + inset, innerWidth, innerHeight,
                System.Math.Max(0, cornerRadius - inset));

            if (style.DrawHighlight && (state & UiVisualState.Disabled) == 0 && innerWidth > 2)
            {
                g.setColor(style.Highlight);
                g.fillRect(Bounds.X + inset + 1, Bounds.Y + inset, innerWidth - 2, 1);
            }

            if (Enabled && Focused && style.DrawFocusBorder)
            {
                g.setColor(style.FocusBorder);
                g.drawRect(Bounds.X + 1, Bounds.Y + 1, System.Math.Max(0, Bounds.Width - 3),
                    System.Math.Max(0, Bounds.Height - 3));
            }

            PaintContent(g, style, state);
        }

        private void PaintContent(mGraphics g, UiActionButtonStyle style, UiVisualState state)
        {
            mFont font = (state & UiVisualState.Disabled) != 0 ? style.DisabledFont
                : (state & UiVisualState.Selected) != 0 ? style.SelectedFont
                : style.NormalFont;
            if (font == null) font = mFont.tahoma_7b_dark;

            string text = Text ?? string.Empty;
            int iconWidth = Icon != null ? mGraphics.getImageWidth(Icon) : 0;
            int textWidth = text.Length > 0 ? font.getWidth(text) : 0;
            int contentWidth = iconWidth + (iconWidth > 0 && text.Length > 0 ? style.IconGap : 0) + textWidth;
            int contentX;
            if (style.TextAlignment == mFont.LEFT)
                contentX = Bounds.X + style.Padding;
            else
                contentX = Bounds.X + (Bounds.Width - contentWidth) / 2;
            int centerY = Bounds.Y + Bounds.Height / 2;

            if (Icon != null)
            {
                g.drawImage(Icon, contentX + iconWidth / 2, centerY,
                    mGraphics.HCENTER | mGraphics.VCENTER);
                contentX += iconWidth + (text.Length > 0 ? style.IconGap : 0);
            }

            if (text.Length > 0)
            {
                int textX = Icon != null ? contentX
                    : style.TextAlignment == mFont.LEFT ? Bounds.X + style.Padding
                    : Bounds.X + Bounds.Width / 2;
                font.drawString(g, text, textX, centerY - font.getHeight() / 2,
                    Icon != null ? mFont.LEFT : style.TextAlignment);
            }
        }

        private void CancelPress()
        {
            IsPressed = false;
        }
    }
}
