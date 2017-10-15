
using System;
using System.Drawing;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.Services;
using MyMood.DL;

namespace MyMood
{
	public partial class SetLocationViewController : UIViewController
	{


		private int selectedRegion;
		private string selectedRegionText;
		private bool acceptedTerms = false;

		public SetLocationViewController () : base ("SetLocationViewController", null)
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




			selectedRegion = -1;
			for (int i=1;i<10;i++)
			{
				UIButton btn = (UIButton) this.View.ViewWithTag(i);
				btn.TouchUpInside += (object sender, EventArgs e) => {
					Console.WriteLine("Selecting location {0}",i);
					//Console.WriteLine("Selecting location");
					int x = i;
					this.setLocation(btn.Tag);
				};
				btn.SetImage(Resources.UnselectedLocation,UIControlState.Normal);

			}

			// Perform any additional setup after loading the view, typically from a nib.
			btnConfirm.TouchUpInside += (object sender, EventArgs e) => {
				Continue();
			};

			ShowTerms();
		}

		protected void Continue ()
		{
			if (!acceptedTerms) {
				acceptedTerms = true;
				ShowRegions();
			} else {
				confirmLocation();
			}
		}

		protected void ShowTerms ()
		{
			this.RegionSelector.Hidden = true;
			this.Terms.Hidden = false;
			this.btnConfirm.SetTitle("Continue", UIControlState.Normal);
			this.Title.Text = "All myMood responses are anonymous";
		}

		protected void ShowRegions ()
		{
			this.RegionSelector.Hidden = false;
			this.Terms.Hidden = true;
			this.btnConfirm.SetTitle("Confirm", UIControlState.Normal);
			this.Title.Text = "Please select the region where you work:";
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
			return (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight);
		}

		[Export("setLocation:")]
		public void setLocation(int i)
		{
			Console.WriteLine("old Selected location = {0}",selectedRegionText);
			if (selectedRegion != -1)
			{
				UIButton btnSel = (UIButton) this.View.ViewWithTag(selectedRegion);
				//btnSel.ImageView.Image = Resources.UnselectedLocation;
				btnSel.SetImage(Resources.UnselectedLocation,UIControlState.Normal);
			}

			UIButton btn = (UIButton) this.View.ViewWithTag(i);
			btn.SetImage(Resources.SelectedLocation,UIControlState.Normal);
			selectedRegion = i;
			UILabel lbl =(UILabel) this.View.ViewWithTag(i+10);
			selectedRegionText = lbl.Text;
		}

		public void confirmLocation()
		{

			if (selectedRegion == -1)
			{
				UIAlertView unselectedAlert = new UIAlertView("myMood","Please select your region",null,"OK",null);
				unselectedAlert.Show();

				return;
			}

			var app = ApplicationState.Current;
			app.ResponderRegion = selectedRegionText;
			app.Save();

			MyMoodLogger.Current.Log("Region set - " + selectedRegionText, "", 2);

			this.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
			this.NavigationController.PopViewControllerAnimated(false);

		}
	}
}

