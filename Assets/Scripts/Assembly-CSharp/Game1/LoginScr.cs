using System;
using UnityEngine;

namespace Game1
{
	public class LoginScr : mScreen, IActionListener
	{
		public TField tfUser;

		public TField tfPass;

		public static bool isContinueToLogin = false;

		private int focus;

		private int wC;

		private int yL;

		private int defYL;

		public bool isCheck;

		public bool isRes;

		public Command cmdLogin;

		public Command cmdCheck;

		public Command cmdRes;

		public Command cmdMenu;

		public Command cmdBackFromRegister;

		public string listFAQ = string.Empty;

		public string titleFAQ;

		public string subtitleFAQ;

		private string numSupport = string.Empty;

		public static bool isLocal = false;

		public static bool isUpdateAll;

		public static bool isUpdateData;

		public static bool isUpdateMap;

		public static bool isUpdateSkill;

		public static bool isUpdateItem;

		public static string serverName;

		public int plX;

		public int plY;

		public int lY;

		public int lX;

		public int logoDes;

		public int lineX;

		public int lineY;

		public static int[] bgId = new int[5] { 0, 8, 2, 6, 9 };

		public static bool isTryGetIPFromWap;

		public static short timeLogin;

		public static long lastTimeLogin;

		public static long currTimeLogin;

		private int yt;

		private Command cmdSelect;

		private Command cmdOK;

		private int xLog;

		private int yLog;

		public static GameMidlet m;

		private int yy = GameCanvas.hh - mScreen.ITEM_HEIGHT - 5;

		private int freeAreaHeight;

		private int xP;

		private int yP;

		private int wP;

		private int hP;

		private int t = 20;

		private bool isRegistering;

		private string passRe = string.Empty;

		public bool isFAQ;

		private int tipid = -1;

		public bool isLogin2;

		private int v = 2;

		private int g;

		private int ylogo = -40;

		private int dir = 1;

		public static bool isLoggingIn;

		public LoginScr()
		{
			yLog = GameCanvas.hh - 30;
			TileMap.bgID = (sbyte)(mSystem.currentTimeMillis() % 9);
			if (TileMap.bgID == 5 || TileMap.bgID == 6)
			{
				TileMap.bgID = 4;
			}
			GameScr.loadCamera(fullmScreen: true, -1, -1);
			GameScr.cmx = 100;
			GameScr.cmy = 200;
			Main.closeKeyBoard();
			if (GameCanvas.h > 200)
			{
				defYL = GameCanvas.hh - 80;
			}
			else
			{
				defYL = GameCanvas.hh - 65;
			}
			resetLogo();
			wC = ((GameCanvas.w < 200) ? 140 : 160);
			yt = GameCanvas.hh - mScreen.ITEM_HEIGHT - 5;
			if (GameCanvas.h <= 160)
			{
				yt = 20;
			}
			tfUser = new TField();
			tfUser.y = GameCanvas.hh - mScreen.ITEM_HEIGHT - 9;
			tfUser.width = wC;
			tfUser.height = mScreen.ITEM_HEIGHT + 2;
			tfUser.isFocus = true;
			tfUser.setIputType(TField.INPUT_TYPE_ANY);
			tfUser.name = mResources.phone;
			tfPass = new TField();
			tfPass.y = GameCanvas.hh - 4;
			tfPass.setIputType(TField.INPUT_TYPE_PASSWORD);
			tfPass.width = wC;
			tfPass.height = mScreen.ITEM_HEIGHT + 2;
			yt += 35;
			isCheck = true;
			switch (Rms.loadRMSInt("check"))
			{
			case 2:
				isCheck = false;
				break;
			case 1:
				isCheck = true;
				break;
			}
			tfUser.setText(Rms.loadRMSString("acc"));
			tfPass.setText(Rms.loadRMSString("pass"));
			focus = 0;
			cmdLogin = new Command((GameCanvas.w <= 200) ? mResources.login2 : mResources.login, GameCanvas.instance, 888393, null);
			cmdCheck = new Command(mResources.remember, this, 2001, null);
			cmdRes = new Command(mResources.register, this, 2002, null);
			cmdBackFromRegister = new Command(mResources.CANCEL, this, 10021, null);
			left = (cmdMenu = new Command(mResources.MENU, this, 2003, null));
			freeAreaHeight = tfUser.y - 2 * tfUser.height;
			if (GameCanvas.isTouch)
			{
				cmdLogin.x = GameCanvas.w / 2 + 8;
				cmdMenu.x = GameCanvas.w / 2 - mScreen.cmdW - 8;
				if (GameCanvas.h >= 200)
				{
					cmdLogin.y = yLog + 110;
					cmdMenu.y = yLog + 110;
				}
				cmdBackFromRegister.x = GameCanvas.w / 2 + 3;
				cmdBackFromRegister.y = yLog + 110;
				cmdRes.x = GameCanvas.w / 2 - 84;
				cmdRes.y = cmdMenu.y;
			}
			wP = 170;
			hP = ((!isRes) ? 100 : 110);
			xP = GameCanvas.hw - wP / 2;
			yP = tfUser.y - 15;
			int num = 4;
			int num2 = num * 32 + 23 + 33;
			if (num2 >= GameCanvas.w)
			{
				num--;
				num2 = num * 32 + 23 + 33;
			}
			xLog = GameCanvas.w / 2 - num2 / 2;
			yLog = GameCanvas.hh - 30;
			lY = ((GameCanvas.w < 200) ? (tfUser.y - 30) : (yLog - 30));
			tfUser.x = xLog + 10;
			tfUser.y = yLog + 20;
			cmdOK = new Command(mResources.OK, this, 2008, null);
			cmdOK.x = GameCanvas.w / 2 - 84;
			cmdOK.y = cmdLogin.y;
			ModFunc.cmdAccManager = new Command(ModFunc.strAccManager, ModFunc.GI(), 101, null);
			ModFunc.cmdAccManager.x = GameCanvas.w / 2 + 3;
			ModFunc.cmdAccManager.y = cmdLogin.y;
			center = cmdOK;
			left = ModFunc.cmdAccManager;
			ModFunc.cmdCloseAccManager = new Command(mResources.CLOSE, ModFunc.GI(), 104, null);
			ModFunc.GI().LoadAcc();
		}

