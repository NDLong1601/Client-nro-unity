namespace Game1
{
	/// <summary>Client-side snapshot of the server-authoritative shared clan tree.</summary>
	public class ClanTree : IActionListener
	{
		public const sbyte REQUEST_VIEW = 104;
		public const sbyte REQUEST_WATER = 105;
		public const sbyte REQUEST_FERTILIZE = 106;
		public const sbyte REQUEST_HARVEST = 107;
		public const sbyte REQUEST_ASK_HELP = 108;
		public const sbyte REQUEST_HELP_WATER = 109;
		public const sbyte REQUEST_COMPLETE_UPGRADE = 110;
		public const sbyte REQUEST_START_UPGRADE = 111;

		private const int TREE_X = 900;
		private const int TREE_GROUND_Y = 192;
		private const int INTERACTION_DISTANCE_X = 95;
		private const int INTERACTION_DISTANCE_Y = 80;
		private const int ACTION_OPEN_MENU = 21000;
		private const int ACTION_WATER = 21001;
		private const int ACTION_FERTILIZE = 21002;
		private const int ACTION_HARVEST = 21003;
		private const int ACTION_ASK_HELP = 21004;
		private const int ACTION_COMPLETE_UPGRADE = 21005;
		private const int ACTION_START_UPGRADE = 21006;
		private const int IMAGE_SET_VERSION = 3;
		private const string IMAGE_SET_VERSION_KEY = "ClanTreeImageVersion_Game1";

		public static ClanTree current = new ClanTree();
		public int clanId = -1;
		public int level = 1;
		public long growth;
		public long growthRequired;
		public int water;
		public int waterRequired;
		public int fertilizer;
		public int fertilizerRequired;
		public int waterToday;
		public int waterLimit;
		public int fertilizerToday;
		public int fertilizerLimit;
		public long pendingGold;
		public int pendingCapsule;
		public long helpExpiresAt;
		public long upgradeStartedAt;
		public long upgradeReadyAt;
		public long version;
		public bool loaded;
		private static long lastSnapshotRequestAt;
		private static Command interactionCommand;
		private static bool imageSetVersionChecked;

		public static bool receive(Message message, sbyte action)
		{
			if (action != REQUEST_VIEW)
			{
				return false;
			}
			try
			{
				ensureImageSetVersion();
				int receivedClanId = message.reader().readInt();
				if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId)
				{
					return true;
				}
				current.clanId = receivedClanId;
				current.level = message.reader().readUnsignedByte();
				current.growth = message.reader().readLong();
				current.growthRequired = message.reader().readLong();
				current.water = message.reader().readInt();
				current.waterRequired = message.reader().readInt();
				current.fertilizer = message.reader().readInt();
				current.fertilizerRequired = message.reader().readInt();
				current.waterToday = message.reader().readUnsignedByte();
				current.waterLimit = message.reader().readUnsignedByte();
				current.fertilizerToday = message.reader().readUnsignedByte();
				current.fertilizerLimit = message.reader().readUnsignedByte();
				current.pendingGold = message.reader().readLong();
				current.pendingCapsule = message.reader().readInt();
				current.helpExpiresAt = message.reader().readLong();
				current.upgradeStartedAt = message.reader().readLong();
				current.upgradeReadyAt = message.reader().readLong();
				current.version = message.reader().readLong();
				if (message.reader().available() > 0)
				{
					int detailVersion = message.reader().readUnsignedByte();
					if (detailVersion >= 1 && message.reader().available() >= 24)
					{
						long clanExp = message.reader().readLong();
						long clanExpRequired = message.reader().readLong();
						long progressionVersion = message.reader().readLong();
						ClanProgression.applyExpSnapshot(receivedClanId, clanExp,
							clanExpRequired, progressionVersion);
						if (detailVersion >= 2 && message.reader().available() > 0)
						{
							ClanAppearanceData appearance = ClanAppearance.readDetails(message.reader(), 1);
							ClanAppearance.applySnapshot(appearance, receivedClanId);
						}
					}
				}
				current.loaded = true;
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanTree.receive: " + ex.Message);
			}
			return true;
		}

		public static bool isReady(int expectedClanId)
		{
			return current.loaded && current.clanId == expectedClanId;
		}

		public static void requestSnapshot(bool force)
		{
			if (Char.myCharz().clan == null)
			{
				return;
			}
			long now = mSystem.currentTimeMillis();
			if (!force && now - lastSnapshotRequestAt < 2000L)
			{
				return;
			}
			lastSnapshotRequestAt = now;
			Service.gI().clanTree(REQUEST_VIEW);
		}

		public static void reset()
		{
			current = new ClanTree();
			lastSnapshotRequestAt = 0L;
			interactionCommand = null;
		}

		public static string imageName()
		{
			return "cay_lv_" + current.level.ToString("D2");
		}

		private static string imageName(int displayLevel)
		{
			return "cay_lv_" + displayLevel.ToString("D2");
		}

		private static string displayImageName(int clanId, int displayLevel)
		{
			if (ClanAppearance.isReady(clanId) && ClanAppearance.current.enabled
				&& ClanAppearance.current.treeLevel == displayLevel)
			{
				return ClanAppearance.current.resourceName;
			}
			return imageName(displayLevel);
		}

		private static void ensureImageSetVersion()
		{
			if (imageSetVersionChecked)
			{
				return;
			}
			imageSetVersionChecked = true;
			if (Rms.loadRMSInt(IMAGE_SET_VERSION_KEY) == IMAGE_SET_VERSION)
			{
				return;
			}
			for (int level = 1; level <= 20; level++)
			{
				string name = imageName(level);
				ImgByName.hashImagePath.remove(name);
				for (int scale = 1; scale <= 4; scale++)
				{
					Rms.deleteRecord(scale + "ImgByName_" + name);
				}
			}
			Rms.saveRMSInt(IMAGE_SET_VERSION_KEY, IMAGE_SET_VERSION);
		}

		public static bool isUpgrading()
		{
			return current.upgradeReadyAt > 0L;
		}

		public static bool canCompleteUpgrade()
		{
			return isUpgrading() && mSystem.currentTimeMillis() >= current.upgradeReadyAt;
		}

		public static string upgradeRemainingText()
		{
			if (!isUpgrading())
			{
				return "Chưa nâng cấp";
			}
			long seconds = (current.upgradeReadyAt - mSystem.currentTimeMillis() + 999L) / 1000L;
			if (seconds <= 0L)
			{
				return "Đã hoàn tất";
			}
			return NinjaUtil.getTime((int)System.Math.Min(2147483647L, seconds));
		}

		/// <summary>Shows the same map interaction bubble used by nearby interactive objects.</summary>
		public static void updateInteraction()
		{
			if (TileMap.mapID != 153 || Char.myCharz().clan == null || !GameCanvas.isTouch)
			{
				clearInteractionCommand();
				return;
			}
			bool near = System.Math.Abs(Char.myCharz().cx - TREE_X) <= INTERACTION_DISTANCE_X
				&& System.Math.Abs(Char.myCharz().cy - TREE_GROUND_Y) <= INTERACTION_DISTANCE_Y;
			if (!near || Char.myCharz().charFocus != null)
			{
				clearInteractionCommand();
				return;
			}
			if (interactionCommand == null)
			{
				interactionCommand = new Command("Cây bang", current, ACTION_OPEN_MENU, null);
				interactionCommand.isPlaySoundButton = false;
			}
			if (Char.myCharz().cmdMenu != null && Char.myCharz().cmdMenu.actionListener != current)
			{
				return;
			}
			int displayLevel = isReady(Char.myCharz().clan.ID) ? current.level : 1;
			MainImage image = ImgByName.getImagePath(displayImageName(Char.myCharz().clan.ID, displayLevel), ImgByName.hashImagePath);
			int imageHeight = image.img == null ? 80 : image.img.getHeight();
			interactionCommand.x = TREE_X - GameScr.cmx;
			interactionCommand.y = TREE_GROUND_Y - imageHeight - 16 - GameScr.cmy;
			Char.myCharz().cmdMenu = interactionCommand;
		}

		private static void clearInteractionCommand()
		{
			if (Char.myCharz().cmdMenu != null && Char.myCharz().cmdMenu.actionListener == current)
			{
				Char.myCharz().cmdMenu = null;
			}
		}

		/// <summary>Clicking the rendered tree walks to it, then opens its local action menu.</summary>
		public static bool tryHandleClick(int mapX, int mapY)
		{
			if (TileMap.mapID != 153 || Char.myCharz().clan == null)
			{
				return false;
			}
			int displayLevel = isReady(Char.myCharz().clan.ID) ? current.level : 1;
			MainImage image = ImgByName.getImagePath(displayImageName(Char.myCharz().clan.ID, displayLevel), ImgByName.hashImagePath);
			int width = image.img == null ? 100 : image.img.getWidth();
			int height = image.img == null ? 90 : image.img.getHeight();
			if (mapX < TREE_X - width / 2 - 18 || mapX > TREE_X + width / 2 + 18
				|| mapY < TREE_GROUND_Y - height - 18 || mapY > TREE_GROUND_Y + 12)
			{
				return false;
			}
			GameCanvas.clearAllPointerEvent();
			if (isNearTree())
			{
				current.openInteractionMenu();
			}
			else
			{
				int targetX = Char.myCharz().cx < TREE_X ? TREE_X - 55 : TREE_X + 55;
				Char.myCharz().currentMovePoint = new MovePoint(targetX, TREE_GROUND_Y);
				Char.myCharz().endMovePointCommand = new Command(null, current, ACTION_OPEN_MENU, null);
			}
			return true;
		}

		private static bool isNearTree()
		{
			return System.Math.Abs(Char.myCharz().cx - TREE_X) <= INTERACTION_DISTANCE_X
				&& System.Math.Abs(Char.myCharz().cy - TREE_GROUND_Y) <= INTERACTION_DISTANCE_Y;
		}

		private void openInteractionMenu()
		{
			if (!isNearTree())
			{
				GameScr.info1.addInfo("Hãy lại gần Cây bang.", 0);
				return;
			}
			if (!isReady(Char.myCharz().clan.ID))
			{
				requestSnapshot(true);
			}
			MyVector choices = new MyVector();
			if (!isUpgrading())
			{
				choices.addElement(new Command("Tưới cây", this, ACTION_WATER, null));
				choices.addElement(new Command("Bón phân", this, ACTION_FERTILIZE, null));
				choices.addElement(new Command("Nâng cấp", this, ACTION_START_UPGRADE, null));
			}
			else
			{
				choices.addElement(new Command(canCompleteUpgrade() ? "Hoàn tất nâng cấp" : "Đang nâng: " + upgradeRemainingText(), this, ACTION_COMPLETE_UPGRADE, null));
			}
			choices.addElement(new Command("Nhận thưởng", this, ACTION_HARVEST, null));
			choices.addElement(new Command("Kêu gọi chăm", this, ACTION_ASK_HELP, null));
			GameCanvas.menu.startAt(choices, 3);
		}

		public void perform(int idAction, object p)
		{
			switch (idAction)
			{
			case ACTION_OPEN_MENU:
				openInteractionMenu();
				break;
			case ACTION_WATER:
				Service.gI().clanTree(REQUEST_WATER);
				break;
			case ACTION_FERTILIZE:
				Service.gI().clanTree(REQUEST_FERTILIZE);
				break;
			case ACTION_HARVEST:
				Service.gI().clanTree(REQUEST_HARVEST);
				break;
			case ACTION_ASK_HELP:
				Service.gI().clanTree(REQUEST_ASK_HELP);
				break;
			case ACTION_COMPLETE_UPGRADE:
				Service.gI().clanTree(REQUEST_COMPLETE_UPGRADE);
				break;
			case ACTION_START_UPGRADE:
				Service.gI().clanTree(REQUEST_START_UPGRADE);
				break;
			}
		}

		/// <summary>Renders the single shared tree only while viewing the clan territory.</summary>
		public static void paintInTerritory(mGraphics g)
		{
			if (TileMap.mapID != 153 || Char.myCharz().clan == null)
			{
				return;
			}
			ensureImageSetVersion();
			bool snapshotReady = isReady(Char.myCharz().clan.ID);
			if (!snapshotReady)
			{
				requestSnapshot(false);
			}
			if (!ClanAppearance.isReady(Char.myCharz().clan.ID))
			{
				ClanAppearance.requestSnapshot(false);
			}
			// Every clan owns a level-1 tree. Draw that safe visual fallback while
			// waiting for the server snapshot, then switch to the authoritative level.
			int displayLevel = snapshotReady ? current.level : 1;
			MainImage image = ImgByName.getImagePath(displayImageName(Char.myCharz().clan.ID, displayLevel), ImgByName.hashImagePath);
			if (image.img == null)
			{
				return;
			}
			paintAppearanceAura(g, image.img.getWidth(), image.img.getHeight());
			g.drawImage(image.img, TREE_X, TREE_GROUND_Y, mGraphics.BOTTOM | mGraphics.HCENTER);
			string label = "Cây bang " + Char.myCharz().clan.name + " - Cấp " + displayLevel;
			if (ClanAppearance.isReady(Char.myCharz().clan.ID) && ClanAppearance.current.enabled
				&& ClanAppearance.current.tierName.Length > 0)
			{
				label += " - " + ClanAppearance.current.tierName;
			}
			int labelY = TREE_GROUND_Y - image.img.getHeight() - 12;
			bool nightMode = GameScr.gI().isRongThanXuatHien;
			mFont labelFont = nightMode ? mFont.tahoma_7b_white : mFont.tahoma_7b_yellow;
			if (!nightMode && ClanAppearance.isReady(Char.myCharz().clan.ID) && ClanAppearance.current.enabled)
			{
				labelFont.drawStringColor(g, label, TREE_X, labelY, mFont.CENTER, ClanAppearance.current.accentRgb);
			}
			else
			{
				labelFont.drawString(g, label, TREE_X, labelY, mFont.CENTER);
			}
			if (snapshotReady && isUpgrading())
			{
				string upgradeLabel = canCompleteUpgrade() ? "Nâng cấp hoàn tất - chạm cây để nhận" : "Đang nâng cấp: " + upgradeRemainingText();
				mFont.tahoma_7b_white.drawString(g, upgradeLabel, TREE_X, labelY + 14, mFont.CENTER);
			}
		}

		private static void paintAppearanceAura(mGraphics g, int imageWidth, int imageHeight)
		{
			if (Char.myCharz().clan == null || !ClanAppearance.isReady(Char.myCharz().clan.ID)
				|| !ClanAppearance.current.enabled || ClanAppearance.current.auraStyle <= 0)
			{
				return;
			}
			int pulse = (GameCanvas.gameTick / 4) % 3;
			g.setColor(ClanAppearance.current.accentRgb);
			for (int ring = 0; ring < ClanAppearance.current.auraStyle; ring++)
			{
				int padding = 3 + pulse + ring * 4;
				g.drawRect(TREE_X - imageWidth / 2 - padding,
					TREE_GROUND_Y - imageHeight - padding,
					imageWidth + padding * 2, imageHeight + padding * 2);
			}
		}
	}
}
