
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.DL;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using MonoTouch.ObjCRuntime;
using MyMood.Services;
using OurMood.Touch;

namespace OurMood.Touch
{
	public partial class SupportViewController : UIViewController
	{
		SyncStatusButtonView syncStatusBtnView;

		public SupportViewController () : base ("SupportViewController", null)
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


			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("SyncingWithServer:"), "SyncingWithServer", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("SyncWithServerComplete:"), "SyncWithServerComplete", null);

			syncStatusBtnView = new SyncStatusButtonView(new RectangleF(300, 80, 25, 22));
			this.Add(syncStatusBtnView);
		
			CancelBtn.TouchUpInside += (object sender, EventArgs e) => {
				this.NavigationController.PopViewControllerAnimated(true);
			};

			SyncDataBtn.TouchUpInside += (object sender, EventArgs e) => {
				NSNotificationCenter.DefaultCenter.PostNotificationName("SyncDataAndNotifications",null);
			};

			RefreshViewBtn.TouchUpInside += (object sender, EventArgs e) => {
				Refresh();
			};

			SaveChangesBtn.TouchUpInside += (object sender, EventArgs e) => {
				SaveApp();
			};

			LogsBtn.TouchUpInside += (object sender, EventArgs e) => {
				ShowLogs();
			};

			PromptsBtn.TouchUpInside += (object sender, EventArgs e) => {
				ShowPrompts();
			};

			NotificationsBtn.TouchUpInside += (object sender, EventArgs e) => {
				ShowNotifications();
			};

			Refresh();

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
			var app = ApplicationState.Current;
			string version = NSBundle.MainBundle.ObjectForInfoDictionary ("Version").ToString ();
			this.CurrentVersion.Text = version;
			this.EventName.Text = app.EventName;
			this.EventPasscode.Text = app.PassCode;
			this.GoLiveDate.Text = app.LiveDate.ToLocalTime().ToString("dd MMM yyyy H:mm:ss");
			this.LANServiceUri.Text = app.LANWebServiceUri;
			this.LastSuccessfulDataPush.Text = app.LastSuccessfulDataPush.HasValue
				? app.LastSuccessfulDataPush.Value.ToLocalTime().ToString("dd MMM yyyy H:mm:ss")
					: "Never";
			this.LastSuccessfulServiceUpdate.Text = app.LastSuccessfulServiceUpdate.HasValue
				? app.LastSuccessfulServiceUpdate.Value.ToLocalTime().ToString("dd MMM yyyy H:mm:ss")
					: "Never";
			this.ResponderId.Text = app.ResponderId;
			this.ResponderRegion.Text = app.ResponderRegion;
			this.RunningMode.Text = app.RunningMode.ToString();
			this.SyncDataInterval.Text = app.SyncDataInterval.ToString();
			this.SyncMode.Text = app.SyncMode.ToString();
			this.WANServiceUri.Text = app.WANWebServiceUri;
			this.DatabaseSize.Text = MyMoodViciDbContext.DbFileSize.ToString() + "K";
			this.EventUTCOffset.Text = app.EventTimeOffset.ToString();
			this.UpdateAppUri.Text = app.UpdateAppUri;
			this.ConnectionTimeout.Text = app.ConnectionTimeout.ToString();
			this.DBInitialisedOn.Text = app.InitialisedOn.ToLocalTime().ToString("dd MMM yyyy H:mm:ss");

			this.LogView.Source = new LogTableSource();
			this.LogView.ReloadData();
			this.PromptView.Source = new ActivityTableSource();
			this.PromptView.ReloadData();
			this.NotificationView.Source = new SnapshotTableSource();
			this.NotificationView.ReloadData();
		}

		public void ShowLogs ()
		{
			//this.LogView.Source = new LogTableSource();
			//this.LogView.ReloadData();
			this.PromptView.Hidden = true;
			this.NotificationView.Hidden = true;
			this.LogView.Hidden = false; 

		}

		public void ShowPrompts ()
		{
			//this.PromptView.Source = new MoodPromptTableSource();
			//this.PromptView.ReloadData();
			this.PromptView.Hidden = false;
			this.NotificationView.Hidden = true;
			this.LogView.Hidden = true; 
		}

		public void ShowNotifications ()
		{
			//this.NotificationView.Source = new NotificationTableSource();
			//this.NotificationView.ReloadData();
			this.PromptView.Hidden = true;
			this.NotificationView.Hidden = false;
			this.LogView.Hidden = true; 
		}


		public void SaveApp ()
		{
			var app = ApplicationState.Current;

			app.LANWebServiceUri = LANServiceUri.Text;
			app.WANWebServiceUri = WANServiceUri.Text;
			app.UpdateAppUri = UpdateAppUri.Text;
			app.EventName = EventName.Text;
			app.PassCode = EventPasscode.Text;
			app.Save();

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
	}
}

