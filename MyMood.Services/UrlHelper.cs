
using System;
using System.Collections.Generic;
using System.Linq;

using MyMood.DL;


namespace MyMood.Services
{
	public class UrlHelper 
	{
		public static string ToUrl (string appUri, string eventName, string action, string passCode)
		{
			return string.Concat (appUri, "App/", ApplicationState.Current.EventName, "/", action, "/", passCode);
		}

		public static string ToUpdateUrl(string appUri, string eventName, string passCode){
			return ToUrl(appUri, eventName, "Install", passCode);
		}
	}
}
