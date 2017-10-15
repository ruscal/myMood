using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.DL;
using MyMood.AL;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

namespace MyMood
{
	public class TimelineColumnView : UIView
	{
	
		public int ColumnIndex {
			get;
			set;
		}

		protected UIImageView backgroundImage;

		public TimelineColumnView (int columnIndex)
			:base(new RectangleF(0, 0, 140, 560))
		{
			this.ColumnIndex = columnIndex;

			this.backgroundImage = new UIImageView(this.Bounds);
			this.backgroundImage.Bounds = new RectangleF(0, 0, 140, 560);
			this.backgroundImage.ContentMode = UIViewContentMode.ScaleToFill;
			this.backgroundImage.Opaque = true;
			this.backgroundImage.ClearsContextBeforeDrawing = true;
			this.backgroundImage.AutosizesSubviews = true;
			this.Add(this.backgroundImage);
		}
	}
}

