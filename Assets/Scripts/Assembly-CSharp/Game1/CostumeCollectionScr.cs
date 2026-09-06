namespace Game1
{
	public class CostumeCollectionEntry
	{
		public int id;

		public string name;

		public string rarity;

		public string description;

		public int iconId;

		public string saleInfo;

		public MyVector options = new MyVector();

		public bool unlocked;

		public bool isUsing;

		public int rankColor;

		public int headPart;

		public int bodyPart;

		public int legPart;

		public sbyte gender = 3;

		public Char preview;

		public bool previewFailed;
	}

	public class CostumeCollectionAchievement
	{
		public int id;

		public string name;

		public string description;

		public int progress;

		public int target;

		public long rewardGold;

		public int rewardGem;

		public sbyte conditionType;

		public MyVector rewardItems = new MyVector();

		public MyVector requiredCostumes = new MyVector();

		public bool completed;

		public bool claimed;
	}

	public class CostumeCollectionRewardItem
	{
		public int id;

		public int iconId;

		public string name;

		public int quantity;
	}

	public class CostumeCollectionTargetItem
	{
		public int id;

		public int iconId;

		public string name;

		public bool owned;
	}

	public class CostumeCollectionScr : mScreen, IActionListener
	{
		private const int ACTION_CLOSE = 0;

		private const int BOOKMARK_COSTUMES = 0;

		private const int BOOKMARK_ACHIEVEMENTS = 1;

		private static CostumeCollectionScr instance;

		private readonly Scroll gridScroll = new Scroll();

		private readonly Scroll infoScroll = new Scroll();

		private readonly Scroll achievementScroll = new Scroll();

		private readonly Scroll achievementInfoScroll = new Scroll();

		private MyVector entries = new MyVector();

		private MyVector achievements = new MyVector();

		private int selectedIndex;

		private int selectedAchievementIndex;

		private int activeBookmark;

		private int bookX;

		private int bookY;

		private int bookW;

		private int bookH;

		private int pageW;

		private int leftX;

		private int rightX;

		private int gridX;

		private int gridY;

		private int gridW;

		private int gridH;

		private int columns;

		private int cellSize;

		private int lastWidth = -1;

		private int lastHeight = -1;

		private int lastEntryCount = -1;

		private bool loading;

		private int lastInfoEntryId = -1;

		private int lastInfoContentHeight = -1;

		private int lastInfoViewportHeight = -1;

		private bool claimingAchievement;

		private int achievementOwnedCount;

		private int achievementTotalCount;

		private int achievementListX;

		private int achievementListY;

		private int achievementListW;

		private int achievementListH;

		private int lastAchievementCount = -1;

		private int lastAchievementListW = -1;

		private int lastAchievementListH = -1;

		private int lastAchievementInfoId = -1;

		private int lastAchievementInfoContentHeight = -1;

		private int lastAchievementInfoViewportHeight = -1;

		private const int ACHIEVEMENT_CARD_STEP = 47;

		private const int LOCK_ICON_SIZE = 20;

		private const int BOOKMARK_ICON_SIZE = 25;

		private Image lockIcon;

		private int lockIconZoom = -1;

		private Image costumeBookmarkIcon;

		private Image achievementBookmarkIcon;

		private int bookmarkIconZoom = -1;

		public static CostumeCollectionScr gI()
		{
			if (instance == null)
			{
				instance = new CostumeCollectionScr();
			}
			return instance;
		}

		public void requestOpen()
		{
			loading = true;
			entries = new MyVector();
			achievements = new MyVector();
			selectedIndex = 0;
			selectedAchievementIndex = 0;
			activeBookmark = BOOKMARK_COSTUMES;
			claimingAchievement = false;
			lastEntryCount = -1;
			lastAchievementCount = -1;
			lastAchievementInfoId = -1;
			achievementInfoScroll.clear();
			switchToMe();
			Service.gI().SendRada(44, -1);
		}

		public void setEntries(MyVector costumeEntries)
		{
			receiveEntries(costumeEntries, true, true);
		}

		public void receiveEntries(MyVector costumeEntries, bool reset, bool complete)
		{
			int selectedEntryId = -1;
			CostumeCollectionEntry selectedEntry = getSelected();
			if (!reset && selectedEntry != null)
			{
				selectedEntryId = selectedEntry.id;
			}
			if (reset)
			{
				entries = new MyVector();
				selectedIndex = 0;
				gridScroll.clear();
			}
			if (costumeEntries != null)
			{
				for (int i = 0; i < costumeEntries.size(); i++)
				{
					entries.addElement(costumeEntries.elementAt(i));
				}
			}
			sortOwnedFirst();
			if (selectedEntryId >= 0)
			{
				selectedIndex = findEntryIndex(selectedEntryId);
			}
			loading = !complete;
			if (selectedIndex < 0 || selectedIndex >= entries.size())
			{
				selectedIndex = 0;
			}
			lastEntryCount = -1;
			prepareLayout();
		}

		public void receiveAchievements(MyVector collectionAchievements, int ownedCount, int totalCount)
		{
			int selectedId = -1;
			CostumeCollectionAchievement selectedAchievement = getSelectedAchievement();
			if (selectedAchievement != null)
			{
				selectedId = selectedAchievement.id;
			}
			achievements = collectionAchievements ?? new MyVector();
			achievementOwnedCount = ownedCount;
			achievementTotalCount = totalCount;
			if (selectedId >= 0)
			{
				for (int i = 0; i < achievements.size(); i++)
				{
					if (((CostumeCollectionAchievement)achievements.elementAt(i)).id == selectedId)
					{
						selectedAchievementIndex = i;
						break;
					}
				}
			}
			if (selectedAchievementIndex < 0 || selectedAchievementIndex >= achievements.size())
			{
				selectedAchievementIndex = 0;
			}
			claimingAchievement = false;
			lastAchievementCount = -1;
			lastAchievementInfoId = -1;
			prepareAchievementScroll();
		}

		private void sortOwnedFirst()
		{
			MyVector sortedEntries = new MyVector();
			for (int i = 0; i < entries.size(); i++)
			{
				CostumeCollectionEntry entry = (CostumeCollectionEntry)entries.elementAt(i);
				if (entry.unlocked)
				{
					sortedEntries.addElement(entry);
				}
			}
			for (int i = 0; i < entries.size(); i++)
			{
				CostumeCollectionEntry entry = (CostumeCollectionEntry)entries.elementAt(i);
				if (!entry.unlocked)
				{
					sortedEntries.addElement(entry);
				}
			}
			entries = sortedEntries;
		}

		private int findEntryIndex(int entryId)
		{
			for (int i = 0; i < entries.size(); i++)
			{
				if (((CostumeCollectionEntry)entries.elementAt(i)).id == entryId)
				{
					return i;
				}
			}
			return -1;
		}

		public override void switchToMe()
		{
			GameScr.isPaintOther = true;
			left = new Command(mResources.CLOSE, this, ACTION_CLOSE, null);
			center = null;
			prepareLayout();
			base.switchToMe();
		}

		private void prepareLayout()
		{
			if (lastWidth == GameCanvas.w && lastHeight == GameCanvas.h && lastEntryCount == entries.size())
			{
				return;
			}
			lastWidth = GameCanvas.w;
			lastHeight = GameCanvas.h;
			lastEntryCount = entries.size();

			bookW = System.Math.Min(468, GameCanvas.w - 42);
			if (bookW < 300)
			{
				bookW = GameCanvas.w - 8;
			}
			bookH = System.Math.Min(310, GameCanvas.h - mScreen.cmdH - 8);
			if (bookH < 176)
			{
				bookH = GameCanvas.h - mScreen.cmdH - 2;
			}
			bookX = (GameCanvas.w - bookW) / 2;
			bookY = (GameCanvas.h - mScreen.cmdH - bookH) / 2;
			pageW = (bookW - 8) / 2;
			leftX = bookX;
			rightX = bookX + pageW + 8;

			gridY = bookY + 35;
			gridH = bookH - 60;
			gridW = pageW - 20;
			columns = ((gridW >= 156) ? 4 : 3);
			cellSize = System.Math.Min(48, gridW / columns);
			if (cellSize < 30)
			{
				cellSize = 30;
			}
			gridX = leftX + (pageW - columns * cellSize) / 2;
			gridW = columns * cellSize;

			int oldTarget = gridScroll.cmtoY;
			gridScroll.clear();
			gridScroll.setStyle(entries.size(), cellSize, gridX, gridY, gridW, gridH, true, columns);
			gridScroll.moveTo(oldTarget);
		}

		private void prepareAchievementScroll()
		{
			achievementListX = leftX + 11;
			achievementListY = bookY + 42;
			achievementListW = pageW - 22;
			achievementListH = System.Math.Max(45, bookH - 58);
			if (lastAchievementCount == achievements.size()
				&& lastAchievementListW == achievementListW
				&& lastAchievementListH == achievementListH)
			{
				return;
			}
			int oldTarget = achievementScroll.cmtoY;
			achievementScroll.clear();
			achievementScroll.setStyle(achievements.size(), ACHIEVEMENT_CARD_STEP, achievementListX,
				achievementListY, achievementListW, achievementListH, true, 1);
			achievementScroll.moveTo(oldTarget);
			lastAchievementCount = achievements.size();
			lastAchievementListW = achievementListW;
			lastAchievementListH = achievementListH;
		}

		public override void update()
		{
			prepareLayout();
			prepareAchievementScroll();
			gridScroll.updatecm();
			infoScroll.updatecm();
			achievementScroll.updatecm();
			achievementInfoScroll.updatecm();
		}

		public override void updateKey()
		{
			if (GameCanvas.keyPressed[12] || mScreen.getCmdPointerLast(left))
			{
				GameCanvas.keyPressed[12] = false;
				GameCanvas.isPointerJustRelease = false;
				closeScreen();
				return;
			}

			if (handleBookmarkTouch())
			{
				return;
			}

			if (activeBookmark == BOOKMARK_ACHIEVEMENTS)
			{
				handleAchievementInput();
				return;
			}

			if (GameCanvas.isTouch)
			{
				ScrollResult result = gridScroll.updateKey();
				if (result.isFinish && result.selected >= 0 && result.selected < entries.size())
				{
					select(result.selected);
				}
				infoScroll.updateKey();
			}

			int nextIndex = selectedIndex;
			if (GameCanvas.keyPressed[(!Main.isPC) ? 4 : 23])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 4 : 23] = false;
				nextIndex--;
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 6 : 24])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 6 : 24] = false;
				nextIndex++;
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21] = false;
				nextIndex -= columns;
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22] = false;
				nextIndex += columns;
			}
			if (nextIndex != selectedIndex && entries.size() > 0)
			{
				if (nextIndex < 0)
				{
					nextIndex = 0;
				}
				if (nextIndex >= entries.size())
				{
					nextIndex = entries.size() - 1;
				}
				select(nextIndex);
			}

		}

		private bool handleBookmarkTouch()
		{
			if (!GameCanvas.isPointerClick || !GameCanvas.isPointerJustRelease)
			{
				return false;
			}
			int x = getBookmarkX();
			if (GameCanvas.isPointerHoldIn(x, bookY + 40, 36, 54))
			{
				activeBookmark = BOOKMARK_COSTUMES;
				consumePointer();
				return true;
			}
			if (GameCanvas.isPointerHoldIn(x, bookY + 96, 36, 54))
			{
				activeBookmark = BOOKMARK_ACHIEVEMENTS;
				consumePointer();
				return true;
			}
			return false;
		}

		private void handleAchievementInput()
		{
			if (GameCanvas.isTouch)
			{
				ScrollResult result = achievementScroll.updateKey();
				if (result.isFinish && result.selected >= 0 && result.selected < achievements.size())
				{
					selectedAchievementIndex = result.selected;
					lastAchievementInfoId = -1;
					achievementScroll.moveTo(selectedAchievementIndex * ACHIEVEMENT_CARD_STEP);
					SoundMn.gI().radarItem();
					return;
				}
				achievementInfoScroll.updateKey();
			}

			int nextIndex = selectedAchievementIndex;
			if (GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21] = false;
				nextIndex--;
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22] = false;
				nextIndex++;
			}
			if (nextIndex != selectedAchievementIndex && achievements.size() > 0)
			{
				selectedAchievementIndex = System.Math.Max(0, System.Math.Min(achievements.size() - 1, nextIndex));
				lastAchievementInfoId = -1;
				achievementScroll.moveTo(selectedAchievementIndex * ACHIEVEMENT_CARD_STEP);
				SoundMn.gI().radarItem();
			}

			if (!GameCanvas.isPointerClick || !GameCanvas.isPointerJustRelease)
			{
				return;
			}

			CostumeCollectionAchievement achievement = getSelectedAchievement();
			if (achievement != null && achievement.completed && !achievement.claimed && !claimingAchievement)
			{
				int buttonY = bookY + bookH - 28;
				int buttonX = rightX + 20;
				int buttonW = pageW - 40;
				if (GameCanvas.isPointerHoldIn(buttonX, buttonY, buttonW, 19))
				{
					claimingAchievement = true;
					Service.gI().SendRada(46, achievement.id);
					consumePointer();
				}
			}
		}

		private void consumePointer()
		{
			GameCanvas.isPointerClick = false;
			GameCanvas.isPointerJustRelease = false;
			GameCanvas.isPointerJustDown = false;
		}

		private int getBookmarkX()
		{
			return System.Math.Min(GameCanvas.w - 36, bookX + bookW - 3);
		}

		private int getAchievementCardY(int index)
		{
			return achievementListY + index * ACHIEVEMENT_CARD_STEP;
		}

		private void select(int index)
		{
			selectedIndex = index;
			lastInfoEntryId = -1;
			gridScroll.moveTo(selectedIndex / columns * cellSize);
			SoundMn.gI().radarItem();
		}

		public override void paint(mGraphics g)
		{
			GameScr.gI().paint(g);
			g.translate(-GameScr.cmx, -GameScr.cmy);
			g.translate(0, GameCanvas.transY);
			GameScr.resetTranslate(g);
			prepareLayout();

			g.fillRect(0, 0, GameCanvas.w, GameCanvas.h, 0, 72);
			paintBook(g);
			if (activeBookmark == BOOKMARK_ACHIEVEMENTS)
			{
				paintAchievementPages(g);
			}
			else
			{
				paintLeftPage(g);
				paintRightPage(g);
			}
			paintBookmarks(g);
			base.paint(g);
		}

		private void paintBook(mGraphics g)
		{
			g.setColor(0x2D160B);
			g.fillRect(bookX - 4, bookY + 3, bookW + 8, bookH + 3, 7);
			g.setColor(0x6D3E19);
			g.fillRect(bookX - 2, bookY, bookW + 4, bookH, 7);
			g.setColor(0xE8C98F);
			g.fillRect(leftX, bookY + 2, pageW, bookH - 4, 9);
			g.fillRect(rightX, bookY + 2, pageW, bookH - 4, 9);
			g.setColor(0xF3D9A7);
			g.fillRect(leftX + 3, bookY + 5, pageW - 5, bookH - 10, 8);
			g.fillRect(rightX + 2, bookY + 5, pageW - 5, bookH - 10, 8);
			g.setColor(0xA06F32);
			g.drawRect(leftX + 5, bookY + 7, pageW - 10, bookH - 14);
			g.drawRect(rightX + 5, bookY + 7, pageW - 10, bookH - 14);

			g.setColor(0x6A3B19);
			g.fillRect(bookX + pageW - 1, bookY + 3, 10, bookH - 6, 5);
			g.setColor(0xC18A47);
			g.fillRect(bookX + pageW + 2, bookY + 5, 2, bookH - 10);
			g.setColor(0xFFF0C5);
			g.fillRect(bookX + pageW + 6, bookY + 8, 1, bookH - 16);
		}

		private void paintBookmarks(mGraphics g)
		{
			int x = getBookmarkX();
			paintBookmark(g, x, bookY + 40, BOOKMARK_COSTUMES, 0x80511F);
			paintBookmark(g, x, bookY + 96, BOOKMARK_ACHIEVEMENTS, 0x6B4A8D);
		}

		private void paintBookmark(mGraphics g, int x, int y, int bookmark, int accentColor)
		{
			bool active = activeBookmark == bookmark;
			int width = 34;
			int height = 52;
			g.setColor(0x4E2C18);
			g.fillRect(x + 2, y + 3, width, height, 7);
			g.setColor(active ? accentColor : 0x5A4838);
			g.fillRect(x, y, width, height, 7);
			g.setColor(0xD9A544);
			g.drawRect(x + 1, y + 1, width - 2, height - 2);
			g.setColor(active ? 0xFFE391 : 0xB88642);
			g.drawRect(x + 3, y + 3, width - 6, height - 6);
			g.fillRect(x + 5, y + 5, width - 10, 2, 1);
			g.fillRect(x + 5, y + height - 7, width - 10, 2, 1);
			g.setColor(active ? 0xFFF0B3 : 0xD5A763);
			g.fillRect(x + width / 2 - 2, y + 7, 4, 4, 2);
			g.fillRect(x + width / 2 - 2, y + height - 11, 4, 4, 2);
			Image icon = getBookmarkIcon(bookmark);
			if (icon != null)
			{
				g.drawImage(icon, x + width / 2, y + height / 2, mGraphics.VCENTER | mGraphics.HCENTER);
			}
			else
			{
				g.setColor(active ? 0xFFF2B4 : 0xCFA766);
				g.fillRect(x + width / 2 - 6, y + height / 2 - 6, 12, 12, 6);
			}
			if (active)
			{
				g.setColor((GameCanvas.gameTick / 5 % 2 == 0) ? 0xFFF6C2 : 0xE5AD3E);
				g.drawRect(x - 1, y - 1, width + 2, height + 2);
			}
		}

		private void paintAchievementPages(mGraphics g)
		{
			paintTitle(g, leftX + pageW / 2, "THÀNH TỰU SƯU TẬP");
			paintTitle(g, rightX + pageW / 2, "THÔNG TIN MỐC");
			mFont.tahoma_7b_dark.drawString(g, "Đã sưu tập: " + achievementOwnedCount + "/" + achievementTotalCount,
				leftX + pageW / 2, bookY + 33, mFont.CENTER);

			if (achievements.size() == 0)
			{
				mFont.tahoma_7_grey.drawString(g, loading ? "Đang tải thành tựu..." : "Chưa có thành tựu sưu tập",
					leftX + pageW / 2, bookY + bookH / 2, mFont.CENTER);
				return;
			}
			g.setClip(achievementListX, achievementListY, achievementListW + 6, achievementListH);
			g.translate(0, -achievementScroll.cmy);
			int firstVisible = System.Math.Max(0, achievementScroll.cmy / ACHIEVEMENT_CARD_STEP);
			int lastVisible = System.Math.Min(achievements.size(),
				(achievementScroll.cmy + achievementListH) / ACHIEVEMENT_CARD_STEP + 2);
			for (int i = firstVisible; i < lastVisible; i++)
			{
				paintAchievementCard(g, (CostumeCollectionAchievement)achievements.elementAt(i), i, i == selectedAchievementIndex);
			}
			g.translate(0, -g.getTranslateY());
			g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			paintAchievementScrollBar(g);
			paintAchievementDetail(g, getSelectedAchievement());
		}

		private void paintAchievementCard(mGraphics g, CostumeCollectionAchievement achievement, int index, bool selected)
		{
			int x = achievementListX;
			int y = getAchievementCardY(index);
			int width = achievementListW;
			bool canClaim = achievement.completed && !achievement.claimed;
			int border = selected ? 0xFFF0A0 : (canClaim ? 0xD98BFF : (achievement.claimed ? 0x7B8F61 : 0x957048));
			g.setColor(0x5D381E);
			g.fillRect(x + 1, y + 2, width, 42, 5);
			g.setColor(border);
			g.fillRect(x, y, width, 41, 5);
			g.setColor(canClaim ? 0x4D3567 : (achievement.claimed ? 0x5B7354 : 0xE1C69A));
			g.fillRect(x + 2, y + 2, width - 4, 37, 4);
			if (canClaim)
			{
				g.setColor(0x75518C);
				g.fillRect(x + 4, y + 4, width - 8, 2, 1);
				g.setColor((GameCanvas.gameTick / 4 % 2 == 0) ? 0xFFF2A6 : 0xE3AA47);
				g.fillRect(x + 5, y + 31, width - 10, 2, 1);
				g.fillRect(x + 6, y + 8, 3, 3, 2);
				g.fillRect(x + width - 9, y + 8, 3, 3, 2);
			}
			string progress = achievement.progress + "/" + achievement.target;
			if (canClaim)
			{
				mFont.tahoma_7b_yellow.drawString(g, achievement.name, x + 7, y + 6, mFont.LEFT);
				mFont.tahoma_7b_yellow.drawString(g, progress, x + width - 6, y + 6, mFont.RIGHT);
				mFont.tahoma_7b_yellow.drawString(g, "HOÀN THÀNH - NHẬN THƯỞNG", x + 7, y + 22, mFont.LEFT);
			}
			else if (achievement.claimed)
			{
				mFont.tahoma_7b_white.drawString(g, achievement.name, x + 7, y + 6, mFont.LEFT);
				mFont.tahoma_7b_white.drawString(g, progress, x + width - 6, y + 6, mFont.RIGHT);
				mFont.tahoma_7b_white.drawString(g, "Đã nhận thưởng", x + 7, y + 22, mFont.LEFT);
			}
			else
			{
				mFont.tahoma_7b_dark.drawString(g, achievement.name, x + 7, y + 6, mFont.LEFT);
				mFont.tahoma_7b_dark.drawString(g, progress, x + width - 6, y + 6, mFont.RIGHT);
				mFont.tahoma_7_grey.drawString(g, "Chưa hoàn thành", x + 7, y + 22, mFont.LEFT);
			}
			if (selected)
			{
				g.setColor(0xFFF3A0);
				g.drawRect(x - 1, y - 1, width + 2, 43);
			}
		}

		private void paintAchievementScrollBar(mGraphics g)
		{
			if (achievementScroll.cmyLim <= 0)
			{
				return;
			}
			int x = leftX + pageW - 11;
			g.setColor(0x8A5A2A);
			g.fillRect(x, achievementListY, 4, achievementListH, 2);
			int thumbH = System.Math.Max(16, achievementListH * achievementListH
				/ (achievementListH + achievementScroll.cmyLim));
			int thumbY = achievementListY + achievementScroll.cmy
				* (achievementListH - thumbH) / achievementScroll.cmyLim;
			g.setColor(0xE5B24E);
			g.fillRect(x - 1, thumbY, 6, thumbH, 3);
		}

		private void paintAchievementDetail(mGraphics g, CostumeCollectionAchievement achievement)
		{
			if (achievement == null)
			{
				return;
			}
			int x = rightX + 12;
			int width = pageW - 24;
			int y = bookY + 42;
			bool canClaim = achievement.completed && !achievement.claimed;
			g.setColor(canClaim ? 0x51366B : 0x3B294C);
			g.fillRect(x, y, width, 48, 6);
			g.setColor(canClaim ? 0xFFE28A : (achievement.completed ? 0x7B9E6A : 0x9A7955));
			g.drawRect(x + 1, y + 1, width - 2, 46);
			mFont.tahoma_7b_yellow.drawString(g, achievement.name, x + width / 2, y + 8, mFont.CENTER);
			mFont.tahoma_7b_white.drawString(g, canClaim ? "HOÀN THÀNH - SẴN SÀNG NHẬN" : (achievement.completed ? "ĐÃ NHẬN THƯỞNG" : "ĐANG THỰC HIỆN"),
				x + width / 2, y + 25, mFont.CENTER);

			int buttonX = rightX + 20;
			int buttonY = bookY + bookH - 28;
			int buttonW = pageW - 40;
			int viewportY = y + 56;
			int viewportHeight = System.Math.Max(18, buttonY - viewportY - 4);
			string description = achievement.description ?? string.Empty;
			string[] descriptionLines = mFont.tahoma_7.splitFontArray(description, width - 8);
			int rewardLineCount = ((achievement.rewardGold > 0L) ? 1 : 0)
				+ ((achievement.rewardGem > 0) ? 1 : 0);
			int rewardItemCount = (achievement.rewardItems == null) ? 0 : achievement.rewardItems.size();
			int targetCount = (achievement.requiredCostumes == null) ? 0 : achievement.requiredCostumes.size();
			int contentHeight = descriptionLines.Length * 10 + 18 + 15
				+ ((rewardLineCount == 0 && rewardItemCount == 0) ? 13 : rewardLineCount * 13 + rewardItemCount * 24)
				+ ((achievement.conditionType == 1) ? 17 + System.Math.Max(1, targetCount) * 24 : 0) + 6;
			prepareAchievementInfoScroll(achievement.id, x, viewportY, width, viewportHeight, contentHeight);

			g.setClip(x, viewportY, width, viewportHeight);
			g.translate(0, -achievementInfoScroll.cmy);
			int contentY = viewportY;
			for (int i = 0; i < descriptionLines.Length; i++)
			{
				mFont.tahoma_7_grey.drawString(g, descriptionLines[i], x, contentY, mFont.LEFT);
				contentY += 10;
			}
			contentY += 5;
			mFont.tahoma_7b_dark.drawString(g, "Tiến độ: " + achievement.progress + "/" + achievement.target, x, contentY, mFont.LEFT);
			contentY += 13;
			contentY = paintAchievementSectionTitle(g, x, contentY, width, "PHẦN THƯỞNG");
			if (achievement.rewardGold > 0L)
			{
				mFont.tahoma_7_green.drawString(g, "• Vàng x" + formatReward(achievement.rewardGold), x + 4, contentY, mFont.LEFT);
				contentY += 13;
			}
			if (achievement.rewardGem > 0)
			{
				mFont.tahoma_7_green.drawString(g, "• Ngọc x" + formatReward(achievement.rewardGem), x + 4, contentY, mFont.LEFT);
				contentY += 13;
			}
			for (int i = 0; i < rewardItemCount; i++)
			{
				contentY = paintAchievementRewardItem(g,
					(CostumeCollectionRewardItem)achievement.rewardItems.elementAt(i), x, contentY, width);
			}
			if (rewardLineCount == 0 && rewardItemCount == 0)
			{
				mFont.tahoma_7_grey.drawString(g, "Không có phần thưởng", x + 4, contentY, mFont.LEFT);
				contentY += 13;
			}

			if (achievement.conditionType == 1)
			{
				contentY = paintAchievementSectionTitle(g, x, contentY + 4, width, "MỤC TIÊU BỘ SƯU TẬP");
				if (targetCount == 0)
				{
					mFont.tahoma_7_grey.drawString(g, "Chưa có dữ liệu cải trang trong bộ", x + 4, contentY, mFont.LEFT);
					contentY += 24;
				}
				else
				{
					for (int i = 0; i < targetCount; i++)
					{
						contentY = paintAchievementTargetItem(g,
							(CostumeCollectionTargetItem)achievement.requiredCostumes.elementAt(i), x, contentY, width);
					}
				}
			}

			g.translate(0, achievementInfoScroll.cmy);
			g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			paintAchievementInfoScrollBar(g, x + width - 2, viewportY, viewportHeight);

			bool canClaimButton = canClaim && !claimingAchievement;
			g.setColor(canClaimButton ? 0x80511F : 0x6C6254);
			g.fillRect(buttonX, buttonY, buttonW, 19, 6);
			g.setColor(canClaimButton ? 0xE0AF50 : 0x998A76);
			g.drawRect(buttonX + 1, buttonY + 1, buttonW - 2, 17);
			string label = achievement.claimed ? "ĐÃ NHẬN" : (claimingAchievement ? "ĐANG NHẬN..." : (achievement.completed ? "NHẬN THƯỞNG" : "CHƯA HOÀN THÀNH"));
			mFont.tahoma_7b_yellow.drawString(g, label, buttonX + buttonW / 2, buttonY + 5, mFont.CENTER);
		}

		private string formatReward(long value)
		{
			string text = value.ToString();
			int firstDigit = text.StartsWith("-") ? 1 : 0;
			for (int index = text.Length - 3; index > firstDigit; index -= 3)
			{
				text = text.Insert(index, ".");
			}
			return text;
		}

		private int paintAchievementSectionTitle(mGraphics g, int x, int y, int width, string title)
		{
			g.setColor(0xA77A3D);
			g.drawLine(x, y, x + width - 3, y);
			mFont.tahoma_7b_dark.drawString(g, title, x + width / 2, y - 4, mFont.CENTER);
			return y + 10;
		}

		private int paintAchievementRewardItem(mGraphics g, CostumeCollectionRewardItem reward,
			int x, int y, int width)
		{
			g.setColor(0xE2D0A8);
			g.fillRect(x + 2, y, width - 7, 21, 4);
			if (reward.iconId >= 0)
			{
				SmallImage.drawSmallImage(g, reward.iconId, x + 15, y + 10, 0,
					mGraphics.VCENTER | mGraphics.HCENTER);
			}
			mFont.tahoma_7_green.drawString(g, reward.name + " x" + formatReward(reward.quantity),
				x + 29, y + 2, mFont.LEFT);
			mFont.tahoma_7_grey.drawString(g, "Vật phẩm #" + reward.id, x + 29, y + 11, mFont.LEFT);
			return y + 24;
		}

		private int paintAchievementTargetItem(mGraphics g, CostumeCollectionTargetItem target,
			int x, int y, int width)
		{
			g.setColor(target.owned ? 0xD7E5B2 : 0xDDD0B7);
			g.fillRect(x + 2, y, width - 7, 21, 4);
			paintCollectionCheck(g, x + 5, y + 6, target.owned);
			if (target.iconId >= 0)
			{
				SmallImage.drawSmallImage(g, target.iconId, x + 28, y + 10, 0,
					mGraphics.VCENTER | mGraphics.HCENTER);
			}
			if (target.owned)
			{
				mFont.tahoma_7_green.drawString(g, target.name, x + 42, y + 2, mFont.LEFT);
				mFont.tahoma_7_green.drawString(g, "#" + target.id + " • ĐÃ SỞ HỮU", x + 42, y + 11, mFont.LEFT);
			}
			else
			{
				mFont.tahoma_7_grey.drawString(g, target.name, x + 42, y + 2, mFont.LEFT);
				mFont.tahoma_7_grey.drawString(g, "#" + target.id + " • CHƯA SỞ HỮU", x + 42, y + 11, mFont.LEFT);
			}
			return y + 24;
		}

		private void paintCollectionCheck(mGraphics g, int x, int y, bool owned)
		{
			g.setColor(owned ? 0x4C9A42 : 0x9B8B74);
			g.drawRect(x, y, 9, 9);
			if (owned)
			{
				g.drawLine(x + 2, y + 5, x + 4, y + 7);
				g.drawLine(x + 4, y + 7, x + 8, y + 2);
			}
		}

		private void prepareAchievementInfoScroll(int achievementId, int x, int y,
			int width, int height, int contentHeight)
		{
			if (lastAchievementInfoId == achievementId
				&& lastAchievementInfoContentHeight == contentHeight
				&& lastAchievementInfoViewportHeight == height)
			{
				return;
			}
			int oldTarget = (lastAchievementInfoId == achievementId) ? achievementInfoScroll.cmtoY : 0;
			lastAchievementInfoId = achievementId;
			lastAchievementInfoContentHeight = contentHeight;
			lastAchievementInfoViewportHeight = height;
			achievementInfoScroll.clear();
			achievementInfoScroll.setStyle(System.Math.Max(1, (contentHeight + 9) / 10), 10,
				x, y, width, height, true, 1);
			achievementInfoScroll.moveTo(oldTarget);
		}

		private void paintAchievementInfoScrollBar(mGraphics g, int x, int y, int height)
		{
			if (achievementInfoScroll.cmyLim <= 0)
			{
				return;
			}
			g.setColor(0x8A5A2A);
			g.fillRect(x, y, 3, height, 2);
			int thumbH = System.Math.Max(12, height * height / (height + achievementInfoScroll.cmyLim));
			int thumbY = y + achievementInfoScroll.cmy * (height - thumbH) / achievementInfoScroll.cmyLim;
			g.setColor(0xE5B24E);
			g.fillRect(x - 1, thumbY, 5, thumbH, 2);
		}

		private void paintLeftPage(mGraphics g)
		{
			paintTitle(g, leftX + pageW / 2, "BỘ SƯU TẬP CẢI TRANG");
			g.setClip(gridX, gridY, gridW + 6, gridH);
			g.translate(0, -gridScroll.cmy);
			int firstVisibleRow = System.Math.Max(0, gridScroll.cmy / cellSize);
			int lastVisibleRow = System.Math.Min((entries.size() + columns - 1) / columns - 1,
				(gridScroll.cmy + gridH) / cellSize + 1);
			int firstVisibleIndex = firstVisibleRow * columns;
			int lastVisibleIndex = System.Math.Min(entries.size(), (lastVisibleRow + 1) * columns);
			for (int i = firstVisibleIndex; i < lastVisibleIndex; i++)
			{
				CostumeCollectionEntry entry = (CostumeCollectionEntry)entries.elementAt(i);
				int col = i % columns;
				int row = i / columns;
				int x = gridX + col * cellSize + 2;
				int y = gridY + row * cellSize + 2;
				paintGridCell(g, entry, x, y, cellSize - 5, i == selectedIndex);
			}
			g.translate(0, -g.getTranslateY());
			g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			paintScrollBar(g);

			int unlocked = 0;
			for (int i = 0; i < entries.size(); i++)
			{
				CostumeCollectionEntry entry = (CostumeCollectionEntry)entries.elementAt(i);
				if (entry.unlocked)
				{
					unlocked++;
				}
			}
			mFont.tahoma_7b_dark.drawString(g, "Đã sưu tập: " + unlocked + "/" + entries.size(), leftX + pageW / 2, bookY + bookH - 19, mFont.CENTER);
		}

		private void paintTitle(mGraphics g, int centerX, string title)
		{
			int titleW = System.Math.Min(pageW - 24, mFont.tahoma_7b_yellow.getWidth(title) + 18);
			g.setColor(0x4E2C18);
			g.fillRect(centerX - titleW / 2, bookY + 11, titleW, 18, 6);
			g.setColor(0xC58B38);
			g.drawRect(centerX - titleW / 2 + 1, bookY + 12, titleW - 2, 16);
			mFont.tahoma_7b_yellow.drawString(g, title, centerX, bookY + 16, mFont.CENTER);
		}

		private void paintGridCell(mGraphics g, CostumeCollectionEntry entry, int x, int y, int size, bool selected)
		{
			int borderColor = selected ? 0xF6C84A : (entry.unlocked ? 0xC28B35 : 0x98724B);
			g.setColor(0x67401F);
			g.fillRect(x + 1, y + 2, size, size, 5);
			g.setColor(borderColor);
			g.fillRect(x, y, size, size, 5);
			g.setColor(0xFFF0C5);
			g.fillRect(x + 2, y + 2, size - 4, size - 4, 4);
			g.setColor(entry.unlocked ? 0xE6C98F : 0xF0E1C4);
			g.fillRect(x + 4, y + 4, size - 8, size - 8, 3);
			g.setColor(entry.unlocked ? 0xA8742E : 0xAC8B62);
			g.drawRect(x + 3, y + 3, size - 6, size - 6);

			if (entry.iconId >= 0)
			{
				if (entry.unlocked)
				{
					SmallImage.drawSmallImage(g, entry.iconId, x + size / 2, y + size / 2, 0, mGraphics.VCENTER | mGraphics.HCENTER);
				}
				else
				{
					SmallImage.drawSmallImageGray(g, entry.iconId, x + size / 2, y + size / 2, mGraphics.VCENTER | mGraphics.HCENTER);
					paintLock(g, x + size - 10, y + size - 10);
				}
			}
			else
			{
				paintLockedSilhouette(g, x, y, size);
				if (!entry.unlocked)
				{
					paintLock(g, x + size - 10, y + size - 10);
				}
			}

			if (selected)
			{
				g.setColor((GameCanvas.gameTick / 5 % 2 == 0) ? 0xFFF3A0 : 0xE7A928);
				g.drawRect(x - 1, y - 1, size + 2, size + 2);
			}
		}

		private void paintLockedSilhouette(mGraphics g, int x, int y, int size)
		{
			int cx = x + size / 2;
			g.setColor(0x554637);
			g.fillRect(cx - 6, y + 7, 12, 12, 6);
			g.fillRect(cx - 9, y + 17, 18, size - 23, 6);
		}

		private void paintLock(mGraphics g, int x, int y)
		{
			Image icon = getLockIcon();
			if (icon != null)
			{
				g.drawImage(icon, x, y, mGraphics.VCENTER | mGraphics.HCENTER);
				return;
			}
			g.setColor(0x4A2C16);
			g.fillRect(x - 9, y - 9, 18, 18, 8);
			g.setColor(0xFFF0A3);
			g.drawRect(x - 8, y - 8, 16, 16);
			g.setColor(0x4B321F);
			g.fillRect(x - 5, y - 6, 10, 7, 4);
			g.setColor(0xFFE38A);
			g.fillRect(x - 3, y - 4, 6, 5, 3);
			g.setColor(0xF1B936);
			g.fillRect(x - 7, y - 1, 14, 10, 3);
			g.setColor(0xFFF1A6);
			g.drawRect(x - 6, y, 12, 8);
			g.setColor(0x5C3918);
			g.fillRect(x - 1, y + 2, 3, 5, 2);
		}

		private Image getLockIcon()
		{
			if (lockIcon != null && lockIconZoom == mGraphics.zoomLevel)
			{
				return lockIcon;
			}
			Image source = GameCanvas.loadImage("/collection/lock.png");
			if (source == null || source.texture == null)
			{
				return null;
			}
			int pixelSize = LOCK_ICON_SIZE * mGraphics.zoomLevel;
			lockIcon = new Image();
			lockIcon.texture = GameCanvas.Resize(source.texture, pixelSize, pixelSize);
			lockIcon.w = pixelSize;
			lockIcon.h = pixelSize;
			lockIcon.texture.filterMode = UnityEngine.FilterMode.Bilinear;
			lockIcon.texture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
			lockIconZoom = mGraphics.zoomLevel;
			return lockIcon;
		}

		private Image getBookmarkIcon(int bookmark)
		{
			if (bookmarkIconZoom != mGraphics.zoomLevel)
			{
				costumeBookmarkIcon = null;
				achievementBookmarkIcon = null;
				bookmarkIconZoom = mGraphics.zoomLevel;
			}
			if (bookmark == BOOKMARK_COSTUMES && costumeBookmarkIcon == null)
			{
				costumeBookmarkIcon = loadBookmarkIcon("/collection/bookmark_costume.png");
			}
			if (bookmark == BOOKMARK_ACHIEVEMENTS && achievementBookmarkIcon == null)
			{
				achievementBookmarkIcon = loadBookmarkIcon("/collection/bookmark_achievement.png");
			}
			return bookmark == BOOKMARK_COSTUMES ? costumeBookmarkIcon : achievementBookmarkIcon;
		}

		private Image loadBookmarkIcon(string path)
		{
			Image source = GameCanvas.loadImage(path);
			if (source == null || source.texture == null)
			{
				return null;
			}
			int pixelSize = BOOKMARK_ICON_SIZE * mGraphics.zoomLevel;
			Image result = new Image();
			result.texture = GameCanvas.Resize(source.texture, pixelSize, pixelSize);
			result.w = pixelSize;
			result.h = pixelSize;
			result.texture.filterMode = UnityEngine.FilterMode.Bilinear;
			result.texture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
			return result;
		}

		private void paintScrollBar(mGraphics g)
		{
			if (gridScroll.cmyLim <= 0)
			{
				return;
			}
			int x = leftX + pageW - 11;
			g.setColor(0x8A5A2A);
			g.fillRect(x, gridY, 4, gridH, 2);
			int thumbH = System.Math.Max(16, gridH * gridH / (gridH + gridScroll.cmyLim));
			int thumbY = gridY + gridScroll.cmy * (gridH - thumbH) / gridScroll.cmyLim;
			g.setColor(0xE5B24E);
			g.fillRect(x - 1, thumbY, 6, thumbH, 3);
		}

		private bool hasPartData(CostumeCollectionEntry entry)
		{
			return entry.headPart >= 0 || entry.bodyPart >= 0 || entry.legPart >= 0;
		}

		private bool isValidPart(int partId)
		{
			return partId >= 0 && GameScr.parts != null && partId < GameScr.parts.Length && GameScr.parts[partId] != null;
		}

		private Char getPreview(CostumeCollectionEntry entry)
		{
			if (entry.previewFailed || !hasPartData(entry) || Char.myCharz() == null)
			{
				return null;
			}
			int head = (entry.headPart >= 0) ? entry.headPart : Char.myCharz().head;
			int body = (entry.bodyPart >= 0) ? entry.bodyPart : Char.myCharz().body;
			int leg = (entry.legPart >= 0) ? entry.legPart : Char.myCharz().leg;
			if (!isValidPart(head) || !isValidPart(body) || !isValidPart(leg))
			{
				return null;
			}
			if (entry.preview == null || entry.preview.head != head || entry.preview.body != body || entry.preview.leg != leg)
			{
				entry.preview = new Char
				{
					head = head,
					body = body,
					leg = leg,
					bag = -1,
					cgender = Char.myCharz().cgender
				};
			}
			return entry.preview;
		}

		private string getCostumeType(CostumeCollectionEntry entry)
		{
			if (entry.headPart >= 0 && entry.bodyPart < 0 && entry.legPart < 0)
			{
				return "Thay đổi khuôn mặt/đầu";
			}
			if (entry.headPart >= 0 && entry.bodyPart >= 0 && entry.legPart >= 0)
			{
				return "Cải trang toàn thân";
			}
			if (hasPartData(entry))
			{
				return "Cải trang từng phần";
			}
			return "Ngoại hình đặc biệt";
		}

		private string getCostumeEffect(CostumeCollectionEntry entry)
		{
			if (entry.headPart >= 0 && entry.bodyPart < 0 && entry.legPart < 0)
			{
				return "Chỉ thay đổi khuôn mặt/đầu; giữ nguyên thân và chân hiện tại.";
			}
			if (entry.headPart >= 0 && entry.bodyPart >= 0 && entry.legPart >= 0)
			{
				return "Thay đổi toàn bộ ngoại hình nhân vật.";
			}
			if (hasPartData(entry))
			{
				return "Thay đổi các phần ngoại hình được cấu hình cho vật phẩm.";
			}
			return "Ngoại hình được kích hoạt bằng hiệu ứng riêng của vật phẩm.";
		}

		private string getGenderText(sbyte gender)
		{
			switch (gender)
			{
			case 0:
				return "Trái Đất";
			case 1:
				return "Namek";
			case 2:
				return "Xayda";
			default:
				return "Dùng chung";
			}
		}

		private void paintEntryPreview(mGraphics g, CostumeCollectionEntry entry, int x, int y, int width, int height)
		{
			bool painted = false;
			Char preview = getPreview(entry);
			if (preview != null)
			{
				try
				{
					preview.paintCharBody(g, x + width / 2, y + height - 12, 1, GameCanvas.gameTick / 8 % 2, isPaintBag: true);
					painted = true;
				}
				catch (System.Exception)
				{
					entry.previewFailed = true;
					entry.preview = null;
				}
			}
			if (!painted && entry.iconId >= 0)
			{
				SmallImage.drawSmallImage(g, entry.iconId, x + width / 2, y + height / 2, 0, mGraphics.VCENTER | mGraphics.HCENTER);
				painted = true;
			}
			if (!painted)
			{
				paintLockedSilhouette(g, x + width / 2 - 22, y + height / 2 - 22, 44);
			}
			if (!entry.unlocked)
			{
				g.fillRect(x + 2, y + 2, width - 4, height - 4, 0, 46);
				paintLock(g, x + width / 2, y + height / 2);
			}
		}

		private void paintRightPage(mGraphics g)
		{
			CostumeCollectionEntry entry = getSelected();
			if (entry == null)
			{
				mFont.tahoma_7_grey.drawString(g, loading ? "Đang tải dữ liệu cải trang..." : "Chưa có dữ liệu cải trang", rightX + pageW / 2, bookY + bookH / 2, mFont.CENTER);
				return;
			}

			int innerX = rightX + 11;
			int innerW = pageW - 22;
			int previewY = bookY + 13;
			int previewH = System.Math.Max(64, bookH / 2 - 28);
			g.setColor(0x3B294C);
			g.fillRect(innerX, previewY, innerW, previewH, 6);
			g.setColor(entry.rankColor);
			g.drawRect(innerX + 1, previewY + 1, innerW - 2, previewH - 2);
			g.setColor(0x59406B);
			for (int y = previewY + 8; y < previewY + previewH - 5; y += 12)
			{
				g.drawLine(innerX + 5, y, innerX + innerW - 5, y);
			}

			paintEntryPreview(g, entry, innerX, previewY, innerW, previewH);

			int nameY = previewY + previewH + 4;
			g.setColor(0x4E2C18);
			g.fillRect(innerX, nameY, innerW, 18, 5);
			mFont.tahoma_7b_yellow.drawString(g, entry.name, innerX + innerW / 2, nameY + 5, mFont.CENTER);

			int infoY = nameY + 22;
			mFont.tahoma_7b_dark.drawString(g, entry.rarity, innerX, infoY, mFont.LEFT);
			mFont.tahoma_7_grey.drawString(g, entry.unlocked ? "Đã sở hữu" : "Chưa sở hữu", innerX + innerW, infoY, mFont.RIGHT);
			int buttonY = bookY + bookH - 26;
			infoY += 13;
			string description = (string.IsNullOrEmpty(entry.description) || entry.description.Trim().Length == 0)
				? "Không có mô tả riêng từ dữ liệu vật phẩm."
				: entry.description;
			string saleInfo = (string.IsNullOrEmpty(entry.saleInfo) || entry.saleInfo.Trim().Length == 0)
				? "Chưa mở bán"
				: entry.saleInfo;
			string[] descriptionLines = mFont.tahoma_7.splitFontArray(description, innerW - 5);
			string[] effectLines = mFont.tahoma_7.splitFontArray("Hiệu ứng: " + getCostumeEffect(entry), innerW - 5);
			string[] saleLines = entry.unlocked ? new string[0] : mFont.tahoma_7.splitFontArray(saleInfo, innerW - 5);
			MyVector displayOptions = getDisplayOptions(entry);
			int optionCount = displayOptions.size();
			int contentHeight = descriptionLines.Length * 10 + 23 + effectLines.Length * 10 + 13
				+ System.Math.Max(1, optionCount) * 10 + saleLines.Length * 10 + 23;
			int viewportHeight = System.Math.Max(18, buttonY - infoY - 3);
			prepareInfoScroll(entry.id, innerX, infoY, innerW, viewportHeight, contentHeight);

			g.setClip(innerX, infoY, innerW, viewportHeight);
			g.translate(0, -infoScroll.cmy);
			int contentY = infoY;
			for (int i = 0; i < descriptionLines.Length; i++)
			{
				mFont.tahoma_7_grey.drawString(g, descriptionLines[i], innerX, contentY, mFont.LEFT);
				contentY += 10;
			}
			mFont.tahoma_7_blue.drawString(g, "Loại: " + getCostumeType(entry), innerX, contentY, mFont.LEFT);
			contentY += 10;
			mFont.tahoma_7_blue.drawString(g, "Phạm vi: " + getGenderText(entry.gender), innerX, contentY, mFont.LEFT);
			contentY += 10;
			for (int i = 0; i < effectLines.Length; i++)
			{
				mFont.tahoma_7_blue.drawString(g, effectLines[i], innerX, contentY, mFont.LEFT);
				contentY += 10;
			}
			contentY += 3;
			g.setColor(0xA77A3D);
			g.drawLine(innerX, contentY, innerX + innerW - 4, contentY);
			mFont.tahoma_7b_dark.drawString(g, "THÔNG SỐ", innerX + innerW / 2, contentY - 4, mFont.CENTER);
			contentY += 7;
			if (optionCount == 0)
			{
				mFont.tahoma_7_grey.drawString(g, "Không cộng thêm chỉ số", innerX, contentY, mFont.LEFT);
				contentY += 10;
			}
			else
			{
				for (int i = 0; i < optionCount; i++)
				{
					mFont.tahoma_7_green.drawString(g, "• " + (string)displayOptions.elementAt(i), innerX, contentY, mFont.LEFT);
					contentY += 10;
				}
			}
			for (int i = 0; i < saleLines.Length; i++)
			{
				mFont.tahoma_7_blue.drawString(g, saleLines[i], innerX, contentY, mFont.LEFT);
				contentY += 10;
			}
			g.translate(0, infoScroll.cmy);
			g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			paintInfoScrollBar(g, innerX + innerW - 3, infoY, viewportHeight);

			g.setColor(entry.unlocked ? 0x80511F : 0x6C6254);
			g.fillRect(innerX + 9, buttonY, innerW - 18, 17, 6);
			g.setColor(entry.unlocked ? 0xE0AF50 : 0x998A76);
			g.drawRect(innerX + 10, buttonY + 1, innerW - 20, 15);
			string status = entry.isUsing ? "ĐANG SỬ DỤNG" : (entry.unlocked ? "ĐÃ SỞ HỮU" : "CHƯA SỞ HỮU");
			mFont.tahoma_7b_yellow.drawString(g, status, innerX + innerW / 2, buttonY + 4, mFont.CENTER);
		}

		private MyVector getDisplayOptions(CostumeCollectionEntry entry)
		{
			// Dữ liệu cũ có thể dùng chuỗi giữ chỗ thay cho chỉ số thật.
			MyVector result = new MyVector();
			if (entry.options == null)
			{
				return result;
			}
			for (int i = 0; i < entry.options.size(); i++)
			{
				string option = ((string)entry.options.elementAt(i) ?? string.Empty).Trim();
				if (!string.IsNullOrEmpty(option)
					&& !string.Equals(option, "Không có option", System.StringComparison.OrdinalIgnoreCase))
				{
					result.addElement(option);
				}
			}
			return result;
		}

		private void prepareInfoScroll(int entryId, int x, int y, int width, int height, int contentHeight)
		{
			if (lastInfoEntryId == entryId && lastInfoContentHeight == contentHeight && lastInfoViewportHeight == height)
			{
				return;
			}
			int oldTarget = (lastInfoEntryId == entryId) ? infoScroll.cmtoY : 0;
			lastInfoEntryId = entryId;
			lastInfoContentHeight = contentHeight;
			lastInfoViewportHeight = height;
			infoScroll.clear();
			infoScroll.setStyle(System.Math.Max(1, (contentHeight + 9) / 10), 10, x, y, width, height, true, 1);
			infoScroll.moveTo(oldTarget);
		}

		private void paintInfoScrollBar(mGraphics g, int x, int y, int height)
		{
			if (infoScroll.cmyLim <= 0)
			{
				return;
			}
			g.setColor(0x8A5A2A);
			g.fillRect(x, y, 3, height, 2);
			int thumbH = System.Math.Max(12, height * height / (height + infoScroll.cmyLim));
			int thumbY = y + infoScroll.cmy * (height - thumbH) / infoScroll.cmyLim;
			g.setColor(0xE5B24E);
			g.fillRect(x - 1, thumbY, 5, thumbH, 2);
		}

		private CostumeCollectionEntry getSelected()
		{
			if (selectedIndex < 0 || selectedIndex >= entries.size())
			{
				return null;
			}
			return (CostumeCollectionEntry)entries.elementAt(selectedIndex);
		}

		private CostumeCollectionAchievement getSelectedAchievement()
		{
			if (selectedAchievementIndex < 0 || selectedAchievementIndex >= achievements.size())
			{
				return null;
			}
			return (CostumeCollectionAchievement)achievements.elementAt(selectedAchievementIndex);
		}

		public void perform(int idAction, object p)
		{
			if (idAction == ACTION_CLOSE)
			{
				closeScreen();
			}
		}

		private void closeScreen()
		{
			GameScr.isPaintOther = false;
			GameScr.gI().switchToMe();
		}

	}
}
