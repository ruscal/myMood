using System;
using System.Net;
using MyMood.DL;
using System.Text;
using System.IO;
using Discover;
using Discover.Net;
using Discover.Logging;
using Vici.CoolStorage;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Discover.Core;

namespace MyMood.Services
{
	public class MyMoodService : WebServiceRequestHandler
	{
		int _minSyncInterval = 10;
		int _maxSyncInterval = 3600;
		static readonly object _lock = new object ();

		public int SyncDataInterval {
			get {
				var app = ApplicationState.Current;
				return app.SyncDataInterval < _minSyncInterval ? _minSyncInterval : app.SyncDataInterval > _maxSyncInterval ? _maxSyncInterval : app.SyncDataInterval;
			}
		}

		public MyMoodService (ILogger logger)
			:base(logger)
		{


		}

		public ReportRequestStatus RequestGlobalMoodReport ()
		{
			var app = ApplicationState.Current;
			ReportRequestStatus rtnStatus = new ReportRequestStatus ();

			lock (_lock) {
				this.Log ("Service request for global report", "", 3);

				WebServiceJsonRequestStatus status = ServerJsonRequest ("RequestGlobalReportInfo",
				                                                        new RequestGlobalReportInfoModel (){
					LastReportRequested = app.LastSuccessfulGlobalReportRequest
				});
				if (status.Success) {
					GlobalMoodReportInfoModel reportInfo = status.Response.ToObject<GlobalMoodReportInfoModel> ();
					app.LastSuccessfulGlobalReportRequest = reportInfo.RequestTimeStamp;
					app.Save ();


					if (reportInfo.HasNewData) {
						UpdateReportInfo (reportInfo);
						rtnStatus.NewReportAvailable = true;
					}

					rtnStatus.Success = true;
				}
			}
			
			return rtnStatus;
		}

		public ReportRequestStatus RequestGlobalMoodReportData (MoodReport report)
		{
			ReportRequestStatus rtnStatus = new ReportRequestStatus ();
			try {
				var app = ApplicationState.Current;

			
				var startDate = report.StartsOn;
				List<GlobalMoodReportModel> days = new List<GlobalMoodReportModel> ();
				var fail = 3;
				while (fail > 0 && startDate < report.EndsOn) {
					var endDate = startDate.Date.AddDays (1).AddMinutes (-10);
					NSString key = new NSString ("Status");
					NSDictionary userInfo = NSDictionary.FromObjectAndKey(NSObject.FromObject(string.Format("Requesting day data for {0:ddd dd MMM}", startDate)), key);
					NSNotificationCenter.DefaultCenter.PostNotificationName("RequestingReportStatus",null, userInfo);

					var dataReqStatus = RequestGlobalMoodReportData (report.Id, startDate, endDate);
					if (dataReqStatus.Success) {
						//report = MoodReport.List ().FirstOrDefault (r => r.Id == reportId);
						//report.LastRequestedDay = startDate;
						//report.Save ();
						days.Add (dataReqStatus.ReportData);
						startDate = startDate.Date.AddDays (1);
						fail = 3;
					} else {
						fail --;
					}
				}
				if (fail > 0) {
							
					UpdateReportDays (report, days);

					rtnStatus.Success = true;
					rtnStatus.NewReportAvailable = true;
				} else {
					MyMoodLogger.Current.Log ("Report download failed", "", 1);
					rtnStatus.Success = false;		
				}	

			} catch (Exception ex) {
				MyMoodLogger.Current.Error("Error downloading day data", ex, 1);
			}
			return rtnStatus;
		}

