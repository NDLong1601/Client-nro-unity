using System;
using System.Collections;
using System.Collections.Generic;

namespace Game1
{
    public class Math
    {
        public static int abs(int value) { return value > 0 ? value : -value; }
        public static int min(int left, int right) { return left < right ? left : right; }
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

    public class MyVector
    {
        private readonly List<object> _items = new List<object>();

        public MyVector() { }
        public MyVector(string caption) { }

        public void addElement(object item)
        {
            _items.Add(item);
        }

        public int size()
        {
            return _items.Count;
        }

        public object elementAt(int index)
        {
            if (index < 0 || index >= _items.Count) return null;
            return _items[index];
        }
    }

    public class PKHistoryEntry
    {
        public string opponentName;
        public bool won;
        public int completedAt;
    }

    public class TopInfo
    {
        public int pId;
        public string name = string.Empty;
        public string info = string.Empty;
        public int rank;
        public int headID;
        public short headICON = -1;
    }

    public class PartImage
    {
        public short id;
        public sbyte dx;
        public sbyte dy;
    }

    public class Part
    {
        public PartImage[] pi;
    }

    public class Char
    {
        public int charID = 100;
        public string cName = "PlayerOne";
        private static Char _instance = new Char();

        public static readonly int[][][] CharInfo = new int[1][][]
        {
            new int[1][]
            {
                new int[1] { 0 }
            }
        };

        public static Char myCharz()
        {
            return _instance;
        }
    }

    public static class GameScr
    {
        public static Part[] parts = new Part[100];
    }

    public static class SmallImage
    {
        public static int DrawSmallImageCount;

        public static void drawSmallImage(mGraphics g, int id, int x, int y, int transform, int anchor)
        {
            DrawSmallImageCount++;
        }

        public static void ResetTracking()
        {
            DrawSmallImageCount = 0;
        }
    }

    public class NinjaUtil
    {
        public static string getTimeAgo(long seconds)
        {
            if (seconds < 60) return seconds + " giây";
            long minutes = seconds / 60;
            if (minutes < 60) return minutes + " phút";
            long hours = minutes / 60;
            if (hours < 24) return hours + " giờ";
            long days = hours / 24;
            return days + " ngày";
        }
    }

    public static class mResources
    {
        public static string ago = "trước";
        public static string rank = "Hạng";
    }

    public static class mSystem
    {
        public static long MockTimeMillis = 1000000000000L;

        public static long currentTimeMillis()
        {
            return MockTimeMillis;
        }
    }

    public class mGraphics
    {
        public const int HCENTER = 1;
        public const int VCENTER = 2;
        public const int LEFT = 4;
        public const int RIGHT = 8;
        public const int TOP = 16;
        public const int BOTTOM = 32;

        public static int zoomLevel = 1;
        public static int addYWhenOpenKeyBoard;

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

        public List<int> DrawnColors = new List<int>();
        public List<string> DrawnStrings = new List<string>();
        public List<string> DrawnFontTags = new List<string>();
        public List<RawRenderState> DrawnStringStates = new List<RawRenderState>();
        public int FillRectCount;
        public int FillRectBorderCount;
        public int DrawImageCount;
        public int LastColor;

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

        public void setColor(int color)
        {
            LastColor = color;
            DrawnColors.Add(color);
        }

        public void fillRect(int x, int y, int width, int height)
        {
            FillRectCount++;
        }

        public void fillRect(int x, int y, int width, int height, int border)
        {
            FillRectBorderCount++;
        }

        public void drawImage(Image img, int x, int y, int anchor)
        {
            DrawImageCount++;
        }

        public void ResetTracking()
        {
            DrawnColors.Clear();
            DrawnStrings.Clear();
            DrawnFontTags.Clear();
            DrawnStringStates.Clear();
            FillRectCount = 0;
            FillRectBorderCount = 0;
            DrawImageCount = 0;
        }
    }

    public class mFont
    {
        public const int LEFT = 0;
        public const int CENTER = 2;
        public const int RIGHT = 1;

        public static readonly mFont tahoma_7b_dark = new mFont("dark");
        public static readonly mFont tahoma_7_grey = new mFont("grey");
        public static readonly mFont tahoma_7b_green = new mFont("green");
        public static readonly mFont tahoma_7b_red = new mFont("red");
        public static readonly mFont tahoma_7_blue = new mFont("blue");
        public static readonly mFont tahoma_7_green2 = new mFont("green2");

        public readonly string FontTag;

        public mFont(string tag)
        {
            FontTag = tag;
        }

        public void drawString(mGraphics graphics, string text, int x, int y, int align)
        {
            if (graphics != null)
            {
                graphics.DrawnStrings.Add(text);
                graphics.DrawnFontTags.Add(FontTag);
                graphics.DrawnStringStates.Add(graphics.getRawState());
            }
        }
    }

    public class TField
    {
        public const int INPUT_TYPE_ANY = 0;
        public const int INPUT_TYPE_NUMERIC = 1;
        public const int INPUT_TYPE_PASSWORD = 2;
        public const int INPUT_ALPHA_NUMBER_ONLY = 3;

        public int x;
        public int y;
        public int width;
        public int height;
        public string name = string.Empty;
        public bool isFocus;
        public int inputType;
        public int maxTextLength = 500;
        private string _text = string.Empty;

        public int KeyPressedCount;
        public int LastKeyCode;
        public bool KeyPressedReturnValue = true;
        public int UpdateCount;
        public int PaintCount;

        public void setFocusWithKb(bool focus) { isFocus = focus; }
        public void setFocus(bool focus) { isFocus = focus; }
        public void setIputType(int type) { inputType = type; }
        public void setMaxTextLenght(int len) { maxTextLength = len; }
        public string getText() { return _text ?? string.Empty; }
        public void setText(string text) { _text = text; }
        public void update() { UpdateCount++; }
        public void paint(mGraphics g) { PaintCount++; }
        public bool keyPressed(int keyCode)
        {
            KeyPressedCount++;
            LastKeyCode = keyCode;
            return KeyPressedReturnValue;
        }

        public void ResetTracking()
        {
            KeyPressedCount = 0;
            LastKeyCode = 0;
            KeyPressedReturnValue = true;
            UpdateCount = 0;
            PaintCount = 0;
        }
    }
}
