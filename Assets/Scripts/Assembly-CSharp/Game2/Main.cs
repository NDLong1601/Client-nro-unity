using System;
using System.Net.NetworkInformation;
using System.Threading;
using UnityEngine;

namespace Game2
{
	public class Main : MonoBehaviour
	{
        private void Awake()
        {
            if (main != null)
            {
                Management.tab = TabType.Tab2;
                Destroy(gameObject);
                return;
            }
            main = this;
            DontDestroyOnLoad(gameObject);
        }
        public static Main main;

		public static mGraphics g;

		public static GameMidlet midlet;

		public static string res = "res";

		public static string mainThreadName;

		public static bool started;

		public static bool isIpod;

		public static bool isIphone4;

		public static bool isPC;

		public static bool isWindowsPhone;

		public static bool isIPhone;

		public static bool IphoneVersionApp;

		public static string IMEI;

		public static int versionIp;

		public static int numberQuit = 1;

		public static int typeClient = 4;

		public const sbyte PC_VERSION = 4;

		public const sbyte IP_APPSTORE = 5;

		public const sbyte WINDOWSPHONE = 6;

		private int level;

		public const sbyte IP_JB = 3;

		private int updateCount;

		private int paintCount;

		private int count;

		private int fps;

		private int max;

		private int up;

		private int upmax;

		private long timefps;

		private long timeup;

		private bool isRun;

		public static int waitTick;

		public static int f;

		public static bool isResume;

		public static bool isMiniApp = true;

		public static bool isQuitApp;

		private Vector2 lastMousePos;

		public static int a = 1;

		public static bool isCompactDevice = true;

		private void Start()
		{
			if (started)
			{
				return;
			}
			if (Thread.CurrentThread.Name != "Main")
			{
				Thread.CurrentThread.Name = "Main";
			}
			mainThreadName = Thread.CurrentThread.Name;
			isPC = Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer;
			isIPhone = (IphoneVersionApp = Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android);
			started = true;
			if (isPC && !isIPhone)
			{
				level = Rms.loadRMSInt("levelScreenKN");
				if (level == 1)
				{
					Screen.SetResolution(720, 320, fullscreen: false);
				}
				else
				{
					Screen.SetResolution(1024, 600, fullscreen: false);
				}
			}
			else if (isIPhone)
			{
				Screen.fullScreen = true;
				GameCanvas.isTouch = true;
			}
			ModFunc.GI().LoadGame();
		}

		private void SetInit()
		{
			base.enabled = true;
		}

		private void OnHideUnity(bool isGameShown)
		{
			if (!isGameShown)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = 1f;
			}
		}

		private void OnGUI()
		{
			if (count < 10)
			{
				return;
			}
			if (fps == 0)
			{
				timefps = mSystem.currentTimeMillis();
			}
			else if (mSystem.currentTimeMillis() - timefps > 1000)
			{
				max = fps;
				fps = 0;
				timefps = mSystem.currentTimeMillis();
			}
			fps++;
			checkInput();
			Session_ME.update();
			Session_ME2.update();
			if (Management.tab == TabType.Tab2 && Event.current.type.Equals(EventType.Repaint) && paintCount <= updateCount)
			{
				if (GameMidlet.gameCanvas != null)
				{
					GameMidlet.gameCanvas.paint(g);
				}
				paintCount++;
				if (g != null)
				{
					g.reset();
				}
			}
		}

