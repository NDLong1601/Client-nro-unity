using System;

namespace Game2
{
	public class ServerListScreen : mScreen, IActionListener
	{
		public static string[] nameServer;

		public static string[] address;

		public static sbyte serverPriority;

		public static bool[] hasConnected;

		public static short[] port;

		public static int selected;

		public static bool isWait;

		public static Command cmdUpdateServer;

		public static sbyte[] language;

		private Command[] cmd;

		private int nCmdPlay;

		public static string keyDecryptString;

		public static Command cmdDeleteRMS;

		public static bool isMultiSever = false;

		public static string ListIP = Management.IpServer;

		public static string linkDefault = Management.IpServer;

		public const sbyte languageVersion = 2;

		public new int keyTouch = -1;

		public static bool stopDownload;

		public static string linkweb = Management.LinkWeb;

		public static int countDieConnect;

		public static bool waitToLogin;

		public static int tWaitToLogin;

		public static int[] lengthServer = new int[3];

		public static int ipSelect;

		public static int flagServer;

		public static bool bigOk;

		public static int percent;

		public static string strWait;

		public static int nBig;

		public static int nBg;

		public static int demPercent;

		public static int maxBg;

		public static bool isGetData = false;

		public static Command cmdDownload;

		private Command cmdStart;

		public string dataSize;

		public static int p;

		public static int testConnect;

		public static bool loadScreen;

		public static bool isAutoConect = true;

		public ServerListScreen()
		{
			int num = 4;
			if (num * 32 + 23 + 33 >= GameCanvas.w)
			{
				num--;
			}
			initCommand();
			if (!GameCanvas.isTouch)
			{
				selected = 0;
				processInput();
			}
			GameScr.loadCamera(fullmScreen: true, -1, -1);
			GameScr.cmx = 100;
			GameScr.cmy = 200;
			cmdUpdateServer = new Command
			{
				actionChat = delegate(string str)
				{
					string text = str;
					string text2 = str;
					if (text == null)
					{
						text = linkDefault;
					}
					else
					{
						if (text == null && text2 != null)
						{
							if (text2.Equals(string.Empty) || text2.Length < 20)
							{
								text2 = linkDefault;
							}
							GetServerList(text2);
						}
						if (text != null && text2 == null)
						{
							if (text.Equals(string.Empty) || text.Length < 20)
							{
								text = linkDefault;
							}
							GetServerList(text);
						}
						if (text != null && text2 != null)
						{
							if (text.Length > text2.Length)
							{
								GetServerList(text);
							}
							else
							{
								GetServerList(text2);
							}
						}
					}
				}
			};
		}

		public static void createDeleteRMS()
		{
		}

		private void initCommand()
		{
			nCmdPlay = 0;
			string text = Rms.loadRMSString("acc");
			Rms.loadRMSString("pass");
			if (text == null)
			{
				if (Rms.loadRMS("userAo" + ipSelect) != null)
				{
					nCmdPlay = 1;
				}
			}
			else if (text.Equals(string.Empty))
			{
				if (Rms.loadRMS("userAo" + ipSelect) != null)
				{
					nCmdPlay = 1;
				}
			}
			else
			{
				nCmdPlay = 1;
			}
			cmd = new Command[(Main.isIPhone || mGraphics.zoomLevel <= 4) ? (4 + nCmdPlay) : (3 + nCmdPlay)];
			int num = GameCanvas.hh - 15 * cmd.Length + 28;
			for (int i = 0; i < cmd.Length; i++)
			{
				switch (i)
				{
				case 0:
					cmd[0] = new Command(string.Empty, this, 3, null);
					if (text == null)
					{
						cmd[0].caption = mResources.playNew;
						if (Rms.loadRMS("userAo" + ipSelect) != null)
						{
							cmd[0].caption = mResources.choitiep;
						}
						break;
					}
					if (text.Equals(string.Empty))
					{
						cmd[0].caption = mResources.playNew;
						if (Rms.loadRMS("userAo" + ipSelect) != null)
						{
							cmd[0].caption = mResources.choitiep;
						}
						break;
					}
					cmd[0].caption = mResources.playAcc + ": " + ((!text.Contains(',')) ? text : "");
					if (cmd[0].caption.Length > 23)
					{
						cmd[0].caption = cmd[0].caption.Substring(0, 23);
						cmd[0].caption += "...";
					}
					break;
				case 1:
					if (nCmdPlay == 1)
					{
						cmd[1] = new Command(string.Empty, this, 10100, null);
						cmd[1].caption = mResources.playNew;
					}
					else
					{
						cmd[1] = new Command(mResources.change_account, this, 7, null);
					}
					break;
				case 2:
					if (nCmdPlay == 1)
					{
						cmd[2] = new Command(mResources.change_account, this, 7, null);
					}
					else
					{
						cmd[2] = new Command(string.Empty, this, 17, null);
					}
					break;
				case 3:
					if (nCmdPlay == 1)
					{
						cmd[3] = new Command(string.Empty, this, 17, null);
					}
					else
					{
						cmd[3] = new Command("Xóa dữ liệu", this, 8, null);
					}
					break;
				case 4:
					cmd[4] = new Command("Xóa dữ liệu", this, 8, null);
					break;
				}
				cmd[i].y = num;
				cmd[i].setType();
				cmd[i].x = (GameCanvas.w - cmd[i].w) / 2;
				num += 30;
			}
		}

