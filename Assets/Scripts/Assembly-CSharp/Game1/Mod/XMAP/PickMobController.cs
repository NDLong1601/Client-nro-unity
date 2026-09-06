using System.Collections.Generic;

namespace Game1.Mod.XMAP
{
	public class PickMobController
	{
		private enum TpyePickItem
		{
			CanNotPickItem = 0,
			PickItemNormal = 1,
			PickItemTDLT = 2,
			PickItemTanSat = 3
		}

		private static readonly sbyte[] IdSkillsMelee = new sbyte[5] { 0, 9, 2, 17, 4 };

		private static readonly sbyte[] IdSkillsCanNotAttack = new sbyte[5] { 10, 11, 14, 23, 7 };

		private static readonly PickMobController _Instance = new PickMobController();

		public static bool IsPickingItems;

		private static bool IsWait;

		private static long TimeStartWait;

		private static long TimeWait;

		public static List<ItemMap> ItemPicks = new List<ItemMap>();

		private static int IndexItemPick = 0;

		public static void Update()
		{
			if (IsWaiting())
			{
				return;
			}
			Char @char = Char.myCharz();
			if (@char.statusMe == 14 || @char.cHP <= 0)
			{
				return;
			}
			if (GameScr.hpPotion >= 1 && (@char.cHP <= @char.cHPFull * PickMob.HpBuff / 100 || @char.cMP <= @char.cMPFull * PickMob.MpBuff / 100))
			{
				GameScr.gI().doUseHP();
			}
			if (PickMob.IsAutoPickItems)
			{
				if (IsPickingItems)
				{
					if (IndexItemPick >= ItemPicks.Count)
					{
						IsPickingItems = false;
						Wait(100);
						return;
					}
					ItemMap itemMap = ItemPicks[IndexItemPick];
					if (GameScr.vItemMap.contains(itemMap))
					{
						Service.gI().pickItem(itemMap.itemMapID);
						itemMap.countAutoPick++;
					}
					Wait(500);
					IndexItemPick++;
				}
				ItemPicks.Clear();
				IndexItemPick = 0;
				for (int i = 0; i < GameScr.vItemMap.size(); i++)
				{
					ItemMap itemMap2 = (ItemMap)GameScr.vItemMap.elementAt(i);
					if (GetTypePickItem(itemMap2) != 0)
					{
						ItemPicks.Add(itemMap2);
					}
				}
				if (ItemPicks.Count > 0)
				{
					IsPickingItems = true;
					return;
				}
			}
			bool flag = ItemTime.isExistItem(4387);
			if (PickMob.tanSat)
			{
				if (@char.isCharge)
				{
					Wait(100);
					return;
				}
				if (@char.mobFocus != null && !IsMobTanSat(@char.mobFocus))
				{
					@char.mobFocus = null;
				}
				if (@char.mobFocus == null)
				{
					@char.mobFocus = GetMobTanSat();
					if (flag && @char.mobFocus != null)
					{
						if (PickMob.telePem)
						{
							if (Math.abs(@char.mobFocus.xFirst - @char.cx) >= 20 || Math.abs(@char.mobFocus.yFirst - @char.cy) >= 20)
							{
								ModFunc.GI().MoveTo(@char.mobFocus.xFirst, @char.mobFocus.yFirst);
							}
							return;
						}
						@char.cx = @char.mobFocus.xFirst;
						@char.cy = @char.mobFocus.yFirst;
						Service.gI().charMove();
					}
				}
				if (@char.mobFocus != null)
				{
					if (@char.skillInfoPaint() == null)
					{
						Skill skillAttack = GetSkillAttack();
						if (skillAttack != null && CanUseSkill(skillAttack))
						{
							Mob mobFocus = @char.mobFocus;
							mobFocus.x = mobFocus.xFirst;
							mobFocus.y = mobFocus.yFirst;
							if (Char.myCharz().myskill != skillAttack)
							{
								GameScr.gI().doSelectSkill(skillAttack, isShortcut: true);
							}
							if (Res.distance(mobFocus.xFirst, mobFocus.yFirst, @char.cx, @char.cy) <= 48)
							{
								if (GameCanvas.gameTick % 50 == 0 && Mob.arrMobTemplate[Char.myCharz().mobFocus.templateId].type == 4)
								{
									ModFunc.GI().MoveTo(mobFocus.xFirst, mobFocus.yFirst + 1);
								}
								if (skillAttack.template.isAttackSkill())
								{
									ModFunc.GI().AttackMob(mobFocus);
									ModFunc.GI().SetUsedSkill(@char.myskill);
								}
								else if (skillAttack.template.isUseAlone())
								{
									ModFunc.GI().DoDoubleClickToObj(mobFocus);
								}
							}
							else
							{
								if (PickMob.telePem)
								{
									if (Math.abs(mobFocus.xFirst - @char.cx) >= 20 || Math.abs(mobFocus.yFirst - @char.cy) >= 20)
									{
										ModFunc.GI().MoveTo(mobFocus.xFirst, mobFocus.yFirst);
									}
									return;
								}
								Move(mobFocus.xFirst, mobFocus.yFirst);
							}
						}
					}
				}
				else if (!flag)
				{
					Mob mobNext = GetMobNext();
					if (mobNext != null)
					{
						Char.myCharz().FocusManualTo(mobNext);
						if (PickMob.telePem)
						{
							if (Math.abs(mobNext.xFirst - @char.cx) >= 20 || Math.abs(mobNext.yFirst - @char.cy) >= 20)
							{
								ModFunc.GI().MoveTo(mobNext.xFirst, mobNext.yFirst);
							}
							return;
						}
						Move(mobNext.xFirst, mobNext.yFirst);
					}
				}
				Wait(100);
			}
			else
			{
				if (!PickMob.tsPlayer)
				{
					return;
				}
				if (@char.isCharge)
				{
					Wait(100);
					return;
				}
				if (@char.charFocus != null && !IsCharTanSat(@char.charFocus))
				{
					@char.charFocus = null;
				}
				if (@char.charFocus == null)
				{
					@char.charFocus = GetCharTanSat();
					if (@char.charFocus != null)
					{
						if (PickMob.telePem)
						{
							if (Math.abs(@char.charFocus.cx - @char.cx) >= 20 || Math.abs(@char.charFocus.cy - @char.cy) >= 20)
							{
								ModFunc.GI().MoveTo(@char.charFocus.cx, @char.charFocus.cy);
							}
							return;
						}
						@char.cx = @char.charFocus.cx;
						@char.cy = @char.charFocus.cy;
						Service.gI().charMove();
					}
				}
				if (@char.charFocus != null && @char.skillInfoPaint() == null)
				{
					Skill skillAttack2 = GetSkillAttack();
					if (skillAttack2 != null && !skillAttack2.paintCanNotUseSkill)
					{
						Char charFocus = @char.charFocus;
						if (@char.myskill != skillAttack2)
						{
							GameScr.gI().doSelectSkill(skillAttack2, isShortcut: true);
						}
						if (Res.distance(charFocus.cx, charFocus.cy, @char.cx, @char.cy) <= 48)
						{
							ModFunc.GI().DoDoubleClickToObj(charFocus);
						}
						else
						{
							if (PickMob.telePem)
							{
								if (Math.abs(charFocus.cx - @char.cx) >= 20 || Math.abs(charFocus.cy - @char.cy) >= 20)
								{
									ModFunc.GI().MoveTo(charFocus.cx, charFocus.cy);
								}
								return;
							}
							Move(charFocus.cx, charFocus.cy);
						}
					}
				}
				Wait(100);
			}
		}

