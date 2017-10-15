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
	public class OutstandingPromptsView : UIView
	{
		public event EventHandler<JumpToPromptEventArgs> JumpToPrompt;
		public bool Showing {
			get;
			set;
		}

		public MoodPrompt TargetPrompt {
			get;
			set;
		}

		public int UnrespondedPrompts {
			get;
			set;
		}


		UIView textContainer;

		public OutstandingPromptsView (RectangleF frame)
			:base(frame)
		{
			this.AutoresizingMask = (UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleBottomMargin | UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth);

			this.UnrespondedPrompts = 0;

			UIImageView backgroundImage = new UIImageView(this.Bounds);
			backgroundImage.Image = Resources.NotificationPanel;
			this.Add (backgroundImage);



			Refresh();

		}

		public void Hide(){

			this.AnimateOut(false);
		}

		public void Refresh ()
		{
			var prompts = MoodPrompt.GetOutstandingPrompts().OrderBy (p => p.TimeStamp).ToList();
			this.UnrespondedPrompts = prompts.Count ();
			this.TargetPrompt = prompts.LastOrDefault ();

			this.BuildNotification();

			//if (lastCount != this.UnrespondedPrompts) this.AnimateOut(true);
			this.AnimateOut(true);
		}

		protected void DoJumpToPrompt()
		{
			if(JumpToPrompt != null) JumpToPrompt(this, new JumpToPromptEventArgs(this.TargetPrompt));
		}

		private void BuildNotification()
		{



			if(this.textContainer != null) this.textContainer.RemoveFromSuperview();

			this.textContainer = new UIView(this.Bounds);
			
			UILabel leftTextLabel = new UILabel();
			leftTextLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",16.0f);
			leftTextLabel.TextColor = UIColor.White;
			leftTextLabel.Text = "You have";
			leftTextLabel.TextAlignment = UITextAlignment.Center;
			leftTextLabel.SizeToFit();
			leftTextLabel.BackgroundColor = UIColor.Clear;
			
			UILabel rightTextLabel = new UILabel();
			rightTextLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",16.0f);
			rightTextLabel.TextColor = UIColor.White;
			if (this.UnrespondedPrompts > 1)
			{
				rightTextLabel.Text = "outstanding mood prompts";
			}
			else
			{
				rightTextLabel.Text = "outstanding mood prompt";
			}
			rightTextLabel.TextAlignment = UITextAlignment.Center;
			rightTextLabel.SizeToFit();
			rightTextLabel.BackgroundColor = UIColor.Clear;
			
			UILabel numLabel = new UILabel();
			numLabel.Font = UIFont.FromName("HelveticaNeue-CondensedBold",18.0f);
			numLabel.TextColor = UIColor.White;
			numLabel.Text = this.UnrespondedPrompts.ToString();
			numLabel.TextAlignment = UITextAlignment.Center;
			numLabel.SizeToFit();
			numLabel.BackgroundColor = UIColor.Clear;
			
			
			UIImageView leftIv = new UIImageView(leftTextLabel.Frame);
			UIImageView rightIv = new UIImageView(rightTextLabel.Frame);
			
			leftIv.AddSubview(leftTextLabel);
			rightIv.AddSubview(rightTextLabel);
			
			UIImage badgeImage2;
			if(this.UnrespondedPrompts >9)
			{
				badgeImage2 = Resources.DoubleBadge;
			}
			else
			{
				badgeImage2 = Resources.SingleBadge;
			}
			
			UIImageView bi2 = new UIImageView(badgeImage2);
			numLabel.Center = new PointF((bi2.Frame.Size.Width/2),(bi2.Frame.Size.Height/2)-6); 
			bi2.AddSubview(numLabel);
			
			RectangleF allTogetherRectangle = new RectangleF(0,0,leftIv.Bounds.Width + bi2.Bounds.Width+rightIv.Bounds.Width,bi2.Bounds.Height);
			
			UIView togetherImageView = new UIView(allTogetherRectangle);
			RectangleF midFrame = new RectangleF(leftIv.Bounds.Width,0,bi2.Bounds.Width,bi2.Bounds.Height);
			RectangleF rightFrame = new RectangleF(leftIv.Bounds.Width+bi2.Bounds.Width,0,rightIv.Bounds.Width,rightIv.Bounds.Height);
			bi2.Frame = midFrame;
			rightIv.Frame = rightFrame;
			togetherImageView.AddSubview(leftIv);
			togetherImageView.AddSubview(bi2);
			togetherImageView.AddSubview(rightIv);
			togetherImageView.UserInteractionEnabled=false;
			RectangleF centerButtonFrame = new RectangleF((this.Frame.Width/2) - (togetherImageView.Frame.Width /2) ,(this.Frame.Height/2)-(togetherImageView.Frame.Height /2)+5,togetherImageView.Bounds.Width,togetherImageView.Bounds.Height);
			
			togetherImageView.Frame = centerButtonFrame;
			this.textContainer.Add(togetherImageView);

			var jumpToPromptBtn = new UIButton(this.Bounds);
			
			jumpToPromptBtn.TouchUpInside += (object sender, EventArgs e) => {
				DoJumpToPrompt();
			};
			
			this.textContainer.Add(jumpToPromptBtn);

			this.Add(this.textContainer);


			
		}

		protected void AnimateIn(){

				//animate in
			var pt = new PointF (this.Center.X, 742.0f);
				UIView.Animate (1, 0.5, UIViewAnimationOptions.CurveEaseIn, () => {
					this.Center = pt;},
				() => {
				this.Showing = true;
				}
				);

		}

		protected void AnimateOut(bool animateBackInIfPrompts){
			var pt = new PointF(this.Center.X,  800.0f);
			UIView.Animate(1,0.5,UIViewAnimationOptions.CurveEaseOut,()=>{
				this.Center = pt;},
			() =>{
				this.Showing = false;
				if(animateBackInIfPrompts && this.UnrespondedPrompts > 0) this.AnimateIn();
			}
			);		
		}
	}
}

