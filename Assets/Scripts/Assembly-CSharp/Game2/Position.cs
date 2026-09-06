namespace Game2
{
	public class Position
	{
		public int x;

		public int y;

		public int anchor;

		public int g;

		public int v;

		public int w;

		public int h;

		public int color;

		public int limitY;

		public Layer layer;

		public short yTo;

		public short xTo;

		public short distant;

		public Position()
		{
			x = 0;
			y = 0;
		}

		public Position(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}
}
