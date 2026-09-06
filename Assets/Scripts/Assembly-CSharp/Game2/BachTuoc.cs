using System;

namespace Game2
{
	public class BachTuoc : Mob, IMapObject
	{
		public static Image shadowBig = GameCanvas.loadImage("/mainImage/shadowBig.png");

		public static EffectData data;

		public int xTo;

		public int yTo;

		public new int xSd;

		public new int ySd;

		private bool isOutMap;

		private int wCount;

		public new bool isShadown = true;

		private int tick;

		private int frame;

		public new static Image imgHP = GameCanvas.loadImage("/mainImage/myTexture2dmobHP.png");

		private int fy;

		private int tShock;

		public new bool isBusyAttackSomeOne = true;

		private Char[] charAttack;

		private int[] dameHP;

		private sbyte type;

		public new int[] stand = new int[12]
		{
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
			1, 1
		};

		public int[] movee = new int[12]
		{
			0, 0, 0, 2, 2, 2, 3, 3, 3, 4,
			4, 4
		};

		public new int[] attack1 = new int[12]
		{
			0, 0, 0, 4, 4, 4, 5, 5, 5, 6,
			6, 6
		};

		public new int[] attack2 = new int[17]
		{
			0, 0, 0, 7, 7, 7, 8, 8, 8, 9,
			9, 9, 10, 10, 10, 11, 11
		};

		public new int[] hurt = new int[4] { 1, 1, 7, 7 };

		private bool shock;

		private sbyte[] cou = new sbyte[2] { -1, 1 };

		public new int forceWait;

		public BachTuoc(int id, short px, short py, int templateID, long hp, long maxHp, int s)
		{
			mobId = id;
			xFirst = (x = px + 20);
			y = py;
			yFirst = py;
			xTo = x;
			yTo = y;
			base.maxHp = maxHp;
			base.hp = hp;
			templateId = templateID;
			w_hp_bar = 100;
			h_hp_bar = 6;
			len = w_hp_bar;
			updateHp_bar();
			getDataB();
			status = 2;
		}

