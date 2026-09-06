namespace Game2
{
    /// <summary>V2: server-owned, reusable formation with one projectile in flight.</summary>
    public sealed class SuperGhostKamikazeEffect : Effect2
    {
        public const int FirstSkillId = 214;
        public const int LastSkillId = 220;
        private const int Summoning = 0, Orbiting = 1, Flying = 2, Returning = 3, Spent = 4;
        private const int SpawnEvent = 10, EndEvent = 14;
        private const int FrameCount = 8, ImpactDuration = 450, RegroupDuration = 250;
        private const int SummonFrameDuration = 180, SummonStagger = 55;
        private const int IdleFrame = 3, FirstFlightFrame = 4, ExplosionFrame = 7;
        private const int InterpolationDuration = 90, ExpiryGraceDuration = 500;
        private static readonly Image[] frames = new Image[FrameCount];
        // Generated frame layout is expressed in x4 pixels; rendering uses logical units.
        private static readonly int[] pivotX = { 129, 127, 130, 137, 205, 205, 208, 138 };
        private static readonly int[] pivotY = { 132, 118, 122, 123, 141, 148, 156, 160 };

        private sealed class Ghost
        {
            public int state, variant, x, y, stateAge;
            public float fromX, fromY, layoutX, layoutY;
        }

        private readonly int casterId, castId;
        private readonly Ghost[] ghosts;
        private int sequence, elapsedAtPacket;
        private int ownerX, ownerY, targetType, targetId, targetX;
        private long receivedAt, expiresAt, layoutAt;
        private int layoutMask = -1;
        private bool ended, cancelled;

        private SuperGhostKamikazeEffect(int casterId, int castId, int count)
        {
            this.casterId = casterId;
            this.castId = castId;
            ghosts = new Ghost[count];
            for (int i = 0; i < count; i++) ghosts[i] = new Ghost();
            loadImages();
        }

        public static bool isSkillId(int skillId)
        {
            return skillId >= FirstSkillId && skillId <= LastSkillId;
        }

        public static int activeCastId(int casterId)
        {
            SuperGhostKamikazeEffect effect = find(casterId);
            return effect != null && !effect.ended && mSystem.currentTimeMillis() < effect.expiresAt
                ? effect.castId : 0;
        }

        // Read every field before mutating visible state. V2 uses event bytes 10..17;
        // this avoids interpreting legacy 16-bit durations as the new payload.
        public static void receive(int casterId, Message message)
        {
            int eventType = message.reader().readByte();
            if (eventType < 10 || eventType > 17) return;
            int id = message.reader().readInt();
            int serial = message.reader().readInt();
            int count = message.reader().readByte();
            int summon = message.reader().readShort();
            int lifetime = message.reader().readInt();
            int elapsed = message.reader().readInt();
            int remaining = message.reader().readInt();
            int cx = message.reader().readShort(), cy = message.reader().readShort();
            int type = message.reader().readByte(), target = message.reader().readInt();
            int tx = message.reader().readShort();
            message.reader().readShort(); // target Y; projectile positions already include vertical motion
            message.reader().readByte(); // changed index; complete state also repairs late observers
            if (count < 1 || count > 7 || lifetime < 1 || summon < 1) return;
            Ghost[] incoming = new Ghost[count];
            for (int i = 0; i < count; i++)
            {
                Ghost g = new Ghost();
                g.state = message.reader().readByte();
                g.variant = message.reader().readByte();
                g.x = message.reader().readShort();
                g.y = message.reader().readShort();
                g.stateAge = message.reader().readInt();
                if (g.state < Summoning || g.state > Spent || g.variant < 0 || g.variant > 2) return;
                incoming[i] = g;
            }
            SuperGhostKamikazeEffect effect = find(casterId);
            if (effect != null && (id < effect.castId || (id == effect.castId && serial <= effect.sequence))) return;
            bool created = effect == null || effect.castId != id;
            if (created)
            {
                if (eventType == EndEvent) return;
                if (effect != null) effect.cancelled = true;
                effect = new SuperGhostKamikazeEffect(casterId, id, count);
                Effect2.vEffect2.addElement(effect);
            }
            if (effect.ghosts.Length != count) return;
            long now = mSystem.currentTimeMillis();
            float[] oldX = new float[count], oldY = new float[count];
            Char caster = effect.getCaster();
            int currentOwnerX = caster == null ? cx : caster.cx;
            int currentOwnerY = caster == null ? cy : caster.cy;
            for (int i = 0; i < count; i++)
            {
                if (created) { oldX[i] = incoming[i].x; oldY[i] = incoming[i].y; }
                else effect.position(i, now, out oldX[i], out oldY[i]);
            }
            effect.sequence = serial;
            effect.elapsedAtPacket = elapsed;
            effect.receivedAt = now;
            effect.expiresAt = now + System.Math.Max(0, remaining);
            effect.ownerX = currentOwnerX; effect.ownerY = currentOwnerY;
            effect.targetType = type; effect.targetId = target;
            effect.targetX = tx;
            effect.ended = eventType == EndEvent;
            int newMask = 0;
            for (int i = 0; i < count; i++)
            {
                Ghost next = incoming[i];
                Ghost g = effect.ghosts[i];
                g.state = next.state; g.variant = next.variant;
                g.x = next.x; g.y = next.y; g.stateAge = next.stateAge;
                g.fromX = oldX[i]; g.fromY = oldY[i];
                if (g.state == Orbiting || g.state == Returning) newMask |= 1 << i;
            }
            if (effect.layoutMask != newMask || created)
            {
                effect.layoutMask = newMask;
                effect.layoutAt = now;
                for (int i = 0; i < count; i++)
                {
                    effect.ghosts[i].layoutX = oldX[i] - currentOwnerX;
                    effect.ghosts[i].layoutY = oldY[i] - currentOwnerY;
                }
            }
            // A snapshot on map entry must not restart the cooldown.
            if (eventType == SpawnEvent && casterId == Char.myCharz().charID
                && Char.myCharz().myskill != null && Char.myCharz().myskill.template.id == 31)
                Char.myCharz().myskill.lastTimeUseThisSkill = now;
        }

        public override void update()
        {
            long now = mSystem.currentTimeMillis();
            Char caster = getCaster();
            if (cancelled || caster == null || caster.cHP <= 0)
            {
                Effect2.vRemoveEffect2.addElement(this);
                return;
            }
            if (ended || now > expiresAt + ExpiryGraceDuration)
            {
                bool impactVisible = false;
                for (int i = 0; i < ghosts.Length; i++)
                    if (ghosts[i].state == Spent && ghosts[i].stateAge + now - receivedAt < ImpactDuration)
                        impactVisible = true;
                if (!impactVisible) Effect2.vRemoveEffect2.addElement(this);
            }
        }

        public override void paint(mGraphics g)
        {
            if (cancelled) return;
            Char caster = getCaster();
            if (caster == null) return;
            long now = mSystem.currentTimeMillis();
            long elapsed = elapsedAtPacket + now - receivedAt;
            for (int i = 0; i < ghosts.Length; i++)
            {
                Ghost ghost = ghosts[i];
                if (ghost.state == Spent)
                {
                    if (ghost.stateAge + now - receivedAt < ImpactDuration)
                        draw(g, ExplosionFrame, ghost.x, ghost.y, 1);
                    continue;
                }
                if (ended || now >= expiresAt) continue;
                int frame = IdleFrame, direction = caster.cdir;
                if (ghost.state == Summoning)
                {
                    long local = elapsed - i * SummonStagger;
                    if (local < 0) continue;
                    frame = local < IdleFrame * SummonFrameDuration
                        ? System.Math.Min(IdleFrame - 1, (int)(local / SummonFrameDuration))
                        : IdleFrame;
                }
                float x, y;
                position(i, now, out x, out y);
                if (ghost.state == Flying || ghost.state == Returning)
                {
                    frame = FirstFlightFrame + ghost.variant;
                    int aimX = ghost.state == Returning ? caster.cx : resolveTargetX();
                    direction = aimX < x ? -1 : 1;
                }
                draw(g, frame, (int)System.Math.Round(x), (int)System.Math.Round(y), direction);
            }
        }

        private int resolveTargetX()
        {
            if (targetType == 1)
            {
                Mob mob = GameScr.findMobInMap(targetId);
                if (mob != null) return mob.x;
            }
            else if (targetType == 2)
            {
                Char victim = targetId == Char.myCharz().charID ? Char.myCharz() : GameScr.findCharInMap(targetId);
                if (victim != null) return victim.cx;
            }
            return targetX;
        }

        private void position(int index, long now, out float x, out float y)
        {
            Ghost ghost = ghosts[index];
            Char caster = getCaster();
            int cx = caster == null ? ownerX : caster.cx, cy = caster == null ? ownerY : caster.cy;
            long elapsed = elapsedAtPacket + now - receivedAt;
            if (ghost.state == Summoning)
            {
                double center = (ghosts.Length - 1) / 2.0;
                x = cx + (float)((index - center) * (18 + System.Math.Max(0, ghosts.Length - 3) * 2));
                y = cy - 54 - (ghosts.Length - 1) * 3 + (float)(System.Math.Abs(index - center) * 10);
            }
            else if (ghost.state == Orbiting)
            {
                int count = 0, rank = 0;
                for (int i = 0; i < ghosts.Length; i++)
                    if (ghosts[i].state == Orbiting || ghosts[i].state == Returning)
                    {
                        if (i < index) rank++;
                        count++;
                    }
                count = System.Math.Max(1, count);
                double angle = elapsed * 0.006 + System.Math.PI * 2 * rank / count;
                float dx = (float)(System.Math.Cos(angle) * (42 + (count - 1) * 8));
                float dy = -38 - (count - 1) * 4 + (float)(System.Math.Sin(angle) * (18 + (count - 1) * 4));
                float t = System.Math.Min(1f, (float)(now - layoutAt) / RegroupDuration);
                t = t * t * (3f - 2f * t);
                x = cx + ghost.layoutX * (1 - t) + dx * t;
                y = cy + ghost.layoutY * (1 - t) + dy * t;
            }
            else
            {
                // Smooth the authoritative positions between server ticks.
                float t = System.Math.Min(1f, System.Math.Max(0f,
                    (float)(now - receivedAt) / InterpolationDuration));
                x = ghost.fromX + (ghost.x - ghost.fromX) * t;
                y = ghost.fromY + (ghost.y - ghost.fromY) * t;
            }
        }

        private Char getCaster()
        {
            return casterId == Char.myCharz().charID ? Char.myCharz() : GameScr.findCharInMap(casterId);
        }

        private static SuperGhostKamikazeEffect find(int casterId)
        {
            for (int i = Effect2.vEffect2.size() - 1; i >= 0; i--)
            {
                SuperGhostKamikazeEffect effect = Effect2.vEffect2.elementAt(i) as SuperGhostKamikazeEffect;
                if (effect != null && !effect.cancelled && effect.casterId == casterId) return effect;
            }
            return null;
        }

        private static void loadImages()
        {
            for (int i = 0; i < FrameCount; i++)
                if (frames[i] == null) frames[i] = GameCanvas.loadImage("/super_ghost_kamikaze/frame_" + i + ".png");
        }

        private static void draw(mGraphics g, int frame, int x, int y, int direction)
        {
            Image image = frames[frame];
            if (image == null) return;
            int size = frame == ExplosionFrame ? 72 : 66;
            int px = pivotX[frame] / 4, py = pivotY[frame] / 4;
            // drawImageScale mirrors by negative width; its x must be the RIGHT
            // edge when mirrored. The pivot stays fixed on the ghost's face.
            g.drawImageScale(image, direction < 0 ? x + px : x - px,
                y - py, size, size, direction < 0 ? 1 : 0);
        }
    }
}