		public static void doUpdateServer()
		{
			if (cmdUpdateServer == null && GameCanvas.serverScreen == null)
			{
				GameCanvas.serverScreen = new ServerListScreen();
			}
		}

		public static void GetServerList(string str)
		{
			try
			{
				lengthServer = new int[3];
				string[] array = Res.split(str.Trim(), ",", 0);
				Res.outz("tem leng= " + array.Length);
				if (array.Length < 3)
				{
					nameServer = new string[1] { "NRO" };
					address = new string[1] { "127.0.0.1" };
					port = new short[1] { 14445 };
					language = new sbyte[1] { 0 };
					hasConnected = new bool[2];
					serverPriority = 0;
					ipSelect = 0;
					return;
				}
				mResources.loadLanguague(sbyte.Parse(array[^2]));
				nameServer = new string[array.Length - 2];
				address = new string[array.Length - 2];
				port = new short[array.Length - 2];
				language = new sbyte[array.Length - 2];
				hasConnected = new bool[2];
				for (int i = 0; i < array.Length - 2; i++)
				{
					string[] array2 = Res.split(array[i].Trim(), ":", 0);
					nameServer[i] = array2[0];
					address[i] = array2[1];
					port[i] = short.Parse(array2[2]);
					language[i] = sbyte.Parse(array2[3].Trim());
					lengthServer[language[i]]++;
				}
				serverPriority = sbyte.Parse(array[^1]);
				ipSelect = 0;
				SaveIP();
			}
			catch (Exception ex)
			{
				mSystem.println("GetServerList error: " + ex);
			}
		}

