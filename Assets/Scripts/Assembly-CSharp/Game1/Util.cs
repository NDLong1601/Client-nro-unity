namespace Game1
{
	public static class Util
	{
		public static bool CanDoWithTime(long lastTime, long waitTime)
		{
			return mSystem.currentTimeMillis() - lastTime > waitTime;
		}
	}
}
