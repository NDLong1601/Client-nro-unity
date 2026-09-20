$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('ui-phase5-' + [guid]::NewGuid().ToString('N'))

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

try {
    Write-Output '--- 1. Kiem tra file production Phase 5 va Unity metadata ---'
    $scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'
    $uiSharedDir = Join-Path $scriptsDir 'UIShared'

    $expectedGameFiles = @(
        'UI\PanelContent\TopPanelContent.cs'
    )

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        Require (Test-Path -LiteralPath (Join-Path $gameDir 'UI\PanelContent.meta')) ($game + ' missing UI\PanelContent.meta')

        foreach ($relFile in $expectedGameFiles) {
            $fullPath = Join-Path $gameDir $relFile
            Require (Test-Path -LiteralPath $fullPath) ($game + ' missing file: ' + $relFile)
            Require (Test-Path -LiteralPath ($fullPath + '.meta')) ($game + ' missing meta: ' + $relFile + '.meta')

            $content = [System.IO.File]::ReadAllText($fullPath)
            Require-Contains $content ('namespace ' + $game + '.UI.PanelContent') ($relFile + ' must belong to ' + $game + '.UI.PanelContent namespace')
            Require (!$content.Contains('Service.gI')) ($fullPath + ' must not contain Service.gI')
            Require (!$content.Contains('loadImage(')) ($fullPath + ' must not call loadImage in view/component')
            Require (!$content.Contains('setClip(0, 0, GameCanvas.w, GameCanvas.h)')) ($fullPath + ' must not reset clip to fullscreen')
            Require (!$content.Contains('msg.reader()')) ($fullPath + ' must not parse network message')
            Require (!$content.Contains('Message msg')) ($fullPath + ' must not receive network message')
            Require (!$content.Contains('GameCanvas.panel2 =')) ($fullPath + ' must not mutate GameCanvas.panel2')
        }
    }
    Write-Output 'Production files, namespaces, metadata, and dependency guards: OK'

    Write-Output '--- 2. Kiem tra API parity Game1/Game2 ---'
    foreach ($relFile in $expectedGameFiles) {
        $path1 = Join-Path $scriptsDir ('Game1\' + $relFile)
        $path2 = Join-Path $scriptsDir ('Game2\' + $relFile)
        $content1 = [System.IO.File]::ReadAllText($path1)
        $content2 = [System.IO.File]::ReadAllText($path2)
        Require ($content1.Replace('Game1', 'Game2') -eq $content2) ('API parity mismatch in file: ' + $relFile)
    }
    Write-Output 'Game1/Game2 source parity: OK'

    Write-Output '--- 3. Kiem tra Panel.cs, Controller.cs, route mang va co rollback Phase 5 ---'
    foreach ($game in @('Game1', 'Game2')) {
        $panelPath = Join-Path $scriptsDir ($game + '\Panel.cs')
        Require (Test-Path -LiteralPath $panelPath) ($game + ' Panel.cs not found')
        $panel = [System.IO.File]::ReadAllText($panelPath)

        # Rollback flags
        Require-Contains $panel 'public static bool USE_TOP_PANEL_CONTENT' ($game + ' Panel missing USE_TOP_PANEL_CONTENT rollback flag')
        Require-Contains $panel 'public static bool USE_NEW_TOP_UI' ($game + ' Panel missing USE_NEW_TOP_UI rollback flag')

        # Top Ranking invariants
        Require-Contains $panel 'public void setTypeTop(sbyte t)' ($game + ' Panel missing setTypeTop')
        Require-Contains $panel 'public void setTabTop()' ($game + ' Panel missing setTabTop')
        Require-Contains $panel 'public void paintTop(mGraphics g)' ($game + ' Panel missing paintTop')
        Require-Contains $panel 'private void doFireTop()' ($game + ' Panel missing doFireTop')

        $paintTop = Get-MethodBody $panel 'public void paintTop(mGraphics g)'
        Require-Contains $paintTop 'USE_TOP_PANEL_CONTENT' ($game + ' paintTop must branch on USE_TOP_PANEL_CONTENT')
        Require-Contains $paintTop 'paintTopLegacy(g, topPanelContent.ScrollY, topPanelContent.SelectedIndex, topPanelContent.ItemsCount);' ($game + ' content path must honor the legacy renderer rollback with content-owned state')
        Require-Contains $paintTop 'paintScrollArrow(g, topPanelContent.ScrollY, topPanelContent.ScrollLimit, topPanelContent.ItemsCount);' ($game + ' content path must paint arrows from content-owned scroll state')

        Require-Contains $panel 'private void paintScrollArrow(mGraphics g, int scrollY, int scrollLimit, int itemCount)' ($game + ' Panel must expose a state-parameterized scroll-arrow renderer')
        Require-Contains $panel 'private bool isTopContentDragging()' ($game + ' Panel must expose the active Top content drag guard')
        Require-Contains $panel '&& !isTopContentDragging()' ($game + ' outside-close path must not close while Top content is dragging')
        Require-Contains $panel 'private bool isTopContentActive()' ($game + ' Panel must expose the Top content ownership guard')

        $moveCamera = Get-MethodBody $panel 'public void moveCamera()'
        Require-Contains $moveCamera 'if (!isTopContentActive())' ($game + ' moveCamera must not update legacy vertical list state while Top content owns it')

        $setType = Get-MethodBody $panel 'private void setType(int position)'
        Require-Contains $setType 'unbindTopContentWhenLeavingType();' ($game + ' changing to another panel type must unbind Top content and cancel its pending action')
        Require-Contains $panel 'private void unbindTopContentWhenLeavingType()' ($game + ' Panel must provide a centralized Top content type-transition cleanup')

        $setTypeTop = Get-MethodBody $panel 'public void setTypeTop(sbyte t)'
        Require-Contains $setTypeTop 'cancelPendingTopAction();' ($game + ' setTypeTop must cancel delayed actions from an older binding')

        $setTabTop = Get-MethodBody $panel 'public void setTabTop()'
        Require-Contains $setTabTop 'USE_TOP_PANEL_CONTENT' ($game + ' setTabTop must branch on USE_TOP_PANEL_CONTENT')
        Require-Contains $setTabTop 'cancelPendingTopAction();' ($game + ' setTabTop refresh must cancel delayed actions from the previous data revision')

        $executeTopAction = Get-MethodBody $panel 'private void executeTopAction(TopContentAction action)'
        Require-Contains $executeTopAction 'topPanelContent.IsActionCurrent(action)' ($game + ' executeTopAction must reject stale binding actions')

        $hide = Get-MethodBody $panel 'public void hide()'
        Require-Contains $hide 'Service.gI().sendThachDau(-1);' ($game + ' hide must send sendThachDau(-1) on type == 15')

        # Controller packet -96 check
        $controllerPath = Join-Path $scriptsDir ($game + '\Controller.cs')
        Require (Test-Path -LiteralPath $controllerPath) ($game + ' Controller.cs not found')
        $controller = [System.IO.File]::ReadAllText($controllerPath)
        Require-Contains $controller 'GameCanvas.panel.setTypeTop(typeTop);' ($game + ' Controller must dispatch packet -96 to setTypeTop')
        Require-Contains $controller 'GameCanvas.panel.show();' ($game + ' Controller must call show() on panel')
    }

    $panel1 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game1\Panel.cs'))
    $panel2 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game2\Panel.cs'))
    $topPanelMethods = @(
        'public void setTypeTop(sbyte t)',
        'public void setTabTop()',
        'public void paintTop(mGraphics g)',
        'private void paintTopComponent(mGraphics g)',
        'private void paintTopLegacy(mGraphics g)',
        'private void paintTopLegacy(mGraphics g, int scrollY, int selectedIndex, int itemCount)',
        'private void ensureTopPanelContent()',
        'private bool isTopContentActive()',
        'private bool isTopContentDragging()',
        'private void cancelPendingTopAction()',
        'private void unbindTopContentWhenLeavingType()',
        'private void updateKeyTopContent()',
        'private void executeTopAction(TopContentAction action)'
    )
    foreach ($signature in $topPanelMethods) {
        $body1 = (Get-MethodBody $panel1 $signature).Replace('Game1', 'Game2').Replace("`r`n", "`n")
        $body2 = (Get-MethodBody $panel2 $signature).Replace("`r`n", "`n")
        Require ($body1 -eq $body2) ('Game1/Game2 Top panel integration parity mismatch: ' + $signature)
    }
    Write-Output 'Scoped Top Panel.cs parity, Controller.cs contracts, and rollback flags: OK'

    Write-Output '--- 4. Bien dich va chay truc tiep bo test hanh vi Phase 5 ---'
    $regressionSource = Join-Path $PSScriptRoot 'UiComponentPhase5Regression.cs'
    $stubSource = Join-Path $PSScriptRoot 'UiComponentPhase5LegacyStubs.cs'
    Require (Test-Path -LiteralPath $regressionSource) 'UiComponentPhase5Regression.cs not found'
    Require (Test-Path -LiteralPath $stubSource) 'UiComponentPhase5LegacyStubs.cs not found'

    $uiSharedFiles = Get-ChildItem -LiteralPath $uiSharedDir -Filter '*.cs' | Select-Object -ExpandProperty FullName
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null

    $projectVersionPath = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
    $projectVersionContent = [System.IO.File]::ReadAllText($projectVersionPath)
    $versionMatch = [regex]::Match($projectVersionContent, 'm_EditorVersion:\s*([^\r\n]+)')
    Require $versionMatch.Success 'Unable to determine Unity editor version from ProjectVersion.txt'
    $unityVersion = $versionMatch.Groups[1].Value.Trim()
    $unityEditorRoot = Join-Path ${env:ProgramFiles} ('Unity\Hub\Editor\' + $unityVersion + '\Editor')
    $compilerPath = Join-Path $unityEditorRoot 'Data\DotNetSdkRoslyn\csc.dll'
    $monoPath = Join-Path $unityEditorRoot 'Data\MonoBleedingEdge\bin\mono.exe'
    $monoReferenceDir = Join-Path $unityEditorRoot 'Data\MonoBleedingEdge\lib\mono\4.8-api'
    $dotnetCommand = Get-Command dotnet -ErrorAction Stop

    Require (Test-Path -LiteralPath $compilerPath) ('Unity Roslyn compiler not found: ' + $compilerPath)
    Require (Test-Path -LiteralPath $monoPath) ('Unity Mono runtime not found: ' + $monoPath)

    $compilerReferences = @('mscorlib.dll', 'System.dll', 'System.Core.dll') | ForEach-Object {
        $referencePath = Join-Path $monoReferenceDir $_
        Require (Test-Path -LiteralPath $referencePath) ('Unity Mono reference not found: ' + $referencePath)
        '/reference:"' + $referencePath + '"'
    }

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        $productionSources = @(
            (Join-Path $gameDir 'UI\UiInputContext.cs'),
            (Join-Path $gameDir 'UI\Adapters\UiRenderState.cs'),
            (Join-Path $gameDir 'UI\Components\UiListRow.cs'),
            (Join-Path $gameDir 'UI\Pilots\TopRankingView.cs'),
            (Join-Path $gameDir 'UI\PanelContent\TopPanelContent.cs')
        )
        $gameStub = Join-Path $outputDirectory ($game + '-Phase5LegacyStubs.cs')
        $gameRegression = Join-Path $outputDirectory ($game + '-Phase5Regression.cs')

        $stubContent = [System.IO.File]::ReadAllText($stubSource)
        $regressionContent = [System.IO.File]::ReadAllText($regressionSource)
        if ($game -eq 'Game2') {
            $stubContent = $stubContent.Replace('Game1', 'Game2')
            $regressionContent = $regressionContent.Replace('Game1', 'Game2')
        }
        [System.IO.File]::WriteAllText($gameStub, $stubContent)
        [System.IO.File]::WriteAllText($gameRegression, $regressionContent)

        $outputExe = Join-Path $outputDirectory ($game + '-UiComponentPhase5Regression.exe')
        $compileSources = @($gameStub, $gameRegression) + $uiSharedFiles + $productionSources
        $responseFile = Join-Path $outputDirectory ($game + '-phase5-compile.rsp')
        $compilerArguments = @(
            '/nologo',
            '/nostdlib+',
            '/target:exe',
            '/langversion:9.0',
            '/warnaserror+',
            ('/out:"' + $outputExe + '"')
        ) + $compilerReferences + ($compileSources | ForEach-Object { '"' + $_ + '"' })
        [System.IO.File]::WriteAllLines($responseFile, $compilerArguments)

        $compilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $responseFile) 2>&1
        if ($LASTEXITCODE -ne 0) {
            $compilerOutput | Write-Output
            throw ($game + ' Phase 5 compilation failed with exit code: ' + $LASTEXITCODE)
        }
        Require (Test-Path -LiteralPath $outputExe) ($game + ' Phase 5 compilation did not generate executable')

        $testOutput = & $monoPath $outputExe
        if ($LASTEXITCODE -ne 0) {
            throw ($game + ' Phase 5 regression failed with exit code: ' + $LASTEXITCODE)
        }
        Require ($testOutput -contains ('UI_COMPONENT_PHASE5_PRODUCTION_' + $game + '_OK')) ($game + ' completion marker missing')
        Write-Output ($game + ' Phase 5 production source compilation and behavior regression: OK')
    }

    Write-Output '--- 5. Bien dich toan bo Assembly-CSharp bang Unity Roslyn ---'
    $unityResponseCandidates = Get-ChildItem -Path (Join-Path $projectRoot 'Library\Bee\artifacts') -Filter 'Assembly-CSharp.rsp' -Recurse |
        Sort-Object LastWriteTime -Descending
    $unityResponseFile = $null
    foreach ($candidate in $unityResponseCandidates) {
        $candidateContent = [System.IO.File]::ReadAllText($candidate.FullName)
        if ($candidateContent.Contains('-define:UNITY_EDITOR')) {
            $unityResponseFile = $candidate
            break
        }
    }
    Require ($null -ne $unityResponseFile) 'Unity Editor Assembly-CSharp response file was not found under Library/Bee/artifacts'

    $fullAssemblyPath = Join-Path $outputDirectory 'Assembly-CSharp-Phase5Verification.dll'
    $fullReferencePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase5Verification.ref.dll'
    $fullResponsePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase5Verification.rsp'

    # Append new Phase 5 sources to bee response if not present
    $newPhase5Sources = @(
        (Join-Path $scriptsDir 'UIShared\UiListRange.cs'),
        (Join-Path $scriptsDir 'UIShared\UiListLayout.cs'),
        (Join-Path $scriptsDir 'UIShared\UiVerticalListState.cs'),
        (Join-Path $scriptsDir 'Game1\UI\Adapters\TextFieldAdapter.cs'),
        (Join-Path $scriptsDir 'Game2\UI\Adapters\TextFieldAdapter.cs'),
        (Join-Path $scriptsDir 'Game1\UI\Components\UiListRow.cs'),
        (Join-Path $scriptsDir 'Game2\UI\Components\UiListRow.cs'),
        (Join-Path $scriptsDir 'Game1\UI\Pilots\TopRankingView.cs'),
        (Join-Path $scriptsDir 'Game2\UI\Pilots\TopRankingView.cs'),
        (Join-Path $scriptsDir 'Game1\UI\Pilots\PKHistoryView.cs'),
        (Join-Path $scriptsDir 'Game2\UI\Pilots\PKHistoryView.cs'),
        (Join-Path $scriptsDir 'Game1\UI\PanelContent\TopPanelContent.cs'),
        (Join-Path $scriptsDir 'Game2\UI\PanelContent\TopPanelContent.cs')
    )

    $existingRspLines = Get-Content -LiteralPath $unityResponseFile.FullName
    $fullResponseLines = [System.Collections.Generic.List[string]]::new()

    foreach ($line in $existingRspLines) {
        if ($line -match '^-out:') {
            $fullResponseLines.Add('-out:"' + $fullAssemblyPath + '"')
        }
        elseif ($line -match '^-refout:') {
            $fullResponseLines.Add('-refout:"' + $fullReferencePath + '"')
        }
        else {
            $fullResponseLines.Add($line)
        }
    }

    $normalizedExisting = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($line in $existingRspLines) {
        $clean = $line.Trim().Trim('"').Replace('/', '\')
        $normalizedExisting.Add($clean) | Out-Null
    }

    foreach ($source in $newPhase5Sources) {
        $cleanSource = $source.Trim().Trim('"').Replace('/', '\')
        if (!$normalizedExisting.Contains($cleanSource)) {
            $fullResponseLines.Add('"' + $source + '"')
        }
    }

    [System.IO.File]::WriteAllLines($fullResponsePath, $fullResponseLines)

    $fullCompilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $fullResponsePath) 2>&1
    if ($LASTEXITCODE -ne 0) {
        $fullCompilerOutput | Write-Output
        throw ('Full Assembly-CSharp compilation failed with exit code: ' + $LASTEXITCODE)
    }
    Require (Test-Path -LiteralPath $fullAssemblyPath) 'Full Assembly-CSharp compilation did not produce assembly'
    Write-Output 'Full Unity Assembly-CSharp compilation: OK'

    Write-Output 'UI_COMPONENT_PHASE5_PANEL_CONTENT_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        Remove-Item -LiteralPath $outputDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }
}
