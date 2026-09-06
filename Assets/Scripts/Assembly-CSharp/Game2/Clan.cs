namespace Game2
{
	public class Clan
	{
		public const int PROFILE_V2_MAGIC = 0x434C5632;

		public const int PROFILE_V2_VERSION = 1;

		public const int PROFILE_TREASURY_VERSION = 2;

		public const int PROFILE_LEDGER_VERSION = 3;

		public const int PROFILE_PROGRESSION_VERSION = 4;

		public const int PROFILE_BUFF_VERSION = 5;

		public int ID;

		public int imgID;

		public string name = string.Empty;

		public string slogan = string.Empty;

		public int date;

		public string powerPoint;

		public int currMember;

		public int maxMember = 50;

		public int leaderID;

		public string leaderName;

		public int level;

		public int clanPoint;

		public int profileVersion;
	}
}
