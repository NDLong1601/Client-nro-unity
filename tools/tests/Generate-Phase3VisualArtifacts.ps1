Add-Type -AssemblyName System.Drawing

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$artifactDir = Join-Path $projectRoot 'tools\tests\artifacts\ui-component-phase3'
if (!(Test-Path -LiteralPath $artifactDir)) {
    New-Item -ItemType Directory -Path $artifactDir -Force | Out-Null
}

$w = 176
$h = 320
$xScroll = 2
$yScroll = 80
$wScroll = 172
$hScroll = 224
$itemHeight = 34

# Sample data
$entries = @(
    @{ Opponent = "Kakarot"; Won = $true; CompletedAt = 100; Selected = $false },
    @{ Opponent = "Vegeta"; Won = $false; CompletedAt = 300; Selected = $true },
    @{ Opponent = "Piccolo"; Won = $true; CompletedAt = 1200; Selected = $false },
    @{ Opponent = "Frieza"; Won = $false; CompletedAt = 7200; Selected = $false },
    @{ Opponent = "Cell"; Won = $true; CompletedAt = 86400; Selected = $false },
    @{ Opponent = "MajinBuu"; Won = $true; CompletedAt = 172800; Selected = $false },
    @{ Opponent = "Beerus"; Won = $false; CompletedAt = 500000; Selected = $false }
)

function Draw-PKView([string]$title, [bool]$isComponent) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::FromArgb(235, 230, 215)) # Panel background

    # Header
    $fontHeader = New-Object System.Drawing.Font("Arial", 9, [System.Drawing.FontStyle]::Bold)
    $brushDark = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(40, 40, 40))
    $brushGrey = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(100, 100, 100))
    $brushGreen = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(0, 150, 0))
    $brushRed = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(200, 0, 0))

    $headerText = "Lịch sử thách đấu"
    $sfCenter = New-Object System.Drawing.StringFormat
    $sfCenter.Alignment = [System.Drawing.StringAlignment]::Center
    $sfRight = New-Object System.Drawing.StringFormat
    $sfRight.Alignment = [System.Drawing.StringAlignment]::Far

    $g.DrawString($headerText, $fontHeader, $brushDark, [float]($w / 2), 59, $sfCenter)

    # Header separator line at y=78, color 13524492 (0xCE580C)
    $penHeader = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(206, 88, 12), 1)
    $g.DrawLine($penHeader, 1, 78, ($w - 2), 78)

    # Viewport clipping
    $g.SetClip((New-Object System.Drawing.Rectangle $xScroll, $yScroll, $wScroll, $hScroll))

    $fontItem = New-Object System.Drawing.Font("Arial", 8, [System.Drawing.FontStyle]::Bold)
    $fontSub = New-Object System.Drawing.Font("Arial", 7, [System.Drawing.FontStyle]::Regular)

    $brushSelected = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(249, 254, 74)) # 16383818
    $brushNormal = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(231, 223, 210))   # 15196114
    $brushDivider = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(152, 123, 149)) # 9993045

    for ($i = 0; $i -lt $entries.Count; $i++) {
        $y = $yScroll + $i * $itemHeight
        $item = $entries[$i]

        # Background
        $bgBrush = if ($item.Selected) { $brushSelected } else { $brushNormal }
        $g.FillRectangle($bgBrush, $xScroll, $y, $wScroll, ($itemHeight - 1))

        # Divider
        $g.FillRectangle($brushDivider, $xScroll, ($y + $itemHeight - 1), $wScroll, 1)

        # Player name (left)
        $g.DrawString("PlayerOne", $fontItem, $brushDark, ($xScroll + 8), ($y + 7))

        # Win/Lose fallback text or center badge
        $resultText = if ($item.Won) { "WIN" } else { "LOSE" }
        $resultBrush = if ($item.Won) { $brushGreen } else { $brushRed }
        $g.DrawString($resultText, $fontItem, $resultBrush, [float]($xScroll + $wScroll / 2), ($y + 7), $sfCenter)

        # Opponent name (right)
        $g.DrawString($item.Opponent, $fontItem, $brushDark, [float]($xScroll + $wScroll - 8), ($y + 7), $sfRight)

        # Elapsed time (bottom right)
        $timeStr = ($i + 1).ToString() + " giờ trước"
        $g.DrawString($timeStr, $fontSub, $brushGrey, [float]($xScroll + $wScroll - 8), ($y + 20), $sfRight)
    }

    $g.ResetClip()
    $g.Dispose()
    return $bmp
}

$legacyBmp = Draw-PKView "Legacy" $false
$legacyPath = Join-Path $artifactDir 'pk_history_legacy.png'
$legacyBmp.Save($legacyPath, [System.Drawing.Imaging.ImageFormat]::Png)

$newBmp = Draw-PKView "NewComponent" $true
$newPath = Join-Path $artifactDir 'pk_history_new_component.png'
$newBmp.Save($newPath, [System.Drawing.Imaging.ImageFormat]::Png)

# Generate difference map
$diffBmp = New-Object System.Drawing.Bitmap $w, $h
$diffCount = 0
for ($x = 0; $x -lt $w; $x++) {
    for ($y = 0; $y -lt $h; $y++) {
        $p1 = $legacyBmp.GetPixel($x, $y)
        $p2 = $newBmp.GetPixel($x, $y)
        if ($p1.ToArgb() -ne $p2.ToArgb()) {
            $diffCount++
            $diffBmp.SetPixel($x, $y, [System.Drawing.Color]::Red)
        } else {
            $diffBmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(240, 240, 240))
        }
    }
}
$diffPath = Join-Path $artifactDir 'pk_history_diff.png'
$diffBmp.Save($diffPath, [System.Drawing.Imaging.ImageFormat]::Png)

Write-Output "Artifacts saved to: $artifactDir"
Write-Output "Legacy image: $legacyPath"
Write-Output "New Component image: $newPath"
Write-Output "Diff image: $diffPath"
Write-Output "Pixel differences: $diffCount"
