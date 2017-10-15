using System;
using Vici.CoolStorage;
using Vici.Core;
using Discover.Core;

namespace MyMood.DL
{
	[MapTo("Snapshot")]
	public class Snapshot : CSObject<Snapshot,string>
	{
		public string Id {
			get { return (string)GetField("Id"); }
			set{ SetField("Id", value); }
		}

		public DateTime TimeOfSnapshot {
			get { return (DateTime)GetField("TimeOfSnapshot"); }
			set{ SetField("TimeOfSnapshot", value); }
		}

		public DateTime TimeOfSnapshotLocal {
			get { return (DateTime)GetField("TimeOfSnapshotLocal"); }
			set{ SetField("TimeOfSnapshotLocal", value); }
		}


		public int TotalResponses {
			get { return (int)GetField("TotalResponses"); }
			set{ SetField("TotalResponses", value); }
		}

//		public bool IsFirstGlance {
//			get { return (int)GetField("IsFirstGlance") > 0 ? true : false; }
//			set{ SetField("IsFirstGlance", value ? 1 : 0); }
//		}

		public DateTime CreatedOn {
			get { return (DateTime)GetField("CreatedOn"); }
			set{ SetField("CreatedOn", value); }
		}

		[ManyToOne(LocalKey="MoodReportId", ForeignKey="Id")]
		public MoodReport MoodReport {
			get{ return (MoodReport) GetField("MoodReport");} set{ SetField("MoodReport", value);}
		}

		[Prefetch]
		[OneToMany(LocalKey="Id", ForeignKey="SnapshotId")]
		public CSList<MoodSnapshot> Moods {
			get {
				return (CSList<MoodSnapshot>)GetField ("Moods");
			} 
		}


	}
}

