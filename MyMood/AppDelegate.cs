using System;
using System.Collections.Generic;
using System.Linq;
using MyMood.DL;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using MyMood.Services;
using System.Threading;
using MonoTouch.ObjCRuntime;
using System.IO;

namespace MyMood
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController navController;
		UILocalNotification lastNotif;
		MyMoodViciDbContext db;
		Thread syncThread;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			//init NavigationController
			var myMoodViewController = new MyMoodViewController ();
			this.navController = new UINavigationController (myMoodViewController);
			this.navController.SetNavigationBarHidden (true, false);

			if (app.RespondsToSelector(new Selector("setStatusBarHidden: withAnimation:")))
				app.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
			else
				app.SetStatusBarHidden(true, true);

			//init db
			string version = NSBundle.MainBundle.ObjectForInfoDictionary ("Version").ToString ();
			db = new MyMoodViciDbContext (version, null);

			NSNotificationCenter.DefaultCenter.AddObserver (this, new Selector ("SyncDataAndNotifications"), "SyncDataAndNotifications", null);

			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			CheckForGoLive ();

			this.ShowBadgeNumber ();

			startBackgroundThread ();
			var appState = ApplicationState.Current;

			if (version != appState.CurrentVersion && appState.ForceUpdate > 0) {
				UpdateRequiredViewController uc = new UpdateRequiredViewController ();
				navController.PushViewController (uc, true);
			} else {

				if (options != null) {
					NSString pId = new NSString ("PromptId");
					//Console.WriteLine("Launching with options - {0}", options);
					var startupLocalNotification = options.ValueForKey (UIApplication.LaunchOptionsLocalNotificationKey) as UILocalNotification;
					var startupRemoteNotification = options.ValueForKey (UIApplication.LaunchOptionsRemoteNotificationKey);
					if(startupLocalNotification != null && startupLocalNotification.UserInfo != null){
						//Console.WriteLine("Local notification prompt recieved as option");
						NSString promptId = (NSString)startupLocalNotification.UserInfo.ValueForKey (pId);
						myMoodViewController.promptID = promptId.ToString ();
					}else {

						//Console.WriteLine("Remote notification recieved as option");
						myMoodViewController.EnteredViaRemoteNotification = true;
					}
				}
			}
			window.RootViewController = this.navController;
			// make the window visible
			window.MakeKeyAndVisible ();	

			//Register for remote notifications
			UIApplication.SharedApplication.RegisterForRemoteNotificationTypes (UIRemoteNotificationType.Alert
				| UIRemoteNotificationType.Badge
				| UIRemoteNotificationType.Sound);

			return true;
		}

		public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
		{
			var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey ("PushDeviceToken");
			
			//There's probably a better way to do this
			var strFormat = new NSString ("%@");
			var dt = new NSString (MonoTouch.ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr (new MonoTouch.ObjCRuntime.Class ("NSString").Handle, new MonoTouch.ObjCRuntime.Selector ("stringWithFormat:").Handle, strFormat.Handle, deviceToken.Handle));
			var newDeviceToken = dt.ToString ().Replace ("<", "").Replace (">", "").Replace (" ", "");
			
			if (string.IsNullOrEmpty (oldDeviceToken) || !deviceToken.Equals (newDeviceToken)) {
				var app = ApplicationState.Current;
				app.APNSId = newDeviceToken;
				app.Save ();
			}
			
			//Save device token now
			NSUserDefaults.StandardUserDefaults.SetString (newDeviceToken, "PushDeviceToken");
			
			Console.WriteLine ("Device Token: " + newDeviceToken);
		}
		
		public override void FailedToRegisterForRemoteNotifications (UIApplication application, NSError error)
		{
			MyMoodLogger.Current.Log ("Failed registering for remote notifications", error.Description, 1);
		}
		
		public override void ReceivedRemoteNotification (UIApplication application, NSDictionary userInfo)
		{
			Console.WriteLine ("Received Remote Notification!");
			try {
				MyMoodLogger.Current.Log ("Push notification received", userInfo.ToString (), 2);

				if (application.ApplicationState == UIApplicationState.Active) {
					//Console.WriteLine("Remote notification received but app not active");						
				} else {
					//Console.WriteLine("Posting RemoteNotificationRecieved");
					NSNotificationCenter.DefaultCenter.PostNotificationName ("RemoteNotificationRecieved", null, userInfo);
						
				}

			} catch (Exception ex) {
				//Console.WriteLine ("Error on receiving remote notification - " + ex.ToString ());
				MyMoodLogger.Current.Error ("Error on push notification", ex, 2);
			}
		}

		public override void ReceivedLocalNotification (UIApplication app, UILocalNotification notif)
		{

			if (notif == lastNotif)
				return;
			lastNotif = notif;

			NSDictionary promptId = (NSDictionary)notif.UserInfo;
			this.ShowBadgeNumber ();

			if (app.ApplicationState == UIApplicationState.Active) {
				Console.WriteLine("Received local notification - app active");
				NSNotificationCenter.DefaultCenter.PostNotificationName ("newPromptRecieved", null, promptId);
			} else {
				Console.WriteLine("Received local notification - app not active");
				NSNotificationCenter.DefaultCenter.PostNotificationName ("newPromptBackgroundRecieved", null, promptId);
			}
		}

		//app has entered background mode i.e user has exited app
		public override void DidEnterBackground (UIApplication app)
		{
			Console.WriteLine ("App Entered Background");
			int bgTask = 0;
			bgTask = app.BeginBackgroundTask (() => {
				Console.WriteLine ("Background Task {0}", bgTask);
				SyncDataAndNotifications ();

			});
		}


		// app returning from background mode
		// check the background thread is still running if not restart it.
		public override void WillEnterForeground (UIApplication application)
		{
			Console.WriteLine ("App Entered Foreground");
			//check to see if our background thread is still running
			//if not restart it.
			if (syncThread == null || !syncThread.IsAlive) {
				Console.WriteLine ("Restarting background thread");
				startBackgroundThread ();

			}

			NSNotificationCenter.DefaultCenter.PostNotificationName ("newPromptRecieved", null);
		}

		[Export ("SyncDataAndNotifications")]
		void SyncDataAndNotificationsInBackground ()
		{

			System.Threading.Tasks.Task.Factory.StartNew (() => {
				SyncDataAndNotifications ();
			});

		}

		void SyncDataAndNotifications ()
		{
			//check before so can get latest update if reset
			CheckForGoLive ();

			Console.WriteLine ("SyncDataAndNotificationss");
			NotifySyncing ();
			ServiceSyncStatus status = MyMoodService.Current.SyncDataWithServer (); 
			NotifySyncComplete (status);

			string version = NSBundle.MainBundle.ObjectForInfoDictionary ("Version").ToString ();
			if (version != ApplicationState.Current.CurrentVersion && ApplicationState.Current.ForceUpdate > 0) {
				InvokeOnMainThread (delegate {
					NSNotificationCenter.DefaultCenter.PostNotificationName ("ForceVersionUpdate", null);
				});
			}

			if (status.HasPromptUpdates) {
				InvokeOnMainThread (delegate {
					Console.WriteLine ("Has Prompt Updates");

					NotificationManager.SyncNotificationsWithPrompts (MoodPrompt.All ());
					ShowBadgeNumber ();

					NSNotificationCenter.DefaultCenter.PostNotificationName ("PromptsUpdated", null);
				});
			
			}

			//check if go live has been set
			CheckForGoLive ();
		}

		void NotifySyncing ()
		{
			InvokeOnMainThread (delegate {
				NSNotificationCenter.DefaultCenter.PostNotificationName ("SyncingWithServer", null);
			});
		}

		void NotifySyncComplete (ServiceSyncStatus status)
		{
			InvokeOnMainThread (delegate {
				NSString key = new NSString ("SyncStatus");
				var level = status.SuccessLevel.ToString ();
				NSDictionary userInfo = NSDictionary.FromObjectAndKey (NSObject.FromObject (level), key);
				NSNotificationCenter.DefaultCenter.PostNotificationName ("SyncWithServerComplete", null, userInfo);
			});
		}

		void startBackgroundThread ()
		{
			syncThread = new Thread (new ThreadStart (() => {
				using (var pool = new NSAutoreleasePool ()) {
					NSTimer.CreateRepeatingScheduledTimer (MyMoodService.Current.SyncDataInterval, delegate {
						SyncDataAndNotifications ();
					});
					SyncDataAndNotifications ();
					NSRunLoop.Current.Run ();
				}
			}));
			syncThread.IsBackground = true;
			syncThread.Start ();
		}

		private void ShowBadgeNumber ()
		{
			UIApplication.SharedApplication.ApplicationIconBadgeNumber = MoodPrompt.GetOutstandingPrompts ().Count ();
		}

		private void CheckForGoLive ()
		{
			if (ApplicationState.Current.LiveDate > ApplicationState.Current.InitialisedOn && ApplicationState.Current.LiveDate <= DateTime.UtcNow) {
				string version = NSBundle.MainBundle.ObjectForInfoDictionary ("Version").ToString ();
				string deviceId = ApplicationState.Current.APNSId;
				db.ResetDatabase (version, deviceId);
				MyMoodLogger.Current.Log("Database gone live - all previous data reset", "",  1);
				InvokeOnMainThread (delegate {
					NSNotificationCenter.DefaultCenter.PostNotificationName ("DatabaseReset", null);
				});
			}

		}


	}
}

