$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('social-v2-phase7-' + [guid]::NewGuid().ToString('N'))

try {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
    $sources = @(
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\myReader.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\myWriter.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\SocialV2\FriendSocialState.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\SocialV2\FriendSocialProtocol.cs'),
        (Join-Path $PSScriptRoot 'SocialV2Phase7Regression.cs')
    )

    Add-Type -Path $sources -OutputAssembly (Join-Path $outputDirectory 'SocialV2Phase7Regression.exe') -OutputType ConsoleApplication -IgnoreWarnings
    & (Join-Path $outputDirectory 'SocialV2Phase7Regression.exe')
    if ($LASTEXITCODE -ne 0) {
        throw "Social V2 Phase 7 regression failed with exit code $LASTEXITCODE."
    }

    $contracts = @('openFriendSocialChat', 'setTypeFriendSocialChat', 'paintFriendSocialChat',
        'updateKeyFriendSocialChat', 'SOCIAL_V2_CHAT_INPUT', 'closeFriendSocialChat')
    foreach ($game in @('Game1', 'Game2')) {
        $panelPath = Join-Path $projectRoot ("Assets\Scripts\Assembly-CSharp\{0}\Panel.cs" -f $game)
        $content = Get-Content -LiteralPath $panelPath -Raw
        foreach ($contract in $contracts) {
            if (!$content.Contains($contract)) {
                throw "$game Panel is missing Phase 7 integration: $contract"
            }
        }
        $canvasPath = Join-Path $projectRoot ("Assets\Scripts\Assembly-CSharp\{0}\GameCanvas.cs" -f $game)
        if (!(Get-Content -LiteralPath $canvasPath -Raw).Contains('tryCloseFriendSocialChatPanel2')) {
            throw "$game GameCanvas does not preserve the social pair close sequence"
        }
    }
    Write-Output 'SOCIAL_V2_PHASE7_INTEGRATION_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        Remove-Item -LiteralPath $outputDirectory -Force -Recurse
    }
}
