

using System;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.DL;
using System.Linq;
using Discover.Drawing;
using MonoTouch.ObjCRuntime;
using MyMood.Services;


namespace OurMood.Touch
{
	public partial class LoadingReportViewController : UIViewController
	{
		OurMoodViewController moodMapController;
		UIImageView loadingImage;
		UILabel loadingTitle;
		bool loadingFirstReport = false;

		public LoadingReportViewController () : base ("LoadingReportViewController", null)
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
			Console.WriteLine ("Loading screen - ViewDidLoad");

			base.ViewDidLoad ();

			// Perform any additional setup after loading the view, typically from a nib.



			if (this.moodMapController == null)
				this.moodMapController = new OurMoodViewController ();


			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("RequestingReportStatus"), "RequestingReportStatus", null);

//			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("ReportImagesGenerated:"), "ReportImagesGenerated", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("NewReportReceived"), "NewReportReceived", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("NoReportData"), "NoReportData", null);


			var images = Resources.LoadingImages;

			loadingImage = new UIImageView (new RectangleF (0, 0, 1024, 748));
			loadingImage.AnimationImages = images.ToArray ();
			//loadingImage.Center = new PointF(512, 383);
			loadingImage.ContentMode = UIViewContentMode.Center;
			loadingImage.BackgroundColor = UIColor.Black;

			this.Add (loadingImage);

			 loadingTitle = new UILabel (new RectangleF (0, 300, 400, 60));
			loadingTitle.Text = "Loading data ....";
			loadingTitle.Center = new PointF (512, 300);
			loadingTitle.TextColor = UIColor.White;
			loadingTitle.BackgroundColor = UIColor.Clear;
			loadingTitle.TextAlignment = UITextAlignment.Center;
			this.Add (loadingTitle);

			loadingFirstReport = !MoodReport.HasValidReport;

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);


				if(!ReportManager.RefreshCurrentReport){
					MoodReport report = MoodReport.LatestGeneratedReport;
					if (report != null) {
						this.NavigationController.PushViewController (moodMapController, false);
					}
				}

		}

		public override void ViewDidAppear (bool animated)
		{
			Console.WriteLine("Loading page - ViewDidAppear");
			loadingTitle.Text = "Loading data ....";
			loadingImage.StartAnimating();
			MoodReport report = MoodReport.CurrentReport;
			if (report != null) {
				if(report.ImagesGenerated == 0){
					GenerateReportImages(report);
				}else{
					ReportImagesGenerated ();
				}
			}
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		protected void GenerateReportImages(MoodReport report){
			loadingTitle.Text = "Downloading data ...";
			System.Threading.Tasks.Task.Factory.StartNew(() =>{

				if(ReportManager.FetchReportDays(report).Success){
					InvokeOnMainThread (delegate {
						loadingTitle.Text = "Generating images ...";
					});
					ReportManager.GenerateReportImagesForAllLevels(report);

				}else{
					InvokeOnMainThread (delegate {
						loadingTitle.Text = "Download failed.";
					});
					//todo:pause

				}
				InvokeOnMainThread (delegate {
					ReportImagesGenerated();
				});
			});

		}

		[Export ("RequestingReportStatus")]
		void RequestingReportStatus (NSDictionary n)
		{
			NSString key = new NSString ("userInfo.Status");
			var status = n.ValueForKeyPath(key).ToString();

			InvokeOnMainThread (delegate {
				loadingTitle.Text = status;
			});
		}

		[Export ("NoReportData")]
		void NoReportData (NSDictionary n)
		{
			if (loadingFirstReport) {
				loadingFirstReport = false;
				Console.WriteLine ("No report data received in loading screen");
				InvokeOnMainThread (delegate {
					if (this.NavigationController.TopViewController.View.Handle == this.View.Handle) {
						ReportImagesGenerated ();
					}
				});
			}
		}

		[Export ("NewReportReceived")]
		void NewReportReceived (NSDictionary n)
		{
			if (loadingFirstReport) {
				loadingFirstReport = false;
				Console.WriteLine ("New report received in loading screen");
				InvokeOnMainThread (delegate {

					if (this.NavigationController.TopViewController.View.Handle == this.View.Handle) {
						MoodReport report = MoodReport.CurrentReport;
						GenerateReportImages (report);
						
					}

				});
			}
		}

		//[Export("ReportImagesGenerated:")]
		protected void ReportImagesGenerated ()
		{
			if (this.NavigationController.TopViewController.Handle == this.Handle) {

				Console.WriteLine ("Images ready in loading screen");

				var report = MoodReport.LatestGeneratedReport;

				ReportManager.DeleteAllOldReports (report);

				Console.WriteLine ("Old reports deleted");

				InvokeOnMainThread (delegate {
//				if(this.NavigationController.TopViewController.View.Handle == this.View.Handle){
//					Console.WriteLine("Reloading map controller");
//				if(this.moodMapController != null){
//					this.moodMapController.Dispose();
//					this.moodMapController = null;
//				}
//					this.moodMapController = new OurMoodViewController();
//					this.NavigationController.PushViewController(moodMapController, true);

					if (this.moodMapController == null || this.moodMapController.NeedsDisposing) {
						this.moodMapController = new OurMoodViewController ();
					} else {
						this.moodMapController.Refresh ();
					}
					this.NavigationController.PushViewController (moodMapController, true);
					//}

				});
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

