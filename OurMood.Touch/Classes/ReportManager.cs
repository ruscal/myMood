using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using MyMood.DL;
using MyMood.Services;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using Discover.Drawing;
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreAnimation;
using Discover.Core;
using System.IO;
using Vici.CoolStorage;
using System.Threading;

namespace OurMood.Touch
{
	public class ReportManager : NSObject
	{
		public ReportManager() : base()
		{

		}

		//sync data
		private Thread syncThread;
		public void StartSyncThread ()
		{
			Console.WriteLine("Start sync thread");
			
			if (syncThread == null || !syncThread.IsAlive) {
				syncThread = new Thread (new ThreadStart (() => {
					using (var pool = new NSAutoreleasePool()) {
						Console.WriteLine("Sync thread started");
						NSTimer.CreateRepeatingScheduledTimer (60, delegate { 
							//MyMoodService.Current.CheckForServiceUpdates ();
							SyncData ();
						});
						
						
						SyncData ();
						NSRunLoop.Current.Run ();
					}
				}));           
				
				syncThread.IsBackground = true;
				Console.WriteLine("Starting sync thread");
				syncThread.Start ();
			}
		}
		
		public void SyncData ()
		{
			Console.WriteLine ("SyncData");
			NotifySyncing();
			ReportRequestStatus status = ReportManager.FetchNewReport (); 
			NotifySyncComplete(status.Success ? SyncSuccessLevel.Success : SyncSuccessLevel.FailedSevere);
			Console.WriteLine ("SyncData done.");
		}
		
		public void NotifySyncing(){
			InvokeOnMainThread (delegate {
				NSNotificationCenter.DefaultCenter.PostNotificationName("SyncingWithServer",null);
			});
		}
		
		public void NotifySyncComplete(SyncSuccessLevel status){
			InvokeOnMainThread (delegate {
				NSString key = new NSString ("SyncStatus");
				NSDictionary userInfo = NSDictionary.FromObjectAndKey(NSObject.FromObject(status), key);
				NSNotificationCenter.DefaultCenter.PostNotificationName("SyncWithServerComplete",null, userInfo);
			});
		}

		public static TimeSpan DayStartTime = new TimeSpan (7, 0, 0);
		public static TimeSpan DayEndTime = new TimeSpan (22, 0, 0);
		private static bool fetchingData = false;
		static readonly object _lock = new object ();

		public static ReportRequestStatus FetchNewReport ()
		{
			Console.WriteLine ("Fetch new report");
			var status = new ReportRequestStatus();
			lock (_lock) {
				fetchingData = true;
				try{
					var hasValidReport = MoodReport.HasValidReport;
					status = MyMoodService.Current.RequestGlobalMoodReport ();

					if (status.Success && status.NewReportAvailable) {

						NSNotificationCenter.DefaultCenter.PostNotificationName ("NewReportReceived", null);
					} else {
						if (!hasValidReport) {
							//first report so need to let loading screen know we are loaded
							NSNotificationCenter.DefaultCenter.PostNotificationName ("NoReportData", null);
						}
					}
				}catch(Exception ex){
					MyMoodLogger.Current.Error("Error fetching new report", ex, 1);
				} finally{
					fetchingData = false;
				}
			}
			Console.WriteLine ("Fetched new report");
			return status;
		}


		public static ReportRequestStatus FetchReportDays (MoodReport report)
		{
			Console.WriteLine ("Fetch report days");
			var status = new ReportRequestStatus();
			lock (_lock) {
				fetchingData = true;
				try{
					//get rid of any pre fetched snapshots
					DeleteReportSnapshots (report);
					status = MyMoodService.Current.RequestGlobalMoodReportData (report);
				}catch(Exception ex){
					MyMoodLogger.Current.Error("Error fetching new report", ex, 1);
				}finally{
					fetchingData = false;
				}

			}
			Console.WriteLine ("Fetched report days");
			return status;

		}

		public static double GetReportHours (MoodReport report)
		{
			var reportStartLocal = GetReportStartLocal(report);
			var reportEndLocal = GetReportEndLocal(report);
			return (reportEndLocal.Subtract(reportStartLocal).Days+1) * (DayEndTime.Subtract(DayStartTime).TotalHours);
		}

		public static DateTime GetReportStartLocal (MoodReport report)
		{
			return report.StartsOnLocal.Date.Add (DayStartTime);
		}

		public static DateTime GetReportEndLocal (MoodReport report)
		{
			return report.EndsOnLocal.Date.Add (DayEndTime);
		}

