using System;

internal static class SocialV2Phase7Regression
{
    public static int Main()
    {
        try
        {
            VerifyConversationActivationKeepsDraftAndMarksRead();
            VerifyPrivateChatBeforeCapabilityIsRetained();
            VerifyIncomingChatUsesActiveReadState();
            VerifyUnreadIndicatorTracksConversationReadState();
            VerifyConversationRevisionChangesAtCapacity();
            VerifyPresenceTransitionKeepsDraftUntilRemoval();
            Console.WriteLine("SOCIAL_V2_PHASE7_OK");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static void VerifyConversationActivationKeepsDraftAndMarksRead()
    {
        Game2.FriendSocialState state = new Game2.FriendSocialState();
        state.Conversations.AppendText(101, 101, "Tin chua doc", DateTime.UtcNow.Ticks);
        Game2.FriendConversation first = state.ActivateConversation(101);
        Require(state.ActiveChatFriendId == 101 && first.UnreadCount == 0 && first.HasBeenOpened,
            "Opening a conversation makes it active, marks it viewed, and clears only its unread count");
        first.Draft = "Ban nhap duoc luu";
        first.ScrollOffset = 27;

        state.ActivateConversation(202);
        Game2.FriendConversation restored = state.ActivateConversation(101);
        Require(restored.Draft == "Ban nhap duoc luu" && restored.ScrollOffset == 27,
            "Switching conversations preserves the per-friend draft and scroll position");
        Require(state.ActiveChatFriendId == 101,
            "Reopening a conversation restores the active friend independently of row selection");
    }

    private static void VerifyPrivateChatBeforeCapabilityIsRetained()
    {
        Game2.FriendSocialState state = new Game2.FriendSocialState();
        Require(!state.CapabilityObserved && !state.SupportsSocialV2,
            "A fresh login has not received the friend-tab capability tail yet");

        Require(Game2.FriendSocialProtocol.TryAppendPrivateChat(state, 1, 606, "|5|Tin den truoc khi mo tab"),
            "A private-chat event received before the first friend-tab open is retained");
        Game2.FriendConversation conversation;
        Require(state.Conversations.TryPeek(606, out conversation)
            && conversation.Messages.Count == 1
            && conversation.Messages[0].Text == "Tin den truoc khi mo tab"
            && conversation.UnreadCount == 1,
            "The pre-capability message is normalized and remains unread in the correct conversation");

        state.ApplyCapability(1, Game2.FriendSocialState.CapabilitySocialV2, 100, 1, 0);
        Game2.FriendConversation opened = state.ActivateConversation(606);
        Require(object.ReferenceEquals(conversation, opened)
            && opened.Messages.Count == 1
            && opened.UnreadCount == 0,
            "Opening the friend chat after capability discovery reveals the retained message and marks it read");
    }

    private static void VerifyPresenceTransitionKeepsDraftUntilRemoval()
    {
        Game2.FriendSocialState state = new Game2.FriendSocialState();
        Game2.FriendConversation conversation = state.ActivateConversation(303);
        conversation.Draft = "Khong tu dong gui";
        state.SetPresence(303, false);

        bool online;
        Require(state.TryGetPresence(303, out online) && !online,
            "Offline presence is observable by the chat panel without altering the conversation");
        Require(conversation.Draft == "Khong tu dong gui" && state.ActiveChatFriendId == 303,
            "Going offline hides input in the view but keeps the unsent draft and active history");

        state.RemoveFriend(303);
        Game2.FriendConversation ignored;
        Require(state.ActiveChatFriendId == -1 && !state.Conversations.TryPeek(303, out ignored),
            "Removing the active friend closes the selection and erases its in-memory chat state");
    }

    private static void VerifyIncomingChatUsesActiveReadState()
    {
        Game2.FriendSocialState state = new Game2.FriendSocialState();
        state.ApplyCapability(1, Game2.FriendSocialState.CapabilitySocialV2, 100, 0, 0);
        state.ActivateConversation(404);
        Require(Game2.FriendSocialProtocol.TryAppendPrivateChat(state, 1, 404, "|5|Tin dang mo"),
            "An incoming social chat is accepted for an active conversation");
        Game2.FriendConversation conversation;
        Require(state.Conversations.TryPeek(404, out conversation) && conversation.UnreadCount == 0,
            "An incoming chat to the active conversation is immediately marked read");
        Require(conversation.Messages[0].Text == "Tin dang mo" && conversation.Revision == 1,
            "The Social V2 history removes one legacy color prefix and versions the new message");

        state.DeactivateConversation(404);
        Require(Game2.FriendSocialProtocol.TryAppendPrivateChat(state, 1, 404, "Tin chua doc"),
            "An incoming social chat is retained after the chat panel closes");
        Require(conversation.UnreadCount == 1,
            "An incoming chat to an inactive conversation increments only that unread counter");
    }

    private static void VerifyConversationRevisionChangesAtCapacity()
    {
        Game2.FriendConversationStore store = new Game2.FriendConversationStore();
        for (int index = 0; index < Game2.FriendConversationStore.MaxMessagesPerConversation; index++)
        {
            store.AppendText(505, 505, "Tin " + index, DateTime.UtcNow.Ticks);
        }
        Game2.FriendConversation conversation;
        Require(store.TryPeek(505, out conversation), "The capped conversation remains available");
        int revisionAtCapacity = conversation.Revision;
        store.AppendText(505, 505, "Tin moi nhat", DateTime.UtcNow.Ticks);
        Require(conversation.Messages.Count == Game2.FriendConversationStore.MaxMessagesPerConversation
            && conversation.Revision == revisionAtCapacity + 1
            && conversation.Messages[conversation.Messages.Count - 1].Text == "Tin moi nhat",
            "A new message changes the revision even when the capped list count stays unchanged");
    }

    private static void VerifyUnreadIndicatorTracksConversationReadState()
    {
        Game2.FriendSocialState state = new Game2.FriendSocialState();
        Require(!state.HasUnreadMessages,
            "A fresh social state does not light the global friend-message indicator");

        Require(Game2.FriendSocialProtocol.TryAppendPrivateChat(state, 1, 707, "Tin nhan moi"),
            "An incoming inactive friend message is retained for the menu indicator");
        Require(state.HasUnreadMessages,
            "An unread friend conversation lights the global menu indicator");

        state.ActivateConversation(707);
        Require(!state.HasUnreadMessages,
            "Opening the unread conversation clears the global menu indicator");

        Require(Game2.FriendSocialProtocol.TryAppendPrivateChat(state, 1, 707, "Tin dang xem"),
            "An incoming message remains accepted while the conversation is active");
        Require(!state.HasUnreadMessages,
            "Messages received in the active conversation do not relight the menu indicator");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
