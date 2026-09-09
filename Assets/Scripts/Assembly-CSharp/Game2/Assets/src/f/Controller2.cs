using System;
using Game2.Assets.src.g;

namespace Game2.Assets.src.f
{
	internal class Controller2
	{
		public static void readMessage(Message msg)
		{
			try
			{
				switch (msg.command)
				{
				case sbyte.MinValue:
					readInfoEffChar(msg);
					break;
				case -127:
					readLuckyRound(msg);
					break;
				case -126:
				{
					sbyte b6 = msg.reader().readByte();
					Res.outz("type quay= " + b6);
					if (b6 == 1)
					{
						msg.reader().readByte();
						string num10 = msg.reader().readUTF();
						string finish = msg.reader().readUTF();
						GameScr.gI().showWinNumber(num10, finish);
					}
					if (b6 == 0)
					{
						GameScr.gI().showYourNumber(msg.reader().readUTF());
					}
					break;
				}
				case -125:
				{
					ChatTextField.gI().isShow = false;
					string text3 = msg.reader().readUTF();
					Res.outz("titile= " + text3);
					sbyte b21 = msg.reader().readByte();
					ClientInput.gI().setInput(b21, text3);
					for (int num34 = 0; num34 < b21; num34++)
					{
						ClientInput.gI().tf[num34].name = msg.reader().readUTF();
						sbyte num35 = msg.reader().readByte();
						if (num35 == 0)
						{
							ClientInput.gI().tf[num34].setIputType(TField.INPUT_TYPE_NUMERIC);
						}
						if (num35 == 1)
						{
							ClientInput.gI().tf[num34].setIputType(TField.INPUT_TYPE_ANY);
						}
						if (num35 == 2)
						{
							ClientInput.gI().tf[num34].setIputType(TField.INPUT_TYPE_PASSWORD);
						}
					}
					break;
				}
				case -124:
				{
					sbyte b22 = msg.reader().readByte();
					sbyte num36 = msg.reader().readByte();
					if (num36 == 0)
					{
						if (b22 == 2)
						{
							int num37 = msg.reader().readInt();
							if (num37 == Char.myCharz().charID)
							{
								Char.myCharz().removeEffect();
							}
							else if (GameScr.findCharInMap(num37) != null)
							{
								GameScr.findCharInMap(num37).removeEffect();
							}
						}
						int num38 = msg.reader().readUnsignedByte();
						int num39 = msg.reader().readInt();
						if (num38 == 32)
						{
							if (b22 == 1)
							{
								int num40 = msg.reader().readInt();
								if (num39 == Char.myCharz().charID)
								{
									Char.myCharz().holdEffID = num38;
									GameScr.findCharInMap(num40).setHoldChar(Char.myCharz());
								}
								else if (GameScr.findCharInMap(num39) != null && num40 != Char.myCharz().charID)
								{
									GameScr.findCharInMap(num39).holdEffID = num38;
									GameScr.findCharInMap(num40).setHoldChar(GameScr.findCharInMap(num39));
								}
								else if (GameScr.findCharInMap(num39) != null && num40 == Char.myCharz().charID)
								{
									GameScr.findCharInMap(num39).holdEffID = num38;
									Char.myCharz().setHoldChar(GameScr.findCharInMap(num39));
								}
							}
							else if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().removeHoleEff();
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).removeHoleEff();
							}
						}
						if (num38 == 33)
						{
							if (b22 == 1)
							{
								if (num39 == Char.myCharz().charID)
								{
									Char.myCharz().protectEff = true;
								}
								else if (GameScr.findCharInMap(num39) != null)
								{
									GameScr.findCharInMap(num39).protectEff = true;
								}
							}
							else if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().removeProtectEff();
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).removeProtectEff();
							}
						}
						if (num38 == 39)
						{
							if (b22 == 1)
							{
								if (num39 == Char.myCharz().charID)
								{
									Char.myCharz().huytSao = true;
								}
								else if (GameScr.findCharInMap(num39) != null)
								{
									GameScr.findCharInMap(num39).huytSao = true;
								}
							}
							else if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().removeHuytSao();
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).removeHuytSao();
							}
						}
						if (num38 == 40)
						{
							if (b22 == 1)
							{
								if (num39 == Char.myCharz().charID)
								{
									Char.myCharz().blindEff = true;
								}
								else if (GameScr.findCharInMap(num39) != null)
								{
									GameScr.findCharInMap(num39).blindEff = true;
								}
							}
							else if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().removeBlindEff();
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).removeBlindEff();
							}
						}
						if (num38 == 41)
						{
							if (b22 == 1)
							{
								if (num39 == Char.myCharz().charID)
								{
									Char.myCharz().sleepEff = true;
								}
								else if (GameScr.findCharInMap(num39) != null)
								{
									GameScr.findCharInMap(num39).sleepEff = true;
								}
							}
							else if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().removeSleepEff();
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).removeSleepEff();
							}
						}
						if (num38 == 42)
						{
							if (b22 == 1)
							{
								if (num39 == Char.myCharz().charID)
								{
									Char.myCharz().stone = true;
								}
							}
							else if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().stone = false;
							}
						}
					}
					if (num36 != 1)
					{
						break;
					}
					int num41 = msg.reader().readUnsignedByte();
					sbyte mobIndex = msg.reader().readByte();
					Res.outz("modbHoldID= " + mobIndex + " skillID= " + num41 + "eff ID= " + b22);
					if (num41 == 32)
					{
						if (b22 == 1)
						{
							int num42 = msg.reader().readInt();
							if (num42 == Char.myCharz().charID)
							{
								GameScr.findMobInMap(mobIndex).holdEffID = num41;
								Char.myCharz().setHoldMob(GameScr.findMobInMap(mobIndex));
							}
							else if (GameScr.findCharInMap(num42) != null)
							{
								GameScr.findMobInMap(mobIndex).holdEffID = num41;
								GameScr.findCharInMap(num42).setHoldMob(GameScr.findMobInMap(mobIndex));
							}
						}
						else
						{
							GameScr.findMobInMap(mobIndex).removeHoldEff();
						}
					}
					if (num41 == 40)
					{
						if (b22 == 1)
						{
							GameScr.findMobInMap(mobIndex).blindEff = true;
						}
						else
						{
							GameScr.findMobInMap(mobIndex).removeBlindEff();
						}
					}
					if (num41 == 41)
					{
						if (b22 == 1)
						{
							GameScr.findMobInMap(mobIndex).sleepEff = true;
						}
						else
						{
							GameScr.findMobInMap(mobIndex).removeSleepEff();
						}
					}
					break;
				}
				case -123:
				{
					int charId2 = msg.reader().readInt();
					if (GameScr.findCharInMap(charId2) != null)
					{
						GameScr.findCharInMap(charId2).perCentMp = msg.reader().readByte();
					}
					break;
				}
				case -122:
				{
					Npc npc = GameScr.findNPCInMap(msg.reader().readShort());
					sbyte b7 = msg.reader().readByte();
					npc.duahau = new int[b7];
					for (int n = 0; n < b7; n++)
					{
						npc.duahau[n] = msg.reader().readShort();
					}
					npc.setStatus(msg.reader().readByte(), msg.reader().readInt());
					break;
				}
				case -121:
					Service.logMap = mSystem.currentTimeMillis() - Service.curCheckMap;
					Service.gI().sendCheckMap();
					break;
				case -120:
					Service.logController = mSystem.currentTimeMillis() - Service.curCheckController;
					Service.gI().sendCheckController();
					break;
				case -119:
					Char.myCharz().rank = msg.reader().readInt();
					break;
				case -117:
					GameScr.gI().tMabuEff = 0;
					GameScr.gI().percentMabu = msg.reader().readByte();
					if (GameScr.gI().percentMabu == 100)
					{
						GameScr.gI().mabuEff = true;
					}
					if (GameScr.gI().percentMabu == 101)
					{
						Npc.mabuEff = true;
					}
					break;
				case -116:
					GameScr.canAutoPlay = msg.reader().readByte() == 1;
					break;
				case -115:
					Char.myCharz().setPowerInfo(msg.reader().readUTF(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort());
					break;
				case -113:
				{
					sbyte[] array3 = new sbyte[10];
					for (int l = 0; l < 10; l++)
					{
						array3[l] = msg.reader().readByte();
						Res.outz("vlue i= " + array3[l]);
					}
					GameScr.gI().onKSkill(array3);
					GameScr.gI().onOSkill(array3);
					GameScr.gI().onCSkill(array3);
					break;
				}
				case -111:
				{
					short num11 = msg.reader().readShort();
					ImageSource.vSource = new MyVector();
					for (int m = 0; m < num11; m++)
					{
						string iD = msg.reader().readUTF();
						sbyte version = msg.reader().readByte();
						ImageSource.vSource.addElement(new ImageSource(iD, version));
					}
					ImageSource.checkRMS();
					ImageSource.saveRMS();
					break;
				}
				case -110:
				{
					sbyte num43 = msg.reader().readByte();
					if (num43 == 1)
					{
						int id5 = msg.reader().readInt();
						sbyte[] array11 = Rms.loadRMS(id5 + string.Empty);
						if (array11 == null)
						{
							Service.gI().sendServerData(1, -1, null);
						}
						else
						{
							Service.gI().sendServerData(1, id5, array11);
						}
					}
					if (num43 == 0)
					{
						int num44 = msg.reader().readInt();
						short num45 = msg.reader().readShort();
						sbyte[] data = new sbyte[num45];
						msg.reader().read(ref data, 0, num45);
						Rms.saveRMS(num44 + string.Empty, data);
					}
					break;
				}
				case -106:
				{
					short num28 = msg.reader().readShort();
					int num29 = msg.reader().readShort();
					if (ItemTime.isExistItem(num28))
					{
						ItemTime.getItemById(num28).initTime(num29);
						break;
					}
					ItemTime o = new ItemTime(num28, num29);
					Char.vItemTime.addElement(o);
					break;
				}
				case -105:
					TransportScr.gI().time = 0;
					TransportScr.gI().maxTime = msg.reader().readShort();
					TransportScr.gI().last = (TransportScr.gI().curr = mSystem.currentTimeMillis());
					TransportScr.gI().type = msg.reader().readByte();
					TransportScr.gI().switchToMe();
					break;
				case -103:
					switch (msg.reader().readByte())
					{
					case 0:
					{
						GameCanvas.panel.vFlag.removeAllElements();
						sbyte b13 = msg.reader().readByte();
						for (int num21 = 0; num21 < b13; num21++)
						{
							Item item = new Item();
							short num22 = msg.reader().readShort();
							if (num22 != -1)
							{
								item.template = ItemTemplates.get(num22);
								sbyte b14 = msg.reader().readByte();
								if (b14 != -1)
								{
									item.itemOption = new ItemOption[b14];
									for (int num23 = 0; num23 < item.itemOption.Length; num23++)
									{
										int num24 = msg.reader().readUnsignedByte();
										int param2 = msg.reader().readUnsignedShort();
										if (num24 != -1)
										{
											item.itemOption[num23] = new ItemOption(num24, param2);
										}
									}
								}
							}
							GameCanvas.panel.vFlag.addElement(item);
						}
						GameCanvas.panel.setTypeFlag();
						GameCanvas.panel.show();
						break;
					}
					case 1:
					{
						int num25 = msg.reader().readInt();
						sbyte b15 = msg.reader().readByte();
						Res.outz("---------------actionFlag1:  " + num25 + " : " + b15);
						if (num25 == Char.myCharz().charID)
						{
							Char.myCharz().cFlag = b15;
						}
						else if (GameScr.findCharInMap(num25) != null)
						{
							GameScr.findCharInMap(num25).cFlag = b15;
						}
						GameScr.gI().getFlagImage(num25, b15);
						break;
					}
					case 2:
					{
						sbyte b12 = msg.reader().readByte();
						int num18 = msg.reader().readShort();
						PKFlag pKFlag = new PKFlag();
						pKFlag.cflag = b12;
						pKFlag.IDimageFlag = num18;
						GameScr.vFlag.addElement(pKFlag);
						for (int num19 = 0; num19 < GameScr.vFlag.size(); num19++)
						{
							PKFlag pKFlag2 = (PKFlag)GameScr.vFlag.elementAt(num19);
							Res.outz("i: " + num19 + "  cflag: " + pKFlag2.cflag + "   IDimageFlag: " + pKFlag2.IDimageFlag);
						}
						for (int num20 = 0; num20 < GameScr.vCharInMap.size(); num20++)
						{
							Char char3 = (Char)GameScr.vCharInMap.elementAt(num20);
							if (char3 != null && char3.cFlag == b12)
							{
								char3.flagImage = num18;
							}
						}
						if (Char.myCharz().cFlag == b12)
						{
							Char.myCharz().flagImage = num18;
						}
						break;
					}
					}
					break;
				case -102:
				{
					sbyte b20 = msg.reader().readByte();
					if (b20 != 0 && b20 == 1)
					{
						GameCanvas.loginScr.isLogin2 = false;
						Service.gI().login(Rms.loadRMSString("acc"), Rms.loadRMSString("pass"), GameMidlet.VERSION, 0);
						LoginScr.isLoggingIn = true;
					}
					break;
				}
				case -101:
				{
					GameCanvas.loginScr.isLogin2 = true;
					GameCanvas.connect();
					string text2 = msg.reader().readUTF();
					Rms.saveRMSString("userAo" + ServerListScreen.ipSelect, text2);
					Service.gI().setClientType();
					Service.gI().login(text2, string.Empty, GameMidlet.VERSION, 1);
					break;
				}
				case -100:
				{
					InfoDlg.hide();
					bool flag = false;
					if (GameCanvas.w > 2 * Panel.WIDTH_PANEL)
					{
						flag = true;
					}
					sbyte b3 = msg.reader().readByte();
					Res.outz("t Indxe= " + b3);
					GameCanvas.panel.maxPageShop[b3] = msg.reader().readByte();
					GameCanvas.panel.currPageShop[b3] = msg.reader().readByte();
					Res.outz("max page= " + GameCanvas.panel.maxPageShop[b3] + " curr page= " + GameCanvas.panel.currPageShop[b3]);
					int num6 = msg.reader().readUnsignedByte();
					Char.myCharz().arrItemShop[b3] = new Item[num6];
					for (int j = 0; j < num6; j++)
					{
						short num7 = msg.reader().readShort();
						if (num7 == -1)
						{
							continue;
						}
						Res.outz("template id= " + num7);
						Char.myCharz().arrItemShop[b3][j] = new Item();
						Char.myCharz().arrItemShop[b3][j].template = ItemTemplates.get(num7);
						Char.myCharz().arrItemShop[b3][j].itemId = msg.reader().readShort();
						Char.myCharz().arrItemShop[b3][j].buyCoin = msg.reader().readInt();
						Char.myCharz().arrItemShop[b3][j].buyGold = msg.reader().readInt();
						Char.myCharz().arrItemShop[b3][j].buyType = msg.reader().readByte();
						Char.myCharz().arrItemShop[b3][j].quantity = msg.reader().readInt();
						Char.myCharz().arrItemShop[b3][j].isMe = msg.reader().readByte();
						Panel.strWantToBuy = mResources.say_wat_do_u_want_to_buy;
						sbyte b4 = msg.reader().readByte();
						if (b4 != -1)
						{
							Char.myCharz().arrItemShop[b3][j].itemOption = new ItemOption[b4];
							for (int k = 0; k < Char.myCharz().arrItemShop[b3][j].itemOption.Length; k++)
							{
								int num8 = msg.reader().readUnsignedByte();
								int param = msg.reader().readUnsignedShort();
								if (num8 != -1)
								{
									Char.myCharz().arrItemShop[b3][j].itemOption[k] = new ItemOption(num8, param);
									Char.myCharz().arrItemShop[b3][j].compare = GameCanvas.panel.getCompare(Char.myCharz().arrItemShop[b3][j]);
								}
							}
						}
						if (msg.reader().readByte() == 1)
						{
							int headTemp = msg.reader().readShort();
							int bodyTemp = msg.reader().readShort();
							int legTemp = msg.reader().readShort();
							int bagTemp = msg.reader().readShort();
							Char.myCharz().arrItemShop[b3][j].setPartTemp(headTemp, bodyTemp, legTemp, bagTemp);
						}
					}
					if (flag)
					{
						GameCanvas.panel2.setTabKiGui();
					}
					GameCanvas.panel.setTabShop();
					GameCanvas.panel.cmy = (GameCanvas.panel.cmtoY = 0);
					break;
				}
				case -89:
					GameCanvas.open3Hour = msg.reader().readByte() == 1;
					break;
				case 42:
					GameCanvas.endDlg();
					LoginScr.isContinueToLogin = false;
					Char.isLoadingMap = false;
					msg.reader().readByte();
					if (GameCanvas.registerScr == null)
					{
						GameCanvas.registerScr = new RegisterScreen();
					}
					GameCanvas.registerScr.switchToMe();
					break;
				case 31:
				{
					int num26 = msg.reader().readInt();
					if (msg.reader().readByte() == 1)
					{
						short smallID = msg.reader().readShort();
						sbyte b16 = -1;
						int[] array10 = null;
						short wimg = 0;
						short himg = 0;
						try
						{
							b16 = msg.reader().readByte();
							if (b16 > 0)
							{
								sbyte b17 = msg.reader().readByte();
								array10 = new int[b17];
								for (int num27 = 0; num27 < b17; num27++)
								{
									array10[num27] = msg.reader().readByte();
								}
								wimg = msg.reader().readShort();
								himg = msg.reader().readShort();
							}
						}
						catch (Exception)
						{
						}
						if (num26 == Char.myCharz().charID)
						{
							Char.myCharz().petFollow = new PetFollow();
							Char.myCharz().petFollow.smallID = smallID;
							if (b16 > 0)
							{
								Char.myCharz().petFollow.SetImg(b16, array10, wimg, himg);
							}
							break;
						}
						Char char4 = GameScr.findCharInMap(num26);
						char4.petFollow = new PetFollow();
						char4.petFollow.smallID = smallID;
						if (b16 > 0)
						{
							char4.petFollow.SetImg(b16, array10, wimg, himg);
						}
					}
					else if (num26 == Char.myCharz().charID)
					{
						Char.myCharz().petFollow.remove();
						Char.myCharz().petFollow = null;
					}
					else
					{
						Char char5 = GameScr.findCharInMap(num26);
						char5.petFollow.remove();
						char5.petFollow = null;
					}
					break;
				}
				case 48:
					ServerListScreen.ipSelect = msg.reader().readByte();
					GameCanvas.instance.doResetToLoginScr(GameCanvas.serverScreen);
					Session_ME.gI().close();
					GameCanvas.endDlg();
					ServerListScreen.waitToLogin = true;
					break;
				case 51:
				{
					Mabu mabu = (Mabu)GameScr.findCharInMap(msg.reader().readInt());
					sbyte id4 = msg.reader().readByte();
					short x2 = msg.reader().readShort();
					short y2 = msg.reader().readShort();
					sbyte b11 = msg.reader().readByte();
					Char[] array8 = new Char[b11];
					int[] array9 = new int[b11];
					for (int num16 = 0; num16 < b11; num16++)
					{
						int num17 = msg.reader().readInt();
						Res.outz("char ID=" + num17);
						array8[num16] = null;
						if (num17 != Char.myCharz().charID)
						{
							array8[num16] = GameScr.findCharInMap(num17);
						}
						else
						{
							array8[num16] = Char.myCharz();
						}
						array9[num16] = msg.reader().readInt();
					}
					mabu.setSkill(id4, x2, y2, array8, array9);
					break;
				}
				case 52:
				{
					sbyte num = msg.reader().readByte();
					if (num == 1)
					{
						int num2 = msg.reader().readInt();
						if (num2 == Char.myCharz().charID)
						{
							Char.myCharz().setMabuHold(m: true);
							Char.myCharz().cx = msg.reader().readShort();
							Char.myCharz().cy = msg.reader().readShort();
						}
						else
						{
							Char @char = GameScr.findCharInMap(num2);
							if (@char != null)
							{
								@char.setMabuHold(m: true);
								@char.cx = msg.reader().readShort();
								@char.cy = msg.reader().readShort();
							}
						}
					}
					if (num == 0)
					{
						int num3 = msg.reader().readInt();
						if (num3 == Char.myCharz().charID)
						{
							Char.myCharz().setMabuHold(m: false);
						}
						else
						{
							GameScr.findCharInMap(num3)?.setMabuHold(m: false);
						}
					}
					if (num == 2)
					{
						int charId = msg.reader().readInt();
						int id2 = msg.reader().readInt();
						((Mabu)GameScr.findCharInMap(charId)).eat(id2);
					}
					if (num == 3)
					{
						GameScr.mabuPercent = msg.reader().readByte();
					}
					break;
				}
				case 93:
				{
					string str = msg.reader().readUTF();
					str = Res.changeString(str);
					GameScr.gI().chatVip(str);
					break;
				}
				case 98:
				{
					string notif = msg.reader().readUTF();
					ModFunc.GI().AddNotifTichXanh(notif);
					break;
				}
				case 100:
				{
					sbyte num30 = msg.reader().readByte();
					sbyte b18 = msg.reader().readByte();
					Item item2 = null;
					if (num30 == 0)
					{
						item2 = Char.myCharz().arrItemBody[b18];
					}
					if (num30 == 1)
					{
						item2 = Char.myCharz().arrItemBag[b18];
					}
					short num31 = msg.reader().readShort();
					if (num31 == -1)
					{
						break;
					}
					item2.template = ItemTemplates.get(num31);
					item2.quantity = msg.reader().readInt();
					item2.info = msg.reader().readUTF();
					item2.content = msg.reader().readUTF();
					sbyte b19 = msg.reader().readByte();
					if (b19 == 0)
					{
						break;
					}
					item2.itemOption = new ItemOption[b19];
					for (int num32 = 0; num32 < item2.itemOption.Length; num32++)
					{
						int num33 = msg.reader().readUnsignedByte();
						Res.outz("id o= " + num33);
						int param3 = msg.reader().readUnsignedShort();
						if (num33 != -1)
						{
							item2.itemOption[num32] = new ItemOption(num33, param3);
						}
					}
					break;
				}
				case 101:
				{
					Res.outz("big boss--------------------------------------------------");
					BigBoss bigBoss = Mob.getBigBoss();
					if (bigBoss == null)
					{
						break;
					}
					sbyte b = msg.reader().readByte();
					if (b == 0 || b == 1 || b == 2 || b == 4 || b == 3)
					{
						if (b == 3)
						{
							bigBoss.xTo = (bigBoss.xFirst = msg.reader().readShort());
							bigBoss.yTo = (bigBoss.yFirst = msg.reader().readShort());
							bigBoss.setFly();
						}
						else
						{
							sbyte b2 = msg.reader().readByte();
							Res.outz("CHUONG nChar= " + b2);
							Char[] array = new Char[b2];
							int[] array2 = new int[b2];
							for (int i = 0; i < b2; i++)
							{
								int num5 = msg.reader().readInt();
								Res.outz("char ID=" + num5);
								array[i] = null;
								if (num5 != Char.myCharz().charID)
								{
									array[i] = GameScr.findCharInMap(num5);
								}
								else
								{
									array[i] = Char.myCharz();
								}
								array2[i] = msg.reader().readInt();
							}
							bigBoss.setAttack(array, array2, b);
						}
					}
					if (b == 5)
					{
						bigBoss.haftBody = true;
						bigBoss.status = 2;
					}
					if (b == 6)
					{
						bigBoss.getDataB2();
						bigBoss.x = msg.reader().readShort();
						bigBoss.y = msg.reader().readShort();
					}
					if (b == 7)
					{
						bigBoss.setAttack(null, null, b);
					}
					if (b == 8)
					{
						bigBoss.xTo = (bigBoss.xFirst = msg.reader().readShort());
						bigBoss.yTo = (bigBoss.yFirst = msg.reader().readShort());
						bigBoss.status = 2;
					}
					if (b == 9)
					{
						bigBoss.x = (bigBoss.y = (bigBoss.xTo = (bigBoss.yTo = (bigBoss.xFirst = (bigBoss.yFirst = -1000)))));
					}
					break;
				}
				case 102:
				{
					sbyte b8 = msg.reader().readByte();
					if (b8 == 0 || b8 == 1 || b8 == 2 || b8 == 6)
					{
						BigBoss2 bigBoss2 = Mob.getBigBoss2();
						if (bigBoss2 == null)
						{
							break;
						}
						if (b8 == 6)
						{
							bigBoss2.x = (bigBoss2.y = (bigBoss2.xTo = (bigBoss2.yTo = (bigBoss2.xFirst = (bigBoss2.yFirst = -1000)))));
							break;
						}
						sbyte b9 = msg.reader().readByte();
						Char[] array4 = new Char[b9];
						int[] array5 = new int[b9];
						for (int num12 = 0; num12 < b9; num12++)
						{
							int num13 = msg.reader().readInt();
							array4[num12] = null;
							if (num13 != Char.myCharz().charID)
							{
								array4[num12] = GameScr.findCharInMap(num13);
							}
							else
							{
								array4[num12] = Char.myCharz();
							}
							array5[num12] = msg.reader().readInt();
						}
						bigBoss2.setAttack(array4, array5, b8);
					}
					if (b8 == 3 || b8 == 4 || b8 == 5 || b8 == 7)
					{
						BachTuoc bachTuoc = Mob.getBachTuoc();
						if (bachTuoc == null)
						{
							break;
						}
						switch (b8)
						{
						case 7:
							bachTuoc.x = (bachTuoc.y = (bachTuoc.xTo = (bachTuoc.yTo = (bachTuoc.xFirst = (bachTuoc.yFirst = -1000)))));
							return;
						case 3:
						case 4:
						{
							sbyte b10 = msg.reader().readByte();
							Char[] array6 = new Char[b10];
							int[] array7 = new int[b10];
							for (int num14 = 0; num14 < b10; num14++)
							{
								int num15 = msg.reader().readInt();
								array6[num14] = null;
								if (num15 != Char.myCharz().charID)
								{
									array6[num14] = GameScr.findCharInMap(num15);
								}
								else
								{
									array6[num14] = Char.myCharz();
								}
								array7[num14] = msg.reader().readInt();
							}
							bachTuoc.setAttack(array6, array7, b8);
							break;
						}
						}
						if (b8 == 5)
						{
							short xMoveTo = msg.reader().readShort();
							bachTuoc.move(xMoveTo);
						}
					}
					if (b8 > 9 && b8 < 30)
					{
						readActionBoss(msg, b8);
					}
					break;
				}
				case 114:
					try
					{
						msg.reader().readUTF();
						mSystem.curINAPP = msg.reader().readByte();
						mSystem.maxINAPP = msg.reader().readByte();
					}
					catch (Exception)
					{
					}
					break;
				case 121:
					mSystem.publicID = msg.reader().readUTF();
					mSystem.strAdmob = msg.reader().readUTF();
					Res.outz("SHOW AD public ID= " + mSystem.publicID);
					mSystem.createAdmob();
					break;
				case 122:
				{
					short timeLogin = msg.reader().readShort();
					Res.outz("second login = " + timeLogin);
					LoginScr.timeLogin = timeLogin;
					LoginScr.currTimeLogin = (LoginScr.lastTimeLogin = mSystem.currentTimeMillis());
					GameCanvas.endDlg();
					break;
				}
				case 123:
				{
					Res.outz("SET POSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSss");
					int num9 = msg.reader().readInt();
					short xPos = msg.reader().readShort();
					short yPos = msg.reader().readShort();
					sbyte b5 = msg.reader().readByte();
					Char char2 = null;
					if (num9 == Char.myCharz().charID)
					{
						char2 = Char.myCharz();
					}
					else if (GameScr.findCharInMap(num9) != null)
					{
						char2 = GameScr.findCharInMap(num9);
					}
					if (char2 != null)
					{
						ServerEffect.addServerEffect((b5 != 0) ? 173 : 60, char2, 1);
						char2.setPos(xPos, yPos, b5);
					}
					break;
				}
				case 124:
				{
					short id3 = msg.reader().readShort();
					string text = msg.reader().readUTF();
					Res.outz("noi chuyen = " + text + "npc ID= " + id3);
					GameScr.findNPCInMap(id3)?.addInfo(text);
					break;
				}
				case 125:
				{
					sbyte fusion = msg.reader().readByte();
					int num4 = msg.reader().readInt();
					if (num4 == Char.myCharz().charID)
					{
						Char.myCharz().setFusion(fusion);
					}
					else if (GameScr.findCharInMap(num4) != null)
					{
						GameScr.findCharInMap(num4).setFusion(fusion);
					}
					break;
				}
				case sbyte.MaxValue:
					readInfoRada(msg);
					break;
				case 113:
				{
					int loop = 0;
					int layer = 0;
					int id = 0;
					short x = 0;
					short y = 0;
					short loopCount = -1;
					try
					{
						loop = msg.reader().readByte();
						layer = msg.reader().readByte();
						id = msg.reader().readUnsignedByte();
						x = msg.reader().readShort();
						y = msg.reader().readShort();
						loopCount = msg.reader().readShort();
					}
					catch (Exception)
					{
					}
					EffecMn.addEff(new Effect(id, x, y, layer, loop, loopCount));
					break;
				}
				}
			}
			catch (Exception ex4)
			{
				Res.outz("=====> Controller2 " + ex4.StackTrace);
			}
		}

		private static void readLuckyRound(Message msg)
		{
			try
			{
				switch (msg.reader().readByte())
				{
				case 0:
				{
					sbyte b2 = msg.reader().readByte();
					short[] array2 = new short[b2];
					for (int j = 0; j < b2; j++)
					{
						array2[j] = msg.reader().readShort();
					}
					sbyte b3 = msg.reader().readByte();
					int price = msg.reader().readInt();
					short idTicket = msg.reader().readShort();
					CrackBallScr.gI().SetCrackBallScr(array2, (byte)b3, price, idTicket);
					break;
				}
				case 1:
				{
					sbyte b = msg.reader().readByte();
					short[] array = new short[b];
					for (int i = 0; i < b; i++)
					{
						array[i] = msg.reader().readShort();
					}
					CrackBallScr.gI().DoneCrackBallScr(array);
					break;
				}
				}
			}
			catch (Exception)
			{
			}
		}

		private static void readInfoRada(Message msg)
		{
			readInfoRada(msg, msg.reader().readByte());
		}

		public static void readInfoRada(Message msg, sbyte action)
		{
			try
			{
				switch (action)
				{
				case 0:
				{
					RadarScr.gI();
					MyVector myVector = new MyVector(string.Empty);
					short num2 = msg.reader().readShort();
					int num3 = 0;
					for (int i = 0; i < num2; i++)
					{
						Info_RadaScr info_RadaScr = new Info_RadaScr();
						int id = msg.reader().readShort();
						int no = i + 1;
						int idIcon = msg.reader().readShort();
						sbyte rank = msg.reader().readByte();
						sbyte amount = msg.reader().readByte();
						sbyte max_amount = msg.reader().readByte();
						short templateId = -1;
						Char charInfo = null;
						sbyte b = msg.reader().readByte();
						if (b == 0)
						{
							templateId = msg.reader().readShort();
						}
						else
						{
							short head = msg.reader().readShort();
							int body = msg.reader().readShort();
							int leg = msg.reader().readShort();
							int bag = msg.reader().readShort();
							charInfo = Info_RadaScr.SetCharInfo(head, body, leg, bag);
						}
						string name = msg.reader().readUTF();
						string info = msg.reader().readUTF();
						sbyte b2 = msg.reader().readByte();
						sbyte use = msg.reader().readByte();
						sbyte b3 = msg.reader().readByte();
						ItemOption[] array = null;
						if (b3 != 0)
						{
							array = new ItemOption[b3];
							for (int j = 0; j < array.Length; j++)
							{
								int num4 = msg.reader().readUnsignedByte();
								int param = msg.reader().readUnsignedShort();
								sbyte activeCard = msg.reader().readByte();
								if (num4 != -1)
								{
									array[j] = new ItemOption(num4, param);
									array[j].activeCard = activeCard;
								}
							}
						}
						info_RadaScr.SetInfo(id, no, idIcon, rank, b, templateId, name, info, charInfo, array);
						info_RadaScr.SetLevel(b2);
						info_RadaScr.SetUse(use);
						info_RadaScr.SetAmount(amount, max_amount);
						myVector.addElement(info_RadaScr);
						if (b2 > 0)
						{
							num3++;
						}
					}
					RadarScr.gI().SetRadarScr(myVector, num3, num2);
					RadarScr.gI().switchToMe();
					break;
				}
				case 1:
				{
					int id3 = msg.reader().readShort();
					sbyte use2 = msg.reader().readByte();
					if (Info_RadaScr.GetInfo(RadarScr.list, id3) != null)
					{
						Info_RadaScr.GetInfo(RadarScr.list, id3).SetUse(use2);
					}
					RadarScr.SetListUse();
					break;
				}
				case 2:
				{
					int num5 = msg.reader().readShort();
					sbyte level = msg.reader().readByte();
					int num6 = 0;
					for (int k = 0; k < RadarScr.list.size(); k++)
					{
						Info_RadaScr info_RadaScr2 = (Info_RadaScr)RadarScr.list.elementAt(k);
						if (info_RadaScr2 != null)
						{
							if (info_RadaScr2.id == num5)
							{
								info_RadaScr2.SetLevel(level);
							}
							if (info_RadaScr2.level > 0)
							{
								num6++;
							}
						}
					}
					RadarScr.SetNum(num6, RadarScr.list.size());
					if (Info_RadaScr.GetInfo(RadarScr.listUse, num5) != null)
					{
						Info_RadaScr.GetInfo(RadarScr.listUse, num5).SetLevel(level);
					}
					break;
				}
				case 3:
				{
					int id2 = msg.reader().readShort();
					sbyte amount2 = msg.reader().readByte();
					sbyte max_amount2 = msg.reader().readByte();
					if (Info_RadaScr.GetInfo(RadarScr.list, id2) != null)
					{
						Info_RadaScr.GetInfo(RadarScr.list, id2).SetAmount(amount2, max_amount2);
					}
					if (Info_RadaScr.GetInfo(RadarScr.listUse, id2) != null)
					{
						Info_RadaScr.GetInfo(RadarScr.listUse, id2).SetAmount(amount2, max_amount2);
					}
					break;
				}
				case 4:
				{
					int num = msg.reader().readInt();
					short idAuraEff = msg.reader().readShort();
					Char @char = ((num != Char.myCharz().charID) ? GameScr.findCharInMap(num) : Char.myCharz());
					if (@char != null)
					{
						@char.idAuraEff = idAuraEff;
						@char.idEff_Set_Item = msg.reader().readByte();
					}
					break;
				}
				case 43:
				{
					MyVector myVector2 = new MyVector();
					int num2 = msg.reader().readUnsignedByte();
					for (int l = 0; l < num2; l++)
					{
						PKHistoryEntry entry = new PKHistoryEntry();
						entry.opponentName = msg.reader().readUTF();
						entry.won = msg.reader().readBoolean();
						entry.completedAt = msg.reader().readInt();
						myVector2.addElement(entry);
					}
					GameCanvas.panel.setPKHistory(myVector2);
					break;
				}
				case 44:
				{
					MyVector costumes = new MyVector();
					bool resetCollection = msg.reader().readBoolean();
					bool completeCollection = msg.reader().readBoolean();
					int costumeCount = msg.reader().readUnsignedShort();
					for (int i = 0; i < costumeCount; i++)
					{
						CostumeCollectionEntry entry = new CostumeCollectionEntry();
						entry.id = msg.reader().readShort();
						entry.iconId = msg.reader().readShort();
						entry.name = msg.reader().readUTF();
						entry.description = msg.reader().readUTF();
						entry.unlocked = msg.reader().readBoolean();
						entry.isUsing = msg.reader().readBoolean();
						int level = msg.reader().readUnsignedByte();
						int head = msg.reader().readShort();
						int body = msg.reader().readShort();
						int leg = msg.reader().readShort();
						entry.saleInfo = msg.reader().readUTF();
						entry.rarity = "Cấp: " + level;
						entry.rankColor = (level >= 4) ? 0xFFCE54 : ((level == 3) ? 0xB78CFF : ((level == 2) ? 0x8BD5FF : 0x73D9B0));
						entry.headPart = head;
						entry.bodyPart = body;
						entry.legPart = leg;
						ItemTemplate costumeTemplate = ItemTemplates.get((short)entry.id);
						entry.gender = (costumeTemplate == null) ? ((sbyte)3) : costumeTemplate.gender;
						int optionCount = msg.reader().readUnsignedByte();
						for (int j = 0; j < optionCount; j++)
						{
							string option = msg.reader().readUTF().Trim();
							if (!string.IsNullOrEmpty(option)
								&& !string.Equals(option, "Không có option", StringComparison.OrdinalIgnoreCase))
							{
								entry.options.addElement(option);
							}
						}
						costumes.addElement(entry);
					}
					CostumeCollectionScr.gI().receiveEntries(costumes, resetCollection, completeCollection);
					break;
				}
				case 45:
				{
					MyVector collectionAchievements = new MyVector();
					int ownedCount = msg.reader().readUnsignedShort();
					int totalCount = msg.reader().readUnsignedShort();
					int achievementCount = msg.reader().readUnsignedByte();
					for (int i = 0; i < achievementCount; i++)
					{
						CostumeCollectionAchievement achievement = new CostumeCollectionAchievement();
						achievement.id = msg.reader().readShort();
						achievement.name = msg.reader().readUTF();
						achievement.description = msg.reader().readUTF();
						achievement.progress = msg.reader().readUnsignedShort();
						achievement.target = msg.reader().readUnsignedShort();
						achievement.rewardGold = msg.reader().readLong();
						achievement.rewardGem = msg.reader().readInt();
						achievement.completed = msg.reader().readBoolean();
						achievement.claimed = msg.reader().readBoolean();
						collectionAchievements.addElement(achievement);
					}
					if (msg.reader().available() > 0)
					{
						int detailVersion = msg.reader().readUnsignedByte();
						if (detailVersion >= 1 && msg.reader().available() > 0)
						{
							int detailCount = msg.reader().readUnsignedByte();
							for (int i = 0; i < detailCount; i++)
							{
								int achievementId = msg.reader().readShort();
								CostumeCollectionAchievement detail = null;
								for (int j = 0; j < collectionAchievements.size(); j++)
								{
									CostumeCollectionAchievement candidate = (CostumeCollectionAchievement)collectionAchievements.elementAt(j);
									if (candidate.id == achievementId)
									{
										detail = candidate;
										break;
									}
								}
								string detailDescription = msg.reader().readUTF();
								sbyte conditionType = msg.reader().readByte();
								int rewardItemCount = msg.reader().readUnsignedByte();
								for (int j = 0; j < rewardItemCount; j++)
								{
									CostumeCollectionRewardItem reward = new CostumeCollectionRewardItem();
									reward.id = msg.reader().readShort();
									reward.iconId = msg.reader().readShort();
									reward.quantity = msg.reader().readInt();
									reward.name = msg.reader().readUTF();
									if (detail != null)
									{
										detail.rewardItems.addElement(reward);
									}
								}
								int targetCount = msg.reader().readUnsignedByte();
								for (int j = 0; j < targetCount; j++)
								{
									CostumeCollectionTargetItem target = new CostumeCollectionTargetItem();
									target.id = msg.reader().readShort();
									target.iconId = msg.reader().readShort();
									target.name = msg.reader().readUTF();
									target.owned = msg.reader().readBoolean();
									if (detail != null)
									{
										detail.requiredCostumes.addElement(target);
									}
								}
								if (detail != null)
								{
									detail.description = detailDescription;
									detail.conditionType = conditionType;
								}
							}
						}
					}
					CostumeCollectionScr.gI().receiveAchievements(collectionAchievements, ownedCount, totalCount);
					break;
				}
				}
			}
			catch (Exception)
			{
			}
		}

		private static void readInfoEffChar(Message msg)
		{
			try
			{
				sbyte b = msg.reader().readByte();
				int num = msg.reader().readInt();
				Char @char = ((num != Char.myCharz().charID) ? GameScr.findCharInMap(num) : Char.myCharz());
				switch (b)
				{
				case 0:
				{
					int id = msg.reader().readShort();
					int layer = msg.reader().readByte();
					int loop = msg.reader().readByte();
					short loopCount = msg.reader().readShort();
					sbyte isStand = msg.reader().readByte();
					@char?.addEffChar(new Effect(id, @char, layer, loop, loopCount, isStand));
					break;
				}
				case 1:
				{
					int id2 = msg.reader().readShort();
					@char?.removeEffChar(0, id2);
					break;
				}
				case 2:
					@char?.removeEffChar(-1, 0);
					break;
				}
			}
			catch (Exception)
			{
			}
		}

		private static void readActionBoss(Message msg, int actionBoss)
		{
			try
			{
				NewBoss newBoss = Mob.getNewBoss(msg.reader().readByte());
				if (newBoss == null)
				{
					return;
				}
				if (actionBoss == 10)
				{
					short xMoveTo = msg.reader().readShort();
					short yMoveTo = msg.reader().readShort();
					newBoss.move(xMoveTo, yMoveTo);
				}
				if (actionBoss >= 11 && actionBoss <= 20)
				{
					sbyte b = msg.reader().readByte();
					Char[] array = new Char[b];
					int[] array2 = new int[b];
					for (int i = 0; i < b; i++)
					{
						int num = msg.reader().readInt();
						array[i] = null;
						if (num != Char.myCharz().charID)
						{
							array[i] = GameScr.findCharInMap(num);
						}
						else
						{
							array[i] = Char.myCharz();
						}
						array2[i] = msg.reader().readInt();
					}
					sbyte dir = msg.reader().readByte();
					newBoss.setAttack(array, array2, (sbyte)(actionBoss - 10), dir);
				}
				if (actionBoss == 21)
				{
					newBoss.xTo = msg.reader().readShort();
					newBoss.yTo = msg.reader().readShort();
					newBoss.setFly();
				}
				if (actionBoss == 23)
				{
					newBoss.setDie();
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
