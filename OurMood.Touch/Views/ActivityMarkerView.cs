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

	public class PromptMarkerView : ActivityMarkerView
	{
		public PromptMarkerView (Activity prompt)
			:base(Resources.PromptMarker)
		{
			this.time.Frame = new RectangleF(45f, 30f, 50f, 20f);
			this.title.Frame = new RectangleF(10f, 45f, this.Bounds.Width-20f, 50f);
			time.Text = prompt.TimeStampLocal.ToString ("H:mm");
			title.Text = prompt.Title;
		}

		protected override void Reposition(){
			this.Frame = new RectangleF(this.Frame.X,
			                            0,
			                            this.Frame.Width,
			                            this.Frame.Height);
			this.button.Center = new PointF(this.Bounds.Width/2, this.button.Bounds.Height*this.button.Transform.yy/2);
		}
	}

	public class EventMarkerView : ActivityMarkerView
	{
		public EventMarkerView (Activity evnt)
			:base(Resources.EventMarker)
		{
			this.time.Frame = new RectangleF(40f, 15f, 50f, 20f);
			this.title.Frame = new RectangleF(10f, 35f, this.Bounds.Width-20f, 50f);
			time.Text = evnt.TimeStampLocal.ToString ("H:mm");
			title.Text = evnt.Title;
		}

		protected override void Reposition ()
		{
			if (this.Superview != null) {
				var y = this.Superview.Frame.Height - this.Bounds.Height * this.Transform.yy;
				this.Frame = new RectangleF (this.Frame.X,
			                            y,
			                            this.Frame.Width,
			                            this.Frame.Height);
				this.button.Center = new PointF (this.Bounds.Width / 2, this.Bounds.Height - this.button.Bounds.Height * this.button.Transform.yy / 2);
			}
		}
	}

	public abstract class ActivityMarkerView : UIView
	{
		protected UILabel time;
		protected UILabel title;
		protected PointF originalCenter;
		//protected UIButton button;
		protected bool shrunk = false;
		protected UIButton button;

		public ActivityMarkerView (UIImage image)
					:base(new RectangleF(new PointF(0, 0), image.Size))
		{
			this.BackgroundColor = UIColor.Clear;

			this.button = new UIButton(this.Bounds);
			this.button.SetBackgroundImage(image, UIControlState.Normal);
			this.button.BackgroundColor = UIColor.Clear;
			this.Add(this.button);

			time = new UILabel(new RectangleF(40f, 15f, 50f, 20f));

			time.BackgroundColor = UIColor.Clear;
			time.Font = UIFont.FromName("HelveticaNeue",14.0f);
			this.button.Add(time);
			
			 title = new UILabel(new RectangleF(10f, 35f, this.Bounds.Width-20f, 50f));
			title.TextAlignment = UITextAlignment.Center;

			title.BackgroundColor = UIColor.Clear;
			title.Font = UIFont.FromName("HelveticaNeue-Bold",11.0f);
			title.Lines = 2;
			this.button.Add(title);

//			this.button = new UIButton(this.Bounds);
//			this.button.TouchUpInside += (object sender, EventArgs e) => {
//				this.Toggle();
//			};
//			this.Add(this.button);

			this.originalCenter = this.Center;

			this.button.TouchUpInside += (object sender, EventArgs e) => {
				this.Toggle();
			};


	
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);
			this.Reposition();
		}

		public void Toggle ()
		{
			if (this.shrunk) {
				this.Grow();
			} else {
				this.Shrink();
			}
		}

		public virtual void Shrink ()
		{
			if (!this.shrunk) {
				this.title.Hidden = true;
				this.time.Hidden = true;

				this.button.Transform = CGAffineTransform.MakeScale (0.4f, 0.4f);
				this.shrunk = true;

				this.Reposition ();
				//this.SetNeedsDisplay();
			}
		}

		public virtual void Grow ()
		{
			if (this.shrunk) {
				this.title.Hidden = false;
				this.time.Hidden = false;

				this.button.Transform = CGAffineTransform.MakeScale (1, 1);
				this.shrunk = false;

				this.Reposition ();

				//this.SetNeedsDisplay();
			}
		}



		protected abstract void Reposition();

	}
}

