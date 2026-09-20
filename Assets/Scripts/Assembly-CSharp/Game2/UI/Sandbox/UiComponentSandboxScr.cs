using System;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.Sandbox
{
    public class UiComponentSandboxScr : mScreen
    {
        private static UiComponentSandboxScr _instance;

        private mScreen _previousScreen;
        private int _counter;
        private int _selectedRow = -1;
        private string _lastAction = "None";

        private UiButton _btnIncrement;
        private UiButton _btnDisabled;
        private ScrollViewAdapter _scrollAdapter;
        private UiInputContext _inputContext;

        private const int SampleRowCount = 20;
        private const int RowHeight = 22;
        private readonly string[] _sampleRows = new string[SampleRowCount];

        // Layout bounds
        private UiRect _mainFrameRect;
        private UiRect _btnIncRect;
        private UiRect _btnDisRect;
        private UiRect _listViewportRect;
        private UiRect _nestedOuterRect;
        private UiRect _nestedInnerRect;
        private UiRect _diagRect;

        public static void Open()
        {
            if (_instance == null)
            {
                _instance = new UiComponentSandboxScr();
            }
            if (object.ReferenceEquals(GameCanvas.currentScreen, _instance))
            {
                return;
            }
            _instance._previousScreen = GameCanvas.currentScreen;
            _instance.Init();
            _instance.switchToMe();
        }

        public void Close()
        {
            if (_previousScreen != null)
            {
                _previousScreen.switchToMe();
            }
            else if (GameCanvas.serverScreen != null)
            {
                GameCanvas.serverScreen.switchToMe();
            }
        }

        private void Init()
        {
            for (int i = 0; i < SampleRowCount; i++)
            {
                _sampleRows[i] = "Mục #" + (i + 1) + " (Dữ liệu thử nghiệm)";
            }

            _inputContext = new UiInputContext();

            // Calculate layout metrics based on GameCanvas dimensions
            int frameW = System.Math.Min(320, GameCanvas.w - 20);
            int frameH = System.Math.Min(220, GameCanvas.h - 20);
            int frameX = (GameCanvas.w - frameW) / 2;
            int frameY = (GameCanvas.h - frameH) / 2;
            _mainFrameRect = new UiRect(frameX, frameY, frameW, frameH);

            _btnIncRect = new UiRect(frameX + 10, frameY + 28, 80, 24);
            _btnDisRect = new UiRect(frameX + 96, frameY + 28, 80, 24);

            _listViewportRect = new UiRect(frameX + 10, frameY + 58, 140, 110);
            _nestedOuterRect = new UiRect(frameX + 160, frameY + 58, 140, 48);
            _nestedInnerRect = new UiRect(0, 0, 200, 70); // deliberately exceeds outer
            _diagRect = new UiRect(frameX + 160, frameY + 112, 140, 60);

            Command incCmd = CommandFactory.CreateStandard("Tăng (+1)", OnIncrementClicked, _btnIncRect);
            _btnIncrement = new UiButton(incCmd, _btnIncRect);

            Command disCmd = CommandFactory.CreateStandard("Khóa", OnDisabledClicked, _btnDisRect);
            _btnDisabled = new UiButton(disCmd, _btnDisRect) { Enabled = false };

            _scrollAdapter = new ScrollViewAdapter();
            _scrollAdapter.Configure(_listViewportRect, SampleRowCount, RowHeight);

            center = CommandFactory.CreateStandard("Đóng", Close, new UiRect(GameCanvas.hw - 38, GameCanvas.h - 26, 76, 26));
        }

        private void OnIncrementClicked()
        {
            _counter++;
            _lastAction = "Đã click tăng: " + _counter;
        }

        private void OnDisabledClicked()
        {
            _lastAction = "LỖI: Button disabled đã nhận click!";
        }

        public override void update()
        {
            base.update();
            _scrollAdapter?.Update();
        }

        public override void updateKey()
        {
            if (_inputContext == null) return;

            // Update button inputs
            bool parentDragging = _scrollAdapter != null && _scrollAdapter.IsDragging;

            if (_btnIncrement.UpdateInput(_inputContext, parentDragging))
            {
                return;
            }

            if (_btnDisabled.UpdateInput(_inputContext, parentDragging))
            {
                return;
            }

            // Update scroll & row selection
            if (_scrollAdapter != null && _scrollAdapter.UpdateKey(_inputContext, out int clickedIndex))
            {
                _selectedRow = clickedIndex;
                _lastAction = "Đã chọn dòng #" + (clickedIndex + 1);
                return;
            }

            base.updateKey();
        }

        public override void paint(mGraphics g)
        {
            base.paint(g);

            // 1. Draw Main Frame
            UiFrame.Paint(g, _mainFrameRect, UiFrameStyle.Simple);

            // Title
            mFont.tahoma_7b_dark.drawString(g, "Game2 UI Sandbox", _mainFrameRect.X + _mainFrameRect.Width / 2, _mainFrameRect.Y + 8, mFont.CENTER);

            // 2. Paint Buttons
            _btnIncrement.Paint(g);
            _btnDisabled.Paint(g);

            // 3. Paint Scrollable List
            using (UiRenderState.Push(g, _listViewportRect, clip: true))
            {
                // Background of list
                g.setColor(0xD6CBB8);
                g.fillRect(_listViewportRect.X, _listViewportRect.Y, _listViewportRect.Width, _listViewportRect.Height);

                int scrollY = _scrollAdapter.ScrollY;
                for (int i = 0; i < SampleRowCount; i++)
                {
                    int rowY = _listViewportRect.Y + i * RowHeight - scrollY;

                    // Viewport culling
                    if (rowY + RowHeight < _listViewportRect.Y || rowY > _listViewportRect.Y + _listViewportRect.Height)
                    {
                        continue;
                    }

                    bool isSelected = (i == _selectedRow);
                    g.setColor(isSelected ? UiColorTokens.RowSelected : (i % 2 == 0 ? UiColorTokens.RowNormal : 0xF2EDE4));
                    g.fillRect(_listViewportRect.X + 1, rowY, _listViewportRect.Width - 2, RowHeight - 1);

                    g.setColor(UiColorTokens.RowDivider);
                    g.fillRect(_listViewportRect.X + 1, rowY + RowHeight - 1, _listViewportRect.Width - 2, 1);

                    mFont font = isSelected ? mFont.tahoma_7b_dark : mFont.tahoma_7_grey;
                    font.drawString(g, _sampleRows[i], _listViewportRect.X + 6, rowY + 4, mFont.LEFT);
                }
            }

            // List border
            g.setColor(0x5A4738);
            g.drawRect(_listViewportRect.X, _listViewportRect.Y, _listViewportRect.Width, _listViewportRect.Height);

            // 4. Nested Clip Scope Demonstration
            g.setColor(0x5A4738);
            g.drawRect(_nestedOuterRect.X, _nestedOuterRect.Y, _nestedOuterRect.Width, _nestedOuterRect.Height);
            using (UiRenderState.Push(g, _nestedOuterRect, clip: true))
            {
                g.setColor(0xB8C8D6);
                g.fillRect(_nestedOuterRect.X, _nestedOuterRect.Y, _nestedOuterRect.Width, _nestedOuterRect.Height);

                // Draw inner content deliberately larger than outer viewport
                g.setColor(0xCE580C);
                g.fillRect(_nestedOuterRect.X + 5, _nestedOuterRect.Y + 5, _nestedInnerRect.Width, _nestedInnerRect.Height);

                mFont.tahoma_7b_white.drawString(g, "Clip lồng nhau (140x48)", _nestedOuterRect.X + 8, _nestedOuterRect.Y + 8, mFont.LEFT);
                mFont.tahoma_7_white.drawString(g, "Vùng này không tràn ra ngoài!", _nestedOuterRect.X + 8, _nestedOuterRect.Y + 24, mFont.LEFT);
            }

            // 5. Diagnostics Area
            g.setColor(0x3E3024);
            g.fillRect(_diagRect.X, _diagRect.Y, _diagRect.Width, _diagRect.Height);

            mFont.tahoma_7_white.drawString(g, "Counter: " + _counter, _diagRect.X + 4, _diagRect.Y + 4, mFont.LEFT);
            mFont.tahoma_7_white.drawString(g, "Dòng chọn: " + (_selectedRow >= 0 ? (_selectedRow + 1).ToString() : "Chưa chọn"), _diagRect.X + 4, _diagRect.Y + 16, mFont.LEFT);
            mFont.tahoma_7_white.drawString(g, "Cuộn Y: " + _scrollAdapter.ScrollY + "/" + _scrollAdapter.ScrollLimit + (_scrollAdapter.IsDragging ? " [KÉO]" : ""), _diagRect.X + 4, _diagRect.Y + 28, mFont.LEFT);
            mFont.tahoma_7_white.drawString(g, "Tab: Game2", _diagRect.X + 4, _diagRect.Y + 40, mFont.LEFT);
            mFont.tahoma_7_white.drawString(g, "Action: " + _lastAction, _diagRect.X + 4, _diagRect.Y + 50, mFont.LEFT);

            // Command bar at bottom
            center?.paint(g);
        }

        public override void unLoad()
        {
            base.unLoad();
            _inputContext?.Reset();
            _scrollAdapter?.Reset();
        }
    }
}
