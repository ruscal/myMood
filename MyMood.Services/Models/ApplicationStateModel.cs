using System;

namespace MyMood.Services
{
	public class ApplicationStateModel
	{
		public string EventTimeZone {
			get;
			set;
		}

		public int? EventTimeOffset {
			get;
			set;
		}

		public string WANWebServiceUri {
			get;
			set;
		}

		public string LANWebServiceUri {
			get;
			set;
		}

		public string CurrentVersion {
			get;
			set;
		}

		public string UpdateAppUri {
			get;
			set;
		}

		public int? SyncDataInterval {
			get;
			set;
		}

		public int? SyncReportInterval {
			get;
			set;
		}

		public DateTime? GoLiveDate {
			get;
			set;
		}

		public int? WarnSyncFailureAfterMins {
			get;
			set;
		}

		public bool? ForceUpdate {
			get;
			set;
		}

		public string SyncMode {
			get;
			set;
		}

		public int? ConnectionTimeout {
			get;
			set;
		}
	}
}