		protected void UpdateReportInfo (GlobalMoodReportInfoModel reportInfo)
		{
			Console.WriteLine("Update report info");
			if (reportInfo.Application != null) {
				UpdateApplication (reportInfo.Application, true);
			}
			
			if (reportInfo.Prompts != null) {
				Console.WriteLine("Update prompts");
				Activity.List ("ActivityType=@ActivityType", new{ ActivityType = ActivityType.MoodPrompt}).DeleteAll ();
				foreach (var p in reportInfo.Prompts) {
					var prompt = Activity.New ();
					prompt.Id = p.Id;
					prompt.ActivityType = ActivityType.MoodPrompt;
					prompt.Title = p.Title;
					prompt.TimeStamp = p.TimeStamp;
					prompt.Save ();
					
				}
			}
			
			if (reportInfo.Events != null) {
				Console.WriteLine("Update events");
				Activity.List ("ActivityType=@ActivityType", new{ ActivityType = ActivityType.Event}).DeleteAll ();
				foreach (var e in reportInfo.Events) {
					var evnt = Activity.New ();
					evnt.Id = e.Id;
					evnt.ActivityType = ActivityType.Event;
					evnt.Title = e.Title;
					evnt.TimeStamp = e.TimeStamp;
					evnt.Save ();
					
				}
			}

			if (reportInfo.HasNewData) {
				MoodReport report = MoodReport.New ();
				var reportId = System.Guid.NewGuid ().ToString ();
				report.Id = reportId;
				report.RequestedOn = reportInfo.RequestTimeStamp;
				report.StartsOn = reportInfo.ReportStartDate;
				report.EndsOn = reportInfo.ReportEndDate;
				report.RequestCompleted = 0;
				report.DayStartsOn = reportInfo.DayStartsOn;
				report.DayEndsOn = reportInfo.DayEndsOn;
				report.RequestCompleted = 1;
				report.Save ();
			}
		}

		protected void UpdateReportDays (MoodReport report, IEnumerable<GlobalMoodReportModel> days)
		{
			Console.WriteLine("Update report days");
			var allMoods = Mood.All ().ToList ();

			NSDictionary userInfo = NSDictionary.FromObjectAndKey(NSObject.FromObject("Saving data ..."), new NSString ("Status"));
			NSNotificationCenter.DefaultCenter.PostNotificationName("RequestingReportStatus",null, userInfo);
			int count = 1;
			var insertSnapshotSql = "insert into Snapshot (Id, MoodReportId, TimeOfSnapshot, TimeOfSnapshotLocal, TotalResponses, CreatedOn) " +
				" values (@Id, @MoodReportId, @TimeOfSnapshot, @TimeOfSnapshotLocal, @TotalResponses,  @CreatedOn)";

			var insertMoodSnapshotSql = "insert into MoodSnapshot (Id, SnapshotId, MoodId, ResponseCount, ResponsePercentage) " +
				" values (@Id, @SnapshotId, @MoodId, @ResponseCount, @ResponsePercentage)";

			foreach (var day in days) {
				foreach (var s in day.Snapshots) {

					userInfo = NSDictionary.FromObjectAndKey(NSObject.FromObject(string.Format("Saving snapshot {0}", count)), new NSString ("Status"));
					NSNotificationCenter.DefaultCenter.PostNotificationName("RequestingReportStatus",null, userInfo);
					count++;

					var snapshotId = System.Guid.NewGuid ().ToString ();
					CSDatabase.ExecuteNonQuery(insertSnapshotSql, new{ Id = snapshotId,
						MoodReportId = report.Id,
						TimeOfSnapshot = s.t,
						TimeOfSnapshotLocal = s.t.ToLocalTime (ApplicationState.Current.EventTimeOffset),
						TotalResponses = s.r,
						CreatedOn = DateTime.UtcNow});


					

//					Snapshot snap = Snapshot.New ();
//					snap.Id = System.Guid.NewGuid ().ToString ();
//					snap.MoodReport = report;
//					snap.TimeOfSnapshot = s.t;
//					snap.TimeOfSnapshotLocal = s.t.ToLocalTime (ApplicationState.Current.EventTimeOffset);
//					snap.TotalResponses = s.r;
//					snap.IsFirstGlance = false;
//					snap.CreatedOn = DateTime.UtcNow;
//					snap.Save ();
					//make sure a snapshot is recorde for all moods even if no data provided
					var snapMoods = from m in allMoods
												join d in s.d
													on m.DisplayIndex equals d.i into gm
													from subm in gm.DefaultIfEmpty ()
											select new {
												Mood = m,
												
												ResponseCount = subm == null ? 0 : subm.c,
												ResponsePercentage = subm == null ? 0 : subm.p,
												MoodType = m.MoodType
											};
				
				
					foreach (var d in snapMoods) {

						CSDatabase.ExecuteNonQuery(insertMoodSnapshotSql, new {
							Id = System.Guid.NewGuid ().ToString (),
							SnapshotId = snapshotId,
							MoodId = d.Mood.Id,
							ResponseCount = d.ResponseCount,
							ResponsePercentage = d.ResponsePercentage
						});


//						MoodSnapshot ms = MoodSnapshot.New ();
//						ms.Id = System.Guid.NewGuid ().ToString ();
//						ms.Snapshot = snap;
//						ms.Mood = d.Mood;
//						//ms.Name = d.Mood.Name;
//						//ms.DisplayIndex = d.Mood.DisplayIndex;
//						ms.ResponseCount = d.ResponseCount;
//						ms.ResponsePercentage = d.ResponsePercentage;
//						ms.Save ();
					}
				}
			}

		}

