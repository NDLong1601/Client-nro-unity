using System.Collections.Generic;

namespace Game2.Mod.XMAP
{
	public class XmapController : IActionListener
	{
		private static readonly string listMap = "Làng Aru,Đồi hoa cúc,Thung lũng tre,Rừng nấm,Rừng xương,Đảo Kamê,Đông Karin,Làng Mori,Đồi nấm tím,Thị trấn Moori,Thung lũng Namếc,Thung lũng Maima,Vực maima,Đảo Guru,Làng Kakarot,Đồi hoang,Làng Plant,Rừng nguyên sinh,Rừng thông Xayda,Thành phố Vegeta,Vách núi đen,Nhà Gôhan,Nhà Moori,Nhà Broly,Trạm tàu vũ trụ,Trạm tàu vũ trụ,Trạm tàu vũ trụ,Rừng Bamboo,Rừng dương xỉ,Nam Kamê,Đảo Bulông,Núi hoa vàng,Núi hoa tím,Nam Guru,Đông Nam Guru,Rừng cọ,Rừng đá,Thung lũng đen,Bờ vực đen,Vách núi Aru,Vách núi Moori,Vực Plant,Vách núi Aru,Vách núi Moori,Vách núi Kakarot,Thần điện,Tháp Karin,Rừng Karin,Hành tinh Kaio,Phòng tập thời gian,Thánh địa Kaio,Đấu trường,Đại hội võ thuật,Tường thành 1,Tầng 3,Tầng 1,Tầng 2,Tầng 4,Tường thành 2,Tường thành 3,Trại độc nhãn 1,Trại độc nhãn 2,Trại độc nhãn 3,Trại lính Fide,Núi dây leo,Núi cây quỷ,Trại qủy già,Vực chết,Thung lũng Nappa,Vực cấm,Núi Appule,Căn cứ Raspberry,Thung lũng Raspberry,Thung lũng chết,Đồi cây Fide,Khe núi tử thần,Núi đá,Rừng đá,Lãnh  địa Fize,Núi khỉ đỏ,Núi khỉ vàng,Hang quỷ chim,Núi khỉ đen,Hang khỉ đen,Siêu Thị,Hành tinh M-2,Hành tinh Polaris,Hành tinh Cretaceous,Hành tinh Monmaasu,Hành tinh Rudeeze,Hành tinh Gelbo,Hành tinh Tigere,Thành phố phía đông,Thành phố phía nam,Đảo Balê,95,Cao nguyên,Thành phố phía bắc,Ngọn núi phía bắc,Thung lũng phía bắc,Thị trấn Ginder,101,Nhà Bunma,Võ đài Xên bọ hung,Sân sau siêu thị,Cánh đồng tuyết,Rừng tuyết,Núi tuyết,Dòng sông băng,Rừng băng,Hang băng,Đông Nam Karin,Võ đài Hạt Mít,Đại hội võ thuật,Cổng phi thuyền,Phòng chờ,Thánh địa Kaio,Cửa Ải 1,Cửa Ải 2,Cửa Ải 3,Phòng chỉ huy,Đấu trường,Ngũ Hành Sơn,Ngũ Hành Sơn,Ngũ Hành Sơn,Võ đài Bang,Thành phố Santa,Cổng phi thuyền,Bụng Mabư,Đại hội võ thuật,Đại hội võ thuật Vũ Trụ,Hành Tinh Yardart,Hành Tinh Yardart 2,Hành Tinh Yardart 3,Đại hội võ thuật Vũ Trụ 6-7,Động hải tặc,Hang Bạch Tuộc,Động kho báu,Cảng hải tặc,Hành tinh Potaufeu,Hang động Potaufeu,Con đường rắn độc,Con đường rắn độc,Con đường rắn độc,Hoang mạc,Võ Đài Siêu Cấp,Tây Karin,Sa mạc,Lâu đài Lychee,Thành phố Santa,Lôi Đài,Hành tinh bóng tối,Vùng đất băng giá,Lãnh địa bang hội,Hành tinh Bill,Hành tinh ngục tù,Tây thánh địa,Đông thánh Địa,Bắc thánh địa,Nam thánh Địa,Khu hang động,Bìa rừng nguyên thủy,Rừng nguyên thủy,Làng Plant nguyên thủy,Tranh ngọc Namếc";

		public static readonly string[] mapNames = listMap.Split(new char[1] { ',' });

		public static int VuDangMapNext;

		private static int step;

		private static readonly XmapController _Instance = new XmapController();

		public static int IdMapEnd;

		private static List<int> WayXmap;

		private static int IndexWay;

		private static bool IsNextMapFailed;

		private static bool IsWait;

		private static long TimeStartWait;

		private static long TimeWait;

