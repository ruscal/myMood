using System;

namespace OurMood.Touch
{
	public static class ChartHelper
	{
		//only used by imager for a day so no need for day marker width at mo - should prob update however
		public static float ToXPos(DateTime reportStart, DateTime reportEnd, DateTime currentTime, float chartWidth){
			return (float)currentTime.Subtract (reportStart).TotalMilliseconds / (float)reportEnd.Subtract (reportStart).TotalMilliseconds * (float)chartWidth;
		}

		public static float ToXPos(DateTime reportStart, DateTime reportEnd, DateTime currentTime, float dayWidth, TimeSpan dayStart, TimeSpan dayEnd, float dayMargin){
			if(currentTime < reportStart) currentTime = reportStart;
			if(currentTime > reportEnd) currentTime = reportEnd;

			var days = (float)currentTime.Date.Subtract (reportStart.Date).Days;
			if(currentTime < currentTime.Date.Add(dayStart)) currentTime = currentTime.Date.Add(dayStart);
			if(currentTime > currentTime.Date.Add(dayEnd)) currentTime = currentTime.Date.Add(dayEnd);

			return days*dayWidth
				+ (days+1) * dayMargin
				+  (float)currentTime.Subtract (currentTime.Date.Add(dayStart)).TotalMinutes / (float)dayEnd.Subtract(dayStart).TotalMinutes * (float)dayWidth;
	
		}

		public static DateTime? ToCurrentTime (DateTime reportStart, DateTime reportEnd, float xPos, RenderLevel renderLevel                               )
		{
			return ToCurrentTime(reportStart, reportEnd, xPos, renderLevel, false);
		}

		public static DateTime? ToCurrentTime(DateTime reportStart, DateTime reportEnd, float xPos, RenderLevel renderLevel, bool returnClosestIfNull){

			var startTime = reportStart.Date.Add(ReportManager.DayStartTime);
			var endTime = reportEnd.Date.Add(ReportManager.DayEndTime);
		
			var minsInaDay = (ReportManager.DayEndTime.Subtract(ReportManager.DayStartTime).TotalMinutes);
			//adjust to remove marker widths
			var daysInCurentPos = Math.Floor(xPos / renderLevel.TotalDayWidth);
			var posX = (xPos / renderLevel.TotalDayWidth - daysInCurentPos) * renderLevel.TotalDayWidth - renderLevel.DayMarkerWidth;
			if(posX < 0){
				if(returnClosestIfNull){
					daysInCurentPos = daysInCurentPos - 1;
					if(daysInCurentPos < 0) return startTime;
					posX = renderLevel.DayImageWidth;
				}else{
					return null;
				}
			}
			var minsToAdd =  (posX / renderLevel.DayImageWidth)* minsInaDay ;
			var currentTime =  startTime.AddDays(Math.Floor(daysInCurentPos)).AddMinutes((double)minsToAdd);
			if(currentTime < startTime) return startTime;
			if(currentTime > endTime) return endTime;
			return currentTime;
		}


		public static float CalculateHoursPerWindow(RenderLevel level, float scale, float windowWidth){
			var hoursInDay = (float)ReportManager.DayEndTime.Subtract(ReportManager.DayStartTime).TotalHours;
			var dayWidth = level.TotalDayWidth * scale;
			return (windowWidth / dayWidth)*hoursInDay;
		}

		public static float CalculateScaleOfChart(RenderLevel level, float hoursPerWindow, float windowWidth){
			var hoursInDay = (float)ReportManager.DayEndTime.Subtract(ReportManager.DayStartTime).TotalHours;
			var days = hoursPerWindow / hoursInDay;
			var scaledDayW = windowWidth / days;
			var scale = scaledDayW / level.TotalDayWidth;
			return scale;
		}


	}
}

