using System;

namespace Discover.Core
{
	public static class DateTimeHelper
	{
		public static DateTime EndOfDay(this DateTime dt)
		{
			return dt.Date.AddDays(1).AddTicks(-1);
		}
		
		public static DateTime AddBusinessDays(this DateTime dt, int days)
		{
			var sign = Math.Sign(days);
			var unsignedDays = Math.Abs(days);
			for (var i = 0; i < unsignedDays; i++)
			{
				do
				{
					dt = dt.AddDays(sign);
				}
				while (dt.DayOfWeek == DayOfWeek.Saturday ||
				       dt.DayOfWeek == DayOfWeek.Sunday);
			}
			return dt;
			
		}
		
		public static DateTime ToLocalTime (this DateTime dt, string timeZoneId)
		{
			if (string.IsNullOrWhiteSpace (timeZoneId))
				return dt.ToLocalTime ();
			try {
				var tz = TimeZoneInfo.FindSystemTimeZoneById (timeZoneId);
				if (tz == null)
					throw new ArgumentException ("Invalid timezone - " + timeZoneId);
				return TimeZoneInfo.ConvertTimeFromUtc(dt.ToUniversalTime(), tz);
			} catch (Exception ex) {
				throw new ArgumentException(string.Format("Could not parse timeZoneId - check is valid IOS timeZone [{0}]", timeZoneId), ex);
			}

		}

		public static DateTime ToLocalTime (this DateTime dt, int offset)
		{
			DateTime local = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Local);
			local = local.AddHours(offset);
			return local;
		}

		public static DateTime ToDateTime(this MonoTouch.Foundation.NSDate date)
		{
			return (new DateTime(2001,1,1,0,0,0, DateTimeKind.Utc)).AddSeconds(date.SecondsSinceReferenceDate);
		}
		
		public static MonoTouch.Foundation.NSDate ToNSDate(this DateTime date)
		{
			return MonoTouch.Foundation.NSDate.FromTimeIntervalSinceReferenceDate((date-(new DateTime(2001,1,1,0,0,0, DateTimeKind.Utc))).TotalSeconds);
		}
	}
}

