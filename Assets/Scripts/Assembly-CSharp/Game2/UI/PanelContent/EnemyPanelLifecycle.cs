namespace Game2.UI.PanelContent
{
    public enum EnemyListDisposition
    {
        CacheOnly = 0,
        OpenRequested = 1,
        RefreshCurrent = 2
    }

    public struct EnemyMenuContext
    {
        public readonly int DataRevision;
        public readonly InfoItem Target;
        public readonly int CharId;

        public EnemyMenuContext(int dataRevision, InfoItem target)
        {
            DataRevision = dataRevision;
            Target = target;
            CharId = (target != null && target.charInfo != null) ? target.charInfo.charID : -1;
        }

        public bool IsValid
        {
            get
            {
                return DataRevision > 0 && Target != null && CharId != -1;
            }
        }
    }

    public class EnemyPanelLifecycle
    {
        public bool IsOpenRequested { get; private set; }
        public bool IsWaitDialogOwned { get; private set; }
        public int DataRevision { get; private set; }
        public EnemyMenuContext ActiveMenuContext { get; private set; }

        public void BeginRequest()
        {
            IsOpenRequested = true;
            BeginWaitDialog();
        }

        public void BeginWaitDialog()
        {
            IsWaitDialogOwned = true;
        }

        public bool ConsumeWaitDialogOwnership()
        {
            bool wasOwned = IsWaitDialogOwned;
            IsWaitDialogOwned = false;
            return wasOwned;
        }

        public void CancelRequest()
        {
            IsOpenRequested = false;
        }

        public EnemyListDisposition OnListReceived(bool isEnemyOpen)
        {
            DataRevision++;
            InvalidateMenuContext();

            if (isEnemyOpen)
            {
                IsOpenRequested = false;
                return EnemyListDisposition.RefreshCurrent;
            }

            if (IsOpenRequested)
            {
                IsOpenRequested = false;
                return EnemyListDisposition.OpenRequested;
            }

            return EnemyListDisposition.CacheOnly;
        }

        public void OpenMenu(InfoItem target)
        {
            ActiveMenuContext = new EnemyMenuContext(DataRevision, target);
        }

        public void InvalidateMenuContext()
        {
            ActiveMenuContext = default(EnemyMenuContext);
        }

        public bool IsMenuContextValid(object p, bool isEnemyOpen, MyVector currentList)
        {
            if (!ActiveMenuContext.IsValid)
            {
                return false;
            }
            if (!isEnemyOpen)
            {
                return false;
            }
            if (ActiveMenuContext.DataRevision != DataRevision)
            {
                return false;
            }
            InfoItem target = p as InfoItem;
            if (target == null || target != ActiveMenuContext.Target)
            {
                return false;
            }
            if (target.charInfo == null || target.charInfo.charID != ActiveMenuContext.CharId)
            {
                return false;
            }
            if (currentList != null && !currentList.contains(target))
            {
                return false;
            }
            return true;
        }

        public bool OnLeavingType()
        {
            bool shouldHideWaitDialog = ConsumeWaitDialogOwnership();
            CancelRequest();
            InvalidateMenuContext();
            return shouldHideWaitDialog;
        }
    }
}