		public void setsizeChange()
		{
			if (!isRun)
			{
				Screen.orientation = ScreenOrientation.AutoRotation;
				Application.runInBackground = true;
				base.useGUILayout = false;
				isCompactDevice = detectCompactDevice();
				if (main == null)
				{
					main = this;
				}
				isRun = true;
				ScaleGUI.initScaleGUI();
				if (isPC)
				{
					IMEI = SystemInfo.deviceUniqueIdentifier;
				}
				else
				{
					IMEI = GetMacAddress();
				}
				if (isPC && !isIPhone)
				{
					Screen.fullScreen = false;
				}
				if (isIPhone && !isPC)
				{
					Screen.fullScreen = true;
				}
				if (isPC)
				{
					typeClient = 4;
				}
				if (isWindowsPhone)
				{
					typeClient = 6;
				}
				if (isIPhone || IphoneVersionApp)
				{
					typeClient = 4;
				}
				if (iPhoneSettings.generation == iPhoneGeneration.iPodTouch4Gen)
				{
					isIpod = true;
				}
				if (iPhoneSettings.generation == iPhoneGeneration.iPhone4)
				{
					isIphone4 = true;
				}
				g = new mGraphics();
				midlet = new GameMidlet();
				TileMap.loadBg();
				Paint.loadbg();
				PopUp.loadBg();
				GameScr.loadBg();
				InfoMe.gI().loadCharId();
				Panel.loadBg();
				Menu.loadBg();
				Key.mapKeyPC();
				SoundMn.gI().loadSound(TileMap.mapID);
				g.CreateLineMaterial();
			}
		}

		public static void setBackupIcloud(string path)
		{
		}

		public string GetMacAddress()
		{
			_ = string.Empty;
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			for (int i = 0; i < allNetworkInterfaces.Length; i++)
			{
				PhysicalAddress physicalAddress = allNetworkInterfaces[i].GetPhysicalAddress();
				if (physicalAddress.ToString() != string.Empty)
				{
					return physicalAddress.ToString();
				}
			}
			return string.Empty;
		}

		public void doClearRMS()
		{
			if (isPC && Rms.loadRMSInt("lastZoomlevel") != mGraphics.zoomLevel)
			{
				Rms.saveRMSInt("lastZoomlevel", mGraphics.zoomLevel);
				Rms.saveRMSInt("levelScreenKN", level);
			}
		}

		public static void closeKeyBoard()
		{
			if (TouchScreenKeyboard.visible)
			{
				TField.kb.active = false;
				TField.kb = null;
			}
		}

		
		private void FixedUpdate()
		{
			Rms.update();
			count++;
			if (count >= 10)
			{
				if (up == 0)
				{
					timeup = mSystem.currentTimeMillis();
				}
				else if (mSystem.currentTimeMillis() - timeup > 1000)
				{
					upmax = up;
					up = 0;
					timeup = mSystem.currentTimeMillis();
				}
				up++;
				setsizeChange();
				updateCount++;
				ipKeyboard.update();
				if (GameMidlet.gameCanvas != null)
				{
					GameMidlet.gameCanvas.update();
				}
				Image.update();
				DataInputStream.update();
				f++;
				if (f > 8)
				{
					f = 0;
				}
				if (!isPC)
				{
					_ = 1 / a;
				}
			}
		}

