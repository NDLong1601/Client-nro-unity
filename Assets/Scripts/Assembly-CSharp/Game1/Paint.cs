namespace Game1
{
	public class Paint
	{
		public static int COLORBACKGROUND = 15787715;

		public static int COLORLIGHT = 16383818;

		public static int COLORDARK = 3937280;

		public static int COLORBORDER = 15224576;

		public static int COLORFOCUS = 16777215;

		public static Image imgBg;

		public static Image imgLogo;

		public static Image imgLB;

		public static Image imgLT;

		public static Image imgRB;

		public static Image imgRT;

		public static Image imgChuong;

		public static Image imgSelectBoard;

		public static Image imgtoiSmall;

		public static Image imgTayTren;

		public static Image imgTayDuoi;

		public static Image[] imgTick = new Image[2];

		public static Image[] imgMsg = new Image[2];

		public static Image[] goc = new Image[6];

		public static int hTab = 24;

		public static int lenCaption = 0;

		public int[] color = new int[7] { 15970400, 13479911, 2250052, 16374659, 15906669, 12931125, 3108954 };

		public static Image imgCheck = GameCanvas.loadImage("/mainImage/myTexture2dcheck.png");

		public static void loadbg()
		{
			for (int i = 0; i < goc.Length; i++)
			{
				goc[i] = GameCanvas.loadImage("/mainImage/myTexture2dgoc" + (i + 1) + ".png");
			}
		}

		public void paintCmdBar(mGraphics g, Command left, Command center, Command right)
		{
			mFont mFont2 = ((!GameCanvas.isTouch) ? mFont.tahoma_7b_dark : mFont.tahoma_7b_dark);
			int num = 3;
			if (left != null)
			{
				lenCaption = mFont2.getWidth(left.caption);
				if (lenCaption > 0)
				{
					if (left.x >= 0 && left.y > 0)
					{
						left.paint(g);
					}
					else
					{
						g.drawImage((mScreen.keyTouch != 0) ? GameScr.imgLbtn : GameScr.imgLbtnFocus, 1, GameCanvas.h - mScreen.cmdH - 1, 0);
						mFont2.drawString(g, left.caption, 35, GameCanvas.h - mScreen.cmdH + 3 + num, 2);
					}
				}
			}
			if (center != null)
			{
				lenCaption = mFont2.getWidth(center.caption);
				if (lenCaption > 0)
				{
					if (center.x > 0 && center.y > 0)
					{
						center.paint(g);
					}
					else
					{
						g.drawImage((mScreen.keyTouch != 1) ? GameScr.imgLbtn : GameScr.imgLbtnFocus, GameCanvas.hw - 35, GameCanvas.h - mScreen.cmdH - 1, 0);
						mFont2.drawString(g, center.caption, GameCanvas.hw, GameCanvas.h - mScreen.cmdH + 3 + num, 2);
					}
				}
			}
			if (right == null)
			{
				return;
			}
			lenCaption = mFont2.getWidth(right.caption);
			if (lenCaption > 0)
			{
				if (right.x > 0 && right.y > 0)
				{
					right.paint(g);
					return;
				}
				g.drawImage((mScreen.keyTouch != 2) ? GameScr.imgLbtn : GameScr.imgLbtnFocus, GameCanvas.w - 71, GameCanvas.h - mScreen.cmdH - 1, 0);
				mFont2.drawString(g, right.caption, GameCanvas.w - 35, GameCanvas.h - mScreen.cmdH + 3 + num, 2);
			}
		}

		public void paintTabSoft(mGraphics g)
		{
		}

		public void paintPopUp(int x, int y, int w, int h, mGraphics g)
		{
			g.setColor(9340251);
			g.drawRect(x + 18, y, (w - 36) / 2 - 32, h);
			g.drawRect(x + 18 + (w - 36) / 2 + 32, y, (w - 36) / 2 - 22, h);
			g.drawRect(x, y + 8, w, h - 17);
			g.setColor(COLORBACKGROUND);
			g.fillRect(x + 18, y + 3, (w - 36) / 2 - 32, h - 4);
			g.fillRect(x + 18 + (w - 36) / 2 + 31, y + 3, (w - 38) / 2 - 22, h - 4);
			g.fillRect(x + 1, y + 6, w - 1, h - 11);
			g.setColor(14667919);
			g.fillRect(x + 18, y + 1, (w - 36) / 2 - 32, 2);
			g.fillRect(x + 18 + (w - 36) / 2 + 32, y + 1, (w - 36) / 2 - 12, 2);
			g.fillRect(x + 18, y + h - 2, (w - 36) / 2 - 31, 2);
			g.fillRect(x + 18 + (w - 36) / 2 + 32, y + h - 2, (w - 36) / 2 - 31, 2);
			g.fillRect(x + 1, y + 11, 2, h - 18);
			g.fillRect(x + w - 2, y + 11, 2, h - 18);
			g.drawImage(goc[0], x - 3, y - 2, mGraphics.TOP | mGraphics.LEFT);
			g.drawImage(goc[2], x + w + 3, y - 2, StaticObj.TOP_RIGHT);
			g.drawImage(goc[1], x - 3, y + h + 3, StaticObj.BOTTOM_LEFT);
			g.drawImage(goc[3], x + w + 4, y + h + 2, StaticObj.BOTTOM_RIGHT);
			g.drawImage(goc[4], x + w / 2, y, StaticObj.TOP_CENTER);
			g.drawImage(goc[5], x + w / 2, y + h + 1, StaticObj.BOTTOM_HCENTER);
		}

		public void paintFrameSimple(int x, int y, int w, int h, mGraphics g)
		{
			g.setColor(6702080);
			g.fillRect(x, y, w, h);
			g.setColor(14338484);
			g.fillRect(x + 1, y + 1, w - 2, h - 2);
		}
	}
}
