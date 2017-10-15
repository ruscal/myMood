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
	public class SnapshotChartView : UIView
	{
		UIView chart;
		MoodReport report;
		Snapshot currentSnapshot;
		UILabel title;
		DateTime? lastSnapshotTime;
		List<SnapshotBarView> bars;
		UIImageView background;
		DateTime lastRefresh;
		float labelH = 20f;
		float labelW = 120f;
		float margin = 5f;
		double minRefreshTimeSecs = 10;
		int eventTimeOffset = 1;
		UILabel totalLabel;

		public SnapshotChartView (MoodReport report, RectangleF frame)
			:base(frame)
		{
			Console.WriteLine("Init chart view");
			this.report = report;
			this.BackgroundColor = UIColor.Clear;
			this.ContentMode = UIViewContentMode.Center;

			this.background = new UIImageView (this.Bounds);
			this.background.Image = Resources.SnapshotPanelRight;
			this.Add (this.background);
			this.Hidden = true;
			this.lastRefresh = DateTime.Now.AddDays (-1);

			this.eventTimeOffset = ApplicationState.Current.EventTimeOffset;
		}

		public void ShowBackgroundLeft ()
		{
			this.background.Image = Resources.SnapshotPanelLeft;
		}

		public void ShowBackgroundRight ()
		{
			this.background.Image = Resources.SnapshotPanelRight;
		}

		public void ShowSnapshotPosition (DateTime? snapshotTime)
		{
			Console.WriteLine("ShowSnapPosition in chart view");
			if (DateTime.Now.Subtract (lastRefresh).TotalSeconds > minRefreshTimeSecs) {
				ShowSnapshot (snapshotTime);
			} else {
				ShowPosition (snapshotTime);
			}
		}

		protected void ShowPosition (DateTime? snapshotTime)
		{
			Console.WriteLine("ShowPosition in chart view");
			if (snapshotTime != null) {
				this.Hidden = false;
				if (this.title == null)
					BuildTitle (snapshotTime);
				
				title.Text = string.Format ("Snapshot - {0}", snapshotTime.Value.ToString ("ddd dd H:mm"));
				if (this.chart != null && this.IsCurrentSnapshotValid (snapshotTime)) {
					this.chart.Hidden = false;
				} else {
					if (this.chart != null)
						this.chart.Hidden = true;
				}
				
			} else {
				this.Hidden = true;
			}
		}
		                     
		protected bool IsCurrentSnapshotValid (DateTime? snapshotTime)
		{
			Console.WriteLine("IsCurrentSnapshotValid in chart view");
			if (snapshotTime == null)
				return false;
			return (this.currentSnapshot != null 
				&& lastSnapshotTime != null
				&& this.currentSnapshot.TimeOfSnapshotLocal < snapshotTime.Value
				&& snapshotTime.Value < this.lastSnapshotTime.Value);
		}

		public void ShowSnapshot (DateTime? snapshotTime)
		{
			Console.WriteLine("Show snapshot in chart view");
			if (snapshotTime != null) {
				this.ShowPosition (snapshotTime);
				Console.WriteLine("showed position");
				var eventNow = MoodResponse.GetRoundedResponseTime(DateTime.UtcNow.ToLocalTime(this.eventTimeOffset));
				Console.WriteLine("got event now");
				var roundedSnapshotTime = MoodResponse.GetRoundedResponseTime(snapshotTime.Value);
				Console.WriteLine("got roundedSnapshotTime");
				if (snapshotTime.Value <= eventNow) {
					var sameSnapshot = IsCurrentSnapshotValid (roundedSnapshotTime);
					Console.WriteLine("got iscurrentsnapshot");
					var snapshot = sameSnapshot ? this.currentSnapshot : ReportManager.GetClosestSnapshot (report, roundedSnapshotTime);
					Console.WriteLine("got closest snapshot : {0}", sameSnapshot);
					this.lastRefresh = DateTime.Now;
					if (snapshot != null) {
						Console.WriteLine("got snapshot");
						if (this.currentSnapshot == null || snapshot.Id != this.currentSnapshot.Id) {
							Console.WriteLine("new snapshot");
							this.currentSnapshot = snapshot;
							if (this.chart == null) {
								Console.WriteLine("lets build chart");
								this.BuildChart (snapshotTime);
							} else {
								Console.WriteLine("lets refresh chart");
								this.RefreshChart (snapshotTime);
							}
							this.lastSnapshotTime = roundedSnapshotTime;
						} 
						this.chart.Hidden = false;
						this.Hidden = false;

						return;
					} 
					this.currentSnapshot = null;
					this.lastSnapshotTime = null;

				}else{
					return;
				}

			}
			this.Hidden = true;

		}

		protected void RefreshChart (DateTime? snapshotTime)
		{
			Console.WriteLine("Refresh chart in chart view");
			var moods = ReportManager.GetMoodBreakdownForSnapshot (this.currentSnapshot).ToList ();
			SnapshotBarView last = null;
			var w = labelW;
			for (int i=0; i<moods.Count(); i++) {
				var bar = this.bars [i];
				var m = moods [i];
				bar.Update (m.Name, (float)m.ResponsePercentage, m.ResponseCount);
				last = bar;
				if (bar.Frame.Width > w)
					w = bar.Frame.Width;
			}
			this.totalLabel.Text = string.Format("Total responders = {0}", moods.Sum(m => m.ResponseCount));
			this.Frame = new RectangleF (this.Frame.Location, new SizeF (this.Frame.Width, totalLabel.Frame.Y + totalLabel.Frame.Height + margin * 2));
			this.background.Frame = new RectangleF (this.background.Frame.Location, this.Frame.Size);
//			this.title.Frame = new RectangleF(this.title.Frame.Location, new SizeF(this.Frame.Width, this.title.Frame.Height));

			this.Center = new PointF (this.Center.X, this.Superview.Bounds.Height / 2);
			this.chart.Center = new PointF (this.Frame.Width / 2, this.chart.Center.Y);
			this.chart.Hidden = false;
			this.Hidden = false;
		}

		protected void BuildTitle (DateTime? snapshotTime)
		{
			title = new UILabel (new RectangleF (0, 0, this.Bounds.Width, 30f));
			title.Text = string.Format ("Snapshot - {0}", snapshotTime.Value.ToString ("H:mm"));
			title.Font = UIFont.FromName ("HelveticaNeue-CondensedBold", 14.0f);
			title.TextColor = UIColor.DarkGray;
			title.TextAlignment = UITextAlignment.Center;
			title.BackgroundColor = UIColor.Clear;
			this.Add (title);
		}

		protected void BuildChart (DateTime? snapshotTime)
		{
			Console.WriteLine("Build chart in chart view");
			if (chart != null) {
				chart.RemoveFromSuperview ();
				chart = null;
			}
			chart = new UIView (this.Bounds);
			chart.ContentMode = UIViewContentMode.Left;
			var snapshot = this.currentSnapshot;

			if (title == null) {
				BuildTitle (snapshotTime);
			}

			var moods = ReportManager.GetMoodBreakdownForSnapshot (snapshot);
			var y = labelH + title.Frame.Y + margin;
			var w = labelW;
			bars = new List<SnapshotBarView> ();
			
			foreach (var m in moods) {
				var bar = new SnapshotBarView (new RectangleF (0f, y, labelW, labelH), m.Name, (float)m.ResponsePercentage, m.ResponseCount, m.DisplayColor); 
				chart.Add (bar);
				this.bars.Add (bar);
				if (bar.Frame.Width > w)
					w = bar.Frame.Width;
				y += labelH + margin;
			}

			this.totalLabel = new UILabel(new RectangleF (5f, y+10f, this.chart.Bounds.Width, labelH));
			this.totalLabel.Text = string.Format("Total responders = {0}", moods.Sum(m => m.ResponseCount));
			this.totalLabel.Font = UIFont.FromName ("HelveticaNeue-CondensedBold", 12.0f);
			this.totalLabel.TextColor = UIColor.Gray;
			this.totalLabel.TextAlignment = UITextAlignment.Center;
			this.totalLabel.BackgroundColor = UIColor.Clear;
			this.chart.Add(this.totalLabel);
			y += labelH + margin;

			this.Add (chart);
			
			this.Frame = new RectangleF (this.Frame.Location, new SizeF (this.Frame.Width, y + margin * 2));
			this.background.Frame = new RectangleF (this.background.Frame.Location, this.Frame.Size);
//			this.title.Frame = new RectangleF(this.title.Frame.Location, new SizeF(this.Frame.Width, this.title.Frame.Height));

			this.chart.Center = new PointF (this.Frame.Width / 2, this.chart.Center.Y);
			this.Center = new PointF (this.Center.X, this.Superview.Bounds.Height / 2);
			this.chart.Hidden = false;
			this.Hidden = false;
		}
	}

	public class SnapshotBarView : UIView
	{
		UILabel titleLabel;
		UIView bar;
		UILabel pcLabel;
		float barScale = 1.5f;

		public SnapshotBarView (RectangleF frame, string title, float percentage, float count, CGColor displayColor)
			:base(frame)
		{
			var labelH = 20f;
			var labelW = 120f;
			var margin = 5f;


			titleLabel = new UILabel (new RectangleF (margin, margin, labelW, labelH));
			titleLabel.Text = title.ToUpper ();
			titleLabel.Font = UIFont.FromName ("HelveticaNeue-CondensedBold", 12.0f);
			titleLabel.TextColor = UIColor.Gray;
			titleLabel.TextAlignment = UITextAlignment.Right;
			titleLabel.BackgroundColor = UIColor.Clear;
			this.Add (titleLabel);
			
			bar = new UIView (new RectangleF (titleLabel.Frame.X + titleLabel.Frame.Width + 5f, 
			                                        margin, 
			                                        (float)percentage * barScale, 
			                                        labelH));
			bar.BackgroundColor = new UIColor (displayColor);
			this.Add (bar);
			
			pcLabel = new UILabel (new RectangleF (bar.Frame.X + bar.Frame.Width + 5f, 
			                                               margin, 
			                                               50f, 
			                                               labelH));
			pcLabel.Text = string.Format ("{0}% ({1})", percentage, count);
			pcLabel.Font = UIFont.FromName ("HelveticaNeue-CondensedBold", 12.0f);
			pcLabel.TextColor = UIColor.Gray;
			pcLabel.TextAlignment = UITextAlignment.Left;
			pcLabel.BackgroundColor = UIColor.Clear;
			this.Add (pcLabel);
			var width = pcLabel.Frame.X + pcLabel.Frame.Width;

			this.Frame = new RectangleF (this.Frame.Location, new SizeF (width, this.Frame.Height));
		}

		public void Update (string title, float percentage, float count)
		{
			this.titleLabel.Text = title.ToUpper ();
			this.pcLabel.Text = string.Format ("{0}% ({1})", percentage, count);
			this.bar.Frame = new RectangleF (this.bar.Frame.X,
			                                this.bar.Frame.Y,
			                                (float)percentage * barScale,
			                                this.bar.Frame.Height);
			this.pcLabel.Frame = new RectangleF (bar.Frame.X + bar.Frame.Width + 5f,
			                                    this.pcLabel.Frame.Y,
			                                    this.pcLabel.Frame.Width,
			                                    this.pcLabel.Frame.Height);

			var width = pcLabel.Frame.X + pcLabel.Frame.Width;
			
			this.Frame = new RectangleF (this.Frame.Location, new SizeF (width, this.Frame.Height));
		}
	}
}

