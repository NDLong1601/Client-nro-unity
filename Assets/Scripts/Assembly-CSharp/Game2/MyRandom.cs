using System;

namespace Game2
{
	public class MyRandom
	{
		public Random r;

		public MyRandom()
		{
			r = new Random();
		}

		public int nextInt()
		{
			return r.Next();
		}

		public int nextInt(int a)
		{
			return r.Next(a);
		}
	}
}
