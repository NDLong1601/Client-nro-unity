using System;
using System.Collections.Generic;

namespace Game1
{
	/// <summary>
	/// Client-only presentation for the versioned Activity Points snapshot (-58).
	/// All progression and reward decisions remain on the server.
	/// </summary>
	public class ActivityScreen : mScreen, IActionListener
	{
		private const sbyte COMMAND = -58;
		private const int PROTOCOL_VERSION = 1;
		private const int ACTION_CLOSE = 1;
		private const int ACTION_CENTER = 2;
		private const int ACTION_NEXT_TAB = 3;
		private const int TAB_DAILY = 0;
		private const int TAB_WEEKLY = 1;
		private const int TAB_SOURCES = 2;
		private const int ROW_HEIGHT = 40;
		private const int MAX_TIER_COUNT = 16;
		private const int MAX_SOURCE_COUNT = 32;
		private const int COLOR_TITLE = 6826798;
		private const int COLOR_HEADER = 7487774;
		private const int COLOR_HEADER_ACCENT = 10576230;
		private const int COLOR_TAB_IDLE = 9338734;
		private const int COLOR_TAB_ACTIVE = 16111067;
		private const int COLOR_ROW_EVEN = 15723742;
		private const int COLOR_ROW_ODD = 15196114;
		private const int COLOR_ROW_SELECTED = 16771006;
		private const int COLOR_ROW_SELECTED_BORDER = 10763812;
		private const int COLOR_BAR_EMPTY = 5988180;
		private const int COLOR_BADGE_LOCKED = 11826738;
		private const int COLOR_BADGE_AVAILABLE = 2789452;
		private const int COLOR_BADGE_CLAIMED = 6974058;
		private const int COLOR_BADGE_DISABLED = 11427608;

		private static readonly string[] SourceNames = new string[14]
		{
			"Đăng nhập", "Điểm danh", "Thu hoạch đậu", "NV phụ dễ",
			"NV phụ thường", "NV phụ khó", "NV phụ rất khó", "NV phụ đặc biệt",
			"Điểm danh bang", "Câu cá", "Thắng PvP", "Phó bản", "Hạ boss", "Đa dạng"
		};

		private static ActivityScreen instance;
		private readonly Scroll scroll = new Scroll();
		private readonly List<ActivityTierEntry> dailyTiers = new List<ActivityTierEntry>();
		private readonly List<ActivityTierEntry> weeklyTiers = new List<ActivityTierEntry>();
		private readonly List<ActivitySourceEntry> sources = new List<ActivitySourceEntry>();

		private bool loading;
		private bool legacyPointsReceived;
		private int dailyPoints;
		private int dailyMax = 100;
		private int weeklyPoints;
		private int qualifiedDays;
		private int revision;
		private int featureFlags;
		private long dailyResetAt;
		private long weeklyResetAt;
		private int activeTab;
		private int selectedIndex;
		private int panelX;
		private int panelY;
		private int panelW;
		private int panelH;
		private int listY;
		private int listH;
		private int lastWidth = -1;
		private int lastHeight = -1;
		private int lastRowCount = -1;

		public static ActivityScreen gI()
		{
			if (instance == null)
			{
				instance = new ActivityScreen();
			}
			return instance;
		}

		public void requestOpen()
		{
			selectedIndex = 0;
			switchToMe();
			requestPanelData();
		}

		public void requestPanelData()
		{
			loading = true;
			Service.gI().requestActivityDashboard();
		}

		public bool isPanelLoading()
		{
			return loading;
		}

		public int getPanelTierCount(bool weekly)
		{
			return weekly ? weeklyTiers.Count : dailyTiers.Count;
		}

		public int getPanelSourceCount()
		{
			return sources.Count;
		}

		public string getPanelHeaderTitle()
		{
			return "NĂNG ĐỘNG";
		}

		public string getPanelDailySummary()
		{
			return "Hôm nay: " + dailyPoints + "/" + dailyMax;
		}

		public string getPanelWeeklySummary()
		{
			return "Tuần: " + weeklyPoints + " | Chuẩn: " + qualifiedDays;
		}

