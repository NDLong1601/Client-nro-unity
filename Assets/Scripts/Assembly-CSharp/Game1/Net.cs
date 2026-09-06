namespace Game1
{
	internal class Net
	{
		public static Command h;

		public static void connectHTTP2(string link, Command h)
		{
			Net.h = h;
			if (link != null)
			{
				h.perform(link);
			}
		}
	}
}