        internal void checkInput()
        {
            if (Management.tab != TabType.Tab2) return;
            // During a Unity domain reload OnGUI can run before the game canvas is rebuilt.
            // Ignore that frame instead of dereferencing a not-yet-initialized canvas.
            if (GameMidlet.gameCanvas == null) return;
            if (Input.GetMouseButtonDown(0))
			{
				Vector3 mousePosition = Input.mousePosition;
				GameMidlet.gameCanvas.pointerPressed((int)(mousePosition.x / (float)mGraphics.zoomLevel), (int)(((float)Screen.height - mousePosition.y) / (float)mGraphics.zoomLevel) + mGraphics.addYWhenOpenKeyBoard);
				lastMousePos.x = mousePosition.x / (float)mGraphics.zoomLevel;
				lastMousePos.y = mousePosition.y / (float)mGraphics.zoomLevel + (float)mGraphics.addYWhenOpenKeyBoard;
			}
			if (Input.GetMouseButton(0))
			{
				Vector3 mousePosition2 = Input.mousePosition;
				GameMidlet.gameCanvas.pointerDragged((int)(mousePosition2.x / (float)mGraphics.zoomLevel), (int)(((float)Screen.height - mousePosition2.y) / (float)mGraphics.zoomLevel) + mGraphics.addYWhenOpenKeyBoard);
				lastMousePos.x = mousePosition2.x / (float)mGraphics.zoomLevel;
				lastMousePos.y = mousePosition2.y / (float)mGraphics.zoomLevel + (float)mGraphics.addYWhenOpenKeyBoard;
			}
			if (Input.GetMouseButtonUp(0))
			{
				Vector3 mousePosition3 = Input.mousePosition;
				lastMousePos.x = mousePosition3.x / (float)mGraphics.zoomLevel;
				lastMousePos.y = mousePosition3.y / (float)mGraphics.zoomLevel + (float)mGraphics.addYWhenOpenKeyBoard;
				GameMidlet.gameCanvas.pointerReleased((int)(mousePosition3.x / (float)mGraphics.zoomLevel), (int)(((float)Screen.height - mousePosition3.y) / (float)mGraphics.zoomLevel) + mGraphics.addYWhenOpenKeyBoard);
			}
			if (Event.current.type == EventType.KeyDown)
			{
				int num = MyKeyMap.map(Event.current.keyCode);
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					switch (Event.current.keyCode)
					{
					case KeyCode.Alpha2:
						num = 64;
						break;
					case KeyCode.Minus:
						num = 95;
						break;
					}
				}
				if (num != 0)
				{
					GameMidlet.gameCanvas.keyPressedz(num);
				}
				Event.current.Use();
			}
			if (Event.current.type == EventType.KeyUp)
			{
				int num2 = MyKeyMap.map(Event.current.keyCode);
				if (num2 != 0)
				{
					GameMidlet.gameCanvas.keyReleasedz(num2);
				}
				Event.current.Use();
			}
			if (isPC)
			{
				GameMidlet.gameCanvas.scrollMouse((int)(Input.GetAxis("Mouse ScrollWheel") * 10f));
				int num3 = (int)Input.mousePosition.x;
				float y = Input.mousePosition.y;
				int x = num3 / mGraphics.zoomLevel;
				int y2 = (Screen.height - (int)y) / mGraphics.zoomLevel;
				GameMidlet.gameCanvas.pointerMouse(x, y2);
			}
		}

		private void OnApplicationQuit()
		{
			Rms.flush();
			GameCanvas.bRun = false;
			Session_ME.gI().close();
			Session_ME2.gI().close();
			if (isPC)
			{
				Application.Quit();
			}
		}

		private void OnApplicationPause(bool paused)
		{
			isResume = false;
			if (paused)
			{
				Rms.flush();
				if (GameCanvas.isWaiting())
				{
					isQuitApp = true;
				}
			}
			else
			{
				isResume = true;
			}
			if (TouchScreenKeyboard.visible)
			{
				TField.kb.active = false;
				TField.kb = null;
			}
			if (isQuitApp)
			{
				Application.Quit();
			}
		}

		public static void exit()
		{
			if (isPC)
			{
				main.OnApplicationQuit();
			}
			else
			{
				a = 0;
			}
		}

		public static bool detectCompactDevice()
		{
			if (iPhoneSettings.generation != iPhoneGeneration.iPhone && iPhoneSettings.generation != iPhoneGeneration.iPhone3G && iPhoneSettings.generation != iPhoneGeneration.iPodTouch1Gen)
			{
				return iPhoneSettings.generation != iPhoneGeneration.iPodTouch2Gen;
			}
			return false;
		}

		public static bool checkCanSendSMS()
		{
			if (iPhoneSettings.generation != iPhoneGeneration.iPhone3GS && iPhoneSettings.generation != iPhoneGeneration.iPhone4)
			{
				return iPhoneSettings.generation > iPhoneGeneration.iPodTouch4Gen;
			}
			return true;
		}
	}
}
