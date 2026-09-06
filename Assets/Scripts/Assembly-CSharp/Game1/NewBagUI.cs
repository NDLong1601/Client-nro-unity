namespace Game1
{
	public class NewBagUI : IActionListener
	{
		private static NewBagUI instance;

		public bool isShow;

		private Command cmdClose;

		public static NewBagUI GI()
		{
			NewBagUI result;
			if ((result = instance) == null)
			{
				result = (instance = new NewBagUI());
			}
			return result;
		}

		public NewBagUI()
		{
			cmdClose = new Command(string.Empty, this, 0, null)
			{
				x = 40,
				y = 40,
				img = Panel.imgX,
				cmdClosePanel = true
			};
		}

		public void Paint(mGraphics g)
		{
			if (isShow)
			{
				PopUp.paintPopUp(g, 40, 40, GameCanvas.w - 80, GameCanvas.h - 80, -1, isButton: true);
				cmdClose.paint(g);
			}
		}

		public void perform(int idAction, object p)
		{
			if (idAction == 0)
			{
				isShow = false;
			}
		}
	}
}
