using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game1.Mod;
using Game1.Mod.XMAP;
using UnityEngine;
using UnityEngine.Networking;

namespace Game1
{
	public class ModFunc : IActionListener
	{
		public class Point
		{
			public int x;

			public int y;

			public Point(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		private static readonly ModFunc Instance = new ModFunc();

		public static string homeUrl = "https://srcnrofree.online/";

		public static bool ModNotLogo = true;
		

		public static bool ModNotLogoGif = true;

		public static bool isReadInt = true;

		public static bool isVietnamese = false;

		public static bool isShowMenuChat = false;

		public static bool isMenuVisible = false;

		public static float arrowRotation = 0f;

		public static float menuX = 0f;

		public static float targetMenuX = 0f;

		public static float targetArrowRotation = 0f;

		public static float ANIMATION_SPEED = 0.1f;

		private static bool isDebugEnable = false;

		private static long lastTimeLog = 0L;

		public bool canUpdate;

		public static Command cmdAccManager;

		public static bool isOpenAccMAnager = false;

		public static List<Account> accounts = new List<Account>();

		public List<Command> cmdsChooseAcc = new List<Command>();

		public List<Command> cmdsDelAcc = new List<Command>();

		public static Command cmdCloseAccManager;

		private static int modKeyPosX;

		private static int modKeyPosY;

		public static bool isAutoLogin = false;

		public static bool dangLogin = false;

		public static AutoLogin autoLogin;

		public static bool isAutoNoitai = false;

		public bool autoAttack;

		public bool autoWakeUp;

		public long lastAutoWakeUp;

		public bool isAutoPhaLe;

		public bool isAutoVQMM;

		public long lastVQMM;

		private long lastAutoAttack;

		private int paramIntrinsic = -1;

		private readonly List<Skill> listSkillsAuto = new List<Skill>();

		public List<ItemAuto> listItemAuto = new List<ItemAuto>();

		private static bool isAutoChat = false;

		private static string textAutoChat = string.Empty;

		private static bool isAutoChatTG = false;

		private static string textAutoChatTG = string.Empty;

		public static bool startAutoItem = false;

		private long lastAutoChat;

		private long lastAutoChatTG;

		public static bool isFilterItem = false;

		public static bool isAutoFilterItem = false;

		public static List<ItemAutoFilter> listFilterItems = new List<ItemAutoFilter>();

		public static bool isShowFilterList = false;

		private bool isResizing;

		private int lastMouseX;

		private int lastMouseY;

		private int lastPanelX;

		private int lastPanelY;

		private int lastPanelW;

		private int lastPanelH;

		private int lastScrollY;

		private bool isScrolling;

		private int scrollY;

		private readonly int MAX_ITEMS_VISIBLE = 10;

		private long lastFilterTime;

		public static bool notifBoss = true;

		public static bool notifKillBoss = true;

		private bool lineToBoss;

		private bool focusBoss;

		private long lastFocusBoss;

		public static MyVector activeBossNotif = new MyVector();

		public static MyVector killedBossNotif = new MyVector();

		public bool showCharsInMap = true;

		public bool userOpenZones;

		public bool isUpdateZones = false;

		private long lastUpdateZones;

		public MyVector charsInMap = new MyVector();

		public static int zoneMacDinh;

		public static bool isdoBoss;

		private static long currDoBoss;

		public static string bossCanDo;

		public Item itemPhale;

		public int maxPhale = -1;

		public int currPhale = -1;

		public bool isCollectAll;

		public bool isPaintThuongDe;

		public bool isOpenThuongDe;

		private static int currentPage = 0;

		private int ChiSoNoiTai = -1;

		public string curSelectIntrinsic = "";

		private string CurrentNoiTai = "";

		private string currentPlayerNoiTai = "";

		public MyVector listNotifTichXanh = new MyVector();

		private bool startChat;

		private int xNotif;

		private long lastUpdateNotif;

		public bool isPeanPet;

		private long lastPeanPet;

		public static bool autoPointForPet = false;

		public static bool userOpenPet = false;

		public static bool hienthiBackground = true;

		public static int indexAutoPoint = -1;

		public static int pointIncrease = 0;

		public bool showInfoMe;

		private long lastUpdateInfoMe;

		public bool isShowButton = true;

		public bool isIntroOff;

		public static bool isInventory = true;

		public static bool isEffectInven = false;

	 public static bool isLogo = true;
		// public static bool isLogo = Management.isLogo;

		public static bool isLogoGif = false;

		public static bool GiamDungLuong = false;

		public static bool AnPlayer = false;

		public static bool isHideBadges = false;

		public bool isHighFps;

		public static bool isShowID = false;

		private static int isfpscao;

		private static int FrameGif = 54;

		private static int FrameGifMenu = 16;

		public static Image[] ticks = new Image[20];

		private static Image logo = new Image();

		private static Image[] logos = new Image[55];

		private static Image[] logosMenu = new Image[FrameGifMenu];

		public static Image imgLogoBig = null;

		public static Image imgBg = null;

		public static bool isShortOptionTemp = false;

		public static Image imgMenuChat = null;

		public static Image imgCloseButton = null;

		public static Image imgNextPage = null;

		public static Image imgNextPage2 = null;

		public static Image imgPrevPage = null;

		public static Image imgPrevPage2 = null;

		public static int musicCount = 0;

		public static bool loadedMusic = false;

		public static bool isPlayingMusic = false;

		public static List<AudioClip> musics = new List<AudioClip>();

		private static string backgroundColor = "0.6 0.8 0.9";

		public static string strBossNotif = "Thông báo Boss";

		public static bool isEditButton = false;

		private static Dictionary<string, Point> buttonPositions = new Dictionary<string, Point>();

		private static string selectedButton = null;

		private static Point dragStart = null;

		private static bool isDragging = false;

		public static string ipServer = "Đổi IP";

		public static bool isLockFocus = false;

		public static string strAddAutoItem = "Thêm vào\nAutoItem";

		public static string strRemoveAutoItem = "Xoá khỏi\nAutoItem";

		public static string strAddFilterItem = "Thêm vào\nDS lọc";

		public static string strRemoveFilterItem = "Xóa khỏi\nDS lọc";

		public static string strTeleportTo = "Dịch\nchuyển tới";

		public static string strAutoBuy = "Mua 20 lần";

		public static string strAutoBuy50 = "Mua 50 lần";

		public static string strAutoBuy100 = "Mua 100 lần";

		public static string strAutoBuy200 = "Mua 1000 lần";

		public static string strChooseIntrinsic = "Chọn chỉ số";

		public static string strInCrease = "Tăng\ntới\nmức";

		public static MyVector bossNotif = new MyVector();

		public static string[] strPointTypes = new string[5] { "HP", "MP", "Sức Đánh", "Giáp", "Chí mạng" };

		public static string strAccManager = "Q.L.T.K";

		public static string strModFunc = "Chức Năng MOD";

		public static string strUpdateZones = "Cập Nhật Khu";

		public static string strCharsInMap = "Nhân Vật Trong Khu";

		public static string strInfoMe = "Thông Tin Bản Thân";

		public static string strAutoPhaLe = "Tự Động Pha Lê Hóa";

		public static string strAutoVQMM = "Tự Động VQMM";

		public static string strAutoWakeUp = "Tự Động Hồi Sinh";

		public static string strAutoLogin = "Tự Động Đăng Nhập";

		public static string strShowButton = "Hiện Nút Trợ Năng";

		public static string strIntroOff = "Tắt Intro";

		public static string strInventoryOFF = "Hiện Hành Trang Lưới";

		public static string strEffectOff = "Hiệu Ứng Hành Trang";

		public static string strHighFps = "FPS Cao";

		public static string strClickToChat = " [Ấn để chat]";

		public static string strPlayerInfo = "Thông tin player";

		public static string strPet2 = "Người iuu";

		public static string strUseForPet2 = "Sử dụng\ncho\nNg.iuu";

		public static string strLogo = "Ẩn / hiện Logo";

		public static string strGiamDungLuong = "Giảm Dung Lượng";

		public static string strAnPlayer = "Ẩn Player";

		public static string strHideBadges = "Ẩn Danh Hiệu";

		public static string strLogoGif = "Logo động";

		public static string strShowID = "Hiện ID Item/NPC";

		public static string strEditButton = "Chỉnh sửa nút";

		public static string strVietnamese = "Gõ Tiếng Việt";

		public static string strShowMenuChat = "Thông Tin Lệnh Chat";

		private static readonly Dictionary<string, Point> defaultButtonPositions = new Dictionary<string, Point>
		{
			{
				"Capsule",
				new Point(20, -26)
			},
			{
				"Fusion",
				new Point(-21, 21)
			},
			{
				"Zone",
				new Point(-66, 62)
			},
			{
				"MapLeft",
				new Point(-106, 62)
			},
			{
				"MapCenter",
				new Point(-66, 21)
			},
			{
				"MapRight",
				new Point(-21, -26)
			},
			{
				"tanSatquai",
				new Point(-110, 21)
			}
		};

		private static int panelX = GameCanvas.w / 3 + 25;

		private static int panelY = 15;

		private static int panelW = 200;

		private static int panelH = 170;

		public static void InitButtonPositions()
		{
			if (buttonPositions.Count != 0)
			{
				return;
			}
			foreach (KeyValuePair<string, Point> defaultButtonPosition in defaultButtonPositions)
			{
				buttonPositions[defaultButtonPosition.Key] = new Point(defaultButtonPosition.Value.x, defaultButtonPosition.Value.y);
			}
		}

		public static ModFunc GI()
		{
			return Instance ?? new ModFunc();
		}

		public void OpenMenu()
		{
			MyVector myVector = new MyVector();
			myVector.addElement(new Command("Bộ sưu tập", 59));
			myVector.addElement(new Command("Bản đồ", 883));
			myVector.addElement(new Command("Luyện tập", 45));
			myVector.addElement(new Command("Nhặt đồ", 89));
			myVector.addElement(new Command("Đệ tử", 16));
			myVector.addElement(new Command("BOSS", 32));
			myVector.addElement(new Command("Khác", 53));
			GameCanvas.menu.startAt(myVector, 4);
		}

		public static Color GetColor()
		{
			string[] array = backgroundColor.Split(' ');
			return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
		}

		public bool UpdateKey(int key)
		{
			switch (key)
			{
			case 97:
				MoveTo(Char.myCharz().cx - 100, Char.myCharz().cy);
				return true;
			case 98:
				GameScr.gI().onChatFromMe("boss", string.Empty);
				return true;
			case 99:
				UseItem(194);
				return true;
			case 100:
				MoveTo(Char.myCharz().cx + 100, Char.myCharz().cy);
				return true;
			case 101:
				Service.gI().friend(0, -1);
				InfoDlg.showWait();
				return true;
			case 102:
				UsePorata();
				return true;
			case 103:
				if (Char.myCharz().charFocus != null)
				{
					Service.gI().giaodich(0, Char.myCharz().charFocus.charID, -1, -1);
					GameScr.info1.addInfo("Đã gửi lời mời giao dịch đến " + Char.myCharz().charFocus.cName, 0);
					return true;
				}
				return true;
			case 104:
				GameScr.gI().onChatFromMe("ukhu", string.Empty);
				return true;
			case 106:
				ManualXmap.GI().LoadMapLeft();
				return true;
			case 107:
				ManualXmap.GI().LoadMapCenter();
				return true;
			case 108:
				ManualXmap.GI().LoadMapRight();
				return true;
			case 109:
				userOpenZones = true;
				Service.gI().openUIZone();
				return true;
			case 110:
				PickMob.IsAutoPickItems = !PickMob.IsAutoPickItems;
				GameScr.info1.addInfo("Tự động nhặt: " + (PickMob.IsAutoPickItems ? "Bật" : "Tắt"), 0);
				return true;
			case 115:
				MoveTo(Char.myCharz().cx, Char.myCharz().cy + 100);
				return true;
			case 116:
				UseItem(521);
				return true;
			case 117:
				perform(42, null);
				return true;
			case 119:
				MoveTo(Char.myCharz().cx, Char.myCharz().cy - 100);
				return true;
			case 120:
				OpenMenu();
				return true;
			default:
				return false;
			}
		}

		public void LoadGame()
		{
			if (!loadedMusic)
			{
				InitMusic();
				loadedMusic = true;
			}
			Time.timeScale = 2f;
			isfpscao = 60;
			listSkillsAuto.Clear();
			listItemAuto.Clear();
			isHighFps = true;
			int inventorySetting = Rms.loadRMSInt("inventory");
			int inventoryEffectSetting = Rms.loadRMSInt("effectinven");
			isInventory = inventorySetting == -1 || inventorySetting == 1;
			isEffectInven = inventoryEffectSetting == -1 || inventoryEffectSetting == 1;
			GiamDungLuong = Rms.loadRMSInt("background") == 1;
			AnPlayer = Rms.loadRMSInt("anplayer") == 1;
			isHideBadges = Rms.loadRMSInt("hideBadges") == 1;
			autoWakeUp = Rms.loadRMSInt("autoWakeUp") == 1;
			if (Rms.loadRMSInt("new logo") != 1)
			{
				Rms.saveRMSInt("logoGif", 1);
				Rms.saveRMSInt("logo", 1);
				Rms.saveRMSInt("new logo", 1);
			}
			if (!ModNotLogo)
			{
				isLogo = Rms.loadRMSInt("logo") == 1;
				isLogoGif = Rms.loadRMSInt("logoGif") == 1;
				if (isLogo)
				{
					_ = isLogoGif;
				}
			}
			ChangeFPSTarget();
			if (autoWakeUp)
			{
				GameScr.info1.addInfo("Tự động hồi sinh [Bật]", 0);
			}
			LoadButtonPositions();
			
		}

		public void MoveTo(int x, int y)
		{
			Char.myCharz().cx = x;
			Char.myCharz().cy = y;
			Service.gI().charMove();
			if (!ItemTime.isExistItem(4387))
			{
				Char.myCharz().cx = x;
				Char.myCharz().cy = y + 1;
				Service.gI().charMove();
				Char.myCharz().cx = x;
				Char.myCharz().cy = y;
				Service.gI().charMove();
			}
		}

		public void GotoNpc(int npcID)
		{
			for (int i = 0; i < GameScr.vNpc.size(); i++)
			{
				Npc npc = (Npc)GameScr.vNpc.elementAt(i);
				if (npc.template.npcTemplateId == npcID && Math.abs(npc.cx - Char.myCharz().cx) >= 50)
				{
					MoveTo(npc.cx, npc.cy - 1);
					Char.myCharz().FocusManualTo(npc);
					break;
				}
			}
		}

		public int FindItemIndex(int idItem)
		{
			if (Char.myCharz().arrItemBag == null)
			{
				return -1;
			}
			for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
			{
				if (Char.myCharz().arrItemBag[i] != null && Char.myCharz().arrItemBag[i].template.id == idItem)
				{
					return Char.myCharz().arrItemBag[i].indexUI;
				}
			}
			return -1;
		}

		private void AttackChar()
		{
			try
			{
				MyVector myVector = new MyVector();
				myVector.addElement(Char.myCharz().charFocus);
				Service.gI().sendPlayerAttack(new MyVector(), myVector, 2);
			}
			catch
			{
			}
		}

		public void AttackMob(Mob mob)
		{
			try
			{
				MyVector myVector = new MyVector();
				myVector.addElement(mob);
				Service.gI().sendPlayerAttack(myVector, new MyVector(), 1);
			}
			catch
			{
			}
		}

		public void AutoAttack()
		{
			Char @char = Char.myCharz();
			if (!Char.isLoadingMap && !@char.stone && !@char.meDead && @char.statusMe != 14 && @char.statusMe != 5 && @char.myskill.template.type == 1 && @char.myskill.template.id != 10 && @char.myskill.template.id != 11 && !@char.myskill.paintCanNotUseSkill && mSystem.currentTimeMillis() - lastAutoAttack > 500)
			{
				if (GameScr.gI().isMeCanAttackMob(@char.mobFocus) && Res.abs(@char.mobFocus.xFirst - @char.cx) < @char.myskill.dx * 2)
				{
					AttackMob(@char.mobFocus);
					SetUsedSkill(@char.myskill);
				}
				else if (@char.isMeCanAttackOtherPlayer(@char.charFocus) && Res.abs(@char.charFocus.cx - @char.cx) < @char.myskill.dx * 2)
				{
					AttackChar();
					SetUsedSkill(@char.myskill);
				}
				lastAutoAttack = mSystem.currentTimeMillis();
			}
		}

		public void SetUsedSkill(Skill skill)
		{
			skill.paintCanNotUseSkill = true;
			skill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
		}

		public void UsePorata()
		{
            for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
            {
                Item item = Char.myCharz().arrItemBag[i];
				if (item == null)
				{
					GameScr.info1.addInfo("Nghèo Đéo Có Bông Tai", 0);
					return;
				}
                if (item.template.name.Contains("Bông tai") || item.template.name.Contains("Bông Tai")
                    || item.template.name.Contains("Porata") || item.template.name.Contains("porata"))
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    break;
                }
            }
        }

