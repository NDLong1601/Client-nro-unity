namespace Game2
{
	public class ClanAppearanceData
	{
		public bool enabled;
		public int protocolVersion;
		public int appearanceVersion;
		public int clanId = -1;
		public int tierId;
		public int tierCount;
		public string tierName = string.Empty;
		public string title = string.Empty;
		public string resourceName = "cay_lv_01";
		public int accentRgb = 0xFFF500;
		public int auraStyle;
		public int clanLevel = 1;
		public int treeLevel = 1;
		public long clanValue;
		public long clanValueVersion;
		public long visualRevision;
		public int nextTierId = -1;
		public string nextTierName = string.Empty;
		public int nextClanLevel;
		public int nextTreeLevel;
		public long nextClanValue;
		public int remainingClanLevels;
		public int remainingTreeLevels;
		public long remainingClanValue;
	}

	/// <summary>Server-selected visual tier for the shared clan tree.</summary>
	public class ClanAppearance
	{
		public const sbyte REQUEST_VIEW = unchecked((sbyte)128);
		public const sbyte RESPONSE_SNAPSHOT = unchecked((sbyte)128);

		public static ClanAppearanceData current = new ClanAppearanceData();
		public static bool loaded;
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
				int protocolVersion = reader.readUnsignedByte();
				ClanAppearanceData received = readDetails(reader, protocolVersion);
				applySnapshot(received, received.clanId);
			}
			catch (System.Exception ex)
			{
				Res.outz("ClanAppearance.receive: " + ex.Message);
			}
			return true;
		}

		public static ClanAppearanceData readDetails(myReader reader, int protocolVersion)
		{
			if (protocolVersion < 1)
			{
				throw new System.Exception("unsupported appearance protocol");
			}
			ClanAppearanceData data = new ClanAppearanceData();
			data.protocolVersion = protocolVersion;
			data.enabled = reader.readBoolean();
			data.appearanceVersion = reader.readUnsignedByte();
			data.clanId = reader.readInt();
			data.tierId = reader.readUnsignedByte();
			data.tierCount = reader.readUnsignedByte();
			data.tierName = ClanPhase5Wire.readBoundedUtf(reader, 64, 40);
			data.title = ClanPhase5Wire.readBoundedUtf(reader, 128, 80);
			string receivedResourceName = ClanPhase5Wire.readBoundedUtf(reader, 64, 32);
			data.accentRgb = reader.readInt() & 0xFFFFFF;
			data.auraStyle = reader.readUnsignedByte();
			data.clanLevel = reader.readInt();
			data.treeLevel = reader.readInt();
			data.clanValue = reader.readLong();
			data.clanValueVersion = reader.readLong();
			data.visualRevision = reader.readLong();
			data.nextTierId = reader.readByte();
			data.nextTierName = ClanPhase5Wire.readBoundedUtf(reader, 64, 40);
			data.nextClanLevel = reader.readInt();
			data.nextTreeLevel = reader.readInt();
			data.nextClanValue = reader.readLong();
			data.remainingClanLevels = reader.readInt();
			data.remainingTreeLevels = reader.readInt();
			data.remainingClanValue = reader.readLong();
			if (data.clanId < 0 || data.tierCount < 1 || data.tierCount > 8
				|| data.tierId < 0 || data.tierId >= data.tierCount
				|| data.auraStyle < 0 || data.auraStyle > 3 || data.clanLevel < 1
				|| data.treeLevel < 1 || data.treeLevel > 20 || data.clanValue < 0L
				|| data.clanValueVersion < 0L || data.visualRevision < 0L
				|| data.nextTierId < -1 || data.nextTierId >= data.tierCount
				|| data.nextClanLevel < 0 || data.nextTreeLevel < 0
				|| data.nextClanValue < 0L || data.remainingClanLevels < 0
				|| data.remainingTreeLevels < 0 || data.remainingClanValue < 0L)
			{
				throw new System.Exception("invalid appearance snapshot");
			}
			string expectedResourceName = treeResourceName(data.treeLevel);
			data.resourceName = receivedResourceName == expectedResourceName
				? receivedResourceName : expectedResourceName;
			return data;
		}

		public static void applySnapshot(ClanAppearanceData data, int expectedClanId)
		{
			if (data == null || data.clanId != expectedClanId || Char.myCharz().clan == null
				|| Char.myCharz().clan.ID != expectedClanId)
			{
				return;
			}
			if (loaded && current.clanId == expectedClanId
				&& data.visualRevision < current.visualRevision)
			{
				return;
			}
			current = data;
			loaded = true;
		}

		public static bool isReady(int expectedClanId)
		{
			return loaded && current.clanId == expectedClanId;
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
			Service.gI().clanAppearanceView();
		}

		public static string treeResourceName(int treeLevel)
		{
			int safeLevel = treeLevel;
			if (safeLevel < 1) safeLevel = 1;
			if (safeLevel > 20) safeLevel = 20;
			return "cay_lv_" + safeLevel.ToString("D2");
		}

		public static void reset()
		{
			current = new ClanAppearanceData();
			loaded = false;
			lastSnapshotRequestAt = 0L;
		}
	}

	/// <summary>Strict helpers for strings crossing the custom clan protocol boundary.</summary>
	internal static class ClanPhase5Wire
	{
		public static string readBoundedUtf(myReader reader, int maxBytes, int maxChars)
		{
			int length = reader.readUnsignedShort();
			if (length < 0 || length > maxBytes || length > reader.available())
			{
				throw new System.Exception("invalid UTF length");
			}
			sbyte[] signedBytes = new sbyte[length];
			reader.readFully(ref signedBytes);
			byte[] bytes = new byte[length];
			for (int i = 0; i < length; i++)
			{
				bytes[i] = myReader.convertSbyteToByte(signedBytes[i]);
			}
			string value = decodeModifiedUtf(bytes);
			System.Text.StringBuilder clean = new System.Text.StringBuilder(value.Length);
			for (int i = 0; i < value.Length && clean.Length < maxChars; i++)
			{
				if (char.IsHighSurrogate(value[i]) && i + 1 < value.Length
					&& char.IsLowSurrogate(value[i + 1]))
				{
					if (clean.Length + 2 > maxChars)
					{
						break;
					}
					clean.Append(value[i]);
					clean.Append(value[++i]);
					continue;
				}
				if (!char.IsControl(value[i]) && !char.IsSurrogate(value[i]))
				{
					clean.Append(value[i]);
				}
			}
			return clean.ToString();
		}

		private static string decodeModifiedUtf(byte[] bytes)
		{
			System.Text.StringBuilder value = new System.Text.StringBuilder(bytes.Length);
			int offset = 0;
			while (offset < bytes.Length)
			{
				int first = bytes[offset++] & 0xFF;
				if ((first & 0x80) == 0)
				{
					value.Append((char)first);
					continue;
				}
				if ((first & 0xE0) == 0xC0)
				{
					if (offset >= bytes.Length || (bytes[offset] & 0xC0) != 0x80)
					{
						throw new System.Exception("invalid modified UTF");
					}
					int code = ((first & 0x1F) << 6) | (bytes[offset++] & 0x3F);
					if (code < 0x80 && code != 0)
					{
						throw new System.Exception("overlong modified UTF");
					}
					value.Append((char)code);
					continue;
				}
				if ((first & 0xF0) == 0xE0)
				{
					if (offset + 1 >= bytes.Length || (bytes[offset] & 0xC0) != 0x80
						|| (bytes[offset + 1] & 0xC0) != 0x80)
					{
						throw new System.Exception("invalid modified UTF");
					}
					int code = ((first & 0x0F) << 12) | ((bytes[offset] & 0x3F) << 6)
						| (bytes[offset + 1] & 0x3F);
					offset += 2;
					if (code < 0x800)
					{
						throw new System.Exception("overlong modified UTF");
					}
					value.Append((char)code);
					continue;
				}
				throw new System.Exception("invalid modified UTF lead byte");
			}
			return value.ToString();
		}
	}
}
