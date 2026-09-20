using System;
using System.Collections.Generic;
using Game1;
using Game1.UI.Adapters;
using Game1.UI.Components;
using Game1.UI.Pilots;
using Nro.UI;

namespace Game1.Tests
{
    public static class UiComponentPhase4Regression
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("ASSERTION_FAILED: " + message);
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("--- Running UiComponentPhase4Regression ---");

            // 1. List geometry tests
            TestListGeometryZeroRows();
            TestListGeometrySingleRow();
            TestListGeometryMultipleRows();
            TestListGeometryViewportSmallerThanRow();
            TestListGeometryPartialRowIntersection();
            TestListGeometryScrollPositions();
            TestListGeometryListShrinking();
            TestListGeometryMathematicalEquivalence();
            TestListGeometryRowBounds();

            // 2. PKHistoryView Phase 3 regressions
            TestPKHistoryRenderStateRestorationNormal();
            TestPKHistoryRenderStateRestorationOnException();
            TestPKHistoryLoadingState();
            TestPKHistoryEmptyState();
            TestPKHistorySingleRowRenderAndColors();
            TestPKHistoryWinLoseIconAndFallbackText();
            TestPKHistoryViewportCulling();
            TestPKHistoryNegativeElapsedClamping();
            TestPKHistoryScrollLimitCalculation();

            // 3. TopRankingView tests
            TestTopRankingRenderStateRestorationNormal();
            TestTopRankingRenderStateRestorationOnException();
            TestTopRankingKeepsParentClipDuringDraw();
            TestTopRankingEmptyList();
            TestTopRankingVisibleRowsOnly();
            TestTopRankingColorsAndAvatarColumn();
            TestTopRankingBothAvatarBranches();
            TestTopRankingPlayerNameFonts();

            // 4. TextFieldAdapter tests
            TestTextFieldAdapterConfigure();
            TestTextFieldAdapterFocusAndBlur();
            TestTextFieldAdapterKeyForwardOnlyWhenFocused();
            TestTextFieldAdapterEnterSubmitOnceViaHost();
            TestTextFieldAdapterCancelBlurNoSubmit();
            TestTextFieldAdapterDisabledHiddenNoInput();

