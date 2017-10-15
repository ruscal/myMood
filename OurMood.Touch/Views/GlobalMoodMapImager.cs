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

namespace OurMood.Touch
{
	public class GlobalMoodMapImager
	{
		public float Tension { get; set; }
		public bool SmoothLines { get; set; }
		public bool ShowDataPoints { get; set; }
		public DateTime StartDate {
			get;
			set;
		}
		public DateTime EndDate {
			get;
			set;
		}



		int _dataPointWidth = 10;
		List<MoodDataSet> _moods = new List<MoodDataSet> ();
		bool endsPrematurely = false;
		int minDataPointsRequiredForSpline = 4;

		public GlobalMoodMapImager(DateTime startDate, DateTime endDate){
			this.Tension = 0.2f;
			this.ShowDataPoints = false;
			this.SmoothLines = true;
			this.StartDate = startDate;
			this.EndDate = endDate;
		}

		public void AddSnapshot (DateTime timeStamp, IEnumerable<MoodMapItem> moods, bool endOnThis)
		{
			decimal cumulativePercentage = 0;
			
			foreach (var mood in moods) {
				var responseCount = mood.ResponseCount;
				decimal responsePercentage = mood.ResponsePercentage;
				var moodData = _moods.FirstOrDefault (m => m.Mood.Name.Equals (mood.Name, StringComparison.InvariantCultureIgnoreCase));
				if (moodData == null) {
					moodData = new MoodDataSet ()
					{
						Mood = new MoodItem()
						{
							DisplayColor = mood.DisplayColor,
							DisplayIndex = mood.DisplayIndex,
							MoodType = mood.MoodType,
							Name = mood.Name
						},
						DataPoints = new List<DataPoint>()
					};
					_moods.Add (moodData);
				}
				moodData.DataPoints.Add (new DataPoint () { TimeStamp = timeStamp, ResponseCount = responseCount, ResponsePercentage = responsePercentage, CumulativePercentage = cumulativePercentage });
				if(endOnThis){
//					endsPrematurely = true;
//					moodData.DataPoints.Add (new DataPoint () { TimeStamp = timeStamp, ResponseCount = 0, ResponsePercentage = 0, CumulativePercentage = 100 });
				}

				cumulativePercentage += responsePercentage;
			}
		}
		
		public UIImage DrawMapImage (RectangleF frame)
		{	
			
			UIGraphics.BeginImageContext (frame.Size);
			var g = UIGraphics.GetCurrentContext ();


			var positiveMoods = (from m in _moods where m.Mood.MoodType == MoodType.Positive select m).OrderByDescending(m => m.Mood.DisplayIndex);
			var neutralMoods = (from m in _moods where m.Mood.MoodType == MoodType.Neutral select m).OrderByDescending(m => m.Mood.DisplayIndex);
			var negativeMoods = (from m in _moods where m.Mood.MoodType == MoodType.Negative select m).OrderByDescending(m => m.Mood.DisplayIndex);
			
			using (g) {

				//AddMoodLayer (g, frame, positiveMoods.ToArray()[1], ShowDataPoints);

				foreach (MoodDataSet mood in negativeMoods) {
					AddMoodLayer (g, frame, mood, ShowDataPoints);
				}

				foreach (MoodDataSet mood in neutralMoods) {
					AddMoodLayer (g, frame, mood, ShowDataPoints);
				}

				foreach (MoodDataSet mood in positiveMoods) {
					AddMoodLayer (g, frame, mood, ShowDataPoints);
				}
				

				

				
				if (neutralMoods.Count () > 0) {
					AddSeperator (g, frame, neutralMoods.First ());
				}
				
				if (positiveMoods.Count () > 0) {
					AddSeperator (g, frame, positiveMoods.First ());
				}
			}
			
			var mapImage = UIGraphics.GetImageFromCurrentImageContext();
	
			UIGraphics.EndImageContext();
			return mapImage;  
		}
		
		private void AddSeperator (CGContext g, RectangleF frame, MoodDataSet mood)
		{

			var color = new CGColor(255, 255, 255);
			var dataPoints = GetPointsForSpline(mood.DataPoints, frame);

			var first = dataPoints.First ();
			CGLayer layer = CGLayer.Create (g, new SizeF (frame.Width, frame.Height));
			layer.Context.SetStrokeColor (color);
			layer.Context.SetLineWidth(5f);
			layer.Context.MoveTo(first.X, first.Y);

			if(endsPrematurely) dataPoints.RemoveAt(dataPoints.Count()-1);
			                                     

			if (this.SmoothLines && dataPoints.Count() >= minDataPointsRequiredForSpline) {
				layer.Context.AddSpline (dataPoints.ToArray (), this.Tension);
				var last = dataPoints.Last();
				layer.Context.AddLineToPoint(last.X, last.Y);
			} else {
				layer.Context.AddLines (dataPoints.ToArray ());
			}

			layer.Context.DrawPath(CGPathDrawingMode.Stroke);
			g.DrawLayer(layer, new PointF(0, 0));

		}

