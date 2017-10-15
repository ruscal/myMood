using System;
using Vici.CoolStorage;
using System.Linq;
using System.Collections.Generic;

namespace MyMood.DL
{
	[MapTo("ApplicationState")]
	public class ApplicationState : CSObject<ApplicationState,string>
	{
		public string EventName {
			get { return (string)GetField ("EventName"); }
			set{ SetField ("EventName", value); }
		}

		public string EventTimeZone {
			get { return (string)GetField ("EventTimeZone"); }
			set{ SetField ("EventTimeZone", value); }
		}

		public int EventTimeOffset {
			get { return (int)GetField ("EventTimeOffset"); }
			set{ SetField ("EventTimeOffset", value); }
		}

		public string PassCode {
			get { return (string)GetField ("PassCode"); }
			set{ SetField ("PassCode", value); }
		}

		public string ResponderId {
			get { return (string)GetField ("ResponderId"); }
			set{ SetField ("ResponderId", value); }
		}

		public string APNSId {
			get { return (string)GetField ("APNSId"); }
			set{ SetField ("APNSId", value); }
		}

		public string ResponderRegion {
			get { return (string)GetField ("ResponderRegion"); }
			set{ SetField ("ResponderRegion", value); }
		}

		public RunningMode RunningMode {
			get { 
				var val = GetField("RunningMode");
				return (RunningMode)Enum.Parse(typeof(RunningMode), val.ToString()); }
			set{ SetField("RunningMode", value); }
		}

		public DateTime InitialisedOn {
			get { return (DateTime)GetField ("InitialisedOn"); }
			set{ SetField ("InitialisedOn", value); }
		}

		public DateTime LiveDate {
			get { return (DateTime)GetField ("LiveDate"); }
			set{ SetField ("LiveDate", value); }
		}

		public string WANWebServiceUri {
			get { return (string)GetField ("WANWebServiceUri"); }
			set{ SetField ("WANWebServiceUri", value); }
		}

		public string LANWebServiceUri {
			get { return (string)GetField ("LANWebServiceUri"); }
			set{ SetField ("LANWebServiceUri", value); }
		}
			
		public DateTime? LastSuccessfulServiceUpdate {
			get { return (DateTime?)GetField ("LastSuccessfulServiceUpdate"); }
			set{ SetField ("LastSuccessfulServiceUpdate", value); }
		}
			
		public DateTime? LastSuccessfulDataPush {
			get { return (DateTime?)GetField ("LastSuccessfulDataPush"); }
			set{ SetField ("LastSuccessfulDataPush", value); }
		}

		public DateTime? LastSuccessfulGlobalReportRequest {
			get { return (DateTime?)GetField ("LastSuccessfulGlobalReportRequest"); }
			set{ SetField ("LastSuccessfulGlobalReportRequest", value); }
		}

		public int WarnSyncFailureAfterMins {
			get { return (int)GetField ("WarnSyncFailureAfterMins"); }
			set{ SetField ("WarnSyncFailureAfterMins", value); }
		}

		public int ForceUpdate {
			get { return (int)GetField ("ForceUpdate"); }
			set{ SetField ("ForceUpdate", value); }
		}

		public string CurrentVersion {
			get { return (string)GetField ("CurrentVersion"); }
			set{ SetField ("CurrentVersion", value); }
		}

		public string UpdateAppUri {
			get { return (string)GetField ("UpdateAppUri"); }
			set{ SetField ("UpdateAppUri", value); }
		}

		public int SyncModeEnum {
			get { return (int)GetField ("SyncModeEnum"); }
			set{ SetField ("SyncModeEnum", value); }
		}
			
		public int SyncDataInterval {
			get { return (int)GetField ("SyncDataInterval"); }
			set{ SetField ("SyncDataInterval", value); }
		}

		public int SyncReportInterval {
			get { return (int)GetField ("SyncReportInterval"); }
			set{ SetField ("SyncReportInterval", value); }
		}

		public int ConnectionTimeout {
			get { return (int)GetField ("ConnectionTimeout"); }
			set{ SetField ("ConnectionTimeout", value); }
		}

		public SyncMode SyncMode {
			get {
				return (SyncMode)SyncModeEnum;
			}
			set {
				SyncModeEnum = (int)value;
			}
		}

		public static ApplicationState Current
		{
			get
			{
				return ApplicationState.List().FirstOrDefault();
			}
		}

	}

	public enum SyncMode
	{
		LANthenWAN = 1,
		WANthenLAN = 2,
		LANonly = 4,
		WANonly = 8
	}

	public enum RunningMode
	{
		TestMode = 1,
		FirstUse = 2,
		Normal = 4
	}
}

