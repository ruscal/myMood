using System;
using System.Collections.Generic;

namespace MyMood.Services
{
	public class GlobalMoodReportInfoModel
	{
		public DateTime RequestTimeStamp {
			get;
			set;
		}

		public int DaysInReport {
			get;
			set;
		}

		public DateTime ReportStartDate {
			get;
			set;
		}

		public DateTime ReportEndDate {
			get;
			set;
		}

		public DateTime DayStartsOn {
			get;
			set;
		}
		
		public DateTime DayEndsOn {
			get;
			set;
		}

		public bool HasNewData {
			get;
			set;
		}

		public DateTime UpdatedOn {
			get;
			set;
		}
		
		public ApplicationStateModel Application {
			get;
			set;
		}

		
		public IEnumerable<ActivityModel> Prompts{
			get;
			set;
		}

		public IEnumerable<ActivityModel> Events{
			get;
			set;
		}

	}
}

