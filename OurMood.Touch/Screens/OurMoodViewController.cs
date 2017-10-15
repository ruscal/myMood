
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
	public partial class OurMoodViewController : UIViewController
	{
		GlobalMoodMapView globalMoodMapView;
		SyncStatusButtonView syncStatusBtnView;
		RefreshDataButtonView refreshBtn;
		UIButton supportBtn;
		MoodReport currentReport;

		public bool NeedsDisposing {
			get {
				return globalMoodMapView == null;
			}
		}

		public OurMoodViewController () : base ("OurMoodViewController", null)
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

		public override void LoadView ()
		{
			base.LoadView ();


//			this.View = globalMoodMap;
//			globalMoodMap.SetNeedsDisplay();

		}
		
		public override void ViewDidLoad ()
		{
			Console.WriteLine("Our mood ViewDidLoad");

			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.

			//MoodReport report = MoodReport.List().FirstOrDefault();

//			GlobalMoodMap map = new GlobalMoodMap(){
//				Width = (int)ChartContainer.Bounds.Width,
//				Height = (int)ChartContainer.Bounds.Height,
//				ReportStart = report.StartsOn,
//				ReportEnd = report.EndsOn,
//				Tension = 0.7f,
//				ShowDataPoints = false
//			};
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("NewReportReceived"), "NewReportReceived", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("SyncingWithServer:"), "SyncingWithServer", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("SyncWithServerComplete:"), "SyncWithServerComplete", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("HideSupportScreen:"), "HideSupportScreen", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("ShowSupportScreen:"), "ShowSupportScreen", null);

			//globalMoodMap = new GlobalMoodMapView(new RectangleF(0, 0, this.ChartContainer.Frame.Width, this.ChartContainer.Frame.Height), null, false, true, 0.7f);


			globalMoodMapView = new GlobalMoodMapView(this.ChartLayer.Bounds);
			this.ChartLayer.Add(globalMoodMapView);



			syncStatusBtnView = new SyncStatusButtonView(new RectangleF(10, 12, 25, 22));
			this.Add(syncStatusBtnView);
			//syncStatusBtnView.Hidden = true;

			refreshBtn = new RefreshDataButtonView();
			refreshBtn.Center = new PointF(this.View.Center.X, -20);
			refreshBtn.Refresh += (object sender, EventArgs e) => {
				RefreshImages();
			};
			this.Add(refreshBtn);

			supportBtn = new UIButton(UIButtonType.RoundedRect);
			supportBtn.Frame = new RectangleF(300, 60, 70, 30);
			supportBtn.SetTitle("Support", UIControlState.Normal);
			supportBtn.TouchUpInside += (object sender, EventArgs e) => {
				ShowSupportScreen();
			};
			supportBtn.Hidden = true;
			this.Add(supportBtn);

			this.currentReport = MoodReport.LatestGeneratedReport;
			globalMoodMapView.Refresh(this.currentReport);

			Console.WriteLine("Snapshot count: {0}", Snapshot.Count());
			Console.WriteLine("MoodSnapshot count: {0}", MoodSnapshot.Count());
			Console.WriteLine("Report count: {0}", MoodReport.Count());
			Console.WriteLine("Log count: {0}", Log.Count());

			//this.Refresh();
//
//
//			var reports = MoodReport.All().ToList();
//			var snapshots = MoodSnapshot.Count();
//			var currenSnapshots = MoodReport.CurrentReport.Snapshots.Count();

		}

		public override void ViewDidAppear (bool animated)
		{
			Console.WriteLine("Our mood ViewDidAppear");
			//this.refreshBtn.Show();
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

		public void Refresh ()
		{
			this.currentReport = MoodReport.LatestGeneratedReport;
			globalMoodMapView.Refresh(this.currentReport);
			//this.refreshBtn.Show();
		}

		protected void RefreshImages ()
		{
			ReportManager.RefreshCurrentReport = true;
				this.NavigationController.PopViewControllerAnimated (true);
		}

		[Export ("ShowSupportScreen:")]
		private void ShowSupportBtn ()
		{
			this.supportBtn.Hidden = false;
		}
		
		[Export ("HideSupportScreen:")]
		private void HideSupportBtn ()
		{
			this.supportBtn.Hidden = true;
		}

		[Export ("NewReportReceived")]
		void NewReportReceived (NSDictionary n)
		{


			InvokeOnMainThread (delegate {

				//this.NavigationController.PopToRootViewController(true);
				refreshBtn.Show();
			});
		}

		[Export ("SyncingWithServer:")]
		void SyncingWithServer (NSDictionary n)
		{
			this.syncStatusBtnView.ShowSyncing();
		}
		
		
		
		[Export ("SyncWithServerComplete:")]
		void SyncWithServerComplete (NSDictionary n)
		{
			NSString key = new NSString ("userInfo.SyncStatus");
			
			SyncSuccessLevel status = (SyncSuccessLevel)Enum.Parse(typeof(SyncSuccessLevel), n.ValueForKeyPath(key).ToString());
			this.syncStatusBtnView.ShowStatus(status);
		}

		private void ShowSupportScreen ()
		{
			SupportViewController supportVC = new SupportViewController ();
			this.NavigationController.PushViewController (supportVC, true);
		}

		protected override void Dispose (bool disposing)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver (this);


			if (this.globalMoodMapView != null) {
				this.globalMoodMapView.Dispose ();
				this.globalMoodMapView = null;
			}

			if (this.supportBtn != null) {
				this.supportBtn.Dispose ();
				this.supportBtn = null;
			}

			if (this.refreshBtn != null) {
				this.refreshBtn.Dispose ();
				this.refreshBtn = null;
			}

			if (this.syncStatusBtnView != null) {
				this.syncStatusBtnView.Dispose ();
				this.syncStatusBtnView = null;
			}
			this.currentReport = null;

			base.Dispose (disposing);
		}
	}
}

