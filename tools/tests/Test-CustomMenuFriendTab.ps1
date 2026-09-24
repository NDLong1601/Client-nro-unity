$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')

function Require([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
}

foreach ($variant in @('Game1', 'Game2')) {
    $dir = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu"
    $core = Get-Content (Join-Path $dir 'CustomMenuScr.cs') -Raw -Encoding UTF8
    $chrome = Get-Content (Join-Path $dir 'CustomMenuScr.Chrome.cs') -Raw -Encoding UTF8
    $navigation = Get-Content (Join-Path $dir 'CustomMenuScr.Navigation.cs') -Raw -Encoding UTF8
    $function = Get-Content (Join-Path $dir 'CustomMenuScr.Function.cs') -Raw -Encoding UTF8
    $logic = Get-Content (Join-Path $dir 'CustomMenuScr.Friend.cs') -Raw -Encoding UTF8
    $view = Get-Content (Join-Path $dir 'CustomMenuScr.Friend.View.cs') -Raw -Encoding UTF8
    $controller = Get-Content (Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\Controller.cs") -Raw -Encoding UTF8

    Require ($core.Contains('MainTabCount = 8') -and $core.Contains('/custom_menu/main_friend.png')) "$variant missing the scrollable friend main tab."
    Require ($chrome.Contains('PaintFriendTabContent(g)') -and $navigation.Contains('HandleFriendConfirm()')) "$variant missing friend rendering or keyboard interaction."
    Require ($core.Contains('HandleFriendPointerInput()') -and $core.Contains('ConfigureFriendRects()')) "$variant missing friend layout or pointer interaction."
    Require ($controller.Contains("$variant.UI.CustomMenu.CustomMenuScr.ConsumeFriendListResponseForCustomMenu()")) "$variant must keep the legacy friend panel from covering the new tab."
    Require ($function.Contains('SwitchToFriendTab()')) "$variant account menu must open the new friend tab."
    foreach ($call in @('Service.gI().friend(0, -1)', 'Service.gI().searchFriendSocialV2(',
        'Service.gI().loadFriendSocialV2Inbox(', 'Service.gI().sendFriendSocialV2Request(',
        'Service.gI().acceptFriendSocialV2Request(', 'Service.gI().rejectFriendSocialV2Request(',
        'Service.gI().sendFriendSocialV2Chat(', 'Service.gI().shareFriendSocialV2Location(')) {
        Require ($logic.Contains($call)) "$variant missing live social action: $call"
    }
    foreach ($model in @('vFriend', 'Search.Results', 'Inbox.Results', 'Conversations.TryPeek')) {
        Require ($view.Contains($model) -or $logic.Contains($model)) "$variant missing live social data: $model"
    }
    Require ($view.Contains('UiMenuTheme.PaintHeader') -and $view.Contains('PaintFriendAvatar') -and
        $view.Contains('PaintFriendChatMessage')) "$variant missing the two-column friend and chat layout."
    foreach ($name in @('CustomMenuScr.Friend.cs', 'CustomMenuScr.Friend.View.cs')) {
        Require (Test-Path (Join-Path $dir ($name + '.meta'))) "$variant missing Unity metadata for $name"
    }
}

$icon = Join-Path $root 'Assets\Resources\res\x4\custom_menu\main_friend.png'
Require (Test-Path $icon) 'Missing supplied friend main-tab icon.'
Require ((Get-Content ($icon + '.meta') -Raw -Encoding UTF8).Contains('maxTextureSize: 256')) 'Friend icon import size must be bounded.'
$game1 = Join-Path $root 'Assets\Scripts\Assembly-CSharp\Game1\UI\CustomMenu'
$game2 = Join-Path $root 'Assets\Scripts\Assembly-CSharp\Game2\UI\CustomMenu'
foreach ($name in @('CustomMenuScr.cs', 'CustomMenuScr.Chrome.cs', 'CustomMenuScr.Navigation.cs',
    'CustomMenuScr.Function.cs', 'CustomMenuScr.Friend.cs', 'CustomMenuScr.Friend.View.cs')) {
    $a = Get-Content (Join-Path $game1 $name) -Raw -Encoding UTF8
    $b = Get-Content (Join-Path $game2 $name) -Raw -Encoding UTF8
    Require ($a.Replace('Game1', 'Game2') -eq $b) "Game1/Game2 parity mismatch: $name"
}

Write-Host 'Custom menu friend-tab regression checks passed.'
