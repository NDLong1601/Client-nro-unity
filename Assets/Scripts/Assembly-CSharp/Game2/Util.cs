namespace Game2
{
	public static class Util
	{
		public static bool CanDoWithTime(long lastTime, long waitTime)
		{
			return mSystem.currentTimeMillis() - lastTime > waitTime;
		}
	}
}
