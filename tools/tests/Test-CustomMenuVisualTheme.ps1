$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')

function Require([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
}

foreach ($variant in @('Game1', 'Game2')) {
    $uiDir = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI"
    $componentDir = Join-Path $uiDir 'Components'
    $menuDir = Join-Path $uiDir 'CustomMenu'
    $themePath = Join-Path $componentDir 'UiMenuTheme.cs'
    Require (Test-Path -LiteralPath $themePath) "$variant must provide the shared raised menu theme."

    $theme = Get-Content -LiteralPath $themePath -Raw -Encoding UTF8
    $tabs = Get-Content -LiteralPath (Join-Path $componentDir 'UiTabBar.cs') -Raw -Encoding UTF8
    $skillPanel = Get-Content -LiteralPath (Join-Path $componentDir 'UiSkillPanel.cs') -Raw -Encoding UTF8
    $inventory = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Inventory.View.cs') -Raw -Encoding UTF8
    $task = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Task.View.cs') -Raw -Encoding UTF8
    $skill = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Skill.View.cs') -Raw -Encoding UTF8
    $function = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Function.View.cs') -Raw -Encoding UTF8
    $clan = Get-Content -LiteralPath (Join-Path $menuDir 'CustomMenuScr.Clan.View.cs') -Raw -Encoding UTF8

    Require ($theme.Contains('NormalFill = 0xE99A00') -and $theme.Contains('SelectedFill = 0x64C70D')) "$variant normal and selected buttons must use the clan palette."
    Require ($theme.Contains('UiFrame.PaintRaisedHeader') -and $theme.Contains('UiFrame.PaintRaisedSurface') -and $theme.Contains('UiActionButton.PaintRaised')) "$variant theme must render raised headers, surfaces, and buttons."
    Require ($theme.Contains('public static void PaintCard')) "$variant theme must provide the clan-style raised row card."
    Require ($tabs.Contains('public void PaintRaised(mGraphics g)')) "$variant tab bar must support raised tabs without changing hit regions."
    Require ($inventory.Contains('ButtonStyle = UiMenuTheme.ButtonStyle') -and $inventory.Contains('_inventoryLeftTabBar.PaintRaised(g)') -and $inventory.Contains('_inventoryBagTabBar.PaintRaised(g)')) "$variant inventory subtabs must use the amber/green raised clan style."
    Require ($inventory.Contains('UiMenuTheme.PaintSurface')) "$variant inventory panels must use raised surfaces."
    Require ($inventory.Contains('UiMenuTheme.PaintCard')) "$variant inventory list rows must use raised cards."
    Require ($inventory.Contains('UiMenuTheme.PaintButton(g, rect, label, focused)')) "$variant inventory item actions must use raised buttons."
    Require ($task.Contains('UiMenuTheme.PaintButton') -and $task.Contains('UiMenuTheme.PaintHeader') -and $task.Contains('UiMenuTheme.PaintSurface')) "$variant task buttons and panels must use the shared raised style."
    Require ($task.Contains('UiMenuTheme.PaintCard')) "$variant task rows must use raised cards."
    Require ($skillPanel.Contains('UiMenuTheme.PaintHeader') -and $skillPanel.Contains('UiMenuTheme.PaintSurface')) "$variant skill headers and columns must use the shared raised style."
    Require ($skill.Contains('UiMenuTheme.PaintButton')) "$variant skill action buttons must use the shared raised style."
    Require ($skill.Contains('UiMenuTheme.PaintCard')) "$variant skill rows must use raised cards."
    Require ($skill.Contains('UiMenuTheme.PaintSurface(g, _intrinsicInputDialogRect')) "$variant intrinsic dialog must use a raised panel."
    Require ($function.Contains('UiMenuTheme.PaintHeader') -and $function.Contains('UiMenuTheme.PaintButton')) "$variant function headers and buttons must use the shared raised style."
    Require ($function.Contains('UiMenuTheme.PaintButton(g, headerRect')) "$variant notification headers must use raised buttons."
    Require ($function.Contains('UiMenuTheme.PaintCard')) "$variant function lists and toggle rows must use raised cards."
    Require ($clan.Contains('UiMenuTheme.PaintHeader') -and $clan.Contains('UiMenuTheme.PaintSurface') -and $clan.Contains('UiMenuTheme.PaintButton')) "$variant clan must use the same shared theme as the other tabs."
}

Write-Host 'Custom menu visual theme regression checks passed.'
