
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MediaPlayer;
using MyMood.DL;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.CoreAnimation;
using ApngPlayerBinding;
using MonoTouch.ObjCRuntime;
using Discover.Core;
using MyMood.Services;
using System.Threading;

namespace MyMood
{
	public partial class SetMyMoodViewController : UIViewController
	{

		Mood currentMood;
//		UIImageView overlayImage;
		//UIButton startButton;
		AVAnimatorMedia animatorMedia;
		ApngPlayerBinding.AVAnimatorView animatorView;


		public MoodPrompt CurrentPrompt {
			get;
			set;
		}

		public SetMyMoodViewController () : base ("SetMyMoodViewController", null)
		{
			this.Title = "Set Mood";
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
			this.View.Tag = 1001; // so we can check which if the setmymood view is current.
			this.moodBackground.Image = Resources.NeutralBackground;

			// Notification leads to crash when GC disposes of this controller but native code keeps handle on selector
			//NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("DatabaseReset:"), "DatabaseReset", null);

			displaySetMood();

		}

		private void displaySetMood()
		{
			btnInfo.Hidden = false;
			btnConfirm.Hidden = false;
			if (CurrentPrompt != null)
			{
				this.txtLog.Text = string.Format("{0:H:mm} {1}",CurrentPrompt.TimeStamp.ToLocalTime(ApplicationState.Current.EventTimeOffset), CurrentPrompt.NotificationText);;
			}
			else
			{
				this.txtLog.Text = string.Format("{0:H:mm} General Mood Log",DateTime.UtcNow.ToLocalTime(ApplicationState.Current.EventTimeOffset));
			}
			this.txtLog.TextColor = UIColor.White;
			this.txtPrompt.Hidden = true;
			this.txtPrompt.TextColor = UIColor.White;
			this.txtPrompt.Text = CurrentPrompt != null ? CurrentPrompt.NotificationText : "";
			this.txtFeel.Hidden = false;
			this.txtFeel.TextColor = UIColor.White;
			this.txtFeel.Text = CurrentPrompt != null && CurrentPrompt.ActiveTil < DateTime.UtcNow ? "What was your mood?" : "What is your mood?";
			this.txtMood.Hidden = false;
			this.txtMood.TextColor = UIColor.White;
			this.txtMood.Text = "";
			this.btnConfirm.Hidden = true;
			this.btnInfo.Enabled = true;
			this.btnCancel.Enabled = true;
			this.btnCancel.Alpha = 1;
			this.btnInfo.Alpha = 1;

			// Perform any additional setup after loading the view, typically from a nib.
			var moods = Mood.List ().OrderBy(m => m.DisplayIndex).ToList ();
			List<UIButton> btns = new List<UIButton> (){
				SetMood1,
				SetMood2,
				SetMood3,
				SetMood4,
				SetMood5,
				SetMood6,
				SetMood7,
				SetMood8,
				SetMood9,
				SetMood10
			};
			
			for (int i=0; i<moods.Count(); i++) {
				var mood = moods [i];
				var btn = btns [i];
				//btn.SetTitle (mood.Name, UIControlState.Normal);
				btn.TouchUpInside += (object sender, EventArgs e) => {
					displayMood (mood);
				};
			}

			btnInfo.TouchUpInside += (object sender, EventArgs e) => {
				ShowHelp();
			};
			btnConfirm.TouchUpInside += (object sender, EventArgs e) => {
				SetMyMood();
			};

			btnCancel.TouchUpInside += (object sender, EventArgs e) => {
				cancel();
			};
			animatorView = new ApngPlayerBinding.AVAnimatorView();//  (new RectangleF(100,100,200,200));
			animatorView.Frame = new RectangleF(300,160,400,400);
			animatorView.Image = Resources.NeutralCharacter;

			//animatorView.Bounds = new RectangleF(40,148,240,240);
			//animatorView.BackgroundColor = UIColor.Red;
			this.View.Add(animatorView);
		}

		private void ShowHelp()
		{
			SetMood1.Enabled = false;
			SetMood2.Enabled = false;
			SetMood3.Enabled = false;
			SetMood4.Enabled = false;
			SetMood5.Enabled = false;
			SetMood6.Enabled = false;
			SetMood7.Enabled = false;
			SetMood8.Enabled = false;
			SetMood9.Enabled = false;
			SetMood10.Enabled = false;
			btnInfo.Enabled = false;
			btnCancel.Enabled = false;
			txtFeel.Hidden = true;
			txtMood.Hidden = true;
			btnConfirm.Hidden = true;

			SelectMoodHelpOverlayView selectMoodHelp = new SelectMoodHelpOverlayView(this.View.Bounds);
			selectMoodHelp.Closed += (object sender, EventArgs e) => {
				EnableButtons();
			};
			this.Add(selectMoodHelp);
		}

