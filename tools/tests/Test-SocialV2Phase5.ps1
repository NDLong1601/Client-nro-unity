$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('social-v2-phase5-' + [guid]::NewGuid().ToString('N'))

try {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
    $sources = @(
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game1\myReader.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game1\myWriter.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game1\SocialV2\FriendSocialState.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game1\SocialV2\FriendSocialProtocol.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\myReader.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\myWriter.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\SocialV2\FriendSocialState.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\SocialV2\FriendSocialProtocol.cs'),
        (Join-Path $PSScriptRoot 'SocialV2Phase5Regression.cs')
    )

    Add-Type -Path $sources -OutputAssembly (Join-Path $outputDirectory 'SocialV2Phase5Regression.exe') -OutputType ConsoleApplication -IgnoreWarnings
    & (Join-Path $outputDirectory 'SocialV2Phase5Regression.exe')
    if ($LASTEXITCODE -ne 0) {
        throw "Social V2 Phase 5 regression failed with exit code $LASTEXITCODE."
    }

    $integrationContracts = @{
        'Game1 Service' = @('openFriendSocialV2', 'sendFriendSocialV2Action(10', 'sendFriendSocialV2Chat')
        'Game2 Service' = @('openFriendSocialV2', 'sendFriendSocialV2Action(10', 'sendFriendSocialV2Chat')
        'Game1 Controller' = @('TryReadCapabilityTail', 'TryHandleAction', 'TryAppendPrivateChat', '!friendSocialState.SupportsSocialV2', 'RemoveFriend(num174)')
        'Game2 Controller' = @('TryReadCapabilityTail', 'TryHandleAction', 'TryAppendPrivateChat', '!friendSocialState.SupportsSocialV2', 'RemoveFriend(num170)')
        'Game1 GameCanvas' = @('FriendSocialState.gI().Reset()')
        'Game2 GameCanvas' = @('FriendSocialState.gI().Reset()')
    }
    $integrationFiles = @{
        'Game1 Service' = 'Assets\Scripts\Assembly-CSharp\Game1\Service.cs'
        'Game2 Service' = 'Assets\Scripts\Assembly-CSharp\Game2\Service.cs'
        'Game1 Controller' = 'Assets\Scripts\Assembly-CSharp\Game1\Controller.cs'
        'Game2 Controller' = 'Assets\Scripts\Assembly-CSharp\Game2\Controller.cs'
        'Game1 GameCanvas' = 'Assets\Scripts\Assembly-CSharp\Game1\GameCanvas.cs'
        'Game2 GameCanvas' = 'Assets\Scripts\Assembly-CSharp\Game2\GameCanvas.cs'
    }
    foreach ($name in $integrationContracts.Keys) {
        $content = Get-Content -LiteralPath (Join-Path $projectRoot $integrationFiles[$name]) -Raw
        foreach ($needle in $integrationContracts[$name]) {
            if (!$content.Contains($needle)) {
                throw "$name is missing Phase 5 integration: $needle"
            }
        }
    }
    Write-Output 'SOCIAL_V2_PHASE5_INTEGRATION_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        Remove-Item -LiteralPath $outputDirectory -Force -Recurse
    }
}
