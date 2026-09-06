namespace Game1
{
	public interface IMapObject
	{
		int getX();

		int getY();

		int getW();

		int getH();

		void stopMoving();

		bool isInvisible();
	}
}
