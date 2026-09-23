$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$variants = @('Game1', 'Game2')

function Assert-Contains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -notmatch $Pattern) {
        throw $Message
    }
}

function Assert-NotContains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -match $Pattern) {
        throw $Message
    }
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
    $menuDir = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu"
    $controllerPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\Controller.cs"
    $mainPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\Main.cs"
    $canvasPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\GameCanvas.cs"
    $menu = (Get-ChildItem -LiteralPath $menuDir -Filter 'CustomMenuScr*.cs' -File |
        Sort-Object Name | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 }) -join "`n"
    $controller = Get-Content -LiteralPath $controllerPath -Raw
    $main = Get-Content -LiteralPath $mainPath -Raw
    $canvas = Get-Content -LiteralPath $canvasPath -Raw

    Assert-Contains $menu 'MaxFrameWidth\s*=\s*460' "$variant custom menu must preserve the 460 logical-pixel reference width."
    Assert-Contains $menu 'MaxFrameHeight\s*=\s*242' "$variant custom menu must match the 242 logical-pixel reference height."
    Assert-Contains $menu 'SidebarWidth\s*=\s*70' "$variant custom menu must use the 70px reference sidebar."
    Assert-Contains $menu '_footerRect\s*=\s*new UiRect\(contentX' "$variant footer must start after the sidebar."
    Assert-Contains $menu '_tabBarRect\s*=\s*new UiRect\(frameX,\s*frameY,\s*SidebarWidth,\s*frameH\)' "$variant sidebar must span the whole frame height."
    Assert-Contains $menu 'int columnWidth\s*=\s*\(contentW\s*-\s*12\)\s*/\s*2' "$variant task columns must use the same width."

    $leftViewport = Get-MethodBody $menu 'private UiRect GetLeftListViewport'
    Assert-NotContains $leftViewport 'MainTaskRowHeight\s*\*\s*5' "$variant main-task viewport must fill the left column instead of stopping after five rows."

    $taskList = Get-MethodBody $menu 'private void PaintTaskListColumn'
    Assert-Contains $taskList 'isSelected\s*\?\s*mFont\.tahoma_7b_dark\s*:\s*mFont\.tahoma_7_orange' "$variant selected active-task status must use a dark high-contrast font on the yellow background."

    Assert-Contains $menu 'public static bool IsOpen' "$variant custom menu must expose overlay state without replacing the active game screen."
    $openMenu = Get-MethodBody $menu 'public static void Open'
    Assert-NotContains $openMenu 'switchToMe\(' "$variant custom menu must open as an overlay instead of replacing GameCanvas.currentScreen."
    $closeMenu = Get-MethodBody $menu 'public void Close'
    Assert-NotContains $closeMenu 'switchToMe\(' "$variant custom menu must close without switching or reloading the game screen."
    $paintMenu = Get-MethodBody $menu 'public override void paint'
    Assert-NotContains $paintMenu '_previousScreen\.paint\(' "$variant overlay must not repaint the game screen a second time."
    Assert-Contains $canvas 'CustomMenuScr\.UpdateOverlay\(\)' "$variant GameCanvas must update custom-menu input while the game screen keeps updating."
    Assert-Contains $canvas '(?s)CustomMenuScr\.UpdateOverlay\(\).*?currentScreen\.update\(\)' "$variant game-world update must continue after custom-menu overlay update."
    Assert-Contains $canvas '!.*CustomMenuScr\.IsOpen' "$variant gameplay updateKey must be blocked while the custom menu owns input."
    Assert-Contains $canvas 'CustomMenuScr\.PaintOverlay\(g\)' "$variant GameCanvas must paint the custom menu as an overlay."

    $keyboardNavigation = Get-MethodBody $menu 'private bool HandleKeyboardNavigation'
    Assert-Contains $keyboardNavigation 'GameCanvas\.keyPressed\[23\].*GameCanvas\.keyPressed\[4\]' "$variant Left/legacy-4 must navigate to the previous tab."
    Assert-Contains $keyboardNavigation 'GameCanvas\.keyPressed\[24\].*GameCanvas\.keyPressed\[6\]' "$variant Right/legacy-6 must navigate to the next tab."
    Assert-Contains $keyboardNavigation 'GameCanvas\.keyPressed\[21\].*GameCanvas\.keyPressed\[2\]' "$variant Up/legacy-2 must navigate to the previous row."
    Assert-Contains $keyboardNavigation 'GameCanvas\.keyPressed\[22\].*GameCanvas\.keyPressed\[8\]' "$variant Down/legacy-8 must navigate to the next row."
    Assert-Contains $menu 'KeyboardFocusMainTabs' "$variant must distinguish the outer main-tab focus from inner content focus."
    Assert-Contains $menu 'KeyboardFocusContent' "$variant must retain row navigation inside the selected tab content."
    Assert-Contains $keyboardNavigation 'MoveHorizontalFocus\(-1\)' "$variant Left must move backward inside the current tab level."
    Assert-Contains $keyboardNavigation 'MoveHorizontalFocus\(1\)' "$variant Right must move forward inside the current tab level."
    Assert-Contains $keyboardNavigation 'MoveVerticalSelection\(-1\)' "$variant Up must navigate the active hierarchy level."
    Assert-Contains $keyboardNavigation 'MoveVerticalSelection\(1\)' "$variant Down must navigate the active hierarchy level."
    Assert-NotContains $menu 'int pageCount\s*=\s*MainTabCount\s*\+\s*1' "$variant must not flatten task subtabs and main tabs into one horizontal sequence."

    $horizontalNavigation = Get-MethodBody $menu 'private void MoveHorizontalFocus'
    Assert-Contains $horizontalNavigation '_selectedSubTab\s*<\s*1' "$variant Right may enter Other Tasks only from Main Tasks."
    Assert-Contains $horizontalNavigation '_selectedSubTab\s*>\s*0' "$variant Left must return from Other Tasks to Main Tasks."
    Assert-Contains $horizontalNavigation '_keyboardFocus\s*=\s*KeyboardFocusMainTabs' "$variant Left from the first inner tab must return focus to the outer main-tab bar."

    $verticalNavigation = Get-MethodBody $menu 'private void MoveVerticalSelection'
    Assert-Contains $verticalNavigation '_keyboardFocus\s*==\s*KeyboardFocusMainTabs' "$variant Up/Down must change outer main tabs after focus returns to the sidebar."
    Assert-Contains $verticalNavigation 'MoveRowSelection\(direction\)' "$variant Up/Down must continue changing rows while focus remains inside content."
    Assert-Contains $menu 'MoveRowSelection' "$variant must move the selected row in the active task page."
    Assert-Contains $menu 'OtherQuestNavigationOrder' "$variant other-task keyboard navigation must follow the visual group order."
    Assert-Contains $menu '_leftScrollAdapter\?\.ScrollToIndex\(_selectedTaskPosition\)' "$variant keyboard row navigation must keep the selected main task visible."

    Assert-Contains $menu 'BuildMainTaskSequence' "$variant must build a progression sequence instead of comparing raw task ids."
    Assert-Contains $menu '4\s*\+\s*gender' "$variant must collapse task ids 4/5/6 to the player gender branch."
    Assert-Contains $menu 'ScrollToIndex\(_selectedTaskPosition\)' "$variant must reveal the current task when the menu opens."
    Assert-NotContains $menu 'to\.taskId\s*==\s*0' "$variant must not misclassify legacy kill orders as daily quests."
    Assert-Contains $menu '(?s)text\.StartsWith\("Nhiệm vụ:"\).*?ApplyProgressMessage\(questClan' "$variant must route clan progress notifications to the clan task."
    Assert-Contains $menu 'ApplyProgressMessage\(questBoMong' "$variant must parse daily progress independently from clan progress."
    Assert-Contains $menu 'CanonicalTaskGuides' "$variant must provide guidance for completed and current main tasks."
    Assert-Contains $menu 'GetMainTaskName\(taskId\)' "$variant must render task names instead of generic task numbers."
    Assert-Contains $menu 'Vui lòng hoàn thành nhiệm vụ trước đó để xem' "$variant must hide locked-task details behind a clear message."

    Assert-Contains $menu '/custom_menu/tab_task\.png' "$variant must load the supplied task-tab icon."
    Assert-Contains $menu '/custom_menu/task_daily\.png' "$variant must load the supplied daily-task icon."
    Assert-Contains $menu '/custom_menu/status_tick\.png' "$variant must load the supplied completed-task icon."
    Assert-Contains $menu '/custom_menu/status_lock\.png' "$variant must load the supplied locked-task icon."
    Assert-Contains $menu 'Nhiệm vụ hằng ngày' "$variant must group daily quests as in the reference."
    Assert-Contains $menu 'Nhiệm vụ bang' "$variant must group guild quests as in the reference."
    Assert-Contains $menu 'Nhiệm vụ câu cá' "$variant must group fishing quests as in the reference."

    $otherDetails = Get-MethodBody $menu 'private void PaintOtherTaskDetailBody'
    Assert-NotContains $otherDetails 'Phần thưởng' "$variant other-task details must not show random rewards."
    Assert-Contains $otherDetails 'Hướng dẫn' "$variant other-task details must show player guidance."
    Assert-Contains $otherDetails 'defaultHint' "$variant other-task guidance must use location/NPC instructions."

    Assert-NotContains $controller 'OnReceiveThongBao\(thongBao\)' "$variant controller must not dispatch notifications twice."
    Assert-NotContains $controller 'OnReceiveNpcFishingDialog\(text\)' "$variant controller must not reinterpret every NPC dialog as fishing."
    Assert-Contains $main 'KeyCode\.F7[\s\S]*?CustomMenuScr\.Toggle\(\)' "$variant must toggle the custom menu with F7."
    Assert-NotContains $main '#if UNITY_EDITOR \|\| DEVELOPMENT_BUILD\s+if \(Event\.current[^\r\n]+KeyCode\.F7' "$variant F7 menu must remain available in non-development builds."
}

$assetRoot = Join-Path $root 'Assets\Resources\res\x4\custom_menu'
$requiredAssets = @(
    'tab_task.png', 'tab_inventory.png', 'tab_skill.png', 'tab_clan.png', 'tab_function.png',
    'task_daily.png', 'task_clan.png', 'task_fishing.png', 'task_infinity.png', 'task_other.png',
    'status_tick.png', 'status_lock.png'
)
foreach ($asset in $requiredAssets) {
    if (-not (Test-Path -LiteralPath (Join-Path $assetRoot $asset))) {
        throw "Missing custom-menu asset: $asset"
    }
}

Write-Host 'Custom menu task-tab regression checks passed.'
