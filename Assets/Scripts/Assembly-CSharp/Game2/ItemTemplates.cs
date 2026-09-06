namespace Game2
{
	public class ItemTemplates
	{
		public static MyHashTable itemTemplates = new MyHashTable();

		public static void add(ItemTemplate it)
		{
			itemTemplates.put(it.id, it);
		}

		public static ItemTemplate get(short id)
		{
			return (ItemTemplate)itemTemplates.get(id);
		}
	}
}
