namespace Game2.Assets.src.g
{
	public class RegisterScreen : mScreen, IActionListener
	{
		public TField tfPassword;

		public TField tfUsername;

		public TField tfMaGioiThieu;

		private int focus;

		private readonly Command cmdExit;

		private readonly Command cmdOK;

		public static string serverName;

		public static Image imgTitle;

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

		private int xLog;

		private int yLog;

		private readonly int v = 2;

		private int g;

		private int ylogo = -40;

		private int dir = 1;

		public RegisterScreen()
		{
			yLog = 130;
			TileMap.bgID = (sbyte)(mSystem.currentTimeMillis() % 9);
			if (TileMap.bgID == 5 || TileMap.bgID == 6)
			{
				TileMap.bgID = 4;
			}
			GameScr.loadCamera(fullmScreen: true, -1, -1);
			GameScr.cmx = 100;
			GameScr.cmy = 200;
			tfUsername = new TField
			{
				width = 220,
				height = mScreen.ITEM_HEIGHT + 2,
				name = "Tên tài khoản",
				isFocus = true
			};
			tfPassword = new TField
			{
				width = 220,
				height = mScreen.ITEM_HEIGHT + 2,
				name = "Mật khẩu"
			};
			tfPassword.setIputType(TField.INPUT_TYPE_PASSWORD);
			tfMaGioiThieu = new TField
			{
				width = 220,
				height = mScreen.ITEM_HEIGHT + 2,
				name = "Mã giới thiệu"
			};
			focus = 0;
			int num = 4;
			int num2 = num * 32 + 23 + 33;
			if (num2 >= GameCanvas.w)
			{
				num--;
				num2 = num * 32 + 23 + 33;
			}
			xLog = GameCanvas.w / 2 - num2 / 2;
			yLog = 5;
			lY = ((GameCanvas.w < 200) ? (tfPassword.y - 30) : (yLog - 30));
			tfPassword.x = xLog + 10;
			tfPassword.y = yLog + 20;
			cmdOK = new Command(mResources.OK, this, 2008, null)
			{
				x = GameCanvas.w / 2 - 40,
				y = GameCanvas.h - 70
			};
			cmdExit = new Command("Thoát", this, 1003, null)
			{
				x = GameCanvas.w / 2 - 40,
				y = GameCanvas.h - 40
			};
			if (GameCanvas.w < 250)
			{
				cmdOK.x = GameCanvas.w / 2 - 80;
				cmdExit.x = GameCanvas.w / 2 + 10;
				cmdExit.y = (cmdOK.y = GameCanvas.h - 25);
			}
			center = cmdOK;
			left = cmdExit;
			imgTitle = ModFunc.imgLogoBig;
		}

		public new void switchToMe()
		{
			SoundMn.gI().stopAll();
			focus = 0;
			if (GameCanvas.isTouch)
			{
				tfUsername.isFocus = false;
				focus = -1;
			}
			base.switchToMe();
		}

		public override void update()
		{
			tfPassword.update();
			tfUsername.update();
			tfMaGioiThieu.update();
			for (int i = 0; i < Effect2.vEffect2.size(); i++)
			{
				((Effect2)Effect2.vEffect2.elementAt(i)).update();
			}
			GameScr.cmx++;
			if (GameScr.cmx > GameCanvas.w * 3 + 100)
			{
				GameScr.cmx = 100;
			}
			if (ChatPopup.currChatPopup == null && g >= 0)
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
		}

