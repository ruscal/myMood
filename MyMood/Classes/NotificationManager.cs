using System;
using MyMood.DL;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Discover.Core;

namespace MyMood
{
	public static class NotificationManager
	{
		public static void SyncNotificationsWithPrompts (IEnumerable<MoodPrompt> prompts)
		{
			var notifications = UIApplication.SharedApplication.ScheduledLocalNotifications;
			var existing = new List<MoodPrompt> ();

			foreach (var n in notifications) {
				var val = n.UserInfo.ObjectForKey (NSObject.FromObject ("PromptId"));
				var prompt = prompts.FirstOrDefault (p => p.Id == val.ToString ());
				if (prompt == null || prompt.Response != null) {
					UIApplication.SharedApplication.CancelLocalNotification (n);
				} else {
					existing.Add (prompt);
					var fireDate = DateTime.SpecifyKind (n.FireDate, DateTimeKind.Unspecified);
					if (prompt.TimeStamp != fireDate) {
						UIApplication.SharedApplication.CancelLocalNotification (n);
						ScheduleLocalNotification (prompt);
					}
				}
			}

			var promptsToAdd = prompts.Where (p => p.TimeStamp > DateTime.UtcNow && !existing.Any (e => e.Id == p.Id));

			foreach (var pa in promptsToAdd) {
				Console.WriteLine("Scheduling Notification");
				ScheduleLocalNotification(pa);
			}
		}


		public static void CancelLocalNotification (MoodPrompt prompt)
		{
			if (prompt != null) {
				foreach (var n in UIApplication.SharedApplication.ScheduledLocalNotifications) {
					var val = n.UserInfo.ObjectForKey (NSObject.FromObject ("PromptId"));
					if (val.ToString () == prompt.Id){
						UIApplication.SharedApplication.CancelLocalNotification (n);
					}
				}
			}
		}

		public static void ScheduleLocalNotification (MoodPrompt prompt)
		{
			if (prompt != null) {
				
				var keys = new object[] { "PromptId" };
				var objects = new object[] { prompt.Id };
				var userInfo = NSDictionary.FromObjectsAndKeys (objects, keys);
				var fireDate = DateTime.SpecifyKind(prompt.ActiveFrom, DateTimeKind.Utc);

				UILocalNotification notification = new UILocalNotification{
				FireDate = fireDate.ToLocalTime().ToNSDate(),
				TimeZone = NSTimeZone.FromName("UTC"),
				AlertBody = prompt.NotificationText,
				RepeatInterval = 0,
				AlertAction = "Set my mood",
				UserInfo = userInfo
				
			};
				UIApplication.SharedApplication.ScheduleLocalNotification (notification);
			}
		}
	}
}

