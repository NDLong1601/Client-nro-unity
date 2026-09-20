$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('ui-phase6-' + [guid]::NewGuid().ToString('N'))

function Require([bool]$condition, [string]$message) {
    if (!$condition) {
        throw $message
    }
}

function Require-Contains([string]$content, [string]$expected, [string]$message) {
    if (!$content.Contains($expected)) {
        throw $message
    }
}

function Require-OrderedContains([string]$content, [string[]]$expected, [string]$message) {
    $searchFrom = 0
    foreach ($marker in $expected) {
        $position = $content.IndexOf($marker, $searchFrom, [System.StringComparison]::Ordinal)
        if ($position -lt 0) {
            throw ($message + ': missing or out of order: ' + $marker)
        }
        $searchFrom = $position + $marker.Length
    }
}

function Get-MethodBody([string]$content, [string]$signature) {
    $start = $content.IndexOf($signature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) {
        throw ("Cannot find method signature: " + $signature)
    }
    $openBrace = $content.IndexOf('{', $start)
    $depth = 0
    for ($index = $openBrace; $index -lt $content.Length; $index++) {
        if ($content[$index] -eq '{') { $depth++ }
        if ($content[$index] -eq '}') {
            $depth--
            if ($depth -eq 0) {
                return $content.Substring($openBrace, $index - $openBrace + 1)
            }
        }
    }
    throw ("Cannot find closing brace for: " + $signature)
}

function Get-UnityMetaGuid([string]$metaPath) {
    Require (Test-Path -LiteralPath $metaPath) ('Unity metadata file not found: ' + $metaPath)
    $metaContent = [System.IO.File]::ReadAllText($metaPath)
    $guidMatch = [regex]::Match($metaContent, '(?m)^guid:\s*([0-9a-fA-F]{32})\s*$')
    Require $guidMatch.Success ('Unity metadata GUID is missing or invalid: ' + $metaPath)
    return $guidMatch.Groups[1].Value.ToLowerInvariant()
}

