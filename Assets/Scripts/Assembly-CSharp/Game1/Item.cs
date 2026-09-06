namespace Game1
{
	public class Item
	{
		public const int OPT_STAR = 34;

		public const int OPT_MOON = 35;

		public const int OPT_SUN = 36;

		public const int OPT_COLORNAME = 41;

		public const int OPT_LVITEM = 72;

		public const int OPT_STARSLOT = 102;

		public const int OPT_MAXSTARSLOT = 107;

		public const int TYPE_BODY_MIN = 0;

		public const int TYPE_BODY_MAX = 6;

		public const int TYPE_AO = 0;

		public const int TYPE_QUAN = 1;

		public const int TYPE_GANGTAY = 2;

		public const int TYPE_GIAY = 3;

		public const int TYPE_RADA = 4;

		public const int TYPE_HAIR = 5;

		public const int TYPE_DAUTHAN = 6;

		public const int TYPE_NGOCRONG = 12;

		public const int TYPE_SACH = 7;

		public const int TYPE_NHIEMVU = 8;

		public const int TYPE_GOLD = 9;

		public const int TYPE_DIAMOND = 10;

		public const int TYPE_BALO = 11;

		public const int TYPE_MOUNT = 23;

		public const int TYPE_MOUNT_VIP = 24;

		public const int TYPE_DIAMOND_LOCK = 34;

		public const int TYPE_TRAINSUIT = 32;

		public const int TYPE_HAT = 35;

		public const sbyte UI_WEAPON = 2;

		public const sbyte UI_BAG = 3;

		public const sbyte UI_BOX = 4;

		public const sbyte UI_BODY = 5;

		public const sbyte UI_STACK = 6;

		public const sbyte UI_STACK_LOCK = 7;

		public const sbyte UI_GROCERY = 8;

		public const sbyte UI_GROCERY_LOCK = 9;

		public const sbyte UI_UPGRADE = 10;

		public const sbyte UI_UPPEARL = 11;

		public const sbyte UI_UPPEARL_LOCK = 12;

		public const sbyte UI_SPLIT = 13;

		public const sbyte UI_STORE = 14;

		public const sbyte UI_BOOK = 15;

		public const sbyte UI_LIEN = 16;

		public const sbyte UI_NHAN = 17;

		public const sbyte UI_NGOCBOI = 18;

		public const sbyte UI_PHU = 19;

		public const sbyte UI_NONNAM = 20;

		public const sbyte UI_NONNU = 21;

		public const sbyte UI_AONAM = 22;

		public const sbyte UI_AONU = 23;

		public const sbyte UI_GANGTAYNAM = 24;

		public const sbyte UI_GANGTAYNU = 25;

		public const sbyte UI_QUANNAM = 26;

		public const sbyte UI_QUANNU = 27;

		public const sbyte UI_GIAYNAM = 28;

		public const sbyte UI_GIAYNU = 29;

		public const sbyte UI_TRADE = 30;

		public const sbyte UI_UPGRADE_GOLD = 31;

		public const sbyte UI_FASHION = 32;

		public const sbyte UI_CONVERT = 33;

		public ItemOption[] itemOption;

		public ItemTemplate template;

		public MyVector options;

		public int itemId;

		public int playerId;

		public bool isSelect;

		public int indexUI;

		public int quantity;

		public int quantilyToBuy;

		public long powerRequire;

		public bool isLock;

		public int sys;

		public int upgrade;

		public int buyCoin;

		public int buyCoinLock;

		public int buyGold;

		public int buyGoldLock;

		public int saleCoinLock;

		public int buySpec;

		public int buyRuby;

		public short iconSpec = -1;

		public sbyte buyType = -1;

		public int typeUI;

		public bool isExpires;

		public bool isBuySpec;

		public EffectCharPaint eff;

		public int indexEff;

		public Image img;

		public string info;

		public string content;

		public string reason = string.Empty;

		public int compare;

		public sbyte isMe;

		public bool newItem;

		public int headTemp = -1;

		public int bodyTemp = -1;

		public int legTemp = -1;

		public int bagTemp = -1;

		public int wpTemp = -1;

		private int[] color = new int[18]
		{
			0, 0, 0, 0, 600841, 600841, 667658, 667658, 3346944, 3346688,
			4199680, 5052928, 3276851, 3932211, 4587571, 5046280, 6684682, 3359744
		};

		private int[][] colorBorder = new int[5][]
		{
			new int[6] { 18687, 16869, 15052, 13235, 11161, 9344 },
			new int[6] { 45824, 39168, 32768, 26112, 19712, 13056 },
			new int[6] { 16744192, 15037184, 13395456, 11753728, 10046464, 8404992 },
			new int[6] { 13500671, 12058853, 10682572, 9371827, 7995545, 6684800 },
			new int[6] { 16711705, 15007767, 13369364, 11730962, 10027023, 8388621 }
		};

		private int[] size = new int[6] { 2, 1, 1, 1, 1, 1 };

		public void getCompare()
		{
			compare = GameCanvas.panel.getCompare(this);
		}

		public bool isHaveOption(int id)
		{
			for (int i = 0; i < this.itemOption.Length; i++)
			{
				ItemOption itemOption = this.itemOption[i];
				if (itemOption != null && itemOption.optionTemplate.id == id)
				{
					return true;
				}
			}
			return false;
		}

		public Item clone()
		{
			Item item = new Item();
			item.template = template;
			if (options != null)
			{
				item.options = new MyVector();
				for (int i = 0; i < options.size(); i++)
				{
					ItemOption itemOption = new ItemOption();
					itemOption.optionTemplate = ((ItemOption)options.elementAt(i)).optionTemplate;
					itemOption.param = ((ItemOption)options.elementAt(i)).param;
					item.options.addElement(itemOption);
				}
			}
			item.itemId = itemId;
			item.playerId = playerId;
			item.indexUI = indexUI;
			item.quantity = quantity;
			item.isLock = isLock;
			item.sys = sys;
			item.upgrade = upgrade;
			item.buyCoin = buyCoin;
			item.buyCoinLock = buyCoinLock;
			item.buyGold = buyGold;
			item.buyGoldLock = buyGoldLock;
			item.saleCoinLock = saleCoinLock;
			item.typeUI = typeUI;
			item.isExpires = isExpires;
			return item;
		}

		public bool isTypeBody()
		{
			if ((0 > template.type || template.type >= 6) && template.type != 32 && template.type != 35 && template.type != 11)
			{
				return template.type == 23;
			}
			return true;
		}

		public void setPartTemp(int headTemp, int bodyTemp, int legTemp, int bagTemp)
		{
			this.headTemp = headTemp;
			this.bodyTemp = bodyTemp;
			this.legTemp = legTemp;
			this.bagTemp = bagTemp;
		}
	}
}
