using System;
using Game1.Assets.src.e;
using Game1.Assets.src.f;
using Game1.Assets.src.g;
using Game1.Mod.XMAP;
using UnityEngine;

namespace Game1
{
	public class Controller : IMessageHandler
	{
		protected static Controller me;

		public Message messWait;

		public static bool isLoadingData = false;

		public static bool isConnectOK;

		public static bool isConnectionFail;

		public static bool isDisconnected;

		public static bool isMain;

		public static bool isStopReadMessage;

		public static MyHashTable frameHT_NEWBOSS = new MyHashTable();

		// The server can split a large item-template refresh into more than one
		// subtype-2 packet. This is reset by subtype 1 and committed by subtype 3.
		private int itemAppendPacketCount;

		private int itemNextTemplateId;

		public static Controller gI()
		{
			if (me == null)
			{
				me = new Controller();
			}
			return me;
		}

		public void onConnectOK(bool isMain1)
		{
			isMain = isMain1;
			mSystem.onConnectOK();
		}

		public void onConnectionFail(bool isMain1)
		{
			isMain = isMain1;
			mSystem.onConnectionFail();
		}

		public void onDisconnected(bool isMain1)
		{
			isMain = isMain1;
			mSystem.onDisconnected();
		}

		public void requestItemPlayer(Message msg)
		{
			try
			{
				int num = msg.reader().readUnsignedByte();
				Item item = GameScr.currentCharViewInfo.arrItemBody[num];
				item.saleCoinLock = msg.reader().readInt();
				item.sys = msg.reader().readByte();
				item.options = new MyVector();
				try
				{
					while (true)
					{
						item.options.addElement(new ItemOption(msg.reader().readUnsignedByte(), msg.reader().readUnsignedShort()));
					}
				}
				catch (Exception)
				{
				}
			}
			catch (Exception)
			{
			}
		}

