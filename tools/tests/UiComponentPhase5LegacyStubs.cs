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

    public class Res
    {
        public static int abs(int value) { return value > 0 ? value : -value; }
    }

    public static class Main
    {
        public static bool isPC = true;
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

        public void removeAllElements()
        {
            _items.Clear();
        }
    }

    public class TopInfo
    {
        public int pId;
        public string name = string.Empty;
        public string info = string.Empty;
        public int rank;
        public int headID;
        public short headICON = -1;
        public short body = -1;
        public short leg = -1;
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

    public static class mResources
    {
        public static string rank = "Hạng";
        public static string[] CHAR_ORDER = new string[] { "Thách đấu" };
    }

    public class SoundMn
    {
        private static SoundMn _instance = new SoundMn();
        public static int PanelClickCount;

        public static SoundMn gI()
        {
            return _instance;
        }

        public void panelClick()
        {
            PanelClickCount++;
        }

        public static void ResetTracking()
        {
            PanelClickCount = 0;
        }
    }

    public class Service
    {
        private static Service _instance = new Service();
        public static readonly List<string> SentTopCalls = new List<string>();
        public static readonly List<int> SentThachDauCalls = new List<int>();

        public static Service gI()
        {
            return _instance;
        }

        public void sendTop(string topName, sbyte selected)
        {
            SentTopCalls.Add(topName + ":" + selected);
        }

        public void sendThachDau(int pId)
        {
            SentThachDauCalls.Add(pId);
        }

        public static void ResetTracking()
        {
            SentTopCalls.Clear();
            SentThachDauCalls.Clear();
        }
    }

    public static class GameCanvas
    {
        public static int px;
        public static int py;
        public static int pxMouse;
        public static int pyMouse;
        public static int w = 320;
        public static int h = 240;
        public static bool isPointerDown;
        public static bool isPointerJustDown;
        public static bool isPointerJustRelease;
        public static bool isPointerClick;
        public static bool isTouch = false;

        public static readonly bool[] keyPressed = new bool[50];

        public static bool isPointer(int x, int y, int width, int height)
        {
            return px >= x && px <= x + width && py >= y && py <= y + height;
        }

        public static void clearAllPointerEvent()
        {
            isPointerDown = false;
            isPointerJustDown = false;
            isPointerJustRelease = false;
            isPointerClick = false;
        }

        public static void clearKeyPressed()
        {
            for (int i = 0; i < keyPressed.Length; i++)
            {
                keyPressed[i] = false;
            }
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
}
