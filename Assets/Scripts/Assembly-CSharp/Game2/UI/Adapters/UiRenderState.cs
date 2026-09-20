using System;
using Nro.UI;

namespace Game2.UI.Adapters
{
    public struct UiRenderState : IDisposable
    {
        private readonly mGraphics _graphics;
        private readonly mGraphics.RawRenderState _previousState;
        private bool _disposed;

        private UiRenderState(mGraphics g, mGraphics.RawRenderState previousState)
        {
            _graphics = g;
            _previousState = previousState;
            _disposed = false;
        }

        public static UiRenderState Push(mGraphics g, UiRect viewport, bool clip = true, bool translate = false)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));

            mGraphics.RawRenderState prevState = g.getRawState();

            if (clip)
            {
                int zoom = mGraphics.zoomLevel;
                int childScreenX = viewport.X * zoom + (prevState.isTranslate ? prevState.translateX : 0);
                int childScreenY = viewport.Y * zoom + (prevState.isTranslate ? prevState.translateY : 0);
                int childScreenW = System.Math.Max(0, viewport.Width * zoom);
                int childScreenH = System.Math.Max(0, viewport.Height * zoom);

                int finalScreenX;
                int finalScreenY;
                int finalScreenW;
                int finalScreenH;

                if (prevState.isClip)
                {
                    int parentScreenX = prevState.clipX + (prevState.isTranslate ? prevState.clipTX : 0);
                    int parentScreenY = prevState.clipY + (prevState.isTranslate ? prevState.clipTY : 0);
                    int parentScreenW = prevState.clipW;
                    int parentScreenH = prevState.clipH;

                    int x1 = System.Math.Max(parentScreenX, childScreenX);
                    int y1 = System.Math.Max(parentScreenY, childScreenY);
                    int x2 = System.Math.Min(parentScreenX + parentScreenW, childScreenX + childScreenW);
                    int y2 = System.Math.Min(parentScreenY + parentScreenH, childScreenY + childScreenH);

                    finalScreenX = x1;
                    finalScreenY = y1;
                    finalScreenW = System.Math.Max(0, x2 - x1);
                    finalScreenH = System.Math.Max(0, y2 - y1);
                }
                else
                {
                    finalScreenX = childScreenX;
                    finalScreenY = childScreenY;
                    finalScreenW = childScreenW;
                    finalScreenH = childScreenH;
                }

                mGraphics.RawRenderState newState = prevState;
                newState.isClip = true;
                newState.clipTX = prevState.translateX;
                newState.clipTY = prevState.translateY;
                newState.clipX = finalScreenX - (prevState.isTranslate ? prevState.translateX : 0);
                newState.clipY = finalScreenY - (prevState.isTranslate ? prevState.translateY : 0);
                newState.clipW = finalScreenW;
                newState.clipH = finalScreenH;

                g.restoreRawState(newState);
            }

            if (translate && (viewport.X != 0 || viewport.Y != 0))
            {
                g.translate(viewport.X, viewport.Y);
            }

            return new UiRenderState(g, prevState);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _graphics.restoreRawState(_previousState);
            }
        }
    }
}
