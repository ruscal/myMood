using System;
using Vici.CoolStorage;


namespace MyMood.DL
{
		[MapTo("Log")]
		public class Log : CSObject<Log,string>
		{
			public string Id {
				get { return (string)GetField("Id"); }
				set{ SetField("Id", value); }
			}

		public int LogLevel {
			get { return (int)GetField("LogLevel"); }
			set{ SetField("LogLevel", value); }
		}
			
			public string LogType {
				get { return (string)GetField("LogType"); }
				set{ SetField("LogType", value); }
			}
			
			public string Message {
				get { return (string)GetField("Message"); }
				set{ SetField("Message", value); }
			}
			
			public string Detail {
				get { return (string)GetField("Detail"); }
				set{ SetField("Detail", value); }
			}
			
			public string Exception {
				get { return (string)GetField("Exception"); }
				set{ SetField("Exception", value); }
			}
			
			public DateTime TimeStamp {
				get { return (DateTime)GetField("TimeStamp"); }
				set{ SetField("TimeStamp", value); }
			}
		}
}

