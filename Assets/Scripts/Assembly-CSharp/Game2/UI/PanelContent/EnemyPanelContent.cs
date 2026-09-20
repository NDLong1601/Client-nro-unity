using Game2.UI.Pilots;
using Nro.UI;

namespace Game2.UI.PanelContent
{
    public enum EnemyContentActionType
    {
        None = 0,
        OpenActions = 1
    }

    public struct EnemyContentAction
    {
        public static readonly EnemyContentAction None = new EnemyContentAction(EnemyContentActionType.None, -1, null, 0, 0, -1);

        public EnemyContentActionType Type { get; }
        public int SelectedIndex { get; }
        public InfoItem SelectedInfo { get; }
        public int MenuY { get; }
        public int DelayFrames { get; }
        public int BindingRevision { get; }

        public EnemyContentAction(EnemyContentActionType type, int selectedIndex, InfoItem selectedInfo, int menuY, int delayFrames, int bindingRevision)
        {
            Type = type;
            SelectedIndex = selectedIndex;
            SelectedInfo = selectedInfo;
            MenuY = menuY;
            DelayFrames = delayFrames;
            BindingRevision = bindingRevision;
        }

        public static EnemyContentAction CreateOpenActions(int selectedIndex, InfoItem selectedInfo, int menuY, int delayFrames, int bindingRevision)
        {
            return new EnemyContentAction(EnemyContentActionType.OpenActions, selectedIndex, selectedInfo, menuY, delayFrames, bindingRevision);
        }
    }

    public class EnemyPanelContent
    {
        public const int ITEM_HEIGHT = 24;

        private readonly UiVerticalListState listState = new UiVerticalListState();

        public bool IsBound => listState.IsBound;
        public bool IsActive => listState.IsActive;
        public MyVector Items { get; private set; }
        public int ItemsCount => listState.ItemCount;
        public UiRect Viewport => listState.Viewport;
        public int SelectedIndex => listState.SelectedIndex;
        public int ScrollY => listState.ScrollY;
        public int ScrollTargetY => listState.ScrollTargetY;
        public int ScrollLimit => listState.ScrollLimit;
        public bool IsDragging => listState.IsDragging;
        public int BindingRevision => listState.BindingRevision;

        public void Bind(MyVector items, UiRect viewport, bool isTouch)
        {
            Items = items;
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

        public EnemyContentAction HandleInput(UiInputContext input)
        {
            if (!IsActive || ItemsCount <= 0 || input == null || input.IsModalBlocked)
            {
                return EnemyContentAction.None;
            }

            bool isUp = GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21];
            if (isUp)
            {
                GameCanvas.clearKeyPressed();
                listState.MoveSelection(-1);
                return EnemyContentAction.None;
            }

            bool isDown = GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22];
            if (isDown)
            {
                GameCanvas.clearKeyPressed();
                listState.MoveSelection(1);
                return EnemyContentAction.None;
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
                    listState.DragPointer(GameCanvas.py, UiVerticalListDragMode.Clamped);
                }
            }

            if (GameCanvas.isPointerJustRelease && listState.IsDragging)
            {
                GameCanvas.isPointerJustRelease = false;
                return CreateAction(listState.ReleasePointer(GameCanvas.py));
            }

            return EnemyContentAction.None;
        }

        public void UpdateScrollMouse(int wheelDelta)
        {
            listState.UpdateScrollMouse(wheelDelta);
        }

        public void Paint(mGraphics g)
        {
            if (!IsActive)
            {
                return;
            }

            EnemyListView.Paint(
                g,
                Viewport.X,
                Viewport.Y,
                Viewport.Width,
                Viewport.Height,
                ScrollY,
                SelectedIndex,
                Items,
                ItemsCount
            );
        }

        public bool IsActionCurrent(EnemyContentAction action)
        {
            return IsActive
                && action.Type != EnemyContentActionType.None
                && action.BindingRevision == BindingRevision
                && action.SelectedIndex >= 0
                && action.SelectedIndex < ItemsCount
                && Items != null
                && (InfoItem)Items.elementAt(action.SelectedIndex) == action.SelectedInfo;
        }

        public void Unbind()
        {
            Items = null;
            listState.Unbind();
        }

        private EnemyContentAction CreateAction(UiVerticalListActivation activation)
        {
            if (!activation.IsTriggered
                || Items == null
                || activation.SelectedIndex < 0
                || activation.SelectedIndex >= ItemsCount)
            {
                return EnemyContentAction.None;
            }

            InfoItem info = (InfoItem)Items.elementAt(activation.SelectedIndex);
            return EnemyContentAction.CreateOpenActions(
                activation.SelectedIndex,
                info,
                activation.MenuY,
                activation.DelayFrames,
                activation.BindingRevision
            );
        }
    }
}
