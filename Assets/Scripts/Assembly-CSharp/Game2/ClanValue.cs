namespace Game2
{
	/// <summary>Server-authoritative score used by the phase-5 clan ranking.</summary>
	public class ClanValue
	{
		public const sbyte REQUEST_VIEW = 126;
		public const sbyte RESPONSE_SNAPSHOT = 126;

		public static ClanValue current = new ClanValue();
		public bool enabled;
		public bool loaded;
		public int clanId = -1;
		public int protocolVersion;
		public int formulaVersion;
		public long totalValue;
		public long clanLevelScore;
		public long spentPotentialScore;
		public long treeLevelScore;
		public long achievementScore;
		public long weeklyActivityScore;
		public long version;
		private static long lastSnapshotRequestAt;

		public static bool receive(Message message, sbyte action)
		{
			if (action != RESPONSE_SNAPSHOT)
			{
				return false;
			}
			try
			{
				myReader reader = message.reader();
				int receivedClanId = reader.readInt();
				int receivedProtocolVersion = reader.readUnsignedByte();
				if (receivedProtocolVersion < 1)
				{
					throw new System.Exception("unsupported protocol");
				}
				int receivedFormulaVersion = reader.readUnsignedByte();
				long receivedTotalValue = nonNegative(reader.readLong());
				long receivedClanLevelScore = nonNegative(reader.readLong());
				long receivedSpentPotentialScore = nonNegative(reader.readLong());
				long receivedTreeLevelScore = nonNegative(reader.readLong());
				long receivedAchievementScore = nonNegative(reader.readLong());
				long receivedWeeklyActivityScore = nonNegative(reader.readLong());
				long receivedVersion = nonNegative(reader.readLong());
				applySnapshot(receivedClanId, true, receivedProtocolVersion,
					receivedFormulaVersion, receivedTotalValue, receivedClanLevelScore,
					receivedSpentPotentialScore, receivedTreeLevelScore,
					receivedAchievementScore, receivedWeeklyActivityScore, receivedVersion);
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanValue.receive: " + ex.Message);
			}
			return true;
		}

		public static void applyProfileSnapshot(int receivedClanId, bool receivedEnabled,
			int receivedFormulaVersion, long receivedTotalValue, long receivedClanLevelScore,
			long receivedSpentPotentialScore, long receivedTreeLevelScore,
			long receivedAchievementScore, long receivedWeeklyActivityScore, long receivedVersion)
		{
			applySnapshot(receivedClanId, receivedEnabled, 1, receivedFormulaVersion,
				receivedTotalValue, receivedClanLevelScore, receivedSpentPotentialScore,
				receivedTreeLevelScore, receivedAchievementScore,
				receivedWeeklyActivityScore, receivedVersion);
		}

		private static void applySnapshot(int receivedClanId, bool receivedEnabled,
			int receivedProtocolVersion, int receivedFormulaVersion, long receivedTotalValue,
			long receivedClanLevelScore, long receivedSpentPotentialScore,
			long receivedTreeLevelScore, long receivedAchievementScore,
			long receivedWeeklyActivityScore, long receivedVersion)
		{
			if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId)
			{
				return;
			}
			if (current.loaded && current.clanId == receivedClanId && receivedVersion < current.version)
			{
				return;
			}
			current.enabled = receivedEnabled;
			current.loaded = true;
			current.clanId = receivedClanId;
			current.protocolVersion = receivedProtocolVersion;
			current.formulaVersion = receivedFormulaVersion;
			current.totalValue = nonNegative(receivedTotalValue);
			current.clanLevelScore = nonNegative(receivedClanLevelScore);
			current.spentPotentialScore = nonNegative(receivedSpentPotentialScore);
			current.treeLevelScore = nonNegative(receivedTreeLevelScore);
			current.achievementScore = nonNegative(receivedAchievementScore);
			current.weeklyActivityScore = nonNegative(receivedWeeklyActivityScore);
			current.version = nonNegative(receivedVersion);
		}

		public static bool isReady(int expectedClanId)
		{
			return current.loaded && current.clanId == expectedClanId;
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
			Service.gI().clanValueView();
		}

		public static void reset()
		{
			current = new ClanValue();
			lastSnapshotRequestAt = 0L;
		}

		private static long nonNegative(long value)
		{
			return value < 0L ? 0L : value;
		}
	}
}
