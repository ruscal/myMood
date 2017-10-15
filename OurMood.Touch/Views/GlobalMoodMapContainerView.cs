using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using MyMood.DL;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using Discover.Drawing;
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreAnimation;
using Discover.Core;

namespace OurMood.Touch
{
	public class GlobalMoodMapContainerView : UIView
	{
		public GlobalMoodMapContainerView (RectangleF frame)
			:base (frame)
		{
		}

		public override CGAffineTransform Transform {
			get {
				return base.Transform;
			}
			set {
				value.yy = 1;
				base.Transform = value;
			}
		}
	}
}

