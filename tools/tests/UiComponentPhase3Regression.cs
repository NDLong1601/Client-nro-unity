using System;
using System.Collections.Generic;
using Game1;
using Game1.UI.Adapters;
using Game1.UI.Pilots;
using Nro.UI;

namespace Game1.Tests
{
    public static class UiComponentPhase3Regression
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Assertion failed: " + message);
            }
        }

        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("[PHASE3] Starting PKHistoryView regression tests...");

                TestRenderStateRestorationNormal();
                TestRenderStateRestorationOnException();
                TestLoadingState();
                TestEmptyState();
                TestSingleRowRenderAndColors();
                TestWinLoseIconAndFallbackText();
                TestViewportCulling();
                TestNegativeElapsedClamping();
                TestScrollLimitCalculation();

                Console.WriteLine("UI_COMPONENT_PHASE3_PRODUCTION_Game1_OK");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[PHASE3 ERROR] " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static void TestRenderStateRestorationNormal()
        {
            mGraphics g = new mGraphics();
            g.translate(10, 20);
            g.setClip(5, 5, 200, 300);
            mGraphics.RawRenderState before = g.getRawState();

            MyVector entries = new MyVector();
            entries.addElement(new PKHistoryEntry { opponentName = "Goku", won = true, completedAt = (int)(mSystem.currentTimeMillis() / 1000L) });

            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: 0,
                isLoading: false,
                entries: entries,
                imgWin: null,
                imgLose: null,
                myCharName: "Hero");

            mGraphics.RawRenderState after = g.getRawState();
            Assert(before.translateX == after.translateX && before.translateY == after.translateY, "Normal paint: translation must be restored");
            Assert(before.clipX == after.clipX && before.clipY == after.clipY && before.clipW == after.clipW && before.clipH == after.clipH, "Normal paint: clip must be restored");
            Assert(before.isClip == after.isClip && before.isTranslate == after.isTranslate, "Normal paint: clip/translate flags must match");
            Console.WriteLine(" - TestRenderStateRestorationNormal: PASSED");
        }

        private static void TestRenderStateRestorationOnException()
        {
            mGraphics g = new mGraphics();
            g.translate(15, 25);
            g.setClip(0, 0, 100, 100);
            mGraphics.RawRenderState before = g.getRawState();

            // A broken vector element that throws an InvalidCastException when cast to PKHistoryEntry
            MyVector brokenVector = new MyVector();
            brokenVector.addElement("This is not a PKHistoryEntry");

            bool caught = false;
            try
            {
                PKHistoryView.Paint(
                    g,
                    xScroll: 2,
                    yScroll: 80,
                    wScroll: 172,
                    hScroll: 224,
                    cmy: 0,
                    selected: 0,
                    isLoading: false,
                    entries: brokenVector,
                    imgWin: null,
                    imgLose: null,
                    myCharName: "Hero");
            }
            catch (InvalidCastException)
            {
                caught = true;
            }

            Assert(caught, "Expected InvalidCastException on malformed element");
            mGraphics.RawRenderState after = g.getRawState();
            Assert(before.translateX == after.translateX && before.translateY == after.translateY, "Exception paint: translation must be restored");
            Assert(before.clipX == after.clipX && before.clipY == after.clipY && before.clipW == after.clipW && before.clipH == after.clipH, "Exception paint: clip must be restored");
            Console.WriteLine(" - TestRenderStateRestorationOnException: PASSED");
        }

        private static void TestLoadingState()
        {
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            entries.addElement(new PKHistoryEntry { opponentName = "Vegeta", won = true, completedAt = 100 });

            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: 0,
                isLoading: true,
                entries: entries,
                imgWin: null,
                imgLose: null,
                myCharName: "Hero");

            Assert(g.DrawnStrings.Contains("Đang tải..."), "Loading state must draw 'Đang tải...'");
            Assert(g.FillRectCount == 0, "Loading state must not draw rows");
            Console.WriteLine(" - TestLoadingState: PASSED");
        }

        private static void TestEmptyState()
        {
            mGraphics g = new mGraphics();
            MyVector empty = new MyVector();

            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: 0,
                isLoading: false,
                entries: empty,
                imgWin: null,
                imgLose: null,
                myCharName: "Hero");

            Assert(g.DrawnStrings.Contains("Chưa có lịch sử thách đấu"), "Empty state must draw 'Chưa có lịch sử thách đấu'");
            Assert(g.FillRectCount == 0, "Empty state must not draw rows");

            // Also test null entries
            g.ResetTracking();
            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: 0,
                isLoading: false,
                entries: null,
                imgWin: null,
                imgLose: null,
                myCharName: "Hero");

            Assert(g.DrawnStrings.Contains("Chưa có lịch sử thách đấu"), "Null entries must draw 'Chưa có lịch sử thách đấu'");
            Console.WriteLine(" - TestEmptyState: PASSED");
        }

        private static void TestSingleRowRenderAndColors()
        {
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            entries.addElement(new PKHistoryEntry { opponentName = "Piccolo", won = true, completedAt = (int)(mSystem.currentTimeMillis() / 1000L - 120) });

            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: 0,
                isLoading: false,
                entries: entries,
                imgWin: null,
                imgLose: null,
                myCharName: "PlayerOne");

            // 1 row = 1 background fillRect + 1 divider fillRect = 2 fillRects
            Assert(g.FillRectCount == 2, "Single row must draw background and divider (2 fillRect calls)");
            Assert(g.DrawnColors.Contains(UiColorTokens.RowSelected), "Selected row must use UiColorTokens.RowSelected");
            Assert(g.DrawnColors.Contains(UiColorTokens.RowDivider), "Divider must use UiColorTokens.RowDivider");
            Assert(g.DrawnStrings.Contains("PlayerOne"), "Must draw player name");
            Assert(g.DrawnStrings.Contains("Piccolo"), "Must draw opponent name");
            Console.WriteLine(" - TestSingleRowRenderAndColors: PASSED");
        }

        private static void TestWinLoseIconAndFallbackText()
        {
            // With icons null -> fallback texts
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            entries.addElement(new PKHistoryEntry { opponentName = "Frieza", won = true, completedAt = 100 });
            entries.addElement(new PKHistoryEntry { opponentName = "Cell", won = false, completedAt = 100 });

            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: -1,
                isLoading: false,
                entries: entries,
                imgWin: null,
                imgLose: null,
                myCharName: "Hero");

            Assert(g.DrawImageCount == 0, "No drawImage when icons are null");
            Assert(g.DrawnStrings.Contains("WIN"), "Must draw fallback 'WIN'");
            Assert(g.DrawnStrings.Contains("LOSE"), "Must draw fallback 'LOSE'");

            // With icons provided -> drawImage called
            g.ResetTracking();
            Image iconWin = new Image(16, 16);
            Image iconLose = new Image(16, 16);

            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: -1,
                isLoading: false,
                entries: entries,
                imgWin: iconWin,
                imgLose: iconLose,
                myCharName: "Hero");

            Assert(g.DrawImageCount == 2, "Must call drawImage twice when icons are provided");
            Assert(!g.DrawnStrings.Contains("WIN"), "Must not draw fallback 'WIN' when iconWin is present");
            Assert(!g.DrawnStrings.Contains("LOSE"), "Must not draw fallback 'LOSE' when iconLose is present");
            Console.WriteLine(" - TestWinLoseIconAndFallbackText: PASSED");
        }

        private static void TestViewportCulling()
        {
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            for (int i = 0; i < 30; i++)
            {
                entries.addElement(new PKHistoryEntry { opponentName = "Opponent" + i, won = (i % 2 == 0), completedAt = 100 });
            }

            // Viewport height: 100px. Each item is 34px.
            // Items visible from yScroll (80) to yScroll + hScroll (180).
            // visible rows: ceil(100 / 34) + 1 = 4 rows max.
            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 100,
                cmy: 0,
                selected: -1,
                isLoading: false,
                entries: entries,
                imgWin: null,
                imgLose: null,
                myCharName: "Hero");

            int drawnRows = g.FillRectCount / 2;
            Assert(drawnRows >= 3 && drawnRows <= 5, "Visible row culling check: drawn rows must be 3-5 instead of all 30 (got " + drawnRows + ")");
            Assert(g.DrawnStrings.Contains("Opponent0"), "Opponent0 must be drawn");
            Assert(!g.DrawnStrings.Contains("Opponent25"), "Opponent25 must be culled");
            Console.WriteLine(" - TestViewportCulling: PASSED (culled 30 items down to " + drawnRows + " visible)");
        }

        private static void TestNegativeElapsedClamping()
        {
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            long nowSec = mSystem.currentTimeMillis() / 1000L;
            // Completed in future (+5000 seconds)
            entries.addElement(new PKHistoryEntry { opponentName = "FutureMan", won = true, completedAt = (int)(nowSec + 5000) });

            PKHistoryView.Paint(
                g,
                xScroll: 2,
                yScroll: 80,
                wScroll: 172,
                hScroll: 224,
                cmy: 0,
                selected: 0,
                isLoading: false,
                entries: entries,
                imgWin: null,
                imgLose: null,
                myCharName: "Hero");

            // Elapsed clamped to 0 -> NinjaUtil.getTimeAgo(0) gives "0 giây"
            Assert(g.DrawnStrings.Exists(s => s.Contains("0 giây")), "Negative elapsed time must be clamped to 0");
            Console.WriteLine(" - TestNegativeElapsedClamping: PASSED");
        }

        private static void TestScrollLimitCalculation()
        {
            int itemCount = 10;
            int itemHeight = PKHistoryView.ITEM_HEIGHT;
            int hScroll = 224;

            int cmyLim = itemCount * itemHeight - hScroll;
            if (cmyLim < 0) cmyLim = 0;
            Assert(cmyLim == 116, "Scroll limit for 10 items (340px) with hScroll=224 must be 116");

            // Few items
            itemCount = 2;
            cmyLim = itemCount * itemHeight - hScroll;
            if (cmyLim < 0) cmyLim = 0;
            Assert(cmyLim == 0, "Scroll limit for 2 items (68px) with hScroll=224 must be clamped to 0");
            Console.WriteLine(" - TestScrollLimitCalculation: PASSED");
        }
    }
}
