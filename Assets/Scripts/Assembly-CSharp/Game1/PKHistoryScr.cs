namespace Game1
{
	public class PKHistoryScr : mScreen, IActionListener
	{
		private const int ROW_HEIGHT = 34;

		private const int HEADER_Y = 70;

		private const int HEADER_HEIGHT = 26;

		private static PKHistoryScr instance;

		private MyVector entries = new MyVector();

		private readonly Scroll scroll = new Scroll();

		private bool isLoading;

		private int panelWidth;

		private int listY;

		private int listHeight;

		private int lastWidth = -1;

		private int lastHeight = -1;

		private int lastEntryCount = -1;

		public static PKHistoryScr gI()
		{
			if (instance == null)
			{
				instance = new PKHistoryScr();
			}
			return instance;
		}

		public override void switchToMe()
		{
			GameScr.isPaintOther = true;
			left = new Command(mResources.CLOSE, this, 0, null);
			center = new Command("Tải lại", this, 1, null);
			prepareLayout();
			base.switchToMe();
			requestHistory();
		}

		public void setHistory(MyVector history)
		{
			entries = ((history != null) ? history : new MyVector());
			isLoading = false;
			lastEntryCount = -1;
			prepareLayout();
		}

		private void requestHistory()
		{
			isLoading = true;
			Service.gI().requestPKHistory();
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
			panelWidth = Panel.WIDTH_PANEL;
			if (panelWidth > GameCanvas.w)
			{
				panelWidth = GameCanvas.w;
			}
			listY = HEADER_Y + HEADER_HEIGHT;
			listHeight = GameCanvas.h - listY - 31;
			if (listHeight < ROW_HEIGHT)
			{
				listHeight = ROW_HEIGHT;
			}
			scroll.setStyle(entries.size(), ROW_HEIGHT, 0, listY, panelWidth, listHeight, true, 1);
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
				requestHistory();
				return;
			}
			if (GameCanvas.isTouch)
			{
				scroll.updateKey();
			}
			if (entries.size() == 0)
			{
				return;
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21] = false;
				scroll.moveTo(scroll.cmtoY - ROW_HEIGHT);
			}
			if (GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22])
			{
				GameCanvas.keyPressed[(!Main.isPC) ? 8 : 22] = false;
				scroll.moveTo(scroll.cmtoY + ROW_HEIGHT);
			}
		}

		public override void paint(mGraphics g)
		{
			GameScr.gI().paint(g);
			g.translate(-GameScr.cmx, -GameScr.cmy);
			g.translate(0, GameCanvas.transY);
			GameScr.resetTranslate(g);
			prepareLayout();
			GameCanvas.paintz.paintFrameSimple(0, 0, panelWidth, GameCanvas.h, g);
			g.setColor(15196114);
			g.fillRect(0, HEADER_Y, panelWidth, HEADER_HEIGHT);
			mFont.tahoma_7b_dark.drawString(g, "Lịch sử thách đấu", panelWidth / 2, HEADER_Y + 8, mFont.CENTER);

			if (isLoading)
			{
				mFont.tahoma_7_grey.drawString(g, "Đang tải...", panelWidth / 2, listY + 12, mFont.CENTER);
			}
			else if (entries.size() == 0)
			{
				mFont.tahoma_7_grey.drawString(g, "Chưa có lịch sử thách đấu", panelWidth / 2, listY + 12, mFont.CENTER);
			}
			else
			{
				g.setClip(0, listY, panelWidth, listHeight);
				g.translate(0, -scroll.cmy);
				for (int i = 0; i < entries.size(); i++)
				{
					int y = listY + i * ROW_HEIGHT;
					PKHistoryEntry entry = (PKHistoryEntry)entries.elementAt(i);
					g.setColor((i % 2 == 0) ? 15196114 : 16383818);
					g.fillRect(0, y, panelWidth, ROW_HEIGHT - 1);
					mFont.tahoma_7b_dark.drawString(g, "Bạn", 8, y + 7, mFont.LEFT);
					mFont resultFont = entry.won ? mFont.tahoma_7b_green : mFont.tahoma_7b_red;
					resultFont.drawString(g, entry.won ? "WIN" : "LOSE", panelWidth / 2, y + 7, mFont.CENTER);
					mFont.tahoma_7b_dark.drawString(g, entry.opponentName, panelWidth - 8, y + 7, mFont.RIGHT);
					long elapsed = mSystem.currentTimeMillis() / 1000L - entry.completedAt;
					if (elapsed < 0L)
					{
						elapsed = 0L;
					}
					mFont.tahoma_7_grey.drawString(g, NinjaUtil.getTimeAgo(elapsed) + " " + mResources.ago, panelWidth - 8, y + 20, mFont.RIGHT);
				}
				g.translate(0, -g.getTranslateY());
				g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			}
			base.paint(g);
		}

		public void perform(int idAction, object p)
		{
			if (idAction == 0)
			{
				closeScreen();
			}
			else if (idAction == 1)
			{
				requestHistory();
			}
		}

		private void closeScreen()
		{
			isLoading = false;
			GameScr.isPaintOther = false;
			GameScr.gI().switchToMe();
		}
	}
}
