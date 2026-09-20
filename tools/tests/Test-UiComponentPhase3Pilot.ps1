$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('ui-phase3-' + [guid]::NewGuid().ToString('N'))

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
    Write-Output '--- 1. Kiem tra file production Phase 3 va Unity metadata ---'
    $scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'
    $expectedRelativeFiles = @(
        'UI\Pilots\PKHistoryView.cs'
    )

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        Require (Test-Path -LiteralPath (Join-Path $gameDir 'UI\Pilots.meta')) ($game + ' missing Pilots.meta')

        foreach ($relFile in $expectedRelativeFiles) {
            $fullPath = Join-Path $gameDir $relFile
            Require (Test-Path -LiteralPath $fullPath) ($game + ' missing file: ' + $relFile)
            Require (Test-Path -LiteralPath ($fullPath + '.meta')) ($game + ' missing Unity metadata: ' + $relFile + '.meta')

            $content = [System.IO.File]::ReadAllText($fullPath)
            Require-Contains $content ('namespace ' + $game + '.UI.Pilots') ($fullPath + ' must belong to ' + $game + '.UI.Pilots namespace')
            Require (!$content.Contains('Service.gI')) ($fullPath + ' must not contain Service.gI')
            Require (!$content.Contains('requestPKHistory')) ($fullPath + ' must not reference PKHistory request')
            Require (!$content.Contains('loadImage(')) ($fullPath + ' must not call loadImage in paint view')
            Require (!$content.Contains('setClip(0, 0, GameCanvas.w, GameCanvas.h)')) ($fullPath + ' must not reset clip to fullscreen')
        }
    }
    Write-Output 'Phase 3 pilot files, namespaces, metadata, and dependency guards: OK'

    Write-Output '--- 2. Kiem tra API parity Game1/Game2 ---'
    foreach ($relFile in $expectedRelativeFiles) {
        $path1 = Join-Path $scriptsDir ('Game1\' + $relFile)
        $path2 = Join-Path $scriptsDir ('Game2\' + $relFile)
        $content1 = [System.IO.File]::ReadAllText($path1)
        $content2 = [System.IO.File]::ReadAllText($path2)
        Require ($content1.Replace('Game1', 'Game2') -eq $content2) ('API parity mismatch in file: ' + $relFile)
    }
    Write-Output 'Game1/Game2 pilot source parity: OK'

    Write-Output '--- 3. Kiem tra Panel.cs, route mang va co rollback ---'
    $csFiles = Get-ChildItem -Path $scriptsDir -Filter '*.cs' -Recurse
    $pkHistoryScrCallers = @()
    foreach ($file in $csFiles) {
        if ($file.Name -eq 'PKHistoryScr.cs') { continue }
        $text = [System.IO.File]::ReadAllText($file.FullName)
        if ($text.Contains('PKHistoryScr')) {
            $pkHistoryScrCallers += $file.FullName
        }
    }
    Require ($pkHistoryScrCallers.Count -eq 0) 'PKHistoryScr must have 0 callers'

    foreach ($game in @('Game1', 'Game2')) {
        $panelPath = Join-Path $scriptsDir ($game + '\Panel.cs')
        $controller2Path = Join-Path $scriptsDir ($game + '\Assets\src\f\Controller2.cs')
        Require (Test-Path -LiteralPath $panelPath) ($game + ' Panel.cs not found')
        Require (Test-Path -LiteralPath $controller2Path) ($game + ' Controller2.cs not found')

        $panel = [System.IO.File]::ReadAllText($panelPath)
        $controller2 = [System.IO.File]::ReadAllText($controller2Path)

        # 3.1 Constants and rollback flag
        Require-Contains $panel 'private const int TYPE_PK_HISTORY = 29;' ($game + ' Panel missing TYPE_PK_HISTORY = 29')
        Require-Contains $panel 'public static bool USE_NEW_PK_HISTORY_UI' ($game + ' Panel missing rollback flag USE_NEW_PK_HISTORY_UI')

        # 3.2 Network route
        $doFireTool = Get-MethodBody $panel 'private void doFireTool()'
        Require-Contains $doFireTool 'doPKHistory()' ($game + ' doFireTool does not invoke doPKHistory')

        $doPKHistory = Get-MethodBody $panel 'private void doPKHistory()'
        Require-Contains $doPKHistory 'setTypePKHistory()' ($game + ' doPKHistory does not invoke setTypePKHistory')

        $setTypePKHistory = Get-MethodBody $panel 'public void setTypePKHistory()'
        Require-Contains $setTypePKHistory 'type = TYPE_PK_HISTORY;' ($game + ' setTypePKHistory does not set type')
        Require-Contains $setTypePKHistory 'setType(0);' ($game + ' setTypePKHistory does not set position 0')
        Require-Contains $setTypePKHistory 'loadPKHistoryImages();' ($game + ' setTypePKHistory must pre-load images')
        Require-Contains $setTypePKHistory 'setTabPKHistory();' ($game + ' setTypePKHistory does not setTabPKHistory')
        Require-Contains $setTypePKHistory 'reloadPKHistory();' ($game + ' setTypePKHistory does not trigger reload')

        $reloadPKHistory = Get-MethodBody $panel 'public void reloadPKHistory()'
        Require-Contains $reloadPKHistory 'Service.gI().requestPKHistory();' ($game + ' reloadPKHistory does not call service')

        Require-Contains $controller2 'case 43:' ($game + ' Controller2 missing case 43')
        Require-Contains $controller2 'GameCanvas.panel.setPKHistory(myVector2);' ($game + ' Controller2 case 43 does not forward to GameCanvas.panel.setPKHistory')

        # 3.3 Dispatcher and Renderers
        $paintPKHistory = Get-MethodBody $panel 'private void paintPKHistory(mGraphics g)'
        Require-Contains $paintPKHistory 'USE_NEW_PK_HISTORY_UI' ($game + ' paintPKHistory must branch on USE_NEW_PK_HISTORY_UI')
        Require-Contains $paintPKHistory 'paintPKHistoryComponent' ($game + ' paintPKHistory must call paintPKHistoryComponent')
        Require-Contains $paintPKHistory 'paintPKHistoryLegacy' ($game + ' paintPKHistory must call paintPKHistoryLegacy')

        $paintLegacy = Get-MethodBody $panel 'private void paintPKHistoryLegacy(mGraphics g)'
        Require-Contains $paintLegacy 'loadPKHistoryImages();' ($game + ' paintPKHistoryLegacy must retain legacy behavior')
        Require-Contains $paintLegacy 'g.setClip(0, 0, GameCanvas.w, GameCanvas.h);' ($game + ' paintPKHistoryLegacy must retain legacy fullscreen clip reset')

        $paintComponent = Get-MethodBody $panel 'private void paintPKHistoryComponent(mGraphics g)'
        Require-Contains $paintComponent 'PKHistoryView.Paint' ($game + ' paintPKHistoryComponent must delegate to PKHistoryView.Paint')
        Require (!$paintComponent.Contains('loadImage(')) ($game + ' paintPKHistoryComponent must not call loadImage')

        # 3.4 Row action and bounds
        $doFirePKHistory = Get-MethodBody $panel 'private void doFirePKHistory()'
        Require-Contains $doFirePKHistory 'if (selected < 0 || selected >= pkHistoryEntries.size())' ($game + ' doFirePKHistory missing bounds guard')
        Require-Contains $doFirePKHistory 'GameCanvas.startOKDlg(' ($game + ' doFirePKHistory must display dialog')
    }
    Write-Output 'Panel.cs contracts, rollback flag, network route, and renderer delegation: OK'

    Write-Output '--- 4. Bien dich va chay truc tiep bo test hanh vi Phase 3 ---'
    $regressionSource = Join-Path $PSScriptRoot 'UiComponentPhase3Regression.cs'
    $stubSource = Join-Path $PSScriptRoot 'UiComponentPhase3LegacyStubs.cs'
    Require (Test-Path -LiteralPath $regressionSource) 'UiComponentPhase3Regression.cs not found'
    Require (Test-Path -LiteralPath $stubSource) 'UiComponentPhase3LegacyStubs.cs not found'

    $uiSharedDir = Join-Path $scriptsDir 'UIShared'
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
            (Join-Path $gameDir 'UI\Components\UiListRow.cs'),
            (Join-Path $gameDir 'UI\Pilots\PKHistoryView.cs')
        )
        $gameStub = Join-Path $outputDirectory ($game + '-Phase3LegacyStubs.cs')
        $gameRegression = Join-Path $outputDirectory ($game + '-Phase3Regression.cs')

        $stubContent = [System.IO.File]::ReadAllText($stubSource)
        $regressionContent = [System.IO.File]::ReadAllText($regressionSource)
        if ($game -eq 'Game2') {
            $stubContent = $stubContent.Replace('Game1', 'Game2')
            $regressionContent = $regressionContent.Replace('Game1', 'Game2')
        }
        [System.IO.File]::WriteAllText($gameStub, $stubContent)
        [System.IO.File]::WriteAllText($gameRegression, $regressionContent)

        $outputExe = Join-Path $outputDirectory ($game + '-UiComponentPhase3Regression.exe')
        $compileSources = @($gameStub, $gameRegression) + $uiSharedFiles + $productionSources
        $responseFile = Join-Path $outputDirectory ($game + '-phase3-compile.rsp')
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
            throw ($game + ' Phase 3 compilation failed with exit code: ' + $LASTEXITCODE)
        }
        Require (Test-Path -LiteralPath $outputExe) ($game + ' Phase 3 compilation did not generate executable')

        $testOutput = & $monoPath $outputExe
        if ($LASTEXITCODE -ne 0) {
            throw ($game + ' Phase 3 regression failed with exit code: ' + $LASTEXITCODE)
        }
        Require ($testOutput -contains ('UI_COMPONENT_PHASE3_PRODUCTION_' + $game + '_OK')) ($game + ' completion marker missing')
        Write-Output ($game + ' Phase 3 production source compilation and behavior regression: OK')
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

    $fullAssemblyPath = Join-Path $outputDirectory 'Assembly-CSharp-Phase3Verification.dll'
    $fullReferencePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase3Verification.ref.dll'
    $fullResponsePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase3Verification.rsp'

    # Build full list of production sources, appending any new Phase 3 files not yet in Bee's cached rsp
    $newPhase3Sources = @(
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

    # Ensure all Phase 3 sources are included
    $joinedRsp = [string]::Join("`n", $fullResponseLines)
    foreach ($newSrc in $newPhase3Sources) {
        $normalizedPath = $newSrc.Replace('\', '/')
        if (!$joinedRsp.Contains($normalizedPath) -and !$joinedRsp.Contains($newSrc)) {
            $fullResponseLines.Add('"' + $newSrc + '"')
        }
    }

    [System.IO.File]::WriteAllLines($fullResponsePath, $fullResponseLines)

    $fullCompilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $fullResponsePath) 2>&1
    if ($LASTEXITCODE -ne 0) {
        $fullCompilerOutput | Write-Output
        throw ('Full Unity Assembly-CSharp compilation failed with exit code: ' + $LASTEXITCODE)
    }
    Require (Test-Path -LiteralPath $fullAssemblyPath) 'Full Unity compilation did not generate Assembly-CSharp verification DLL'
    Write-Output 'Full Unity Assembly-CSharp compilation: OK'

    Write-Output 'UI_COMPONENT_PHASE3_PILOT_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        $resolvedOutputDirectory = [System.IO.Path]::GetFullPath($outputDirectory)
        $resolvedTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        $outputLeaf = Split-Path -Leaf $resolvedOutputDirectory
        if (!$resolvedOutputDirectory.StartsWith($resolvedTempRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
            !$outputLeaf.StartsWith('ui-phase3-', [System.StringComparison]::Ordinal)) {
            throw ('Refusing to remove unexpected test output directory: ' + $resolvedOutputDirectory)
        }
        Remove-Item -LiteralPath $resolvedOutputDirectory -Force -Recurse
    }
}
