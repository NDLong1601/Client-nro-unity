namespace Game1
{
	public interface IChatable
	{
		void onChatFromMe(string text, string to);

		void onCancelChat();
	}
}
