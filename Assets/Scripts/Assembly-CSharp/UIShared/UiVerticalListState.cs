namespace Nro.UI
{
    public enum UiVerticalListDragMode
    {
        Elastic = 0,
        Clamped = 1
    }

    public struct UiVerticalListActivation
    {
        public static readonly UiVerticalListActivation None = new UiVerticalListActivation(false, -1, 0, 0, -1);

        public bool IsTriggered { get; }
        public int SelectedIndex { get; }
        public int MenuY { get; }
        public int DelayFrames { get; }
        public int BindingRevision { get; }

        private UiVerticalListActivation(bool isTriggered, int selectedIndex, int menuY, int delayFrames, int bindingRevision)
        {
            IsTriggered = isTriggered;
            SelectedIndex = selectedIndex;
            MenuY = menuY;
            DelayFrames = delayFrames;
            BindingRevision = bindingRevision;
        }

        public static UiVerticalListActivation Create(int selectedIndex, int menuY, int delayFrames, int bindingRevision)
        {
            return new UiVerticalListActivation(true, selectedIndex, menuY, delayFrames, bindingRevision);
        }
    }

    public sealed class UiVerticalListState
    {
        public bool IsBound { get; private set; }
        public bool IsActive { get; private set; }
        public int ItemCount { get; private set; }
        public UiRect Viewport { get; private set; }
        public int SelectedIndex { get; private set; } = -1;
        public int ScrollY => currentScrollY;
        public int ScrollTargetY => targetScrollY;
        public int ScrollLimit => scrollLimit;
        public bool IsDragging => pointerIsDowning;
        public int BindingRevision { get; private set; }

        private int itemHeight;
        private int currentScrollY;
        private int targetScrollY;
        private int scrollLimit;
        private int scrollMomentum;
        private int scrollVelocity;
        private int scrollRemainder;

        private bool pointerIsDowning;
        private int pointerDownFirstY = -1000;
        private readonly int[] pointerDownLastY = new int[3];
        private int pointerDownTime;
        private bool isDownWhenRunning;
        private int mouseIndex = -1;

        public void Bind(int itemCount, int itemHeight, UiRect viewport, bool isTouch)
        {
            BindingRevision++;
            ItemCount = itemCount;
            this.itemHeight = itemHeight;
            Viewport = viewport;

            scrollLimit = UiListLayout.CalculateMaxScroll(itemCount, itemHeight, viewport.Height);
            currentScrollY = 0;
            targetScrollY = 0;
            scrollMomentum = 0;
            scrollVelocity = 0;
            scrollRemainder = 0;

            SelectedIndex = isTouch ? -1 : (itemCount > 0 ? 0 : -1);
            ResetPointerState();

            IsBound = true;
            IsActive = true;
        }

        public void Refresh(int itemCount, UiRect viewport)
        {
            BindingRevision++;
            ItemCount = itemCount;
            Viewport = viewport;

            scrollLimit = UiListLayout.CalculateMaxScroll(itemCount, itemHeight, viewport.Height);
            currentScrollY = Clamp(currentScrollY, 0, scrollLimit);
            targetScrollY = Clamp(targetScrollY, 0, scrollLimit);

            if (SelectedIndex >= itemCount)
            {
                SelectedIndex = itemCount - 1;
            }
            if (itemCount == 0)
            {
                SelectedIndex = -1;
            }

            IsBound = true;
            IsActive = true;
        }

        public void Update()
        {
            if (!IsActive)
            {
                return;
            }

            if (scrollMomentum != 0 && !pointerIsDowning)
            {
                targetScrollY += scrollMomentum / 100;
                if (targetScrollY < 0)
                {
                    targetScrollY = 0;
                }
                else if (targetScrollY > scrollLimit)
                {
                    targetScrollY = scrollLimit;
                }
                else
                {
                    currentScrollY = targetScrollY;
                }
                scrollMomentum = scrollMomentum * 9 / 10;
                if (scrollMomentum < 100 && scrollMomentum > -100)
                {
                    scrollMomentum = 0;
                }
            }

            if (currentScrollY != targetScrollY && !pointerIsDowning)
            {
                scrollVelocity = targetScrollY - currentScrollY << 2;
                scrollRemainder += scrollVelocity;
                currentScrollY += scrollRemainder >> 4;
                scrollRemainder &= 15;
            }
        }

        public void MoveSelection(int delta)
        {
            if (!IsActive || ItemCount <= 0)
            {
                return;
            }

            SelectedIndex += delta;
            if (SelectedIndex < 0)
            {
                SelectedIndex = ItemCount - 1;
            }
            if (SelectedIndex > ItemCount - 1)
            {
                SelectedIndex = 0;
            }

            targetScrollY = SelectedIndex * itemHeight - Viewport.Height / 2;
            targetScrollY = Clamp(targetScrollY, 0, scrollLimit);
            currentScrollY = targetScrollY;
        }

        public UiVerticalListActivation ActivateSelection(int delayFrames)
        {
            if (!IsActive || SelectedIndex < 0 || SelectedIndex >= ItemCount)
            {
                return UiVerticalListActivation.None;
            }
            return CreateActivation(delayFrames);
        }

        public void BeginPointer(int pointerY)
        {
            if (!IsActive || ItemCount <= 0 || pointerIsDowning)
            {
                return;
            }

            for (int i = 0; i < pointerDownLastY.Length; i++)
            {
                pointerDownLastY[i] = pointerY;
            }
            pointerDownFirstY = pointerY;
            pointerIsDowning = true;
            isDownWhenRunning = scrollMomentum != 0;
            scrollMomentum = 0;
        }

        public void DragPointer(int pointerY, UiVerticalListDragMode dragMode)
        {
            if (!IsActive || !pointerIsDowning)
            {
                return;
            }

            pointerDownTime++;
            if (pointerDownTime > 5 && pointerDownFirstY == pointerY && !isDownWhenRunning)
            {
                pointerDownFirstY = -1000;
                SelectedIndex = (targetScrollY + pointerY - Viewport.Y) / itemHeight;
                if (SelectedIndex >= ItemCount)
                {
                    SelectedIndex = -1;
                }
            }
            else
            {
                mouseIndex = -1;
            }

            int deltaY = pointerY - pointerDownLastY[0];
            if (deltaY != 0 && SelectedIndex != -1)
            {
                SelectedIndex = -1;
            }
            for (int i = pointerDownLastY.Length - 1; i > 0; i--)
            {
                pointerDownLastY[i] = pointerDownLastY[i - 1];
            }
            pointerDownLastY[0] = pointerY;

            targetScrollY -= deltaY;
            targetScrollY = Clamp(targetScrollY, 0, scrollLimit);

            if (dragMode == UiVerticalListDragMode.Elastic)
            {
                if (currentScrollY < 0 || currentScrollY > scrollLimit)
                {
                    deltaY /= 2;
                }
                currentScrollY -= deltaY;
            }
            else
            {
                currentScrollY -= deltaY;
                currentScrollY = Clamp(currentScrollY, 0, scrollLimit);
            }
        }

        public UiVerticalListActivation ReleasePointer(int pointerY)
        {
            if (!IsActive || !pointerIsDowning)
            {
                return UiVerticalListActivation.None;
            }

            pointerIsDowning = false;
            int dragDelta = pointerY - pointerDownLastY[0];
            int totalDelta = pointerY - pointerDownFirstY;

            if (Abs(dragDelta) < 20 && Abs(totalDelta) < 20 && !isDownWhenRunning)
            {
                scrollMomentum = 0;
                targetScrollY = currentScrollY;
                pointerDownFirstY = -1000;
                int clickIndex = (targetScrollY + pointerY - Viewport.Y) / itemHeight;
                pointerDownTime = 0;
                if (clickIndex >= 0 && clickIndex < ItemCount)
                {
                    SelectedIndex = clickIndex;
                    return CreateActivation(10);
                }
                SelectedIndex = -1;
                return UiVerticalListActivation.None;
            }

            if (SelectedIndex != -1 && pointerDownTime > 5)
            {
                pointerDownTime = 0;
                return CreateActivation(1);
            }

            if (SelectedIndex == -1 && !isDownWhenRunning)
            {
                if (currentScrollY < 0)
                {
                    targetScrollY = 0;
                }
                else if (currentScrollY > scrollLimit)
                {
                    targetScrollY = scrollLimit;
                }
                else
                {
                    int momentum = pointerY - pointerDownLastY[0]
                        + (pointerDownLastY[0] - pointerDownLastY[1])
                        + (pointerDownLastY[1] - pointerDownLastY[2]);
                    momentum = momentum > 10 ? 10 : (momentum < -10 ? -10 : 0);
                    scrollMomentum = -momentum * 100;
                }
            }
            pointerDownTime = 0;
            return UiVerticalListActivation.None;
        }

        public void UpdateScrollMouse(int wheelDelta)
        {
            if (!IsActive || ItemCount <= 0)
            {
                return;
            }

            if (mouseIndex == -1)
            {
                mouseIndex = SelectedIndex >= 0 ? SelectedIndex : 0;
            }
            if (wheelDelta > 0)
            {
                mouseIndex -= wheelDelta;
            }
            else if (wheelDelta < 0)
            {
                mouseIndex += -wheelDelta;
            }
            if (mouseIndex < 0)
            {
                mouseIndex = 0;
            }
            targetScrollY = Clamp(mouseIndex * 12, 0, scrollLimit);
        }

        public void Unbind()
        {
            BindingRevision++;
            IsBound = false;
            IsActive = false;
            ItemCount = 0;
            Viewport = UiRect.Empty;
            SelectedIndex = -1;
            itemHeight = 0;
            currentScrollY = 0;
            targetScrollY = 0;
            scrollLimit = 0;
            scrollMomentum = 0;
            scrollVelocity = 0;
            scrollRemainder = 0;
            ResetPointerState();
        }

        private UiVerticalListActivation CreateActivation(int delayFrames)
        {
            int menuY = (SelectedIndex + 1) * itemHeight - currentScrollY + Viewport.Y;
            return UiVerticalListActivation.Create(SelectedIndex, menuY, delayFrames, BindingRevision);
        }

        private void ResetPointerState()
        {
            pointerIsDowning = false;
            pointerDownTime = 0;
            pointerDownFirstY = -1000;
            isDownWhenRunning = false;
            for (int i = 0; i < pointerDownLastY.Length; i++)
            {
                pointerDownLastY[i] = 0;
            }
            mouseIndex = -1;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        private static int Abs(int value)
        {
            return value > 0 ? value : -value;
        }
    }
}
