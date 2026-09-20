using System;
using Nro.UI;

namespace Game1.UI.Adapters
{
    public sealed class TextFieldAdapter
    {
        private readonly TField _target;
        private bool _isEnabled = true;
        private bool _isVisible = true;

        public TField Target => _target;

        public bool IsFocused => _target != null && _target.isFocus;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (!_isEnabled && IsFocused)
                {
                    SetFocused(false);
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                if (!_isVisible && IsFocused)
                {
                    SetFocused(false);
                }
            }
        }

        public UiRect Bounds => _target != null
            ? new UiRect(_target.x, _target.y, _target.width, _target.height)
            : UiRect.Empty;

        public TextFieldAdapter(TField target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public void Configure(int x, int y, int width, int height, UiInputType inputType, int maxLength, string name = null)
        {
            if (_target == null)
            {
                return;
            }
            _target.x = x;
            _target.y = y;
            _target.width = width;
            _target.height = height;
            _target.setIputType((int)inputType);
            _target.setMaxTextLenght(maxLength);
            if (name != null)
            {
                _target.name = name;
            }
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_target == null)
            {
                return;
            }
            _target.x = x;
            _target.y = y;
            _target.width = width;
            _target.height = height;
        }

        public void SetBounds(UiRect bounds)
        {
            SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public void SetFocused(bool focused)
        {
            if (_target == null)
            {
                return;
            }
            if (focused && (!_isEnabled || !_isVisible))
            {
                return;
            }
            _target.setFocusWithKb(focused);
        }

        public string GetText()
        {
            return _target?.getText() ?? string.Empty;
        }

        public void SetText(string text)
        {
            _target?.setText(text);
        }

        public void Update()
        {
            if (_target == null || !_isEnabled || !_isVisible)
            {
                return;
            }
            _target.update();
        }

        public void Paint(mGraphics g)
        {
            if (_target == null || !_isVisible)
            {
                return;
            }
            _target.paint(g);
        }

        public bool KeyPressed(int keyCode)
        {
            if (_target == null || !_isEnabled || !_isVisible || !_target.isFocus)
            {
                return false;
            }
            _target.keyPressed(keyCode);
            return true;
        }
    }
}
