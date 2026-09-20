using System;

namespace Game2.UI
{
    public class UiInputContext
    {
        public int PointerX => GameCanvas.px;
        public int PointerY => GameCanvas.py;
        public bool IsPointerDown => GameCanvas.isPointerDown;
        public bool IsPointerJustDown => GameCanvas.isPointerJustDown;
        public bool IsPointerJustRelease => GameCanvas.isPointerJustRelease;
        public bool IsPointerClick => GameCanvas.isPointerClick;

        public bool IsModalBlocked { get; set; }

        public void ConsumePointer()
        {
            GameCanvas.isPointerJustRelease = false;
            GameCanvas.isPointerClick = false;
            GameCanvas.isPointerJustDown = false;
        }

        public void Reset()
        {
            IsModalBlocked = false;
        }
    }
}
