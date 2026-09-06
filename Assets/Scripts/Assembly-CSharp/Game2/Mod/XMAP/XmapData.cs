using System;
using System.Collections.Generic;
using System.Text;

namespace Game2.Mod.XMAP
{
	public class XmapData
	{
		public string[] DataXMap = new string[37]
		{
			"24 25 1 10 0", "25 24 1 11 0", "45 48 1 19 3", "48 45 1 20 3 0", "48 50 1 20 3 1", "50 48 1 44 0", "24 26 1 10 1", "26 24 1 12 0", "25 26 1 11 1", "26 25 1 12 1",
			"24 84 1 10 2", "25 84 1 11 2", "26 84 1 12 2", "19 68 1 12 1", "68 19 1 12 0", "80 131 1 60 0", "27 102 1 38 1", "28 102 1 38 1", "29 102 1 38 1", "102 24 1 38 1",
			"27 53 1 25 0", "52 129 1 23 3", "0 149 1 67 3 0", "45 48 1 19 3", "50 48 1 44 0", "139 25 1 63 1", "139 26 1 63 2", "24 139 1 63 0", "139 24 1 63 0", "19 126 1 53 0",
			"126 19 1 53 0", "24 139 1 63 0", "139 24 1 63 0", "19 126 1 53 0", "126 19 1 53 0", "0 122 1 49 0", "80 160 1 60 0"
		};

		public string[] DataWayPoint = new string[29]
		{
			"42 0 1 2 3 4 5 6", "3 27 28 29 30", "2 24", "1 47 46 45", "5 29", "47 111", "53 58 59 60 61 62 55 56 54 57", "53 27", "43 7 8 9 11 12 13 10", "11 31 32 33 34",
			"9 25", "13 33", "52 44 14 15 16 17 18 20 19", "17 35 36 37 38", "16 26", "20 37", "68 69 70 71 72 64 65 63 66 67 73 74 75 76 77 81 82 83 79 80", "102 92 93 94 96 97 98 99 100 103", "109 108 107 110 106", "109 105",
			"109 106", "106 107", "108 105", "131 132 133", "80 105", "160 161 162 163", "139 140", "149 147 152 151 148", "122 123 124"
		};

		public List<GroupMap> GroupMaps;

		public Dictionary<int, List<MapNext>> MyLinkMaps;

		public bool IsLoading;

		private bool IsLoadingCapsule;

		private static XmapData Instance;

		private XmapData()
		{
			GroupMaps = new List<GroupMap>();
			MyLinkMaps = null;
			IsLoading = false;
			IsLoadingCapsule = false;
		}

		public static XmapData GI()
		{
			if (Instance == null)
			{
				Instance = new XmapData();
			}
			return Instance;
		}

		public void LoadLinkMaps()
		{
			IsLoading = true;
		}

		public void Update()
		{
			if (IsLoadingCapsule)
			{
				if (!IsWaitInfoMapTrans())
				{
					LoadLinkMapCapsule();
					IsLoadingCapsule = false;
					IsLoading = false;
				}
				return;
			}
			LoadLinkMapBase();
			if (CanUseCapsuleVip())
			{
				XmapController.UseCapsuleVip();
				IsLoadingCapsule = true;
			}
			else if (CanUseCapsuleNormal())
			{
				XmapController.UseCapsuleNormal();
				IsLoadingCapsule = true;
			}
			else
			{
				IsLoading = false;
			}
		}

		public void LoadGroupMapsFromFile()
		{
			GroupMapData groupMapData = new GroupMapData(new List<int>
			{
				44, 23, 14, 15, 16, 17, 18, 20, 19, 35,
				36, 37, 38, 26, 52, 84, 129
			}, "Xayda");
			GroupMapData groupMapData2 = new GroupMapData(new List<int>
			{
				42, 21, 0, 1, 2, 3, 4, 5, 6, 27,
				28, 29, 30, 47, 46, 45, 48, 50, 111, 24,
				53, 58, 59, 60, 61, 62, 55, 56, 54, 57
			}, "Trái Đất");
			GroupMapData groupMapData3 = new GroupMapData(new List<int>
			{
				43, 22, 7, 8, 9, 11, 12, 13, 10, 31,
				32, 33, 34, 25
			}, "Namek");
			GroupMapData groupMapData4 = new GroupMapData(new List<int>
			{
				68, 69, 70, 71, 72, 64, 65, 63, 66, 67,
				73, 74, 75, 76, 77, 81, 82, 83, 79, 80,
				131, 132, 133
			}, "Nappa");
			GroupMapData groupMapData5 = new GroupMapData(new List<int> { 102, 92, 93, 94, 96, 97, 98, 99, 100, 103 }, "Tương Lai");
			GroupMapData groupMapData6 = new GroupMapData(new List<int> { 109, 108, 107, 110, 106, 105 }, "Cold");
			GroupMapData groupMapData7 = new GroupMapData(new List<int> { 122, 123, 124 }, "Ngũ Hành Sơn");
			GroupMaps.Clear();
			try
			{
				GroupMaps.Add(new GroupMap(groupMapData.mapname, groupMapData.datamap));
				GroupMaps.Add(new GroupMap(groupMapData2.mapname, groupMapData2.datamap));
				GroupMaps.Add(new GroupMap(groupMapData3.mapname, groupMapData3.datamap));
				GroupMaps.Add(new GroupMap(groupMapData4.mapname, groupMapData4.datamap));
				GroupMaps.Add(new GroupMap(groupMapData5.mapname, groupMapData5.datamap));
				GroupMaps.Add(new GroupMap(groupMapData6.mapname, groupMapData6.datamap));
				GroupMaps.Add(new GroupMap(groupMapData7.mapname, groupMapData7.datamap));
			}
			catch (Exception ex)
			{
				GameScr.info1.addInfo(ex.Message, 0);
			}
			RemoveMapsHomeInGroupMaps();
		}

