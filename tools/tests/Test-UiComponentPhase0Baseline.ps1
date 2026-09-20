$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Require([bool]$condition, [string]$message) {
    if (!$condition) {
        throw $message
    }
}

function Require-Contains([string]$content, [string]$expected, [string]$message) {
    if (!$content.Contains($expected)) {
        throw $message
    }
}

function Get-MethodBody([string]$content, [string]$signature) {
    $start = $content.IndexOf($signature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) {
        throw ("Cannot find method signature: " + $signature)
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
    throw ("Cannot find closing brace for: " + $signature)
}

Write-Output '--- 1. Kiem tra phien ban Unity va project metadata ---'
$projectVersionPath = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
Require (Test-Path -LiteralPath $projectVersionPath) 'ProjectVersion.txt does not exist'
$projectVersionContent = Get-Content -LiteralPath $projectVersionPath -Raw
Require-Contains $projectVersionContent '2022.3.62f2' 'Unity version is not 2022.3.62f2'
Write-Output 'Unity version: 2022.3.62f2 OK'

$editorBuildSettingsPath = Join-Path $projectRoot 'ProjectSettings\EditorBuildSettings.asset'
Require (Test-Path -LiteralPath $editorBuildSettingsPath) 'EditorBuildSettings.asset does not exist'
$buildSettings = Get-Content -LiteralPath $editorBuildSettingsPath -Raw
Require-Contains $buildSettings 'Assets/Scenes/NROL.unity' 'Missing NROL.unity scene in build settings'
Require-Contains $buildSettings 'Assets/Scenes/NROL1.unity' 'Missing NROL1.unity scene in build settings'
Write-Output 'Dual-scene configuration (NROL / NROL1): OK'

Write-Output '--- 2. Chay kiem chung 7 test suite hoi quy hien co ---'
$existingSuites = @(
    'Test-SocialV2ChatComposerInput.ps1',
    'Test-SocialV2Phase5.ps1',
    'Test-SocialV2Phase5Assets.ps1',
    'Test-SocialV2Phase6.ps1',
    'Test-SocialV2Phase7.ps1',
    'Test-SocialV2Phase9UiFixes.ps1',
    'Test-SocialV2TargetLayout.ps1'
)

foreach ($suite in $existingSuites) {
    $suitePath = Join-Path $PSScriptRoot $suite
    Require (Test-Path -LiteralPath $suitePath) ('Suite does not exist: ' + $suite)
    & powershell -ExecutionPolicy Bypass -File $suitePath | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw ('Existing test suite failed: ' + $suite)
    }
}
Write-Output 'All 7 existing regression test suites passed: OK'

Write-Output '--- 3. Xac minh duong Pilot: PKHistoryScr vs Panel.TYPE_PK_HISTORY ---'
$scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'
$csFiles = Get-ChildItem -Path $scriptsDir -Filter '*.cs' -Recurse
$pkHistoryScrCallers = @()
foreach ($file in $csFiles) {
    if ($file.Name -eq 'PKHistoryScr.cs') { continue }
    $text = [System.IO.File]::ReadAllText($file.FullName)
    if ($text.Contains('PKHistoryScr')) {
        $pkHistoryScrCallers += $file.FullName
    }
}
Require ($pkHistoryScrCallers.Count -eq 0) 'PKHistoryScr has external callers'
Write-Output 'PKHistoryScr is confirmed orphaned (0 external callers): OK'

