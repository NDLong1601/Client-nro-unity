$ErrorActionPreference = 'Stop'

if ($PSVersionTable.PSEdition -eq 'Core') {
    $windowsPowerShell = Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
    if (!(Test-Path -LiteralPath $windowsPowerShell)) {
        throw 'Windows PowerShell 5.1 is required because Add-Type cannot emit an executable in PowerShell Core.'
    }

    Write-Output 'PowerShell Core detected; re-running the test with Windows PowerShell 5.1.'
    & $windowsPowerShell -NoLogo -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$uiSharedDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\UIShared'
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('ui-phase1-' + [guid]::NewGuid().ToString('N'))

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
    Write-Output '--- 1. Kiem tra su ton tai va co lap phu thuoc cua cac file UIShared ---'
    $uiSharedMetaPath = $uiSharedDir + '.meta'
    Require (Test-Path -LiteralPath $uiSharedMetaPath) 'Missing Unity metadata file: UIShared.meta'

    $expectedFiles = @(
        'UiRect.cs',
        'UiLayout.cs',
        'UiMetrics.cs',
        'UiColorTokens.cs',
        'UiInputType.cs',
        'TextAlign.cs'
    )

    $sources = @()
    foreach ($file in $expectedFiles) {
        $filePath = Join-Path $uiSharedDir $file
        Require (Test-Path -LiteralPath $filePath) ('Missing expected UIShared file: ' + $file)
        Require (Test-Path -LiteralPath ($filePath + '.meta')) ('Missing Unity metadata file: ' + $file + '.meta')
        $content = [System.IO.File]::ReadAllText($filePath)

        # Kiem tra namespace
        Require-Contains $content 'namespace Nro.UI' ($file + ' must be in namespace Nro.UI')

        # Kiem tra khong phu thuoc namespace hoac class legacy
        $forbiddenTokens = @(
            'using UnityEngine',
            'using Game1',
            'using Game2',
            'GameCanvas',
            'mGraphics',
            'mFont',
            'Command',
            'TField',
            'Scroll'
        )

        foreach ($token in $forbiddenTokens) {
            Require (!$content.Contains($token)) ($file + ' must not reference forbidden token: ' + $token)
        }

        $sources += $filePath
    }
    Write-Output 'All 6 UIShared files and Unity metadata exist and pass dependency isolation checks: OK'

    Write-Output '--- 2. Doi chieu enum voi hang legacy cua Game1 va Game2 ---'
    $scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'
    foreach ($game in @('Game1', 'Game2')) {
        $tFieldContent = [System.IO.File]::ReadAllText((Join-Path $scriptsDir ($game + '\TField.cs')))
        Require-Contains $tFieldContent 'INPUT_TYPE_ANY = 0;' ($game + ' TField.INPUT_TYPE_ANY must equal 0')
        Require-Contains $tFieldContent 'INPUT_TYPE_NUMERIC = 1;' ($game + ' TField.INPUT_TYPE_NUMERIC must equal 1')
        Require-Contains $tFieldContent 'INPUT_TYPE_PASSWORD = 2;' ($game + ' TField.INPUT_TYPE_PASSWORD must equal 2')
        Require-Contains $tFieldContent 'INPUT_ALPHA_NUMBER_ONLY = 3;' ($game + ' TField.INPUT_ALPHA_NUMBER_ONLY must equal 3')

        $fontContent = [System.IO.File]::ReadAllText((Join-Path $scriptsDir ($game + '\mFont.cs')))
        Require-Contains $fontContent 'LEFT = 0;' ($game + ' mFont.LEFT must equal 0')
        Require-Contains $fontContent 'RIGHT = 1;' ($game + ' mFont.RIGHT must equal 1')
        Require-Contains $fontContent 'CENTER = 2;' ($game + ' mFont.CENTER must equal 2')
    }
    Write-Output 'UiInputType and TextAlign values match Game1/Game2 legacy constants: OK'

    Write-Output '--- 3. Bien dich va chay bo test hanh vi UiComponentPhase1Regression ---'
    $regressionSource = Join-Path $PSScriptRoot 'UiComponentPhase1Regression.cs'
    Require (Test-Path -LiteralPath $regressionSource) 'UiComponentPhase1Regression.cs not found'
    $sources += $regressionSource

    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
    $outputExe = Join-Path $outputDirectory 'UiComponentPhase1Regression.exe'

    Add-Type -Path $sources -OutputAssembly $outputExe -OutputType ConsoleApplication -IgnoreWarnings
    Require (Test-Path -LiteralPath $outputExe) 'Compilation failed to generate test executable'

    $testOutput = & $outputExe
    if ($LASTEXITCODE -ne 0) {
        throw ('UiComponentPhase1Regression failed with exit code: ' + $LASTEXITCODE)
    }
    Require ($testOutput -contains 'UI_COMPONENT_PHASE1_FOUNDATION_OK') 'Test output missing completion marker'
    Write-Output 'UiComponentPhase1Regression passed all geometric, layout, and token tests: OK'

    Write-Output 'UI_COMPONENT_PHASE1_FOUNDATION_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        Remove-Item -LiteralPath $outputDirectory -Force -Recurse
    }
}
