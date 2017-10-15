using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.DL;
using MyMood.AL;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using MonoTouch.ObjCRuntime;
using Discover.Core;

namespace MyMood
{
	public class MoodResponseColumnView : TimelineColumnView
	{
		public MoodResponse Response {
			get;
			set;
		}

		protected UILabel timeLabel;
		protected UILabel titleLabel;

		public MoodResponseColumnView (int columnIndex, MoodResponse response)
			:base(columnIndex)
		{
			this.Response = response;

			this.backgroundImage.Image = Resources.GetMoodResponseTimelineImage (response.Mood);

			if (this.Response.Mood.MoodType == MoodType.Positive) {
				this.timeLabel = new UILabel (new RectangleF (20, 357, 100, 21));
				this.titleLabel = new UILabel (new RectangleF (17, 380, 106, 50));
			} else {
				this.titleLabel = new UILabel (new RectangleF (18, 128, 106, 50));
				this.timeLabel = new UILabel (new RectangleF (20, 180, 100, 22));
			}
			this.timeLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",16.0f);
			this.timeLabel.TextColor = UIColor.White;
			this.timeLabel.BackgroundColor = UIColor.Clear;
			this.timeLabel.Text = response.TimeStamp.ToLocalTime(ApplicationState.Current.EventTimeOffset).ToString("H:mm");
			this.timeLabel.TextAlignment = UITextAlignment.Center;
			this.timeLabel.BaselineAdjustment = UIBaselineAdjustment.AlignBaselines;
			this.Add(this.timeLabel);

			this.titleLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",16.0f);
			this.titleLabel.TextColor = UIColor.White;
			this.titleLabel.BackgroundColor = UIColor.Clear;
			this.titleLabel.Text = response.Prompt == null ? "My Mood" : response.Prompt.Title;
			this.titleLabel.TextAlignment = UITextAlignment.Center;
			this.titleLabel.BaselineAdjustment = UIBaselineAdjustment.AlignBaselines;
			this.Add(this.titleLabel);


		}
	}
}

