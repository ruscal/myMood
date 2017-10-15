using System;
using Vici.CoolStorage;
using Discover.Core;

namespace MyMood.DL
{
	[MapTo("Activity")]
	public class Activity : CSObject<Activity,string>
	{
		public string Id {
			get { return (string)GetField("Id"); }
			set{ SetField("Id", value); }
		}

		public string Title {
			get { return (string)GetField("Title"); }
			set{ SetField("Title", value); }
		}

		public DateTime TimeStamp {
			get { return (DateTime)GetField("TimeStamp"); }
			set{ SetField("TimeStamp", value); }
		}

		public DateTime TimeStampLocal {
			get {
				return TimeStamp.ToLocalTime(ApplicationState.Current.EventTimeOffset);
			}
		}

		public ActivityType ActivityType {
			get { 
				var val = GetField("ActivityType");
				return (ActivityType)Enum.Parse(typeof(ActivityType), val.ToString()); }
			set{ SetField("ActivityType", value); }
		}

	}

	public enum ActivityType
	{
		MoodPrompt,
		Event
	}
}

