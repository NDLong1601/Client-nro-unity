using System;
using Game2.Assets.src.g;

namespace Game2
{
	public class Service
	{
		private ISession session = Session_ME.gI();

		protected static Service instance;

		public static long curCheckController;

		public static long curCheckMap;

		public static long logController;

		public static long logMap;

		public static bool reciveFromMainSession;

		public static Service gI()
		{
			if (instance == null)
			{
				instance = new Service();
			}
			return instance;
		}

		public void sendFishingQuickTimeInput(int token, sbyte stage, sbyte direction)
		{
			Message message = null;
			try
			{
				message = new Message(126);
				message.writer().writeByte(81);
				message.writer().writeByte(1);
				message.writer().writeInt(token);
				message.writer().writeByte(stage);
				message.writer().writeByte(direction);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}

		public void gotoPlayer(int id)
		{
			Message message = null;
			try
			{
				message = new Message(18);
				message.writer().writeInt(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void androidPack()
		{
			if (mSystem.android_pack == null)
			{
				return;
			}
			Message message = null;
			try
			{
				message = new Message(126);
				message.writer().writeUTF(mSystem.android_pack);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void charInfo(string day, string month, string year, string address, string cmnd, string dayCmnd, string noiCapCmnd, string sdt, string name)
		{
			Message message = null;
			try
			{
				message = new Message(42);
				message.writer().writeUTF(day);
				message.writer().writeUTF(month);
				message.writer().writeUTF(year);
				message.writer().writeUTF(address);
				message.writer().writeUTF(cmnd);
				message.writer().writeUTF(dayCmnd);
				message.writer().writeUTF(noiCapCmnd);
				message.writer().writeUTF(sdt);
				message.writer().writeUTF(name);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void androidPack2()
		{
			if (mSystem.android_pack == null)
			{
				return;
			}
			Message message = null;
			try
			{
				message = new Message(126);
				message.writer().writeUTF(mSystem.android_pack);
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void combine(sbyte action, MyVector id)
		{
			Res.outz("combine");
			Message message = null;
			try
			{
				message = new Message(-81);
				message.writer().writeByte(action);
				if (action == 1)
				{
					message.writer().writeByte(id.size());
					for (int i = 0; i < id.size(); i++)
					{
						message.writer().writeByte(((Item)id.elementAt(i)).indexUI);
						Res.outz("gui id " + ((Item)id.elementAt(i)).indexUI);
					}
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void giaodich(sbyte action, int playerID, sbyte index, int num)
		{
			Res.outz2("giao dich action = " + action);
			Message message = null;
			try
			{
				message = new Message(-86);
				message.writer().writeByte(action);
				if (action == 0 || action == 1)
				{
					Res.outz2(">>>> len playerID =" + playerID);
					message.writer().writeInt(playerID);
				}
				if (action == 2)
				{
					Res.outz2("gui len index =" + index + " num= " + num);
					message.writer().writeByte(index);
					message.writer().writeInt(num);
				}
				if (action == 4)
				{
					Res.outz2(">>>> len index =" + index);
					message.writer().writeByte(index);
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void sendClientInput(TField[] t)
		{
			Message message = null;
			try
			{
				Res.outz(" gui input ");
				message = new Message(-125);
				Res.outz("byte lent = " + t.Length);
				message.writer().writeByte(t.Length);
				for (int i = 0; i < t.Length; i++)
				{
					message.writer().writeUTF(t[i].getText());
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void speacialSkill(sbyte index)
		{
			Message message = null;
			try
			{
				message = new Message(112);
				message.writer().writeByte(index);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void mobCapcha(char ch)
		{
			Res.outz("cap char c= " + ch);
			Message message = null;
			try
			{
				message = new Message(-85);
				message.writer().writeChar(ch);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void friend(sbyte action, int playerId)
		{
			Res.outz("add friend");
			Message message = null;
			try
			{
				message = new Message(-80);
				message.writer().writeByte(action);
				if (playerId != -1)
				{
					message.writer().writeInt(playerId);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getArchivemnt(int index)
		{
			Res.outz("get ngoc");
			Message message = null;
			try
			{
				message = new Message(-76);
				message.writer().writeByte(index);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getPlayerMenu(int playerID)
		{
			Message message = null;
			try
			{
				message = new Message(-79);
				message.writer().writeInt(playerID);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clanImage(sbyte id)
		{
			Message message = null;
			try
			{
				message = new Message(-62);
				message.writer().writeByte(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void skill_not_focus(sbyte status)
		{
			Message message = null;
			try
			{
				message = new Message(-45);
				message.writer().writeByte(status);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clanDonate(int id)
		{
			Message message = null;
			try
			{
				message = new Message(-54);
				message.writer().writeInt(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clanTreasuryView()
		{
			sendClanTreasury(ClanTreasury.REQUEST_VIEW, 0L, 0L, null);
		}

		public void clanTreasuryDeposit(sbyte currency, long amount, string requestId)
		{
			sbyte action = (currency == ClanTreasury.CURRENCY_GEM) ? ClanTreasury.REQUEST_DEPOSIT_GEM : ClanTreasury.REQUEST_DEPOSIT_GOLD;
			sendClanTreasury(action, amount, 0L, requestId);
		}

		public void clanTreasuryLedger(long cursor)
		{
			sendClanTreasury(ClanTreasury.REQUEST_LEDGER_PAGE, 0L, cursor, null);
		}

		private void sendClanTreasury(sbyte action, long amount, long cursor, string requestId)
		{
			Message message = null;
			try
			{
				message = new Message(127);
				message.writer().writeByte(action);
				if (action == ClanTreasury.REQUEST_DEPOSIT_GOLD || action == ClanTreasury.REQUEST_DEPOSIT_GEM)
				{
					message.writer().writeLong(amount);
					message.writer().writeUTF(requestId ?? string.Empty);
				}
				else if (action == ClanTreasury.REQUEST_LEDGER_PAGE)
				{
					message.writer().writeLong(cursor);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}

		public void clanTree(sbyte action)
		{
			Message message = null;
			try
			{
				message = new Message(127);
				message.writer().writeByte(action);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}

		public void clanProgression(sbyte action, sbyte branch = 0)
		{
			Message message = null;
			try
			{
				message = new Message(127);
				message.writer().writeByte(action);
				if (action == ClanProgression.REQUEST_ALLOCATE)
				{
					message.writer().writeByte(branch);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}

		public void clanShopView()
		{
			sendClanShop(ClanShop.REQUEST_VIEW, 0, null);
		}

		public void clanItemStorageView()
		{
			clanItemStorage(121, 0);
		}

		public void clanItemStorageUse(int slot)
		{
			clanItemStorage(122, slot);
		}

		private void clanItemStorage(sbyte action, int slot)
		{
			Message message = null;
			try
			{
				message = new Message(127);
				message.writer().writeByte(action);
				if (action == 122)
				{
					message.writer().writeByte(slot);
				}
				session.sendMessage(message);
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}

		public void clanShopRestock(int itemId, string requestId)
		{
			sendClanShop(ClanShop.REQUEST_RESTOCK, itemId, requestId);
		}

		public void clanShopBuy(int itemId, string requestId)
		{
			sendClanShop(ClanShop.REQUEST_BUY, itemId, requestId);
		}

		public void clanGift(long receiverId, string requestId)
		{
			Message message = null;
			try
			{
				message = new Message(127);
				message.writer().writeByte(120);
				message.writer().writeLong(receiverId);
				message.writer().writeUTF(requestId ?? string.Empty);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				if (message != null) message.cleanup();
			}
		}

		private void sendClanShop(sbyte action, int itemId, string requestId)
		{
			Message message = null;
			try
			{
				message = new Message(127);
				message.writer().writeByte(action);
				if (action == ClanShop.REQUEST_RESTOCK || action == ClanShop.REQUEST_BUY)
				{
					message.writer().writeInt(itemId);
					message.writer().writeUTF(requestId ?? string.Empty);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				if (message != null) message.cleanup();
			}
		}

		public void clanMessage(int type, string text, int clanID)
		{
			Message message = null;
			try
			{
				message = new Message(-51);
				message.writer().writeByte(type);
				if (type == 0)
				{
					message.writer().writeUTF(text);
				}
				if (type == 2)
				{
					message.writer().writeInt(clanID);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void shareClanLocation()
		{
			clanMessage(3, null, -1);
		}

		public void useItem(sbyte type, sbyte where, sbyte index, short template)
		{
			Cout.println("USE ITEM! " + type);
			if (Char.myCharz().statusMe == 14)
			{
				return;
			}
			Message message = null;
			try
			{
				message = new Message(-43);
				message.writer().writeByte(type);
				message.writer().writeByte(where);
				message.writer().writeByte(index);
				if (index == -1)
				{
					message.writer().writeShort(template);
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void joinClan(int id, sbyte action)
		{
			Message message = null;
			try
			{
				message = new Message(-49);
				message.writer().writeInt(id);
				message.writer().writeByte(action);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clanMember(int id)
		{
			Message message = null;
			try
			{
				message = new Message(-50);
				message.writer().writeInt(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void searchClan(string text)
		{
			Message message = null;
			try
			{
				message = new Message(-47);
				message.writer().writeUTF(text);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clanRemote(int id, sbyte role)
		{
			Message message = null;
			try
			{
				message = new Message(-56);
				message.writer().writeInt(id);
				message.writer().writeByte(role);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void leaveClan()
		{
			Message message = null;
			try
			{
				message = new Message(-55);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clanInvite(sbyte action, int playerID, int clanID, int code)
		{
			Message message = null;
			try
			{
				message = new Message(-57);
				message.writer().writeByte(action);
				if (action == 0)
				{
					message.writer().writeInt(playerID);
				}
				if (action == 1 || action == 2)
				{
					message.writer().writeInt(clanID);
					message.writer().writeInt(code);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getClan(sbyte action, sbyte id, string text)
		{
			Message message = null;
			try
			{
				message = new Message(-46);
				message.writer().writeByte(action);
				if (action == 2 || action == 4)
				{
					message.writer().writeByte(id);
				}
				if (action == 2 || action == 4 || action == 5)
				{
					message.writer().writeUTF(text);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void updateCaption(sbyte gender)
		{
			Message message = null;
			try
			{
				message = new Message(-41);
				message.writer().writeByte(gender);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getItem(sbyte type, sbyte id)
		{
			Message message = null;
			try
			{
				message = new Message(-40);
				message.writer().writeByte(type);
				message.writer().writeByte(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public Message messageNotLogin(sbyte command)
		{
			Message message = new Message(-29);
			message.writer().writeByte(command);
			return message;
		}

		public Message messageNotMap(sbyte command)
		{
			Message message = new Message(-28);
			message.writer().writeByte(command);
			return message;
		}

		public static Message messageSubCommand(sbyte command)
		{
			Message message = new Message(-30);
			message.writer().writeByte(command);
			return message;
		}

		public void setClientType()
		{
			if (Rms.loadRMSInt("clienttype") != -1)
			{
				Main.typeClient = Rms.loadRMSInt("clienttype");
			}
			try
			{
				Message message = messageNotLogin(2);
				message.writer().writeByte(Main.typeClient);
				message.writer().writeByte(mGraphics.zoomLevel);
				message.writer().writeBoolean(value: false);
				message.writer().writeInt(GameCanvas.w);
				message.writer().writeInt(GameCanvas.h);
				message.writer().writeBoolean(TField.isQwerty);
				message.writer().writeBoolean(GameCanvas.isTouch);
				message.writer().writeUTF(GameCanvas.getPlatformName() + "|" + GameMidlet.VERSION);
				DataInputStream dataInputStream = MyStream.readFile("/info");
				if (dataInputStream != null)
				{
					sbyte[] data = new sbyte[dataInputStream.r.buffer.Length];
					dataInputStream.read(ref data);
					if (data != null)
					{
						message.writer().writeShort(data.Length);
						message.writer().write(data);
						Res.err("write " + data.Length + "|" + GameMidlet.VERSION);
					}
				}
				session.sendMessage(message);
				message.cleanup();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			SendRemoteAddress();
		}

		public void setClientType2()
		{
			Res.outz("SET CLIENT TYPE");
			if (Rms.loadRMSInt("clienttype") != -1)
			{
				mSystem.clientType = Rms.loadRMSInt("clienttype");
			}
			try
			{
				Res.outz("setType");
				Message message = messageNotLogin(2);
				message.writer().writeByte(mSystem.clientType);
				message.writer().writeByte(mGraphics.zoomLevel);
				Res.outz("gui zoomlevel = " + mGraphics.zoomLevel);
				message.writer().writeBoolean(value: false);
				message.writer().writeInt(GameCanvas.w);
				message.writer().writeInt(GameCanvas.h);
				message.writer().writeBoolean(TField.isQwerty);
				message.writer().writeBoolean(GameCanvas.isTouch);
				message.writer().writeUTF(GameCanvas.getPlatformName() + "|" + GameMidlet.VERSION);
				DataInputStream dataInputStream = MyStream.readFile("/info");
				if (dataInputStream != null)
				{
					sbyte[] data = new sbyte[dataInputStream.r.buffer.Length];
					dataInputStream.read(ref data);
					if (data != null)
					{
						message.writer().writeShort(data.Length);
						message.writer().write(data);
						Res.err("write " + data.Length + "|" + GameMidlet.VERSION);
					}
				}
				session = Session_ME2.gI();
				session.sendMessage(message);
				session = Session_ME.gI();
				message.cleanup();
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
		}

		public void sendCheckController()
		{
			Message message = null;
			try
			{
				message = new Message(-120);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				curCheckController = mSystem.currentTimeMillis();
				message.cleanup();
			}
		}

		public void sendCheckMap()
		{
			Message message = null;
			try
			{
				message = new Message(-121);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				curCheckMap = mSystem.currentTimeMillis();
				message.cleanup();
			}
		}

		public void login(string username, string pass, string version, sbyte type)
		{
			try
			{
				Message message = messageNotLogin(0);
				message.writer().writeUTF(username);
				message.writer().writeUTF(pass);
				message.writer().writeUTF(version);
				message.writer().writeByte(type);
				session.sendMessage(message);
				message.cleanup();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
		}

		public void requestRegister(string username, string pass, string usernameAo, string passAo, string version)
		{
			try
			{
				Message message = messageNotLogin(1);
				message.writer().writeUTF(username);
				message.writer().writeUTF(pass);
				if (usernameAo != null && !usernameAo.Equals(string.Empty))
				{
					message.writer().writeUTF(usernameAo);
					message.writer().writeUTF("a");
				}
				session.sendMessage(message);
				message.cleanup();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
		}

		public void requestChangeMap()
		{
			Message message = new Message(-23);
			session.sendMessage(message);
			message.cleanup();
		}

		public void magicTree(sbyte type)
		{
			Message message = new Message(-34);
			try
			{
				message.writer().writeByte(type);
				session.sendMessage(message);
				message.cleanup();
			}
			catch (Exception)
			{
			}
		}

		public void requestChangeZone(int zoneId, int indexUI)
		{
			Message message = new Message(21);
			try
			{
				message.writer().writeByte(zoneId);
				session.sendMessage(message);
				message.cleanup();
			}
			catch (Exception)
			{
			}
		}

		public void checkMMove(int second)
		{
			Message message = new Message(-78);
			try
			{
				message.writer().writeInt(second);
				session.sendMessage(message);
				message.cleanup();
			}
			catch (Exception)
			{
			}
		}

		public void charMove()
		{
			int num = Char.myCharz().cx - Char.myCharz().cxSend;
			int num2 = Char.myCharz().cy - Char.myCharz().cySend;
			if (Char.ischangingMap || (num == 0 && num2 == 0) || Controller.isStopReadMessage || Char.myCharz().isTeleport || Char.myCharz().cy <= 0 || Char.myCharz().telePortSkill)
			{
				return;
			}
			try
			{
				Message message = new Message(-7);
				Char.myCharz().cxSend = Char.myCharz().cx;
				Char.myCharz().cySend = Char.myCharz().cy;
				Char.myCharz().cdirSend = Char.myCharz().cdir;
				Char.myCharz().cactFirst = Char.myCharz().statusMe;
				if (TileMap.tileTypeAt(Char.myCharz().cx / TileMap.size, Char.myCharz().cy / TileMap.size) == 0)
				{
					message.writer().writeByte(1);
					if (Char.myCharz().canFly)
					{
						if (!Char.myCharz().isHaveMount)
						{
							Char.myCharz().cMP -= Char.myCharz().cMPGoc / 100 * ((Char.myCharz().isMonkey != 1) ? 1 : 2);
						}
						if (Char.myCharz().cMP < 0)
						{
							Char.myCharz().cMP = 0L;
						}
						GameScr.gI().isInjureMp = true;
						GameScr.gI().twMp = 0;
					}
				}
				else
				{
					message.writer().writeByte(0);
				}
				message.writer().writeShort(Char.myCharz().cx);
				if (num2 != 0)
				{
					message.writer().writeShort(Char.myCharz().cy);
				}
				session.sendMessage(message);
				GameScr.tickMove++;
				message.cleanup();
			}
			catch (Exception ex)
			{
				Cout.LogError("LOI CHAR MOVE " + ex.ToString());
			}
		}

		public void createChar(string name, int gender, int hair)
		{
			Message message = new Message(-28);
			try
			{
				message.writer().writeByte(2);
				message.writer().writeUTF(name);
				message.writer().writeByte(gender);
				message.writer().writeByte(hair);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			session.sendMessage(message);
		}

		public void requestModTemplate(int modTemplateId)
		{
			Message message = null;
			try
			{
				message = new Message(11);
				message.writer().writeByte(modTemplateId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void saleItem(sbyte action, sbyte type, short id)
		{
			Message message = null;
			try
			{
				message = new Message(7);
				message.writer().writeByte(action);
				message.writer().writeByte(type);
				message.writer().writeShort(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void buyItem(sbyte type, int id, int quantity)
		{
			Message message = null;
			try
			{
				message = new Message(6);
				message.writer().writeByte(type);
				message.writer().writeShort(id);
				if (quantity > 1)
				{
					message.writer().writeShort(quantity);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void selectSkill(int skillTemplateId)
		{
			Cout.println(Char.myCharz().cName + " SELECT SKILL " + skillTemplateId);
			Message message = null;
			try
			{
				message = new Message(34);
				message.writer().writeShort(skillTemplateId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getEffData(short id)
		{
			Message message = null;
			try
			{
				message = new Message(-66);
				message.writer().writeShort(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void openUIZone()
		{
			Message message = null;
			try
			{
				message = new Message(29);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void confirmMenu(short npcID, sbyte select)
		{
			Message message = null;
			try
			{
				message = new Message(32);
				message.writer().writeShort(npcID);
				message.writer().writeByte(select);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void openMenu(int npcId)
		{
			Message message = null;
			try
			{
				message = new Message(33);
				message.writer().writeShort(npcId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void menu(int npcId, int menuId, int optionId)
		{
			Cout.println("menuid: " + menuId);
			Message message = null;
			try
			{
				message = new Message(22);
				message.writer().writeByte(npcId);
				message.writer().writeByte(menuId);
				message.writer().writeByte(optionId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void menuId(short menuId)
		{
			Message message = null;
			try
			{
				message = new Message(27);
				message.writer().writeShort(menuId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void textBoxId(short menuId, string str)
		{
			Message message = null;
			try
			{
				message = new Message(88);
				message.writer().writeShort(menuId);
				message.writer().writeUTF(str);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void crystalCollectLock(Item[] items)
		{
			GameCanvas.msgdlg.pleasewait();
			Message message = null;
			try
			{
				message = new Message(13);
				for (int i = 0; i < items.Length; i++)
				{
					if (items[i] != null)
					{
						message.writer().writeByte(items[i].indexUI);
					}
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void acceptInviteTrade(int playerMapId)
		{
			Message message = null;
			try
			{
				message = new Message(37);
				message.writer().writeInt(playerMapId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void cancelInviteTrade()
		{
			Message message = null;
			try
			{
				message = new Message(50);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void sendPlayerAttack(MyVector vMob, MyVector vChar, int type)
		{
			try
			{
				Message message = null;
				if (type == 0)
				{
					return;
				}
				if (vMob.size() > 0 && vChar.size() > 0)
				{
					switch (type)
					{
					case 2:
						message = new Message(67);
						break;
					case 1:
						message = new Message(-4);
						break;
					}
					message.writer().writeByte(vMob.size());
					for (int i = 0; i < vMob.size(); i++)
					{
						Mob mob = (Mob)vMob.elementAt(i);
						message.writer().writeByte(mob.mobId);
					}
					for (int j = 0; j < vChar.size(); j++)
					{
						Char @char = (Char)vChar.elementAt(j);
						if (@char != null)
						{
							message.writer().writeInt(@char.charID);
						}
						else
						{
							message.writer().writeInt(-1);
						}
					}
				}
				else if (vMob.size() > 0)
				{
					message = new Message(54);
					for (int k = 0; k < vMob.size(); k++)
					{
						Mob mob2 = (Mob)vMob.elementAt(k);
						if (!mob2.isMobMe)
						{
							message.writer().writeByte(mob2.mobId);
							continue;
						}
						message.writer().writeByte(-1);
						message.writer().writeInt(mob2.mobId);
					}
				}
				else if (vChar.size() > 0)
				{
					message = new Message(-60);
					for (int l = 0; l < vChar.size(); l++)
					{
						Char char2 = (Char)vChar.elementAt(l);
						message.writer().writeInt(char2.charID);
					}
				}
				message.writer().writeSByte((sbyte)Char.myCharz().cdir);
				if (message != null)
				{
					session.sendMessage(message);
				}
			}
			catch (Exception)
			{
			}
		}

		public void pickItem(int itemMapId)
		{
			Message message = null;
			try
			{
				message = new Message(-20);
				message.writer().writeShort(itemMapId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void returnTownFromDead()
		{
			Message message = null;
			try
			{
				message = new Message(-15);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void wakeUpFromDead()
		{
			Message message = null;
			try
			{
				message = new Message(-16);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void chat(string text)
		{
			Message message = null;
			try
			{
				message = new Message(44);
				message.writer().writeUTF(text);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void updateData()
		{
			Message message = null;
			try
			{
				message = new Message(-87);
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void updateMap()
		{
			Message message = null;
			try
			{
				message = messageNotMap(6);
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void updateSkill()
		{
			Message message = null;
			try
			{
				message = messageNotMap(7);
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void updateItem()
		{
			Message message = null;
			try
			{
				message = messageNotMap(8);
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clientOk()
		{
			Message message = null;
			try
			{
				message = messageNotMap(13);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void addFriend(string name)
		{
			Message message = null;
			try
			{
				message = new Message(53);
				message.writer().writeUTF(name);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void addPartyAccept(int charId)
		{
			Message message = null;
			try
			{
				message = new Message(76);
				message.writer().writeInt(charId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void addPartyCancel(int charId)
		{
			Message message = null;
			try
			{
				message = new Message(77);
				message.writer().writeInt(charId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void player_vs_player(sbyte action, sbyte type, int playerId)
		{
			Message message = null;
			try
			{
				message = new Message(-59);
				message.writer().writeByte(action);
				message.writer().writeByte(type);
				message.writer().writeInt(playerId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void requestMaptemplate(int maptemplateId)
		{
			Message message = null;
			try
			{
				message = messageNotMap(10);
				message.writer().writeByte(maptemplateId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void acceptPleaseParty(string str)
		{
			Message message = null;
			try
			{
				message = new Message(17);
				message.writer().writeUTF(str);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void chatPlayer(string text, int id)
		{
			Res.outz("chat player text = " + text);
			Message message = null;
			try
			{
				message = new Message(-72);
				message.writer().writeInt(id);
				message.writer().writeUTF(text);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void chatGlobal(string text)
		{
			Message message = null;
			try
			{
				message = new Message(-71);
				message.writer().writeUTF(text);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void sendCardInfo(string NAP, string PIN)
		{
			Message message = null;
			try
			{
				message = messageNotMap(16);
				message.writer().writeUTF(NAP);
				message.writer().writeUTF(PIN);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void changeName(string name, int id)
		{
			Message message = null;
			try
			{
				message = messageNotMap(18);
				message.writer().writeInt(id);
				message.writer().writeUTF(name);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void requestIcon(int id)
		{
			GameCanvas.connect();
			Message message = null;
			try
			{
				message = new Message(-67);
				message.writer().writeInt(id);
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void activeAccProtect(int pass)
		{
			Message message = null;
			try
			{
				message = messageNotMap(37);
				message.writer().writeInt(pass);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void clearAccProtect(int pass)
		{
			Message message = null;
			try
			{
				message = messageNotMap(41);
				message.writer().writeInt(pass);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void openLockAccProtect(int pass2)
		{
			Message message = null;
			try
			{
				message = messageNotMap(39);
				message.writer().writeInt(pass2);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getBgTemplate(short id)
		{
			Message message = null;
			try
			{
				message = new Message(-32);
				message.writer().writeShort(id);
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getMapOffline()
		{
			Message message = null;
			try
			{
				message = new Message(-33);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void finishUpdate()
		{
			Message message = null;
			try
			{
				message = new Message(-38);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void finishUpdate(int playerID)
		{
			Message message = null;
			try
			{
				message = new Message(-38);
				message.writer().writeInt(playerID);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void finishLoadMap()
		{
			Message message = null;
			try
			{
				message = new Message(-39);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void requestBagImage(sbyte ID)
		{
			Message message = null;
			try
			{
				message = new Message(-63);
				message.writer().writeByte(ID);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void login2(string user)
		{
			Res.outz("Login 2");
			Message message = null;
			try
			{
				message = new Message(-101);
				message.writer().writeUTF(user);
				message.writer().writeByte(1);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getMagicTree(sbyte action)
		{
			Message message = null;
			try
			{
				message = new Message(-34);
				message.writer().writeByte(action);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void upPotential(bool forPet, int typePotential, int num)
		{
			Message message = null;
			try
			{
				message = messageSubCommand((sbyte)(forPet ? 18 : 16));
				message.writer().writeByte(typePotential);
				message.writer().writeShort(num);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getResource(sbyte action, MyVector vResourceIndex)
		{
			Res.outz("request resource action= " + action);
			Message message = null;
			try
			{
				message = new Message(-74);
				message.writer().writeByte(action);
				if (action == 2 && vResourceIndex != null)
				{
					message.writer().writeShort(vResourceIndex.size());
					for (int i = 0; i < vResourceIndex.size(); i++)
					{
						message.writer().writeShort(short.Parse((string)vResourceIndex.elementAt(i)));
					}
				}
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					reciveFromMainSession = true;
					session = Session_ME.gI();
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void requestMapSelect(int selected)
		{
			Res.outz("request magic tree");
			Message message = null;
			try
			{
				message = new Message(-91);
				message.writer().writeByte(selected);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void petInfo()
		{
			Message message = null;
			try
			{
				message = new Message(-107);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void PetInfo2()
		{
			Message message = null;
			try
			{
				message = new Message(3);
				message.writer().writeByte(0);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void sendTop(string topName, sbyte selected)
		{
			Message message = null;
			try
			{
				message = new Message(-96);
				message.writer().writeUTF(topName);
				message.writer().writeByte(selected);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void enemy(sbyte b, int charID)
		{
			Message message = null;
			Res.outz("add enemy");
			try
			{
				message = new Message(-99);
				message.writer().writeByte(b);
				if (b == 1 || b == 2)
				{
					message.writer().writeInt(charID);
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void kigui(sbyte action, int itemId, sbyte moneyType, int money, int quaintly)
		{
			Message message = null;
			try
			{
				Res.outz("ki gui action= " + action);
				message = new Message(-100);
				message.writer().writeByte(action);
				if (action == 0)
				{
					message.writer().writeShort(itemId);
					message.writer().writeByte(moneyType);
					message.writer().writeInt(money);
					message.writer().writeInt(quaintly);
				}
				if (action == 1 || action == 2)
				{
					message.writer().writeShort(itemId);
				}
				if (action == 3)
				{
					message.writer().writeShort(itemId);
					message.writer().writeByte(moneyType);
					message.writer().writeInt(money);
				}
				if (action == 4)
				{
					message.writer().writeByte(moneyType);
					message.writer().writeByte(money);
					Res.outz("currTab= " + moneyType + " page= " + money);
				}
				if (action == 5)
				{
					message.writer().writeShort(itemId);
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getFlag(sbyte action, sbyte flagType)
		{
			Message message = null;
			try
			{
				message = new Message(-103);
				message.writer().writeByte(action);
				Res.outz("------------service--  " + action + "   " + flagType);
				if (action != 0)
				{
					message.writer().writeByte(flagType);
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void setLockInventory(int pass)
		{
			Message message = null;
			try
			{
				Res.outz("------------setLockInventory:     " + pass);
				message = new Message(-104);
				message.writer().writeInt(pass);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void petStatus(sbyte status)
		{
			Message message = null;
			try
			{
				message = new Message(-108);
				message.writer().writeByte(status);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void pet2Status(sbyte status)
		{
			Message message = null;
			try
			{
				message = new Message(3);
				message.writer().writeByte(1);
				message.writer().writeByte(status);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void transportNow()
		{
			Message message = null;
			try
			{
				Res.outz("------------transportNow  ");
				message = new Message(-105);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void funsion(sbyte type)
		{
			Message message = null;
			try
			{
				Res.outz("FUNSION");
				message = new Message(125);
				message.writer().writeByte(type);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void imageSource(MyVector vID)
		{
			Message message = null;
			try
			{
				Res.outz("IMAGE SOURCE size= " + vID.size());
				message = new Message(-111);
				message.writer().writeShort(vID.size());
				if (vID.size() > 0)
				{
					for (int i = 0; i < vID.size(); i++)
					{
						Res.outz("gui len str " + ((ImageSource)vID.elementAt(i)).id);
						message.writer().writeUTF(((ImageSource)vID.elementAt(i)).id);
					}
				}
				if (Session_ME2.gI().isConnected() && !Session_ME2.connecting)
				{
					session = Session_ME2.gI();
				}
				else
				{
					session = Session_ME.gI();
					reciveFromMainSession = true;
				}
				session.sendMessage(message);
				session = Session_ME.gI();
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void sendServerData(sbyte action, int id, sbyte[] data)
		{
			Message message = null;
			try
			{
				Res.outz("SERVER DATA");
				message = new Message(-110);
				message.writer().writeByte(action);
				if (action == 1)
				{
					message.writer().writeInt(id);
					if (data != null)
					{
						int num = data.Length;
						message.writer().writeShort(num);
						message.writer().write(ref data, 0, num);
					}
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void changeOnKeyScr(sbyte[] skill)
		{
			Message message = null;
			try
			{
				message = new Message(-113);
				for (int i = 0; i < GameScr.onScreenSkill.Length; i++)
				{
					message.writer().writeByte(skill[i]);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void requestPean()
		{
			Message message = null;
			try
			{
				message = new Message(-114);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void sendThachDau(int id)
		{
			Res.outz("GUI THACH DAU");
			Message message = null;
			try
			{
				message = new Message(-118);
				message.writer().writeInt(id);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void messagePlayerMenu(int charId)
		{
			Message message = null;
			try
			{
				message = new Message(-30);
				message.writer().writeByte(63);
				message.writer().writeInt(charId);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void playerMenuAction(int charId, short select)
		{
			Message message = null;
			try
			{
				message = new Message(-30);
				message.writer().writeByte(64);
				message.writer().writeInt(charId);
				message.writer().writeShort(select);
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			finally
			{
				message.cleanup();
			}
		}

		public void getImgByName(string nameImg)
		{
			Message message = null;
			try
			{
				message = new Message(66);
				message.writer().writeUTF(nameImg);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void SendCrackBall(byte type, byte soluong)
		{
			Message message = new Message(-127);
			try
			{
				message.writer().writeByte(type);
				if (soluong > 0)
				{
					message.writer().writeByte(soluong);
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void SendRada(int i, int id)
		{
			Message message = new Message(sbyte.MaxValue);
			try
			{
				message.writer().writeByte(i);
				if (id != -1)
				{
					message.writer().writeShort(id);
				}
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void requestPKHistory()
		{
			Message message = new Message(sbyte.MaxValue);
			try
			{
				message.writer().writeByte(43);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void requestActivityDashboard()
		{
			Message message = null;
			try
			{
				message = new Message(-58);
				message.writer().writeByte(0);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}

		public void claimActivityReward(bool weekly, string tierId)
		{
			if (string.IsNullOrEmpty(tierId) || tierId.Length > 64)
			{
				return;
			}
			Message message = null;
			try
			{
				message = new Message(-58);
				message.writer().writeByte(1);
				message.writer().writeByte(weekly ? 1 : 0);
				message.writer().writeUTF(tierId);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}

		public void sendDelAcc()
		{
			Message message = new Message(69);
			try
			{
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				message.cleanup();
			}
		}

		public void new_skill_not_focus(sbyte idTemplateSkill, sbyte dir, short x, short y)
		{
			Message message = null;
			try
			{
				message = new Message(-45);
				message.writer().writeSByte(20);
				message.writer().writeSByte(idTemplateSkill);
				message.writer().writeShort(Char.myCharz().cx);
				message.writer().writeShort(Char.myCharz().cy);
				message.writer().writeSByte(dir);
				message.writer().writeShort(x);
				message.writer().writeShort(y);
				if (idTemplateSkill == 31)
				{
					Char caster = Char.myCharz();
					message.writer().writeInt(SuperGhostKamikazeEffect.activeCastId(caster.charID));
					int targetType = 0, targetId = -1;
					if (caster.charFocus != null && !caster.charFocus.isPet && !caster.charFocus.isMiniPet)
					{
						targetType = 2; targetId = caster.charFocus.charID;
					}
					else if (caster.mobFocus != null)
					{
						targetType = 1; targetId = caster.mobFocus.mobId;
					}
					message.writer().writeSByte((sbyte)targetType);
					message.writer().writeInt(targetId);
				}
				session.sendMessage(message);
			}
			catch (Exception ex)
			{
				Cout.println(ex.Message + ex.StackTrace);
			}
			finally
			{
				message.cleanup();
			}
		}

		public void SendRemoteAddress()
		{
		}

		public void setBadgeVisibility(bool hidden)
		{
			Message message = null;
			try
			{
				message = new Message(-48);
				message.writer().writeByte(hidden ? 1 : 0);
				session.sendMessage(message);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (message != null)
				{
					message.cleanup();
				}
			}
		}
	}
}