		private static bool IsWaitNextMap;

		public static void Update()
		{
			if (IsWaiting() || XmapData.GI().IsLoading)
			{
				return;
			}
			if (IsWaitNextMap)
			{
				Wait(100);
				IsWaitNextMap = false;
				return;
			}
			if (IsNextMapFailed)
			{
				XmapData.GI().MyLinkMaps = null;
				WayXmap = null;
				IsNextMapFailed = false;
				return;
			}
			if (WayXmap == null)
			{
				if (XmapData.GI().MyLinkMaps == null)
				{
					XmapData.GI().LoadLinkMaps();
					return;
				}
				WayXmap = XmapAlgorithm.FindWay(TileMap.mapID, IdMapEnd);
				IndexWay = 0;
				if (WayXmap == null)
				{
					GameScr.info1.addInfo("Không thể tìm thấy đường đi", 0);
					FinishXmap();
					return;
				}
			}
			int mapID = TileMap.mapID;
			List<int> wayXmap = WayXmap;
			if (mapID == wayXmap[wayXmap.Count - 1] && !XmapData.IsMyCharDie())
			{
				FinishXmap();
			}
			else if (TileMap.mapID == WayXmap[IndexWay])
			{
				if (XmapData.IsMyCharDie())
				{
					Service.gI().returnTownFromDead();
					IsWaitNextMap = (IsNextMapFailed = true);
				}
				else if (XmapData.CanNextMap())
				{
					NextMap(WayXmap[IndexWay + 1]);
					IsWaitNextMap = true;
				}
				Wait(500);
			}
			else if (TileMap.mapID == WayXmap[IndexWay + 1])
			{
				IndexWay++;
			}
			else
			{
				IsNextMapFailed = true;
			}
		}

		public void perform(int idAction, object p)
		{
			if (idAction == 1)
			{
				ShowPanelXmap((List<int>)p);
			}
		}

		private static void Wait(int time)
		{
			IsWait = true;
			TimeStartWait = mSystem.currentTimeMillis();
			TimeWait = time;
		}

		private static bool IsWaiting()
		{
			if (IsWait && mSystem.currentTimeMillis() - TimeStartWait >= TimeWait)
			{
				IsWait = false;
			}
			return IsWait;
		}

		public static void ShowXmapMenu()
		{
			XmapData.GI().LoadGroupMapsFromFile();
			MyVector myVector = new MyVector();
			foreach (GroupMap groupMap in XmapData.GI().GroupMaps)
			{
				myVector.addElement(new Command(groupMap.NameGroup, _Instance, 1, groupMap.IdMaps));
			}
			GameCanvas.menu.startAt(myVector, 3);
		}

		public static string GetMapName(int id)
		{
			string result = TileMap.mapName;
			if (id >= 0 && id < mapNames.Length && mapNames[id] != null && mapNames[id] != string.Empty)
			{
				result = mapNames[id];
			}
			return result;
		}

		public static void ShowPanelXmap(List<int> idMaps)
		{
			AutoXmap.IsMapTransAsXmap = true;
			int count = idMaps.Count;
			GameCanvas.panel.mapNames = new string[count];
			GameCanvas.panel.planetNames = new string[count];
			for (int i = 0; i < count; i++)
			{
				string mapName = GetMapName(idMaps[i]);
				GameCanvas.panel.mapNames[i] = mapName + " [" + idMaps[i] + "]";
				GameCanvas.panel.planetNames[i] = "";
			}
			GameCanvas.panel.setTypeMapTrans();
			GameCanvas.panel.show();
		}

		public static void StartRunToMapId(int idMap)
		{
			IdMapEnd = idMap;
			AutoXmap.IsXmapRunning = true;
		}

		public static void FinishXmap()
		{
			AutoXmap.IsXmapRunning = false;
			IsNextMapFailed = false;
			XmapData.GI().MyLinkMaps = null;
			WayXmap = null;
			GameCanvas.panel.hide();
		}

		public static void SaveIdMapCapsuleReturn()
		{
			AutoXmap.IdMapCapsuleReturn = TileMap.mapID;
		}

		private static void NextMap(int idMapNext)
		{
			VuDangMapNext = idMapNext;
			List<MapNext> mapNexts = XmapData.GI().GetMapNexts(TileMap.mapID);
			if (mapNexts != null)
			{
				foreach (MapNext item in mapNexts)
				{
					if (item.MapID == idMapNext)
					{
						NextMap(item);
						return;
					}
				}
			}
			GameScr.info1.addInfo("Lỗi tại dữ liệu", 0);
		}

