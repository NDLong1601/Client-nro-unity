using System;
using UnityEngine;

namespace Game1
{
	public class AdminPopup : IActionListener
	{
		private static AdminPopup instance;

		private int size;

		private int iconID;

		private bool isShow;

		private Scroll scr = new Scroll();

		private int x;

		private int y;

		private int w;

		private int h;

		public static AdminPopup gI()
		{
			if (instance == null)
			{
				instance = new AdminPopup();
			}
			return instance;
		}

		public AdminPopup()
		{
			w = 320;
			h = GameCanvas.h / 2;
			x = GameCanvas.w / 2 - w / 2;
			y = GameCanvas.h / 2 - h / 2 - 20;
		}

		public void paint(mGraphics g)
		{
			if (!isShow)
			{
				return;
			}
			try
			{
				PopUp.paintPopUp(g, x, y, w, h, 16777215, isButton: false);
				g.translate(0, -scr.cmy);
				for (int i = 0; i < size; i++)
				{
					SmallImage.drawSmallImage(g, iconID, x + 5, y + 20 * i, 0, 0);
				}
				g.translate(0, scr.cmy);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public void perform(int idAction, object p)
		{
			throw new NotImplementedException();
		}
	}
}
