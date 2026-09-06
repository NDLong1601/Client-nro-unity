using System.Collections.Generic;
using System.Threading;

namespace Game1.Mod.XMAP
{
	public class PickMob
	{
		private static readonly sbyte[] IdSkillsBase = new sbyte[5] { 0, 2, 17, 4, 13 };

		public static readonly short[] IdItemBlockBase = new short[10] { 225, 353, 354, 355, 356, 357, 358, 359, 360, 362 };

		public static bool tanSat = false;

		public static bool tsPlayer = false;

		public static bool neSieuQuai = false;

		public static bool vuotDiaHinh = true;

		public static bool telePem = true;

		public static bool isGoBack;

		public static int mapGoback;

		public static int zoneGoback;

		public static int xGoback;

		public static int yGoback;

		public static List<int> IdMobsTanSat = new List<int>();

		public static List<int> TypeMobsTanSat = new List<int>();

		public static List<sbyte> IdSkillsTanSat = new List<sbyte>(IdSkillsBase);

		public static bool IsAutoPickItems = true;

		public static bool IsPickItemsAll = true;

		public static bool IsPickItemsDis = false;

		public static bool IsLimitTimesPickItem = true;

		public static int TimesAutoPickItemMax = 20;

		public static List<short> IdItemPicks = new List<short>();

		public static List<short> IdItemBlocks = new List<short>(IdItemBlockBase);

		public static List<sbyte> TypeItemPicks = new List<sbyte>();

		public static List<sbyte> TypeItemBlock = new List<sbyte>();

		public static int HpBuff = 0;

		public static int MpBuff = 0;

		public static void Update()
		{
			PickMobController.Update();
		}

		public static void GoBack()
		{
			Thread.Sleep(5000);
			if (!GameScr.gI().magicTree.isUpdate && GameScr.gI().magicTree.currPeas > 0 && TileMap.mapID == Char.myCharz().cgender + 21)
			{
				Service.gI().magicTree(1);
				Thread.Sleep(500);
				GameCanvas.gI().keyPressedz(-5);
				Thread.Sleep(1000);
			}
			for (int i = 0; i < GameScr.vItemMap.size(); i++)
			{
				ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(i);
				Char.myCharz().cx = itemMap.x;
				Service.gI().charMove();
				Thread.Sleep(1000);
				Service.gI().pickItem(itemMap.itemMapID);
				Thread.Sleep(1000);
			}
			XmapController.StartRunToMapId(mapGoback);
			while (TileMap.mapID != mapGoback)
			{
				Thread.Sleep(200);
			}
			while (TileMap.zoneID != zoneGoback)
			{
				Thread.Sleep(1000);
				Service.gI().requestChangeZone(zoneGoback, -1);
			}
			Thread.Sleep(2000);
			ModFunc.GI().MoveTo(xGoback, yGoback);
			GameScr.isAutoPlay = true;
		}
	}
}
