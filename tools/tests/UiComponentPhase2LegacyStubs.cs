using System;

namespace Game1
{
    // Intentionally mirrors the legacy name collision in the real client.
    // Phase 2 production code must qualify System.Math instead of binding here.
    public class Math
    {
        public static int abs(int value) { return value > 0 ? value : -value; }
        public static int min(int left, int right) { return left < right ? left : right; }
        public static int pow(int value, int exponent)
        {
            int result = 1;
            for (int i = 0; i < exponent; i++) result *= value;
            return result;
        }
    }

    public class Image
    {
        public readonly int width;
        public readonly int height;

        public Image(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }

    public interface IActionListener
    {
        void perform(int idAction, object p);
    }

    public class Command
    {
        public string caption;
        public IActionListener actionListener;
        public int idAction;
        public object p;
        public Action action;
        public int x;
        public int y;
        public int w = 68;
        public int h = 26;
        public int hw;
        public int type;
        public bool isFocus;
        public Image img;

        public Command(string caption, Action action)
        {
            this.caption = caption;
            this.action = action;
        }

        public Command(string caption, IActionListener listener, int idAction, object p, int x, int y)
        {
            this.caption = caption;
            actionListener = listener;
            this.idAction = idAction;
            this.p = p;
            this.x = x;
            this.y = y;
        }

        public void setType()
        {
            type = 1;
            w = 160;
            hw = 80;
        }

        public void setTypeDelete()
        {
            type = 2;
            w = 50;
            hw = 28;
        }

        public void performAction()
        {
            GameCanvas.clearAllPointerEvent();
            if (idAction > 0 && actionListener != null)
            {
                actionListener.perform(idAction, p);
            }
            if (action != null)
            {
                action.Invoke();
            }
        }

        public void paint(mGraphics graphics)
        {
            if (img != null)
            {
                graphics.LastCommandWidth = img.width;
                graphics.LastCommandHeight = img.height;
                return;
            }

            graphics.LastCommandWidth = type == 1 ? 160 : (type == 2 ? w : 76);
            graphics.LastCommandHeight = 24;
        }
    }

    public static class GameCanvas
    {
        public static int px;
        public static int py;
        public static int w = 320;
        public static int h = 240;
        public static int hw = 160;
        public static bool isPointerDown;
        public static bool isPointerJustDown;
        public static bool isPointerJustRelease;
        public static bool isPointerClick;
        public static mScreen currentScreen;
        public static mScreen serverScreen;

        public static void clearAllPointerEvent()
        {
            isPointerDown = false;
            isPointerJustDown = false;
            isPointerJustRelease = false;
            isPointerClick = false;
        }
    }

    public class ScrollResult
    {
        public bool isDowning;
        public int selected = -1;
        public bool isFinish;
    }

    public class Scroll
    {
        public int cmtoX;
        public int cmtoY;
        public int cmx;
        public int cmy;
        public int cmxLim;
        public int cmyLim;
        public int xPos;
        public int yPos;
        public int width;
        public int height;
        public int ITEM_SIZE;
        public int nITEM;
        public int ITEM_PER_LINE;
        public bool styleUPDOWN = true;
        public bool pointerIsDowning;
        public int selectedItem;

        // Matches the real legacy clear(): gesture ownership is intentionally not reset.
        public void clear()
        {
            cmtoX = 0;
            cmtoY = 0;
            cmx = 0;
            cmy = 0;
            cmxLim = 0;
            cmyLim = 0;
            width = 0;
            height = 0;
        }

        public void setStyle(int itemCount, int itemSize, int x, int y, int viewWidth, int viewHeight, bool vertical, int itemsPerLine)
        {
            xPos = x;
            yPos = y;
            ITEM_SIZE = itemSize;
            nITEM = itemCount;
            width = viewWidth;
            height = viewHeight;
            styleUPDOWN = vertical;
            ITEM_PER_LINE = itemsPerLine;
            int lines = itemCount / itemsPerLine;
            if (itemCount % itemsPerLine != 0) lines++;
            cmyLim = vertical ? lines * itemSize - viewHeight : 0;
            if (cmyLim < 0) cmyLim = 0;
        }

        public void updatecm()
        {
            if (cmtoY < 0) cmtoY = 0;
            if (cmtoY > cmyLim) cmtoY = cmyLim;
            cmy = cmtoY;
        }

        public ScrollResult updateKey()
        {
            if (GameCanvas.isPointerDown
                && !pointerIsDowning
                && GameCanvas.px >= xPos && GameCanvas.px < xPos + width
                && GameCanvas.py >= yPos && GameCanvas.py < yPos + height)
            {
                pointerIsDowning = true;
                selectedItem = (cmy + GameCanvas.py - yPos) / ITEM_SIZE;
            }

            bool finished = false;
            if (GameCanvas.isPointerJustRelease && pointerIsDowning)
            {
                pointerIsDowning = false;
                GameCanvas.isPointerJustRelease = false;
                finished = true;
            }

            return new ScrollResult
            {
                selected = selectedItem,
                isFinish = finished,
                isDowning = pointerIsDowning
            };
        }
    }

    public class mGraphics
    {
        public static int zoomLevel = 1;
        public static int addYWhenOpenKeyBoard;
        public int LastCommandWidth;
        public int LastCommandHeight;

        public struct RawRenderState
        {
            public bool isClip;
            public bool isTranslate;
            public int translateX;
            public int translateY;
            public int clipTX;
            public int clipTY;
            public int clipX;
            public int clipY;
            public int clipW;
            public int clipH;
        }

        private RawRenderState state;

        public static int getImageWidth(Image image) { return image == null ? 0 : image.width; }
        public static int getImageHeight(Image image) { return image == null ? 0 : image.height; }

        public void translate(int x, int y)
        {
            state.translateX += x * zoomLevel;
            state.translateY += y * zoomLevel;
            state.isTranslate = state.translateX != 0 || state.translateY != 0;
        }

        public void setClip(int x, int y, int width, int height)
        {
            state.clipTX = state.translateX;
            state.clipTY = state.translateY;
            state.clipX = x * zoomLevel;
            state.clipY = y * zoomLevel;
            state.clipW = width * zoomLevel;
            state.clipH = height * zoomLevel;
            state.isClip = true;
        }

        public RawRenderState getRawState() { return state; }
        public void restoreRawState(RawRenderState value) { state = value; }
        public void setColor(int color) { }
        public void drawRect(int x, int y, int width, int height) { }
        public void fillRect(int x, int y, int width, int height) { }
    }

    public class mScreen
    {
        public Command center;

        public virtual void switchToMe()
        {
            if (GameCanvas.currentScreen != null)
            {
                GameCanvas.currentScreen.unLoad();
            }
            GameCanvas.currentScreen = this;
        }

        public virtual void unLoad() { }
        public virtual void update() { }
        public virtual void updateKey() { }
        public virtual void paint(mGraphics graphics) { }
    }

    public class mFont
    {
        public const int LEFT = 0;
        public const int CENTER = 2;
        public static readonly mFont tahoma_7b_dark = new mFont();
        public static readonly mFont tahoma_7_grey = new mFont();
        public static readonly mFont tahoma_7b_white = new mFont();
        public static readonly mFont tahoma_7_white = new mFont();

        public void drawString(mGraphics graphics, string text, int x, int y, int align) { }
    }

    public static class PopUp
    {
        public static Image imgPopUp;
        public static Image imgPopUp2;
        public static void paintPopUp(mGraphics graphics, int x, int y, int width, int height, int color, bool isButton) { }
    }
}