		protected ReportDataRequestStatus RequestGlobalMoodReportData (string reportId, DateTime startDate, DateTime endDate)
		{

			ReportDataRequestStatus rtnStatus = new ReportDataRequestStatus ();
			lock (_lock) {
				this.Log ("Service request for global report data", reportId, 3);
				
				WebServiceJsonRequestStatus status = ServerJsonRequest ("RequestGlobalReportData",
				                                                        new GlobalMoodReportDataRequestModel (){
					StartDate = startDate,
					EndDate = endDate,
					ReportId = reportId
				});
				if (status.Success) {
					GlobalMoodReportModel reportModel = status.Response.ToObject<GlobalMoodReportModel> ();
					//var report = MoodReport.List ().FirstOrDefault (r => r.Id == reportId);
					rtnStatus.ReportData = reportModel;
				}
				rtnStatus.Success = status.Success;
				
			}
			
			return rtnStatus;
		}


		public ServiceRequestStatus RequestPersonalMoodReport (string recipientEmailAddress)
		{
			var app = ApplicationState.Current;
			ServiceRequestStatus rtnStatus = new ServiceRequestStatus ();
			lock (_lock) {
				this.Log ("Service request for personal report", recipientEmailAddress, 3);
				
				WebServiceJsonRequestStatus status = ServerJsonRequest ("RequestPersonalMoodReport", 
				                                                        new RequestPersonalMoodReportModel (){
					rid = app.ResponderId,
					reg = app.ResponderRegion,
					apn = app.APNSId,
					ReportRecipient = recipientEmailAddress
				});
				if (status.Success) {
					ResponseModel response = status.Response.ToObject<ResponseModel> ();
					rtnStatus.Success = response.Success;
				} else {
					rtnStatus.Success = status.Success;
				}
				
			}
			
			return rtnStatus;
		}

		public ServiceRequestStatus RegisterInterestInApp (string recipientEmailAddress)
		{
			var app = ApplicationState.Current;
			ServiceRequestStatus rtnStatus = new ServiceRequestStatus ();
			lock (_lock) {
				this.Log ("Service request for registering interest", recipientEmailAddress, 3);
				
				WebServiceJsonRequestStatus status = ServerJsonRequest ("RegisterInterestInApp", 
				                                                        new RegisterInterestInAppModel (){
					rid = app.ResponderId,
					reg = app.ResponderRegion,
					apn = app.APNSId,
					InterestedParty = recipientEmailAddress
				});

				if (status.Success) {
					ResponseModel response = status.Response.ToObject<ResponseModel> ();
					rtnStatus.Success = response.Success;
				} else {
					rtnStatus.Success = status.Success;
				}
				
			}
			
			return rtnStatus;
		}

		public ServiceRequestStatus RegisterForAPNS (string apnsId)
		{
			var app = ApplicationState.Current;
			ServiceRequestStatus rtnStatus = new ServiceRequestStatus ();
			lock (_lock) {
				this.Log ("Service request for APNS registration", apnsId, 2);
				
				WebServiceJsonRequestStatus status = ServerJsonRequest ("RegisterForAPNS", 
				                                                        new RequestModelBase (){
					rid = app.ResponderId,
					reg = app.ResponderRegion,
					apn = apnsId
				});
				rtnStatus.Success = status.Success;
				
			}
			
			return rtnStatus;
		}

