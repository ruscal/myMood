using System;

namespace MyMood.Services
{
	public class GlobalMoodReportDataRequestModel
	{
		public string ReportId {
			get;
			set;
		}

		public DateTime StartDate {
			get;
			set;
		}

		public DateTime EndDate {
			get;
			set;
		}
	}
}

