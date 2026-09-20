$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('ui-phase2-' + [guid]::NewGuid().ToString('N'))

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

try {
    Write-Output '--- 1. Kiem tra file production Phase 2 va Unity metadata ---'
    $expectedRelativeFiles = @(
        'UI\UiInputContext.cs',
        'UI\Adapters\CommandFactory.cs',
        'UI\Adapters\ScrollViewAdapter.cs',
        'UI\Adapters\UiRenderState.cs',
        'UI\Components\UiButton.cs',
        'UI\Components\UiFrame.cs',
        'UI\Sandbox\UiComponentSandboxScr.cs'
    )

    $scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        Require (Test-Path -LiteralPath (Join-Path $scriptsDir ($game + '\UI.meta'))) ($game + ' missing UI.meta')

        foreach ($relFile in $expectedRelativeFiles) {
            $fullPath = Join-Path $gameDir $relFile
            Require (Test-Path -LiteralPath $fullPath) ($game + ' missing file: ' + $relFile)
            Require (Test-Path -LiteralPath ($fullPath + '.meta')) ($game + ' missing Unity metadata: ' + $relFile + '.meta')

            $content = [System.IO.File]::ReadAllText($fullPath)
            Require-Contains $content ('namespace ' + $game + '.UI') ($fullPath + ' must belong to ' + $game + '.UI namespace')
            Require (!$content.Contains('Service.gI')) ($fullPath + ' must not contain Service.gI')
            Require (!$content.Contains('requestPKHistory')) ($fullPath + ' must not reference PKHistory request')
            Require (!$content.Contains('loadImage(')) ($fullPath + ' must not load runtime assets directly')
        }
    }
    Write-Output 'Production files, namespaces, metadata, and dependency guards: OK'

    Write-Output '--- 2. Kiem tra API parity Game1/Game2 ---'
    foreach ($relFile in $expectedRelativeFiles) {
        $path1 = Join-Path $scriptsDir ('Game1\' + $relFile)
        $path2 = Join-Path $scriptsDir ('Game2\' + $relFile)
        $content1 = [System.IO.File]::ReadAllText($path1)
        $content2 = [System.IO.File]::ReadAllText($path2)
        Require ($content1.Replace('Game1', 'Game2') -eq $content2) ('API parity mismatch in file: ' + $relFile)
    }
    Write-Output 'Game1/Game2 source parity: OK'

    Write-Output '--- 3. Kiem tra additive API va legacy surfaces khong doi ---'
    foreach ($game in @('Game1', 'Game2')) {
        $mGraphicsPath = Join-Path $scriptsDir ($game + '\mGraphics.cs')
        $mGraphicsContent = [System.IO.File]::ReadAllText($mGraphicsPath)
        Require-Contains $mGraphicsContent 'struct RawRenderState' ($game + ' mGraphics missing RawRenderState')
        Require-Contains $mGraphicsContent 'RawRenderState getRawState()' ($game + ' mGraphics missing getRawState()')
        Require-Contains $mGraphicsContent 'void restoreRawState(RawRenderState' ($game + ' mGraphics missing restoreRawState()')

        foreach ($relFile in $expectedRelativeFiles) {
            $adapterPath = Join-Path $scriptsDir ($game + '\' + $relFile)
            $adapterContent = [System.IO.File]::ReadAllText($adapterPath)
            Require (!$adapterContent.Contains('Panel.')) ($game + ' ' + $relFile + ' must not depend on Panel')
        }
    }
    Write-Output 'mGraphics additive API present and Phase 2 adapters decoupled from Panel: OK'

    Write-Output '--- 4. Bien dich va chay truc tiep production source cho Game1 va Game2 ---'
    $regressionSource = Join-Path $PSScriptRoot 'UiComponentPhase2Regression.cs'
    $stubSource = Join-Path $PSScriptRoot 'UiComponentPhase2LegacyStubs.cs'
    Require (Test-Path -LiteralPath $regressionSource) 'UiComponentPhase2Regression.cs not found'
    Require (Test-Path -LiteralPath $stubSource) 'UiComponentPhase2LegacyStubs.cs not found'

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
        $productionSources = foreach ($relFile in $expectedRelativeFiles) { Join-Path $gameDir $relFile }
        $gameStub = Join-Path $outputDirectory ($game + '-LegacyStubs.cs')
        $gameRegression = Join-Path $outputDirectory ($game + '-Regression.cs')

        $stubContent = [System.IO.File]::ReadAllText($stubSource)
        $regressionContent = [System.IO.File]::ReadAllText($regressionSource)
        if ($game -eq 'Game2') {
            $stubContent = $stubContent.Replace('Game1', 'Game2')
            $regressionContent = $regressionContent.Replace('Game1', 'Game2')
        }
        [System.IO.File]::WriteAllText($gameStub, $stubContent)
        [System.IO.File]::WriteAllText($gameRegression, $regressionContent)

        $outputExe = Join-Path $outputDirectory ($game + '-UiComponentPhase2Regression.exe')
        $compileSources = @($gameStub, $gameRegression) + $uiSharedFiles + $productionSources
        $responseFile = Join-Path $outputDirectory ($game + '-compile.rsp')
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
            throw ($game + ' production compilation failed with exit code: ' + $LASTEXITCODE)
        }
        Require (Test-Path -LiteralPath $outputExe) ($game + ' production compilation did not generate an executable')

        $testOutput = & $monoPath $outputExe
        if ($LASTEXITCODE -ne 0) {
            throw ($game + ' production regression failed with exit code: ' + $LASTEXITCODE)
        }
        Require ($testOutput -contains ('UI_COMPONENT_PHASE2_PRODUCTION_' + $game + '_OK')) ($game + ' completion marker missing')
        Write-Output ($game + ' production source compilation and behavior regression: OK')
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

    $fullAssemblyPath = Join-Path $outputDirectory 'Assembly-CSharp-Phase2Verification.dll'
    $fullReferencePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase2Verification.ref.dll'
    $fullResponsePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase2Verification.rsp'
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

    $allCsFiles = Get-ChildItem -Path $scriptsDir -Filter '*.cs' -Recurse | Select-Object -ExpandProperty FullName
    $joinedRsp = [string]::Join("`n", $fullResponseLines)
    foreach ($csFile in $allCsFiles) {
        $normalizedPath = $csFile.Replace('\', '/')
        if (!$joinedRsp.Contains($normalizedPath) -and !$joinedRsp.Contains($csFile)) {
            $fullResponseLines.Add('"' + $csFile + '"')
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

    Write-Output 'UI_COMPONENT_PHASE2_ADAPTERS_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        $resolvedOutputDirectory = [System.IO.Path]::GetFullPath($outputDirectory)
        $resolvedTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        $outputLeaf = Split-Path -Leaf $resolvedOutputDirectory
        if (!$resolvedOutputDirectory.StartsWith($resolvedTempRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
            !$outputLeaf.StartsWith('ui-phase2-', [System.StringComparison]::Ordinal)) {
            throw ('Refusing to remove unexpected test output directory: ' + $resolvedOutputDirectory)
        }
        Remove-Item -LiteralPath $resolvedOutputDirectory -Force -Recurse
    }
}
