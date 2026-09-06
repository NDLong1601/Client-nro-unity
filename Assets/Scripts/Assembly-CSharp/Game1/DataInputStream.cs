using System;
using UnityEngine;

namespace Game1
{
	public class DataInputStream
	{
		public myReader r;

		public static DataInputStream istemp;

		private static int status;

		private static string filenametemp;

		public DataInputStream(string filename)
		{
			TextAsset textAsset = (TextAsset)Resources.Load(filename, typeof(TextAsset));
			r = new myReader(ArrayCast.cast(textAsset.bytes));
		}

		public DataInputStream(sbyte[] data)
		{
			r = new myReader(data);
		}

		public static void update()
		{
			if (status == 2)
			{
				status = 1;
				istemp = __getResourceAsStream(filenametemp);
				status = 0;
			}
		}

		public static DataInputStream getResourceAsStream(string filename)
		{
			return __getResourceAsStream(filename);
		}

		private static DataInputStream __getResourceAsStream(string filename)
		{
			try
			{
				return new DataInputStream(filename);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public short readShort()
		{
			return r.readShort();
		}

		public int read()
		{
			return r.readUnsignedByte();
		}

		public void read(ref sbyte[] data)
		{
			r.read(ref data);
		}

		public void close()
		{
			r.Close();
		}

		public string readUTF()
		{
			return r.readUTF();
		}

		public sbyte readByte()
		{
			return r.readByte();
		}

		public int readUnsignedByte()
		{
			return (byte)r.readByte();
		}

		public int available()
		{
			return r.available();
		}
	}
}
