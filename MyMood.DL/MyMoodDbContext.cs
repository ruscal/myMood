using System;
using System.IO;
using System.Linq;
using Vici.Core;
using Vici.CoolStorage;
using System.Collections.Generic;
using Discover.Core;
using MonoTouch.Foundation;

namespace MyMood.DL
{
	public class MyMoodViciDbContext
	{

		public static string DbFileName = "MyMood_v1-26.db3";

		public static double DbFileSize {
			get {
				FileInfo file = new FileInfo (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), DbFileName));
				return file.Exists ? Math.Round (file.Length / 1000.0, 1) : 0;
			}
		}

		private string _sqlCreateApplicationState = @"CREATE TABLE ApplicationState (EventName TEXT, EventTimeZone TEXT, EventTimeOffset INTEGER, PassCode TEXT PRIMARY KEY, ResponderId TEXT, APNSId TEXT, ResponderRegion TEXT, RunningMode INTEGER, InitialisedOn DATETIME, LiveDate DATETIME, WANWebServiceUri TEXT, LANWebServiceUri TEXT, SyncModeEnum INTEGER, LastSuccessfulServiceUpdate DATETIME null, LastSuccessfulDataPush DATETIME null, LastSuccessfulGlobalReportRequest DATETIME null, WarnSyncFailureAfterMins INTEGER, ForceUpdate INTEGER, CurrentVersion TEXT, UpdateAppUri TEXT, SyncDataInterval INTEGER, ConnectionTimeout INTEGER, SyncReportInterval INTEGER)";
		private string _sqlCreateMood = @"CREATE TABLE Mood (Id TEXT PRIMARY KEY, MoodCategoryId TEXT, Name TEXT, MoodType INTEGER, DisplayIndex INTEGER, DisplayColor TEXT)";
		private string _sqlCreateMoodCategory = @"CREATE TABLE MoodCategory (Id TEXT PRIMARY KEY, Name TEXT)";
		private string _sqlCreateLog = @"CREATE TABLE Log (Id TEXT PRIMARY KEY, LogLevel INTEGER, LogType TEXT, Message TEXT, Detail TEXT, Exception TEXT, TimeStamp DATETIME)";
		private string _sqlCreateMoodPrompt = @"CREATE TABLE MoodPrompt (Id TEXT PRIMARY KEY, Name TEXT, Title TEXT, TimeStamp DATETIME, NotificationText TEXT, ActiveFrom DATETIME, ActiveTil DATETIME)";
		private string _sqlCreateIxMoodPrompt = @"CREATE UNIQUE INDEX IF NOT EXISTS IX_MoodPrompt on MoodPrompt (Id, TimeStamp)";

		private string _sqlCreateMoodResponse = @"CREATE TABLE MoodResponse (Id TEXT PRIMARY KEY, MoodId TEXT, MoodPromptId TEXT NULL, TimeStamp datetime, CreatedOn DATETIME)";
		private string _sqlCreateIxMoodResponse = @"CREATE UNIQUE INDEX IF NOT EXISTS IX_MoodResponse on MoodResponse (Id, MoodPromptId, MoodId, TimeStamp)";
		private string _sqlCreateMoodReport = @"CREATE TABLE MoodReport (Id TEXT PRIMARY KEY, StartsOn DATETIME, EndsOn DATETIME, DayStartsOn DATETIME, DayEndsOn DATETIME, RequestedOn DATETIME, RequestCompleted INTEGER, LastRequestedZoomLevel INTEGER, LastRequestedDay DATETIME, ImagesGenerated INTEGER)";
		private string _sqlCreateSnapshot = @"CREATE TABLE Snapshot (Id TEXT PRIMARY KEY, MoodReportId TEXT, TimeOfSnapshot DATETIME, TimeOfSnapshotLocal DATETIME, TotalResponses INTEGER, IsFirstGlance INTEGER, CreatedOn datetime)";
		private string _sqlCreateMoodSnapshot = @"CREATE TABLE MoodSnapshot (Id TEXT PRIMARY KEY, SnapshotId TEXT, MoodId TEXT, ResponseCount INTEGER, ResponsePercentage FLOAT)";
		private string _sqlCreateActivity = @"CREATE TABLE Activity (Id TEXT PRIMARY KEY, Title TEXT, TimeStamp DATETIME, ActivityType INTEGER)";

		private string _sqlCreateIxMoodReport = @"CREATE UNIQUE INDEX IF NOT EXISTS IX_MoodReport on MoodReport (Id, RequestedOn, RequestCompleted, ImagesGenerated)";
		private string _sqlCreateIxSnapshot = @"CREATE UNIQUE INDEX IF NOT EXISTS IX_Snapshot on Snapshot (Id, MoodReportId, TimeOfSnapshotLocal)";
		private string _sqlCreateIxMoodSnapshot = @"CREATE UNIQUE INDEX IF NOT EXISTS IX_MoodSnapshot on MoodSnapshot (Id, SnapshotId, MoodId)";


		string _wanServiceUri = "https://www.learning-performance.com/MyMood/";
		//string _wanServiceUri = "https://www.learning-performance.com/MyMoodTest/";
		string _lanServiceUri = "http://10.40.0.11/mymood/";
		//string _lanServiceUri = "http://192.168.248.113:8080/";
		string _myMoodAppId = "ABBA6130-9663-4FA4-D1D4-08CF435A7DE9";
		string _myMoodEventName = "NovartisGPS13";
		//string _myMoodEventName = "NovartisTest";
		SyncMode _defaultSyncMode = SyncMode.WANonly;

		private List<MoodModel> _defaultMoods = new List<MoodModel> (){
			new MoodModel(){
				Name = "Passionate",
				DisplayIndex = 1,
				DisplayColor = "#F2932F",
				MoodType = MoodType.Positive
			},
			new MoodModel(){
				Name = "Excited",
				DisplayIndex = 2,
				DisplayColor = "#F9A924",
				MoodType = MoodType.Positive
			},
			new MoodModel(){
				Name = "Proud",
				DisplayIndex = 3,
				DisplayColor = "#FDBC18",
				MoodType = MoodType.Positive
			},
			new MoodModel(){
				Name = "Engaged",
				DisplayIndex = 4,
				DisplayColor = "#FFCC03",
				MoodType = MoodType.Positive
			},
			new MoodModel(){
				Name = "Optimistic",
				DisplayIndex = 5,
				DisplayColor = "#FFDA00",
				MoodType = MoodType.Positive
			},
			new MoodModel(){
				Name = "Frustrated",
				DisplayIndex = 6,
				DisplayColor = "#EB641C",
				MoodType = MoodType.Negative
			},
			new MoodModel(){
				Name = "Worried",
				DisplayIndex = 7,
				DisplayColor = "#C85424",
				MoodType = MoodType.Negative
			},
			new MoodModel(){
				Name = "Bored",
				DisplayIndex = 8,
				DisplayColor = "#A3442A",
				MoodType = MoodType.Negative
			},
			new MoodModel(){
				Name = "Deflated",
				DisplayIndex = 9,
				DisplayColor = "#8E4C31",
				MoodType = MoodType.Negative
			},
			new MoodModel(){
				Name = "Disengaged",
				DisplayIndex = 10,
				DisplayColor = "#775536",
				MoodType = MoodType.Negative
			}
		};

