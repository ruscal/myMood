using System;

namespace MyMood.Services
{
	public class RequestPersonalMoodReportModel : RequestModelBase
	{
		public string ReportRecipient {
			get;
			set;
		}
	}
}

