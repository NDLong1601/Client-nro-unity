using System;

namespace Game2
{
	public class Timer
	{
		public static int idAction;

		public static long timeExecute;

		public static bool isON;

		public static void update()
		{
			long num = mSystem.currentTimeMillis();
			if (!isON || num <= timeExecute)
			{
				return;
			}
			isON = false;
			try
			{
				if (idAction > 0)
				{
					GameScr.gI().actionPerform(idAction, null);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
