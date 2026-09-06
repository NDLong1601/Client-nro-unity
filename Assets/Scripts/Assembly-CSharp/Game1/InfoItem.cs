namespace Game1
{
	public class InfoItem
	{
		public string s;

		private mFont f;

		public int speed = 70;

		public Char charInfo;

		public bool isChatServer;

		public bool isOnline;

		public int timeCount;

		public int maxTime;

		public long last;

		public long curr;

		public InfoItem(string s)
		{
			f = mFont.tahoma_7_green2;
			this.s = s;
			speed = 20;
		}
	}
}