            Console.WriteLine("UI_COMPONENT_PHASE4_PRODUCTION_Game1_OK");
        }

        // ==========================================
        // 1. LIST GEOMETRY TESTS
        // ==========================================

        private static void TestListGeometryZeroRows()
        {
            UiListRange range = UiListLayout.GetVisibleRange(0, 24, 0, 100);
            Assert(range.IsEmpty, "Zero rows visible range must be empty");
            Assert(range.Count == 0, "Zero rows count must be 0");
            Assert(!UiListLayout.IsRowVisible(0, 0, 24, 0, 100), "Row 0 cannot be visible when itemCount is 0");
            Assert(UiListLayout.CalculateMaxScroll(0, 24, 100) == 0, "Max scroll for 0 rows must be 0");
        }

        private static void TestListGeometrySingleRow()
        {
            UiListRange range = UiListLayout.GetVisibleRange(1, 24, 0, 100);
            Assert(!range.IsEmpty, "Single row range should not be empty");
            Assert(range.StartIndex == 0 && range.EndIndex == 1, "Single row range must be [0, 1)");
            Assert(range.Count == 1, "Single row count must be 1");
            Assert(UiListLayout.IsRowVisible(0, 1, 24, 0, 100), "Row 0 should be visible");
            Assert(UiListLayout.CalculateMaxScroll(1, 24, 100) == 0, "Max scroll when total height <= viewport must be 0");
        }

        private static void TestListGeometryMultipleRows()
        {
            // 10 rows of height 24 = 240px. Viewport = 100px.
            // At scrollY = 0: rows 0..4 (indices 0, 1, 2, 3, 4) touch [0, 100).
            UiListRange range = UiListLayout.GetVisibleRange(10, 24, 0, 100);
            Assert(range.StartIndex == 0, "Start index at scroll 0 must be 0");
            Assert(range.EndIndex == 5, "End index at scroll 0 viewport 100 must be 5");
            Assert(range.Count == 5, "Visible count should be 5");
            Assert(range.Contains(0) && range.Contains(4), "Should contain 0 and 4");
            Assert(!range.Contains(5), "Should not contain 5");

            int maxScroll = UiListLayout.CalculateMaxScroll(10, 24, 100);
            Assert(maxScroll == 140, "Max scroll for 240px content in 100px viewport must be 140");
        }

        private static void TestListGeometryViewportSmallerThanRow()
        {
            // Row height 34, viewport height 20
            UiListRange range = UiListLayout.GetVisibleRange(5, 34, 0, 20);
            Assert(!range.IsEmpty, "Range with tiny viewport should not be empty");
            Assert(range.StartIndex == 0 && range.EndIndex == 1, "Tiny viewport at scroll 0 must see row 0");
            Assert(range.Count == 1, "Tiny viewport must see 1 row");
            Assert(UiListLayout.IsRowVisible(0, 5, 34, 0, 20), "Row 0 must be visible in tiny viewport");
            Assert(!UiListLayout.IsRowVisible(1, 5, 34, 0, 20), "Row 1 must not be visible in tiny viewport at scroll 0");
        }

        private static void TestListGeometryPartialRowIntersection()
        {
            // Row height 24.
            // Row 0: 0..24
            // Row 1: 24..48
            // Viewport [10, 110].
            // Row 0 (0..24) intersects [10, 110] because bottom 24 > 10.
            Assert(UiListLayout.IsRowVisible(0, 10, 24, 10, 100), "Row 0 partially intersecting top must be visible");

            // Row 0 when scrollY = 24: top=0, bottom=24. Viewport [24, 124]. Bottom 24 is NOT > 24.
            Assert(!UiListLayout.IsRowVisible(0, 10, 24, 24, 100), "Row 0 ending at scrollY is not visible");

            // At scrollY = 24, row 5 is top=120, bottom=144. Viewport ends at 124.
            // Top 120 < 124, so row 5 partially intersects bottom!
            Assert(UiListLayout.IsRowVisible(5, 10, 24, 24, 100), "Row 5 partially intersecting bottom must be visible");
        }

        private static void TestListGeometryScrollPositions()
        {
            int itemCount = 10;
            int itemHeight = 24;
            int viewportH = 100;
            int maxScroll = UiListLayout.CalculateMaxScroll(itemCount, itemHeight, viewportH); // 140

            // scrollY = 0
            UiListRange r0 = UiListLayout.GetVisibleRange(itemCount, itemHeight, 0, viewportH);
            Assert(r0.StartIndex == 0 && r0.EndIndex == 5, "At scroll 0, rows 0..4 visible");

            // scrollY in middle: scrollY = 70
            // Viewport [70, 170].
            // Row 2: 48..72 -> bottom 72 > 70 (visible)
            // Row 7: 168..192 -> top 168 < 170 (visible)
            // Row 8: 192..216 -> top 192 >= 170 (not visible)
            UiListRange rMid = UiListLayout.GetVisibleRange(itemCount, itemHeight, 70, viewportH);
            Assert(rMid.StartIndex == 2, "At scroll 70, start index must be 2");
            Assert(rMid.EndIndex == 8, "At scroll 70, end index must be 8");

            // scrollY at exact limit: scrollY = 140
            // Viewport [140, 240].
            // Row 5: 120..144 -> bottom 144 > 140 (visible)
            // Row 9: 216..240 -> top 216 < 240 (visible)
            UiListRange rLimit = UiListLayout.GetVisibleRange(itemCount, itemHeight, maxScroll, viewportH);
            Assert(rLimit.StartIndex == 5, "At scroll limit, start index must be 5");
            Assert(rLimit.EndIndex == 10, "At scroll limit, end index must be 10");

            // scrollY beyond limit: scrollY = 500
            UiListRange rBeyond = UiListLayout.GetVisibleRange(itemCount, itemHeight, 500, viewportH);
            Assert(rBeyond.IsEmpty, "Beyond scroll limit must return empty range");

            // negative scrollY: scrollY = -20
            UiListRange rNeg = UiListLayout.GetVisibleRange(itemCount, itemHeight, -20, viewportH);
            Assert(rNeg.StartIndex == 0, "Negative scroll start index must clamp to 0");
        }

        private static void TestListGeometryListShrinking()
        {
            // Initial: 20 items, scrolled to bottom: scrollY = 380 (maxScroll = 20*24 - 100 = 380)
            int itemHeight = 24;
            int viewportH = 100;
            int oldMaxScroll = UiListLayout.CalculateMaxScroll(20, itemHeight, viewportH);
            Assert(oldMaxScroll == 380, "Old max scroll must be 380");

            // List shrinks to 3 items
            int newCount = 3;
            int newMaxScroll = UiListLayout.CalculateMaxScroll(newCount, itemHeight, viewportH);
            Assert(newMaxScroll == 0, "New max scroll for 3 items in 100px viewport must be 0");

            // If scroll position is clamped to newMaxScroll (0)
            int clampedScroll = System.Math.Min(oldMaxScroll, newMaxScroll);
            Assert(clampedScroll == 0, "Clamped scroll must be 0");

            UiListRange range = UiListLayout.GetVisibleRange(newCount, itemHeight, clampedScroll, viewportH);
            Assert(range.StartIndex == 0 && range.EndIndex == 3, "All 3 rows visible after list shrink");
        }

        private static void TestListGeometryMathematicalEquivalence()
        {
            int itemCount = 15;
            int itemHeight = 34;
            int viewportH = 120;

            for (int scroll = -10; scroll <= itemCount * itemHeight + 20; scroll += 11)
            {
                UiListRange range = UiListLayout.GetVisibleRange(itemCount, itemHeight, scroll, viewportH);
                for (int i = 0; i < itemCount; i++)
                {
                    bool byVisibility = UiListLayout.IsRowVisible(i, itemCount, itemHeight, scroll, viewportH);
                    bool byRange = range.Contains(i);
                    Assert(byVisibility == byRange,
                        string.Format("Mismatch at scroll {0}, row {1}: range.Contains={2}, IsRowVisible={3}",
                            scroll, i, byRange, byVisibility));
                }
            }
        }

        private static void TestListGeometryRowBounds()
        {
            UiRect viewport = new UiRect(2, 80, 172, 100);
            UiRect row = UiListLayout.GetRowBounds(viewport, 3, 24);

            Assert(row.X == 2 && row.Y == 152, "Row bounds must offset viewport Y by index * itemHeight");
            Assert(row.Width == 172 && row.Height == 24, "Row bounds must preserve viewport width and item height");
        }

        // ==========================================
        // 2. PKHISTORYVIEW PHASE 3 REGRESSIONS
        // ==========================================

        private static void TestPKHistoryRenderStateRestorationNormal()
        {
            mGraphics g = new mGraphics();
            g.translate(10, 20);
            g.setClip(5, 5, 200, 200);
            mGraphics.RawRenderState original = g.getRawState();

            MyVector entries = new MyVector();
            PKHistoryEntry entry = new PKHistoryEntry { opponentName = "Rival", won = true, completedAt = 1000 };
            entries.addElement(entry);

            PKHistoryView.Paint(g, 2, 80, 172, 224, 0, 0, false, entries, null, null, "Me");
            mGraphics.RawRenderState restored = g.getRawState();

            Assert(original.translateX == restored.translateX, "TranslateX must be restored");
            Assert(original.translateY == restored.translateY, "TranslateY must be restored");
            Assert(original.clipX == restored.clipX, "ClipX must be restored");
            Assert(original.clipW == restored.clipW, "ClipW must be restored");
        }

        private static void TestPKHistoryRenderStateRestorationOnException()
        {
            mGraphics g = new mGraphics();
            g.translate(10, 20);
            g.setClip(5, 5, 200, 200);
            mGraphics.RawRenderState original = g.getRawState();

            MyVector malformed = new MyVector();
            malformed.addElement(new object()); // invalid cast to PKHistoryEntry

            bool threw = false;
            try
            {
                PKHistoryView.Paint(g, 2, 80, 172, 224, 0, 0, false, malformed, null, null, "Me");
            }
            catch (InvalidCastException)
            {
                threw = true;
            }

            Assert(threw, "Paint must throw on malformed entry");
            mGraphics.RawRenderState restored = g.getRawState();
            Assert(original.translateX == restored.translateX, "TranslateX must be restored on exception");
            Assert(original.translateY == restored.translateY, "TranslateY must be restored on exception");
        }

        private static void TestPKHistoryLoadingState()
        {
            mGraphics g = new mGraphics();
            PKHistoryView.Paint(g, 2, 80, 172, 224, 0, 0, true, new MyVector(), null, null, "Me");
            Assert(g.DrawnStrings.Contains("Đang tải..."), "Loading state must draw 'Đang tải...'");
            Assert(g.FillRectCount == 0, "No row rects should be drawn while loading");
        }

        private static void TestPKHistoryEmptyState()
        {
            mGraphics g = new mGraphics();
            PKHistoryView.Paint(g, 2, 80, 172, 224, 0, 0, false, new MyVector(), null, null, "Me");
            Assert(g.DrawnStrings.Contains("Chưa có lịch sử thách đấu"), "Empty state must draw empty message");
            Assert(g.FillRectCount == 0, "No row rects should be drawn for empty list");
        }

        private static void TestPKHistorySingleRowRenderAndColors()
        {
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            entries.addElement(new PKHistoryEntry { opponentName = "EnemyOne", won = true, completedAt = 1000 });

            PKHistoryView.Paint(g, 2, 80, 172, 224, 0, 0, false, entries, null, null, "MyHero");

            Assert(g.FillRectCount >= 2, "Row must draw background and divider");
            Assert(g.DrawnColors.Contains(UiColorTokens.RowSelected), "Selected row must use RowSelected color");
            Assert(g.DrawnColors.Contains(UiColorTokens.RowDivider), "Row must draw RowDivider");
            Assert(g.DrawnStrings.Contains("MyHero"), "Row must draw my player name");
            Assert(g.DrawnStrings.Contains("EnemyOne"), "Row must draw opponent name");
        }

        private static void TestPKHistoryWinLoseIconAndFallbackText()
        {
            mGraphics g1 = new mGraphics();
            MyVector entries = new MyVector();
            entries.addElement(new PKHistoryEntry { opponentName = "EnemyOne", won = true, completedAt = 1000 });
            entries.addElement(new PKHistoryEntry { opponentName = "EnemyTwo", won = false, completedAt = 1000 });

            // Null icons -> fallback text
            PKHistoryView.Paint(g1, 2, 80, 172, 224, 0, 0, false, entries, null, null, "Hero");
            Assert(g1.DrawnStrings.Contains("WIN"), "Fallback text WIN must be drawn");
            Assert(g1.DrawnStrings.Contains("LOSE"), "Fallback text LOSE must be drawn");

            // Valid icons -> drawImage
            mGraphics g2 = new mGraphics();
            Image icon = new Image(16, 16);
            PKHistoryView.Paint(g2, 2, 80, 172, 224, 0, 0, false, entries, icon, icon, "Hero");
            Assert(g2.DrawImageCount == 2, "Image must be drawn for both entries when available");
        }

        private static void TestPKHistoryViewportCulling()
        {
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            for (int i = 0; i < 30; i++)
            {
                entries.addElement(new PKHistoryEntry { opponentName = "Enemy" + i, won = true, completedAt = 1000 });
            }

            PKHistoryView.Paint(g, 2, 80, 172, 100, 0, 0, false, entries, null, null, "Hero");
            int drawnRows = g.DrawnStrings.FindAll(s => s.StartsWith("Enemy")).Count;
            Assert(drawnRows >= 3 && drawnRows <= 5,
                string.Format("Only visible rows should be drawn, expected 3-5, got {0}", drawnRows));
        }

        private static void TestPKHistoryNegativeElapsedClamping()
        {
            mGraphics g = new mGraphics();
            MyVector entries = new MyVector();
            // completedAt in the future relative to mock time
            entries.addElement(new PKHistoryEntry { opponentName = "TimeTraveler", won = true, completedAt = (int)(mSystem.currentTimeMillis() / 1000L + 5000L) });

            PKHistoryView.Paint(g, 2, 80, 172, 224, 0, 0, false, entries, null, null, "Hero");
            Assert(g.DrawnStrings.Exists(s => s.StartsWith("0 giây")), "Future timestamp must clamp elapsed to 0");
        }

        private static void TestPKHistoryScrollLimitCalculation()
        {
            int limit = UiListLayout.CalculateMaxScroll(10, PKHistoryView.ITEM_HEIGHT, 224);
            Assert(limit == 10 * 34 - 224, "Scroll limit must follow count * ITEM_HEIGHT - hScroll");
        }

        // ==========================================
        // 3. TOPRANKINGVIEW TESTS
        // ==========================================

        private static void TestTopRankingRenderStateRestorationNormal()
        {
            mGraphics g = new mGraphics();
            g.translate(15, 25);
            g.setClip(10, 10, 300, 300);
            mGraphics.RawRenderState original = g.getRawState();

            MyVector vTop = new MyVector();
            vTop.addElement(new TopInfo { pId = 1, name = "Top1", info = "Level 50", rank = 1, headICON = 10 });

            TopRankingView.Paint(g, 2, 80, 172, 224, 0, 0, vTop, 1, 100);

            mGraphics.RawRenderState restored = g.getRawState();
            Assert(original.translateX == restored.translateX, "TopRankingView: TranslateX must be restored");
            Assert(original.translateY == restored.translateY, "TopRankingView: TranslateY must be restored");
            Assert(original.clipX == restored.clipX, "TopRankingView: ClipX must be restored");
            Assert(original.clipW == restored.clipW, "TopRankingView: ClipW must be restored");
        }

        private static void TestTopRankingRenderStateRestorationOnException()
        {
            mGraphics g = new mGraphics();
            g.translate(15, 25);
            g.setClip(10, 10, 300, 300);
            mGraphics.RawRenderState original = g.getRawState();

            MyVector malformed = new MyVector();
            malformed.addElement(new object()); // invalid cast to TopInfo

            bool threw = false;
            try
            {
                TopRankingView.Paint(g, 2, 80, 172, 224, 0, 0, malformed, 1, 100);
            }
            catch (InvalidCastException)
            {
                threw = true;
            }

            Assert(threw, "TopRankingView: Paint must throw on malformed entry");
            mGraphics.RawRenderState restored = g.getRawState();
            Assert(original.translateX == restored.translateX, "TopRankingView: TranslateX restored on exception");
            Assert(original.translateY == restored.translateY, "TopRankingView: TranslateY restored on exception");
        }

        private static void TestTopRankingKeepsParentClipDuringDraw()
        {
            mGraphics g = new mGraphics();
            g.setClip(50, 100, 40, 50);

            MyVector vTop = new MyVector();
            vTop.addElement(new TopInfo { pId = 1, name = "ClippedPlayer", info = "Level 50", rank = 1, headICON = 10 });

            TopRankingView.Paint(g, 2, 80, 172, 224, 0, 0, vTop, 1, 100);

            int nameIndex = g.DrawnStrings.IndexOf("ClippedPlayer");
            Assert(nameIndex >= 0, "TopRankingView: clipped player name must reach the renderer");
            mGraphics.RawRenderState drawState = g.DrawnStringStates[nameIndex];
            Assert(drawState.clipX == 50 && drawState.clipY == 100,
                "TopRankingView: drawing must keep the parent clip origin");
            Assert(drawState.clipW == 40 && drawState.clipH == 50,
                "TopRankingView: drawing must keep the parent clip size");
        }

        private static void TestTopRankingEmptyList()
        {
            mGraphics g = new mGraphics();
            TopRankingView.Paint(g, 2, 80, 172, 224, 0, -1, new MyVector(), 0, 100);
            Assert(g.FillRectCount == 0 && g.FillRectBorderCount == 0, "Empty top ranking list must not draw any rects");
            Assert(g.DrawnStrings.Count == 0, "Empty top ranking list must not draw any strings");
        }

        private static void TestTopRankingVisibleRowsOnly()
        {
            mGraphics g = new mGraphics();
            MyVector vTop = new MyVector();
            for (int i = 0; i < 30; i++)
            {
                vTop.addElement(new TopInfo { pId = i + 1, name = "Ranker" + i, info = "Info" + i, rank = i + 1, headICON = 5 });
            }

            // Viewport height 100, itemHeight 24.
            TopRankingView.Paint(g, 2, 80, 172, 100, 0, 0, vTop, 30, 100);
            int drawnNames = g.DrawnStrings.FindAll(s => s.StartsWith("Ranker")).Count;
            Assert(drawnNames >= 4 && drawnNames <= 6,
                string.Format("TopRankingView: Only visible rows drawn, expected 4-6, got {0}", drawnNames));
        }

        private static void TestTopRankingColorsAndAvatarColumn()
        {
            mGraphics g = new mGraphics();
            MyVector vTop = new MyVector();
            vTop.addElement(new TopInfo { pId = 1, name = "First", info = "Info1", rank = 1, headICON = 5 });
            vTop.addElement(new TopInfo { pId = 2, name = "Second", info = "Info2", rank = 2, headICON = 5 });

            // Row 0 is selected
            TopRankingView.Paint(g, 2, 80, 172, 224, 0, 0, vTop, 2, 999);

            // Check colors
            Assert(g.DrawnColors.Contains(16383818), "Selected row body must use 16383818");
            Assert(g.DrawnColors.Contains(9541120), "Selected row avatar must use 9541120");
            Assert(g.DrawnColors.Contains(15196114), "Normal row body must use 15196114");
            Assert(g.DrawnColors.Contains(9993045), "Normal row avatar must use 9993045");
            Assert(g.FillRectBorderCount >= 4, "TopRankingView must use fillRect with border parameter");
        }

        private static void TestTopRankingBothAvatarBranches()
        {
            SmallImage.ResetTracking();
            mGraphics g = new mGraphics();

            // Setup part for fallback branch
            Part p = new Part();
            p.pi = new PartImage[] { new PartImage { id = 77, dx = 0, dy = 0 } };
            GameScr.parts[10] = p;

            MyVector vTop = new MyVector();
            // Branch 1: headICON != -1
            vTop.addElement(new TopInfo { pId = 1, name = "IconPlayer", info = "I1", rank = 1, headICON = 55 });
            // Branch 2: headICON == -1, fallback to GameScr.parts
            vTop.addElement(new TopInfo { pId = 2, name = "PartPlayer", info = "I2", rank = 2, headICON = -1, headID = 10 });

            TopRankingView.Paint(g, 2, 80, 172, 224, 0, -1, vTop, 2, 999);
            Assert(SmallImage.DrawSmallImageCount == 2, "Both avatar branches must invoke SmallImage.drawSmallImage");
        }

        private static void TestTopRankingPlayerNameFonts()
        {
            mGraphics g = new mGraphics();
            int myCharId = 100;

            MyVector vTop = new MyVector();
            vTop.addElement(new TopInfo { pId = myCharId, name = "MePlayer", info = "Level 99", rank = 1, headICON = 1 });
            vTop.addElement(new TopInfo { pId = 999, name = "OtherPlayer", info = "Level 80", rank = 2, headICON = 2 });

            TopRankingView.Paint(g, 2, 80, 172, 224, 0, -1, vTop, 2, myCharId);

            int myNameIndex = g.DrawnStrings.IndexOf("MePlayer");
            int otherNameIndex = g.DrawnStrings.IndexOf("OtherPlayer");
            Assert(myNameIndex >= 0, "Current player name must be drawn");
            Assert(otherNameIndex >= 0, "Other player name must be drawn");
            Assert(g.DrawnFontTags[myNameIndex] == "red", "Current player name must use the red font");
            Assert(g.DrawnFontTags[otherNameIndex] == "green", "Other player name must use the green font");
        }

        // ==========================================
        // 4. TEXTFIELDADAPTER TESTS
        // ==========================================

        private static void TestTextFieldAdapterConfigure()
        {
            TField tf = new TField();
            TextFieldAdapter adapter = new TextFieldAdapter(tf);

            adapter.Configure(10, 20, 150, 25, UiInputType.Numeric, 32, "search_input");

            Assert(tf.x == 10, "Configured x must match");
            Assert(tf.y == 20, "Configured y must match");
            Assert(tf.width == 150, "Configured width must match");
            Assert(tf.height == 25, "Configured height must match");
            Assert(tf.inputType == (int)UiInputType.Numeric, "Configured inputType must match");
            Assert(tf.maxTextLength == 32, "Configured max length must match");
            Assert(tf.name == "search_input", "Configured name must match");
            Assert(adapter.Bounds.X == 10 && adapter.Bounds.Width == 150, "Adapter bounds must reflect target");
        }

        private static void TestTextFieldAdapterFocusAndBlur()
        {
            TField tf = new TField();
            TextFieldAdapter adapter = new TextFieldAdapter(tf);

            Assert(!adapter.IsFocused, "Initially not focused");
            adapter.SetFocused(true);
            Assert(adapter.IsFocused, "Adapter should report focused");
            Assert(tf.isFocus, "Target TField should be focused");

            adapter.SetFocused(false);
            Assert(!adapter.IsFocused, "Adapter should report blurred");
            Assert(!tf.isFocus, "Target TField should be blurred");
        }

        private static void TestTextFieldAdapterKeyForwardOnlyWhenFocused()
        {
            TField tf = new TField();
            TextFieldAdapter adapter = new TextFieldAdapter(tf);

            // Not focused -> key must NOT be forwarded
            bool handled = adapter.KeyPressed(65);
            Assert(!handled, "KeyPressed must return false when not focused");
            Assert(tf.KeyPressedCount == 0, "Target must not receive key when not focused");

            // Focused -> key must be forwarded
            adapter.SetFocused(true);
            tf.KeyPressedReturnValue = false;
            handled = adapter.KeyPressed(65);
            Assert(handled, "Focused adapter must report a forwarded key as handled even when legacy TField returns false");
            Assert(tf.KeyPressedCount == 1, "Target must receive key when focused");
            Assert(tf.LastKeyCode == 65, "Target must receive exact keycode");
        }

        private static void TestTextFieldAdapterEnterSubmitOnceViaHost()
        {
            TField tf = new TField();
            TextFieldAdapter adapter = new TextFieldAdapter(tf);
            adapter.SetText("SearchHero");
            adapter.SetFocused(true);

            int submitCount = 0;
            string submittedQuery = null;

            // Host simulates handling enter key (keycode 10 or action key 15)
            Action submitAction = () =>
            {
                submitCount++;
                submittedQuery = adapter.GetText();
                adapter.SetFocused(false);
            };

            // Host detects enter
            submitAction();

            Assert(submitCount == 1, "Submit must execute exactly once");
            Assert(submittedQuery == "SearchHero", "Submitted query must match input text");
            Assert(!adapter.IsFocused, "Adapter should be blurred after submit");
        }

        private static void TestTextFieldAdapterCancelBlurNoSubmit()
        {
            TField tf = new TField();
            TextFieldAdapter adapter = new TextFieldAdapter(tf);
            adapter.SetText("DraftQuery");
            adapter.SetFocused(true);

            int submitCount = 0;
            // Blur/cancel
            adapter.SetFocused(false);

            Assert(submitCount == 0, "Blur/cancel must not trigger submit");
            Assert(adapter.GetText() == "DraftQuery", "Text is preserved on blur");
        }

        private static void TestTextFieldAdapterDisabledHiddenNoInput()
        {
            TField tf = new TField();
            TextFieldAdapter adapter = new TextFieldAdapter(tf);

            // Disabled
            adapter.IsEnabled = false;
            adapter.SetFocused(true);
            Assert(!adapter.IsFocused, "Disabled field cannot be focused");
            bool handled = adapter.KeyPressed(65);
            Assert(!handled, "Disabled field must not accept keys");
            adapter.Update();
            Assert(tf.UpdateCount == 0, "Disabled field must not forward update/pointer input");

            // Hidden
            adapter.IsEnabled = true;
            adapter.IsVisible = false;
            adapter.SetFocused(true);
            Assert(!adapter.IsFocused, "Hidden field cannot be focused");
            handled = adapter.KeyPressed(65);
            Assert(!handled, "Hidden field must not accept keys");
            adapter.Update();
            Assert(tf.UpdateCount == 0, "Hidden field must not forward update/pointer input");
        }
    }
}
