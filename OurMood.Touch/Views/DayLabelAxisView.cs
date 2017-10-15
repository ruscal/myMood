using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using MyMood.DL;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using Discover.Drawing;
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreAnimation;
using Discover.Core;

namespace OurMood.Touch
{
	public class DayLabelAxisView : UIView
	{
		ScrollableMoodMapView globalMap;
		List<UILabel> dayLabels = new List<UILabel>();
		float labelW = 90;
		float labelH = 20;
		UIFont font = UIFont.FromName ("HelveticaNeue-CondensedBold", 16.0f);

		public DayLabelAxisView (RectangleF frame, ScrollableMoodMapView globalMap)
			: base(frame)
		{
			this.globalMap = globalMap;
		}

		public void Refresh (MoodReport report, RenderLevel renderLevel)
		{
			var viewStartTime = ChartHelper.ToCurrentTime (report.StartsOn, report.EndsOn, this.globalMap.ContentOffset.X* 1/this.globalMap.ZoomScale, renderLevel, true).Value;
			var viewEndTime = ChartHelper.ToCurrentTime (report.StartsOn, report.EndsOn, (this.globalMap.ContentOffset.X + this.Frame.Width) * 1/this.globalMap.ZoomScale, renderLevel, true).Value;

			var labelTime = viewStartTime;
			var labelIndex = 0;
			var labelPos = 0f;
			while (labelTime < viewEndTime) {
				var pxTilEndOfDay = (float)(ReportManager.DayEndTime.Subtract (labelTime.TimeOfDay).TotalHours / ReportManager.DayEndTime.Subtract (ReportManager.DayStartTime).TotalHours)
					* renderLevel.DayImageWidth * this.globalMap.ZoomScale;
				if (pxTilEndOfDay >= labelW) {
					UILabel label;
					if (this.dayLabels.Count () > labelIndex) {
						label = this.dayLabels [labelIndex];
					} else {
						label = new UILabel (new RectangleF (0, 0, labelW, labelH));
						label.BackgroundColor = UIColor.Clear;
						label.TextColor = UIColor.White;
						label.Font = font;
						this.Add (label);
						this.dayLabels.Add (label);
					}
					label.Text = labelTime.ToString ("ddd dd MMM");
					label.Frame = new RectangleF (new PointF (labelPos, label.Frame.Y), label.Frame.Size);
					labelIndex++;
				}
				labelTime = labelTime.Date.AddDays (1);
				labelPos = (ChartHelper.ToXPos (report.StartsOn, report.EndsOn, labelTime, renderLevel.DayImageWidth, ReportManager.DayStartTime, ReportManager.DayEndTime, renderLevel.DayMarkerWidth)
						 *this.globalMap.ZoomScale)-this.globalMap.ContentOffset.X;
			}

			while (labelIndex < this.dayLabels.Count()) {
				var i = this.dayLabels.Count()-1;
				this.dayLabels[i].RemoveFromSuperview();
				this.dayLabels.RemoveAt (i);
			}


		}
	}
}

