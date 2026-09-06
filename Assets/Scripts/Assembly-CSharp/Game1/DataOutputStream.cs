namespace Game1
{
	public class DataOutputStream
	{
		private myWriter w = new myWriter();

		public DataOutputStream()
		{
		}

		public DataOutputStream(int len)
		{
			w = new myWriter(len);
		}

		public void writeShort(short i)
		{
			w.writeShort(i);
		}

		public sbyte[] toByteArray()
		{
			return w.getData();
		}

		public void close()
		{
			w.Close();
		}

		public void writeByte(sbyte b)
		{
			w.writeByte(b);
		}

		public void writeUTF(string name)
		{
			w.writeUTF(name);
		}
	}
}
