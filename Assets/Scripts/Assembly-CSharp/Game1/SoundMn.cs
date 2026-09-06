using System;
using System.Collections.Generic;

namespace Game1
{
	public class SoundMn
	{
		public static bool IsDelAcc;

		public static SoundMn gIz;

		public static bool isSound = true;

		public static float volume = 0.5f;

		private static int MAX_VOLUME = 10;

		public static int AIR_SHIP;

		public static int RAIN = 1;

		public static int TAITAONANGLUONG = 2;

		public static int GET_ITEM;

		public static int MOVE = 1;

		public static int LOW_PUNCH = 2;

		public static int LOW_KICK = 3;

		public static int FLY = 4;

		public static int JUMP = 5;

		public static int PANEL_OPEN = 6;

		public static int BUTTON_CLOSE = 7;

		public static int BUTTON_CLICK = 8;

		public static int MEDIUM_PUNCH = 9;

		public static int MEDIUM_KICK = 10;

		public static int PANEL_CLICK = 11;

		public static int EAT_PEAN = 12;

		public static int OPEN_DIALOG = 13;

		public static int NORMAL_KAME = 14;

		public static int NAMEK_KAME = 15;

		public static int XAYDA_KAME = 16;

		public static int EXPLODE_1 = 17;

		public static int EXPLODE_2 = 18;

		public static int TRAIDAT_KAME = 19;

		public static int HP_UP = 20;

		public static int THAIDUONGHASAN = 21;

		public static int HOISINH = 22;

		public static int GONG = 23;

		public static int KHICHAY = 24;

		public static int BIG_EXPLODE = 25;

		public static int NAMEK_LAZER = 26;

		public static int NAMEK_CHARGE = 27;

		public static int RADAR_CLICK = 28;

		public static int RADAR_ITEM = 29;

		public static int FIREWORK = 30;

		public static int KAMEX10_0 = 31;

		public static int KAMEX10_1 = 32;

		public static int DESTROY_0 = 33;

		public static int DESTROY_1 = 34;

		public static int MAFUBA_0 = 35;

		public static int MAFUBA_1 = 36;

		public static int MAFUBA_2 = 37;

		public static int DESTROY_2 = 38;

		public int poolCount;

		public static int cout = 1;

		public static SoundMn gI()
		{
			if (gIz == null)
			{
				gIz = new SoundMn();
			}
			return gIz;
		}

		public void loadSound(int mapID)
		{
			Sound.init(new int[3] { AIR_SHIP, RAIN, TAITAONANGLUONG }, new int[39]
			{
				GET_ITEM, MOVE, LOW_PUNCH, LOW_KICK, FLY, JUMP, PANEL_OPEN, BUTTON_CLOSE, BUTTON_CLICK, MEDIUM_PUNCH,
				MEDIUM_KICK, PANEL_OPEN, EAT_PEAN, OPEN_DIALOG, NORMAL_KAME, NAMEK_KAME, XAYDA_KAME, EXPLODE_1, EXPLODE_2, TRAIDAT_KAME,
				HP_UP, THAIDUONGHASAN, HOISINH, GONG, KHICHAY, BIG_EXPLODE, NAMEK_LAZER, NAMEK_CHARGE, RADAR_CLICK, RADAR_ITEM,
				FIREWORK, KAMEX10_0, KAMEX10_1, DESTROY_0, DESTROY_1, MAFUBA_0, MAFUBA_1, MAFUBA_2, DESTROY_2
			});
		}

