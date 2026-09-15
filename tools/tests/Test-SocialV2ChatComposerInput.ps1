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

    $chatInput = Get-MethodBody $panel 'private void updateKeyFriendSocialChat()'
    $chatPainter = Get-MethodBody $panel 'private void paintFriendSocialChat(mGraphics g)'
    $composerPainter = Get-MethodBody $panel 'private void paintFriendSocialChatComposer(mGraphics g, FriendConversation conversation, int friendId)'
    $inputFrame = Get-MethodBody $panel 'private static void paintFriendSocialInputFrame(mGraphics g, TField input, int x, int y, int width, int height)'
    Require-Contains $chatInput 'Main.isPC' "$game chat composer does not guard desktop Backspace handling"
    Require-Contains $chatInput 'GameCanvas.keyPressed[14]' "$game chat composer does not receive Backspace"
    Require-Contains $chatInput 'friendSocialChatInput.keyPressed(-8)' "$game chat composer does not route Backspace to its text field"
    Require-Contains $chatInput 'shareFriendSocialV2Location(friendId)' "$game chat composer does not retain the location action"
    Require-Contains $chatInput 'showFriendSocialEmojiPicker()' "$game chat composer does not open an emoji picker"
    Require-Contains $chatInput 'sendFriendSocialChatDraft(conversation, friendId)' "$game Enter/send action does not submit the current draft"
    Require-Contains $panel 'FRIEND_SOCIAL_EMOJI_ACTION' "$game has no selectable emoji action"
    Require-Contains $panel 'appendFriendSocialChatExpression' "$game cannot append a selected renderable emoji expression"
    Require-Contains $composerPainter 'paintFriendSocialInputFrame' "$game composer does not use the shared focused input frame"
    Require-Contains $inputFrame 'input.isFocus' "$game shared input frame has no focused visual state"
    Require-Contains $inputFrame 'FRIEND_SOCIAL_COLOR_INPUT_FOCUS' "$game shared input frame does not change color while focused"
    Require-Contains $panel 'if (!Main.isPC)' "$game custom desktop input still lets TField consume the send button pointer"
    $translateIndex = $chatPainter.IndexOf('g.translate(0, cmy);', [System.StringComparison]::Ordinal)
    $clipIndex = $chatPainter.IndexOf('g.setClip(0, 0, GameCanvas.w, GameCanvas.h);', [System.StringComparison]::Ordinal)
    $composerIndex = $chatPainter.IndexOf('paintFriendSocialChatComposer', [System.StringComparison]::Ordinal)
    if ($translateIndex -lt 0 -or $clipIndex -lt $translateIndex -or $composerIndex -lt $clipIndex) {
        throw "$game chat composer is still painted with the message-area clip"
    }
    Require-Contains $main 'updateDesktopImeTextField' "$game does not bridge the Social V2 field through Unity IME"
    Require-Contains $main 'GUI.TextField' "$game has no native IME composition control"
    Require-Contains $main 'Input.imeCompositionMode = IMECompositionMode.On' "$game does not enable desktop IME composition"
    Require-Contains $main 'KeyCode.KeypadEnter' "$game does not support keypad Enter for quick send"
    Require-Contains $main 'GameCanvas.keyPressed[15] = true' "$game does not route Enter from the IME control to quick send"
    Require-Contains $canvas 'keyCode > 122 && keyCode <= char.MaxValue' "$game canvas rejects non-ASCII fallback text input"
}

Write-Output 'SOCIAL_V2_CHAT_COMPOSER_INPUT_OK'
