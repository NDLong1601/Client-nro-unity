$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$sharedDir = Join-Path $root 'Assets\Scripts\Assembly-CSharp\UIShared'
$rectSource = Join-Path $sharedDir 'UiRect.cs'
$layoutSource = Join-Path $sharedDir 'UiAccordionLayout.cs'

function Require([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
}

function Get-MethodBody([string]$text, [string]$signature) {
    $start = $text.IndexOf($signature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Cannot find method: $signature" }
    $open = $text.IndexOf('{', $start)
    $depth = 0
    for ($i = $open; $i -lt $text.Length; $i++) {
        if ($text[$i] -eq '{') { $depth++ }
        elseif ($text[$i] -eq '}') {
            $depth--
            if ($depth -eq 0) { return $text.Substring($open, $i - $open + 1) }
        }
    }
    throw "Cannot find method end: $signature"
}

Require (Test-Path -LiteralPath $layoutSource) 'The shared accordion layout source is missing.'
Add-Type -Path @($rectSource, $layoutSource) -ErrorAction Stop

$viewport = [Nro.UI.UiRect]::new(10, 20, 100, 55)
$closed = [Nro.UI.UiAccordionLayout]::new($viewport, 3, -1, 0)
Require ($closed.ContentHeight -eq 96) 'Three collapsed notifications must occupy 96 pixels.'
Require ($closed.GetHeaderBounds(0, 0).Y -eq 23) 'The first header must start after the top inset.'
Require ($closed.GetHeaderBounds(2, 0).Y -eq 83) 'Collapsed headers must have a stable row stride.'
Require ($closed.GetBodyBounds(1, 0).IsEmpty) 'Collapsed notification must have no body bounds.'
Require ($closed.HitTestHeader(20, 24, 0) -eq 0) 'A visible header must be clickable.'
Require ($closed.HitTestHeader(20, 51, 0) -eq -1) 'The gap between headers must not be clickable.'
Require ($closed.HitTestHeader(20, 84, 0) -eq -1) 'A header clipped outside the viewport must not be clickable.'

$emptyExpanded = [Nro.UI.UiAccordionLayout]::new($viewport, 3, 1, 0)
Require ($emptyExpanded.ContentHeight -eq 128) 'An expanded empty notification must reserve its visible body height.'
Require ($emptyExpanded.GetBodyBounds(1, 0).Y -eq 81) 'Expanded body must follow the selected header.'
Require ($emptyExpanded.GetBodyBounds(1, 0).Height -eq 30) 'Empty content must still render one line and padding.'
Require ($emptyExpanded.GetHeaderBounds(2, 0).Y -eq 115) 'Later headers must move below the expanded body.'
Require ($emptyExpanded.HitTestHeader(20, 82, 0) -eq -1) 'Clicking expanded body must not toggle its header.'
Require ($emptyExpanded.HitTestHeader(20, 38, 15) -eq 1) 'Hit testing must use the same scroll offset as painting.'

$multiline = [Nro.UI.UiAccordionLayout]::new($viewport, 3, 1, 3)
Require ($multiline.GetBodyBounds(1, 0).Height -eq 62) 'Three content lines must use the shared line-height rule.'
Require ($multiline.ContentHeight -eq 160) 'Content height must include the complete expanded body.'
Require ($multiline.GetHeaderOffset(2) -eq 127) 'Keyboard scrolling must use the variable-height header offset.'

$empty = [Nro.UI.UiAccordionLayout]::new($viewport, 0, -1, 0)
Require ($empty.ContentHeight -eq 0) 'An empty notification list must have no scrollable content.'
Require ($empty.HitTestHeader(20, 24, 0) -eq -1) 'An empty notification list must not accept header clicks.'

foreach ($variant in @('Game1', 'Game2')) {
    $path = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu\CustomMenuScr.Function.cs"
    $viewPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu\CustomMenuScr.Function.View.cs"
    $mainPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu\CustomMenuScr.cs"
    $source = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    $viewSource = Get-Content -LiteralPath $viewPath -Raw -Encoding UTF8
    $mainSource = Get-Content -LiteralPath $mainPath -Raw -Encoding UTF8
    $height = Get-MethodBody $source 'private int GetFunctionNotificationTotalHeight()'
    $paint = Get-MethodBody $viewSource 'private void PaintFunctionNotification(mGraphics g)'
    $input = Get-MethodBody $source 'private bool HandleFunctionPointerInput()'
    $navigate = Get-MethodBody $source 'private void MoveFunctionVerticalSelection(int direction)'
    $configure = Get-MethodBody $mainSource 'private void ConfigureScrollAdapters()'
    $update = Get-MethodBody $mainSource 'public override void update()'
    Require ($height.Contains('BuildFunctionNotificationLayout')) "$variant scroll height must use the shared layout."
    Require ($paint.Contains('GetHeaderBounds') -and $paint.Contains('GetBodyBounds')) "$variant painting must use the shared layout bounds."
    Require ($input.Contains('HitTestHeader')) "$variant pointer hit testing must use the shared layout."
    Require ($input.Contains('row >= 0 && _functionView != FunctionViewNotifications')) "$variant pixel scroll positions must not toggle notifications."
    Require ($navigate.Contains('GetHeaderOffset')) "$variant keyboard scrolling must target the real header position."
    Require ($configure.Contains('_rightScrollAdapter.Configure(viewport, totalH, 1)')) "$variant scrolling must use the exact content height in pixels."
    Require ($update.Contains('? GetFunctionNotificationTotalHeight()')) "$variant data refresh must compare the exact content height."
}

Write-Host 'Custom menu notification layout regression checks passed.'
