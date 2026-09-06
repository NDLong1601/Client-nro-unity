using System.Collections.Generic;

namespace Game1.Mod.XMAP
{
	public struct GroupMap
	{
		public string NameGroup;

		public List<int> IdMaps;

		public GroupMap(string nameGroup, List<int> idMaps)
		{
			NameGroup = nameGroup;
			IdMaps = idMaps;
		}
	}
}