		public override void switchToMe()
		{
			isRegistering = false;
			SoundMn.gI().stopAll();
			tfUser.isFocus = true;
			tfPass.isFocus = false;
			if (GameCanvas.isTouch)
			{
				tfUser.isFocus = false;
			}
			GameCanvas.loadBG(1);
			base.switchToMe();
		}

		public void setUserPass()
		{
			string text = Rms.loadRMSString("acc");
			if (text != null && !text.Equals(string.Empty))
			{
				tfUser.setText(text);
			}
			string text2 = Rms.loadRMSString("pass");
			if (text2 != null && !text2.Equals(string.Empty))
			{
				tfPass.setText(text2);
			}
		}

		public void updateTfWhenOpenKb()
		{
		}

		protected void doMenu()
		{
			MyVector myVector = new MyVector();
			myVector.addElement(new Command(mResources.registerNewAcc, this, 2004, null));
			if (!isLogin2)
			{
				myVector.addElement(new Command(mResources.selectServer, this, 1004, null));
			}
			myVector.addElement(new Command(mResources.forgetPass, this, 1003, null));
			myVector.addElement(new Command(mResources.website, this, 1005, null));
			if (Main.isPC)
			{
				myVector.addElement(new Command(mResources.EXIT, GameCanvas.instance, 8885, null));
			}
			GameCanvas.menu.startAt(myVector, 0);
		}

		protected void doRegister()
		{
			if (tfUser.getText().Equals(string.Empty))
			{
				GameCanvas.startOKDlg(mResources.userBlank);
				return;
			}
			tfUser.getText().ToCharArray();
			if (tfPass.getText().Equals(string.Empty))
			{
				GameCanvas.startOKDlg(mResources.passwordBlank);
				return;
			}
			if (tfUser.getText().Length < 5)
			{
				GameCanvas.startOKDlg(mResources.accTooShort);
				return;
			}
			int num = 0;
			string text = null;
			if (mResources.language == 2)
			{
				if (tfUser.getText().IndexOf("@") == -1 || tfUser.getText().IndexOf(".") == -1)
				{
					text = mResources.emailInvalid;
				}
				num = 0;
			}
			else
			{
				try
				{
					long.Parse(tfUser.getText());
					if (tfUser.getText().Length < 8 || tfUser.getText().Length > 12 || (!tfUser.getText().StartsWith("0") && !tfUser.getText().StartsWith("84")))
					{
						text = mResources.phoneInvalid;
					}
					num = 1;
				}
				catch (Exception)
				{
					if (tfUser.getText().IndexOf("@") == -1 || tfUser.getText().IndexOf(".") == -1)
					{
						text = mResources.emailInvalid;
					}
					num = 0;
				}
			}
			if (text != null)
			{
				GameCanvas.startOKDlg(text);
			}
			else
			{
				GameCanvas.msgdlg.setInfo(mResources.plsCheckAcc + ((num != 1) ? (mResources.email + ": ") : (mResources.phone + ": ")) + tfUser.getText() + "\n" + mResources.password + ": " + tfPass.getText(), new Command(mResources.ACCEPT, this, 4000, null), null, new Command(mResources.NO, GameCanvas.instance, 8882, null));
			}
			GameCanvas.currentDialog = GameCanvas.msgdlg;
		}