		private void AddMoodLayer (CGContext g, RectangleF frame, MoodDataSet mood, bool showDataPoints)
		{
			PointF bottomRight = new PointF (frame.Width, frame.Height + 100);
			PointF bottomLeft = new PointF (0, frame.Height + 100);
			var color = mood.Mood.DisplayColor;
			var dataPoints = GetPointsForSpline(mood.DataPoints, frame);
			var last = dataPoints [dataPoints.Count - 1];
			var first = dataPoints.First ();
			CGLayer layer = CGLayer.Create (g, new SizeF (frame.Width, frame.Height));
			layer.Context.SetFillColor (color);
			layer.Context.SetStrokeColor (new CGColor (0, 0, 0));

			layer.Context.MoveTo (bottomLeft.X, bottomLeft.Y);
			layer.Context.AddLineToPoint (first.X, first.Y);


			if (this.SmoothLines && dataPoints.Count() >= minDataPointsRequiredForSpline) {
				layer.Context.AddSpline (dataPoints.ToArray (), this.Tension);
			} else {
				layer.Context.AddLines (dataPoints.ToArray ());
			}

			layer.Context.AddLineToPoint(last.X, bottomRight.Y);
			layer.Context.AddLineToPoint(bottomLeft.X, bottomLeft.Y);

			
			layer.Context.DrawPath(CGPathDrawingMode.EOFill);
			g.DrawLayer(layer, new PointF(0, 0));
			
			if (ShowDataPoints) {
				var dpW = _dataPointWidth / 2;
				g.SetFillColor(new CGColor(0, 0, 0));
				foreach (var dp in dataPoints) {
					g.FillEllipseInRect(new RectangleF(dp.X - dpW, dp.Y - dpW, _dataPointWidth, _dataPointWidth));
				}
			}
			
		}

		private List<PointF> GetPointsForSpline (IEnumerable<DataPoint> dataPoints, RectangleF frame)
		{
			List<PointF> points = new List<PointF> ();
			foreach (var dp in dataPoints) {
				points.Add(DataPointToPoint(frame, dp));
			}

			if (points.Count () == 1) return points;
			
			if(points.Count() < minDataPointsRequiredForSpline) {
				var missing = minDataPointsRequiredForSpline - dataPoints.Count();
				var xx = (points[1].X - points[0].X) / missing;
				var yy = (points[1].Y - points[0].Y) / missing;
				var x = points[0].X + xx;
				var y = points[0].Y + yy;
				while(points.Count() < minDataPointsRequiredForSpline){
					points.Insert(1, new PointF(x, y));
					x += xx;
					y += yy;
				}
			}

			return points;
		}
		
		public class MoodMapItem : MoodItem
		{
			public int ResponseCount { get; set; }
			
			public decimal ResponsePercentage { get; set; }
		}
		
		public class MoodItem
		{
			public string Name { get; set; }
			
			public int DisplayIndex { get; set; }
			
			public CGColor DisplayColor { get; set; }
			
			public MoodType MoodType { get; set; }
		}
		
		private PointF DataPointToPoint (RectangleF frame, DataPoint dp)
		{
			//var x = (decimal)dp.TimeStamp.Subtract (this.StartDate).TotalMilliseconds / (decimal)this.EndDate.Subtract (this.StartDate).TotalMilliseconds * (decimal)frame.Width;
			var x = ChartHelper.ToXPos(this.StartDate, this.EndDate, dp.TimeStamp, frame.Width);
			var y = (decimal)dp.CumulativePercentage / 100M * (decimal)frame.Height;
			return new PointF ((float)x, (float)y);
		}
		
		private class MoodDataSet
		{
			public MoodItem Mood { get; set; }
			
			public List<DataPoint> DataPoints { get; set; }
		}
		
		private class DataPoint
		{
			public DateTime TimeStamp { get; set; }
			
			public int ResponseCount { get; set; }
			
			public decimal ResponsePercentage { get; set; }
			
			public decimal CumulativePercentage { get; set; }
			
		}
	}
}

