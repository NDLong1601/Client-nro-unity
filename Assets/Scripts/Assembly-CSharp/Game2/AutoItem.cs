using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Game2
{
	internal class AutoItem : IChatable
	{
		internal static List<AutoItem> listItemUses = new List<AutoItem>();

		internal int ID { get; set; }

		internal int Quantity { get; set; }

		internal int timeDelay { get; set; }

		internal sbyte indexUI { get; set; }

		internal bool isGold { get; set; }

		internal bool isGem
		{
			[CompilerGenerated]
			set
			{
			}
		}

		internal long lastTimeUseItem { get; set; }

		internal long lastTimeBuy { get; set; }

		internal static AutoItem itemToAuto { get; set; }

		internal static AutoItem instance { get; set; }

		internal static AutoItem gI()
		{
			AutoItem result;
			if ((result = instance) == null)
			{
				result = (instance = new AutoItem());
			}
			return result;
		}

		internal AutoItem()
		{
		}

		internal AutoItem(int ID, sbyte indexUI, int timeDelay, bool isGold = false, bool isGem = false)
		{
			this.ID = ID;
			this.indexUI = indexUI;
			this.timeDelay = timeDelay;
			this.isGold = isGold;
			this.isGem = isGem;
		}

		internal static void Update()
		{
			AutoUse();
		}

		private static void AutoUse()
		{
			if (listItemUses.Count == 0 || GameCanvas.gameTick % 20 != 0)
			{
				return;
			}
			AutoItem item = listItemUses.FirstOrDefault((AutoItem i) => mSystem.currentTimeMillis() - i.lastTimeUseItem > i.timeDelay * 1000);
			if (item != null)
			{
				Item item2 = Char.myCharz().arrItemBag.FirstOrDefault((Item i) => i != null && i.indexUI == item.indexUI);
				if (item2 != null && item2.template.id == item.ID)
				{
					item.lastTimeUseItem = mSystem.currentTimeMillis();
					Service.gI().useItem(0, 1, item.indexUI, -1);
				}
				else
				{
					listItemUses.Remove(item);
					GameScr.info1.addInfo("Đã ngưng sử dụng " + ItemTemplates.get((short)item.ID).name, 0);
				}
			}
		}

		private static void AutoBuy(AutoItem item)
		{
		}

		internal void addItemBuys(AutoItem item)
		{
			GameCanvas.panel.hideNow();
			itemToAuto = item;
			string name = ItemTemplates.get((short)item.ID).name;
			ChatTextField.gI().strChat = "Auto Mua " + name;
			ChatTextField.gI().tfChat.name = "Nhập số lượng";
			ChatTextField.gI().tfChat.setIputType(TField.INPUT_TYPE_NUMERIC);
			ChatTextField.gI().startChat(this, string.Empty);
		}

		public void onChatFromMe(string text, string to)
		{
			if (ChatTextField.gI().strChat.StartsWith("Auto Sử Dụng"))
			{
				if (int.TryParse(text, out var result))
				{
					GameScr.info1.addInfo($"Auto: {ItemTemplates.get((short)ID).name} [{result} giây]", 0);
					listItemUses.Add(new AutoItem(ID, indexUI, result));
				}
				else
				{
					GameCanvas.startOKDlg("Invaild Value!");
				}
			}
			else if (ChatTextField.gI().strChat.StartsWith("Auto Mua"))
			{
				if (int.TryParse(text, out var result2))
				{
					itemToAuto.Quantity = result2;
					AutoBuy(itemToAuto);
				}
				else
				{
					GameCanvas.startOKDlg("Invaild Value!");
				}
			}
			onCancelChat();
		}

		internal static void ResetTF(ChatTextField tf)
		{
			tf.strChat = "Chat";
			tf.tfChat.name = "chat";
			tf.to = "";
			tf.tfChat.setIputType(TField.INPUT_TYPE_ANY);
			tf.isShow = false;
			tf.parentScreen = GameScr.gI();
		}

		public void onCancelChat()
		{
			ResetTF(ChatTextField.gI());
		}
	}
}