		protected void doRegister(string user)
		{
			isFAQ = false;
			GameCanvas.startWaitDlg(mResources.CONNECTING);
			GameCanvas.connect();
			GameCanvas.startWaitDlg(mResources.REGISTERING);
			passRe = tfPass.getText();
			Service.gI().requestRegister(user, tfPass.getText(), Rms.loadRMSString("userAo" + ServerListScreen.ipSelect), Rms.loadRMSString("passAo" + ServerListScreen.ipSelect), GameMidlet.VERSION);
			Rms.saveRMSString("acc", user);
			Rms.saveRMSString("pass", tfPass.getText());
			t = 20;
			isRegistering = true;
		}

		public void doLogin()
		{
			string text = Rms.loadRMSString("acc");
			string text2 = Rms.loadRMSString("pass");
			if (text != null && !text.Equals(string.Empty))
			{
				isLogin2 = false;
			}
			else if (Rms.loadRMSString("userAo" + ServerListScreen.ipSelect) != null && !Rms.loadRMSString("userAo" + ServerListScreen.ipSelect).Equals(string.Empty))
			{
				isLogin2 = true;
			}
			else
			{
				isLogin2 = false;
			}
			if ((text == null || text.Equals(string.Empty)) && isLogin2)
			{
				text = Rms.loadRMSString("userAo" + ServerListScreen.ipSelect);
				text2 = "a";
			}
			if (text == null || text2 == null || GameMidlet.VERSION == null || text.Equals(string.Empty))
			{
				return;
			}
			if (text2.Equals(string.Empty))
			{
				focus = 1;
				tfUser.isFocus = false;
				tfPass.isFocus = true;
				if (!GameCanvas.isTouch)
				{
					right = tfPass.cmdClear;
				}
				return;
			}
			if (!Session_ME.gI().isConnected())
			{
				GameCanvas.connect();
			}
			Service.gI().login(text, text2, GameMidlet.VERSION, (sbyte)(isLogin2 ? 1 : 0));
			if (Session_ME.connected)
			{
				GameCanvas.startWaitDlg();
			}
			else
			{
				GameCanvas.startOKDlg(mResources.maychutathoacmatsong);
			}
			focus = 0;
			if (!isLogin2)
			{
				actRegisterLeft();
			}
			GameCanvas.timeBreakLoading = mSystem.currentTimeMillis() + 30000;
			if (ModFunc.isAutoLogin)
			{
				ModFunc.dangLogin = false;
				Controller.isConnectionFail = false;
				Controller.isDisconnected = false;
			}
		}

		public void savePass()
		{
			if (isCheck)
			{
				Rms.saveRMSInt("check", 1);
				Rms.saveRMSString("acc", tfUser.getText().Trim());
				Rms.saveRMSString("pass", tfPass.getText().Trim());
			}
			else
			{
				Rms.saveRMSInt("check", 2);
				Rms.saveRMSString("acc", string.Empty);
				Rms.saveRMSString("pass", string.Empty);
			}
		}

