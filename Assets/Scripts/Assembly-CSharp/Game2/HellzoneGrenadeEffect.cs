namespace Game2
{
	/// <summary>Client VFX for the server-authoritative Hellzone Grenade cast.</summary>
	public sealed class HellzoneGrenadeEffect : Effect2
	{
		public const int FirstSkillId = 193;

		public const int LastSkillId = 199;

		private const int FrameCount = 6;

		private const int ChargeFrameCount = 3;

		private const int OrbCount = 6;

		private const int ChargeDuration = 520;

		private const int LaunchStagger = 65;

		private static readonly int[] formationX = { -34, -12, 16, 38, -24, 24 };

		private static readonly int[] formationY = { -28, -50, -54, -30, -7, -10 };

		private static readonly int[] impactX = { -12, 5, -6, 13, 0, 9 };

		private static readonly int[] impactY = { -8, -15, 7, -3, -7, 9 };

		private static readonly int[] curveDistance = { -28, 24, -16, 31, -24, 15 };

		private static readonly Image[] frames = new Image[FrameCount];

		private readonly int startX;

		private readonly int startY;

		private readonly int endX;

		private readonly int endY;

		private readonly sbyte direction;

		private readonly int duration;

		private readonly long startedAt;

		private HellzoneGrenadeEffect(short startX, short startY, short endX, short endY,
			sbyte direction, short duration)
		{
			this.startX = startX;
			this.startY = startY;
			this.endX = endX;
			this.endY = endY;
			this.direction = (sbyte)((direction < 0) ? (-1) : 1);
			this.duration = ((duration > 0) ? duration : 1600);
			startedAt = mSystem.currentTimeMillis();
			loadImages();
		}

		public static void add(int casterId, short startX, short startY, short endX,
			short endY, sbyte direction, short duration)
		{
			if (casterId == Char.myCharz().charID && Char.myCharz().myskill != null
				&& Char.myCharz().myskill.template.id == 28)
			{
				Char.myCharz().myskill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
			}
			Effect2.vEffect2.addElement(new HellzoneGrenadeEffect(startX, startY, endX,
				endY, direction, duration));
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
			}
		}

		public override void paint(mGraphics g)
		{
			int elapsed = (int)(mSystem.currentTimeMillis() - startedAt);
			int chargeDuration = System.Math.Min(ChargeDuration, System.Math.Max(320, duration / 3));
			if (elapsed < chargeDuration)
			{
				for (int i = 0; i < OrbCount; i++)
				{
					if (elapsed < i * 45)
					{
						continue;
					}
					int frameIndex = (elapsed / 75 + i) % ChargeFrameCount;
					int floatY = (int)(System.Math.Sin((elapsed + i * 90) * 0.012) * 3.0);
					draw(g, frames[frameIndex], startX + formationX[i] * direction,
						startY + formationY[i] + floatY, direction);
				}
				return;
			}

			int travelDuration = System.Math.Max(1, duration - chargeDuration);
			int travelElapsed = System.Math.Min(travelDuration, elapsed - chargeDuration);
			int deltaX = endX - startX;
			int deltaY = endY - startY;
			double distance = System.Math.Max(1.0, System.Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
			double perpendicularX = -deltaY / distance;
			double perpendicularY = deltaX / distance;

			for (int i = 0; i < OrbCount; i++)
			{
				int sphereStartX = startX + formationX[i] * direction;
				int sphereStartY = startY + formationY[i];
				int localElapsed = travelElapsed - i * LaunchStagger;
				if (localElapsed < 0)
				{
					int waitFrame = (travelElapsed / 75 + i) % ChargeFrameCount;
					draw(g, frames[waitFrame], sphereStartX, sphereStartY, direction);
					continue;
				}

				int localDuration = System.Math.Max(1, travelDuration - i * LaunchStagger);
				float progress = System.Math.Min(1f, (float)localElapsed / localDuration);
				float eased = progress * progress * (3f - 2f * progress);
				double bend = System.Math.Sin(progress * System.Math.PI) * curveDistance[i];
				int targetX = endX + impactX[i];
				int targetY = endY + impactY[i];
				int currentX = sphereStartX + (int)((targetX - sphereStartX) * eased
					+ perpendicularX * bend);
				int currentY = sphereStartY + (int)((targetY - sphereStartY) * eased
					+ perpendicularY * bend);
				int travelFrame = ChargeFrameCount
					+ (localElapsed / 70 + i) % (FrameCount - ChargeFrameCount);
				draw(g, frames[travelFrame], currentX, currentY, direction);
			}
		}

		private static void loadImages()
		{
			for (int i = 0; i < FrameCount; i++)
			{
				if (frames[i] == null)
				{
					frames[i] = GameCanvas.loadImage("/hellzone_grenade/frame_" + i + ".png");
				}
			}
		}

		private static void draw(mGraphics g, Image image, int x, int y, sbyte direction)
		{
			if (image != null)
			{
				int transform = (direction > 0) ? mGraphics.TRANS_MIRROR : mGraphics.TRANS_NONE;
				g.drawRegion(image, 0, 0, image.getWidth(), image.getHeight(), transform, x, y,
					mGraphics.HCENTER | mGraphics.VCENTER);
			}
		}
	}
}
