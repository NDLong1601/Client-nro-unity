$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Require-Contains([string]$content, [string]$expected, [string]$message) {
    if (!$content.Contains($expected)) {
        throw $message
    }
}

function Get-MethodBody([string]$content, [string]$signature) {
    $start = $content.IndexOf($signature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) {
        throw "Cannot find method signature: $signature"
    }
    $openBrace = $content.IndexOf('{', $start)
    $depth = 0
    for ($index = $openBrace; $index -lt $content.Length; $index++) {
        if ($content[$index] -eq '{') { $depth++ }
        if ($content[$index] -eq '}') {
            $depth--
            if ($depth -eq 0) {
                return $content.Substring($openBrace, $index - $openBrace + 1)
            }
        }
    }
    throw "Cannot find closing brace for: $signature"
}

foreach ($game in @('Game1', 'Game2')) {
    $basePath = Join-Path $projectRoot ("Assets\Scripts\Assembly-CSharp\{0}" -f $game)
    $panel = Get-Content -LiteralPath (Join-Path $basePath 'Panel.cs') -Raw
    $main = Get-Content -LiteralPath (Join-Path $basePath 'Main.cs') -Raw
    $canvas = Get-Content -LiteralPath (Join-Path $basePath 'GameCanvas.cs') -Raw
    $socialState = Get-Content -LiteralPath (Join-Path $basePath 'SocialV2\FriendSocialState.cs') -Raw
    $socialProtocol = Get-Content -LiteralPath (Join-Path $basePath 'SocialV2\FriendSocialProtocol.cs') -Raw

    $desktopInput = Get-MethodBody $main 'private bool updateDesktopImeTextField(Event currentEvent)'
    Require-Contains $desktopInput 'GUI.TextField' "$game still funnels desktop text through the one-character keyAsciiPress slot"
    Require-Contains $desktopInput 'KeyCode.KeypadEnter' "$game does not submit Social V2 text with keypad Enter"
    Require-Contains $desktopInput 'GameCanvas.keyPressed[15] = true' "$game does not route native Enter to Social V2 submit"
    $submitCaptureIndex = $desktopInput.IndexOf('bool submit =', [System.StringComparison]::Ordinal)
    $nativeFieldIndex = $desktopInput.IndexOf('GUI.TextField', [System.StringComparison]::Ordinal)
    if ($submitCaptureIndex -lt 0 -or $nativeFieldIndex -lt 0 -or $submitCaptureIndex -gt $nativeFieldIndex) {
        throw "$game captures Enter after GUI.TextField may already consume the event"
    }
    $imePreview = Get-MethodBody $main 'internal static string getSocialDesktopInputPreview(TField field)'
    Require-Contains $imePreview 'Input.compositionString' "$game hides the active Vietnamese IME composition until Enter commits it"

    Require-Contains $panel 'FRIEND_SOCIAL_CHAT_COMPOSER_MIN_HEIGHT' "$game chat composer still has a single fixed height"
    Require-Contains $panel 'FRIEND_SOCIAL_CHAT_COMPOSER_MAX_HEIGHT' "$game chat composer has no bounded multiline height"
    $composerHeight = Get-MethodBody $panel 'private int getFriendSocialChatComposerHeight(FriendConversation conversation, int friendId)'
    Require-Contains $composerHeight 'splitFontArray' "$game chat composer height is not derived from wrapped text"
    Require-Contains $composerHeight 'Main.getSocialDesktopInputPreview' "$game composer height ignores the visible Vietnamese IME composition"
    $composerPainter = Get-MethodBody $panel 'private void paintFriendSocialChatComposer(mGraphics g, FriendConversation conversation, int friendId)'
    Require-Contains $composerPainter 'draftLines' "$game chat composer does not paint wrapped draft lines"
    Require-Contains $composerPainter 'Main.getSocialDesktopInputPreview' "$game composer does not paint the active Vietnamese IME composition"

    $chatScroll = Get-MethodBody $panel 'private void refreshFriendSocialChatScroll(FriendConversation conversation, int friendId, bool online)'
    Require-Contains $chatScroll 'friendSocialChatScrollFriendId' "$game reloads chat scroll state every frame instead of only when switching conversations"
    Require-Contains $chatScroll 'friendSocialChatComposerHeight' "$game message viewport does not reserve the dynamic composer height"
    Require-Contains $chatScroll 'yScroll = 80' "$game still reserves a blank row for the removed chat date/time header"
    Require-Contains $chatScroll 'H - 96' "$game does not return the removed date/time row to the message viewport"
    $mouseScroll = Get-MethodBody $panel 'public void updateScroolMouse(int a)'
    Require-Contains $mouseScroll 'friendSocialChatOnly' "$game has no variable-height chat wheel scrolling path"
    Require-Contains $mouseScroll 'GameCanvas.pxMouse < xScroll' "$game wheel hitbox still assumes the panel starts at x=0"
    Require-Contains $mouseScroll 'cmy = cmtoY;' "$game friend chat wheel waits for camera easing instead of tracking the scroll position immediately"

    Require-Contains $socialState 'public int Revision;' "$game conversations cannot distinguish a new message when the capped message count is unchanged"
    $appendConversation = Get-MethodBody $socialState 'private void Append(int friendId, FriendChatMessage message)'
    Require-Contains $appendConversation 'conversation.Revision++' "$game does not version conversation changes"
    Require-Contains $panel 'friendSocialChatLayoutRevision' "$game chat layout cache only watches message count"
    $chatLayout = Get-MethodBody $panel 'private void refreshFriendSocialChatLayout(FriendConversation conversation, int friendId)'
    Require-Contains $chatLayout 'friendSocialChatScrollToBottom = true' "$game does not jump to the newest message when a visible conversation receives one"

    $appendPrivateChat = Get-MethodBody $socialProtocol 'public static bool TryAppendPrivateChat(FriendSocialState state, int localPlayerId, int senderId, string text)'
    Require-Contains $appendPrivateChat 'NormalizePrivateChatText(text)' "$game stores the legacy |5| color marker as visible chat text"

    $topInfo = Get-MethodBody $panel 'private void paintFriendSocialChatTopInfo(mGraphics g)'
    Require-Contains $topInfo 'paintFriendSocialChatTopAvatar' "$game chat header does not mirror the player avatar rendering"
    $topAvatar = Get-MethodBody $panel 'private void paintFriendSocialChatTopAvatar(mGraphics g, Char character, int fallbackHead)'
    Require-Contains $topAvatar 'avatarz()' "$game chat header does not prioritize the same mapped head icon as the player header"
    Require-Contains $topAvatar 'X + W - 32, 50, 0, 33' "$game chat header avatar is still clipped against the right edge"
    Require-Contains $topAvatar 'X + W - 55, 4' "$game chat header fallback avatar is still clipped against the right edge"
    $chatAvatar = Get-MethodBody $panel 'private void paintFriendSocialAvatar(mGraphics g, Char character, int fallbackHead, int x, int y, int transform)'
    Require-Contains $chatAvatar 'transform, 0' "$game chat avatar helper ignores the requested horizontal orientation"
    $chatPainter = Get-MethodBody $panel 'private void paintFriendSocialChat(mGraphics g)'
    Require-Contains $chatPainter 'bubble.Width' "$game chat bubbles do not size themselves to their content"
    Require-Contains $chatPainter 'bubble.SenderLabel' "$game chat bubbles omit the sender label from the supplied design"
    Require-Contains $chatPainter 'bubble.IsOutgoing ? xScroll + 29' "$game does not place the local player's messages on the left as designed"
    Require-Contains $chatPainter 'xScroll + wScroll - 26' "$game positions the remote sender head outside the right-side clip"
    Require-Contains $chatPainter 'mGraphics.TRANS_NONE' "$game local chat head does not keep the left-side orientation"
    Require-Contains $chatPainter 'mGraphics.TRANS_MIRROR' "$game friend chat head does not face the local player"
    Require-Contains $chatPainter 'g.fillRect(bubbleX, bubbleY, bubbleWidth, bubble.Height, 4)' "$game chat bubbles still have square corners"
    if ($chatPainter.Contains('bubbleX + 4, bubbleY + bubble.Height - 1, bubbleWidth - 8')) {
        throw "$game chat bubble still paints the obsolete dark bottom divider"
    }
    if ($chatPainter.Contains('paintFriendSocialChatHeader')) {
        throw "$game still paints the removed chat date/time row"
    }

    $toolbar = Get-MethodBody $panel 'private void paintFriendSocialToolbar(mGraphics g)'
    Require-Contains $toolbar 'Main.getSocialDesktopInputPreview' "$game search field does not paint the active Vietnamese IME composition"

    $routeWheel = Get-MethodBody $canvas 'public void scrollMouse(int a)'
    Require-Contains $routeWheel 'panel2.updateScroolMouse(a)' "$game always routes the mouse wheel to the left panel"

    $search = Get-MethodBody $panel 'private void startFriendSocialSearch(string query)'
    Require-Contains $search 'searchFriendSocialV2' "$game no longer sends name/id search requests"
}

Write-Output 'SOCIAL_V2_PHASE9_UI_FIXES_OK'
