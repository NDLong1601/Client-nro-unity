namespace Game2
{
	public class ClanRankingEntry
	{
		public int rank;
		public int clanId;
		public int emblemId;
		public int clanLevel;
		public int treeLevel;
		public int currentMembers;
		public int maxMembers;
		public long clanValue;
		public long clanValueVersion;
		public int leaderId;
		public int leaderHead;
		public int leaderBody;
		public int leaderLeg;
		public string name = string.Empty;
		public string shortName = string.Empty;
	}

	/// <summary>Bounded, paged phase-5 clan ranking snapshot.</summary>
	public class ClanRanking
	{
		public const sbyte REQUEST_PAGE = 127;
		public const sbyte RESPONSE_PAGE = 127;
		public const int DEFAULT_PAGE_SIZE = 20;
		public const int MAX_PAGE_SIZE = 50;

		public static ClanRanking current = new ClanRanking();
		public bool loaded;
		public int protocolVersion;
		public int pageNumber;
		public int pageSize = DEFAULT_PAGE_SIZE;
		public int totalEntries;
		public int totalPages;
		public int requesterRank = -1;
		public long boardVersion;
		public long generatedAt;
		public MyVector entries = new MyVector();
		private static long lastRequestAt;
		private static int lastRequestedPage = -1;

		public static bool receive(Message message, sbyte action)
		{
			if (action != RESPONSE_PAGE)
			{
				return false;
			}
			try
			{
				myReader reader = message.reader();
				int receivedProtocolVersion = reader.readUnsignedByte();
				int receivedPageNumber = reader.readUnsignedShort();
				int receivedPageSize = reader.readUnsignedByte();
				int receivedTotalEntries = reader.readInt();
				int receivedTotalPages = reader.readInt();
				int receivedRequesterRank = reader.readInt();
				long receivedBoardVersion = reader.readLong();
				long receivedGeneratedAt = reader.readLong();
				int rowCount = reader.readUnsignedByte();
				if (receivedProtocolVersion < 1 || receivedPageSize < 1
					|| receivedPageSize > MAX_PAGE_SIZE || rowCount > receivedPageSize
					|| rowCount > MAX_PAGE_SIZE || receivedTotalEntries < 0
					|| receivedTotalPages < 0 || receivedBoardVersion < 0L
					|| receivedGeneratedAt < 0L)
				{
					throw new System.Exception("invalid ranking header");
				}
				MyVector receivedEntries = new MyVector();
				for (int i = 0; i < rowCount; i++)
				{
					ClanRankingEntry entry = new ClanRankingEntry();
					entry.rank = reader.readInt();
					entry.clanId = reader.readInt();
					entry.emblemId = reader.readInt();
					entry.clanLevel = reader.readInt();
					entry.treeLevel = reader.readInt();
					entry.currentMembers = reader.readUnsignedShort();
					entry.maxMembers = reader.readUnsignedShort();
					entry.clanValue = reader.readLong();
					entry.clanValueVersion = reader.readLong();
					entry.leaderId = reader.readInt();
					entry.leaderHead = reader.readShort();
					entry.leaderBody = reader.readShort();
					entry.leaderLeg = reader.readShort();
					entry.name = ClanPhase5Wire.readBoundedUtf(reader, 128, 80);
					entry.shortName = ClanPhase5Wire.readBoundedUtf(reader, 64, 32);
					if (entry.rank < 1 || entry.clanId < 0 || entry.clanLevel < 1
						|| entry.treeLevel < 1 || entry.treeLevel > 20
						|| entry.currentMembers > entry.maxMembers || entry.clanValue < 0L
						|| entry.clanValueVersion < 0L)
					{
						throw new System.Exception("invalid ranking row");
					}
					receivedEntries.addElement(entry);
				}
				current.loaded = true;
				current.protocolVersion = receivedProtocolVersion;
				current.pageNumber = receivedPageNumber;
				current.pageSize = receivedPageSize;
				current.totalEntries = receivedTotalEntries;
				current.totalPages = receivedTotalPages;
				current.requesterRank = receivedRequesterRank;
				current.boardVersion = receivedBoardVersion;
				current.generatedAt = receivedGeneratedAt;
				current.entries = receivedEntries;
				if (GameCanvas.panel != null && GameCanvas.panel.isClanRanking)
				{
					GameCanvas.panel.initTabClans();
				}
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanRanking.receive: " + ex.Message);
			}
			return true;
		}

		public static void requestPage(int requestedPage, bool force)
		{
			if (Char.myCharz().clan == null)
			{
				return;
			}
			int safePage = requestedPage;
			if (safePage < 0) safePage = 0;
			if (current.loaded && current.totalPages > 0 && safePage >= current.totalPages)
			{
				safePage = current.totalPages - 1;
			}
			if (safePage > 65535) safePage = 65535;
			long now = mSystem.currentTimeMillis();
			if (!force && safePage == lastRequestedPage && now - lastRequestAt < 1500L)
			{
				return;
			}
			lastRequestedPage = safePage;
			lastRequestAt = now;
			Service.gI().clanRankingPage(safePage, DEFAULT_PAGE_SIZE);
		}

		public static void reset()
		{
			current = new ClanRanking();
			lastRequestAt = 0L;
			lastRequestedPage = -1;
		}
	}
}