		public void AutoFocusBoss()
		{
			for (int i = 0; i < GameScr.vCharInMap.size(); i++)
			{
				Char @char = (Char)GameScr.vCharInMap.elementAt(i);
				if (@char != null && @char.charID < 0 && @char.cTypePk == 5 && !@char.cName.StartsWith("Đ"))
				{
					Char.myCharz().FocusManualTo(@char);
					break;
				}
			}
		}

		public int GetMapID(string mapName)
		{
			int result = -1;
			for (int i = 0; i < XmapController.mapNames.Length; i++)
			{
				if (XmapController.mapNames[i].Trim().ToLower().Equals(mapName.Trim().ToLower()))
				{
					result = i;
				}
			}
			return result;
		}

		private string CharGender(Char @char)
		{
			if (@char.cTypePk == 5)
			{
				return "BOSS";
			}
			if (@char.cgender == 0)
			{
				return "TĐ";
			}
			if (@char.cgender == 1)
			{
				return "NM";
			}
			if (@char.cgender == 2)
			{
				return "XD";
			}
			return "";
		}

		public void UseItem(int itemId)
		{
			int num = FindItemIndex(itemId);
			if (num != -1)
			{
				Service.gI().useItem(0, 1, (sbyte)num, -1);
			}
			else
			{
				GameScr.info1.addInfo("Không tìm thấy vật phẩm", 0);
			}
		}

		public void UseItemAuto()
		{
			if (listItemAuto.Count <= 0 || !startAutoItem)
			{
				if (!startAutoItem)
				{
					System.Threading.Tasks.Task.Delay(200).ContinueWith((System.Threading.Tasks.Task t) => startAutoItem = true);
				}
				return;
			}
			for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
			{
				Item item = Char.myCharz().arrItemBag[i];
				if (item == null)
				{
					continue;
				}
				foreach (ItemAuto item2 in listItemAuto)
				{
					if (item.template.iconID == item2.iconID && item.template.id == item2.id && !ItemTime.isExistItem(item.template.iconID))
					{
						Service.gI().useItem(0, 1, (sbyte)FindItemIndex(item.template.id), -1);
						if (listItemAuto.Count == 1)
						{
							return;
						}
						break;
					}
				}
			}
		}

		private void AutoHoiSinh()
		{
			if (Char.myCharz().cHP <= 0 || Char.myCharz().meDead || Char.myCharz().statusMe == 14)
			{
				Service.gI().wakeUpFromDead();
			}
		}

		public static int GetCurrPhaLe(Item item)
		{
			for (int i = 0; i < item.itemOption.Length; i++)
			{
				if (item.itemOption[i].optionTemplate.id == 107)
				{
					return item.itemOption[i].param;
				}
			}
			return 0;
		}

		public void AutoPhaLe()
		{
			while (isAutoPhaLe)
			{
				if (TileMap.mapID != 5)
				{
					GameScr.info1.addInfo("Cần đến Đảo Kame để sử dụng Tự động Pha lê hóa", 0);
					Thread.Sleep(500);
					break;
				}
				if (currPhale >= maxPhale && itemPhale != null && currPhale >= 0 && maxPhale > 0)
				{
					Sound.start(1f, Sound.l1);
					GameScr.info1.addInfo("Đã đạt đến số sao yêu cầu", 0);
					maxPhale = -1;
					itemPhale = null;
				}
				if (Char.myCharz().xu > 10000000000L)
				{
					GotoNpc(21);
					if (itemPhale != null && maxPhale > 0)
					{
						while (!GameCanvas.menu.showMenu)
						{
							Service.gI().combine(1, GameCanvas.panel.vItemCombine);
							Thread.Sleep(100);
						}
						Service.gI().confirmMenu(21, 0);
						GameCanvas.menu.doCloseMenu();
						GameCanvas.panel.currItem = null;
						GameCanvas.panel.chatTField.isShow = false;
					}
				}
				else if (itemPhale != null)
				{
					BanVang();
				}
				Thread.Sleep(500);
			}
		}

		private void BanVang()
		{
			if (TileMap.mapID != 5)
			{
				GameScr.info1.addInfo("Cần đến Đảo Kame để Tự động bán vàng", 0);
				Thread.Sleep(1000);
				return;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				GameScr.info1.addInfo("Dừng bán vàng", 0);
				return;
			}
			while (Char.myCharz().xu <= 60000000000L && !Input.GetKey(KeyCode.Q))
			{
				if (FindItemIndex(457) == -1)
				{
					GameScr.info1.addInfo("Không tìm thấy thỏi vàng", 0);
					if (isAutoPhaLe)
					{
						isAutoPhaLe = false;
						GameScr.info1.addInfo("Vàng không đủ, đã tắt Tự động Pha lê hóa", 0);
					}
					return;
				}
				Service.gI().useItem(0, 1, (sbyte)FindItemIndex(457), -1);
				GameScr.info1.addInfo("Đang bán thỏi vàng", 0);
				Thread.Sleep(500);
			}
			GameScr.info1.addInfo("Đã bán xong", 0);
			Thread.Sleep(500);
		}

		public static Item FindItemBagWithIndexUI(int index)
		{
			Item[] arrItemBag = Char.myCharz().arrItemBag;
			foreach (Item item in arrItemBag)
			{
				if (item != null && item.indexUI == index)
				{
					return item;
				}
			}
			return null;
		}

		public void CollectAllThuongDe()
		{
			isCollectAll = true;
			Service.gI().openMenu(19);
			Service.gI().confirmMenu(19, 2);
			Service.gI().confirmMenu(19, 1);
			Service.gI().buyItem(2, 0, 0);
			Thread.Sleep(2000);
			isCollectAll = false;
		}

		private void OpenMenuThuongDe()
		{
			isOpenThuongDe = true;
			Service.gI().openMenu(19);
			Service.gI().confirmMenu(19, 2);
			Service.gI().confirmMenu(19, 0);
			isOpenThuongDe = false;
		}

		public void quayThuongDe()
		{
			if (isCollectAll || isOpenThuongDe)
			{
				return;
			}
			if (!isPaintThuongDe && TileMap.mapID == 45)
			{
				OpenMenuThuongDe();
			}
			else if (TileMap.mapID == 45)
			{
				if (Input.GetKey("q") || Char.myCharz().xu <= 200000000)
				{
					GameScr.info1.addInfo("Đã tắt Auto VQMM (2)", 0);
					isAutoVQMM = false;
				}
				else
				{
					Service.gI().openMenu(19);
					Service.gI().SendCrackBall(2, 7);
				}
			}
		}

		public bool Chat(string text)
		{
			switch (text)
			{
				case "bpa":
					SoundMn.gI().analogToolOption();
					return true;
			case "loadskill":
				perform(57, null);
				return true;
			case "ak":
				perform(42, null);
				return true;
			case "ts":
				perform(44, null);
				return true;
			case "tsnguoi":
				perform(48, null);
				return true;
			case "vqmm":
				isPaintThuongDe = false;
				isAutoVQMM = !isAutoVQMM;
				GameScr.info1.addInfo("Auto VQMM: " + (isAutoVQMM ? "Bật" : "Tắt"), 0);
				return true;
			case "ukhu":
				isUpdateZones = !isUpdateZones;
				GameScr.info1.addInfo("Tự động cập nhật khu: " + (isUpdateZones ? "Bật" : "Tắt"), 0);
				return true;
			default:
				if (text.StartsWith("k "))
				{
					if (int.TryParse(text.Replace("k ", ""), out var result) && result >= 0)
					{
						Service.gI().requestChangeZone(result, -1);
					}
					return true;
				}
				if (text.StartsWith("s "))
				{
					ChangeGameSpeed(text.Replace("s ", ""));
					return true;
				}
				if (text.StartsWith("atc "))
				{
					textAutoChat = text.Replace("atc ", "");
					return true;
				}
				if (text.StartsWith("atctg "))
				{
					textAutoChatTG = text.Replace("atctg ", "");
					return true;
				}
				if (text.StartsWith("do "))
				{
					bossCanDo = text.Replace("do ", "");
					GameScr.info1.addInfo("Boss cần dò: " + bossCanDo, 0);
					return true;
				}
				if (text == "dbx")
				{
					isdoBoss = !isdoBoss;
					GameScr.info1.addInfo("Tự động dò boss: " + (isdoBoss ? "Bật" : "Tắt"), 0);
					return true;
				}
				if (text == "gtv")
				{
					isVietnamese = !isVietnamese;
					GameScr.info1.addInfo("Gõ Tiếng Việt: " + (isVietnamese ? "Bật" : "Tắt"), 0);
					return true;
				}
				return false;
			}
		}