//		private string[] _defaultMoods2 = new string[]{
//			"Passionate",
//			"Excited",
//			"Proud",
//			"Engaged",
//			"Optimistic",
//			"Frustrated",
//			"Worried",
//			"Bored",
//			"Deflated",
//			"Disengaged"
//		};
//		private string[] _defaultMoodDisplayColors = new string[]{
//			"#F2932F",
//			"#F9A924",
//			"#FDBC18",
//			"#FFCC03",
//			"#FFDA00",
//			"#EB641C",
//			"#C85424",
//			"#A3442A",
//			"#8E4C31",
//			"#775536"
//		};
		private string[] _oldDbNames = new string[]{
			"MyMood_v1-25.db3"
		};

		public MyMoodViciDbContext (string version, DateTime? goLiveDate)
		{
			RemoveOldDatabases ();

			Init (version, null, goLiveDate);
		}

		protected void Init (string version, string deviceId, DateTime? goLiveDate)
		{
			string dbName = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), DbFileName);
			InitDB (dbName);
			InitApplicationState (version, deviceId, goLiveDate);
			InitDefaultMoods ();
			//SeedDemoReportData ();
		}

		protected void RemoveOldDatabases ()
		{
			// clean up old dbs
			foreach (var db in _oldDbNames) {
				string dbPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), db);
				bool dbExists = File.Exists (dbPath);
				if (dbExists)
					File.Delete (dbPath);
			}
		}

		public void ResetDatabase (string version, string deviceId)
		{
			// Reset database for go live
			string dbPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), DbFileName);
			bool dbExists = File.Exists (dbPath);
			if (dbExists)
				File.Delete (dbPath);
				
			Init (version, deviceId, null);
		}

		protected void InitDB (string dbName)
		{
			// The following line will tell CoolStorage where the database is,
			// create it if it does not exist, and call a delegate which
			// creates the necessary tables (only if the database file was
			// created new)
			CSConfig.SetDB (dbName, SqliteOption.CreateIfNotExists, () => {
				CSDatabase.ExecuteNonQuery (_sqlCreateApplicationState);
				CSDatabase.ExecuteNonQuery (_sqlCreateLog);
				CSDatabase.ExecuteNonQuery (_sqlCreateMoodCategory);
				CSDatabase.ExecuteNonQuery (_sqlCreateMood);
				CSDatabase.ExecuteNonQuery (_sqlCreateActivity);
				CSDatabase.ExecuteNonQuery (_sqlCreateMoodPrompt);
				CSDatabase.ExecuteNonQuery (_sqlCreateMoodResponse);
				CSDatabase.ExecuteNonQuery (_sqlCreateMoodReport);
				CSDatabase.ExecuteNonQuery (_sqlCreateSnapshot);
				CSDatabase.ExecuteNonQuery (_sqlCreateMoodSnapshot);
				//indexes
				CSDatabase.ExecuteNonQuery (_sqlCreateIxMoodPrompt);
				CSDatabase.ExecuteNonQuery (_sqlCreateIxMoodResponse);

				CSDatabase.ExecuteNonQuery (_sqlCreateIxMoodReport);
				CSDatabase.ExecuteNonQuery (_sqlCreateIxSnapshot);
				CSDatabase.ExecuteNonQuery (_sqlCreateIxMoodSnapshot);
			});


			MoodReport.AnyObjectDeleting += (MoodReport sender, ObjectDeleteEventArgs e) => {
				Console.WriteLine("Delete report event");
				sender.Snapshots.DeleteAll();
			};

			Snapshot.AnyObjectDeleting  += (Snapshot sender, ObjectDeleteEventArgs e) => {
				Console.WriteLine("Delete snapshot event");
				sender.Moods.DeleteAll();
			};
		}

		public void InitDefaultMoods ()
		{
	
			MoodCategory cat = MoodCategory.Count () > 0 ? MoodCategory.List ().FirstOrDefault (c => c.Name == MoodCategory.DefaultCategoryName) : null;
			if (cat == null) {
				cat = MoodCategory.New ();
				cat.Id = MoodCategory.DefaultCategoryId;
				cat.Name = MoodCategory.DefaultCategoryName;
				cat.Save ();
			}

			for (var i=0; i<_defaultMoods.Count(); i++) {
				var m = _defaultMoods [i];
				if (Mood.Count () == 0 || Mood.GetMoodByName(m.Name) == null) {
					Mood mood = Mood.New ();
					mood.Id = System.Guid.NewGuid ().ToString ();
					mood.Name = m.Name;
					mood.Category = cat;
					mood.DisplayIndex = m.DisplayIndex;
					mood.DisplayColor = m.DisplayColor;
					mood.MoodType = m.MoodType;
					mood.Save ();
				}
			}
		}

		protected void InitApplicationState (string version, string deviceId, DateTime? goLiveDate)
		{
			if (ApplicationState.Count () == 0) {
				var liveDate = goLiveDate.HasValue ? goLiveDate.Value : DateTime.UtcNow;

				ApplicationState app = new ApplicationState (){
				EventName = _myMoodEventName,
				EventTimeZone = "Central Europe Standard Time",
				EventTimeOffset = 1,
				PassCode = _myMoodAppId,
				ResponderId = Guid.NewGuid().ToString(),
				CurrentVersion = version,
				UpdateAppUri = _wanServiceUri,
				WANWebServiceUri = _wanServiceUri,
				LANWebServiceUri = _lanServiceUri,
					SyncModeEnum = (int)_defaultSyncMode,
				SyncDataInterval = 60,
				SyncReportInterval = 60,
				RunningMode = RunningMode.FirstUse,
				LiveDate = liveDate,
				InitialisedOn = DateTime.UtcNow,
				WarnSyncFailureAfterMins = 5,
				ForceUpdate = 0,
				ConnectionTimeout = 15000,
					APNSId = deviceId
			};
				app.Save ();
			}
		}


		private static MyMoodViciDbContext db;
		public static void Initialise ()
		{
			if (db == null) {
				//init db
				string version = NSBundle.MainBundle.ObjectForInfoDictionary ("Version").ToString ();
				db = new MyMoodViciDbContext (version, null);
			
				var appState = ApplicationState.Current;
				appState.SyncMode = SyncMode.WANonly;
				appState.Save ();
			}
		}

		private void SeedDemoReportData ()
		{
			if (MoodReport.Count () == 0) {
				var dayStart = new TimeSpan(6, 0, 0);
				var dayEnd = new  TimeSpan(21, 0, 0);
				var dayMins = dayEnd.Subtract(dayStart).TotalMinutes;
				var dataPointsPerDay = 10;

				var now = DateTime.UtcNow;
				DateTime reportStart = now.AddDays(-3).Date.Add(dayStart);
				DateTime reportEnd = now.Date.Add(dayEnd);

				MoodReport report = MoodReport.New ();
				report.Id = System.Guid.NewGuid ().ToString ();
				report.StartsOn = reportStart;
				report.EndsOn = reportEnd;
				report.RequestCompleted = 1;
				report.RequestedOn = reportEnd;
				report.Save ();

				var moods = Mood.All ().ToList ();
				var minutes = reportEnd.Subtract (reportStart).TotalMinutes;
				var step = (double)dayMins / (double)(dataPointsPerDay-1);
				var snapTime = reportStart;
				Random random = new Random ();
				var timeOffset = ApplicationState.Current.EventTimeOffset;
				//add first glance
				while (snapTime < reportEnd) {
					var snapshot = Snapshot.New ();
					var totalResponses = 100;
					snapshot.Id = System.Guid.NewGuid ().ToString ();
					snapshot.MoodReport = report;
					//snapshot.IsFirstGlance = true;
					snapshot.TimeOfSnapshot = snapTime;
					snapshot.TimeOfSnapshotLocal = snapTime.ToLocalTime(timeOffset);
					snapshot.TotalResponses = totalResponses;
					snapshot.Save ();
		

					foreach (var m in moods.OrderBy(o => o.DisplayIndex)) {
						var responses = m.DisplayIndex < moods.Count () ? random.Next (totalResponses/random.Next(1, 3)) : totalResponses;
						MoodSnapshot ms = MoodSnapshot.New ();
						ms.Id = System.Guid.NewGuid ().ToString ();
						ms.Mood = m;
						ms.ResponseCount = responses;
						ms.ResponsePercentage = responses;
						ms.Snapshot = snapshot;
						ms.Save ();
						totalResponses -= responses;
					}


					snapTime = snapTime.AddMinutes (random.Next(100, (int)(step*10))/10);
					if(snapTime > snapTime.Date.Add(dayEnd)){
						snapTime = snapTime.Date.AddDays(1).Add(dayStart);
					}
				}

				var day = reportStart.Date;

				Activity act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(18);
				act.Title =  "Welcome Buffet";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(18);
				act.Title =  "Arrival mood";
				act.Save();

				day = day.AddDays(1);

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(8);
				act.Title =  "Prior to meeting";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(8);
				act.Title =  "Corporate Opening";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(9);
				act.Title =  "Guest Speaker";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(9.5);
				act.Title =  "Day 1 Start";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(14);
				act.Title =  "Our Culture";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(17.5);
				act.Title =  "PM Sessions Day 1";
				act.Save();

				day = day.AddDays(1);

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(8);
				act.Title =  "Future Is Now";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(9.5);
				act.Title =  "Day 2 Start";
				act.Save();
				
				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(9);
				act.Title =  "AM Sessions";
				act.Save();
				
				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(14);
				act.Title =  "PM Sessions";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(17.5);
				act.Title =  "PM Sessions Day 2";
				act.Save();

				day = day.AddDays(1);

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(9.5);
				act.Title =  "Day 3 Start";
				act.Save();
				
				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(8.5);
				act.Title =  "Share & Learn";
				act.Save();
				
				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(12);
				act.Title =  "Network Lunches";
				act.Save();
				
				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.Event;
				act.TimeStamp = day.AddHours(15);
				act.Title =  "Close";
				act.Save();

				act = Activity.New();
				act.Id = System.Guid.NewGuid().ToString();
				act.ActivityType = ActivityType.MoodPrompt;
				act.TimeStamp = day.AddHours(14.5);
				act.Title =  "Final Mood";
				act.Save();



	
				
//				for(var i=0;i<7;i++){
//					Random ran = new Random ((i+1)*100);
//					Activity act = Activity.New();
//					act.Id = System.Guid.NewGuid().ToString();
//					act.ActivityType = ActivityType.MoodPrompt;
//					act.TimeStamp = report.StartsOn.AddMinutes(ran.Next (timespan));
//					act.Title =  string.Format("Prompt - {0}", i);
//					act.Save();
//				}
			}

		}

		private class MoodModel
		{
			public string Name {
				get;
				set;
			}

			public int DisplayIndex {
				get;
				set;
			}

			public string DisplayColor {
				get;
				set;
			}

			public MoodType MoodType {
				get;
				set;
			}
		}
	}
}

