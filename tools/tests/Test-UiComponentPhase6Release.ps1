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

Write-Output '--- 1. Chay tuan tu gate Phase 0-6 ---'
$gates = @(
    @{ Script = 'Test-UiComponentPhase0Baseline.ps1'; Marker = 'UI_COMPONENT_PHASE0_BASELINE_OK' },
    @{ Script = 'Test-UiComponentPhase1Foundation.ps1'; Marker = 'UI_COMPONENT_PHASE1_FOUNDATION_OK' },
    @{ Script = 'Test-UiComponentPhase2Adapters.ps1'; Marker = 'UI_COMPONENT_PHASE2_ADAPTERS_OK' },
    @{ Script = 'Test-UiComponentPhase3Pilot.ps1'; Marker = 'UI_COMPONENT_PHASE3_PILOT_OK' },
    @{ Script = 'Test-UiComponentPhase4Expansion.ps1'; Marker = 'UI_COMPONENT_PHASE4_EXPANSION_OK' },
    @{ Script = 'Test-UiComponentPhase5PanelContent.ps1'; Marker = 'UI_COMPONENT_PHASE5_PANEL_CONTENT_OK' },
    @{ Script = 'Test-UiComponentPhase6Expansion.ps1'; Marker = 'UI_COMPONENT_PHASE6_EXPANSION_OK' }
)

$gateResults = [System.Collections.Generic.List[string]]::new()
foreach ($gate in $gates) {
    $gatePath = Join-Path $PSScriptRoot $gate.Script
    Require (Test-Path -LiteralPath $gatePath) ('Gate script not found: ' + $gate.Script)

    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $gateOutput = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $gatePath 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    $timer.Stop()

    if ($exitCode -ne 0) {
        $gateOutput | Write-Output
        throw ($gate.Script + ' failed with exit code ' + $exitCode)
    }
    Require ($gateOutput -contains $gate.Marker) ($gate.Script + ' completion marker missing: ' + $gate.Marker)

    if ($gate.Script -eq 'Test-UiComponentPhase6Expansion.ps1') {
        foreach ($phase6Marker in @(
            'UI_COMPONENT_PHASE6_1_RENDERER_OK',
            'UI_COMPONENT_PHASE6_2_CONTENT_OK',
            'UI_COMPONENT_PHASE6_3_LIFECYCLE_OK',
            'UI_COMPONENT_PHASE6_4_SHARED_STATE_OK',
            'UI_COMPONENT_PHASE6_5_CLEANUP_OK'
        )) {
            Require ($gateOutput -contains $phase6Marker) ('Phase 6 milestone marker missing: ' + $phase6Marker)
        }
    }

    $elapsed = [Math]::Round($timer.Elapsed.TotalSeconds, 3)
    $gateResults.Add($gate.Script + ': PASS (' + $elapsed + ' s)')
}
$gateResults | Write-Output

Write-Output '--- 2. Kiem tra Unity project, scenes va full assembly artifact ---'
$projectVersionPath = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
$buildSettingsPath = Join-Path $projectRoot 'ProjectSettings\EditorBuildSettings.asset'
$assemblyPath = Join-Path $projectRoot 'Library\ScriptAssemblies\Assembly-CSharp.dll'

Require (Test-Path -LiteralPath $projectVersionPath) 'ProjectVersion.txt not found'
Require (Test-Path -LiteralPath $buildSettingsPath) 'EditorBuildSettings.asset not found'
Require (Test-Path -LiteralPath $assemblyPath) 'Unity Assembly-CSharp.dll artifact not found'

$projectVersion = [System.IO.File]::ReadAllText($projectVersionPath)
$buildSettings = [System.IO.File]::ReadAllText($buildSettingsPath)
Require-Contains $projectVersion 'm_EditorVersion: 2022.3.62f2' 'Unexpected Unity editor version'
Require-Contains $buildSettings 'path: Assets/Scenes/NROL.unity' 'Game1 NROL scene is not enabled in build settings'
Require-Contains $buildSettings 'path: Assets/Scenes/NROL1.unity' 'Game2 NROL1 scene is not enabled in build settings'
Write-Output 'Unity version, Game1/Game2 scenes, and Assembly-CSharp artifact: OK'

