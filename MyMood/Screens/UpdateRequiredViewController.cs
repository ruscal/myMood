
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.DL;
using MyMood.Services;

namespace MyMood
{
	public partial class UpdateRequiredViewController : UIViewController
	{
		public UpdateRequiredViewController () : base ("UpdateRequiredViewController", null)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.

			var updateBtn = new UIButton(new RectangleF(451, 438, 154, 156));
			updateBtn.TouchUpInside += (object sender, EventArgs e) => {
				var a = ApplicationState.Current;
				var uri = a.UpdateAppUri ?? a.WANWebServiceUri;
				var updateUrl = UrlHelper.ToUpdateUrl(uri, a.EventName, a.PassCode);
				//reset update timestamps so force to update when reinstalled
				a.LastSuccessfulDataPush = null;
				a.LastSuccessfulServiceUpdate = null;
				a.Save();

				NSUrl nsu = new NSUrl(updateUrl);
				UIApplication.SharedApplication.OpenUrl(nsu);
			};
			this.Add(updateBtn);

			this.CancelBtn.TouchUpInside +=	 (object sender, EventArgs e) => {
				this.NavigationController.PopToRootViewController(true);
			};
			string version = NSBundle.MainBundle.ObjectForInfoDictionary ("Version").ToString ();
			var app = ApplicationState.Current;
			if(app.CurrentVersion == version) NavigationController.PopToRootViewController(false);

			if(app.ForceUpdate > 0){
				this.BackgroundImage.Image = Resources.UpdateRequiredBackground;
				this.CancelBtn.Hidden = true;
			}else{
				this.BackgroundImage.Image = Resources.UpdateAvailableBackground;
				this.CancelBtn.Hidden = false;
			}

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

