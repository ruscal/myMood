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
	public class RefreshDataButtonView : UIImageView
	{
		public event EventHandler<EventArgs> Refresh;
		UIButton button;
		UILabel label;
		bool showing = false;
		PointF startPoint;

		public RefreshDataButtonView ()
			:base(Resources.RefreshButton)
		{
			this.startPoint = this.Center;
			this.UserInteractionEnabled = true;
			this.label = new UILabel(this.Bounds);
			this.label.TextAlignment = UITextAlignment.Center;
			this.label.BackgroundColor = UIColor.Clear;
			this.label.TextColor = UIColor.White;
			this.label.Text = "Refresh Chart Data";
			this.label.Font = UIFont.FromName("HelveticaNeue-CondensedBold",18.0f);
			this.Add(this.label);

			this.button = new UIButton(this.Bounds);
			this.button.TouchUpInside += (object sender, EventArgs e) => {
				this.AnimateOut(false);
				if(Refresh != null) Refresh(this, new EventArgs());
			};
			this.Add (this.button);


		}

		public void Show ()
		{
			if (this.showing) {
				this.AnimateOut (true);
			} else {
				this.AnimateIn();
			}
		}

		protected void AnimateIn(){
			
			//animate in
			var pt = new PointF (this.Center.X, 20);
			UIView.Animate (1, 0.5, UIViewAnimationOptions.CurveEaseIn, () => {
				this.Center = pt;},
			() => {
				this.showing = true;
			}
			);
			
		}
		
		protected void AnimateOut(bool animateBackIn){
			var pt = new PointF(this.Center.X,  -20);
			UIView.Animate(1,0.5,UIViewAnimationOptions.CurveEaseOut,()=>{
				this.Center = pt;},
			() =>{
				this.showing = false;
				if(animateBackIn) this.AnimateIn();
			}
			);		
		}
	}
}

