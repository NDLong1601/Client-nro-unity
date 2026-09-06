using System;

namespace Game2.Assets.src.g
{
	internal class ImageSource
	{
		public sbyte version;

		public string id;

		public static MyVector vSource = new MyVector();

		public static MyVector vRms = new MyVector();

		public ImageSource(string ID, sbyte version)
		{
			id = ID;
			this.version = version;
		}

		public static void checkRMS()
		{
			MyVector vID = new MyVector();
			sbyte[] array = Rms.loadRMS("ImageSource");
			if (array == null)
			{
				Service.gI().imageSource(vID);
				return;
			}
			vRms = new MyVector();
			DataInputStream dataInputStream = new DataInputStream(array);
			if (dataInputStream == null)
			{
				return;
			}
			try
			{
				short num = dataInputStream.readShort();
				string[] array2 = new string[num];
				sbyte[] array3 = new sbyte[num];
				for (int i = 0; i < num; i++)
				{
					array2[i] = dataInputStream.readUTF();
					array3[i] = dataInputStream.readByte();
					vRms.addElement(new ImageSource(array2[i], array3[i]));
				}
				dataInputStream.close();
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
			Service.gI().imageSource(vID);
		}

		public static void saveRMS()
		{
			DataOutputStream dataOutputStream = new DataOutputStream();
			try
			{
				dataOutputStream.writeShort((short)vSource.size());
				for (int i = 0; i < vSource.size(); i++)
				{
					dataOutputStream.writeUTF(((ImageSource)vSource.elementAt(i)).id);
					dataOutputStream.writeByte(((ImageSource)vSource.elementAt(i)).version);
				}
				Rms.saveRMS("ImageSource", dataOutputStream.toByteArray());
				dataOutputStream.close();
			}
			catch (Exception ex)
			{
				ex.StackTrace.ToString();
			}
		}
	}
}
