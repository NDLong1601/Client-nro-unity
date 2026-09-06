using System;

namespace Game1
{
	public class NinjaUtil
	{
		public static int randomNumber(int max)
		{
			return new MyRandom().nextInt(max);
		}

		public static sbyte[] readByteArray(Message msg)
		{
			try
			{
				int num = msg.reader().readInt();
				if (num > 1)
				{
					sbyte[] data = new sbyte[num];
					msg.reader().read(ref data);
					return data;
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		public static sbyte[] readByteArray(myReader dos)
		{
			try
			{
				sbyte[] data = new sbyte[dos.readInt()];
				dos.read(ref data);
				return data;
			}
			catch (Exception)
			{
				Cout.LogError("LOI DOC readByteArray dos  NINJAUTIL");
			}
			return null;
		}

		public static string NumberTostring2(long num)
		{
			if (num >= 1000000000)
			{
				return $"{num / 1000000000:0.0}B";
			}
			if (num >= 1000000)
			{
				return $"{num / 1000000:0.0}M";
			}
			if (num >= 1000)
			{
				return $"{num / 1000:0.0}K";
			}
			return num.ToString();
		}

		public static string Replace(string text, string regex, string replacement)
		{
			return text.Replace(regex, replacement);
		}

		public static string getDate(int second)
		{
			long num = (long)second * 1000L;
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(new TimeSpan(num * 10000)).ToUniversalTime();
			int hour = dateTime.Hour;
			_ = dateTime.Minute;
			int day = dateTime.Day;
			int month = dateTime.Month;
			int year = dateTime.Year;
			return day + "/" + month + "/" + year + " " + hour + "h";
		}

		public static string getDate2(long second)
		{
			long num = second + 25200000;
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(new TimeSpan(num * 10000)).ToUniversalTime();
			int hour = dateTime.Hour;
			int minute = dateTime.Minute;
			return hour + "h" + minute + "m";
		}

		public static string getTime(int timeRemainS)
		{
			int num = 0;
			if (timeRemainS > 60)
			{
				num = timeRemainS / 60;
				timeRemainS %= 60;
			}
			int num2 = 0;
			if (num > 60)
			{
				num2 = num / 60;
				num %= 60;
			}
			int num3 = 0;
			if (num2 > 24)
			{
				num3 = num2 / 24;
				num2 %= 24;
			}
			string empty = string.Empty;
			if (num3 > 0)
			{
				empty += num3;
				empty += "d";
				return empty + num2 + "h";
			}
			if (num2 > 0)
			{
				empty += num2;
				empty += "h";
				return empty + num + "'";
			}
			empty = ((num <= 9) ? (empty + "0" + num) : (empty + num));
			empty += ":";
			if (timeRemainS > 9)
			{
				return empty + timeRemainS;
			}
			return empty + "0" + timeRemainS;
		}

		public static string getMoneys(long m)
		{
			string text = string.Empty;
			long num = m / 1000 + 1;
			for (int i = 0; i < num; i++)
			{
				if (m < 1000)
				{
					text = m + text;
					break;
				}
				long num2 = m % 1000;
				text = ((num2 != 0L) ? ((num2 >= 10) ? ((num2 >= 100) ? ("." + num2 + text) : (".0" + num2 + text)) : (".00" + num2 + text)) : (".000" + text));
				m /= 1000;
			}
			return text;
		}

		public static string getTimeAgo(long timeRemainS)
		{
			long num = 0L;
			if (timeRemainS > 60)
			{
				num = timeRemainS / 60;
			}
			long num2 = 0L;
			if (num > 60)
			{
				num2 = num / 60;
				num %= 60;
			}
			long num3 = 0L;
			if (num2 > 24)
			{
				num3 = num2 / 24;
				num2 %= 24;
			}
			string empty = string.Empty;
			if (num3 > 0)
			{
				empty += num3;
				empty += "d";
				return empty + num2 + "h";
			}
			if (num2 > 0)
			{
				empty += num2;
				empty += "h";
				return empty + num + "'";
			}
			if (num == 0L)
			{
				num = 1L;
			}
			empty += num;
			return empty + "ph";
		}
	}
}
