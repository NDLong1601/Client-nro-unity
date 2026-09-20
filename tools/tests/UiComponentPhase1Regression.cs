using System;
using System.Reflection;
using Nro.UI;

internal static class UiComponentPhase1Regression
{
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    public static int Main()
    {
        try
        {
            VerifyDependencyIsolation();
            VerifyUiRectGeometryAndHitTest();
            VerifyUiRectIntersection();
            VerifyUiRectInset();
            VerifyUiLayoutStack();
            VerifyUiLayoutExceptions();
            VerifyEnumsAndColorTokens();
            Console.WriteLine("UI_COMPONENT_PHASE1_FOUNDATION_OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void VerifyDependencyIsolation()
    {
        Assembly asm = typeof(UiRect).Assembly;
        foreach (AssemblyName refAsm in asm.GetReferencedAssemblies())
        {
            string name = refAsm.Name.ToLowerInvariant();
            Require(!name.Contains("unityengine"), "UIShared must not reference UnityEngine");
            Require(!name.Contains("game1"), "UIShared must not reference Game1");
            Require(!name.Contains("game2"), "UIShared must not reference Game2");
        }
    }

    private static void VerifyUiRectGeometryAndHitTest()
    {
        UiRect rect = new UiRect(10, 20, 100, 50);
        Require(rect.X == 10 && rect.Y == 20 && rect.Width == 100 && rect.Height == 50, "Properties match constructor");
        Require(rect.Left == 10 && rect.Top == 20 && rect.Right == 110 && rect.Bottom == 70, "Left/Top/Right/Bottom match logic");
        Require(!rect.IsEmpty, "Standard rect is not empty");

        // Diem o goc trai/tren (bao gom canh trai va canh tren)
        Require(rect.Contains(10, 20), "Contains top-left corner");

        // Diem ngay truoc canh phai/duoi (ben trong rect)
        Require(rect.Contains(109, 69), "Contains pixel right before bottom-right edge");

        // Diem dung canh phai/duoi (khong bao gom)
        Require(!rect.Contains(110, 20), "Does not contain right edge");
        Require(!rect.Contains(10, 70), "Does not contain bottom edge");
        Require(!rect.Contains(110, 70), "Does not contain bottom-right corner");

        // Diem hoan toan ben ngoai
        Require(!rect.Contains(9, 20), "Does not contain left outside");
        Require(!rect.Contains(10, 19), "Does not contain top outside");
        Require(!rect.Contains(150, 80), "Does not contain far outside");

        // Rect rong (width = 0 hoac height = 0)
        UiRect emptyW = new UiRect(10, 20, 0, 50);
        Require(emptyW.IsEmpty, "Width 0 is empty");
        Require(!emptyW.Contains(10, 20), "Empty width rect contains nothing");

        UiRect emptyH = new UiRect(10, 20, 100, 0);
        Require(emptyH.IsEmpty, "Height 0 is empty");
        Require(!emptyH.Contains(10, 20), "Empty height rect contains nothing");

        // Width/height am (khong tu doi sang duong, IsEmpty = true)
        UiRect negW = new UiRect(10, 20, -50, 50);
        Require(negW.IsEmpty, "Negative width is empty");
        Require(negW.Width == -50, "Negative width is not auto converted to positive");
        Require(!negW.Contains(10, 20), "Negative width contains nothing");

        UiRect negH = new UiRect(10, 20, 100, -30);
        Require(negH.IsEmpty, "Negative height is empty");
        Require(negH.Height == -30, "Negative height is not auto converted to positive");
        Require(!negH.Contains(10, 20), "Negative height contains nothing");
    }

    private static void VerifyUiRectIntersection()
    {
        UiRect r1 = new UiRect(10, 10, 40, 40); // 10..50, 10..50
        UiRect r2 = new UiRect(30, 30, 40, 40); // 30..70, 30..70

        // Hai rect chong nhau
        UiRect inter = r1.Intersect(r2);
        Require(!inter.IsEmpty, "Overlapping rects produce non-empty intersection");
        Require(inter.X == 30 && inter.Y == 30 && inter.Width == 20 && inter.Height == 20,
            "Intersection bounds match overlapping area (30,30,20,20)");

        // Cham canh nhung khong giao dien tich (r1.Right == rEdge.Left)
        UiRect rEdge = new UiRect(50, 10, 30, 40); // Left is 50, touching r1.Right
        UiRect interEdge = r1.Intersect(rEdge);
        Require(interEdge.IsEmpty, "Touching edge rects have empty intersection (0 width)");
        Require(interEdge.Width == 0 && interEdge.Height == 0, "Empty intersection has 0 dimensions, no negative");

        // Cham day (r1.Bottom == rBottom.Top)
        UiRect rBottom = new UiRect(10, 50, 40, 20); // Top is 50, touching r1.Bottom
        UiRect interBottom = r1.Intersect(rBottom);
        Require(interBottom.IsEmpty, "Touching bottom edge has empty intersection (0 height)");

        // Hoan toan khong giao nhau (roi rac)
        UiRect rDisjoint = new UiRect(100, 100, 20, 20);
        UiRect interDisjoint = r1.Intersect(rDisjoint);
        Require(interDisjoint.IsEmpty, "Disjoint rects have empty intersection");
        Require(interDisjoint.Width == 0 && interDisjoint.Height == 0, "No negative dimensions on disjoint intersection");

        // Mot rect nam hoan toan trong rect con lai
        UiRect rOuter = new UiRect(0, 0, 100, 100);
        UiRect rInner = new UiRect(20, 25, 30, 40);
        UiRect interContained = rOuter.Intersect(rInner);
        Require(interContained == rInner, "Intersection of outer and inner is inner rect");
    }

    private static void VerifyUiRectInset()
    {
        UiRect r = new UiRect(10, 10, 100, 80);
        UiRect inset1 = r.Inset(5, 10);
        Require(inset1.X == 15 && inset1.Y == 20 && inset1.Width == 90 && inset1.Height == 60, "Symmetric inset matches");

        UiRect inset2 = r.Inset(2, 4, 6, 8);
        Require(inset2.X == 12 && inset2.Y == 14 && inset2.Width == 92 && inset2.Height == 68, "Asymmetric inset matches");

        // Inset vuot qua kich thuoc khong sinh kich thuoc am
        UiRect excessive = r.Inset(60, 50);
        Require(excessive.Width == 0 && excessive.Height == 0 && excessive.IsEmpty, "Excessive inset clamps to 0");
    }

    private static void VerifyUiLayoutStack()
    {
        UiRect container = new UiRect(10, 20, 100, 80);

        // Count = 1
        UiRect singleH = UiLayout.StackHorizontal(container, 0, 1, 5);
        Require(singleH == container, "Single item horizontal stack matches container");

        UiRect singleV = UiLayout.StackVertical(container, 0, 1, 5);
        Require(singleV == container, "Single item vertical stack matches container");

        // Stack ngang co gap va chia deu: container.Width = 100, count = 4, gap = 4
        // totalItemSpace = 100 - (3 * 4) = 88. baseSize = 88 / 4 = 22, remainder = 0.
        for (int i = 0; i < 4; i++)
        {
            UiRect item = UiLayout.StackHorizontal(container, i, 4, 4);
            Require(item.Width == 22, "Evenly divisible horizontal stack item width is 22");
            Require(item.Height == container.Height, "Horizontal stack item height matches container");
            Require(item.Y == container.Y, "Horizontal stack item Y matches container");
            int expectedX = container.X + i * (22 + 4);
            Require(item.X == expectedX, "Horizontal stack item X matches gap formula");
        }

        // Stack ngang co phan du: container.Width = 100, count = 3, gap = 0
        // totalItemSpace = 100. baseSize = 33, remainder = 1.
        // Item 0: width 34, x = 10.
        // Item 1: width 33, x = 10 + 34 = 44.
        // Item 2: width 33, x = 44 + 33 = 77.
        // Total width = 34 + 33 + 33 = 100. Right edge = 77 + 33 = 110 == container.Right
        UiRect h0 = UiLayout.StackHorizontal(container, 0, 3, 0);
        UiRect h1 = UiLayout.StackHorizontal(container, 1, 3, 0);
        UiRect h2 = UiLayout.StackHorizontal(container, 2, 3, 0);
        Require(h0.Width == 34 && h0.X == 10, "Remainder item 0 gets base+1");
        Require(h1.Width == 33 && h1.X == 44, "Item 1 gets base size");
        Require(h2.Width == 33 && h2.X == 77, "Item 2 gets base size");
        Require(h2.Right == container.Right, "Last item right edge does not exceed container");
        Require(h0.Width + h1.Width + h2.Width == container.Width, "Total width equals container width");

        // Stack doc co phan du va gap: container.Height = 86, count = 4, gap = 3
        // gaps = 3 * 3 = 9. totalItemSpace = 86 - 9 = 77. baseSize = 19, remainder = 1.
        UiRect cV = new UiRect(0, 0, 50, 86);
        int totalH = 0;
        for (int i = 0; i < 4; i++)
        {
            UiRect item = UiLayout.StackVertical(cV, i, 4, 3);
            Require(item.X == cV.X && item.Width == cV.Width, "Vertical stack preserves X and Width");
            if (i < 3)
            {
                UiRect next = UiLayout.StackVertical(cV, i + 1, 4, 3);
                Require(item.Bottom + 3 == next.Top, "Distance between adjacent vertical items matches gap");
            }
            totalH += item.Height;
        }
        Require(totalH + 3 * 3 == cV.Height, "Total height with gaps equals container height");
        UiRect lastV = UiLayout.StackVertical(cV, 3, 4, 3);
        Require(lastV.Bottom == cV.Bottom, "Last item bottom edge does not exceed container");
    }

    private static void VerifyUiLayoutExceptions()
    {
        UiRect r = new UiRect(0, 0, 100, 100);

        // count <= 0
        bool caughtCount = false;
        try { UiLayout.StackHorizontal(r, 0, 0); }
        catch (ArgumentOutOfRangeException) { caughtCount = true; }
        Require(caughtCount, "StackHorizontal throws ArgumentOutOfRangeException for count 0");

        // index < 0
        bool caughtNegIndex = false;
        try { UiLayout.StackHorizontal(r, -1, 3); }
        catch (ArgumentOutOfRangeException) { caughtNegIndex = true; }
        Require(caughtNegIndex, "StackHorizontal throws ArgumentOutOfRangeException for negative index");

        // index >= count
        bool caughtOverflowIndex = false;
        try { UiLayout.StackHorizontal(r, 3, 3); }
        catch (ArgumentOutOfRangeException) { caughtOverflowIndex = true; }
        Require(caughtOverflowIndex, "StackHorizontal throws ArgumentOutOfRangeException for index >= count");

        // gap < 0
        bool caughtNegGap = false;
        try { UiLayout.StackHorizontal(r, 0, 2, -1); }
        catch (ArgumentOutOfRangeException) { caughtNegGap = true; }
        Require(caughtNegGap, "StackHorizontal throws ArgumentOutOfRangeException for negative gap");

        // Tuong tu voi StackVertical
        bool caughtV = false;
        try { UiLayout.StackVertical(r, 2, 2); }
        catch (ArgumentOutOfRangeException) { caughtV = true; }
        Require(caughtV, "StackVertical throws ArgumentOutOfRangeException for index >= count");

        // Tong gap khong duoc lon hon kich thuoc container vi se day item ra ngoai bounds.
        bool caughtHorizontalGapOverflow = false;
        try { UiLayout.StackHorizontal(new UiRect(0, 0, 10, 10), 0, 3, 6); }
        catch (ArgumentOutOfRangeException) { caughtHorizontalGapOverflow = true; }
        Require(caughtHorizontalGapOverflow,
            "StackHorizontal rejects a total gap larger than the container width");

        bool caughtVerticalGapOverflow = false;
        try { UiLayout.StackVertical(new UiRect(0, 0, 10, 10), 0, 3, 6); }
        catch (ArgumentOutOfRangeException) { caughtVerticalGapOverflow = true; }
        Require(caughtVerticalGapOverflow,
            "StackVertical rejects a total gap larger than the container height");

        // Phep tinh tong gap phai an toan voi int lon, khong duoc tran so roi chap nhan input sai.
        bool caughtIntegerOverflow = false;
        try { UiLayout.StackHorizontal(r, 0, int.MaxValue, int.MaxValue); }
        catch (ArgumentOutOfRangeException) { caughtIntegerOverflow = true; }
        Require(caughtIntegerOverflow, "StackHorizontal calculates total gap without integer overflow");
    }

    private static void VerifyEnumsAndColorTokens()
    {
        // UiInputType tuong thich chinh xac voi gia tri TField
        Require((int)UiInputType.Any == 0, "UiInputType.Any == 0");
        Require((int)UiInputType.Numeric == 1, "UiInputType.Numeric == 1");
        Require((int)UiInputType.Password == 2, "UiInputType.Password == 2");
        Require((int)UiInputType.AlphaNumeric == 3, "UiInputType.AlphaNumeric == 3");

        // TextAlign tuong thich chinh xac voi mFont
        Require((int)TextAlign.Left == 0, "TextAlign.Left == 0");
        Require((int)TextAlign.Right == 1, "TextAlign.Right == 1");
        Require((int)TextAlign.Center == 2, "TextAlign.Center == 2");

        // UiColorTokens khop voi cac token legacy da xac minh
        Require(UiColorTokens.RowNormal == 15196114, "UiColorTokens.RowNormal == 15196114");
        Require(UiColorTokens.RowSelected == 16383818, "UiColorTokens.RowSelected == 16383818");
        Require(UiColorTokens.RowDivider == 9993045, "UiColorTokens.RowDivider == 9993045");
        Require(UiColorTokens.HeaderDivider == 13524492, "UiColorTokens.HeaderDivider == 13524492");

        // UiMetrics
        Require(UiMetrics.DefaultPadding == 4, "UiMetrics.DefaultPadding == 4");
        Require(UiMetrics.DefaultGap == 2, "UiMetrics.DefaultGap == 2");
    }
}