		public override void update()
		{
			if (ModFunc.AutoLogin())
			{
				return;
			}
			if (Main.isWindowsPhone && isRegistering)
			{
				if (t < 0)
				{
					GameCanvas.endDlg();
					Session_ME.gI().close();
					GameCanvas.serverScreen.switchToMe();
					isRegistering = false;
				}
				else
				{
					t--;
				}
			}
			if (timeLogin > 0)
			{
				GameCanvas.startWaitDlg();
				currTimeLogin = mSystem.currentTimeMillis();
				if (currTimeLogin - lastTimeLogin >= 1000)
				{
					timeLogin--;
					if (timeLogin == 0)
					{
						GameCanvas.loginScr.doLogin();
					}
					lastTimeLogin = currTimeLogin;
				}
			}
			if (isLogin2 && !isRes)
			{
				tfUser.name = mResources.phone;
				tfPass.name = mResources.password;
				tfUser.isPaintCarret = false;
				tfPass.isPaintCarret = false;
				tfUser.update();
				tfPass.update();
			}
			else
			{
				tfUser.name = mResources.phone;
				tfPass.name = mResources.password;
				tfUser.update();
				tfPass.update();
			}
			if (TouchScreenKeyboard.visible)
			{
				mGraphics.addYWhenOpenKeyBoard = 50;
			}
			for (int i = 0; i < Effect2.vEffect2.size(); i++)
			{
				((Effect2)Effect2.vEffect2.elementAt(i)).update();
			}
			if (isUpdateAll && !isUpdateData && !isUpdateItem && !isUpdateMap && !isUpdateSkill)
			{
				isUpdateAll = false;
				mSystem.gcc();
				Service.gI().finishUpdate();
			}
			GameScr.cmx++;
			if (GameScr.cmx > GameCanvas.w * 3 + 100)
			{
				GameScr.cmx = 100;
			}
			if (ChatPopup.currChatPopup != null)
			{
				return;
			}
			updateLogo();
			if (g >= 0)
			{
				ylogo += dir * g;
				g += dir * v;
				if (g <= 0)
				{
					dir *= -1;
				}
				if (ylogo > 0)
				{
					dir *= -1;
					g -= 2 * v;
				}
			}
			if (tipid >= 0 && GameCanvas.gameTick % 100 == 0)
			{
				doChangeTip();
			}
			if (isLogin2 && !isRes)
			{
				tfUser.isPaintCarret = false;
				tfPass.isPaintCarret = false;
				tfUser.update();
				tfPass.update();
			}
			else
			{
				tfUser.name = mResources.phone;
				tfPass.name = mResources.password;
				tfUser.update();
				tfPass.update();
			}
			if (GameCanvas.isTouch)
			{
				if (isRes)
				{
					center = cmdRes;
					left = cmdBackFromRegister;
				}
				else
				{
					center = cmdOK;
					left = ModFunc.cmdAccManager;
				}
			}
			else if (isRes)
			{
				center = cmdRes;
				left = cmdBackFromRegister;
			}
			else
			{
				center = cmdOK;
				left = ModFunc.cmdAccManager;
			}
			if (!Main.isPC && !TouchScreenKeyboard.visible && !Main.isMiniApp && !Main.isWindowsPhone)
			{
				string text = tfUser.getText().ToLower().Trim();
				string text2 = tfPass.getText().ToLower().Trim();
				if (!text.Equals(string.Empty) && !text2.Equals(string.Empty))
				{
					doLogin();
				}
				Main.isMiniApp = true;
			}
			updateTfWhenOpenKb();
		}

		private void doChangeTip()
		{
			tipid++;
			if (tipid >= mResources.tips.Length)
			{
				tipid = 0;
			}
			if (GameCanvas.currentDialog == GameCanvas.msgdlg && GameCanvas.msgdlg.isWait)
			{
				GameCanvas.msgdlg.setInfo(mResources.tips[tipid]);
			}
		}

		public void updateLogo()
		{
			if (defYL != yL)
			{
				yL += defYL - yL >> 1;
			}
		}

		public override void keyPress(int keyCode)
		{
			if (tfUser.isFocus)
			{
				tfUser.keyPressed(keyCode);
			}
			else if (tfPass.isFocus)
			{
				tfPass.keyPressed(keyCode);
			}
			base.keyPress(keyCode);
		}

		public override void unLoad()
		{
			base.unLoad();
		}

