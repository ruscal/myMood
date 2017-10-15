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
	public class SnapshotToolView : UIView
	{
		UIImageView backgroundImage;
		SnapshotChartView chart;
		DateTime? currentSnapshotTime;
		MoodReport report;
		ScrollableMoodMapView globalMap;
		UIImageView staleLine;
		UIImageView staleRightMask;

		public SnapshotToolView (MoodReport report, ScrollableMoodMapView globalMap, RectangleF frame)
			:base(frame)
		{
			this.ContentMode = UIViewContentMode.Center;
			this.report = report;
			this.globalMap = globalMap;
			backgroundImage = new UIImageView (DrawSnapshotDropLine ());
			backgroundImage.Center = new PointF (this.Center.X, this.Center.Y);
			this.Add (backgroundImage);

			this.AddStaleLine();
		
			var center = new PointF(this.Bounds.Width/2, this.Bounds.Height/2);
			chart = new SnapshotChartView (this.report, new RectangleF (center.X + 10f,
			                                                            center.Y - 100f,
			                                              350f,
			                                              200f));
			this.Add (chart);

			this.currentSnapshotTime = GetCurrentSnapshotTime ();
			this.RefreshChart();


			this.globalMap.Scrolled +=
				this.Scrolling;

			this.globalMap.ScrollAnimationEnded += 
				this.Scrolled;	

			this.globalMap.DecelerationEnded += 
				this.Scrolled;		

			this.globalMap.ZoomingEnded +=
				this.Zoomed;
				
		}

		protected void Scrolling (object sender, EventArgs e)
		{
			//Console.WriteLine("Scrolling - {0} - {1}", this.globalMap.Decelerating, this.globalMap.DecelerationRate);
			this.RefreshChartPos();
		}

		protected void Scrolled (object sender, EventArgs e)
		{
			//Console.WriteLine("Scrolled");
			this.RefreshChart();
		}

		protected void Zoomed (object sender, EventArgs e)
		{
			this.RefreshChart();
			this.RepositionStaleLine();
		}


		public void MoveX (float changeX)
		{
			this.Frame = new RectangleF (this.Frame.X + changeX,
			                                       this.Frame.Y,
			                                       this.Frame.Width,
			                                       this.Frame.Height);
			this.RefreshChartPos();
			//this.RefreshChart();
		}

		public void Refresh (MoodReport report)
		{
			this.report = report;
			this.Refresh();
		}

		public void Refresh ()
		{
			if(this.Superview != null){
				RefreshChart();
			}
		}

		protected void RefreshChartPos ()
		{
			if(this.Superview != null){
				this.currentSnapshotTime = GetCurrentSnapshotTime ();
				this.chart.ShowSnapshotPosition (this.currentSnapshotTime);
				RepositionChart();
			}
		}

		protected void RefreshChart ()
		{
			if( this.Superview != null){
				Console.WriteLine("Refresh chart");
				this.currentSnapshotTime = GetCurrentSnapshotTime ();
				this.chart.ShowSnapshot (this.currentSnapshotTime);
				RepositionChart();
			}
		}

		protected void RepositionChart ()
		{
			var center = new PointF(this.Bounds.Width/2, this.Bounds.Height/2);
			if (this.chart.Frame.X + this.Frame.X + this.chart.Frame.Width > this.globalMap.Frame.Width) {
				this.chart.Frame = new RectangleF (new PointF (center.X - 10f - this.chart.Frame.Width, this.chart.Frame.Y), this.chart.Frame.Size);
				this.chart.ShowBackgroundLeft();
			} else if (this.chart.Frame.X + this.Frame.X  < this.globalMap.Frame.X) {
				this.chart.Frame = new RectangleF (new PointF (center.X + + 10f, this.chart.Frame.Y), this.chart.Frame.Size);
				this.chart.ShowBackgroundRight();
			}
		}

		protected void AddStaleLine ()
		{
			if (staleLine != null) {
				staleLine.RemoveFromSuperview();
				staleLine.Dispose();
			}

			staleLine = new UIImageView(DrawSnapshotStaleLine());
			this.Add (staleLine);
			RepositionStaleLine();

			staleRightMask = new UIImageView(DrawSnapshotStaleLine());
			staleRightMask.Frame = new RectangleF(Center.X, 0, staleRightMask.Bounds.Width, staleLine.Bounds.Height);
			this.Add(staleRightMask);
		}

		protected void RepositionStaleLine ()
		{
			var xPos = this.Bounds.Width/2 -(CalculateWidthOfAnHour() + staleLine.Bounds.Width);
			staleLine.Frame = new RectangleF(xPos, 0, this.staleLine.Bounds.Width, this.staleLine.Bounds.Height);
		}

		protected float CalculateWidthOfAnHour ()
		{
			var dayTimespan = ReportManager.DayEndTime.Subtract(ReportManager.DayStartTime).TotalHours;
			return (globalMap.CurrentRenderLevel.DayImageWidth / (float)dayTimespan)*this.globalMap.ZoomScale;
		}

		protected DateTime? GetCurrentSnapshotTime ()
		{
			return ChartHelper.ToCurrentTime (ReportManager.GetReportStartLocal(this.report), ReportManager.GetReportEndLocal(this.report), GetCurrentXPosition (), this.globalMap.CurrentRenderLevel);
		}

		protected float GetCurrentXPosition ()
		{
			return (this.Center.X + this.globalMap.ContentOffset.X) * 1/this.globalMap.ZoomScale;
		}

		protected UIImage DrawSnapshotStaleLine ()
		{	
			var w = this.Bounds.Width;
			var gap = 1f;
			var chartH = this.Frame.Height;
			
			UIGraphics.BeginImageContext (new SizeF (w, chartH));
			var g = UIGraphics.GetCurrentContext ();
			
			g.SetFillColor (new CGColor (0, 0, 0, 0.6f));
			g.FillRect (new RectangleF (new PointF (0, 0), new SizeF (w, chartH)));

			
			
			var lineImg = UIGraphics.GetImageFromCurrentImageContext ();
			
			UIGraphics.EndImageContext ();
			return lineImg;  
		}

		protected UIImage DrawSnapshotDropLine ()
		{	
			var w = 5f;
			var gap = 1f;
			var chartH = this.Frame.Height;
			
			UIGraphics.BeginImageContext (new SizeF (w * 2 + gap, chartH));
			var g = UIGraphics.GetCurrentContext ();
			
			g.SetFillColor (new CGColor (255, 255, 255, 0.3f));
			g.FillRect (new RectangleF (new PointF (0, 0), new SizeF (w, chartH)));
			g.FillRect (new RectangleF (new PointF (w + gap, 0), new SizeF (w, chartH)));
			
			
			var lineImg = UIGraphics.GetImageFromCurrentImageContext ();
			
			UIGraphics.EndImageContext ();
			return lineImg;  
		}

		protected override void Dispose (bool disposing)
		{


			this.globalMap.Scrolled -=
				this.Scrolling;
			
			
			this.globalMap.DecelerationEnded -= 
				this.Scrolled;
			
			this.globalMap.ZoomingEnded -=
				this.Zoomed;

			base.Dispose (disposing);
		}
	}
}

