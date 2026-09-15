using System;
using System.Collections.Generic;

namespace Game2
{
	public static class FriendSocialProtocol
	{
		private const int CapabilityTailLength = 9;
		private const int Success = 0;
		private const int Error = 1;
		private const int MaxPageEntries = 20;

		public static bool TryReadCapabilityTail(myReader reader, FriendSocialState state)
		{
			if (reader == null || state == null || reader.available() < CapabilityTailLength)
			{
				if (state != null)
				{
					state.MarkCapabilityMissing();
				}
				return false;
			}
			try
			{
				int protocolVersion = reader.readUnsignedByte();
				int capabilities = reader.readInt();
				int friendLimit = reader.readUnsignedByte();
				int onlineFriendCount = reader.readUnsignedByte();
				int pendingRequestCount = reader.readUnsignedShort();
				state.ApplyCapability(protocolVersion, capabilities, friendLimit, onlineFriendCount, pendingRequestCount);
				return true;
			}
			catch (Exception)
			{
				state.MarkCapabilityMissing();
				return false;
			}
		}

		public static bool TryHandleAction(int action, myReader reader, FriendSocialState state, int localPlayerId)
		{
			if (reader == null || state == null || !state.SupportsSocialV2)
			{
				return false;
			}
			try
			{
				switch (action)
				{
				case 3:
					state.SetPresence(reader.readInt(), reader.readBoolean());
					return true;
				case 4:
					return TryReadSearchPage(reader, state);
				case 5:
					return TryReadSendRequestEnvelope(reader, state);
				case 10:
					return TryReadEnvelope(reader, state);
				case 7:
				case 8:
					return TryReadInboxOperationEnvelope(reader, state, action);
				case 6:
					return TryReadInboxPage(reader, state);
				case 9:
					return TryReadProfile(reader, state);
				case 11:
					return TryReadLocation(reader, state, localPlayerId);
				case 12:
					state.PendingRequestCount = reader.readUnsignedShort();
					return true;
				default:
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static bool TryAppendPrivateChat(FriendSocialState state, int localPlayerId, int senderId, string text)
		{
			if (state == null || senderId <= 0)
			{
				return false;
			}
			int friendId = senderId == localPlayerId ? state.ConsumePendingChatRecipient() : senderId;
			if (friendId <= 0)
			{
				return false;
			}
			state.Conversations.AppendText(friendId, senderId, NormalizePrivateChatText(text), DateTime.UtcNow.Ticks);
			FriendConversation conversation;
			if (senderId != localPlayerId && state.Conversations.TryGet(friendId, out conversation)
				&& state.ActiveChatFriendId != friendId)
			{
				conversation.UnreadCount++;
			}
			return true;
		}

		private static string NormalizePrivateChatText(string text)
		{
			const string legacyColorPrefix = "|5|";
			string normalized = text ?? string.Empty;
			return normalized.StartsWith(legacyColorPrefix, StringComparison.Ordinal)
				? normalized.Substring(legacyColorPrefix.Length) : normalized;
		}

		private static bool TryReadEnvelope(myReader reader, FriendSocialState state)
		{
			int result = reader.readUnsignedByte();
			if (result == Success)
			{
				state.LastErrorCode = 0;
				return true;
			}
			if (result != Error || reader.available() < 1)
			{
				return false;
			}
			state.LastErrorCode = reader.readUnsignedByte();
			return true;
		}

		private static bool TryReadSendRequestEnvelope(myReader reader, FriendSocialState state)
		{
			int result = reader.readUnsignedByte();
			if (result == Success)
			{
				state.LastErrorCode = 0;
				if (reader.available() == 0)
				{
					return true;
				}
				if (reader.available() < 8)
				{
					return false;
				}
				int playerId = reader.readInt();
				short head = reader.readShort();
				string name = reader.readUTF();
				if (playerId <= 0 || string.IsNullOrEmpty(name) || name.Trim().Length == 0)
				{
					return false;
				}
				state.UpsertOutgoingRequest(playerId, head, name);
				return true;
			}
			if (result != Error || reader.available() < 1)
			{
				return false;
			}
			state.LastErrorCode = reader.readUnsignedByte();
			return true;
		}

		private static bool TryReadInboxOperationEnvelope(myReader reader, FriendSocialState state, int action)
		{
			int result = reader.readUnsignedByte();
			if (result == Success)
			{
				state.LastErrorCode = 0;
				state.CompleteInboxOperation(action, true);
				return true;
			}
			if (result != Error || reader.available() < 1)
			{
				return false;
			}
			state.LastErrorCode = reader.readUnsignedByte();
			state.CompleteInboxOperation(action, false);
			return true;
		}

		private static bool TryReadSearchPage(myReader reader, FriendSocialState state)
		{
			int result = reader.readUnsignedByte();
			if (result != Success)
			{
				return ReadError(result, reader, state);
			}
			int requestToken = reader.readInt();
			int nextCursor = reader.readInt();
			bool hasMore = reader.readBoolean();
			int count = reader.readUnsignedByte();
			if (count > MaxPageEntries)
			{
				return false;
			}
			List<FriendSearchResult> entries = new List<FriendSearchResult>(count);
			for (int index = 0; index < count; index++)
			{
				entries.Add(new FriendSearchResult
				{
					PlayerId = reader.readInt(),
					Head = reader.readShort(),
					Name = reader.readUTF(),
					Relationship = reader.readUnsignedByte()
				});
			}
			state.Search.Apply(requestToken, nextCursor, hasMore, entries);
			return true;
		}

		private static bool TryReadInboxPage(myReader reader, FriendSocialState state)
		{
			int result = reader.readUnsignedByte();
			if (result != Success)
			{
				return ReadError(result, reader, state);
			}
			int requestToken = reader.readInt();
			int nextCursor = reader.readInt();
			bool hasMore = reader.readBoolean();
			int count = reader.readUnsignedByte();
			if (count > MaxPageEntries)
			{
				return false;
			}
			List<FriendInboxRequest> entries = new List<FriendInboxRequest>(count);
			for (int index = 0; index < count; index++)
			{
				entries.Add(new FriendInboxRequest
				{
					RequestId = reader.readLong(),
					SenderId = reader.readInt(),
					Head = reader.readShort(),
					SenderName = reader.readUTF(),
					ExpiresAtEpochMillis = reader.readLong()
				});
			}
			state.Inbox.Apply(requestToken, nextCursor, hasMore, entries);
			return true;
		}

		private static bool TryReadProfile(myReader reader, FriendSocialState state)
		{
			int result = reader.readUnsignedByte();
			if (result != Success)
			{
				return ReadError(result, reader, state);
			}
			state.Profile = new FriendSocialProfile
			{
				FriendId = reader.readInt(),
				Head = reader.readShort(),
				Name = reader.readUTF(),
				ClanName = reader.readUTF(),
				ActivityLabel = reader.readUTF(),
				RawPower = reader.readLong(),
				FormattedPower = reader.readUTF(),
				Online = reader.readBoolean()
			};
			state.SetPresence(state.Profile.FriendId, state.Profile.Online);
			return true;
		}

		private static bool TryReadLocation(myReader reader, FriendSocialState state, int localPlayerId)
		{
			int senderId = reader.readInt();
			FriendLocation location = new FriendLocation
			{
				MapId = reader.readShort(),
				ZoneId = reader.readShort(),
				X = reader.readShort(),
				Y = reader.readShort()
			};
			int friendId = senderId == localPlayerId ? state.ConsumePendingLocationRecipient() : senderId;
			if (friendId <= 0)
			{
				return true;
			}
			state.Conversations.AppendLocation(friendId, senderId, location, DateTime.UtcNow.Ticks);
			FriendConversation conversation;
			if (senderId != localPlayerId && state.Conversations.TryGet(friendId, out conversation)
				&& state.ActiveChatFriendId != friendId)
			{
				conversation.UnreadCount++;
			}
			return true;
		}

		private static bool ReadError(int result, myReader reader, FriendSocialState state)
		{
			if (result != Error || reader.available() < 1)
			{
				return false;
			}
			state.LastErrorCode = reader.readUnsignedByte();
			return true;
		}
	}
}
