namespace Game2
{
	public class MotherCanvas
	{
		public static MotherCanvas instance;

		public GameCanvas tCanvas;

		public int zoomLevel = 1;

		private int OUTPUTSIZE = 20;

		public MotherCanvas()
		{
			checkZoomLevel(getWidth(), getHeight());
		}

		public void checkZoomLevel(int w, int h)
		{
			if (Main.isWindowsPhone)
			{
				mGraphics.zoomLevel = 2;
				if (w * h >= 2073600)
				{
					mGraphics.zoomLevel = 4;
				}
				else if (w * h > 384000)
				{
					mGraphics.zoomLevel = 3;
				}
			}
			else if (!Main.isPC || Main.isIPhone)
			{
				if (Main.isIpod)
				{
					mGraphics.zoomLevel = 2;
				}
				else if (w * h >= 2073600)
				{
					mGraphics.zoomLevel = 4;
				}
				else if (w * h >= 691200)
				{
					mGraphics.zoomLevel = 3;
				}
				else if (w * h > 153600)
				{
					mGraphics.zoomLevel = 2;
				}
			}
			else
			{
				mGraphics.zoomLevel = 2;
				if (w * h < 480000)
				{
					mGraphics.zoomLevel = 1;
				}
			}
		}

		public int getWidth()
		{
			return (int)ScaleGUI.WIDTH;
		}

		public int getHeight()
		{
			return (int)ScaleGUI.HEIGHT;
		}

		public void setChildCanvas(GameCanvas tCanvas)
		{
			this.tCanvas = tCanvas;
		}

		public int getWidthz()
		{
			int width = getWidth();
			return width / mGraphics.zoomLevel + width % mGraphics.zoomLevel;
		}

		public int getHeightz()
		{
			int height = getHeight();
			return height / mGraphics.zoomLevel + height % mGraphics.zoomLevel;
		}
	}
}
