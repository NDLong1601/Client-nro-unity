$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$variants = @('Game1', 'Game2')

function Assert-Contains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -notmatch $Pattern) {
        throw $Message
    }
}

function Assert-NotContains {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ($Text -match $Pattern) {
        throw $Message
    }
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

    Assert-Contains $menu 'InventoryEquipmentSlotCount\s*=\s*16' "$variant inventory must reserve sixteen visual equipment slots."
    Assert-Contains $menu 'InventoryEquipmentBorderInset\s*=\s*2' "$variant equipment-level frame must be inset two pixels from its cell."
    Assert-Contains $menu 'EquipmentVisualOrder\s*=\s*new int\[\]\s*\{\s*0,\s*1,\s*4,\s*2,\s*3,\s*5,\s*6,\s*7,\s*8,\s*9,\s*10,\s*11,\s*13,\s*12,\s*14,\s*15\s*\}' "$variant equipment layout must match the reference while preserving server slot ids."
    Assert-Contains $menu '"Tr\u1ED1ng"' "$variant must label the two future accessory placeholders."

    $tabStart = Get-MethodBody $menu 'private static int GetInventoryTabStart'
    Assert-Contains $tabStart '\(bagLength\s*\+\s*1\)\s*/\s*2' "$variant inventory tabs must split bag capacity into two balanced consecutive ranges."

    $scrollConfig = Get-MethodBody $menu 'private void ConfigureScrollAdapters'
    Assert-Contains $scrollConfig '_selectedMainTab\s*==\s*1' "$variant inventory must configure its own scroll behavior."
    Assert-Contains $scrollConfig 'ModFunc\.isInventory\s*\?\s*InventoryGridRowHeight\s*:\s*InventoryListRowHeight' "$variant inventory scrolling must follow the active grid/list mode."

    $paint = Get-MethodBody $menu 'public override void paint'
    Assert-Contains $paint '_selectedMainTab\s*==\s*1[\s\S]*?PaintInventoryTabContent\(g\)' "$variant main inventory tab must be wired into the custom-menu renderer."

    $inventoryPaint = Get-MethodBody $menu 'private void PaintInventoryTabContent'
    Assert-Contains $inventoryPaint 'PaintInventoryLeftTabs\(g\)' "$variant inventory must expose both left-side modes."
    Assert-Contains $inventoryPaint 'PaintInventoryBagTabs\(g\)' "$variant inventory must expose Tab 1/Tab 2 on the right."
    Assert-Contains $inventoryPaint 'ModFunc\.isInventory' "$variant inventory must respect the existing grid/list preference."
    Assert-Contains $inventoryPaint 'PaintInventoryGrid\(g\)' "$variant inventory must render the grid flow."
    Assert-Contains $inventoryPaint 'PaintInventoryList\(g\)' "$variant inventory must render the default list flow."

    $equipmentPaint = Get-MethodBody $menu 'private void PaintEquipmentSlots'
    Assert-Contains $equipmentPaint 'UiEquipmentGrid\.Paint' "$variant equipment layout must delegate slot rendering to the reusable grid component."
    Assert-Contains $equipmentPaint 'paintCharBody' "$variant equipment panel must render the equipped character preview."

    $equipmentGridPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\Components\UiEquipmentGrid.cs"
    $equipmentGrid = Get-Content -LiteralPath $equipmentGridPath -Raw -Encoding UTF8
    $equipmentGridPaint = Get-MethodBody $equipmentGrid 'public static void Paint'
    Assert-Contains $equipmentGridPaint 'serverSlot\s*<\s*items\.Length\s*\?\s*items\[serverSlot\]\s*:\s*null' "$variant placeholder equipment slots must never index past the server body slots."
    Assert-Contains $equipmentGridPaint 'UiItemSlot\.PaintEmptyLabel' "$variant equipment grid must render caller-provided labels for empty slots."

    $equipmentLayout = Get-MethodBody $menu 'private void ConfigureEquipmentSlotRects'
    Assert-Contains $equipmentLayout 'const\s+int\s+columns\s*=\s*5' "$variant equipment layout must use one five-column grid."
    Assert-Contains $equipmentLayout 'const\s+int\s+rows\s*=\s*5' "$variant equipment layout must use one five-row grid."
    Assert-Contains $equipmentLayout '_equipmentSlotRects\[row\]\s*=\s*new UiRect\(gridX,\s*y,\s*cellWidth,\s*cellHeight\)' "$variant main equipment cells must use the shared cell size."
    Assert-Contains $equipmentLayout '_equipmentSlotRects\[index\s*\+\s*6\]\s*=\s*new UiRect\(x,\s*y,\s*cellWidth,\s*cellHeight\)' "$variant accessory cells must use the same shared cell size."
    Assert-NotContains $equipmentLayout 'sideWidth|sideHeight|bottomCellWidth|bottomCellHeight' "$variant equipment layout must not keep separate main/accessory cell dimensions."

    $pointer = Get-MethodBody $menu 'private bool HandleInventoryPointerInput'
    Assert-Contains $pointer '_inventoryLeftTab0Rect' "$variant inventory pointer flow must switch to equipment."
    Assert-Contains $pointer '_inventoryLeftTab1Rect' "$variant inventory pointer flow must switch to player information."
    Assert-Contains $pointer '_inventoryBagTab0Rect' "$variant inventory pointer flow must switch to bag Tab 1."
    Assert-Contains $pointer '_inventoryBagTab1Rect' "$variant inventory pointer flow must switch to bag Tab 2."
    Assert-Contains $pointer '_rightScrollAdapter\.UpdateKey' "$variant both bag tabs must use the shared scrolling adapter contract."

    Assert-Contains $menu 'InventoryFocusBagTabs\s*=\s*1' "$variant bag tabs must have a dedicated keyboard-focus state."
    Assert-Contains $menu 'InventoryFocusBagItems\s*=\s*2' "$variant bag cells must have a dedicated keyboard-focus state."
    Assert-Contains $menu 'InventoryFocusActions\s*=\s*3' "$variant item actions must have a dedicated keyboard-focus state."
    Assert-Contains $menu 'bool\s+_showInventoryDetail' "$variant must keep item focus separate from opening the detail panel."
    Assert-Contains $menu 'InventoryBagToBody\s*=\s*4' "$variant must preserve the original Panel BAG_BODY protocol value."
    Assert-Contains $menu 'InventoryBodyToBag\s*=\s*5' "$variant must preserve the original Panel BODY_BAG protocol value."
    Assert-Contains $menu 'InventoryBagToPet\s*=\s*6' "$variant must preserve the original Panel BAG_PET protocol value."
    Assert-Contains $menu 'InventoryBagToPet2\s*=\s*8' "$variant must preserve the original Panel BAG_PET2 protocol value."

    $horizontal = Get-MethodBody $menu 'private void MoveInventoryHorizontalSelection'
    Assert-Contains $horizontal 'localIndex\s*%\s*InventoryGridColumns' "$variant horizontal grid navigation must track the current column."
    Assert-Contains $horizontal 'targetRow\s*!=\s*currentRow' "$variant horizontal grid navigation must not wrap into another row."

    $vertical = Get-MethodBody $menu 'private void MoveInventoryVerticalSelection'
    Assert-Contains $vertical 'localIndex\s*<\s*InventoryGridColumns' "$variant Up from the first grid row must be detected."
    Assert-Contains $vertical '!ModFunc\.isInventory\s*&&\s*localIndex\s*==\s*0' "$variant default-list navigation must reach the bag tabs only from its first item."
    Assert-Contains $vertical 'FocusInventoryBagTabs\(\)' "$variant Up from the first bag row must move focus to the bag tabs."

    $detailLayout = Get-MethodBody $menu 'private void ConfigureInventoryDetailRects'
    Assert-Contains $detailLayout '!_showInventoryDetail' "$variant must not reserve an overlay until the player explicitly opens item details."

    $bagSelection = Get-MethodBody $menu 'private void SelectInventoryBagItem'
    Assert-Contains $bagSelection 'openDetail' "$variant must distinguish first-selection from activating the same bag item."
    Assert-Contains $bagSelection 'OpenInventoryDetail\(\)' "$variant must open bag details only on a repeated selection."

    $bodySelection = Get-MethodBody $menu 'private void SelectInventoryBodyItem'
    Assert-Contains $bodySelection 'openDetail' "$variant must distinguish first-selection from activating the same equipment item."
    Assert-Contains $bodySelection 'OpenInventoryDetail\(\)' "$variant must open equipment details only on a repeated selection."

    $confirm = Get-MethodBody $menu 'private void HandleInventoryConfirm'
    Assert-Contains $confirm 'OpenInventoryDetail\(\)' "$variant Enter must explicitly open the selected item's detail/actions."

    $infoLines = Get-MethodBody $menu 'private static List<string> BuildPlayerInformationLines'
    Assert-Contains $infoLines 'optionTotals' "$variant player information must aggregate every equipped option like the original Panel."
    Assert-Contains $infoLines 'option\.IsValidOption\(\)' "$variant player information must filter options with the original Panel validity rule."
    Assert-Contains $infoLines 'tlDef|tlPst|tlNeDon|tlHutHp|tlHutMp' "$variant player information must include the original Panel attribute fields."

    $profile = Get-MethodBody $menu 'private void PaintPlayerInformation'
    Assert-Contains $profile 'profileAvatarRect' "$variant player profile must use an explicit balanced avatar box."
    Assert-Contains $profile 'UiRenderState\.Push\(g, profileAvatarRect, clip:\s*true\)' "$variant player avatar must be clipped to its profile box."

    $detail = Get-MethodBody $menu 'private void PaintInventoryItemDetail'
    Assert-Contains $detail '!_showInventoryDetail' "$variant focus alone must never paint the item detail overlay."
    Assert-Contains $detail 'GetInventoryActionLabel' "$variant selected inventory items must render their related actions."
    Assert-Contains $detail 'PaintCrystalStars' "$variant item details must restore filled and empty crystal-star slots."
    Assert-Contains $detail 'headerDividerY[\s\S]*footerY[\s\S]*starY' "$variant item detail must keep distinct header, requirement and crystal-star dividers."

    $slotPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\Components\UiItemSlot.cs"
    $slot = Get-Content -LiteralPath $slotPath -Raw -Encoding UTF8
    $skillPanelPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\Components\UiSkillPanel.cs"
    $skillPanel = Get-Content -LiteralPath $skillPanelPath -Raw -Encoding UTF8
    Assert-Contains $menu 'UiSkillPanel\.PaintHeader' "$variant skill headers must use the reusable skill-panel component."
    Assert-Contains $menu 'UiSkillPanel\.PaintListColumn' "$variant skill rows must keep scrolling in the shared skill-panel component."
    Assert-Contains $menu 'UiSkillPanel\.PaintDetailColumn' "$variant skill detail content must render inside the shared panel surface."
    Assert-Contains $skillPanel 'Action<mGraphics,\s*int,\s*UiRect>\s+paintRow' "$variant skill-panel rows must accept caller-owned content."
    Assert-Contains $skillPanel 'Action<mGraphics>\s+paintContent' "$variant skill-panel details must accept caller-owned content."
    $cellLayers = Get-MethodBody $slot 'public static void Paint(mGraphics g, Item item'
    Assert-Contains $menu 'UiItemSlot\.Paint\(g, item, rect, selected, 0xB7A489, InventoryEquipmentBorderInset\)' "$variant inventory grid cells must use the shared item-slot component."
    Assert-Contains $menu 'UiItemSlot\.Paint\(graphics, item, iconRect, selected, 0xB7A489, InventoryEquipmentBorderInset\)' "$variant item-list rows must use the shared item-slot component."
    Assert-Contains $cellLayers 'g\.setColor\(backgroundColor\)' "$variant item cells must retain one neutral background regardless of crystal stars."
    Assert-NotContains $cellLayers 'getCrystalCellColor' "$variant crystal stars must not tint the item-cell background."
    Assert-Contains $cellLayers 'paintInventoryGridEffect[\s\S]*includeUpgradeLevel:\s*false[\s\S]*includeCrystalStars:\s*false' "$variant item-slot component must disable both upgrade and crystal-star background effects."
    Assert-Contains $cellLayers 'paintInventoryGridItemMarkers[\s\S]*paintCrystalSlotBorder:\s*false' "$variant item-slot component must keep crystal stars out of the equipment-border channel."
    Assert-Contains $cellLayers 'Panel\.paintEquipmentCellFrame[\s\S]*inset:\s*equipmentBorderInset' "$variant equipment-level frame must use the caller's inner inset."
    Assert-Contains $cellLayers 'g\.drawRect\(bounds\.X,\s*bounds\.Y,\s*bounds\.Width\s*-\s*1,\s*bounds\.Height\s*-\s*1\)' "$variant item cells must retain a stable outer grid divider."
    Assert-Contains $cellLayers 'PaintFocusFrame' "$variant focused cells must have an explicit focus glow."

    $focusFrame = Get-MethodBody $slot 'public static void PaintFocusFrame'
    Assert-NotContains $focusFrame 'bounds\.X\s*-\s*1|bounds\.Y\s*-\s*1' "$variant focus frame must never bleed outside its cell."
    Assert-Contains $focusFrame 'bounds\.X\s*\+\s*1[\s\S]*bounds\.Y\s*\+\s*1' "$variant focus glow must use a second inner line."

    $starPaint = Get-MethodBody $menu 'private static void PaintCrystalStars'
    Assert-Contains $starPaint 'Panel\.imgMaxStar' "$variant must paint empty crystal-star slots with the original asset."
    Assert-Contains $starPaint 'Panel\.imgStar' "$variant must paint filled crystal stars with the original asset."
    Assert-Contains $starPaint 'Panel\.imgStar8' "$variant must preserve the original high-tier crystal-star asset."

    $performAction = Get-MethodBody $menu 'private void PerformInventoryAction'
    Assert-Contains $performAction 'Service\.gI\(\)\.getItem' "$variant inventory actions must use the existing item-move protocol."
    Assert-Contains $performAction 'Service\.gI\(\)\.useItem' "$variant inventory actions must use the existing item-use protocol."
    Assert-Contains $performAction 'ModFunc\.GI\(\)\.perform' "$variant inventory actions must preserve the existing Auto Item flow."

    $panelPath = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\Panel.cs"
    $panel = Get-Content -LiteralPath $panelPath -Raw -Encoding UTF8
    Assert-Contains $panel 'public void paintInventoryGridEffect' "$variant Panel must expose its authoritative item-effect layer to CustomMenu."
    Assert-Contains $panel 'public void paintInventoryGridItemMarkers' "$variant Panel must expose its authoritative item-marker layer to CustomMenu."
    Assert-Contains $panel 'public static int getEquipmentCellColor' "$variant Panel must retain the original inventory upgrade-background palette."
    Assert-NotContains $panel 'getCrystalCellColor|crystalStarCellColors' "$variant must remove the obsolete crystal-star cell-background palette."
    Assert-Contains $panel 'public static void paintEquipmentCellFrame' "$variant Panel must expose its authoritative upgrade frame to CustomMenu."
    Assert-Contains $panel 'MaxEquipmentUpgradeLevel\s*=\s*10' "$variant max equipment level must have an explicit visual contract."
    Assert-Contains $panel 'EquipmentBorderLoopDurationMs\s*=\s*3000L' "$variant moving border lights must complete one loop in three seconds."
    Assert-Contains $panel 'equipmentUpgradeBorderColors[\s\S]*0xF2F4FF[\s\S]*0x42FF85[\s\S]*0x39A9FF[\s\S]*0xFF3B3B' "$variant non-max equipment tiers must use the high-contrast border palette."
    Assert-Contains $panel 'equipmentUpgradeSparkleColors' "$variant equipment tiers must provide brighter same-hue moving highlights."
    Assert-Contains $panel 'equipmentMaxUpgradeColors' "$variant max equipment level must define a multicolor border palette."
    $effectLayer = Get-MethodBody $panel 'public void paintInventoryGridEffect'
    Assert-Contains $effectLayer 'includeUpgradeLevel' "$variant inventory effects must allow CustomMenu to separate upgrade and crystal channels."
    Assert-Contains $effectLayer 'includeCrystalStars' "$variant inventory effects must allow CustomMenu to suppress crystal-star backgrounds."

    $markerLayer = Get-MethodBody $panel 'public void paintInventoryGridItemMarkers'
    Assert-Contains $markerLayer 'paintCrystalSlotBorder' "$variant inventory markers must allow CustomMenu to suppress the legacy crystal perimeter effect."

    $equipmentFrame = Get-MethodBody $panel 'public static void paintEquipmentCellFrame'
    Assert-Contains $equipmentFrame 'upgradeLevel\s*>=\s*MaxEquipmentUpgradeLevel' "$variant max level must select the rainbow-border renderer."
    Assert-Contains $equipmentFrame 'paintRainbowEquipmentBorder' "$variant max level must render an animated multicolor border."
    Assert-Contains $equipmentFrame 'paintAnimatedEquipmentBorder' "$variant non-max levels must render moving same-color border lights."
    Assert-Contains $panel 'paintEquipmentCellFrame[\s\S]*?bool\s+isSelected,\s*int\s+inset\s*=\s*0' "$variant shared equipment-frame API must preserve the original Panel default."
    Assert-Contains $equipmentFrame 'const\s+int\s+helperExpansion\s*=\s*2[\s\S]*frameX\s*=\s*x\s*\+\s*inset\s*\+\s*helperExpansion' "$variant every level frame must reserve the same two-pixel animated stroke."

    $animatedBorder = Get-MethodBody $panel 'private static void paintAnimatedEquipmentBorder'
    Assert-Contains $animatedBorder 'getEquipmentBorderAnimationPoint\(perimeter\)' "$variant normal equipment border lights must use the five-second time-based phase."
    Assert-NotContains $animatedBorder 'GameCanvas\.gameTick' "$variant normal border speed must not depend on frame ticks."
    Assert-Contains $animatedBorder 'trailPoint' "$variant moving equipment lights must retain a visible trailing dot."
    Assert-Contains $animatedBorder 'g\.drawRect\(x\s*-\s*2,\s*y\s*-\s*2,\s*width\s*\+\s*3,\s*height\s*\+\s*3\)' "$variant every non-max level must receive the full outer frame."
    Assert-NotContains $animatedBorder 'g\.drawRect\(x,\s*y,\s*width\s*-\s*1,\s*height\s*-\s*1\)' "$variant non-max frame must stay at two lines instead of the previous three-line thickness."
    Assert-Contains $animatedBorder 'headHaloPoint' "$variant every non-max level must have a clearly visible moving light halo."
    Assert-Contains $animatedBorder 'sparkleColor\s*=\s*equipmentUpgradeSparkleColors\[upgradeLevel\s*-\s*1\]' "$variant moving lights must use a brighter color from the same equipment tier."
    Assert-NotContains $animatedBorder 'upgradeLevel\s*>=\s*7' "$variant two-layer animation must not be restricted to high equipment levels."

    $rainbowBorder = Get-MethodBody $panel 'private static void paintRainbowEquipmentBorder'
    Assert-Contains $rainbowBorder 'equipmentMaxUpgradeColors' "$variant max-level border must cycle through the full palette."
    Assert-Contains $rainbowBorder 'getEquipmentBorderAnimationPoint\(perimeter\)' "$variant max-level highlights must use the same five-second movement phase."
    Assert-NotContains $rainbowBorder 'GameCanvas\.gameTick' "$variant max-level highlight speed must not depend on frame ticks."
    Assert-NotContains $rainbowBorder 'g\.drawRect\(x,\s*y,\s*width\s*-\s*1,\s*height\s*-\s*1\)' "$variant max-level frame must use the same thinner two-line geometry."
    Assert-Contains $rainbowBorder 'paintEquipmentBorderPoint[\s\S]*size:\s*3' "$variant max-level moving highlights must be large enough to remain visible."

    $animationPoint = Get-MethodBody $panel 'private static int getEquipmentBorderAnimationPoint'
    Assert-Contains $animationPoint 'mSystem\.currentTimeMillis\(\)\s*%\s*EquipmentBorderLoopDurationMs' "$variant border animation must use elapsed real time."
    Assert-Contains $animationPoint 'elapsed\s*\*\s*perimeter\s*/\s*EquipmentBorderLoopDurationMs' "$variant elapsed time must map linearly onto one full perimeter."
}

Write-Host 'Custom menu inventory-tab regression checks passed.'