		public override void paint(mGraphics g)
		{
			if (!loadScreen)
			{
				g.setColor(0);
				g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
			}
			else
			{
				GameCanvas.paintBGGameScr(g);
			}
			int num = 2;
			mFont.tahoma_7_white.drawStringBorder(g, "v" + GameMidlet.VERSION + " (x" + mGraphics.zoomLevel + ")", GameCanvas.w - 2, num, 1, mFont.tahoma_7_grey);
			string empty = string.Empty;
			empty = ((testConnect != 0) ? (empty + nameServer[ipSelect] + " connected") : (empty + nameServer[ipSelect] + " disconnect"));
			if (mSystem.isTest)
			{
				mFont.tahoma_7_white.drawString(g, empty, GameCanvas.w - 2, num + 15 + 15, 1, mFont.tahoma_7_grey);
			}
			if (!isGetData || loadScreen)
			{
				if (mSystem.clientType == 1 && !GameCanvas.isTouch)
				{
					mFont.tahoma_7_white.drawStringBorder(g, linkweb, GameCanvas.w - 2, GameCanvas.h - 15, 1, mFont.tahoma_7_grey);
				}
				else
				{
					mFont.tahoma_7_white.drawStringBorder(g, linkweb, GameCanvas.w - 2, num + 10, 1, mFont.tahoma_7_grey);
				}
			}
			else
			{
				mFont.tahoma_7_white.drawStringBorder(g, linkweb, GameCanvas.w - 2, num + 10, 1, mFont.tahoma_7_grey);
			}
			_ = GameCanvas.w;
			if (cmdDeleteRMS != null)
			{
				mFont.tahoma_7_white.drawStringBorder(g, mResources.xoadulieu, GameCanvas.w - 2, GameCanvas.h - 15, 1, mFont.tahoma_7_grey);
			}
			if (GameCanvas.currentDialog == null)
			{
				if (!loadScreen)
				{
					if (!bigOk)
					{
						ModFunc.PaintLogoGif(g, GameCanvas.hw, GameCanvas.hh - 32, 3);
						if (!isGetData)
						{
							mFont.tahoma_7b_white.drawString(g, mResources.taidulieudechoi, GameCanvas.hw, GameCanvas.hh + 24, 2);
							cmdDownload?.paint(g);
						}
						else
						{
							cmdDownload?.paint(g);
							GameScr.paintOngMauPercent(GameScr.frBarPow20, GameScr.frBarPow21, GameScr.frBarPow22, GameCanvas.w / 2 - 50, GameCanvas.hh + 45, 100, 100f, g);
							GameScr.paintOngMauPercent(GameScr.frBarPow0, GameScr.frBarPow1, GameScr.frBarPow2, GameCanvas.w / 2 - 50, GameCanvas.hh + 45, 100, percent, g);
						}
					}
				}
				else
				{
					int num2 = GameCanvas.hh - 15 * cmd.Length - 15;
					if (num2 < 25)
					{
						num2 = 25;
					}
					if (ModFunc.imgLogoBig != null)
					{
						ModFunc.PaintLogoGif(g, GameCanvas.hw, num2, 3);
					}
					for (int i = 0; i < cmd.Length; i++)
					{
						cmd[i].paint(g);
					}
					g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
					if (testConnect == -1)
					{
						if (GameCanvas.gameTick % 20 > 10)
						{
							g.drawRegion(GameScr.imgRoomStat, 0, 14, 7, 7, 0, (GameCanvas.w - mFont.tahoma_7b_dark.getWidth(cmd[2 + nCmdPlay].caption) >> 1) - 10, cmd[2 + nCmdPlay].y + 10, 0);
						}
					}
					else
					{
						g.drawRegion(GameScr.imgRoomStat, 0, testConnect * 7, 7, 7, 0, (GameCanvas.w - mFont.tahoma_7b_dark.getWidth(cmd[2 + nCmdPlay].caption) >> 1) - 10, cmd[2 + nCmdPlay].y + 9, 0);
					}
				}
			}
			base.paint(g);
		}

		public void selectServer()
		{
			flagServer = 100;
			GameCanvas.startWaitDlg(mResources.PLEASEWAIT);
			Session_ME.gI().close();
			GameMidlet.IP = address[ipSelect];
			GameMidlet.PORT = port[ipSelect];
			GameMidlet.LANGUAGE = language[ipSelect];
			Rms.saveRMSInt("svselect", ipSelect);
			if (language[ipSelect] != mResources.language)
			{
				mResources.loadLanguague(language[ipSelect]);
			}
			LoginScr.serverName = nameServer[ipSelect];
			initCommand();
			loadScreen = true;
			countDieConnect = 0;
			testConnect = -1;
			isAutoConect = true;
		}

		public override void update()
		{
			if (ModFunc.AutoLogin())
			{
				return;
			}
			if (waitToLogin)
			{
				tWaitToLogin++;
				if (tWaitToLogin == 50)
				{
					GameCanvas.serverScreen.selectServer();
				}
				if (tWaitToLogin == 100)
				{
					if (GameCanvas.loginScr == null)
					{
						GameCanvas.loginScr = new LoginScr();
					}
					GameCanvas.loginScr.doLogin();
					Service.gI().finishUpdate();
					waitToLogin = false;
				}
			}
			if (flagServer > 0)
			{
				flagServer--;
				if (flagServer == 0)
				{
					GameCanvas.endDlg();
				}
				if (testConnect == 2)
				{
					flagServer = 0;
					GameCanvas.endDlg();
				}
			}
			if (flagServer <= 0 && isAutoConect)
			{
				countDieConnect++;
				if (countDieConnect > 100000)
				{
					countDieConnect = 0;
				}
			}
			for (int i = 0; i < cmd.Length; i++)
			{
				if (i == selected)
				{
					cmd[i].isFocus = true;
				}
				else
				{
					cmd[i].isFocus = false;
				}
			}
			GameScr.cmx++;
			if (!loadScreen && (bigOk || percent == 100))
			{
				cmdDownload = null;
			}
			base.update();
			if (Char.isLoadingMap || !loadScreen || !isAutoConect || GameCanvas.currentScreen != this || testConnect == 2)
			{
				return;
			}
			if (countDieConnect < 5)
			{
				if (flagServer <= 0)
				{
					flagServer = 100;
					GameCanvas.startWaitDlg(mResources.PLEASEWAIT);
					GameCanvas.connect();
				}
			}
			else if (!Session_ME.gI().isConnected())
			{
				if (flagServer <= 0)
				{
					Command cmdYes = new Command(mResources.YES, GameCanvas.serverScreen, 18, null);
					Command cmdNo = new Command(mResources.NO, GameCanvas.serverScreen, 19, null);
					GameCanvas.startYesNoDlg(mResources.maychutathoacmatsong + ". " + mResources.confirmChangeServer, cmdYes, cmdNo);
					flagServer = 100;
				}
			}
			else if (flagServer <= 0)
			{
				countDieConnect = 0;
			}
		}

