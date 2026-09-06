namespace Game1
{
	public class Arrowpaint
	{
		public int id;

		public int life;

		public int ax;

		public int ay;

		public int axTo;

		public int ayTo;

		public int avx;

		public int avy;

		public int adx;

		public int ady;

		public Char charBelong;

		public int[] imgId = new int[3];

		public static sbyte[] FRAME = new sbyte[25]
		{
			0, 1, 2, 1, 0, 1, 2, 1, 0, 1,
			2, 1, 0, 1, 2, 1, 0, 1, 2, 1,
			0, 1, 2, 1, 0
		};

		public static int[] ARROWINDEX = new int[18]
		{
			0, 15, 37, 52, 75, 105, 127, 142, 165, 195,
			217, 232, 255, 285, 307, 322, 345, 370
		};

		public static int[] TRANSFORM = new int[16]
		{
			0, 0, 0, 7, 6, 6, 6, 2, 2, 3,
			3, 4, 5, 5, 5, 1
		};
	}
}