		public override void paint(mGraphics g)
		{
			GameCanvas.paintBGGameScr(g);
			int num = tfUser.y - 70;
			if (GameCanvas.h <= 220)
			{
				num += 5;
			}
			mFont.tahoma_7_white.drawStringBorder(g, "v" + GameMidlet.VERSION, GameCanvas.w - 2, 12, 1, mFont.tahoma_7_grey);
			if (mSystem.clientType == 1 && !GameCanvas.isTouch)
			{
				mFont.tahoma_7_white.drawStringBorder(g, ServerListScreen.linkweb, GameCanvas.w - 2, GameCanvas.h - 15, 1, mFont.tahoma_7_grey);
			}
			else
			{
				mFont.tahoma_7_white.drawStringBorder(g, ServerListScreen.linkweb, GameCanvas.w - 2, 2, 1, mFont.tahoma_7_grey);
			}
			if (ChatPopup.currChatPopup != null || ChatPopup.serverChatPopUp != null)
			{
				return;
			}
			if (GameCanvas.currentDialog == null)
			{
				if (ModFunc.isOpenAccMAnager)
				{
					int num2 = 180;
					int num3 = ((GameCanvas.w < 200) ? 160 : 260);
					int num4 = (GameCanvas.w - num3) / 2;
					int num5 = (GameCanvas.h - num2) / 2;
					PopUp.paintPopUp(g, num4, num5, num3, num2, -1, isButton: true);
					left = null;
					center = null;
					int num6 = num5 - 15;
					for (int i = 0; i < ModFunc.GI().cmdsChooseAcc.Count; i++)
					{
						ModFunc.GI().cmdsChooseAcc[i].x = num4 + 20;
						ModFunc.GI().cmdsChooseAcc[i].y = num6;
						ModFunc.GI().cmdsChooseAcc[i].paint(g);
						ModFunc.GI().cmdsDelAcc[i].x = num4 + 190;
						ModFunc.GI().cmdsDelAcc[i].y = num6;
						ModFunc.GI().cmdsDelAcc[i].paint(g);
						num6 += 30;
					}
					ModFunc.cmdCloseAccManager.x = (GameCanvas.w - ModFunc.cmdCloseAccManager.w) / 2;
					ModFunc.cmdCloseAccManager.y = num5 + num2 + 10;
					ModFunc.cmdCloseAccManager.paint(g);
					if (GameCanvas.h > 160 && ModFunc.imgLogoBig != null)
					{
						ModFunc.PaintLogoGif(g, GameCanvas.hw, num5 - 20, 3);
					}
				}
				else
				{
					int h = 105;
					int w = ((GameCanvas.w < 200) ? 160 : 180);
					PopUp.paintPopUp(g, xLog, yLog - 10, w, h, -1, isButton: true);
					if (GameCanvas.h > 160 && ModFunc.imgLogoBig != null)
					{
						ModFunc.PaintLogoGif(g, GameCanvas.hw, num, 3);
					}
					int num7 = 4;
					int num8 = num7 * 32 + 23 + 33;
					if (num8 >= GameCanvas.w)
					{
						num7--;
						num8 = num7 * 32 + 23 + 33;
					}
					xLog = GameCanvas.w / 2 - num8 / 2;
					tfUser.x = xLog + 10;
					tfUser.y = yLog + 20;
					tfPass.x = xLog + 10;
					tfPass.y = yLog + 55;
					tfUser.paint(g);
					tfPass.paint(g);
					if (GameCanvas.w < 176)
					{
						mFont.tahoma_7b_green2.drawString(g, mResources.acc + ":", tfUser.x - 35, tfUser.y + 7, 0);
						mFont.tahoma_7b_green2.drawString(g, mResources.pwd + ":", tfPass.x - 35, tfPass.y + 7, 0);
						mFont.tahoma_7b_green2.drawString(g, mResources.server + ":" + serverName, GameCanvas.w / 2, tfPass.y + 32, 2);
					}
				}
			}
			base.paint(g);
		}

