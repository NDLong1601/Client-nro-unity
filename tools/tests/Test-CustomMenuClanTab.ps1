$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$variants = @('Game1', 'Game2')

function Assert-Contains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -notmatch $Pattern) { throw $Message }
}

function Assert-NotContains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -match $Pattern) { throw $Message }
}

function Get-MethodBody {
    param([string]$Text, [string]$Signature)
    $start = $Text.IndexOf($Signature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Cannot find method: $Signature" }
    $open = $Text.IndexOf('{', $start)
    $depth = 0
    for ($i = $open; $i -lt $Text.Length; $i++) {
        if ($Text[$i] -eq '{') { $depth++ }
        elseif ($Text[$i] -eq '}') {
            $depth--
            if ($depth -eq 0) { return $Text.Substring($open, $i - $open + 1) }
        }
    }
    throw "Cannot find method end: $Signature"
}

foreach ($variant in $variants) {
    $menuPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu\CustomMenuScr.cs"
    $menu = Get-Content -LiteralPath $menuPath -Raw -Encoding UTF8

    $adapterPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\Adapters\ScrollViewAdapter.cs"
    $adapter = Get-Content -LiteralPath $adapterPath -Raw -Encoding UTF8
    $wheelScroll = Get-MethodBody $adapter 'public bool ScrollByWheel'
    Assert-Contains $wheelScroll '_scroll\.cmtoY' "$variant mouse-wheel scrolling must animate toward the adapter target."
    Assert-Contains $wheelScroll 'wheelDelta' "$variant mouse-wheel scrolling must respect wheel direction."

    $wheelRoute = Get-MethodBody $menu 'public static bool HandleMouseWheel'
    Assert-Contains $wheelRoute '_leftScrollAdapter' "$variant custom menu must route the wheel to its left list."
    Assert-Contains $wheelRoute '_rightScrollAdapter' "$variant custom menu must route the wheel to its right list."

    $layout = Get-MethodBody $menu 'private void ConfigureClanRects'
    Assert-Contains $layout 'sideWidth\s*=\s*54' "$variant clan side actions must use the compact width."
    Assert-Contains $layout 'sideHeight\s*=\s*23' "$variant clan side actions must use the compact height."

    Assert-Contains $menu 'ClanViewMembers\s*=\s*0' "$variant clan tab must default to the member view."
    Assert-Contains $menu 'ClanViewInfo\s*=\s*1' "$variant clan tab must expose the information view."
    Assert-Contains $menu 'ClanViewTreasury\s*=\s*2' "$variant clan tab must expose the treasury view."
    Assert-Contains $menu 'ClanViewPotential\s*=\s*3' "$variant clan tab must expose the potential view."
    Assert-Contains $menu 'ClanViewUpgrade\s*=\s*4' "$variant clan tab must expose the upgrade view."
    Assert-Contains $menu 'ClanViewHistory\s*=\s*5' "$variant clan tab must expose the contribution history view."

    $paint = Get-MethodBody $menu 'public override void paint'
    Assert-Contains $paint '_selectedMainTab\s*==\s*3[\s\S]*?PaintClanTabContent\(g\)' "$variant clan tab must be wired into the custom-menu renderer."

    $clanPaint = Get-MethodBody $menu 'private void PaintClanTabContent'
    Assert-Contains $clanPaint 'PaintClanChatColumn\(g\)' "$variant clan tab must retain the chat column in every joined-clan view."
    Assert-Contains $clanPaint 'PaintClanFunctionTabs\(g\)' "$variant normal clan views must show the six function buttons."
    Assert-Contains $clanPaint 'PaintClanHistory\(g\)' "$variant history must use its dedicated layout."
    Assert-Contains $clanPaint 'PaintClanSideActions\(g\)' "$variant clan tab must expose context actions outside the content columns."

    $refresh = Get-MethodBody $menu 'private void RefreshClanData'
    Assert-Contains $refresh 'ClanProgression\.requestSnapshot' "$variant clan tab must refresh progression through the original clan flow."
    Assert-Contains $refresh 'ClanProgression\.requestBuffSnapshot' "$variant clan tab must refresh live clan buffs."
    Assert-Contains $refresh 'Service\.gI\(\)\.clanTreasuryView\(\)' "$variant clan tab must refresh treasury data through the original service."
    Assert-Contains $refresh 'ClanValue\.requestSnapshot' "$variant clan information must use the authoritative Clan Value snapshot."
    Assert-Contains $refresh 'ClanAppearance\.requestSnapshot' "$variant clan information must use the authoritative appearance snapshot."

    $chat = Get-MethodBody $menu 'private void PaintClanChatMessages'
    Assert-Contains $chat 'ClanMessage\.vMessage' "$variant chat column must render live clan messages."
    Assert-Contains $chat 'FindClanMember' "$variant chat avatars must resolve against the live clan-member collection."

    $members = Get-MethodBody $menu 'private static MyVector GetClanMembers'
    Assert-Contains $members 'GameCanvas\.panel\.myMember' "$variant member view must use the collection populated by the original panel flow."

    $info = Get-MethodBody $menu 'private static List<string> BuildClanInfoLines'
    Assert-Contains $info 'Char\.myCharz\(\)\.clan' "$variant clan information must come from the live character clan."
    Assert-Contains $info 'ClanTreasury\.current' "$variant clan information must use live treasury balances."
    Assert-Contains $info 'ClanProgression\.buffStatusText' "$variant clan information must use live buff status and remaining time."
    Assert-Contains $info 'ClanValue\.current' "$variant clan information must expose server-calculated Clan Value."
    Assert-Contains $info 'spentPotentialScore' "$variant clan information must include spent clan potential value."
    Assert-Contains $info 'achievementScore' "$variant clan information must include achievement value."
    Assert-Contains $info 'weeklyActivityScore' "$variant clan information must include weekly activity value."
    Assert-Contains $info 'ClanAppearance\.current' "$variant clan information must include the live clan-tree appearance tier."
    Assert-Contains $info 'ClanProgression\.effectPercentText' "$variant clan information must include every live potential attribute."

    $paintInfo = Get-MethodBody $menu 'private void PaintClanInfo'
    Assert-Contains $paintInfo 'GetClanScrollableBodyRect\(\)' "$variant clan information must paint inside the same inset viewport used to calculate its scroll limit."
    Assert-Contains $paintInfo 'GetClanBuffDisplayType' "$variant clan buff rows must preserve the original panel ordering."
    Assert-Contains $paintInfo 'GetClanBuffColor' "$variant active clan buffs must use their original individual colors."
    Assert-Contains $paintInfo 'PaintClanBuffStatus' "$variant clan buff rows must use the single-pass contrast renderer."

    $paintBuff = Get-MethodBody $menu 'private static void PaintClanBuffStatus'
    Assert-Contains $paintBuff 'NeedsClanBuffContrastBackground' "$variant white and yellow clan buffs must receive a contrast background."
    Assert-Contains $paintBuff 'fillRect' "$variant bright clan buffs must use a background instead of overlapping shadow text."
    Assert-Contains $paintBuff 'drawStringColor' "$variant active clan buffs must render with their original explicit colors."
    Assert-NotContains $paintBuff 'tahoma_7b_dark\.drawString' "$variant clan buffs must not draw a second mismatched font over the colored text."

    $scrollBody = Get-MethodBody $menu 'private UiRect GetClanScrollableBodyRect'
    Assert-Contains $scrollBody '_selectedClanView\s*==\s*ClanViewInfo' "$variant clan information must reserve inner padding in its scroll viewport."

    $paintDialog = Get-MethodBody $menu 'private void PaintClanDialog'
    Assert-Contains $paintDialog 'GetClanDialogAccent' "$variant contribution and slogan dialogs must use a mode-specific visual accent."
    Assert-Contains $paintDialog 'PaintClanDialogIcon' "$variant contribution dialogs must identify the selected currency visually."

    $potential = Get-MethodBody $menu 'private void PaintClanPotential'
    Assert-Contains $potential 'ClanProgression\.BRANCH_COUNT' "$variant potential view must render every server-defined branch."
    Assert-Contains $potential 'ClanProgression\.effectText' "$variant potential rows must use the original progression formulas."
    Assert-Contains $potential 'ClanProgression\.current\.unspentPoints' "$variant potential view must show the live number of points still available."

    $activate = Get-MethodBody $menu 'private void ActivateClanContent'
    Assert-Contains $activate 'ClanProgression\.allocate' "$variant potential plus actions must use the original allocation request."
    Assert-Contains $activate 'ClanProgression\.REQUEST_UPGRADE' "$variant upgrade action must use the original progression request id."
    Assert-Contains $activate 'Service\.gI\(\)\.clanItemStorageUse' "$variant clan-storage items must use the original shared-item protocol."

    $treasury = Get-MethodBody $menu 'private void OpenClanTreasury'
    Assert-Contains $treasury 'Service\.gI\(\)\.clanItemStorageView\(\)' "$variant treasury must request the authoritative shared storage payload."

    $storage = Get-MethodBody $menu 'private void PaintClanTreasury'
    Assert-Contains $storage 'Char\.myCharz\(\)\.arrItemBox' "$variant treasury grid must render the payload used by the original clan box."

    $dialog = Get-MethodBody $menu 'private void SubmitClanDialog'
    Assert-Contains $dialog 'Service\.gI\(\)\.clanTreasuryDeposit' "$variant contribution dialog must submit through the original treasury service."
    Assert-Contains $dialog 'Service\.gI\(\)\.getClan\(4' "$variant slogan dialog must submit through the original clan service."

    $sideAction = Get-MethodBody $menu 'private void ActivateClanSideAction'
    Assert-Contains $sideAction 'Service\.gI\(\)\.leaveClan\(\)' "$variant leave button must use the original clan flow."
    Assert-Contains $sideAction 'OpenClanIconPicker\(\)' "$variant icon button must expose the original clan-icon picker."
    Assert-Contains $sideAction 'Service\.gI\(\)\.clanTreasuryLedger\(0L\)' "$variant history button must refresh the original contribution ledger."

    $iconPicker = Get-MethodBody $menu 'private void OpenClanIconPicker'
    Assert-Contains $iconPicker 'Close\(\)' "$variant icon picker must move above CustomMenu instead of opening behind it."
    Assert-Contains $iconPicker 'currentTabIndex\s*=\s*3' "$variant icon picker must return to the legacy clan panel."
    Assert-Contains $iconPicker 'Service\.gI\(\)\.getClan\(3' "$variant icon picker must request the authoritative icon list."

    $ownMessage = Get-MethodBody $menu 'private static bool IsOwnClanMessage'
    Assert-Contains $ownMessage 'message\.playerId\s*==\s*me\.charID' "$variant clan-message ownership must use the live character id."
    Assert-Contains $ownMessage 'message\.playerName' "$variant clan-message ownership must fall back to the stable character name after relog."

    $visibleOptions = Get-MethodBody $menu 'private static int GetVisibleClanMessageOptionCount'
    Assert-Contains $visibleOptions 'IsOwnClanMessage' "$variant self bean requests must never expose a donate button."
    Assert-Contains $visibleOptions 'message\.recieve\s*>=\s*message\.maxCap' "$variant completed bean requests must not expose stale actions."

    $activateMessage = Get-MethodBody $menu 'private void ActivateClanMessage'
    Assert-Contains $activateMessage 'GetVisibleClanMessageOptionCount' "$variant clan-message clicks must use the same eligibility rule as painting."
    Assert-Contains $activateMessage 'Service\.gI\(\)\.clanDonate' "$variant eligible bean requests must keep the original donate flow."

    $backspace = Get-MethodBody $menu 'private static bool HandleClanTextBackspace'
    Assert-Contains $backspace 'GameCanvas\.keyPressed\[14\]' "$variant clan text fields must consume the PC Backspace mapping."
    Assert-Contains $backspace 'field\.keyPressed\(-8\)' "$variant clan text fields must delete through TField's native edit path."

    $clanInput = Get-MethodBody $menu 'private bool HandleClanPointerInput'
    Assert-Contains $clanInput 'HandleClanTextBackspace\(_clanChatField' "$variant clan chat must support Backspace."
    $dialogInput = Get-MethodBody $menu 'private void HandleClanDialogInput'
    Assert-Contains $dialogInput 'HandleClanTextBackspace\(_clanDialogField' "$variant contribution and slogan dialogs must support Backspace."

    $init = Get-MethodBody $menu 'private void Init'
    Assert-Contains $menu '_hasInitializedState' "$variant custom menu must retain navigation state across close and reopen."
    Assert-Contains $init 'if\s*\(!_hasInitializedState\)' "$variant custom menu defaults must only be applied on the first open."
    Assert-Contains $init '_selectedMainTab\s*==\s*3[\s\S]*?RefreshClanData\(\)' "$variant reopened clan tab must refresh live data without changing the selected view."

    $controllerPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\Controller.cs"
    $controller = Get-Content -LiteralPath $controllerPath -Raw -Encoding UTF8
    Assert-Contains $controller "${variant}\.UI\.CustomMenu\.CustomMenuScr\.TryConsumeClanStorageOpen\(\)" "$variant controller must keep clan storage inside CustomMenu instead of opening the legacy panel."
    $readClanMessage = Get-MethodBody $controller 'public void readClanMsg'
    Assert-Contains $readClanMessage 'clanMessage\.playerName[\s\S]*?currentPlayer\.cName' "$variant clan-message parsing must recognize self requests by stable character name after relog."
    Assert-Contains $readClanMessage 'StringComparison\.OrdinalIgnoreCase' "$variant clan-message ownership fallback must not depend on name casing."

    $canvasPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\GameCanvas.cs"
    $canvas = Get-Content -LiteralPath $canvasPath -Raw -Encoding UTF8
    $canvasWheel = Get-MethodBody $canvas 'public void scrollMouse'
    Assert-Contains $canvasWheel "${variant}\.UI\.CustomMenu\.CustomMenuScr\.HandleMouseWheel\(a\)" "$variant game canvas must offer mouse-wheel input to the open custom menu first."
    $canvasPaint = Get-MethodBody $canvas 'public void paint(mGraphics gx)'
    Assert-Contains $canvasPaint "isPKHistoryOpen\)\s*&& !${variant}\.UI\.CustomMenu\.CustomMenuScr\.IsOpen" "$variant global tab-switch control must stay behind the open custom menu."
}

Write-Host 'Custom menu clan-tab regression checks passed.'