		private void EnableButtons()
		{
			SetMood1.Enabled = true;
			SetMood2.Enabled = true;
			SetMood3.Enabled = true;
			SetMood4.Enabled = true;
			SetMood5.Enabled = true;
			SetMood6.Enabled = true;
			SetMood7.Enabled = true;
			SetMood8.Enabled = true;
			SetMood9.Enabled = true;
			SetMood10.Enabled = true;


			btnInfo.Hidden = false;
			btnConfirm.Hidden = false;
			//this.txtLog.Text = "08:30 General Mood Log";
			this.txtFeel.Hidden = false;
			this.txtMood.Hidden = false;
			this.btnConfirm.Hidden = true;
			this.btnInfo.Enabled = true;
			this.btnCancel.Enabled = true;
		}

		private void displayMood(Mood mood)
		{
			currentMood = mood;
			setMoodView(mood.Name);
			//SetMyMood (mood.Id);
		}

		private void setMoodView(string mood)
		{

			this.txtFeel.Text = CurrentPrompt != null && CurrentPrompt.ActiveTil < DateTime.UtcNow ? "Did you feel" : "Do you feel";
			this.txtMood.Hidden = false;
			this.txtFeel.Hidden = false;
			this.btnConfirm.Hidden = false;
			this.txtFeel.TextColor = UIColor.Black;
			this.txtMood.Text = string.Format("{0}?", mood);

			switch (mood)
			{
			case "Optimistic":
				this.moodBackground.Image = Resources.OptimisticBackground;
				animatorView.Image = Resources.OptimisticCharacter;
				break;
			case "Engaged":
				this.moodBackground.Image = Resources.EngagedBackground;
				animatorView.Image = Resources.EngagedCharacter;
				break;
			case "Proud":
				this.moodBackground.Image = Resources.ProudBackground;
				animatorView.Image = Resources.ProudCharacter;
				break;
			case "Excited":
				this.moodBackground.Image = Resources.ExcitedBackground;
				animatorView.Image = Resources.ExcitedCharacter;
				//playMedia("export_option7alphabest",false);
				break;
			case "Passionate":
				this.moodBackground.Image = Resources.PassionateBackground;
				animatorView.Image = Resources.PassionateCharacter;
				break;
			case "Frustrated":
				this.moodBackground.Image = Resources.FrustratedBackground;
				animatorView.Image = Resources.FrustratedCharacter;
				//playMedia("export_Frustrated",false);
				break;
			case "Worried":
				this.moodBackground.Image = Resources.WorriedBackground;
				animatorView.Image = Resources.WorriedCharacter;
				break;
			case "Bored":
				this.moodBackground.Image = Resources.BoredBackground;
				animatorView.Image = Resources.BoredCharacter;
				break;
			case "Deflated":
				this.moodBackground.Image = Resources.DeflatedBackground;
				animatorView.Image = Resources.DeflatedCharacter;
				break;
			case "Disengaged":
				this.moodBackground.Image = Resources.DisengagedBackground;
				animatorView.Image = Resources.DisengagedCharacter;
				break;
			default:

				break;
			
			}

		}

		private void SetMyMood ()
		{
			Console.WriteLine ("Set my mood");
			var mood = Mood.Read (currentMood.Id);
			Console.WriteLine ("Setting my mood to " + mood.Name);
			MoodResponse response = MoodResponse.New ();
			if (CurrentPrompt == null) {
				response.Id = System.Guid.NewGuid ().ToString ();
				response.TimeStamp = DateTime.UtcNow;
			} else {
				response = CurrentPrompt.Response;
			}
			response.Mood = mood;
			response.CreatedOn = DateTime.UtcNow;
			response.Save ();
			Console.WriteLine ("Mood response saved");
			var app = ApplicationState.Current;
			if (app.RunningMode != RunningMode.Normal) {
				app.RunningMode = RunningMode.Normal;
				app.Save ();
			}

			MyMoodLogger.Current.Log("Mood Response - " + mood.Name, CurrentPrompt == null ? "No prompt" : "Prompt: " + CurrentPrompt.Title, 2);

			//NSNotificationCenter.DefaultCenter.PostNotificationName("SyncDataAndNotifications",null);
			Console.WriteLine ("Lets create a thread for pushing response");



				System.Threading.Tasks.Task.Factory.StartNew(() =>{
					//NSTimer.CreateScheduledTimer (5, delegate {
						Console.WriteLine ("Submit response to server");
						MyMoodService.Current.SubmitMoodResponse(response);
						Console.WriteLine ("Mood response submitted");
					//});
				});
			

			if (CurrentPrompt != null)
			{
				Console.WriteLine ("Cancel notification");
				//if we have gone vie the timeline then we need to cancel the prompt so the user doesn't select it again from the notification drop down
				NotificationManager.CancelLocalNotification(CurrentPrompt);

				Console.WriteLine ("Update badge numbers");
				//decrease mood prompt badges
				UIApplication.SharedApplication.ApplicationIconBadgeNumber = MoodPrompt.GetOutstandingPrompts().Count ();
				//UIApplication.SharedApplication.ApplicationIconBadgeNumber = UIApplication.SharedApplication.ApplicationIconBadgeNumber -1;
			}

			CurrentPrompt = null;

			Console.WriteLine ("Done with setting mood - navigate back");
			this.NavigationController.PopViewControllerAnimated(true);
		}

