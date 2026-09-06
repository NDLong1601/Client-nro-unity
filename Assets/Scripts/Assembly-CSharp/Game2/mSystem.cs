using System;
using UnityEngine;

namespace Game2
{
	public class mSystem
	{
		public static bool isTest;

		public static string strAdmob;

		public static string publicID;

		public static string android_pack;

		public static int clientType = 4;

		public static sbyte curINAPP;

		public static sbyte maxINAPP = 5;

		public static void AddIpTest()
		{
		}

		public static void resetCurInapp()
		{
			curINAPP = 0;
		}

		public static string getTimeCountDown(long timeStart, int secondCount, bool isOnlySecond, bool isShortText)
		{
			string result = string.Empty;
			long num = (timeStart + secondCount * 1000 - currentTimeMillis()) / 1000;
			if (num <= 0)
			{
				return string.Empty;
			}
			long num2 = 0L;
			long num3 = 0L;
			long num4 = num / 60;
			long num5 = num;
			if (isOnlySecond)
			{
				return num5 + string.Empty;
			}
			if (num >= 86400)
			{
				num2 = num / 86400;
				num3 = num % 86400 / 3600;
			}
			else if (num >= 3600)
			{
				num3 = num / 3600;
				num4 = num % 3600 / 60;
			}
			else if (num >= 60)
			{
				num4 = num / 60;
				num5 = num % 60;
			}
			else
			{
				num5 = num;
			}
			if (isShortText)
			{
				if (num2 > 0)
				{
					return num2 + "d";
				}
				if (num3 > 0)
				{
					return num3 + "h";
				}
				if (num4 > 0)
				{
					return num4 + "m";
				}
				if (num5 > 0)
				{
					return num5 + "s";
				}
			}
			if (num2 > 0)
			{
				if (num2 >= 10)
				{
					result = ((num3 < 1) ? (num2 + "d") : ((num3 >= 10) ? (num2 + "d" + num3 + "h") : (num2 + "d0" + num3 + "h")));
				}
				else if (num2 < 10)
				{
					result = ((num3 < 1) ? (num2 + "d") : ((num3 >= 10) ? (num2 + "d" + num3 + "h") : (num2 + "d0" + num3 + "h")));
				}
			}
			else if (num3 > 0)
			{
				if (num3 >= 10)
				{
					result = ((num4 < 1) ? (num3 + "h") : ((num4 >= 10) ? (num3 + "h" + num4 + "m") : (num3 + "h0" + num4 + "m")));
				}
				else if (num3 < 10)
				{
					result = ((num4 < 1) ? (num3 + "h") : ((num4 >= 10) ? (num3 + "h" + num4 + "m") : (num3 + "h0" + num4 + "m")));
				}
			}
			else if (num4 > 0)
			{
				if (num4 >= 10)
				{
					if (num5 >= 10)
					{
						result = num4 + "m" + num5 + string.Empty;
					}
					else if (num5 < 10)
					{
						result = num4 + "m0" + num5 + string.Empty;
					}
				}
				else if (num4 < 10)
				{
					if (num5 >= 10)
					{
						result = num4 + "m" + num5 + string.Empty;
					}
					else if (num5 < 10)
					{
						result = num4 + "m0" + num5 + string.Empty;
					}
				}
			}
			else
			{
				result = ((num5 >= 10) ? (num5 + string.Empty) : ("0" + num5 + string.Empty));
			}
			return result;
		}

		public static string numberTostring(long number)
		{
			string text = string.Empty + number;
			bool flag = false;
			try
			{
				string empty = string.Empty;
				if (number < 0)
				{
					flag = true;
					number = -number;
					text = string.Empty + number;
				}
				int length;
				if (number >= 1000000000)
				{
					empty = "b";
					number /= 1000000000;
					length = (string.Empty + number).Length;
				}
				else if (number >= 1000000)
				{
					empty = "m";
					number /= 1000000;
					length = (string.Empty + number).Length;
				}
				else
				{
					if (number < 1000)
					{
						if (flag)
						{
							return "-" + text;
						}
						return text;
					}
					empty = "k";
					number /= 1000;
					length = (string.Empty + number).Length;
				}
				int num = int.Parse(text.Substring(length, 2));
				text = ((num == 0) ? (text.Substring(0, length) + empty) : ((num % 10 != 0) ? (text.Substring(0, length) + "," + text.Substring(length, 2) + empty) : (text.Substring(0, length) + "," + text.Substring(length, 1) + empty)));
			}
			catch (Exception)
			{
			}
			if (flag)
			{
				return "-" + text;
			}
			return text;
		}