		public void onMessage(Message msg)
		{
			GameCanvas.debugSession.removeAllElements();
			GameCanvas.debug("SA1", 2);
			try
			{
				Char @char = null;
				MyVector myVector = new MyVector();
				int num = 0;
				GameCanvas.timeLoading = 15;
				Controller2.readMessage(msg);
				switch (msg.command)
				{
				case 127:
				{
					sbyte clanPacketAction = msg.reader().readByte();
					if (!ClanTree.receive(msg, clanPacketAction) && !ClanProgression.receive(msg, clanPacketAction) && !ClanShop.receive(msg, clanPacketAction))
					{
						ClanTreasury.receive(msg, clanPacketAction);
					}
				}
					break;
				case 126:
					FishingQuickTime.readMessage(msg);
					break;
				case -112:
				{
					sbyte num65 = msg.reader().readByte();
					if (num65 == 0)
					{
						GameScr.findMobInMap(msg.reader().readByte()).clearBody();
					}
					if (num65 == 1)
					{
						GameScr.findMobInMap(msg.reader().readByte()).setBody(msg.reader().readShort());
					}
					break;
				}
				case -109:
					Char.myPetz().cHPGoc = msg.readLong();
					Char.myPetz().cMPGoc = msg.readLong();
					Char.myPetz().cDamGoc = msg.readLong();
					Char.myPetz().cDefGoc = msg.reader().readInt();
					Char.myPetz().cCriticalGoc = msg.reader().readInt();
					break;
				case -107:
				{
					sbyte num179 = msg.reader().readByte();
					if (num179 == 0)
					{
						Char.myCharz().havePet = false;
					}
					if (num179 == 1)
					{
						Char.myCharz().havePet = true;
					}
					if (num179 == 2)
					{
						InfoDlg.hide();
						Char.myPetz().head = msg.reader().readShort();
						Char.myPetz().setDefaultPart();
						int num180 = msg.reader().readUnsignedByte();
						Char.myPetz().arrItemBody = new Item[num180];
						for (int num181 = 0; num181 < num180; num181++)
						{
							short num182 = msg.reader().readShort();
							if (num182 == -1)
							{
								continue;
							}
							Char.myPetz().arrItemBody[num181] = new Item
							{
								template = ItemTemplates.get(num182)
							};
							int type4 = Char.myPetz().arrItemBody[num181].template.type;
							Char.myPetz().arrItemBody[num181].quantity = msg.reader().readInt();
							Char.myPetz().arrItemBody[num181].info = msg.reader().readUTF();
							Char.myPetz().arrItemBody[num181].content = msg.reader().readUTF();
							int num183 = msg.reader().readUnsignedByte();
							if (num183 != 0)
							{
								Char.myPetz().arrItemBody[num181].itemOption = new ItemOption[num183];
								for (int num184 = 0; num184 < Char.myPetz().arrItemBody[num181].itemOption.Length; num184++)
								{
									int num185 = msg.reader().readUnsignedByte();
									int param7 = msg.reader().readUnsignedShort();
									if (num185 != -1)
									{
										Char.myPetz().arrItemBody[num181].itemOption[num184] = new ItemOption(num185, param7);
									}
								}
							}
							switch (type4)
							{
							case 0:
								Char.myPetz().body = Char.myPetz().arrItemBody[num181].template.part;
								break;
							case 1:
								Char.myPetz().leg = Char.myPetz().arrItemBody[num181].template.part;
								break;
							}
						}
						Char.myPetz().cHP = msg.readLong();
						Char.myPetz().cHPFull = msg.readLong();
						Char.myPetz().cMP = msg.readLong();
						Char.myPetz().cMPFull = msg.readLong();
						Char.myPetz().cDamFull = msg.readLong();
						Char.myPetz().cName = msg.reader().readUTF();
						Char.myPetz().currStrLevel = msg.reader().readUTF();
						Char.myPetz().cPower = msg.reader().readLong();
						Char.myPetz().cTiemNang = msg.reader().readLong();
						Char.myPetz().petStatus = msg.reader().readByte();
						Char.myPetz().cStamina = msg.reader().readShort();
						Char.myPetz().cMaxStamina = msg.reader().readShort();
						Char.myPetz().cCriticalFull = msg.reader().readByte();
                        Char.myPetz().cDefull = msg.reader().readInt();
						Char.myPetz().arrPetSkill = new Skill[msg.reader().readByte()];
						for (int num186 = 0; num186 < Char.myPetz().arrPetSkill.Length; num186++)
						{
							short num187 = msg.reader().readShort();
							if (num187 != -1)
							{
								Char.myPetz().arrPetSkill[num186] = Skills.get(num187);
								continue;
							}
							Char.myPetz().arrPetSkill[num186] = new Skill();
							Char.myPetz().arrPetSkill[num186].template = null;
							Char.myPetz().arrPetSkill[num186].moreInfo = msg.reader().readUTF();
						}
						if (!ModFunc.userOpenPet)
						{
							return;
						}
						if (GameCanvas.w > 2 * Panel.WIDTH_PANEL)
						{
							GameCanvas.panel2 = new Panel();
							GameCanvas.panel2.tabName[7] = new string[1][] { new string[1] { string.Empty } };
							GameCanvas.panel2.setTypeBodyOnly();
							GameCanvas.panel2.show();
							GameCanvas.panel.setTypePetMain();
							GameCanvas.panel.show();
							ModFunc.userOpenPet = false;
						}
						else
						{
							GameCanvas.panel.tabName[21] = mResources.petMainTab;
							GameCanvas.panel.setTypePetMain();
							GameCanvas.panel.show();
							ModFunc.userOpenPet = false;
						}
					}
					goto case -66;
				}
				case -99:
					InfoDlg.hide();
					if (msg.reader().readByte() == 0)
					{
						GameCanvas.panel.vEnemy.removeAllElements();
						int num114 = msg.reader().readUnsignedByte();
						for (int num115 = 0; num115 < num114; num115++)
						{
							Char char7 = new Char();
							char7.charID = msg.reader().readInt();
							char7.head = msg.reader().readShort();
							char7.headICON = msg.reader().readShort();
							char7.body = msg.reader().readShort();
							char7.leg = msg.reader().readShort();
							char7.bag = msg.reader().readShort();
							char7.cName = msg.reader().readUTF();
							InfoItem infoItem = new InfoItem(msg.reader().readUTF());
							bool isOnline = msg.reader().readBoolean();
							infoItem.charInfo = char7;
							infoItem.isOnline = isOnline;
							GameCanvas.panel.vEnemy.addElement(infoItem);
						}
						GameCanvas.panel.setTypeEnemy();
						GameCanvas.panel.show();
					}
					break;
				case -98:
				{
					bool num66 = msg.reader().readByte() != 0;
					GameCanvas.menu.showMenu = false;
					if (!num66)
					{
						GameCanvas.startYesNoDlg(msg.reader().readUTF(), new Command(mResources.YES, GameCanvas.instance, 888397, msg.reader().readUTF()), new Command(mResources.NO, GameCanvas.instance, 888396, null));
					}
					break;
				}
				case -97:
					ActivityScreen.gI().receiveLegacyDailyPoints(msg.reader().readInt());
					break;
				case -58:
					ActivityScreen.gI().receive(msg);
					break;
				case -96:
				{
					sbyte typeTop = msg.reader().readByte();
					GameCanvas.panel.vTop.removeAllElements();
					string topName = msg.reader().readUTF();
					sbyte b21 = msg.reader().readByte();
					for (int num58 = 0; num58 < b21; num58++)
					{
						int rank = msg.reader().readInt();
						int pId = msg.reader().readInt();
						short headID = msg.reader().readShort();
						short headICON = msg.reader().readShort();
						short body = msg.reader().readShort();
						short leg = msg.reader().readShort();
						string name = msg.reader().readUTF();
						string info3 = msg.reader().readUTF();
						TopInfo o2 = new TopInfo
						{
							rank = rank,
							headID = headID,
							headICON = headICON,
							body = body,
							leg = leg,
							name = name,
							info = info3,
							info2 = msg.reader().readUTF(),
							pId = pId
						};
						GameCanvas.panel.vTop.addElement(o2);
					}
					GameCanvas.panel.topName = topName;
					GameCanvas.panel.setTypeTop(typeTop);
					GameCanvas.panel.show();
					break;
				}
				case -95:
				{
					sbyte b28 = msg.reader().readByte();
					if (b28 == 0)
					{
						int num87 = msg.reader().readInt();
						short templateId = msg.reader().readShort();
						long num88 = msg.readLong();
						SoundMn.gI().explode_1();
						if (num87 == Char.myCharz().charID)
						{
							Char.myCharz().mobMe = new Mob(num87, isDisable: false, isDontMove: false, isFire: false, isIce: false, isWind: false, templateId, 1, num88, 0, num88, (short)(Char.myCharz().cx + ((Char.myCharz().cdir != 1) ? (-40) : 40)), (short)Char.myCharz().cy, 4, 0)
							{
								isMobMe = true
							};
							EffecMn.addEff(new Effect(18, Char.myCharz().mobMe.x, Char.myCharz().mobMe.y, 2, 10, -1));
							Char.myCharz().tMobMeBorn = 30;
							GameScr.vMob.addElement(Char.myCharz().mobMe);
						}
						else
						{
							@char = GameScr.findCharInMap(num87);
							if (@char != null)
							{
								@char.mobMe = new Mob(num87, isDisable: false, isDontMove: false, isFire: false, isIce: false, isWind: false, templateId, 1, num88, 0, num88, (short)@char.cx, (short)@char.cy, 4, 0)
								{
									isMobMe = true
								};
								GameScr.vMob.addElement(@char.mobMe);
							}
							else if (GameScr.findMobInMap(num87) == null)
							{
								Mob o3 = new Mob(num87, isDisable: false, isDontMove: false, isFire: false, isIce: false, isWind: false, templateId, 1, num88, 0, num88, -100, -100, 4, 0)
								{
									isMobMe = true
								};
								GameScr.vMob.addElement(o3);
							}
						}
					}
					if (b28 == 1)
					{
						int num89 = msg.reader().readInt();
						int mobId = msg.reader().readByte();
						if (num89 == Char.myCharz().charID)
						{
							if (GameScr.findMobInMap(mobId) != null)
							{
								Char.myCharz().mobMe.attackOtherMob(GameScr.findMobInMap(mobId));
							}
						}
						else
						{
							@char = GameScr.findCharInMap(num89);
							if (@char != null && GameScr.findMobInMap(mobId) != null)
							{
								@char.mobMe.attackOtherMob(GameScr.findMobInMap(mobId));
							}
						}
					}
					if (b28 == 2)
					{
						int num90 = msg.reader().readInt();
						int num91 = msg.reader().readInt();
						long num92 = msg.readLong();
						long cHPNew = msg.readLong();
						if (num90 == Char.myCharz().charID)
						{
							@char = GameScr.findCharInMap(num91);
							if (@char != null)
							{
								@char.cHPNew = cHPNew;
								if (Char.myCharz().mobMe.isBusyAttackSomeOne)
								{
									@char.doInjure(num92, 0L, isCrit: false, isMob: true);
								}
								else
								{
									Char.myCharz().mobMe.dame = num92;
									Char.myCharz().mobMe.setAttack(@char);
								}
							}
						}
						else
						{
							Mob mob4 = GameScr.findMobInMap(num90);
							if (mob4 != null)
							{
								if (num91 == Char.myCharz().charID)
								{
									Char.myCharz().cHPNew = cHPNew;
									if (mob4.isBusyAttackSomeOne)
									{
										Char.myCharz().doInjure(num92, 0L, isCrit: false, isMob: true);
									}
									else
									{
										mob4.dame = num92;
										mob4.setAttack(Char.myCharz());
									}
								}
								else
								{
									@char = GameScr.findCharInMap(num91);
									if (@char != null)
									{
										@char.cHPNew = cHPNew;
										if (mob4.isBusyAttackSomeOne)
										{
											@char.doInjure(num92, 0L, isCrit: false, isMob: true);
										}
										else
										{
											mob4.dame = num92;
											mob4.setAttack(@char);
										}
									}
								}
							}
						}
					}
					if (b28 == 3)
					{
						int num93 = msg.reader().readInt();
						int mobId2 = msg.reader().readInt();
						long hp = msg.readLong();
						long num94 = msg.readLong();
						@char = null;
						@char = ((Char.myCharz().charID != num93) ? GameScr.findCharInMap(num93) : Char.myCharz());
						if (@char != null)
						{
							Mob mob5 = GameScr.findMobInMap(mobId2);
							if (@char.mobMe != null)
							{
								@char.mobMe.attackOtherMob(mob5);
							}
							if (mob5 != null)
							{
								mob5.hp = hp;
								mob5.updateHp_bar();
								if (num94 == 0L)
								{
									mob5.x = mob5.xFirst;
									mob5.y = mob5.yFirst;
									GameScr.startFlyText(mResources.miss, mob5.x, mob5.y - mob5.h, 0, -2, mFont.MISS);
								}
								else
								{
									GameScr.startFlyText("-" + num94, mob5.x, mob5.y - mob5.h, 0, -2, mFont.ORANGE);
								}
							}
						}
					}
					if (b28 == 5)
					{
						int num95 = msg.reader().readInt();
						sbyte b29 = msg.reader().readByte();
						int mobId3 = msg.reader().readInt();
						long num96 = msg.readLong();
                                long num97 = msg.readLong();
						@char = null;
						@char = ((num95 != Char.myCharz().charID) ? GameScr.findCharInMap(num95) : Char.myCharz());
						if (@char == null)
						{
							return;
						}
						if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
						{
							@char.setSkillPaint(GameScr.sks[b29], 0);
						}
						else
						{
							@char.setSkillPaint(GameScr.sks[b29], 1);
						}
						Mob mob6 = GameScr.findMobInMap(mobId3);
						if (@char.cx <= mob6.x)
						{
							@char.cdir = 1;
						}
						else
						{
							@char.cdir = -1;
						}
						@char.mobFocus = mob6;
						mob6.hp = num97;
						mob6.updateHp_bar();
						if (num96 == 0)
						{
							mob6.x = mob6.xFirst;
							mob6.y = mob6.yFirst;
							GameScr.startFlyText(mResources.miss, mob6.x, mob6.y - mob6.h, 0, -2, mFont.MISS);
						}
						else
						{
							GameScr.startFlyText("-" + num96, mob6.x, mob6.y - mob6.h, 0, -2, mFont.ORANGE);
						}
					}
					if (b28 == 6)
					{
						int num98 = msg.reader().readInt();
						if (num98 == Char.myCharz().charID)
						{
							Char.myCharz().mobMe.startDie();
						}
						else
						{
							GameScr.findCharInMap(num98)?.mobMe.startDie();
						}
					}
					if (b28 != 7)
					{
						break;
					}
					int num99 = msg.reader().readInt();
					if (num99 == Char.myCharz().charID)
					{
						Char.myCharz().mobMe = null;
						for (int num100 = 0; num100 < GameScr.vMob.size(); num100++)
						{
							if (((Mob)GameScr.vMob.elementAt(num100)).mobId == num99)
							{
								GameScr.vMob.removeElementAt(num100);
							}
						}
						break;
					}
					@char = GameScr.findCharInMap(num99);
					for (int num101 = 0; num101 < GameScr.vMob.size(); num101++)
					{
						if (((Mob)GameScr.vMob.elementAt(num101)).mobId == num99)
						{
							GameScr.vMob.removeElementAt(num101);
						}
					}
					if (@char != null)
					{
						@char.mobMe = null;
					}
					break;
				}
				case -94:
					while (msg.reader().available() > 0)
					{
						short num124 = msg.reader().readShort();
						int num125 = msg.reader().readInt();
						for (int num126 = 0; num126 < Char.myCharz().vSkill.size(); num126++)
						{
							Skill skill = (Skill)Char.myCharz().vSkill.elementAt(num126);
							if (skill != null && skill.skillId == num124 && num125 < skill.coolDown)
							{
								skill.lastTimeUseThisSkill = mSystem.currentTimeMillis() - (skill.coolDown - num125);
							}
						}
					}
					break;
				case -93:
				{
					short num11 = msg.reader().readShort();
					BgItem.newSmallVersion = new sbyte[num11];
					for (int num12 = 0; num12 < num11; num12++)
					{
						BgItem.newSmallVersion[num12] = msg.reader().readByte();
					}
					break;
				}
				case -92:
					Main.typeClient = msg.reader().readByte();
					if (Rms.loadRMSString("ResVersion") == null)
					{
						Rms.clearAll();
					}
					Rms.saveRMSInt("clienttype", Main.typeClient);
					Rms.saveRMSInt("lastZoomlevel", mGraphics.zoomLevel);
					if (Rms.loadRMSString("ResVersion") == null)
					{
						GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
					}
					break;
				case -91:
				{
					sbyte b20 = msg.reader().readByte();
					GameCanvas.panel.mapNames = new string[b20];
					GameCanvas.panel.planetNames = new string[b20];
					for (int num57 = 0; num57 < b20; num57++)
					{
						GameCanvas.panel.mapNames[num57] = msg.reader().readUTF();
						GameCanvas.panel.planetNames[num57] = msg.reader().readUTF();
					}
					AutoXmap.ShowPanelMapTrans();
					break;
				}
				case -90:
				{
					sbyte num13 = msg.reader().readByte();
					int num14 = msg.reader().readInt();
					@char = ((Char.myCharz().charID != num14) ? GameScr.findCharInMap(num14) : Char.myCharz());
					if (num13 != -1)
					{
						short num15 = msg.reader().readShort();
						short num16 = msg.reader().readShort();
						short num17 = msg.reader().readShort();
						sbyte isMonkey = msg.reader().readByte();
						if (@char != null)
						{
							if (@char.charID == num14)
							{
								@char.isMask = true;
								@char.isMonkey = isMonkey;
								if (@char.isMonkey != 0)
								{
									@char.isWaitMonkey = false;
									@char.isLockMove = false;
								}
							}
							else if (@char != null)
							{
								@char.isMask = true;
								@char.isMonkey = isMonkey;
							}
							if (num15 != -1)
							{
								@char.head = num15;
							}
							if (num16 != -1)
							{
								@char.body = num16;
							}
							if (num17 != -1)
							{
								@char.leg = num17;
							}
						}
					}
					if (num13 == -1 && @char != null)
					{
						@char.isMask = false;
						@char.isMonkey = 0;
					}
					if (@char != null)
					{
					}
					break;
				}
				case -88:
					GameCanvas.endDlg();
					GameCanvas.serverScreen.switchToMe();
					break;
				case -87:
				{
					msg.reader().mark(100000);
					createData(msg.reader(), isSaveRMS: true);
					msg.reader().reset();
					sbyte[] data = new sbyte[msg.reader().available()];
					msg.reader().readFully(ref data);
					sbyte[] data2 = new sbyte[1] { GameScr.vcData };
					Rms.saveRMS("NRdataVersion", data2);
					LoginScr.isUpdateData = false;
					if (GameScr.vsData == GameScr.vcData && GameScr.vsMap == GameScr.vcMap && GameScr.vsSkill == GameScr.vcSkill && GameScr.vsItem == GameScr.vcItem)
					{
						GameScr.gI().readDart();
						GameScr.gI().readEfect();
						GameScr.gI().readArrow();
						GameScr.gI().readSkill();
						Service.gI().clientOk();
						return;
					}
					break;
				}
				case -86:
				{
					sbyte b13 = msg.reader().readByte();
					if (b13 == 0)
					{
						int playerID = msg.reader().readInt();
						GameScr.gI().giaodich(playerID);
					}
					if (b13 == 1)
					{
						int num42 = msg.reader().readInt();
						Char char3 = GameScr.findCharInMap(num42);
						if (char3 == null)
						{
							return;
						}
						GameCanvas.panel.setTypeGiaoDich(char3);
						GameCanvas.panel.show();
						Service.gI().getPlayerMenu(num42);
					}
					if (b13 == 2)
					{
						sbyte b14 = msg.reader().readByte();
						for (int num43 = 0; num43 < GameCanvas.panel.vMyGD.size(); num43++)
						{
							Item item = (Item)GameCanvas.panel.vMyGD.elementAt(num43);
							if (item.indexUI == b14)
							{
								GameCanvas.panel.vMyGD.removeElement(item);
								break;
							}
						}
					}
					if (b13 == 6)
					{
						GameCanvas.panel.isFriendLock = true;
						if (GameCanvas.panel2 != null)
						{
							GameCanvas.panel2.isFriendLock = true;
						}
						GameCanvas.panel.vFriendGD.removeAllElements();
						if (GameCanvas.panel2 != null)
						{
							GameCanvas.panel2.vFriendGD.removeAllElements();
						}
						int friendMoneyGD = msg.reader().readInt();
						sbyte b15 = msg.reader().readByte();
						for (int num44 = 0; num44 < b15; num44++)
						{
							Item item2 = new Item();
							item2.template = ItemTemplates.get(msg.reader().readShort());
							item2.quantity = msg.reader().readInt();
							int num45 = msg.reader().readUnsignedByte();
							if (num45 != 0)
							{
								item2.itemOption = new ItemOption[num45];
								for (int num46 = 0; num46 < item2.itemOption.Length; num46++)
								{
									int num47 = msg.reader().readUnsignedByte();
									int param2 = msg.reader().readUnsignedShort();
									if (num47 != -1)
									{
										item2.itemOption[num46] = new ItemOption(num47, param2);
										item2.compare = GameCanvas.panel.getCompare(item2);
									}
								}
							}
							if (GameCanvas.panel2 != null)
							{
								GameCanvas.panel2.vFriendGD.addElement(item2);
							}
							else
							{
								GameCanvas.panel.vFriendGD.addElement(item2);
							}
						}
						if (GameCanvas.panel2 != null)
						{
							GameCanvas.panel2.setTabGiaoDich(isMe: false);
							GameCanvas.panel2.friendMoneyGD = friendMoneyGD;
						}
						else
						{
							GameCanvas.panel.friendMoneyGD = friendMoneyGD;
							if (GameCanvas.panel.currentTabIndex == 2)
							{
								GameCanvas.panel.setTabGiaoDich(isMe: false);
							}
						}
					}
					if (b13 == 7)
					{
						InfoDlg.hide();
						if (GameCanvas.panel.isShow)
						{
							GameCanvas.panel.hide();
						}
					}
					break;
				}
				case -85:
				{
					sbyte num158 = msg.reader().readByte();
					if (num158 == 0)
					{
						int num159 = msg.reader().readUnsignedShort();
						sbyte[] data5 = new sbyte[num159];
						msg.reader().read(ref data5, 0, num159);
						GameScr.imgCapcha = Image.createImage(data5, 0, num159);
						GameScr.gI().keyInput = "-----";
						GameScr.gI().strCapcha = msg.reader().readUTF();
						GameScr.gI().keyCapcha = new int[GameScr.gI().strCapcha.Length];
						GameScr.gI().mobCapcha = new Mob();
						GameScr.gI().right = null;
					}
					if (num158 == 1)
					{
						MobCapcha.isAttack = true;
					}
					if (num158 == 2)
					{
						MobCapcha.explode = true;
						GameScr.gI().right = GameScr.gI().cmdFocus;
					}
					break;
				}
				case -84:
				{
					int index4 = msg.reader().readUnsignedByte();
					Mob mob8 = null;
					try
					{
						mob8 = (Mob)GameScr.vMob.elementAt(index4);
					}
					catch (Exception)
					{
					}
					if (mob8 != null)
					{
						mob8.maxHp = msg.reader().readInt();
					}
					break;
				}
				case -83:
				{
					sbyte num108 = msg.reader().readByte();
					if (num108 == 0)
					{
						int num109 = msg.reader().readShort();
						int bgRID = msg.reader().readShort();
						int num110 = msg.reader().readUnsignedByte();
						int num111 = msg.reader().readInt();
						msg.reader().readUTF();
						int xR = msg.reader().readShort();
						int yR = msg.reader().readShort();
						if (msg.reader().readByte() == 1)
						{
							GameScr.gI().isRongNamek = true;
						}
						else
						{
							GameScr.gI().isRongNamek = false;
						}
						GameScr.gI().xR = xR;
						GameScr.gI().yR = yR;
						if (Char.myCharz().charID == num111)
						{
							GameCanvas.panel.hideNow();
							GameScr.gI().activeRongThanEff(isMe: true);
						}
						else if (TileMap.mapID == num109 && TileMap.zoneID == num110)
						{
							GameScr.gI().activeRongThanEff(isMe: false);
						}
						else if (mGraphics.zoomLevel > 1)
						{
							GameScr.gI().doiMauTroi();
						}
						GameScr.gI().mapRID = num109;
						GameScr.gI().bgRID = bgRID;
						GameScr.gI().zoneRID = num110;
					}
					if (num108 == 1)
					{
						if (TileMap.mapID == GameScr.gI().mapRID && TileMap.zoneID == GameScr.gI().zoneRID)
						{
							GameScr.gI().hideRongThanEff();
						}
						else
						{
							GameScr.gI().isRongThanXuatHien = false;
							if (GameScr.gI().isRongNamek)
							{
								GameScr.gI().isRongNamek = false;
							}
						}
					}
					if (num108 == 2)
					{
					}
					break;
				}
				case -82:
				{
					sbyte b39 = msg.reader().readByte();
					TileMap.tileIndex = new int[b39][][];
					TileMap.tileType = new int[b39][];
					for (int num147 = 0; num147 < b39; num147++)
					{
						sbyte b40 = msg.reader().readByte();
						TileMap.tileType[num147] = new int[b40];
						TileMap.tileIndex[num147] = new int[b40][];
						for (int num148 = 0; num148 < b40; num148++)
						{
							TileMap.tileType[num147][num148] = msg.reader().readInt();
							sbyte b41 = msg.reader().readByte();
							TileMap.tileIndex[num147][num148] = new int[b41];
							for (int num149 = 0; num149 < b41; num149++)
							{
								TileMap.tileIndex[num147][num148][num149] = msg.reader().readByte();
							}
						}
					}
					break;
				}
				case -81:
				{
					sbyte b32 = msg.reader().readByte();
					if (b32 == 0)
					{
						string src = msg.reader().readUTF();
						string src2 = msg.reader().readUTF();
						GameCanvas.panel.setTypeCombine();
						GameCanvas.panel.combineInfo = mFont.tahoma_7b_blue.splitFontArray(src, Panel.WIDTH_PANEL);
						GameCanvas.panel.combineTopInfo = mFont.tahoma_7.splitFontArray(src2, Panel.WIDTH_PANEL);
						GameCanvas.panel.show();
					}
					if (b32 == 1)
					{
						GameCanvas.panel.vItemCombine.removeAllElements();
						sbyte b33 = msg.reader().readByte();
						for (int num116 = 0; num116 < b33; num116++)
						{
							sbyte b34 = msg.reader().readByte();
							for (int num117 = 0; num117 < Char.myCharz().arrItemBag.Length; num117++)
							{
								Item item3 = Char.myCharz().arrItemBag[num117];
								if (item3 != null && item3.indexUI == b34)
								{
									item3.isSelect = true;
									GameCanvas.panel.vItemCombine.addElement(item3);
								}
							}
						}
						if (GameCanvas.panel.isShow)
						{
							GameCanvas.panel.setTabCombine();
						}
					}
					if (b32 == 2)
					{
						GameCanvas.panel.combineSuccess = 0;
						GameCanvas.panel.setCombineEff(0);
					}
					if (b32 == 3)
					{
						GameCanvas.panel.combineSuccess = 1;
						GameCanvas.panel.setCombineEff(0);
					}
					if (b32 == 4)
					{
						short iconID = msg.reader().readShort();
						GameCanvas.panel.iconID3 = iconID;
						GameCanvas.panel.combineSuccess = 0;
						GameCanvas.panel.setCombineEff(1);
					}
					if (b32 == 5)
					{
						short iconID2 = msg.reader().readShort();
						GameCanvas.panel.iconID3 = iconID2;
						GameCanvas.panel.combineSuccess = 0;
						GameCanvas.panel.setCombineEff(2);
					}
					if (b32 == 6)
					{
						short iconID3 = msg.reader().readShort();
						short iconID4 = msg.reader().readShort();
						GameCanvas.panel.combineSuccess = 0;
						GameCanvas.panel.setCombineEff(3);
						GameCanvas.panel.iconID1 = iconID3;
						GameCanvas.panel.iconID3 = iconID4;
					}
					if (b32 == 7)
					{
						short iconID5 = msg.reader().readShort();
						GameCanvas.panel.iconID3 = iconID5;
						GameCanvas.panel.combineSuccess = 0;
						GameCanvas.panel.setCombineEff(4);
					}
					if (b32 == 8)
					{
						GameCanvas.panel.iconID3 = -1;
						GameCanvas.panel.combineSuccess = 1;
						GameCanvas.panel.setCombineEff(4);
					}
					short num118 = 21;
					try
					{
						num118 = msg.reader().readShort();
						int num119 = msg.reader().readShort();
						int num120 = msg.reader().readShort();
						GameCanvas.panel.xS = num119 - GameScr.cmx;
						GameCanvas.panel.yS = num120 - GameScr.cmy;
					}
					catch (Exception)
					{
					}
					for (int num121 = 0; num121 < GameScr.vNpc.size(); num121++)
					{
						Npc npc = (Npc)GameScr.vNpc.elementAt(num121);
						if (npc.template.npcTemplateId == num118)
						{
							GameCanvas.panel.xS = npc.cx - GameScr.cmx;
							GameCanvas.panel.yS = npc.cy - GameScr.cmy;
							GameCanvas.panel.idNPC = num118;
							break;
						}
					}
					break;
				}
				case -80:
				{
					sbyte b46 = msg.reader().readByte();
					InfoDlg.hide();
					if (b46 == 0)
					{
						GameCanvas.panel.vFriend.removeAllElements();
						int num170 = msg.reader().readUnsignedByte();
						for (int num171 = 0; num171 < num170; num171++)
						{
							Char char9 = new Char();
							char9.charID = msg.reader().readInt();
							char9.head = msg.reader().readShort();
							char9.headICON = msg.reader().readShort();
							char9.body = msg.reader().readShort();
							char9.leg = msg.reader().readShort();
							char9.bag = msg.reader().readUnsignedByte();
							char9.cName = msg.reader().readUTF();
							bool isOnline2 = msg.reader().readBoolean();
							InfoItem infoItem2 = new InfoItem(mResources.power + ": " + msg.reader().readUTF());
							infoItem2.charInfo = char9;
							infoItem2.isOnline = isOnline2;
							GameCanvas.panel.vFriend.addElement(infoItem2);
						}
						GameCanvas.panel.setTypeFriend();
						GameCanvas.panel.show();
					}
					if (b46 == 3)
					{
						MyVector vFriend = GameCanvas.panel.vFriend;
						int num172 = msg.reader().readInt();
						for (int num173 = 0; num173 < vFriend.size(); num173++)
						{
							InfoItem infoItem3 = (InfoItem)vFriend.elementAt(num173);
							if (infoItem3.charInfo != null && infoItem3.charInfo.charID == num172)
							{
								infoItem3.isOnline = msg.reader().readBoolean();
								break;
							}
						}
					}
					if (b46 != 2)
					{
						break;
					}
					MyVector vFriend2 = GameCanvas.panel.vFriend;
					int num174 = msg.reader().readInt();
					for (int num175 = 0; num175 < vFriend2.size(); num175++)
					{
						InfoItem infoItem4 = (InfoItem)vFriend2.elementAt(num175);
						if (infoItem4.charInfo != null && infoItem4.charInfo.charID == num174)
						{
							vFriend2.removeElement(infoItem4);
							break;
						}
					}
					if (GameCanvas.panel.isShow)
					{
						GameCanvas.panel.setTabFriend();
					}
					break;
				}
				case -79:
				{
					InfoDlg.hide();
					msg.reader().readInt();
					Char charMenu = GameCanvas.panel.charMenu;
					if (charMenu == null)
					{
						return;
					}
					charMenu.cPower = msg.reader().readLong();
					charMenu.currStrLevel = msg.reader().readUTF();
					break;
				}
				case -77:
				{
					short num19 = msg.reader().readShort();
					SmallImage.newSmallVersion = new sbyte[num19];
					SmallImage.maxSmall = num19;
					SmallImage.imgNew = new Small[num19];
					for (int num20 = 0; num20 < num19; num20++)
					{
						SmallImage.newSmallVersion[num20] = msg.reader().readByte();
					}
					break;
				}
				case -76:
					switch (msg.reader().readByte())
					{
					case 0:
					{
						sbyte b6 = msg.reader().readByte();
						if (b6 <= 0)
						{
							return;
						}
						Char.myCharz().arrArchive = new Archivement[b6];
						for (int num22 = 0; num22 < b6; num22++)
						{
							Char.myCharz().arrArchive[num22] = new Archivement
							{
								info1 = num22 + 1 + ". " + msg.reader().readUTF(),
								info2 = msg.reader().readUTF(),
								money = msg.reader().readUnsignedShort(),
								isFinish = msg.reader().readBoolean(),
								isRecieve = msg.reader().readBoolean()
							};
						}
						GameCanvas.panel.setTypeArchivement();
						GameCanvas.panel.show();
						break;
					}
					case 1:
					{
						int num21 = msg.reader().readUnsignedByte();
						Archivement[] arrArchive = Char.myCharz().arrArchive;
						if (arrArchive != null && num21 < arrArchive.Length && arrArchive[num21] != null)
						{
							arrArchive[num21].isRecieve = true;
						}
						break;
					}
					}
					break;
				case -74:
				{
					if (ServerListScreen.stopDownload)
					{
						return;
					}
					if (!GameCanvas.isGetResourceFromServer())
					{
						Service.gI().getResource(3, null);
						SmallImage.loadBigRMS();
						if (Rms.loadRMSString("acc") != null || Rms.loadRMSString("userAo" + ServerListScreen.ipSelect) != null)
						{
							LoginScr.isContinueToLogin = true;
						}
						GameCanvas.loginScr = new LoginScr();
						GameCanvas.loginScr.switchToMe();
						return;
					}
					sbyte b42 = msg.reader().readByte();
					if (b42 == 0)
					{
						int num150 = msg.reader().readInt();
						string text5 = Rms.loadRMSString("ResVersion");
						int num151 = ((text5 == null || !(text5 != string.Empty)) ? (-1) : int.Parse(text5));
						if (Session_ME.gI().isCompareIPConnect())
						{
							if (num151 == -1 || num151 != num150)
							{
								GameCanvas.serverScreen.show2();
							}
							else
							{
								SmallImage.loadBigRMS();
								ServerListScreen.loadScreen = true;
								if (GameCanvas.currentScreen != GameCanvas.loginScr)
								{
									GameCanvas.serverScreen.switchToMe();
								}
							}
						}
						else
						{
							Session_ME.gI().close();
							ServerListScreen.loadScreen = true;
							ServerListScreen.isAutoConect = false;
							ServerListScreen.countDieConnect = 1000;
							GameCanvas.serverScreen.switchToMe();
						}
					}
					if (b42 == 1)
					{
						ServerListScreen.strWait = mResources.downloading_data;
						ServerListScreen.nBig = msg.reader().readShort();
						Service.gI().getResource(2, null);
					}
					if (b42 == 2)
					{
						try
						{
							isLoadingData = true;
							GameCanvas.endDlg();
							ServerListScreen.demPercent++;
							ServerListScreen.percent = ServerListScreen.demPercent * 100 / ServerListScreen.nBig;
							string[] array14 = Res.split(msg.reader().readUTF(), "/", 0);
							string rawName = array14[array14.Length - 1];
							string cleanName = rawName.EndsWith(".png") ? rawName.Substring(0, rawName.Length - 4) : rawName;
							int num152 = msg.reader().readInt();
							sbyte[] data3 = new sbyte[num152];
							msg.reader().read(ref data3, 0, num152);
							Rms.saveRMS("x" + mGraphics.zoomLevel + cleanName, data3);
							Rms.saveRMS("x" + mGraphics.zoomLevel + cleanName + ".png", data3);
						}
						catch (Exception)
						{
							GameCanvas.startOK(mResources.pls_restart_game_error, 8885, null);
						}
					}
					if (b42 == 3)
					{
						Rms.saveRMSInt("musicSize", ModFunc.musicCount);
						ModFunc.InitMusic();
						isLoadingData = false;
						Rms.saveRMSString("ResVersion", msg.reader().readInt() + string.Empty);
						Service.gI().getResource(3, null);
						GameCanvas.endDlg();
						SmallImage.loadBigRMS();
						mSystem.gcc();
						ServerListScreen.bigOk = true;
						ServerListScreen.loadScreen = true;
						GameScr.gI().loadGameScr();
						if (GameCanvas.currentScreen != GameCanvas.loginScr)
						{
							GameCanvas.serverScreen.switchToMe();
						}
					}
					if (b42 == 4)
					{
						string text6 = msg.reader().readUTF();
						sbyte[] array15 = null;
						try
						{
							array15 = NinjaUtil.readByteArray(msg);
						}
						catch (Exception)
						{
							array15 = null;
						}
						if (array15 != null)
						{
							ModFunc.musicCount++;
							Rms.saveRMS("music_" + text6, array15);
						}
					}
					break;
				}
				case -70:
				{
					GameCanvas.endDlg();
					if (PickMob.tanSat)
					{
						ModFunc.GI().perform(44, true);
						return;
					}
					int avatar = msg.reader().readShort();
					string chat = msg.reader().readUTF();
					Npc c = new Npc(-1, 0, 0, 0, 0, 0)
					{
						avatar = avatar
					};
					ChatPopup.addBigMessage(chat, 100000, c);
					sbyte num79 = msg.reader().readByte();
					if (num79 == 0)
					{
						ChatPopup.serverChatPopUp.cmdMsg1 = new Command(mResources.CLOSE, ChatPopup.serverChatPopUp, 1001, null)
						{
							x = GameCanvas.w / 2 - 35,
							y = GameCanvas.h - 35
						};
					}
					if (num79 == 1)
					{
						string p = msg.reader().readUTF();
						string caption3 = msg.reader().readUTF();
						ChatPopup.serverChatPopUp.cmdMsg1 = new Command(mResources.CLOSE, ChatPopup.serverChatPopUp, 1001, null)
						{
							x = GameCanvas.w / 2 + 11,
							y = GameCanvas.h - 35
						};
						ChatPopup.serverChatPopUp.cmdMsg2 = new Command(caption3, ChatPopup.serverChatPopUp, 1000, p)
						{
							x = GameCanvas.w / 2 - 75,
							y = GameCanvas.h - 35
						};
					}
					break;
				}
				case -69:
					Char.myCharz().cMaxStamina = msg.reader().readShort();
					break;
				case -68:
					Char.myCharz().cStamina = msg.reader().readShort();
					break;
				case -67:
				{
					int num59 = msg.reader().readInt();
					try
					{
						sbyte[] arr = NinjaUtil.readByteArray(msg);
						Image image = createImage(arr);
						if (num59 >= 0 && num59 < SmallImage.imgNew.Length)
						{
							if (SmallImage.imgNew[num59] == null)
							{
								SmallImage.imgNew[num59] = new Small();
							}
							SmallImage.imgNew[num59].img = image;
						}
						if (mGraphics.zoomLevel > 1 && image != null)
						{
							SmallImage.imageRaw[num59] = image;
						}
					}
					catch (Exception)
					{
						if (num59 >= 0 && num59 < SmallImage.imgNew.Length)
						{
							if (SmallImage.imgNew[num59] == null)
							{
								SmallImage.imgNew[num59] = new Small();
							}
							SmallImage.imgNew[num59].img = Image.createRGBImage(new int[1], 1, 1, bl: true);
						}
					}
					break;
				}
				case -65:
				{
					InfoDlg.hide();
					int num157 = msg.reader().readInt();
					sbyte b44 = msg.reader().readByte();
					if (b44 == 0)
					{
						break;
					}
					if (Char.myCharz().charID == num157)
					{
						isStopReadMessage = true;
						GameScr.lockTick = 500;
						GameScr.gI().center = null;
						if (b44 == 0 || b44 == 1 || b44 == 3)
						{
							Teleport.addTeleport(new Teleport(Char.myCharz().cx, Char.myCharz().cy, Char.myCharz().head, Char.myCharz().cdir, 0, isMe: true, (b44 != 1) ? b44 : Char.myCharz().cgender));
						}
						if (b44 == 2)
						{
							GameScr.lockTick = 50;
							Char.myCharz().hide();
						}
					}
					else
					{
						Char char8 = GameScr.findCharInMap(num157);
						if ((b44 == 0 || b44 == 1 || b44 == 3) && char8 != null)
						{
							char8.isUsePlane = true;
							Teleport.addTeleport(new Teleport(char8.cx, char8.cy, char8.head, char8.cdir, 0, isMe: false, (b44 != 1) ? b44 : char8.cgender)
							{
								id = num157
							});
						}
						if (b44 == 2)
						{
							char8.hide();
						}
					}
					break;
				}
				case -64:
				{
					int num160 = msg.reader().readInt();
					int bag = msg.reader().readUnsignedByte();
					@char = null;
					@char = ((num160 != Char.myCharz().charID) ? GameScr.findCharInMap(num160) : Char.myCharz());
					if (@char == null)
					{
						return;
					}
					@char.bag = bag;
					for (int num161 = 0; num161 < 54; num161++)
					{
						@char.removeEffChar(0, 201 + num161);
					}
					if (@char.bag >= 201 && @char.bag < 255)
					{
						@char.addEffChar(new Effect(@char.bag, @char, 2, -1, 10, 1)
						{
							typeEff = 5
						});
					}
					break;
				}
				case -63:
				{
					byte b16 = msg.reader().readUnsignedByte();
					sbyte b17 = msg.reader().readByte();
					int[] array5 = new int[b17];
					if (b17 > 0)
					{
						for (int num49 = 0; num49 < b17; num49++)
						{
							array5[num49] = msg.reader().readShort();
						}
					}
					short iD2;
					try
					{
						iD2 = msg.reader().readShort();
					}
					catch
					{
						iD2 = b16;
					}
					if (b17 > 0)
					{
						for (int num50 = 0; num50 < b17; num50++)
						{
							try
							{
								array5[num50] = msg.reader().readInt();
							}
							catch
							{
							}
						}
					}
					ClanImage v = new ClanImage
					{
						ID = iD2,
						idImage = array5
					};
					if (b17 > 0)
					{
						ClanImage.idImages.put(b16 + string.Empty, v);
					}
					break;
				}
				case -62:
				{
					byte b7 = msg.reader().readUnsignedByte();
					sbyte b8 = msg.reader().readByte();
					int[] array3 = new int[b8];
					if (b8 > 0)
					{
						for (int num24 = 0; num24 < b8; num24++)
						{
							array3[num24] = msg.reader().readShort();
							if (array3[num24] > 0)
							{
								SmallImage.vKeys.addElement(array3[num24] + string.Empty);
							}
						}
					}
					short iD;
					try
					{
						iD = msg.reader().readShort();
					}
					catch
					{
						iD = b7;
					}
					if (b8 > 0)
					{
						for (int num25 = 0; num25 < b8; num25++)
						{
							try
							{
								array3[num25] = msg.reader().readInt();
							}
							catch
							{
							}
							if (array3[num25] > 0)
							{
								SmallImage.vKeys.addElement(array3[num25] + string.Empty);
							}
						}
					}
					ClanImage clanImage = ClanImage.getClanImage(iD);
					if (clanImage != null)
					{
						clanImage.idImage = array3;
					}
					break;
				}
				case -61:
				{
					int num178 = msg.reader().readInt();
					if (num178 != Char.myCharz().charID)
					{
						if (GameScr.findCharInMap(num178) != null)
						{
							GameScr.findCharInMap(num178).clanID = msg.reader().readInt();
							if (GameScr.findCharInMap(num178).clanID == -2)
							{
								GameScr.findCharInMap(num178).isCopy = true;
							}
						}
					}
					else if (Char.myCharz().clan != null)
					{
						Char.myCharz().clan.ID = msg.reader().readInt();
					}
					break;
				}
				case -60:
				{
					GameCanvas.debug("SA7666", 2);
					int num80 = msg.reader().readInt();
					int num81 = -1;
					if (num80 != Char.myCharz().charID)
					{
						Char char5 = GameScr.findCharInMap(num80);
						if (char5 == null)
						{
							return;
						}
						if (char5.currentMovePoint != null)
						{
							char5.createShadow(char5.cx, char5.cy, 10);
							char5.cx = char5.currentMovePoint.xEnd;
							char5.cy = char5.currentMovePoint.yEnd;
						}
						int num82 = msg.reader().readUnsignedByte();
						if (!KienzanEffect.isSkillId(num82) && !HellzoneGrenadeEffect.isSkillId(num82)
							&& !BarrierPrisonEffect.isSkillId(num82) && !SuperGhostKamikazeEffect.isSkillId(num82))
						{
							if ((TileMap.tileTypeAtPixel(char5.cx, char5.cy) & 2) == 2)
							{
								char5.setSkillPaint(GameScr.sks[num82], 0);
							}
							else
							{
								char5.setSkillPaint(GameScr.sks[num82], 1);
							}
						}
						Char[] array7 = new Char[msg.reader().readByte()];
						for (num = 0; num < array7.Length; num++)
						{
							num81 = msg.reader().readInt();
							Char char6;
							if (num81 == Char.myCharz().charID)
							{
								char6 = Char.myCharz();
								if (!GameScr.isChangeZone && GameScr.isAutoPlay && GameScr.canAutoPlay)
								{
									Service.gI().requestChangeZone(-1, -1);
									GameScr.isChangeZone = true;
								}
							}
							else
							{
								char6 = GameScr.findCharInMap(num81);
							}
							array7[num] = char6;
							if (num == 0)
							{
								if (char5.cx <= char6.cx)
								{
									char5.cdir = 1;
								}
								else
								{
									char5.cdir = -1;
								}
							}
						}
						if (num > 0)
						{
							char5.attChars = new Char[num];
							for (num = 0; num < char5.attChars.Length; num++)
							{
								char5.attChars[num] = array7[num];
							}
							char5.mobFocus = null;
							char5.charFocus = char5.attChars[0];
						}
					}
					else
					{
						msg.reader().readByte();
						msg.reader().readByte();
						num81 = msg.reader().readInt();
					}
					try
					{
						if (msg.reader().readByte() != 1)
						{
							break;
						}
						sbyte b27 = msg.reader().readByte();
						if (num81 == Char.myCharz().charID)
						{
							@char = Char.myCharz();
							long num83 = msg.readLong();
							@char.isDie = msg.reader().readBoolean();
							if (@char.isDie)
							{
								Char.isLockKey = true;
							}
							long num84 = 0L;
							bool isCrit = (@char.isCrit = msg.reader().readBoolean());
							@char.isMob = false;
							num83 = (@char.damHP = num83 + num84);
							if (b27 == 0)
							{
								@char.doInjure(num83, 0L, isCrit, isMob: false);
							}
						}
						else
						{
							@char = GameScr.findCharInMap(num81);
							if (@char == null)
							{
								return;
							}
							long num85 = msg.readLong();
							@char.isDie = msg.reader().readBoolean();
							long num86 = 0L;
							bool isCrit2 = (@char.isCrit = msg.reader().readBoolean());
							@char.isMob = false;
							num85 = (@char.damHP = num85 + num86);
							if (b27 == 0)
							{
								@char.doInjure(num85, 0L, isCrit2, isMob: false);
							}
						}
					}
					catch (Exception)
					{
					}
					break;
				}
				case -59:
				{
					sbyte typePK = msg.reader().readByte();
					GameScr.gI().player_vs_player(msg.reader().readInt(), msg.reader().readInt(), msg.reader().readUTF(), typePK);
					break;
				}
				case -57:
				{
					string strInvite = msg.reader().readUTF();
					int clanID = msg.reader().readInt();
					int code = msg.reader().readInt();
					GameScr.gI().clanInvite(strInvite, clanID, code);
					break;
				}
				case -53:
				{
					InfoDlg.hide();
					bool flag = false;
					int num6 = msg.reader().readInt();
					if (num6 == -1)
					{
						Char.myCharz().clan = null;
						ClanMessage.vMessage.removeAllElements();
						if (GameCanvas.panel.member != null)
						{
							GameCanvas.panel.member.removeAllElements();
						}
						if (GameCanvas.panel.myMember != null)
						{
							GameCanvas.panel.myMember.removeAllElements();
						}
						if (GameCanvas.currentScreen == GameScr.gI())
						{
							GameCanvas.panel.setTabClans();
						}
						return;
					}
					GameCanvas.panel.tabIcon = null;
					if (Char.myCharz().clan == null)
					{
						Char.myCharz().clan = new Clan();
					}
					Char.myCharz().clan.profileVersion = 0;
					Char.myCharz().clan.ID = num6;
					Char.myCharz().clan.name = msg.reader().readUTF();
					Char.myCharz().clan.slogan = msg.reader().readUTF();
					Char.myCharz().clan.imgID = msg.reader().readUnsignedByte();
					Char.myCharz().clan.powerPoint = msg.reader().readUTF();
					Char.myCharz().clan.leaderName = msg.reader().readUTF();
					Char.myCharz().clan.currMember = msg.reader().readUnsignedByte();
					Char.myCharz().clan.maxMember = msg.reader().readUnsignedByte();
					Char.myCharz().role = msg.reader().readByte();
					Char.myCharz().clan.clanPoint = msg.reader().readInt();
					Char.myCharz().clan.level = msg.reader().readByte();
					GameCanvas.panel.myMember = new MyVector();
					for (int m = 0; m < Char.myCharz().clan.currMember; m++)
					{
						Member member = new Member();
						member.ID = msg.reader().readInt();
						member.head = msg.reader().readShort();
						member.headICON = msg.reader().readShort();
						member.leg = msg.reader().readShort();
						member.body = msg.reader().readShort();
						member.name = msg.reader().readUTF();
						member.role = msg.reader().readByte();
						member.powerPoint = msg.reader().readUTF();
						member.donate = msg.reader().readInt();
						member.receive_donate = msg.reader().readInt();
						member.clanPoint = msg.reader().readInt();
						member.curClanPoint = msg.reader().readInt();
						member.joinTime = NinjaUtil.getDate(msg.reader().readInt());
						GameCanvas.panel.myMember.addElement(member);
					}
					int num7 = msg.reader().readUnsignedByte();
					for (int n = 0; n < num7; n++)
					{
						readClanMsg(msg, -1);
					}
					// CLV2 is an optional tail after the legacy -53 clan payload.
					try
					{
						int profileMagic = msg.reader().readInt();
						if (profileMagic == Clan.PROFILE_V2_MAGIC)
						{
							int profileVersion = msg.reader().readUnsignedByte();
							int profileClanId = msg.reader().readInt();
							int profileLevel = msg.reader().readInt();
							int profileMaxMember = msg.reader().readInt();
							int profileCurrentMember = msg.reader().readInt();
							long profileClanGold = 0L;
							long profileClanGem = 0L;
							long profileTreasuryVersion = 0L;
							long profileMemberContribution = 0L;
							MyVector profileLedger = new MyVector();
							long profileClanExp = 0L;
							long profileExpRequired = 0L;
							int profileCapsuleRequired = 0;
							long profileGoldRequired = 0L;
							long profileGemRequired = 0L;
							int profilePotentialTotal = 0;
							int profilePotentialUnspent = 0;
							long profileProgressionVersion = 0L;
							int[] profilePotentialRanks = new int[ClanProgression.BRANCH_COUNT];
							int[] profileBuffPercents = new int[ClanProgression.BUFF_COUNT];
							long[] profileBuffRemainingMillis = new long[ClanProgression.BUFF_COUNT];
							if (profileVersion >= 2)
							{
								profileClanGold = msg.reader().readLong();
								profileClanGem = msg.reader().readLong();
								profileTreasuryVersion = msg.reader().readLong();
								profileMemberContribution = msg.reader().readLong();
							}
							if (profileVersion >= Clan.PROFILE_LEDGER_VERSION)
							{
								int profileLedgerCount = msg.reader().readUnsignedByte();
								for (int profileLedgerIndex = 0; profileLedgerIndex < profileLedgerCount; profileLedgerIndex++)
								{
									ClanLedgerEntry profileEntry = new ClanLedgerEntry();
									profileEntry.id = msg.reader().readLong();
									profileEntry.actorId = msg.reader().readLong();
									profileEntry.actorName = msg.reader().readUTF();
									profileEntry.actionType = msg.reader().readUTF();
									profileEntry.currencyType = msg.reader().readByte();
									profileEntry.amount = msg.reader().readLong();
									profileEntry.balanceAfter = msg.reader().readLong();
									profileEntry.createdAt = msg.reader().readLong();
									profileLedger.addElement(profileEntry);
								}
							}
							if (profileVersion >= Clan.PROFILE_PROGRESSION_VERSION)
							{
								profileClanExp = msg.reader().readLong();
								profileExpRequired = msg.reader().readLong();
								profileCapsuleRequired = msg.reader().readInt();
								profileGoldRequired = msg.reader().readLong();
								profileGemRequired = msg.reader().readLong();
								profilePotentialTotal = msg.reader().readInt();
								profilePotentialUnspent = msg.reader().readInt();
								profileProgressionVersion = msg.reader().readLong();
								for (int profileRankIndex = 0; profileRankIndex < ClanProgression.BRANCH_COUNT; profileRankIndex++)
								{
									profilePotentialRanks[profileRankIndex] = msg.reader().readUnsignedByte();
								}
							}
							if (profileVersion >= Clan.PROFILE_BUFF_VERSION)
							{
								int profileBuffCount = msg.reader().readUnsignedByte();
								for (int profileBuffIndex = 0; profileBuffIndex < profileBuffCount; profileBuffIndex++)
								{
									int profileBuffType = msg.reader().readUnsignedByte();
									int profileBuffPercent = msg.reader().readShort();
									long profileBuffRemaining = msg.reader().readLong();
									if (profileBuffType >= 0 && profileBuffType < ClanProgression.BUFF_COUNT)
									{
										profileBuffPercents[profileBuffType] = profileBuffPercent;
										profileBuffRemainingMillis[profileBuffType] = profileBuffRemaining;
									}
								}
							}
							if (profileVersion >= Clan.PROFILE_V2_VERSION && profileClanId == Char.myCharz().clan.ID && profileLevel >= 1 && profileMaxMember >= profileCurrentMember)
							{
								Char.myCharz().clan.profileVersion = profileVersion;
								Char.myCharz().clan.level = profileLevel;
								Char.myCharz().clan.maxMember = profileMaxMember;
								Char.myCharz().clan.currMember = profileCurrentMember;
								if (profileVersion >= Clan.PROFILE_TREASURY_VERSION)
								{
									ClanTreasury.applyProfileSnapshot(profileClanId, Char.myCharz().clan.clanPoint,
										profileClanGold, profileClanGem, profileMemberContribution, profileTreasuryVersion);
								}
								if (profileVersion >= Clan.PROFILE_LEDGER_VERSION)
								{
									ClanTreasury.applyProfileLedger(profileClanId, profileLedger);
								}
								if (profileVersion >= Clan.PROFILE_PROGRESSION_VERSION)
								{
									ClanProgression.applySnapshot(profileClanId, profileLevel,
										profileClanExp, profileExpRequired, profileCapsuleRequired,
										profileGoldRequired, profileGemRequired, profilePotentialTotal,
										profilePotentialUnspent, profileProgressionVersion, profilePotentialRanks);
								}
								if (profileVersion >= Clan.PROFILE_BUFF_VERSION)
								{
									ClanProgression.applyProfileBuffSnapshot(profileClanId,
										profileBuffPercents, profileBuffRemainingMillis);
								}
							}
						}
					}
					catch (Exception)
					{
					}
					if (GameCanvas.panel.isSearchClan || GameCanvas.panel.isViewMember || GameCanvas.panel.isMessage)
					{
						GameCanvas.panel.setTabClans();
					}
					if (flag)
					{
						GameCanvas.panel.setTabClans();
					}
					break;
				}
				case -52:
				{
					sbyte num155 = msg.reader().readByte();
					if (num155 == 0)
					{
						Member o4 = new Member
						{
							ID = msg.reader().readInt(),
							head = msg.reader().readShort(),
							headICON = msg.reader().readShort(),
							leg = msg.reader().readShort(),
							body = msg.reader().readShort(),
							name = msg.reader().readUTF(),
							role = msg.reader().readByte(),
							powerPoint = msg.reader().readUTF(),
							donate = msg.reader().readInt(),
							receive_donate = msg.reader().readInt(),
							clanPoint = msg.reader().readInt(),
							joinTime = NinjaUtil.getDate(msg.reader().readInt())
						};
						if (GameCanvas.panel.myMember == null)
						{
							GameCanvas.panel.myMember = new MyVector();
						}
						GameCanvas.panel.myMember.addElement(o4);
						GameCanvas.panel.initTabClans();
					}
					if (num155 == 1)
					{
						GameCanvas.panel.myMember.removeElementAt(msg.reader().readByte());
						GameCanvas.panel.currentListLength--;
						GameCanvas.panel.initTabClans();
					}
					if (num155 != 2)
					{
						break;
					}
					Member member3 = new Member();
					member3.ID = msg.reader().readInt();
					member3.head = msg.reader().readShort();
					member3.headICON = msg.reader().readShort();
					member3.leg = msg.reader().readShort();
					member3.body = msg.reader().readShort();
					member3.name = msg.reader().readUTF();
					member3.role = msg.reader().readByte();
					member3.powerPoint = msg.reader().readUTF();
					member3.donate = msg.reader().readInt();
					member3.receive_donate = msg.reader().readInt();
					member3.clanPoint = msg.reader().readInt();
					member3.joinTime = NinjaUtil.getDate(msg.reader().readInt());
					for (int num156 = 0; num156 < GameCanvas.panel.myMember.size(); num156++)
					{
						Member member4 = (Member)GameCanvas.panel.myMember.elementAt(num156);
						if (member4.ID == member3.ID)
						{
							if (Char.myCharz().charID == member3.ID)
							{
								Char.myCharz().role = member3.role;
							}
							Member o5 = member3;
							GameCanvas.panel.myMember.removeElement(member4);
							GameCanvas.panel.myMember.insertElementAt(o5, num156);
							return;
						}
					}
					break;
				}
				case -51:
					InfoDlg.hide();
					readClanMsg(msg, 0);
					if (GameCanvas.panel.isMessage && GameCanvas.panel.type == 5)
					{
						GameCanvas.panel.initTabClans();
					}
					break;
				case -50:
				{
					InfoDlg.hide();
					GameCanvas.panel.member = new MyVector();
					sbyte b38 = msg.reader().readByte();
					for (int num146 = 0; num146 < b38; num146++)
					{
						Member member2 = new Member();
						member2.ID = msg.reader().readInt();
						member2.head = msg.reader().readShort();
						member2.headICON = msg.reader().readShort();
						member2.leg = msg.reader().readShort();
						member2.body = msg.reader().readShort();
						member2.name = msg.reader().readUTF();
						member2.role = msg.reader().readByte();
						member2.powerPoint = msg.reader().readUTF();
						member2.donate = msg.reader().readInt();
						member2.receive_donate = msg.reader().readInt();
						member2.clanPoint = msg.reader().readInt();
						member2.joinTime = NinjaUtil.getDate(msg.reader().readInt());
						GameCanvas.panel.member.addElement(member2);
					}
					GameCanvas.panel.isViewMember = true;
					GameCanvas.panel.isSearchClan = false;
					GameCanvas.panel.isMessage = false;
					GameCanvas.panel.currentListLength = GameCanvas.panel.member.size() + 2;
					GameCanvas.panel.initTabClans();
					break;
				}
				case -47:
				{
					InfoDlg.hide();
					sbyte b30 = msg.reader().readByte();
					if (b30 == 0)
					{
						GameCanvas.panel.clanReport = mResources.cannot_find_clan;
						GameCanvas.panel.clans = null;
					}
					else
					{
						GameCanvas.panel.clans = new Clan[b30];
						for (int num112 = 0; num112 < GameCanvas.panel.clans.Length; num112++)
						{
							GameCanvas.panel.clans[num112] = new Clan();
							GameCanvas.panel.clans[num112].ID = msg.reader().readInt();
							GameCanvas.panel.clans[num112].name = msg.reader().readUTF();
							GameCanvas.panel.clans[num112].slogan = msg.reader().readUTF();
							GameCanvas.panel.clans[num112].imgID = msg.reader().readUnsignedByte();
							GameCanvas.panel.clans[num112].powerPoint = msg.reader().readUTF();
							GameCanvas.panel.clans[num112].leaderName = msg.reader().readUTF();
							GameCanvas.panel.clans[num112].currMember = msg.reader().readUnsignedByte();
							GameCanvas.panel.clans[num112].maxMember = msg.reader().readUnsignedByte();
							GameCanvas.panel.clans[num112].date = msg.reader().readInt();
						}
					}
					GameCanvas.panel.isSearchClan = true;
					GameCanvas.panel.isViewMember = false;
					GameCanvas.panel.isMessage = false;
					if (GameCanvas.panel.isSearchClan)
					{
						GameCanvas.panel.initTabClans();
					}
					break;
				}
				case -46:
				{
					InfoDlg.hide();
					sbyte b22 = msg.reader().readByte();
					if (b22 == 1 || b22 == 3)
					{
						GameCanvas.endDlg();
						ClanImage.vClanImage.removeAllElements();
						int num63 = msg.reader().readUnsignedByte();
						for (int num64 = 0; num64 < num63; num64++)
						{
							byte iD3 = msg.reader().readUnsignedByte();
							string name2 = msg.reader().readUTF();
							int xu = msg.reader().readInt();
							int luong = msg.reader().readInt();
							ClanImage clanImage2 = new ClanImage
							{
								ID = iD3,
								name = name2,
								xu = xu,
								luong = luong
							};
							if (!ClanImage.isExistClanImage(clanImage2.ID))
							{
								ClanImage.addClanImage(clanImage2);
								continue;
							}
							ClanImage.getClanImage((short)clanImage2.ID).name = clanImage2.name;
							ClanImage.getClanImage((short)clanImage2.ID).xu = clanImage2.xu;
							ClanImage.getClanImage((short)clanImage2.ID).luong = clanImage2.luong;
						}
						if (Char.myCharz().clan != null)
						{
							GameCanvas.panel.changeIcon();
						}
					}
					if (b22 == 4)
					{
						Char.myCharz().clan.imgID = msg.reader().readUnsignedByte();
						Char.myCharz().clan.slogan = msg.reader().readUTF();
					}
					break;
				}
				case -45:
				{
					sbyte b9 = msg.reader().readByte();
					int num36 = msg.reader().readInt();
					short num37 = msg.reader().readShort();
					if (b9 == 22)
					{
						KienzanEffect.add(num36, msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readByte(), msg.reader().readShort());
					}
					if (b9 == 23)
					{
						HellzoneGrenadeEffect.add(num36, msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readByte(), msg.reader().readShort());
					}
					if (b9 == 24)
					{
						short prisonX = msg.reader().readShort();
						short prisonY = msg.reader().readShort();
						short prisonDuration = msg.reader().readShort();
						sbyte prisonTargetType = 0;
						int prisonTargetId = -1;
						try
						{
							prisonTargetType = msg.reader().readByte();
							prisonTargetId = msg.reader().readInt();
						}
						catch (Exception)
						{
						}
						BarrierPrisonEffect.add(num36, prisonX, prisonY, prisonDuration,
							prisonTargetType, prisonTargetId);
					}
					if (b9 == 25)
					{
						SuperGhostKamikazeEffect.receive(num36, msg);
					}
					if (b9 == 20)
					{
						sbyte typeFrame = msg.reader().readByte();
						sbyte dir = msg.reader().readByte();
						short timeGong = msg.reader().readShort();
						bool isFly = msg.reader().readByte() != 0;
						sbyte typePaint = msg.reader().readByte();
						sbyte typeItem = -1;
						try
						{
							typeItem = msg.reader().readByte();
						}
						catch (Exception)
						{
						}
						sbyte level = -1;
						try
						{
							level = msg.reader().readByte();
						}
						catch (Exception)
						{
						}
						@char = ((Char.myCharz().charID != num36) ? GameScr.findCharInMap(num36) : Char.myCharz());
						@char.SetSkillPaint_NEW(num37, isFly, typeFrame, typePaint, dir, timeGong, typeItem, level);
					}
					if (b9 == 21)
					{
						Point targetDame = new Point
						{
							x = msg.reader().readShort(),
							y = msg.reader().readShort()
						};
						short timeDame = msg.reader().readShort();
						short rangeDame = msg.reader().readShort();
						sbyte typePaint2 = 0;
						sbyte typeItem2 = -1;
						Point[] array4 = null;
						@char = ((Char.myCharz().charID != num36) ? GameScr.findCharInMap(num36) : Char.myCharz());
						try
						{
							typePaint2 = msg.reader().readByte();
							array4 = new Point[msg.reader().readByte()];
							for (int num38 = 0; num38 < array4.Length; num38++)
							{
								array4[num38] = new Point
								{
									type = msg.reader().readByte()
								};
								if (array4[num38].type == 0)
								{
									array4[num38].id = msg.reader().readByte();
								}
								else
								{
									array4[num38].id = msg.reader().readInt();
								}
							}
						}
						catch (Exception)
						{
						}
						try
						{
							typeItem2 = msg.reader().readByte();
						}
						catch (Exception)
						{
						}
						sbyte level2 = -1;
						try
						{
							level2 = msg.reader().readByte();
						}
						catch (Exception)
						{
						}
						@char.SetSkillPaint_STT(1, num37, targetDame, timeDame, rangeDame, typePaint2, array4, typeItem2, level2);
					}
					if (b9 == 0)
					{
						Res.outz("id use= " + num36);
						if (Char.myCharz().charID != num36)
						{
							@char = GameScr.findCharInMap(num36);
							if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
							{
								@char.setSkillPaint(GameScr.sks[num37], 0);
							}
							else
							{
								@char.setSkillPaint(GameScr.sks[num37], 1);
								@char.delayFall = 20;
							}
						}
						else
						{
							Char.myCharz().saveLoadPreviousSkill();
							Res.outz("LOAD LAST SKILL");
						}
						sbyte b10 = msg.reader().readByte();
						Res.outz("npc size= " + b10);
						for (int num39 = 0; num39 < b10; num39++)
						{
							sbyte index = msg.reader().readByte();
							sbyte seconds = msg.reader().readByte();
							Res.outz("index= " + index);
							if (num37 >= 42 && num37 <= 48)
							{
								((Mob)GameScr.vMob.elementAt(index)).isFreez = true;
								((Mob)GameScr.vMob.elementAt(index)).seconds = seconds;
								((Mob)GameScr.vMob.elementAt(index)).last = (((Mob)GameScr.vMob.elementAt(index)).cur = mSystem.currentTimeMillis());
							}
						}
						sbyte b11 = msg.reader().readByte();
						for (int num40 = 0; num40 < b11; num40++)
						{
							int num41 = msg.reader().readInt();
							sbyte b12 = msg.reader().readByte();
							Res.outz("player ID= " + num41 + " my ID= " + Char.myCharz().charID);
							if (num37 < 42 || num37 > 48)
							{
								continue;
							}
							if (num41 == Char.myCharz().charID)
							{
								if (!Char.myCharz().isFlyAndCharge && !Char.myCharz().isStandAndCharge)
								{
									GameScr.gI().isFreez = true;
									Char.myCharz().isFreez = true;
									Char.myCharz().freezSeconds = b12;
									Char.myCharz().lastFreez = (Char.myCharz().currFreez = mSystem.currentTimeMillis());
									Char.myCharz().isLockMove = true;
								}
							}
							else
							{
								@char = GameScr.findCharInMap(num41);
								if (@char != null && !@char.isFlyAndCharge && !@char.isStandAndCharge)
								{
									@char.isFreez = true;
									@char.seconds = b12;
									@char.freezSeconds = b12;
									@char.lastFreez = (GameScr.findCharInMap(num41).currFreez = mSystem.currentTimeMillis());
								}
							}
						}
					}
					if (b9 == 1 && num36 != Char.myCharz().charID)
					{
						GameScr.findCharInMap(num36).isCharge = true;
					}
					if (b9 == 3)
					{
						if (num36 == Char.myCharz().charID)
						{
							Char.myCharz().isCharge = false;
							SoundMn.gI().taitaoPause();
							Char.myCharz().saveLoadPreviousSkill();
						}
						else
						{
							GameScr.findCharInMap(num36).isCharge = false;
						}
					}
					if (b9 == 4)
					{
						if (num36 == Char.myCharz().charID)
						{
							Char.myCharz().seconds = msg.reader().readShort() - 1000;
							Char.myCharz().last = mSystem.currentTimeMillis();
							Res.outz("second= " + Char.myCharz().seconds + " last= " + Char.myCharz().last);
						}
						else if (GameScr.findCharInMap(num36) != null)
						{
							switch (GameScr.findCharInMap(num36).cgender)
							{
							case 1:
								GameScr.findCharInMap(num36).useChargeSkill(isGround: true);
								break;
							case 0:
								GameScr.findCharInMap(num36).useChargeSkill(isGround: false);
								break;
							}
							GameScr.findCharInMap(num36).skillTemplateId = num37;
							GameScr.findCharInMap(num36).isUseSkillAfterCharge = true;
							GameScr.findCharInMap(num36).seconds = msg.reader().readShort();
							GameScr.findCharInMap(num36).last = mSystem.currentTimeMillis();
						}
					}
					if (b9 == 5)
					{
						if (num36 == Char.myCharz().charID)
						{
							Char.myCharz().stopUseChargeSkill();
						}
						else if (GameScr.findCharInMap(num36) != null)
						{
							GameScr.findCharInMap(num36).stopUseChargeSkill();
						}
					}
					if (b9 == 6)
					{
						if (num36 == Char.myCharz().charID)
						{
							Char.myCharz().setAutoSkillPaint(GameScr.sks[num37], 0);
						}
						else if (GameScr.findCharInMap(num36) != null)
						{
							GameScr.findCharInMap(num36).setAutoSkillPaint(GameScr.sks[num37], 0);
							SoundMn.gI().gong();
						}
					}
					if (b9 == 7)
					{
						if (num36 == Char.myCharz().charID)
						{
							Char.myCharz().seconds = msg.reader().readShort();
							Res.outz("second = " + Char.myCharz().seconds);
							Char.myCharz().last = mSystem.currentTimeMillis();
						}
						else if (GameScr.findCharInMap(num36) != null)
						{
							GameScr.findCharInMap(num36).useChargeSkill(isGround: true);
							GameScr.findCharInMap(num36).seconds = msg.reader().readShort();
							GameScr.findCharInMap(num36).last = mSystem.currentTimeMillis();
							SoundMn.gI().gong();
						}
					}
					if (b9 == 8 && num36 != Char.myCharz().charID && GameScr.findCharInMap(num36) != null)
					{
						GameScr.findCharInMap(num36).setAutoSkillPaint(GameScr.sks[num37], 0);
					}
					break;
				}
				case -44:
				{
					bool flag5 = false;
					if (GameCanvas.w > 2 * Panel.WIDTH_PANEL)
					{
						flag5 = true;
					}
					sbyte b35 = msg.reader().readByte();
					int num133 = msg.reader().readUnsignedByte();
					Char.myCharz().arrItemShop = new Item[num133][];
					GameCanvas.panel.shopTabName = new string[num133 + ((!flag5) ? 1 : 0)][];
					for (int num134 = 0; num134 < GameCanvas.panel.shopTabName.Length; num134++)
					{
						GameCanvas.panel.shopTabName[num134] = new string[2];
					}
					if (b35 == 2)
					{
						GameCanvas.panel.maxPageShop = new int[num133];
						GameCanvas.panel.currPageShop = new int[num133];
					}
					if (!flag5)
					{
						GameCanvas.panel.shopTabName[num133] = mResources.inventory;
					}
					for (int num135 = 0; num135 < num133; num135++)
					{
						string[] array9 = Res.split(msg.reader().readUTF(), "\n", 0);
						if (b35 == 2)
						{
							GameCanvas.panel.maxPageShop[num135] = msg.reader().readUnsignedByte();
						}
						if (array9.Length == 2)
						{
							GameCanvas.panel.shopTabName[num135] = array9;
						}
						if (array9.Length == 1)
						{
							GameCanvas.panel.shopTabName[num135][0] = array9[0];
							GameCanvas.panel.shopTabName[num135][1] = string.Empty;
						}
						int num136 = msg.reader().readUnsignedByte();
						Char.myCharz().arrItemShop[num135] = new Item[num136];
						Panel.strWantToBuy = mResources.say_wat_do_u_want_to_buy;
						if (b35 == 1)
						{
							Panel.strWantToBuy = mResources.say_wat_do_u_want_to_buy2;
						}
						for (int num137 = 0; num137 < num136; num137++)
						{
							short num138 = msg.reader().readShort();
							if (num138 == -1)
							{
								continue;
							}
							ItemTemplate itemTemplate = ItemTemplates.get(num138);
							if (itemTemplate == null)
							{
								Debug.LogError("Thiếu item template " + num138 + " khi mở shop; yêu cầu đồng bộ lại cache item.");
								GameScr.vcItem = -1;
								GameCanvas.isLoading = false;
								GameCanvas.endDlg();
								Service.gI().updateItem();
								return;
							}
							Char.myCharz().arrItemShop[num135][num137] = new Item();
							Char.myCharz().arrItemShop[num135][num137].template = itemTemplate;
							Res.outz("name " + num135 + " = " + Char.myCharz().arrItemShop[num135][num137].template.name + " id templat= " + Char.myCharz().arrItemShop[num135][num137].template.id);
							switch (b35)
							{
							case 8:
								Char.myCharz().arrItemShop[num135][num137].buyCoin = msg.reader().readInt();
								Char.myCharz().arrItemShop[num135][num137].buyGold = msg.reader().readInt();
								Char.myCharz().arrItemShop[num135][num137].quantity = msg.reader().readInt();
								break;
							case 4:
								Char.myCharz().arrItemShop[num135][num137].reason = msg.reader().readUTF();
								break;
							case 0:
								Char.myCharz().arrItemShop[num135][num137].buyCoin = msg.reader().readInt();
								Char.myCharz().arrItemShop[num135][num137].buyGold = msg.reader().readInt();
								break;
							case 1:
								Char.myCharz().arrItemShop[num135][num137].powerRequire = msg.reader().readLong();
								break;
							case 2:
								Char.myCharz().arrItemShop[num135][num137].itemId = msg.reader().readShort();
								Char.myCharz().arrItemShop[num135][num137].buyCoin = msg.reader().readInt();
								Char.myCharz().arrItemShop[num135][num137].buyGold = msg.reader().readInt();
								Char.myCharz().arrItemShop[num135][num137].buyType = msg.reader().readByte();
								Char.myCharz().arrItemShop[num135][num137].quantity = msg.reader().readInt();
								Char.myCharz().arrItemShop[num135][num137].isMe = msg.reader().readByte();
								break;
							case 3:
								Char.myCharz().arrItemShop[num135][num137].isBuySpec = true;
								Char.myCharz().arrItemShop[num135][num137].iconSpec = msg.reader().readShort();
								Char.myCharz().arrItemShop[num135][num137].buySpec = msg.reader().readInt();
								break;
							}
							int num139 = msg.reader().readUnsignedByte();
							if (num139 != 0)
							{
								Char.myCharz().arrItemShop[num135][num137].itemOption = new ItemOption[num139];
								for (int num140 = 0; num140 < Char.myCharz().arrItemShop[num135][num137].itemOption.Length; num140++)
								{
									int num141 = msg.reader().readUnsignedByte();
									int param6 = msg.reader().readUnsignedShort();
									if (num141 != -1)
									{
										Char.myCharz().arrItemShop[num135][num137].itemOption[num140] = new ItemOption(num141, param6);
										Char.myCharz().arrItemShop[num135][num137].compare = GameCanvas.panel.getCompare(Char.myCharz().arrItemShop[num135][num137]);
									}
								}
							}
							sbyte b36 = msg.reader().readByte();
							Char.myCharz().arrItemShop[num135][num137].newItem = b36 != 0;
							if (msg.reader().readByte() == 1)
							{
								int headTemp = msg.reader().readShort();
								int bodyTemp = msg.reader().readShort();
								int legTemp = msg.reader().readShort();
								int bagTemp = msg.reader().readShort();
								Char.myCharz().arrItemShop[num135][num137].setPartTemp(headTemp, bodyTemp, legTemp, bagTemp);
							}
						}
					}
					if (flag5)
					{
						if (b35 != 2)
						{
							GameCanvas.panel2 = new Panel();
							GameCanvas.panel2.tabName[7] = new string[1][] { new string[1] { string.Empty } };
							GameCanvas.panel2.setTypeBodyOnly();
							GameCanvas.panel2.show();
						}
						else
						{
							GameCanvas.panel2 = new Panel();
							GameCanvas.panel2.setTypeKiGuiOnly();
							GameCanvas.panel2.show();
						}
					}
					GameCanvas.panel.tabName[1] = GameCanvas.panel.shopTabName;
					if (b35 == 2)
					{
						string[][] array10 = GameCanvas.panel.tabName[1];
						if (flag5)
						{
							GameCanvas.panel.tabName[1] = new string[4][]
							{
								array10[0],
								array10[1],
								array10[2],
								array10[3]
							};
						}
						else
						{
							GameCanvas.panel.tabName[1] = new string[5][]
							{
								array10[0],
								array10[1],
								array10[2],
								array10[3],
								array10[4]
							};
						}
					}
					GameCanvas.panel.setTypeShop(b35);
					GameCanvas.panel.show();
					break;
				}
				case -43:
				{
					sbyte itemAction = msg.reader().readByte();
					sbyte where = msg.reader().readByte();
					sbyte index2 = msg.reader().readByte();
					string info4 = msg.reader().readUTF();
					GameCanvas.panel.itemRequest(itemAction, info4, where, index2);
					break;
				}
				case -42:
					Char.myCharz().cHPGoc = msg.readLong();
					Char.myCharz().cMPGoc = msg.readLong();
					Char.myCharz().cDamGoc = msg.readLong();
					Char.myCharz().cHPFull = msg.readLong();
					Char.myCharz().cMPFull = msg.readLong();
					Char.myCharz().cHP = msg.readLong();
					Char.myCharz().cMP = msg.readLong();
					Char.myCharz().cspeed = msg.reader().readByte();
					Char.myCharz().hpFrom1000TiemNang = msg.reader().readByte();
					Char.myCharz().mpFrom1000TiemNang = msg.reader().readByte();
					Char.myCharz().damFrom1000TiemNang = msg.reader().readByte();
					Char.myCharz().cDamFull = msg.readLong();
					Char.myCharz().cDefull = msg.reader().readInt();
					Char.myCharz().cCriticalFull = msg.reader().readByte();
					// Unlike HP/MP/damage, potential is always encoded as a real
					// 64-bit value in the server's -42 packet. Bypass Message.readLong()
					// because ModFunc.isReadInt would consume only half of this field.
					Char.myCharz().cTiemNang = msg.reader().readLong();
					Char.myCharz().expForOneAdd = msg.reader().readShort();
					Char.myCharz().cDefGoc = msg.reader().readShort();
					Char.myCharz().cCriticalGoc = msg.reader().readByte();
					
					InfoDlg.hide();
					break;
				case -41:
				{
					sbyte b31 = msg.reader().readByte();
					Char.myCharz().strLevel = new string[b31];
					for (int num113 = 0; num113 < b31; num113++)
					{
						string text3 = msg.reader().readUTF();
						Char.myCharz().strLevel[num113] = text3;
					}
					Res.outz("---   xong  level caption cmd : " + msg.command);
					break;
				}
				case -37:
				{
					if (msg.reader().readByte() != 0)
					{
						break;
					}
					Char.myCharz().head = msg.reader().readShort();
					Char.myCharz().setDefaultPart();
					int num102 = msg.reader().readUnsignedByte();
					Res.outz("num body = " + num102);
					Char.myCharz().arrItemBody = new Item[num102];
					for (int num103 = 0; num103 < num102; num103++)
					{
						short num104 = msg.reader().readShort();
						if (num104 == -1)
						{
							continue;
						}
						Char.myCharz().arrItemBody[num103] = new Item();
						Char.myCharz().arrItemBody[num103].template = ItemTemplates.get(num104);
						int type3 = Char.myCharz().arrItemBody[num103].template.type;
						Char.myCharz().arrItemBody[num103].quantity = msg.reader().readInt();
						Char.myCharz().arrItemBody[num103].info = msg.reader().readUTF();
						Char.myCharz().arrItemBody[num103].content = msg.reader().readUTF();
						int num105 = msg.reader().readUnsignedByte();
						if (num105 != 0)
						{
							Char.myCharz().arrItemBody[num103].itemOption = new ItemOption[num105];
							for (int num106 = 0; num106 < Char.myCharz().arrItemBody[num103].itemOption.Length; num106++)
							{
								int num107 = msg.reader().readUnsignedByte();
								int param5 = msg.reader().readUnsignedShort();
								if (num107 != -1)
								{
									Char.myCharz().arrItemBody[num103].itemOption[num106] = new ItemOption(num107, param5);
								}
							}
						}
						switch (type3)
						{
						case 1:
							Char.myCharz().leg = Char.myCharz().arrItemBody[num103].template.part;
							break;
						case 0:
							Char.myCharz().body = Char.myCharz().arrItemBody[num103].template.part;
							break;
						}
					}
					break;
				}
				case -36:
				{
					sbyte b25 = msg.reader().readByte();
					Res.outz("cAction= " + b25);
					if (b25 == 0)
					{
						int num71 = msg.reader().readUnsignedByte();
						Char.myCharz().arrItemBag = new Item[num71];
						GameScr.hpPotion = 0;
						Res.outz("numC=" + num71);
						for (int num72 = 0; num72 < num71; num72++)
						{
							short num73 = msg.reader().readShort();
							if (num73 == -1)
							{
								continue;
							}
							Char.myCharz().arrItemBag[num72] = new Item();
							Char.myCharz().arrItemBag[num72].template = ItemTemplates.get(num73);
							Char.myCharz().arrItemBag[num72].quantity = msg.reader().readInt();
							Char.myCharz().arrItemBag[num72].info = msg.reader().readUTF();
							Char.myCharz().arrItemBag[num72].content = msg.reader().readUTF();
							Char.myCharz().arrItemBag[num72].indexUI = num72;
							int num74 = msg.reader().readUnsignedByte();
							if (num74 != 0)
							{
								Char.myCharz().arrItemBag[num72].itemOption = new ItemOption[num74];
								for (int num75 = 0; num75 < Char.myCharz().arrItemBag[num72].itemOption.Length; num75++)
								{
									int num76 = msg.reader().readUnsignedByte();
									int param4 = msg.reader().readUnsignedShort();
									if (num76 != -1)
									{
										Char.myCharz().arrItemBag[num72].itemOption[num75] = new ItemOption(num76, param4);
									}
								}
								Char.myCharz().arrItemBag[num72].compare = GameCanvas.panel.getCompare(Char.myCharz().arrItemBag[num72]);
							}
							_ = Char.myCharz().arrItemBag[num72].template.type;
							if (Char.myCharz().arrItemBag[num72].template.type == 6)
							{
								GameScr.hpPotion += Char.myCharz().arrItemBag[num72].quantity;
							}
						}
					}
					if (b25 == 2)
					{
						sbyte b26 = msg.reader().readByte();
						int quantity2 = msg.reader().readInt();
						int quantity3 = Char.myCharz().arrItemBag[b26].quantity;
						Char.myCharz().arrItemBag[b26].quantity = quantity2;
						if (Char.myCharz().arrItemBag[b26].quantity < quantity3 && Char.myCharz().arrItemBag[b26].template.type == 6)
						{
							GameScr.hpPotion -= quantity3 - Char.myCharz().arrItemBag[b26].quantity;
						}
						if (Char.myCharz().arrItemBag[b26].quantity == 0)
						{
							Char.myCharz().arrItemBag[b26] = null;
						}
					}
					break;
				}
				case -35:
				{
					sbyte b18 = msg.reader().readByte();
					Res.outz("cAction= " + b18);
					if (b18 == 0)
					{
						int num51 = msg.reader().readUnsignedByte();
						Char.myCharz().arrItemBox = new Item[num51];
						GameCanvas.panel.hasUse = 0;
						for (int num52 = 0; num52 < num51; num52++)
						{
							short num53 = msg.reader().readShort();
							if (num53 == -1)
							{
								continue;
							}
							Char.myCharz().arrItemBox[num52] = new Item();
							Char.myCharz().arrItemBox[num52].template = ItemTemplates.get(num53);
							Char.myCharz().arrItemBox[num52].quantity = msg.reader().readInt();
							Char.myCharz().arrItemBox[num52].info = msg.reader().readUTF();
							Char.myCharz().arrItemBox[num52].content = msg.reader().readUTF();
							int num54 = msg.reader().readUnsignedByte();
							if (num54 != 0)
							{
								Char.myCharz().arrItemBox[num52].itemOption = new ItemOption[num54];
								for (int num55 = 0; num55 < Char.myCharz().arrItemBox[num52].itemOption.Length; num55++)
								{
									int num56 = msg.reader().readUnsignedByte();
									int param3 = msg.reader().readUnsignedShort();
									if (num56 != -1)
									{
										Char.myCharz().arrItemBox[num52].itemOption[num55] = new ItemOption(num56, param3);
									}
								}
							}
							GameCanvas.panel.hasUse++;
						}
						// A clan-storage snapshot is sent only after its transaction commits.
						// Request the authoritative buff state now so the remaining duration
						// changes in the open client session, without requiring relogin.
						if (GameCanvas.panel.isBoxClan)
						{
							ClanProgression.requestBuffSnapshot(true);
						}
					}
					if (b18 == 1)
					{
						bool isBoxClan = false;
						try
						{
							if (msg.reader().readByte() == 1)
							{
								isBoxClan = true;
							}
						}
						catch (Exception)
						{
						}
						GameCanvas.panel.setTypeBox();
						GameCanvas.panel.isBoxClan = isBoxClan;
						if (isBoxClan)
						{
							GameCanvas.panel.setClanBoxTitle();
						}
						GameCanvas.panel.show();
					}
					if (b18 == 2)
					{
						sbyte b19 = msg.reader().readByte();
						int quantity = msg.reader().readInt();
						Char.myCharz().arrItemBox[b19].quantity = quantity;
						if (Char.myCharz().arrItemBox[b19].quantity == 0)
						{
							Char.myCharz().arrItemBox[b19] = null;
						}
					}
					break;
				}
				case -34:
				{
					sbyte b3 = msg.reader().readByte();
					Res.outz("act= " + b3);
					if (b3 == 0 && GameScr.gI().magicTree != null)
					{
						Res.outz("toi duoc day");
						MagicTree magicTree = GameScr.gI().magicTree;
						magicTree.id = msg.reader().readShort();
						magicTree.name = msg.reader().readUTF();
						magicTree.name = Res.changeString(magicTree.name);
						magicTree.x = msg.reader().readShort();
						magicTree.y = msg.reader().readShort();
						magicTree.level = msg.reader().readByte();
						magicTree.currPeas = msg.reader().readShort();
						magicTree.maxPeas = msg.reader().readShort();
						Res.outz("curr Peas= " + magicTree.currPeas);
						magicTree.strInfo = msg.reader().readUTF();
						magicTree.seconds = msg.reader().readInt();
						magicTree.timeToRecieve = magicTree.seconds;
						sbyte b4 = msg.reader().readByte();
						magicTree.peaPostionX = new int[b4];
						magicTree.peaPostionY = new int[b4];
						for (int l = 0; l < b4; l++)
						{
							magicTree.peaPostionX[l] = msg.reader().readByte();
							magicTree.peaPostionY[l] = msg.reader().readByte();
						}
						magicTree.isUpdate = msg.reader().readBool();
						magicTree.last = (magicTree.cur = mSystem.currentTimeMillis());
						GameScr.gI().magicTree.isUpdateTree = true;
					}
					if (b3 == 1)
					{
						myVector = new MyVector();
						try
						{
							while (msg.reader().available() > 0)
							{
								string caption2 = msg.reader().readUTF();
								myVector.addElement(new Command(caption2, GameCanvas.instance, 888392, null));
							}
						}
						catch (Exception ex2)
						{
							Cout.println("Loi MAGIC_TREE " + ex2.ToString());
						}
						GameCanvas.menu.startAt(myVector, 3);
					}
					if (b3 == 2)
					{
						GameScr.gI().magicTree.remainPeas = msg.reader().readShort();
						GameScr.gI().magicTree.seconds = msg.reader().readInt();
						GameScr.gI().magicTree.last = (GameScr.gI().magicTree.cur = mSystem.currentTimeMillis());
						GameScr.gI().magicTree.isUpdateTree = true;
						GameScr.gI().magicTree.isPeasEffect = true;
					}
					break;
				}
				case -32:
				{
					short id4 = msg.reader().readShort();
					int num176 = msg.reader().readInt();
					sbyte[] array20 = null;
					Image image2 = null;
					try
					{
						array20 = new sbyte[num176];
						for (int num177 = 0; num177 < num176; num177++)
						{
							array20[num177] = msg.reader().readByte();
						}
						image2 = Image.createImage(array20, 0, num176);
						BgItem.imgNew.put(id4 + string.Empty, image2);
					}
					catch (Exception)
					{
						array20 = null;
						BgItem.imgNew.put(id4 + string.Empty, Image.createRGBImage(new int[1], 1, 1, bl: true));
					}
					if (array20 != null)
					{
						if (mGraphics.zoomLevel > 1)
						{
							Rms.saveRMS(mGraphics.zoomLevel + "bgItem" + id4, array20);
						}
						BgItemMn.blendcurrBg(id4, image2);
					}
					break;
				}
				case -31:
				{
					TileMap.vItemBg.removeAllElements();
					short num163 = msg.reader().readShort();
					for (int num164 = 0; num164 < num163; num164++)
					{
						BgItem bgItem = new BgItem();
						bgItem.id = num164;
						bgItem.idImage = msg.reader().readShort();
						bgItem.layer = msg.reader().readByte();
						bgItem.dx = msg.reader().readShort();
						bgItem.dy = msg.reader().readShort();
						sbyte b45 = msg.reader().readByte();
						bgItem.tileX = new int[b45];
						bgItem.tileY = new int[b45];
						for (int num165 = 0; num165 < b45; num165++)
						{
							bgItem.tileX[num164] = msg.reader().readByte();
							bgItem.tileY[num164] = msg.reader().readByte();
						}
						TileMap.vItemBg.addElement(bgItem);
					}
					break;
				}
				case -30:
					messageSubCommand(msg);
					break;
				case -29:
					messageNotLogin(msg);
					break;
				case -28:
					messageNotMap(msg);
					break;
				case -26:
				{
					ServerListScreen.testConnect = 2;
					string text7 = msg.reader().readUTF();
					if (text7 == "Vui lòng mở giới hạn sức mạnh" || text7 == "")
					{
						ModFunc.indexAutoPoint = -1;
						ModFunc.pointIncrease = 0;
						ModFunc.autoPointForPet = false;
						GameScr.info1.addInfo("Chỉ số đã đạt tối đa", 0);
					}
					GameCanvas.startOKDlg(text7);
					InfoDlg.hide();
					LoginScr.isContinueToLogin = false;
					Char.isLoadingMap = false;
					if (GameCanvas.currentScreen == GameCanvas.loginScr)
					{
						GameCanvas.serverScreen.switchToMe();
					}
					if (ModFunc.autoLogin != null)
					{
						ModFunc.autoLogin.waitToNextLogin = false;
					}
					break;
				}
				case -25:
					GameScr.info1.addInfo(msg.reader().readUTF(), 0);
					break;
				case -24:
					if (GameCanvas.currentScreen is GameScr)
					{
						GameCanvas.timeBreakLoading = mSystem.currentTimeMillis() + 3000;
					}
					else
					{
						GameCanvas.timeBreakLoading = mSystem.currentTimeMillis() + 30000;
					}
					Char.isLoadingMap = true;
					GameScr.gI().magicTree = null;
					GameCanvas.isLoading = true;
					GameScr.resetAllvector();
					GameCanvas.endDlg();
					TileMap.vGo.removeAllElements();
					PopUp.vPopups.removeAllElements();
					mSystem.gcc();
					TileMap.mapID = msg.reader().readUnsignedByte();
					TileMap.planetID = msg.reader().readByte();
					TileMap.tileID = msg.reader().readByte();
					TileMap.bgID = msg.reader().readByte();
					TileMap.typeMap = msg.reader().readByte();
					TileMap.mapName = msg.reader().readUTF();
					TileMap.zoneID = msg.reader().readByte();
					try
					{
						TileMap.loadMapFromResource(TileMap.mapID);
					}
					catch (Exception)
					{
						Service.gI().requestMaptemplate(TileMap.mapID);
						messWait = msg;
						return;
					}
					loadInfoMap(msg);
					try
					{
						TileMap.isMapDouble = msg.reader().readByte() != 0;
					}
					catch (Exception)
					{
					}
					GameScr.cmx = GameScr.cmtoX;
					GameScr.cmy = GameScr.cmtoY;
					break;
				case -23:
					LoadAuraNpcs(msg);
					break;
				case -22:
					Char.isLockKey = true;
					Char.ischangingMap = true;
					GameScr.gI().timeStartMap = 0;
					GameScr.gI().timeLengthMap = 0;
					Char.myCharz().mobFocus = null;
					Char.myCharz().npcFocus = null;
					Char.myCharz().charFocus = null;
					Char.myCharz().itemFocus = null;
					Char.myCharz().focus.removeAllElements();
					Char.myCharz().testCharId = -9999;
					Char.myCharz().killCharId = -9999;
					GameCanvas.resetBg();
					GameScr.gI().resetButton();
					GameScr.gI().center = null;
					break;
				case -21:
				{
					GameCanvas.debug("SA60", 2);
					short num131 = msg.reader().readShort();
					for (int num132 = 0; num132 < GameScr.vItemMap.size(); num132++)
					{
						if (((ItemMap)GameScr.vItemMap.elementAt(num132)).itemMapID == num131)
						{
							GameScr.vItemMap.removeElementAt(num132);
							break;
						}
					}
					break;
				}
				case -20:
				{
					GameCanvas.debug("SA61", 2);
					Char.myCharz().itemFocus = null;
					short num127 = msg.reader().readShort();
					for (int num128 = 0; num128 < GameScr.vItemMap.size(); num128++)
					{
						ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(num128);
						if (itemMap.itemMapID != num127)
						{
							continue;
						}
						itemMap.setPoint(Char.myCharz().cx, Char.myCharz().cy - 10);
						string text4 = msg.reader().readUTF();
						num = 0;
						try
						{
							num = msg.reader().readShort();
							if (itemMap.template.type == 9)
							{
								num = msg.reader().readShort();
								Char.myCharz().xu += num;
								Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
							}
							else if (itemMap.template.type == 10)
							{
								num = msg.reader().readShort();
								Char.myCharz().luong += num;
								Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
							}
							else if (itemMap.template.type == 34)
							{
								num = msg.reader().readShort();
								Char.myCharz().luongKhoa += num;
								Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
							}
						}
						catch (Exception)
						{
						}
						if (text4.Equals(string.Empty))
						{
							if (itemMap.template.type == 9)
							{
								GameScr.startFlyText(((num >= 0) ? "+" : string.Empty) + num, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -2, mFont.YELLOW);
								SoundMn.gI().getItem();
							}
							else if (itemMap.template.type == 10)
							{
								GameScr.startFlyText(((num >= 0) ? "+" : string.Empty) + num, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -2, mFont.GREEN);
								SoundMn.gI().getItem();
							}
							else if (itemMap.template.type == 34)
							{
								GameScr.startFlyText(((num >= 0) ? "+" : string.Empty) + num, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -2, mFont.RED);
								SoundMn.gI().getItem();
							}
							else
							{
								GameScr.info1.addInfo(mResources.you_receive + " " + ((num <= 0) ? string.Empty : (num + " ")) + itemMap.template.name, 0);
								SoundMn.gI().getItem();
							}
							if (num > 0 && Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 4683)
							{
								ServerEffect.addServerEffect(55, Char.myCharz().petFollow.cmx, Char.myCharz().petFollow.cmy, 1);
								ServerEffect.addServerEffect(55, Char.myCharz().cx, Char.myCharz().cy, 1);
							}
						}
						else if (text4.Length == 1)
						{
							Cout.LogError3("strInf.Length =1:  " + text4);
						}
						else
						{
							GameScr.info1.addInfo(text4, 0);
						}
						break;
					}
					break;
				}
				case -19:
				{
					short num129 = msg.reader().readShort();
					@char = GameScr.findCharInMap(msg.reader().readInt());
					for (int num130 = 0; num130 < GameScr.vItemMap.size(); num130++)
					{
						ItemMap itemMap2 = (ItemMap)GameScr.vItemMap.elementAt(num130);
						if (itemMap2.itemMapID != num129)
						{
							continue;
						}
						if (@char == null)
						{
							return;
						}
						itemMap2.setPoint(@char.cx, @char.cy - 10);
						if (itemMap2.x < @char.cx)
						{
							@char.cdir = -1;
						}
						else if (itemMap2.x > @char.cx)
						{
							@char.cdir = 1;
						}
						break;
					}
					break;
				}
				case -18:
				{
					GameCanvas.debug("SA63", 2);
					int num123 = msg.reader().readByte();
					GameScr.vItemMap.addElement(new ItemMap(msg.reader().readShort(), Char.myCharz().arrItemBag[num123].template.id, Char.myCharz().cx, Char.myCharz().cy, msg.reader().readShort(), msg.reader().readShort()));
					Char.myCharz().arrItemBag[num123] = null;
					break;
				}
				case -14:
					@char = GameScr.findCharInMap(msg.reader().readInt());
					if (@char == null)
					{
						return;
					}
					GameScr.vItemMap.addElement(new ItemMap(msg.reader().readShort(), msg.reader().readShort(), @char.cx, @char.cy, msg.reader().readShort(), msg.reader().readShort()));
					break;
				case -4:
				{
					GameCanvas.debug("SA76", 2);
					@char = GameScr.findCharInMap(msg.reader().readInt());
					if (@char == null)
					{
						return;
					}
					GameCanvas.debug("SA76v1", 2);
					if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
					{
						@char.setSkillPaint(GameScr.sks[msg.reader().readUnsignedByte()], 0);
					}
					else
					{
						@char.setSkillPaint(GameScr.sks[msg.reader().readUnsignedByte()], 1);
					}
					GameCanvas.debug("SA76v2", 2);
					@char.attMobs = new Mob[msg.reader().readByte()];
					for (int num77 = 0; num77 < @char.attMobs.Length; num77++)
					{
						Mob mob3 = (Mob)GameScr.vMob.elementAt(msg.reader().readByte());
						@char.attMobs[num77] = mob3;
						if (num77 == 0)
						{
							if (@char.cx <= mob3.x)
							{
								@char.cdir = 1;
							}
							else
							{
								@char.cdir = -1;
							}
						}
					}
					GameCanvas.debug("SA76v3", 2);
					@char.charFocus = null;
					@char.mobFocus = @char.attMobs[0];
					Char[] array6 = new Char[10];
					num = 0;
					try
					{
						for (num = 0; num < array6.Length; num++)
						{
							int num78 = msg.reader().readInt();
							Char char4 = (array6[num] = ((num78 != Char.myCharz().charID) ? GameScr.findCharInMap(num78) : Char.myCharz()));
							if (num == 0)
							{
								if (@char.cx <= char4.cx)
								{
									@char.cdir = 1;
								}
								else
								{
									@char.cdir = -1;
								}
							}
						}
					}
					catch (Exception ex14)
					{
						Cout.println("Loi PLAYER_ATTACK_N_P " + ex14.ToString());
					}
					GameCanvas.debug("SA76v4", 2);
					if (num > 0)
					{
						@char.attChars = new Char[num];
						for (num = 0; num < @char.attChars.Length; num++)
						{
							@char.attChars[num] = array6[num];
						}
						@char.charFocus = @char.attChars[0];
						@char.mobFocus = null;
					}
					GameCanvas.debug("SA76v5", 2);
					break;
				}
				case 0:
					readLogin(msg);
					break;
				case 1:
				{
					bool flag4 = msg.reader().readBool();
					Res.outz("isRes= " + flag4);
					if (!flag4)
					{
						GameCanvas.startOKDlg(msg.reader().readUTF());
						break;
					}
					GameCanvas.loginScr.isLogin2 = false;
					Rms.saveRMSString("userAo" + ServerListScreen.ipSelect, string.Empty);
					GameCanvas.endDlg();
					GameCanvas.loginScr.doLogin();
					break;
				}
				case 2:
					Char.isLoadingMap = false;
					LoginScr.isLoggingIn = false;
					if (!GameScr.isLoadAllData)
					{
						GameScr.gI().initSelectChar();
					}
					BgItem.clearHashTable();
					GameCanvas.endDlg();
					CreateCharScr.isCreateChar = true;
					CreateCharScr.gI().switchToMe();
					break;
				case 3:
				{
					sbyte num26 = msg.reader().readByte();
					if (num26 == 0)
					{
						Char.myCharz().havePet2 = false;
					}
					if (num26 == 1)
					{
						Char.myCharz().havePet2 = true;
					}
					if (num26 != 2)
					{
						break;
					}
					InfoDlg.hide();
					Char.MyPet2z().head = msg.reader().readShort();
					Char.MyPet2z().setDefaultPart();
					int num27 = msg.reader().readUnsignedByte();
					Char.MyPet2z().arrItemBody = new Item[num27];
					for (int num28 = 0; num28 < num27; num28++)
					{
						short num29 = msg.reader().readShort();
						if (num29 == -1)
						{
							continue;
						}
						Char.MyPet2z().arrItemBody[num28] = new Item
						{
							template = ItemTemplates.get(num29)
						};
						int type2 = Char.MyPet2z().arrItemBody[num28].template.type;
						Char.MyPet2z().arrItemBody[num28].quantity = msg.reader().readInt();
						Char.MyPet2z().arrItemBody[num28].info = msg.reader().readUTF();
						Char.MyPet2z().arrItemBody[num28].content = msg.reader().readUTF();
						int num30 = msg.reader().readUnsignedByte();
						if (num30 != 0)
						{
							Char.MyPet2z().arrItemBody[num28].itemOption = new ItemOption[num30];
							for (int num31 = 0; num31 < Char.MyPet2z().arrItemBody[num28].itemOption.Length; num31++)
							{
								int num32 = msg.reader().readUnsignedByte();
								int param = msg.reader().readUnsignedShort();
								if (num32 != -1)
								{
									Char.MyPet2z().arrItemBody[num28].itemOption[num31] = new ItemOption(num32, param);
								}
							}
						}
						switch (type2)
						{
						case 1:
							Char.MyPet2z().leg = Char.MyPet2z().arrItemBody[num28].template.part;
							break;
						case 0:
							Char.MyPet2z().body = Char.MyPet2z().arrItemBody[num28].template.part;
							break;
						}
					}
					Char.MyPet2z().cHP = msg.readLong();
					Char.MyPet2z().cHPFull = msg.readLong();
					Char.MyPet2z().cMP = msg.readLong();
					Char.MyPet2z().cMPFull = msg.readLong();
					Char.MyPet2z().cDamFull = msg.readLong();
					Char.MyPet2z().cName = msg.reader().readUTF();
					Char.MyPet2z().currStrLevel = msg.reader().readUTF();
					Char.MyPet2z().cPower = msg.reader().readLong();
					Char.MyPet2z().cTiemNang = msg.reader().readLong();
					Char.MyPet2z().petStatus = msg.reader().readByte();
					Char.MyPet2z().cStamina = msg.reader().readShort();
					Char.MyPet2z().cMaxStamina = msg.reader().readShort();
					Char.MyPet2z().cCriticalFull = msg.reader().readByte();
					Char.MyPet2z().cDefull = msg.reader().readInt();
					Char.MyPet2z().arrPetSkill = new Skill[msg.reader().readByte()];
					for (int num33 = 0; num33 < Char.MyPet2z().arrPetSkill.Length; num33++)
					{
						short num34 = msg.reader().readShort();
						if (num34 != -1)
						{
							Char.MyPet2z().arrPetSkill[num33] = Skills.get(num34);
							continue;
						}
						Char.MyPet2z().arrPetSkill[num33] = new Skill();
						Char.MyPet2z().arrPetSkill[num33].template = null;
						Char.MyPet2z().arrPetSkill[num33].moreInfo = msg.reader().readUTF();
					}
					if (GameCanvas.w > 2 * Panel.WIDTH_PANEL)
					{
						GameCanvas.panel2 = new Panel();
						GameCanvas.panel2.tabName[7] = new string[1][] { new string[1] { string.Empty } };
						GameCanvas.panel2.setTypeBodyOnly();
						GameCanvas.panel2.show();
						GameCanvas.panel.setTypePet2Main();
						GameCanvas.panel.show();
					}
					else
					{
						GameCanvas.panel.tabName[21] = mResources.petMainTab;
						GameCanvas.panel.setTypePet2Main();
						GameCanvas.panel.show();
					}
					break;
				}
				case 6:
					Char.myCharz().xu = msg.reader().readLong();
					Char.myCharz().luong = msg.reader().readInt();
					Char.myCharz().luongKhoa = msg.reader().readInt();
					Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
					Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
					Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
					GameCanvas.endDlg();
					break;
				case 7:
				{
					sbyte type = msg.reader().readByte();
					short id = msg.reader().readShort();
					string info2 = msg.reader().readUTF();
					GameCanvas.panel.saleRequest(type, info2, id);
					break;
				}
				case 11:
				{
					GameCanvas.debug("SA9", 2);
					int num8 = msg.reader().readByte();
					sbyte b5 = msg.reader().readByte();
					if (b5 != 0)
					{
						Mob.arrMobTemplate[num8].data.readDataNewBoss(NinjaUtil.readByteArray(msg), b5);
					}
					else
					{
						Mob.arrMobTemplate[num8].data.readData(NinjaUtil.readByteArray(msg));
					}
					for (int num9 = 0; num9 < GameScr.vMob.size(); num9++)
					{
						Mob mob = (Mob)GameScr.vMob.elementAt(num9);
						if (mob.templateId == num8)
						{
							mob.w = Mob.arrMobTemplate[num8].data.width;
							mob.h = Mob.arrMobTemplate[num8].data.height;
						}
					}
					sbyte[] array2 = NinjaUtil.readByteArray(msg);
					Image img = Image.createImage(array2, 0, array2.Length);
					Mob.arrMobTemplate[num8].data.img = img;
					int num10 = msg.reader().readByte();
					Mob.arrMobTemplate[num8].data.typeData = num10;
					if (num10 == 1 || num10 == 2)
					{
						readFrameBoss(msg, num8);
					}
					break;
				}
				case 20:
					phuban_Info(msg);
					break;
				case 24:
					read_opt(msg);
					break;
				case 27:
				{
					myVector = new MyVector();
					msg.reader().readUTF();
					int num4 = msg.reader().readByte();
					for (int k = 0; k < num4; k++)
					{
						string caption = msg.reader().readUTF();
						short num5 = msg.reader().readShort();
						myVector.addElement(new Command(caption, GameCanvas.instance, 88819, num5));
					}
					GameCanvas.menu.startWithoutCloseButton(myVector, 3);
					break;
				}
				case 29:
					GameCanvas.debug("SA58", 2);
					GameScr.gI().openUIZone(msg);
					break;
				case 32:
				{
					int num166 = msg.reader().readShort();
					for (int num167 = 0; num167 < GameScr.vNpc.size(); num167++)
					{
						Npc npc4 = (Npc)GameScr.vNpc.elementAt(num167);
						if (npc4.template.npcTemplateId == num166 && npc4.Equals(Char.myCharz().npcFocus))
						{
							string text8 = msg.reader().readUTF();
							string[] array18 = new string[msg.reader().readByte()];
							for (int num168 = 0; num168 < array18.Length; num168++)
							{
								array18[num168] = msg.reader().readUTF();
							}
							GameScr.gI().createMenu(array18, npc4);
							ChatPopup.addChatPopup(text8, 100000, npc4);
							if (num166 == 21 && text8.Contains("tối đa"))
							{
								ModFunc.GI().maxPhale = ModFunc.GI().currPhale;
							}
							return;
						}
					}
					Npc npc5 = new Npc(num166, 0, -100, 100, num166, GameScr.info1.charId[Char.myCharz().cgender][2]);
					string chat2 = msg.reader().readUTF();
					string[] array19 = new string[msg.reader().readByte()];
					for (int num169 = 0; num169 < array19.Length; num169++)
					{
						array19[num169] = msg.reader().readUTF();
					}
					try
					{
						short avatar2 = msg.reader().readShort();
						npc5.avatar = avatar2;
					}
					catch (Exception)
					{
					}
					GameScr.gI().createMenu(array19, npc5);
					ChatPopup.addChatPopup(chat2, 100000, npc5);
					break;
				}
				case 33:
				{
					InfoDlg.hide();
					GameCanvas.clearKeyHold();
					GameCanvas.clearKeyPressed();
					myVector = new MyVector();
					try
					{
						while (true)
						{
							string caption4 = msg.reader().readUTF();
							myVector.addElement(new Command(caption4, GameCanvas.instance, 88822, null));
						}
					}
					catch (Exception ex27)
					{
						Cout.println("Loi OPEN_UI_MENU " + ex27.ToString());
					}
					if (Char.myCharz().npcFocus == null)
					{
						return;
					}
					for (int num162 = 0; num162 < Char.myCharz().npcFocus.template.menu.Length; num162++)
					{
						string[] array17 = Char.myCharz().npcFocus.template.menu[num162];
						myVector.addElement(new Command(array17[0], GameCanvas.instance, 88820, array17));
					}
					GameCanvas.menu.startAt(myVector, 3);
					break;
				}
				case 38:
				{
					InfoDlg.hide();
					int num153 = msg.reader().readShort();
					string str6 = msg.reader().readUTF();
					str6 = Res.changeString(str6);
					for (int num154 = 0; num154 < GameScr.vNpc.size(); num154++)
					{
						Npc npc2 = (Npc)GameScr.vNpc.elementAt(num154);
						if (npc2.template.npcTemplateId == num153)
						{
							ChatPopup.addChatPopupMultiLine(str6, 100000, npc2);
							GameCanvas.panel.hideNow();
							return;
						}
					}
					Npc npc3 = new Npc(num153, 0, 0, 0, num153, GameScr.info1.charId[Char.myCharz().cgender][2]);
					if (npc3.template.npcTemplateId == 5)
					{
						npc3.charID = 5;
					}
					try
					{
						npc3.avatar = msg.reader().readShort();
					}
					catch (Exception)
					{
					}
					ChatPopup.addChatPopupMultiLine(str6, 100000, npc3);
					GameCanvas.panel.hideNow();
					break;
				}
				case 39:
					GameCanvas.debug("SA49", 2);
					GameScr.gI().typeTradeOrder = 2;
					if (GameScr.gI().typeTrade >= 2 && GameScr.gI().typeTradeOrder >= 2)
					{
						InfoDlg.showWait();
					}
					break;
				case 40:
				{
					GameCanvas.debug("SA52", 2);
					GameCanvas.taskTick = 150;
					short taskId = msg.reader().readShort();
					sbyte index3 = msg.reader().readByte();
					string str2 = msg.reader().readUTF();
					str2 = Res.changeString(str2);
					string str3 = msg.reader().readUTF();
					str3 = Res.changeString(str3);
					string[] array11 = new string[msg.reader().readByte()];
					string[] array12 = new string[array11.Length];
					GameScr.tasks = new int[array11.Length];
					GameScr.mapTasks = new int[array11.Length];
					short[] array13 = new short[array11.Length];
					short count = -1;
					for (int num144 = 0; num144 < array11.Length; num144++)
					{
						string str4 = msg.reader().readUTF();
						str4 = Res.changeString(str4);
						GameScr.tasks[num144] = msg.reader().readByte();
						GameScr.mapTasks[num144] = msg.reader().readShort();
						string str5 = msg.reader().readUTF();
						str5 = Res.changeString(str5);
						array13[num144] = -1;
						if (!str4.Equals(string.Empty))
						{
							array11[num144] = str4;
							array12[num144] = str5;
						}
					}
					try
					{
						count = msg.reader().readShort();
						for (int num145 = 0; num145 < array11.Length; num145++)
						{
							array13[num145] = msg.reader().readShort();
						}
					}
					catch (Exception ex22)
					{
						Cout.println("Loi TASK_GET " + ex22.ToString());
					}
					Char.myCharz().taskMaint = new Task(taskId, index3, str2, str3, array11, array13, count, array12);
					if (Char.myCharz().npcFocus != null)
					{
						Npc.clearEffTask();
					}
					Char.taskAction(isNextStep: false);
					break;
				}
				case 41:
					GameCanvas.debug("SA53", 2);
					GameCanvas.taskTick = 100;
					Res.outz("TASK NEXT");
					Char.myCharz().taskMaint.index++;
					Char.myCharz().taskMaint.count = 0;
					Npc.clearEffTask();
					Char.taskAction(isNextStep: true);
					break;
				case 43:
					GameCanvas.taskTick = 50;
					GameCanvas.debug("SA55", 2);
					Char.myCharz().taskMaint.count = msg.reader().readShort();
					if (Char.myCharz().npcFocus != null)
					{
						Npc.clearEffTask();
					}
					try
					{
						short x_hint = msg.reader().readShort();
						short y_hint = msg.reader().readShort();
						Char.myCharz().x_hint = x_hint;
						Char.myCharz().y_hint = y_hint;
						Res.outz("CMD   TASK_UPDATE:43_mapID =    x|y " + x_hint + "|" + y_hint);
						for (int num143 = 0; num143 < TileMap.vGo.size(); num143++)
						{
							Res.outz("===> " + TileMap.vGo.elementAt(num143));
						}
					}
					catch (Exception)
					{
					}
					break;
				case 46:
					GameCanvas.debug("SA5", 2);
					Cout.LogWarning("Controler RESET_POINT  " + Char.ischangingMap);
					Char.isLockKey = false;
					Char.myCharz().setResetPoint(msg.reader().readShort(), msg.reader().readShort());
					break;
				case 47:
					GameScr.gI().resetButton();
					break;
				case 50:
				{
					sbyte b37 = msg.reader().readByte();
					Panel.vGameInfo.removeAllElements();
					for (int num142 = 0; num142 < b37; num142++)
					{
						GameInfo gameInfo = new GameInfo();
						gameInfo.id = msg.reader().readShort();
						gameInfo.main = msg.reader().readUTF();
						gameInfo.content = msg.reader().readUTF();
						Panel.vGameInfo.addElement(gameInfo);
						bool hasRead = Rms.loadRMSInt(gameInfo.id + string.Empty) != -1;
						gameInfo.hasRead = hasRead;
					}
					break;
				}
				case 54:
				{
					@char = GameScr.findCharInMap(msg.reader().readInt());
					if (@char == null)
					{
						return;
					}
					int num122 = msg.reader().readUnsignedByte();
					if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
					{
						@char.setSkillPaint(GameScr.sks[num122], 0);
					}
					else
					{
						@char.setSkillPaint(GameScr.sks[num122], 1);
					}
					Mob[] array8 = new Mob[10];
					num = 0;
					try
					{
						for (num = 0; num < array8.Length; num++)
						{
							Mob mob7 = (array8[num] = (Mob)GameScr.vMob.elementAt(msg.reader().readByte()));
							if (num == 0)
							{
								if (@char.cx <= mob7.x)
								{
									@char.cdir = 1;
								}
								else
								{
									@char.cdir = -1;
								}
							}
						}
					}
					catch (Exception)
					{
					}
					if (num > 0)
					{
						@char.attMobs = new Mob[num];
						for (num = 0; num < @char.attMobs.Length; num++)
						{
							@char.attMobs[num] = array8[num];
						}
						@char.charFocus = null;
						@char.mobFocus = @char.attMobs[0];
					}
					break;
				}
				case 56:
				{
					@char = null;
					int num67 = msg.reader().readInt();
					if (num67 == Char.myCharz().charID)
					{
						bool flag2 = false;
						@char = Char.myCharz();
						@char.cHP = msg.readLong();
						long num68 = msg.readLong();
						if (num68 != 0L)
						{
							@char.doInjure();
						}
						try
						{
							flag2 = msg.reader().readBoolean();
							sbyte b23 = msg.reader().readByte();
							if (b23 != -1)
							{
								EffecMn.addEff(new Effect(b23, @char.cx, @char.cy, 3, 1, -1));
							}
						}
						catch (Exception)
						{
						}
						if (Char.myCharz().cTypePk != 4)
						{
							if (num68 == 0L)
							{
								GameScr.startFlyText(mResources.miss, @char.cx, @char.cy - @char.ch, 0, -3, mFont.MISS_ME);
							}
							else
							{
								GameScr.startFlyText("-" + num68, @char.cx, @char.cy - @char.ch, 0, -3, flag2 ? mFont.FATAL : mFont.RED);
							}
						}
						break;
					}
					@char = GameScr.findCharInMap(num67);
					if (@char == null)
					{
						return;
					}
					@char.cHP = msg.readLong();
					bool flag3 = false;
					long num69 = msg.readLong();
					if (num69 != 0L)
					{
						@char.doInjure();
					}
					int num70 = 0;
					try
					{
						flag3 = msg.reader().readBoolean();
						sbyte b24 = msg.reader().readByte();
						if (b24 != -1)
						{
							EffecMn.addEff(new Effect(b24, @char.cx, @char.cy, 3, 1, -1));
						}
					}
					catch (Exception)
					{
					}
					num69 += num70;
					if (@char.cTypePk != 4)
					{
						if (num69 == 0L)
						{
							GameScr.startFlyText(mResources.miss, @char.cx, @char.cy - @char.ch, 0, -3, mFont.MISS);
						}
						else
						{
							GameScr.startFlyText("-" + num69, @char.cx, @char.cy - @char.ch, 0, -3, flag3 ? mFont.FATAL : mFont.ORANGE);
						}
					}
					break;
				}
				case 57:
				{
					GameCanvas.debug("SZ6", 2);
					MyVector myVector2 = new MyVector();
					myVector2.addElement(new Command(msg.reader().readUTF(), GameCanvas.instance, 88817, null));
					GameCanvas.menu.startAt(myVector2, 3);
					break;
				}
				case 58:
				{
					int num60 = msg.reader().readInt();
					Char obj5 = ((num60 != Char.myCharz().charID) ? GameScr.findCharInMap(num60) : Char.myCharz());
					obj5.moveFast = new short[3];
					obj5.moveFast[0] = 0;
					short num61 = msg.reader().readShort();
					short num62 = msg.reader().readShort();
					obj5.moveFast[1] = num61;
					obj5.moveFast[2] = num62;
					try
					{
						num60 = msg.reader().readInt();
						Char obj6 = ((num60 != Char.myCharz().charID) ? GameScr.findCharInMap(num60) : Char.myCharz());
						obj6.cx = num61;
						obj6.cy = num62;
					}
					catch (Exception ex11)
					{
						Cout.println("Loi MOVE_FAST " + ex11.ToString());
					}
					break;
				}
				case 62:
					@char = GameScr.findCharInMap(msg.reader().readInt());
					if (@char != null)
					{
						@char.killCharId = Char.myCharz().charID;
						Char.myCharz().npcFocus = null;
						Char.myCharz().mobFocus = null;
						Char.myCharz().itemFocus = null;
						Char.myCharz().charFocus = @char;
						Char.isManualFocus = true;
						GameScr.info1.addInfo(@char.cName + mResources.CUU_SAT, 0);
					}
					break;
				case 63:
					Char.myCharz().killCharId = msg.reader().readInt();
					Char.myCharz().npcFocus = null;
					Char.myCharz().mobFocus = null;
					Char.myCharz().itemFocus = null;
					Char.myCharz().charFocus = GameScr.findCharInMap(Char.myCharz().killCharId);
					Char.isManualFocus = true;
					break;
				case 64:
					GameCanvas.debug("SZ5", 2);
					@char = Char.myCharz();
					try
					{
						@char = GameScr.findCharInMap(msg.reader().readInt());
					}
					catch (Exception ex9)
					{
						Cout.println("Loi CLEAR_CUU_SAT " + ex9.ToString());
					}
					@char.killCharId = -9999;
					break;
				case 65:
				{
					sbyte id2 = msg.reader().readSByte();
					string text2 = msg.reader().readUTF();
					short num48 = msg.reader().readShort();
					if (!ItemTime.isExistMessage(id2))
					{
						ItemTime itemTime = new ItemTime();
						itemTime.initTimeText(id2, text2, num48);
						GameScr.textTime.addElement(itemTime);
					}
					else if (num48 != 0)
					{
						ItemTime.getMessageById(id2).initTimeText(id2, text2, num48);
					}
					else
					{
						GameScr.textTime.removeElement(ItemTime.getMessageById(id2));
					}
					break;
				}
				case 66:
					readGetImgByName(msg);
					break;
				case 68:
				{
					short itemMapID = msg.reader().readShort();
					short itemTemplateID = msg.reader().readShort();
					int x = msg.reader().readShort();
					int y = msg.reader().readShort();
					int num35 = msg.reader().readInt();
					short r = 0;
					if (num35 == -2)
					{
						r = msg.reader().readShort();
					}
					ItemMap o = new ItemMap(num35, itemMapID, itemTemplateID, x, y, r);
					GameScr.vItemMap.addElement(o);
					break;
				}
				case 69:
					SoundMn.IsDelAcc = msg.reader().readByte() != 0;
					break;
				case 81:
					((Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte())).isDisable = msg.reader().readBool();
					break;
				case 82:
					((Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte())).isDontMove = msg.reader().readBool();
					break;
				case 83:
				{
					int num23 = msg.reader().readInt();
					@char = ((num23 != Char.myCharz().charID) ? GameScr.findCharInMap(num23) : Char.myCharz());
					if (@char == null)
					{
						return;
					}
					Mob mobToAttack = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
					if (@char.mobMe != null)
					{
						@char.mobMe.attackOtherMob(mobToAttack);
					}
					break;
				}
				case 84:
				{
					int num18 = msg.reader().readInt();
					if (num18 == Char.myCharz().charID)
					{
						@char = Char.myCharz();
					}
					else
					{
						@char = GameScr.findCharInMap(num18);
						if (@char == null)
						{
							return;
						}
					}
					@char.cHP = @char.cHPFull;
					@char.cMP = @char.cMPFull;
					@char.cx = msg.reader().readShort();
					@char.cy = msg.reader().readShort();
					@char.liveFromDead();
					break;
				}
				case 85:
					((Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte())).isFire = msg.reader().readBool();
					break;
				case 86:
				{
					Mob mob2 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
					mob2.isIce = msg.reader().readBool();
					if (!mob2.isIce)
					{
						ServerEffect.addServerEffect(77, mob2.x, mob2.y - 9, 1);
					}
					break;
				}
				case 87:
					((Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte())).isWind = msg.reader().readBool();
					break;
				case 92:
				{
					if (GameCanvas.currentScreen == GameScr.instance)
					{
						GameCanvas.endDlg();
					}
					string text = msg.reader().readUTF();
					string str = msg.reader().readUTF();
					str = Res.changeString(str);
					string empty = string.Empty;
					Char char2 = null;
					sbyte b2 = 0;
					if (!text.Equals(string.Empty))
					{
						char2 = new Char();
						char2.charID = msg.reader().readInt();
						char2.head = msg.reader().readShort();
						char2.headICON = msg.reader().readShort();
						char2.body = msg.reader().readShort();
						char2.bag = msg.reader().readShort();
						char2.leg = msg.reader().readShort();
						b2 = msg.reader().readByte();
						char2.cName = text;
						try
						{
							char2.isTichXanh = msg.reader().readByte() == 1;
						}
						catch (Exception)
						{
							char2.isTichXanh = false;
						}
					}
					empty += str;
					InfoDlg.hide();
					if (text.Equals(string.Empty))
					{
						GameScr.info1.addInfo(empty, 0);
						break;
					}
					GameScr.info2.addInfoWithChar(empty, char2, b2 == 0);
					if (GameCanvas.panel.isShow && GameCanvas.panel.type == 8)
					{
						GameCanvas.panel.initLogMessage();
					}
					break;
				}
				case 94:
					GameScr.info1.addInfo(msg.reader().readUTF(), 0);
					break;
				case 112:
					switch (msg.reader().readByte())
					{
					case 0:
						Panel.spearcialImage = msg.reader().readShort();
						Panel.specialInfo = msg.reader().readUTF();
						ModFunc.GI().CheckAutoIntrinsic(Panel.specialInfo);
						break;
					case 1:
					{
						sbyte b = msg.reader().readByte();
						Char.myCharz().infoSpeacialSkill = new string[b][];
						Char.myCharz().imgSpeacialSkill = new short[b][];
						GameCanvas.panel.speacialTabName = new string[b][];
						for (int i = 0; i < b; i++)
						{
							GameCanvas.panel.speacialTabName[i] = new string[2];
							string[] array = Res.split(msg.reader().readUTF(), "\n", 0);
							if (array.Length == 2)
							{
								GameCanvas.panel.speacialTabName[i] = array;
							}
							if (array.Length == 1)
							{
								GameCanvas.panel.speacialTabName[i][0] = array[0];
								GameCanvas.panel.speacialTabName[i][1] = string.Empty;
							}
							int num3 = msg.reader().readByte();
							Char.myCharz().infoSpeacialSkill[i] = new string[num3];
							Char.myCharz().imgSpeacialSkill[i] = new short[num3];
							for (int j = 0; j < num3; j++)
							{
								Char.myCharz().imgSpeacialSkill[i][j] = msg.reader().readShort();
								Char.myCharz().infoSpeacialSkill[i][j] = msg.reader().readUTF();
							}
						}
						GameCanvas.panel.tabName[25] = GameCanvas.panel.speacialTabName;
						GameCanvas.panel.setTypeSpeacialSkill();
						GameCanvas.panel.show();
						break;
					}
					}
					break;
				case -66:
				{
					short id3 = msg.reader().readShort();
					sbyte[] data4 = NinjaUtil.readByteArray(msg);
					EffectData effDataById = Effect.getEffDataById(id3);
					sbyte b43 = msg.reader().readSByte();
					if (b43 == 0)
					{
						effDataById.readData(data4);
					}
					else
					{
						effDataById.readDataNewBoss(data4, b43);
					}
					sbyte[] array16 = NinjaUtil.readByteArray(msg);
					effDataById.img = Image.createImage(array16, 0, array16.Length);
					break;
				}
				case 88:
				{
					string info = msg.reader().readUTF();
					short num2 = msg.reader().readShort();
					GameCanvas.inputDlg.show(info, new Command(mResources.ACCEPT, GameCanvas.instance, 88818, num2), TField.INPUT_TYPE_ANY);
					break;
				}
				case 90:
					GameCanvas.debug("SA577", 2);
					requestItemPlayer(msg);
					break;
				}
				Char char10;
				short num191;
				sbyte cFlag;
				switch (msg.command)
				{
				case -73:
				{
					sbyte b50 = msg.reader().readByte();
					for (int num200 = 0; num200 < GameScr.vNpc.size(); num200++)
					{
						Npc npc6 = (Npc)GameScr.vNpc.elementAt(num200);
						if (npc6.template.npcTemplateId == b50)
						{
							if (msg.reader().readByte() == 0)
							{
								npc6.isHide = true;
							}
							else
							{
								npc6.isHide = false;
							}
							break;
						}
					}
					break;
				}
				case -75:
				{
					Mob mob15 = null;
					try
					{
						mob15 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
					}
					catch (Exception)
					{
					}
					if (mob15 != null)
					{
						mob15.levelBoss = msg.reader().readByte();
						if (mob15.levelBoss > 0)
						{
							mob15.typeSuperEff = Res.random(0, 3);
						}
					}
					break;
				}
				case -17:
					Char.myCharz().meDead = true;
					Char.myCharz().cPk = msg.reader().readByte();
					Char.myCharz().startDie(msg.reader().readShort(), msg.reader().readShort());
					try
					{
						Char.myCharz().cPower = msg.reader().readLong();
						Char.myCharz().applyCharLevelPercent();
					}
					catch (Exception)
					{
						Cout.println("Loi tai ME_DIE " + msg.command);
					}
					Char.myCharz().countKill = 0;
					break;
				case -16:
					if (Char.myCharz().wdx != 0 || Char.myCharz().wdy != 0)
					{
						Char.myCharz().cx = Char.myCharz().wdx;
						Char.myCharz().cy = Char.myCharz().wdy;
						Char.myCharz().wdx = (Char.myCharz().wdy = 0);
					}
					Char.myCharz().liveFromDead();
					Char.myCharz().isLockMove = false;
					Char.myCharz().meDead = false;
					break;
				case -13:
				{
					int num192 = msg.reader().readUnsignedByte();
					if (num192 > GameScr.vMob.size() - 1 || num192 < 0)
					{
						break;
					}
					Mob mob11 = (Mob)GameScr.vMob.elementAt(num192);
					if (mob11.status == 0 || mob11.status == 1)
					{
						mob11.sys = msg.reader().readByte();
						mob11.levelBoss = msg.reader().readByte();
						if (mob11.levelBoss != 0)
						{
							mob11.typeSuperEff = Res.random(0, 3);
						}
						mob11.x = mob11.xFirst;
						mob11.y = mob11.yFirst;
						mob11.status = 5;
						mob11.injureThenDie = false;
						mob11.hp = msg.readLong();
						mob11.maxHp = mob11.hp;
						mob11.updateHp_bar();
						ServerEffect.addServerEffect(60, mob11.x, mob11.y, 1);
					}
					break;
				}
				case -12:
				{
					Mob mob12 = null;
					try
					{
						mob12 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
					}
					catch (Exception)
					{
					}
					if (mob12 == null || mob12.status == 0 || mob12.status == 0)
					{
						break;
					}
					mob12.startDie();
					try
					{
						long num196 = msg.readLong();
						if (msg.reader().readBool())
						{
							GameScr.startFlyText("-" + num196, mob12.x, mob12.y - mob12.h, 0, -2, mFont.FATAL);
						}
						else
						{
							GameScr.startFlyText("-" + num196, mob12.x, mob12.y - mob12.h, 0, -2, mFont.ORANGE);
						}
						sbyte b49 = msg.reader().readByte();
						for (int num197 = 0; num197 < b49; num197++)
						{
							ItemMap itemMap4 = new ItemMap(msg.reader().readShort(), msg.reader().readShort(), mob12.x, mob12.y, msg.reader().readShort(), msg.reader().readShort());
							itemMap4.playerId = msg.reader().readInt();
							GameScr.vItemMap.addElement(itemMap4);
							if (Res.abs(itemMap4.y - Char.myCharz().cy) < 24 && Res.abs(itemMap4.x - Char.myCharz().cx) < 24)
							{
								Char.myCharz().charFocus = null;
							}
						}
						break;
					}
					catch (Exception)
					{
						break;
					}
				}
				case -11:
				{
					Mob mob13 = null;
					try
					{
						int index5 = msg.reader().readUnsignedByte();
						mob13 = (Mob)GameScr.vMob.elementAt(index5);
					}
					catch (Exception)
					{
					}
					if (mob13 != null)
					{
						Char.myCharz().isDie = false;
						Char.isLockKey = false;
						long num201 = msg.readLong();
						long num202;
						try
						{
							num202 = msg.readLong();
						}
						catch (Exception)
						{
							num202 = 0;
						}
						if (mob13.isBusyAttackSomeOne)
						{
							Char.myCharz().doInjure(num201, num202, isCrit: false, isMob: true);
							break;
						}
						mob13.dame = num201;
						mob13.dameMp = num202;
						mob13.setAttack(Char.myCharz());
					}
					break;
				}
				case -10:
				{
					Mob mob17 = null;
					try
					{
						mob17 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
					}
					catch (Exception)
					{
					}
					if (mob17 == null)
					{
						break;
					}
					@char = GameScr.findCharInMap(msg.reader().readInt());
					if (@char != null)
					{
						long num207 = msg.readLong();
						mob17.dame = @char.cHP - num207;
						@char.cHPNew = num207;
						try
						{
							@char.cMP = msg.readLong();
						}
						catch (Exception)
						{
						}
						if (mob17.isBusyAttackSomeOne)
						{
							@char.doInjure(mob17.dame, 0L, isCrit: false, isMob: true);
						}
						else
						{
							mob17.setAttack(@char);
						}
					}
					break;
				}
				case -9:
				{
					Mob mob14 = null;
					try
					{
						mob14 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
					}
					catch (Exception)
					{
					}
					if (mob14 == null)
					{
						break;
					}
					mob14.hp = msg.readLong();
					mob14.updateHp_bar();
					long num204 = msg.readLong();
					if (num204 != 1)
					{
						if (num204 > 1)
						{
							mob14.setInjure();
						}
						bool flag6 = false;
						try
						{
							flag6 = msg.reader().readBoolean();
						}
						catch (Exception)
						{
						}
						sbyte b51 = msg.reader().readByte();
						if (b51 != -1)
						{
							EffecMn.addEff(new Effect(b51, mob14.x, mob14.getY(), 3, 1, -1));
						}
						if (flag6)
						{
							GameScr.startFlyText("-" + num204, mob14.x, mob14.getY() - mob14.getH(), 0, -2, mFont.FATAL);
						}
						else if (num204 == 0L)
						{
							mob14.x = mob14.xFirst;
							mob14.y = mob14.yFirst;
							GameScr.startFlyText(mResources.miss, mob14.x, mob14.getY() - mob14.getH(), 0, -2, mFont.MISS);
						}
						else if (num204 > 1)
						{
							GameScr.startFlyText("-" + num204, mob14.x, mob14.getY() - mob14.getH(), 0, -2, mFont.ORANGE);
						}
					}
					break;
				}
				case -8:
					@char = GameScr.findCharInMap(msg.reader().readInt());
					if (@char != null)
					{
						@char.cPk = msg.reader().readByte();
						@char.waitToDie(msg.reader().readShort(), msg.reader().readShort());
					}
					break;
				case -7:
				{
					int num210 = msg.reader().readInt();
					for (int num211 = 0; num211 < GameScr.vCharInMap.size(); num211++)
					{
						Char char13 = null;
						try
						{
							char13 = (Char)GameScr.vCharInMap.elementAt(num211);
						}
						catch (Exception)
						{
						}
						if (char13 == null)
						{
							break;
						}
						if (char13.charID == num210)
						{
							GameCanvas.debug("SA8x2y" + num211, 2);
							char13.moveTo(msg.reader().readShort(), msg.reader().readShort(), 0);
							char13.lastUpdateTime = mSystem.currentTimeMillis();
							break;
						}
					}
					GameCanvas.debug("SA80x3", 2);
					break;
				}
				case -6:
				{
					GameCanvas.debug("SA81", 2);
					int num205 = msg.reader().readInt();
					for (int num206 = 0; num206 < GameScr.vCharInMap.size(); num206++)
					{
						Char char11 = (Char)GameScr.vCharInMap.elementAt(num206);
						if (char11 != null && char11.charID == num205)
						{
							if (!char11.isInvisiblez && !char11.isUsePlane)
							{
								ServerEffect.addServerEffect(60, char11.cx, char11.cy, 1);
							}
							if (!char11.isUsePlane)
							{
								GameScr.vCharInMap.removeElementAt(num206);
							}
							break;
						}
					}
					break;
				}
				case -5:
				{
					int charID = msg.reader().readInt();
					int num189 = msg.reader().readInt();
					char10 = ((num189 == -100) ? new Mabu
					{
						charID = charID,
						clanID = num189
					} : new Char
					{
						charID = charID,
						clanID = num189
					});
					if (char10.clanID == -2)
					{
						char10.isCopy = true;
					}
					if (readCharInfo(char10, msg))
					{
						sbyte b47 = msg.reader().readByte();
						if (char10.cy <= 10 && b47 != 0 && b47 != 2)
						{
							Teleport p2 = new Teleport(char10.cx, char10.cy, char10.head, char10.cdir, 1, isMe: false, (b47 != 1) ? b47 : char10.cgender)
							{
								id = char10.charID
							};
							char10.isTeleport = true;
							Teleport.addTeleport(p2);
						}
						if (b47 == 2)
						{
							char10.show();
						}
						for (int num190 = 0; num190 < GameScr.vMob.size(); num190++)
						{
							Mob mob10 = (Mob)GameScr.vMob.elementAt(num190);
							if (mob10 != null && mob10.isMobMe && mob10.mobId == char10.charID)
							{
								char10.mobMe = mob10;
								char10.mobMe.x = char10.cx;
								char10.mobMe.y = char10.cy - 40;
								break;
							}
						}
						if (GameScr.findCharInMap(char10.charID) == null)
						{
							GameScr.vCharInMap.addElement(char10);
						}
						char10.isMonkey = msg.reader().readByte();
						num191 = msg.reader().readShort();
						if (num191 != -1)
						{
							char10.isHaveMount = true;
							if (num191 <= 351)
							{
								if (num191 - 346 <= 2)
								{
									char10.isMountVip = false;
								}
								else
								{
									if (num191 - 349 > 2)
									{
										goto IL_9b65;
									}
									char10.isMountVip = true;
								}
							}
							else if (num191 == 396)
							{
								char10.isEventMount = true;
							}
							else
							{
								if (num191 != 532)
								{
									goto IL_9b65;
								}
								char10.isSpeacialMount = true;
							}
						}
						else
						{
							char10.isHaveMount = false;
						}
					}
					goto IL_9b91;
				}
				case -3:
				{
					sbyte num193 = msg.reader().readByte();
					long num194 = msg.readLong();
					if (num193 == 0)
					{
						Char.myCharz().cPower += num194;
					}
					if (num193 == 1)
					{
						Char.myCharz().cTiemNang += num194;
					}
					if (num193 == 2)
					{
						Char.myCharz().cPower += num194;
						Char.myCharz().cTiemNang += num194;
					}
					Char.myCharz().applyCharLevelPercent();
					if (Char.myCharz().cTypePk != 3)
					{
						GameScr.startFlyText(((num194 <= 0) ? string.Empty : "+") + num194, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -4, mFont.GREEN);
						if (num194 > 0 && Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 5002)
						{
							ServerEffect.addServerEffect(55, Char.myCharz().petFollow.cmx, Char.myCharz().petFollow.cmy, 1);
							ServerEffect.addServerEffect(55, Char.myCharz().cx, Char.myCharz().cy, 1);
						}
					}
					break;
				}
				case -2:
				{
					GameCanvas.debug("SA77", 22);
					int num188 = msg.reader().readInt();
					Char.myCharz().yen += num188;
					GameScr.startFlyText((num188 <= 0) ? (string.Empty + num188) : ("+" + num188), Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 10, 0, -2, mFont.YELLOW);
					break;
				}
				case -1:
				{
					GameCanvas.debug("SA77", 222);
					int num212 = msg.reader().readInt();
					Char.myCharz().xu += num212;
					Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
					Char.myCharz().yen -= num212;
					GameScr.startFlyText("+" + num212, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 10, 0, -2, mFont.YELLOW);
					break;
				}
				case 18:
				{
					sbyte b52 = msg.reader().readByte();
					for (int num208 = 0; num208 < b52; num208++)
					{
						int charId = msg.reader().readInt();
						int cx = msg.reader().readShort();
						int cy = msg.reader().readShort();
						long num209 = msg.readLong();
						Char char12 = GameScr.findCharInMap(charId);
						if (char12 != null)
						{
							char12.cx = cx;
							char12.cy = cy;
							char12.cHP = (char12.cHPShow = num209);
							char12.lastUpdateTime = mSystem.currentTimeMillis();
						}
					}
					break;
				}
				case 19:
					Char.myCharz().countKill = msg.reader().readUnsignedShort();
					Char.myCharz().countKillMax = msg.reader().readUnsignedShort();
					break;
				case 45:
				{
					Mob mob16 = null;
					try
					{
						mob16 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
					}
					catch (Exception)
					{
					}
					if (mob16 != null)
					{
						mob16.hp = msg.reader().readInt();
						mob16.updateHp_bar();
						GameScr.startFlyText(mResources.miss, mob16.x, mob16.y - mob16.h, 0, -2, mFont.MISS);
					}
					break;
				}
				case 44:
				{
					int num203 = msg.reader().readInt();
					string info5 = msg.reader().readUTF();
					((Char.myCharz().charID != num203) ? GameScr.findCharInMap(num203) : Char.myCharz())?.addInfo(info5);
					break;
				}
				case 95:
				{
					GameCanvas.debug("SA77", 22);
					int num198 = msg.reader().readInt();
					Char.myCharz().xu += num198;
					Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
					GameScr.startFlyText((num198 <= 0) ? (string.Empty + num198) : ("+" + num198), Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 10, 0, -2, mFont.YELLOW);
					break;
				}
				case 96:
					GameCanvas.debug("SA77a", 22);
					Char.myCharz().taskOrders.addElement(new TaskOrder(msg.reader().readByte(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readUTF(), msg.reader().readUTF(), msg.reader().readByte(), msg.reader().readByte()));
					break;
				case 97:
				{
					sbyte b48 = msg.reader().readByte();
					for (int num195 = 0; num195 < Char.myCharz().taskOrders.size(); num195++)
					{
						TaskOrder taskOrder = (TaskOrder)Char.myCharz().taskOrders.elementAt(num195);
						if (taskOrder.taskId == b48)
						{
							taskOrder.count = msg.reader().readShort();
							break;
						}
					}
					break;
				}
				case 74:
					{
						Mob mob9 = null;
						try
						{
							mob9 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
						}
						catch (Exception)
						{
						}
						if (mob9 != null && mob9.status != 0 && mob9.status != 0)
						{
							mob9.status = 0;
							ServerEffect.addServerEffect(60, mob9.x, mob9.y, 1);
							ItemMap itemMap3 = new ItemMap(msg.reader().readShort(), msg.reader().readShort(), mob9.x, mob9.y, msg.reader().readShort(), msg.reader().readShort());
							GameScr.vItemMap.addElement(itemMap3);
							if (Res.abs(itemMap3.y - Char.myCharz().cy) < 24 && Res.abs(itemMap3.x - Char.myCharz().cx) < 24)
							{
								Char.myCharz().charFocus = null;
							}
						}
						break;
					}
					IL_9b65:
					if (num191 >= Char.ID_NEW_MOUNT)
					{
						char10.idMount = num191;
					}
					goto IL_9b91;
					IL_9b91:
					cFlag = msg.reader().readByte();
					char10.cFlag = cFlag;
					char10.isNhapThe = msg.reader().readByte() == 1;
					try
					{
						char10.idAuraEff = msg.reader().readShort();
						char10.idEff_Set_Item = msg.reader().readSByte();
						char10.idHat = msg.reader().readShort();
						if (char10.bag >= 201 && char10.bag < 255)
						{
							char10.addEffChar(new Effect(char10.bag, char10, 2, -1, 10, 1)
							{
								typeEff = 5
							});
						}
						else
						{
							for (int num199 = 0; num199 < 54; num199++)
							{
								char10.removeEffChar(0, 201 + num199);
							}
						}
					}
					catch (Exception ex33)
					{
						Res.outz("cmd: -5 err: " + ex33.StackTrace);
					}
					char10.isTichXanh = msg.reader().readByte() == 1;
					GameScr.gI().getFlagImage(char10.charID, char10.cFlag);
					break;
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				msg?.cleanup();
			}
		}

		private void readLogin(Message msg)
		{
			sbyte b = msg.reader().readByte();
			ChooseCharScr.playerData = new PlayerData[b];
			Res.outz("[LEN] sl nguoi choi " + b);
			for (int i = 0; i < b; i++)
			{
				int playerID = msg.reader().readInt();
				string name = msg.reader().readUTF();
				short head = msg.reader().readShort();
				short body = msg.reader().readShort();
				short leg = msg.reader().readShort();
				long ppoint = msg.reader().readLong();
				ChooseCharScr.playerData[i] = new PlayerData(playerID, name, head, body, leg, ppoint);
			}
			GameCanvas.chooseCharScr.switchToMe();
			GameCanvas.chooseCharScr.updateChooseCharacter((byte)b);
		}

		private void createSkill(myReader d)
		{
			GameScr.vcSkill = d.readByte();
			GameScr.gI().sOptionTemplates = new SkillOptionTemplate[d.readByte()];
			for (int i = 0; i < GameScr.gI().sOptionTemplates.Length; i++)
			{
				GameScr.gI().sOptionTemplates[i] = new SkillOptionTemplate();
				GameScr.gI().sOptionTemplates[i].id = i;
				GameScr.gI().sOptionTemplates[i].name = d.readUTF();
			}
			GameScr.nClasss = new NClass[d.readByte()];
			for (int j = 0; j < GameScr.nClasss.Length; j++)
			{
				GameScr.nClasss[j] = new NClass();
				GameScr.nClasss[j].classId = j;
				GameScr.nClasss[j].name = d.readUTF();
				GameScr.nClasss[j].skillTemplates = new SkillTemplate[d.readByte()];
				for (int k = 0; k < GameScr.nClasss[j].skillTemplates.Length; k++)
				{
					GameScr.nClasss[j].skillTemplates[k] = new SkillTemplate();
					GameScr.nClasss[j].skillTemplates[k].id = d.readByte();
					GameScr.nClasss[j].skillTemplates[k].name = d.readUTF();
					GameScr.nClasss[j].skillTemplates[k].maxPoint = d.readByte();
					GameScr.nClasss[j].skillTemplates[k].manaUseType = d.readByte();
					GameScr.nClasss[j].skillTemplates[k].type = d.readByte();
					GameScr.nClasss[j].skillTemplates[k].iconId = d.readShort();
					GameScr.nClasss[j].skillTemplates[k].damInfo = d.readUTF();
					int lineWidth = 130;
					if (GameCanvas.w == 128 || GameCanvas.h <= 208)
					{
						lineWidth = 100;
					}
					GameScr.nClasss[j].skillTemplates[k].description = mFont.tahoma_7_green2.splitFontArray(d.readUTF(), lineWidth);
					GameScr.nClasss[j].skillTemplates[k].skills = new Skill[d.readByte()];
					for (int l = 0; l < GameScr.nClasss[j].skillTemplates[k].skills.Length; l++)
					{
						GameScr.nClasss[j].skillTemplates[k].skills[l] = new Skill();
						GameScr.nClasss[j].skillTemplates[k].skills[l].skillId = d.readShort();
						GameScr.nClasss[j].skillTemplates[k].skills[l].template = GameScr.nClasss[j].skillTemplates[k];
						GameScr.nClasss[j].skillTemplates[k].skills[l].point = d.readByte();
						GameScr.nClasss[j].skillTemplates[k].skills[l].powRequire = d.readLong();
						GameScr.nClasss[j].skillTemplates[k].skills[l].manaUse = d.readShort();
						GameScr.nClasss[j].skillTemplates[k].skills[l].coolDown = d.readInt();
						GameScr.nClasss[j].skillTemplates[k].skills[l].dx = d.readShort();
						GameScr.nClasss[j].skillTemplates[k].skills[l].dy = d.readShort();
						GameScr.nClasss[j].skillTemplates[k].skills[l].maxFight = d.readByte();
						GameScr.nClasss[j].skillTemplates[k].skills[l].damage = d.readShort();
						GameScr.nClasss[j].skillTemplates[k].skills[l].price = d.readShort();
						GameScr.nClasss[j].skillTemplates[k].skills[l].moreInfo = d.readUTF();
						Skills.add(GameScr.nClasss[j].skillTemplates[k].skills[l]);
					}
				}
			}
		}

		private void createMap(myReader d)
		{
			GameScr.vcMap = d.readByte();
			TileMap.mapNames = new string[d.readUnsignedByte()];
			for (int i = 0; i < TileMap.mapNames.Length; i++)
			{
				TileMap.mapNames[i] = d.readUTF();
			}
			Npc.arrNpcTemplate = new NpcTemplate[d.readByte()];
			for (sbyte b = 0; b < Npc.arrNpcTemplate.Length; b++)
			{
				Npc.arrNpcTemplate[b] = new NpcTemplate();
				Npc.arrNpcTemplate[b].npcTemplateId = b;
				Npc.arrNpcTemplate[b].name = d.readUTF();
				Npc.arrNpcTemplate[b].headId = d.readShort();
				Npc.arrNpcTemplate[b].bodyId = d.readShort();
				Npc.arrNpcTemplate[b].legId = d.readShort();
				Npc.arrNpcTemplate[b].menu = new string[d.readByte()][];
				for (int j = 0; j < Npc.arrNpcTemplate[b].menu.Length; j++)
				{
					Npc.arrNpcTemplate[b].menu[j] = new string[d.readByte()];
					for (int k = 0; k < Npc.arrNpcTemplate[b].menu[j].Length; k++)
					{
						Npc.arrNpcTemplate[b].menu[j][k] = d.readUTF();
					}
				}
			}
			Mob.arrMobTemplate = new MobTemplate[d.readByte()];
			for (sbyte b2 = 0; b2 < Mob.arrMobTemplate.Length; b2++)
			{
				Mob.arrMobTemplate[b2] = new MobTemplate();
				Mob.arrMobTemplate[b2].mobTemplateId = b2;
				Mob.arrMobTemplate[b2].type = d.readByte();
				Mob.arrMobTemplate[b2].name = d.readUTF();
				Mob.arrMobTemplate[b2].hp = d.readInt();
				Mob.arrMobTemplate[b2].rangeMove = d.readByte();
				Mob.arrMobTemplate[b2].speed = d.readByte();
				Mob.arrMobTemplate[b2].dartType = d.readByte();
			}
		}

		private void createData(myReader d, bool isSaveRMS)
		{
			GameScr.vcData = d.readByte();
			if (isSaveRMS)
			{
				Rms.saveRMS("NR_dart", NinjaUtil.readByteArray(d));
				Rms.saveRMS("NR_arrow", NinjaUtil.readByteArray(d));
				Rms.saveRMS("NR_effect", NinjaUtil.readByteArray(d));
				Rms.saveRMS("NR_image", NinjaUtil.readByteArray(d));
				Rms.saveRMS("NR_part", NinjaUtil.readByteArray(d));
				Rms.saveRMS("NR_skill", NinjaUtil.readByteArray(d));
				Rms.DeleteStorage("NRdata");
			}
		}

		private Image createImage(sbyte[] arr)
		{
			try
			{
				return Image.createImage(arr, 0, arr.Length);
			}
			catch (Exception)
			{
			}
			return null;
		}

		public void readClanMsg(Message msg, int index)
		{
			try
			{
				ClanMessage clanMessage = new ClanMessage();
				sbyte b = (sbyte)(clanMessage.type = msg.reader().readByte());
				clanMessage.id = msg.reader().readInt();
				clanMessage.playerId = msg.reader().readInt();
				clanMessage.playerName = msg.reader().readUTF();
				clanMessage.role = msg.reader().readByte();
				clanMessage.time = msg.reader().readInt() + 1000000000;
				bool flag = false;
				GameScr.isNewClanMessage = false;
				switch (b)
				{
				case 0:
				case 4:
				{
					string text = msg.reader().readUTF();
					GameScr.isNewClanMessage = true;
					if (mFont.tahoma_7.getWidth(text) > Panel.WIDTH_PANEL - 60)
					{
						clanMessage.chat = mFont.tahoma_7.splitFontArray(text, Panel.WIDTH_PANEL - 10);
					}
					else
					{
						clanMessage.chat = new string[1];
						clanMessage.chat[0] = text;
					}
					clanMessage.color = msg.reader().readByte();
					if (b == 4 && clanMessage.playerId != Char.myCharz().charID)
					{
						clanMessage.option = new string[1] { "Giúp tưới" };
					}
					break;
				}
				case 1:
					clanMessage.recieve = msg.reader().readByte();
					clanMessage.maxCap = msg.reader().readByte();
					flag = msg.reader().readByte() == 1;
					if (flag)
					{
						GameScr.isNewClanMessage = true;
					}
					if (clanMessage.playerId != Char.myCharz().charID)
					{
						if (clanMessage.recieve < clanMessage.maxCap)
						{
							clanMessage.option = new string[1] { mResources.donate };
						}
						else
						{
							clanMessage.option = null;
						}
					}
					if (GameCanvas.panel.cp != null)
					{
						GameCanvas.panel.updateRequest(clanMessage.recieve, clanMessage.maxCap);
					}
					break;
				case 2:
					if (Char.myCharz().role == 0)
					{
						GameScr.isNewClanMessage = true;
						clanMessage.option = new string[2]
						{
							mResources.CANCEL,
							mResources.receive
						};
					}
					break;
				}
				if (GameCanvas.currentScreen != GameScr.instance)
				{
					GameScr.isNewClanMessage = false;
				}
				else if (GameCanvas.panel.isShow && GameCanvas.panel.type == 0 && GameCanvas.panel.currentTabIndex == 3)
				{
					GameScr.isNewClanMessage = false;
				}
				ClanMessage.addMessage(clanMessage, index, flag);
			}
			catch (Exception)
			{
			}
		}

		public void loadCurrMap(sbyte teleport3)
		{
			GameScr.gI().auto = 0;
			GameScr.isChangeZone = false;
			CreateCharScr.instance = null;
			GameScr.info1.isUpdate = false;
			GameScr.info2.isUpdate = false;
			GameScr.lockTick = 0;
			GameCanvas.panel.isShow = false;
			SoundMn.gI().stopAll();
			if (!GameScr.isLoadAllData && !CreateCharScr.isCreateChar)
			{
				GameScr.gI().initSelectChar();
			}
			GameScr.loadCamera(fullmScreen: false, (teleport3 != 1) ? (-1) : Char.myCharz().cx, (teleport3 == 0) ? (-1) : 0);
			TileMap.loadMainTile();
			TileMap.loadMap(TileMap.tileID);
			Char.myCharz().cvx = 0;
			Char.myCharz().statusMe = 4;
			Char.myCharz().currentMovePoint = null;
			Char.myCharz().mobFocus = null;
			Char.myCharz().charFocus = null;
			Char.myCharz().npcFocus = null;
			Char.myCharz().itemFocus = null;
			Char.myCharz().skillPaint = null;
			Char.myCharz().setMabuHold(m: false);
			Char.myCharz().skillPaintRandomPaint = null;
			GameCanvas.clearAllPointerEvent();
			if (Char.myCharz().cy >= TileMap.pxh - 100)
			{
				Char.myCharz().isFlyUp = true;
				Char.myCharz().cx += Res.abs(Res.random(0, 80));
				Service.gI().charMove();
			}
			GameScr.gI().loadGameScr();
			GameCanvas.loadBG(TileMap.bgID);
			Char.isLockKey = false;
			for (int i = 0; i < Char.myCharz().vEff.size(); i++)
			{
				if (((EffectChar)Char.myCharz().vEff.elementAt(i)).template.type == 10)
				{
					Char.isLockKey = true;
					break;
				}
			}
			GameCanvas.clearKeyHold();
			GameCanvas.clearKeyPressed();
			GameScr.gI().dHP = Char.myCharz().cHP;
			GameScr.gI().dMP = Char.myCharz().cMP;
			Char.ischangingMap = false;
			GameScr.gI().switchToMe();
			if (Char.myCharz().cy <= 10 && teleport3 != 0 && teleport3 != 2)
			{
				Teleport.addTeleport(new Teleport(Char.myCharz().cx, Char.myCharz().cy, Char.myCharz().head, Char.myCharz().cdir, 1, isMe: true, (teleport3 != 1) ? teleport3 : Char.myCharz().cgender));
				Char.myCharz().isTeleport = true;
			}
			if (teleport3 == 2)
			{
				Char.myCharz().show();
			}
			if (GameScr.gI().isRongThanXuatHien)
			{
				if (TileMap.mapID == GameScr.gI().mapRID && TileMap.zoneID == GameScr.gI().zoneRID)
				{
					GameScr.gI().callRongThan(GameScr.gI().xR, GameScr.gI().yR);
				}
				if (mGraphics.zoomLevel > 1)
				{
					GameScr.gI().doiMauTroi();
				}
			}
			InfoDlg.hide();
			InfoDlg.show(TileMap.mapName, mResources.zone + " " + TileMap.zoneID, 30);
			GameCanvas.endDlg();
			GameCanvas.isLoading = false;
			Hint.clickMob();
			Hint.clickNpc();
		}

		public void loadInfoMap(Message msg)
		{
			try
			{
				if (mGraphics.zoomLevel == 1)
				{
					SmallImage.clearHastable();
				}
				Char.myCharz().cx = (Char.myCharz().cxSend = (Char.myCharz().cxFocus = msg.reader().readShort()));
				Char.myCharz().cy = (Char.myCharz().cySend = (Char.myCharz().cyFocus = msg.reader().readShort()));
				Char.myCharz().xSd = Char.myCharz().cx;
				Char.myCharz().ySd = Char.myCharz().cy;
				if (Char.myCharz().cx >= 0 && Char.myCharz().cx <= 100)
				{
					Char.myCharz().cdir = 1;
				}
				else if (Char.myCharz().cx >= TileMap.tmw - 100 && Char.myCharz().cx <= TileMap.tmw)
				{
					Char.myCharz().cdir = -1;
				}
				int num = msg.reader().readByte();
				if (!GameScr.info1.isDone)
				{
					GameScr.info1.cmx = Char.myCharz().cx - GameScr.cmx;
					GameScr.info1.cmy = Char.myCharz().cy - GameScr.cmy;
				}
				for (int i = 0; i < num; i++)
				{
					Waypoint waypoint = new Waypoint(msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readUTF());
					if ((TileMap.mapID == 21 || TileMap.mapID == 22 || TileMap.mapID == 23) && waypoint.minX >= 0)
					{
						_ = waypoint.minX;
					}
				}
				Resources.UnloadUnusedAssets();
				GC.Collect();
				num = msg.reader().readByte();
				Mob.newMob.removeAllElements();
				for (sbyte b = 0; b < num; b++)
				{
					Mob mob = new Mob(b, msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readByte(), msg.reader().readByte(), msg.readLong(), msg.reader().readByte(), msg.readLong(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readByte(), msg.reader().readByte());
					mob.xSd = mob.x;
					mob.ySd = mob.y;
					mob.isBoss = msg.reader().readBoolean();
					if (Mob.arrMobTemplate[mob.templateId].type != 0)
					{
						if (b % 3 == 0)
						{
							mob.dir = -1;
						}
						else
						{
							mob.dir = 1;
						}
						mob.x += 10 - b % 20;
					}
					mob.isMobMe = false;
					BigBoss bigBoss = null;
					BachTuoc bachTuoc = null;
					BigBoss2 bigBoss2 = null;
					NewBoss newBoss = null;
					if (mob.templateId == 70)
					{
						bigBoss = new BigBoss(b, (short)mob.x, (short)mob.y, 70, mob.hp, mob.maxHp, mob.sys);
					}
					if (mob.templateId == 71)
					{
						bachTuoc = new BachTuoc(b, (short)mob.x, (short)mob.y, 71, mob.hp, mob.maxHp, mob.sys);
					}
					if (mob.templateId == 72)
					{
						bigBoss2 = new BigBoss2(b, (short)mob.x, (short)mob.y, 72, mob.hp, mob.maxHp, 3);
					}
					if (mob.isBoss)
					{
						newBoss = new NewBoss(b, (short)mob.x, (short)mob.y, mob.templateId, mob.hp, mob.maxHp, mob.sys);
					}
					if (newBoss != null)
					{
						GameScr.vMob.addElement(newBoss);
					}
					else if (bigBoss != null)
					{
						GameScr.vMob.addElement(bigBoss);
					}
					else if (bachTuoc != null)
					{
						GameScr.vMob.addElement(bachTuoc);
					}
					else if (bigBoss2 != null)
					{
						GameScr.vMob.addElement(bigBoss2);
					}
					else
					{
						GameScr.vMob.addElement(mob);
					}
				}
				if (Char.myCharz().mobMe != null && GameScr.findMobInMap(Char.myCharz().mobMe.mobId) == null)
				{
					Char.myCharz().mobMe.getData();
					Char.myCharz().mobMe.x = Char.myCharz().cx;
					Char.myCharz().mobMe.y = Char.myCharz().cy - 40;
					GameScr.vMob.addElement(Char.myCharz().mobMe);
				}
				num = msg.reader().readByte();
				for (byte b2 = 0; b2 < num; b2++)
				{
				}
				num = msg.reader().readByte();
				for (int j = 0; j < num; j++)
				{
					sbyte status = msg.reader().readByte();
					short cx = msg.reader().readShort();
					short num2 = msg.reader().readShort();
					sbyte b3 = msg.reader().readByte();
					short num3 = msg.reader().readShort();
					if (b3 != 6 && ((Char.myCharz().taskMaint.taskId >= 7 && (Char.myCharz().taskMaint.taskId != 7 || Char.myCharz().taskMaint.index > 1)) || (b3 != 7 && b3 != 8 && b3 != 9)) && (Char.myCharz().taskMaint.taskId >= 6 || b3 != 16))
					{
						if (b3 == 4)
						{
							GameScr.gI().magicTree = new MagicTree(j, status, cx, num2, b3, num3);
							Service.gI().magicTree(2);
							GameScr.vNpc.addElement(GameScr.gI().magicTree);
						}
						else
						{
							Npc o = new Npc(j, status, cx, num2 + 3, b3, num3);
							GameScr.vNpc.addElement(o);
						}
					}
				}
				num = msg.reader().readByte();
				string empty = string.Empty;
				empty = empty + "item: " + num;
				for (int k = 0; k < num; k++)
				{
					short itemMapID = msg.reader().readShort();
					short itemTemplateID = msg.reader().readShort();
					int x = msg.reader().readShort();
					int y = msg.reader().readShort();
					int num4 = msg.reader().readInt();
					short r = 0;
					if (num4 == -2)
					{
						r = msg.reader().readShort();
					}
					ItemMap itemMap = new ItemMap(num4, itemMapID, itemTemplateID, x, y, r);
					bool flag = false;
					for (int l = 0; l < GameScr.vItemMap.size(); l++)
					{
						if (((ItemMap)GameScr.vItemMap.elementAt(l)).itemMapID == itemMap.itemMapID)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						GameScr.vItemMap.addElement(itemMap);
					}
					empty = empty + itemTemplateID + ",";
				}
				TileMap.vCurrItem.removeAllElements();
				if (mGraphics.zoomLevel == 1)
				{
					BgItem.clearHashTable();
				}
				BgItem.vKeysNew.removeAllElements();
				if (!GameCanvas.lowGraphic || (GameCanvas.lowGraphic && TileMap.isVoDaiMap()) || TileMap.mapID == 45 || TileMap.mapID == 46 || TileMap.mapID == 47 || TileMap.mapID == 48)
				{
					short num5 = msg.reader().readShort();
					empty = "item high graphic: ";
					for (int m = 0; m < num5; m++)
					{
						short id = msg.reader().readShort();
						short num6 = msg.reader().readShort();
						short num7 = msg.reader().readShort();
						if (TileMap.getBIById(id) != null)
						{
							BgItem bIById = TileMap.getBIById(id);
							BgItem bgItem = new BgItem();
							bgItem.id = id;
							bgItem.idImage = bIById.idImage;
							bgItem.dx = bIById.dx;
							bgItem.dy = bIById.dy;
							bgItem.x = num6 * TileMap.size;
							bgItem.y = num7 * TileMap.size;
							bgItem.layer = bIById.layer;
							if (TileMap.isExistMoreOne(bgItem.id))
							{
								bgItem.trans = ((m % 2 != 0) ? 2 : 0);
								if (TileMap.mapID == 45)
								{
									bgItem.trans = 0;
								}
							}
							if (!BgItem.imgNew.containsKey(bgItem.idImage + string.Empty))
							{
								if (mGraphics.zoomLevel == 1)
								{
									Image image = GameCanvas.loadImage("/mapBackGround/" + bgItem.idImage + ".png");
									if (image == null)
									{
										image = Image.createRGBImage(new int[1], 1, 1, bl: true);
										Service.gI().getBgTemplate(bgItem.idImage);
									}
									BgItem.imgNew.put(bgItem.idImage + string.Empty, image);
								}
								else
								{
									bool flag2 = false;
									sbyte[] array = Rms.loadRMS(mGraphics.zoomLevel + "bgItem" + bgItem.idImage);
									if (array != null)
									{
										if (BgItem.newSmallVersion != null && array.Length % 127 != BgItem.newSmallVersion[bgItem.idImage])
										{
											flag2 = true;
										}
										if (!flag2)
										{
											Image image2 = Image.createImage(array, 0, array.Length);
											if (image2 != null)
											{
												BgItem.imgNew.put(bgItem.idImage + string.Empty, image2);
											}
											else
											{
												flag2 = true;
											}
										}
									}
									else
									{
										flag2 = true;
									}
									if (flag2)
									{
										Image image3 = GameCanvas.loadImage("/mapBackGround/" + bgItem.idImage + ".png");
										if (image3 == null)
										{
											image3 = Image.createRGBImage(new int[1], 1, 1, bl: true);
											Service.gI().getBgTemplate(bgItem.idImage);
										}
										BgItem.imgNew.put(bgItem.idImage + string.Empty, image3);
									}
								}
								BgItem.vKeysLast.addElement(bgItem.idImage + string.Empty);
							}
							if (!BgItem.isExistKeyNews(bgItem.idImage + string.Empty))
							{
								BgItem.vKeysNew.addElement(bgItem.idImage + string.Empty);
							}
							bgItem.changeColor();
							TileMap.vCurrItem.addElement(bgItem);
						}
						empty = empty + id + ",";
					}
					for (int n = 0; n < BgItem.vKeysLast.size(); n++)
					{
						string text = (string)BgItem.vKeysLast.elementAt(n);
						if (!BgItem.isExistKeyNews(text))
						{
							BgItem.imgNew.remove(text);
							if (BgItem.imgNew.containsKey(text + "blend" + 1))
							{
								BgItem.imgNew.remove(text + "blend" + 1);
							}
							if (BgItem.imgNew.containsKey(text + "blend" + 3))
							{
								BgItem.imgNew.remove(text + "blend" + 3);
							}
							BgItem.vKeysLast.removeElementAt(n);
							n--;
						}
					}
					BackgroudEffect.isFog = false;
					BackgroudEffect.nCloud = 0;
					EffecMn.vEff.removeAllElements();
					BackgroudEffect.vBgEffect.removeAllElements();
					Effect.newEff.removeAllElements();
					short num8 = msg.reader().readShort();
					for (int num9 = 0; num9 < num8; num9++)
					{
						string key = msg.reader().readUTF();
						string value = msg.reader().readUTF();
						keyValueAction(key, value);
					}
				}
				else
				{
					short num10 = msg.reader().readShort();
					for (int num11 = 0; num11 < num10; num11++)
					{
						msg.reader().readShort();
						msg.reader().readShort();
						msg.reader().readShort();
					}
					short num12 = msg.reader().readShort();
					for (int num13 = 0; num13 < num12; num13++)
					{
						msg.reader().readUTF();
						msg.reader().readUTF();
					}
				}
				TileMap.bgType = msg.reader().readByte();
				sbyte teleport = msg.reader().readByte();
				loadCurrMap(teleport);
				Char.isLoadingMap = false;
				Resources.UnloadUnusedAssets();
				GC.Collect();
				ModFunc.GI().canUpdate = true;
			}
			catch (Exception)
			{
				AutoXmap.FixBlackScreen();
			}
		}

		public void LoadAuraNpcs(Message msg)
		{
			sbyte b = msg.reader().readByte();
			for (sbyte b2 = 0; b2 < b; b2++)
			{
				sbyte tempId = msg.reader().readByte();
				short idAura = msg.reader().readShort();
				Npc npcByTempId = ModFunc.GetNpcByTempId(tempId);
				if (npcByTempId != null)
				{
					npcByTempId.idAura = idAura;
				}
			}
		}

		public void keyValueAction(string key, string value)
		{
			if (!key.Equals("eff"))
			{
				if (key.Equals("beff") && Panel.graphics <= 1)
				{
					BackgroudEffect.addEffect(int.Parse(value));
				}
			}
			else
			{
				if (Panel.graphics > 0)
				{
					return;
				}
				string[] array = Res.split(value, ".", 0);
				int id = int.Parse(array[0]);
				int layer = int.Parse(array[1]);
				int x = int.Parse(array[2]);
				int y = int.Parse(array[3]);
				int loop;
				int loopCount;
				if (array.Length <= 4)
				{
					loop = -1;
					loopCount = 1;
				}
				else
				{
					loop = int.Parse(array[4]);
					loopCount = int.Parse(array[5]);
				}
				Effect effect = new Effect(id, x, y, layer, loop, loopCount);
				if (array.Length > 6)
				{
					effect.typeEff = int.Parse(array[6]);
					if (array.Length > 7)
					{
						effect.indexFrom = int.Parse(array[7]);
						effect.indexTo = int.Parse(array[8]);
					}
				}
				EffecMn.addEff(effect);
			}
		}

		public void messageNotMap(Message msg)
		{
			try
			{
				switch (msg.reader().readByte())
				{
				case 4:
				{
					GameCanvas.loginScr.savePass();
					GameScr.isAutoPlay = false;
					GameScr.canAutoPlay = false;
					LoginScr.isUpdateAll = true;
					LoginScr.isUpdateData = true;
					LoginScr.isUpdateMap = true;
					LoginScr.isUpdateSkill = true;
					LoginScr.isUpdateItem = true;
					GameScr.vsData = msg.reader().readByte();
					GameScr.vsMap = msg.reader().readByte();
					GameScr.vsSkill = msg.reader().readByte();
					GameScr.vsItem = msg.reader().readByte();
					msg.reader().readByte();
					if (GameCanvas.loginScr.isLogin2)
					{
						Rms.saveRMSString("acc", string.Empty);
						Rms.saveRMSString("pass", string.Empty);
					}
					else
					{
						Rms.saveRMSString("userAo" + ServerListScreen.ipSelect, string.Empty);
					}
					if (GameScr.vsData != GameScr.vcData)
					{
						GameScr.isLoadAllData = false;
						Service.gI().updateData();
					}
					else
					{
						try
						{
							LoginScr.isUpdateData = false;
						}
						catch (Exception)
						{
							GameScr.vcData = -1;
							Service.gI().updateData();
						}
					}
					if (GameScr.vsMap != GameScr.vcMap)
					{
						GameScr.isLoadAllData = false;
						Service.gI().updateMap();
					}
					else
					{
						try
						{
							if (!GameScr.isLoadAllData)
							{
								DataInputStream dataInputStream = new DataInputStream(Rms.loadRMS("NRmap"));
								createMap(dataInputStream.r);
							}
							LoginScr.isUpdateMap = false;
						}
						catch (Exception)
						{
							GameScr.vcMap = -1;
							Service.gI().updateMap();
						}
					}
					if (GameScr.vsSkill != GameScr.vcSkill)
					{
						GameScr.isLoadAllData = false;
						Service.gI().updateSkill();
					}
					else
					{
						try
						{
							if (!GameScr.isLoadAllData)
							{
								DataInputStream dataInputStream2 = new DataInputStream(Rms.loadRMS("NRskill"));
								createSkill(dataInputStream2.r);
							}
							LoginScr.isUpdateSkill = false;
						}
						catch (Exception)
						{
							GameScr.vcSkill = -1;
							Service.gI().updateSkill();
						}
					}
					if (GameScr.vsItem != GameScr.vcItem)
					{
						GameScr.isLoadAllData = false;
						Service.gI().updateItem();
					}
					else
					{
						try
						{
							DataInputStream dataInputStream3 = new DataInputStream(Rms.loadRMS("NRitem0"));
							loadItemNew(dataInputStream3.r, 0, isSave: false);
							DataInputStream dataInputStream4 = new DataInputStream(Rms.loadRMS("NRitem1"));
							loadItemNew(dataInputStream4.r, 1, isSave: false);
							sbyte[] appendCountData = Rms.loadRMS("NRitem2Count");
							if (appendCountData != null && appendCountData.Length == 1)
							{
								int appendCount = appendCountData[0] & 0xFF;
								for (int appendIndex = 0; appendIndex < appendCount; appendIndex++)
								{
									DataInputStream appendData = new DataInputStream(Rms.loadRMS("NRitem2_" + appendIndex));
									loadItemNew(appendData.r, 2, isSave: false);
								}
							}
							else
							{
								// Backward compatibility for the old one-append cache layout.
								DataInputStream dataInputStream5 = new DataInputStream(Rms.loadRMS("NRitem2"));
								loadItemNew(dataInputStream5.r, 2, isSave: false);
							}
							DataInputStream dataInputStream6 = new DataInputStream(Rms.loadRMS("NRitem100"));
							loadItemNew(dataInputStream6.r, 100, isSave: false);
							LoginScr.isUpdateItem = false;
						}
						catch (Exception)
						{
							GameScr.vcItem = -1;
							Service.gI().updateItem();
						}
					}
					if (GameScr.vsData == GameScr.vcData && GameScr.vsMap == GameScr.vcMap && GameScr.vsSkill == GameScr.vcSkill && GameScr.vsItem == GameScr.vcItem)
					{
						if (!GameScr.isLoadAllData)
						{
							GameScr.gI().readDart();
							GameScr.gI().readEfect();
							GameScr.gI().readArrow();
							GameScr.gI().readSkill();
						}
						Service.gI().clientOk();
					}
					GameScr.exps = new long[msg.reader().readByte()];
					for (int j = 0; j < GameScr.exps.Length; j++)
					{
						GameScr.exps[j] = msg.reader().readLong();
					}
					break;
				}
				case 6:
				{
					msg.reader().mark(100000);
					createMap(msg.reader());
					msg.reader().reset();
					sbyte[] data3 = new sbyte[msg.reader().available()];
					msg.reader().readFully(ref data3);
					Rms.saveRMS("NRmap", data3);
					sbyte[] data4 = new sbyte[1] { GameScr.vcMap };
					Rms.saveRMS("NRmapVersion", data4);
					LoginScr.isUpdateMap = false;
					if (GameScr.vsData == GameScr.vcData && GameScr.vsMap == GameScr.vcMap && GameScr.vsSkill == GameScr.vcSkill && GameScr.vsItem == GameScr.vcItem)
					{
						GameScr.gI().readDart();
						GameScr.gI().readEfect();
						GameScr.gI().readArrow();
						GameScr.gI().readSkill();
						Service.gI().clientOk();
					}
					break;
				}
				case 7:
				{
					msg.reader().mark(100000);
					createSkill(msg.reader());
					msg.reader().reset();
					sbyte[] data = new sbyte[msg.reader().available()];
					msg.reader().readFully(ref data);
					Rms.saveRMS("NRskill", data);
					sbyte[] data2 = new sbyte[1] { GameScr.vcSkill };
					Rms.saveRMS("NRskillVersion", data2);
					LoginScr.isUpdateSkill = false;
					if (GameScr.vsData == GameScr.vcData && GameScr.vsMap == GameScr.vcMap && GameScr.vsSkill == GameScr.vcSkill && GameScr.vsItem == GameScr.vcItem)
					{
						GameScr.gI().readDart();
						GameScr.gI().readEfect();
						GameScr.gI().readArrow();
						GameScr.gI().readSkill();
						Service.gI().clientOk();
					}
					break;
				}
				case 8:
					Res.outz("GET UPDATE_ITEM " + msg.reader().available() + " bytes");
					createItemNew(msg.reader());
					break;
				case 9:
					GameCanvas.debug("SA11", 2);
					break;
				case 10:
					try
					{
						Char.isLoadingMap = true;
						Res.outz("REQUEST MAP TEMPLATE");
						GameCanvas.isLoading = true;
						TileMap.maps = null;
						TileMap.types = null;
						mSystem.gcc();
						GameCanvas.debug("SA99", 2);
						TileMap.tmw = msg.reader().readByte();
						TileMap.tmh = msg.reader().readByte();
						TileMap.maps = new int[TileMap.tmw * TileMap.tmh];
						Res.err("   M apsize= " + TileMap.tmw * TileMap.tmh);
						for (int i = 0; i < TileMap.maps.Length; i++)
						{
							int num2 = msg.reader().readByte();
							if (num2 < 0)
							{
								num2 += 256;
							}
							TileMap.maps[i] = (ushort)num2;
						}
						TileMap.types = new int[TileMap.maps.Length];
						msg = messWait;
						loadInfoMap(msg);
						try
						{
							TileMap.isMapDouble = msg.reader().readByte() != 0;
						}
						catch (Exception ex)
						{
							Res.err(" 1 LOI TAI CASE REQUEST_MAPTEMPLATE " + ex.ToString());
						}
					}
					catch (Exception ex2)
					{
						Res.err("2 LOI TAI CASE REQUEST_MAPTEMPLATE " + ex2.ToString());
					}
					msg.cleanup();
					messWait.cleanup();
					msg = (messWait = null);
					GameScr.gI().switchToMe();
					break;
				case 12:
					GameCanvas.debug("SA10", 2);
					break;
				case 16:
					MoneyCharge.gI().switchToMe();
					break;
				case 17:
					Char.myCharz().clearTask();
					break;
				case 18:
				{
					GameCanvas.isLoading = false;
					GameCanvas.endDlg();
					int num = msg.reader().readInt();
					GameCanvas.inputDlg.show(mResources.changeNameChar, new Command(mResources.OK, GameCanvas.instance, 88829, num), TField.INPUT_TYPE_ANY);
					break;
				}
				case 20:
					Char.myCharz().cPk = msg.reader().readByte();
					GameScr.info1.addInfo(mResources.PK_NOW + " " + Char.myCharz().cPk, 0);
					break;
				case 36:
					GameScr.typeActive = msg.reader().readByte();
					break;
				case 35:
					GameCanvas.endDlg();
					GameScr.gI().resetButton();
					GameScr.info1.addInfo(msg.reader().readUTF(), 0);
					break;
				}
			}
			catch (Exception)
			{
				Cout.LogError("LOI TAI messageNotMap + " + msg.command);
			}
			finally
			{
				msg?.cleanup();
			}
		}

		public void messageNotLogin(Message msg)
		{
			try
			{
				if (msg.reader().readByte() != 2)
				{
					return;
				}
				string linkDefault = msg.reader().readUTF();
				try
				{
					Panel.CanNapTien = msg.reader().readByte() == 1;
					sbyte x = msg.reader().readByte();
				}
				catch (Exception)
				{
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				msg?.cleanup();
			}
		}

		public void messageSubCommand(Message msg)
		{
			try
			{
				switch (msg.reader().readByte())
				{
				case 0:
				{
					RadarScr.list = new MyVector();
					Teleport.vTeleport.removeAllElements();
					GameScr.vCharInMap.removeAllElements();
					GameScr.vItemMap.removeAllElements();
					Char.vItemTime.removeAllElements();
					GameScr.loadImg();
					GameScr.currentCharViewInfo = Char.myCharz();
					Char.myCharz().charID = msg.reader().readInt();
					Char.myCharz().ctaskId = msg.reader().readByte();
					Char.myCharz().cgender = msg.reader().readByte();
					Char.myCharz().head = msg.reader().readShort();
					Char.myCharz().cName = msg.reader().readUTF();
					Char.myCharz().cPk = msg.reader().readByte();
					Char.myCharz().cTypePk = msg.reader().readByte();
					Char.myCharz().cPower = msg.reader().readLong();
					Char.myCharz().applyCharLevelPercent();
					Char.myCharz().eff5BuffHp = msg.reader().readShort();
					Char.myCharz().eff5BuffMp = msg.reader().readShort();
					Char.myCharz().nClass = GameScr.nClasss[msg.reader().readByte()];
					Char.myCharz().vSkill.removeAllElements();
					Char.myCharz().vSkillFight.removeAllElements();
					GameScr.gI().dHP = Char.myCharz().cHP;
					GameScr.gI().dMP = Char.myCharz().cMP;
					sbyte b2 = msg.reader().readByte();
					for (sbyte b3 = 0; b3 < b2; b3++)
					{
						Skill skill = Skills.get(msg.reader().readShort());
						useSkill(skill);
					}
					GameScr.gI().sortSkill();
					GameScr.gI().loadSkillShortcut();
					Char.myCharz().xu = msg.reader().readLong();
					Char.myCharz().luongKhoa = msg.reader().readInt();
					Char.myCharz().luong = msg.reader().readInt();
					Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
					Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
					Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
					Char.myCharz().arrItemBody = new Item[msg.reader().readByte()];
					try
					{
						Char.myCharz().setDefaultPart();
						for (int j = 0; j < Char.myCharz().arrItemBody.Length; j++)
						{
							short num2 = msg.reader().readShort();
							if (num2 == -1)
							{
								continue;
							}
							ItemTemplate itemTemplate = ItemTemplates.get(num2);
							int type = itemTemplate.type;
							Char.myCharz().arrItemBody[j] = new Item();
							Char.myCharz().arrItemBody[j].template = itemTemplate;
							Char.myCharz().arrItemBody[j].quantity = msg.reader().readInt();
							Char.myCharz().arrItemBody[j].info = msg.reader().readUTF();
							Char.myCharz().arrItemBody[j].content = msg.reader().readUTF();
							int num3 = msg.reader().readUnsignedByte();
							if (num3 != 0)
							{
								Char.myCharz().arrItemBody[j].itemOption = new ItemOption[num3];
								for (int k = 0; k < Char.myCharz().arrItemBody[j].itemOption.Length; k++)
								{
									int num4 = msg.reader().readUnsignedByte();
									int param = msg.reader().readUnsignedShort();
									if (num4 != -1)
									{
										Char.myCharz().arrItemBody[j].itemOption[k] = new ItemOption(num4, param);
									}
								}
							}
							switch (type)
							{
							case 1:
								Char.myCharz().leg = Char.myCharz().arrItemBody[j].template.part;
								break;
							case 0:
								Char.myCharz().body = Char.myCharz().arrItemBody[j].template.part;
								break;
							}
						}
					}
					catch (Exception)
					{
					}
					Char.myCharz().arrItemBag = new Item[msg.reader().readByte()];
					GameScr.hpPotion = 0;
					for (int l = 0; l < Char.myCharz().arrItemBag.Length; l++)
					{
						short num5 = msg.reader().readShort();
						if (num5 == -1)
						{
							continue;
						}
						Char.myCharz().arrItemBag[l] = new Item();
						Char.myCharz().arrItemBag[l].template = ItemTemplates.get(num5);
						Char.myCharz().arrItemBag[l].quantity = msg.reader().readInt();
						Char.myCharz().arrItemBag[l].info = msg.reader().readUTF();
						Char.myCharz().arrItemBag[l].content = msg.reader().readUTF();
						Char.myCharz().arrItemBag[l].indexUI = l;
						sbyte b4 = msg.reader().readByte();
						if (b4 != 0)
						{
							Char.myCharz().arrItemBag[l].itemOption = new ItemOption[b4];
							for (int m = 0; m < Char.myCharz().arrItemBag[l].itemOption.Length; m++)
							{
								int num6 = msg.reader().readUnsignedByte();
								int param2 = msg.reader().readUnsignedShort();
								if (num6 != -1)
								{
									Char.myCharz().arrItemBag[l].itemOption[m] = new ItemOption(num6, param2);
									Char.myCharz().arrItemBag[l].getCompare();
								}
							}
						}
						if (Char.myCharz().arrItemBag[l].template.type == 6)
						{
							GameScr.hpPotion += Char.myCharz().arrItemBag[l].quantity;
						}
					}
					Char.myCharz().arrItemBox = new Item[msg.reader().readByte()];
					GameCanvas.panel.hasUse = 0;
					for (int n = 0; n < Char.myCharz().arrItemBox.Length; n++)
					{
						short num7 = msg.reader().readShort();
						if (num7 == -1)
						{
							continue;
						}
						Char.myCharz().arrItemBox[n] = new Item();
						Char.myCharz().arrItemBox[n].template = ItemTemplates.get(num7);
						Char.myCharz().arrItemBox[n].quantity = msg.reader().readInt();
						Char.myCharz().arrItemBox[n].info = msg.reader().readUTF();
						Char.myCharz().arrItemBox[n].content = msg.reader().readUTF();
						Char.myCharz().arrItemBox[n].itemOption = new ItemOption[msg.reader().readByte()];
						for (int num8 = 0; num8 < Char.myCharz().arrItemBox[n].itemOption.Length; num8++)
						{
							int num9 = msg.reader().readUnsignedByte();
							int param3 = msg.reader().readUnsignedShort();
							if (num9 != -1)
							{
								Char.myCharz().arrItemBox[n].itemOption[num8] = new ItemOption(num9, param3);
								Char.myCharz().arrItemBox[n].getCompare();
							}
						}
						GameCanvas.panel.hasUse++;
					}
					Char.myCharz().statusMe = 4;
					if (Rms.loadRMSInt(Char.myCharz().cName + "vci") < 1)
					{
						GameScr.isViewClanInvite = false;
					}
					else
					{
						GameScr.isViewClanInvite = true;
					}
					short num10 = msg.reader().readShort();
					Char.idHead = new short[num10];
					Char.idAvatar = new short[num10];
					for (int num11 = 0; num11 < num10; num11++)
					{
						Char.idHead[num11] = msg.reader().readShort();
						Char.idAvatar[num11] = msg.reader().readShort();
					}
					for (int num12 = 0; num12 < GameScr.info1.charId.Length; num12++)
					{
						GameScr.info1.charId[num12] = new int[3];
					}
					GameScr.info1.charId[Char.myCharz().cgender][0] = msg.reader().readShort();
					GameScr.info1.charId[Char.myCharz().cgender][1] = msg.reader().readShort();
					GameScr.info1.charId[Char.myCharz().cgender][2] = msg.reader().readShort();
					Char.myCharz().isNhapThe = msg.reader().readByte() == 1;
					GameScr.deltaTime = mSystem.currentTimeMillis() - (long)msg.reader().readInt() * 1000L;
					GameScr.isNewMember = msg.reader().readByte();
					Char.myCharz().isTichXanh = GameScr.isNewMember == 1;
					Service.gI().updateCaption((sbyte)Char.myCharz().cgender);
					Service.gI().androidPack();
					try
					{
						Char.myCharz().idAuraEff = msg.reader().readShort();
						Char.myCharz().idEff_Set_Item = msg.reader().readSByte();
						Char.myCharz().idHat = msg.reader().readShort();
						break;
					}
					catch (Exception)
					{
						break;
					}
				}
				case 1:
					GameCanvas.debug("SA13", 2);
					Char.myCharz().nClass = GameScr.nClasss[msg.reader().readByte()];
					Char.myCharz().cTiemNang = msg.reader().readLong();
					Char.myCharz().vSkill.removeAllElements();
					Char.myCharz().vSkillFight.removeAllElements();
					Char.myCharz().myskill = null;
					break;
				case 2:
				{
					GameCanvas.debug("SA14", 2);
					if (Char.myCharz().statusMe != 14 && Char.myCharz().statusMe != 5)
					{
						Char.myCharz().cHP = Char.myCharz().cHPFull;
						Char.myCharz().cMP = Char.myCharz().cMPFull;
						Cout.LogError2(" ME_LOAD_SKILL");
					}
					Char.myCharz().vSkill.removeAllElements();
					Char.myCharz().vSkillFight.removeAllElements();
					sbyte b5 = msg.reader().readByte();
					for (sbyte b6 = 0; b6 < b5; b6++)
					{
						Skill skill2 = Skills.get(msg.reader().readShort());
						useSkill(skill2);
					}
					GameScr.gI().sortSkill();
					if (GameScr.isPaintInfoMe)
					{
						GameScr.indexRow = -1;
						GameScr.gI().left = (GameScr.gI().center = null);
					}
					break;
				}
				case 5:
				{
					long cHP = Char.myCharz().cHP;
					Char.myCharz().cHP = msg.readLong();
					if (Char.myCharz().cHP > cHP && Char.myCharz().cTypePk != 4)
					{
						GameScr.startFlyText("+" + (Char.myCharz().cHP - cHP) + " " + mResources.HP, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 20, 0, -1, mFont.HP);
						SoundMn.gI().HP_MPup();
						if (Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 5003)
						{
							MonsterDart.addMonsterDart(Char.myCharz().petFollow.cmx + ((Char.myCharz().petFollow.dir != 1) ? (-10) : 10), Char.myCharz().petFollow.cmy + 10, isBoss: true, -1L, -1L, Char.myCharz(), 29);
						}
					}
					if (Char.myCharz().cHP < cHP)
					{
						GameScr.startFlyText("-" + (cHP - Char.myCharz().cHP) + " " + mResources.HP, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 20, 0, -1, mFont.HP);
					}
					GameScr.gI().dHP = Char.myCharz().cHP;
					if (!GameScr.isPaintInfoMe)
					{
					}
					break;
				}
				case 6:
				{
					if (Char.myCharz().statusMe == 14 || Char.myCharz().statusMe == 5)
					{
						break;
					}
					long cMP = Char.myCharz().cMP;
					Char.myCharz().cMP = msg.readLong();
					if (Char.myCharz().cMP > cMP)
					{
						GameScr.startFlyText("+" + (Char.myCharz().cMP - cMP) + " " + mResources.KI, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 23, 0, -2, mFont.MP);
						SoundMn.gI().HP_MPup();
						if (Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 5001)
						{
							MonsterDart.addMonsterDart(Char.myCharz().petFollow.cmx + ((Char.myCharz().petFollow.dir != 1) ? (-10) : 10), Char.myCharz().petFollow.cmy + 10, isBoss: true, -1L, -1L, Char.myCharz(), 29);
						}
					}
					if (Char.myCharz().cMP < cMP)
					{
						GameScr.startFlyText("-" + (cMP - Char.myCharz().cMP) + " " + mResources.KI, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 23, 0, -2, mFont.MP);
					}
					GameScr.gI().dMP = Char.myCharz().cMP;
					if (!GameScr.isPaintInfoMe)
					{
					}
					break;
				}
				case 7:
				{
					Char char5 = GameScr.findCharInMap(msg.reader().readInt());
					if (char5 == null)
					{
						break;
					}
					char5.clanID = msg.reader().readInt();
					if (char5.clanID == -2)
					{
						char5.isCopy = true;
					}
					readCharInfo(char5, msg);
					try
					{
						char5.idAuraEff = msg.reader().readShort();
						char5.idEff_Set_Item = msg.reader().readSByte();
						char5.idHat = msg.reader().readShort();
						if (char5.bag >= 201)
						{
							char5.addEffChar(new Effect(char5.bag, char5, 2, -1, 10, 1)
							{
								typeEff = 5
							});
						}
						else
						{
							char5.removeEffChar(0, 201);
						}
						break;
					}
					catch (Exception)
					{
						break;
					}
				}
				case 9:
				{
					GameCanvas.debug("SA27", 2);
					Char char10 = GameScr.findCharInMap(msg.reader().readInt());
					if (char10 != null)
					{
						char10.cHP = msg.readLong();
						char10.cHPFull = msg.readLong();
					}
					break;
				}
				case 10:
				{
					GameCanvas.debug("SA28", 2);
					Char char11 = GameScr.findCharInMap(msg.reader().readInt());
					if (char11 != null)
					{
						char11.cHP = msg.readLong();
						char11.cHPFull = msg.readLong();
						char11.eff5BuffHp = msg.reader().readShort();
						char11.eff5BuffMp = msg.reader().readShort();
						char11.wp = msg.reader().readShort();
						if (char11.wp == -1)
						{
							char11.setDefaultWeapon();
						}
					}
					break;
				}
				case 11:
				{
					GameCanvas.debug("SA29", 2);
					Char char3 = GameScr.findCharInMap(msg.reader().readInt());
					if (char3 != null)
					{
						char3.cHP = msg.readLong();
						char3.cHPFull = msg.readLong();
						char3.eff5BuffHp = msg.reader().readShort();
						char3.eff5BuffMp = msg.reader().readShort();
						char3.body = msg.reader().readShort();
						if (char3.body == -1)
						{
							char3.setDefaultBody();
						}
					}
					break;
				}
				case 12:
				{
					GameCanvas.debug("SA30", 2);
					Char char7 = GameScr.findCharInMap(msg.reader().readInt());
					if (char7 != null)
					{
						char7.cHP = msg.readLong();
						char7.cHPFull = msg.readLong();
						char7.eff5BuffHp = msg.reader().readShort();
						char7.eff5BuffMp = msg.reader().readShort();
						char7.leg = msg.reader().readShort();
						if (char7.leg == -1)
						{
							char7.setDefaultLeg();
						}
					}
					break;
				}
				case 13:
				{
					GameCanvas.debug("SA31", 2);
					int num13 = msg.reader().readInt();
					Char char6 = ((num13 != Char.myCharz().charID) ? GameScr.findCharInMap(num13) : Char.myCharz());
					if (char6 != null)
					{
						char6.cHP = msg.readLong();
						char6.cHPFull = msg.readLong();
						char6.eff5BuffHp = msg.reader().readShort();
						char6.eff5BuffMp = msg.reader().readShort();
					}
					break;
				}
				case 14:
				{
					Char char4 = GameScr.findCharInMap(msg.reader().readInt());
					if (char4 != null)
					{
						char4.cHP = msg.readLong();
						sbyte num = msg.reader().readByte();
						if (num == 1)
						{
							ServerEffect.addServerEffect(11, char4, 5);
							ServerEffect.addServerEffect(104, char4, 4);
						}
						if (num == 2)
						{
							char4.doInjure();
						}
						try
						{
							char4.cHPFull = msg.readLong();
							break;
						}
						catch (Exception)
						{
							break;
						}
					}
					break;
				}
				case 19:
					GameCanvas.debug("SA17", 2);
					Char.myCharz().boxSort();
					break;
				case 21:
				{
					int num17 = msg.reader().readInt();
					Char.myCharz().xuInBox -= num17;
					Char.myCharz().xu += num17;
					Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
					break;
				}
				case 23:
				{
					short num14 = msg.reader().readShort();
					Skill skill3 = Skills.get(num14);
					useSkill(skill3);
					if (num14 != 0 && num14 != 14 && num14 != 28)
					{
						GameScr.info1.addInfo(mResources.LEARN_SKILL + " " + skill3.template.name, 0);
					}
					break;
				}
				case 35:
				{
					GameCanvas.debug("SY3", 2);
					int num15 = msg.reader().readInt();
					Res.outz("CID = " + num15);
					if (TileMap.mapID == 130)
					{
						GameScr.gI().starVS();
					}
					if (num15 == Char.myCharz().charID)
					{
						Char.myCharz().cTypePk = msg.reader().readByte();
						if (GameScr.gI().isVS() && Char.myCharz().cTypePk != 0)
						{
							GameScr.gI().starVS();
						}
						Res.outz("type pk= " + Char.myCharz().cTypePk);
						Char.myCharz().npcFocus = null;
						if (!GameScr.gI().isMeCanAttackMob(Char.myCharz().mobFocus))
						{
							Char.myCharz().mobFocus = null;
						}
						Char.myCharz().itemFocus = null;
					}
					else
					{
						Char char8 = GameScr.findCharInMap(num15);
						if (char8 != null)
						{
							Res.outz("type pk= " + char8.cTypePk);
							char8.cTypePk = msg.reader().readByte();
							if (char8.isAttacPlayerStatus())
							{
								Char.myCharz().charFocus = char8;
							}
						}
					}
					for (int num16 = 0; num16 < GameScr.vCharInMap.size(); num16++)
					{
						Char char9 = GameScr.findCharInMap(num16);
						if (char9 != null && char9.cTypePk != 0 && char9.cTypePk == Char.myCharz().cTypePk)
						{
							if (!Char.myCharz().mobFocus.isMobMe)
							{
								Char.myCharz().mobFocus = null;
							}
							Char.myCharz().npcFocus = null;
							Char.myCharz().itemFocus = null;
							break;
						}
					}
					Res.outz("update type pk= ");
					break;
				}
				case 61:
				{
					string text = msg.reader().readUTF();
					sbyte[] data = new sbyte[msg.reader().readInt()];
					msg.reader().read(ref data);
					if (data.Length == 0)
					{
						data = null;
					}
					if (text.Equals("KSkill"))
					{
						GameScr.gI().onKSkill(data);
					}
					else if (text.Equals("OSkill"))
					{
						GameScr.gI().onOSkill(data);
					}
					else if (text.Equals("CSkill"))
					{
						GameScr.gI().onCSkill(data);
					}
					break;
				}
				case 62:
					Res.outz("ME UPDATE SKILL");
					read_UpdateSkill(msg);
					break;
				case 63:
				{
					sbyte b = msg.reader().readByte();
					if (b > 0)
					{
						GameCanvas.panel.vPlayerMenu_id.removeAllElements();
						InfoDlg.showWait();
						MyVector vPlayerMenu = GameCanvas.panel.vPlayerMenu;
						for (int i = 0; i < b; i++)
						{
							string caption = msg.reader().readUTF();
							string caption2 = msg.reader().readUTF();
							short menuSelect = msg.reader().readShort();
							GameCanvas.panel.vPlayerMenu_id.addElement(menuSelect + string.Empty);
							Char.myCharz().charFocus.menuSelect = menuSelect;
							vPlayerMenu.addElement(new Command(caption, 11115, Char.myCharz().charFocus)
							{
								caption2 = caption2
							});
						}
						InfoDlg.hide();
						GameCanvas.panel.setTabPlayerMenu();
					}
					break;
				}
				case 4:
					Char.myCharz().xu = msg.reader().readLong();
					Char.myCharz().luong = msg.reader().readInt();
					Char.myCharz().cHP = msg.readLong();
					Char.myCharz().cMP = msg.readLong();
					Char.myCharz().luongKhoa = msg.reader().readInt();
					Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
					Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
					Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
					break;
				case 8:
				{
					GameCanvas.debug("SA26", 2);
					Char char2 = GameScr.findCharInMap(msg.reader().readInt());
					if (char2 != null)
					{
						char2.cspeed = msg.reader().readByte();
					}
					break;
				}
				case 15:
				{
					Char @char = GameScr.findCharInMap(msg.reader().readInt());
					if (@char != null)
					{
						@char.cHP = msg.readLong();
						@char.cHPFull = msg.readLong();
						@char.cx = msg.reader().readShort();
						@char.cy = msg.reader().readShort();
						@char.statusMe = 1;
						@char.cp3 = 3;
						ServerEffect.addServerEffect(109, @char, 2);
					}
					break;
				}
				}
			}
			catch (Exception ex5)
			{
				Cout.println("Loi tai Sub : " + ex5.ToString());
			}
			finally
			{
				msg?.cleanup();
			}
		}

		private void useSkill(Skill skill)
		{
			if (Char.myCharz().myskill == null)
			{
				Char.myCharz().myskill = skill;
			}
			else if (skill.template.Equals(Char.myCharz().myskill.template))
			{
				Char.myCharz().myskill = skill;
			}
			Char.myCharz().vSkill.addElement(skill);
			if ((skill.template.type == 1 || skill.template.type == 4 || skill.template.type == 2 || skill.template.type == 3) && (skill.template.maxPoint == 0 || (skill.template.maxPoint > 0 && skill.point > 0)))
			{
				if (skill.template.id == Char.myCharz().skillTemplateId)
				{
					Service.gI().selectSkill(Char.myCharz().skillTemplateId);
				}
				Char.myCharz().vSkillFight.addElement(skill);
			}
		}

		public bool readCharInfo(Char c, Message msg)
		{
			try
			{
				c.clevel = msg.reader().readByte();
				c.isInvisiblez = msg.reader().readBoolean();
				c.cTypePk = msg.reader().readByte();
				c.nClass = GameScr.nClasss[msg.reader().readByte()];
				c.cgender = msg.reader().readByte();
				c.head = msg.reader().readShort();
				c.cName = msg.reader().readUTF();
				c.cHP = msg.readLong();
				c.dHP = c.cHP;
				if (c.cHP == 0L)
				{
					c.statusMe = 14;
				}
				c.cHPFull = msg.readLong();
				if (c.cy >= TileMap.pxh - 100)
				{
					c.isFlyUp = true;
				}
				c.body = msg.reader().readShort();
				c.leg = msg.reader().readShort();
				c.bag = msg.reader().readUnsignedByte();
				c.isShadown = true;
				msg.reader().readByte();
				if (c.wp == -1)
				{
					c.setDefaultWeapon();
				}
				if (c.body == -1)
				{
					c.setDefaultBody();
				}
				if (c.leg == -1)
				{
					c.setDefaultLeg();
				}
				c.cx = msg.reader().readShort();
				c.cy = msg.reader().readShort();
				c.xSd = c.cx;
				c.ySd = c.cy;
				c.eff5BuffHp = msg.reader().readShort();
				c.eff5BuffMp = msg.reader().readShort();
				int num = msg.reader().readByte();
				for (int i = 0; i < num; i++)
				{
					EffectChar effectChar = new EffectChar(msg.reader().readByte(), msg.reader().readInt(), msg.reader().readInt(), msg.reader().readShort());
					c.vEff.addElement(effectChar);
					if (effectChar.template.type == 12 || effectChar.template.type == 11)
					{
						c.isInvisiblez = true;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			return false;
		}

		private void readGetImgByName(Message msg)
		{
			try
			{
				string text = msg.reader().readUTF();
				sbyte nFrame = msg.reader().readByte();
				sbyte[] array = NinjaUtil.readByteArray(msg);
				Image img = createImage(array);
				ImgByName.SetImage(text, img, nFrame);
				if (array != null)
				{
					ImgByName.saveRMS(text, nFrame, array);
				}
			}
			catch (Exception)
			{
			}
		}

		private void createItemNew(myReader d)
		{
			try
			{
				loadItemNew(d, -1, isSave: true);
			}
			catch (Exception)
			{
			}
		}

		private void loadItemNew(myReader d, sbyte type, bool isSave)
		{
			try
			{
				d.mark(100000);
				GameScr.vcItem = d.readByte();
				type = d.readByte();
				switch (type)
				{
				case 0:
				{
					GameScr.gI().iOptionTemplates = new ItemOptionTemplate[d.readUnsignedByte()];
					for (int k = 0; k < GameScr.gI().iOptionTemplates.Length; k++)
					{
						GameScr.gI().iOptionTemplates[k] = new ItemOptionTemplate();
						GameScr.gI().iOptionTemplates[k].id = k;
						GameScr.gI().iOptionTemplates[k].name = d.readUTF();
						GameScr.gI().iOptionTemplates[k].type = d.readByte();
					}
					if (isSave)
					{
						d.reset();
						sbyte[] data5 = new sbyte[d.available()];
						d.readFully(ref data5);
						Rms.saveRMS("NRitem0", data5);
					}
					break;
				}
				case 1:
				{
					ItemTemplates.itemTemplates.clear();
					int num = d.readShort();
					for (int i = 0; i < num; i++)
					{
						ItemTemplates.add(new ItemTemplate((short)i, d.readByte(), d.readByte(), d.readUTF(), d.readUTF(), d.readByte(), d.readInt(), d.readShort(), d.readShort(), d.readBoolean()));
					}
					itemNextTemplateId = num;
					if (isSave)
					{
						itemAppendPacketCount = 0;
						d.reset();
						sbyte[] data2 = new sbyte[d.available()];
						d.readFully(ref data2);
						Rms.saveRMS("NRitem1", data2);
					}
					break;
				}
				case 2:
				{
					short num2 = d.readShort();
					int num3 = d.readShort();
					if (num2 != itemNextTemplateId || num3 < num2)
					{
						throw new Exception("Sai thứ tự mảnh item cache: " + num2 + ".." + num3 + ", chờ " + itemNextTemplateId);
					}
					for (int j = num2; j < num3; j++)
					{
						ItemTemplates.add(new ItemTemplate((short)j, d.readByte(), d.readByte(), d.readUTF(), d.readUTF(), d.readByte(), d.readInt(), d.readShort(), d.readShort(), d.readBoolean()));
					}
					itemNextTemplateId = num3;
					if (isSave)
					{
						d.reset();
						sbyte[] data3 = new sbyte[d.available()];
						d.readFully(ref data3);
						Rms.saveRMS("NRitem2_" + itemAppendPacketCount, data3);
						itemAppendPacketCount++;
					}
					break;
				}
				case 3:
				{
					int expectedTemplateCount = d.readShort();
					if (!isSave)
					{
						break;
					}
					if (!hasCompleteItemTemplateCache(expectedTemplateCount))
					{
						Debug.LogError("Item cache chưa đủ hoặc sai chỉ mục; yêu cầu tải lại từ server.");
						GameScr.vcItem = -1;
						Service.gI().updateItem();
						break;
					}
					Rms.saveRMS("NRitem2Count", new sbyte[1] { (sbyte)itemAppendPacketCount });
					Rms.saveRMS("NRitemVersion", new sbyte[1] { GameScr.vcItem });
					LoginScr.isUpdateItem = false;
					if (GameScr.vsData == GameScr.vcData && GameScr.vsMap == GameScr.vcMap && GameScr.vsSkill == GameScr.vcSkill && GameScr.vsItem == GameScr.vcItem)
					{
						GameScr.gI().readDart();
						GameScr.gI().readEfect();
						GameScr.gI().readArrow();
						GameScr.gI().readSkill();
						Service.gI().clientOk();
					}
					break;
				}
				case 100:
					Char.Arr_Head_2Fr = readArrHead(d);
					if (isSave)
					{
						d.reset();
						sbyte[] data = new sbyte[d.available()];
						d.readFully(ref data);
						Rms.saveRMS("NRitem100", data);
					}
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Lỗi đọc gói item cache: " + ex);
				GameScr.vcItem = -1;
				Service.gI().updateItem();
			}
		}

		private static bool hasCompleteItemTemplateCache(int expectedTemplateCount)
		{
			if (expectedTemplateCount < 0 || ItemTemplates.itemTemplates.size() != expectedTemplateCount)
			{
				return false;
			}
			for (int i = 0; i < expectedTemplateCount; i++)
			{
				if (ItemTemplates.get((short)i) == null)
				{
					return false;
				}
			}
			return true;
		}

		private void readFrameBoss(Message msg, int mobTemplateId)
		{
			try
			{
				int num = msg.reader().readByte();
				int[][] array = new int[num][];
				for (int i = 0; i < num; i++)
				{
					int num2 = msg.reader().readByte();
					array[i] = new int[num2];
					for (int j = 0; j < num2; j++)
					{
						array[i][j] = msg.reader().readByte();
					}
				}
				frameHT_NEWBOSS.put(mobTemplateId + string.Empty, array);
			}
			catch (Exception)
			{
			}
		}

		private int[][] readArrHead(myReader d)
		{
			int[][] array = new int[1][] { new int[2] { 542, 543 } };
			try
			{
				array = new int[d.readShort()][];
				for (int i = 0; i < array.Length; i++)
				{
					int num = d.readByte();
					array[i] = new int[num];
					for (int j = 0; j < num; j++)
					{
						array[i][j] = d.readShort();
					}
				}
			}
			catch (Exception)
			{
			}
			return array;
		}

		public void phuban_Info(Message msg)
		{
			try
			{
				sbyte b = msg.reader().readByte();
				if (b == 0)
				{
					readPhuBan_CHIENTRUONGNAMEK(msg, b);
				}
			}
			catch (Exception)
			{
			}
		}

		private void readPhuBan_CHIENTRUONGNAMEK(Message msg, int type_PB)
		{
			try
			{
				switch (msg.reader().readByte())
				{
				case 0:
				{
					short idmapPaint = msg.reader().readShort();
					string nameTeam = msg.reader().readUTF();
					string nameTeam2 = msg.reader().readUTF();
					int maxPoint = msg.reader().readInt();
					short timeSecond = msg.reader().readShort();
					int maxLife = msg.reader().readByte();
					GameScr.phuban_Info = new InfoPhuBan(type_PB, idmapPaint, nameTeam, nameTeam2, maxPoint, timeSecond);
					GameScr.phuban_Info.maxLife = maxLife;
					GameScr.phuban_Info.updateLife(type_PB, 0, 0);
					break;
				}
				case 1:
				{
					int pointTeam = msg.reader().readInt();
					int pointTeam2 = msg.reader().readInt();
					if (GameScr.phuban_Info != null)
					{
						GameScr.phuban_Info.updatePoint(type_PB, pointTeam, pointTeam2);
					}
					break;
				}
				case 2:
				{
					sbyte b = msg.reader().readByte();
					short type = 0;
					switch (b)
					{
					case 1:
						type = 1;
						break;
					case 2:
						type = 2;
						break;
					}
					short subtype = -1;
					GameScr.phuban_Info = null;
					GameScr.addEffectEnd(type, subtype, 0, GameCanvas.hw, GameCanvas.hh, 0, 0, -1, null);
					break;
				}
				case 5:
				{
					short timeSecond2 = msg.reader().readShort();
					if (GameScr.phuban_Info != null)
					{
						GameScr.phuban_Info.updateTime(type_PB, timeSecond2);
					}
					break;
				}
				case 4:
				{
					int lifeTeam = msg.reader().readByte();
					int lifeTeam2 = msg.reader().readByte();
					if (GameScr.phuban_Info != null)
					{
						GameScr.phuban_Info.updateLife(type_PB, lifeTeam, lifeTeam2);
					}
					break;
				}
				}
			}
			catch (Exception)
			{
			}
		}

		public void read_opt(Message msg)
		{
			try
			{
				switch (msg.reader().readByte())
				{
				case 0:
				{
					short idHat = msg.reader().readShort();
					Char.myCharz().idHat = idHat;
					SoundMn.gI().getStrOption();
					break;
				}
				case 2:
				{
					int num2 = msg.reader().readInt();
					sbyte b = msg.reader().readByte();
					short num3 = msg.reader().readShort();
					if (b <= 0 || num3 < 0)
					{
						GameCanvas.danhHieu.remove(num2 + string.Empty);
						break;
					}
					string v = num3 + "," + b;
					ImgByName.getImagePath("banner_" + num3, ImgByName.hashImagePath);
					GameCanvas.danhHieu.put(num2 + string.Empty, v);
					break;
				}
				case 3:
				{
					short num = msg.reader().readShort();
					SmallImage.createImage(num);
					BackgroudEffect.id_water1 = num;
					break;
				}
				case 4:
				{
					string o = msg.reader().readUTF();
					GameCanvas.messageServer.addElement(o);
					break;
				}
				case 5:
					ModFunc.setHideBadgesFromServer(msg.reader().readByte() == 1);
					break;
				}
			}
			catch (Exception)
			{
			}
		}

		public void read_UpdateSkill(Message msg)
		{
			try
			{
				short num = msg.reader().readShort();
				sbyte b = -1;
				try
				{
					b = msg.reader().readSByte();
				}
				catch (Exception)
				{
				}
				switch (b)
				{
				case 0:
				{
					short curExp = msg.reader().readShort();
					short actualLevel = 0;
					short damage = 0;
					int manaUse = 0;
					int coolDown = 0;
					int dx = 0;
					int dy = 0;
					int maxFight = 0;
					bool hasMasteryStats = false;
					try
					{
						actualLevel = msg.reader().readShort();
						damage = msg.reader().readShort();
						manaUse = msg.reader().readInt();
						coolDown = msg.reader().readInt();
						dx = msg.reader().readInt();
						dy = msg.reader().readInt();
						maxFight = msg.reader().readInt();
						hasMasteryStats = true;
					}
					catch (Exception)
					{
					}
					for (int m = 0; m < Char.myCharz().vSkill.size(); m++)
					{
						Skill skill2 = (Skill)Char.myCharz().vSkill.elementAt(m);
						if (skill2.skillId == num)
						{
							skill2.curExp = curExp;
							if (hasMasteryStats)
							{
								skill2.actualLevel = actualLevel;
								skill2.damage = damage;
								skill2.manaUse = manaUse;
								skill2.coolDown = coolDown;
								skill2.dx = dx;
								skill2.dy = dy;
								skill2.maxFight = maxFight;
							}
							break;
						}
					}
					break;
				}
				case 1:
				{
					sbyte b2 = msg.reader().readByte();
					for (int n = 0; n < Char.myCharz().vSkill.size(); n++)
					{
						Skill skill3 = (Skill)Char.myCharz().vSkill.elementAt(n);
						if (skill3.skillId == num)
						{
							for (int num2 = 0; num2 < 20; num2++)
							{
								ImgByName.getImagePath("Skills_" + skill3.template.id + "_" + b2 + "_" + num2, ImgByName.hashImagePath);
							}
							break;
						}
					}
					break;
				}
				case -1:
				{
					Skill skill = Skills.get(num);
					for (int i = 0; i < Char.myCharz().vSkill.size(); i++)
					{
						if (((Skill)Char.myCharz().vSkill.elementAt(i)).template.id == skill.template.id)
						{
							Char.myCharz().vSkill.setElementAt(skill, i);
							break;
						}
					}
					for (int j = 0; j < Char.myCharz().vSkillFight.size(); j++)
					{
						if (((Skill)Char.myCharz().vSkillFight.elementAt(j)).template.id == skill.template.id)
						{
							Char.myCharz().vSkillFight.setElementAt(skill, j);
							break;
						}
					}
					for (int k = 0; k < GameScr.onScreenSkill.Length; k++)
					{
						if (GameScr.onScreenSkill[k] != null && GameScr.onScreenSkill[k].template.id == skill.template.id)
						{
							GameScr.onScreenSkill[k] = skill;
							break;
						}
					}
					for (int l = 0; l < GameScr.keySkill.Length; l++)
					{
						if (GameScr.keySkill[l] != null && GameScr.keySkill[l].template.id == skill.template.id)
						{
							GameScr.keySkill[l] = skill;
							break;
						}
					}
					if (Char.myCharz().myskill.template.id == skill.template.id)
					{
						Char.myCharz().myskill = skill;
					}
					GameScr.info1.addInfo(mResources.hasJustUpgrade1 + skill.template.name + mResources.hasJustUpgrade2 + skill.point, 0);
					break;
				}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