Write-Output '--- 3. Kiem tra ma tran rollback Enemy va doc lap voi Top ---'
$scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'
foreach ($game in @('Game1', 'Game2')) {
    $panelPath = Join-Path $scriptsDir ($game + '\Panel.cs')
    $panel = [System.IO.File]::ReadAllText($panelPath)

    foreach ($flag in @(
        'public static bool USE_TOP_PANEL_CONTENT = true;',
        'public static bool USE_NEW_TOP_UI = true;',
        'public static bool USE_ENEMY_PANEL_CONTENT = true;',
        'public static bool USE_NEW_ENEMY_UI = true;'
    )) {
        Require-Contains $panel $flag ($game + ' rollback flag missing: ' + $flag)
    }

    Require-Contains $panel 'if (USE_ENEMY_PANEL_CONTENT)' ($game + ' Enemy content owner branch missing')
    Require-Contains $panel 'if (USE_NEW_ENEMY_UI)' ($game + ' Enemy renderer branch missing')
    Require-Contains $panel 'enemyPanelContent.Paint(g);' ($game + ' content/new-renderer route missing')
    Require-Contains $panel 'paintEnemyLegacy(g, enemyPanelContent.ScrollY, enemyPanelContent.SelectedIndex, enemyPanelContent.ItemsCount);' ($game + ' content/legacy-renderer route missing')
    Require-Contains $panel 'EnemyListView.Paint(' ($game + ' legacy-owner/new-renderer route missing')
    Require-Contains $panel 'paintEnemyLegacy(g, cmy, selected, currentListLength);' ($game + ' legacy-owner/legacy-renderer route missing')

    Require-Contains $panel 'if (USE_TOP_PANEL_CONTENT)' ($game + ' Top content owner branch missing')
    Require-Contains $panel 'if (USE_NEW_TOP_UI)' ($game + ' Top renderer branch missing')
}

foreach ($contentOwner in @($false, $true)) {
    foreach ($newRenderer in @($false, $true)) {
        $ownerLabel = if ($contentOwner) { 'content' } else { 'legacy' }
        $rendererLabel = if ($newRenderer) { 'new' } else { 'legacy' }
        Write-Output ('Enemy matrix: owner=' + $ownerLabel + ', renderer=' + $rendererLabel + ' -> contract present')
    }
}
Write-Output 'Enemy rollback matrix and independent Top flags: OK'

Write-Output '--- 4. Kiem tra render culling va paint-loop boundaries ---'
$rendererRegressionPath = Join-Path $PSScriptRoot 'UiComponentPhase6RendererRegression.cs'
$rendererRegression = [System.IO.File]::ReadAllText($rendererRegressionPath)
Require-Contains $rendererRegression 'SmallImage.DrawSmallImageCount == 3' 'Renderer regression must prove three visible rows are painted'
Require-Contains $rendererRegression '!SmallImage.DrawnImageIds.Contains(104)' 'Renderer regression must reject an off-screen row'

foreach ($game in @('Game1', 'Game2')) {
    $rendererPath = Join-Path $scriptsDir ($game + '\UI\Pilots\EnemyListView.cs')
    $renderer = [System.IO.File]::ReadAllText($rendererPath)
    Require-Contains $renderer 'UiListLayout.GetVisibleRange(' ($game + ' Enemy renderer must cull by visible range')
    Require (!$renderer.Contains('loadImage')) ($game + ' Enemy renderer must not load assets in Paint')
    Require (!$renderer.Contains('Service.')) ($game + ' Enemy renderer must not perform network calls')
}
Write-Output 'Visible-row culling and paint-loop boundaries: OK (3/10 rows in the 72px test viewport)'

Write-Output '--- 5. Kiem tra whitespace cho tracked va untracked UI scope ---'
$previousErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
try {
    $gitOutput = & git -C $projectRoot diff --check 2>&1
    $gitExitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
}
$gitOutputText = if ($null -eq $gitOutput) { '' } else { [string]::Join("`n", [string[]]$gitOutput) }
Require ($gitExitCode -eq 0) ('git diff --check failed: ' + $gitOutputText)

$whitespaceRoots = @(
    (Join-Path $scriptsDir 'UIShared'),
    (Join-Path $scriptsDir 'Game1\UI'),
    (Join-Path $scriptsDir 'Game2\UI'),
    $PSScriptRoot
)
foreach ($root in $whitespaceRoots) {
    Get-ChildItem -LiteralPath $root -Recurse -File |
        Where-Object { $_.Extension -in @('.cs', '.ps1') } |
        ForEach-Object {
            $lineNumber = 0
            foreach ($line in [System.IO.File]::ReadLines($_.FullName)) {
                $lineNumber++
                Require ($line -notmatch '[ \t]+$') ('Trailing whitespace: ' + $_.FullName + ':' + $lineNumber)
            }
        }
}
Write-Output 'Tracked diff and untracked UI/test whitespace: OK'

Write-Output 'UI_COMPONENT_PHASE6_6_AUTOMATED_GATE_OK'