		public string getPanelRuntimeSummary()
		{
			if ((featureFlags & 1) == 0) return "Tạm khóa";
			if ((featureFlags & 2) != 0) return "Shadow - không phát quà";
			return "Đang hoạt động";
		}

		public string getPanelTierTitle(bool weekly, int index)
		{
			ActivityTierEntry tier = getPanelTier(weekly, index);
			return tier == null ? "" : "Mốc " + tier.threshold + " điểm";
		}

		public string getPanelTierStatus(bool weekly, int index)
		{
			ActivityTierEntry tier = getPanelTier(weekly, index);
			return tier == null ? "" : statusText(tier, weekly);
		}

		public string getPanelTierDetail(bool weekly, int index)
		{
			ActivityTierEntry tier = getPanelTier(weekly, index);
			if (tier == null) return "";
			return weekly && tier.minQualifiedDays > 0
				? "Cần " + tier.minQualifiedDays + " ngày chuẩn" : "Đạt mốc để mở quà";
		}

		public int getPanelTierState(bool weekly, int index)
		{
			ActivityTierEntry tier = getPanelTier(weekly, index);
			return tier == null ? 0 : tier.state;
		}

		public int getPanelTierProgress(bool weekly)
		{
			return weekly ? weeklyPoints : dailyPoints;
		}

		public int getPanelTierTarget(bool weekly, int index)
		{
			ActivityTierEntry tier = getPanelTier(weekly, index);
			return tier == null ? 1 : System.Math.Max(1, tier.threshold);
		}

		public void claimPanelTier(bool weekly, int index)
		{
			ActivityTierEntry tier = getPanelTier(weekly, index);
			if (tier == null || tier.state != 1) return;
			loading = true;
			Service.gI().claimActivityReward(weekly, tier.id);
		}

		public string getPanelSourceTitle(int index)
		{
			ActivitySourceEntry source = getPanelSource(index);
			if (source == null) return "";
			return source.typeIndex >= 0 && source.typeIndex < SourceNames.Length
				? SourceNames[source.typeIndex] : "Nguồn " + source.typeIndex;
		}

		public string getPanelSourceStatus(int index)
		{
			ActivitySourceEntry source = getPanelSource(index);
			if (source == null) return "";
			return source.enabled ? source.current + "/" + source.cap : "Tạm tắt";
		}

		public string getPanelSourceDetail(int index)
		{
			ActivitySourceEntry source = getPanelSource(index);
			return source != null && source.enabled ? "Tiến độ hôm nay" : "Nguồn đang tắt";
		}

		public int getPanelSourceProgress(int index)
		{
			ActivitySourceEntry source = getPanelSource(index);
			return source == null ? 0 : source.current;
		}

		public int getPanelSourceCap(int index)
		{
			ActivitySourceEntry source = getPanelSource(index);
			return source == null ? 1 : System.Math.Max(1, source.cap);
		}

		public bool isPanelSourceEnabled(int index)
		{
			ActivitySourceEntry source = getPanelSource(index);
			return source != null && source.enabled;
		}

		public override void switchToMe()
		{
			GameScr.isPaintOther = true;
			left = new Command(mResources.CLOSE, this, ACTION_CLOSE, null);
			center = new Command("Tải lại", this, ACTION_CENTER, null);
			right = new Command("Đổi", this, ACTION_NEXT_TAB, null);
			prepareLayout();
			base.switchToMe();
		}

		public void receive(Message message)
		{
			try
			{
				if (message == null || message.reader() == null || message.reader().readUnsignedByte() != 0)
				{
					return;
				}
				if (message.reader().readUnsignedByte() != PROTOCOL_VERSION)
				{
					return;
				}
				revision = System.Math.Max(0, message.reader().readInt());
				featureFlags = message.reader().readUnsignedByte();
				dailyPoints = System.Math.Max(0, message.reader().readInt());
				dailyMax = System.Math.Max(1, message.reader().readInt());
				weeklyPoints = System.Math.Max(0, message.reader().readInt());
				qualifiedDays = message.reader().readUnsignedByte();
				dailyResetAt = message.reader().readLong();
				weeklyResetAt = message.reader().readLong();
				readTiers(message, dailyTiers);
				readTiers(message, weeklyTiers);
				readSources(message);
				Char.myCharz().cNangdong = dailyPoints;
				legacyPointsReceived = true;
				loading = false;
				clampSelection();
				lastRowCount = -1;
				prepareLayout();
				notifyPanelUpdated();
			}
			catch (Exception)
			{
				loading = false;
			}
		}

