using System;
using UnityEngine;

namespace Game2
{
	public class CreateCharScr : mScreen, IActionListener
	{
		public static CreateCharScr instance;

		public static bool isCreateChar = false;

		public static TField tAddName;

		public static int indexGender;

		public static int indexHair;

		public static int selected;

		public static int[][] hairID = new int[3][]
		{
			new int[3] { 106, 108, 127 },
			new int[3] { 111, 112, 128 },
			new int[3] { 105, 107, 126 }
		};

		public static int[][] hairIDOld = new int[3][]
		{
			new int[3] { 64, 30, 31 },
			new int[3] { 9, 29, 32 },
			new int[3] { 6, 27, 28 }
		};

		public static int[] defaultLeg = new int[3] { 158, 150, 152 };

		public static int[] defaultBody = new int[3] { 157, 149, 151 };

		private int yButton;

		private int disY;

		public int yBegin;

		private readonly int cx;

		private readonly int cy;

		private readonly int dy = 45;

		private int cp1;

		private int cf;

		private readonly int xPopup;

		private readonly int yPopup;

		private mFont mFontGender;

		public CreateCharScr()
		{
			try
			{
				if (!GameCanvas.lowGraphic)
				{
					loadMapFromResource(new sbyte[3] { 39, 40, 41 });
				}
				loadMapTableFromResource(new sbyte[3] { 39, 40, 41 });
			}
			catch (Exception)
			{
			}
			xPopup = (GameCanvas.w - 160) / 2;
			yPopup = (GameCanvas.h - 160) / 2;
			cx = GameCanvas.w / 2;
			cy = yPopup + 70;
			if (GameCanvas.w <= 200)
			{
				GameScr.setPopupSize(128, 100);
				GameScr.popupX = (GameCanvas.w - 128) / 2;
				GameScr.popupY = 10;
				cy += 15;
				dy -= 15;
			}
			tAddName = new TField
			{
				width = ((GameCanvas.w < 200) ? 60 : 160),
				height = mScreen.ITEM_HEIGHT + 2,
				strInfo = mResources.char_name,
				showSubTextField = true,
				name = mResources.char_name,
				x = xPopup,
				y = yPopup - 30,
				isFocus = !GameCanvas.isTouch
			};
			tAddName.setIputType(TField.INPUT_TYPE_ANY);
			if (tAddName.getText().Equals("@"))
			{
				tAddName.setText(GameCanvas.loginScr.tfUser.getText().Substring(0, GameCanvas.loginScr.tfUser.getText().IndexOf("@")));
			}
			indexGender = 1;
			indexHair = 0;
			center = new Command(mResources.NEWCHAR, this, 8000, null);
			left = new Command(mResources.BACK, this, 8001, null);
			if (!GameCanvas.isTouch)
			{
				right = tAddName.cmdClear;
			}
			yBegin = tAddName.y;
			mFontGender = mFont.tahoma_7b_dark;
		}

		public static CreateCharScr gI()
		{
			if (instance == null)
			{
				instance = new CreateCharScr();
			}
			return instance;
		}

		public static void loadMapFromResource(sbyte[] mapID)
		{
			for (int i = 0; i < mapID.Length; i++)
			{
				DataInputStream dataInputStream = MyStream.readFile("/mymap/" + mapID[i]);
				MapTemplate.tmw[i] = (ushort)dataInputStream.read();
				MapTemplate.tmh[i] = (ushort)dataInputStream.read();
				MapTemplate.maps[i] = new int[dataInputStream.available()];
				for (int j = 0; j < MapTemplate.tmw[i] * MapTemplate.tmh[i]; j++)
				{
					MapTemplate.maps[i][j] = dataInputStream.read();
				}
				MapTemplate.types[i] = new int[MapTemplate.maps[i].Length];
			}
		}