		public override void keyPress(int keyCode)
		{
			if (tfPassword.isFocus)
			{
				tfPassword.keyPressed(keyCode);
			}
			else if (tfUsername.isFocus)
			{
				tfUsername.keyPressed(keyCode);
			}
			else if (tfMaGioiThieu.isFocus)
			{
				tfMaGioiThieu.keyPressed(keyCode);
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
			if (ChatPopup.currChatPopup != null || ChatPopup.serverChatPopUp != null)
			{
				return;
			}
			if (GameCanvas.currentDialog == null)
			{
				xLog = (GameCanvas.w - tfUsername.width) / 2 - 10;
				int num = tfUsername.height * 3 + 50;
				if (GameCanvas.w < 260)
				{
					xLog = (GameCanvas.w - 240) / 2;
				}
				yLog = (GameCanvas.h - num) / 2;
				PopUp.paintPopUp(g, xLog, yLog, 240, num, -1, isButton: true);
				if (GameCanvas.h > 160 && imgTitle != null)
				{
					g.drawImage(imgTitle, GameCanvas.hw, tfUsername.y - 40, 3);
				}
				tfUsername.x = xLog + 10;
				tfUsername.y = yLog + 15;
				tfPassword.x = tfUsername.x;
				tfPassword.y = tfUsername.y + 30;
				tfMaGioiThieu.x = tfPassword.x;
				tfMaGioiThieu.y = tfPassword.y + 30;
				tfPassword.paint(g);
				tfUsername.paint(g);
				tfMaGioiThieu.paint(g);
				if (GameCanvas.w < 176)
				{
					mFont.tahoma_7b_green2.drawString(g, mResources.acc + ":", tfUsername.x - 35, tfUsername.y + 7, 0);
					mFont.tahoma_7b_green2.drawString(g, mResources.pwd + ":", tfPassword.x - 35, tfPassword.y + 7, 0);
					mFont.tahoma_7b_green2.drawString(g, mResources.server + ": " + serverName, GameCanvas.w / 2, tfPassword.y + 32, 2);
				}
			}
			g.setColor(GameCanvas.skyColor);
			g.fillRect(GameCanvas.w - 40, 4, 36, 11);
			GameCanvas.resetTrans(g);
			base.paint(g);
		}

		private void turnOffFocus()
		{
			tfPassword.isFocus = false;
			tfUsername.isFocus = false;
			tfMaGioiThieu.isFocus = false;
		}

		private void processFocus()
		{
			turnOffFocus();
			switch (focus)
			{
			case 0:
				tfUsername.isFocus = true;
				break;
			case 1:
				tfPassword.isFocus = true;
				break;
			case 2:
				tfMaGioiThieu.isFocus = true;
				break;
			}
		}

		public override void updateKey()
		{
			if (!GameCanvas.isTouch)
			{
				if (tfPassword.isFocus)
				{
					right = tfPassword.cmdClear;
				}
				else if (tfUsername.isFocus)
				{
					right = tfUsername.cmdClear;
				}
				else if (tfMaGioiThieu.isFocus)
				{
					right = tfMaGioiThieu.cmdClear;
				}
			}
			if (GameCanvas.keyPressed[21])
			{
				focus--;
				if (focus < 0)
				{
					focus = 2;
				}
				processFocus();
			}
			else if (GameCanvas.keyPressed[22])
			{
				focus++;
				if (focus > 2)
				{
					focus = 0;
				}
				processFocus();
			}
			if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[22])
			{
				GameCanvas.clearKeyPressed();
				if (focus == 1)
				{
					tfUsername.isFocus = false;
					tfPassword.isFocus = true;
				}
				else if (focus == 0)
				{
					tfUsername.isFocus = true;
					tfPassword.isFocus = false;
				}
				else
				{
					tfUsername.isFocus = false;
					tfPassword.isFocus = false;
				}
			}
			if (GameCanvas.isPointerJustRelease)
			{
				if (GameCanvas.isPointerHoldIn(tfPassword.x, tfPassword.y, tfPassword.width, tfPassword.height))
				{
					focus = 1;
					processFocus();
				}
				else if (GameCanvas.isPointerHoldIn(tfUsername.x, tfUsername.y, tfUsername.width, tfUsername.height))
				{
					focus = 0;
					processFocus();
				}
				else if (GameCanvas.isPointerHoldIn(tfMaGioiThieu.x, tfMaGioiThieu.y, tfMaGioiThieu.width, tfMaGioiThieu.height))
				{
					focus = 2;
					processFocus();
				}
			}
			base.updateKey();
			GameCanvas.clearKeyPressed();
		}

		public void perform(int idAction, object p)
		{
			switch (idAction)
			{
			case 1003:
				GameCanvas.serverScreen.switchToMe();
				break;
			case 2008:
				if (tfUsername.getText().Equals(string.Empty) || tfPassword.getText().Equals(string.Empty))
				{
					GameCanvas.startOKDlg("Vui lòng điền đầy đủ thông tin");
					break;
				}
				GameCanvas.startOKDlg(mResources.PLEASEWAIT);
				Service.gI().charInfo("1", "1", "1", "1", "1", "1", (tfMaGioiThieu.getText().Length > 0) ? tfMaGioiThieu.getText() : "-1", tfUsername.getText(), tfPassword.getText());
				break;
			}
		}
	}
}