		private void processInput()
		{
			if (loadScreen)
			{
				center = new Command(string.Empty, this, cmd[selected].idAction, null);
			}
			else
			{
				center = cmdDownload;
			}
		}

		public static void updateDeleteData()
		{
			if (cmdDeleteRMS != null && cmdDeleteRMS.isPointerPressInside())
			{
				cmdDeleteRMS.performAction();
			}
		}

		public override void updateKey()
		{
			if (GameCanvas.isTouch)
			{
				updateDeleteData();
				if (!loadScreen)
				{
					if (cmdDownload != null && cmdDownload.isPointerPressInside())
					{
						cmdDownload.performAction();
					}
					base.updateKey();
					return;
				}
				for (int i = 0; i < cmd.Length; i++)
				{
					if (cmd[i] == null || !cmd[i].isPointerPressInside())
					{
						continue;
					}
					if (testConnect == -1 || testConnect == 0)
					{
						if (cmd[i].caption.IndexOf(mResources.server) != -1)
						{
							cmd[i].performAction();
						}
					}
					else
					{
						cmd[i].performAction();
					}
				}
			}
			else if (loadScreen)
			{
				if (GameCanvas.keyPressed[8])
				{
					int num = ((mGraphics.zoomLevel <= 1) ? 4 : 2);
					GameCanvas.keyPressed[8] = false;
					selected++;
					if (selected > num)
					{
						selected = 0;
					}
					processInput();
				}
				if (GameCanvas.keyPressed[2])
				{
					int num2 = ((mGraphics.zoomLevel <= 1) ? 4 : 2);
					GameCanvas.keyPressed[2] = false;
					selected--;
					if (selected < 0)
					{
						selected = num2;
					}
					processInput();
				}
			}
			if (!isWait)
			{
				base.updateKey();
			}
		}

		public static void SaveIP()
		{
			try
			{
				Rms.saveRMSString("NRlink2", linkDefault);
				SplashScr.loadIP();
			}
			catch (Exception)
			{
			}
		}

		public static void SaveIPNew(string ip)
		{
			try
			{
				Rms.saveRMSString("NRlink2", ip);
				SplashScr.loadIP();
			}
			catch (Exception)
			{
			}
		}

		public static void LoadIP()
		{
			try
			{
				linkDefault = Management.IpServer;
				GetServerList(linkDefault);
				ipSelect = 0;
				serverPriority = 0;
				SplashScr.loadIP();
			}
			catch (Exception ex)
			{
				mSystem.println("LoadIP Error: " + ex.Message);
			}
		}

		public override void switchToMe()
		{
			EffectManager.remove();
			GameScr.cmy = 0;
			GameScr.cmx = 0;
			initCommand();
			isWait = false;
			GameCanvas.loginScr = null;
			string text = Rms.loadRMSString("ResVersion");
			if (((text == null || !(text != string.Empty)) ? (-1) : int.Parse(text)) > 0)
			{
				loadScreen = true;
				GameCanvas.loadBG(1);
			}
			bigOk = true;
			cmd[2 + nCmdPlay].caption = mResources.server + ": " + nameServer[ipSelect];
			center = new Command(string.Empty, this, cmd[selected].idAction, null);
			cmd[1 + nCmdPlay].caption = mResources.change_account;
			if (cmd.Length == 5 + nCmdPlay)
			{
				cmd[4 + nCmdPlay].caption = "Xóa dữ liệu";
			}
			Char.isLoadingMap = false;
			ModFunc.startAutoItem = false;
			mSystem.resetCurInapp();
			base.switchToMe();
		}