		private void RemoveMapsHomeInGroupMaps()
		{
			int cgender = Char.myCharz().cgender;
			foreach (GroupMap groupMap in GroupMaps)
			{
				switch (cgender)
				{
				default:
					groupMap.IdMaps.Remove(21);
					groupMap.IdMaps.Remove(22);
					break;
				case 1:
					groupMap.IdMaps.Remove(21);
					groupMap.IdMaps.Remove(23);
					break;
				case 0:
					groupMap.IdMaps.Remove(22);
					groupMap.IdMaps.Remove(23);
					break;
				}
			}
		}

		private void LoadLinkMapCapsule()
		{
			AddKeyLinkMaps(TileMap.mapID);
			string[] mapNames = GameCanvas.panel.mapNames;
			for (int i = 0; i < mapNames.Length; i++)
			{
				int idMapFromName = GetIdMapFromName(mapNames[i]);
				if (idMapFromName != -1)
				{
					int[] info = new int[1] { i };
					MyLinkMaps[TileMap.mapID].Add(new MapNext(idMapFromName, TypeMapNext.Capsule, info));
				}
			}
		}

		private void LoadLinkMapBase()
		{
			MyLinkMaps = new Dictionary<int, List<MapNext>>();
			LoadLinkMapsFromFile();
			LoadLinkMapsAutoWaypointFromFile();
			LoadLinkMapsHome();
			LoadLinkMapSieuThi();
			LoadLinkMapToCold();
		}

		private void LoadLinkMapsFromFile()
		{
			try
			{
				for (int i = 0; i < DataXMap.Length; i++)
				{
					string text;
					if ((text = DataXMap[i]) == null)
					{
						continue;
					}
					text = text.Trim();
					if (!text.StartsWith("#") && !text.Equals(""))
					{
						int[] array = Array.ConvertAll(text.Split(new char[1] { ' ' }), (string s) => int.Parse(s));
						int num = array.Length - 3;
						int[] array2 = new int[num];
						Array.Copy(array, 3, array2, 0, num);
						LoadLinkMap(array[0], array[1], (TypeMapNext)array[2], array2);
					}
				}
			}
			catch (Exception ex)
			{
				GameScr.info1.addInfo(ex.Message, 0);
			}
		}

		private void LoadLinkMapsAutoWaypointFromFile()
		{
			try
			{
				for (int i = 0; i < DataWayPoint.Length; i++)
				{
					string text;
					if ((text = DataWayPoint[i]) == null)
					{
						continue;
					}
					text = text.Trim();
					if (text.StartsWith("#") || text.Equals(""))
					{
						continue;
					}
					int[] array = Array.ConvertAll(text.Split(new char[1] { ' ' }), (string s) => int.Parse(s));
					for (int j = 0; j < array.Length; j++)
					{
						if (j != 0)
						{
							LoadLinkMap(array[j], array[j - 1], TypeMapNext.AutoWaypoint, null);
						}
						if (j != array.Length - 1)
						{
							LoadLinkMap(array[j], array[j + 1], TypeMapNext.AutoWaypoint, null);
						}
					}
				}
			}
			catch (Exception ex)
			{
				GameScr.info1.addInfo(ex.Message, 0);
			}
		}

		private void LoadLinkMapsHome()
		{
			int cgender = Char.myCharz().cgender;
			int num = 21 + cgender;
			int num2 = 7 * cgender;
			LoadLinkMap(num2, num, TypeMapNext.AutoWaypoint, null);
			LoadLinkMap(num, num2, TypeMapNext.AutoWaypoint, null);
		}

		private void LoadLinkMapSieuThi()
		{
			int cgender = Char.myCharz().cgender;
			int idMapNext = 24 + cgender;
			int[] info = new int[2] { 10, 0 };
			LoadLinkMap(84, idMapNext, TypeMapNext.NpcMenu, info);
		}

		private void LoadLinkMapToCold()
		{
			if (Char.myCharz().taskMaint.taskId > 30)
			{
				int[] info = new int[2] { 12, 0 };
				LoadLinkMap(19, 109, TypeMapNext.NpcMenu, info);
			}
		}