		public void receiveLegacyDailyPoints(int points)
		{
			points = System.Math.Max(0, points);
			int previous = dailyPoints;
			bool shouldShowGain = legacyPointsReceived && points > previous;
			dailyPoints = points;
			Char.myCharz().cNangdong = points;
			legacyPointsReceived = true;
			if (shouldShowGain && GameScr.info1 != null)
			{
				GameScr.info1.addInfo("+" + (points - previous) + " Năng động", 0);
			}
			notifyPanelUpdated();
		}

		private void notifyPanelUpdated()
		{
			if (GameCanvas.panel != null)
			{
				GameCanvas.panel.onActivityDashboardUpdated();
			}
		}

		private void readTiers(Message message, List<ActivityTierEntry> destination)
		{
			destination.Clear();
			int count = message.reader().readUnsignedByte();
			if (count > MAX_TIER_COUNT)
			{
				throw new Exception("Activity tier count is too large");
			}
			for (int index = 0; index < count; index++)
			{
				ActivityTierEntry tier = new ActivityTierEntry();
				tier.id = message.reader().readUTF();
				tier.threshold = message.reader().readUnsignedShort();
				tier.minQualifiedDays = message.reader().readUnsignedByte();
				tier.state = message.reader().readUnsignedByte();
				destination.Add(tier);
			}
		}

		private void readSources(Message message)
		{
			sources.Clear();
			int count = message.reader().readUnsignedByte();
			if (count > MAX_SOURCE_COUNT)
			{
				throw new Exception("Activity source count is too large");
			}
			for (int index = 0; index < count; index++)
			{
				ActivitySourceEntry source = new ActivitySourceEntry();
				source.typeIndex = message.reader().readUnsignedByte();
				source.current = message.reader().readUnsignedShort();
				source.cap = message.reader().readUnsignedShort();
				source.enabled = message.reader().readUnsignedByte() != 0;
				source.category = message.reader().readUnsignedByte();
				sources.Add(source);
			}
		}

