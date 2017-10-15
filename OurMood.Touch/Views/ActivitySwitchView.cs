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
	public class SnapshotToolSwitchView : SwitchView
	{
		public SnapshotToolSwitchView (PointF position)
			:base(position)
		{
		}

		protected override void BuildView ()
		{
			this.backgroundImageOff = Resources.SnapshotSwitchOff;
			this.backgroundImageOn = Resources.SnapshotSwitchOn;
			this.labelText = "Show snapshot";
			base.BuildView ();
		}
	}

	public class EventsSwitchView : SwitchView
	{
		public EventsSwitchView (PointF position)
			:base(position)
		{
		}

		protected override void BuildView ()
		{
			this.backgroundImageOff = Resources.EventSwitchOff;
			this.backgroundImageOn = Resources.EventSwitchOn;
			this.labelText = "Show agenda";
			base.BuildView ();
		}
	}

	public class PromptsSwitchView : SwitchView
	{
		public PromptsSwitchView (PointF position)
			:base(position)
		{
		}

		protected override void BuildView ()
		{
			this.backgroundImageOff = Resources.PromptSwitchOff;
			this.backgroundImageOn = Resources.PromptSwitchOn;
			this.labelText = "Show prompts";
			base.BuildView ();
		}
	}

	public abstract class SwitchView : UIView
	{
		public  event EventHandler<EventArgs> Changed;

		UIImageView backgroundImage;
		protected UIImage backgroundImageOff;
		protected UIImage backgroundImageOn;
		protected string labelText;
		UISwitch switchView;
		ActivityType activityType;
		UILabel title;
		bool active = false;

		public bool Active {
			get {
				return active;
			}
		}

		public SwitchView (PointF position)
			: base(new RectangleF(position, new SizeF(120f, 125f)))
		{
			this.BuildView();
		}

		public void Reset ()
		{

			//this.switchView.On = false;
			if(this.Active) this.SetStatus(false);
		}

		protected virtual void BuildView ()
		{
			this.title = new UILabel (new RectangleF (5f, 85f, 100f, 20f));
			this.title.TextAlignment = UITextAlignment.Center;
			this.title.BackgroundColor = UIColor.Clear;
			this.title.TextColor = UIColor.White;
			this.title.Font = UIFont.FromName ("HelveticaNeue-CondensedBold", 16.0f);
			this.title.Center = new PointF (this.Bounds.Width / 2, this.title.Center.Y);
			
			this.backgroundImage = new UIImageView (this.backgroundImageOff);
			this.title.Text = this.labelText;
			
			this.backgroundImage.Center = new PointF (this.Bounds.Width / 2, this.backgroundImage.Center.Y);
			
			this.switchView = new UISwitch (new RectangleF (0, 50f, this.Bounds.Width, 30f));
			this.switchView.Center = new PointF (this.Bounds.Width / 2, this.switchView.Center.Y);
			this.switchView.Selected = false;
			this.switchView.ValueChanged += (object sender, EventArgs e) => {
				this.SetStatus(!this.active);
			};
			
			this.Add (this.backgroundImage);
			this.Add (this.title);
			this.Add (this.switchView);
			
			this.SetBackgroundImage ();
		}

		protected void SetStatus (bool active)
		{
			this.switchView.On = active;
			this.active = active;
			this.SetBackgroundImage ();
			if (Changed != null)
				Changed (this, new EventArgs ());
		}

		protected void SetBackgroundImage ()
		{
			this.backgroundImage.Image = this.active ? this.backgroundImageOn : this.backgroundImageOff;
		}
	}
}