		public ServiceSyncStatus SyncDataWithServer ()
		{

			ServiceSyncStatus rtnStatus = new ServiceSyncStatus ();
			lock (_lock) {
				this.Log ("Service request to Sync with Server", "", 3);
				var app = ApplicationState.Current;
				IEnumerable<MoodResponseUpdateModel> responsesToUpdate = GetResponsesToSaveToServer (app.LastSuccessfulDataPush);
				rtnStatus.HasOutstandingResponsesToSync = responsesToUpdate.Count () > 0;

				WebServiceJsonRequestStatus status = SendSyncRequest (app, responsesToUpdate);

				app = ApplicationState.Current;
				if (status.Success) {
					UpdateAppFromServiceModel updates = status.Response.ToObject<UpdateAppFromServiceModel> ();

					if (status.Success) {
						ApplyUpdates (updates);
						rtnStatus.HasPromptUpdates = updates.HasPromptUpdates;
						rtnStatus.HasOutstandingResponsesToSync = false;
					}

					//server has different total of responses so resubmit all
					if (updates.ResError) {
						this.Log ("Server res error - resyncing", "", 2);
						status = SendSyncRequest (app, GetResponsesToSaveToServer (null));
						if (status.Success) {
							ApplyUpdates (updates);
							rtnStatus.HasPromptUpdates = updates.HasPromptUpdates;
							rtnStatus.HasOutstandingResponsesToSync = false;
						}
					}
				}
				rtnStatus.Success = status.Success;
				rtnStatus.LastSuccessfulDataPush = app.LastSuccessfulDataPush;
				rtnStatus.SuccessLevel = GetSyncSuccessLevel (status.Success, responsesToUpdate.Count ());
			}

			return rtnStatus;
		}


		// quick submit  -dont care if works or not as sync should capture any fails
		public void SubmitMoodResponse (MoodResponse response)
		{
			if (response != null) {
				var app = ApplicationState.Current;

				lock (_lock) {
					this.Log ("Submitting response - " + response.Mood.Name, "", 2);
					WebServiceJsonRequestStatus status = ServerJsonRequest ("SubmitMoodResponse", 
				                                                        new SubmitResponseModel (){
						rid = app.ResponderId,
						reg = app.ResponderRegion,
						apn = app.APNSId,
						r =  new MoodResponseUpdateModel(){
							i = response.Id,
							m = response.Mood.Name,
							t = response.TimeStamp,
							p = response.Prompt != null ? response.Prompt.Id : null
						}
					});
				}
			}
		}
		
		public void CheckForServiceUpdates ()
		{
			var app = ApplicationState.Current;
			lock (_lock) {
				this.Log ("Service request to Check for Service Updates", "", 3);
				WebServiceJsonRequestStatus status = ServerJsonRequest ("GetUpdates", new GetServiceUpdatesModel (){
					rid = app.ResponderId,
					reg = app.ResponderRegion,
					apn = app.APNSId,
					LastUpdate = app.LastSuccessfulServiceUpdate
			});
				app = ApplicationState.Current;
				if (status.Success) {
					UpdateAppFromServiceModel updates = status.Response.ToObject<UpdateAppFromServiceModel> ();
					if (updates.HasPromptUpdates || updates.Application != null) {
						ApplyUpdates (updates);
					}
					app = ApplicationState.Current;
					app.LastSuccessfulServiceUpdate = DateTime.UtcNow;
					app.Save ();
				}
			}
		}