		public override void update()
		{
			prepareLayout();
			scroll.updatecm();
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
			if (GameCanvas.keyPressed[(!Main.isPC) ? 5 : 25] || mScreen.getCmdPointerLast(center))
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 5 : 25] = false;
				GameCanvas.isPointerJustRelease = false;
				performCenter();
				return;
			}
			if (GameCanvas.keyPressed[13] || mScreen.getCmdPointerLast(right))
			{
				GameCanvas.keyPressed[13] = false;
				GameCanvas.isPointerJustRelease = false;
				nextTab();
				return;
			}
			if (GameCanvas.isTouch)
			{
				ScrollResult result = scroll.updateKey();
				if (result.isFinish && result.selected >= 0 && result.selected < rowCount())
				{
					selectedIndex = result.selected;
				}
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21] = false;
				moveSelection(-1);
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22] = false;
				moveSelection(1);
			}
		}

		public override void paint(mGraphics g)
		{
			GameScr.gI().paint(g);
			g.translate(-GameScr.cmx, -GameScr.cmy);
			g.translate(0, GameCanvas.transY);
			GameScr.resetTranslate(g);
			prepareLayout();

			GameCanvas.paintz.paintFrameSimple(panelX, panelY, panelW, panelH, g);
			g.setColor(COLOR_TITLE);
			g.fillRect(panelX + 2, panelY + 2, panelW - 4, 23);
			mFont.tahoma_7b_white.drawString(g, "NĂNG ĐỘNG", panelX + 10, panelY + 7, mFont.LEFT);
			paintModeBadge(g);
			paintHeader(g);
			paintTabs(g);
			paintRows(g);
			base.paint(g);
		}

		private void paintHeader(mGraphics g)
		{
			int y = panelY + 28;
			g.setColor(COLOR_HEADER);
			g.fillRect(panelX + 3, y, panelW - 6, 64);
			mFont.tahoma_7b_white.drawString(g, "Hôm nay: " + dailyPoints + "/" + dailyMax,
				panelX + 9, y + 5, mFont.LEFT);
			paintBar(g, panelX + 9, y + 18, panelW - 18, 7, dailyPoints, dailyMax, 35873);
			mFont.tahoma_7_yellow.drawString(g, "Tuần: " + weeklyPoints + "  |  Chuẩn: " + qualifiedDays,
				panelX + 9, y + 30, mFont.LEFT);
			mFont.tahoma_7_white.drawString(g, "Reset ngày: " + formatCountdown(dailyResetAt), panelX + 9, y + 43, mFont.LEFT);
			mFont.tahoma_7_white.drawString(g, "Reset tuần: " + formatCountdown(weeklyResetAt), panelX + 9, y + 55, mFont.LEFT);
		}

		private void paintModeBadge(mGraphics g)
		{
			bool enabled = (featureFlags & 1) != 0;
			bool shadow = (featureFlags & 2) != 0;
			string label = !enabled ? "TẠM KHÓA" : (shadow ? "SHADOW" : "ĐANG CHẠY");
			int color = !enabled ? COLOR_BADGE_DISABLED : (shadow ? COLOR_HEADER_ACCENT : COLOR_BADGE_AVAILABLE);
			int width = System.Math.Min(78, System.Math.Max(44, label.Length * 7 + 10));
			int x = panelX + panelW - width - 8;
			g.setColor(color);
			g.fillRect(x, panelY + 6, width, 13);
			mFont.tahoma_7b_white.drawString(g, label, x + width / 2, panelY + 8, mFont.CENTER);
		}

		private void paintTabs(mGraphics g)
		{
			int tabY = panelY + 96;
			int tabW = panelW / 3;
			string[] labels = new string[3] { "Ngày", "Tuần", "Nguồn" };
			for (int index = 0; index < labels.Length; index++)
			{
				int x = panelX + index * tabW + 3;
				int width = (index == labels.Length - 1) ? panelX + panelW - x - 3 : tabW - 4;
				g.setColor(index == activeTab ? COLOR_TAB_ACTIVE : COLOR_TAB_IDLE);
				g.fillRect(x, tabY, width, 21);
				mFont font = index == activeTab ? mFont.tahoma_7b_dark : mFont.tahoma_7b_white;
				font.drawString(g, labels[index], x + width / 2, tabY + 6, mFont.CENTER);
			}
		}

		private void paintRows(mGraphics g)
		{
			if (loading)
			{
				mFont.tahoma_7_grey.drawString(g, "Đang tải tiến độ Năng động...", panelX + panelW / 2,
					listY + 16, mFont.CENTER);
				return;
			}
			if (rowCount() == 0)
			{
				mFont.tahoma_7_grey.drawString(g, "Chưa có dữ liệu để hiển thị", panelX + panelW / 2,
					listY + 16, mFont.CENTER);
				return;
			}
			g.setClip(panelX + 2, listY, panelW - 4, listH);
			g.translate(0, -scroll.cmy);
			for (int index = 0; index < rowCount(); index++)
			{
				int y = listY + index * ROW_HEIGHT;
				int rowColor = (index % 2 == 0) ? COLOR_ROW_EVEN : COLOR_ROW_ODD;
				if (index == selectedIndex)
				{
					g.setColor(COLOR_ROW_SELECTED_BORDER);
					g.fillRect(panelX + 3, y, panelW - 6, ROW_HEIGHT - 1);
					g.setColor(COLOR_ROW_SELECTED);
					g.fillRect(panelX + 5, y + 2, panelW - 10, ROW_HEIGHT - 5);
				}
				else
				{
					g.setColor(rowColor);
					g.fillRect(panelX + 3, y, panelW - 6, ROW_HEIGHT - 1);
				}
				if (activeTab == TAB_SOURCES)
				{
					paintSourceRow(g, sources[index], y);
				}
				else
				{
					paintTierRow(g, currentTiers()[index], y);
				}
			}
			g.translate(0, -g.getTranslateY());
			g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
		}

		private void paintTierRow(mGraphics g, ActivityTierEntry tier, int y)
		{
			mFont.tahoma_7b_dark.drawString(g, "Mốc " + tier.threshold + " điểm", panelX + 10, y + 5, mFont.LEFT);
			paintStatusBadge(g, statusText(tier), tier.state, y);
			string detail = activeTab == TAB_WEEKLY && tier.minQualifiedDays > 0
				? "Cần " + tier.minQualifiedDays + " ngày đạt chuẩn" : "Đạt mốc để mở quà";
			mFont.tahoma_7b_dark.drawString(g, detail, panelX + 10, y + 18, mFont.LEFT);
			int points = activeTab == TAB_WEEKLY ? weeklyPoints : dailyPoints;
			paintBar(g, panelX + 10, y + 30, panelW - 20, 6, points, tier.threshold,
				tier.state == 1 ? COLOR_BADGE_AVAILABLE : COLOR_HEADER_ACCENT);
		}

		private void paintSourceRow(mGraphics g, ActivitySourceEntry source, int y)
		{
			string name = source.typeIndex >= 0 && source.typeIndex < SourceNames.Length
				? SourceNames[source.typeIndex] : "Nguồn " + source.typeIndex;
			mFont.tahoma_7b_dark.drawString(g, name, panelX + 10, y + 5, mFont.LEFT);
			paintStatusBadge(g, source.enabled ? source.current + "/" + source.cap : "Tạm tắt",
				source.enabled ? 1 : 3, y);
			mFont.tahoma_7b_dark.drawString(g, source.enabled ? "Tiến độ hôm nay" : "Nguồn này đang tắt",
				panelX + 10, y + 18, mFont.LEFT);
			paintBar(g, panelX + 10, y + 30, panelW - 20, 6, source.current, System.Math.Max(1, source.cap),
				source.enabled ? 35873 : COLOR_BADGE_DISABLED);
		}

		private void paintStatusBadge(mGraphics g, string text, int state, int y)
		{
			int color = state == 1 ? COLOR_BADGE_AVAILABLE : (state == 2 ? COLOR_BADGE_CLAIMED :
				(state == 3 ? COLOR_BADGE_DISABLED : COLOR_BADGE_LOCKED));
			int width = System.Math.Min(panelW / 2, System.Math.Max(38, text.Length * 7 + 10));
			int x = panelX + panelW - width - 9;
			g.setColor(color);
			g.fillRect(x, y + 4, width, 13);
			mFont.tahoma_7b_white.drawString(g, text, x + width / 2, y + 6, mFont.CENTER);
		}

		private void paintBar(mGraphics g, int x, int y, int width, int height, int value, int max, int color)
		{
			g.setColor(COLOR_BAR_EMPTY);
			g.fillRect(x, y, width, height);
			int fill = System.Math.Max(0, System.Math.Min(width, (int)((long)System.Math.Max(0, value) * width / System.Math.Max(1, max))));
			if (fill > 0)
			{
				g.setColor(color);
				g.fillRect(x, y, fill, height);
			}
		}

		private void prepareLayout()
		{
			int rows = rowCount();
			if (lastWidth == GameCanvas.w && lastHeight == GameCanvas.h && lastRowCount == rows)
			{
				updateCommands();
				return;
			}
			lastWidth = GameCanvas.w;
			lastHeight = GameCanvas.h;
			lastRowCount = rows;
			panelW = System.Math.Min(320, GameCanvas.w);
			panelW = System.Math.Max(170, panelW);
			panelX = (GameCanvas.w - panelW) / 2;
			panelY = 0;
			panelH = System.Math.Max(145, GameCanvas.h - mScreen.cmdH - 3);
			listY = panelY + 122;
			listH = System.Math.Max(ROW_HEIGHT, panelH - 124);
			int previousTarget = scroll.cmtoY;
			scroll.clear();
			scroll.setStyle(rows, ROW_HEIGHT, panelX + 2, listY, panelW - 4, listH, true, 1);
			scroll.moveTo(previousTarget);
			updateCommands();
		}

		private void updateCommands()
		{
			if (center != null)
			{
				center.caption = selectedTierCanClaim() ? "Nhận" : "Tải lại";
			}
		}

		private void performCenter()
		{
			ActivityTierEntry tier = selectedTier();
			if (tier != null && tier.state == 1)
			{
				loading = true;
				Service.gI().claimActivityReward(activeTab == TAB_WEEKLY, tier.id);
				return;
			}
			loading = true;
			Service.gI().requestActivityDashboard();
		}

		private void nextTab()
		{
			activeTab = (activeTab + 1) % 3;
			selectedIndex = 0;
			lastRowCount = -1;
			prepareLayout();
		}

		private void moveSelection(int delta)
		{
			int count = rowCount();
			if (count <= 0)
			{
				return;
			}
			selectedIndex = System.Math.Max(0, System.Math.Min(count - 1, selectedIndex + delta));
			scroll.moveTo(selectedIndex * ROW_HEIGHT - listH / 2);
			updateCommands();
		}

		private void clampSelection()
		{
			selectedIndex = System.Math.Max(0, System.Math.Min(System.Math.Max(0, rowCount() - 1), selectedIndex));
		}

		private int rowCount()
		{
			return activeTab == TAB_DAILY ? dailyTiers.Count : (activeTab == TAB_WEEKLY ? weeklyTiers.Count : sources.Count);
		}

		private List<ActivityTierEntry> currentTiers()
		{
			return activeTab == TAB_WEEKLY ? weeklyTiers : dailyTiers;
		}

		private ActivityTierEntry selectedTier()
		{
			if (activeTab == TAB_SOURCES || selectedIndex < 0 || selectedIndex >= currentTiers().Count)
			{
				return null;
			}
			return currentTiers()[selectedIndex];
		}

		private ActivityTierEntry getPanelTier(bool weekly, int index)
		{
			List<ActivityTierEntry> tiers = weekly ? weeklyTiers : dailyTiers;
			return index >= 0 && index < tiers.Count ? tiers[index] : null;
		}

		private ActivitySourceEntry getPanelSource(int index)
		{
			return index >= 0 && index < sources.Count ? sources[index] : null;
		}

		private bool selectedTierCanClaim()
		{
			ActivityTierEntry tier = selectedTier();
			return tier != null && tier.state == 1;
		}

		private string statusText(ActivityTierEntry tier)
		{
			return statusText(tier, activeTab == TAB_WEEKLY);
		}

		private string statusText(ActivityTierEntry tier, bool weekly)
		{
			if (tier.state == 1) return "Có thể nhận";
			if (tier.state == 2) return "Đã nhận";
			if (tier.state == 3) return "Tạm đóng";
			int points = weekly ? weeklyPoints : dailyPoints;
			int missing = System.Math.Max(0, tier.threshold - points);
			return missing > 0 ? "Thiếu " + missing : "Chưa đủ điều kiện";
		}

		private string formatCountdown(long epochSeconds)
		{
			long remaining = epochSeconds - mSystem.currentTimeMillis() / 1000L;
			if (remaining <= 0L) return "đang làm mới";
			long days = remaining / 86400L;
			long hours = remaining % 86400L / 3600L;
			long minutes = remaining % 3600L / 60L;
			if (days > 0L) return days + "ng " + hours + "h";
			return hours + "h " + minutes + "p";
		}

		public void perform(int idAction, object p)
		{
			if (idAction == ACTION_CLOSE)
			{
				closeScreen();
			}
			else if (idAction == ACTION_CENTER)
			{
				performCenter();
			}
			else if (idAction == ACTION_NEXT_TAB)
			{
				nextTab();
			}
		}

		private void closeScreen()
		{
			loading = false;
			GameScr.isPaintOther = false;
			GameScr.gI().switchToMe();
		}

		private sealed class ActivityTierEntry
		{
			public string id;
			public int threshold;
			public int minQualifiedDays;
			public int state;
		}

		private sealed class ActivitySourceEntry
		{
			public int typeIndex;
			public int current;
			public int cap;
			public bool enabled;
			public int category;
		}
	}
}
