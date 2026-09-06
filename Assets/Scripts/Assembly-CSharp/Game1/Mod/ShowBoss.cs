using System;

namespace Game1.Mod
{
	public class ShowBoss
	{
		public DateTime AppearTime;

		private static int SHOW_TIME = 20000;

		private long startShowTime;

		private static int VERTICAL_SPACING = 10;

		private int startX;

		private int targetX;

		private int yPos;

		private int currentX;

		private bool isShowing;

		private bool isDone;

		private static object lockObject = new object();

		public string nameBoss;

		public string mapName;

		public string playerKill;

		public int mapID;

		public long time;

		public int stringLength;

		public ShowBoss(string text)
		{
			text = text.Replace(text.Substring(0, 5), "|");
			text = text.Replace(" vừa xuất hiện tại", "|");
			text = text.Replace(" khu vực", "|");
			string[] array2 = text.Split(new char[1] { '|' });
			nameBoss = array2[1].Trim();
			mapName = array2[2].Trim();
			mapID = ModFunc.GI().GetMapID(mapName);
			time = DateTime.Now.Ticks;
		}

		public static void HandleChatVip(string chatVip)
		{
			string text = chatVip.Trim().ToLower();
			ShowBoss showBoss = new ShowBoss(chatVip);
			if (text.Contains("boss") && text.Contains("xuất hiện"))
			{
				lock (lockObject)
				{
					ModFunc.activeBossNotif.addElement(showBoss);
					if (ModFunc.activeBossNotif.size() > 5)
					{
						ModFunc.activeBossNotif.removeElementAt(0);
					}
					return;
				}
			}
			if (!text.Contains("tiêu diệt"))
			{
				return;
			}
			lock (lockObject)
			{
				if (ModFunc.killedBossNotif.size() >= 5)
				{
					ModFunc.killedBossNotif.removeElementAt(0);
					for (int i = 0; i < ModFunc.killedBossNotif.size(); i++)
					{
						((ShowBoss)ModFunc.killedBossNotif.elementAt(i)).yPos = 65 + i * VERTICAL_SPACING;
					}
				}
				showBoss.yPos = 65 + ModFunc.killedBossNotif.size() * VERTICAL_SPACING;
				ModFunc.killedBossNotif.addElement(showBoss);
			}
		}

		private static void RemoveNotification(ShowBoss notification)
		{
			lock (lockObject)
			{
				if (!string.IsNullOrEmpty(notification.playerKill))
				{
					int num = ModFunc.killedBossNotif.indexOf(notification);
					ModFunc.killedBossNotif.removeElement(notification);
					for (int i = num; i < ModFunc.killedBossNotif.size(); i++)
					{
						((ShowBoss)ModFunc.killedBossNotif.elementAt(i)).yPos = 65 + i * VERTICAL_SPACING;
					}
				}
				else
				{
					ModFunc.activeBossNotif.removeElement(notification);
				}
			}
		}

		private string GetTimeString(TimeSpan timeSpan)
		{
			string text = "";
			int num = (int)System.Math.Floor((decimal)timeSpan.TotalHours);
			if (num > 0)
			{
				text += $"{num}h";
			}
			if (timeSpan.Minutes > 0)
			{
				text += $"{timeSpan.Minutes}m";
			}
			return text + $"{timeSpan.Seconds}s";
		}

		public static void UpdateNotifications()
		{
			for (int num = ModFunc.killedBossNotif.size() - 1; num >= 0; num--)
			{
				ShowBoss showBoss = (ShowBoss)ModFunc.killedBossNotif.elementAt(num);
				if (showBoss.isDone)
				{
					RemoveNotification(showBoss);
					for (int i = num; i < ModFunc.killedBossNotif.size(); i++)
					{
						((ShowBoss)ModFunc.killedBossNotif.elementAt(i)).yPos = 65 + i * VERTICAL_SPACING;
					}
				}
			}
			for (int num2 = ModFunc.activeBossNotif.size() - 1; num2 >= 0; num2--)
			{
				ShowBoss showBoss2 = (ShowBoss)ModFunc.activeBossNotif.elementAt(num2);
				if (showBoss2.isDone)
				{
					RemoveNotification(showBoss2);
				}
			}
			if (ModFunc.killedBossNotif.size() <= 5)
			{
				return;
			}
			lock (lockObject)
			{
				_ = (ShowBoss)ModFunc.killedBossNotif.elementAt(0);
				ModFunc.killedBossNotif.removeElementAt(0);
				for (int j = 0; j < ModFunc.killedBossNotif.size(); j++)
				{
					((ShowBoss)ModFunc.killedBossNotif.elementAt(j)).yPos = 65 + j * VERTICAL_SPACING;
				}
			}
		}

		public void PaintBoss(mGraphics g, int x, int y)
		{
			long num = (DateTime.Now.Ticks - time) / 10000000;
			string text;
			if (num >= 60)
			{
				long num2 = num / 60;
				long num3 = num % 60;
				text = $"{num2}m {num3}s";
			}
			else
			{
				text = num + "s";
			}
			mFont mFont = mFont.tahoma_7_orange;
			if (TileMap.mapName.Trim().ToLower() == mapName.Trim().ToLower())
			{
				y += 2;
				mFont = mFont.tahoma_7_red;
				for (int i = 0; i < GameScr.vCharInMap.size(); i++)
				{
					Char @char = (Char)GameScr.vCharInMap.elementAt(i);
					if (@char.charID < 0 && @char.cName == nameBoss)
					{
						mFont = mFont.tahoma_7_red;
						break;
					}
				}
			}
			string text2 = nameBoss + " - " + mapName + " - " + text;
			stringLength = mFont.getWidthExactOf(text2);
			g.FillRect(x - stringLength - 3, y + 3, stringLength + 2, 10, 0, 0.2f);
			mFont.drawStringBorder(g, text2, x, y, mFont.RIGHT, mFont.tahoma_7_grey);
		}
	}
}