		public List<MapNext> GetMapNexts(int idMap)
		{
			if (CanGetMapNexts(idMap))
			{
				return MyLinkMaps[idMap];
			}
			return null;
		}

		public bool CanGetMapNexts(int idMap)
		{
			return MyLinkMaps.ContainsKey(idMap);
		}

		private void LoadLinkMap(int idMapStart, int idMapNext, TypeMapNext type, int[] info)
		{
			AddKeyLinkMaps(idMapStart);
			MapNext item = new MapNext(idMapNext, type, info);
			MyLinkMaps[idMapStart].Add(item);
		}

		private void AddKeyLinkMaps(int idMap)
		{
			if (!MyLinkMaps.ContainsKey(idMap))
			{
				MyLinkMaps.Add(idMap, new List<MapNext>());
			}
		}

		private bool IsWaitInfoMapTrans()
		{
			return !AutoXmap.IsShowPanelMapTrans;
		}

		public static int GetIdMapFromPanelXmap(string mapName)
		{
			return int.Parse(mapName.Split(new char[1] { '[' })[1].Trim(']'));
		}

		public static Waypoint FindWaypoint(int idMap)
		{
			for (int i = 0; i < TileMap.vGo.size(); i++)
			{
				Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);
				if (XmapController.IdMapEnd == 124 && TileMap.mapID == 123)
				{
					for (int j = 0; j < TileMap.vGo.size(); j++)
					{
						Waypoint result = (Waypoint)TileMap.vGo.elementAt(j);
						if (j == TileMap.vGo.size() - 1)
						{
							return result;
						}
					}
				}
				if (GetTextPopup(waypoint.popup).Trim().ToLower().Equals(XmapController.GetMapName(idMap).Trim().ToLower()))
				{
					return waypoint;
				}
			}
			return null;
		}

		public static int GetPosWaypointX(Waypoint waypoint)
		{
			if (waypoint.maxX < 60)
			{
				return 15;
			}
			if (waypoint.minX > TileMap.pxw - 60)
			{
				return TileMap.pxw - 15;
			}
			return waypoint.minX + 30;
		}

		public static int GetPosWaypointY(Waypoint waypoint)
		{
			return waypoint.maxY;
		}

		public static bool IsMyCharDie()
		{
			if (Char.myCharz().statusMe != 14)
			{
				return Char.myCharz().cHP <= 0;
			}
			return true;
		}

		public static bool CanNextMap()
		{
			if (!Char.isLoadingMap && !Char.ischangingMap)
			{
				return !Controller.isStopReadMessage;
			}
			return false;
		}

		private static int GetIdMapFromName(string mapName)
		{
			int cgender = Char.myCharz().cgender;
			if (mapName.Equals("Về nhà"))
			{
				return 21 + cgender;
			}
			if (mapName.Equals("Trạm tàu vũ trụ"))
			{
				return 24 + cgender;
			}
			if (mapName.Contains("Về chỗ cũ: "))
			{
				mapName = mapName.Replace("Về chỗ cũ: ", "");
				if (XmapController.GetMapName(AutoXmap.IdMapCapsuleReturn).Equals(mapName))
				{
					return AutoXmap.IdMapCapsuleReturn;
				}
				if (mapName.Equals("Rừng đá"))
				{
					return -1;
				}
			}
			for (int i = 0; i < TileMap.mapNames.Length; i++)
			{
				if (mapName.Equals(XmapController.GetMapName(i)))
				{
					return i;
				}
			}
			return -1;
		}

		public static string GetTextPopup(PopUp popUp)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < popUp.says.Length; i++)
			{
				stringBuilder.Append(popUp.says[i]);
				stringBuilder.Append(" ");
			}
			return stringBuilder.ToString().Trim();
		}

		private static bool CanUseCapsuleNormal()
		{
			if (!IsMyCharDie() && AutoXmap.IsUseCapsuleNormal)
			{
				return HasItemCapsuleNormal();
			}
			return false;
		}

		private static bool HasItemCapsuleNormal()
		{
			Item[] arrItemBag = Char.myCharz().arrItemBag;
			for (int i = 0; i < arrItemBag.Length; i++)
			{
				if (arrItemBag[i] != null && arrItemBag[i].template.id == 193)
				{
					return true;
				}
			}
			return false;
		}

		private static bool CanUseCapsuleVip()
		{
			if (!IsMyCharDie() && AutoXmap.IsUseCapsuleVip)
			{
				return HasItemCapsuleVip();
			}
			return false;
		}

		private static bool HasItemCapsuleVip()
		{
			Item[] arrItemBag = Char.myCharz().arrItemBag;
			for (int i = 0; i < arrItemBag.Length; i++)
			{
				if (arrItemBag[i] != null && arrItemBag[i].template.id == 194)
				{
					return true;
				}
			}
			return false;
		}
	}
}
