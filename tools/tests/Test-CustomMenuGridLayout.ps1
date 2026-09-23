$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$sharedDir = Join-Path $root 'Assets\Scripts\Assembly-CSharp\UIShared'
$layoutSource = Join-Path $sharedDir 'UiGridLayout.cs'
if (!(Test-Path -LiteralPath $layoutSource)) { throw 'Shared grid layout source is missing.' }
Add-Type -Path @((Join-Path $sharedDir 'UiRect.cs'), $layoutSource) -ErrorAction Stop

function Require([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
}

$viewport = [Nro.UI.UiRect]::new(10, 20, 250, 90)
$bag = [Nro.UI.UiGridLayout]::new($viewport, 5, 2, 2, 32, 30)
Require ($bag.CellWidth -eq 47) 'Bag cell width must match the existing five-column formula.'
Require ($bag.GetCellBounds(0, 0).Equals([Nro.UI.UiRect]::new(12, 22, 47, 30))) 'First bag cell bounds changed.'
Require ($bag.GetCellBounds(6, 5).Equals([Nro.UI.UiRect]::new(61, 49, 47, 30))) 'Scrolled bag cell bounds changed.'
Require ($bag.GetClampedColumn(12) -eq 0) 'First bag column must remain selectable.'
Require ($bag.GetClampedColumn(259) -eq 4) 'Last bag column must remain selectable.'
Require ($bag.GetProportionalColumn(61) -eq 1) 'Proportional storage columns must preserve the existing click mapping.'
Require ($bag.IsCellVisible(0, 200) -eq $false) 'Off-screen bag cells must be culled.'

$zones = [Nro.UI.UiGridLayout]::new($viewport, 4, 3, 3, 29, 26)
Require ($zones.CellWidth -eq 58) 'Zone cell width must match the existing four-column formula.'
Require ($zones.GetCellBounds(5, 7).Equals([Nro.UI.UiRect]::new(74, 45, 58, 26))) 'Scrolled zone bounds changed.'
Require ($zones.HitTestCell(75, 46, 8, 7) -eq 5) 'A zone cell must hit its own index.'
Require ($zones.HitTestCell(72, 46, 8, 7) -eq -1) 'The gap between zone cells must not be selectable.'
Require ($zones.HitTestCell(75, 110, 8, 7) -eq -1) 'A click outside the viewport must not select a zone.'

foreach ($variant in @('Game1', 'Game2')) {
    $menuDir = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu"
    $inventory = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Inventory.cs') -Raw -Encoding UTF8
    $inventoryView = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Inventory.View.cs') -Raw -Encoding UTF8
    $clan = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Clan.cs') -Raw -Encoding UTF8
    $clanView = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Clan.View.cs') -Raw -Encoding UTF8
    $function = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Function.cs') -Raw -Encoding UTF8
    $functionView = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Function.View.cs') -Raw -Encoding UTF8
    Require ($inventory.Contains('UiGridLayout') -and $inventoryView.Contains('UiGridLayout')) "$variant inventory grid must use the shared layout for input and painting."
    Require ($clan.Contains('UiGridLayout') -and $clanView.Contains('UiGridLayout')) "$variant clan storage must use the shared layout for input and painting."
    Require ($function.Contains('UiGridLayout') -and $functionView.Contains('UiGridLayout')) "$variant zone grid must use the shared layout for input and painting."
}

Write-Host 'Custom menu grid-layout regression checks passed.'
