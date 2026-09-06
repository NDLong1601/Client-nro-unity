using System;
using System.Collections.Generic;
using Game2.Assets.src.e;
using UnityEngine;

namespace Game2
{
	public class SmallImage
	{
		private const int HellzoneGrenadeIconId = 14642;

		private const int BarrierPrisonIconId = 14650;

		private const int SuperGhostKamikazeIconId = 26554;

		private const int SkillIconSize = 20;

		public static int[][] smallImg;

		public static SmallImage instance;

		public static Image[] imgbig;

		public static Small[] imgNew;

		public static MyVector vKeys = new MyVector();

		public static Image imgEmpty = null;

		public static sbyte[] newSmallVersion;

		public static int smallCount;

		public static short maxSmall;

		public static Dictionary<int, Image> imageRaw = new Dictionary<int, Image>();

		private static long lastBigImageLoadTime;

		private static Dictionary<int, long> lastIconRequest = new Dictionary<int, long>();

		private static Dictionary<int, Image> grayImages = new Dictionary<int, Image>();

		public SmallImage()
		{
			readImage();
		}

		public static void loadBigRMS()
		{
			if (imgbig != null && imgbig.Length == 5 && isBigImageReady(imgbig[0]) && isBigImageReady(imgbig[1]) && isBigImageReady(imgbig[2]) && isBigImageReady(imgbig[3]) && isBigImageReady(imgbig[4]))
			{
				return;
			}
			long now = mSystem.currentTimeMillis();
			if (now - lastBigImageLoadTime < 2000L)
			{
				return;
			}
			lastBigImageLoadTime = now;
			if (imgbig == null || imgbig.Length != 5)
			{
				imgbig = new Image[5];
			}
			for (int i = 0; i < imgbig.Length; i++)
			{
				if (!isBigImageReady(imgbig[i]))
				{
					imgbig[i] = GameCanvas.loadImageRMS("/img/Big" + i + ".png");
				}
			}
		}

		private static bool isImageReady(Image image)
		{
			return image != null && image.texture != null && image.getRealImageWidth() > 0 && image.getRealImageHeight() > 0;
		}

		private static bool isBigImageReady(Image image)
		{
			int minimumSize = 255 * mGraphics.zoomLevel;
			return isImageReady(image) && image.getRealImageWidth() >= minimumSize && image.getRealImageHeight() >= minimumSize;
		}

		private static void requestIcon(int id)
		{
			long now = mSystem.currentTimeMillis();
			long lastRequest;
			if (!lastIconRequest.TryGetValue(id, out lastRequest) || now - lastRequest >= 2000L)
			{
				lastIconRequest[id] = now;
				Service.gI().requestIcon(id);
			}
		}

		public static void loadBigImage()
		{
			imgEmpty = Image.createRGBImage(new int[1], 1, 1, bl: true);
		}

		public static void init()
		{
			instance = null;
			instance = new SmallImage();
		}

		public void readImage()
		{
			int num = 0;
			try
			{
				DataInputStream dataInputStream = new DataInputStream(Rms.loadRMS("NR_image"));
				short num2 = dataInputStream.readShort();
				smallImg = new int[num2][];
				for (int i = 0; i < smallImg.Length; i++)
				{
					smallImg[i] = new int[5];
				}
				for (int j = 0; j < num2; j++)
				{
					num++;
					smallImg[j][0] = dataInputStream.readUnsignedByte();
					smallImg[j][1] = dataInputStream.readShort();
					smallImg[j][2] = dataInputStream.readShort();
					smallImg[j][3] = dataInputStream.readShort();
					smallImg[j][4] = dataInputStream.readShort();
				}
			}
			catch (Exception ex)
			{
				Cout.LogError3("Loi readImage: " + ex.ToString() + "i= " + num);
			}
		}

		public static void clearHastable()
		{
		}

