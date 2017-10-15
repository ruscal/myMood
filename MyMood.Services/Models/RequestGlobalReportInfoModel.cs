using System;

namespace MyMood.Services
{
	public class RequestGlobalReportInfoModel
	{
		public DateTime? LastReportRequested {
			get;
			set;
		}

		public DateTime? LastUpdate
		{
			get;
			set;
		}
	}
}