		public void getSoundOption()
		{
			if (GameCanvas.loginScr.isLogin2 && Char.myCharz().taskMaint != null && Char.myCharz().taskMaint.taskId >= 2)
			{
				if (Char.myCharz().havePet && Char.myCharz().havePet2)
				{
					Panel.strTool = new string[13]
					{
						mResources.gameInfo,
						mResources.quayso,
						ModFunc.strPlayerInfo,
						mResources.radaCard,
						mResources.pet,
						ModFunc.strPet2,
						mResources.change_flag,
						mResources.change_zone,
						mResources.chat_world,
						mResources.account,
						mResources.option,
						mResources.change_account,
						mResources.REGISTOPROTECT
					};
				}
				else if (Char.myCharz().havePet || Char.myCharz().havePet2)
				{
					Panel.strTool = new string[12]
					{
						mResources.gameInfo,
						mResources.quayso,
						ModFunc.strPlayerInfo,
						mResources.radaCard,
						Char.myCharz().havePet ? mResources.pet : ModFunc.strPet2,
						mResources.change_flag,
						mResources.change_zone,
						mResources.chat_world,
						mResources.account,
						mResources.option,
						mResources.change_account,
						mResources.REGISTOPROTECT
					};
				}
				else
				{
					Panel.strTool = new string[11]
					{
						mResources.gameInfo,
						mResources.quayso,
						ModFunc.strPlayerInfo,
						mResources.radaCard,
						mResources.change_flag,
						mResources.change_zone,
						mResources.chat_world,
						mResources.account,
						mResources.option,
						mResources.change_account,
						mResources.REGISTOPROTECT
					};
				}
			}
			else if (Char.myCharz().havePet && Char.myCharz().havePet2)
			{
				Panel.strTool = new string[12]
				{
					mResources.gameInfo,
					mResources.quayso,
					ModFunc.strPlayerInfo,
					mResources.radaCard,
					mResources.pet,
					ModFunc.strPet2,
					mResources.change_flag,
					mResources.change_zone,
					mResources.chat_world,
					mResources.account,
					mResources.option,
					mResources.change_account
				};
			}
			else if (Char.myCharz().havePet || Char.myCharz().havePet2)
			{
				Panel.strTool = new string[11]
				{
					mResources.gameInfo,
					mResources.quayso,
					ModFunc.strPlayerInfo,
					mResources.radaCard,
					Char.myCharz().havePet ? mResources.pet : ModFunc.strPet2,
					mResources.change_flag,
					mResources.change_zone,
					mResources.chat_world,
					mResources.account,
					mResources.option,
					mResources.change_account
				};
			}
			else
			{
				Panel.strTool = new string[10]
				{
					mResources.gameInfo,
					mResources.quayso,
					ModFunc.strPlayerInfo,
					mResources.radaCard,
					mResources.change_flag,
					mResources.change_zone,
					mResources.chat_world,
					mResources.account,
					mResources.option,
					mResources.change_account
				};
			}
			InsertActivityTool();
			if (IsDelAcc)
			{
				string[] array = new string[Panel.strTool.Length + 1];
				for (int i = 0; i < Panel.strTool.Length; i++)
				{
					array[i] = Panel.strTool[i];
				}
				array[Panel.strTool.Length] = mResources.delacc;
				Panel.strTool = array;
			}
			int historyIndex = Panel.strTool.Length;
			for (int j = 0; j < Panel.strTool.Length; j++)
			{
				if (Panel.strTool[j].Equals(mResources.chat_world))
				{
					historyIndex = j;
					break;
				}
			}
			string[] historyTools = new string[Panel.strTool.Length + 1];
			for (int k = 0; k < historyIndex; k++)
			{
				historyTools[k] = Panel.strTool[k];
			}
			historyTools[historyIndex] = Panel.PK_HISTORY_TOOL;
			for (int l = historyIndex; l < Panel.strTool.Length; l++)
			{
				historyTools[l + 1] = Panel.strTool[l];
			}
			Panel.strTool = historyTools;
		}

		private static void InsertActivityTool()
		{
			if (Panel.strTool == null)
			{
				return;
			}
			for (int index = 0; index < Panel.strTool.Length; index++)
			{
				if (Panel.strTool[index].Equals(Panel.ACTIVITY_TOOL))
				{
					return;
				}
			}
			int insertAt = 0;
			for (int index = 0; index < Panel.strTool.Length; index++)
			{
				if (Panel.strTool[index].Equals(mResources.gameInfo))
				{
					insertAt = index + 1;
					break;
				}
			}
			string[] tools = new string[Panel.strTool.Length + 1];
			for (int index = 0; index < insertAt; index++)
			{
				tools[index] = Panel.strTool[index];
			}
			tools[insertAt] = Panel.ACTIVITY_TOOL;
			for (int index = insertAt; index < Panel.strTool.Length; index++)
			{
				tools[index + 1] = Panel.strTool[index];
			}
			Panel.strTool = tools;
		}

