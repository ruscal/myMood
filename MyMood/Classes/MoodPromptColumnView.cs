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
	public class MoodPromptColumnView : TimelineColumnView
	{
		public event EventHandler<AddNewResponseEventArgs> AddNewMood;

		public MoodPrompt Prompt {
			get;
			set;
		}

		protected UIButton addResponseBtn;

		public MoodPromptColumnView (int columnIndex, MoodPrompt prompt)
			: base(columnIndex)
		{
			this.Prompt = prompt;
		
			this.backgroundImage.Image = Resources.PromptedNode;

			this.addResponseBtn = new UIButton(new RectangleF(32,240,74,74));
			this.addResponseBtn.TouchUpInside += (object sender, EventArgs e) => {
				AddMood();
			};
			this.addResponseBtn.Enabled = true;
			this.Add(addResponseBtn);

			this.ShowTitle();
		}

		protected void AddMood(){
			if(AddNewMood != null) AddNewMood(this, new AddNewResponseEventArgs(this.Prompt));
		}

		protected void ShowTitle(){


			var timeLabel = new UILabel (new RectangleF (20, 357, 100, 21));
			timeLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",18.0f);
			timeLabel.TextColor = UIColor.White;
			timeLabel.BackgroundColor = UIColor.Clear;
			timeLabel.Text = Prompt.TimeStamp.ToLocalTime(ApplicationState.Current.EventTimeOffset).ToString("H:mmm");
			timeLabel.TextAlignment = UITextAlignment.Center;
			timeLabel.BaselineAdjustment = UIBaselineAdjustment.AlignBaselines;
			this.Add(timeLabel);

			var titleLabel = new UILabel (new RectangleF (17, 380, 106, 50));
			titleLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",16.0f);
			titleLabel.TextColor = UIColor.White;
			titleLabel.BackgroundColor = UIColor.Clear;
			titleLabel.Text = Prompt.Title;
			titleLabel.TextAlignment = UITextAlignment.Center;
			titleLabel.BaselineAdjustment = UIBaselineAdjustment.AlignBaselines;
			titleLabel.Lines = 2;
			this.Add(titleLabel);
		}
	}
}

