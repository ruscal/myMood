using System;
using MonoTouch.UIKit;
using System.Drawing;
using MyMood.Services;

namespace MyMood
{
	public class InterestRequest :EmailRequest
	{
		public InterestRequest (UIViewController parentView)
		{
			_parentView = parentView;
			overlayImage.Image = Resources.InterestRequest;
			requestButton.Frame = new RectangleF(152,322,126,44);
			cancelButton.Frame = new RectangleF(28,322,126,44);
			emailTxt.Frame = new RectangleF(32,270,242,30);
			requestButton.TouchUpInside += (object sender, EventArgs e) => {
				request();
			};
		}

		public void show()
		{
			setView();
		}

		private void request()
		{
			if (isValidEmail(emailTxt.Text))
			{
				emailTxt.ResignFirstResponder();
				ServiceRequestStatus req = MyMoodService.Current.RegisterInterestInApp(emailTxt.Text);
				if (req.Success !=true)
				{
					emailAlert = new UIAlertView("My Mood","There was a problem connecting to the server, please try again.",null,"OK",null);
					emailAlert.Show();
					
				}
				else
				{
					string msg = "A myMood information request has been successfully registered";
					emailAlert = new UIAlertView("My Mood",msg,null,"OK",null);
					dissmissOverlay();
					emailAlert.Show();
				}
			}
			else
			{
				emailAlert = new UIAlertView("My Mood","There was a problem validating your email address, please check and try again.",null,"OK",null);
				emailAlert.Show();
				
			}
		}

	}
}