		public void getStrOption()
		{
			if (GameCanvas.isLoadRes)
			{
				string text = "[x]   ";
				string text2 = "[  ]   ";
				string text3 = ((GameScr.isAnalog != 0) ? (text + mResources.turnOffAnalog) : (text2 + mResources.turnOnAnalog));
				if (!GameCanvas.isTouch)
				{
					text3 = (GameScr.isPaintChatVip ? (text + mResources.serverchat_off) : (text2 + mResources.serverchat_off));
				}
				Panel.strCauhinh = new string[5]
				{
					Char.isPaintAura ? (text + mResources.aura_off.Trim()) : (text2 + mResources.aura_off.Trim()),
					Char.isPaintAura2 ? (text + mResources.aura_off_2.Trim()) : (text2 + mResources.aura_off_2.Trim()),
					(!GameCanvas.isPlaySound) ? (text2 + mResources.turnOffSound.Trim()) : (text + mResources.turnOffSound.Trim()),
					(!GameCanvas.lowGraphic) ? (text2 + mResources.cauhinhthap.Trim()) : (text + mResources.cauhinhthap.Trim()),
					text3
				};
			}
		}

		public void GetStrModFunc()
		{
			if (!GameCanvas.isLoadRes)
			{
				return;
			}
			(bool, string)[][] array = new(bool, string)[4][]
			{
				new(bool, string)[5]
				{
					(ModFunc.GI().isHighFps, ModFunc.strHighFps),
					(ModFunc.GI().isUpdateZones, ModFunc.strUpdateZones),
					(ModFunc.GI().showCharsInMap, ModFunc.strCharsInMap),
					(ModFunc.GI().showInfoMe, ModFunc.strInfoMe),
					(ModFunc.GI().isShowButton, ModFunc.strShowButton)
				},
				new(bool, string)[4]
				{
					(ModFunc.GI().isAutoPhaLe, ModFunc.strAutoPhaLe),
					(ModFunc.GI().isAutoVQMM, ModFunc.strAutoVQMM),
					(ModFunc.GI().autoWakeUp, ModFunc.strAutoWakeUp),
					(ModFunc.isAutoLogin, ModFunc.strAutoLogin)
				},
				new(bool, string)[7]
				{
					(!ModFunc.ModNotLogo && ModFunc.isLogo, ModFunc.strLogo),
					(!ModFunc.ModNotLogo && ModFunc.isLogoGif, ModFunc.strLogoGif),
					(ModFunc.AnPlayer, ModFunc.strAnPlayer),
					(ModFunc.isShowID, ModFunc.strShowID),
					(ModFunc.isInventory, ModFunc.strInventoryOFF),
					(ModFunc.isEffectInven, ModFunc.strEffectOff),
					(ModFunc.isHideBadges, ModFunc.strHideBadges)
				},
				new(bool, string)[5]
				{
					(ModFunc.GI().isIntroOff, ModFunc.strIntroOff),
					(ModFunc.GiamDungLuong, ModFunc.strGiamDungLuong),
					(ModFunc.isEditButton, ModFunc.strEditButton),
					(ModFunc.isFilterItem, "Lọc đồ"),
					(ModFunc.isVietnamese, "Gõ Tiếng Việt")
				}
			};
			int currentTabIndex = GameCanvas.panel.currentTabIndex;
			if (currentTabIndex >= 0 && currentTabIndex < array.Length)
			{
				(bool, string)[] obj = array[currentTabIndex];
				List<string> list = new List<string>();
				(bool, string)[] array2 = obj;
				for (int i = 0; i < array2.Length; i++)
				{
					var (flag, text) = array2[i];
					if (!ModFunc.ModNotLogo || (text != ModFunc.strLogo && text != ModFunc.strLogoGif))
					{
						list.Add((flag ? "[x]   " : "[  ]   ") + text);
					}
				}
				Panel.strModFunc = list.ToArray();
			}
			else
			{
				Panel.strModFunc = new string[0];
			}
		}

		public void HP_MPup()
		{
			Sound.playSound(HP_UP, 0.5f);
		}

