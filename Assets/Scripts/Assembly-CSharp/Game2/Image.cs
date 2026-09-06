using System;
using UnityEngine;

namespace Game2
{
	public class Image
	{
		private const int INTERVAL = 5;

		private const int MAXTIME = 500;

		public Texture2D texture = new Texture2D(1, 1);

		public static Image imgTemp;

		public static string filenametemp;

		public static byte[] datatemp;

		public static Image imgSrcTemp;

		public static int xtemp;

		public static int ytemp;

		public static int wtemp;

		public static int htemp;

		public static int transformtemp;

		public int w;

		public int h;

		public static int status;

		public Color colorBlend = Color.black;

		public static Image createImage(string filename)
		{
			return __createImage(filename);
		}

		public static Image createImage(byte[] imageData)
		{
			return __createImage(imageData);
		}

		public static Image createImage(int w, int h)
		{
			return __createImage(w, h);
		}

		public static Image createImage(Image source, int x, int y, int w, int h, int transform)
		{
			return __createImage(source, x, y, w, h, transform);
		}

		public static Image createImage(sbyte[] imageData, int offset, int lenght)
		{
			if (offset + lenght > imageData.Length)
			{
				return null;
			}
			byte[] array = new byte[lenght];
			for (int i = 0; i < lenght; i++)
			{
				array[i] = convertSbyteToByte(imageData[i + offset]);
			}
			return createImage(array);
		}

		public static byte convertSbyteToByte(sbyte var)
		{
			if (var > 0)
			{
				return (byte)var;
			}
			return (byte)(var + 256);
		}

		public static Image createRGBImage(int[] rbg, int w, int h, bool bl)
		{
			Image image = createImage(w, h);
			Color[] array = new Color[rbg.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = setColorFromRBG(rbg[i]);
			}
			image.texture.SetPixels(0, 0, w, h, array);
			image.texture.Apply();
			return image;
		}

		public static Color setColorFromRBG(int rgb)
		{
			int num = rgb & 0xFF;
			int num2 = (rgb >> 8) & 0xFF;
			float num3 = (rgb >> 16) & 0xFF;
			return new Color(b: (float)num / 256f, g: (float)num2 / 256f, r: num3 / 256f);
		}

		public static void update()
		{
			if (status == 2)
			{
				status = 1;
				imgTemp = __createEmptyImage();
				status = 0;
			}
			else if (status == 3)
			{
				status = 1;
				imgTemp = __createImage(filenametemp);
				status = 0;
			}
			else if (status == 4)
			{
				status = 1;
				imgTemp = __createImage(datatemp);
				status = 0;
			}
			else if (status == 5)
			{
				status = 1;
				imgTemp = __createImage(imgSrcTemp, xtemp, ytemp, wtemp, htemp, transformtemp);
				status = 0;
			}
			else if (status == 6)
			{
				status = 1;
				imgTemp = __createImage(wtemp, htemp);
				status = 0;
			}
		}

		private static Image __createImage(string filename)
		{
			Image image = new Image();
			Texture2D texture2D = Resources.Load(filename) as Texture2D;
			if (texture2D == null)
			{
				throw new Exception("NULL POINTER EXCEPTION AT Image __createImage " + filename);
			}
			image.texture = texture2D;
			image.w = image.texture.width;
			image.h = image.texture.height;
			setTextureQuality(image);
			return image;
		}

		private static Image __createImage(byte[] imageData)
		{
			if (imageData == null || imageData.Length == 0)
			{
				Cout.LogError("Create Image from byte array fail");
				return null;
			}
			Image image = new Image();
			try
			{
				if (!image.texture.LoadImage(imageData))
				{
					Cout.LogError("CREATE IMAGE FROM ARRAY RETURNED INVALID DATA");
					return null;
				}
				image.w = image.texture.width;
				image.h = image.texture.height;
				setTextureQuality(image);
				return image;
			}
			catch (Exception)
			{
				Cout.LogError("CREAT IMAGE FROM ARRAY FAIL \n" + Environment.StackTrace);
				return null;
			}
		}

		private static Image __createImage(Image src, int x, int y, int w, int h, int transform)
		{
			Image image = new Image();
			image.texture = new Texture2D(w, h);
			y = src.texture.height - y - h;
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					int num = i;
					if (transform == 2)
					{
						num = w - i;
					}
					int num2 = j;
					image.texture.SetPixel(i, j, src.texture.GetPixel(x + num, y + num2));
				}
			}
			image.texture.Apply();
			image.w = image.texture.width;
			image.h = image.texture.height;
			setTextureQuality(image);
			return image;
		}

		private static Image __createEmptyImage()
		{
			return new Image();
		}

		public static Image __createImage(int w, int h)
		{
			Image obj = new Image
			{
				texture = new Texture2D(w, h, TextureFormat.RGBA32, mipChain: false)
			};
			setTextureQuality(obj);
			obj.w = w;
			obj.h = h;
			obj.texture.Apply();
			return obj;
		}

		public int getWidth()
		{
			return w / mGraphics.zoomLevel;
		}

		public int getHeight()
		{
			return h / mGraphics.zoomLevel;
		}

		private static void setTextureQuality(Image img)
		{
			setTextureQuality(img.texture);
		}

		public static void setTextureQuality(Texture2D texture)
		{
			texture.anisoLevel = 0;
			texture.filterMode = FilterMode.Point;
			texture.mipMapBias = 0f;
			texture.wrapMode = TextureWrapMode.Clamp;
		}

		public int getRealImageWidth()
		{
			return w;
		}

		public int getRealImageHeight()
		{
			return h;
		}

		public void getRGB(ref int[] data, int x1, int x2, int x, int y, int w, int h)
		{
			Color[] pixels = texture.GetPixels(x, this.h - 1 - y, w, h);
			for (int i = 0; i < pixels.Length; i++)
			{
				data[i] = mGraphics.getIntByColor(pixels[i]);
			}
		}
	}
}
