using System;

namespace Game2
{
	public class NewBoss : Mob, IMapObject
	{
		public static Image shadowBig = mSystem.loadImage("/mainImage/shadowBig.png");

		public int xTo;

		public int yTo;

		public new int xSd;

		public new int ySd;

		public new bool isShadown = true;

		private int tick;

		private int frame;

		public new static Image imgHP = mSystem.loadImage("/mainImage/myTexture2dmobHP.png");

		private int fy;

		private bool flyUp;

		public new bool isBusyAttackSomeOne = true;

		private Char[] charAttack;

		private int[] dameHP;

		private sbyte type;

		private int offset;

		private int xTempRight = -1;

		private int xTempLeft = -1;

		private int yTemp = -1;

		private sbyte[] cou = new sbyte[2] { -1, 1 };

		public new int forceWait;

		private int[][] frameArr = new int[17][]
		{
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
			new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 }
		};

		public NewBoss(int id, short px, short py, int templateID, long hp, long maxHp, int s)
		{
			mobId = id;
			x = (xFirst = px + 20);
			yFirst = py;
			y = py;
			xTo = x;
			yTo = y;
			base.maxHp = maxHp;
			base.hp = hp;
			templateId = templateID;
			h_hp_bar = 6;
			w_hp_bar = 100;
			len = w_hp_bar;
			updateHp_bar();
			if (Mob.arrMobTemplate[templateId].data == null)
			{
				Service.gI().requestModTemplate(templateId);
			}
			status = 2;
			frameArr = null;
		}

		public override void setBody(short id)
		{
			changBody = true;
			smallBody = id;
		}

		public override void clearBody()
		{
			changBody = false;
		}

		public new void checkFrameTick(int[] array)
		{
			tick++;
			if (tick > array.Length - 1)
			{
				tick = 0;
			}
			frame = array[tick];
		}

		public void updateShadown()
		{
			int num = 0;
			xSd = x;
			if (TileMap.tileTypeAt(x, y, 2))
			{
				ySd = y;
				return;
			}
			ySd = y;
			while (num < 30)
			{
				num++;
				ySd += 24;
				if (TileMap.tileTypeAt(xSd, ySd, 2))
				{
					if (ySd % 24 != 0)
					{
						ySd -= ySd % 24;
					}
					break;
				}
			}
		}

		private void paintShadow(mGraphics g)
		{
			int size = TileMap.size;
			if ((TileMap.mapID < 114 || TileMap.mapID > 120) && TileMap.mapID != 127 && TileMap.mapID != 128)
			{
				if (TileMap.tileTypeAt(xSd + size / 2, ySd + 1, 4))
				{
					g.setClip(xSd / size * size, (ySd - 30) / size * size, size, 100);
				}
				else if (TileMap.tileTypeAt((xSd - size / 2) / size, (ySd + 1) / size) == 0)
				{
					g.setClip(xSd / size * size, (ySd - 30) / size * size, 100, 100);
				}
				else if (TileMap.tileTypeAt((xSd + size / 2) / size, (ySd + 1) / size) == 0)
				{
					g.setClip(xSd / size * size, (ySd - 30) / size * size, size, 100);
				}
				else if (TileMap.tileTypeAt(xSd - size / 2, ySd + 1, 8))
				{
					g.setClip(xSd / 24 * size, (ySd - 30) / size * size, size, 100);
				}
			}
			g.drawImage(shadowBig, xSd, ySd - 5, 3);
			g.setClip(GameScr.cmx, GameScr.cmy - GameCanvas.transY, GameScr.gW, GameScr.gH + 2 * GameCanvas.transY);
		}

		public override void update()
		{
			if (frameArr == null && Mob.arrMobTemplate[templateId].data != null)
			{
				GetFrame();
			}
			if (frameArr == null || !isUpdate())
			{
				return;
			}
			updateShadown();
			switch (status)
			{
			case 0:
			case 1:
				updateDead();
				break;
			case 2:
				updateMobStandWait();
				break;
			case 3:
				updateMobAttack();
				break;
			case 4:
				updateMobFly();
				break;
			case 5:
				timeStatus = 0;
				updateMobWalk();
				break;
			case 6:
				timeStatus = 0;
				p1++;
				y += p1;
				if (y >= yFirst)
				{
					y = yFirst;
					p1 = 0;
					status = 5;
				}
				break;
			case 7:
				updateInjure();
				base.update();
				break;
			}
		}

		private void updateDead()
		{
			tick++;
			if (tick > frameArr[13].Length - 1)
			{
				tick = frameArr[13].Length - 1;
			}
			frame = frameArr[13][tick];
			if (x != xTo || y != yTo)
			{
				x += (xTo - x) / 4;
				y += (yTo - y) / 4;
			}
		}

		private void updateMobFly()
		{
		}

		private void updateInjure()
		{
		}

		private void updateMobStandWait()
		{
			checkFrameTick(frameArr[0]);
			if (x != xTo || y != yTo)
			{
				x += (xTo - x) / 4;
				y += (yTo - y) / 4;
			}
		}

		public void setFly()
		{
			status = 4;
			flyUp = true;
		}

		public void setAttack(Char[] cAttack, int[] dame, sbyte type, sbyte dir)
		{
			charAttack = cAttack;
			dameHP = dame;
			this.type = type;
			base.dir = dir;
			status = 3;
			if (x != xTo || y != yTo)
			{
				x += (xTo - x) / 4;
				y += (yTo - y) / 4;
			}
		}

