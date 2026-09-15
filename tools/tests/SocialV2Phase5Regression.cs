using System;

internal static class SocialV2Phase5Regression
{
    private const int CapabilitySocialV2 = 1;

    public static int Main()
    {
        try
        {
            VerifyGame2();
            VerifyGame1();
            Console.WriteLine("SOCIAL_V2_PHASE5_OK");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static void VerifyGame2()
    {
        Game2.FriendSocialState state = new Game2.FriendSocialState();
        Game2.myWriter capabilityWriter = new Game2.myWriter();
        capabilityWriter.writeByte(1);
        capabilityWriter.writeInt(CapabilitySocialV2);
        capabilityWriter.writeByte(100);
        capabilityWriter.writeByte(2);
        capabilityWriter.writeShort(3);
        Require(Game2.FriendSocialProtocol.TryReadCapabilityTail(new Game2.myReader(capabilityWriter.getData()), state),
            "Game2 parses the complete capability tail");
        Require(state.SupportsSocialV2 && state.FriendLimit == 100 && state.OnlineFriendCount == 2
            && state.PendingRequestCount == 3, "Game2 records only the documented capability tail");

        state.Reset();
        Require(!Game2.FriendSocialProtocol.TryReadCapabilityTail(new Game2.myReader(new sbyte[0]), state),
            "Game2 rejects an old action-0 response without a tail");
        Require(!state.SupportsSocialV2, "Game2 remains fail-closed without a complete tail");
        Game2.myWriter ungatedPresenceWriter = new Game2.myWriter();
        ungatedPresenceWriter.writeInt(41);
        ungatedPresenceWriter.writeBoolean(true);
        Require(!Game2.FriendSocialProtocol.TryHandleAction(3, new Game2.myReader(ungatedPresenceWriter.getData()), state, 10),
            "Game2 rejects V2 events before a server grants capability");
        Require(state.FriendOnline.Count == 0, "Game2 does not mutate social state from an ungated V2 event");
        state.ApplyCapability(1, CapabilitySocialV2, 100, 2, 3);

        state.BeginSearch(11);
        Game2.myWriter searchWriter = new Game2.myWriter();
        searchWriter.writeByte(0);
        searchWriter.writeInt(11);
        searchWriter.writeInt(20);
        searchWriter.writeBoolean(true);
        searchWriter.writeByte(1);
        searchWriter.writeInt(41);
        searchWriter.writeShort(7);
        searchWriter.writeUTF("Bạn mới");
        searchWriter.writeByte(2);
        Require(Game2.FriendSocialProtocol.TryHandleAction(4, new Game2.myReader(searchWriter.getData()), state, 10),
            "Game2 parses a social search page");
        Require(state.Search.Results.Count == 1 && state.Search.Results[0].PlayerId == 41 && state.Search.HasMore,
            "Game2 records a current search page");

        state.BeginSearch(12);
        Require(Game2.FriendSocialProtocol.TryHandleAction(4, new Game2.myReader(searchWriter.getData()), state, 10),
            "Game2 consumes stale pages without throwing");
        Require(state.Search.Results.Count == 0 && state.Search.RequestToken == 12 && state.Search.IsLoading,
            "Game2 keeps a new search empty until its own response arrives and ignores a stale page");

        state.QueueLocationRecipient(41);
        Game2.myWriter locationWriter = new Game2.myWriter();
        locationWriter.writeInt(10);
        locationWriter.writeShort(3);
        locationWriter.writeShort(4);
        locationWriter.writeShort(120);
        locationWriter.writeShort(121);
        Require(Game2.FriendSocialProtocol.TryHandleAction(11, new Game2.myReader(locationWriter.getData()), state, 10),
            "Game2 parses a server-derived location event");
        Game2.FriendConversation conversation;
        Require(state.Conversations.TryGet(41, out conversation)
            && conversation.Messages.Count == 1 && conversation.Messages[0].Kind == Game2.FriendChatMessageKind.Location,
            "Game2 stores an echoed location against the requested friend");
        state.ActiveChatFriendId = 41;
        state.QueueOutgoingChat(41);
        state.QueueLocationRecipient(41);
        state.SetPresence(41, true);
        state.RemoveFriend(41);
        Require(!state.Conversations.TryGet(41, out conversation) && !state.FriendOnline.ContainsKey(41)
            && state.ActiveChatFriendId == -1 && state.PendingChatRecipientId == -1
            && state.PendingLocationRecipientId == -1, "Game2 removes all friend-scoped social state together");

        for (int friendId = 1; friendId <= 21; friendId++)
        {
            for (int index = 0; index < 101; index++)
            {
                state.Conversations.AppendText(friendId, friendId, "m" + index, 1L);
            }
        }
        Require(state.Conversations.Count == 20, "Game2 evicts conversations above the session cap");
        Require(state.Conversations.TryGet(21, out conversation) && conversation.Messages.Count == 100,
            "Game2 keeps the latest 100 messages in an active conversation");

        state.Reset();
        Require(state.Conversations.Count == 0 && state.ActiveChatFriendId == -1 && state.PendingRequestCount == 0,
            "Game2 clears every account-scoped social value on reset");
    }

    private static void VerifyGame1()
    {
        Game1.FriendSocialState state = new Game1.FriendSocialState();
        Game1.myWriter capabilityWriter = new Game1.myWriter();
        capabilityWriter.writeByte(1);
        capabilityWriter.writeInt(CapabilitySocialV2);
        capabilityWriter.writeByte(100);
        capabilityWriter.writeByte(1);
        capabilityWriter.writeShort(0);
        Require(Game1.FriendSocialProtocol.TryReadCapabilityTail(new Game1.myReader(capabilityWriter.getData()), state),
            "Game1 parses the complete capability tail");
        Require(state.SupportsSocialV2 && state.OnlineFriendCount == 1, "Game1 records social capability independently");

        state.QueueOutgoingChat(66);
        Require(state.PendingChatRecipientId == 66, "Game1 remembers an unconfirmed chat recipient only until echo");
        state.Conversations.AppendText(66, 9, "đã nhận", 1L);
        state.Reset();
        Require(state.Conversations.Count == 0 && state.PendingChatRecipientId == -1 && !state.SupportsSocialV2,
            "Game1 reset does not leak social data to the next account");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
