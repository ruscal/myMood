using System;
using Discover.Logging;
using MyMood.DL;
using Discover;
using Vici.CoolStorage;
using System.Linq;
using System.Collections.Generic;

namespace MyMood.Services
{
	public class MyMoodLogger : ILogger
	{
		public MyMoodLogger ()
		{
			//clean out old logs
			var oldLogDate = DateTime.UtcNow.AddDays (-1);
			var logs = MyMood.DL.Log.List ("LogLevel > 2 and TimeStamp < @OldLogDate", new{ OldLogDate = oldLogDate });

			logs.DeleteAll();

		}

		#region ILogger implementation

		public void Log (string message, string detail, int logLevel)
		{
			Console.WriteLine("LOG : " + message);
			Log log = new Log(){
				Id = Guid.NewGuid().ToString(),
				LogType = "Info",
				TimeStamp = DateTime.UtcNow,
				Message = message,
				Detail = detail,
				LogLevel = logLevel
			};
			log.Save();
		}

		public void Error (string message, Exception ex, int logLevel)
		{
			Console.WriteLine("ERROR : " + message);
			Log log = new Log(){
				Id = Guid.NewGuid().ToString(),
				LogType = "Error",
				TimeStamp = DateTime.UtcNow,
				Message = message,
				Exception = ex.ToString(),
				LogLevel = logLevel
			};
			log.Save();
		}

		#endregion

		private static MyMoodLogger logger;

		public static MyMoodLogger Current {
			get {
				if(logger == null) logger = new MyMoodLogger();
				return logger;
			}
		}
	}
}

