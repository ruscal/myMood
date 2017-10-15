
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
using MonoTouch.ObjCRuntime;

using MyMood.Services;

namespace MyMood
{
	public partial class MyMoodViewController : UIViewController
	{
		OutstandingPromptsView outstandingPromptsView;
		SyncStatusButtonView syncStatusBtnView;
		public int _currentPageIndex;
		bool _introductionShown = false;
		UIView backView;
		public bool EnteredViaRemoteNotification = false;


		// set to true to display the initial overlay.
		public string promptID {
			get;
			set;
		}

		public MyMoodViewController () : base ("MyMoodViewController", null)
		{
			this.Title = "myMood";
			//this.TabBarItem.Image = UIImage.FromBundle ("myMood");
			this.PrefersStatusBarHidden ();
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

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// This particular screen is landscape only!
			return (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Console.WriteLine ("ViewDidLoad");

			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("DatabaseReset:"), "DatabaseReset", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("RemoteNotificationRecieved:"), "RemoteNotificationRecieved", null);

			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("HideSupportScreen:"), "HideSupportScreen", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("ShowSupportScreen:"), "ShowSupportScreen", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("ForceVersionUpdate:"), "ForceVersionUpdate", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("PromptsUpdated:"), "PromptsUpdated", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("SyncingWithServer:"), "SyncingWithServer", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("SyncWithServerComplete:"), "SyncWithServerComplete", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("newPromptRecieved:"), "newPromptRecieved", null);
			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("newPromptBackgroundRecieved:"), "newPromptBackgroundRecieved", null);

			btnInfo.TouchUpInside += (object sender, EventArgs e) => {
				//ShowHelp ();
				NavigateToIntroduction ();
			};
			btnReport.TouchUpInside += (object sender, EventArgs e) => {
				ShowReport ();
			};

			btnInterest.TouchUpInside += (object sender, EventArgs e) => {
				ShowInterest ();
			};

			SupportBtn.TouchUpInside += (object sender, EventArgs e) => {
				ShowSupportScreen ();
			};

			UpdateBtn.TouchUpInside += (object sender, EventArgs e) => {
				this.NavigateToUpdateApp ();
			};

			this.tableView.AddNewMood += (object sender, AddNewResponseEventArgs e) => {
				this.AddNewMood (e.CurrentPrompt);
			};

			syncStatusBtnView = new SyncStatusButtonView (new RectangleF (10, 12, 25, 22));
			this.Add (syncStatusBtnView);
			SupportBtn.Hidden = true;



			// Perform any additional setup after loading the view, typically from a nib.
			//Refresh ();
		}

		public override void ViewWillAppear (bool animated)
		{
			Console.WriteLine ("ViewWillAppear");
			base.ViewWillAppear (animated);

			//check for updates

			this.DetermineState();

		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			Console.WriteLine ("ViewDidAppear");


			//Refresh ();
			if (this.tableView != null) {
				this.tableView.SnapToEnd ();
			}

			this.ShowOutstandingPromptsView ();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);

			this.tableView.clearViews ();
		}

