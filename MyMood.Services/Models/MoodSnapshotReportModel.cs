using System;
using System.Collections.Generic;

namespace MyMood.Services
{
	public class MoodSnapshotReportModel
	{
		// timestamp
		public DateTime t {
			get;
			set;
		}

		//data
		public IEnumerable<MoodSnapshotDataModel> d {
			get;
			set;
		}

		//response count
		public int r {
			get;
			set;
		}

		//moods - not used for global map report
		public IEnumerable<MoodModel> m {
			get;
			set;
		}
	}
}

