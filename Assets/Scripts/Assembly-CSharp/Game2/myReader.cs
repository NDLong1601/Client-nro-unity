using System;
using System.Text;

namespace Game2
{
	public class myReader
	{
		public sbyte[] buffer;

		private int posRead;

		private int posMark;

		public myReader(sbyte[] data)
		{
			buffer = data;
		}

		public sbyte readSByte()
		{
			if (posRead < buffer.Length)
			{
				return buffer[posRead++];
			}
			posRead = buffer.Length;
			throw new Exception(" loi doc sbyte eof ");
		}

		public sbyte readByte()
		{
			return readSByte();
		}

		public void mark(int readlimit)
		{
			posMark = posRead;
		}

		public void reset()
		{
			posRead = posMark;
		}

		public byte readUnsignedByte()
		{
			return convertSbyteToByte(readSByte());
		}

		public short readShort()
		{
			short num = 0;
			for (int i = 0; i < 2; i++)
			{
				num = (short)(num << 8);
				short num2 = num;
				short num3 = 255;
				sbyte[] array = buffer;
				num = (short)(num2 | (num3 & array[posRead++]));
			}
			return num;
		}

		public ushort readUnsignedShort()
		{
			ushort num = 0;
			for (int i = 0; i < 2; i++)
			{
				num = (ushort)(num << 8);
				ushort num2 = num;
				ushort num3 = 255;
				sbyte[] array = buffer;
				num = (ushort)(num2 | (num3 & array[posRead++]));
			}
			return num;
		}

		public int readInt()
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				num <<= 8;
				int num2 = num;
				int num3 = 255;
				sbyte[] array = buffer;
				num = num2 | (num3 & array[posRead++]);
			}
			return num;
		}

		public long readLong()
		{
			long num = 0L;
			for (int i = 0; i < 8; i++)
			{
				num <<= 8;
				long num2 = num;
				long num3 = 255L;
				sbyte[] array = buffer;
				num = num2 | (num3 & array[posRead++]);
			}
			return num;
		}

		public bool readBool()
		{
			return readSByte() > 0;
		}

		public bool readBoolean()
		{
			return readSByte() > 0;
		}

		public string readStringUTF()
		{
			short num = readShort();
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = convertSbyteToByte(readSByte());
			}
			return new UTF8Encoding().GetString(array);
		}

		public string readUTF()
		{
			return readStringUTF();
		}

		public int read(ref sbyte[] data)
		{
			if (data == null)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = readSByte();
				if (posRead > buffer.Length)
				{
					return -1;
				}
				num++;
			}
			return num;
		}

		public void readFully(ref sbyte[] data)
		{
			if (data != null && data.Length + posRead <= buffer.Length)
			{
				for (int i = 0; i < data.Length; i++)
				{
					data[i] = readSByte();
				}
			}
		}

		public int available()
		{
			return buffer.Length - posRead;
		}

		public static byte convertSbyteToByte(sbyte var)
		{
			if (var > 0)
			{
				return (byte)var;
			}
			return (byte)(var + 256);
		}

		public void Close()
		{
			buffer = null;
		}

		public void read(ref sbyte[] data, int arg1, int arg2)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < arg2; i++)
			{
				data[i + arg1] = readSByte();
				if (posRead > buffer.Length)
				{
					break;
				}
			}
		}
	}
}