function Assert-UniqueUnityMetaGuids([string]$assetsRoot) {
    $guidOwners = [System.Collections.Generic.Dictionary[string,string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $metaFiles = Get-ChildItem -LiteralPath $assetsRoot -Recurse -File -Filter '*.meta'

    foreach ($metaFile in $metaFiles) {
        $guid = Get-UnityMetaGuid $metaFile.FullName
        if ($guidOwners.ContainsKey($guid)) {
            throw ('Duplicate Unity metadata GUID ' + $guid + ': ' + $guidOwners[$guid] + ' and ' + $metaFile.FullName)
        }
        $guidOwners.Add($guid, $metaFile.FullName)
    }

    Require ($guidOwners.Count -gt 0) ('No Unity metadata GUIDs found under: ' + $assetsRoot)
    return $guidOwners.Count
}

try {
    Write-Output '--- 1. Kiem tra workspace, toolchain va cac artifact Phase 0-5 ---'
    $scriptsDir = Join-Path $projectRoot 'Assets\Scripts\Assembly-CSharp'
    $uiSharedDir = Join-Path $scriptsDir 'UIShared'

    # Check project version and Unity toolchain
    $projectVersionPath = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
    Require (Test-Path -LiteralPath $projectVersionPath) 'ProjectVersion.txt does not exist'
    $projectVersionContent = [System.IO.File]::ReadAllText($projectVersionPath)
    $versionMatch = [regex]::Match($projectVersionContent, 'm_EditorVersion:\s*([^\r\n]+)')
    Require $versionMatch.Success 'Unable to determine Unity editor version from ProjectVersion.txt'
    $unityVersion = $versionMatch.Groups[1].Value.Trim()
    $unityEditorRoot = Join-Path ${env:ProgramFiles} ('Unity\Hub\Editor\' + $unityVersion + '\Editor')
    $compilerPath = Join-Path $unityEditorRoot 'Data\DotNetSdkRoslyn\csc.dll'
    $monoPath = Join-Path $unityEditorRoot 'Data\MonoBleedingEdge\bin\mono.exe'
    $monoReferenceDir = Join-Path $unityEditorRoot 'Data\MonoBleedingEdge\lib\mono\4.8-api'
    $dotnetCommand = Get-Command dotnet -ErrorAction Stop

    Require (Test-Path -LiteralPath $compilerPath) ('Unity Roslyn compiler not found: ' + $compilerPath)
    Require (Test-Path -LiteralPath $monoPath) ('Unity Mono runtime not found: ' + $monoPath)

    $compilerReferences = @('mscorlib.dll', 'System.dll', 'System.Core.dll') | ForEach-Object {
        $referencePath = Join-Path $monoReferenceDir $_
        Require (Test-Path -LiteralPath $referencePath) ('Unity Mono reference not found: ' + $referencePath)
        '/reference:"' + $referencePath + '"'
    }

    $assetMetaCount = Assert-UniqueUnityMetaGuids (Join-Path $projectRoot 'Assets')
    Write-Output ('Unity metadata GUID integrity: OK (' + $assetMetaCount + ' unique GUIDs)')

    # Verify UIShared files
    Require (Test-Path -LiteralPath (Join-Path $scriptsDir 'UIShared.meta')) 'UIShared.meta not found'
    $expectedSharedFiles = @(
        'UiRect.cs',
        'UiMetrics.cs',
        'UiColorTokens.cs',
        'UiLayout.cs',
        'UiInputType.cs',
        'TextAlign.cs',
        'UiListRange.cs',
        'UiListLayout.cs',
        'UiVerticalListState.cs'
    )
    foreach ($sharedFile in $expectedSharedFiles) {
        $fullPath = Join-Path $uiSharedDir $sharedFile
        Require (Test-Path -LiteralPath $fullPath) ('UIShared missing file: ' + $sharedFile)
        Require (Test-Path -LiteralPath ($fullPath + '.meta')) ('UIShared missing meta: ' + $sharedFile + '.meta')
    }

    # Verify Phase 1-5 production files exist
    $expectedPriorFiles = @(
        'UI\UiInputContext.cs',
        'UI\Adapters\UiRenderState.cs',
        'UI\Adapters\TextFieldAdapter.cs',
        'UI\Components\UiListRow.cs',
        'UI\Pilots\PKHistoryView.cs',
        'UI\Pilots\TopRankingView.cs',
        'UI\PanelContent\TopPanelContent.cs'
    )
    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        foreach ($relFile in $expectedPriorFiles) {
            $fullPath = Join-Path $gameDir $relFile
            Require (Test-Path -LiteralPath $fullPath) ($game + ' missing prior phase file: ' + $relFile)
        }
    }

    # Verify Phase 6 test seam files
    $regressionSource = Join-Path $PSScriptRoot 'UiComponentPhase6Regression.cs'
    $stubSource = Join-Path $PSScriptRoot 'UiComponentPhase6LegacyStubs.cs'
    $rendererRegressionSource = Join-Path $PSScriptRoot 'UiComponentPhase6RendererRegression.cs'
    Require (Test-Path -LiteralPath $regressionSource) 'UiComponentPhase6Regression.cs not found'
    Require (Test-Path -LiteralPath $stubSource) 'UiComponentPhase6LegacyStubs.cs not found'
    Require (Test-Path -LiteralPath $rendererRegressionSource) 'UiComponentPhase6RendererRegression.cs not found'

    $regressionContract = [System.IO.File]::ReadAllText($regressionSource)
    Require (!$regressionContract.Contains('UiInputType.Keyboard')) 'Phase 6 regression must use the existing parameterless UiInputContext API'
    Require (!$regressionContract.Contains('UiInputType.Touch')) 'Phase 6 regression must use GameCanvas for pointer state'
    Require (!$regressionContract.Contains('range.Length')) 'Phase 6 regression must use UiListRange.Count'
    Require-Contains $regressionContract 'new UiInputContext()' 'Phase 6 regression must use the existing UiInputContext constructor'
    Require-Contains $regressionContract 'SmallImage.DrawSmallImageCount == range.Count' 'Phase 6 regression must verify renderer culling through observed draw calls'

    Write-Output 'Workspace, toolchain, Phase 0-5 artifacts, and Phase 6 test files: OK'

    Write-Output '--- 2. Kiem tra caller, protocol, network va action invariants cua Enemy hien tai ---'
    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        $panelPath = Join-Path $gameDir 'Panel.cs'
        $controllerPath = Join-Path $gameDir 'Controller.cs'
        $servicePath = Join-Path $gameDir 'Service.cs'

        Require (Test-Path -LiteralPath $panelPath) ($game + ' Panel.cs not found')
        Require (Test-Path -LiteralPath $controllerPath) ($game + ' Controller.cs not found')
        Require (Test-Path -LiteralPath $servicePath) ($game + ' Service.cs not found')

        $panel = [System.IO.File]::ReadAllText($panelPath)
        $controller = [System.IO.File]::ReadAllText($controllerPath)
        $service = [System.IO.File]::ReadAllText($servicePath)

        # 2.1 Panel constants & collections
        Require-Contains $panel 'public const int TYPE_ENEMY = 16;' ($game + ' Panel missing TYPE_ENEMY = 16')
        Require-Contains $panel 'public MyVector vEnemy = new MyVector();' ($game + ' Panel missing vEnemy')

        # 2.2 Panel methods
        Require-Contains $panel 'public void setTypeEnemy()' ($game + ' Panel missing setTypeEnemy')
        Require-Contains $panel 'public void setTabEnemy()' ($game + ' Panel missing setTabEnemy')
        Require-Contains $panel 'private void paintEnemy(mGraphics g)' ($game + ' Panel missing paintEnemy')
        Require-Contains $panel 'private void doFireEnemy()' ($game + ' Panel missing doFireEnemy')
        Require-Contains $panel 'private void addFriend(InfoItem info)' ($game + ' Panel missing addFriend helper')

        $setTypeEnemy = Get-MethodBody $panel 'public void setTypeEnemy()'
        Require-OrderedContains $setTypeEnemy @(
            'type = 16;',
            'setType(0);',
            'ITEM_HEIGHT = 24;',
            'selected = (GameCanvas.isTouch ? (-1) : 0);',
            'setTabEnemy();'
        ) ($game + ' setTypeEnemy initialization order changed')

        $setTabEnemy = Get-MethodBody $panel 'public void setTabEnemy()'
        Require-OrderedContains $setTabEnemy @(
            'currentListLength = vEnemy.size();',
            'cmyLim = currentListLength * ITEM_HEIGHT - hScroll;',
            'cmy = (cmtoY = cmyLast[currentTabIndex]);',
            'if (cmy < 0)',
            'if (cmy > cmyLim)',
            'if (selected > currentListLength - 1)',
            'cmx = (cmtoX = 0);'
        ) ($game + ' setTabEnemy scroll/selection contract changed')

        $paintEnemy = Get-MethodBody $panel 'private void paintEnemy(mGraphics g)'
        Require-Contains $paintEnemy 'paintScrollArrow(g);' ($game + ' paintEnemy must retain paintScrollArrow')
        $legacyRenderer = if ($panel.Contains('private void paintEnemyLegacy(')) {
            Get-MethodBody $panel 'private void paintEnemyLegacy('
        } else {
            $paintEnemy
        }
        foreach ($marker in @(
            'g.setClip(xScroll, yScroll, wScroll, hScroll);',
            'mResources.no_enemy',
            'int num3 = 24;',
            '15196114',
            '16383818',
            '9993045',
            '9541120',
            'infoItem.charInfo.headICON != -1',
            'GameScr.parts[infoItem.charInfo.head]',
            'infoItem.isOnline',
            'mFont.tahoma_7b_green',
            'mFont.tahoma_7_blue',
            'mFont.tahoma_7_grey'
        )) {
            Require-Contains $legacyRenderer $marker ($game + ' paintEnemy missing legacy rendering contract: ' + $marker)
        }
        Require ($legacyRenderer.Contains('g.translate(0, -cmy);') -or $legacyRenderer.Contains('g.translate(0, -scrollY);')) ($game + ' paintEnemy missing legacy translation contract')

        $doFireEnemy = Get-MethodBody $panel 'private void doFireEnemy()'
        Require-OrderedContains $doFireEnemy @(
            'selected >= 0 && vEnemy.size() != 0',
            'currInfoItem = selected;',
            'new Command(mResources.REVENGE, this, 10000, (InfoItem)vEnemy.elementAt(currInfoItem))',
            'new Command(mResources.DELETE, this, 10001, (InfoItem)vEnemy.elementAt(currInfoItem))',
            'GameCanvas.menu.startAt(myVector, X, (selected + 1) * ITEM_HEIGHT - cmy + yScroll);',
            'addFriend((InfoItem)vEnemy.elementAt(selected));'
        ) ($game + ' doFireEnemy action/menu contract changed')

        # 2.3 Panel.perform actions
        $revengeAction = Get-MethodBody $panel 'if (idAction == 10000)'
        Require-OrderedContains $revengeAction @(
            'InfoItem infoItem4 = (InfoItem)p;',
            'Service.gI().enemy(1, infoItem4.charInfo.charID);',
            'GameCanvas.panel.hideNow();'
        ) ($game + ' action 10000 contract changed')

        $deleteAction = Get-MethodBody $panel 'if (idAction == 10001)'
        Require-OrderedContains $deleteAction @(
            'InfoItem infoItem5 = (InfoItem)p;',
            'Service.gI().enemy(2, infoItem5.charInfo.charID);',
            'InfoDlg.showWait();'
        ) ($game + ' action 10001 contract changed')

        # 2.4 Account menu trigger
        $doFireAccount = Get-MethodBody $panel 'private void doFireAccount()'
        Require-OrderedContains $doFireAccount @(
            'case 2:',
            'Service.gI().enemy(0, -1);',
            'InfoDlg.showWait();'
        ) ($game + ' doFireAccount case 2 contract changed')

        # 2.5 Controller packet -99
        $packetStart = $controller.IndexOf('case -99:', [System.StringComparison]::Ordinal)
        Require ($packetStart -ge 0) ($game + ' Controller packet -99 block not found')
        $packetEnd = $controller.IndexOf('case -98:', $packetStart, [System.StringComparison]::Ordinal)
        Require ($packetEnd -gt $packetStart) ($game + ' Controller packet -99 block is not terminated by case -98')
        $packet99 = $controller.Substring($packetStart, $packetEnd - $packetStart)
        Require-OrderedContains $packet99 @(
            'case -99:',
            'sbyte enemyResponseType = msg.reader().readByte();',
            'if (enemyResponseType == 0)',
            'GameCanvas.panel.vEnemy.removeAllElements();',
            'msg.reader().readUnsignedByte();',
            'char7.charID = msg.reader().readInt();',
            'char7.head = msg.reader().readShort();',
            'char7.headICON = msg.reader().readShort();',
            'char7.body = msg.reader().readShort();',
            'char7.leg = msg.reader().readShort();',
            'char7.bag = msg.reader().readShort();',
            'char7.cName = msg.reader().readUTF();',
            'new InfoItem(msg.reader().readUTF());',
            'msg.reader().readBoolean();',
            'infoItem.charInfo = char7;',
            'infoItem.isOnline = isOnline;',
            'GameCanvas.panel.vEnemy.addElement(infoItem);',
            'GameCanvas.panel.onEnemyListReceived();',
            'else',
            'InfoDlg.hide();'
        ) ($game + ' packet -99 read order changed')
        $hideWaitIndex = $packet99.IndexOf('InfoDlg.hide();', [System.StringComparison]::Ordinal)
        $lifecycleRouteIndex = $packet99.IndexOf('GameCanvas.panel.onEnemyListReceived();', [System.StringComparison]::Ordinal)
        Require ($hideWaitIndex -gt $lifecycleRouteIndex) ($game + ' packet -99 must not hide a global wait dialog before lifecycle routing')

        # 2.6 Service network message -99
        $serviceEnemy = Get-MethodBody $service 'public void enemy(sbyte b, int charID)'
        Require-OrderedContains $serviceEnemy @(
            'message = new Message(-99);',
            'message.writer().writeByte(b);',
            'if (b == 1 || b == 2)',
            'message.writer().writeInt(charID);',
            'session.sendMessage(message);'
        ) ($game + ' Service.enemy serialization contract changed')

        # 2.7 Legacy routing baseline
        $updateKey = Get-MethodBody $panel 'public void updateKey()'
        $enemyCase = $updateKey.IndexOf('case 16:', [System.StringComparison]::Ordinal)
        Require ($enemyCase -ge 0) ($game + ' updateKey missing case 16')
        $enemyScroll = $updateKey.IndexOf('updateKeyScrollView();', $enemyCase, [System.StringComparison]::Ordinal)
        Require ($enemyScroll -gt $enemyCase) ($game + ' updateKey case 16 must route to legacy scroll before Phase 6 integration')
        Require-Contains $panel 'public void updateScroolMouse(int a)' ($game + ' Panel missing mouse-wheel routing')
        Require-Contains $panel 'public void moveCamera()' ($game + ' Panel missing camera interpolation')
    }
    Write-Output 'Enemy caller, protocol, network, and action invariants: OK'

    Write-Output '--- 3. Kiem tra API parity Game1/Game2 cho cac duong Enemy ---'
    $panel1 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game1\Panel.cs'))
    $panel2 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game2\Panel.cs'))
    $enemyPanelMethods = @(
        'public void setTypeEnemy()',
        'public void setTabEnemy()',
        'private void paintEnemy(mGraphics g)',
        'private void paintEnemyLegacy(mGraphics g, int scrollY, int selectedIndex, int itemCount)',
        'private void ensureEnemyPanelContent()',
        'private bool isEnemyContentActive()',
        'private bool isEnemyContentDragging()',
        'private void cancelPendingEnemyAction()',
        'private void leaveEnemyLifecycle()',
        'private void unbindEnemyContentWhenLeavingType()',
        'private void updateKeyEnemyContent()',
        'private bool isEnemyMenuContextValid(object p)',
        'private void invalidateEnemyMenuContext()',
        'public EnemyListDisposition onEnemyListReceived()',
        'private void executeEnemyAction(EnemyContentAction action)'
    )
    foreach ($signature in $enemyPanelMethods) {
        $body1 = (Get-MethodBody $panel1 $signature).Replace('Game1', 'Game2').Replace("`r`n", "`n")
        $body2 = (Get-MethodBody $panel2 $signature).Replace("`r`n", "`n")
        Require ($body1 -eq $body2) ('Game1/Game2 Enemy panel integration parity mismatch: ' + $signature)
    }

    $c1 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game1\Controller.cs'))
    $c2 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game2\Controller.cs'))
    $start1 = $c1.IndexOf('case -99:')
    $end1 = $c1.IndexOf('case -98:', $start1)
    $p99_1 = $c1.Substring($start1, $end1 - $start1).Trim().Replace("`r`n", "`n")
    $start2 = $c2.IndexOf('case -99:')
    $end2 = $c2.IndexOf('case -98:', $start2)
    $p99_2 = $c2.Substring($start2, $end2 - $start2).Trim().Replace("`r`n", "`n")
    $p99_1Norm = [regex]::Replace($p99_1, 'num1\d\d', 'loopVar')
    $p99_2Norm = [regex]::Replace($p99_2, 'num1\d\d', 'loopVar')
    Require ($p99_1Norm -eq $p99_2Norm) 'Game1/Game2 Controller.cs packet -99 parity mismatch'

    $s1 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game1\Service.cs'))
    $s2 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game2\Service.cs'))
    $sStart1 = $s1.IndexOf('public void enemy(')
    $sEnd1 = $s1.IndexOf('public void ', $sStart1 + 20)
    $enemy1 = $s1.Substring($sStart1, $sEnd1 - $sStart1).Trim().Replace("`r`n", "`n")
    $sStart2 = $s2.IndexOf('public void enemy(')
    $sEnd2 = $s2.IndexOf('public void ', $sStart2 + 20)
    $enemy2 = $s2.Substring($sStart2, $sEnd2 - $sStart2).Trim().Replace("`r`n", "`n")
    Require ($enemy1 -eq $enemy2) 'Game1/Game2 Service.cs enemy() parity mismatch'

    Write-Output 'Game1/Game2 scoped Enemy integration parity: OK'

    Write-Output '--- 4A. Kiem tra Renderer Phase 6.1 (EnemyListView) ---'
    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        $viewPath = Join-Path $gameDir 'UI\Pilots\EnemyListView.cs'
        $viewMeta = Join-Path $gameDir 'UI\Pilots\EnemyListView.cs.meta'

        Require (Test-Path -LiteralPath $viewPath) ($game + ' missing file: UI\Pilots\EnemyListView.cs')
        Require (Test-Path -LiteralPath $viewMeta) ($game + ' missing meta: UI\Pilots\EnemyListView.cs.meta')
        $viewMetaGuid = Get-UnityMetaGuid $viewMeta
        Require (![string]::IsNullOrEmpty($viewMetaGuid)) ($game + ' EnemyListView metadata GUID is invalid')

        $viewContent = [System.IO.File]::ReadAllText($viewPath)
        Require-Contains $viewContent ('namespace ' + $game + '.UI.Pilots') ($game + ' EnemyListView incorrect namespace')
        Require-Contains $viewContent 'public const int ITEM_HEIGHT = 24;' ($game + ' EnemyListView missing ITEM_HEIGHT = 24')
        Require-Contains $viewContent 'public const int AVATAR_COLUMN_WIDTH = 24;' ($game + ' EnemyListView missing AVATAR_COLUMN_WIDTH = 24')
        Require-Contains $viewContent 'public static void Paint(' ($game + ' EnemyListView missing Paint method')

        Require (!$viewContent.Contains('Service.gI')) ($viewPath + ' must not contain Service.gI')
        Require (!$viewContent.Contains('loadImage(')) ($viewPath + ' must not call loadImage')
        Require (!$viewContent.Contains('setClip(0, 0, GameCanvas.w, GameCanvas.h)')) ($viewPath + ' must not reset clip to fullscreen')
        Require (!$viewContent.Contains('msg.reader()')) ($viewPath + ' must not parse network message')
        Require (!$viewContent.Contains('Message msg')) ($viewPath + ' must not receive network message')
        Require (!$viewContent.Contains('GameCanvas.panel2 =')) ($viewPath + ' must not mutate GameCanvas.panel2')

        # Check Panel.cs integration for renderer
        $panelPath = Join-Path $gameDir 'Panel.cs'
        $panelContent = [System.IO.File]::ReadAllText($panelPath)
        Require-Contains $panelContent 'public static bool USE_NEW_ENEMY_UI = true;' ($game + ' Panel missing USE_NEW_ENEMY_UI')
        Require-Contains $panelContent 'private void paintEnemyLegacy(' ($game + ' Panel missing paintEnemyLegacy helper')

        $paintEnemyMethod = Get-MethodBody $panelContent 'private void paintEnemy(mGraphics g)'
        Require-Contains $paintEnemyMethod 'USE_NEW_ENEMY_UI' ($game + ' paintEnemy must branch on USE_NEW_ENEMY_UI')
        Require-Contains $paintEnemyMethod 'EnemyListView.Paint(' ($game + ' paintEnemy must call EnemyListView.Paint')
        Require-Contains $paintEnemyMethod 'paintEnemyLegacy(' ($game + ' paintEnemy must call paintEnemyLegacy when disabled')
        Require-Contains $paintEnemyMethod 'paintScrollArrow(g);' ($game + ' paintEnemy must call paintScrollArrow')
        foreach ($arg in @('cmy', 'selected', 'vEnemy', 'currentListLength')) {
            Require-Contains $paintEnemyMethod $arg ($game + ' paintEnemy delegation must pass ' + $arg)
        }
    }

    # Normalized parity of EnemyListView.cs
    $view1 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game1\UI\Pilots\EnemyListView.cs'))
    $view2 = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game2\UI\Pilots\EnemyListView.cs'))
    Require ($view1.Replace('Game1', 'Game2') -eq $view2) 'Game1/Game2 EnemyListView.cs parity mismatch'

    # Compile and execute renderer regression tests for Game1 and Game2
    $uiSharedFiles = Get-ChildItem -LiteralPath $uiSharedDir -Filter '*.cs' | Select-Object -ExpandProperty FullName
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        $rendererSources = @(
            (Join-Path $gameDir 'UI\UiInputContext.cs'),
            (Join-Path $gameDir 'UI\Adapters\UiRenderState.cs'),
            (Join-Path $gameDir 'UI\Components\UiListRow.cs'),
            (Join-Path $gameDir 'UI\Pilots\EnemyListView.cs')
        )
        $gameStub = Join-Path $outputDirectory ($game + '-Phase6RendererLegacyStubs.cs')
        $gameRegression = Join-Path $outputDirectory ($game + '-Phase6RendererRegression.cs')

        $stubContent = [System.IO.File]::ReadAllText($stubSource)
        $rendererRegressionContent = [System.IO.File]::ReadAllText($rendererRegressionSource)
        if ($game -eq 'Game2') {
            $stubContent = $stubContent.Replace('Game1', 'Game2')
            $rendererRegressionContent = $rendererRegressionContent.Replace('Game1', 'Game2')
        }
        [System.IO.File]::WriteAllText($gameStub, $stubContent)
        [System.IO.File]::WriteAllText($gameRegression, $rendererRegressionContent)

        $outputExe = Join-Path $outputDirectory ($game + '-UiComponentPhase6RendererRegression.exe')
        $compileSources = @($gameStub, $gameRegression) + $uiSharedFiles + $rendererSources
        $responseFile = Join-Path $outputDirectory ($game + '-phase6-renderer-compile.rsp')
        $compilerArguments = @(
            '/nologo',
            '/nostdlib+',
            '/target:exe',
            '/langversion:9.0',
            '/warnaserror+',
            ('/out:"' + $outputExe + '"')
        ) + $compilerReferences + ($compileSources | ForEach-Object { '"' + $_ + '"' })
        [System.IO.File]::WriteAllLines($responseFile, $compilerArguments)

        $compilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $responseFile) 2>&1
        if ($LASTEXITCODE -ne 0) {
            $compilerOutput | Write-Output
            throw ($game + ' Phase 6 renderer compilation failed with exit code: ' + $LASTEXITCODE)
        }
        Require (Test-Path -LiteralPath $outputExe) ($game + ' Phase 6 renderer compilation did not generate executable')

        $testOutput = & $monoPath $outputExe
        if ($LASTEXITCODE -ne 0) {
            throw ($game + ' Phase 6 renderer regression failed with exit code: ' + $LASTEXITCODE)
        }
        Require ($testOutput -contains ('UI_COMPONENT_PHASE6_RENDERER_' + $game + '_OK')) ($game + ' renderer completion marker missing')
        Write-Output ($game + ' Phase 6 renderer regression: OK')
    }

    # Full Assembly-CSharp compile with new renderer sources
    $unityResponseCandidates = Get-ChildItem -Path (Join-Path $projectRoot 'Library\Bee\artifacts') -Filter 'Assembly-CSharp.rsp' -Recurse |
        Sort-Object LastWriteTime -Descending
    $unityResponseFile = $null
    foreach ($candidate in $unityResponseCandidates) {
        $candidateContent = [System.IO.File]::ReadAllText($candidate.FullName)
        if ($candidateContent.Contains('-define:UNITY_EDITOR')) {
            $unityResponseFile = $candidate
            break
        }
    }
    Require ($null -ne $unityResponseFile) 'Unity Editor Assembly-CSharp response file was not found under Library/Bee/artifacts'

    $fullAssemblyPath = Join-Path $outputDirectory 'Assembly-CSharp-Phase6RendererVerification.dll'
    $fullReferencePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase6RendererVerification.ref.dll'
    $fullResponsePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase6RendererVerification.rsp'

    $newPhase6RendererSources = @(
        (Join-Path $scriptsDir 'UIShared\UiVerticalListState.cs'),
        (Join-Path $scriptsDir 'Game1\UI\Pilots\EnemyListView.cs'),
        (Join-Path $scriptsDir 'Game2\UI\Pilots\EnemyListView.cs')
    )

    $existingRspLines = Get-Content -LiteralPath $unityResponseFile.FullName
    $fullResponseLines = [System.Collections.Generic.List[string]]::new()

    foreach ($line in $existingRspLines) {
        if ($line -match '^-out:') {
            $fullResponseLines.Add('-out:"' + $fullAssemblyPath + '"')
        }
        elseif ($line -match '^-refout:') {
            $fullResponseLines.Add('-refout:"' + $fullReferencePath + '"')
        }
        else {
            $fullResponseLines.Add($line)
        }
    }

    $normalizedExisting = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($line in $existingRspLines) {
        $clean = $line.Trim().Trim('"').Replace('/', '\')
        $normalizedExisting.Add($clean) | Out-Null
    }

    foreach ($source in $newPhase6RendererSources) {
        $cleanSource = $source.Trim().Trim('"').Replace('/', '\')
        if (!$normalizedExisting.Contains($cleanSource)) {
            $fullResponseLines.Add('"' + $source + '"')
        }
    }

    [System.IO.File]::WriteAllLines($fullResponsePath, $fullResponseLines)

    $fullCompilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $fullResponsePath) 2>&1
    if ($LASTEXITCODE -ne 0) {
        $fullCompilerOutput | Write-Output
        throw ('Full Assembly-CSharp compilation failed with exit code: ' + $LASTEXITCODE)
    }
    Require (Test-Path -LiteralPath $fullAssemblyPath) 'Full Assembly-CSharp compilation did not produce assembly'
    Write-Output 'Full Unity Assembly-CSharp compilation with EnemyListView: OK'

    Write-Output 'UI_COMPONENT_PHASE6_1_RENDERER_OK'

    Write-Output '--- 4B. Kiem tra production files va integration markers Phase 6.2 (Expected RED) ---'
    $expectedGameFiles = @(
        'UI\PanelContent\EnemyPanelContent.cs'
    )

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game

        foreach ($relFile in $expectedGameFiles) {
            $fullPath = Join-Path $gameDir $relFile
            Require (Test-Path -LiteralPath $fullPath) ($game + ' missing file: ' + $relFile)
            Require (Test-Path -LiteralPath ($fullPath + '.meta')) ($game + ' missing meta: ' + $relFile + '.meta')

            $content = [System.IO.File]::ReadAllText($fullPath)
            Require (!$content.Contains('Service.gI')) ($fullPath + ' must not contain Service.gI')
            Require (!$content.Contains('loadImage(')) ($fullPath + ' must not call loadImage in view/component')
            Require (!$content.Contains('setClip(0, 0, GameCanvas.w, GameCanvas.h)')) ($fullPath + ' must not reset clip to fullscreen')
            Require (!$content.Contains('msg.reader()')) ($fullPath + ' must not parse network message')
            Require (!$content.Contains('Message msg')) ($fullPath + ' must not receive network message')
            Require (!$content.Contains('GameCanvas.panel2 =')) ($fullPath + ' must not mutate GameCanvas.panel2')
        }

        # Rollback flags
        $panelPath = Join-Path $gameDir 'Panel.cs'
        $panelContent = [System.IO.File]::ReadAllText($panelPath)
        Require-Contains $panelContent 'public static bool USE_ENEMY_PANEL_CONTENT' ($game + ' Panel missing USE_ENEMY_PANEL_CONTENT rollback flag')

        # Host/content integration contracts. File existence alone must never make this gate green.
        Require-Contains $panelContent 'EnemyPanelContent enemyPanelContent' ($game + ' Panel missing EnemyPanelContent host field')
        Require-Contains $panelContent 'EnemyContentAction pendingEnemyAction' ($game + ' Panel missing pending Enemy action state')
        Require-Contains $panelContent 'private bool isEnemyContentActive()' ($game + ' Panel missing active ownership guard')
        Require-Contains $panelContent 'private bool isEnemyContentDragging()' ($game + ' Panel missing drag lifecycle guard')
        Require-Contains $panelContent '!isEnemyContentDragging()' ($game + ' outside-close must respect EnemyPanelContent drag')
        Require-Contains $panelContent 'private void cancelPendingEnemyAction()' ($game + ' Panel missing pending Enemy action cancellation')
        Require-Contains $panelContent 'private void unbindEnemyContentWhenLeavingType()' ($game + ' Panel missing Enemy type-change cleanup')
        Require-Contains $panelContent 'private void updateKeyEnemyContent()' ($game + ' Panel missing Enemy input delegation helper')
        Require-Contains $panelContent 'private void executeEnemyAction(EnemyContentAction action)' ($game + ' Panel missing Enemy action execution helper')

        $setTypeEnemyIntegrated = Get-MethodBody $panelContent 'public void setTypeEnemy()'
        Require-Contains $setTypeEnemyIntegrated 'USE_ENEMY_PANEL_CONTENT' ($game + ' setTypeEnemy must honor content rollback')
        Require-Contains $setTypeEnemyIntegrated 'enemyPanelContent.Bind(' ($game + ' setTypeEnemy must bind EnemyPanelContent')
        $contentBranchIndex = $setTypeEnemyIntegrated.IndexOf('if (USE_ENEMY_PANEL_CONTENT)', [System.StringComparison]::Ordinal)
        $contentBindIndex = $setTypeEnemyIntegrated.IndexOf('enemyPanelContent.Bind(', [System.StringComparison]::Ordinal)
        $contentReturnIndex = $setTypeEnemyIntegrated.IndexOf('return;', $contentBindIndex, [System.StringComparison]::Ordinal)
        $legacySelectedIndex = $setTypeEnemyIntegrated.IndexOf('selected =', [System.StringComparison]::Ordinal)
        $legacySetTabIndex = $setTypeEnemyIntegrated.IndexOf('setTabEnemy();', [System.StringComparison]::Ordinal)
        Require ($contentBranchIndex -ge 0 -and
            $contentBindIndex -gt $contentBranchIndex -and
            $contentReturnIndex -gt $contentBindIndex -and
            $legacySelectedIndex -gt $contentReturnIndex -and
            $legacySetTabIndex -gt $legacySelectedIndex) ($game + ' setTypeEnemy content path must Bind and return before legacy state mutations')
        $setTabEnemyIntegrated = Get-MethodBody $panelContent 'public void setTabEnemy()'
        Require-Contains $setTabEnemyIntegrated 'USE_ENEMY_PANEL_CONTENT' ($game + ' setTabEnemy must honor content rollback')
        Require-Contains $setTabEnemyIntegrated 'enemyPanelContent.Refresh(' ($game + ' setTabEnemy must refresh EnemyPanelContent')

        $isEnemyContentActive = Get-MethodBody $panelContent 'private bool isEnemyContentActive()'
        Require-Contains $isEnemyContentActive 'USE_ENEMY_PANEL_CONTENT' ($game + ' Enemy active guard must honor content rollback')
        Require-Contains $isEnemyContentActive 'type == TYPE_ENEMY' ($game + ' Enemy active guard must require TYPE_ENEMY')
        Require-Contains $isEnemyContentActive 'enemyPanelContent.IsActive' ($game + ' Enemy active guard must require active content')

        $paintEnemyIntegrated = Get-MethodBody $panelContent 'private void paintEnemy(mGraphics g)'
        Require-Contains $paintEnemyIntegrated 'enemyPanelContent.Paint(' ($game + ' content-owned renderer path missing')
        foreach ($stateMarker in @('enemyPanelContent.ScrollY', 'enemyPanelContent.SelectedIndex', 'enemyPanelContent.ItemsCount')) {
            Require-Contains $paintEnemyIntegrated $stateMarker ($game + ' content-owned legacy renderer must receive ' + $stateMarker)
        }

        $updateKeyIntegrated = Get-MethodBody $panelContent 'public void updateKey()'
        $enemyCaseIntegrated = $updateKeyIntegrated.IndexOf('case 16:', [System.StringComparison]::Ordinal)
        Require ($enemyCaseIntegrated -ge 0) ($game + ' updateKey missing case 16 after Phase 6 integration')
        $enemyDelegate = $updateKeyIntegrated.IndexOf('updateKeyEnemyContent();', $enemyCaseIntegrated, [System.StringComparison]::Ordinal)
        Require ($enemyDelegate -gt $enemyCaseIntegrated) ($game + ' updateKey case 16 must route to EnemyPanelContent')

        $mouseIntegrated = Get-MethodBody $panelContent 'public void updateScroolMouse(int a)'
        Require-Contains $mouseIntegrated 'isEnemyContentActive()' ($game + ' mouse wheel must check EnemyPanelContent ownership')
        Require-Contains $mouseIntegrated 'enemyPanelContent.UpdateScrollMouse(a);' ($game + ' mouse wheel must route to EnemyPanelContent')

        $updateIntegrated = Get-MethodBody $panelContent 'public void update()'
        Require-Contains $updateIntegrated 'enemyPanelContent.Update();' ($game + ' Panel.update must update EnemyPanelContent')
        Require-Contains $updateIntegrated 'executeEnemyAction(pendingEnemyAction);' ($game + ' delayed Enemy action must execute through host validation')

        $setTypeIntegrated = Get-MethodBody $panelContent 'private void setType(int position)'
        Require-Contains $setTypeIntegrated 'unbindEnemyContentWhenLeavingType();' ($game + ' changing to another panel type must unbind Enemy content')

        foreach ($hideSignature in @('public void hideNow()', 'public void hide()')) {
            $hideBody = Get-MethodBody $panelContent $hideSignature
            Require-Contains $hideBody 'cancelPendingEnemyAction();' ($game + ' ' + $hideSignature + ' must cancel pending Enemy action')
            Require-Contains $hideBody 'enemyPanelContent.Unbind();' ($game + ' ' + $hideSignature + ' must unbind Enemy content')
        }

        $executeEnemyAction = Get-MethodBody $panelContent 'private void executeEnemyAction(EnemyContentAction action)'
        Require-Contains $executeEnemyAction 'enemyPanelContent.IsActionCurrent(action)' ($game + ' executeEnemyAction must reject stale binding revisions')
        Require-Contains $executeEnemyAction 'action.SelectedInfo' ($game + ' executeEnemyAction must use the selected InfoItem snapshot')
    }
    Write-Output 'Production files, namespaces, metadata, and rollback flags: OK'

    Write-Output '--- 5. Bien dich va chay truc tiep bo test hanh vi Phase 6.2 ---'
    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        $productionSources = @(
            (Join-Path $gameDir 'UI\UiInputContext.cs'),
            (Join-Path $gameDir 'UI\Adapters\UiRenderState.cs'),
            (Join-Path $gameDir 'UI\Components\UiListRow.cs'),
            (Join-Path $gameDir 'UI\Pilots\EnemyListView.cs'),
            (Join-Path $gameDir 'UI\PanelContent\EnemyPanelContent.cs'),
            (Join-Path $gameDir 'UI\PanelContent\EnemyPanelLifecycle.cs')
        )
        $gameStub = Join-Path $outputDirectory ($game + '-Phase6LegacyStubs.cs')
        $gameRegression = Join-Path $outputDirectory ($game + '-Phase6Regression.cs')

        $stubContent = [System.IO.File]::ReadAllText($stubSource)
        $regressionContent = [System.IO.File]::ReadAllText($regressionSource)
        if ($game -eq 'Game2') {
            $stubContent = $stubContent.Replace('Game1', 'Game2')
            $regressionContent = $regressionContent.Replace('Game1', 'Game2')
        }
        [System.IO.File]::WriteAllText($gameStub, $stubContent)
        [System.IO.File]::WriteAllText($gameRegression, $regressionContent)

        $outputExe = Join-Path $outputDirectory ($game + '-UiComponentPhase6Regression.exe')
        $compileSources = @($gameStub, $gameRegression) + $uiSharedFiles + $productionSources
        $responseFile = Join-Path $outputDirectory ($game + '-phase6-compile.rsp')
        $compilerArguments = @(
            '/nologo',
            '/nostdlib+',
            '/target:exe',
            '/langversion:9.0',
            '/warnaserror+',
            ('/out:"' + $outputExe + '"')
        ) + $compilerReferences + ($compileSources | ForEach-Object { '"' + $_ + '"' })
        [System.IO.File]::WriteAllLines($responseFile, $compilerArguments)

        $compilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $responseFile) 2>&1
        if ($LASTEXITCODE -ne 0) {
            $compilerOutput | Write-Output
            throw ($game + ' Phase 6 compilation failed with exit code: ' + $LASTEXITCODE)
        }
        Require (Test-Path -LiteralPath $outputExe) ($game + ' Phase 6 compilation did not generate executable')

        $testOutput = & $monoPath $outputExe
        if ($LASTEXITCODE -ne 0) {
            throw ($game + ' Phase 6 regression failed with exit code: ' + $LASTEXITCODE)
        }
        Require ($testOutput -contains ('UI_COMPONENT_PHASE6_PRODUCTION_' + $game + '_OK')) ($game + ' completion marker missing')
        Write-Output ($game + ' Phase 6 production source compilation and behavior regression: OK')
    }

    Write-Output '--- 6. Bien dich toan bo Assembly-CSharp bang Unity Roslyn ---'
    $fullAssemblyPath = Join-Path $outputDirectory 'Assembly-CSharp-Phase6Verification.dll'
    $fullReferencePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase6Verification.ref.dll'
    $fullResponsePath = Join-Path $outputDirectory 'Assembly-CSharp-Phase6Verification.rsp'

    $newPhase6Sources = @(
        (Join-Path $scriptsDir 'UIShared\UiVerticalListState.cs'),
        (Join-Path $scriptsDir 'Game1\UI\Pilots\EnemyListView.cs'),
        (Join-Path $scriptsDir 'Game2\UI\Pilots\EnemyListView.cs'),
        (Join-Path $scriptsDir 'Game1\UI\PanelContent\EnemyPanelContent.cs'),
        (Join-Path $scriptsDir 'Game2\UI\PanelContent\EnemyPanelContent.cs')
    )

    $existingRspLines = Get-Content -LiteralPath $unityResponseFile.FullName
    $fullResponseLines = [System.Collections.Generic.List[string]]::new()

    foreach ($line in $existingRspLines) {
        if ($line -match '^-out:') {
            $fullResponseLines.Add('-out:"' + $fullAssemblyPath + '"')
        }
        elseif ($line -match '^-refout:') {
            $fullResponseLines.Add('-refout:"' + $fullReferencePath + '"')
        }
        else {
            $fullResponseLines.Add($line)
        }
    }

    $normalizedExisting = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($line in $existingRspLines) {
        $clean = $line.Trim().Trim('"').Replace('/', '\')
        $normalizedExisting.Add($clean) | Out-Null
    }

    foreach ($source in $newPhase6Sources) {
        $cleanSource = $source.Trim().Trim('"').Replace('/', '\')
        if (!$normalizedExisting.Contains($cleanSource)) {
            $fullResponseLines.Add('"' + $source + '"')
        }
    }

    [System.IO.File]::WriteAllLines($fullResponsePath, $fullResponseLines)

    $fullCompilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $fullResponsePath) 2>&1
    if ($LASTEXITCODE -ne 0) {
        $fullCompilerOutput | Write-Output
        throw ('Full Assembly-CSharp compilation failed with exit code: ' + $LASTEXITCODE)
    }
    Require (Test-Path -LiteralPath $fullAssemblyPath) 'Full Assembly-CSharp compilation did not produce assembly'
    Write-Output 'Full Unity Assembly-CSharp compilation: OK'

    Write-Output 'UI_COMPONENT_PHASE6_2_CONTENT_OK'

    Write-Output '--- 7. Kiem tra production files va integration markers Phase 6.3 (Lifecycle) ---'
    $expectedLifecycleFiles = @(
        'UI\PanelContent\EnemyPanelLifecycle.cs'
    )

    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game

        foreach ($relFile in $expectedLifecycleFiles) {
            $fullPath = Join-Path $gameDir $relFile
            Require (Test-Path -LiteralPath $fullPath) ($game + ' missing file: ' + $relFile)
            Require (Test-Path -LiteralPath ($fullPath + '.meta')) ($game + ' missing meta: ' + $relFile + '.meta')

            $content = [System.IO.File]::ReadAllText($fullPath)
            Require (!$content.Contains('Service.gI')) ($fullPath + ' must not contain Service.gI')
            Require (!$content.Contains('loadImage(')) ($fullPath + ' must not call loadImage')
            Require (!$content.Contains('setClip(0, 0, GameCanvas.w, GameCanvas.h)')) ($fullPath + ' must not reset clip to fullscreen')
            Require (!$content.Contains('msg.reader()')) ($fullPath + ' must not parse network message')
            Require (!$content.Contains('Message msg')) ($fullPath + ' must not receive network message')
            Require (!$content.Contains('GameCanvas.panel2 =')) ($fullPath + ' must not mutate GameCanvas.panel2')
            Require-Contains $content 'public enum EnemyListDisposition' ($fullPath + ' missing EnemyListDisposition enum')
            Require-Contains $content 'public struct EnemyMenuContext' ($fullPath + ' missing EnemyMenuContext struct')
            Require-Contains $content 'public class EnemyPanelLifecycle' ($fullPath + ' missing EnemyPanelLifecycle class')
            Require-Contains $content 'public bool IsWaitDialogOwned' ($fullPath + ' missing wait-dialog ownership state')
            Require-Contains $content 'public void BeginRequest()' ($fullPath + ' missing BeginRequest method')
            Require-Contains $content 'public void BeginWaitDialog()' ($fullPath + ' missing BeginWaitDialog method')
            Require-Contains $content 'public bool ConsumeWaitDialogOwnership()' ($fullPath + ' missing ConsumeWaitDialogOwnership method')
            Require-Contains $content 'public void CancelRequest()' ($fullPath + ' missing CancelRequest method')
            Require-Contains $content 'public EnemyListDisposition OnListReceived(bool isEnemyOpen)' ($fullPath + ' missing OnListReceived method')
            Require-Contains $content 'public void OpenMenu(InfoItem target)' ($fullPath + ' missing OpenMenu method')
            Require-Contains $content 'public void InvalidateMenuContext()' ($fullPath + ' missing InvalidateMenuContext method')
            Require-Contains $content 'public bool IsMenuContextValid(' ($fullPath + ' missing IsMenuContextValid method')
            Require-Contains $content 'public bool OnLeavingType()' ($fullPath + ' missing OnLeavingType method')
        }

        # Panel.cs integration for lifecycle
        $panelPath = Join-Path $gameDir 'Panel.cs'
        $panelContent = [System.IO.File]::ReadAllText($panelPath)
        Require-Contains $panelContent 'EnemyPanelLifecycle enemyLifecycle' ($game + ' Panel missing enemyLifecycle field')
        Require-Contains $panelContent 'public EnemyListDisposition onEnemyListReceived()' ($game + ' Panel missing onEnemyListReceived method')
        Require-Contains $panelContent 'private bool isEnemyMenuContextValid(' ($game + ' Panel missing isEnemyMenuContextValid helper')
        Require-Contains $panelContent 'private void invalidateEnemyMenuContext()' ($game + ' Panel missing invalidateEnemyMenuContext helper')

        $doFireAccountBody = Get-MethodBody $panelContent 'private void doFireAccount()'
        Require-OrderedContains $doFireAccountBody @(
            'case 2:',
            'enemyLifecycle.BeginRequest();',
            'Service.gI().enemy(0, -1);'
        ) ($game + ' doFireAccount case 2 must mark BeginRequest before Service.enemy(0, -1)')

        $executeEnemyActionBody = Get-MethodBody $panelContent 'private void executeEnemyAction(EnemyContentAction action)'
        Require-Contains $executeEnemyActionBody 'enemyLifecycle.OpenMenu(' ($game + ' executeEnemyAction must call enemyLifecycle.OpenMenu')

        $doFireEnemyBody = Get-MethodBody $panelContent 'private void doFireEnemy()'
        Require-Contains $doFireEnemyBody 'enemyLifecycle.OpenMenu(' ($game + ' doFireEnemy must call enemyLifecycle.OpenMenu')

        $revengeActionBody = Get-MethodBody $panelContent 'if (idAction == 10000)'
        Require-Contains $revengeActionBody 'isEnemyMenuContextValid(' ($game + ' action 10000 must validate menu context')

        $deleteActionBody = Get-MethodBody $panelContent 'if (idAction == 10001)'
        Require-Contains $deleteActionBody 'isEnemyMenuContextValid(' ($game + ' action 10001 must validate menu context')
        Require-OrderedContains $deleteActionBody @(
            'enemyLifecycle.BeginWaitDialog();',
            'Service.gI().enemy(2, infoItem5.charInfo.charID);',
            'InfoDlg.showWait();'
        ) ($game + ' action 10001 must own its wait dialog before sending the delete request')

        $onListReceivedBody = Get-MethodBody $panelContent 'public EnemyListDisposition onEnemyListReceived()'
        Require-Contains $onListReceivedBody 'enemyLifecycle.ConsumeWaitDialogOwnership()' ($game + ' onEnemyListReceived must consume only Enemy-owned wait dialogs')
        Require-Contains $onListReceivedBody 'InfoDlg.hide();' ($game + ' onEnemyListReceived must hide an owned Enemy wait dialog')
        Require-Contains $onListReceivedBody 'enemyLifecycle.OnListReceived(' ($game + ' onEnemyListReceived must delegate to enemyLifecycle.OnListReceived')
        Require-Contains $onListReceivedBody 'setTabEnemy();' ($game + ' onEnemyListReceived must call setTabEnemy on RefreshCurrent')
        Require-Contains $onListReceivedBody 'setTypeEnemy();' ($game + ' onEnemyListReceived must call setTypeEnemy on OpenRequested')
        Require-Contains $onListReceivedBody 'show();' ($game + ' onEnemyListReceived must call show on OpenRequested')

        $leaveLifecycleBody = Get-MethodBody $panelContent 'private void leaveEnemyLifecycle()'
        Require-Contains $leaveLifecycleBody 'enemyLifecycle.OnLeavingType()' ($game + ' leaveEnemyLifecycle must notify enemyLifecycle')
        Require-Contains $leaveLifecycleBody 'InfoDlg.hide();' ($game + ' leaveEnemyLifecycle must close only the Enemy-owned wait dialog')

        $unbindBody = Get-MethodBody $panelContent 'private void unbindEnemyContentWhenLeavingType()'
        Require-Contains $unbindBody 'leaveEnemyLifecycle();' ($game + ' unbindEnemyContentWhenLeavingType must release lifecycle ownership')

        foreach ($hideSig in @('public void hideNow()', 'public void hide()')) {
            $hideBody = Get-MethodBody $panelContent $hideSig
            Require-Contains $hideBody 'leaveEnemyLifecycle();' ($game + ' ' + $hideSig + ' must release lifecycle ownership')
        }

        # Controller.cs packet -99 routing
        $controllerPath = Join-Path $gameDir 'Controller.cs'
        $controllerContent = [System.IO.File]::ReadAllText($controllerPath)
        $p99Start = $controllerContent.IndexOf('case -99:', [System.StringComparison]::Ordinal)
        $p99End = $controllerContent.IndexOf('case -98:', $p99Start, [System.StringComparison]::Ordinal)
        $p99Block = $controllerContent.Substring($p99Start, $p99End - $p99Start)
        Require-Contains $p99Block 'GameCanvas.panel.onEnemyListReceived();' ($game + ' Controller packet -99 must route to onEnemyListReceived')
        Require-OrderedContains $p99Block @(
            'sbyte enemyResponseType = msg.reader().readByte();',
            'if (enemyResponseType == 0)',
            'GameCanvas.panel.onEnemyListReceived();',
            'else',
            'InfoDlg.hide();'
        ) ($game + ' Controller packet -99 must preserve subtype -1 without hiding before lifecycle routing')
        Require (!$p99Block.Contains('GameCanvas.panel.setTypeEnemy();')) ($game + ' Controller packet -99 must not call setTypeEnemy directly')
        Require (!$p99Block.Contains('GameCanvas.panel.show();')) ($game + ' Controller packet -99 must not call show directly')
    }
    Write-Output 'Lifecycle production files, contracts, and routing: OK'

    Write-Output '--- 8. Bien dich va chay bo test hanh vi Phase 6.3 ---'
    foreach ($game in @('Game1', 'Game2')) {
        $gameDir = Join-Path $scriptsDir $game
        $productionSources = @(
            (Join-Path $gameDir 'UI\UiInputContext.cs'),
            (Join-Path $gameDir 'UI\Adapters\UiRenderState.cs'),
            (Join-Path $gameDir 'UI\Components\UiListRow.cs'),
            (Join-Path $gameDir 'UI\Pilots\EnemyListView.cs'),
            (Join-Path $gameDir 'UI\PanelContent\EnemyPanelContent.cs'),
            (Join-Path $gameDir 'UI\PanelContent\EnemyPanelLifecycle.cs')
        )
        $gameStub = Join-Path $outputDirectory ($game + '-Phase63LegacyStubs.cs')
        $gameRegression = Join-Path $outputDirectory ($game + '-Phase63Regression.cs')

        $stubContent = [System.IO.File]::ReadAllText($stubSource)
        $regressionContent = [System.IO.File]::ReadAllText($regressionSource)
        if ($game -eq 'Game2') {
            $stubContent = $stubContent.Replace('Game1', 'Game2')
            $regressionContent = $regressionContent.Replace('Game1', 'Game2')
        }
        [System.IO.File]::WriteAllText($gameStub, $stubContent)
        [System.IO.File]::WriteAllText($gameRegression, $regressionContent)

        $outputExe = Join-Path $outputDirectory ($game + '-UiComponentPhase63Regression.exe')
        $compileSources = @($gameStub, $gameRegression) + $uiSharedFiles + $productionSources
        $responseFile = Join-Path $outputDirectory ($game + '-phase63-compile.rsp')
        $compilerArguments = @(
            '/nologo',
            '/nostdlib+',
            '/target:exe',
            '/langversion:9.0',
            '/warnaserror+',
            ('/out:"' + $outputExe + '"')
        ) + $compilerReferences + ($compileSources | ForEach-Object { '"' + $_ + '"' })
        [System.IO.File]::WriteAllLines($responseFile, $compilerArguments)

        $compilerOutput = & $dotnetCommand.Source $compilerPath ('@' + $responseFile) 2>&1
        if ($LASTEXITCODE -ne 0) {
            $compilerOutput | Write-Output
            throw ($game + ' Phase 6.3 compilation failed with exit code: ' + $LASTEXITCODE)
        }
        Require (Test-Path -LiteralPath $outputExe) ($game + ' Phase 6.3 compilation did not generate executable')

        $testOutput = & $monoPath $outputExe
        if ($LASTEXITCODE -ne 0) {
            throw ($game + ' Phase 6.3 regression failed with exit code: ' + $LASTEXITCODE)
        }
        Require ($testOutput -contains ('UI_COMPONENT_PHASE6_PRODUCTION_' + $game + '_OK')) ($game + ' completion marker missing')
        Write-Output ($game + ' Phase 6.3 production source compilation and behavior regression: OK')
    }

    Write-Output '--- 9. Bien dich toan bo Assembly-CSharp bao gom EnemyPanelLifecycle ---'
    $fullAssemblyPath63 = Join-Path $outputDirectory 'Assembly-CSharp-Phase63Verification.dll'
    $fullReferencePath63 = Join-Path $outputDirectory 'Assembly-CSharp-Phase63Verification.ref.dll'
    $fullResponsePath63 = Join-Path $outputDirectory 'Assembly-CSharp-Phase63Verification.rsp'

    $newPhase63Sources = @(
        (Join-Path $scriptsDir 'UIShared\UiVerticalListState.cs'),
        (Join-Path $scriptsDir 'Game1\UI\Pilots\EnemyListView.cs'),
        (Join-Path $scriptsDir 'Game2\UI\Pilots\EnemyListView.cs'),
        (Join-Path $scriptsDir 'Game1\UI\PanelContent\EnemyPanelContent.cs'),
        (Join-Path $scriptsDir 'Game2\UI\PanelContent\EnemyPanelContent.cs'),
        (Join-Path $scriptsDir 'Game1\UI\PanelContent\EnemyPanelLifecycle.cs'),
        (Join-Path $scriptsDir 'Game2\UI\PanelContent\EnemyPanelLifecycle.cs')
    )

    $fullResponseLines63 = [System.Collections.Generic.List[string]]::new()
    foreach ($line in $existingRspLines) {
        if ($line -match '^-out:') {
            $fullResponseLines63.Add('-out:"' + $fullAssemblyPath63 + '"')
        }
        elseif ($line -match '^-refout:') {
            $fullResponseLines63.Add('-refout:"' + $fullReferencePath63 + '"')
        }
        else {
            $fullResponseLines63.Add($line)
        }
    }

    foreach ($source in $newPhase63Sources) {
        $cleanSource = $source.Trim().Trim('"').Replace('/', '\')
        if (!$normalizedExisting.Contains($cleanSource)) {
            $fullResponseLines63.Add('"' + $source + '"')
        }
    }

    [System.IO.File]::WriteAllLines($fullResponsePath63, $fullResponseLines63)

    $fullCompilerOutput63 = & $dotnetCommand.Source $compilerPath ('@' + $fullResponsePath63) 2>&1
    if ($LASTEXITCODE -ne 0) {
        $fullCompilerOutput63 | Write-Output
        throw ('Full Assembly-CSharp compilation failed with exit code: ' + $LASTEXITCODE)
    }
    Require (Test-Path -LiteralPath $fullAssemblyPath63) 'Full Assembly-CSharp compilation did not produce assembly'
    Write-Output 'Full Unity Assembly-CSharp compilation with EnemyPanelLifecycle: OK'

    Write-Output 'UI_COMPONENT_PHASE6_3_LIFECYCLE_OK'

    Write-Output '--- 10. Kiem tra shared vertical-list state va integration Phase 6.4 ---'
    $sharedListStatePath = Join-Path $uiSharedDir 'UiVerticalListState.cs'
    Require (Test-Path -LiteralPath $sharedListStatePath) 'UiVerticalListState.cs not found'
    Require (Test-Path -LiteralPath ($sharedListStatePath + '.meta')) 'UiVerticalListState.cs.meta not found'

    $sharedListState = [System.IO.File]::ReadAllText($sharedListStatePath)
    foreach ($marker in @(
        'public enum UiVerticalListDragMode',
        'public struct UiVerticalListActivation',
        'public sealed class UiVerticalListState',
        'public void Bind(int itemCount, int itemHeight, UiRect viewport, bool isTouch)',
        'public void Refresh(int itemCount, UiRect viewport)',
        'public void MoveSelection(int delta)',
        'public UiVerticalListActivation ActivateSelection(int delayFrames)',
        'public void BeginPointer(int pointerY)',
        'public void DragPointer(int pointerY, UiVerticalListDragMode dragMode)',
        'public UiVerticalListActivation ReleasePointer(int pointerY)',
        'public void UpdateScrollMouse(int wheelDelta)',
        'public void Unbind()'
    )) {
        Require-Contains $sharedListState $marker ('UiVerticalListState missing contract: ' + $marker)
    }

    foreach ($forbidden in @(
        'GameCanvas',
        'MyVector',
        'TopInfo',
        'InfoItem',
        'Service.gI',
        'TopRankingView',
        'EnemyListView',
        'mGraphics',
        'IPanelContent'
    )) {
        Require (!$sharedListState.Contains($forbidden)) ('UiVerticalListState must remain domain-free: ' + $forbidden)
    }

    foreach ($game in @('Game1', 'Game2')) {
        $topContentPath = Join-Path $scriptsDir ($game + '\UI\PanelContent\TopPanelContent.cs')
        $enemyContentPath = Join-Path $scriptsDir ($game + '\UI\PanelContent\EnemyPanelContent.cs')
        $topContent = [System.IO.File]::ReadAllText($topContentPath)
        $enemyContent = [System.IO.File]::ReadAllText($enemyContentPath)

        foreach ($contentContract in @(
            'private readonly UiVerticalListState listState = new UiVerticalListState();',
            'listState.Bind(',
            'listState.Refresh(',
            'listState.Update();',
            'listState.MoveSelection(',
            'listState.BeginPointer(',
            'listState.DragPointer(',
            'listState.ReleasePointer(',
            'listState.UpdateScrollMouse(',
            'listState.Unbind();'
        )) {
            Require-Contains $topContent $contentContract ($game + ' TopPanelContent missing shared-state integration: ' + $contentContract)
            Require-Contains $enemyContent $contentContract ($game + ' EnemyPanelContent missing shared-state integration: ' + $contentContract)
        }

        Require-Contains $topContent 'UiVerticalListDragMode.Elastic' ($game + ' TopPanelContent must preserve elastic drag behavior')
        Require-Contains $enemyContent 'UiVerticalListDragMode.Clamped' ($game + ' EnemyPanelContent must preserve clamped drag behavior')

        foreach ($obsoleteState in @(
            'private int cmy;',
            'private int cmtoY;',
            'private int cmyLim;',
            'private int cmRun;',
            'private bool pointerIsDowning;',
            'private int bindingRevision;'
        )) {
            Require (!$topContent.Contains($obsoleteState)) ($game + ' TopPanelContent still owns duplicated list state: ' + $obsoleteState)
            Require (!$enemyContent.Contains($obsoleteState)) ($game + ' EnemyPanelContent still owns duplicated list state: ' + $obsoleteState)
        }
    }

    $game1TopNormalized = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game1\UI\PanelContent\TopPanelContent.cs')).Replace('Game1', 'GameX')
    $game2TopNormalized = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game2\UI\PanelContent\TopPanelContent.cs')).Replace('Game2', 'GameX')
    $game1EnemyNormalized = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game1\UI\PanelContent\EnemyPanelContent.cs')).Replace('Game1', 'GameX')
    $game2EnemyNormalized = [System.IO.File]::ReadAllText((Join-Path $scriptsDir 'Game2\UI\PanelContent\EnemyPanelContent.cs')).Replace('Game2', 'GameX')
    Require ($game1TopNormalized -ceq $game2TopNormalized) 'TopPanelContent Game1/Game2 parity mismatch'
    Require ($game1EnemyNormalized -ceq $game2EnemyNormalized) 'EnemyPanelContent Game1/Game2 parity mismatch'

    Require-Contains $regressionContract 'TestSharedVerticalListLifecycleAndRevision();' 'Phase 6 regression missing shared lifecycle/revision coverage'
    Require-Contains $regressionContract 'TestSharedVerticalListActivationAndWheel();' 'Phase 6 regression missing shared activation/wheel coverage'
    Require-Contains $regressionContract 'TestSharedVerticalListPreservesDragPolicies();' 'Phase 6 regression missing drag-policy coverage'

    Write-Output 'Shared vertical-list state contracts, behavior policies, and Game1/Game2 integration: OK'
    Write-Output 'UI_COMPONENT_PHASE6_4_SHARED_STATE_OK'

    Write-Output '--- 11. Kiem ke caller va cleanup Phase 6.5 ---'
    $scopedPanelHelpers = @(
        'paintTopComponent',
        'paintTopLegacy',
        'doFireTop',
        'ensureTopPanelContent',
        'isTopContentActive',
        'isTopContentDragging',
        'cancelPendingTopAction',
        'unbindTopContentWhenLeavingType',
        'updateKeyTopContent',
        'executeTopAction',
        'paintEnemyLegacy',
        'doFireEnemy',
        'ensureEnemyPanelContent',
        'isEnemyContentActive',
        'isEnemyContentDragging',
        'cancelPendingEnemyAction',
        'leaveEnemyLifecycle',
        'unbindEnemyContentWhenLeavingType',
        'updateKeyEnemyContent',
        'isEnemyMenuContextValid',
        'invalidateEnemyMenuContext',
        'executeEnemyAction'
    )

    foreach ($game in @('Game1', 'Game2')) {
        $panelPath = Join-Path $scriptsDir ($game + '\Panel.cs')
        $panelContent = [System.IO.File]::ReadAllText($panelPath)

        foreach ($helper in $scopedPanelHelpers) {
            $callPattern = '\b' + [regex]::Escape($helper) + '\s*\('
            $occurrenceCount = [regex]::Matches($panelContent, $callPattern).Count
            $declarationPattern = '(?m)^\s*private\s+[^\r\n(]+\b' + [regex]::Escape($helper) + '\s*\('
            $declarationCount = [regex]::Matches($panelContent, $declarationPattern).Count
            Require ($declarationCount -ge 1) ($game + ' scoped helper declaration missing: ' + $helper)
            Require ($occurrenceCount -gt $declarationCount) ($game + ' scoped helper has no production caller and needs removal: ' + $helper)
        }

        foreach ($rollbackMarker in @(
            'public static bool USE_TOP_PANEL_CONTENT = true;',
            'public static bool USE_NEW_TOP_UI = true;',
            'public static bool USE_ENEMY_PANEL_CONTENT = true;',
            'public static bool USE_NEW_ENEMY_UI = true;',
            'private void paintTopLegacy(',
            'private void paintEnemyLegacy(',
            'private void doFireTop()',
            'private void doFireEnemy()'
        )) {
            Require-Contains $panelContent $rollbackMarker ($game + ' rollback exception removed before Play Mode acceptance: ' + $rollbackMarker)
        }

        foreach ($rendererRelativePath in @(
            'UI\Pilots\TopRankingView.cs',
            'UI\Pilots\EnemyListView.cs'
        )) {
            $rendererPath = Join-Path $scriptsDir ($game + '\' + $rendererRelativePath)
            $rendererContent = [System.IO.File]::ReadAllText($rendererPath)
            Require (!$rendererContent.Contains('using System;')) ($game + ' renderer still has unused System import: ' + $rendererRelativePath)
        }
    }

    Write-Output 'Scoped caller inventory, rollback exceptions, and dead-import cleanup: OK'
    Write-Output 'UI_COMPONENT_PHASE6_5_CLEANUP_OK'
    Write-Output 'UI_COMPONENT_PHASE6_EXPANSION_OK'
}
finally {
    if (Test-Path -LiteralPath $outputDirectory) {
        Remove-Item -LiteralPath $outputDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }
}