		public void charPunch(bool isKick, float volumn)
		{
			if (!Char.myCharz().me)
			{
				volume /= 2f;
			}
			if (volumn <= 0f)
			{
				volumn = 0.01f;
			}
			int num = Res.random(0, 3);
			if (isKick)
			{
				Sound.playSound((num != 0) ? MEDIUM_KICK : LOW_KICK, 0.1f);
			}
			else
			{
				Sound.playSound((num != 0) ? MEDIUM_PUNCH : LOW_PUNCH, 0.1f);
			}
			poolCount++;
		}

		public void thaiduonghasan()
		{
			Sound.playSound(THAIDUONGHASAN, 0.5f);
			poolCount++;
		}

		public void rain()
		{
			Sound.playMus(RAIN, 0.3f, loop: true);
		}

		public void gongName()
		{
			Sound.playSound(NAMEK_CHARGE, 0.3f);
			poolCount++;
		}

		public void gong()
		{
			Sound.playSound(GONG, 0.2f);
			poolCount++;
		}

		public void getItem()
		{
			Sound.playSound(GET_ITEM, 0.3f);
			poolCount++;
		}

		public void soundToolOption()
		{
			GameCanvas.isPlaySound = !GameCanvas.isPlaySound;
			if (GameCanvas.isPlaySound)
			{
				gI().loadSound(TileMap.mapID);
				Rms.saveRMSInt("isPlaySound", 1);
			}
			else
			{
				gI().closeSound();
				Rms.saveRMSInt("isPlaySound", 0);
			}
			getStrOption();
		}

		public void chatVipToolOption()
		{
			GameScr.isPaintChatVip = !GameScr.isPaintChatVip;
			if (GameScr.isPaintChatVip)
			{
				Rms.saveRMSInt("serverchat", 0);
			}
			else
			{
				Rms.saveRMSInt("serverchat", 1);
			}
			getStrOption();
		}

		public void analogToolOption()
		{
			if (GameScr.isAnalog == 0)
			{
				GameScr.isAnalog = 1;
				Rms.saveRMSInt("analog", GameScr.isAnalog);
				GameScr.setSkillBarPosition();
			}
			else
			{
				GameScr.isAnalog = 0;
				Rms.saveRMSInt("analog", GameScr.isAnalog);
				GameScr.setSkillBarPosition();
			}
			getStrOption();
		}

		public void CaseAnalog()
		{
			if (!Main.isPC || Main.isIPhone)
			{
				if (!GameCanvas.isTouch)
				{
					chatVipToolOption();
				}
				else
				{
					analogToolOption();
				}
			}
		}

		public void CaseSizeScr()
		{
			if (GameCanvas.lowGraphic)
			{
				Rms.saveRMSInt("lowGraphic", 0);
				GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
			}
			else
			{
				Rms.saveRMSInt("lowGraphic", 1);
				GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
			}
			getStrOption();
		}

		public void AuraToolOption()
		{
			if (Char.isPaintAura)
			{
				Rms.saveRMSInt("isPaintAura", 0);
				Char.isPaintAura = false;
				Rms.saveRMSInt("auraDisplayMode", 0);
			}
			else
			{
				Rms.saveRMSInt("isPaintAura", 1);
				Char.isPaintAura = true;
				Rms.saveRMSInt("isPaintAura2", 0);
				Char.isPaintAura2 = false;
				Rms.saveRMSInt("auraDisplayMode", 1);
			}
			getStrOption();
		}

		public void AuraToolOption2()
		{
			if (Char.isPaintAura2)
			{
				Rms.saveRMSInt("isPaintAura2", 0);
				Char.isPaintAura2 = false;
				Rms.saveRMSInt("auraDisplayMode", 0);
			}
			else
			{
				Rms.saveRMSInt("isPaintAura2", 1);
				Char.isPaintAura2 = true;
				Rms.saveRMSInt("isPaintAura", 0);
				Char.isPaintAura = false;
				Rms.saveRMSInt("auraDisplayMode", 2);
			}
			getStrOption();
		}

		public void update()
		{
		}

		public void closeSound()
		{
			Sound.stopAll = true;
			stopAll();
		}

