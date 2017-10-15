using System;
using Vici.CoolStorage;

namespace MyMood.DL
{
	[MapTo("MoodSnapshot")]
	public class MoodSnapshot : CSObject<MoodSnapshot,string>
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


		public int ResponseCount {
			get { return (int)GetField("ResponseCount"); }
			set{ SetField("ResponseCount", value); }
		}

		public decimal ResponsePercentage {
			get { return (decimal)GetField("ResponsePercentage"); }
			set{ SetField("ResponsePercentage", value); }
		}

		[ManyToOne(LocalKey="SnapshotId", ForeignKey="Id")]
		public Snapshot Snapshot {
			get{ return (Snapshot) GetField("Snapshot");} set{ SetField("Snapshot", value);}
		}
	}
}

