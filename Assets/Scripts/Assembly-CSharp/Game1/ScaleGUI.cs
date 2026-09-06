using System.Collections.Generic;
using UnityEngine;

namespace Game1
{
	public class ScaleGUI
	{
		public static bool scaleScreen;

		public static float WIDTH;

		public static float HEIGHT;

		private static List<Matrix4x4> stack = new List<Matrix4x4>();

		public static void initScaleGUI()
		{
			Cout.println("Init Scale GUI: Screen.w=" + Screen.width + " Screen.h=" + Screen.height);
			WIDTH = Screen.width;
			HEIGHT = Screen.height;
			scaleScreen = false;
			_ = Screen.width;
		}
	}
}
