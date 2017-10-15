using System;
using Vici.CoolStorage;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Linq;
using System.Collections.Generic;

namespace MyMood.DL
{
	[MapTo("MoodPrompt")]
	public class MoodPrompt : CSObject<MoodPrompt,string>
	{
		
		public string Id {
			get { return (string)GetField("Id"); }
			set{ SetField("Id", value); }
		}

		public string Name {
			get { return (string)GetField("Name"); }
			set{ SetField("Name", value); }
		}
		
		public string Title {
			get { return (string)GetField("Title"); }
			set{ SetField("Title", value); }
		}
		
		public DateTime TimeStamp {
			get { return (DateTime)GetField("TimeStamp"); }
			set{ SetField("TimeStamp", value); }
		}

		public string NotificationText {
			get { return (string)GetField("NotificationText"); }	
			set{ SetField("NotificationText", value); }
		}

		public DateTime ActiveFrom {
			get { return (DateTime)GetField("ActiveFrom"); }
			set{ SetField("ActiveFrom", value); }
		}

		public DateTime ActiveTil {
			get { return (DateTime)GetField("ActiveTil"); }
			set{ SetField("ActiveTil", value); }
		}

		public bool IsDefault {
			get {
				return Id == System.Guid.Empty.ToString();
			}
		}
	
		[Prefetch]
		[OneToOne(LocalKey="Id", ForeignKey="MoodPromptId")]
		public MoodResponse Response {
			get { return (MoodResponse)GetField("Response"); }
				set{ SetField("Response", value); }
		}


		public static int GetOutstandingPromptsCount()
		{
			return MoodPrompt.Count ("ActiveFrom < @Now and Response.Mood is NULL", new { Now = DateTime.UtcNow });
		}

		public static IEnumerable<MoodPrompt> GetOutstandingPrompts()
		{
			return GetActivePrompts(false);
		}

		public static IEnumerable<MoodPrompt> GetActivePrompts (bool includeRespondedPrompts)
		{
			var now = DateTime.UtcNow;
			if (includeRespondedPrompts) {
				return MoodPrompt.List ("ActiveFrom < @Now", new { Now = DateTime.UtcNow });
			} else {
				return MoodPrompt.List ("ActiveFrom < @Now and Response.Mood is NULL", new { Now = DateTime.UtcNow });
			}

			//return MoodPrompt.List ().Where(p => p.ActiveFrom < now && (includeRespondedPrompts || p.Responses.Count() == 0));
		}

		public static MoodPrompt GetCurrentPrompt(){

			return MoodPrompt.OrderedList("ActiveTil", "Response.Mood is NULL and ActiveFrom <= @Now and ActiveTil >= @Now", new { Now = DateTime.UtcNow })
				.LastOrDefault();
		}
	}
}

