using System;
using Nro.UI;

namespace Game1.UI.Adapters
{
    public static class CommandFactory
    {
        public static Command CreateStandard(string caption, Action action, UiRect bounds)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            Command cmd = new Command(caption, action);
            cmd.idAction = 0;
            cmd.actionListener = null;
            cmd.p = null;
            ApplyBounds(cmd, bounds, defaultW: 76, defaultH: 26, fixedWidth: true, fixedHeight: true);
            return cmd;
        }

        public static Command CreateStandard(string caption, IActionListener listener, int idAction, object p, UiRect bounds)
        {
            ValidateListenerRoute(listener, idAction);
            Command cmd = new Command(caption, listener, idAction, p, bounds.X, bounds.Y);
            cmd.action = null;
            ApplyBounds(cmd, bounds, defaultW: 76, defaultH: 26, fixedWidth: true, fixedHeight: true);
            return cmd;
        }

        public static Command CreateWide(string caption, Action action, UiRect bounds)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            Command cmd = new Command(caption, action);
            cmd.idAction = 0;
            cmd.actionListener = null;
            cmd.p = null;
            cmd.setType();
            ApplyBounds(cmd, bounds, defaultW: 160, defaultH: 26, fixedWidth: true, fixedHeight: true);
            return cmd;
        }

        public static Command CreateWide(string caption, IActionListener listener, int idAction, object p, UiRect bounds)
        {
            ValidateListenerRoute(listener, idAction);
            Command cmd = new Command(caption, listener, idAction, p, bounds.X, bounds.Y);
            cmd.action = null;
            cmd.setType();
            ApplyBounds(cmd, bounds, defaultW: 160, defaultH: 26, fixedWidth: true, fixedHeight: true);
            return cmd;
        }

        public static Command CreateDelete(string caption, Action action, UiRect bounds)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            Command cmd = new Command(caption, action);
            cmd.idAction = 0;
            cmd.actionListener = null;
            cmd.p = null;
            cmd.setTypeDelete();
            ApplyBounds(cmd, bounds, defaultW: 50, defaultH: 26, fixedWidth: false, fixedHeight: true);
            cmd.hw = cmd.w / 2;
            return cmd;
        }

        public static Command CreateDelete(string caption, IActionListener listener, int idAction, object p, UiRect bounds)
        {
            ValidateListenerRoute(listener, idAction);
            Command cmd = new Command(caption, listener, idAction, p, bounds.X, bounds.Y);
            cmd.action = null;
            cmd.setTypeDelete();
            ApplyBounds(cmd, bounds, defaultW: 50, defaultH: 26, fixedWidth: false, fixedHeight: true);
            cmd.hw = cmd.w / 2;
            return cmd;
        }

        public static Command CreateImageOnly(Image img, Action action, UiRect bounds)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            Command cmd = new Command(string.Empty, action);
            cmd.idAction = 0;
            cmd.actionListener = null;
            cmd.p = null;
            cmd.img = img;
            int defW = img != null ? mGraphics.getImageWidth(img) : bounds.Width;
            int defH = img != null ? mGraphics.getImageHeight(img) : bounds.Height;
            ApplyBounds(cmd, bounds, defaultW: defW, defaultH: defH, fixedWidth: img != null, fixedHeight: img != null);
            return cmd;
        }

        public static Command CreateImageOnly(Image img, IActionListener listener, int idAction, object p, UiRect bounds)
        {
            ValidateListenerRoute(listener, idAction);
            Command cmd = new Command(string.Empty, listener, idAction, p, bounds.X, bounds.Y);
            cmd.action = null;
            cmd.img = img;
            int defW = img != null ? mGraphics.getImageWidth(img) : bounds.Width;
            int defH = img != null ? mGraphics.getImageHeight(img) : bounds.Height;
            ApplyBounds(cmd, bounds, defaultW: defW, defaultH: defH, fixedWidth: img != null, fixedHeight: img != null);
            return cmd;
        }

        private static void ValidateListenerRoute(IActionListener listener, int idAction)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            if (idAction <= 0)
            {
                throw new ArgumentException("idAction must be greater than 0 for IActionListener route because Command.performAction() ignores idAction <= 0", nameof(idAction));
            }
        }

        private static void ApplyBounds(Command cmd, UiRect bounds, int defaultW, int defaultH, bool fixedWidth, bool fixedHeight)
        {
            cmd.x = bounds.X;
            cmd.y = bounds.Y;
            cmd.w = fixedWidth ? defaultW : (bounds.Width > 0 ? bounds.Width : defaultW);
            cmd.h = fixedHeight ? defaultH : (bounds.Height > 0 ? bounds.Height : defaultH);
        }
    }
}