		public void switchToMe2()
		{
			GameScr.cmy = 0;
			GameScr.cmx = 0;
			initCommand();
			isWait = false;
			GameCanvas.loginScr = null;
			string text = Rms.loadRMSString("ResVersion");
			if (((text == null || !(text != string.Empty)) ? (-1) : int.Parse(text)) > 0)
			{
				loadScreen = true;
				GameCanvas.loadBG(1);
			}
			bigOk = true;
			cmd[2 + nCmdPlay].caption = mResources.server + ": " + nameServer[ipSelect];
			center = new Command(string.Empty, this, cmd[selected].idAction, null);
			cmd[1 + nCmdPlay].caption = mResources.change_account;
			if (cmd.Length == 5 + nCmdPlay)
			{
				cmd[4 + nCmdPlay].caption = "Xóa dữ liệu";
			}
			mSystem.resetCurInapp();
			base.switchToMe();
		}

		public void cancel()
		{
			if (GameCanvas.serverScreen == null)
			{
				GameCanvas.serverScreen = new ServerListScreen();
			}
			demPercent = 0;
			percent = 0;
			stopDownload = true;
			GameCanvas.serverScreen.show2();
			isGetData = false;
			cmdDownload.isFocus = true;
			center = new Command(string.Empty, this, 2, null);
		}