		public void getDataB()
		{
			data = null;
			data = new EffectData();
			string patch = "/x" + mGraphics.zoomLevel + "/effectdata/" + 108 + "/data";
			try
			{
				data.readData2(patch);
				data.img = GameCanvas.loadImage("/effectdata/" + 108 + "/img.png");
			}
			catch (Exception)
			{
				Service.gI().requestModTemplate(templateId);
			}
			w = data.width;
			h = data.height;
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

		private void updateShadown()
		{
			int size = TileMap.size;
			xSd = x;
			wCount = 0;
			if (ySd <= 0 || TileMap.tileTypeAt(xSd, ySd, 2))
			{
				return;
			}
			if (TileMap.tileTypeAt(xSd / size, ySd / size) == 0)
			{
				isOutMap = true;
			}
			else if (TileMap.tileTypeAt(xSd / size, ySd / size) != 0 && !TileMap.tileTypeAt(xSd, ySd, 2))
			{
				xSd = x;
				ySd = y;
				isOutMap = false;
			}
			while (isOutMap && wCount < 10)
			{
				wCount++;
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
			_ = TileMap.size;
			g.drawImage(shadowBig, xSd, yFirst, 3);
			g.setClip(GameScr.cmx, GameScr.cmy - GameCanvas.transY, GameScr.gW, GameScr.gH + 2 * GameCanvas.transY);
		}

		public override void update()
		{
			if (!isUpdate())
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
				break;
			case 4:
				break;
			}
		}

		private void updateDead()
		{
			checkFrameTick(stand);
			if (GameCanvas.gameTick % 5 == 0)
			{
				ServerEffect.addServerEffect(167, Res.random(x - getW() / 2, x + getW() / 2), Res.random(getY() + getH() / 2, getY() + getH()), 1);
			}
			if (x != xTo || y != yTo)
			{
				x += (xTo - x) / 4;
				y += (yTo - y) / 4;
			}
		}

		private void updateInjure()
		{
		}

		private void updateMobStandWait()
		{
			checkFrameTick(stand);
			if (x != xTo || y != yTo)
			{
				x += (xTo - x) / 4;
				y += (yTo - y) / 4;
			}
		}

		public void setAttack(Char[] cAttack, int[] dame, sbyte type)
		{
			charAttack = cAttack;
			dameHP = dame;
			this.type = type;
			status = 3;
		}

		public new void updateMobAttack()
		{
			if (type == 3)
			{
				if (tick == attack1.Length - 1)
				{
					status = 2;
				}
				dir = ((x < charAttack[0].cx) ? 1 : (-1));
				checkFrameTick(attack1);
				x += (charAttack[0].cx - x) / 4;
				y += (charAttack[0].cy - y) / 4;
				xTo = x;
				if (tick == 8)
				{
					for (int i = 0; i < charAttack.Length; i++)
					{
						charAttack[i].doInjure(dameHP[i], 0L, isCrit: false, isMob: false);
						ServerEffect.addServerEffect(102, charAttack[i].cx, charAttack[i].cy, 1);
					}
				}
			}
			if (type != 4)
			{
				return;
			}
			if (tick == attack2.Length - 1)
			{
				status = 2;
			}
			dir = ((x < charAttack[0].cx) ? 1 : (-1));
			checkFrameTick(attack2);
			if (tick == 8)
			{
				for (int j = 0; j < charAttack.Length; j++)
				{
					charAttack[j].doInjure(dameHP[j], 0L, isCrit: false, isMob: false);
					ServerEffect.addServerEffect(102, charAttack[j].cx, charAttack[j].cy, 1);
				}
			}
		}

		public new void updateMobWalk()
		{
			checkFrameTick(movee);
			x += ((x >= xTo) ? (-2) : 2);
			y = yTo;
			dir = ((x < xTo) ? 1 : (-1));
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
			if (data == null || isHide)
			{
				return;
			}
			if (!isMafuba)
			{
				if (isShadown && status != 0)
				{
					paintShadow(g);
				}
				g.translate(0, GameCanvas.transY);
				if (!changBody)
				{
					data.paintFrame(g, frame, x, y + fy, (dir != 1) ? 1 : 0, 2);
				}
				else
				{
					SmallImage.drawSmallImage(g, smallBody, x, y + fy - 9, (dir != 1) ? 2 : 0, mGraphics.BOTTOM | mGraphics.HCENTER);
				}
				g.translate(0, -GameCanvas.transY);
				int imageWidth = mGraphics.getImageWidth(imgHPtem);
				int imageHeight = mGraphics.getImageHeight(imgHPtem);
				int num = imageWidth;
				int num2 = x - imageWidth;
				int num3 = y - h - 5;
				int num4 = imageWidth * 2 * per / 100;
				int num5;
				if (num4 > num)
				{
					num5 = num4 - num;
					if (num5 <= 0)
					{
						num5 = 0;
					}
				}
				else
				{
					num = num4;
					num5 = 0;
				}
				g.drawImage(GameScr.imgHP_tm_xam, num2, num3, mGraphics.TOP | mGraphics.LEFT);
				g.drawImage(GameScr.imgHP_tm_xam, num2 + imageWidth, num3, mGraphics.TOP | mGraphics.LEFT);
				g.drawRegion(imgHPtem, 0, 0, num, imageHeight, 0, num2, num3, mGraphics.TOP | mGraphics.LEFT);
				g.drawRegion(imgHPtem, 0, 0, num5, imageHeight, 0, num2 + imageWidth, num3, mGraphics.TOP | mGraphics.LEFT);
				if (shock)
				{
					tShock++;
					EffecMn.addEff(new Effect((type != 2) ? 22 : 19, x + tShock * 50, y + 25, 2, 1, -1));
					EffecMn.addEff(new Effect((type != 2) ? 22 : 19, x - tShock * 50, y + 25, 2, 1, -1));
					if (tShock == 50)
					{
						tShock = 0;
						shock = false;
					}
				}
			}
			else if (!changBody)
			{
				data.paintFrame(g, frame, xMFB, yMFB, (dir != 1) ? 1 : 0, 2);
			}
			else
			{
				SmallImage.drawSmallImage(g, smallBody, xMFB, yMFB, (dir != 1) ? 2 : 0, mGraphics.BOTTOM | mGraphics.HCENTER);
			}
		}

		public new int getX()
		{
			return x;
		}

		public new int getY()
		{
			return y - 40;
		}

		public new int getH()
		{
			return 40;
		}

		public new int getW()
		{
			return 40;
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

		public new void move(short xMoveTo)
		{
			xTo = xMoveTo;
			status = 5;
		}
	}
}