		public static IEnumerable<UIImage> GetReportImages (MoodReport report, RenderLevel renderLevel)
		{
			List<UIImage> images = new List<UIImage>();
			string imagesFolder = Path.Combine(Environment.GetFolderPath( Environment.SpecialFolder.Personal),"Reports", report.Id.ToString());
			var reportStartLocal = GetReportStartLocal (report);
			var reportEndLocal = GetReportEndLocal (report);
			var days = (float)reportEndLocal.Date.Subtract (reportStartLocal.Date).Days + 1;
			for(var i =0;i<days;i++){
				var imagePath = Path.Combine(imagesFolder, GetDayImageFileName(renderLevel, i));
				images.Add(UIImage.FromFile (imagePath));
			}
			return images;
		}

		public static void GenerateReportImagesForAllLevels (MoodReport report)
		{

			foreach (var rl in RenderLevel.RenderLevels) {
				GenerateReportImages(report, rl);
			}
			report.ImagesGenerated = 1;
			report.Save();

			//NSNotificationCenter.DefaultCenter.PostNotificationName ("ReportImagesGenerated", null);

			//DeleteAllOldReports();
		}

		public static void DeleteAllOldReports (MoodReport currentReport)
		{
			try {
				if(currentReport != null){
					Console.WriteLine ("Delete old reports");
					//var currentReport = MoodReport.CurrentReport;
					var lastReport = MoodReport.ReadFirst ("RequestedOn < @RequestedOn and RequestCompleted > 0", new { RequestedOn = currentReport.RequestedOn });
					var oldReports = MoodReport.List ("RequestedOn < @RequestedOn", new { RequestedOn = currentReport.RequestedOn }).ToList ();
					foreach (var report in oldReports) {
						if (lastReport == null || report.Id != lastReport.Id)
							DeleteReport (report);
					}
				}
			} catch (Exception ex) {

				MyMoodLogger.Current.Error("Error deleting old reports", ex, 1);
			}
		}

		public static void DeleteReport (MoodReport report)
		{
			try {
				Console.WriteLine ("Delete report {0}", report.Id);
				string imagesFolder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Reports", report.Id.ToString ());
				DirectoryInfo dir = new DirectoryInfo (imagesFolder);
				Console.WriteLine ("Lets delete image files");
				if(dir.Exists) dir.Delete (true);
				Console.WriteLine ("Deleted image files");
//
//				foreach (var snapshot in report.Snapshots) {
//					snapshot.Moods.DeleteAll ();
//				}
//
//				report.Snapshots.DeleteAll ();

				DeleteReportSnapshots(report);
				Console.WriteLine ("Delete report");
				var deleteReport = "delete from MoodReport where Id = @ReportId";
				CSDatabase.ExecuteNonQuery (deleteReport, new { ReportId = report.Id});

				//report.Delete ();

				Console.WriteLine("Report deleted");
			} catch (Exception ex) {
				MyMoodLogger.Current.Error("Failed deleting report", ex, 1);
			}

		}

		private static void DeleteReportSnapshots (MoodReport report)
		{
			var deleteMoodSnapshotsSql = "delete from MoodSnapshot where Id in" +
				"(select ms.Id from MoodSnapshot ms inner join Snapshot s on ms.SnapshotId = s.Id where s.MoodReportId = @ReportId)";
			
			var deleteSnapshotSql = "delete from Snapshot where MoodReportId = @ReportId";

			Console.WriteLine ("Delete mood snapshots");
			CSDatabase.ExecuteNonQuery (deleteMoodSnapshotsSql, new { ReportId = report.Id});
			Console.WriteLine ("Delete snapshots");
			CSDatabase.ExecuteNonQuery (deleteSnapshotSql, new { ReportId = report.Id});
		}

