using System;
using System.Collections.Generic;

namespace MyMood.Services
{
	public class GlobalMoodReportModel
	{
		public string ReportId {
			get;
			set;
		}

		public IEnumerable<GlobalActivityModel> Activities {
			get;
			set;
		}

		public IEnumerable<GlobalActivityModel> Prompts {
			get;
			set;
		}

		public IEnumerable<MoodSnapshotReportModel> Snapshots {
			get;
			set;
		}

		public IEnumerable<MoodModel> Moods {
			get;
			set;
		}
	}
}

