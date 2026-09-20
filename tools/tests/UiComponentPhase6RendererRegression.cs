using System;
using System.Collections.Generic;
using Game1;
using Game1.UI.Adapters;
using Game1.UI.Pilots;
using Nro.UI;

namespace Game1.Tests
{
    public static class UiComponentPhase6RendererRegression
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
            Console.WriteLine("--- Running UiComponentPhase6RendererRegression ---");

            Test1_EmptyListDrawsNoEnemy();
            Test2_RowNormalAndSelectedColors();
            Test3_OnlineVsOfflineFonts();
            Test4_HeadIconPath();
            Test5_FallbackPartPath();
            Test6_NullSafetyAndOutOfBounds();
            Test7_CurrentListLengthClampedToVectorSize();
            Test8_VisibleRangeCullingAtCmy0();
            Test9_ScrollOffsetSelectsCorrectRange();
            Test10_ClipAndTranslateRestoredAfterPaint();
            Test11_NestedClipNotExpanded();
            Test12_NoAssetLoadOrNetwork();

            Console.WriteLine("UI_COMPONENT_PHASE6_RENDERER_Game1_OK");
        }

        private static void Test1_EmptyListDrawsNoEnemy()
        {
            mGraphics g = new mGraphics();
            MyVector emptyList = new MyVector("Empty");
            EnemyListView.Paint(g, 10, 80, 200, 120, 0, -1, emptyList, 0);

            Assert(g.DrawnStrings.Contains(mResources.no_enemy), "Empty state must draw mResources.no_enemy");
            Assert(SmallImage.DrawSmallImageCount == 0, "Empty state must not draw any small images");
        }

        private static void Test2_RowNormalAndSelectedColors()
        {
            mGraphics g = new mGraphics();
            SetupMockParts();
            MyVector items = CreateMockEnemyList(2);

            EnemyListView.Paint(g, 10, 80, 200, 120, 0, 1, items, 2);

            Assert(g.DrawnColors.Contains(15196114), "Normal row background 15196114 missing");
            Assert(g.DrawnColors.Contains(16383818), "Selected row background 16383818 missing");
            Assert(g.DrawnColors.Contains(9993045), "Normal avatar background 9993045 missing");
            Assert(g.DrawnColors.Contains(9541120), "Selected avatar background 9541120 missing");
        }

        private static void Test3_OnlineVsOfflineFonts()
        {
            mGraphics g = new mGraphics();
            SetupMockParts();
            MyVector items = CreateMockEnemyList(2);

            EnemyListView.Paint(g, 10, 80, 200, 120, 0, 0, items, 2);

            Assert(g.DrawnFontTags.Contains("green"), "Online enemy name must use green font");
            Assert(g.DrawnFontTags.Contains("blue"), "Online enemy power info must use blue font");
            Assert(g.DrawnFontTags.Contains("grey"), "Offline enemy must use grey font");
        }

        private static void Test4_HeadIconPath()
        {
            mGraphics g = new mGraphics();
            SmallImage.ResetTracking();
            MyVector list = new MyVector();
            Char c = new Char
            {
                cName = "Hero",
                headICON = 456
            };
            InfoItem item = new InfoItem("Hero Power") { charInfo = c, isOnline = true };
            list.addElement(item);

            EnemyListView.Paint(g, 10, 50, 200, 100, 0, 0, list, 1);

            Assert(SmallImage.Calls.Count == 1, "Must call drawSmallImage once");
            var call = SmallImage.Calls[0];
            Assert(call.id == 456, "HeadICON ID must match");
            Assert(call.x == 10, "Avatar X must be xScroll");
            Assert(call.y == 50, "Avatar Y must be yScroll");
            Assert(call.transform == 0, "Transform must be 0");
            Assert(call.anchor == 0, "Anchor must be 0");
        }

        private static void Test5_FallbackPartPath()
        {
            mGraphics g = new mGraphics();
            SetupMockParts();
            SmallImage.ResetTracking();
            MyVector list = new MyVector();
            Char c = new Char
            {
                cName = "FallbackHero",
                headICON = -1,
                head = 2
            };
            InfoItem item = new InfoItem("Power") { charInfo = c, isOnline = true };
            list.addElement(item);

            EnemyListView.Paint(g, 10, 50, 200, 100, 0, 0, list, 1);

            Assert(SmallImage.Calls.Count == 1, "Must call drawSmallImage once for fallback");
            var call = SmallImage.Calls[0];
            Assert(call.id == 502, "Fallback image ID must match part image id");
            Assert(call.x == 10 + 2, "Avatar X must be avatarX + dx");
            Assert(call.y == 50 + 3 + 1, "Avatar Y must be rowY + 3 + dy");
            Assert(call.transform == 0, "Transform must be 0");
            Assert(call.anchor == 0, "Anchor must be 0");
        }

        private static void Test6_NullSafetyAndOutOfBounds()
        {
            mGraphics g = new mGraphics();

            EnemyListView.Paint(null, 0, 0, 100, 100, 0, 0, null, 0);

            EnemyListView.Paint(g, 0, 0, 100, 100, 0, 0, null, 5);
            Assert(g.DrawnStrings.Contains(mResources.no_enemy), "Null vEnemy must render empty state");

            g.ResetTracking();
            MyVector corruptList = new MyVector();
            corruptList.addElement(null);
            corruptList.addElement(new InfoItem("NoChar") { charInfo = null });
            Char badHeadChar = new Char { head = 999, headICON = -1, cName = "BadHead" };
            corruptList.addElement(new InfoItem("BadHead") { charInfo = badHeadChar });

            EnemyListView.Paint(g, 0, 0, 100, 100, 0, 0, corruptList, 3);
        }

        private static void Test7_CurrentListLengthClampedToVectorSize()
        {
            mGraphics g = new mGraphics();
            MyVector list = CreateMockEnemyList(2);

            EnemyListView.Paint(g, 0, 0, 200, 200, 0, 0, list, 50);
        }

        private static void Test8_VisibleRangeCullingAtCmy0()
        {
            mGraphics g = new mGraphics();
            SetupMockParts();
            SmallImage.ResetTracking();
            MyVector list = CreateMockEnemyList(10);

            EnemyListView.Paint(g, 0, 0, 200, 72, 0, 0, list, 10);

            Assert(SmallImage.DrawSmallImageCount == 3, "At cmy=0, exactly 3 rows must be drawn");
            Assert(SmallImage.DrawnImageIds.Contains(100), "Row 0 avatar drawn");
            Assert(SmallImage.DrawnImageIds.Contains(501), "Row 1 avatar drawn");
            Assert(SmallImage.DrawnImageIds.Contains(102), "Row 2 avatar drawn");
            Assert(!SmallImage.DrawnImageIds.Contains(104), "Off-screen row 4 avatar must not be drawn");
        }

        private static void Test9_ScrollOffsetSelectsCorrectRange()
        {
            mGraphics g = new mGraphics();
            SetupMockParts();
            SmallImage.ResetTracking();
            MyVector list = CreateMockEnemyList(10);

            EnemyListView.Paint(g, 0, 0, 200, 72, 48, 0, list, 10);

            Assert(SmallImage.DrawSmallImageCount == 3, "Exactly 3 rows must be drawn");
            Assert(!SmallImage.DrawnImageIds.Contains(100), "Row 0 (scrolled off) must not be drawn");
            Assert(!SmallImage.DrawnImageIds.Contains(501), "Row 1 (scrolled off) must not be drawn");
            Assert(SmallImage.DrawnImageIds.Contains(102), "Row 2 must be drawn");
            Assert(SmallImage.DrawnImageIds.Contains(503), "Row 3 must be drawn");
            Assert(SmallImage.DrawnImageIds.Contains(104), "Row 4 must be drawn");
        }

        private static void Test10_ClipAndTranslateRestoredAfterPaint()
        {
            mGraphics g = new mGraphics();
            MyVector list = CreateMockEnemyList(5);

            mGraphics.RawRenderState before = g.getRawState();
            EnemyListView.Paint(g, 10, 20, 150, 100, 30, 0, list, 5);
            mGraphics.RawRenderState after = g.getRawState();

            Assert(before.translateX == after.translateX, "TranslateX must be restored");
            Assert(before.translateY == after.translateY, "TranslateY must be restored");
            Assert(before.clipX == after.clipX, "ClipX must be restored");
            Assert(before.clipY == after.clipY, "ClipY must be restored");
            Assert(before.clipW == after.clipW, "ClipW must be restored");
            Assert(before.clipH == after.clipH, "ClipH must be restored");
        }

        private static void Test11_NestedClipNotExpanded()
        {
            mGraphics g = new mGraphics();
            MyVector list = CreateMockEnemyList(5);

            g.setClip(25, 25, 60, 60);
            mGraphics.RawRenderState parentState = g.getRawState();

            EnemyListView.Paint(g, 0, 0, 200, 200, 0, 0, list, 5);

            mGraphics.RawRenderState restoredState = g.getRawState();
            Assert(restoredState.clipX == parentState.clipX, "Parent ClipX must not expand");
            Assert(restoredState.clipY == parentState.clipY, "Parent ClipY must not expand");
            Assert(restoredState.clipW == parentState.clipW, "Parent ClipW must not expand");
            Assert(restoredState.clipH == parentState.clipH, "Parent ClipH must not expand");
        }

        private static void Test12_NoAssetLoadOrNetwork()
        {
            mGraphics g = new mGraphics();
            Service.ResetTracking();
            MyVector list = CreateMockEnemyList(5);

            EnemyListView.Paint(g, 0, 0, 200, 200, 0, 0, list, 5);

            Assert(Service.SentEnemyCalls.Count == 0, "Paint must not invoke Service network calls");
        }
    }
}
