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
	public class GlobalMoodMapView : UIView
	{
		UIView chartContainer;
		UIImageView chartOverlay;
		ScrollableMoodMapView globalMoodMap;
		UIImageView chartKey;
		UIView axisContainer;
		UIView axis;
		UIImage axisDropLine;
		PromptsSwitchView promptSwitchView;
		EventsSwitchView eventSwitchView;
		SnapshotToolSwitchView snapshotSwitchView;
		UIView snapshotOverlay;
		UIView snapshotBtnOverlay;
		DayLabelAxisView dayLabelAxis;
		SnapshotToolView snapshotToolView;
		SnapshotDragButtonView snapshotDragBtn;

		List<UIView> axisLabels;

		float tickHeight = 20f;
		float axisDropLineWidth = 6f;

		public ZoomLevel CurrentZoomLevel {
			get;
			set;
		}

		public DateTime ReportStartLocal {
			get;
			set;
		}

		public DateTime ReportEndLocal {
			get;
			set;
		}


		public MoodReport CurrentReport {
			get;
			protected set;
		}

		public GlobalMoodMapView (RectangleF frame)
			:base(frame)
		{
			this.BuildView();
			this.CurrentZoomLevel = ZoomLevel.ZoomLevels.Where(z => z.ZoomIndex == 2).First();
		}

		public void Refresh (MoodReport report)
		{
			if (report != null) {

				this.snapshotSwitchView.Reset();
				this.promptSwitchView.Reset();
				this.eventSwitchView.Reset();
				this.SetCurrentReport (report);
				this.globalMoodMap.Refresh (report, this.CurrentZoomLevel);
				this.BuildAxis ();
				if(this.snapshotToolView != null)
					this.snapshotToolView.Refresh();
			}
		}

		protected void ZoomLevelChanged ()
		{
			this.CurrentZoomLevel = this.globalMoodMap.CurrentZoomLevel;
			this.BuildAxis();
			this.ReScaleAxis();
		}

		protected void SetCurrentReport (MoodReport report)
		{
			this.CurrentReport = report;
			this.ReportStartLocal = ReportManager.GetReportStartLocal(report);
			this.ReportEndLocal = ReportManager.GetReportEndLocal(report);
		}

		protected void BuildView ()
		{
			this.chartContainer = new UIView (new RectangleF (22f, 169.5f, 975f, 444f));
			this.chartContainer.BackgroundColor = new UIColor (18, 23, 29, 1);
			this.Add (this.chartContainer);
			
			this.chartOverlay = new UIImageView (this.Bounds);
			this.chartOverlay.Image = Resources.MoodMapWindow;
			this.chartOverlay.UserInteractionEnabled = false;
			this.Add (this.chartOverlay);

			var chartFrame = new RectangleF (0, 0, this.chartContainer.Bounds.Width, this.chartContainer.Bounds.Height);
			this.globalMoodMap = new ScrollableMoodMapView (chartFrame, 
			                                               ReportManager.DayStartTime,
			                                                ReportManager.DayEndTime);
			this.chartContainer.Add (this.globalMoodMap);
			this.globalMoodMap.ZoomLevelChanged += (object sender, EventArgs e) => {
				this.ZoomLevelChanged();
			};

			this.chartKey = new UIImageView (new RectangleF (25f, 693f, 974f, 50f));
			this.chartKey.Image = Resources.MoodMapKey;
			this.Add (this.chartKey);

			this.axisContainer = new UIView (new RectangleF (this.chartContainer.Frame.X, 
			                                               this.chartContainer.Frame.Y ,
			                                               this.chartContainer.Frame.Width,
			                                               this.chartContainer.Frame.Height + 100));
			this.axisContainer.ClipsToBounds = true;
			this.axisContainer.UserInteractionEnabled = false;
			this.Add (this.axisContainer);

			this.dayLabelAxis = new DayLabelAxisView(new RectangleF(this.chartContainer.Frame.X,
			                                                        this.chartContainer.Frame.Y-20f, 
			                                                        this.chartContainer.Frame.Width,
			                                                        20f), this.globalMoodMap);
			this.dayLabelAxis.UserInteractionEnabled = false;
			this.Add(this.dayLabelAxis);

			this.snapshotSwitchView = new SnapshotToolSwitchView (new PointF (670f, 20f));
			this.snapshotSwitchView.Changed += (object sender, EventArgs e) => {
				this.ToggleSnapshotTool ();
			};
			this.Add (this.snapshotSwitchView);

			this.promptSwitchView = new PromptsSwitchView (new PointF (780f, 20f));
			this.promptSwitchView.Changed += (object sender, EventArgs e) => {
				this.TogglePrompts ();
			};
			this.Add (this.promptSwitchView);

			this.eventSwitchView = new EventsSwitchView (new PointF (890f, 20f));
			this.eventSwitchView.Changed += (object sender, EventArgs e) => {
				this.ToggleEvents ();
			};
			this.Add (this.eventSwitchView);

			this.globalMoodMap.Scrolled += (object sender, EventArgs e) => {
				this.RepositionAxis ();
			};
			this.globalMoodMap.DidZoom += (object sender, EventArgs e) => {
				this.ReScaleAxis();
				this.RepositionAxis ();
			};

		}

		protected void BuildAxis ()
		{
			if (this.axis != null) {
				this.axis.RemoveFromSuperview ();
				this.axis = null;
			}


			this.axis = new UIView (new RectangleF (0, 0, this.globalMoodMap.ContentSize.Width, this.axisContainer.Bounds.Height));
			this.axis.UserInteractionEnabled = false;
			this.axis.ContentMode = UIViewContentMode.Left;
			this.axisContainer.Add (this.axis);
			this.axisLabels = new List<UIView>();

			var interval = this.CurrentZoomLevel.IntervalMins;

			var day = this.ReportStartLocal.Date.Add (ReportManager.DayStartTime);
			var timeStamp = this.ReportStartLocal.AddMinutes (this.CurrentZoomLevel.OffsetMins);

			while (timeStamp < this.ReportEndLocal) {
				var label = BuildAxisLabel (timeStamp);
				this.axisLabels.Add(label);
				this.axis.Add (label);
				timeStamp = timeStamp.AddMinutes (interval);
				if (timeStamp >= day.Date.Add (ReportManager.DayEndTime)) {
					day = day.AddDays (1);
					timeStamp = day.AddMinutes (this.CurrentZoomLevel.OffsetMins);
				}
			}

			RepositionAxis();
		}

		protected void ReScaleAxis ()
		{
			if (this.axis != null) {
				this.axis.Transform = CGAffineTransform.MakeScale(this.globalMoodMap.ZoomScale, 1);
				foreach(var label in this.axisLabels){
					label.Transform = CGAffineTransform.MakeScale(1/this.globalMoodMap.ZoomScale, 1);
				}
			}
		}

		protected void RepositionAxis ()
		{
			if (this.axis != null) {
				this.axis.Frame = new RectangleF (new PointF (-(this.globalMoodMap.ContentOffset.X), this.axis.Frame.Location.Y),
			                                 this.axis.Frame.Size);
			}

			this.dayLabelAxis.Refresh(this.CurrentReport, this.CurrentZoomLevel.RenderLevel);
		}

		protected void ToggleSnapshotTool ()
		{
			if (this.snapshotOverlay == null) {
				BuildSnaphotOverlay ();
			} else {
					this.RemoveSnapshotOverlay();
			}
		}

		protected void ToggleEvents ()
		{
			this.globalMoodMap.ToggleEventMarkers();
		}

		protected void TogglePrompts ()
		{
			this.globalMoodMap.TogglePromptMarkers();
		}

		protected UIView BuildAxisLabel(DateTime timeStampLocal){
			var x = ChartHelper.ToXPos(this.ReportStartLocal, 
			                           this.ReportEndLocal, 
			                           timeStampLocal, 
			                           this.CurrentZoomLevel.RenderLevel.DayImageWidth,
			                           ReportManager.DayStartTime,
			                           ReportManager.DayEndTime,
			                           this.CurrentZoomLevel.RenderLevel.DayMarkerWidth);
			var titleH = 20f;
			var titleTopMargin = 5f;
			var labelW = 50f;  
			var labelH = this.tickHeight + titleH + titleTopMargin;
			x = x - (labelW/2);
			var y = this.globalMoodMap.Frame.Y;
			if(this.axisDropLine == null) this.axisDropLine = DrawAxisDropLine();

			UIView axisLabel = new UIView(new RectangleF(x, y, labelW, this.globalMoodMap.Frame.Height));
			UIImageView dropLine = new UIImageView(this.axisDropLine);
			dropLine.Center = new PointF(labelW/2, dropLine.Center.Y);
			axisLabel.Add(dropLine);

			UILabel title = new UILabel(new RectangleF(0, this.globalMoodMap.Frame.Height + tickHeight + titleTopMargin, labelW, titleH));
			title.Font = UIFont.FromName("HelveticaNeue-CondensedBold",22.0f);
			title.TextColor = UIColor.White;
			title.Text = timeStampLocal.ToString("H:mm");
			title.BackgroundColor = UIColor.Clear;
			title.TextAlignment = UITextAlignment.Center;
			axisLabel.Add(title);

			return axisLabel;
		}

		protected void RemoveSnapshotOverlay ()
		{
			if (this.snapshotToolView != null) {
				this.snapshotToolView.Dispose ();
				this.snapshotToolView.RemoveFromSuperview ();
				this.snapshotToolView = null;
			}

			if (this.snapshotOverlay != null) {
				this.snapshotOverlay.RemoveFromSuperview ();
				this.snapshotOverlay.Dispose ();
				this.snapshotOverlay = null;
			}

			if (this.snapshotBtnOverlay != null) {
				this.snapshotBtnOverlay.RemoveFromSuperview ();
				this.snapshotBtnOverlay.Dispose ();
				this.snapshotBtnOverlay = null;
			}
		}

		protected void BuildSnaphotOverlay ()
		{
			if (this.snapshotOverlay != null) {
				this.RemoveSnapshotOverlay();
			}


			this.snapshotOverlay = new UIView (new RectangleF (this.chartContainer.Frame.X, this.chartContainer.Frame.Y, this.chartContainer.Frame.Width, this.chartContainer.Frame.Height));
			this.snapshotOverlay.UserInteractionEnabled = false;
			this.snapshotOverlay.ContentMode = UIViewContentMode.Left;
			this.Add (this.snapshotOverlay);

			this.snapshotBtnOverlay = new UIView(new RectangleF(this.chartContainer.Frame.X,
			                                                    this.chartContainer.Frame.Y - 30f,
			                                                    this.chartContainer.Frame.Width,
			                                                    30f));
			this.snapshotBtnOverlay.ContentMode = UIViewContentMode.Left;
			this.snapshotBtnOverlay.UserInteractionEnabled = true;
			this.Add(this.snapshotBtnOverlay);

			snapshotToolView = new SnapshotToolView(this.CurrentReport, 
			                                        this.globalMoodMap,
			                                        new RectangleF(0, 0, this.snapshotOverlay.Frame.Width, this.snapshotOverlay.Frame.Height));
			this.snapshotOverlay.Add(snapshotToolView);
			this.snapshotToolView.Refresh();

			if (this.snapshotDragBtn != null) {
				this.snapshotDragBtn.RemoveFromSuperview();
				this.snapshotDragBtn = null;
			}

			this.snapshotDragBtn = new SnapshotDragButtonView(new PointF(0, 0));
			this.snapshotDragBtn.Center = new PointF(this.snapshotToolView.Center.X, 15f);
			this.snapshotDragBtn.DragTarget = this.snapshotToolView;
			this.snapshotBtnOverlay.Add(this.snapshotDragBtn);
		}

		protected UIImage DrawAxisDropLine ()
		{	
			var w = axisDropLineWidth;
			var chartH = this.globalMoodMap.Frame.Height;
			var tickH = tickHeight;

			UIGraphics.BeginImageContext (new SizeF(w, chartH+tickH));
			var g = UIGraphics.GetCurrentContext ();

			g.SetFillColor(new CGColor(255, 255,  255, 0.1f));
			g.FillRect(new RectangleF(new PointF(0, 0), new SizeF(w, chartH)));
			g.SetFillColor(new CGColor(255, 255,  255));
			g.FillRect(new RectangleF(new PointF(0, chartH), new SizeF(w, tickH)));

			
			var lineImg = UIGraphics.GetImageFromCurrentImageContext();
			
			UIGraphics.EndImageContext();
			return lineImg;  
		}

		protected override void Dispose (bool disposing)
		{
			RemoveSnapshotOverlay ();

			foreach (var label in axisLabels) {
				label.Dispose();

			}
			axisLabels = null;

			promptSwitchView.Dispose ();
			promptSwitchView = null;

			eventSwitchView.Dispose ();
			eventSwitchView = null;

			snapshotSwitchView.Dispose ();
			snapshotSwitchView = null;

			axis.Dispose ();
			axis = null;

			axisDropLine.Dispose ();
			axisDropLine = null;

			axisContainer.Dispose ();
			axisContainer = null;

			globalMoodMap.Dispose ();
			globalMoodMap = null;

			chartOverlay.Dispose ();
			chartOverlay = null;

			chartContainer.Dispose ();
			chartContainer = null;

			chartKey.Dispose ();
			chartKey = null;



			dayLabelAxis.Dispose ();
			dayLabelAxis = null;



			base.Dispose (disposing);
		}
	}
}

