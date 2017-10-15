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


namespace MyMood
{
	public class AddResponseColumnView : TimelineColumnView
	{
		public event EventHandler<AddNewResponseEventArgs> AddNewMood;

		public MoodPrompt CurrentPrompt {
			get;
			set;
		}

		protected UIButton addResponseBtn;

		public AddResponseColumnView (int columnIndex, MoodPrompt currentPrompt)
			:base(columnIndex)
		{
			this.CurrentPrompt = currentPrompt;

			this.backgroundImage.Image = Resources.AddNode;

			this.addResponseBtn = new UIButton(new RectangleF(20,228,100,100));
			this.addResponseBtn.TouchUpInside += (object sender, EventArgs e) => {
				AddMood();
			};
			this.addResponseBtn.Enabled = true;
			this.Add(addResponseBtn);
		}

		protected void AddMood(){
			if(AddNewMood != null) AddNewMood(this, new AddNewResponseEventArgs(this.CurrentPrompt));
		}


	}
}

