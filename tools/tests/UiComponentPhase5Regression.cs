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
    public static class UiComponentPhase5Regression
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("ASSERTION_FAILED: " + message);
            }
        }

        private static MyVector CreateMockTopList(int count)
        {
            MyVector list = new MyVector("TopList");
            for (int i = 0; i < count; i++)
            {
                TopInfo info = new TopInfo
                {
                    pId = 1000 + i,
                    name = "Player_" + i,
                    info = "Power: " + (1000000 - i * 50000),
                    rank = i + 1,
                    headID = 10,
                    headICON = (short)(20 + i)
                };
                list.addElement(info);
            }
            return list;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("--- Running UiComponentPhase5Regression ---");

            TestTopPanelContentLifecycle();
            TestTopPanelContentOwnershipAndLimits();
            TestTopPanelContentKeyboardNavigation();
            TestTopPanelContentKeyboardFire();
            TestTopPanelContentClickVsDrag();
            TestTopPanelContentMouseWheel();
            TestTopPanelContentEmptyListSafety();
            TestTopPanelContentShrinkingListClamping();
            TestTopPanelContentModalAndInactiveGuards();
            TestTopPanelContentDelayedActionSingleEmission();
            TestTopPanelContentDragLifecycleIsVisibleToHost();
            TestTopPanelContentRefreshInvalidatesPendingAction();
            TestTopPanelContentPaintDelegation();

            Console.WriteLine("UI_COMPONENT_PHASE5_PRODUCTION_Game1_OK");
        }

        private static void TestTopPanelContentLifecycle()
        {
            var content = new TopPanelContent();
            Assert(!content.IsBound, "New content must not be bound");
            Assert(!content.IsActive, "New content must not be active");

            MyVector items = CreateMockTopList(5);
            UiRect viewport = new UiRect(10, 80, 200, 160);

            // 1. Bind on PC (not touch) -> SelectedIndex should be 0
            content.Bind(items, "Top Sức Mạnh", challengeMode: false, viewport: viewport, isTouch: false);
            Assert(content.IsBound, "Content must be bound after Bind");
            Assert(content.IsActive, "Content must be active after Bind");
            Assert(content.ItemsCount == 5, "ItemsCount must match list size");
            Assert(content.SelectedIndex == 0, "PC bind must select first item (0)");
            Assert(content.TopName == "Top Sức Mạnh", "TopName must match");
            Assert(!content.IsChallengeMode, "Challenge mode must be false");

            // 2. Refresh with same count
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
            content.Bind(items, "Top Thách Đấu", challengeMode: true, viewport: viewport, isTouch: true);
            Assert(content.IsBound, "Content must be bound");
            Assert(content.SelectedIndex == -1, "Touch bind must default to selected -1");
            Assert(content.IsChallengeMode, "Challenge mode must be true");

            content.Unbind();
        }

        private static void TestTopPanelContentOwnershipAndLimits()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(10); // 10 * 24 = 240px
            UiRect viewport = new UiRect(10, 80, 200, 100); // viewport height = 100px
            content.Bind(items, "Top 10", challengeMode: false, viewport: viewport, isTouch: false);

            int expectedMaxScroll = UiListLayout.CalculateMaxScroll(10, TopPanelContent.ITEM_HEIGHT, 100);
            Assert(expectedMaxScroll == 140, "Expected max scroll is 240 - 100 = 140");
            Assert(content.ScrollLimit == expectedMaxScroll, "Content ScrollLimit must match UiListLayout calculation");
            Assert(content.ScrollY == 0, "Initial ScrollY must be 0");
            Assert(content.ScrollTargetY == 0, "Initial ScrollTargetY must be 0");

            content.Unbind();
        }

        private static void TestTopPanelContentKeyboardNavigation()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(4);
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(items, "Top Nav", challengeMode: false, viewport: viewport, isTouch: false);
            var input = new UiInputContext();

            Assert(content.SelectedIndex == 0, "Initial selection at index 0");

            // Press Down: 0 -> 1
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true; // PC down arrow
            TopContentAction act = content.HandleInput(input);
            Assert(act.Type == TopContentActionType.None, "Navigation must return ActionType.None");
            Assert(content.SelectedIndex == 1, "Selected must be 1 after Down");

            // Press Down: 1 -> 2
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input);
            Assert(content.SelectedIndex == 2, "Selected must be 2 after Down");

            // Press Down: 2 -> 3
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input);
            Assert(content.SelectedIndex == 3, "Selected must be 3 after Down");

            // Press Down: 3 -> 0 (wrap around)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[22] = true;
            content.HandleInput(input);
            Assert(content.SelectedIndex == 0, "Selected must wrap to 0 after Down at end");

            // Press Up: 0 -> 3 (wrap around)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[21] = true; // PC up arrow
            content.HandleInput(input);
            Assert(content.SelectedIndex == 3, "Selected must wrap to 3 after Up at top");

            GameCanvas.clearKeyPressed();
            content.Unbind();
        }

        private static void TestTopPanelContentKeyboardFire()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(3);
            UiRect viewport = new UiRect(10, 80, 200, 100);
            content.Bind(items, "Top Fire", challengeMode: false, viewport: viewport, isTouch: false);
            var input = new UiInputContext();

            // At index 0, press Fire (key 12 or 25)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[25] = true; // PC fire
            TopContentAction action = content.HandleInput(input);
            Assert(action.Type == TopContentActionType.Fire, "Fire key must produce TopContentActionType.Fire");
            Assert(action.SelectedIndex == 0, "Action selected index must match");
            Assert(action.SelectedInfo != null && action.SelectedInfo.name == "Player_0", "Action SelectedInfo must match row");
            Assert(action.DelayFrames == 2, "Keyboard Fire delay must be 2 frames");

            // Touch mode: selected == -1 -> Fire should do nothing
            content.Unbind();
            content.Bind(items, "Top Fire Touch", challengeMode: false, viewport: viewport, isTouch: true);
            Assert(content.SelectedIndex == -1, "Touch start selected is -1");
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[25] = true;
            TopContentAction touchAction = content.HandleInput(input);
            Assert(touchAction.Type == TopContentActionType.None, "Fire key when selected == -1 must return None");

            GameCanvas.clearKeyPressed();
            content.Unbind();
        }

        private static void TestTopPanelContentClickVsDrag()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(10);
            UiRect viewport = new UiRect(10, 80, 200, 100);
            content.Bind(items, "Top Gesture", challengeMode: false, viewport: viewport, isTouch: true);
            var input = new UiInputContext();

            // Scenario 1: Clean click on row 2
            // Row 2 is at y = 80 + 2 * 24 = 128. Click at (50, 135)
            GameCanvas.clearAllPointerEvent();
            GameCanvas.px = 50;
            GameCanvas.py = 135;
            GameCanvas.isPointerDown = true;
            GameCanvas.isPointerJustDown = true;

            content.HandleInput(input);

            // Release at same position -> Click!
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;

            TopContentAction clickAction = content.HandleInput(input);
            Assert(clickAction.Type == TopContentActionType.Fire, "Click must produce Fire action");
            Assert(clickAction.SelectedIndex == 2, "Clicking at y=135 in viewport y=80 must select row 2 ((135-80)/24 = 2)");
            Assert(clickAction.SelectedInfo != null && clickAction.SelectedInfo.name == "Player_2", "SelectedInfo must be Player_2");
            Assert(clickAction.DelayFrames == 10, "Click action must have 10 delay frames for visual feedback");

            // Scenario 2: Drag (scroll)
            // Down at y=130, drag to y=90 (delta = 40px > threshold 20px)
            GameCanvas.clearAllPointerEvent();
            GameCanvas.px = 50;
            GameCanvas.py = 130;
            GameCanvas.isPointerDown = true;
            content.HandleInput(input);

            GameCanvas.py = 90;
            content.HandleInput(input);
            Assert(content.SelectedIndex == -1, "Dragging must deselect active row (-1)");

            // Release after drag
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            TopContentAction dragAction = content.HandleInput(input);
            Assert(dragAction.Type == TopContentActionType.None, "Drag release must NOT emit Fire action");

            GameCanvas.clearAllPointerEvent();
            content.Unbind();
        }

        private static void TestTopPanelContentMouseWheel()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(10); // max scroll = 140
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(items, "Top Wheel", challengeMode: false, viewport: viewport, isTouch: false);

            Assert(content.ScrollTargetY == 0, "Initial target is 0");

            // Wheel down (a < 0, e.g. -2)
            content.UpdateScrollMouse(-2);
            Assert(content.ScrollTargetY > 0, "Scrolling wheel down should increase scroll target");

            // Wheel up (a > 0, e.g. +5)
            content.UpdateScrollMouse(5);
            Assert(content.ScrollTargetY >= 0, "Scrolling wheel up should clamp target >= 0");

            content.Unbind();
        }

        private static void TestTopPanelContentEmptyListSafety()
        {
            var content = new TopPanelContent();
            MyVector emptyItems = new MyVector("Empty");
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(emptyItems, "Empty Top", challengeMode: false, viewport: viewport, isTouch: false);

            Assert(content.ItemsCount == 0, "ItemsCount must be 0");
            Assert(content.SelectedIndex == -1, "Selected index for empty list must be -1");
            Assert(content.ScrollLimit == 0, "ScrollLimit for empty list must be 0");

            var input = new UiInputContext();
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[25] = true; // Fire
            TopContentAction action = content.HandleInput(input);
            Assert(action.Type == TopContentActionType.None, "Input on empty list must return None");

            // Pointer click on empty list
            GameCanvas.clearAllPointerEvent();
            GameCanvas.px = 50;
            GameCanvas.py = 50;
            GameCanvas.isPointerDown = true;
            content.HandleInput(input);
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            TopContentAction pointerAct = content.HandleInput(input);
            Assert(pointerAct.Type == TopContentActionType.None, "Pointer click on empty list must return None");

            content.Unbind();
        }

        private static void TestTopPanelContentShrinkingListClamping()
        {
            var content = new TopPanelContent();
            MyVector items5 = CreateMockTopList(5);
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(items5, "Top Shrink", challengeMode: false, viewport: viewport, isTouch: false);

            // Move selection to index 4 (last)
            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[21] = true; // Up wraps 0 -> 4
            var input = new UiInputContext();
            content.HandleInput(input);
            Assert(content.SelectedIndex == 4, "Selection is at 4");

            // Refresh with 3 items -> selection must clamp to 2
            MyVector items3 = CreateMockTopList(3);
            content.Refresh(items3, viewport);
            Assert(content.ItemsCount == 3, "ItemsCount updated to 3");
            Assert(content.SelectedIndex == 2, "Selection at 4 must be clamped to 2 (items.size - 1)");

            // Refresh with 0 items -> selection must clamp to -1
            MyVector items0 = new MyVector("Zero");
            content.Refresh(items0, viewport);
            Assert(content.ItemsCount == 0, "ItemsCount updated to 0");
            Assert(content.SelectedIndex == -1, "Selection must clamp to -1 for 0 items");

            content.Unbind();
        }

        private static void TestTopPanelContentModalAndInactiveGuards()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(3);
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(items, "Top Guards", challengeMode: false, viewport: viewport, isTouch: false);

            var input = new UiInputContext();
            input.IsModalBlocked = true;

            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[25] = true;
            TopContentAction act = content.HandleInput(input);
            Assert(act.Type == TopContentActionType.None, "Modal blocked input must return None");

            input.IsModalBlocked = false;
            content.Unbind(); // now inactive

            TopContentAction inactiveAct = content.HandleInput(input);
            Assert(inactiveAct.Type == TopContentActionType.None, "Inactive content must return None");
        }

        private static void TestTopPanelContentDelayedActionSingleEmission()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(3);
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(items, "Top Single Emission", challengeMode: false, viewport: viewport, isTouch: false);
            var input = new UiInputContext();

            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[25] = true; // Fire
            TopContentAction act1 = content.HandleInput(input);
            Assert(act1.Type == TopContentActionType.Fire, "First fire should emit Fire action");

            // Next frame without keys
            GameCanvas.clearKeyPressed();
            TopContentAction act2 = content.HandleInput(input);
            Assert(act2.Type == TopContentActionType.None, "Next frame without key must not repeat Fire action");

            content.Unbind();
        }

        private static void TestTopPanelContentDragLifecycleIsVisibleToHost()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(5);
            UiRect viewport = new UiRect(10, 80, 200, 100);
            content.Bind(items, "Top Drag State", challengeMode: false, viewport: viewport, isTouch: true);
            var input = new UiInputContext();

            GameCanvas.clearAllPointerEvent();
            GameCanvas.px = 50;
            GameCanvas.py = 100;
            GameCanvas.isPointerDown = true;
            content.HandleInput(input);
            Assert(content.IsDragging, "Host must be able to observe an active content drag");

            GameCanvas.py = 40;
            GameCanvas.isPointerDown = false;
            GameCanvas.isPointerJustRelease = true;
            content.HandleInput(input);
            Assert(!content.IsDragging, "Content drag state must clear after release outside the viewport");

            GameCanvas.clearAllPointerEvent();
            content.Unbind();
        }

        private static void TestTopPanelContentRefreshInvalidatesPendingAction()
        {
            var content = new TopPanelContent();
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(CreateMockTopList(3), "Top Revision A", challengeMode: false, viewport: viewport, isTouch: false);
            var input = new UiInputContext();

            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[25] = true;
            TopContentAction staleAction = content.HandleInput(input);
            Assert(staleAction.Type == TopContentActionType.Fire, "Initial binding must emit a fire action");
            Assert(content.IsActionCurrent(staleAction), "Newly emitted action must belong to the active binding");

            content.Refresh(CreateMockTopList(2), viewport);
            Assert(!content.IsActionCurrent(staleAction), "Refreshing data must invalidate delayed actions from the previous revision");

            GameCanvas.clearKeyPressed();
            GameCanvas.keyPressed[25] = true;
            TopContentAction currentAction = content.HandleInput(input);
            Assert(content.IsActionCurrent(currentAction), "Action emitted after refresh must use the current binding revision");

            GameCanvas.clearKeyPressed();
            content.Unbind();
            Assert(!content.IsActionCurrent(currentAction), "Unbind must invalidate actions emitted while the content was active");
        }

        private static void TestTopPanelContentPaintDelegation()
        {
            var content = new TopPanelContent();
            MyVector items = CreateMockTopList(5);
            UiRect viewport = new UiRect(0, 0, 200, 100);
            content.Bind(items, "Top Paint", challengeMode: false, viewport: viewport, isTouch: false);

            mGraphics g = new mGraphics();
            content.Paint(g, myCharId: 1000); // 1000 is Player_0
            Assert(g.DrawnStrings.Count > 0, "Paint must draw strings for top ranking rows");

            content.Unbind();
        }
    }
}
