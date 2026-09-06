using System;

namespace Game1
{
	/// <summary>
	/// Native fishing quick-time overlay. The server owns the sequence, timer,
	/// and outcome; this class only draws its current state and forwards each
	/// physical direction key for validation.
	/// </summary>
	public static class FishingQuickTime
	{
		private const sbyte ActionStart = 0;
		private const sbyte ActionInput = 1;
		private const sbyte ActionEnd = 2;
		private const sbyte ActionPrepare = 3;

		private const int Left = 0;
		private const int Up = 1;
		private const int Right = 2;
		private const int Down = 3;

		// These are x4 crops of the supplied button2.png sheet.  Keeping them as
		// normal client images instead of drawing arrows makes every direction
		// legible on both zoom modes.
		private static readonly string[] ButtonImagePaths =
		{
			"/fishing_qte/button_left.png",
			"/fishing_qte/button_up.png",
			"/fishing_qte/button_right.png",
			"/fishing_qte/button_down.png"
		};

		private static Image[] buttonImages;

		private static bool active;
		private static bool failed;
		private static bool completed;
		private static int token;
		private static int[][] stages;
		private static int[] durations;
		private static int stageIndex;
		private static int keyIndex;
		private static long deadline;
		private static long failedUntil;
		private static bool reopenInventory;

		public static bool IsActive => active;

		public static void readMessage(Message message)
		{
			try
			{
				sbyte action = message.reader().readByte();
				if (action == ActionStart)
				{
					start(message);
				}
				else if (action == ActionPrepare)
				{
					prepare();
				}
				else if (action == ActionEnd)
				{
					finish(message.reader().readByte() == 1);
				}
			}
			catch (Exception)
			{
				reset();
			}
		}

		public static void update()
		{
			if (failed && mSystem.currentTimeMillis() >= failedUntil)
			{
				reset();
			}
		}

		/// <summary>Consumes all keys while fishing is active so the character cannot move or use skills.</summary>
		public static bool tryHandleKey(int keyCode)
		{
			if (!active)
			{
				return false;
			}
			if (failed || completed)
			{
				return true;
			}
			int direction = toDirection(keyCode);
			if (direction < 0)
			{
				return true;
			}
			if (mSystem.currentTimeMillis() > deadline)
			{
				return true;
			}

			Service.gI().sendFishingQuickTimeInput(token, (sbyte)stageIndex, (sbyte)direction);
			if (direction != stages[stageIndex][keyIndex])
			{
				failed = true;
				failedUntil = mSystem.currentTimeMillis() + 300L;
				return true;
			}

			keyIndex++;
			if (keyIndex == stages[stageIndex].Length)
			{
				keyIndex = 0;
				stageIndex++;
				if (stageIndex == stages.Length)
				{
					completed = true;
					return true;
				}
				deadline = mSystem.currentTimeMillis() + durations[stageIndex];
			}
			return true;
		}

		public static void paint(mGraphics g)
		{
			if (!active || stages == null)
			{
				return;
			}
			int panelW = System.Math.Min(GameCanvas.w - 10, 246);
			int panelH = 72;
			int panelX = (GameCanvas.w - panelW) / 2;
			int panelY = System.Math.Max(8, GameCanvas.h - panelH - 5);

			g.translate(-g.getTranslateX(), -g.getTranslateY());
			g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			g.FillRect(panelX + 2, panelY + 2, panelW, panelH, 0, 0.35f);
			GameCanvas.paintz.paintFrameSimple(panelX, panelY, panelW, panelH, g);
			g.setColor(15596245);
			g.fillRect(panelX + 2, panelY + 2, panelW - 4, panelH - 4);
			g.setColor(8015905);
			g.fillRect(panelX + 3, panelY + 3, panelW - 6, 16);
			g.setColor(6702080);
			g.drawRect(panelX + 2, panelY + 2, panelW - 5, panelH - 5);
			mFont.tahoma_7b_white.drawString(g, failed ? "CÁ ĐÃ THOÁT!" : (completed ? "ĐANG KÉO CÁ..." : "CÁ ĐANG CẮN CÂU!"),
				GameCanvas.w / 2, panelY + 6, mFont.CENTER);

			int visibleStage = stageIndex < stages.Length ? stageIndex : stages.Length - 1;
			paintStage(g, panelX + 9, panelY + 22, panelW - 18, visibleStage);
		}

		private static void start(Message message)
		{
			token = message.reader().readInt();
			int count = message.reader().readUnsignedByte();
			if (count < 1 || count > 4)
			{
				throw new Exception("Invalid fishing quick-time stage count.");
			}
			stages = new int[count][];
			durations = new int[count];
			for (int stage = 0; stage < count; stage++)
			{
				durations[stage] = message.reader().readUnsignedShort();
				int keys = message.reader().readUnsignedByte();
				if (keys < 1 || keys > 7 || durations[stage] < 500)
				{
					throw new Exception("Invalid fishing quick-time data.");
				}
				stages[stage] = new int[keys];
				for (int key = 0; key < keys; key++)
				{
					int direction = message.reader().readUnsignedByte();
					if (direction < Left || direction > Down)
					{
						throw new Exception("Invalid fishing direction.");
					}
					stages[stage][key] = direction;
				}
			}
			stageIndex = 0;
			keyIndex = 0;
			deadline = mSystem.currentTimeMillis() + durations[0];
			prepare();
			failed = false;
			completed = false;
			active = true;
			GameCanvas.keyAsciiPress = 0;
			GameCanvas.clearKeyHold();
			GameCanvas.clearKeyPressed();
		}