		public static void Move(int x, int y)
		{
			Char @char = Char.myCharz();
			if (!PickMob.vuotDiaHinh)
			{
				@char.currentMovePoint = new MovePoint(x, y);
				return;
			}
			int[] pointYsdMax = GetPointYsdMax(@char.cx, x);
			if (pointYsdMax[1] >= y || (pointYsdMax[1] >= @char.cy && (@char.statusMe == 2 || @char.statusMe == 1)))
			{
				pointYsdMax[0] = x;
				pointYsdMax[1] = y;
			}
			@char.currentMovePoint = new MovePoint(pointYsdMax[0], pointYsdMax[1]);
		}

		private static TpyePickItem GetTypePickItem(ItemMap itemMap)
		{
			Char @char = Char.myCharz();
			if (itemMap.playerId != @char.charID && itemMap.playerId != -1 && !PickMob.IsPickItemsAll)
			{
				return TpyePickItem.CanNotPickItem;
			}
			if (PickMob.IsLimitTimesPickItem && itemMap.countAutoPick > PickMob.TimesAutoPickItemMax)
			{
				return TpyePickItem.CanNotPickItem;
			}
			if (!FilterItemPick(itemMap))
			{
				return TpyePickItem.CanNotPickItem;
			}
			if (PickMob.IsPickItemsDis || (Res.abs(@char.cx - itemMap.xEnd) < 100 && Res.abs(@char.cy - itemMap.yEnd) < 100))
			{
				return TpyePickItem.PickItemNormal;
			}
			if (ItemTime.isExistItem(4387))
			{
				return TpyePickItem.PickItemTDLT;
			}
			if (PickMob.tanSat)
			{
				return TpyePickItem.PickItemTanSat;
			}
			return TpyePickItem.CanNotPickItem;
		}

