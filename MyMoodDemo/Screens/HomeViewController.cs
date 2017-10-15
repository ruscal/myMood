
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using System.Collections.Generic;

namespace MyMoodDemo
{
	public partial class HomeViewController : UIViewController
	{
		int currentPos = 0;
		List<UIImageView> imageViews = new List<UIImageView>();

		public HomeViewController () : base ("HomeViewController", null)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.

			var images = new UIImage[]{
				UIImage.FromBundle("BG-v0.7.png"),
				UIImage.FromBundle("BG-v0.8.png")
			};

			var x = 1;
			foreach(var img in images){
				UIImageView imgView = new UIImageView(img);
				imgView.Bounds = new Rectangle(0, 0, 1024, 768);
				imgView.Center = new PointF(x*1024 - 512, 384);
				this.View.Add(imgView);
				imageViews.Add(imgView);
				x++;
			}

			var swipeRightGestureRecognizer = new UISwipeGestureRecognizer(this, new MonoTouch.ObjCRuntime.Selector("SwipeRightSelector"));
			swipeRightGestureRecognizer.Direction = UISwipeGestureRecognizerDirection.Right;
			View.AddGestureRecognizer(swipeRightGestureRecognizer);
			var swipeLeftGestureRecognizer = new UISwipeGestureRecognizer(this, new MonoTouch.ObjCRuntime.Selector("SwipeLeftSelector"));
			swipeLeftGestureRecognizer.Direction = UISwipeGestureRecognizerDirection.Left;
			View.AddGestureRecognizer(swipeLeftGestureRecognizer);

		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return true;
		}

		[Export("SwipeRightSelector")]
		protected void SwipeRight (UIGestureRecognizer sender)
		{
			if (currentPos > 0) {
				foreach(var iv in imageViews){
					iv.Center = new PointF(iv.Center.X+1024, iv.Center.Y);
				}
				currentPos -= 1024;
			}
		}
		
		[Export("SwipeLeftSelector")]
		protected void SwipeLeft (UIGestureRecognizer sender)
		{
			if (currentPos < 1024*(imageViews.Count-1)) {
				foreach(var iv in imageViews){
					iv.Center = new PointF(iv.Center.X-1024, iv.Center.Y);
				}
				currentPos += 1024;
			}
		}

		private void SwipeRight ()
		{
			if (currentPos < 1024*(imageViews.Count-1)) {
				foreach(var iv in imageViews){
					iv.Center = new PointF(iv.Center.X-1024, iv.Center.Y);
				}
				currentPos -= 1024;
			}
		}
	}
}

