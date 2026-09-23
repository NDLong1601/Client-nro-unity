$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$variants = @('Game1', 'Game2')

function Assert-Contains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -notmatch $Pattern) { throw $Message }
}

function Assert-NotContains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -match $Pattern) { throw $Message }
}

function Get-MethodBody {
    param([string]$Text, [string]$Signature)
    $start = $Text.IndexOf($Signature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Cannot find method: $Signature" }
    $open = $Text.IndexOf('{', $start)
    $depth = 0
    for ($i = $open; $i -lt $Text.Length; $i++) {
        if ($Text[$i] -eq '{') { $depth++ }
        elseif ($Text[$i] -eq '}') {
            $depth--
            if ($depth -eq 0) { return $Text.Substring($open, $i - $open + 1) }
        }
    }
    throw "Cannot find method end: $Signature"
}

foreach ($variant in $variants) {
    $menuDir = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu"
    $menu = (Get-ChildItem -LiteralPath $menuDir -Filter 'CustomMenuScr*.cs' -File |
        Sort-Object Name | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 }) -join "`n"

    Assert-Contains $menu 'FunctionNames\s*=\s*new string\[\]' "$variant function tab must define its twelve fixed menu entries."
    foreach ($label in @('Thông báo', 'Đổi khu', 'Đệ tử', 'Đổi cờ', 'Năng động', 'Sổ sưu tầm', 'Chat thế giới', 'Chức năng', 'Tài khoản', 'Cấu hình', 'Lịch sử', 'Đổi tài khoản')) {
        Assert-Contains $menu ([regex]::Escape('"' + $label + '"')) "$variant function tab is missing: $label"
    }

    $functionButton = Get-MethodBody $menu 'private static void PaintFunctionMenuButton'
    Assert-Contains $functionButton 'UiMenuTheme\.PaintButton' "$variant function buttons must use the shared raised menu theme."
    Assert-NotContains $functionButton 'g\.fillRect' "$variant function buttons must not duplicate their component's visual renderer."
    $skillButton = Get-MethodBody $menu 'private static void PaintSkillActionButton'
    Assert-Contains $skillButton 'UiMenuTheme\.PaintButton' "$variant skill action buttons must use the shared raised menu theme."

    $paint = Get-MethodBody $menu 'public override void paint'
    Assert-Contains $paint '_selectedMainTab\s*==\s*4[\s\S]*?PaintFunctionTabContent\(g\)' "$variant function tab must be wired into the custom-menu renderer."

    $functionPaint = Get-MethodBody $menu 'private void PaintFunctionTabContent'
    Assert-Contains $functionPaint 'PaintFunctionMenu' "$variant function tab must render the two-column action menu."
    Assert-Contains $functionPaint 'PaintFunctionNotification' "$variant function tab must render notification details."
    Assert-Contains $functionPaint 'PaintFunctionZones' "$variant function tab must render the zone grid."
    Assert-Contains $functionPaint 'PaintFunctionFlags' "$variant function tab must render flag choices."
    Assert-Contains $functionPaint 'PaintFunctionActivity' "$variant function tab must render activity overview and drill-down pages."
    Assert-Contains $functionPaint 'PaintFunctionWorldChat' "$variant function tab must render world-chat history and composer."
    Assert-Contains $functionPaint 'PaintFunctionToggles' "$variant function tab must render live MOD toggles."

    $activate = Get-MethodBody $menu 'private void ActivateFunction'
    Assert-Contains $activate 'Service\.gI\(\)\.openUIZone\(\)' "$variant zone action must request the authoritative zone payload."
    Assert-Contains $activate 'Service\.gI\(\)\.getFlag\(0, -1\)' "$variant flag action must request the authoritative flag payload."
    Assert-Contains $activate 'ActivityScreen\.gI\(\)\.requestPanelData\(\)' "$variant activity action must request the authoritative dashboard payload."

    $zone = Get-MethodBody $menu 'private void SelectFunctionZone'
    Assert-Contains $zone 'Service\.gI\(\)\.requestChangeZone' "$variant zone cells must use the original change-zone service."

    $flag = Get-MethodBody $menu 'private void SelectFunctionFlag'
    Assert-Contains $flag 'Service\.gI\(\)\.getFlag\(1' "$variant flag rows must use the original select-flag service."

    $activity = Get-MethodBody $menu 'private void ActivateFunctionActivityAction'
    Assert-Contains $activity 'claimPanelTier' "$variant activity rewards must stay server-authoritative."

    $chat = Get-MethodBody $menu 'private void SendFunctionWorldChat'
    Assert-Contains $chat 'checkLuong\(\)\s*<\s*5' "$variant world chat must retain the five-gem affordability check."
    Assert-Contains $chat 'Service\.gI\(\)\.chatGlobal' "$variant world chat must use the original service request."

    $toggle = Get-MethodBody $menu 'private void ToggleFunctionSetting'
    Assert-Contains $toggle 'isHighFps' "$variant function settings must control high FPS."
    Assert-Contains $toggle 'showCharsInMap' "$variant function settings must control characters-in-map."
    Assert-Contains $toggle 'showInfoMe' "$variant function settings must control self information."
    Assert-Contains $toggle 'isAutoPhaLe' "$variant function settings must control auto crystal insertion."
    Assert-Contains $toggle 'isAutoLogin' "$variant function settings must control auto login."

    $controllerPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\Assets\src\f\Controller2.cs"
    $controller = Get-Content -LiteralPath $controllerPath -Raw -Encoding UTF8
    Assert-Contains $controller "${variant}\.UI\.CustomMenu\.CustomMenuScr\.TryConsumeFunctionFlagOpen\(\)" "$variant flag payload must stay inside the open custom menu."
    Assert-Contains $controller "${variant}\.UI\.CustomMenu\.CustomMenuScr\.OnWorldChat\(str\)" "$variant world-chat payload must be recorded by the custom menu."
}

Write-Host 'Custom menu function-tab regression checks passed.'
