using System;
using Nro.UI;

namespace Game1.UI.Components
{
    public class UiButton
    {
        public Command Command { get; }
        public UiRect Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = NormalizeBounds(value);
                ApplyCommandBounds();
            }
        }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool Pressed { get; private set; }

        private UiRect _bounds;
        private int _downX;
        private int _downY;
        private const int DragThreshold = 10;

        public UiButton(Command command, UiRect bounds)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Bounds = bounds;
        }

        private UiRect NormalizeBounds(UiRect bounds)
        {
            int width;
            int height;

            if (Command.img != null)
            {
                width = mGraphics.getImageWidth(Command.img);
                height = mGraphics.getImageHeight(Command.img);
            }
            else if (Command.type == 1)
            {
                width = 160;
                height = 26;
            }
            else if (Command.type == 2)
            {
                width = bounds.Width > 0 ? bounds.Width : (Command.w > 0 ? Command.w : 50);
                height = 26;
            }
            else
            {
                width = 76;
                height = 26;
            }

            return new UiRect(bounds.X, bounds.Y, width, height);
        }

        private void ApplyCommandBounds()
        {
            Command.x = _bounds.X;
            Command.y = _bounds.Y;
            Command.w = _bounds.Width;
            Command.h = _bounds.Height;
            if (Command.type == 1 || Command.type == 2)
            {
                Command.hw = _bounds.Width / 2;
            }
        }

        public bool UpdateInput(UiInputContext input, bool isParentDragging = false)
        {
            if (!Visible || !Enabled || input == null || input.IsModalBlocked)
            {
                if (Pressed)
                {
                    Pressed = false;
                    if (Command != null) Command.isFocus = false;
                }
                return false;
            }

            if (isParentDragging)
            {
                if (Pressed)
                {
                    Pressed = false;
                    if (Command != null) Command.isFocus = false;
                }
                return false;
            }

            int px = input.PointerX;
            int py = input.PointerY;

            if (input.IsPointerJustDown)
            {
                if (Bounds.Contains(px, py))
                {
                    Pressed = true;
                    _downX = px;
                    _downY = py;
                    if (Command != null) Command.isFocus = true;
                    input.ConsumePointer();
                    return true;
                }
            }

            if (Pressed)
            {
                if (input.IsPointerDown)
                {
                    int dx = System.Math.Abs(px - _downX);
                    int dy = System.Math.Abs(py - _downY);
                    if (dx > DragThreshold || dy > DragThreshold)
                    {
                        Pressed = false;
                        if (Command != null) Command.isFocus = false;
                        return false;
                    }

                    if (Command != null) Command.isFocus = Bounds.Contains(px, py);
                    return true;
                }

                if (input.IsPointerJustRelease)
                {
                    Pressed = false;
                    if (Command != null) Command.isFocus = false;

                    if (Bounds.Contains(px, py))
                    {
                        input.ConsumePointer();
                        Command?.performAction();
                        return true;
                    }
                    else
                    {
                        // Released outside
                        input.ConsumePointer();
                        return true;
                    }
                }
            }

            return false;
        }

        public void Paint(mGraphics g)
        {
            if (!Visible || Command == null || g == null) return;

            ApplyCommandBounds();
            Command.isFocus = Pressed;

            Command.paint(g);
        }
    }
}