		private void UpdateTouch()
		{
			if (GameScr.gI().isNotPaintTouchControl())
			{
				return;
			}
			if (GameCanvas.isPointerHoldIn(GameScr.imgPanel.getWidth() + 8, 3, GameScr.imgModFunc.getWidth() + 2, GameScr.imgModFunc.getHeight() + 2) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
			{
				OpenMenu();
				SoundMn.gI().buttonClick();
				GameCanvas.clearAllPointerEvent();
			}
			if (!isEditButton)
			{
				foreach (KeyValuePair<string, Point> buttonPosition in buttonPositions)
				{
					int num = modKeyPosX + buttonPosition.Value.x;
					int num2 = modKeyPosY + buttonPosition.Value.y;
					if (GameCanvas.isPointerHoldIn(num - 16, num2 - 16, 32, 32) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
					{
						switch (buttonPosition.Key)
						{
						case "Zone":
							userOpenZones = true;
							Service.gI().openUIZone();
							break;
						case "Fusion":
							UsePorata();
							break;
						}
						GameCanvas.clearAllPointerEvent();
						break;
					}
				}
				return;
			}
			int num3 = 60;
			int h = 24;
			int num4 = 10;
			int y = 40;
			int x = GameCanvas.w / 2 - num3 - num4 / 2;
			int x2 = GameCanvas.w / 2 + num4 / 2;
			if (GameCanvas.isPointerHoldIn(x, y, num3, h) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
			{
				SaveButtonPositions();
				isEditButton = false;
				GameScr.info1.addInfo("Đã lưu vị trí các nút", 0);
				GameCanvas.clearAllPointerEvent();
			}
			else if (GameCanvas.isPointerHoldIn(x2, y, num3, h) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
			{
				buttonPositions.Clear();
				InitButtonPositions();
				SaveButtonPositions();
				GameScr.info1.addInfo("Đã reset vị trí các nút về mặc định", 0);
				GameCanvas.clearAllPointerEvent();
			}
			else if (GameCanvas.isPointerDown)
			{
				if (!isDragging)
				{
					foreach (KeyValuePair<string, Point> buttonPosition2 in buttonPositions)
					{
						int num5 = modKeyPosX + buttonPosition2.Value.x;
						int num6 = modKeyPosY + buttonPosition2.Value.y;
						if (GameCanvas.isPointerHoldIn(num5 - 16, num6 - 16, 32, 32))
						{
							selectedButton = buttonPosition2.Key;
							dragStart = new Point(GameCanvas.px - num5, GameCanvas.py - num6);
							isDragging = true;
							GameCanvas.isPointerJustDown = false;
							break;
						}
					}
					return;
				}
				if (selectedButton != null)
				{
					int val = GameCanvas.px - modKeyPosX - dragStart.x;
					int val2 = GameCanvas.py - modKeyPosY - dragStart.y;
					val = System.Math.Max(-modKeyPosX + 20, System.Math.Min(GameCanvas.w - modKeyPosX - 25, val));
					val2 = System.Math.Max(-modKeyPosY + 20, System.Math.Min(GameCanvas.h - modKeyPosY - 25, val2));
					buttonPositions[selectedButton] = new Point(val, val2);
				}
			}
			else if (isDragging)
			{
				SaveButtonPositions();
				isDragging = false;
				selectedButton = null;
				dragStart = null;
				GameCanvas.clearAllPointerEvent();
			}
		}

		public void Update()
		{
			UpdateTouch();
			AutoItem.Update();
			ShowBoss.UpdateNotifications();
			long num = mSystem.currentTimeMillis();
			if (isPeanPet && num - lastPeanPet >= 3000)
			{
				Char @char = Char.myPetz();
				if (!@char.isDie && (@char.cStamina <= @char.cMaxStamina * 20 / 100 || @char.cHP < @char.cHPFull * 20 / 100 || @char.cMP < @char.cMPFull * 20 / 100))
				{
					GameScr.gI().doUseHP();
					lastPeanPet = num;
				}
			}
			if (isAutoPhaLe && itemPhale != null)
			{
				currPhale = GetCurrPhaLe(FindItemBagWithIndexUI(itemPhale.indexUI));
			}
			else
			{
				currPhale = -1;
			}
			if (isAutoChat && num - lastAutoChat >= 4000)
			{
				AutoChat();
				lastAutoChat = num;
			}
			if (isAutoChatTG && num - lastAutoChatTG >= 30000)
			{
				AutoChatTG();
				lastAutoChatTG = num;
			}
			if (!TileMap.isOfflineMap() && mSystem.currentTimeMillis() - lastUpdateZones >= 1000)
			{
				UseItemAuto();
				if (isUpdateZones)
				{
					Service.gI().openUIZone();
				}
				lastUpdateZones = mSystem.currentTimeMillis();
			}
			if (isAutoVQMM && num - lastVQMM >= 1000)
			{
				quayThuongDe();
				lastVQMM = num;
			}
			if (autoWakeUp && num - lastAutoWakeUp >= 1000)
			{
				AutoHoiSinh();
				lastAutoWakeUp = num;
			}
			if (focusBoss && num - lastFocusBoss >= 500)
			{
				AutoFocusBoss();
				lastFocusBoss = num;
			}
			if (autoAttack)
			{
				AutoAttack();
			}
			UpdateNotifTichXanh();
			if (isAutoNoitai && Input.GetKey("q"))
			{
				isAutoNoitai = false;
				ChiSoNoiTai = -1;
				curSelectIntrinsic = "";
				GameScr.info1.addInfo("Đã dừng auto mở nội tại", 0);
			}
			if (isAutoFilterItem && num - lastFilterTime >= 500)
			{
				DoFilter();
				lastFilterTime = num;
			}
			if (isdoBoss && mSystem.currentTimeMillis() - currDoBoss >= 1000)
			{
				DoBoss();
				currDoBoss = mSystem.currentTimeMillis();
			}
		}

        public static Image imgCapsule;

        public static Image imgCapsuleF;

        public static Image imgChangeZone;

        public static Image imgChangeZoneF;

        public static Image imgFusion;

        public static Image imgFusionF;

        public static Image imgNextRight;

        public static Image imgNextRightF;

        public static Image imgNextLeft;

        public static Image imgNextLeftF;

        public static Image imgNextCenter;

        public static Image imgNextCenterF;
        public static void LoadImgBtn()
		{
            imgCapsule = GameCanvas.loadImage("/button/btnCapsule.png");
            imgCapsuleF = GameCanvas.loadImage("/button/btnCapsuleF.png");
            imgChangeZone = GameCanvas.loadImage("/button/zone.png");
            imgChangeZoneF = GameCanvas.loadImage("/button/zoneF.png");
            imgFusion = GameCanvas.loadImage("/button/btnPorata.png");
            imgFusionF = GameCanvas.loadImage("/button/btnPorataF.png");
            imgNextRight = GameCanvas.loadImage("/button/btnNextMap.png");
            imgNextRightF = GameCanvas.loadImage("/button/btnNextMapF.png");
            imgNextCenter = GameCanvas.loadImage("/button/btnMidMap.png");
            imgNextCenterF = GameCanvas.loadImage("/button/btnMidMapF.png");
            imgNextLeft = GameCanvas.loadImage("/button/btnPreMap.png");
            imgNextLeftF = GameCanvas.loadImage("/button/btnPreMapF.png");
        }
		public void PaintButton(mGraphics g, int xAnchor, int yAnchor)
		{
			if (!isShowButton || GameCanvas.currentDialog != null || ChatPopup.currChatPopup != null || GameCanvas.menu.showMenu || GameScr.gI().isPaintPopup() || GameCanvas.panel.isShow || Char.myCharz().taskMaint.taskId == 0 || ChatTextField.gI().isShow || GameCanvas.currentScreen == MoneyCharge.instance)
			{
				return;
			}
			modKeyPosX = xAnchor;
			modKeyPosY = yAnchor;
			InitButtonPositions();
			foreach (KeyValuePair<string, Point> buttonPosition in buttonPositions)
			{
				string key = buttonPosition.Key;
				Point value = buttonPosition.Value;
				int num = xAnchor + value.x;
				int num2 = yAnchor + value.y;
				switch (key)
				{
				case "Capsule":
					g.drawImage(imgCapsule, num - 70, num2 + 60, mGraphics.HCENTER | mGraphics.VCENTER);
					if (GameCanvas.isPointerHoldIn(num - 70 - 15, num2 + 60 - 15, 30, 30) && GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
					{
						g.drawImage(imgCapsuleF, num - 70, num2 + 60, mGraphics.HCENTER | mGraphics.VCENTER);
                            UseItem(194);
                            GameCanvas.clearAllPointerEvent();
                        }
					break;
				case "Fusion":
					g.drawImage(imgFusion, num, num2, mGraphics.HCENTER | mGraphics.VCENTER);
					if (GameCanvas.isPointerHoldIn(num - 15, num2 - 15, 30, 30))
					{
						g.drawImage(imgFusionF, num, num2, mGraphics.HCENTER | mGraphics.VCENTER);
					}
					break;
				case "Zone":
					g.drawImage(imgChangeZone, num, num2, mGraphics.HCENTER | mGraphics.VCENTER);
					if (GameCanvas.isPointerHoldIn(num - 15, num2 - 15, 30, 30))
					{
						g.drawImage(imgChangeZoneF, num, num2, mGraphics.HCENTER | mGraphics.VCENTER);
					}
					break;
				case "MapLeft":
					g.drawImage(imgNextLeft, num + 15, num2 - 20, mGraphics.HCENTER | mGraphics.VCENTER);
					if (GameCanvas.isPointerHoldIn(num + 15 - 15, num2 - 20 - 15, 30, 30))
					{
                            ManualXmap.GI().LoadMapLeft();
                            g.drawImage(imgNextLeftF, num + 15, num2 - 20, mGraphics.HCENTER | mGraphics.VCENTER);
					}
					break;
				case "MapCenter":
					g.drawImage(imgNextCenter, num - 10, num2 - 10, mGraphics.HCENTER | mGraphics.VCENTER);
					if (GameCanvas.isPointerHoldIn(num - 10 - 15, num2 - 10 - 15, 30, 30))
					{
                            ManualXmap.GI().LoadMapCenter();
                            g.drawImage(imgNextCenterF, num - 10, num2 - 10, mGraphics.HCENTER | mGraphics.VCENTER);
					}
					break;
				case "MapRight":
					g.drawImage(imgNextRight, num - 25, num2 + 20, mGraphics.HCENTER | mGraphics.VCENTER);
					if (GameCanvas.isPointerHoldIn(num - 25 - 15, num2 + 20 - 15, 30, 30))
					{
                            ManualXmap.GI().LoadMapRight();
                            g.drawImage(imgNextRightF, num - 25, num2 + 20, mGraphics.HCENTER | mGraphics.VCENTER);
					}
					break;
				}
			}
		}

		public void Paint(mGraphics g)
		{
			g.drawImage(imgLogoBig, GameCanvas.w / 2, 30, 3);
			int imageWidth = mGraphics.getImageWidth(GameScr.imgHP);
			int imageWidth2 = mGraphics.getImageWidth(GameScr.imgMP);
			mFont.tahoma_7_red.drawStringBorder(g, NinjaUtil.getMoneys(Char.myCharz().cHP), 84 + imageWidth / 2, 4, mFont.CENTER, mFont.tahoma_7_grey);
			mFont.tahoma_7_blue1.drawStringBorder(g, NinjaUtil.getMoneys(Char.myCharz().cMP), 84 + imageWidth2 / 2, 17, mFont.CENTER, mFont.tahoma_7_grey);
			int num = 90;
			int num2 = GameScr.gI().cmdMenu.y - 20;
			if (!showInfoMe && !isEditButton && !isAutoNoitai)
			{
			    mFont.tahoma_7_white.drawStringBorder(g, TileMap.mapName + "  - K" + TileMap.zoneID, num - 5, num2, mFont.LEFT, mFont.tahoma_7_grey);
			}
			if (isAutoPhaLe && !isEditButton)
			{
				mFont.tahoma_7b_red.drawString(g, (itemPhale != null) ? itemPhale.template.name : "Chưa Có", GameCanvas.w / 2, 72, mFont.CENTER);
				mFont.tahoma_7b_red.drawString(g, (itemPhale != null) ? ("Số Sao : " + currPhale) : "Số Sao : -1", GameCanvas.w / 2, 82, mFont.CENTER);
				mFont.tahoma_7b_red.drawString(g, "Số Sao Cần Đập : " + maxPhale + " Sao", GameCanvas.w / 2, 92, mFont.CENTER);
			}
			if ((isAutoPhaLe || isAutoVQMM) && !isEditButton)
			{
				Item item = FindItemBagWithIndexUI(FindItemIndex(457));
				mFont.tahoma_7b_red.drawString(g, "Ngọc Xanh : " + NinjaUtil.getMoneys(Char.myCharz().luong) + " Ngọc Hồng : " + NinjaUtil.getMoneys(Char.myCharz().luongKhoa), GameCanvas.w / 2, 102, mFont.CENTER);
				mFont.tahoma_7b_red.drawString(g, "Vàng : " + NinjaUtil.getMoneys(Char.myCharz().xu) + " Thỏi Vàng : " + (item?.quantity ?? 0), GameCanvas.w / 2, 112, mFont.CENTER);
			}
			if (showInfoMe && !isEditButton)
			{
				PaintInfoMe(g, num, num2);
			}
			PaintListInfo(g);
			if (lineToBoss)
			{
				for (int j = 0; j < GameScr.vCharInMap.size(); j++)
				{
					Char @char = (Char)GameScr.vCharInMap.elementAt(j);
					if (@char != null && @char.cTypePk == 5 && !@char.cName.StartsWith("Đ"))
					{
						g.setColor(Color.red);
						g.drawLine(Char.myCharz().cx - GameScr.cmx, Char.myCharz().cy - GameScr.cmy, @char.cx - GameScr.cmx, @char.cy - GameScr.cmy);
					}
				}
			}
			g.drawImage(GameScr.imgModFunc, GameScr.imgPanel.getWidth() + 20, 15, 3);
			PaintPlayerTichXanh(g);
			if (isEditButton)
			{
				PaintEditButton(g);
			}
			if (isShowFilterList)
			{
				ShowFilterList(g);
			}
			if (isShowMenuChat)
			{
				PaintMenuChat(g);
			}
			int num4 = 52;
			if (notifBoss)
			{
				int num5 = 0;
				for (int k = 0; k < bossNotif.size(); k++)
				{
					ShowBoss showBoss = (ShowBoss)bossNotif.elementAt(k);
					showBoss.PaintBoss(g, GameCanvas.w - 5, num4);
					if (showBoss.stringLength > num5)
					{
						num5 = showBoss.stringLength;
					}
					num4 += 10;
				}
				if (num5 != 0)
				{
					int widthExactOf = mFont.tahoma_7_white.getWidthExactOf(strBossNotif);
					g.drawRegion(Mob.imgHP, 0, 18, 9, 6, 4, GameCanvas.w - 12 - widthExactOf, 46, mGraphics.VCENTER | mGraphics.HCENTER);
					g.setColor(Color.yellow);
					g.DrawLine(GameCanvas.w - 16 - widthExactOf, 46, GameCanvas.w - num5 - 11, 46, 3);
					g.DrawLine(GameCanvas.w - num5 - 10, 45, GameCanvas.w - num5 - 10, num4 + 6, 3);
					g.DrawLine(GameCanvas.w - num5 - 11, num4 + 5, GameCanvas.w, num4 + 5, 3);
					g.DrawLine(GameCanvas.w, 46, GameCanvas.w - 5, 46, 3);
					g.FillRect(GameCanvas.w - 8 - widthExactOf, 42, widthExactOf + 2, mFont.tahoma_7_white.getHeight() - 2, 0, 0.8f);
					mFont.tahoma_7_white.drawString(g, strBossNotif, GameCanvas.w - 5, 40, mFont.RIGHT);
				}
				num4 += 20;
			}
			if (!showCharsInMap)
			{
				return;
			}
			int num6 = num4 - 12;
			int num7 = 0;
			int num8 = GameCanvas.w - 2;
			charsInMap.removeAllElements();
			int widthExactOf2;
			for (int l = 0; l < GameScr.vCharInMap.size(); l++)
			{
				Char char2 = (Char)GameScr.vCharInMap.elementAt(l);
				if (char2.Equals(Char.myCharz()))
				{
					continue;
				}
				if (num4 > GameScr.yHP - 40)
				{
					mFont.tahoma_7_white.drawStringBorder(g, "...", num8, num4, mFont.RIGHT, mFont.tahoma_7_grey);
					num4 += 10;
					break;
				}
				if (char2 != null && char2.cName != null && char2.cName.Length > 0 && !char2.isPet && !char2.isMiniPet && char2.cName.ToLower() != "trọng tài")
				{
					string text = char2.cName + " [" + NinjaUtil.NumberTostring2(char2.cHP).ToString() + "/" + NinjaUtil.NumberTostring2(char2.cHPFull).ToString() + "] - " + CharGender(char2);
					if (char2 == Char.myCharz().charFocus)
					{
						mFont.tahoma_7_yellow.drawStringBorder(g, text, num8, num4, mFont.RIGHT, mFont.tahoma_7_grey);
						widthExactOf2 = mFont.tahoma_7_yellow.getWidthExactOf(text);
					}
					else if (char2.charID < 0 && char2.charID > -1000 && char2.charID != -114)
					{
						mFont.tahoma_7_red.drawStringBorder(g, text, num8, num4, mFont.RIGHT, mFont.tahoma_7_grey);
						widthExactOf2 = mFont.tahoma_7_red.getWidthExactOf(text);
					}
					else if (Char.myCharz().clan != null && char2.clanID == Char.myCharz().clan.ID)
					{
						mFont.tahoma_7_green.drawStringBorder(g, text, num8, num4, mFont.RIGHT, mFont.tahoma_7_grey);
						widthExactOf2 = mFont.tahoma_7_green.getWidthExactOf(text);
					}
					else
					{
						mFont.tahoma_7_white.drawStringBorder(g, text, num8, num4, mFont.RIGHT, mFont.tahoma_7_grey);
						widthExactOf2 = mFont.tahoma_7_white.getWidthExactOf(text);
					}
					if (widthExactOf2 > num7)
					{
						num7 = widthExactOf2;
					}
					g.FillRect(num8 - widthExactOf2 - 3, num4 + 1, widthExactOf2 + 2, 10, 0, 0.2f);
					charsInMap.addElement(char2);
					num4 += 10;
				}
			}
			string text2 = "Số người : " + GameScr.vCharInMap.size();
			widthExactOf2 = mFont.tahoma_7_white.getWidthExactOf(text2);
			g.drawRegion(Mob.imgHP, 0, 18, 9, 6, 4, GameCanvas.w - 12 - widthExactOf2, num6 + 6, mGraphics.VCENTER | mGraphics.HCENTER);
			g.FillRect(GameCanvas.w - 8 - widthExactOf2, num6 + 2, widthExactOf2 + 2, mFont.tahoma_7_white.getHeight() - 2, 0, 0.8f);
			mFont.tahoma_7_white.drawString(g, text2, GameCanvas.w - 5, num6, mFont.RIGHT);
			if (num7 != 0)
			{
				g.setColor(Color.yellow);
				g.DrawLine(GameCanvas.w - 16 - widthExactOf2, num6 + 6, GameCanvas.w - num7 - 11, num6 + 6, 3);
				g.DrawLine(GameCanvas.w - num7 - 10, num6 + 5, GameCanvas.w - num7 - 10, num4 + 6, 3);
				g.DrawLine(GameCanvas.w - num7 - 11, num4 + 5, GameCanvas.w, num4 + 5, 3);
				g.DrawLine(GameCanvas.w, num6 + 6, GameCanvas.w - 5, num6 + 6, 3);
			}
		}

		private void PaintEditButton(mGraphics g)
		{
			int num = 60;
			int h = 24;
			int num2 = 10;
			int num3 = 40;
			int num4 = GameCanvas.w / 2 - num - num2 / 2;
			g.setColor(0, 0.7f);
			g.fillRect(num4, num3, num, h);
			g.setColor(65280);
			g.drawRect(num4, num3, num, h);
			mFont.tahoma_7b_white.drawString(g, "Lưu", num4 + num / 2, num3 + 5, mFont.CENTER);
			int num5 = GameCanvas.w / 2 + num2 / 2;
			g.setColor(0, 0.7f);
			g.fillRect(num5, num3, num, h);
			g.setColor(16711680);
			g.drawRect(num5, num3, num, h);
			mFont.tahoma_7b_white.drawString(g, "Reset", num5 + num / 2, num3 + 5, mFont.CENTER);
		}

		private void PaintListInfo(mGraphics g)
		{
			int num = 70;
			Char charFocus = Char.myCharz().charFocus;
			if (charFocus != null && Char.myCharz().isMeCanAttackOtherPlayer(charFocus))
			{
				int num2 = 150;
				int num3 = 12;
				int num4 = GameCanvas.w / 2 - num2 / 2;
				int num5 = num;
				g.setColor(8421504);
				g.fillRect(num4 - 3, num5 - 3, num2 + 6, num3 + 6, 12);
				g.setColor(2829099);
				g.fillRect(num4 - 1, num5 - 1, num2 + 2, num3 + 2, 10);
				float num6 = (float)charFocus.cHP / (float)charFocus.cHPFull;
				int w = (int)((float)num2 * num6);
				if (num6 > 0.5f)
				{
					g.setColor(65280);
				}
				else if (num6 > 0.25f)
				{
					g.setColor(16776960);
				}
				else
				{
					g.setColor(16711680);
				}
				g.fillRect(num4, num5, w, num3, 8);
				string moneys = NinjaUtil.getMoneys(charFocus.cHP);
				mFont.tahoma_7b_white.drawStringBorder(g, moneys, GameCanvas.w / 2, num5 + num3 / 2 - 5, mFont.CENTER, mFont.tahoma_7_grey);
				num += 17;
				if (charFocus.protectEff)
				{
					mFont.tahoma_7b_red.drawString(g, "Đang khiên năng lượng", GameCanvas.w / 2, num, mFont.CENTER);
					num += 10;
				}
				if (charFocus.isMonkey == 1)
				{
					mFont.tahoma_7b_red.drawString(g, "Đang biến khỉ", GameCanvas.w / 2, num, mFont.CENTER);
					num += 10;
				}
				if (charFocus.sleepEff)
				{
					mFont.tahoma_7b_red.drawString(g, "Bị thôi miên", GameCanvas.w / 2, num, mFont.CENTER);
					num += 10;
				}
				if (charFocus.holdEffID != 0)
				{
					mFont.tahoma_7b_red.drawString(g, "Bị trói", GameCanvas.w / 2, num, mFont.CENTER);
					num += 10;
				}
				if (charFocus.isFreez)
				{
					mFont.tahoma_7b_red.drawString(g, "Bị TDHS: " + charFocus.freezSeconds, GameCanvas.w / 2, num, mFont.CENTER);
					num += 10;
				}
				if (charFocus.blindEff)
				{
					mFont.tahoma_7b_red.drawString(g, "Bị choáng", GameCanvas.w / 2, num, mFont.CENTER);
				}
			}
		}

		private void PaintInfoMe(mGraphics g, int xText, int yText)
		{
			if (mSystem.currentTimeMillis() - lastUpdateInfoMe > 3000)
			{
				Service.gI().petInfo();
				lastUpdateInfoMe = mSystem.currentTimeMillis();
			}
			int num = 10;
			int num2 = 64;
			mFont.tahoma_7b_yellow.drawStringBorder(g, "Sư Phụ :", xText, yText, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "SM: " + NinjaUtil.getMoneys(Char.myCharz().cPower), xText, yText + num, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "TN: " + NinjaUtil.getMoneys(Char.myCharz().cTiemNang), xText, yText + 2 * num, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "SĐ: " + NinjaUtil.getMoneys(Char.myCharz().cDamFull), xText, yText + 3 * num, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "Giáp: " + NinjaUtil.getMoneys(Char.myCharz().cDefull), xText, yText + 4 * num, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7b_yellow.drawStringBorder(g, "Đệ Tử :", xText, yText + num2, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "SM: " + NinjaUtil.getMoneys(Char.myPetz().cPower), xText, yText + num + num2, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "TN: " + NinjaUtil.getMoneys(Char.myPetz().cTiemNang), xText, yText + 2 * num + num2, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "SĐ: " + NinjaUtil.getMoneys(Char.myPetz().cDamFull), xText, yText + 3 * num + num2, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "HP : " + NinjaUtil.getMoneys(Char.myPetz().cHP), xText, yText + 4 * num + num2, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "MP : " + NinjaUtil.getMoneys(Char.myPetz().cMP), xText, yText + 5 * num + num2, mFont.LEFT, mFont.tahoma_7_grey);
			mFont.tahoma_7_white.drawStringBorder(g, "Giáp: " + NinjaUtil.getMoneys(Char.myPetz().cDefull), xText, yText + 6 * num + num2, mFont.LEFT, mFont.tahoma_7_grey);
		}

		public void perform(int idAction, object p)
		{
			if (idAction > 60)
			{
				if (idAction <= 104)
				{
					switch (idAction)
					{
					case 61:
						if (GameCanvas.panel != null)
						{
							GameCanvas.panel.openActivityDashboard();
						}
						break;
					case 76:
						PickMob.vuotDiaHinh = !PickMob.vuotDiaHinh;
						GameScr.info1.addInfo("Vượt địa hình " + (PickMob.vuotDiaHinh ? "[Bật]" : "[Tắt]"), 0);
						break;
					case 80:
						PickMob.telePem = !PickMob.telePem;
						GameScr.info1.addInfo("Dịch chuyển đến quái\n" + (PickMob.telePem ? "[Bật]" : "[Tắt]"), 0);
						break;
					case 89:
					{
						MyVector myVector = new MyVector();
						myVector.addElement(new Command("Tự động nhặt " + (PickMob.IsAutoPickItems ? "[Bật]" : "[Tắt]"), 90));
						myVector.addElement(new Command("Nhặt tất cả " + (PickMob.IsPickItemsAll ? "[Bật]" : "[Tắt]"), 91));
						myVector.addElement(new Command("Nhặt xa\n" + (PickMob.IsPickItemsDis ? "[Bật]" : "[Tắt]"), 92));
						myVector.addElement(new Command("Xem DS lọc đồ", 93));
						myVector.addElement(new Command("Tự động lọc đồ", 94));
						GameCanvas.menu.startAt(myVector, 4);
						break;
					}
					case 90:
						PickMob.IsAutoPickItems = !PickMob.IsAutoPickItems;
						GameScr.info1.addInfo("Tự động nhặt " + (PickMob.IsAutoPickItems ? "[Bật]" : "[Tắt]"), 0);
						break;
					case 91:
						PickMob.IsPickItemsAll = !PickMob.IsPickItemsAll;
						GameScr.info1.addInfo("Nhặt tất cả " + (PickMob.IsPickItemsAll ? "[Bật]" : "[Tắt]"), 0);
						break;
					case 92:
						PickMob.IsPickItemsDis = !PickMob.IsPickItemsDis;
						GameScr.info1.addInfo("Nhặt xa " + (PickMob.IsPickItemsDis ? "[Bật]" : "[Tắt]"), 0);
						break;
					case 93:
						isShowFilterList = !isShowFilterList;
						GameScr.info1.addInfo("Đã mở danh sách lọc đồ", 0);
						break;
					case 94:
						isAutoFilterItem = !isAutoFilterItem;
						GameScr.info1.addInfo("Tự động lọc đồ " + (isAutoFilterItem ? "[Bật]" : "[Tắt]"), 0);
						break;
					case 100:
					{
						string obj = (string)p;
						int.TryParse(obj.Split("-")[0], out indexAutoPoint);
						bool.TryParse(obj.Split("-")[1], out autoPointForPet);
						GameCanvas.panel.hideNow();
						MyChatTextField(ChatTextField.gI(), "Tăng đến mức", "VD: 220000");
						break;
					}
					case 101:
						isOpenAccMAnager = true;
						break;
					case 102:
					{
						Account account = (Account)p;
						Rms.saveRMSString("acc", account.getUsername());
						Rms.saveRMSString("pass", account.getPassword());
						if (GameCanvas.loginScr != null && GameCanvas.currentScreen == GameCanvas.loginScr)
						{
							GameCanvas.loginScr.setUserPass();
						}
						isOpenAccMAnager = false;
						break;
					}
					case 103:
					{
						int index = accounts.IndexOf((Account)p);
						accounts.RemoveAt(index);
						cmdsChooseAcc.RemoveAt(index);
						cmdsDelAcc.RemoveAt(index);
						SaveAcc();
						break;
					}
					case 104:
						isOpenAccMAnager = false;
						break;
					}
				}
				else if (idAction - 500 <= 1)
				{
					AddOrRemoveAutoItem((Item)p, idAction == 500);
				}
				else if (idAction - 502 > 1)
				{
					if (idAction == 883)
					{
						XmapController.ShowXmapMenu();
					}
				}
				else
				{
					AddOrRemoveFilterItem((Item)p, idAction == 502);
				}
				return;
			}
			switch (idAction)
			{
			case 59:
				CostumeCollectionScr.gI().requestOpen();
				break;
			case 1:
			{
				string s;
				if (int.TryParse((string)p, out var result))
				{
					XmapController.StartRunToMapId(result);
					s = "Di chuyển đến boss ở MAP " + result;
				}
				else
				{
					s = "Địa điểm không hợp lệ!";
				}
				GameScr.info1.addInfo(s, 0);
				break;
			}
			case 2:
				GameScr.info1.addInfo("Đã huỷ di chuyển đến Boss", 0);
				break;
			case 16:
			{
				MyVector myVector2 = new MyVector();
				myVector2.addElement(new Command(isPeanPet ? "Buff đậu cho đệ [Bật]" : "Buff đậu cho đệ [Tắt]", 17));
				GameCanvas.menu.startAt(myVector2, 4);
				break;
			}
			case 32:
			{
				MyVector myVector3 = new MyVector();
				myVector3.addElement(new Command(notifBoss ? "Thông báo BOSS [Bật]" : "Thông báo BOSS [Tắt]", 46));
				myVector3.addElement(new Command(notifKillBoss ? "Thông báo tiêu diệt BOSS [Bật]" : "Thông báo tiêu diệt BOSS [Tắt]", 55));
				myVector3.addElement(new Command(lineToBoss ? "Kẻ đường tới BOSS [Bật]" : "Đường kẻ tới BOSS [Tắt]", 47));
				myVector3.addElement(new Command(focusBoss ? "Focus BOSS [Bật]" : "Focus BOSS [Tắt]", 52));
				GameCanvas.menu.startAt(myVector3, 4);
				break;
			}
			case 38:
				PickMob.mapGoback = TileMap.mapID;
				PickMob.zoneGoback = TileMap.zoneID;
				PickMob.xGoback = Char.myCharz().cx;
				PickMob.yGoback = Char.myCharz().cy;
				PickMob.isGoBack = !PickMob.isGoBack;
				if (PickMob.isGoBack)
				{
					GameScr.info1.addInfo("Map Goback: " + TileMap.mapName + " | Khu: " + TileMap.zoneID, 0);
					GameScr.info1.addInfo("Tọa độ X: " + PickMob.xGoback + " | Y: " + PickMob.yGoback, 0);
					if (Char.myCharz().cHP <= 0 || Char.myCharz().statusMe == 14)
					{
						Service.gI().returnTownFromDead();
						new Thread(PickMob.GoBack).Start();
					}
				}
				GameScr.info1.addInfo("Goback tọa độ " + (PickMob.isGoBack ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 42:
				autoAttack = !autoAttack;
				GameScr.info1.addInfo("Tự đánh " + (autoAttack ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 43:
				PickMob.neSieuQuai = !PickMob.neSieuQuai;
				GameScr.info1.addInfo("Né siêu quái " + (PickMob.neSieuQuai ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 44:
				PickMob.tsPlayer = false;
				PickMob.tanSat = ((p != null) ? ((bool)p) : (!PickMob.tanSat));
				GameScr.info1.addInfo("Tàn sát " + (PickMob.tanSat ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 45:
			{
				MyVector myVector4 = new MyVector();
				MyVector myVector5 = new MyVector();
				for (int i = 0; i < GameScr.vMob.size(); i++)
				{
					Mob mob = (Mob)GameScr.vMob.elementAt(i);
					if (GameScr.gI().isMeCanAttackMob(mob) && !myVector5.contains(mob.templateId) && !PickMob.TypeMobsTanSat.Contains(mob.templateId))
					{
						myVector5.addElement(mob.templateId);
						myVector4.addElement(new Command("Tàn sát " + mob.getTemplate().name, 49, mob));
					}
				}
				myVector4.addElement(new Command(PickMob.tanSat ? "Tàn sát [Bật]" : "Tàn sát [Tắt]", 44));
				myVector4.addElement(new Command(PickMob.tsPlayer ? "Tàn sát\nngười [Bật]" : "Tàn sát\nngười [Tắt]", 48));
				myVector4.addElement(new Command(autoAttack ? "Tự đánh [Bật]" : "Tự đánh [Tắt]", 42));
				myVector4.addElement(new Command(PickMob.neSieuQuai ? "Né siêu quái [Bật]" : "Né siêu quái [Tắt]", 43));
				myVector4.addElement(new Command(PickMob.vuotDiaHinh ? "Vượt địa hình [Bật]" : "Vượt địa hình [Tắt]", 76));
				myVector4.addElement(new Command(PickMob.telePem ? "Dịch chuyển\n[Bật]" : "Dịch chuyển\n[Tắt]", 80));
				myVector4.addElement(new Command(PickMob.isGoBack ? "Goback Tọa Độ [Bật]" : "Goback Tọa Độ [Tắt]", 38));
				myVector4.addElement(new Command("Xoá danh sách tàn sát", 51));
				GameCanvas.menu.startAt(myVector4, 4);
				break;
			}
			case 46:
				notifBoss = !notifBoss;
				GameScr.info1.addInfo("Thông báo BOSS " + (notifBoss ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 47:
				lineToBoss = !lineToBoss;
				GameScr.info1.addInfo("Kẻ đường tới BOSS " + (lineToBoss ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 48:
				PickMob.tanSat = false;
				PickMob.tsPlayer = ((p != null) ? ((bool)p) : (!PickMob.tsPlayer));
				GameScr.info1.addInfo("Tàn sát người " + (PickMob.tsPlayer ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 49:
			{
				Mob mob2 = (Mob)p;
				if (!PickMob.TypeMobsTanSat.Contains(mob2.templateId))
				{
					PickMob.TypeMobsTanSat.Add(mob2.templateId);
				}
				GameScr.info1.addInfo("Tàn sát " + mob2.getTemplate().name, 0);
				perform(44, true);
				break;
			}
			case 51:
				PickMob.TypeMobsTanSat.Clear();
				GameScr.info1.addInfo("Đã xoá danh sách quái tàn sát!", 0);
				break;
			case 52:
				focusBoss = !focusBoss;
				GameScr.info1.addInfo("Focus BOSS " + (focusBoss ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 53:
			{
				MyVector myVector6 = new MyVector();
				myVector6.addElement(new Command("FPS\nGame\n[" + Application.targetFrameRate + "]", 60));
				myVector6.addElement(new Command("Tốc độ\nGame\n[" + Time.timeScale + "]", 54));
				myVector6.addElement(new Command("Giảm\nĐồ Hoạ", 58));
				myVector6.addElement(new Command("Load ô\nskill", 57));
				GameCanvas.menu.startAt(myVector6, 4);
				break;
			}
			case 54:
				MyChatTextField(ChatTextField.gI(), "Nhập tốc độ game", "1 đến 100");
				break;
			case 57:
				LoadSkillToScreen();
				GameScr.info1.addInfo("Đã load ô skill", 0);
				break;
			case 58:
				changeStatusEffectInven();
				GiamDungLuong = !GiamDungLuong;
				GameScr.info1.addInfo("Giảm Dung Lượng " + (GiamDungLuong ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 55:
				notifKillBoss = !notifKillBoss;
				GameScr.info1.addInfo("Thông báo tiêu diệt BOSS " + (notifKillBoss ? "[Bật]" : "[Tắt]"), 0);
				break;
			case 60:
				MyChatTextField(ChatTextField.gI(), "Nhập FPS", "FPS");
				break;
			case 17:
				isPeanPet = !isPeanPet;
				GameScr.info1.addInfo("Buff đậu cho đệ " + (isPeanPet ? "[Bật]" : "[Tắt]"), 0);
				break;
			}
		}

		public void AutoBuyItem(int num, Item itemBuy)
		{
			new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < num; i++)
				{
					Service.gI().buyItem(3, itemBuy.template.id, 0);
					Thread.Sleep(200);
				}
				GameScr.info1.addInfo("Đã mua xong " + num + " " + itemBuy.template.name, 0);
			}).Start();
		}

		private void AddOrRemoveAutoItem(Item item, bool isAdd)
		{
			if (isAdd)
			{
				listItemAuto.Add(new ItemAuto(item.template.iconID, item.template.id));
				GameScr.info1.addInfo("Đã thêm " + item.template.name + " vào Auto Item", 0);
				return;
			}
			foreach (ItemAuto item2 in listItemAuto)
			{
				if (item2.iconID == item.template.iconID && item2.id == item.template.id)
				{
					listItemAuto.Remove(item2);
					GameScr.info1.addInfo("Đã xóa " + item.template.name + " khỏi Auto Item", 0);
					break;
				}
			}
		}

		public void DoDoubleClickToObj(IMapObject obj)
		{
			if ((obj.Equals(Char.myCharz().npcFocus) || GameScr.gI().mobCapcha == null) && !GameScr.gI().checkClickToBotton(obj))
			{
				GameScr.gI().checkEffToObj(obj, isnew: false);
				Char.myCharz().cancelAttack();
				Char.myCharz().currentMovePoint = null;
				Char.myCharz().cvx = (Char.myCharz().cvy = 0);
				obj.stopMoving();
				GameScr.gI().auto = 10;
				GameScr.gI().doFire(isFireByShortCut: false, skipWaypoint: true);
				GameScr.gI().clickToX = obj.getX();
				GameScr.gI().clickToY = obj.getY();
				GameScr.gI().clickOnTileTop = false;
				GameScr.gI().clickMoving = true;
				GameScr.gI().clickMovingRed = true;
				GameScr.gI().clickMovingTimeOut = 20;
				GameScr.gI().clickMovingP1 = 30;
			}
		}

		public void MyChatTextField(ChatTextField chatTField, string strChat, string strName)
		{
			chatTField.strChat = strChat;
			chatTField.tfChat.name = strName;
			chatTField.to = string.Empty;
			chatTField.isShow = true;
			chatTField.tfChat.isFocus = true;
			chatTField.tfChat.setIputType(TField.INPUT_TYPE_NUMERIC);
			chatTField.tfChat.setMaxTextLenght(10);
			if (!Main.isPC)
			{
				chatTField.startChat(GameCanvas.panel, string.Empty);
			}
			else if (GameCanvas.isTouch)
			{
				chatTField.tfChat.doChangeToTextBox();
			}
		}

		public void ChangeGameSpeed(string strSpeed)
		{
			if (int.TryParse(strSpeed, out var result) && result > 0 && result <= 100)
			{
				Time.timeScale = result;
				GameScr.info1.addInfo("Tốc độ game: " + result, 0);
			}
			else
			{
				GameScr.info1.addInfo("Chỉ nhập số từ 1 đến 100", 0);
			}
		}

		public void Changeisfps(string strSpeed)
		{
			if (int.TryParse(strSpeed, out var result) && result >= 60 && result <= 1000)
			{
				isfpscao = result;
				Application.targetFrameRate = result;
				GameScr.info1.addInfo("FPS: " + result, 0);
			}
			else
			{
				GameScr.info1.addInfo("Chỉ nhập số từ 60 đến 1000", 0);
			}
		}

		public void TeleportToPlayer(int charID)
		{
			Service.gI().gotoPlayer(charID);
		}

		public void AddNotifTichXanh(string notif)
		{
			listNotifTichXanh.addElement(notif);
			if (!startChat)
			{
				int num = GameCanvas.w / 2;
				startChat = true;
				xNotif = num + num / 2;
				lastUpdateNotif = mSystem.currentTimeMillis();
			}
		}

		private void PaintPlayerTichXanh(mGraphics g)
		{
			if (listNotifTichXanh.size() != 0)
			{
				string st = (string)listNotifTichXanh.elementAt(0);
				int num = GameCanvas.w / 2;
				g.setClip(num - num / 3, 50, num / 3 * 2, 12);
				g.fillRect(num - num / 3, 50, num / 3 * 2, 12, 0, 60);
				mFont.tahoma_7_yellow.drawStringBorder(g, st, xNotif, 50, 0, mFont.tahoma_7_grey);
				PaintTicks(g, xNotif - 12, 51);
			}
		}

		private void UpdateNotifTichXanh()
		{
			if (!startChat || mSystem.currentTimeMillis() - lastUpdateNotif < 10)
			{
				return;
			}
			xNotif--;
			string s = (string)listNotifTichXanh.elementAt(0);
			lastUpdateNotif = mSystem.currentTimeMillis();
			if (xNotif < GameCanvas.w / 2 - 100 - mFont.tahoma_7_yellow.getWidth(s))
			{
				xNotif = GameCanvas.w / 2 + 100;
				listNotifTichXanh.removeElementAt(0);
				if (listNotifTichXanh.size() == 0)
				{
					startChat = false;
				}
			}
		}

		public void SetIncreasePoint(string strPoint)
		{
			if (int.TryParse(strPoint, out var result) && indexAutoPoint != -1 && result > 0)
			{
				pointIncrease = result;
				new Thread(DoAutoIncreasePoint).Start();
				GameScr.info1.addInfo("Tự động tăng " + strPointTypes[indexAutoPoint] + " đến " + result, 0);
			}
			else
			{
				GameScr.info1.addInfo("Có lỗi xảy ra (100)", 0);
			}
		}

		private void DoAutoIncreasePoint()
		{
			while (indexAutoPoint != -1 && pointIncrease > 0)
			{
				Char @char = (autoPointForPet ? Char.myPetz() : Char.myCharz());
				if (indexAutoPoint switch
				{
					0 => @char.cHPGoc, 
					1 => @char.cMPGoc, 
					2 => @char.cDamGoc, 
					3 => @char.cDefGoc, 
					4 => @char.cCriticalGoc, 
					_ => 0L, 
				} >= pointIncrease)
				{
					indexAutoPoint = -1;
					pointIncrease = 0;
					GameScr.info1.addInfo("Đã đạt chỉ số yêu cầu", 0);
					break;
				}
				Service.gI().upPotential(autoPointForPet, indexAutoPoint, 100);
				Thread.Sleep(500);
			}
		}

		public void LoadAcc()
		{
			string text = Rms.loadRMSString("accManager");
			if (text != null && !(text.Trim('|') == string.Empty))
			{
				accounts.Clear();
				cmdsChooseAcc.Clear();
				cmdsDelAcc.Clear();
				string[] array = text.Trim('|').Split('|');
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split('$');
					Account account = new Account(array2[0], array2[1]);
					accounts.Add(account);
					Command command = new Command(account.getUsername(), this, 102, account);
					command.setType();
					cmdsChooseAcc.Add(command);
					Command command2 = new Command("Xoá", this, 103, account);
					command2.setTypeDelete();
					cmdsDelAcc.Add(command2);
				}
			}
		}

		public void AddAccount(string user, string pass)
		{
			Account item = new Account(user, pass);
			int num = accounts.IndexOf(item);
			if (num != -1)
			{
				accounts.RemoveAt(num);
			}
			accounts.Insert(0, item);
			for (int i = 5; i < accounts.Count; i++)
			{
				accounts.RemoveAt(i);
			}
			SaveAcc();
		}

		private void SaveAcc()
		{
			string text = "";
			foreach (Account account in accounts)
			{
				text += string.Join('$', account.getUsername(), account.getPassword());
				text += "|";
			}
			Rms.saveRMSString("accManager", text.Trim('|'));
		}

		private void AutoChat()
		{
			if (string.IsNullOrEmpty(textAutoChat))
			{
				GameScr.info1.addInfo("Chưa cài nội dung tự động chat", 0);
			}
			else
			{
				Service.gI().chat(textAutoChat);
			}
		}

		private void AutoChatTG()
		{
			if (string.IsNullOrEmpty(textAutoChatTG))
			{
				GameScr.info1.addInfo("Chưa cài nội dung tự động chat thế giới", 0);
			}
			else
			{
				Service.gI().chatGlobal(textAutoChatTG);
			}
		}

		private static byte[] EncodeToBytes(string inputString, string key)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(inputString);
			byte[] bytes2 = Encoding.UTF8.GetBytes(key);
			byte[] array = new byte[bytes.Length];
			for (int i = 0; i < bytes.Length; i++)
			{
				array[i] = (byte)(bytes[i] ^ bytes2[i % bytes2.Length]);
			}
			return array;
		}

		private static string[] SplitByLength(string str, int length)
		{
			int length2 = str.Length;
			int num = (length2 + length - 1) / length;
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = i * length;
				int length3 = Math.min(length, length2 - num2);
				array[i] = str.Substring(num2, length3);
			}
			return array;
		}

		public static string DecodeByteArrayString(string byteArrayString, string key)
		{
			try
			{
				string[] value = byteArrayString.Split('-');
				string text = string.Join("", value);
				byte[] array = new byte[text.Length / 2];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
				}
				return DecodeToString(array, key);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		private static string DecodeToString(byte[] encodedBytes, string key)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(key);
			byte[] array = new byte[encodedBytes.Length];
			for (int i = 0; i < encodedBytes.Length; i++)
			{
				array[i] = (byte)(encodedBytes[i] ^ bytes[i % bytes.Length]);
			}
			return Encoding.UTF8.GetString(array);
		}

		public static void Log(string text)
		{
			if (isDebugEnable)
			{
				Debug.Log(text);
			}
		}

		public static void WriteLog(string message)
		{
			if (!isDebugEnable)
			{
				return;
			}
			try
			{
				StreamWriter streamWriter = new StreamWriter(new FileStream("log_" + DateTime.Today.ToString("yyyyMMdd") + ".txt", FileMode.OpenOrCreate));
				streamWriter.WriteLine(DateTime.Today.ToString("HH:mm:ss") + ": " + message);
				streamWriter.Flush();
				streamWriter.Close();
			}
			catch (Exception ex)
			{
				Log(ex.Message);
			}
		}

		private void LoadSkillToScreen()
		{
			for (int i = 0; i < Char.myCharz().vSkill.size(); i++)
			{
				Skill skill = (Skill)Char.myCharz().vSkill.elementAt(i);
				if (GameCanvas.isTouch && !Main.isPC)
				{
					for (int j = 0; j < GameScr.onScreenSkill.Length; j++)
					{
						if (GameScr.onScreenSkill[j] == skill)
						{
							GameScr.onScreenSkill[j] = null;
						}
					}
					GameScr.onScreenSkill[i] = skill;
					GameScr.gI().saveonScreenSkillToRMS();
					continue;
				}
				for (int k = 0; k < GameScr.keySkill.Length; k++)
				{
					if (GameScr.keySkill[k] == skill)
					{
						GameScr.keySkill[k] = null;
					}
				}
				GameScr.keySkill[i] = skill;
				GameScr.gI().saveKeySkillToRMS();
			}
		}

		public static void DoChatGlobal()
		{
			GameCanvas.endDlg();
			if (Char.myCharz().checkLuong() < 5)
			{
				GameCanvas.startOKDlg(mResources.not_enough_luong_world_channel);
				return;
			}
			if (GameCanvas.panel.chatTField == null)
			{
				GameCanvas.panel.chatTField = new ChatTextField();
				GameCanvas.panel.chatTField.tfChat.y = GameCanvas.h - 35 - ChatTextField.gI().tfChat.height;
				GameCanvas.panel.chatTField.initChatTextField();
				GameCanvas.panel.chatTField.parentScreen = GameCanvas.panel;
			}
			GameCanvas.panel.chatTField.strChat = mResources.world_channel_5_luong;
			GameCanvas.panel.chatTField.tfChat.name = mResources.CHAT;
			GameCanvas.panel.chatTField.to = string.Empty;
			GameCanvas.panel.chatTField.isShow = true;
			GameCanvas.panel.chatTField.tfChat.isFocus = true;
			GameCanvas.panel.chatTField.tfChat.setIputType(TField.INPUT_TYPE_ANY);
			if (Main.isWindowsPhone)
			{
				GameCanvas.panel.chatTField.tfChat.strInfo = GameCanvas.panel.chatTField.strChat;
			}
			if (!Main.isPC)
			{
				GameCanvas.panel.chatTField.startChat(GameCanvas.panel, string.Empty);
			}
			else if (GameCanvas.isTouch)
			{
				GameCanvas.panel.chatTField.tfChat.doChangeToTextBox();
			}
		}

		public void GoToBoss(int mapId)
		{
			MyVector myVector = new MyVector();
			myVector.addElement(new Command("Đi tới\nMAP " + mapId, this, 1, mapId.ToString()));
			myVector.addElement(new Command("Huỷ", this, 2, null));
			GameCanvas.menu.startAt(myVector, 4);
		}

		public void ChangeFPSTarget()
		{
			Rms.saveRMSInt("isHighFps", isHighFps ? 1 : 0);
			if (isHighFps)
			{
				Application.targetFrameRate = isfpscao;
			}
			else
			{
				Application.targetFrameRate = 30;
			}
		}

		public static void changeStatusEffectInven()
		{
			if (isEffectInven)
			{
				isEffectInven = false;
				Rms.saveRMSInt("effectinven", isEffectInven ? 1 : 0);
			}
			else
			{
				isEffectInven = true;
				Rms.saveRMSInt("effectinven", isEffectInven ? 1 : 0);
			}
		}

		public static void changeStatusHideBadges()
		{
			isHideBadges = !isHideBadges;
			Rms.saveRMSInt("hideBadges", isHideBadges ? 1 : 0);
			Service.gI().setBadgeVisibility(isHideBadges);
		}

		public static void setHideBadgesFromServer(bool hidden)
		{
			isHideBadges = hidden;
			Rms.saveRMSInt("hideBadges", hidden ? 1 : 0);
		}

		public static void chanegStatusInventory()
		{
			if (isInventory)
			{
				isInventory = false;
				Rms.saveRMSInt("inventory", isInventory ? 1 : 0);
			}
			else
			{
				isInventory = true;
				Rms.saveRMSInt("inventory", isInventory ? 1 : 0);
			}
		}

		public static void changeStatusLogo()
		{
			if (isLogo)
			{
				isLogo = false;
				imgLogoBig = null;
				logo = GameCanvas.loadImage("/mainimage/logo1.png");
				Rms.saveRMSInt("logo", 0);
				if (isLogoGif)
				{
					isLogoGif = false;
					Rms.saveRMSInt("logogif", 0);
				}
			}
			else
			{
				Rms.saveRMSInt("logo", 1);
				isLogo = true;
				if (isLogoGif)
				{
					Rms.saveRMSInt("logogif", 1);
				}
			}
		}

		public static void changeStatusBackground()
		{
			if (GiamDungLuong)
			{
				GiamDungLuong = false;
				Rms.saveRMSInt("background", GiamDungLuong ? 1 : 0);
			}
			else
			{
				GiamDungLuong = true;
				Rms.saveRMSInt("background", GiamDungLuong ? 1 : 0);
			}
		}

		public static void changeStatusAnPlayer()
		{
			if (AnPlayer)
			{
				AnPlayer = false;
				Rms.saveRMSInt("anplayer", AnPlayer ? 1 : 0);
			}
			else
			{
				AnPlayer = true;
				Rms.saveRMSInt("anplayer", AnPlayer ? 1 : 0);
			}
		}

		public static void changeStatusShowID()
		{
			if (isShowID)
			{
				isShowID = false;
				Rms.saveRMSInt("showid", isShowID ? 1 : 0);
			}
			else
			{
				isShowID = true;
				Rms.saveRMSInt("showid", isShowID ? 1 : 0);
			}
		}

		public static void changeStatusLogoGif()
		{
			if (isLogoGif)
			{
				isLogoGif = false;
				Rms.saveRMSInt("logogif", 0);
			}
			else
			{
				isLogoGif = true;
				Rms.saveRMSInt("logogif", 1);
			}
		}

		public static Npc GetNpcByTempId(int tempId)
		{
			for (int i = 0; i < GameScr.vNpc.size(); i++)
			{
				Npc npc = (Npc)GameScr.vNpc.elementAt(i);
				if (npc.template.npcTemplateId == tempId)
				{
					return npc;
				}
			}
			return null;
		}

		public static void LoadLogoImages()
		{
			imgBg = GameCanvas.LoadImageFromRoot("/bg/bg.png");
			if (isLogo)
			{
				imgLogoBig = GameCanvas.loadImage("/mainimage/logo1.png");
				imgLogoBig = GameCanvas.loadImage("/mainimage/logo1.png");
			}
			if (imgLogoBig == null)
			{
				isLogo = true;
				Rms.saveRMSInt("logo", 0);
			}
            LoadLogoGif();
        }

		public static void LoadLogoGif()
		{
			for (int i = 0; i < FrameGif; i++)
			{
				logos[i] = GameCanvas.loadImage("/logogif/logo-" + i + ".png");
			}
		}

		public static void PaintLogoGif(mGraphics g, int x, int y, int anchor)
		{
            g.drawImage(imgLogoBig, x, y + 10, anchor);			
		}

		public static void LoadLogoGifMenu()
		{
            for (int i = 0; i < maxGif; i++)
            {
                gif[i] = GameCanvas.loadImage("/logogif/logo-" + i + ".png");
            }
        }
        private static Image[] gif = new Image[55];
        private static int maxGif = 54;
        private static int SoAnh;

        public static void PaintLogoGifMenu(mGraphics g, int x, int y, int anchor)
		{
            if (GameCanvas.gameTick % 1 == 0)
            {
                SoAnh++;
            }
            if (SoAnh > maxGif)
            {
                SoAnh = 0;
            }
            g.drawImage(gif[SoAnh], x, y, anchor);
        }

		public static void LoadTickImages()
		{
			for (int i = 0; i < 20; i++)
			{
				ticks[i] = GameCanvas.loadImage("/tick/tick_" + i);
			}
		}

		public static void PaintTicks(mGraphics g, int x, int y)
		{
			int num = GameCanvas.gameTick / 4 % 20;
			if (ticks[num] != null)
			{
				g.drawImage(ticks[num], x, y);
			}
		}

		private static IEnumerator LoadFile(string fullPath)
		{
			string uri = "file://" + fullPath;
			using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS);
			www.certificateHandler = new BypassCertificateHandler();
			yield return www.SendWebRequest();
			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(www.error);
				yield break;
			}
			AudioClip content = DownloadHandlerAudioClip.GetContent(www);
			musics.Add(content);
		}

		public static void InitMusic()
		{
		}

		public static string Decrypt(string encryptedText, int keys)
		{
			Debug.Log("Chuỗi nhận được để giải mã: " + encryptedText);
			if (string.IsNullOrEmpty(encryptedText))
			{
				return string.Empty;
			}
			int i;
			for (i = 0; (encryptedText.Length + i) % 5 != 0; i++)
			{
			}
			if (i > 0)
			{
				encryptedText = encryptedText.PadRight(encryptedText.Length + i, 'u');
			}
			List<byte> list = new List<byte>();
			for (int j = 0; j < encryptedText.Length; j += 5)
			{
				ulong num = 0uL;
				for (int k = 0; k < 5; k++)
				{
					int num2 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~".IndexOf(encryptedText[j + k]);
					if (num2 == -1)
					{
						Debug.LogError($"Ký tự không hợp lệ trong chuỗi mã hóa: {encryptedText[j + k]} tại vị trí {j + k}");
						throw new Exception($"Ký tự không hợp lệ trong chuỗi mã hóa: {encryptedText[j + k]}");
					}
					num = num * 85 + (ulong)num2;
				}
				list.Add((byte)(num >> 24));
				list.Add((byte)(num >> 16));
				list.Add((byte)(num >> 8));
				list.Add((byte)num);
			}
			if (i > 0)
			{
				list.RemoveRange(list.Count - i, i);
			}
			byte[] array = list.ToArray();
			byte[] array2 = new byte[16];
			byte[] array3 = new byte[16];
			byte[] array4 = new byte[array.Length - 32];
			Buffer.BlockCopy(array, 0, array2, 0, 16);
			Buffer.BlockCopy(array, 16, array3, 0, 16);
			Buffer.BlockCopy(array, 32, array4, 0, array4.Length);
			string s = keys.ToString();
			byte[] array5 = Encoding.UTF8.GetBytes(s).Concat(array2).ToArray();
			byte[] array6 = new byte[32];
			for (int l = 0; l < 32; l++)
			{
				array6[l] = array5[l % array5.Length];
			}
			using Aes aes = Aes.Create();
			aes.Key = array6;
			aes.IV = array3;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			using ICryptoTransform transform = aes.CreateDecryptor();
			using MemoryStream stream = new MemoryStream(array4);
			using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
			using StreamReader streamReader = new StreamReader(stream2);
			return streamReader.ReadToEnd();
		}

		public static bool AutoLogin()
		{
			if (autoLogin == null || autoLogin.waitToNextLogin)
			{
				return false;
			}
			if (!Util.CanDoWithTime(autoLogin.lastTimeWait, 500L))
			{
				return false;
			}
			if (ServerListScreen.ipSelect < 0 || ServerListScreen.ipSelect >= ServerListScreen.address.Length || string.IsNullOrEmpty(ServerListScreen.address[ServerListScreen.ipSelect]) || ServerListScreen.testConnect != 2)
			{
				ServerListScreen.LoadIP();
				if (GameCanvas.serverScreen == null)
				{
					GameCanvas.serverScreen = new ServerListScreen();
				}
				GameCanvas.serverScreen.switchToMe();
				autoLogin.lastTimeWait = mSystem.currentTimeMillis();
				return false;
			}
			if (GameCanvas.currentScreen != GameCanvas.loginScr)
			{
				if (GameCanvas.loginScr == null)
				{
					GameCanvas.loginScr = new LoginScr();
				}
				GameCanvas.loginScr.switchToMe();
				autoLogin.lastTimeWait = mSystem.currentTimeMillis();
				return false;
			}
			if (!autoLogin.hasSetUserPass)
			{
				Account accWithUsername = autoLogin.GetAccWithUsername(accounts);
				if (accWithUsername.getUsername().Length > 0)
				{
					Rms.saveRMSString("acc", accWithUsername.getUsername());
					Rms.saveRMSString("pass", accWithUsername.getPassword());
					GameCanvas.loginScr.setUserPass();
					autoLogin.hasSetUserPass = true;
				}
				autoLogin.lastTimeWait = mSystem.currentTimeMillis();
			}
			GameCanvas.loginScr.doLogin();
			autoLogin.waitToNextLogin = true;
			return true;
		}

		private void SaveButtonPositions()
		{
			string text = "";
			foreach (KeyValuePair<string, Point> buttonPosition in buttonPositions)
			{
				text += $"{buttonPosition.Key},{buttonPosition.Value.x},{buttonPosition.Value.y};";
			}
			Rms.saveRMSString("buttonPositions", text);
		}

		private void LoadButtonPositions()
		{
			string text = Rms.loadRMSString("buttonPositions");
			if (!string.IsNullOrEmpty(text))
			{
				buttonPositions.Clear();
				string[] array = text.Split(';');
				foreach (string text2 in array)
				{
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					string[] array2 = text2.Split(',');
					if (array2.Length == 3)
					{
						string key = array2[0];
						if (int.TryParse(array2[1], out var result) && int.TryParse(array2[2], out var result2))
						{
							buttonPositions[key] = new Point(result, result2);
						}
					}
				}
			}
			else
			{
				InitButtonPositions();
			}
		}

		public static void changeStatusEditButton()
		{
			if (isEditButton)
			{
				isEditButton = false;
				Rms.saveRMSInt("editbutton", 0);
				GameScr.info1.addInfo("Đã tắt chế độ chỉnh sửa nút", 0);
			}
			else
			{
				isEditButton = true;
				GameCanvas.panel.isShow = false;
				Rms.saveRMSInt("editbutton", 1);
				GameScr.info1.addInfo("Đã bật chế độ chỉnh sửa nút", 0);
			}
		}

		private void AddOrRemoveFilterItem(Item item, bool isAdd)
		{
			if (isAdd)
			{
				listFilterItems.Add(new ItemAutoFilter(item.template.iconID, item.template.id, item.template.name));
				GameScr.info1.addInfo("Đã thêm " + item.template.name + " vào DS lọc đồ", 0);
				return;
			}
			foreach (ItemAutoFilter listFilterItem in listFilterItems)
			{
				if (listFilterItem.iconID == item.template.iconID && listFilterItem.id == item.template.id && listFilterItem.name == item.template.name)
				{
					listFilterItems.Remove(listFilterItem);
					GameScr.info1.addInfo("Đã xóa " + item.template.name + " khỏi DS lọc đồ", 0);
					break;
				}
			}
		}

		private void ShowFilterList(mGraphics g)
		{
			int num = 25;
			int num2 = 10;
			int h = 30;
			int num3 = 40;
			int num4 = panelX + panelW - num3;
			int num5 = panelY + panelH - num3;
			if (GameCanvas.isPointerDown && GameCanvas.isPointerHoldIn(panelX, panelY, panelW, h))
			{
				GameCanvas.isPointerJustDown = false;
				if (!isDragging)
				{
					isDragging = true;
					lastMouseX = GameCanvas.px;
					lastMouseY = GameCanvas.py;
					lastPanelX = panelX;
					lastPanelY = panelY;
				}
				else
				{
					float num6 = 1f;
					int num7 = GameCanvas.px - lastMouseX;
					int num8 = GameCanvas.py - lastMouseY;
					int num9 = lastPanelX + num7;
					int num10 = lastPanelY + num8;
					panelX = (int)((float)panelX + (float)(num9 - panelX) * num6);
					panelY = (int)((float)panelY + (float)(num10 - panelY) * num6);
					lastMouseX = GameCanvas.px;
					lastMouseY = GameCanvas.py;
					lastPanelX = panelX;
					lastPanelY = panelY;
				}
				panelX = System.Math.Max(0, System.Math.Min(GameCanvas.w - panelW, panelX));
				panelY = System.Math.Max(0, System.Math.Min(GameCanvas.h - panelH, panelY));
			}
			else
			{
				isDragging = false;
			}
			if (GameCanvas.isPointerDown && !isDragging)
			{
				bool num11 = GameCanvas.px >= panelX + panelW - num3 && GameCanvas.px <= panelX + panelW;
				bool flag = GameCanvas.py >= panelY + panelH - num3 && GameCanvas.py <= panelY + panelH;
				if ((num11 && GameCanvas.py >= panelY + panelH - num3) || (flag && GameCanvas.px >= panelX + panelW - num3))
				{
					GameCanvas.isPointerJustDown = false;
					if (!isResizing)
					{
						isResizing = true;
						lastMouseX = GameCanvas.px;
						lastMouseY = GameCanvas.py;
						lastPanelW = panelW;
						lastPanelH = panelH;
					}
					else
					{
						float num12 = 1f;
						int num13 = GameCanvas.px - lastMouseX;
						int num14 = GameCanvas.py - lastMouseY;
						int num15 = lastPanelW + num13;
						int num16 = lastPanelH + num14;
						panelW = (int)((float)panelW + (float)(num15 - panelW) * num12);
						panelH = (int)((float)panelH + (float)(num16 - panelH) * num12);
						lastMouseX = GameCanvas.px;
						lastMouseY = GameCanvas.py;
						lastPanelW = panelW;
						lastPanelH = panelH;
						panelW = System.Math.Max(180, System.Math.Min(GameCanvas.w - panelX, panelW));
						panelH = System.Math.Max(120, System.Math.Min(GameCanvas.h - panelY, panelH));
					}
				}
			}
			else if (!GameCanvas.isPointerDown)
			{
				isResizing = false;
			}
			g.setColor(0, 0.7f);
			g.fillRect(panelX, panelY, panelW, panelH, 5);
			for (int i = 0; i < 3; i++)
			{
				g.setColor(16777215, 0.2f - (float)i * 0.05f);
				g.drawRect(panelX + i, panelY + i, panelW - i * 2, panelH - i * 2);
			}
			g.setColor(16777215, 0.8f);
			for (int j = 0; j < 3; j++)
			{
				g.drawLine(num4 + 5, panelY + panelH - 10 - j * 5, panelX + panelW - 5, panelY + panelH - 10 - j * 5);
			}
			for (int k = 0; k < 3; k++)
			{
				g.drawLine(panelX + panelW - 10 - k * 5, num5 + 5, panelX + panelW - 10 - k * 5, panelY + panelH - 5);
			}
			int num17 = 16;
			g.setColor(16733525);
			g.fillRect(panelX + panelW - num17 - 5, panelY + 5, num17, num17, 5);
			mFont.tahoma_7b_white.drawString(g, "X", panelX + panelW - num17 / 2 - 5, panelY + 7, mFont.CENTER);
			int num18 = 80;
			int num19 = 20;
			int num20 = panelX + panelW - 80;
			int num21 = panelY + panelH - num19 + 25;
			g.setColor(isAutoFilterItem ? 65280 : 16711680);
			g.fillRect(num20, num21, num18, num19, 8);
			string st = (isAutoFilterItem ? "Auto: Bật" : "Auto: Tắt");
			mFont.tahoma_7b_white.drawStringBorder(g, st, num20 + num18 / 2 + 1, num21 + 6 + 1, mFont.CENTER, mFont.tahoma_7_grey);
			if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease && GameCanvas.isPointerHoldIn(num20, num21, num18, num19))
			{
				isAutoFilterItem = !isAutoFilterItem;
				GameScr.info1.addInfo(isAutoFilterItem ? "Đã bật auto lọc đồ" : "Đã tắt auto lọc đồ", 0);
				GameCanvas.clearAllPointerEvent();
			}
			string st2 = "Danh sách vật phẩm lọc";
			int num22 = panelY + num2;
			mFont.tahoma_7b_white.drawString(g, st2, panelX + panelW / 2, num22, mFont.CENTER);
			g.setColor(5987163);
			g.fillRect(panelX + num2, num22 + 12, panelW - num2 * 2, 1, 5);
			g.setClip(panelX, panelY + 35, panelW, panelH - 45);
			int num23 = listFilterItems.Count * num;
			int num24 = panelH - 45;
			int num25 = System.Math.Max(0, num23 - num24);
			if (GameCanvas.isPointerDown && !isDragging && !isResizing)
			{
				GameCanvas.isPointerJustDown = false;
				if (!isScrolling)
				{
					isScrolling = true;
					lastMouseY = GameCanvas.py;
					lastScrollY = scrollY;
				}
				else
				{
					float num26 = 1f;
					int num27 = lastMouseY - GameCanvas.py;
					int num28 = lastScrollY + num27;
					scrollY = (int)((float)scrollY + (float)(num28 - scrollY) * num26);
					lastMouseY = GameCanvas.py;
					lastScrollY = scrollY;
				}
				scrollY = System.Math.Max(0, System.Math.Min(num25, scrollY));
			}
			else if (!GameCanvas.isPointerDown)
			{
				isScrolling = false;
			}
			int num29 = scrollY / num;
			int num30 = System.Math.Min(num29 + MAX_ITEMS_VISIBLE, listFilterItems.Count);
			for (int l = num29; l < num30; l++)
			{
				ItemAutoFilter itemAutoFilter = listFilterItems[l];
				int num31 = panelY + 35 + l * num - scrollY;
				if (l % 2 == 0)
				{
					g.setColor(2105376, 0.3f);
					g.fillRect(panelX + 5, num31, panelW - 10, num - 2, 5);
				}
				string st3 = itemAutoFilter.name ?? "";
				mFont.tahoma_7_white.drawString(g, st3, panelX + num2, num31 + 5, 0);
				mFont.tahoma_7_red.drawString(g, $"ID: {itemAutoFilter.id}", panelX + num2, num31 + 15, 0);
				int num32 = 35;
				int h2 = 18;
				int num33 = panelX + panelW - num32 - num2;
				int num34 = num31 + 3;
				g.setColor(16724787);
				g.fillRect(num33, num34, num32, h2, 5);
				mFont.tahoma_7b_white.drawString(g, "Xóa", num33 + num32 / 2, num34 + 4, mFont.CENTER);
				if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
				{
					GameCanvas.isPointerJustDown = false;
					if (GameCanvas.isPointerHoldIn(num33, num34, num32, h2))
					{
						listFilterItems.RemoveAt(l);
						GameScr.info1.addInfo("Đã xóa vật phẩm khỏi danh sách lọc", 0);
						GameCanvas.clearAllPointerEvent();
					}
				}
			}
			g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			int num35 = panelX + panelW - 8;
			int num36 = panelY + 35;
			int num37 = panelH - 45;
			int num38 = 4;
			g.setColor(3355443);
			g.fillRect(num35, num36, num38, num37, 5);
			g.setColor(6710886);
			g.drawRect(num35, num36, num38, num37);
			if (num23 > num24)
			{
				float num39 = (float)num24 / (float)num23;
				int num40 = (int)((float)num37 * num39);
				int val = num36;
				if (scrollY > 0)
				{
					float num41 = (float)scrollY / (float)num25;
					val = num36 + (int)((float)(num37 - num40) * num41);
				}
				val = System.Math.Max(num36, System.Math.Min(num36 + num37 - num40, val));
				g.setColor(8947848);
				g.fillRect(num35 + 1, val, num38 - 2, num40, 5);
				g.setColor(11184810);
				g.fillRect(num35 + 1, val, num38 - 2, 2, 5);
				g.setColor(6710886);
				g.fillRect(num35 + 1, val + num40 - 2, num38 - 2, 2, 5);
			}
			if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
			{
				GameCanvas.isPointerJustDown = false;
				if (GameCanvas.isPointerHoldIn(panelX + panelW - num17 - 5, panelY + 5, num17, num17))
				{
					isShowFilterList = false;
					scrollY = 0;
					GameCanvas.clearAllPointerEvent();
				}
			}
		}

		private void DoFilter()
		{
			if (!isAutoFilterItem)
			{
				return;
			}
			try
			{
				for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
				{
					Item item = Char.myCharz().arrItemBag[i];
					if (item == null)
					{
						continue;
					}
					foreach (ItemAutoFilter listFilterItem in listFilterItems)
					{
						if (listFilterItem.id == item.template.id)
						{
							Service.gI().useItem(1, 1, (sbyte)item.indexUI, -1);
							Thread.Sleep(50);
							return;
						}
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void DoBoss()
		{
			if (string.IsNullOrEmpty(bossCanDo))
			{
				GameScr.info1.addInfo("Chưa nhập boss cần tìm", 0);
				zoneMacDinh = 0;
				isdoBoss = false;
				return;
			}
			if (Input.GetKey("q"))
			{
				GameScr.info1.addInfo("Đã tắt auto dò boss", 0);
				isdoBoss = false;
				return;
			}
			for (int i = 0; i < GameScr.vCharInMap.size(); i++)
			{
				Char @char = (Char)GameScr.vCharInMap.elementAt(i);
				if (@char != null && @char.cName.ToLower().Contains(bossCanDo.ToLower()) && @char.cTypePk == 5)
				{
					Sound.start(1f, Sound.l1);
					GameScr.info1.addInfo("Đã tìm thấy boss", 0);
					zoneMacDinh = 0;
					isdoBoss = false;
					return;
				}
			}
			if (GameScr.gI().numPlayer == null || GameScr.gI().numPlayer.Length == 0)
			{
				Service.gI().openUIZone();
				return;
			}
			Service.gI().requestChangeZone(zoneMacDinh, -1);
			if (!Char.isLoadingMap && TileMap.zoneID == zoneMacDinh)
			{
				zoneMacDinh++;
				if (zoneMacDinh >= GameScr.gI().numPlayer.Length)
				{
					zoneMacDinh = 0;
				}
			}
		}

		public static void LoadImgMenuChat()
		{
			imgMenuChat = GameCanvas.loadImage("/mainImage/MenuChat.png");
			imgCloseButton = GameCanvas.loadImage("/mainImage/myTexture2dbtX.png");
			imgNextPage = GameCanvas.loadImage("/mainImage/myTexture2dbtnl.png");
			imgNextPage2 = GameCanvas.loadImage("/mainImage/myTexture2dbtnlf.png");
			imgPrevPage = GameCanvas.loadImage("/mainImage/myTexture2dbtnl.png");
			imgPrevPage2 = GameCanvas.loadImage("/mainImage/myTexture2dbtnlf.png");
		}

		public static void PaintMenuChat(mGraphics g)
		{
			if (!isShowMenuChat)
			{
				return;
			}
			int num = (GameCanvas.w - imgMenuChat.getWidth()) / 2;
			int num2 = (GameCanvas.h - imgMenuChat.getHeight()) / 2;
			g.drawImage(imgMenuChat, num, num2);
			g.drawImage(imgCloseButton, num + imgMenuChat.getWidth() - imgCloseButton.getWidth(), num2);
			string st = "Nhập lệnh chat tại đây:";
			mFont.tahoma_7b_red.drawString(g, st, num + 30, num2 + 10, mFont.LEFT);
			Dictionary<string, string> dictionary = new Dictionary<string, string>
			{
				{ "loadskill", "Tải lại ô skill" },
				{ "ak", "Bật/tắt tự động tấn công" },
				{ "ts", "Bật/tắt chế độ tàn sát" },
				{ "vqmm", "Bật/tắt tự động VQMM" },
				{ "ukhu", "Bật/tắt cập nhật khu tự động" },
				{ "k X", "Chuyển đến khu X (VD: k 5)" },
				{ "s X", "Thay đổi tốc độ game (1-10)" },
				{ "showbg", "Để Hiển thị ảnh nền" },
				{ "bg", "set hình nền game" },
				{ "autoitem", "Bật Tắt Auto dùng item" },
				{ "gtv", "Bật/tắt gõ Tiếng Việt" }
			};
			int num3 = (int)System.Math.Ceiling((double)dictionary.Count / 8.0);
			int num4 = currentPage * 11;
			int num5 = System.Math.Min(num4 + 11, dictionary.Count);
			int num6 = num2 + 30;
			for (int i = num4; i < num5; i++)
			{
				KeyValuePair<string, string> keyValuePair = dictionary.ElementAt(i);
				string st2 = keyValuePair.Key + ": " + keyValuePair.Value;
				mFont.tahoma_7_yellow.drawStringBorder(g, st2, num + 35, num6, mFont.LEFT, mFont.tahoma_7_grey);
				num6 += 15;
			}
			if (currentPage > 0)
			{
				g.drawImage(imgPrevPage, num + 30, num2 + imgMenuChat.getHeight() - 30);
				mFont.tahoma_7b_white.drawString(g, "Trang trước", num + 40, num2 + imgMenuChat.getHeight() - 22, mFont.LEFT);
			}
			if (currentPage < num3 - 1)
			{
				g.drawImage(imgNextPage, num + imgMenuChat.getWidth() - 100, num2 + imgMenuChat.getHeight() - 30);
				mFont.tahoma_7b_white.drawString(g, "Trang sau", num + imgMenuChat.getWidth() - 43, num2 + imgMenuChat.getHeight() - 22, mFont.RIGHT);
			}
			PaintLogoGifMenu(g, num + imgMenuChat.getWidth() - 150, num2 + (imgMenuChat.getHeight() - FrameGifMenu) / 2, mFont.CENTER);
			if (GameCanvas.isPointerClick && GameCanvas.isPointerJustRelease)
			{
				if (GameCanvas.isPointerHoldIn(num + imgMenuChat.getWidth() - imgCloseButton.getWidth(), num2, imgCloseButton.getWidth(), imgCloseButton.getHeight()))
				{
					isShowMenuChat = false;
					GameCanvas.clearAllPointerEvent();
				}
				if (currentPage > 0 && GameCanvas.isPointerHoldIn(num + 10, num2 + imgMenuChat.getHeight() - 30, 80, 20))
				{
					currentPage--;
				}
				if (currentPage < num3 - 1 && GameCanvas.isPointerHoldIn(num + imgMenuChat.getWidth() - 80, num2 + imgMenuChat.getHeight() - 30, 80, 20))
				{
					currentPage++;
				}
			}
		}

		public void SetAutoIntrinsic(int param)
		{
			int result;
			if (curSelectIntrinsic.Length <= 0)
			{
				GameScr.info1.addInfo("Chỉ số đã chọn không đúng! (1)", 0);
			}
			else if (int.TryParse(curSelectIntrinsic.Split("đến ")[1].Split("%")[0], out result) && param > 0 && param <= result)
			{
				paramIntrinsic = param;
				if (curSelectIntrinsic.Contains("+"))
				{
					curSelectIntrinsic = curSelectIntrinsic.Split("+")[0].Trim();
				}
				else
				{
					if (!curSelectIntrinsic.Contains("dưới"))
					{
						paramIntrinsic = -1;
						curSelectIntrinsic = "";
						GameScr.info1.addInfo("Có lỗi xảy ra, vui lòng liên hệ ADMIN!", 0);
						return;
					}
					curSelectIntrinsic = curSelectIntrinsic.Split("dưới ")[0].Trim();
				}
				new Thread(DoAutoIntrinsic).Start();
			}
			else
			{
				GameScr.info1.addInfo("Chỉ số đã chọn không đúng! (0)", 0);
			}
		}

		private void DoAutoIntrinsic()
		{
			while (paramIntrinsic != -1)
			{
				Service.gI().speacialSkill(0);
				Thread.Sleep(500);
				Service.gI().confirmMenu(5, 2);
				Thread.Sleep(500);
				Service.gI().confirmMenu(5, 0);
				Thread.Sleep(500);
			}
		}

		public void CheckAutoIntrinsic(string info)
		{
			if (info.Contains("+"))
			{
				string[] array = info.Split("+");
				string text = array[0].Trim();
				if (int.TryParse(array[1].Split("%")[0], out var result) && curSelectIntrinsic == text && result >= paramIntrinsic)
				{
					GameScr.info1.addInfo("Mở nội tại " + curSelectIntrinsic + " " + paramIntrinsic + "% thành công!", 0);
					paramIntrinsic = -1;
					curSelectIntrinsic = "";
					GameCanvas.menu.menuSelectedItem = GameCanvas.menu.menuItems.size() - 1;
					GameCanvas.menu.performSelect();
					GameCanvas.menu.doCloseMenu();
				}
			}
			else if (info.Contains("dưới"))
			{
				string[] array2 = info.Split("dưới ");
				string text2 = array2[0].Trim();
				if (int.TryParse(array2[1].Split("%")[0], out var result2) && curSelectIntrinsic == text2 && result2 >= paramIntrinsic)
				{
					GameScr.info1.addInfo("Mở nội tại " + curSelectIntrinsic + " " + paramIntrinsic + "% thành công!", 0);
					paramIntrinsic = -1;
					curSelectIntrinsic = "";
					GameCanvas.menu.menuSelectedItem = GameCanvas.menu.menuItems.size() - 1;
					GameCanvas.menu.performSelect();
					GameCanvas.menu.doCloseMenu();
				}
			}
			else
			{
				paramIntrinsic = -1;
				curSelectIntrinsic = "";
				GameCanvas.menu.doCloseMenu();
			}
		}
	}
}
