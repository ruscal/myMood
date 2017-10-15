using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

namespace MyMood
{
	public class SelectMoodHelpOverlayView : UIView
	{
		public event EventHandler Closed;

		UIView page1;
		UIView page2;

		public SelectMoodHelpOverlayView (RectangleF frame)
			:base(frame)
		{
			BuildPage1();
			BuildPage2();

			ShowPage1();
		}

		protected void BuildPage1 ()
		{
			page1 = new UIView(this.Bounds);

			UIImageView backgroundImage = new UIImageView(this.Bounds);
			backgroundImage.Image = Resources.SelectMoodHelpBackground1;
			page1.Add(backgroundImage);

			UIImageView setMoodImage = new UIImageView(new RectangleF(this.Frame.Size.Width/2-(114) ,650,228,71));
			setMoodImage.Image = Resources.SetMoodButton;
			page1.Add(setMoodImage);

			UIButton closeBtn = new UIButton(this.Bounds);
			closeBtn.TouchUpInside += (object sender, EventArgs e) => {
				ShowPage2();
			};
			page1.Add (closeBtn);

			page1.Alpha = 0;
			this.Add (page1);
		}

		protected void BuildPage2 ()
		{
			page2 = new UIView(this.Bounds);

			UIImageView backgroundImage = new UIImageView(this.Bounds);
			backgroundImage.Image = Resources.SelectMoodHelpBackground2;
			page2.Add(backgroundImage);

			UIImageView closeImage = new UIImageView(new RectangleF(924,25,78,32));
			closeImage.Image = Resources.CloseIntroButton;
			page2.Add(closeImage);

			UIButton closeBtn = new UIButton(this.Bounds);
			closeBtn.TouchUpInside += (object sender, EventArgs e) => {
				Close();
			};
			page2.Add (closeBtn);

			page2.Alpha = 0;
			this.Add(page2);
		}


		protected void ShowPage1 ()
		{
			UIView.Animate(0.5,()=>{page1.Alpha = 1;});	
		}

		protected void ShowPage2 ()
		{
			page1.Alpha = 0;
			page2.Alpha = 1;
		}

		protected void Close ()
		{
			UIView.Animate(0.5,0,UIViewAnimationOptions.TransitionNone,()=>{
				page2.Alpha=0;},
			() =>{
				if(this.Closed != null) Closed(this, new EventArgs());
				this.RemoveFromSuperview();
			}
			);
		}



	}
}