		public void loadMapTableFromResource(sbyte[] mapID)
		{
			if (GameCanvas.lowGraphic)
			{
				return;
			}
			try
			{
				for (int i = 0; i < mapID.Length; i++)
				{
					DataInputStream dataInputStream = MyStream.readFile("/mymap/mapTable" + mapID[i]);
					short num = dataInputStream.readShort();
					MapTemplate.vCurrItem[i] = new MyVector();
					for (int j = 0; j < num; j++)
					{
						short id = dataInputStream.readShort();
						short num2 = dataInputStream.readShort();
						short num3 = dataInputStream.readShort();
						if (TileMap.getBIById(id) == null)
						{
							continue;
						}
						BgItem bIById = TileMap.getBIById(id);
						BgItem bgItem = new BgItem();
						bgItem.id = id;
						bgItem.idImage = bIById.idImage;
						bgItem.dx = bIById.dx;
						bgItem.dy = bIById.dy;
						bgItem.x = num2 * TileMap.size;
						bgItem.y = num3 * TileMap.size;
						bgItem.layer = bIById.layer;
						MapTemplate.vCurrItem[i].addElement(bgItem);
						if (!BgItem.imgNew.containsKey(bgItem.idImage + string.Empty))
						{
							try
							{
								Image image = GameCanvas.loadImage("/mapBackGround/" + bgItem.idImage + ".png");
								if (image == null)
								{
									BgItem.imgNew.put(bgItem.idImage + string.Empty, Image.createRGBImage(new int[1], 1, 1, bl: true));
									Service.gI().getBgTemplate(bgItem.idImage);
								}
								else
								{
									BgItem.imgNew.put(bgItem.idImage + string.Empty, image);
								}
							}
							catch (Exception)
							{
								Image image2 = GameCanvas.loadImage("/mapBackGround/" + bgItem.idImage + ".png");
								if (image2 == null)
								{
									image2 = Image.createRGBImage(new int[1], 1, 1, bl: true);
									Service.gI().getBgTemplate(bgItem.idImage);
								}
								BgItem.imgNew.put(bgItem.idImage + string.Empty, image2);
							}
							BgItem.vKeysLast.addElement(bgItem.idImage + string.Empty);
						}
						if (!BgItem.isExistKeyNews(bgItem.idImage + string.Empty))
						{
							BgItem.vKeysNew.addElement(bgItem.idImage + string.Empty);
						}
						bgItem.changeColor();
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public override void switchToMe()
		{
			LoginScr.isContinueToLogin = false;
			GameCanvas.menu.showMenu = false;
			GameCanvas.endDlg();
			GameCanvas.loadBG(1);
			base.switchToMe();
			indexGender = Res.random(0, 3);
			indexHair = Res.random(0, 3);
			Char.isLoadingMap = false;
			ServerListScreen.countDieConnect = 0;
		}

		public override void keyPress(int keyCode)
		{
			tAddName.keyPressed(keyCode);
		}

		public override void update()
		{
			cp1++;
			if (cp1 > 30)
			{
				cp1 = 0;
			}
			if (cp1 % 15 < 5)
			{
				cf = 0;
			}
			else
			{
				cf = 1;
			}
			tAddName.update();
			tAddName.isFocus = selected == 0;
		}

		public override void updateKey()
		{
			if (GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21])
			{
				selected--;
				if (selected < 0)
				{
					selected = 2;
				}
			}
			else if (GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22] || GameCanvas.keyPressed[16])
			{
				selected++;
				if (selected > 2)
				{
					selected = 0;
				}
			}
			if (selected == 0)
			{
				if (!GameCanvas.isTouch)
				{
					right = tAddName.cmdClear;
				}
				tAddName.update();
				mFontGender = mFont.tahoma_7b_dark;
			}
			else if (selected == 1)
			{
				if (GameCanvas.keyPressed[(!Main.isPC) ? 4 : 23])
				{
					indexGender--;
					if (indexGender < 0)
					{
						indexGender = mResources.MENUGENDER.Length - 1;
					}
				}
				if (GameCanvas.keyPressed[(!Main.isPC) ? 6 : 24])
				{
					indexGender++;
					if (indexGender > mResources.MENUGENDER.Length - 1)
					{
						indexGender = 0;
					}
				}
				right = null;
				mFontGender = mFont.tahoma_7b_blue;
			}
			else if (selected == 2)
			{
				if (GameCanvas.keyPressed[(!Main.isPC) ? 4 : 23])
				{
					indexHair--;
					if (indexHair < 0)
					{
						indexHair = mResources.hairStyleName[0].Length - 1;
					}
				}
				if (GameCanvas.keyPressed[(!Main.isPC) ? 6 : 24])
				{
					indexHair++;
					if (indexHair > mResources.hairStyleName[0].Length - 1)
					{
						indexHair = 0;
					}
				}
				right = null;
				mFontGender = mFont.tahoma_7b_dark;
			}
			if (GameCanvas.isPointerJustRelease)
			{
				int width = mFont.tahoma_7b_dark.getWidth(mResources.MENUGENDER[indexGender]);
				int height = mFont.tahoma_7b_dark.getHeight();
				if (GameCanvas.isPointerHoldIn(tAddName.x, tAddName.y, tAddName.width, tAddName.height))
				{
					selected = 0;
					mFontGender = mFont.tahoma_7b_dark;
				}
				else if (GameCanvas.isPointerHoldIn((GameCanvas.w - width) / 2 - 10, yPopup + 20, width + 10, height + 10))
				{
					mFontGender = mFont.tahoma_7b_blue;
					selected = 1;
				}
				else if (GameCanvas.isPointerHoldIn(xPopup + 40, yPopup + 80, 80, 80))
				{
					selected = 2;
					mFontGender = mFont.tahoma_7b_dark;
				}
				else if (GameCanvas.isPointerHoldIn(xPopup + 10, yPopup + 25, 23, 26))
				{
					selected = 1;
					indexGender--;
					if (indexGender < 0)
					{
						indexGender = 2;
					}
				}
				else if (GameCanvas.isPointerHoldIn(xPopup + 130, yPopup + 25, 23, 26))
				{
					selected = 1;
					indexGender++;
					if (indexGender > 2)
					{
						indexGender = 0;
					}
				}
				else if (GameCanvas.isPointerHoldIn(xPopup + 10, cy + 20, 23, 26))
				{
					selected = 2;
					indexHair--;
					if (indexHair < 0)
					{
						indexHair = 2;
					}
				}
				else if (GameCanvas.isPointerHoldIn(xPopup + 130, cy + 20, 23, 26))
				{
					selected = 2;
					indexHair++;
					if (indexHair > 2)
					{
						indexHair = 0;
					}
				}
			}
			if (!TouchScreenKeyboard.visible)
			{
				base.updateKey();
			}
			GameCanvas.clearKeyHold();
			GameCanvas.clearKeyPressed();
		}

		public override void paint(mGraphics g)
		{
			if (Char.isLoadingMap)
			{
				return;
			}
			GameCanvas.paintBGGameScr(g);
			int num = 30;
			if (GameCanvas.w == 128)
			{
				num = 20;
			}
			g.translate(-g.getTranslateX(), -g.getTranslateY());
			if (GameCanvas.currentDialog == null)
			{
				PopUp.paintPopUp(g, xPopup - 20, yPopup - 60, 200, 220, -1, isButton: true);
				mFontGender.drawString(g, mResources.MENUGENDER[indexGender], GameCanvas.w / 2, yPopup + 30, mFont.CENTER);
				g.drawRegion(GameScr.arrow, 0, 0, 13, 16, 3, xPopup + 20, yPopup + 35, StaticObj.VCENTER_HCENTER);
				g.drawRegion(GameScr.arrow, 0, 0, 13, 16, 0, xPopup + 140, yPopup + 35, StaticObj.VCENTER_HCENTER);
				int num2 = hairID[indexGender][indexHair];
				int num3 = defaultLeg[indexGender];
				int num4 = defaultBody[indexGender];
				g.drawImage(TileMap.bong, cx, cy + dy, 3);
				Part part = null;
				try
				{
					Part part2 = GameScr.parts[num2];
					Part part3 = GameScr.parts[num3];
					part = GameScr.parts[num4];
					SmallImage.drawSmallImage(g, part2.pi[Char.CharInfo[cf][0][0]].id, cx + Char.CharInfo[cf][0][1] + part2.pi[Char.CharInfo[cf][0][0]].dx, cy - Char.CharInfo[cf][0][2] + part2.pi[Char.CharInfo[cf][0][0]].dy + dy, 0, 0);
					SmallImage.drawSmallImage(g, part3.pi[Char.CharInfo[cf][1][0]].id, cx + Char.CharInfo[cf][1][1] + part3.pi[Char.CharInfo[cf][1][0]].dx, cy - Char.CharInfo[cf][1][2] + part3.pi[Char.CharInfo[cf][1][0]].dy + dy, 0, 0);
					SmallImage.drawSmallImage(g, part.pi[Char.CharInfo[cf][2][0]].id, cx + Char.CharInfo[cf][2][1] + part.pi[Char.CharInfo[cf][2][0]].dx, cy - Char.CharInfo[cf][2][2] + part.pi[Char.CharInfo[cf][2][0]].dy + dy, 0, 0);
				}
				catch (Exception ex)
				{
					ModFunc.WriteLog("Error at paint CreateChar: " + ex.Message + " --- " + ex.StackTrace);
				}
				g.drawRegion(GameScr.arrow, 0, 0, 13, 16, 3, xPopup + 20, cy + 30, StaticObj.VCENTER_HCENTER);
				g.drawRegion(GameScr.arrow, 0, 0, 13, 16, 0, xPopup + 140, cy + 30, StaticObj.VCENTER_HCENTER);
				if (!GameCanvas.lowGraphic)
				{
					for (int i = 0; i < MapTemplate.vCurrItem[indexGender].size(); i++)
					{
						BgItem bgItem = (BgItem)MapTemplate.vCurrItem[indexGender].elementAt(i);
						if (bgItem.idImage != -1 && bgItem.layer == 3)
						{
							bgItem.paint(g);
						}
					}
				}
				if (!Main.isPC)
				{
					if (mGraphics.addYWhenOpenKeyBoard != 0)
					{
						yButton = 110;
						disY = 60;
						if (GameCanvas.w > GameCanvas.h)
						{
							yButton = GameScr.popupY + 30 + 3 * num + part.pi[Char.CharInfo[0][2][0]].dy + dy - 15;
							disY = 35;
						}
					}
					else
					{
						yButton = 110;
						disY = 60;
						if (GameCanvas.w > GameCanvas.h)
						{
							yButton = 100;
							disY = 45;
						}
					}
					tAddName.y = yButton - tAddName.height - disY + 5;
				}
				else
				{
					yButton = 110;
					disY = 60;
					if (GameCanvas.w > GameCanvas.h)
					{
						yButton = 100;
						disY = 45;
					}
					tAddName.y = yBegin;
				}
				tAddName.paint(g);
				g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			}
			if (!TouchScreenKeyboard.visible)
			{
				base.paint(g);
			}
		}

		public void perform(int idAction, object p)
		{
			switch (idAction)
			{
			case 8001:
				if (GameCanvas.loginScr.isLogin2)
				{
					GameCanvas.startYesNoDlg(mResources.note, new Command(mResources.YES, this, 10019, null), new Command(mResources.NO, this, 10020, null));
					break;
				}
				if (Main.isWindowsPhone)
				{
					GameMidlet.isBackWindowsPhone = true;
				}
				Session_ME.gI().close();
				GameCanvas.serverScreen.switchToMe();
				break;
			case 8000:
				if (tAddName.getText().Equals(string.Empty))
				{
					GameCanvas.startOKDlg(mResources.char_name_blank);
					break;
				}
				if (tAddName.getText().Length < 5)
				{
					GameCanvas.startOKDlg(mResources.char_name_short);
					break;
				}
				if (tAddName.getText().Length > 15)
				{
					GameCanvas.startOKDlg(mResources.char_name_long);
					break;
				}
				InfoDlg.showWait();
				Service.gI().createChar(tAddName.getText(), indexGender, hairIDOld[indexGender][indexHair]);
				break;
			case 10019:
				Session_ME.gI().close();
				GameCanvas.endDlg();
				GameCanvas.serverScreen.switchToMe();
				break;
			case 10020:
				GameCanvas.endDlg();
				break;
			}
		}
	}
}
