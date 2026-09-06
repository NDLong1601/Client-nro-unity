namespace Game2
{
	public class SkillTemplate
	{
		public sbyte id;

		public int classId;

		public string name;

		public int maxPoint;

		public int manaUseType;

		public int type;

		public int iconId;

		public string[] description;

		public Skill[] skills;

		public string damInfo;

		public bool isBuffToPlayer()
		{
			return type == 2;
		}

		public bool isUseAlone()
		{
			return type == 3;
		}

		public bool isAttackSkill()
		{
			return type == 1;
		}

		public bool isSkillSpec()
		{
			return type == 4;
		}
	}
}