		public static void createImage(int id)
		{
			if (id < 0 || imgNew == null || id >= imgNew.Length)
			{
				return;
			}
			if (!isImageReady(imgEmpty))
			{
				loadBigImage();
			}
			if (mGraphics.zoomLevel == 1)
			{
				Image image = GameCanvas.loadImage("/SmallImage/Small" + id + ".png");
				if (isImageReady(image))
				{
					imgNew[id] = new Small(image, id);
					return;
				}
				imgNew[id] = new Small(imgEmpty, id);
				requestIcon(id);
				return;
			}
			Image image2 = GameCanvas.loadImage("/SmallImage/Small" + id + ".png");
			if (isImageReady(image2))
			{
				imgNew[id] = new Small(image2, id);
				return;
			}
			bool flag = false;
			if (imageRaw.ContainsKey(id))
			{
				Image value = null;
				imageRaw.TryGetValue(id, out value);
				if (isImageReady(value))
				{
					imgNew[id] = new Small(value, id);
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				imgNew[id] = new Small(imgEmpty, id);
				requestIcon(id);
			}
		}

		private static void drawFallbackImage(mGraphics g, int id, int x, int y, int transform, int anchor)
		{
			if (id < 0 || imgNew == null || id >= imgNew.Length)
			{
				return;
			}
			Small small = imgNew[id];
			if (small == null || !isImageReady(small.img))
			{
				createImage(id);
			}
			else
			{
				if (small.img == imgEmpty)
				{
					requestIcon(id);
				}
				if (id == HellzoneGrenadeIconId || id == BarrierPrisonIconId || id == SuperGhostKamikazeIconId)
				{
					drawSkillIconLikeAtlas(g, small.img, x, y, anchor);
					return;
				}
				small.paint(g, transform, x, y, anchor);
			}
		}

		/// <summary>
		/// The stock skill icons are 20x20 regions in Big0..Big4. Custom skill
		/// icons are standalone fallback images, so normalize their draw bounds to the same
		/// 20x20 contract regardless of the PNG or stale runtime-cache size.
		/// </summary>
		private static void drawSkillIconLikeAtlas(mGraphics g, Image image, int x, int y, int anchor)
		{
			if (!isImageReady(image) || image == imgEmpty)
			{
				return;
			}
			int drawX = x;
			int drawY = y;
			if ((anchor & mGraphics.RIGHT) != 0)
			{
				drawX -= SkillIconSize;
			}
			else if ((anchor & mGraphics.HCENTER) != 0)
			{
				drawX -= SkillIconSize / 2;
			}
			if ((anchor & mGraphics.BOTTOM) != 0)
			{
				drawY -= SkillIconSize;
			}
			else if ((anchor & mGraphics.VCENTER) != 0)
			{
				drawY -= SkillIconSize / 2;
			}
			g.drawImageScale(image, drawX, drawY, SkillIconSize, SkillIconSize, 0);
		}

		public static void drawSmallImage(mGraphics g, int id, int x, int y, int transform, int anchor)
		{
			if (id < 0)
			{
				return;
			}
			loadBigRMS();
			if (imgbig != null && smallImg != null && id < smallImg.Length && smallImg[id] != null)
			{
				int[] array = smallImg[id];
				if (array[1] < 256 && array[2] < 256 && array[3] < 256 && array[4] < 256 && array[0] >= 0 && array[0] < imgbig.Length && isBigImageReady(imgbig[array[0]]))
				{
					g.drawRegion(imgbig[array[0]], array[1], array[2], array[3], array[4], transform, x, y, anchor);
					return;
				}
			}
			drawFallbackImage(g, id, x, y, transform, anchor);
		}

		public static void drawSmallImageGray(mGraphics g, int id, int x, int y, int anchor)
		{
			Image image = getGrayImage(id);
			if (image != null)
			{
				g.drawImage(image, x, y, anchor);
				return;
			}
			drawSmallImage(g, id, x, y, 0, anchor);
		}

		private static Image getGrayImage(int id)
		{
			if (id < 0)
			{
				return null;
			}
			int cacheKey = id * 10 + mGraphics.zoomLevel;
			Image cached;
			if (grayImages.TryGetValue(cacheKey, out cached))
			{
				return cached;
			}
			try
			{
				loadBigRMS();
				Texture2D source = null;
				int sourceX = 0;
				int sourceY = 0;
				int width = 0;
				int height = 0;
				if (imgbig != null && smallImg != null && id < smallImg.Length && smallImg[id] != null)
				{
					int[] info = smallImg[id];
					if (info[1] < 256 && info[2] < 256 && info[3] < 256 && info[4] < 256
						&& info[0] >= 0 && info[0] < imgbig.Length && isBigImageReady(imgbig[info[0]]))
					{
						Texture2D atlas = imgbig[info[0]].texture;
						int atlasX = info[1] * mGraphics.zoomLevel;
						int atlasWidth = info[3] * mGraphics.zoomLevel;
						int atlasHeight = info[4] * mGraphics.zoomLevel;
						int atlasY = atlas.height - (info[2] + info[4]) * mGraphics.zoomLevel;
						if (atlasX >= 0 && atlasY >= 0 && atlasWidth > 0 && atlasHeight > 0
							&& atlasX + atlasWidth <= atlas.width && atlasY + atlasHeight <= atlas.height)
						{
							source = atlas;
							sourceX = atlasX;
							sourceY = atlasY;
							width = atlasWidth;
							height = atlasHeight;
						}
					}
				}
				if (source == null)
				{
					if (imgNew == null || id >= imgNew.Length || imgNew[id] == null || !isImageReady(imgNew[id].img))
					{
						createImage(id);
					}
					if (imgNew != null && id < imgNew.Length && imgNew[id] != null
						&& imgNew[id].img != imgEmpty && isImageReady(imgNew[id].img))
					{
						source = imgNew[id].img.texture;
						width = source.width;
						height = source.height;
					}
				}
				if (source == null || width <= 0 || height <= 0)
				{
					return null;
				}
				Color[] pixels = readPixels(source, sourceX, sourceY, width, height);
				for (int i = 0; i < pixels.Length; i++)
				{
					float luminance = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
					luminance = 0.2f + luminance * 0.72f;
					pixels[i] = new Color(luminance, luminance, luminance, pixels[i].a);
				}
				Image gray = Image.createImage(width, height);
				gray.texture.SetPixels(pixels);
				gray.texture.Apply();
				grayImages[cacheKey] = gray;
				return gray;
			}
			catch (System.Exception)
			{
				return null;
			}
		}

		private static Color[] readPixels(Texture2D source, int x, int y, int width, int height)
		{
			try
			{
				return source.GetPixels(x, y, width, height);
			}
			catch (System.Exception)
			{
			}
			RenderTexture previous = RenderTexture.active;
			RenderTexture temporary = null;
			Texture2D readable = null;
			try
			{
				temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
				UnityEngine.Graphics.Blit(source, temporary);
				RenderTexture.active = temporary;
				readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
				readable.ReadPixels(new Rect(x, y, width, height), 0, 0);
				readable.Apply();
				return readable.GetPixels();
			}
			finally
			{
				RenderTexture.active = previous;
				if (readable != null)
				{
					UnityEngine.Object.Destroy(readable);
				}
				if (temporary != null)
				{
					RenderTexture.ReleaseTemporary(temporary);
				}
			}
		}

		public static void drawSmallImage(mGraphics g, int id, int f, int x, int y, int w, int h, int transform, int anchor)
		{
			if (id < 0 || imgNew == null || id >= imgNew.Length)
			{
				return;
			}
			loadBigRMS();
			if (imgbig != null && smallImg != null && id < smallImg.Length && smallImg[id] != null)
			{
				int sheet = smallImg[id][0];
				if (sheet >= 0 && sheet < imgbig.Length && sheet != 4 && isBigImageReady(imgbig[sheet]))
				{
					g.drawRegion(imgbig[sheet], 0, f * w, w, h, transform, x, y, anchor);
					return;
				}
			}
			Small small = imgNew[id];
			if (small == null || !isImageReady(small.img))
			{
				createImage(id);
			}
			else
			{
				if (small.img == imgEmpty)
				{
					requestIcon(id);
				}
				small.paint(g, transform, f, x, y, w, h, anchor);
			}
		}

		public static void update()
		{
			int num = 0;
			if (GameCanvas.gameTick % 1000 != 0)
			{
				return;
			}
			for (int i = 0; i < imgNew.Length; i++)
			{
				if (imgNew[i] != null)
				{
					num++;
					imgNew[i].update();
					smallCount++;
				}
			}
			if (num > 200 && GameCanvas.lowGraphic)
			{
				imgNew = new Small[maxSmall];
			}
		}
	}
}
