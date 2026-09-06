namespace Game1
{
	/// <summary>Read-only client snapshot for server-authoritative clan progression.</summary>
	public class ClanProgression
	{
		public const sbyte REQUEST_VIEW = 112;
		public const sbyte REQUEST_UPGRADE = 113;
		public const sbyte REQUEST_ALLOCATE = 114;
		public const sbyte RESPONSE_SNAPSHOT = 115;
		public const sbyte RESPONSE_BUFF_SNAPSHOT = 124;
		public const sbyte REQUEST_BUFF_SNAPSHOT = 125;
		public const int BRANCH_COUNT = 6;
		public const int BUFF_COUNT = 6;

		public static ClanProgression current = new ClanProgression();
		public int clanId = -1;
		public int level = 1;
		public long exp;
		public long expRequired;
		public int capsuleRequired;
		public long goldRequired;
		public long gemRequired;
		public int totalPoints;
		public int unspentPoints;
		public long version;
		public int[] ranks = new int[BRANCH_COUNT];
		public int[] buffPercents = new int[BUFF_COUNT];
		public long[] buffExpiresAt = new long[BUFF_COUNT];
		public bool loaded;
		private static long lastSnapshotRequestAt;
		private static long lastBuffSnapshotRequestAt;

		public static bool receive(Message message, sbyte action)
		{
			if (action == RESPONSE_BUFF_SNAPSHOT)
			{
				receiveBuffSnapshot(message);
				return true;
			}
			if (action != RESPONSE_SNAPSHOT)
			{
				return false;
			}
			try
			{
				int receivedClanId = message.reader().readInt();
				int receivedLevel = message.reader().readInt();
				long receivedExp = message.reader().readLong();
				long receivedExpRequired = message.reader().readLong();
				int receivedCapsuleRequired = message.reader().readInt();
				long receivedGoldRequired = message.reader().readLong();
				long receivedGemRequired = message.reader().readLong();
				int receivedTotalPoints = message.reader().readInt();
				int receivedUnspentPoints = message.reader().readInt();
				long receivedVersion = message.reader().readLong();
				int[] receivedRanks = new int[BRANCH_COUNT];
				for (int i = 0; i < BRANCH_COUNT; i++)
				{
					receivedRanks[i] = message.reader().readUnsignedByte();
				}
				int receivedHpMax = -1;
				int receivedMpMax = -1;
				int receivedDamage = -1;
				int receivedHp = -1;
				int receivedMp = -1;
				int[] receivedBuffPercents = new int[BUFF_COUNT];
				long[] receivedBuffExpiresAt = new long[BUFF_COUNT];
				bool receivedBuffSection = false;
				if (message.reader().available() > 0)
				{
					int detailVersion = message.reader().readUnsignedByte();
					if (detailVersion >= 1 && message.reader().available() >= 20)
					{
						receivedHpMax = message.reader().readInt();
						receivedMpMax = message.reader().readInt();
						receivedDamage = message.reader().readInt();
						receivedHp = message.reader().readInt();
						receivedMp = message.reader().readInt();
					}
					if (detailVersion >= 2 && message.reader().available() > 0)
					{
						receivedBuffSection = true;
						int buffCount = message.reader().readUnsignedByte();
						long receivedAt = mSystem.currentTimeMillis();
						for (int i = 0; i < buffCount; i++)
						{
							int type = message.reader().readUnsignedByte();
							int percent = message.reader().readShort();
							long encodedTime = message.reader().readLong();
							if (type >= 0 && type < BUFF_COUNT)
							{
								receivedBuffPercents[type] = percent;
								receivedBuffExpiresAt[type] = detailVersion >= 3
									? (encodedTime > 0L ? receivedAt + encodedTime : 0L)
									: encodedTime;
							}
						}
					}
				}
				applySnapshot(receivedClanId, receivedLevel, receivedExp, receivedExpRequired,
					receivedCapsuleRequired, receivedGoldRequired, receivedGemRequired,
					receivedTotalPoints, receivedUnspentPoints, receivedVersion, receivedRanks);
				applyLiveStats(receivedClanId, receivedHpMax, receivedMpMax,
					receivedDamage, receivedHp, receivedMp);
				if (receivedBuffSection)
				{
					applyBuffSnapshot(receivedClanId, receivedBuffPercents, receivedBuffExpiresAt);
				}
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanProgression.receive: " + ex.Message);
			}
			return true;
		}

		private static void receiveBuffSnapshot(Message message)
		{
			try
			{
				int receivedClanId = message.reader().readInt();
				int protocolVersion = message.reader().readUnsignedByte();
				int count = message.reader().readUnsignedByte();
				int[] percents = new int[BUFF_COUNT];
				long[] expiresAt = new long[BUFF_COUNT];
				long receivedAt = mSystem.currentTimeMillis();
				for (int i = 0; i < count; i++)
				{
					int type = message.reader().readUnsignedByte();
					int percent = message.reader().readShort();
					long remainingMillis = message.reader().readLong();
					if (type >= 0 && type < BUFF_COUNT)
					{
						percents[type] = percent;
						expiresAt[type] = protocolVersion >= 1 && remainingMillis > 0L
							? receivedAt + remainingMillis : 0L;
					}
				}
				applyBuffSnapshot(receivedClanId, percents, expiresAt);
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanProgression.receiveBuffSnapshot: " + ex.Message);
			}
		}

		private static void applyBuffSnapshot(int receivedClanId, int[] percents, long[] expiresAt)
		{
			// This response is unicast by the server to the requesting clan member.
			// Clan.ID can briefly lag behind the custom snapshot during login/profile refresh,
			// so rejecting a valid response here leaves every buff permanently inactive.
			if (Char.myCharz().clan == null)
			{
				return;
			}
			current.clanId = receivedClanId;
			for (int i = 0; i < BUFF_COUNT; i++)
			{
				current.buffPercents[i] = percents != null && i < percents.Length ? percents[i] : 0;
				current.buffExpiresAt[i] = expiresAt != null && i < expiresAt.Length ? expiresAt[i] : 0L;
			}
		}

		public static void applyProfileBuffSnapshot(int receivedClanId, int[] percents, long[] remainingMillis)
		{
			long receivedAt = mSystem.currentTimeMillis();
			long[] expiresAt = new long[BUFF_COUNT];
			for (int i = 0; i < BUFF_COUNT; i++)
			{
				long remaining = remainingMillis != null && i < remainingMillis.Length
					? remainingMillis[i] : 0L;
				expiresAt[i] = remaining > 0L ? receivedAt + remaining : 0L;
			}
			applyBuffSnapshot(receivedClanId, percents, expiresAt);
		}

		public static int activeBuffCount()
		{
			int count = 0;
			long now = mSystem.currentTimeMillis();
			for (int i = 0; i < BUFF_COUNT; i++)
			{
				if (current.buffExpiresAt[i] > now)
				{
					count++;
				}
			}
			return count;
		}

		public static string activeBuffText(int type)
		{
			if (!isBuffActive(type))
			{
				return string.Empty;
			}
			return buffEffectText(type) + ": " + remainingTime(current.buffExpiresAt[type]);
		}

		public static bool isBuffActive(int type)
		{
			return type >= 0 && type < BUFF_COUNT
				&& current.buffExpiresAt[type] > mSystem.currentTimeMillis();
		}

		public static string buffStatusText(int type)
		{
			string effect = buffEffectText(type);
			return isBuffActive(type)
				? effect + ": " + remainingTime(current.buffExpiresAt[type])
				: effect + ": Chưa kích hoạt";
		}

		private static string buffEffectText(int type)
		{
			switch (type)
			{
				case 0: return "HP +1%/5s";
				case 1: return "KI +1%/5s";
				case 2: return "Sức đánh +10%";
				case 3: return "May mắn +10%";
				case 4: return "TN/SM +15%";
				default: return "Vàng quái +20%";
			}
		}

		private static string remainingTime(long expiresAt)
		{
			long seconds = System.Math.Max(0L, (expiresAt - mSystem.currentTimeMillis() + 999L) / 1000L);
			long days = seconds / 86400L;
			long hours = seconds % 86400L / 3600L;
			long minutes = seconds % 3600L / 60L;
			if (days > 0L) return days + "d" + hours + "h" + minutes + "p";
			if (hours > 0L) return hours + "h" + minutes + "p";
			if (minutes > 0L) return minutes + "p";
			return seconds % 60L + "s";
		}

		public static void applySnapshot(int receivedClanId, int receivedLevel,
			long receivedExp, long receivedExpRequired, int receivedCapsuleRequired,
			long receivedGoldRequired, long receivedGemRequired, int receivedTotalPoints,
			int receivedUnspentPoints, long receivedVersion, int[] receivedRanks)
		{
			if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId)
			{
				return;
			}
			current.clanId = receivedClanId;
			current.level = receivedLevel;
			current.exp = receivedExp;
			current.expRequired = receivedExpRequired;
			current.capsuleRequired = receivedCapsuleRequired;
			current.goldRequired = receivedGoldRequired;
			current.gemRequired = receivedGemRequired;
			current.totalPoints = receivedTotalPoints;
			current.unspentPoints = receivedUnspentPoints;
			current.version = receivedVersion;
			for (int i = 0; i < BRANCH_COUNT; i++)
			{
				current.ranks[i] = receivedRanks != null && i < receivedRanks.Length ? receivedRanks[i] : 0;
			}
			current.loaded = true;
		}

		private static void applyLiveStats(int receivedClanId, int hpMax, int mpMax,
			int damage, int hp, int mp)
		{
			Char character = Char.myCharz();
			if (character.clan == null || character.clan.ID != receivedClanId)
			{
				return;
			}
			if (hpMax > 0)
			{
				character.cHPFull = hpMax;
				character.cHP = hp;
			}
			if (mpMax > 0)
			{
				character.cMPFull = mpMax;
				character.cMP = mp;
			}
			if (damage >= 0)
			{
				character.cDamFull = damage;
			}
		}

		public static bool isReady(int expectedClanId)
		{
			return current.loaded && current.clanId == expectedClanId;
		}

		public static void applyExpSnapshot(int receivedClanId, long receivedExp,
			long receivedExpRequired, long receivedVersion)
		{
			if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId)
			{
				return;
			}
			if (current.clanId == receivedClanId && current.loaded
				&& receivedVersion < current.version)
			{
				return;
			}
			current.clanId = receivedClanId;
			current.exp = receivedExp;
			current.expRequired = receivedExpRequired;
			current.version = receivedVersion;
		}

		public static void requestSnapshot(bool force)
		{
			if (Char.myCharz().clan == null)
			{
				return;
			}
			long now = mSystem.currentTimeMillis();
			if (!force && now - lastSnapshotRequestAt < 2000L)
			{
				return;
			}
			lastSnapshotRequestAt = now;
			Service.gI().clanProgression(REQUEST_VIEW);
			requestBuffSnapshot(true);
		}

		/// <summary>Refreshes just the temporary clan buffs after a successful clan-storage action.</summary>
		public static void requestBuffSnapshot(bool force)
		{
			if (Char.myCharz().clan == null)
			{
				return;
			}
			long now = mSystem.currentTimeMillis();
			if (!force && now - lastBuffSnapshotRequestAt < 500L)
			{
				return;
			}
			lastBuffSnapshotRequestAt = now;
			Service.gI().clanProgression(REQUEST_BUFF_SNAPSHOT);
		}

		public static bool allocate(int branch)
		{
			if (Char.myCharz().clan == null || !isReady(Char.myCharz().clan.ID)
				|| branch < 0 || branch >= BRANCH_COUNT)
			{
				return false;
			}
			int cost = costNext(branch);
			if (current.unspentPoints < cost)
			{
				return false;
			}
			Service.gI().clanProgression(REQUEST_ALLOCATE, (sbyte)branch);
			// Reflect the click immediately. The forced authoritative request queued
			// directly after it confirms or rolls back this optimistic value.
			current.ranks[branch]++;
			current.unspentPoints -= cost;
			requestSnapshot(true);
			return true;
		}

		public static int rank(int branch)
		{
			return branch >= 0 && branch < BRANCH_COUNT ? current.ranks[branch] : 0;
		}

		public static int costNext(int branch)
		{
			return 1;
		}

		public static int effectBasisPoints(int branch)
		{
			int points = rank(branch);
			return branch <= 2 ? points * 20 : points * 100;
		}

		public static string effectPercentText(int branch)
		{
			int basisPoints = effectBasisPoints(branch);
			int whole = basisPoints / 100;
			int fraction = basisPoints % 100;
			if (fraction == 0)
			{
				return whole.ToString();
			}
			if (fraction % 10 == 0)
			{
				return whole + "," + (fraction / 10);
			}
			return whole + "," + fraction.ToString("00");
		}

		public static int optionId(int branch)
		{
			int[] ids = new int[BRANCH_COUNT] { 50, 77, 103, 236, 101, 100 };
			return branch >= 0 && branch < ids.Length ? ids[branch] : -1;
		}

		public static string effectText(int branch)
		{
			string value = effectPercentText(branch) + "%";
			switch (branch)
			{
				case 0: return "+" + value + " sức đánh";
				case 1: return "+" + value + " HP";
				case 2: return "+" + value + " KI";
				case 3: return "+" + value + " may mắn";
				case 4: return "+" + value + " tiềm năng, sức mạnh";
				default: return "+" + value + " vàng từ quái";
			}
		}
	}
}
