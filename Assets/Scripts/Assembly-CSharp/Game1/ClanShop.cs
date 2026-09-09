namespace Game1
{
	public class ClanShopItem
	{
		public int itemId;
		public string name;
		public int tier;
		public int stock;
		public int stockCap;
		public int capsuleCost;
		public long contributionRequired;
		public int dailyLimit;
		public bool unlocked;
	}

	/// <summary>Read-only shop state. The server validates every restock and purchase.</summary>
	public class ClanShop
	{
		public const sbyte REQUEST_VIEW = 116;
		public const sbyte REQUEST_RESTOCK = 117;
		public const sbyte REQUEST_BUY = 118;
		public const sbyte RESPONSE_SNAPSHOT = 119;

		public static ClanShop current = new ClanShop();
		public int clanId = -1;
		public bool enabled;
		public int clanLevel;
		public int personalCapsule;
		public long contribution;
		public MyVector items = new MyVector();
		public bool loaded;

		public static bool receive(Message message, sbyte action)
		{
			if (action != RESPONSE_SNAPSHOT)
			{
				return false;
			}
			try
			{
				int receivedClanId = message.reader().readInt();
				if (Char.myCharz().clan == null || Char.myCharz().clan.ID != receivedClanId)
				{
					return true;
				}
				current.clanId = receivedClanId;
				current.enabled = message.reader().readUnsignedByte() == 1;
				current.clanLevel = message.reader().readInt();
				current.personalCapsule = message.reader().readInt();
				current.contribution = message.reader().readLong();
				current.items.removeAllElements();
				int count = message.reader().readUnsignedByte();
				for (int i = 0; i < count; i++)
				{
					ClanShopItem item = new ClanShopItem();
					item.itemId = message.reader().readInt();
					item.name = message.reader().readUTF();
					item.tier = message.reader().readUnsignedByte();
					item.stock = message.reader().readInt();
					item.stockCap = message.reader().readInt();
					item.capsuleCost = message.reader().readInt();
					item.contributionRequired = message.reader().readLong();
					item.dailyLimit = message.reader().readUnsignedByte();
					item.unlocked = message.reader().readUnsignedByte() == 1;
					current.items.addElement(item);
				}
				current.loaded = true;
				if (GameCanvas.panel != null && GameCanvas.panel.isClanShop)
				{
					GameCanvas.panel.initTabClans();
				}
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanShop.receive: " + ex.Message);
			}
			return true;
		}

		public static bool isReady(int expectedClanId)
		{
			return current.loaded && current.clanId == expectedClanId;
		}

		public static void reset()
		{
			current = new ClanShop();
		}
	}
}
