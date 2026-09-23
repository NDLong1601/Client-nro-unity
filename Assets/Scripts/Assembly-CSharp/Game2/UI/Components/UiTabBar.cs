using System;
using System.Collections.Generic;
using Nro.UI;

namespace Game2.UI.Components
{
    public enum UiTabOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    /// <summary>
    /// Equal-sized tab buttons with shared selection and focus state.
    /// Reconfigure only when the bounds or tab labels change.
    /// </summary>
    public sealed class UiTabBar
    {
        private readonly List<UiActionButton> _buttons = new List<UiActionButton>();
        private UiRect _bounds;
        private UiTabOrientation _orientation;
        private int _gap;

        public int SelectedIndex { get; private set; } = -1;
        public int FocusedIndex { get; set; } = -1;
        public UiActionButtonStyle ButtonStyle { get; set; }
        public Action<int> SelectedIndexChanged { get; set; }
        public Action<int> TabClicked { get; set; }
        public int Count { get { return _buttons.Count; } }
        private bool _cancelledPointerSequence;

        public void Configure(UiRect bounds, IList<string> labels, UiTabOrientation orientation = UiTabOrientation.Horizontal,
            int selectedIndex = -1, int gap = 2)
        {
            _cancelledPointerSequence = false;
            for (int i = 0; i < _buttons.Count; i++) _buttons[i].UpdateInput(null);
            _bounds = bounds;
            _orientation = orientation;
            _gap = System.Math.Max(0, gap);
            int count = labels != null ? labels.Count : 0;

            while (_buttons.Count > count) _buttons.RemoveAt(_buttons.Count - 1);
            while (_buttons.Count < count)
            {
                int tabIndex = _buttons.Count;
                _buttons.Add(new UiActionButton(UiRect.Empty, string.Empty,
                    () => ActivateTab(tabIndex), ButtonStyle ?? UiActionButtonStyle.FlatTab));
            }

            for (int i = 0; i < count; i++)
            {
                _buttons[i].Text = labels[i] ?? string.Empty;
                _buttons[i].Bounds = CalculateTabBounds(i, count);
                _buttons[i].Style = ButtonStyle ?? UiActionButtonStyle.FlatTab;
            }

            SelectedIndex = count == 0 || selectedIndex < 0
                ? -1 : System.Math.Min(count - 1, selectedIndex);
            if (FocusedIndex >= count) FocusedIndex = -1;
        }

        public UiRect GetTabBounds(int index)
        {
            return index >= 0 && index < _buttons.Count ? _buttons[index].Bounds : UiRect.Empty;
        }

        public void SetTabEnabled(int index, bool enabled)
        {
            if (index < 0 || index >= _buttons.Count) return;
            _buttons[index].Enabled = enabled;
            if (!enabled && SelectedIndex == index) SelectedIndex = -1;
            if (!enabled && FocusedIndex == index) FocusedIndex = -1;
        }

        public bool SetSelectedIndex(int index, bool notify = false)
        {
            if (index < -1 || index >= _buttons.Count) return false;
            if (index >= 0 && !_buttons[index].Enabled) return false;
            if (SelectedIndex == index) return false;
            SelectedIndex = index;
            if (notify) SelectedIndexChanged?.Invoke(index);
            return true;
        }

        public bool UpdateInput(UiInputContext input, bool isParentDragging = false)
        {
            if (input == null) return false;
            if (_cancelledPointerSequence)
            {
                if (input.IsPointerJustRelease)
                {
                    input.ConsumePointer();
                    _cancelledPointerSequence = false;
                }
                return true;
            }

            for (int i = 0; i < _buttons.Count; i++)
            {
                bool wasPressed = _buttons[i].IsPressed;
                if (_buttons[i].UpdateInput(input, isParentDragging)) return true;
                if (!wasPressed) continue;
                if (input.IsPointerJustRelease)
                {
                    input.ConsumePointer();
                    return true;
                }
                if (input.IsPointerDown)
                {
                    _cancelledPointerSequence = true;
                    return true;
                }
            }
            return false;
        }

        public void Paint(mGraphics g)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].Selected = i == SelectedIndex;
                _buttons[i].Focused = i == FocusedIndex;
                _buttons[i].Paint(g);
            }
        }

        public void PaintRaised(mGraphics g)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                UiActionButton button = _buttons[i];
                if (!button.Visible || !button.Enabled) continue;
                UiActionButton.PaintRaised(g, button.Bounds, button.Text,
                    i == SelectedIndex, i == FocusedIndex, button.Style);
            }
        }

        private UiRect CalculateTabBounds(int index, int count)
        {
            if (count <= 0 || _bounds.IsEmpty) return UiRect.Empty;

            bool horizontal = _orientation == UiTabOrientation.Horizontal;
            int available = horizontal ? _bounds.Width : _bounds.Height;
            available = System.Math.Max(0, available - _gap * (count - 1));
            int baseSize = available / count;
            int remainder = available % count;
            int size = baseSize + (index < remainder ? 1 : 0);
            int offset = index * baseSize + System.Math.Min(index, remainder) + index * _gap;

            return horizontal
                ? new UiRect(_bounds.X + offset, _bounds.Y, size, _bounds.Height)
                : new UiRect(_bounds.X, _bounds.Y + offset, _bounds.Width, size);
        }

        private void ActivateTab(int index)
        {
            if (index < 0 || index >= _buttons.Count || !_buttons[index].Enabled) return;
            SetSelectedIndex(index, true);
            TabClicked?.Invoke(index);
        }
    }
}
