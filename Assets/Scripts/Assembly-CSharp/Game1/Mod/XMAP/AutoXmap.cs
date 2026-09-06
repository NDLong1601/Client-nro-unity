namespace Game1.Mod.XMAP
{
	public class AutoXmap
	{
		public static bool IsXmapRunning = false;

		public static bool IsMapTransAsXmap = false;

		public static bool IsShowPanelMapTrans = true;

		public static bool IsUseCapsuleNormal = true;

		public static bool IsUseCapsuleVip = true;

		public static int IdMapCapsuleReturn = -1;

		public static void Update()
		{
			if (XmapData.GI().IsLoading)
			{
				XmapData.GI().Update();
			}
			if (IsXmapRunning)
			{
				XmapController.Update();
			}
		}

		public static void Info(string text)
		{
			if (text.Equals("Bạn chưa thể đến khu vực này"))
			{
				XmapController.FinishXmap();
			}
			if ((text.ToLower().Contains("chức năng bảo vệ") || text.ToLower().Contains("đã hủy xmap")) && IsXmapRunning)
			{
				XmapController.FinishXmap();
			}
		}

		public static void SelectMapTrans(int selected)
		{
			if (IsMapTransAsXmap)
			{
				XmapController.HideInfoDlg();
				XmapController.StartRunToMapId(XmapData.GetIdMapFromPanelXmap(GameCanvas.panel.mapNames[selected]));
			}
			else
			{
				XmapController.SaveIdMapCapsuleReturn();
				Service.gI().requestMapSelect(selected);
			}
		}

		public static void ShowPanelMapTrans()
		{
			IsMapTransAsXmap = false;
			if (IsShowPanelMapTrans)
			{
				GameCanvas.panel.setTypeMapTrans();
				GameCanvas.panel.show();
			}
			else
			{
				IsShowPanelMapTrans = true;
			}
		}

		public static void FixBlackScreen()
		{
			Controller.gI().loadCurrMap(0);
			Service.gI().finishLoadMap();
			Char.isLoadingMap = false;
		}
	}
}
