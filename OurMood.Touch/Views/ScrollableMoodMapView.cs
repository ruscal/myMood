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
	public class ScrollableMoodMapView : UIScrollView
	{
		public event EventHandler<EventArgs> ZoomLevelChanged;

		public DateTime ReportStartLocal {
			get;
			set;
		}
		
		public DateTime ReportEndLocal {
			get;
			set;
		}

		public RectangleF OriginalFrame {
			get;
			set;
		}

		//UIImageView globalMoodMap;
		GlobalMoodMapContainerView globalMoodMap;
		List<PromptMarkerView> promptMarkers;
		List<EventMarkerView> eventMarkers;
		bool eventMarkersVisible = false;
		bool promptMarkersVisible = false;
		UIView futureMaskView;

		public RenderLevel CurrentRenderLevel {
			get {
				return this.CurrentZoomLevel == null ? null : this.CurrentZoomLevel.RenderLevel;
			}
		}

		public ZoomLevel CurrentZoomLevel {
			get;
			protected set;
		}

		public ZoomLevel DefaultZoomLevel {
			get;
			protected set;
		}

		public MoodReport MoodReport {
			get;
			set;
		}

		public ScrollableMoodMapView (RectangleF frame, TimeSpan dayStartTime, TimeSpan dayEndTime)
			:base(frame)
		{

			this.ScrollEnabled = true;


			this.OriginalFrame = frame;
			this.BackgroundColor = UIColor.DarkGray;
			this.eventMarkers = new List<EventMarkerView> ();
			this.promptMarkers = new List<PromptMarkerView> ();
			
//			this.PinchGestureRecognizer.Enabled = false;
//			pinchGestureRecognizer = new UIPinchGestureRecognizer (HandlePinchGesture);
//			pinchGestureRecognizer.Delegate = new GestureDelegate ();
//			this.AddGestureRecognizer (pinchGestureRecognizer);


			this.ViewForZoomingInScrollView = delegate(UIScrollView scrollView) {
				return globalMoodMap;
			};   

			this.DidZoom += (object sender, EventArgs e) => {
				Zoomed ();
			};

			// create a new tap gesture
			UITapGestureRecognizer tapGesture = null;
			
			NSAction action = () => {
				this.ZoomToDefault ();
			};
			
			tapGesture = new UITapGestureRecognizer (action);
			// configure it
			tapGesture.NumberOfTapsRequired = 2;
			// add the gesture recognizer to the view
			this.AddGestureRecognizer (tapGesture);
		}
		
		public void Refresh (MoodReport report, ZoomLevel zoomLevel)
		{
			Console.WriteLine ("Refresh data  - {0}", zoomLevel.Name);
			this.MoodReport = report;
			this.ReportStartLocal = ReportManager.GetReportStartLocal (report);
			this.ReportEndLocal = ReportManager.GetReportEndLocal (report);
			this.DefaultZoomLevel = zoomLevel;
			this.SetZoomLevel (zoomLevel);
			this.LoadImages ();

			if (this.eventMarkersVisible) {
				BuildEventMarkers ();
			} else {
				this.ClearEventMarkers ();
			}
			if (this.promptMarkersVisible) {
				BuildPromptMarkers ();
			} else {
				this.ClearPromptMarkers ();
			}

			var eventNow = DateTime.UtcNow.ToLocalTime(ApplicationState.Current.EventTimeOffset);

			var offset = eventNow < this.ReportEndLocal ? ChartHelper.ToXPos(this.ReportStartLocal,
			                                                                 this.ReportEndLocal,
			                                                                 eventNow,
			                                                                 this.CurrentRenderLevel.DayImageWidth,
			                                                                 ReportManager.DayStartTime,
			                                                                 ReportManager.DayEndTime,
			                                                                 this.CurrentRenderLevel.DayMarkerWidth)
				- this.Bounds.Size.Width/2

				:this.ContentSize.Width - this.Bounds.Size.Width;
			if(offset > 0)
				this.SetContentOffset (new PointF (offset, 0), true);

			ZoomToDefault ();
		}

		public void ZoomToDefault ()
		{
			this.SetZoomScale (1, true);
		}

		public void ToggleEventMarkers ()
		{
			if (!this.eventMarkersVisible) {
				if (this.eventMarkers.Count () == 0) {
					BuildEventMarkers ();
					ScaleLabels ();
				} else {
					foreach (var e in this.eventMarkers) {
						e.Hidden = false;
						if (this.CurrentZoomLevel.ShrinkLabels) {
							e.Shrink ();
						} else {
							e.Grow ();
						}
					}
				}
				this.eventMarkersVisible = true;
			} else {
				foreach (var e in this.eventMarkers) {
					e.Hidden = true;
				}
				this.eventMarkersVisible = false;
			}
		}

		public void TogglePromptMarkers ()
		{
			if (!this.promptMarkersVisible) {
				if (this.promptMarkers.Count () == 0) {
					BuildPromptMarkers ();
					ScaleLabels ();
				} else {
					foreach (var e in this.promptMarkers) {
						e.Hidden = false;
						if (this.CurrentZoomLevel.ShrinkLabels) {
							e.Shrink ();
						} else {
							e.Grow ();
						}
					}
				}
				this.promptMarkersVisible = true;
			} else {
				foreach (var e in this.promptMarkers) {
					e.Hidden = true;
				}
				this.promptMarkersVisible = false;
			}
		}

		protected void Zoomed ()
		{

			ScaleLabels ();

			//Console.WriteLine("Zoomed - {0}", this.ZoomScale);
			var hpw = ChartHelper.CalculateHoursPerWindow (this.CurrentRenderLevel, this.ZoomScale, this.Bounds.Width);
			var zoomLevel = GetZoomLevel (hpw);
			if (zoomLevel != null && zoomLevel.ZoomIndex != CurrentZoomLevel.ZoomIndex) {
				this.CurrentZoomLevel = zoomLevel;

				if (this.eventMarkersVisible) {
					foreach (var e in this.eventMarkers) {
						if (zoomLevel.ShrinkLabels) {
							e.Shrink ();
						} else {
							e.Grow ();
						}
					}
				}

				if (this.promptMarkersVisible) {
					foreach (var e in this.promptMarkers) {
						if (zoomLevel.ShrinkLabels) {
							e.Shrink ();
						} else {
							e.Grow ();
						}
					}
				}

				if (ZoomLevelChanged != null)
					ZoomLevelChanged (this, new EventArgs ());
			}
		}

		protected void SetZoomLevel (ZoomLevel zoomLevel)
		{
			this.CurrentZoomLevel = zoomLevel;
			this.MinimumZoomScale = ZoomLevel.MinimumZoomLevel.MaxHoursPerWindow == null 
				? ChartHelper.CalculateScaleOfChart (zoomLevel.RenderLevel, (float)ReportManager.GetReportHours (this.MoodReport), this.Bounds.Width)
					: ChartHelper.CalculateScaleOfChart (zoomLevel.RenderLevel, ZoomLevel.MinimumZoomLevel.MaxHoursPerWindow.Value, this.Bounds.Width);
			this.MaximumZoomScale = ZoomLevel.MaximumZoomLevel.MinHoursPerWindow == null
				? 2f
					: ChartHelper.CalculateScaleOfChart (zoomLevel.RenderLevel, ZoomLevel.MaximumZoomLevel.MinHoursPerWindow.Value, this.Bounds.Width);
		}

		protected void LoadImages ()
		{
			if (this.globalMoodMap != null) {
				this.globalMoodMap.RemoveFromSuperview ();
				this.globalMoodMap = null;
			}
			
			globalMoodMap = new GlobalMoodMapContainerView (this.OriginalFrame);
			globalMoodMap.BackgroundColor = UIColor.Black;

			var images = ReportManager.GetReportImages (this.MoodReport, this.CurrentRenderLevel);

			var xPos = 0f;

			foreach (var img in images) {
				xPos += this.CurrentZoomLevel.RenderLevel.DayMarkerWidth;

				var imageFrame = new RectangleF (xPos, 0f, this.CurrentRenderLevel.DayImageWidth, this.CurrentRenderLevel.DayImageHeight);
				UIImageView imgView = new UIImageView (imageFrame);
				imgView.Image = img;
				imgView.BackgroundColor = UIColor.LightGray;
				this.globalMoodMap.Add (imgView);
				xPos += imageFrame.Width;
			}
			
			this.globalMoodMap.Frame = new RectangleF (this.globalMoodMap.Frame.Location,
			                                           new SizeF (xPos,
			           this.globalMoodMap.Frame.Height));
			this.ContentSize = globalMoodMap.Frame.Size;

			this.Add (this.globalMoodMap);

			//add mask for future data
			if (this.futureMaskView != null) {
				this.futureMaskView.RemoveFromSuperview ();
				this.futureMaskView = null;
			}

			var eventNow = DateTime.UtcNow.ToLocalTime(ApplicationState.Current.EventTimeOffset);
			if (eventNow < this.MoodReport.EndsOnLocal) {
				var nowX = ChartHelper.ToXPos (this.MoodReport.StartsOnLocal, 
				                               this.MoodReport.EndsOnLocal, 
				                               eventNow,
				                               this.CurrentRenderLevel.DayImageWidth,
				                               ReportManager.DayStartTime, 
				                               ReportManager.DayEndTime,
				                               this.CurrentRenderLevel.DayMarkerWidth);
				this.futureMaskView = new UIView (new RectangleF (nowX, 0, this.globalMoodMap.Bounds.Width - nowX, this.globalMoodMap.Bounds.Height));
				this.futureMaskView.BackgroundColor = new UIColor (0, 0, 0, 0.5f);
				this.globalMoodMap.Add (this.futureMaskView);
			}
		}



		public ZoomLevel GetZoomLevel (float hoursInWindow)
		{
			var zoomLevel = ZoomLevel.ZoomLevels.Where (z => (z.MaxHoursPerWindow == null || hoursInWindow <= z.MaxHoursPerWindow)
				&& (z.MinHoursPerWindow == null || hoursInWindow > z.MinHoursPerWindow)).FirstOrDefault ();
			if (zoomLevel == null)
				return ZoomLevel.MaximumZoomLevel;
			return zoomLevel;
		}

		protected void ClearEventMarkers ()
		{
			foreach (var e in this.eventMarkers) {
				e.RemoveFromSuperview ();
			}

			this.eventMarkersVisible = false;
		}

		protected void ClearPromptMarkers ()
		{
			foreach (var e in this.promptMarkers) {
				e.RemoveFromSuperview ();
			}
			
			this.promptMarkersVisible = false;
		}

		protected void BuildEventMarkers ()
		{
			foreach (var e in this.eventMarkers) {
				e.RemoveFromSuperview ();
			}

			this.eventMarkers = new List<EventMarkerView> ();
			var events = Activity.List ("ActivityType=@ActivityType and TimeStamp >= @ReportStart and TimeStamp <= @ReportEnd", 
			                            new{ 
												ActivityType = ActivityType.Event,
												ReportStart = this.MoodReport.StartsOn,
												ReportEnd = this.MoodReport.EndsOn
										}).ToList ();
			foreach (var e in events) {
				var eventView = BuildEventLabel (e);
				if (this.CurrentZoomLevel.ShrinkLabels)
					eventView.Shrink ();
				eventMarkers.Add (eventView);
				eventView.Center = new PointF (ChartHelper.ToXPos (this.ReportStartLocal, 
				                                                 this.ReportEndLocal,
				                                                 e.TimeStampLocal,
				                                                 this.CurrentZoomLevel.RenderLevel.DayImageWidth,
				                                                 ReportManager.DayStartTime,
				                                                 ReportManager.DayEndTime,
				                                                 this.CurrentZoomLevel.RenderLevel.DayMarkerWidth),
				                              this.Bounds.Height - 53f);
				this.globalMoodMap.Add (eventView);
			}

		}
		
		protected void BuildPromptMarkers ()
		{
			foreach (var e in this.promptMarkers) {
				e.RemoveFromSuperview ();
			}

			this.promptMarkers = new List<PromptMarkerView> ();
			var prompts = Activity.List ("ActivityType=@ActivityType and TimeStamp >= @ReportStart and TimeStamp <= @ReportEnd", 
			                             new{ 
				ActivityType = ActivityType.MoodPrompt,
				ReportStart = this.MoodReport.StartsOn,
				ReportEnd = this.MoodReport.EndsOn
			}).ToList ();

			foreach (var p in prompts) {
				var promptView = BuildPromptLabel (p);
				if (this.CurrentZoomLevel.ShrinkLabels)
					promptView.Shrink ();
				this.promptMarkers.Add (promptView);
				promptView.Center = new PointF (ChartHelper.ToXPos (this.ReportStartLocal, 
				                                                  this.ReportEndLocal,
				                                                  p.TimeStampLocal,
				                                                  this.CurrentZoomLevel.RenderLevel.DayImageWidth,
				                                                  ReportManager.DayStartTime,
				                                                  ReportManager.DayEndTime,
				                                                  this.CurrentZoomLevel.RenderLevel.DayMarkerWidth),
				                               50f);
				this.globalMoodMap.Add (promptView);
			}
			

		}
		
		protected PromptMarkerView BuildPromptLabel (Activity prompt)
		{
			return new PromptMarkerView (prompt);
		}
		
		protected EventMarkerView BuildEventLabel (Activity evnt)
		{
			return new EventMarkerView (evnt);
		}

		protected void ScaleLabels ()
		{
			foreach (var e in this.eventMarkers) {
				e.Transform = CGAffineTransform.MakeScale (1 / this.ZoomScale, 1);
			}

			foreach (var e in this.promptMarkers) {
				e.Transform = CGAffineTransform.MakeScale (1 / this.ZoomScale, 1);
			}
		}
	}
}

