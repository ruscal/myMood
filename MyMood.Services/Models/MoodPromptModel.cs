using System;

namespace MyMood.Services
{
	public class MoodPromptModel
	{
		public string Id {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public ActivityModel Activity {
			get;
			set;
		}

		public string NotificationText {
			get;
			set;
		}

		public DateTime ActiveFrom {
			get;
			set;
		}

		public DateTime ActiveTil {
			get;
			set;
		}
	}
}