		private static void NextMap(MapNext mapNext)
		{
			switch (mapNext.Type)
			{
			case TypeMapNext.AutoWaypoint:
				NextMapAutoWaypoint(mapNext);
				break;
			case TypeMapNext.NpcMenu:
				NextMapNpcMenu(mapNext);
				break;
			case TypeMapNext.NpcPanel:
				NextMapNpcPanel(mapNext);
				break;
			case TypeMapNext.Position:
				NextMapPosition(mapNext);
				break;
			case TypeMapNext.Capsule:
				NextMapCapsule(mapNext);
				break;
			}
		}

		private static void NextMapAutoWaypoint(MapNext mapNext)
		{
			Waypoint waypoint = XmapData.FindWaypoint(mapNext.MapID);
			if (waypoint != null)
			{
				int posWaypointX = XmapData.GetPosWaypointX(waypoint);
				int posWaypointY = XmapData.GetPosWaypointY(waypoint);
				ModFunc.GI().MoveTo(posWaypointX, posWaypointY);
				if (Char.myCharz().isInEnterOnlinePoint() != null)
				{
					waypoint.popup.command.performAction();
				}
			}
		}

		private static void NextMapNpcMenu(MapNext mapNext)
		{
			int num = mapNext.Info[0];
			if (GameScr.findNPCInMap((short)num) == null)
			{
				Fixtl();
				return;
			}
			ModFunc.GI().GotoNpc(num);
			Service.gI().openMenu(num);
			for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
			{
				if (((Command)GameCanvas.menu.menuItems.elementAt(i)).caption.Trim().ToLower().Contains("võ thuật") && VuDangMapNext == 129)
				{
					Service.gI().confirmMenu((short)num, (sbyte)i);
					return;
				}
				if (((Command)GameCanvas.menu.menuItems.elementAt(i)).caption.Trim().ToLower().Contains("tương lai") && VuDangMapNext >= 92 && VuDangMapNext <= 103)
				{
					Service.gI().confirmMenu((short)num, (sbyte)i);
					return;
				}
				if (((Command)GameCanvas.menu.menuItems.elementAt(i)).caption.Trim().ToLower().Contains("yardart") && VuDangMapNext >= 131 && VuDangMapNext <= 133)
				{
					Service.gI().confirmMenu((short)num, (sbyte)i);
					return;
				}
				if (VuDangMapNext >= 161 && VuDangMapNext <= 164)
				{
					if (((Command)GameCanvas.menu.menuItems.elementAt(i)).caption.Trim().ToLower().Contains("thực vật"))
					{
						Service.gI().confirmMenu((short)num, (sbyte)i);
					}
					else
					{
						Service.gI().useItem(0, 1, (sbyte)ModFunc.GI().FindItemIndex(992), -1);
					}
					return;
				}
			}
			for (int j = 1; j < mapNext.Info.Length; j++)
			{
				int num2 = mapNext.Info[j];
				Service.gI().confirmMenu((short)num, (sbyte)num2);
			}
		}

		private static void Fixtl()
		{
			if (TileMap.mapID == 27)
			{
				NextMap(28);
				IsWaitNextMap = true;
				step = 0;
			}
			else if (TileMap.mapID == 29)
			{
				NextMap(28);
				IsWaitNextMap = true;
				step = 1;
			}
			else if (step == 0)
			{
				NextMap(29);
				IsWaitNextMap = true;
			}
			else if (step == 1)
			{
				NextMap(27);
				IsWaitNextMap = true;
			}
		}

		private static void NextMapNpcPanel(MapNext mapNext)
		{
			int num = mapNext.Info[0];
			int num2 = mapNext.Info[1];
			int selected = mapNext.Info[2];
			ModFunc.GI().GotoNpc(num);
			Service.gI().openMenu(num);
			Service.gI().confirmMenu((short)num, (sbyte)num2);
			Service.gI().requestMapSelect(selected);
		}

		private static void NextMapPosition(MapNext mapNext)
		{
			GameCanvas.startOKDlg("Đây là lỗi, vui lòng báo cáo lại với ADMIN");
			InfoDlg.hide();
		}

		private static void NextMapCapsule(MapNext mapNext)
		{
			SaveIdMapCapsuleReturn();
			int selected = mapNext.Info[0];
			Service.gI().requestMapSelect(selected);
		}

		public static void UseCapsuleNormal()
		{
			AutoXmap.IsShowPanelMapTrans = false;
			Service.gI().useItem(0, 1, (sbyte)ModFunc.GI().FindItemIndex(193), -1);
		}

		public static void UseCapsuleVip()
		{
			AutoXmap.IsShowPanelMapTrans = false;
			Service.gI().useItem(0, 1, (sbyte)ModFunc.GI().FindItemIndex(194), -1);
		}

		public static void HideInfoDlg()
		{
			InfoDlg.hide();
		}
	}
}
