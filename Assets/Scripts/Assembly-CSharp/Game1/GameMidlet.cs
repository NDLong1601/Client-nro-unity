using UnityEngine;

namespace Game1
{
	public class GameMidlet
	{
		public static string IP = "";

		public static int PORT = 113;

		public static string IP2;

		public static int PORT2;

		public static sbyte PROVIDER;

		public static int LANGUAGE;

		public static string VERSION = ((Rms.loadRMSString("version") == null) ? "2.4.8" : Rms.loadRMSString("version"));

		public static GameCanvas gameCanvas;

		public static GameMidlet instance;

		public static bool isConnect2;

		public static bool isBackWindowsPhone;

		public GameMidlet()
		{
			initGame();
		}

		public void initGame()
		{
			instance = this;
			MotherCanvas.instance = new MotherCanvas();
			Session_ME.gI().setHandler(Controller.gI());
			Session_ME2.gI().setHandler(Controller.gI());
			Session_ME2.isMainSession = false;
			instance = this;
			gameCanvas = new GameCanvas();
			gameCanvas.start();
			SplashScr.LoadImg();
			SplashScr.loadSplashScr();
			GameCanvas.currentScreen = new SplashScr();
		}

		public void exit()
		{
			if (Main.typeClient == 6)
			{
				mSystem.exitWP();
				return;
			}
			GameCanvas.bRun = false;
			mSystem.gcc();
			notifyDestroyed();
		}

		public void notifyDestroyed()
		{
			Main.exit();
		}

		public void platformRequest(string url)
		{
			Application.OpenURL(url);
		}
	}
}