		public override void updateKey()
		{
			if (isContinueToLogin)
			{
				return;
			}
			if (!GameCanvas.isTouch)
			{
				if (tfUser.isFocus)
				{
					right = tfUser.cmdClear;
				}
				else
				{
					right = tfPass.cmdClear;
				}
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21])
			{
				focus--;
				if (focus < 0)
				{
					focus = 1;
				}
			}
			else if (GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22] || GameCanvas.keyPressed[16])
			{
				focus++;
				if (focus > 1)
				{
					focus = 0;
				}
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21] || GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22] || GameCanvas.keyPressed[16])
			{
				GameCanvas.clearKeyPressed();
				if (!isLogin2 || isRes)
				{
					if (focus == 1)
					{
						tfUser.isFocus = false;
						tfPass.isFocus = true;
					}
					else if (focus == 0)
					{
						tfUser.isFocus = true;
						tfPass.isFocus = false;
					}
					else
					{
						tfUser.isFocus = false;
						tfPass.isFocus = false;
					}
				}
			}
			if (GameCanvas.isTouch)
			{
				if (isRes)
				{
					center = cmdRes;
					left = cmdBackFromRegister;
				}
				else
				{
					center = cmdOK;
					left = ModFunc.cmdAccManager;
				}
			}
			else if (isRes)
			{
				center = cmdRes;
				left = cmdBackFromRegister;
			}
			else
			{
				center = cmdOK;
				left = ModFunc.cmdAccManager;
			}
			if (GameCanvas.isPointerJustRelease && (!isLogin2 || isRes) && !ModFunc.isOpenAccMAnager)
			{
				if (GameCanvas.isPointerHoldIn(tfUser.x, tfUser.y, tfUser.width, tfUser.height))
				{
					focus = 0;
				}
				else if (GameCanvas.isPointerHoldIn(tfPass.x, tfPass.y, tfPass.width, tfPass.height))
				{
					focus = 1;
				}
			}
			if (Main.isPC && GameCanvas.keyPressed[(!Main.isPC) ? 5 : 25] && right != null)
			{
				right.performAction();
			}
			if (ModFunc.isOpenAccMAnager)
			{
				foreach (Command item in ModFunc.GI().cmdsChooseAcc)
				{
					if (item.isPointerPressInside())
					{
						item.performAction();
					}
				}
				foreach (Command item2 in ModFunc.GI().cmdsDelAcc)
				{
					if (item2.isPointerPressInside())
					{
						item2.performAction();
					}
				}
				if (ModFunc.cmdCloseAccManager.isPointerPressInside())
				{
					ModFunc.cmdCloseAccManager.performAction();
				}
			}
			base.updateKey();
			GameCanvas.clearKeyPressed();
		}

		public void resetLogo()
		{
			yL = -50;
		}

		public void perform(int idAction, object p)
		{
			switch (idAction)
			{
			case 1000:
				try
				{
					GameMidlet.instance.platformRequest((string)p);
				}
				catch (Exception)
				{
				}
				GameCanvas.endDlg();
				break;
			case 1001:
				GameCanvas.endDlg();
				isRes = false;
				break;
			case 1002:
			{
				GameCanvas.startWaitDlg();
				string text3 = Rms.loadRMSString("userAo" + ServerListScreen.ipSelect);
				if (text3 == null || text3.Equals(string.Empty))
				{
					Service.gI().login2(string.Empty);
					break;
				}
				GameCanvas.loginScr.isLogin2 = true;
				GameCanvas.connect();
				Service.gI().setClientType();
				Service.gI().login(text3, string.Empty, GameMidlet.VERSION, 1);
				break;
			}
			case 1003:
				doMenu();
				break;
			case 1004:
				ServerListScreen.doUpdateServer();
				GameCanvas.serverScreen.switchToMe();
				break;
			case 2001:
				if (isCheck)
				{
					isCheck = false;
				}
				else
				{
					isCheck = true;
				}
				break;
			case 2002:
				doRegister();
				break;
			case 2003:
				doMenu();
				break;
			case 2004:
				actRegister();
				break;
			case 2008:
			{
				string text = tfUser.getText().Trim();
				string text2 = tfPass.getText().Trim();
				ModFunc.GI().AddAccount(text, text2);
				Rms.saveRMSString("acc", text);
				Rms.saveRMSString("pass", text2);
				if (ServerListScreen.loadScreen)
				{
					GameCanvas.serverScreen.switchToMe();
				}
				else
				{
					GameCanvas.serverScreen.show2();
				}
				break;
			}
			case 13:
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
				break;
			case 10021:
				actRegisterLeft();
				break;
			case 4000:
				doRegister(tfUser.getText());
				break;
			case 10041:
				Rms.saveRMSInt("lowGraphic", 0);
				GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
				break;
			case 10042:
				Rms.saveRMSInt("lowGraphic", 1);
				GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
				break;
			}
		}

		public void actRegisterLeft()
		{
			if (isLogin2)
			{
				doLogin();
				return;
			}
			isRes = false;
			tfPass.isFocus = false;
			tfUser.isFocus = true;
			left = cmdMenu;
		}

		public void actRegister()
		{
			GameCanvas.endDlg();
			isRes = true;
			tfPass.isFocus = false;
			tfUser.isFocus = true;
		}

		public void backToRegister()
		{
			GameCanvas.timeBreakLoading = mSystem.currentTimeMillis() + 30000;
			ServerListScreen.countDieConnect = 0;
			if (GameCanvas.loginScr.isLogin2)
			{
				GameCanvas.startYesNoDlg(mResources.note, new Command(mResources.YES, GameCanvas.panel, 10019, null), new Command(mResources.NO, GameCanvas.panel, 10020, null));
				return;
			}
			if (Main.isWindowsPhone)
			{
				GameMidlet.isBackWindowsPhone = true;
			}
			GameCanvas.instance.resetToLoginScr = false;
			GameCanvas.instance.doResetToLoginScr(GameCanvas.loginScr);
		}
	}
}
