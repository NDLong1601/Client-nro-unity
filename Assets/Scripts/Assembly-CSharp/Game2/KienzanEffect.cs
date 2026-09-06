using System;

namespace Game2
{
	/// <summary>Timed charge, flight and impact for the server-authoritative skill 27.</summary>
	public sealed class KienzanEffect : Effect2
	{
		public const int FirstSkillId = 186;
		public const int LastSkillId = 192;
		private const int ImpactDuration = 240;
		private const int MissingLaunchTimeout = 1000;
		// The supplied sheet has six unequal-width sprites, not six equal cells.
		private static readonly int[] FrameEdges = { 0, 30, 64, 109, 153, 196, 240 };
		private static Image atlas;
		private static int atlasZoom;
		private static long nextImageRetry;

		private readonly Char caster;
		private readonly int mapId;
		private readonly int chargeDuration;
		private readonly long startedAt;
		private readonly sbyte direction;
		private long launchedAt;
		private int travelDuration;
		private int startX, startY, endX, endY;
		private bool launched;
		private bool removed;

		private KienzanEffect(Char caster, int chargeDuration)
		{
			this.caster = caster;
			this.chargeDuration = System.Math.Max(1, System.Math.Min(2000, chargeDuration));
			direction = (sbyte)(caster.cdir < 0 ? -1 : 1);
			mapId = TileMap.mapID;
			startedAt = mSystem.currentTimeMillis();
			startX = caster.cx;
			startY = caster.cy - 40;
			loadImage();
		}

		public static void startCharge(Char caster, int duration)
		{
			// A repeated/new cast replaces its previous visual without cancelling
			// the newly initialized character state.
			for (int i = 0; i < vEffect2.size(); i++)
			{
				KienzanEffect old = vEffect2.elementAt(i) as KienzanEffect;
				if (old != null && old.caster == caster)
					old.remove();
			}
			vEffect2.addElement(new KienzanEffect(caster, duration));
		}

		public static void launch(Char caster, Point target, int duration)
		{
			KienzanEffect effect = null;
			for (int i = vEffect2.size() - 1; i >= 0; i--)
			{
				KienzanEffect candidate = vEffect2.elementAt(i) as KienzanEffect;
				if (candidate != null && !candidate.removed && candidate.caster == caster)
				{
					effect = candidate;
					break;
				}
			}
			if (target == null)
			{
				if (effect != null) effect.remove();
				caster.FinishKienzanCast();
				return;
			}
			// A missing charge packet must not prevent the projectile being drawn.
			if (effect == null)
			{
				effect = new KienzanEffect(caster, 1);
				vEffect2.addElement(effect);
			}
			effect.startX = caster.cx;
			effect.startY = caster.cy - 40;
			effect.endX = target.x;
			effect.endY = target.y;
			effect.travelDuration = System.Math.Max(1, System.Math.Min(2000, duration));
			effect.launchedAt = mSystem.currentTimeMillis();
			effect.launched = true;
		}

		// Compatibility with the older status-22 packet; new servers use 20/21.
		public static void add(int casterId, short startX, short startY, short endX, short endY, sbyte direction, short travelDuration)
		{
			Char caster = casterId == Char.myCharz().charID ? Char.myCharz() : GameScr.findCharInMap(casterId);
			if (caster == null) return;
			startCharge(caster, 1);
			launch(caster, new Point { x = endX, y = endY }, travelDuration);
		}

		public static bool isSkillId(int skillId)
		{
			return skillId >= FirstSkillId && skillId <= LastSkillId;
		}

		public override void update()
		{
			if (removed) return;
			if (TileMap.mapID != mapId || caster.statusMe == 5 || caster.statusMe == 14)
			{
				caster.FinishKienzanCast();
				remove();
				return;
			}
			long now = mSystem.currentTimeMillis();
			if (!launched)
			{
				if (!caster.isPaintNewSkill || now - startedAt >= chargeDuration + MissingLaunchTimeout)
				{
					caster.FinishKienzanCast();
					remove();
				}
				return;
			}
			if (now - launchedAt >= travelDuration)
			{
				caster.FinishKienzanCast();
				if (now - launchedAt >= travelDuration + ImpactDuration) remove();
			}
		}

		public override void paint(mGraphics g)
		{
			if (removed) return;
			loadImage();
			if (atlas == null) return;
			long now = mSystem.currentTimeMillis();
			int frame, x, y;
			if (!launched)
			{
				long elapsed = System.Math.Max(0, now - startedAt);
				frame = elapsed < chargeDuration ? System.Math.Min(2, (int)(elapsed * 3 / chargeDuration)) : 1 + (int)(elapsed / 80 % 2);
				x = caster.cx;
				y = caster.cy - 40;
			}
			else
			{
				long elapsed = System.Math.Max(0, now - launchedAt);
				if (elapsed < travelDuration)
				{
					frame = 3 + (int)(elapsed / 60 % 2);
					x = startX + (int)((endX - startX) * elapsed / travelDuration);
					y = startY + (int)((endY - startY) * elapsed / travelDuration);
				}
				else
				{
					if (elapsed >= travelDuration + ImpactDuration) return;
					frame = 5;
					x = endX;
					y = endY;
				}
			}
			g.drawRegion(atlas, FrameEdges[frame], 0, FrameEdges[frame + 1] - FrameEdges[frame], 80,
				direction < 0 ? mGraphics.TRANS_MIRROR : mGraphics.TRANS_NONE,
				x, y - GameCanvas.transY, mGraphics.HCENTER | mGraphics.VCENTER);
		}

		private static void loadImage()
		{
			long now = mSystem.currentTimeMillis();
			if (atlas != null && atlasZoom == mGraphics.zoomLevel) return;
			if (now < nextImageRetry) return;
			atlas = GameCanvas.loadImage("/effectdata/27/img.png");
			atlasZoom = mGraphics.zoomLevel;
			nextImageRetry = atlas == null ? now + 1000 : 0;
		}

		private void remove()
		{
			if (removed) return;
			removed = true;
			vRemoveEffect2.addElement(this);
		}
	}
}
