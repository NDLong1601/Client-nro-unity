namespace Game2
{
	public class ClanLedgerEntry
	{
		public long id;
		public long actorId;
		public string actorName;
		public string actionType;
		public sbyte currencyType;
		public long amount;
		public long balanceAfter;
		public long createdAt;
	}

	public class ClanTreasuryDepositRequest
	{
		public sbyte currency;
		public long amount;

		public ClanTreasuryDepositRequest(sbyte currency, long amount)
		{
			this.currency = currency;
			this.amount = amount;
		}
	}

	public class ClanTreasury
	{
		public const sbyte REQUEST_VIEW = 100;
		public const sbyte REQUEST_DEPOSIT_GOLD = 101;
		public const sbyte REQUEST_DEPOSIT_GEM = 102;
		public const sbyte REQUEST_LEDGER_PAGE = 103;
		public const sbyte RESPONSE_SNAPSHOT = 110;
		public const sbyte RESPONSE_LEDGER_PAGE = 111;
		public const sbyte CURRENCY_GOLD = 1;
		public const sbyte CURRENCY_GEM = 2;
		public static ClanTreasury current = new ClanTreasury();
		public int clanId = -1;
		public long capsule;
		public long gold;
		public long gem;
		public long contribution;
		public long version;

		public bool snapshotLoaded;

		public long ledgerVersion = -1L;
		public bool hasMore;
		public long nextLedgerCursor;
		public MyVector ledger = new MyVector();

		public static bool receive(Message msg)
		{
			try
			{
				sbyte action = msg.reader().readByte();
				return receive(msg, action);
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanTreasury.receive: " + ex.Message);
				return true;
			}
		}

		public static bool receive(Message msg, sbyte action)
		{
			try
			{
				if (action == RESPONSE_SNAPSHOT)
				{
					int receivedClanId = msg.reader().readInt();
					long receivedCapsule = msg.reader().readLong();
					long receivedGold = msg.reader().readLong();
					long receivedGem = msg.reader().readLong();
					long receivedContribution = msg.reader().readLong();
					long receivedVersion = msg.reader().readLong();
					applyProfileSnapshot(receivedClanId, receivedCapsule, receivedGold,
						receivedGem, receivedContribution, receivedVersion);
					return true;
				}
				if (action != RESPONSE_LEDGER_PAGE)
				{
					return false;
				}
				int receivedClanId2 = msg.reader().readInt();
				long requestedCursor = msg.reader().readLong();
				bool receivedHasMore = msg.reader().readUnsignedByte() == 1;
				long receivedNextCursor = msg.reader().readLong();
				int count = msg.reader().readUnsignedByte();
				if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId2)
				{
					return true;
				}
				if (current.clanId != receivedClanId2 || requestedCursor == 0L)
				{
					current.clanId = receivedClanId2;
					current.ledger.removeAllElements();
				}
				for (int i = 0; i < count; i++)
				{
					ClanLedgerEntry clanLedgerEntry = new ClanLedgerEntry();
					clanLedgerEntry.id = msg.reader().readLong();
					clanLedgerEntry.actorId = msg.reader().readLong();
					clanLedgerEntry.actorName = msg.reader().readUTF();
					clanLedgerEntry.actionType = msg.reader().readUTF();
					clanLedgerEntry.currencyType = msg.reader().readByte();
					clanLedgerEntry.amount = msg.reader().readLong();
					clanLedgerEntry.balanceAfter = msg.reader().readLong();
					clanLedgerEntry.createdAt = msg.reader().readLong();
					current.ledger.addElement(clanLedgerEntry);
				}
				current.hasMore = receivedHasMore;
				current.nextLedgerCursor = receivedNextCursor;
				if (requestedCursor == 0L)
				{
					current.ledgerVersion = current.version;
				}
				refreshVisibleTreasury();
				return true;
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanTreasury.receive: " + ex.Message);
				return true;
			}
		}

		public static void applyProfileSnapshot(int receivedClanId, long receivedCapsule,
			long receivedGold, long receivedGem, long receivedContribution, long receivedVersion)
		{
			if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId)
			{
				return;
			}
			if (current.clanId != receivedClanId)
			{
				current.ledger.removeAllElements();
				current.ledgerVersion = -1L;
			}
			current.clanId = receivedClanId;
			current.capsule = receivedCapsule;
			current.gold = receivedGold;
			current.gem = receivedGem;
			current.contribution = receivedContribution;
			current.version = receivedVersion;
			current.snapshotLoaded = true;
			refreshVisibleTreasury();
		}

		public static void applyProfileLedger(int receivedClanId, MyVector receivedLedger)
		{
			if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId || receivedLedger == null)
			{
				return;
			}
			current.clanId = receivedClanId;
			current.ledger.removeAllElements();
			for (int i = 0; i < receivedLedger.size(); i++)
			{
				current.ledger.addElement(receivedLedger.elementAt(i));
			}
			current.hasMore = false;
			current.nextLedgerCursor = 0L;
			current.ledgerVersion = current.version;
			refreshVisibleTreasury();
		}

		public static bool isSnapshotReady(int expectedClanId)
		{
			return current.snapshotLoaded && current.clanId == expectedClanId;
		}

		public static bool isLedgerCurrent(int expectedClanId)
		{
			return current.clanId == expectedClanId && current.ledgerVersion == current.version;
		}

		public static void reset()
		{
			current = new ClanTreasury();
		}

		private static void refreshVisibleTreasury()
		{
			try
			{
				if (GameCanvas.panel != null && (GameCanvas.panel.isTreasury ||
					GameCanvas.panel.isTreasuryHistory || GameCanvas.panel.isClanInfo))
				{
					GameCanvas.panel.initTabClans();
				}
			}
			catch (System.Exception)
			{
			}
		}
	}
}
