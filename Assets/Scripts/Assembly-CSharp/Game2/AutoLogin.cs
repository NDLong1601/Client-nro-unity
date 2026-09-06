using System.Collections.Generic;

namespace Game2
{
	public class AutoLogin
	{
		public bool waitToNextLogin;

		public long lastTimeWait;

		public bool hasSetUserPass;

		public string accAutoLogin = "";

		public AutoLogin()
		{
			lastTimeWait = mSystem.currentTimeMillis();
		}

		public Account GetAccWithUsername(List<Account> accounts)
		{
			foreach (Account account in accounts)
			{
				if (account.getUsername().Equals(accAutoLogin))
				{
					return account;
				}
			}
			return new Account("", "");
		}
	}
}
