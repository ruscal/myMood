using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Text.RegularExpressions;

namespace MyMood
{
	public class DialogBaseView : UIView
	{
		public event EventHandler Closed;

		UIView requestView;
		protected UIButton requestButton;
		protected UIButton cancelButton;

		protected UITextField emailTxt;
		protected UIAlertView emailAlert;
		protected UIImageView backgroundImage;

		public DialogBaseView (RectangleF frame)
			:base(frame)
		{
			backgroundImage = new UIImageView(this.Bounds);
			BuildPage();			
			ShowPage();
		}

		protected void BuildPage ()
		{
			requestView = new UIView(this.Bounds);
			requestButton = new UIButton();
			cancelButton = new UIButton();
			emailTxt = new UITextField();
			requestView.Add(backgroundImage);
		
			requestButton.Frame = new RectangleF(135,260,126,44);
			requestButton.SetBackgroundImage(Resources.ReportSubmitButton,UIControlState.Normal);
			requestButton.SetBackgroundImage(Resources.ReportSubmitButton,UIControlState.Selected);
			requestButton.SetTitle("Request",UIControlState.Normal);
			requestButton.TouchUpInside += (object sender, EventArgs e) => {
				request();
			};
			requestView.Add(requestButton);
						
			cancelButton.Frame = new RectangleF(5,260,126,44);
			cancelButton.SetBackgroundImage(Resources.ReportCancelButton,UIControlState.Normal);
			cancelButton.SetBackgroundImage(Resources.ReportCancelButton,UIControlState.Selected);			
			cancelButton.SetTitle("Cancel",UIControlState.Normal);
			cancelButton.TouchUpInside += (object sender, EventArgs e) => {
				Close();
			};
			requestView.Add(cancelButton);
			
			emailTxt.Frame = new RectangleF(15,210,236,30);
			emailTxt.BackgroundColor = UIColor.White;

			requestView.Add(emailTxt);
			
			requestView.Alpha = 0;
			this.Add (requestView);

		}

		virtual public void request()
		{
		}

		protected void ShowPage ()
		{
			UIView.Animate(0.5,()=>{requestView.Alpha = 1;});	
		}
		
		protected void Close ()
		{
			UIView.Animate(0.5,0,UIViewAnimationOptions.TransitionNone,()=>{
				requestView.Alpha=0;},
			() =>{
				if(this.Closed != null) Closed(this, new EventArgs());
				this.RemoveFromSuperview();
			}
			);
		}


		protected bool isValidEmail(string email)
		{

			string rx = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
				+ "@"
					+ @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
			if(email == null) return false;
			Regex reg = new Regex(rx);
			return reg.IsMatch(email);
		}


	}
}