		private static void finish(bool success)
		{
			if (success)
			{
				reset();
				return;
			}
			failed = true;
			completed = false;
			active = true;
			failedUntil = mSystem.currentTimeMillis() + 300L;
		}

		private static void reset()
		{
			active = false;
			failed = false;
			completed = false;
			token = 0;
			stages = null;
			durations = null;
			stageIndex = 0;
			keyIndex = 0;
			deadline = 0L;
			restoreInventoryPanel();
		}

		private static void prepare()
		{
			if (GameCanvas.panel == null || !GameCanvas.panel.isShow || GameCanvas.panel.type != 0 || GameCanvas.panel.currentTabIndex != 1)
			{
				return;
			}
			reopenInventory = true;
			GameCanvas.panel.isShow = false;
			GameCanvas.clearKeyPressed();
		}

		private static void restoreInventoryPanel()
		{
			if (!reopenInventory)
			{
				return;
			}
			reopenInventory = false;
			if (GameCanvas.panel == null || GameCanvas.panel.isShow)
			{
				return;
			}
			GameCanvas.panel.currentTabIndex = 1;
			GameCanvas.panel.setTypeMain();
			GameCanvas.panel.show();
		}

		private static void paintStage(mGraphics g, int x, int y, int width, int index)
		{
			bool isCurrent = !completed && !failed;
			bool isDone = completed;
			long remaining = isCurrent ? System.Math.Max(0L, deadline - mSystem.currentTimeMillis()) : 0L;
			int timerW = width - 2;
			g.setColor(5188133);
			g.fillRect(x, y, timerW, 5);
			g.setColor(6702080);
			g.drawRect(x - 1, y - 1, timerW + 1, 6);
			if (isCurrent)
			{
				int currentW = (int)(timerW * System.Math.Min(1D, remaining / (double)durations[index]));
				g.setColor(remaining < 800L ? 14311936 : 15235072);
				g.fillRect(x, y, currentW, 5);
			}
			else if (isDone)
			{
				g.setColor(7775540);
				g.fillRect(x, y, timerW, 5);
			}

			string label = "LƯỢT " + (index + 1) + "/" + stages.Length;
			if (isCurrent)
			{
				label += "  " + timerText(remaining);
			}
			mFont.tahoma_7b_dark.drawString(g, label, x, y + 7, mFont.LEFT);

			int buttonW = 26;
			int gap = 2;
			int buttonsW = stages[index].Length * buttonW + (stages[index].Length - 1) * gap;
			int buttonX = x + (width - buttonsW) / 2;
			for (int key = 0; key < stages[index].Length; key++)
			{
				int state = failed ? 3 : (isDone || (isCurrent && key < keyIndex) ? 2 : (isCurrent && key == keyIndex ? 1 : 0));
				paintButton(g, buttonX + key * (buttonW + gap), y + 16, buttonW, stages[index][key], state);
			}
		}

		private static void paintButton(mGraphics g, int x, int y, int size, int direction, int state)
		{
			g.setColor(state == 1 ? 15972096 : 4206623);
			g.fillRect(x, y, size, size);
			g.setColor(state == 1 ? 10420224 : 6702080);
			g.drawRect(x, y, size - 1, size - 1);

			Image image = getButtonImage(direction);
			if (image != null)
			{
				g.drawImage(image, x + (size - image.getWidth()) / 2, y + (size - image.getHeight()) / 2, 0);
			}
			else
			{
				g.setColor(16777215);
				paintArrow(g, x + size / 2, y + size / 2, direction);
			}

			if (state == 2)
			{
				// Completed keys are visibly greyed out, but remain recognizable.
				g.FillRect(x, y, size, size, 0, 0.62f);
			}
			else if (state == 3)
			{
				g.FillRect(x, y, size, size, 10027008, 0.62f);
			}
		}

		private static Image getButtonImage(int direction)
		{
			if (direction < Left || direction > Down)
			{
				return null;
			}
			if (buttonImages == null)
			{
				buttonImages = new Image[4];
			}
			if (buttonImages[direction] == null)
			{
				buttonImages[direction] = GameCanvas.loadImage(ButtonImagePaths[direction]);
			}
			return buttonImages[direction];
		}

		private static void paintArrow(mGraphics g, int x, int y, int direction)
		{
			switch (direction)
			{
				case Left:
					g.fillRect(x - 5, y - 2, 7, 4);
					for (int i = 0; i < 5; i++) g.fillRect(x - 5 + i, y - 4 + i, 1, 9 - i * 2);
					break;
				case Up:
					g.fillRect(x - 2, y - 2, 4, 7);
					for (int i = 0; i < 5; i++) g.fillRect(x - i, y - 5 + i, 1 + i * 2, 1);
					break;
				case Right:
					g.fillRect(x - 2, y - 2, 7, 4);
					for (int i = 0; i < 5; i++) g.fillRect(x + 5 - i, y - 4 + i, 1, 9 - i * 2);
					break;
				default:
					g.fillRect(x - 2, y - 5, 4, 7);
					for (int i = 0; i < 5; i++) g.fillRect(x - i, y + 5 - i, 1 + i * 2, 1);
					break;
			}
		}

		private static int toDirection(int keyCode)
		{
			switch (keyCode)
			{
				case -3:
				case 97:
				case 52:
					return Left;
				case -1:
				case 119:
				case 56:
					return Up;
				case -4:
				case 100:
				case 54:
					return Right;
				case -2:
				case 115:
				case 50:
					return Down;
				default:
					return -1;
			}
		}

		private static string timerText(long remaining)
		{
			int tenths = (int)((remaining + 99L) / 100L);
			return tenths / 10 + "." + tenths % 10 + "s";
		}
	}
}
