using System;
using System.Collections.Generic;
using System.Linq;
using MyMood.DL;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using MyMood.Services;
using System.Threading;
using MonoTouch.ObjCRuntime;

namespace OurMood.Touch
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
		Thread syncThread;
		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			//init NavigationController
			//var ourMoodViewController = new OurMoodViewController();


			MyMoodViciDbContext.Initialise();


			var loadingReportController = new LoadingReportViewController();
			this.navController = new UINavigationController(loadingReportController);
			this.navController.SetNavigationBarHidden(true,false);

			ReportManager.Current.StartSyncThread ();

			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);


			
			window.RootViewController = this.navController;
			// make the window visible
			window.MakeKeyAndVisible ();		
			return true;
		}

		public override void WillEnterForeground (UIApplication application)
		{
			Console.WriteLine ("App Entered Foreground");

			ReportManager.Current.StartSyncThread ();


		}


		
		private void appAlert (string name)
		{
			UIAlertView alert = new UIAlertView ("Our Mood Notification", name, null, "OK", null);
			alert.Show ();
			
		}
	}
}

