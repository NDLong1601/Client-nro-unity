namespace Game2
{
	public interface IChatable
	{
		void onChatFromMe(string text, string to);

		void onCancelChat();
	}
}
