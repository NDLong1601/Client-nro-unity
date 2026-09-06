namespace Game2
{
	/// <summary>Client VFX for the server-authoritative Barrier Prison cast.</summary>
	public sealed class BarrierPrisonEffect : Effect2
	{
		public const int FirstSkillId = 200;

		public const int LastSkillId = 206;

		private const int FrameCount = 24;

		private const int FramesPerRow = 6;

		private const int BuildDuration = 1080;

		private const int ClosingDuration = 540;

		// Kept compact enough to frame one character without covering the UI or
		// a large part of the map. The source atlas itself remains high resolution.
		private const int VisualDiameter = 112;

		private const int GroundOffset = 14;

		private const int PrisonRadius = 42;

		private const sbyte TargetMob = 1;

		private const sbyte TargetPlayer = 2;

		private static readonly Image[] frames = new Image[FrameCount];

		private readonly int centerX;

		private readonly int centerY;

		private readonly int constraintCenterX;

		private readonly int constraintCenterY;

		private readonly int duration;

		private readonly long startedAt;

		private readonly Mob targetMob;

		private readonly Char targetChar;

		private BarrierPrisonEffect(short serverCenterX, short serverCenterY, short duration,
			sbyte targetType, int targetId)
		{
			centerX = serverCenterX;
			centerY = serverCenterY;
			constraintCenterX = serverCenterX;
			constraintCenterY = serverCenterY;
			if (targetType == TargetMob)
			{
				targetMob = GameScr.findMobInMap(targetId);
				if (targetMob != null)
				{
					// xSd/ySd are the exact coordinates used by Mob.paintShadow,
					// so the prison base sits on the terrain below airborne/tall mobs.
					centerX = targetMob.xSd;
					centerY = targetMob.ySd;
					constraintCenterX = targetMob.x;
					constraintCenterY = targetMob.y;
				}
			}
			else if (targetType == TargetPlayer)
			{
				targetChar = ((Char.myCharz().charID == targetId)
					? Char.myCharz() : GameScr.findCharInMap(targetId));
				if (targetChar != null)
				{
					centerX = targetChar.xSd;
					centerY = targetChar.ySd;
					constraintCenterX = targetChar.cx;
					constraintCenterY = targetChar.cy;
				}
			}
			this.duration = System.Math.Max(1000, (int)duration);
			startedAt = mSystem.currentTimeMillis();
			loadImages();
		}

		public static void add(int casterId, short centerX, short centerY, short duration,
			sbyte targetType, int targetId)
		{
			if (casterId == Char.myCharz().charID && Char.myCharz().myskill != null
				&& Char.myCharz().myskill.template != null && Char.myCharz().myskill.template.id == 29)
			{
				Char.myCharz().myskill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
			}
			Effect2.vEffect2.addElement(new BarrierPrisonEffect(centerX, centerY, duration,
				targetType, targetId));
		}

		public static bool isSkillId(int skillId)
		{
			return skillId >= FirstSkillId && skillId <= LastSkillId;
		}

		public override void update()
		{
			if (mSystem.currentTimeMillis() - startedAt >= duration)
			{
				Effect2.vRemoveEffect2.addElement(this);
				return;
			}
			constrainTarget();
		}

		public override void paint(mGraphics g)
		{
			int elapsed = (int)(mSystem.currentTimeMillis() - startedAt);
			int frameIndex = getFrameIndex(elapsed);
			Image frame = frames[frameIndex];
			if (frame != null)
			{
				g.drawImageScale(frame, centerX - VisualDiameter / 2,
					centerY - VisualDiameter + GroundOffset,
					VisualDiameter, VisualDiameter, 0);
			}
		}

		private int getFrameIndex(int elapsed)
		{
			if (elapsed < BuildDuration)
			{
				return System.Math.Min(11, elapsed / 90);
			}
			int closingStart = System.Math.Max(BuildDuration, duration - ClosingDuration);
			if (elapsed < closingStart)
			{
				return 12 + ((elapsed - BuildDuration) / 105) % 6;
			}
			return 18 + System.Math.Min(5,
				(elapsed - closingStart) * 6 / System.Math.Max(1, duration - closingStart));
		}

		private static void loadImages()
		{
			if (frames[0] != null)
			{
				return;
			}
			Image sheet = GameCanvas.loadImage("/barrier_prison/frame3_prison_aligned.png");
			if (sheet == null)
			{
				return;
			}
			// Image.createImage crops in physical texture pixels. getWidth/getHeight
			// return logical pixels divided by zoomLevel, which previously caused
			// frames to be cut at half/quarter size and magnified into a blurry block.
			int cellWidth = sheet.texture.width / FramesPerRow;
			int cellHeight = sheet.texture.height / (FrameCount / FramesPerRow);
			if (cellWidth <= 0 || cellHeight <= 0)
			{
				return;
			}
			for (int i = 0; i < FrameCount; i++)
			{
				frames[i] = Image.createImage(sheet, (i % FramesPerRow) * cellWidth,
					(i / FramesPerRow) * cellHeight, cellWidth, cellHeight, 0);
			}
		}

		private void constrainTarget()
		{
			if (targetMob != null)
			{
				clampToPrison(ref targetMob.x, ref targetMob.y);
			}
			if (targetChar != null)
			{
				clampToPrison(ref targetChar.cx, ref targetChar.cy);
			}
		}

		private void clampToPrison(ref int x, ref int y)
		{
			int offsetX = x - constraintCenterX;
			int offsetY = y - constraintCenterY;
			long distanceSquared = (long)offsetX * offsetX + (long)offsetY * offsetY;
			if (distanceSquared <= (long)PrisonRadius * PrisonRadius)
			{
				return;
			}
			double distance = System.Math.Sqrt(distanceSquared);
			x = constraintCenterX + (int)System.Math.Round(offsetX * PrisonRadius / distance);
			y = constraintCenterY + (int)System.Math.Round(offsetY * PrisonRadius / distance);
		}
	}
}