		public void bigeExlode()
		{
			Sound.playSound(BIG_EXPLODE, 0.5f);
			poolCount++;
		}

		public void explode_1()
		{
			Sound.playSound(EXPLODE_1, 0.5f);
			poolCount++;
		}

		public void explode_2()
		{
			Sound.playSound(EXPLODE_1, 0.5f);
			poolCount++;
		}

		public void traidatKame()
		{
			Sound.playSound(TRAIDAT_KAME, 1f);
			poolCount++;
		}

		public void namekKame()
		{
			Sound.playSound(NAMEK_KAME, 0.3f);
			poolCount++;
		}

		public void nameLazer()
		{
			Sound.playSound(NAMEK_LAZER, 0.3f);
			poolCount++;
		}

		public void xaydaKame()
		{
			Sound.playSound(XAYDA_KAME, 0.3f);
			poolCount++;
		}

		public void mobKame(int type)
		{
			int id = XAYDA_KAME;
			if (type == 13)
			{
				id = NORMAL_KAME;
			}
			Sound.playSound(id, 0.1f);
			poolCount++;
		}

		public void charRun(float volumn)
		{
			if (!Char.myCharz().me)
			{
				volume /= 2f;
				if (volumn <= 0f)
				{
					volumn = 0.01f;
				}
			}
			if (GameCanvas.gameTick % 8 == 0)
			{
				Sound.playSound(MOVE, volumn);
				poolCount++;
			}
		}

		public void monkeyRun(float volumn)
		{
			if (GameCanvas.gameTick % 8 == 0)
			{
				Sound.playSound(KHICHAY, 0.2f);
				poolCount++;
			}
		}

		public void charFall()
		{
			Sound.playSound(MOVE, 0.1f);
			poolCount++;
		}

		public void charJump()
		{
			Sound.playSound(MOVE, 0.2f);
			poolCount++;
		}

		public void panelOpen()
		{
			Sound.playSound(PANEL_OPEN, 0.5f);
			poolCount++;
		}

		public void buttonClose()
		{
		}

		public void buttonClick()
		{
			Sound.playSound(BUTTON_CLICK, 0.5f);
			poolCount++;
		}

		public void charFly()
		{
			Sound.playSound(FLY, 0.2f);
			poolCount++;
		}

		public void openMenu()
		{
			Sound.playSound(BUTTON_CLOSE, 0.5f);
			poolCount++;
		}

		public void panelClick()
		{
			Sound.playSound(PANEL_CLICK, 0.5f);
			poolCount++;
		}

		public void eatPeans()
		{
			Sound.playSound(EAT_PEAN, 0.5f);
			poolCount++;
		}

		public void openDialog()
		{
			Sound.playSound(OPEN_DIALOG, 0.5f);
		}

		public void hoisinh()
		{
			Sound.playSound(HOISINH, 0.5f);
			poolCount++;
		}

		public void taitaoPause()
		{
		}

		public bool isPlayRain()
		{
			try
			{
				return Sound.isPlayingSound();
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool isPlayAirShip()
		{
			return false;
		}

		public void airShip()
		{
			cout++;
			if (cout % 2 == 0)
			{
				Sound.playMus(AIR_SHIP, 0.3f, loop: false);
			}
		}

		public void pauseAirShip()
		{
		}

		public void resumeAirShip()
		{
		}

		public void stopAll()
		{
			Sound.stopAllz();
		}

		public void backToRegister()
		{
			Session_ME.gI().close();
			GameCanvas.panel.hide();
			GameCanvas.loginScr.actRegister();
			GameCanvas.loginScr.switchToMe();
		}

		public void newKame()
		{
			poolCount++;
			if (poolCount % 15 == 0)
			{
				Sound.playSound(TRAIDAT_KAME, 0.5f);
			}
		}

		public void radarClick()
		{
			Sound.playSound(RADAR_CLICK, 0.5f);
		}

		public void radarItem()
		{
			Sound.playSound(RADAR_ITEM, 0.5f);
		}

		public static void playSound(int x, int y, int id, float volume)
		{
			Sound.playSound(id, volume);
		}
	}
}