		public new void updateMobAttack()
		{
			if (tick == frameArr[type + 1].Length - 1)
			{
				status = 2;
			}
			checkFrameTick(frameArr[type + 1]);
			if (tick == frameArr[15][type - 1])
			{
				for (int i = 0; i < charAttack.Length; i++)
				{
					charAttack[i].doInjure(dameHP[i], 0L, isCrit: false, isMob: false);
					ServerEffect.addServerEffect(frameArr[16][type - 1], charAttack[i].cx, charAttack[i].cy, 1);
				}
			}
		}

		public new void updateMobWalk()
		{
			checkFrameTick(frameArr[1]);
			sbyte speed = Mob.arrMobTemplate[templateId].speed;
			int num = speed;
			if (Res.abs(x - xTo) < speed)
			{
				num = Res.abs(x - xTo);
			}
			x += ((x >= xTo) ? (-num) : num);
			y = yTo;
			if (x < xTo)
			{
				dir = 1;
			}
			else if (x > xTo)
			{
				dir = -1;
			}
			if (Res.abs(x - xTo) <= 1)
			{
				x = xTo;
				status = 2;
			}
		}

		public new bool isUpdate()
		{
			return status != 0;
		}

		public override void paint(mGraphics g)
		{
			if (Mob.arrMobTemplate[templateId].data == null || isHide)
			{
				return;
			}
			if (isMafuba)
			{
				if (!changBody)
				{
					Mob.arrMobTemplate[templateId].data.paintFrame(g, frame, xMFB, yMFB, (dir != 1) ? 1 : 0, 2);
				}
				else
				{
					SmallImage.drawSmallImage(g, smallBody, xMFB, yMFB, (dir != 1) ? 2 : 0, mGraphics.BOTTOM | mGraphics.HCENTER);
				}
				return;
			}
			if (isShadown)
			{
				paintShadow(g);
			}
			g.translate(0, GameCanvas.transY);
			if (!changBody)
			{
				int num = 33;
				if (yTemp == -1)
				{
					yTemp = y;
				}
				if (TileMap.tileTypeAt(x + num, y + fy, 4))
				{
					xTempLeft = TileMap.tileXofPixel(x + num) - num;
					xTempRight = TileMap.tileXofPixel(x + num);
					if (x > xTempLeft && x < xTempRight && xTempRight != -1)
					{
						x = xTempLeft;
					}
				}
				if (y < yTemp && yTemp != -1)
				{
					yTemp = y;
					x += num;
				}
				if (y > yTemp)
				{
					yTemp = y;
					x -= num;
				}
				Mob.arrMobTemplate[templateId].data.paintFrame(g, frame, x, y + fy, (dir != 1) ? 1 : 0, 2);
			}
			else
			{
				SmallImage.drawSmallImage(g, smallBody, x, y + fy - 9, (dir != 1) ? 2 : 0, mGraphics.BOTTOM | mGraphics.HCENTER);
			}
			g.translate(0, -GameCanvas.transY);
			if (hp <= 0)
			{
				return;
			}
			int imageWidth = mGraphics.getImageWidth(imgHPtem);
			int imageHeight = mGraphics.getImageHeight(imgHPtem);
			int num2 = imageWidth;
			int num3 = x - imageWidth;
			int num4 = y - h - 5;
			int num5 = imageWidth * 2 * per / 100;
			int num6 = num5;
			if (per_tem >= per)
			{
				int num7 = per_tem;
				int num8 = ((GameCanvas.gameTick % 6 <= 3) ? offset : offset++);
				num6 = imageWidth * (per_tem = num7 - num8) / 100;
				if (per_tem <= 0)
				{
					per_tem = 0;
				}
				if (per_tem < per)
				{
					per_tem = per;
				}
				if (offset >= 3)
				{
					offset = 3;
				}
			}
			int num9;
			if (num5 > num2)
			{
				num9 = num5 - num2;
				if (num9 <= 0)
				{
					num9 = 0;
				}
			}
			else
			{
				num2 = num5;
				num9 = 0;
			}
			g.drawImage(GameScr.imgHP_tm_xam, num3, num4, mGraphics.TOP | mGraphics.LEFT);
			g.drawImage(GameScr.imgHP_tm_xam, num3 + imageWidth, num4, mGraphics.TOP | mGraphics.LEFT);
			g.setColor(16777215);
			g.fillRect(num3, num4, num6, 2);
			g.drawRegion(imgHPtem, 0, 0, num2, imageHeight, 0, num3, num4, mGraphics.TOP | mGraphics.LEFT);
			g.drawRegion(imgHPtem, 0, 0, num9, imageHeight, 0, num3 + imageWidth, num4, mGraphics.TOP | mGraphics.LEFT);
		}

		public new int getX()
		{
			return x;
		}

		public new int getY()
		{
			return y;
		}

		public new int getH()
		{
			return h;
		}

		public new int getW()
		{
			return w;
		}

		public new void stopMoving()
		{
			if (status == 5)
			{
				status = 2;
				p1 = (p2 = (p3 = 0));
				forceWait = 50;
			}
		}

		public new bool isInvisible()
		{
			if (status != 0)
			{
				return status == 1;
			}
			return true;
		}

		public new void move(short xMoveTo, short yMoveTo)
		{
			if (yMoveTo == -1)
			{
				xTo = xMoveTo;
				status = 5;
			}
			else if (Res.distance(x, y, xTo, yTo) > 100)
			{
				x = xMoveTo;
				y = yMoveTo;
				status = 2;
			}
			else
			{
				xTo = xMoveTo;
				yTo = yMoveTo;
				status = 5;
			}
		}

		public new void GetFrame()
		{
			try
			{
				frameArr = (int[][])Controller.frameHT_NEWBOSS.get(templateId + string.Empty);
				w = Mob.arrMobTemplate[templateId].data.width;
				h = Mob.arrMobTemplate[templateId].data.height;
			}
			catch (Exception)
			{
			}
		}

		public void setDie()
		{
			status = 0;
		}
	}
}