//		private void ShowHelp ()
//		{
//			MoodHelpOverlayView helpView = new MoodHelpOverlayView (this);
//			helpView.show ();
//		}

		protected void DetermineState ()
		{
			Console.WriteLine("Determin state");
			var app = ApplicationState.Current;
			
			string version = NSBundle.MainBundle.ObjectForInfoDictionary ("Version").ToString ();
			if (app.CurrentVersion != version && app.ForceUpdate > 0) {
				this.NavigateToUpdateApp ();
			} else {
				UpdateBtn.Hidden = app.CurrentVersion == version;
				if (string.IsNullOrWhiteSpace (app.ResponderRegion)) {
					NavigateToSetMyLocation ();
				} else {
					if (!string.IsNullOrWhiteSpace (this.promptID)) {
						var targetPrompt = MoodPrompt.List ().FirstOrDefault (p => p.Id == promptID);
						this.promptID = null;
						NavigateToSetMyMood (targetPrompt);
					} else if (app.RunningMode == RunningMode.FirstUse) {
						if (! _introductionShown) {
							_introductionShown = true;
							NavigateToIntroduction ();
						} else {
							_introductionShown = false;
							NavigateToSetMyMood (null);
						}
					} else {
						if(EnteredViaRemoteNotification){
							EnteredViaRemoteNotification = false;
							NavigateToSetMyMood (null);
						}else{
							Refresh ();
						}
					}
				}
			}
		}

		private void ShowReport ()
		{
			coverBackground ();
			RectangleF reportBounds = new RectangleF (650, 40, 265, 314);
			RequestReportDialogView reportRequest = new RequestReportDialogView (reportBounds);
			reportRequest.Closed += (object sender, EventArgs e) => {
				btnReport.Enabled = true;
				uncoverBackground ();
			};
			this.Add (reportRequest);
			btnReport.Enabled = false;
		}
		
		private void ShowInterest ()
		{
			coverBackground ();
			RectangleF interestBounds = new RectangleF (700, 40, 265, 314);
			RegisterInterestDialogView interestRequest = new RegisterInterestDialogView (interestBounds);
			interestRequest.Closed += (object sender, EventArgs e) => {
				btnInterest.Enabled = true;
				uncoverBackground ();
			};
			this.Add (interestRequest);
			btnInterest.Enabled = false;
		}

		private void coverBackground ()
		{
			backView = new UIView (this.View.Bounds);
			backView.BackgroundColor = UIColor.Black;
			backView.Alpha = 0f;
			this.Add (backView);

			UIView.Animate (0.5, 0, UIViewAnimationOptions.TransitionNone, () => {
				backView.Alpha = 0.5f;},
			() => {
			}
			);
		}

		private void uncoverBackground ()
		{
			UIView.Animate (0.5, 0, UIViewAnimationOptions.TransitionNone, () => {
				backView.Alpha = 0f;},
			() => {
				backView.RemoveFromSuperview ();
			}
			);

		}

		[Export ("ForceVersionUpdate:")]
		private void NavigateToUpdateApp ()
		{
			UpdateRequiredViewController uc = new UpdateRequiredViewController ();
			this.NavigationController.PushViewController (uc, true);
		}

		private void NavigateToIntroduction ()
		{
			IntroductionViewController introVC = new IntroductionViewController ();
			//this.PresentViewController (introVC, false, null);
			this.NavigationController.PushViewController (introVC, false);
		}

		private void NavigateToSetMyLocation ()
		{
			SetLocationViewController setLocVC = new SetLocationViewController ();
			this.NavigationController.PushViewController (setLocVC, true);
		}

		private void NavigateToSetMyMood (MoodPrompt currentPrompt)
		{
			if(currentPrompt == null) currentPrompt = MoodPrompt.GetCurrentPrompt();
			SetMyMoodViewController setMoodVC = new SetMyMoodViewController ();
			setMoodVC.CurrentPrompt = currentPrompt == null || currentPrompt.Response.Mood != null ? null : currentPrompt;				
			this.NavigationController.PushViewController (setMoodVC, true);
		}

		[Export ("ShowSupportScreen:")]
		private void ShowSupportBtn ()
		{
			this.SupportBtn.Hidden = false;
		}

		[Export ("HideSupportScreen:")]
		private void HideSupportBtn ()
		{
			this.SupportBtn.Hidden = true;
		}

		private void ShowSupportScreen ()
		{
			SupportViewController supportVC = new SupportViewController ();
			this.NavigationController.PushViewController (supportVC, true);
		}

		private void ShowOutstandingPromptsView ()
		{
			if (outstandingPromptsView == null) {
				var posX = this.View.Center.X - 339f / 2f;
				outstandingPromptsView = new OutstandingPromptsView (new RectangleF (posX, 820, 339, 55));
				outstandingPromptsView.JumpToPrompt += (object sender, JumpToPromptEventArgs e) => {
					this.tableView.SnapToPrompt (e.Prompt);
				};
				this.Add (outstandingPromptsView);


			}
			outstandingPromptsView.Refresh ();


		}

		public void Refresh ()
		{
			if (this.tableView != null) {
				this.tableView.Refresh ();
				this.tableView.SnapToEnd ();
			}

			Console.WriteLine ("Finished Loading & refreshed");
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

		[Export ("DatabaseReset:")]
		void DatabaseReset (NSDictionary n)
		{
			this.DetermineState ();
		}

		[Export ("PromptsUpdated:")]
		void PromptsUpdated (NSDictionary n)
		{
			if (this.outstandingPromptsView != null) {
				this.Refresh ();
				this.outstandingPromptsView.Refresh ();
			}
		}

		[Export ("SyncingWithServer:")]
		void SyncingWithServer (NSDictionary n)
		{
			this.syncStatusBtnView.ShowSyncing ();
		}

		[Export ("SyncWithServerComplete:")]
		void SyncWithServerComplete (NSDictionary n)
		{
			NSString key = new NSString ("userInfo.SyncStatus");

			SyncSuccessLevel status = (SyncSuccessLevel)Enum.Parse (typeof(SyncSuccessLevel), n.ValueForKeyPath (key).ToString ());
			this.syncStatusBtnView.ShowStatus (status);
		}

		[Export ("newPromptRecieved:")]
		void newPromptRecieved (NSDictionary n)
		{
			this.Refresh ();
			this.ShowOutstandingPromptsView ();

		}

		[Export ("RemoteNotificationRecieved:")]
		void RemoteNotificationRecieved (NSDictionary n)
		{
			Console.WriteLine("Remote notification received on timeline view");
			if (this.NavigationController != null 
				&& this.NavigationController.TopViewController != null
				&& this.NavigationController.TopViewController.View != null) {
				UIView topView = this.NavigationController.TopViewController.View;
				//If we are already displaying a setmy mood then return
				if (topView.Tag == 1001) {
					return;
					//currentViewController.NavigationController.PopViewControllerAnimated(true);
				}
			
				this.NavigateToSetMyMood (null);
			} else {
				EnteredViaRemoteNotification = true;
			}
		}

		[Export ("newPromptBackgroundRecieved:")]
		void newPromptBackgroundRecieved (NSDictionary n)
		{
			UIView topView = this.NavigationController.TopViewController.View;
			//If we are already displaying a setmy mood then return
			if (topView.Tag == 1001) {
				return;
				//currentViewController.NavigationController.PopViewControllerAnimated(true);
			}
			Console.WriteLine ("newPromptBackground {0}", n);
			//this.appAlert("New Prompt from notification");
			NSString pId = new NSString ("userInfo.PromptId");
			var id = n.ValueForKeyPath (new NSString ("userInfo.PromptId")) ?? n.ValueForKeyPath (new NSString ("PromptId"));
			if (id != null) {
				this.NavigateToSetMyMood (MoodPrompt.List ().FirstOrDefault (p => p.Id.Equals (id.ToString (), StringComparison.InvariantCultureIgnoreCase)));
			}
		}

//		void appAlert (string name)
//		{
//			UIAlertView alert = new UIAlertView ("My Mood Notification", name, null, "OK", null);
//			alert.Show ();			
//		}

		private void AddNewMood (MoodPrompt currentPrompt)
		{
			this.outstandingPromptsView.Hide ();

			this.NavigateToSetMyMood (currentPrompt);
		}


	
	}
	

}