		protected WebServiceJsonRequestStatus ServerJsonRequest (string action, object postData)
		{
			var app = ApplicationState.Current;
			WebServiceJsonRequestStatus status;
			string wanUri = app.WANWebServiceUri;
			string lanUri = app.LANWebServiceUri;

			if (!wanUri.EndsWith ("/"))
				wanUri = wanUri + "/";
			if (!lanUri.EndsWith ("/"))
				lanUri = lanUri + "/";

			this.RequestTimeout = app.ConnectionTimeout;

			var syncMode = app.SyncMode;
#if DEBUG
			//syncMode = SyncMode.LANonly;
#endif
			switch (syncMode) {
			
			case SyncMode.WANthenLAN:
				status = JsonRequest (UrlHelper.ToUrl (wanUri, app.EventName, action, app.PassCode), postData);
				if (!status.Success)
					status = JsonRequest (UrlHelper.ToUrl (lanUri, app.EventName, action, app.PassCode), postData);
				return status;
			case SyncMode.LANonly:
				return JsonRequest (UrlHelper.ToUrl (lanUri, app.EventName, action, app.PassCode), postData);
			case SyncMode.WANonly:
				return JsonRequest (UrlHelper.ToUrl (wanUri, app.EventName, action, app.PassCode), postData);
			default:
			case SyncMode.LANthenWAN:
				status = JsonRequest (UrlHelper.ToUrl (lanUri, app.EventName, action, app.PassCode), postData);
				if (!status.Success)
					status = JsonRequest (UrlHelper.ToUrl (wanUri, app.EventName, action, app.PassCode), postData);
				return status;
			}
		}

		protected WebServiceJsonRequestStatus SendSyncRequest (ApplicationState app, IEnumerable<MoodResponseUpdateModel> responsesToUpdate)
		{
			return ServerJsonRequest ("SyncData", 
			                          new UpdateServiceFromAppModel (){
				rid = app.ResponderId,
				reg = app.ResponderRegion,
				apn = app.APNSId,
				Responses = responsesToUpdate,
				ResTotal = MoodResponse.GetConfirmedResponseCount(),
				LastUpdate = app.LastSuccessfulServiceUpdate
			});
		}

		protected void ApplyUpdates (UpdateAppFromServiceModel updates)
		{
			var app = ApplicationState.Current;
			if (updates.HasPromptUpdates || updates.Application != null) {
				if (updates.Prompts != null && updates.Prompts.Count () > 0) {
					SyncPrompts (updates.Prompts);
				}
				if (updates.Categories != null && updates.Categories.Count () > 0) {
					foreach (var c in updates.Categories) {
						SyncCategory (c);
					}
				}
				if (updates.Application != null)
					UpdateApplication (updates.Application, false);
				app.LastSuccessfulServiceUpdate = updates.SyncTimestamp;
				app.Save ();
			}
			if (updates.SyncSuccess) {
				app.LastSuccessfulDataPush = updates.SyncTimestamp;
				app.Save ();
			}
		}

		protected void UpdateApplication (ApplicationStateModel appState, bool ignoreSyncModeUpdate)
		{
			Console.WriteLine("Update application");
			if (appState != null) {

				var app = ApplicationState.Current;

				//TODO: verify update originates from server
				if (appState.EventTimeZone != null)
					app.EventTimeZone = appState.EventTimeZone;
				if (appState.EventTimeOffset != null)
					app.EventTimeOffset = appState.EventTimeOffset.Value;
				if (appState.WANWebServiceUri != null)
					app.WANWebServiceUri = appState.WANWebServiceUri;
				if (appState.LANWebServiceUri != null)
					app.LANWebServiceUri = appState.LANWebServiceUri;
				if (appState.CurrentVersion != null)
					app.CurrentVersion = appState.CurrentVersion;
				if (appState.UpdateAppUri != null)
					app.UpdateAppUri = appState.UpdateAppUri;
				if (appState.SyncDataInterval != null && appState.SyncDataInterval >= _minSyncInterval && appState.SyncDataInterval <= _maxSyncInterval)
					app.SyncDataInterval = (int)appState.SyncDataInterval;
				if (appState.SyncReportInterval != null && appState.SyncReportInterval >= _minSyncInterval && appState.SyncReportInterval <= _maxSyncInterval)
					app.SyncReportInterval = (int)appState.SyncReportInterval;
				if (appState.GoLiveDate != null)
					app.LiveDate = appState.GoLiveDate.Value;
				if (appState.WarnSyncFailureAfterMins != null)
					app.WarnSyncFailureAfterMins = appState.WarnSyncFailureAfterMins.Value;
				if (appState.ForceUpdate != null)
					app.ForceUpdate = appState.ForceUpdate.Value ? 1 : 0;
				if (appState.SyncMode != null && !ignoreSyncModeUpdate)
					app.SyncMode = (SyncMode)Enum.Parse (typeof(SyncMode), appState.SyncMode);
				if (appState.ConnectionTimeout != null)
					app.ConnectionTimeout = appState.ConnectionTimeout.Value;


				app.LastSuccessfulServiceUpdate = DateTime.UtcNow;
				app.Save ();
				//TODO: fire request to update codebase if needed
			}
		}