		public static void callHotlinePC()
		{
			Application.OpenURL(ServerListScreen.linkweb);
		}

		public static void callHotlineJava()
		{
		}

		public static void callHotlineIphone()
		{
		}

		public static void callHotlineWindowsPhone()
		{
		}

		public static void closeBanner()
		{
		}

		public static void createAdmob()
		{
		}

		public static void checkAdComlete()
		{
		}

		public static void paintPopUp2(mGraphics g, int x, int y, int w, int h)
		{
			g.fillRect(x, y, w + 10, h, 0, 90);
		}

		public static long currentTimeMillis()
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (DateTime.UtcNow.Ticks - dateTime.Ticks) / 10000;
		}

		public static void println(object str)
		{
			Debug.Log(str);
		}

		public static void gcc()
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		public static void onConnectOK()
		{
			Controller.isConnectOK = true;
		}

		public static void onConnectionFail()
		{
			Controller.isConnectionFail = true;
		}

		public static void onDisconnected()
		{
			Controller.isDisconnected = true;
		}

		public static void exitWP()
		{
		}

		public static void paintFlyText(mGraphics g)
		{
			for (int i = 0; i < 5; i++)
			{
				if (GameScr.flyTextState[i] != -1 && GameCanvas.isPaint(GameScr.flyTextX[i], GameScr.flyTextY[i]))
				{
					if (GameScr.flyTextColor[i] == mFont.RED)
					{
						mFont.bigNumber_red.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER);
					}
					else if (GameScr.flyTextColor[i] == mFont.YELLOW)
					{
						mFont.bigNumber_yellow.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER);
					}
					else if (GameScr.flyTextColor[i] == mFont.GREEN)
					{
						mFont.bigNumber_green.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER);
					}
					else if (GameScr.flyTextColor[i] == mFont.FATAL)
					{
						mFont.bigNumber_yellow.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER, mFont.bigNumber_black);
					}
					else if (GameScr.flyTextColor[i] == mFont.FATAL_ME)
					{
						mFont.bigNumber_green.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER, mFont.bigNumber_black);
					}
					else if (GameScr.flyTextColor[i] == mFont.MISS)
					{
						mFont.bigNumber_While.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER, mFont.tahoma_7_grey);
					}
					else if (GameScr.flyTextColor[i] == mFont.ORANGE)
					{
						mFont.bigNumber_orange.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER);
					}
					else if (GameScr.flyTextColor[i] == mFont.ADDMONEY)
					{
						mFont.bigNumber_yellow.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER, mFont.bigNumber_black);
					}
					else if (GameScr.flyTextColor[i] == mFont.MISS_ME)
					{
						mFont.bigNumber_While.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER, mFont.bigNumber_black);
					}
					else if (GameScr.flyTextColor[i] == mFont.HP)
					{
						mFont.bigNumber_red.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER, mFont.bigNumber_black);
					}
					else if (GameScr.flyTextColor[i] == mFont.MP)
					{
						mFont.bigNumber_blue.drawStringBorder(g, GameScr.flyTextString[i], GameScr.flyTextX[i], GameScr.flyTextY[i], mFont.CENTER, mFont.bigNumber_black);
					}
				}
			}
		}

		public static void endKey()
		{
		}

		public static FrameImage getFraImage(string nameImg)
		{
			FrameImage result = null;
			MainImage mainImage = null;
			if (mainImage == null)
			{
				mainImage = ImgByName.getImagePath(nameImg, ImgByName.hashImagePath);
			}
			if (mainImage.img != null)
			{
				int num = mainImage.img.getHeight() / mainImage.nFrame;
				if (num < 1)
				{
					num = 1;
				}
				result = new FrameImage(mainImage.img, mainImage.img.getWidth(), num);
			}
			return result;
		}

		public static Image loadImage(string path)
		{
			return GameCanvas.loadImage(path);
		}
	}
}