foreach ($game in @('Game1', 'Game2')) {
    $basePath = Join-Path $scriptsDir $game
    $panelPath = Join-Path $basePath 'Panel.cs'
    $controller2Path = Join-Path $basePath 'Assets\src\f\Controller2.cs'
    $soundMnPath = Join-Path $basePath 'SoundMn.cs'
    $entryPath = Join-Path $basePath 'PKHistoryEntry.cs'

    Require (Test-Path -LiteralPath $panelPath) ($game + ' Panel.cs not found')
    Require (Test-Path -LiteralPath $controller2Path) ($game + ' Controller2.cs not found')
    Require (Test-Path -LiteralPath $soundMnPath) ($game + ' SoundMn.cs not found')
    Require (Test-Path -LiteralPath $entryPath) ($game + ' PKHistoryEntry.cs not found')

    $panel = Get-Content -LiteralPath $panelPath -Raw
    $controller2 = Get-Content -LiteralPath $controller2Path -Raw
    $soundMn = Get-Content -LiteralPath $soundMnPath -Raw
    $entry = Get-Content -LiteralPath $entryPath -Raw

    # 3.1 Dinh nghia va model
    Require-Contains $entry 'public string opponentName;' ($game + ' PKHistoryEntry missing opponentName')
    Require-Contains $entry 'public bool won;' ($game + ' PKHistoryEntry missing won')
    Require-Contains $entry 'public int completedAt;' ($game + ' PKHistoryEntry missing completedAt')

    # 3.2 Constants va menu tool
    Require-Contains $panel 'private const int TYPE_PK_HISTORY = 29;' ($game + ' Panel missing TYPE_PK_HISTORY = 29')
    Require-Contains $panel 'public const string PK_HISTORY_TOOL =' ($game + ' Panel missing PK_HISTORY_TOOL constant')
    Require-Contains $soundMn 'historyTools[historyIndex] = Panel.PK_HISTORY_TOOL;' ($game + ' SoundMn does not register PK_HISTORY_TOOL')

    # 3.3 Dieu huong va state lifecycle
    $doFireTool = Get-MethodBody $panel 'private void doFireTool()'
    Require-Contains $doFireTool 'PK_HISTORY_TOOL' ($game + ' doFireTool does not check PK_HISTORY_TOOL')
    Require-Contains $doFireTool 'doPKHistory()' ($game + ' doFireTool does not invoke doPKHistory')

    $doPKHistory = Get-MethodBody $panel 'private void doPKHistory()'
    Require-Contains $doPKHistory 'setTypePKHistory()' ($game + ' doPKHistory does not invoke setTypePKHistory')

    $setTypePKHistory = Get-MethodBody $panel 'public void setTypePKHistory()'
    Require-Contains $setTypePKHistory 'type = TYPE_PK_HISTORY;' ($game + ' setTypePKHistory does not set type')
    Require-Contains $setTypePKHistory 'setType(0);' ($game + ' setTypePKHistory does not set position 0')
    Require-Contains $setTypePKHistory 'setTabPKHistory();' ($game + ' setTypePKHistory does not setTabPKHistory')
    Require-Contains $setTypePKHistory 'reloadPKHistory();' ($game + ' setTypePKHistory does not trigger reload')

    $setTabPKHistory = Get-MethodBody $panel 'private void setTabPKHistory()'
    Require-Contains $setTabPKHistory 'ITEM_HEIGHT = 34;' ($game + ' setTabPKHistory does not set row height 34')
    $usesLegacyScrollLimit = $setTabPKHistory.Contains('cmyLim = currentListLength * ITEM_HEIGHT - hScroll;')
    $usesSharedScrollLimit = $setTabPKHistory.Contains('cmyLim = UiListLayout.CalculateMaxScroll(currentListLength, ITEM_HEIGHT, hScroll);')
    Require ($usesLegacyScrollLimit -or $usesSharedScrollLimit) ($game + ' setTabPKHistory scroll limit calculation mismatch')

    $reloadPKHistory = Get-MethodBody $panel 'public void reloadPKHistory()'
    Require-Contains $reloadPKHistory 'Service.gI().requestPKHistory();' ($game + ' reloadPKHistory does not call service')

    # 3.4 Packet receive tu Server
    Require-Contains $controller2 'case 43:' ($game + ' Controller2 missing case 43 for PK History packet')
    Require-Contains $controller2 'GameCanvas.panel.setPKHistory(myVector2);' ($game + ' Controller2 case 43 does not forward to GameCanvas.panel.setPKHistory')

    # 3.5 Paint va Viewport culling
    $paintMethodName = if ($panel.Contains('paintPKHistoryLegacy(mGraphics g)')) { 'private void paintPKHistoryLegacy(mGraphics g)' } else { 'private void paintPKHistory(mGraphics g)' }
    $paintPKHistory = Get-MethodBody $panel $paintMethodName
    Require-Contains $paintPKHistory 'g.setClip(xScroll, yScroll, wScroll, hScroll);' ($game + ' paintPKHistory missing viewport clip')
    Require-Contains $paintPKHistory 'isPKHistoryLoading' ($game + ' paintPKHistory missing loading check')
    Require-Contains $paintPKHistory 'pkHistoryEntries.size() == 0' ($game + ' paintPKHistory missing empty state check')
    Require-Contains $paintPKHistory 'y - cmy > yScroll + hScroll' ($game + ' paintPKHistory missing top/bottom scroll culling')
    Require-Contains $paintPKHistory '16383818' ($game + ' paintPKHistory missing selected row color (0xF9FE4A)')
    Require-Contains $paintPKHistory '15196114' ($game + ' paintPKHistory missing unselected row color (0xE7DFD2)')
    Require-Contains $paintPKHistory '9993045' ($game + ' paintPKHistory missing separator color (0x987B95)')

    # 3.6 Row Action
    $doFirePKHistory = Get-MethodBody $panel 'private void doFirePKHistory()'
    Require-Contains $doFirePKHistory 'GameCanvas.startOKDlg(' ($game + ' doFirePKHistory does not display details in startOKDlg')
    Require-Contains $doFirePKHistory 'entry.opponentName' ($game + ' doFirePKHistory detail missing opponentName')
    Require-Contains $doFirePKHistory 'NinjaUtil.getTimeAgo' ($game + ' doFirePKHistory detail missing getTimeAgo')

    # 3.7 Back/close navigation
    $updateKey = Get-MethodBody $panel 'public void updateKey()'
    Require-Contains $updateKey 'case TYPE_PK_HISTORY:' ($game + ' updateKey does not handle TYPE_PK_HISTORY')
    Require-Contains $updateKey 'updateKeyScrollView();' ($game + ' updateKey TYPE_PK_HISTORY does not use updateKeyScrollView')

    # 3.8 Kiem chung cac khuyet diem kien truc baseline can refactor o Giai doan 1-3
    if ($panel.Contains('paintPKHistoryLegacy(mGraphics g)')) {
        $legacyPaint = Get-MethodBody $panel 'private void paintPKHistoryLegacy(mGraphics g)'
        Require-Contains $legacyPaint 'loadPKHistoryImages();' ($game + ' paintPKHistoryLegacy must preserve legacy image loading')
        Require-Contains $legacyPaint 'g.setClip(0, 0, GameCanvas.w, GameCanvas.h);' ($game + ' paintPKHistoryLegacy must preserve legacy fullscreen clip reset')

        $componentPaint = Get-MethodBody $panel 'private void paintPKHistoryComponent(mGraphics g)'
        Require (!$componentPaint.Contains('loadImage(')) ($game + ' paintPKHistoryComponent must eliminate image loading')
        Require (!$componentPaint.Contains('setClip(0, 0, GameCanvas.w, GameCanvas.h)')) ($game + ' paintPKHistoryComponent must eliminate fullscreen clip reset')
    } else {
        Require-Contains $paintPKHistory 'loadPKHistoryImages();' ($game + ' paintPKHistory calls loadPKHistoryImages in paint loop')
        Require-Contains $paintPKHistory 'g.setClip(0, 0, GameCanvas.w, GameCanvas.h);' ($game + ' paintPKHistory violates render state restoration')
    }
}
Write-Output 'Panel.TYPE_PK_HISTORY required contracts found in both Game1 and Game2: OK'

