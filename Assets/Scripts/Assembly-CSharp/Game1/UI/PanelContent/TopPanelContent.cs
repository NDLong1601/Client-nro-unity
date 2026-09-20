using Game1.UI.Pilots;
using Nro.UI;

namespace Game1.UI.PanelContent
{
    public enum TopContentActionType
    {
        None = 0,
        Fire = 1
    }

    public struct TopContentAction
    {
        public static readonly TopContentAction None = new TopContentAction(TopContentActionType.None, -1, null, 0, 0, -1);

        public TopContentActionType Type { get; }
        public int SelectedIndex { get; }
        public TopInfo SelectedInfo { get; }
        public int MenuY { get; }
        public int DelayFrames { get; }
        public int BindingRevision { get; }

        public TopContentAction(TopContentActionType type, int selectedIndex, TopInfo selectedInfo, int menuY, int delayFrames, int bindingRevision)
        {
            Type = type;
            SelectedIndex = selectedIndex;
            SelectedInfo = selectedInfo;
            MenuY = menuY;
            DelayFrames = delayFrames;
            BindingRevision = bindingRevision;
        }

        public static TopContentAction CreateFire(int selectedIndex, TopInfo selectedInfo, int menuY, int delayFrames, int bindingRevision)
        {
            return new TopContentAction(TopContentActionType.Fire, selectedIndex, selectedInfo, menuY, delayFrames, bindingRevision);
        }
    }

    public class TopPanelContent
    {
        public const int ITEM_HEIGHT = 24;

        private readonly UiVerticalListState listState = new UiVerticalListState();

        public bool IsBound => listState.IsBound;
        public bool IsActive => listState.IsActive;
        public MyVector Items { get; private set; }
        public int ItemsCount => listState.ItemCount;
        public string TopName { get; private set; }
        public bool IsChallengeMode { get; private set; }
        public UiRect Viewport => listState.Viewport;
        public int SelectedIndex => listState.SelectedIndex;
        public int ScrollY => listState.ScrollY;
        public int ScrollTargetY => listState.ScrollTargetY;
        public int ScrollLimit => listState.ScrollLimit;
        public bool IsDragging => listState.IsDragging;
        public int BindingRevision => listState.BindingRevision;

        public void Bind(MyVector items, string topName, bool challengeMode, UiRect viewport, bool isTouch)
        {
            Items = items;
            TopName = topName;
            IsChallengeMode = challengeMode;
            listState.Bind(items != null ? items.size() : 0, ITEM_HEIGHT, viewport, isTouch);
        }

        public void Refresh(MyVector items, UiRect viewport)
        {
            Items = items;
            listState.Refresh(items != null ? items.size() : 0, viewport);
        }

        public void Update()
        {
            listState.Update();
        }

        public TopContentAction HandleInput(UiInputContext input)
        {
            if (!IsActive || ItemsCount <= 0 || input == null || input.IsModalBlocked)
            {
                return TopContentAction.None;
            }

            bool isUp = GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21];
            if (isUp)
            {
                GameCanvas.clearKeyPressed();
                listState.MoveSelection(-1);
                return TopContentAction.None;
            }

            bool isDown = GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22];
            if (isDown)
            {
                GameCanvas.clearKeyPressed();
                listState.MoveSelection(1);
                return TopContentAction.None;
            }

            bool isFire = GameCanvas.keyPressed[12] || GameCanvas.keyPressed[(!Main.isPC) ? 5 : 25];
            if (isFire)
            {
                GameCanvas.clearKeyPressed();
                return CreateAction(listState.ActivateSelection(2));
            }

            if (GameCanvas.isPointerDown)
            {
                if (!listState.IsDragging && GameCanvas.isPointer(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height))
                {
                    listState.BeginPointer(GameCanvas.py);
                }
                else if (listState.IsDragging)
                {
                    listState.DragPointer(GameCanvas.py, UiVerticalListDragMode.Elastic);
                }
            }

            if (GameCanvas.isPointerJustRelease && listState.IsDragging)
            {
                GameCanvas.isPointerJustRelease = false;
                return CreateAction(listState.ReleasePointer(GameCanvas.py));
            }

            return TopContentAction.None;
        }

        public void UpdateScrollMouse(int wheelDelta)
        {
            listState.UpdateScrollMouse(wheelDelta);
        }

        public void Paint(mGraphics g, int myCharId)
        {
            if (!IsActive || ItemsCount <= 0)
            {
                return;
            }

            TopRankingView.Paint(
                g,
                Viewport.X,
                Viewport.Y,
                Viewport.Width,
                Viewport.Height,
                ScrollY,
                SelectedIndex,
                Items,
                ItemsCount,
                myCharId
            );
        }

        public bool IsActionCurrent(TopContentAction action)
        {
            return IsActive
                && action.Type != TopContentActionType.None
                && action.BindingRevision == BindingRevision;
        }

        public void Unbind()
        {
            Items = null;
            TopName = null;
            IsChallengeMode = false;
            listState.Unbind();
        }

        private TopContentAction CreateAction(UiVerticalListActivation activation)
        {
            if (!activation.IsTriggered
                || Items == null
                || activation.SelectedIndex < 0
                || activation.SelectedIndex >= ItemsCount)
            {
                return TopContentAction.None;
            }

            TopInfo info = (TopInfo)Items.elementAt(activation.SelectedIndex);
            return TopContentAction.CreateFire(
                activation.SelectedIndex,
                info,
                activation.MenuY,
                activation.DelayFrames,
                activation.BindingRevision
            );
        }
    }
}
