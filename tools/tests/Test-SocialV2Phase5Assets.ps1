$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sourceRoot = 'C:\Users\PC\Downloads\icon_chat_friend'
$destinationRoot = Join-Path $projectRoot 'Assets\Resources\res\x4\mainimage'
$expectedAssets = @(
    @{ Hash = '248D304244DC5F618A985934CB20FF3938F45124439657C82A9878D615861F3D'; Asset = 'social_accept.png'; Size = 80; Color = '#57AA05' },
    @{ Hash = '348CE25ABCCB88259CE236725428B920BB279E13CE63630B15EF1E9220AD8AA3'; Asset = 'social_add.png'; Size = 80; Color = '#5170FF' },
    @{ Hash = '970FCFE45C59ABD6C26E8CD8BC352C5AC297E98715F4FAE6E8071A003B61274C'; Asset = 'social_chat.png'; Size = 96 },
    @{ Hash = 'C07C06EC932974EF7E1AFA8F8D9A02EA13F43993276F78EE5C6FDB7560A9CA71'; Asset = 'social_emoji.png'; Size = 104 },
    @{ Hash = '3DA8873B3467D4842225B5CD9BC31C7A2A073EA69EE882D2F7961E22337823BC'; Asset = 'social_location.png'; Size = 104 },
    @{ Hash = '283E677B49E1E3B9FB385218C0C3126C895AF65D417EA5611360613304C48009'; Asset = 'social_mail.png'; Size = 80 },
    @{ Hash = '6358BD5398F838FC0D5DDF5C4F9B09FF4181370D88DEE6C885B39252F61ED636'; Asset = 'social_remove.png'; Size = 80; Color = '#CF3B2E' },
    @{ Hash = '9A8DAA86E54FC633993855143CE637C62C0A76D70DF1BFE0EEE01B4CBDE17073'; Asset = 'social_search.png'; Size = 80; Color = '#5170FF' },
    @{ Hash = '59A159C8D8D77BF7A2FF4CE62B1EF790D2939BF1B86F1910ED89F42A8B80D257'; Asset = 'social_send.png'; Size = 104 }
)
$sourceHashes = @{}
foreach ($sourceFile in (Get-ChildItem -LiteralPath $sourceRoot -File)) {
    $sourceHashes[(Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash] = $sourceFile.FullName
}
if ($sourceHashes.Count -ne $expectedAssets.Count) {
    throw 'Unexpected source icon inventory'
}

foreach ($asset in $expectedAssets) {
    if (!$sourceHashes.ContainsKey($asset.Hash)) {
        throw "Missing or changed checked source icon hash: $($asset.Hash)"
    }
    $path = Join-Path $destinationRoot $asset.Asset
    if (!(Test-Path -LiteralPath $path)) {
            throw "Missing generated social asset: $($asset.Asset)"
    }
    $bitmap = [System.Drawing.Bitmap]::new($path)
    try {
        if ($bitmap.Width -ne $asset.Size -or $bitmap.Height -ne $asset.Size) {
            throw "$($asset.Asset) must be $($asset.Size)x$($asset.Size), got $($bitmap.Width)x$($bitmap.Height)"
        }
        $minX = $bitmap.Width; $minY = $bitmap.Height; $maxX = -1; $maxY = -1
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                if ($bitmap.GetPixel($x, $y).A -gt 0) {
                    if ($x -lt $minX) { $minX = $x }
                    if ($x -gt $maxX) { $maxX = $x }
                    if ($y -lt $minY) { $minY = $y }
                    if ($y -gt $maxY) { $maxY = $y }
                }
            }
        }
        if ($maxX -lt 0 -or [Math]::Abs((($minX + $maxX) / 2.0) - (($bitmap.Width - 1) / 2.0)) -gt 1.0 -or [Math]::Abs((($minY + $maxY) / 2.0) - (($bitmap.Height - 1) / 2.0)) -gt 1.0) {
            throw "$($asset.Asset) is not transparently trimmed and centered"
        }
        if ($bitmap.GetPixel(0, 0).A -ne 0 -or $bitmap.GetPixel($bitmap.Width - 1, $bitmap.Height - 1).A -ne 0) {
            throw "$($asset.Asset) must retain transparent outer corners"
        }
        if ($asset.ContainsKey('Color')) {
            $target = [System.Drawing.ColorTranslator]::FromHtml($asset.Color)
            $targetPixels = 0
            for ($y = 0; $y -lt $bitmap.Height; $y++) {
                for ($x = 0; $x -lt $bitmap.Width; $x++) {
                    $pixel = $bitmap.GetPixel($x, $y)
                    if ($pixel.A -ge 250 -and [Math]::Abs($pixel.R - $target.R) -le 2 -and [Math]::Abs($pixel.G - $target.G) -le 2 -and [Math]::Abs($pixel.B - $target.B) -le 2) {
                        $targetPixels++
                    }
                }
            }
            if ($targetPixels -lt 20) {
                throw "$($asset.Asset) is missing its approved normalized button color"
            }
        }
    }
    finally {
        $bitmap.Dispose()
    }
    $meta = "$path.meta"
    if (!(Test-Path -LiteralPath $meta) -or !(Get-Content -LiteralPath $meta -Raw).Contains('filterMode: 0') -or !(Get-Content -LiteralPath $meta -Raw).Contains('enableMipMap: 0')) {
        throw "$($asset.Asset) is missing point-filter/no-mipmap importer metadata"
    }
}

Write-Output 'SOCIAL_V2_PHASE5_ASSETS_OK'
