using System;
using Vici.CoolStorage;
using System.Collections.Generic;

namespace MyMood.DL
{
	[MapTo("MoodResponse")]
	public class MoodResponse : CSObject<MoodResponse,string>
	{

		public string Id {
			get { return (string)GetField("Id"); }
			set{ SetField("Id", value); }
		}

		[Prefetch]
		[ManyToOne(LocalKey="MoodId", ForeignKey="Id")]
		public Mood Mood {
			get{ return (Mood) GetField("Mood");} set{ SetField("Mood", value);}
		}

		[Prefetch]
		[OneToOne(LocalKey="MoodPromptId", ForeignKey="Id")]
		public MoodPrompt Prompt {
			get{ return (MoodPrompt) GetField("Prompt");} set{ SetField("Prompt", value);}
		}
		
		public DateTime TimeStamp {
			get { return (DateTime)GetField("TimeStamp"); }
			set{ SetField("TimeStamp", value); }
		}

		public DateTime CreatedOn {
			get { return (DateTime)GetField("CreatedOn"); }
			set{ SetField("CreatedOn", value); }
		}

		public static IEnumerable<MoodResponse> GetUnpromptedResponses(){
			return MoodResponse.List ("Prompt is NULL");
		}

		public static IEnumerable<MoodResponse> GetConfirmedResponses(){
			return MoodResponse.List ("Not(Mood is NULL)");
		}

		public static int GetConfirmedResponseCount(){
			return MoodResponse.Count ("Not(Mood is NULL)");
		}


		public static DateTime GetRoundedResponseTime(DateTime timeStamp)
		{
			var tenMinth = (int)Math.Ceiling((decimal)timeStamp.Minute / 10M) * 10;
			var diff = tenMinth - timeStamp.Minute;
			var ts = timeStamp.AddMinutes(diff);
			return new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, 0, DateTimeKind.Utc);
		}

	}
}