		public void perform(int idAction, object p)
		{
			switch (idAction)
			{
			case 1:
			case 4:
				Session_ME.gI().close();
				isAutoConect = false;
				countDieConnect = 0;
				loadScreen = true;
				testConnect = 0;
				isGetData = false;
				Rms.clearAll();
				switchToMe();
				return;
			case 2:
				stopDownload = false;
				cmdDownload = new Command(mResources.huy, this, 4, null);
				cmdDownload.x = GameCanvas.w / 2 - mScreen.cmdW / 2;
				cmdDownload.y = GameCanvas.hh + 65;
				right = null;
				if (!GameCanvas.isTouch)
				{
					cmdDownload.x = GameCanvas.w / 2 - mScreen.cmdW / 2;
					cmdDownload.y = GameCanvas.h - mScreen.cmdH - 1;
				}
				center = new Command(string.Empty, this, 4, null);
				if (!isGetData)
				{
					Service.gI().updateData();
					Service.gI().getResource(1, null);
					if (!GameCanvas.isTouch)
					{
						cmdDownload.isFocus = true;
						center = new Command(string.Empty, this, 4, null);
					}
					isGetData = true;
				}
				return;
			case 3:
			{
				if (GameCanvas.loginScr == null)
				{
					GameCanvas.loginScr = new LoginScr();
				}
				GameCanvas.loginScr.switchToMe();
				bool num = Rms.loadRMSString("acc") != null && !Rms.loadRMSString("acc").Equals(string.Empty);
				bool flag = Rms.loadRMSString("userAo" + ipSelect) != null && !Rms.loadRMSString("userAo" + ipSelect).Equals(string.Empty);
				if (!num && !flag)
				{
					GameCanvas.connect();
					string text = Rms.loadRMSString("userAo" + ipSelect);
					if (text == null || text.Equals(string.Empty))
					{
						Service.gI().login2(string.Empty);
					}
					else
					{
						GameCanvas.loginScr.isLogin2 = true;
						GameCanvas.connect();
						Service.gI().setClientType();
						Service.gI().login(text, string.Empty, GameMidlet.VERSION, 1);
					}
					if (Session_ME.connected)
					{
						GameCanvas.startWaitDlg();
					}
					else
					{
						GameCanvas.startOKDlg(mResources.maychutathoacmatsong);
					}
				}
				else
				{
					GameCanvas.loginScr.doLogin();
				}
				LoginScr.serverName = nameServer[ipSelect];
				return;
			}
			case 5:
				doUpdateServer();
				if (nameServer.Length != 1)
				{
					MyVector myVector = new MyVector(string.Empty);
					for (int i = 0; i < nameServer.Length; i++)
					{
						myVector.addElement(new Command(nameServer[i], this, 6, null));
					}
					GameCanvas.menu.startAt(myVector, 0);
					if (!GameCanvas.isTouch)
					{
						GameCanvas.menu.menuSelectedItem = ipSelect;
					}
				}
				return;
			case 6:
				ipSelect = GameCanvas.menu.menuSelectedItem;
				selectServer();
				return;
			case 7:
				if (GameCanvas.loginScr == null)
				{
					GameCanvas.loginScr = new LoginScr();
				}
				GameCanvas.loginScr.switchToMe();
				return;
			case 9:
				Rms.saveRMSInt("lowGraphic", 1);
				GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
				return;
			case 10:
				Rms.saveRMSInt("lowGraphic", 0);
				GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
				return;
			case 11:
			{
				if (GameCanvas.loginScr == null)
				{
					GameCanvas.loginScr = new LoginScr();
				}
				GameCanvas.loginScr.switchToMe();
				string text2 = Rms.loadRMSString("userAo" + ipSelect);
				if (text2 == null || text2.Equals(string.Empty))
				{
					Service.gI().login2(string.Empty);
				}
				else
				{
					GameCanvas.loginScr.isLogin2 = true;
					GameCanvas.connect();
					Service.gI().setClientType();
					Service.gI().login(text2, string.Empty, GameMidlet.VERSION, 1);
				}
				GameCanvas.startWaitDlg(mResources.PLEASEWAIT);
				return;
			}
			case 12:
				GameMidlet.instance.exit();
				return;
			case 13:
				if (!isGetData || loadScreen)
				{
					switch (mSystem.clientType)
					{
					case 1:
						mSystem.callHotlineJava();
						break;
					case 3:
					case 5:
						mSystem.callHotlineIphone();
						break;
					case 4:
						mSystem.callHotlinePC();
						break;
					case 6:
						mSystem.callHotlineWindowsPhone();
						break;
					case 2:
						break;
					}
					return;
				}
				break;
			case 1000:
				GameCanvas.connect();
				return;
			case 10100:
				if (GameCanvas.loginScr == null)
				{
					GameCanvas.loginScr = new LoginScr();
				}
				GameCanvas.loginScr.switchToMe();
				GameCanvas.connect();
				Service.gI().login2(string.Empty);
				GameCanvas.startWaitDlg();
				LoginScr.serverName = nameServer[ipSelect];
				return;
			}
			switch (idAction)
			{
			case 8:
			{
				Command cmdYes = new Command(mResources.YES, GameCanvas.serverScreen, 15, null);
				Command cmdNo = new Command(mResources.NO, GameCanvas.serverScreen, 16, null);
				GameCanvas.startYesNoDlg(mResources.deletaDataNote, cmdYes, cmdNo);
				break;
			}
			case 15:
				Rms.clearAll();
				GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
				break;
			case 16:
				InfoDlg.hide();
				GameCanvas.currentDialog = null;
				break;
			case 17:
				if (GameCanvas.serverScr == null)
				{
					GameCanvas.serverScr = new ServerScr();
				}
				GameCanvas.serverScr.switchToMe();
				break;
			case 18:
				GameCanvas.endDlg();
				InfoDlg.hide();
				if (GameCanvas.serverScr == null)
				{
					GameCanvas.serverScr = new ServerScr();
				}
				GameCanvas.serverScr.switchToMe();
				break;
			case 19:
				if (mSystem.clientType == 1)
				{
					InfoDlg.hide();
					GameCanvas.currentDialog = null;
				}
				else
				{
					countDieConnect = 0;
					testConnect = 0;
					isAutoConect = true;
				}
				break;
			}
		}

		public void init()
		{
			if (!loadScreen)
			{
				cmdDownload = new Command(mResources.taidulieu, this, 2, null);
				cmdDownload.isFocus = true;
				cmdDownload.x = GameCanvas.w / 2 - mScreen.cmdW / 2;
				cmdDownload.y = GameCanvas.hh + 45;
				if (cmdDownload.y > GameCanvas.h - 26)
				{
					cmdDownload.y = GameCanvas.h - 26;
				}
				cmdDownload.performAction();
			}
			if (!GameCanvas.isTouch)
			{
				selected = 0;
				processInput();
			}
		}

		public void show2()
		{
			GameScr.cmx = 0;
			GameScr.cmy = 0;
			initCommand();
			loadScreen = false;
			percent = 0;
			bigOk = false;
			isGetData = false;
			p = 0;
			demPercent = 0;
			strWait = mResources.PLEASEWAIT;
			Char.isLoadingMap = false;
			init();
			base.switchToMe();
		}
	}
}
