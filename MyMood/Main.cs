using System;
using System.Collections.Generic;
using System.Linq;


using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MyMood
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
				var ex = (Exception)e.ExceptionObject;

				Console.WriteLine ("Exception in application - {0}", ex);
				throw ex;
			};

			try {
				// if you want to use a different Application Delegate class from "AppDelegate"
				// you can specify it here.
				UIApplication.Main (args, null, "AppDelegate");

			} catch (Exception ex) {
				Console.WriteLine("Exception in main thread - {0}", ex);
				throw ex;
			}
		}
	}
}