		private static bool FilterItemPick(ItemMap itemMap)
		{
			if ((PickMob.IdItemPicks.Count == 0 || PickMob.IdItemPicks.Contains(itemMap.template.id)) && (PickMob.IdItemBlocks.Count == 0 || !PickMob.IdItemBlocks.Contains(itemMap.template.id)) && (PickMob.TypeItemPicks.Count == 0 || PickMob.TypeItemPicks.Contains(itemMap.template.type)))
			{
				if (PickMob.TypeItemBlock.Count != 0)
				{
					return !PickMob.TypeItemBlock.Contains(itemMap.template.type);
				}
				return true;
			}
			return false;
		}

		private static Mob GetMobTanSat()
		{
			Mob result = null;
			int num = int.MaxValue;
			Char @char = Char.myCharz();
			for (int i = 0; i < GameScr.vMob.size(); i++)
			{
				Mob mob = (Mob)GameScr.vMob.elementAt(i);
				int num2 = (mob.xFirst - @char.cx) * (mob.xFirst - @char.cx) + (mob.yFirst - @char.cy) * (mob.yFirst - @char.cy);
				if (IsMobTanSat(mob) && num2 < num)
				{
					result = mob;
					num = num2;
				}
			}
			return result;
		}

		private static Char GetCharTanSat()
		{
			Char result = null;
			for (int i = 0; i < GameScr.vCharInMap.size(); i++)
			{
				Char @char = (Char)GameScr.vCharInMap.elementAt(i);
				if (IsCharTanSat(@char))
				{
					result = @char;
				}
			}
			return result;
		}

		private static Mob GetMobNext()
		{
			Mob result = null;
			long num = mSystem.currentTimeMillis();
			for (int i = 0; i < GameScr.vMob.size(); i++)
			{
				Mob mob = (Mob)GameScr.vMob.elementAt(i);
				if (IsMobNext(mob) && mob.lastDie < num)
				{
					result = mob;
					num = mob.lastDie;
				}
			}
			return result;
		}

		private static bool IsMobTanSat(Mob mob)
		{
			if (mob.status == 0 || mob.status == 1 || mob.hp <= 0 || mob.isMobMe)
			{
				return false;
			}
			bool flag = PickMob.neSieuQuai && !ItemTime.isExistItem(4387);
			return (mob.levelBoss == 0 || !flag) && FilterMobTanSat(mob);
		}

		private static bool IsCharTanSat(Char c)
		{
			return Char.myCharz().isMeCanAttackOtherPlayer(c);
		}