		public static Snapshot GetClosestSnapshot (MoodReport report, DateTime snapshotLocalTime)
		{
			//return report.Snapshots.ToList().Where(s => s.TimeOfSnapshotLocal <= snapshotLocalTime).OrderByDescending(s => s.TimeOfSnapshotLocal).FirstOrDefault();
			Console.WriteLine("Fetching closest snapshot");
			//Id TEXT PRIMARY KEY, MoodReportId TEXT, TimeOfSnapshot DATETIME, TimeOfSnapshotLocal DATETIME, TotalResponses INTEGER, IsFirstGlance INTEGER, CreatedOn datetime
			var sql = "select Id from Snapshot where MoodReportId = @ReportId and TimeOfSnapshotLocal <= @SnapTime order by TimeOfSnapshotLocal desc limit 1";
			var record = CSDatabase.RunQuery(sql, new { ReportId = report.Id, SnapTime = snapshotLocalTime}).FirstOrDefault();
			object snapshotId = null;
			record.TryGetValue("Id", out snapshotId);
			//string snapshotId = CSDatabase.RunQuery(sql, new { ReportId = report.Id, SnapTime = snapshotLocalTime}).FirstOrDefault();
			Snapshot snapshot = null;
			if(snapshotId != null) snapshot = Snapshot.ReadSafe(snapshotId.ToString());

			//var utcTime = snapshotLocalTime.ToLocalTime(-(ApplicationState.Current.EventTimeOffset));
			//var snapshot = Snapshot.OrderedList("TimeOfSnapshotLocal-", "MoodReport.Id = @ReportId and TimeOfSnapshotLocal <= @SnapTime", new { ReportId = report.Id, SnapTime = snapshotLocalTime}).FirstOrDefault();
			Console.WriteLine("Got closest snapshot");
			return snapshot;
		}

		public static void GenerateReportImages (MoodReport report, RenderLevel renderLevel)
		{
			Console.WriteLine("Generate report images - {0} / {1}", report.Id, renderLevel.Index);
			if (report != null) {

				string imagesFolder = Path.Combine(Environment.GetFolderPath( Environment.SpecialFolder.Personal),"Reports", report.Id.ToString());
				DirectoryInfo imagesDir = new DirectoryInfo(imagesFolder);
				if(!imagesDir.Exists) imagesDir.Create();


				var nowLocal = DateTime.UtcNow.ToLocalTime (ApplicationState.Current.EventTimeOffset);
				var reportStartLocal = GetReportStartLocal (report);
				var reportEndLocal = GetReportEndLocal (report);
				//var finalSnapshotTime = nowLocal > reportEndLocal ? reportEndLocal : nowLocal;
				var finalSnapshotTime = reportEndLocal;
			
				var dayStart = reportStartLocal;
				var dayEnd = reportStartLocal.Date.Add (DayEndTime);
				//var dayHours = dayEnd.Subtract (dayStart).TotalHours;
				if (dayEnd > reportEndLocal)
					dayEnd = reportEndLocal;

				var allMoods = Mood.All ().ToList ();
				var lastSnapshot = GenerateEmptyMoodList (allMoods);
				var lastSnapshotAdded = dayStart;
			
				//var days = (float)reportEndLocal.Date.Subtract (reportStartLocal.Date).Days + 1;
				var dayIndex = 0;
						
				while (dayStart < reportEndLocal) {

					var imageFrame = new RectangleF (0f, 0f, renderLevel.DayImageWidth, renderLevel.DayImageHeight);
					var imager = new GlobalMoodMapImager (dayStart, dayEnd);
					bool lastAdded = false;
					var snapshots = Snapshot.OrderedList ("TimeOfSnapshotLocal", 
					                                      "MoodReport.Id = @ReportId and TimeOfSnapshotLocal >= @Start and TimeOfSnapshotLocal <= @End",
					                                      new { ReportId = report.Id, Start = lastSnapshotAdded, End = dayEnd }).ToList ();
					
					var addedFirst = false;
					foreach (var snapshot in snapshots) {
						if (snapshot.TimeOfSnapshotLocal < dayStart) {
							lastSnapshot = GetMoods (snapshot);
						} else {
							if (!addedFirst && dayStart <= finalSnapshotTime) {
								addedFirst = true;
								if (snapshot.TimeOfSnapshotLocal != dayStart) {
									if (dayStart == finalSnapshotTime)
										lastAdded = true;
									imager.AddSnapshot (dayStart, lastSnapshot, lastAdded);
									lastSnapshotAdded = dayStart;
								}
							}
							if (!lastAdded && snapshot.TimeOfSnapshotLocal <= finalSnapshotTime && snapshot.TimeOfSnapshotLocal <= dayEnd) {
								//if last snapshot more than 10 mins ago then add a pre-datapoint using last snapshot data
								if(snapshot.TimeOfSnapshotLocal.Subtract(lastSnapshotAdded).TotalMinutes > 10)
									imager.AddSnapshot (snapshot.TimeOfSnapshotLocal.AddMinutes(-10), lastSnapshot, false);

								if (snapshot.TimeOfSnapshotLocal == finalSnapshotTime)
									lastAdded = true;
								lastSnapshot = GetMoods (snapshot);
								imager.AddSnapshot (snapshot.TimeOfSnapshotLocal, lastSnapshot, lastAdded);
								lastSnapshotAdded = snapshot.TimeOfSnapshotLocal;
							}
						}
					}
					//always make sure there is a snapshot at the start of the day
					if (!addedFirst) {
						addedFirst = true;
						imager.AddSnapshot (dayStart, lastSnapshot, lastAdded);
						lastSnapshotAdded = dayStart;
					}


					
					if (!lastAdded && lastSnapshot != null && lastSnapshotAdded < dayEnd) {
						if (dayStart.AddDays (1) > reportEndLocal) {
							imager.AddSnapshot (finalSnapshotTime, lastSnapshot, true);
							lastSnapshotAdded = finalSnapshotTime;
						} else {
							imager.AddSnapshot (dayEnd, lastSnapshot, false);
							lastSnapshotAdded = dayEnd;
						}
					}
				

					using(var img = imager.DrawMapImage (imageFrame)){
						string filename = Path.Combine( imagesFolder, GetDayImageFileName(renderLevel, dayIndex)); 
						NSError err;
						img.AsPNG().Save(filename, true, out err);
					}


					Console.WriteLine ("Image drawn");
				
					dayStart = dayStart.AddDays (1);
					dayEnd = dayEnd.AddDays (1);
					if (dayEnd > reportEndLocal)
						dayEnd = reportEndLocal;

						dayIndex++;
				}
				Console.WriteLine ("Images generated");
			}

		}

