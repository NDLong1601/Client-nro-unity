using System;
using System.Collections.Generic;

internal static class SocialV2Phase6Regression
{
    public static int Main()
    {
        try
        {
            VerifyPagedResultsDoNotDiscardPriorPages();
            VerifyInboxCompletionChangesOnlyTheConfirmedRequest();
            Console.WriteLine("SOCIAL_V2_PHASE6_OK");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static void VerifyPagedResultsDoNotDiscardPriorPages()
    {
        Game2.FriendPageState<int> page = new Game2.FriendPageState<int>();
        page.Begin(71, 0);
        Require(page.IsLoading, "A first page is marked loading before its response");
        Require(page.Apply(71, 20, true, new List<int> { 10, 11 }), "The first page is accepted");
        Require(page.Results.Count == 2 && page.HasMore && !page.IsLoading,
            "The first page records its cursor and finishes loading");

        page.Begin(71, 20);
        Require(page.Apply(71, 40, true, new List<int> { 12 }), "A next page uses the same request token");
        Require(page.Results.Count == 3 && page.Results[0] == 10 && page.Results[2] == 12,
            "A next page appends instead of replacing prior visible results");

        page.Begin(72, 0);
        Require(!page.Apply(71, 60, false, new List<int> { 13 }), "A late response from an older request is discarded");
        Require(page.Results.Count == 0 && page.IsLoading,
            "A stale response cannot replace the new query or complete its loading state");
        page.CancelLoading(72);
        Require(!page.IsLoading, "The active request can be released after a failed response");
    }

    private static void VerifyInboxCompletionChangesOnlyTheConfirmedRequest()
    {
        Game2.FriendSocialState state = new Game2.FriendSocialState();
        state.Inbox.Begin(22, 0);
        state.Inbox.Apply(22, 0, false, new List<Game2.FriendInboxRequest>
        {
            new Game2.FriendInboxRequest { RequestId = 1001L, SenderId = 30, SenderName = "A" },
            new Game2.FriendInboxRequest { RequestId = 1002L, SenderId = 31, SenderName = "B" }
        });
        Require(state.BeginInboxOperation(7, 1001L), "An accept operation reserves exactly one inbox request");
        state.CompleteInboxOperation(8, true);
        Require(state.Inbox.Results.Count == 2, "A mismatched response cannot remove an inbox row");
        state.CompleteInboxOperation(7, true);
        Require(state.Inbox.Results.Count == 1 && state.Inbox.Results[0].RequestId == 1002L,
            "A successful accept removes only its confirmed request");
        Require(!state.IsInboxOperationPending, "The completed inbox operation is released");

        Game2.FriendSocialState protocolState = new Game2.FriendSocialState();
        protocolState.ApplyCapability(1, 1, 100, 0, 1);
        protocolState.Inbox.Begin(23, 0);
        protocolState.Inbox.Apply(23, 0, false, new List<Game2.FriendInboxRequest>
        {
            new Game2.FriendInboxRequest { RequestId = 2001L, SenderId = 32, SenderName = "C" }
        });
        Require(protocolState.BeginInboxOperation(7, 2001L), "A protocol-tracked accept reserves its request");
        Game2.myWriter response = new Game2.myWriter();
        response.writeByte(0);
        Require(Game2.FriendSocialProtocol.TryHandleAction(7, new Game2.myReader(response.getData()), protocolState, 1),
            "The accept success envelope is parsed");
        Require(protocolState.Inbox.Results.Count == 0 && !protocolState.IsInboxOperationPending,
            "The parsed accept envelope confirms and removes its own inbox request");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
