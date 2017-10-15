
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using MyMood.DL;

namespace MyMood
{
	public partial class IntroductionViewController : UIViewController
	{
		public IntroductionViewController () : base ("IntroductionViewController", null)
		{
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
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
			
			UIImageView backgroundImage = new UIImageView (new RectangleF (0, 0, 1024, 768));
			this.Add(backgroundImage);
			if (ApplicationState.Current.RunningMode == RunningMode.FirstUse) {
				backgroundImage.Image = Resources.IntroductionBackground;
				UIButton setMyMoodBtn = new UIButton(new RectangleF(825, 562, 175, 180 ));
				//setMyMoodBtn.SetBackgroundImage(Resources.AddMoodButton, UIControlState.Normal);
				this.Add(setMyMoodBtn);
				setMyMoodBtn.TouchUpInside += (object sender, EventArgs e) => {
					this.NavigateToTimeline();
				};
			} else {
				backgroundImage.Image = Resources.InfoBackground;
				UIButton closeBtn = new UIButton(new RectangleF(910,700,78,32));
				closeBtn.SetBackgroundImage(Resources.CloseIntroButton, UIControlState.Normal);
				this.Add(closeBtn);
				closeBtn.TouchUpInside += (object sender, EventArgs e) => {
					this.NavigateToTimeline();
				};
			}



		}

		protected void NavigateToTimeline()
		{
			this.NavigationController.PopToRootViewController(false);
			//this.PresentViewController (new MyMoodViewController (), true, null);
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
	}
}

