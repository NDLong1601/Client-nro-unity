$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')

function Require([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
}

foreach ($variant in @('Game1', 'Game2')) {
    $dir = Join-Path $root "Assets\Scripts\Assembly-CSharp\$variant\UI\CustomMenu"
    $core = Get-Content (Join-Path $dir 'CustomMenuScr.cs') -Raw -Encoding UTF8
    $chrome = Get-Content (Join-Path $dir 'CustomMenuScr.Chrome.cs') -Raw -Encoding UTF8
    $logic = Get-Content (Join-Path $dir 'CustomMenuScr.Disciple.cs') -Raw -Encoding UTF8
    $view = Get-Content (Join-Path $dir 'CustomMenuScr.Disciple.View.cs') -Raw -Encoding UTF8
    $function = Get-Content (Join-Path $dir 'CustomMenuScr.Function.cs') -Raw -Encoding UTF8

    Require ($core.Contains('MainTabNames = new string[]') -and $core.Contains('"Đệ tử"')) "$variant must expose the disciple main tab."
    Require ($chrome.Contains('PaintDiscipleTabContent(g)')) "$variant must render the disciple tab."
    Require ($logic.Contains('Service.gI().petInfo()')) "$variant must request the original disciple payload."
    Require ($logic.Contains('Char.myPetz()')) "$variant must bind the original disciple model."
    Require ($logic.Contains('DiscipleStatusIds = { 0, 2, 1, 3, 4, 5 }')) "$variant status buttons must map to original server IDs."
    Require ($logic.Contains('Service.gI().petStatus(status)')) "$variant must use the original status command."
    Require ($logic.Contains('Service.gI().upPotential(true, potentialType, 1)')) "$variant must use the original potential command."
    Require ($logic.Contains('Service.gI().getItem(7, (sbyte)slot)')) "$variant must use the original unequip command."
    foreach ($field in @('cName', 'cPower', 'currStrLevel', 'cStamina', 'cMaxStamina',
        'cHP', 'cHPFull', 'cMP', 'cMPFull', 'cDamFull', 'cDefull', 'cCriticalFull',
        'cTiemNang', 'arrItemBody', 'arrPetSkill')) {
        Require ($view.Contains('pet.' + $field)) "$variant must render live disciple field: $field"
    }
    Require ($view.Contains('UiItemSlot.Paint(') -and $view.Contains('UiProgressBar.PaintFlat(') -and
        $view.Contains('UiMenuTheme.PaintButton(') -and $core.Contains('UiTabBar')) "$variant must reuse shared UI components."
    Require ($core.Contains('MainTabRowHeight = 48') -and $core.Contains('_mainTabScrollAdapter.UpdateKey(_inputContext, out int clickedTab)') -and
        $core.Contains('_mainTabScrollAdapter.ScrollByWheel(wheelDelta)')) "$variant main tabs must support fixed-size scrolling and correct hit testing."
    foreach ($icon in @('main_task', 'main_inventory', 'main_skill', 'main_clan', 'main_function', 'main_disciple')) {
        Require ($core.Contains('/custom_menu/' + $icon + '.png')) "$variant must load the supplied main-tab icon: $icon"
    }
    Require ($view.Contains('SmallImage.drawSmallImage(g, pet.avatarz()') -and
        !$view.Contains('pet.paintCharBody(g, portrait')) "$variant disciple information must show the original profile avatar."
    Require ($view.Contains('mFont.tahoma_7_orange.drawString(g,') -and
        $view.Contains('NinjaUtil.getMoneys(pet.cPower)')) "$variant disciple power must use the new highlight color."
    Require ($view.IndexOf('if (rowIndex < 5)') -lt $view.IndexOf('Skill skill = pet.arrPetSkill[skillIndex]')) "$variant potential rows must precede learned skills."
    Require ($logic.Contains('int potentialType = row;') -and $view.Contains('GetDisciplePotentialValue(pet, potentialType)')) "$variant potential interactions and displayed base stats must follow the new row order."
    Require ($view.Contains('GameScr.imgSkill') -and $view.Contains('GameScr.imgSkill2')) "$variant disciple rows must use the original skill icon frames."
    Require ($view.Contains('mFont.tahoma_7b_blue.drawString') -and $view.Contains('mFont.tahoma_7_green2.drawString')) "$variant disciple stats must use the original blue and green fonts."
    Require ($view.Contains('mResources.potential') -and $view.Contains('mResources.increase') -and
        $view.Contains('Res.formatNumber2(cost)')) "$variant disciple potential cost and increase text must match the original panel."
    Require (!$function.Contains('FunctionViewDisciple')) "$variant must remove the old function submenu."
    foreach ($name in @('CustomMenuScr.Disciple.cs', 'CustomMenuScr.Disciple.View.cs')) {
        Require (Test-Path (Join-Path $dir ($name + '.meta'))) "$variant missing Unity metadata for $name"
    }
}

$icons = Join-Path $root 'Assets\Resources\res\x4\custom_menu'
foreach ($name in @('main_task', 'main_inventory', 'main_skill', 'main_clan', 'main_function', 'main_disciple')) {
    $path = Join-Path $icons ($name + '.png')
    Require (Test-Path $path) "Missing main-tab icon: $name"
    $meta = Get-Content ($path + '.meta') -Raw -Encoding UTF8
    Require ($meta.Contains('maxTextureSize: 256')) "Main-tab icon must be imported at a bounded texture size: $name"
}

$game1 = Join-Path $root 'Assets\Scripts\Assembly-CSharp\Game1\UI\CustomMenu'
$game2 = Join-Path $root 'Assets\Scripts\Assembly-CSharp\Game2\UI\CustomMenu'
foreach ($name in @('CustomMenuScr.cs', 'CustomMenuScr.Chrome.cs', 'CustomMenuScr.Navigation.cs',
    'CustomMenuScr.Function.cs', 'CustomMenuScr.Function.View.cs', 'CustomMenuScr.Disciple.cs',
    'CustomMenuScr.Disciple.View.cs')) {
    $a = Get-Content (Join-Path $game1 $name) -Raw -Encoding UTF8
    $b = Get-Content (Join-Path $game2 $name) -Raw -Encoding UTF8
    Require ($a.Replace('Game1', 'Game2') -eq $b) "Game1/Game2 parity mismatch: $name"
}

Write-Host 'Custom menu disciple-tab regression checks passed.'