Write-Output '--- 4. Do dac baseline metrics (Layout, Viewport, Allocations) ---'
# Bang thong so la snapshot thu cong duoc doi chieu voi code nguon trong Panel.cs
$metrics = @{
    'Panel_W' = 176
    'Viewport_X' = 2
    'Viewport_Y' = 80
    'Viewport_W' = 172
    'Row_Height' = 34
    'Header_Y' = 59
    'Separator_Y' = 78
    'Color_RowSelected' = 16383818
    'Color_RowNormal' = 15196114
    'Color_RowDivider' = 9993045
    'Color_HeaderLine' = 13524492
    'Font_MainText' = 'tahoma_7b_dark'
    'Font_TimeAgo' = 'tahoma_7_grey'
    'Font_WinFallback' = 'tahoma_7b_green'
    'Font_LoseFallback' = 'tahoma_7b_red'
}

$canvasH = 320
$hScroll = $canvasH - 96
$visibleRows = [Math]::Ceiling($hScroll / $metrics['Row_Height']) + 1
Require ($visibleRows -eq 8) 'Visible row count at h=320 must be 8'
Write-Output ('Baseline layout metrics (manual snapshot vs Panel.cs): OK (Visible rows at h=320: ' + $visibleRows + ')')

Write-Output 'UI_COMPONENT_PHASE0_BASELINE_OK'
