using System;
using Vici.CoolStorage;
using Discover.Core;
using System.Linq;

namespace MyMood.DL
{
	[MapTo("MoodReport")]
	public class MoodReport : CSObject<MoodReport,string>
	{
		public string Id {
			get { return (string)GetField("Id"); }
			set{ SetField("Id", value); }
		}
		
		public DateTime StartsOn {
			get { return (DateTime)GetField("StartsOn"); }
			set{ SetField("StartsOn", value); }
		}

		public DateTime StartsOnLocal {
			get {
				return StartsOn.ToLocalTime(ApplicationState.Current.EventTimeOffset);
			}
		}

		public DateTime EndsOn {
			get { return (DateTime)GetField("EndsOn"); }
			set{ SetField("EndsOn", value); }
		}

		public DateTime EndsOnLocal {
			get {
				return EndsOn.ToLocalTime(ApplicationState.Current.EventTimeOffset);
			}
		}

		public DateTime RequestedOn {
			get { return (DateTime)GetField("RequestedOn"); }
			set{ SetField("RequestedOn", value); }
		}

		public int RequestCompleted {
			get { return (int)GetField("RequestCompleted"); }
			set{ SetField("RequestCompleted", value); }
		}

		public int ImagesGenerated {
			get { return (int)GetField("ImagesGenerated"); }
			set{ SetField("ImagesGenerated", value); }
		}

		public int LastRequestedZoomLevel {
			get { return (int)GetField("LastRequestedZoomLevel"); }
			set{ SetField("LastRequestedZoomLevel", value); }
		}

		public DateTime LastRequestedDay {
			get { return (DateTime)GetField("LastRequestedDay"); }
			set{ SetField("LastRequestedDay", value); }
		}

		public DateTime DayStartsOn {
			get { return (DateTime)GetField("DayStartsOn"); }
			set{ SetField("DayStartsOn", value); }
		}

		public DateTime DayEndsOn {
			get { return (DateTime)GetField("DayEndsOn"); }
			set{ SetField("DayEndsOn", value); }
		}


		[OneToMany(LocalKey="Id", ForeignKey="MoodReportId")]
		public CSList<Snapshot> Snapshots {
			get {
				return (CSList<Snapshot>)GetField ("Snapshots");
			} 
		}

		public static bool HasValidReport {
			get {
				return MoodReport.Count("RequestCompleted > 0") > 0;
				
			}
		}

		public static MoodReport CurrentReport {
			get {
				return MoodReport.OrderedList("RequestedOn-", "RequestCompleted > 0").FirstOrDefault();

			}
		}

		public static MoodReport LatestGeneratedReport {
			get {
				return MoodReport.OrderedList("RequestedOn-", "RequestCompleted > 0 and ImagesGenerated > 0").FirstOrDefault();
				
			}
		}


	}
}

