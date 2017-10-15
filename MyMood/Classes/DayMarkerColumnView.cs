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

namespace MyMood
{
	public class DayMarkerColumnView : TimelineColumnView
	{
		protected UILabel dayLabel;
		protected UILabel monthLabel;

		public string LabelText {
			get;
			set;
		}


		public int DayIndex {
			get;
			set;
		}

		public DateTime Date {
			get;
			set;
		}

		public DayMarkerColumnView (int columnIndex, DateTime date, int dayIndex)
			:base(columnIndex)
		{
			this.Date = date;
			this.DayIndex = dayIndex;
			this.LabelText = string.Format("{0:ddd dd}", Date);
			this.dayLabel = new UILabel (new RectangleF (20, 250, 100, 30));
			this.dayLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",30.0f);
			this.dayLabel.TextColor = UIColor.White;
			this.dayLabel.BackgroundColor = UIColor.Clear;
			this.dayLabel.Text = LabelText;
			this.dayLabel.TextAlignment = UITextAlignment.Center;
			this.dayLabel.BaselineAdjustment = UIBaselineAdjustment.AlignBaselines;
			this.Add(this.dayLabel);

			string monthText = string.Format("{0:MMM}", Date);
			this.monthLabel = new UILabel (new RectangleF (20, 280, 100, 30));
			this.monthLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",30.0f);
			this.monthLabel.TextColor = UIColor.White;
			this.monthLabel.BackgroundColor = UIColor.Clear;
			this.monthLabel.Text = monthText;
			this.monthLabel.TextAlignment = UITextAlignment.Center;
			this.monthLabel.BaselineAdjustment = UIBaselineAdjustment.AlignBaselines;
			this.Add(this.monthLabel);


			if (this.DayIndex == 1) {
				this.backgroundImage.Image = Resources.Day1Node;
			} else {
				this.backgroundImage.Image = Resources.Day2Node;
			}
		}
	}
}