		private void cancel()
		{
			this.NavigationController.PopViewControllerAnimated(true);
		}

		[Export ("DatabaseReset:")]
		void DatabaseReset (NSDictionary n)
		{
			this.cancel ();
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
			// This particular screen is landscape only!
			return (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight);
		}


		public void playMedia(string filename,bool convert)
		{
			
			AVAnimatorMedia media = new AVAnimatorMedia();
			media.animatorRepeatCount=10;
			animatorMedia = media;
			genericResourceLoader(filename,convert,media);
			
			this.animatorView.attachMedia(media);
			this.animatorView.media.startAnimator();
			
		}
		
		void genericResourceLoader(string resourcePrefix, bool convertToMvid, AVAnimatorMedia media)
			
		{
			string videoResourceArchiveName;
			string videoResourceEntryName;
			string videoResourceOutName;
			string videoResourceOutPath;
			
			
			string mvidResFilename = string.Format("{0}.mvid.7z", resourcePrefix  );
			string mvidResPath = NSBundle.MainBundle.PathForResource(mvidResFilename, null);
			
			bool convertToMvidLoader = true;
			if (convertToMvid == false) {
				convertToMvidLoader = false;
			}
			
			if (convertToMvid && (mvidResPath != null)) {
				// Extract existing FILENAME.mvid from FILENAME.mvid.7z attached as app resource
				videoResourceArchiveName = string.Format("{0}.mvid.7z", resourcePrefix);
				videoResourceEntryName = string.Format("{0}.mvid", resourcePrefix);
				string resourceTail = resourcePrefix;
				videoResourceOutName = string.Format("{0}.mvid", resourceTail);
				videoResourceOutPath = AVFileUtil.getTmpDirPath(videoResourceOutName);
				convertToMvidLoader = false;
			}  else if (convertToMvid) {
				// Extract to /tmp/FILENAME.mvid
				videoResourceArchiveName = string.Format("{0}.mov.7z", resourcePrefix);
				videoResourceEntryName = string.Format("{0}.mov", resourcePrefix);
				string resourceTail = resourcePrefix;
				videoResourceOutName = string.Format("{0}.mvid", resourceTail);
				videoResourceOutPath = AVFileUtil.getTmpDirPath(videoResourceOutName);
			}  else {
				// Extract to /tmp/FILENAME.mov
				videoResourceArchiveName = string.Format("{0}.mov.7z", resourcePrefix);
				videoResourceEntryName = string.Format("{0}.mov", resourcePrefix);
				string resourceTail = resourcePrefix;
				videoResourceOutName = string.Format("{0}.mov", resourceTail);
				videoResourceOutPath = AVFileUtil.getTmpDirPath(videoResourceOutName);
			}
			
			if (convertToMvidLoader) {
				AV7zQT2MvidResourceLoader resLoader = new AV7zQT2MvidResourceLoader();
				resLoader.archiveFilename = videoResourceArchiveName;
				resLoader.movieFilename = videoResourceEntryName;
				resLoader.outPath = videoResourceOutPath;
				media.resourceLoader = resLoader;
			}  else {
				AV7zAppResourceLoader resLoader = new AV7zAppResourceLoader();
				resLoader.archiveFilename = videoResourceArchiveName;
				resLoader.movieFilename = videoResourceEntryName;
				resLoader.outPath = videoResourceOutPath;
				
				media.resourceLoader = resLoader;
			}
			
			if (convertToMvid) {
				AVMvidFrameDecoder frameDecoder = new AVMvidFrameDecoder();
				media.frameDecoder = frameDecoder;
			}  else {
				AVQTAnimationFrameDecoder frameDecoder = new AVQTAnimationFrameDecoder();
				media.frameDecoder = frameDecoder;
			}
			
			return;
		}


	}
}

