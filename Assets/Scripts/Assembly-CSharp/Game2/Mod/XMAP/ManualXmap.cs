using System.Text;

namespace Game2.Mod.XMAP
{
	public class ManualXmap
	{
		public static ManualXmap _Instance;

		private static int[] wayPointMapLeft;

		private static int[] wayPointMapCenter;

		private static int[] wayPointMapRight;

		public static ManualXmap GI()
		{
			if (_Instance == null)
			{
				_Instance = new ManualXmap();
			}
			return _Instance;
		}

		public void LoadMapLeft()
		{
			LoadMap(0);
		}

		public void LoadMapCenter()
		{
			LoadMap(1);
		}

		public void LoadMapRight()
		{
			LoadMap(2);
		}

		private static void LoadWaypointsInMap()
		{
			ResetSavedWaypoints();
			int num = TileMap.vGo.size();
			if (num != 2)
			{
				for (int i = 0; i < num; i++)
				{
					Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);
					if (waypoint.maxX < 60)
					{
						wayPointMapLeft[0] = waypoint.minX + 15;
						wayPointMapLeft[1] = waypoint.maxY;
					}
					else if (waypoint.maxX > TileMap.pxw - 60)
					{
						wayPointMapRight[0] = waypoint.maxX - 15;
						wayPointMapRight[1] = waypoint.maxY;
					}
					else
					{
						wayPointMapCenter[0] = waypoint.minX + 15;
						wayPointMapCenter[1] = waypoint.maxY;
					}
				}
				return;
			}
			Waypoint waypoint2 = (Waypoint)TileMap.vGo.elementAt(0);
			Waypoint waypoint3 = (Waypoint)TileMap.vGo.elementAt(1);
			if ((waypoint2.maxX < 60 && waypoint3.maxX < 60) || (waypoint2.minX > TileMap.pxw - 60 && waypoint3.minX > TileMap.pxw - 60))
			{
				wayPointMapLeft[0] = waypoint2.minX + 15;
				wayPointMapLeft[1] = waypoint2.maxY;
				wayPointMapRight[0] = waypoint3.maxX - 15;
				wayPointMapRight[1] = waypoint3.maxY;
			}
			else if (waypoint2.maxX < waypoint3.maxX)
			{
				wayPointMapLeft[0] = waypoint2.minX + 15;
				wayPointMapLeft[1] = waypoint2.maxY;
				wayPointMapRight[0] = waypoint3.maxX - 15;
				wayPointMapRight[1] = waypoint3.maxY;
			}
			else
			{
				wayPointMapLeft[0] = waypoint3.minX + 15;
				wayPointMapLeft[1] = waypoint3.maxY;
				wayPointMapRight[0] = waypoint2.maxX - 15;
				wayPointMapRight[1] = waypoint2.maxY;
			}
		}

		private static int GetYGround(int x)
		{
			int num = 50;
			int num2 = 0;
			while (num2 < 30)
			{
				num2++;
				num += 24;
				if (TileMap.tileTypeAt(x, num, 2))
				{
					if (num % 24 != 0)
					{
						num -= num % 24;
					}
					break;
				}
			}
			return num;
		}

		private static void ResetSavedWaypoints()
		{
			wayPointMapLeft = new int[2];
			wayPointMapCenter = new int[2];
			wayPointMapRight = new int[2];
		}

		private static bool IsNRDMap(int mapID)
		{
			if (mapID >= 85)
			{
				return mapID <= 91;
			}
			return false;
		}

		private static bool IsKarinMap(int mapID)
		{
			if (mapID >= 45)
			{
				return mapID <= 47;
			}
			return false;
		}

		public static void LoadMap(int position)
		{
			if (IsKarinMap(TileMap.mapID))
			{
				return;
			}
			if (IsNRDMap(TileMap.mapID))
			{
				TeleportInNRDMap(position);
				return;
			}
			Waypoint waypoint = FindWaypoint(position);
			if (waypoint != null)
			{
				int targetX = GetTargetX(waypoint);
				ModFunc.GI().MoveTo(targetX, waypoint.maxY);
				if (ShouldRequestChangeMap(position, waypoint))
				{
					RequestChangeMap(waypoint);
				}
			}
			else
			{
				MoveToDefaultPosition(position);
			}
		}