		private static bool IsMobNext(Mob mob)
		{
			if (mob.isMobMe)
			{
				return false;
			}
			if (!FilterMobTanSat(mob))
			{
				return false;
			}
			if (PickMob.neSieuQuai && !ItemTime.isExistItem(4387) && mob.getTemplate().hp >= 3000)
			{
				if (mob.levelBoss != 0)
				{
					Mob mob2 = null;
					bool flag = false;
					for (int i = 0; i < GameScr.vMob.size(); i++)
					{
						mob2 = (Mob)GameScr.vMob.elementAt(i);
						if (mob2.countDie == 10 && (mob2.status == 0 || mob2.status == 1))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
					mob.lastDie = mob2.lastDie;
				}
				else if (mob.countDie == 10 && (mob.status == 0 || mob.status == 1))
				{
					return false;
				}
			}
			return true;
		}

		private static bool FilterMobTanSat(Mob mob)
		{
			if ((PickMob.IdMobsTanSat.Count == 0 || PickMob.IdMobsTanSat.Contains(mob.mobId)) && (PickMob.TypeMobsTanSat.Count == 0 || PickMob.TypeMobsTanSat.Contains(mob.templateId)))
			{
				return !mob.isMobMe;
			}
			return false;
		}

		private static Skill GetSkillAttack()
		{
			Skill skill = null;
			SkillTemplate skillTemplate = new SkillTemplate();
			foreach (sbyte item in PickMob.IdSkillsTanSat)
			{
				skillTemplate.id = item;
				Skill skill2 = Char.myCharz().getSkill(skillTemplate);
				if (IsSkillBetter(skill2, skill))
				{
					skill = skill2;
				}
			}
			return skill;
		}

		private static Skill GetSkillAttack2()
		{
			Skill skill = null;
			Skill[] array = (GameCanvas.isTouch ? GameScr.onScreenSkill : GameScr.keySkill);
			foreach (Skill skill2 in array)
			{
				if (skill2 != null && skill2.template.id != 10 && skill2.template.id != 11 && skill2.template.id != 14 && skill2.template.id != 23 && skill2.template.id != 27 && skill2.template.id != 28 && !skill2.template.isBuffToPlayer() && !skill2.template.isSkillSpec() && IsSkillBetter(skill2, skill))
				{
					skill = skill2;
				}
			}
			return skill;
		}

		private static bool IsSkillBetter(Skill SkillBetter, Skill skill)
		{
			if (SkillBetter == null)
			{
				return false;
			}
			if (skill == null)
			{
				return true;
			}
			if (!CanUseSkill(SkillBetter))
			{
				return false;
			}
			if (!CanUseSkill(skill))
			{
				return true;
			}
			bool flag = (SkillBetter.template.id == 17 && skill.template.id == 2) || (SkillBetter.template.id == 9 && skill.template.id == 0);
			return skill.coolDown < SkillBetter.coolDown || flag;
		}

		private static bool CanUseSkill(Skill skill)
		{
			if (mSystem.currentTimeMillis() - skill.lastTimeUseThisSkill > (long)skill.coolDown + 25L)
			{
				skill.paintCanNotUseSkill = false;
			}
			if (!skill.paintCanNotUseSkill)
			{
				return Char.myCharz().cMP >= GetManaUseSkill(skill);
			}
			return false;
		}

		private static long GetManaUseSkill(Skill skill)
		{
			if (skill.template.manaUseType == 2)
			{
				return 1L;
			}
			if (skill.template.manaUseType == 1)
			{
				return skill.manaUse * Char.myCharz().cMPFull / 100;
			}
			return skill.manaUse;
		}

		public static int GetYsd(int xsd)
		{
			Char @char = Char.myCharz();
			int num = TileMap.pxh;
			int result = -1;
			for (int i = 24; i < TileMap.pxh; i += 24)
			{
				if (TileMap.tileTypeAt(xsd, i, 2))
				{
					int num2 = Res.abs(i - @char.cy);
					if (num2 < num)
					{
						num = num2;
						result = i;
					}
				}
			}
			return result;
		}

		private static int[] GetPointYsdMax(int xStart, int xEnd)
		{
			int num = TileMap.pxh;
			int num2 = -1;
			if (xStart > xEnd)
			{
				for (int i = xEnd; i < xStart; i += 24)
				{
					int ysd = GetYsd(i);
					if (ysd < num)
					{
						num = ysd;
						num2 = i;
					}
				}
			}
			else
			{
				for (int num3 = xEnd; num3 > xStart; num3 -= 24)
				{
					int ysd2 = GetYsd(num3);
					if (ysd2 < num)
					{
						num = ysd2;
						num2 = num3;
					}
				}
			}
			return new int[2] { num2, num };
		}

		public static void Wait(int time)
		{
			IsWait = true;
			TimeStartWait = mSystem.currentTimeMillis();
			TimeWait = time;
		}

		public static bool IsWaiting()
		{
			if (IsWait && mSystem.currentTimeMillis() - TimeStartWait >= TimeWait)
			{
				IsWait = false;
			}
			return IsWait;
		}
	}
}
