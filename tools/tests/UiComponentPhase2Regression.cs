using System;
using System.Reflection;
using Game1;
using Game1.UI;
using Game1.UI.Adapters;
using Game1.UI.Components;
using Game1.UI.Sandbox;
using Nro.UI;

internal static class UiComponentPhase2Regression
{
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    private sealed class TestActionListener : IActionListener
    {
        public int CallCount;
        public int LastIdAction;
        public object LastPayload;

        public void perform(int idAction, object p)
        {
            CallCount++;
            LastIdAction = idAction;
            LastPayload = p;
        }
    }

    public static int Main()
    {
        try
        {
            VerifyCommandFactoryRoutesAndVisualBounds();
            VerifyUiButtonUsesTheVisualBounds();
            VerifyScrollViewAdapterResetCreatesCleanState();
            VerifyUiRenderStateRestoresNestedState();
            VerifySandboxCanBeOpenedTwiceAndStillClose();
            Console.WriteLine("UI_COMPONENT_PHASE2_PRODUCTION_Game1_OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void VerifyCommandFactoryRoutesAndVisualBounds()
    {
        int actionCount = 0;
        Command standard = CommandFactory.CreateStandard(
            "Standard",
            () => actionCount++,
            new UiRect(10, 20, 80, 40));

        Require(standard.idAction == 0, "Action route must keep idAction at zero.");
        Require(standard.actionListener == null, "Action route must not retain a listener.");
        Require(standard.w == 76 && standard.h == 26,
            "Standard preset bounds must match the legacy 76x26 visual/hit area.");
        standard.performAction();
        Require(actionCount == 1, "Action route must execute exactly once.");

        var listener = new TestActionListener();
        Command listenerCommand = CommandFactory.CreateStandard(
            "Listener",
            listener,
            42,
            "payload",
            new UiRect(2, 3, 76, 26));
        listenerCommand.performAction();
        Require(listener.CallCount == 1, "Listener route must execute exactly once.");
        Require(listener.LastIdAction == 42, "Listener route must preserve idAction.");
        Require((string)listener.LastPayload == "payload", "Listener route must preserve payload.");

        bool rejectedZeroId = false;
        try
        {
            CommandFactory.CreateStandard("Invalid", listener, 0, null, new UiRect(0, 0, 76, 26));
        }
        catch (ArgumentException)
        {
            rejectedZeroId = true;
        }
        Require(rejectedZeroId, "Listener route must reject idAction <= 0.");

        Command wide = CommandFactory.CreateWide("Wide", () => { }, new UiRect(0, 0, 200, 40));
        Require(wide.w == 160 && wide.h == 26 && wide.hw == 80,
            "Wide preset bounds must match the legacy 160x26 visual/hit area.");

        Command delete = CommandFactory.CreateDelete("Delete", () => { }, new UiRect(0, 0, 90, 40));
        Require(delete.w == 90 && delete.h == 26 && delete.hw == 45,
            "Delete preset must center its caption when a custom supported width is used.");

        var image = new Image(18, 12);
        Command imageOnly = CommandFactory.CreateImageOnly(image, () => { }, new UiRect(0, 0, 60, 60));
        Require(imageOnly.w == 18 && imageOnly.h == 12,
            "Image-only bounds must match the visual image dimensions.");
    }

    private static void VerifyUiButtonUsesTheVisualBounds()
    {
        ResetPointer();
        int callbackCount = 0;
        Command command = CommandFactory.CreateStandard(
            "Button",
            () => callbackCount++,
            new UiRect(20, 30, 80, 40));
        var button = new UiButton(command, new UiRect(20, 30, 80, 40));
        var input = new UiInputContext();

        Require(button.Bounds == new UiRect(20, 30, 76, 26),
            "UiButton bounds must be normalized to the Command visual bounds.");

        GameCanvas.px = 30;
        GameCanvas.py = 40;
        GameCanvas.isPointerDown = true;
        GameCanvas.isPointerJustDown = true;
        Require(button.UpdateInput(input), "Pointer down inside the visual bounds must be consumed.");
        Require(button.Pressed, "Button must enter pressed state.");

        GameCanvas.isPointerDown = false;
        GameCanvas.isPointerJustRelease = true;
        GameCanvas.isPointerClick = true;
        Require(button.UpdateInput(input), "Pointer release inside must be consumed.");
        Require(callbackCount == 1, "Valid click must execute callback exactly once.");

        ResetPointer();
        GameCanvas.px = button.Bounds.Right;
        GameCanvas.py = 40;
        GameCanvas.isPointerDown = true;
        GameCanvas.isPointerJustDown = true;
        Require(!button.UpdateInput(input), "The first pixel outside the visual right edge must not click.");

        ResetPointer();
        button.Enabled = false;
        GameCanvas.px = 30;
        GameCanvas.py = 40;
        GameCanvas.isPointerDown = true;
        GameCanvas.isPointerJustDown = true;
        Require(!button.UpdateInput(input), "Disabled button must not consume pointer input.");
        Require(callbackCount == 1, "Disabled button must not execute its callback.");

        button.Enabled = true;
        ResetPointer();
        GameCanvas.px = 30;
        GameCanvas.py = 40;
        GameCanvas.isPointerDown = true;
        GameCanvas.isPointerJustDown = true;
        button.UpdateInput(input);
        GameCanvas.px = 45;
        GameCanvas.isPointerJustDown = false;
        Require(!button.UpdateInput(input), "Drag beyond threshold must cancel the button press.");
        Require(!button.Pressed && callbackCount == 1, "Cancelled drag must not execute callback.");
    }

    private static void VerifyScrollViewAdapterResetCreatesCleanState()
    {
        var adapter = new ScrollViewAdapter();
        adapter.Configure(new UiRect(10, 20, 100, 80), 10, 20);
        Require(adapter.ScrollLimit == 120, "Long list must calculate the legacy scroll limit.");

        FieldInfo scrollField = typeof(ScrollViewAdapter).GetField("_scroll", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(scrollField != null, "ScrollViewAdapter must retain its wrapped Scroll instance.");
        var legacyScroll = (Scroll)scrollField.GetValue(adapter);
        legacyScroll.pointerIsDowning = true;
        legacyScroll.selectedItem = 7;
        legacyScroll.cmy = 80;
        legacyScroll.cmtoY = 80;

        adapter.Reset();

        Require(!adapter.IsDragging, "Reset must clear stale drag ownership.");
        Require(adapter.SelectedIndex == -1, "Reset must clear public selection.");
        Require(adapter.ScrollY == 0 && adapter.ScrollLimit == 0, "Reset must clear scroll position and limit.");
        Require(!ReferenceEquals(legacyScroll, scrollField.GetValue(adapter)),
            "Reset must replace legacy Scroll because Scroll.clear() leaves private gesture state behind.");
    }

    private static void VerifyUiRenderStateRestoresNestedState()
    {
        var graphics = new mGraphics();
        graphics.translate(5, 7);
        graphics.setClip(10, 20, 100, 80);
        mGraphics.RawRenderState before = graphics.getRawState();

        using (UiRenderState.Push(graphics, new UiRect(50, 40, 100, 100), clip: true, translate: true))
        {
            mGraphics.RawRenderState inside = graphics.getRawState();
            Require(inside.isClip, "Nested render scope must keep clipping enabled.");
            Require(inside.clipW == 60 && inside.clipH == 60,
                "Nested clip must intersect child viewport with the parent clip.");
        }

        Require(SameState(before, graphics.getRawState()),
            "Render scope must restore all raw clip/translate fields after Dispose.");

        try
        {
            using (UiRenderState.Push(graphics, new UiRect(0, 0, 10, 10), clip: true))
            {
                throw new InvalidOperationException("Simulated paint failure");
            }
        }
        catch (InvalidOperationException)
        {
        }

        Require(SameState(before, graphics.getRawState()),
            "Render scope must restore state when nested painting throws.");
    }

    private static void VerifySandboxCanBeOpenedTwiceAndStillClose()
    {
        var previous = new mScreen();
        GameCanvas.currentScreen = previous;

        UiComponentSandboxScr.Open();
        var sandbox = GameCanvas.currentScreen as UiComponentSandboxScr;
        Require(sandbox != null, "Sandbox Open must switch to the sandbox screen.");

        UiComponentSandboxScr.Open();
        Require(ReferenceEquals(GameCanvas.currentScreen, sandbox),
            "Opening an already active sandbox must be idempotent.");

        sandbox.Close();
        Require(ReferenceEquals(GameCanvas.currentScreen, previous),
            "Sandbox must still close to the original previous screen after repeated F8.");
    }

    private static bool SameState(mGraphics.RawRenderState left, mGraphics.RawRenderState right)
    {
        return left.isClip == right.isClip
            && left.isTranslate == right.isTranslate
            && left.translateX == right.translateX
            && left.translateY == right.translateY
            && left.clipTX == right.clipTX
            && left.clipTY == right.clipTY
            && left.clipX == right.clipX
            && left.clipY == right.clipY
            && left.clipW == right.clipW
            && left.clipH == right.clipH;
    }

    private static void ResetPointer()
    {
        GameCanvas.px = 0;
        GameCanvas.py = 0;
        GameCanvas.isPointerDown = false;
        GameCanvas.isPointerJustDown = false;
        GameCanvas.isPointerJustRelease = false;
        GameCanvas.isPointerClick = false;
    }
}