		protected void SyncCategory (MoodCategoryModel category)
		{
			if (category != null) {
				//TODO: update moods in category.
			}
		}

		protected void SyncPrompts (IEnumerable<MoodPromptModel> prompts)
		{
			if (prompts != null) {
				var delete = MoodPrompt.List ().Where (mp => !prompts.Any (p => p.Id == mp.Id)).ToList ();
				foreach (var p in delete) {
					var r = p.Response;
					if (p.Response.Mood == null) {
						r.Delete ();
					} else {
						r.Prompt = null;
						r.Save ();
					}
					p.Delete ();

				}
				foreach (var p in prompts) {
					MoodPrompt prompt = MoodPrompt.ReadSafe (p.Id);
					if (prompt == null) {

						prompt = MoodPrompt.New ();
						prompt.Id = p.Id.ToString ();
						prompt.Save ();

						MoodResponse response = MoodResponse.New ();
						response.Id = System.Guid.NewGuid ().ToString ();
						response.Prompt = prompt;
						response.TimeStamp = p.Activity.TimeStamp;
						response.Save ();
					}
					prompt.Name = p.Name;
					prompt.NotificationText = p.NotificationText;
					prompt.Title = p.Activity.Title;
					prompt.TimeStamp = p.Activity.TimeStamp;
					prompt.ActiveFrom = p.ActiveFrom;
					prompt.ActiveTil = p.ActiveTil;
					prompt.Response.TimeStamp = prompt.TimeStamp;
					prompt.Response.Save ();
					prompt.Save ();


				}
			}
		}

		protected IEnumerable<MoodResponseUpdateModel> GetResponsesToSaveToServer (DateTime? lastSyncDate)
		{
			return MoodResponse.GetConfirmedResponses ().Where (r => lastSyncDate == null || r.CreatedOn > lastSyncDate.Value)
				.Select (r => new MoodResponseUpdateModel (){
					i = r.Id,
					t = r.TimeStamp,
					m =r.Mood.Name,
					p = r.Prompt == null ? null : r.Prompt.Id
				}).ToList ();
		}

		private SyncSuccessLevel GetSyncSuccessLevel (bool success, int outstandingResponseCount)
		{
			if (success) 
				return SyncSuccessLevel.Success;

			var lastSuccessfulPush = ApplicationState.Current.LastSuccessfulDataPush;

			if (outstandingResponseCount > 0 && (lastSuccessfulPush == null || DateTime.UtcNow.Subtract (lastSuccessfulPush.Value).Minutes >= ApplicationState.Current.WarnSyncFailureAfterMins)) {
				return SyncSuccessLevel.FailedSevere;
			} else {
				return SyncSuccessLevel.FailedWarning;
			}

		}

		static MyMoodService current;

		public static MyMoodService Current {
			get {
				if (current == null)
					current = new MyMoodService (MyMoodLogger.Current);
				return current;
			}
		}

	}

	public class ReportRequestStatus
		:ServiceRequestStatus
	{
		public bool NewReportAvailable {
			get;
			set;
		}
	}

	public class ReportDataRequestStatus
		:ServiceRequestStatus
	{
		public GlobalMoodReportModel ReportData {
			get;
			set;
		}
	}

	public class ServiceRequestStatus
	{
		public bool Success {
			get;
			set;
		}

	}

	public class ServiceSyncStatus
	{
		public bool Success {
			get;
			set;
		}

		public bool HasPromptUpdates {
			get;
			set;
		}

		public DateTime? LastSuccessfulDataPush {
			get;
			set;
		}

		public bool HasOutstandingResponsesToSync {
			get;
			set;
		}

		public SyncSuccessLevel SuccessLevel {
			get;
			set;
		}
	}



}

