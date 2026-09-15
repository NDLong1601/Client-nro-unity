$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('social-v2-phase6-' + [guid]::NewGuid().ToString('N'))

try {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
    $sources = @(
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\myReader.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\myWriter.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\SocialV2\FriendSocialState.cs'),
        (Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp\Game2\SocialV2\FriendSocialProtocol.cs'),
        (Join-Path $PSScriptRoot 'SocialV2Phase6Regression.cs')
    )

    Add-Type -Path $sources -OutputAssembly (Join-Path $outputDirectory 'SocialV2Phase6Regression.exe') -OutputType ConsoleApplication -IgnoreWarnings
    & (Join-Path $outputDirectory 'SocialV2Phase6Regression.exe')
    if ($LASTEXITCODE -ne 0) {
        throw "Social V2 Phase 6 regression failed with exit code $LASTEXITCODE."
    }

    $contracts = @{
        'Game1 Panel' = @('setFriendSocialMode', 'paintFriendSocial', 'updateKeyFriendSocial', 'SOCIAL_V2_SEARCH_INPUT')
        'Game2 Panel' = @('setFriendSocialMode', 'paintFriendSocial', 'updateKeyFriendSocial', 'SOCIAL_V2_SEARCH_INPUT')
        'Game1 Service' = @('BeginInboxOperation', 'BeginSearch(requestToken, cursor)')
        'Game2 Service' = @('BeginInboxOperation', 'BeginSearch(requestToken, cursor)')
    }
    $files = @{
        'Game1 Panel' = 'Assets\Scripts\Assembly-CSharp\Game1\Panel.cs'
        'Game2 Panel' = 'Assets\Scripts\Assembly-CSharp\Game2\Panel.cs'
        'Game1 Service' = 'Assets\Scripts\Assembly-CSharp\Game1\Service.cs'
        'Game2 Service' = 'Assets\Scripts\Assembly-CSharp\Game2\Service.cs'
    }
    foreach ($name in $contracts.Keys) {
        $content = Get-Content -LiteralPath (Join-Path $projectRoot $files[$name]) -Raw
        foreach ($needle in $contracts[$name]) {
            if (!$content.Contains($needle)) {
                throw "$name is missing Phase 6 integration: $needle"
            }
        }
    }
    Write-Output 'SOCIAL_V2_PHASE6_INTEGRATION_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        Remove-Item -LiteralPath $outputDirectory -Force -Recurse
    }
}
