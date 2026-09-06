using System.Collections.Generic;

namespace Game1.Mod.XMAP
{
	internal class GroupMapData
	{
		public List<int> datamap;

		public string mapname;

		public GroupMapData(List<int> var1, string var2)
		{
			datamap = var1;
			mapname = var2;
		}
	}
}
