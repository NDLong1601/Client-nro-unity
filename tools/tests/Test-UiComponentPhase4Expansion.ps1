$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('ui-phase4-' + [guid]::NewGuid().ToString('N'))

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
    Write-Output '--- 1. Kiem tra file production Phase 4 va Unity metadata ---'
    $scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'

    # 1.1 UIShared files
    $uiSharedDir = Join-Path $scriptsDir 'UIShared'
    Require (Test-Path -LiteralPath (Join-Path $scriptsDir 'UIShared.meta')) 'UIShared.meta not found'
    $expectedSharedFiles = @(
        'UiRect.cs',
        'UiMetrics.cs',
        'UiColorTokens.cs',
        'UiLayout.cs',
        'UiInputType.cs',
        'TextAlign.cs',
        'UiListRange.cs',
        'UiListLayout.cs'
    )
    foreach ($file in $expectedSharedFiles) {
        $fullPath = Join-Path $uiSharedDir $file
        Require (Test-Path -LiteralPath $fullPath) ('UIShared missing file: ' + $file)
        Require (Test-Path -LiteralPath ($fullPath + '.meta')) ('UIShared missing meta: ' + $file + '.meta')

        $content = [System.IO.File]::ReadAllText($fullPath)
        Require-Contains $content 'namespace Nro.UI' ($file + ' must belong to namespace Nro.UI')
        Require (!$content.Contains('UnityEngine')) ($file + ' must not reference UnityEngine')
        Require (!$content.Contains('GameCanvas')) ($file + ' must not reference GameCanvas')
        Require (!$content.Contains('mGraphics')) ($file + ' must not reference mGraphics')
        Require (!$content.Contains('mFont')) ($file + ' must not reference mFont')
        Require (!$content.Contains('Command')) ($file + ' must not reference Command')
        Require (!$content.Contains('TField')) ($file + ' must not reference TField')
        Require (!$content.Contains('Service')) ($file + ' must not reference Service')
        Require (!$content.Contains('Panel')) ($file + ' must not reference Panel')
        Require (!$content.Contains('Game1')) ($file + ' must not reference Game1')
        Require (!$content.Contains('Game2')) ($file + ' must not reference Game2')
    }

    # 1.2 Game1 and Game2 files
    $expectedGameFiles = @(
        'UI\Adapters\TextFieldAdapter.cs',
        'UI\Components\UiListRow.cs',
        'UI\Pilots\TopRankingView.cs',
        'UI\Pilots\PKHistoryView.cs'
    )

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game

        foreach ($relFile in $expectedGameFiles) {
            $fullPath = Join-Path $gameDir $relFile
            Require (Test-Path -LiteralPath $fullPath) ($game + ' missing file: ' + $relFile)
            Require (Test-Path -LiteralPath ($fullPath + '.meta')) ($game + ' missing meta: ' + $relFile + '.meta')

            $content = [System.IO.File]::ReadAllText($fullPath)
            Require (!$content.Contains('Service.gI')) ($fullPath + ' must not contain Service.gI')
            Require (!$content.Contains('loadImage(')) ($fullPath + ' must not call loadImage in view/component')
            Require (!$content.Contains('setClip(0, 0, GameCanvas.w, GameCanvas.h)')) ($fullPath + ' must not reset clip to fullscreen')
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

    Write-Output '--- 3. Kiem tra PKHistoryView va TopRankingView cung dung primitive list chung ---'
    foreach ($game in @('Game1', 'Game2')) {
        $pkPath = Join-Path $scriptsDir ($game + '\UI\Pilots\PKHistoryView.cs')
        $topPath = Join-Path $scriptsDir ($game + '\UI\Pilots\TopRankingView.cs')
        $pkContent = [System.IO.File]::ReadAllText($pkPath)
        $topContent = [System.IO.File]::ReadAllText($topPath)

        Require-Contains $pkContent 'UiListLayout' ($game + ' PKHistoryView must use UiListLayout')
        Require-Contains $topContent 'UiListLayout' ($game + ' TopRankingView must use UiListLayout')
        Require-Contains $pkContent 'UiListLayout.GetRowBounds' ($game + ' PKHistoryView must use shared row bounds')
        Require-Contains $topContent 'UiListLayout.GetRowBounds' ($game + ' TopRankingView must use shared row bounds')
        Require-Contains $pkContent 'UiListRow.PaintStandardBackground' ($game + ' PKHistoryView must use UiListRow standard background')
        Require-Contains $topContent 'UiListRow.PaintRankingBackground' ($game + ' TopRankingView must use UiListRow ranking background')
    }
    Write-Output 'Shared list primitive used by both consumers: OK'

    Write-Output '--- 4. Kiem tra Panel.cs, route mang va co rollback Top & FriendSearch ---'
    foreach ($game in @('Game1', 'Game2')) {
        $panelPath = Join-Path $scriptsDir ($game + '\Panel.cs')
        Require (Test-Path -LiteralPath $panelPath) ($game + ' Panel.cs not found')
        $panel = [System.IO.File]::ReadAllText($panelPath)

        # Rollback flags
        Require-Contains $panel 'public static bool USE_NEW_TOP_UI' ($game + ' Panel missing USE_NEW_TOP_UI rollback flag')
        Require-Contains $panel 'public static bool USE_NEW_FRIEND_SEARCH_INPUT_UI' ($game + ' Panel missing USE_NEW_FRIEND_SEARCH_INPUT_UI rollback flag')

        # Top Ranking dispatch and legacy retention
        $paintTop = Get-MethodBody $panel 'public void paintTop(mGraphics g)'
        Require-Contains $paintTop 'USE_NEW_TOP_UI' ($game + ' paintTop must branch on USE_NEW_TOP_UI')
        Require-Contains $paintTop 'paintTopComponent' ($game + ' paintTop must call paintTopComponent')
        Require-Contains $paintTop 'paintTopLegacy' ($game + ' paintTop must call paintTopLegacy')

        $paintTopLegacy = Get-MethodBody $panel 'private void paintTopLegacy(mGraphics g)'
        Require-Contains $paintTopLegacy 'paintScrollArrow(g);' ($game + ' paintTopLegacy must retain paintScrollArrow')

        $paintTopComponent = Get-MethodBody $panel 'private void paintTopComponent(mGraphics g)'
        Require-Contains $paintTopComponent 'TopRankingView.Paint' ($game + ' paintTopComponent must delegate to TopRankingView.Paint')
        Require-Contains $paintTopComponent 'paintScrollArrow(g);' ($game + ' paintTopComponent must call paintScrollArrow')

        $setTabTop = Get-MethodBody $panel 'public void setTabTop()'
        Require-Contains $setTabTop 'UiListLayout.CalculateMaxScroll' ($game + ' setTabTop must use shared max-scroll calculation')

        $setTabPKHistory = Get-MethodBody $panel 'private void setTabPKHistory()'
        Require-Contains $setTabPKHistory 'UiListLayout.CalculateMaxScroll' ($game + ' setTabPKHistory must use shared max-scroll calculation')

        # Top Ranking invariants
        Require-Contains $panel 'public void setTypeTop(sbyte t)' ($game + ' Panel missing setTypeTop')
        Require-Contains $panel 'public void setTabTop()' ($game + ' Panel missing setTabTop')
        Require-Contains $panel 'private void doFireTop()' ($game + ' Panel missing doFireTop')

        $hide = Get-MethodBody $panel 'public void hide()'
        Require-Contains $hide 'Service.gI().sendThachDau(-1);' ($game + ' hide must send sendThachDau(-1) on type == 15')

        # Friend social search adapter integration
        Require-Contains $panel 'TextFieldAdapter' ($game + ' Panel must use TextFieldAdapter for friend search')
        Require-Contains $panel 'SOCIAL_V2_SEARCH_INPUT' ($game + ' Panel must retain SOCIAL_V2_SEARCH_INPUT')
    }
    Write-Output 'Panel.cs contracts, rollback flags, and renderer delegation: OK'

    Write-Output '--- 5. Bien dich va chay truc tiep bo test hanh vi Phase 4 ---'
    $regressionSource = Join-Path $PSScriptRoot 'UiComponentPhase4Regression.cs'
    $stubSource = Join-Path $PSScriptRoot 'UiComponentPhase4LegacyStubs.cs'
    Require (Test-Path -LiteralPath $regressionSource) 'UiComponentPhase4Regression.cs not found'
    Require (Test-Path -LiteralPath $stubSource) 'UiComponentPhase4LegacyStubs.cs not found'

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
            (Join-Path $gameDir 'UI\Adapters\UiRenderState.cs'),
            (Join-Path $gameDir 'UI\Adapters\TextFieldAdapter.cs'),
            (Join-Path $gameDir 'UI\Components\UiListRow.cs'),
            (Join-Path $gameDir 'UI\Pilots\PKHistoryView.cs'),
            (Join-Path $gameDir 'UI\Pilots\TopRankingView.cs')
        )
        $gameStub = Join-Path $outputDirectory ($game + '-Phase4LegacyStubs.cs')
        $gameRegression = Join-Path $outputDirectory ($game + '-Phase4Regression.cs')

        $stubContent = [System.IO.File]::ReadAllText($stubSource)
        $regressionContent = [System.IO.File]::ReadAllText($regressionSource)
        if ($game -eq 'Game2') {
            $stubContent = $stubContent.Replace('Game1', 'Game2')
            $regressionContent = $regressionContent.Replace('Game1', 'Game2')
        }
        [System.IO.File]::WriteAllText($gameStub, $stubContent)
        [System.IO.File]::WriteAllText($gameRegression, $regressionContent)

        $outputExe = Join-Path $outputDirectory ($game + '-UiComponentPhase4Regression.exe')
        $compileSources = @($gameStub, $gameRegression) + $uiSharedFiles + $productionSources
        $responseFile = Join-Path $outputDirectory ($game + '-phase4-compile.rsp')
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
            throw ($game + ' Phase 4 compilation failed with exit code: ' + $LASTEXITCODE)
        }
        Require (Test-Path -LiteralPath $outputExe) ($game + ' Phase 4 compilation did not generate executable')

        $testOutput = & $monoPath $outputExe
        if ($LASTEXITCODE -ne 0) {
            throw ($game + ' Phase 4 regression failed with exit code: ' + $LASTEXITCODE)
        }
        Require ($testOutput -contains ('UI_COMPONENT_PHASE4_PRODUCTION_' + $game + '_OK')) ($game + ' completion marker missing')
        Write-Output ($game + ' Phase 4 production source compilation and behavior regression: OK')
    }

    Write-Output '--- 6. Bien dich toan bo Assembly-CSharp bang Unity Roslyn ---'
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

    $fullAssemblyPath = Join-Path $outputDirectory 'Assembly-CSharp-Phase4Verification.dll'
    $fullReferencePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase4Verification.ref.dll'
    $fullResponsePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase4Verification.rsp'

    # Full list of new Phase 4 sources to append if not yet in Bee rsp
    $newPhase4Sources = @(
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

    foreach ($source in $newPhase4Sources) {
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

    Write-Output 'UI_COMPONENT_PHASE4_EXPANSION_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        Remove-Item -LiteralPath $outputDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }
}
