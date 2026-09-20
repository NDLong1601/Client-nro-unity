using System;
using System.Collections.Generic;
using Game1;
using Game1.UI;
using Game1.UI.Adapters;
using Game1.UI.Components;
using Game1.UI.PanelContent;
using Game1.UI.Pilots;
using Nro.UI;

namespace Game1.Tests
{
    public static class UiComponentPhase6Regression
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("ASSERTION_FAILED: " + message);
            }
        }

        private static MyVector CreateMockEnemyList(int count)
        {
            MyVector list = new MyVector("EnemyList");
            for (int i = 0; i < count; i++)
            {
                Char c = new Char
                {
                    charID = 2000 + i,
                    cName = "Enemy_" + i,
                    head = (short)(i % 5),
                    headICON = (short)(i % 2 == 0 ? (100 + i) : -1),
                    body = (short)(10 + i),
                    leg = (short)(20 + i),
                    bag = -1
                };
                InfoItem item = new InfoItem("Power: " + (5000000 - i * 100000))
                {
                    charInfo = c,
                    isOnline = (i % 2 == 0)
                };
                list.addElement(item);
            }
            return list;
        }

        private static void SetupMockParts()
        {
            for (int i = 0; i < GameScr.parts.Length; i++)
            {
                GameScr.parts[i] = new Part
                {
                    pi = new PartImage[]
                    {
                        new PartImage { id = (short)(500 + i), dx = 2, dy = 1 }
                    }
                };
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("--- Running UiComponentPhase6Regression ---");

            TestEnemyPanelContentLifecycle();
            TestEnemyPanelContentOwnershipAndLimits();
            TestEnemyPanelContentKeyboardNavigation();
            TestEnemyPanelContentKeyboardFire();
            TestEnemyPanelContentClickVsDrag();
            TestEnemyPanelContentDragClampsAtBounds();
            TestEnemyPanelContentInertia();
            TestEnemyPanelContentMouseWheel();
            TestEnemyPanelContentEmptyOneManyRows();
            TestEnemyPanelContentShrinkingListClamping();
            TestEnemyPanelContentModalAndInactiveGuards();
            TestEnemyPanelContentDelayedActionSingleEmission();
            TestEnemyPanelContentDragLifecycleIsVisibleToHost();
            TestEnemyPanelContentRefreshInvalidatesPendingAction();
            TestEnemyPanelContentPaintDelegationAndVisibleRange();
            TestEnemyListViewEmptyStateAndAvatarContracts();

            TestSharedVerticalListLifecycleAndRevision();
            TestSharedVerticalListActivationAndWheel();
            TestSharedVerticalListPreservesDragPolicies();

            TestLifecycleBeginRequest();
            TestLifecycleOpenRequestedConsumesMarker();
            TestLifecycleRefreshCurrentWhenOpen();
            TestLifecycleCacheOnlyWhenNotRequestedAndNotOpen();
            TestLifecycleStaleMarkerCancelledOnLeaving();
            TestLifecycleConsecutiveResponsesDoNotReopen();
            TestLifecycleRefreshPreservesOrClampsSelectionAndScroll();
            TestLifecycleRefreshCancelsPendingDelayedAction();
            TestLifecycleRefreshInvalidatesStaleMenuContext();
            TestLifecycleStaleMenuContextRejectedByValidation();
            TestLifecycleCurrentMenuContextAcceptedOnce();
            TestLifecycleMenuContextRequiresMatchingCharIdAndTarget();
            TestLifecycleMenuContextRequiresOpenEnemyPanel();
            TestLifecycleLeavingTypeInvalidatesMenuContextAndCancelsRequest();
            TestLifecycleCacheOnlyDoesNotMutateState();
            TestLifecycleWaitDialogOwnershipIsSingleUse();
            TestLifecycleLateCacheOnlyCannotConsumeAnotherWait();

            Console.WriteLine("UI_COMPONENT_PHASE6_PRODUCTION_Game1_OK");
        }

        private static void TestEnemyPanelContentLifecycle()
        {
            var content = new EnemyPanelContent();
            Assert(!content.IsBound, "New content must not be bound");
            Assert(!content.IsActive, "New content must not be active");
            Assert(content.ItemsCount == 0, "Initial items count must be 0");
            Assert(content.SelectedIndex == -1, "Initial selected index must be -1");

            MyVector items = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 160);

            // 1. Bind on PC (not touch) -> SelectedIndex should be 0
            content.Bind(items, viewport, isTouch: false);
            Assert(content.IsBound, "Content must be bound after Bind");
            Assert(content.IsActive, "Content must be active after Bind");
            Assert(content.ItemsCount == 5, "ItemsCount must match list size");
            Assert(content.SelectedIndex == 0, "PC bind must select first item (0)");

            // 2. Refresh with same count -> stays bound and active
            content.Refresh(items, viewport);
            Assert(content.IsBound, "Content must remain bound after Refresh");
            Assert(content.SelectedIndex == 0, "Selected index should remain 0");

            // 3. Unbind
            content.Unbind();
            Assert(!content.IsBound, "Content must not be bound after Unbind");
            Assert(!content.IsActive, "Content must not be active after Unbind");
            Assert(content.ItemsCount == 0, "ItemsCount must be 0 after Unbind");
            Assert(content.SelectedIndex == -1, "SelectedIndex must be -1 after Unbind");
            Assert(content.ScrollY == 0, "ScrollY must be 0 after Unbind");

            // 4. Bind on touch -> SelectedIndex should be -1
            content.Bind(items, viewport, isTouch: true);
            Assert(content.IsBound, "Content must be bound");
            Assert(content.SelectedIndex == -1, "Touch bind must default to selected -1");

            content.Unbind();
        }

        private static void TestEnemyPanelContentOwnershipAndLimits()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(10);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: false);

            int expectedLimit = 10 * EnemyPanelContent.ITEM_HEIGHT - viewport.Height;
            Assert(content.ScrollLimit == expectedLimit, "ScrollLimit must be itemsCount * 24 - viewport.Height");
            Assert(content.ScrollY == 0, "Initial ScrollY must be 0");
            Assert(content.ScrollTargetY == 0, "Initial ScrollTargetY must be 0");

            content.Unbind();
        }

        private static void TestEnemyPanelContentKeyboardNavigation()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: false);
            Assert(content.SelectedIndex == 0, "Initially index 0 selected on PC");

            var input = new UiInputContext();

            // Down key (key 22 on PC)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            EnemyContentAction act = content.HandleInput(input);
            Assert(act.Type == EnemyContentActionType.None, "Navigation should not trigger open actions");
            Assert(content.SelectedIndex == 1, "Down key must increment selected index to 1");

            // Navigate to last item (index 4)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input); // to 2
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input); // to 3
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input); // to 4
            Assert(content.SelectedIndex == 4, "Selected index should be 4");

            // Wrap to 0
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input);
            Assert(content.SelectedIndex == 0, "Down at end must wrap to index 0");

            // Up wraps from 0 to 4 (key 21 on PC)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[21] = true;
            content.HandleInput(input);
            Assert(content.SelectedIndex == 4, "Up at 0 must wrap to index 4");

            content.Unbind();
        }

        private static void TestEnemyPanelContentKeyboardFire()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: false);

            var input = new UiInputContext();

            // Navigate to index 2
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input); // 1
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input); // 2
            Assert(content.SelectedIndex == 2, "Selected index must be 2");

            // Press Fire key (key 12)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[12] = true;
            EnemyContentAction action = content.HandleInput(input);

            Assert(action.Type == EnemyContentActionType.OpenActions, "Fire key must produce OpenActions action");
            Assert(action.SelectedIndex == 2, "Action SelectedIndex must match");
            Assert(action.SelectedInfo != null, "Action SelectedInfo must not be null");
            Assert(action.SelectedInfo.charInfo.cName == "Enemy_2", "Action SelectedInfo cName must match Enemy_2");
            Assert(action.DelayFrames == 2, "Keyboard fire should have short 2 frame delay");
            Assert(action.BindingRevision == content.BindingRevision, "Action revision must match content revision");
            Assert(content.IsActionCurrent(action), "Action must be valid for current binding");

            content.Unbind();
        }

        private static void TestEnemyPanelContentClickVsDrag()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: true);

            var input = new UiInputContext();

            // Case A: Click / Tap on Row 1 (y = 80 + 24 = 104)
            GameCanvas.isPointerDown = true;
            GameCanvas.px = 50;
            GameCanvas.py = 100;
            content.HandleInput(input);

            // Quick release with zero movement
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            var clickRelease = new UiInputContext();
            EnemyContentAction clickAction = content.HandleInput(clickRelease);

            Assert(clickAction.Type == EnemyContentActionType.OpenActions, "Click release should emit OpenActions");
            Assert(clickAction.SelectedIndex == 0, "Click at y=100 in viewport y=80 should select row 0");
            Assert(clickAction.DelayFrames == 10, "Click release should have 10 frame delay for visual feedback");

            // Case B: Drag (pointer moves 60px down)
            content.Bind(items, viewport, isTouch: true);
            GameCanvas.isPointerDown = true;
            GameCanvas.px = 50;
            GameCanvas.py = 100;
            content.HandleInput(input);

            // Drag down to 160
            GameCanvas.py = 160;
            var dragMove = new UiInputContext();
            content.HandleInput(dragMove);

            // Release after drag
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            var dragRelease = new UiInputContext();
            EnemyContentAction dragAction = content.HandleInput(dragRelease);

            Assert(dragAction.Type == EnemyContentActionType.None, "Drag gesture must NOT emit OpenActions");

            content.Unbind();
        }

        private static void TestEnemyPanelContentDragClampsAtBounds()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(20);
            UiRect viewport = new UiRect(10, 80, 200, 120);
            var input = new UiInputContext();

            content.Bind(items, viewport, isTouch: true);
            GameCanvas.isPointerDown = true;
            GameCanvas.px = 50;
            GameCanvas.py = 100;
            content.HandleInput(input);

            GameCanvas.py = 160;
            content.HandleInput(input);
            Assert(content.ScrollY == 0, "Dragging down at the top must keep ScrollY clamped to 0");

            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            content.HandleInput(input);

            content.Bind(items, viewport, isTouch: false);
            for (int i = 0; i < items.size() - 1; i++)
            {
                GameCanvas.clearKeyPressed();
                GameCanvas.keyPressed[22] = true;
                content.HandleInput(input);
            }
            Assert(content.ScrollY == content.ScrollLimit, "Keyboard navigation must position the list at its lower bound");

            GameCanvas.isPointerDown = true;
            GameCanvas.px = 50;
            GameCanvas.py = 140;
            content.HandleInput(input);

            GameCanvas.py = 80;
            content.HandleInput(input);
            Assert(content.ScrollY == content.ScrollLimit, "Dragging up at the bottom must keep ScrollY clamped to ScrollLimit");

            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            content.HandleInput(input);
            content.Unbind();
        }

        private static void TestEnemyPanelContentInertia()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(20);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: true);

            // Simulate pointer drag down rapidly
            GameCanvas.isPointerDown = true;
            GameCanvas.px = 50;
            GameCanvas.py = 150;
            content.HandleInput(new UiInputContext());

            GameCanvas.py = 100;
            content.HandleInput(new UiInputContext());

            // Release with fling
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            content.HandleInput(new UiInputContext());

            int targetBefore = content.ScrollTargetY;
            // Update frames for inertia
            for (int f = 0; f < 10; f++)
            {
                content.Update();
            }

            Assert(content.ScrollY >= 0, "ScrollY must remain >= 0 after inertia");
            Assert(content.ScrollY <= content.ScrollLimit, "ScrollY must not exceed ScrollLimit after inertia");

            content.Unbind();
        }

        private static void TestEnemyPanelContentMouseWheel()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(15);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: false);
            Assert(content.ScrollTargetY == 0, "ScrollTargetY initially 0");

            // Wheel down (wheelDelta < 0)
            content.UpdateScrollMouse(-2);
            Assert(content.ScrollTargetY > 0, "Wheel down should increase scroll target");
            int targetAfterDown = content.ScrollTargetY;

            // Wheel up (wheelDelta > 0)
            content.UpdateScrollMouse(2);
            Assert(content.ScrollTargetY < targetAfterDown, "Wheel up should decrease scroll target");

            content.Unbind();
        }

        private static void TestEnemyPanelContentEmptyOneManyRows()
        {
            var content = new EnemyPanelContent();
            UiRect viewport = new UiRect(10, 80, 200, 120);

            // Empty list
            MyVector emptyList = new MyVector("Empty");
            content.Bind(emptyList, viewport, isTouch: false);
            Assert(content.ItemsCount == 0, "Empty list count must be 0");
            Assert(content.ScrollLimit == 0, "Empty list scroll limit must be 0");
            Assert(content.SelectedIndex == -1, "Empty list selection must be -1");

            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[12] = true;
            EnemyContentAction emptyAct = content.HandleInput(new UiInputContext());
            Assert(emptyAct.Type == EnemyContentActionType.None, "Fire on empty list must return None");

            // Single item list
            MyVector singleList = CreateMockEnemyList(1);
            content.Bind(singleList, viewport, isTouch: false);
            Assert(content.ItemsCount == 1, "Single list count must be 1");
            Assert(content.ScrollLimit == 0, "Single item should not have scroll limit");
            Assert(content.SelectedIndex == 0, "Single item should be selected on PC");

            // 50 items list
            MyVector largeList = CreateMockEnemyList(50);
            content.Bind(largeList, viewport, isTouch: false);
            Assert(content.ItemsCount == 50, "Large list count must be 50");
            Assert(content.ScrollLimit == 50 * 24 - 120, "Large list scroll limit must match formula");

            content.Unbind();
        }

        private static void TestEnemyPanelContentShrinkingListClamping()
        {
            var content = new EnemyPanelContent();
            MyVector tenItems = CreateMockEnemyList(10);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(tenItems, viewport, isTouch: false);

            // Scroll down to last item (index 9)
            var input = new UiInputContext();
            for (int i = 0; i < 9; i++)
            {
                GameCanvas.clearKeyPressed();
                GameCanvas.keyPressed[22] = true;
                content.HandleInput(input);
            }
            Assert(content.SelectedIndex == 9, "Selected index should be 9");

            // Shrink to 3 items on refresh
            MyVector threeItems = CreateMockEnemyList(3);
            content.Refresh(threeItems, viewport);
            Assert(content.ItemsCount == 3, "ItemsCount must be 3");
            Assert(content.SelectedIndex == 2, "SelectedIndex must be clamped to 2 (itemsCount - 1)");
            Assert(content.ScrollY <= content.ScrollLimit, "ScrollY must be clamped to new ScrollLimit");

            // Shrink to 0 items
            MyVector zeroItems = new MyVector("Zero");
            content.Refresh(zeroItems, viewport);
            Assert(content.ItemsCount == 0, "ItemsCount must be 0");
            Assert(content.SelectedIndex == -1, "SelectedIndex must be clamped to -1 for 0 items");
            Assert(content.ScrollY == 0, "ScrollY must be clamped to 0");

            content.Unbind();
        }

        private static void TestEnemyPanelContentModalAndInactiveGuards()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: false);

            // When modal is active (IsModalBlocked = true), input must be rejected
            var blockedInput = new UiInputContext();
            blockedInput.IsModalBlocked = true;
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[12] = true;
            EnemyContentAction blockedAction = content.HandleInput(blockedInput);
            Assert(blockedAction.Type == EnemyContentActionType.None, "Modal blocked input must return None");

            // When inactive (after Unbind), input must be rejected
            content.Unbind();
            var normalInput = new UiInputContext();
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[12] = true;
            EnemyContentAction inactiveAction = content.HandleInput(normalInput);
            Assert(inactiveAction.Type == EnemyContentActionType.None, "Inactive content must return None");
        }

        private static void TestEnemyPanelContentDelayedActionSingleEmission()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: false);

            var input = new UiInputContext();

            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[12] = true;
            EnemyContentAction act1 = content.HandleInput(input);
            Assert(act1.Type == EnemyContentActionType.OpenActions, "First fire must emit OpenActions");

            // Second frame with no keys pressed
            GameCanvas.clearKeyPressed();
            EnemyContentAction act2 = content.HandleInput(input);
            Assert(act2.Type == EnemyContentActionType.None, "Second frame without new input must emit None");

            content.Unbind();
        }

        private static void TestEnemyPanelContentDragLifecycleIsVisibleToHost()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(10);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: true);
            Assert(!content.IsDragging, "IsDragging must be false initially");

            // Pointer down in viewport
            GameCanvas.isPointerDown = true;
            GameCanvas.px = 50;
            GameCanvas.py = 100;
            content.HandleInput(new UiInputContext());
            Assert(content.IsDragging, "IsDragging must be true while pointer is held down in viewport");

            // Pointer release
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            content.HandleInput(new UiInputContext());
            Assert(!content.IsDragging, "IsDragging must be false after pointer release");

            content.Unbind();
        }

        private static void TestEnemyPanelContentRefreshInvalidatesPendingAction()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 120);

            content.Bind(items, viewport, isTouch: false);

            var input = new UiInputContext();
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[12] = true;
            EnemyContentAction pendingAction = content.HandleInput(input);

            Assert(pendingAction.Type == EnemyContentActionType.OpenActions, "Action created");
            Assert(content.IsActionCurrent(pendingAction), "Action should be current before refresh");

            // Refresh happens (e.g. enemy list update from server)
            content.Refresh(items, viewport);

            Assert(!content.IsActionCurrent(pendingAction), "Action must be invalidated after Refresh due to revision mismatch");

            content.Unbind();
        }

        private static void TestEnemyPanelContentPaintDelegationAndVisibleRange()
        {
            var content = new EnemyPanelContent();
            MyVector items = CreateMockEnemyList(10);
            UiRect viewport = new UiRect(10, 80, 200, 72); // Height 72 = exactly 3 rows visible

            content.Bind(items, viewport, isTouch: false);

            mGraphics g = new mGraphics();
            SetupMockParts();
            SmallImage.ResetTracking();
            content.Paint(g);

            // With height 72 and itemHeight 24, at cmy=0 exactly rows [0, 3) are visible.
            UiListRange range = UiListLayout.GetVisibleRange(10, 24, 0, 72);
            Assert(range.Count == 3, "Visible range must contain exactly 3 rows");
            Assert(SmallImage.DrawSmallImageCount == range.Count, "Paint must draw one avatar per visible row only");
            Assert(!SmallImage.DrawnImageIds.Contains(104), "Paint must not draw off-screen row 4");

            content.Unbind();
        }

        private static void TestEnemyListViewEmptyStateAndAvatarContracts()
        {
            SetupMockParts();
            mGraphics g = new mGraphics();
            SmallImage.ResetTracking();

            // 1. Empty state test
            MyVector emptyList = new MyVector("Empty");
            EnemyListView.Paint(g, 10, 80, 200, 120, 0, -1, emptyList, 0);

            Assert(g.DrawnStrings.Contains(mResources.no_enemy), "Empty state must draw mResources.no_enemy");
            Assert(SmallImage.DrawSmallImageCount == 0, "Empty state must not draw any small images");

            // 2. Populated state test
            g.ResetTracking();
            SmallImage.ResetTracking();
            MyVector items = CreateMockEnemyList(4);
            // items[0]: headICON = 100, isOnline = true
            // items[1]: headICON = -1, head = 1, isOnline = false
            // items[2]: headICON = 102, isOnline = true
            // items[3]: headICON = -1, head = 3, isOnline = false

            EnemyListView.Paint(g, 10, 80, 200, 120, 0, 0, items, 4);

            // Verify avatar contract
            Assert(SmallImage.DrawnImageIds.Contains(100), "Avatar with headICON 100 must be drawn directly");
            Assert(SmallImage.DrawnImageIds.Contains(501), "Avatar with headICON -1 must draw part image 501 from GameScr.parts");

            // Verify online vs offline font contract
            // Online item (items[0]) has green name and blue power
            Assert(g.DrawnFontTags.Contains("green"), "Online enemy name must use green font");
            Assert(g.DrawnFontTags.Contains("blue"), "Online enemy power info must use blue font");

            // Offline item (items[1]) has grey name and grey power
            Assert(g.DrawnFontTags.Contains("grey"), "Offline enemy must use grey font");
        }

        private static void TestSharedVerticalListLifecycleAndRevision()
        {
            var state = new UiVerticalListState();
            UiRect viewport = new UiRect(10, 80, 200, 100);

            Assert(!state.IsBound && !state.IsActive, "Shared list state starts inactive");
            Assert(state.SelectedIndex == -1, "Shared list state starts without selection");

            state.Bind(itemCount: 10, itemHeight: 24, viewport: viewport, isTouch: false);
            Assert(state.IsBound && state.IsActive, "Bind activates shared list state");
            Assert(state.ItemCount == 10, "Bind stores item count");
            Assert(state.SelectedIndex == 0, "PC bind selects the first row");
            Assert(state.ScrollLimit == 140, "Shared state calculates the expected scroll limit");
            Assert(state.BindingRevision == 1, "Bind increments binding revision");

            state.MoveSelection(-1);
            Assert(state.SelectedIndex == 9, "MoveSelection wraps upward from the first row");
            Assert(state.ScrollY == state.ScrollLimit, "Wrapped selection clamps scroll to the lower bound");

            state.Refresh(itemCount: 2, viewport: viewport);
            Assert(state.SelectedIndex == 1, "Refresh clamps selection when the list shrinks");
            Assert(state.ScrollY == 0 && state.ScrollTargetY == 0, "Refresh clamps scroll when the list shrinks");
            Assert(state.BindingRevision == 2, "Refresh increments binding revision");

            state.Unbind();
            Assert(!state.IsBound && !state.IsActive, "Unbind deactivates shared list state");
            Assert(state.SelectedIndex == -1 && state.ScrollY == 0, "Unbind resets selection and scroll");
            Assert(state.BindingRevision == 3, "Unbind invalidates the previous binding revision");
        }

        private static void TestSharedVerticalListActivationAndWheel()
        {
            var state = new UiVerticalListState();
            UiRect viewport = new UiRect(10, 80, 200, 100);
            state.Bind(itemCount: 10, itemHeight: 24, viewport: viewport, isTouch: true);

            state.BeginPointer(pointerY: 100);
            UiVerticalListActivation activation = state.ReleasePointer(pointerY: 100);
            Assert(activation.IsTriggered, "A quick pointer release triggers the selected row");
            Assert(activation.SelectedIndex == 0, "Pointer activation resolves the correct row");
            Assert(activation.MenuY == 104, "Pointer activation preserves menu anchoring");
            Assert(activation.DelayFrames == 10, "Pointer activation preserves visual feedback delay");

            state.UpdateScrollMouse(-2);
            Assert(state.ScrollTargetY == 24, "Mouse wheel updates the shared scroll target");

            state.MoveSelection(1);
            UiVerticalListActivation keyboardActivation = state.ActivateSelection(delayFrames: 2);
            Assert(keyboardActivation.IsTriggered, "Keyboard fire can activate the shared selection");
            Assert(keyboardActivation.SelectedIndex == 1, "Keyboard activation uses the current selection");
            Assert(keyboardActivation.DelayFrames == 2, "Keyboard activation preserves its delay");
        }

        private static void TestSharedVerticalListPreservesDragPolicies()
        {
            UiRect viewport = new UiRect(10, 80, 200, 100);

            var clamped = new UiVerticalListState();
            clamped.Bind(itemCount: 20, itemHeight: 24, viewport: viewport, isTouch: true);
            clamped.BeginPointer(pointerY: 100);
            clamped.DragPointer(pointerY: 160, UiVerticalListDragMode.Clamped);
            Assert(clamped.ScrollY == 0, "Clamped drag mode keeps Enemy scroll at the upper bound");

            var elastic = new UiVerticalListState();
            elastic.Bind(itemCount: 20, itemHeight: 24, viewport: viewport, isTouch: true);
            elastic.BeginPointer(pointerY: 100);
            elastic.DragPointer(pointerY: 160, UiVerticalListDragMode.Elastic);
            Assert(elastic.ScrollY == -60, "Elastic drag mode preserves Top overscroll behavior");
            elastic.ReleasePointer(pointerY: 160);
            Assert(elastic.ScrollTargetY == 0, "Elastic release targets the valid upper bound");
        }

        private static void TestLifecycleBeginRequest()
        {
            var lc = new EnemyPanelLifecycle();
            Assert(!lc.IsOpenRequested, "Initial IsOpenRequested must be false");
            Assert(!lc.IsWaitDialogOwned, "Initial wait-dialog ownership must be false");
            lc.BeginRequest();
            Assert(lc.IsOpenRequested, "IsOpenRequested must be true after BeginRequest");
            Assert(lc.IsWaitDialogOwned, "BeginRequest must own the Enemy wait dialog");
            lc.CancelRequest();
            Assert(!lc.IsOpenRequested, "IsOpenRequested must be false after CancelRequest");
        }

        private static void TestLifecycleOpenRequestedConsumesMarker()
        {
            var lc = new EnemyPanelLifecycle();
            lc.BeginRequest();
            var disp = lc.OnListReceived(isEnemyOpen: false);
            Assert(disp == EnemyListDisposition.OpenRequested, "Must return OpenRequested when request pending and panel not open");
            Assert(!lc.IsOpenRequested, "Marker must be consumed (false) after OpenRequested");
            Assert(lc.DataRevision == 1, "DataRevision must increment to 1");
        }

        private static void TestLifecycleRefreshCurrentWhenOpen()
        {
            var lc = new EnemyPanelLifecycle();
            var disp = lc.OnListReceived(isEnemyOpen: true);
            Assert(disp == EnemyListDisposition.RefreshCurrent, "Must return RefreshCurrent when isEnemyOpen is true");
            Assert(!lc.IsOpenRequested, "IsOpenRequested must be false");
            Assert(lc.DataRevision == 1, "DataRevision must increment to 1");
        }

        private static void TestLifecycleCacheOnlyWhenNotRequestedAndNotOpen()
        {
            var lc = new EnemyPanelLifecycle();
            var disp = lc.OnListReceived(isEnemyOpen: false);
            Assert(disp == EnemyListDisposition.CacheOnly, "Must return CacheOnly when not requested and not open");
            Assert(!lc.IsOpenRequested, "IsOpenRequested must remain false");
            Assert(lc.DataRevision == 1, "DataRevision still increments on received data");
        }

        private static void TestLifecycleStaleMarkerCancelledOnLeaving()
        {
            var lc = new EnemyPanelLifecycle();
            lc.BeginRequest();
            Assert(lc.IsOpenRequested, "Request begun");
            lc.OnLeavingType();
            Assert(!lc.IsOpenRequested, "Request cancelled on leaving type");
            var disp = lc.OnListReceived(isEnemyOpen: false);
            Assert(disp == EnemyListDisposition.CacheOnly, "Must be CacheOnly after leaving");
        }

        private static void TestLifecycleConsecutiveResponsesDoNotReopen()
        {
            var lc = new EnemyPanelLifecycle();
            lc.BeginRequest();
            var disp1 = lc.OnListReceived(isEnemyOpen: false);
            Assert(disp1 == EnemyListDisposition.OpenRequested, "First response opens panel");
            var disp2 = lc.OnListReceived(isEnemyOpen: true);
            Assert(disp2 == EnemyListDisposition.RefreshCurrent, "Second response refreshes in-place, does not reopen");
        }

        private static void TestLifecycleRefreshPreservesOrClampsSelectionAndScroll()
        {
            var content = new EnemyPanelContent();
            MyVector list5 = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(list5, viewport, isTouch: false);
            var input = new UiInputContext();
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input);
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input);
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input);
            Assert(content.SelectedIndex == 3, "SelectedIndex should be 3");

            content.Refresh(list5, viewport);
            Assert(content.SelectedIndex == 3, "Selection preserved on same length");

            MyVector list2 = CreateMockEnemyList(2);
            content.Refresh(list2, viewport);
            Assert(content.SelectedIndex == 1, "Selection clamped to last item (1)");
            Assert(content.ScrollY <= content.ScrollLimit, "Scroll clamped to limit");

            MyVector list0 = new MyVector();
            content.Refresh(list0, viewport);
            Assert(content.SelectedIndex == -1, "Selection reset to -1 when empty");
            Assert(content.ScrollY == 0, "Scroll reset to 0 when empty");
        }

        private static void TestLifecycleRefreshCancelsPendingDelayedAction()
        {
            var content = new EnemyPanelContent();
            MyVector list = CreateMockEnemyList(5);
            UiRect viewport = new UiRect(10, 80, 200, 100);
            content.Bind(list, viewport, isTouch: true);

            var input = new UiInputContext();
            GameCanvas.isPointerDown = true;
            GameCanvas.isPointerJustRelease = false;
            GameCanvas.px = 50;
            GameCanvas.py = 100;
            content.HandleInput(input);

            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            var clickRelease = new UiInputContext();
            EnemyContentAction action = content.HandleInput(clickRelease);

            Assert(action.Type == EnemyContentActionType.OpenActions, "Click release should create OpenActions action");
            Assert(content.IsActionCurrent(action), "Action should be current before refresh");

            content.Refresh(list, viewport);
            Assert(!content.IsActionCurrent(action), "Action must not be current after refresh");
        }

        private static void TestLifecycleRefreshInvalidatesStaleMenuContext()
        {
            var lc = new EnemyPanelLifecycle();
            MyVector list = CreateMockEnemyList(3);
            InfoItem item0 = (InfoItem)list.elementAt(0);

            lc.OnListReceived(isEnemyOpen: true);
            lc.OpenMenu(item0);
            Assert(lc.ActiveMenuContext.IsValid, "Menu context must be valid after OpenMenu");
            Assert(lc.ActiveMenuContext.DataRevision == 1, "Menu revision must be 1");

            lc.OnListReceived(isEnemyOpen: true);
            Assert(!lc.ActiveMenuContext.IsValid, "ActiveMenuContext must be invalidated on new list data");
        }

        private static void TestLifecycleStaleMenuContextRejectedByValidation()
        {
            var lc = new EnemyPanelLifecycle();
            MyVector list = CreateMockEnemyList(3);
            InfoItem item0 = (InfoItem)list.elementAt(0);

            lc.OnListReceived(isEnemyOpen: true);
            lc.OpenMenu(item0);
            lc.OnListReceived(isEnemyOpen: true);

            bool valid = lc.IsMenuContextValid(item0, isEnemyOpen: true, list);
            Assert(!valid, "Stale menu context must be rejected");
        }

        private static void TestLifecycleCurrentMenuContextAcceptedOnce()
        {
            var lc = new EnemyPanelLifecycle();
            MyVector list = CreateMockEnemyList(3);
            InfoItem item0 = (InfoItem)list.elementAt(0);

            lc.OnListReceived(isEnemyOpen: true);
            lc.OpenMenu(item0);
            bool valid = lc.IsMenuContextValid(item0, isEnemyOpen: true, list);
            Assert(valid, "Current menu context must be valid");

            lc.InvalidateMenuContext();
            bool validAfter = lc.IsMenuContextValid(item0, isEnemyOpen: true, list);
            Assert(!validAfter, "Context must be invalid after dispatch");
        }

        private static void TestLifecycleMenuContextRequiresMatchingCharIdAndTarget()
        {
            var lc = new EnemyPanelLifecycle();
            MyVector list = CreateMockEnemyList(3);
            InfoItem item0 = (InfoItem)list.elementAt(0);
            InfoItem item1 = (InfoItem)list.elementAt(1);

            lc.OnListReceived(isEnemyOpen: true);
            lc.OpenMenu(item0);

            Assert(!lc.IsMenuContextValid(item1, isEnemyOpen: true, list), "Wrong target must be rejected");
            Assert(!lc.IsMenuContextValid(null, isEnemyOpen: true, list), "Null target must be rejected");

            int origId = item0.charInfo.charID;
            item0.charInfo.charID = 99999;
            Assert(!lc.IsMenuContextValid(item0, isEnemyOpen: true, list), "Mismatched charID must be rejected");
            item0.charInfo.charID = origId;

            MyVector emptyList = new MyVector();
            Assert(!lc.IsMenuContextValid(item0, isEnemyOpen: true, emptyList), "Target not in list must be rejected");
        }

        private static void TestLifecycleMenuContextRequiresOpenEnemyPanel()
        {
            var lc = new EnemyPanelLifecycle();
            MyVector list = CreateMockEnemyList(3);
            InfoItem item0 = (InfoItem)list.elementAt(0);

            lc.OnListReceived(isEnemyOpen: true);
            lc.OpenMenu(item0);
            Assert(!lc.IsMenuContextValid(item0, isEnemyOpen: false, list), "Must reject when panel is closed or on other type");
        }

        private static void TestLifecycleLeavingTypeInvalidatesMenuContextAndCancelsRequest()
        {
            var lc = new EnemyPanelLifecycle();
            MyVector list = CreateMockEnemyList(3);
            InfoItem item0 = (InfoItem)list.elementAt(0);

            lc.BeginRequest();
            lc.OnListReceived(isEnemyOpen: true);
            lc.OpenMenu(item0);
            lc.BeginRequest();

            Assert(lc.IsOpenRequested, "Request set");
            Assert(lc.ActiveMenuContext.IsValid, "Context valid");

            lc.OnLeavingType();
            Assert(!lc.IsOpenRequested, "Request cancelled on leaving type");
            Assert(!lc.ActiveMenuContext.IsValid, "Context invalidated on leaving type");
            Assert(!lc.IsWaitDialogOwned, "Wait-dialog ownership released on leaving type");
        }

        private static void TestLifecycleCacheOnlyDoesNotMutateState()
        {
            var lc = new EnemyPanelLifecycle();
            var disp = lc.OnListReceived(isEnemyOpen: false);
            Assert(disp == EnemyListDisposition.CacheOnly, "Should be CacheOnly");
            Assert(!lc.IsOpenRequested, "IsOpenRequested remains false");
            Assert(!lc.ActiveMenuContext.IsValid, "ActiveMenuContext remains invalid");
        }

        private static void TestLifecycleWaitDialogOwnershipIsSingleUse()
        {
            var lc = new EnemyPanelLifecycle();
            lc.BeginWaitDialog();

            Assert(lc.IsWaitDialogOwned, "Enemy operation must own its wait dialog");
            Assert(lc.ConsumeWaitDialogOwnership(), "First consume must authorize hiding the Enemy wait dialog");
            Assert(!lc.IsWaitDialogOwned, "Ownership must be cleared after consume");
            Assert(!lc.ConsumeWaitDialogOwnership(), "Ownership must not authorize a second hide");
        }

        private static void TestLifecycleLateCacheOnlyCannotConsumeAnotherWait()
        {
            var lc = new EnemyPanelLifecycle();
            lc.BeginRequest();

            bool shouldHideOnLeave = lc.OnLeavingType();
            Assert(shouldHideOnLeave, "Leaving must tell the host to close the Enemy-owned wait dialog");
            Assert(!lc.IsWaitDialogOwned, "Leaving must release Enemy wait ownership before another operation starts");

            EnemyListDisposition disposition = lc.OnListReceived(isEnemyOpen: false);
            Assert(disposition == EnemyListDisposition.CacheOnly, "Late response must be cache-only after leaving");
            Assert(!lc.ConsumeWaitDialogOwnership(), "Late cache-only response must not hide a later unrelated wait dialog");
        }
    }
}