		private static string GetDayImageFileName (RenderLevel renderLevel, int dayIndex)
		{
			var dayIndexStr = dayIndex.ToString();
			if(dayIndex < 10) dayIndexStr = "00" + dayIndexStr;
			if(dayIndex < 100) dayIndexStr = "0" + dayIndexStr;

			return string.Format("day-{0}-{1}.png", renderLevel.Index, dayIndexStr );
		}

		public static IEnumerable<GlobalMoodMapImager.MoodMapItem> GetMoodBreakdownForSnapshot (Snapshot snapshot)
		{
			return GetMoods(snapshot);
		}

		private static IEnumerable<GlobalMoodMapImager.MoodMapItem> GetMoods (Snapshot snapshot)
		{
			var snapMoods = from m 
				 in snapshot.Moods
	select new GlobalMoodMapImager.MoodMapItem (){
				Name = m.Mood.Name,
				DisplayIndex = m.Mood.DisplayIndex,
				DisplayColor = ColorTranslator.ToCGColor(m.Mood.DisplayColor),
				ResponseCount = m.ResponseCount,
				ResponsePercentage = m.ResponsePercentage,
				MoodType = m.Mood.MoodType
			};
			
			return snapMoods.OrderByDescending (m => m.DisplayIndex).ToList ();
		}

//		private static IEnumerable<GlobalMoodMapImager.MoodMapItem> GetMoods (IEnumerable<Mood> allMoods, Snapshot snapshot)
//		{
//			var snapMoods = from m in allMoods
//				join s in snapshot.Moods
//					on m.Name equals s.Mood.Name into gm
//					from subm in gm.DefaultIfEmpty ()
//					select new GlobalMoodMapImager.MoodMapItem (){
//								Name = m.Name,
//								DisplayIndex = m.DisplayIndex,
//								DisplayColor = ColorTranslator.ToCGColor(m.DisplayColor),
//								ResponseCount = subm == null ? 0 : subm.ResponseCount,
//								ResponsePercentage = subm == null ? 0 : subm.ResponsePercentage,
//								MoodType = m.MoodType
//							};
//			
//			return snapMoods.OrderByDescending (m => m.DisplayIndex).ToList ();
//		}

		private static IEnumerable<GlobalMoodMapImager.MoodMapItem> GenerateEmptyMoodList (IEnumerable<Mood> allMoods)
		{
			var pc = 100M / (decimal)allMoods.Count ();
			
			var snapMoods = from m in allMoods
				
			select new GlobalMoodMapImager.MoodMapItem (){
				Name = m.Name,
				DisplayIndex = m.DisplayIndex,
				DisplayColor = ColorTranslator.ToCGColor(m.DisplayColor),
				ResponseCount = 0,
				ResponsePercentage = pc,
				MoodType = m.MoodType
			};
			
			return snapMoods.OrderByDescending (m => m.DisplayIndex).ToList ();
		}

		private static ReportManager current;
		public static ReportManager Current {
			get {
				if(current == null) current = new ReportManager();
				return current;
			}
		}

		public static bool RefreshCurrentReport = false;

	}
}

