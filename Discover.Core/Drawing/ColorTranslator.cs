using MonoTouch.CoreGraphics;
	
namespace Discover.Drawing
{
	public static class ColorTranslator
	{
		public static CGColor ToCGColor (string htmlColor)
		{
			float r = ToFloat (ToR (htmlColor));
			float g = ToFloat (ToG (htmlColor));
			float b = ToFloat (ToB (htmlColor));
			var col = new CGColor (r, g, b);

			return col;
		}

		private static float ToFloat (int component)
		{
			return (float)component / 255f;
		}

		public static int ToR (string htmlColor)
		{
			if (htmlColor.Length != 7 || !htmlColor.StartsWith ("#"))
				return 0;
			return int.Parse (htmlColor.Substring (1, 2), System.Globalization.NumberStyles.HexNumber);
		}

		public static int ToG (string htmlColor)
		{
			if (htmlColor.Length != 7 || !htmlColor.StartsWith ("#"))
				return 0;
			return int.Parse (htmlColor.Substring (3, 2), System.Globalization.NumberStyles.HexNumber);
		}

		public static int ToB (string htmlColor)
		{
			if (htmlColor.Length != 7 || !htmlColor.StartsWith ("#"))
				return 0;
			return int.Parse (htmlColor.Substring (5, 2), System.Globalization.NumberStyles.HexNumber);
		}
	}
}


