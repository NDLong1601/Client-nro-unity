using System;
using System.Collections.Generic;

namespace Game2
{
	public enum FriendChatMessageKind
	{
		Text,
		Location
	}

	public sealed class FriendChatMessage
	{
		public FriendChatMessageKind Kind;
		public int SenderId;
		public string Text;
		public FriendLocation Location;
		public long ReceivedAtUtcTicks;
	}

	public sealed class FriendLocation
	{
		public short MapId;
		public short ZoneId;
		public short X;
		public short Y;
	}

	public sealed class FriendConversation
	{
		public readonly List<FriendChatMessage> Messages = new List<FriendChatMessage>();
		public string Draft = string.Empty;
		public int UnreadCount;
		public int ScrollOffset;
		public int Revision;
		public bool HasBeenOpened;
		internal long LastAccessSequence;
	}

	public sealed class FriendConversationStore
	{
		public const int MaxConversations = 20;
		public const int MaxMessagesPerConversation = 100;

		private readonly Dictionary<int, FriendConversation> conversations = new Dictionary<int, FriendConversation>();
		private long accessSequence;

		public int Count
		{
			get { return conversations.Count; }
		}

		public bool HasUnreadMessages
		{
			get
			{
				foreach (KeyValuePair<int, FriendConversation> entry in conversations)
				{
					if (entry.Value != null && entry.Value.UnreadCount > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool TryGet(int friendId, out FriendConversation conversation)
		{
			if (conversations.TryGetValue(friendId, out conversation))
			{
				Touch(conversation);
				return true;
			}
			return false;
		}

		public bool TryPeek(int friendId, out FriendConversation conversation)
		{
			return conversations.TryGetValue(friendId, out conversation);
		}

		public FriendConversation GetOrCreate(int friendId)
		{
			if (friendId <= 0)
			{
				throw new ArgumentOutOfRangeException("friendId");
			}
			FriendConversation conversation;
			if (!conversations.TryGetValue(friendId, out conversation))
			{
				if (conversations.Count >= MaxConversations)
				{
					EvictLeastRecentlyUsed();
				}
				conversation = new FriendConversation();
				conversations.Add(friendId, conversation);
			}
			Touch(conversation);
			return conversation;
		}

		public void AppendText(int friendId, int senderId, string text, long receivedAtUtcTicks)
		{
			Append(friendId, new FriendChatMessage
			{
				Kind = FriendChatMessageKind.Text,
				SenderId = senderId,
				Text = text ?? string.Empty,
				ReceivedAtUtcTicks = receivedAtUtcTicks
			});
		}

		public void AppendLocation(int friendId, int senderId, FriendLocation location, long receivedAtUtcTicks)
		{
			if (location == null)
			{
				throw new ArgumentNullException("location");
			}
			Append(friendId, new FriendChatMessage
			{
				Kind = FriendChatMessageKind.Location,
				SenderId = senderId,
				Location = location,
				ReceivedAtUtcTicks = receivedAtUtcTicks
			});
		}

		public void Clear()
		{
			conversations.Clear();
			accessSequence = 0L;
		}

		public void Remove(int friendId)
		{
			conversations.Remove(friendId);
		}

		private void Append(int friendId, FriendChatMessage message)
		{
			FriendConversation conversation = GetOrCreate(friendId);
			if (conversation.Messages.Count == MaxMessagesPerConversation)
			{
				conversation.Messages.RemoveAt(0);
			}
			conversation.Messages.Add(message);
			conversation.Revision++;
			Touch(conversation);
		}

		private void EvictLeastRecentlyUsed()
		{
			int evictedFriendId = -1;
			long oldestAccess = long.MaxValue;
			foreach (KeyValuePair<int, FriendConversation> entry in conversations)
			{
				if (entry.Value.LastAccessSequence < oldestAccess)
				{
					oldestAccess = entry.Value.LastAccessSequence;
					evictedFriendId = entry.Key;
				}
			}
			if (evictedFriendId > 0)
			{
				conversations.Remove(evictedFriendId);
			}
		}

		private void Touch(FriendConversation conversation)
		{
			accessSequence++;
			conversation.LastAccessSequence = accessSequence;
		}
	}

	public sealed class FriendSearchResult
	{
		public int PlayerId;
		public short Head;
		public string Name;
		public byte Relationship;
	}

	public sealed class FriendInboxRequest
	{
		public long RequestId;
		public int SenderId;
		public short Head;
		public string SenderName;
		public long ExpiresAtEpochMillis;
	}

	public sealed class FriendOutgoingRequest
	{
		public int PlayerId;
		public short Head;
		public string Name;
	}

	public sealed class FriendPageState<T>
	{
		public readonly List<T> Results = new List<T>();
		public int RequestToken = -1;
		public int RequestedCursor;
		public int NextCursor;
		public bool HasMore;
		public bool IsLoading;

		public void Begin(int requestToken)
		{
			Begin(requestToken, 0);
		}

		public void Begin(int requestToken, int cursor)
		{
			if (requestToken < 0 || cursor < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (requestToken != RequestToken || cursor == 0)
			{
				Results.Clear();
				NextCursor = 0;
				HasMore = false;
			}
			RequestToken = requestToken;
			RequestedCursor = cursor;
			IsLoading = true;
		}

		public bool Apply(int requestToken, int nextCursor, bool hasMore, List<T> page)
		{
			if (requestToken != RequestToken || page == null || !IsLoading)
			{
				return false;
			}
			if (RequestedCursor == 0)
			{
				Results.Clear();
			}
			Results.AddRange(page);
			NextCursor = nextCursor;
			HasMore = hasMore;
			IsLoading = false;
			return true;
		}

		public void CancelLoading(int requestToken)
		{
			if (RequestToken == requestToken)
			{
				IsLoading = false;
			}
		}

		public void Clear()
		{
			Results.Clear();
			RequestToken = -1;
			RequestedCursor = 0;
			NextCursor = 0;
			HasMore = false;
			IsLoading = false;
		}
	}

	public sealed class FriendSocialProfile
	{
		public int FriendId;
		public short Head;
		public string Name;
		public string ClanName;
		public string ActivityLabel;
		public long RawPower;
		public string FormattedPower;
		public bool Online;
	}

	public sealed class FriendSocialState
	{
		public const int CapabilitySocialV2 = 1;
		private const int MaxOutgoingRequests = 20;

		private static readonly FriendSocialState instance = new FriendSocialState();

		public readonly FriendConversationStore Conversations = new FriendConversationStore();
		public readonly FriendPageState<FriendSearchResult> Search = new FriendPageState<FriendSearchResult>();
		public readonly FriendPageState<FriendInboxRequest> Inbox = new FriendPageState<FriendInboxRequest>();
		public readonly Dictionary<int, bool> FriendOnline = new Dictionary<int, bool>();
		public readonly List<FriendOutgoingRequest> OutgoingRequests = new List<FriendOutgoingRequest>();

		public bool CapabilityObserved;
		public bool SupportsSocialV2;
		public int ProtocolVersion;
		public int FriendLimit;
		public int OnlineFriendCount;
		public int PendingRequestCount;
		public int ActiveChatFriendId = -1;
		public int PendingChatRecipientId = -1;
		public int PendingLocationRecipientId = -1;
		public long PendingInboxRequestId = -1L;
		public int PendingInboxAction;
		public byte LastErrorCode;
		public FriendSocialProfile Profile;

		public static FriendSocialState gI()
		{
			return instance;
		}

		public void ApplyCapability(int protocolVersion, int capabilities, int friendLimit, int onlineFriendCount, int pendingRequestCount)
		{
			CapabilityObserved = true;
			ProtocolVersion = protocolVersion;
			SupportsSocialV2 = protocolVersion >= 1 && (capabilities & CapabilitySocialV2) != 0;
			FriendLimit = SupportsSocialV2 ? friendLimit : 0;
			OnlineFriendCount = SupportsSocialV2 ? onlineFriendCount : 0;
			PendingRequestCount = SupportsSocialV2 ? pendingRequestCount : 0;
		}

		public void MarkCapabilityMissing()
		{
			CapabilityObserved = false;
			SupportsSocialV2 = false;
			ProtocolVersion = 0;
			FriendLimit = 0;
			OnlineFriendCount = 0;
			PendingRequestCount = 0;
		}

		public bool IsInboxOperationPending
		{
			get { return PendingInboxRequestId > 0L; }
		}

		public bool HasUnreadMessages
		{
			get { return Conversations.HasUnreadMessages; }
		}

		public void BeginSearch(int requestToken)
		{
			BeginSearch(requestToken, 0);
		}

		public void BeginSearch(int requestToken, int cursor)
		{
			Search.Begin(requestToken, cursor);
		}

		public void BeginInbox(int requestToken)
		{
			BeginInbox(requestToken, 0);
		}

		public void BeginInbox(int requestToken, int cursor)
		{
			Inbox.Begin(requestToken, cursor);
		}

		public bool BeginInboxOperation(int action, long requestId)
		{
			if ((action != 7 && action != 8) || requestId <= 0L || IsInboxOperationPending)
			{
				return false;
			}
			PendingInboxAction = action;
			PendingInboxRequestId = requestId;
			return true;
		}

		public void CompleteInboxOperation(int action, bool succeeded)
		{
			if (!IsInboxOperationPending || action != PendingInboxAction)
			{
				return;
			}
			long requestId = PendingInboxRequestId;
			PendingInboxAction = 0;
			PendingInboxRequestId = -1L;
			if (!succeeded)
			{
				return;
			}
			for (int index = Inbox.Results.Count - 1; index >= 0; index--)
			{
				if (Inbox.Results[index].RequestId == requestId)
				{
					Inbox.Results.RemoveAt(index);
					break;
				}
			}
		}

		public void UpsertOutgoingRequest(int playerId, short head, string name)
		{
			if (playerId <= 0 || string.IsNullOrEmpty(name) || name.Trim().Length == 0)
			{
				return;
			}
			for (int index = 0; index < OutgoingRequests.Count; index++)
			{
				FriendOutgoingRequest request = OutgoingRequests[index];
				if (request != null && request.PlayerId == playerId)
				{
					request.Head = head;
					request.Name = name;
					return;
				}
			}
			if (OutgoingRequests.Count >= MaxOutgoingRequests)
			{
				OutgoingRequests.RemoveAt(0);
			}
			OutgoingRequests.Add(new FriendOutgoingRequest
			{
				PlayerId = playerId,
				Head = head,
				Name = name
			});
		}

		public void QueueOutgoingChat(int friendId)
		{
			PendingChatRecipientId = friendId > 0 ? friendId : -1;
		}

		public int ConsumePendingChatRecipient()
		{
			int result = PendingChatRecipientId;
			PendingChatRecipientId = -1;
			return result;
		}

		public void QueueLocationRecipient(int friendId)
		{
			PendingLocationRecipientId = friendId > 0 ? friendId : -1;
		}

		public int ConsumePendingLocationRecipient()
		{
			int result = PendingLocationRecipientId;
			PendingLocationRecipientId = -1;
			return result;
		}

		public void SetPresence(int friendId, bool online)
		{
			if (friendId <= 0)
			{
				return;
			}
			FriendOnline[friendId] = online;
			if (Profile != null && Profile.FriendId == friendId)
			{
				Profile.Online = online;
			}
		}

		public bool TryGetPresence(int friendId, out bool online)
		{
			return FriendOnline.TryGetValue(friendId, out online);
		}

		public FriendConversation ActivateConversation(int friendId)
		{
			FriendConversation conversation = Conversations.GetOrCreate(friendId);
			ActiveChatFriendId = friendId;
			conversation.UnreadCount = 0;
			conversation.HasBeenOpened = true;
			return conversation;
		}

		public void DeactivateConversation(int friendId)
		{
			if (ActiveChatFriendId == friendId)
			{
				ActiveChatFriendId = -1;
			}
		}

		public void RemoveFriend(int friendId)
		{
			if (friendId <= 0)
			{
				return;
			}
			Conversations.Remove(friendId);
			FriendOnline.Remove(friendId);
			if (ActiveChatFriendId == friendId)
			{
				ActiveChatFriendId = -1;
			}
			if (PendingChatRecipientId == friendId)
			{
				PendingChatRecipientId = -1;
			}
			if (PendingLocationRecipientId == friendId)
			{
				PendingLocationRecipientId = -1;
			}
			if (Profile != null && Profile.FriendId == friendId)
			{
				Profile = null;
			}
		}

		public void Reset()
		{
			Conversations.Clear();
			Search.Clear();
			Inbox.Clear();
			FriendOnline.Clear();
			OutgoingRequests.Clear();
			CapabilityObserved = false;
			SupportsSocialV2 = false;
			ProtocolVersion = 0;
			FriendLimit = 0;
			OnlineFriendCount = 0;
			PendingRequestCount = 0;
			ActiveChatFriendId = -1;
			PendingChatRecipientId = -1;
			PendingLocationRecipientId = -1;
			PendingInboxRequestId = -1L;
			PendingInboxAction = 0;
			LastErrorCode = 0;
			Profile = null;
		}
	}
}
