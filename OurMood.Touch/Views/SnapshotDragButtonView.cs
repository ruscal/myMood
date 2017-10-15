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
	public class SnapshotDragButtonView : UIImageView
	{
		PointF location;
		PointF startLocation;
		float minX;
		float maxX;

		//UIButton button;


		public SnapshotToolView DragTarget {
			get;
			set;
		}

		public SnapshotDragButtonView (PointF position)
			:base(new RectangleF(new PointF(0, 50) , new SizeF(100f, 50f)))
		{


			this.UserInteractionEnabled = true;
			this.Image = Resources.SnapshotSliderBtn;
//			this.button = new UIButton(this.Bounds);
//			this.button.TouchUpInside += (object sender, EventArgs e) => {
//				Console.WriteLine("Touch up");
//			};
//			this.Add(this.button);
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);
			this.minX =  -this.Frame.Width/2;
			this.maxX = this.Superview.Frame.Width - this.Frame.Width/2;
		}

		public override void DrawRect (RectangleF area, UIViewPrintFormatter formatter)
		{
			base.DrawRect (area, formatter);
			this.minX =  -this.Frame.Width/2;
			this.maxX = this.Superview.Frame.Width - this.Frame.Width/2;
		}


		public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
		{  

			this.minX =  -this.Frame.Width/2;
			this.maxX = this.Superview.Frame.Width - this.Frame.Width/2;
			//Console.WriteLine ("Touched the object");  
			location = this.Frame.Location;  
			
			var touch = (UITouch)e.TouchesForView (this).AnyObject;  
			var bounds = Bounds;  
			
			startLocation = touch.LocationInView (this);  
			this.Frame = new RectangleF (location, bounds.Size);  
			
		}  

	
		
		//This event occurs when you drag it around  
		public override void TouchesMoved (MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
		{  
			//Console.WriteLine ("Dragged the object");  
			var bounds = Bounds;  
			var touch = (UITouch)e.TouchesForView (this).AnyObject;  
			
			//Always refer to the StartLocation of the object that you've been dragging.
			var changeX = touch.LocationInView (this).X - startLocation.X;
			var newX = location.X + changeX;
			if (newX >= minX && newX <= maxX) {
				location.X = newX;  
				if (this.DragTarget != null) {
					this.DragTarget.MoveX(changeX);
				}
			}
			//location.Y += touch.LocationInView (this).Y - startLocation.Y;  
			
			
			this.Frame = new RectangleF (location, bounds.Size);  

		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			if (this.DragTarget != null) {
				this.DragTarget.Refresh();
			}
		}
	}
}