		private static void MoveToDefaultPosition(int position)
		{
			switch (position)
			{
			case 0:
				ModFunc.GI().MoveTo(60, GetYGround(60));
				break;
			case 1:
				ModFunc.GI().MoveTo(TileMap.pxw / 2, GetYGround(TileMap.pxw / 2));
				break;
			case 2:
				ModFunc.GI().MoveTo(TileMap.pxw - 60, GetYGround(TileMap.pxw - 60));
				break;
			}
		}

		private static int GetTargetX(Waypoint waypoint)
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

		private static bool ShouldRequestChangeMap(int position, Waypoint waypoint)
		{
			if (position == 1 || TileMap.vGo.size() == 1)
			{
				return true;
			}
			Char @char = Char.myCharz();
			if (@char.isInEnterOfflinePoint() == null)
			{
				return @char.isInEnterOnlinePoint() != null;
			}
			return true;
		}

		public static Waypoint FindWaypoint(int position)
		{
			if (TileMap.vGo.size() == 0)
			{
				return null;
			}
			if (TileMap.vGo.size() == 1)
			{
				return (Waypoint)TileMap.vGo.elementAt(0);
			}
			LoadWaypointsInMap();
			for (int i = 0; i < TileMap.vGo.size(); i++)
			{
				Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);
				if (IsMatchingWaypoint(waypoint, position))
				{
					return waypoint;
				}
			}
			return null;
		}

		private static bool IsMatchingWaypoint(Waypoint waypoint, int position)
		{
			return position switch
			{
				0 => IsLeftWaypoint(waypoint), 
				1 => IsCenterWaypoint(waypoint), 
				2 => IsRightWaypoint(waypoint), 
				_ => false, 
			};
		}

		private static bool IsLeftWaypoint(Waypoint waypoint)
		{
			if ((TileMap.mapID != 70 || !(GetTextPopup(waypoint.popup) == "Vực cấm")) && (TileMap.mapID != 73 || !(GetTextPopup(waypoint.popup) == "Vực chết")) && (TileMap.mapID != 110 || !(GetTextPopup(waypoint.popup) == "Rừng tuyết")))
			{
				return waypoint.maxX < 60;
			}
			return true;
		}

		private static bool IsCenterWaypoint(Waypoint waypoint)
		{
			if (TileMap.mapID != 27)
			{
				if (!IsSpecialCenterMap(waypoint))
				{
					if (waypoint.minX < TileMap.pxw - 60)
					{
						return waypoint.maxX >= 60;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		private static bool IsRightWaypoint(Waypoint waypoint)
		{
			if (TileMap.mapID != 70 || !(GetTextPopup(waypoint.popup) == "Căn cứ Raspberry"))
			{
				return waypoint.minX > TileMap.pxw - 60;
			}
			return true;
		}

		private static bool IsSpecialCenterMap(Waypoint waypoint)
		{
			string textPopup = GetTextPopup(waypoint.popup);
			int mapID = TileMap.mapID;
			if (((mapID != 106 && mapID != 107) || !(textPopup == "Hang băng")) && ((mapID != 105 && mapID != 108) || !(textPopup == "Rừng băng")))
			{
				if (mapID == 109)
				{
					return textPopup == "Cánh đồng tuyết";
				}
				return false;
			}
			return true;
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

		public static void RequestChangeMap(Waypoint waypoint)
		{
			if (waypoint.isOffline)
			{
				Service.gI().getMapOffline();
			}
			else
			{
				Service.gI().requestChangeMap();
			}
		}

		private static void TeleportInNRDMap(int position)
		{
			switch (position)
			{
			case 0:
				ModFunc.GI().MoveTo(60, GetYGround(60));
				break;
			default:
				ModFunc.GI().MoveTo(TileMap.pxw - 60, GetYGround(TileMap.pxw - 60));
				break;
			case 2:
			{
				for (int i = 0; i < GameScr.vNpc.size(); i++)
				{
					Npc npc = (Npc)GameScr.vNpc.elementAt(i);
					if (npc.template.npcTemplateId >= 30 && npc.template.npcTemplateId <= 36)
					{
						Char.myCharz().npcFocus = npc;
						ModFunc.GI().MoveTo(npc.cx, npc.cy - 3);
						break;
					}
				}
				break;
			}
			}
		}
	}
}
