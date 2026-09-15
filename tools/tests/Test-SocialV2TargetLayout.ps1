$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Require-Contains([string]$content, [string]$expected, [string]$message) {
    if (!$content.Contains($expected)) {
        throw $message
    }
}

function Get-MethodBody([string]$content, [string]$signature) {
    $start = $content.IndexOf($signature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Cannot find method signature: $signature" }
    $openBrace = $content.IndexOf('{', $start)
    $depth = 0
    for ($index = $openBrace; $index -lt $content.Length; $index++) {
        if ($content[$index] -eq '{') { $depth++ }
        if ($content[$index] -eq '}') {
            $depth--
            if ($depth -eq 0) { return $content.Substring($openBrace, $index - $openBrace + 1) }
        }
    }
    throw "Cannot find closing brace for: $signature"
}

foreach ($game in @('Game1', 'Game2')) {
    $panelPath = Join-Path $projectRoot ("Assets\Scripts\Assembly-CSharp\{0}\Panel.cs" -f $game)
    $panel = Get-Content -LiteralPath $panelPath -Raw

    foreach ($contract in @(
        'FRIEND_SOCIAL_LIST_HEADING_HEIGHT',
        'paintFriendSocialToolbar',
        'paintFriendSocialRoundButton',
        'friendSocialSearchInput',
        'paintFriendSocialContentHeading',
        'getFriendSocialContentHeading',
        'paintFriendSocialSearchRow',
        'paintFriendSocialInboxRow',
        'getFriendSocialSearchRelationshipLabel',
        'friendSocialChatInput.getText()',
        '0x5170ff',
        '0x57aa05')) {
        Require-Contains $panel $contract "$game is missing target Social V2 layout contract: $contract"
    }

    Require-Contains $panel 'shareFriendSocialV2Location(friendId)' "$game target composer lost location sharing"
    Require-Contains $panel 'sendFriendSocialV2Chat(text, friendId)' "$game target composer lost chat sending"
    Require-Contains $panel 'sendFriendSocialV2Request(result.PlayerId)' "$game target search lost friend requests"
    Require-Contains $panel 'acceptFriendSocialV2Request(request.RequestId)' "$game target inbox lost request acceptance"
    Require-Contains $panel 'rejectFriendSocialV2Request(request.RequestId)' "$game target inbox lost request rejection"

    $listTitle = Get-MethodBody $panel 'private void paintFriendSocialTitle(mGraphics g)'
    Require-Contains $listTitle 'mResources.friend' "$game repeats friend counters in the center title"
    $chatTitle = Get-MethodBody $panel 'private void paintFriendSocialChatTitle(mGraphics g)'
    Require-Contains $chatTitle 'state.ActiveChatFriendId' "$game chat title does not include the friend id"
    if ($panel.Contains('paintFriendSocialChatHeader')) {
        throw "$game still paints the removed chat date/time row"
    }
    Require-Contains $panel 'yScroll = 80' "$game chat message area does not reclaim the removed date/time row"
    Require-Contains $panel 'X + W - 32, 50, 0, 33' "$game friend profile head icon remains clipped at the panel edge"
    Require-Contains $panel 'xScroll + wScroll - 26' "$game remote message head remains clipped on the right edge"
    Require-Contains $panel 'mGraphics.TRANS_MIRROR' "$game remote message head does not face the local player"
    Require-Contains $panel 'g.fillRect(bubbleX, bubbleY, bubbleWidth, bubble.Height, 4)' "$game message bubbles still have square corners"

    Require-Contains $panel 'FRIEND_SOCIAL_ROW_HEIGHT = 24' "$game friend rows do not match the compact clan-member row height"
    Require-Contains $panel 'FRIEND_SOCIAL_COLOR_INPUT = 0xd7c9b5' "$game Social V2 textfields lost their normal background"
    Require-Contains $panel 'FRIEND_SOCIAL_COLOR_BUTTON = 0xb9a17c' "$game Social V2 buttons do not contrast with the action-panel background"
    Require-Contains $panel 'FRIEND_SOCIAL_COLOR_DIVIDER = 0x8f8374' "$game Social V2 dividers do not use the softened neutral color"
    Require-Contains $panel 'FRIEND_SOCIAL_ACTION_ICON_SIZE = 14' "$game Social V2 action icons are still oversized"
    Require-Contains $panel 'FRIEND_SOCIAL_SEARCH_INPUT_X_OFFSET = 48' "$game player-search input was not extended toward its label"

    $chat = Get-MethodBody $panel 'private void paintFriendSocialChat(mGraphics g)'
    if ($chat.Contains('bubble.Height - 1, bubbleWidth - 8, 1')) {
        throw "$game chat bubbles still paint the unwanted dark bottom line"
    }
    Require-Contains $chat 'paintFriendSocialChatAvatarBox' "$game chat heads are not painted inside a 24px box"
    $chatAvatar = Get-MethodBody $panel 'private void paintFriendSocialChatAvatarBox(mGraphics g, Char character, int fallbackHead, int boxX, int boxY, int transform)'
    Require-Contains $chatAvatar 'FRIEND_SOCIAL_MEMBER_AVATAR_WIDTH' "$game chat head box does not reuse the 24px member-avatar size"
    Require-Contains $chatAvatar '9993045' "$game chat head box does not reuse the member head-box color"
    Require-Contains $chatAvatar 'SmallImage.drawSmallImage' "$game chat head does not render above its background box"
    Require-Contains $chatAvatar 'boxX + FRIEND_SOCIAL_MEMBER_AVATAR_WIDTH / 2' "$game chat head is not anchored to the center of its 24px box"
    if ($chatAvatar.Contains('g.setClip(boxX')) {
        throw "$game chat head is still clipped beneath its 24px background box"
    }
    if ($chatAvatar.Contains('centerX -=') -or $chatAvatar.Contains('centerX +=')) {
        throw "$game mirrored chat head still applies a manual horizontal offset after centering"
    }

    Require-Contains $panel 'FRIEND_SOCIAL_COLOR_ACTION_PANEL = 0xe6ded1' "$game bottom action panel does not use the supplied light background"
    $composer = Get-MethodBody $panel 'private void paintFriendSocialChatComposer(mGraphics g, FriendConversation conversation, int friendId)'
    Require-Contains $composer 'FRIEND_SOCIAL_COLOR_ACTION_PANEL' "$game chat composer does not use the supplied action-panel background"
    Require-Contains $composer 'paintFriendSocialInputFrame' "$game chat composer does not use the shared Social V2 input frame"
    Require-Contains $composer 'FRIEND_SOCIAL_COLOR_BUTTON' "$game composer buttons do not contrast with their panel"
    Require-Contains $composer 'FRIEND_SOCIAL_COLOR_DIVIDER' "$game chat composer did not restore its black top divider"
    Require-Contains $composer 'g.fillRect(xScroll, composerY, wScroll, 1)' "$game chat composer divider is not at the panel edge"

    $toolbar = Get-MethodBody $panel 'private void paintFriendSocialToolbar(mGraphics g)'
    Require-Contains $toolbar 'paintFriendSocialInputFrame' "$game player search does not reuse the chat input frame"
    Require-Contains $toolbar 'inputHeight = FRIEND_SOCIAL_TOOLBAR_ACTION_SIZE' "$game player search input does not match the chat control height"
    Require-Contains $toolbar 'FRIEND_SOCIAL_COLOR_BUTTON' "$game inactive mail button still blends into the toolbar"
    Require-Contains $toolbar 'FRIEND_SOCIAL_SEARCH_INPUT_X_OFFSET' "$game player-search painter does not use the extended input position"
    Require-Contains $toolbar 'FRIEND_SOCIAL_COLOR_DIVIDER' "$game search toolbar did not restore its black bottom divider"
    Require-Contains $toolbar 'toolbarY + FRIEND_SOCIAL_MODE_BAR_HEIGHT - 1' "$game search toolbar divider is not aligned to its bottom edge"
    $inputFrame = Get-MethodBody $panel 'private static void paintFriendSocialInputFrame(mGraphics g, TField input, int x, int y, int width, int height)'
    Require-Contains $inputFrame 'FRIEND_SOCIAL_COLOR_INPUT_FOCUS' "$game shared input frame has no focus background"
    Require-Contains $inputFrame 'FRIEND_SOCIAL_COLOR_INPUT' "$game shared input frame lost its normal background"
    if ($inputFrame.Contains('drawRect') -or $inputFrame.Contains('FRIEND_SOCIAL_COLOR_INPUT_BORDER')) {
        throw "$game focused Social V2 textfield still paints an outline"
    }

    $roundButton = Get-MethodBody $panel 'private static void paintFriendSocialRoundButton(mGraphics g, int x, int y, int color, Image icon)'
    Require-Contains $roundButton 'drawFriendSocialIconScaled' "$game round-button icons are still painted at their oversized source dimensions"

    $tabs = Get-MethodBody $panel 'private void paintTab(mGraphics g)'
    $friendTab = Get-MethodBody $tabs 'if (type == 11)'
    Require-Contains $friendTab 'FRIEND_SOCIAL_COLOR_DIVIDER' "$game friend/chat title did not restore its black divider"
    Require-Contains $friendTab 'fillRect(X + 1, 78, W - 2, 1)' "$game friend/chat title divider is not in its original position"

    $searchInput = Get-MethodBody $panel 'private void ensureFriendSocialSearchInput()'
    Require-Contains $searchInput 'FRIEND_SOCIAL_SEARCH_INPUT_X_OFFSET' "$game native player-search input is not aligned with its painted frame"
    $searchUpdate = Get-MethodBody $panel 'private void updateFriendSocialSearchInput()'
    Require-Contains $searchUpdate 'restoreFriendListWhenSearchIsEmpty' "$game does not react when the player-search input becomes empty"
    $emptySearch = Get-MethodBody $panel 'private bool restoreFriendListWhenSearchIsEmpty(string query)'
    Require-Contains $emptySearch 'Search.Clear()' "$game keeps stale player-search results after clearing the input"
    Require-Contains $emptySearch 'setFriendSocialMode(FRIEND_SOCIAL_MODE_FRIENDS)' "$game does not return to the friend list after clearing the search"
    Require-Contains $emptySearch 'setFocusWithKb(true)' "$game clearing search drops input focus instead of allowing immediate retyping"

    $memberRow = Get-MethodBody $panel 'private static void paintMemberListRowBackground(mGraphics g, int x, int y, int width, int height, bool selected)'
    Require-Contains $memberRow 'FRIEND_SOCIAL_MEMBER_AVATAR_WIDTH' "$game member-style row has no dedicated head box"
    Require-Contains $memberRow '9993045' "$game member-style row does not reuse the clan member head-box color"
    $friendRow = Get-MethodBody $panel 'private void paintFriendSocialRowBackground(mGraphics g, int index, int rowY, bool active)'
    Require-Contains $friendRow 'paintMemberListRowBackground' "$game friend list does not reuse the clan member row painter"
    $friendItem = Get-MethodBody $panel 'private void paintFriendSocialFriendRow(mGraphics g, int index, int rowY)'
    if ($friendItem.Contains('friendSocialChatIcon') -or $friendItem.Contains('/mainimage/social_chat.png')) {
        throw "$game still paints the redundant chat icon in online friend rows"
    }
    $clanRows = Get-MethodBody $panel 'private void paintClans(mGraphics g)'
    Require-Contains $clanRows 'paintMemberListRowBackground' "$game clan member tab is not wired to the shared member-row painter"
    Require-Contains $panel 'paintFriendSocialListAvatar' "$game friend list head is not constrained to the member-style head box"

    $gameScrPath = Join-Path $projectRoot ("Assets\Scripts\Assembly-CSharp\{0}\GameScr.cs" -f $game)
    $gameScr = Get-Content -LiteralPath $gameScrPath -Raw
    Require-Contains $gameScr 'FriendSocialState.gI().HasUnreadMessages' "$game menu button does not blink for unread friend messages"
}

Write-Output 'SOCIAL_V2_TARGET_LAYOUT_OK'
