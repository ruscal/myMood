using System;
using MonoTouch.UIKit;
using System.Drawing;
using MyMood.Services;

namespace MyMood
{
	public class ReportRequest :EmailRequest
	{
		public ReportRequest (UIViewController parentView)
		{
			_parentView = parentView;
			overlayImage.Image = Resources.ReportRequest;
			requestButton.Frame = new RectangleF(837,322,126,44);
			cancelButton.Frame = new RectangleF(714,322,126,44);
			emailTxt.Frame = new RectangleF(719,270,244,30);
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
				//ServiceRequestStatus req =  MyMoodService.Current.s  .RequestPersonalMoodReport(emailTxt.Text);
				//req.service
				ServiceRequestStatus req = MyMoodService.Current.RequestPersonalMoodReport(emailTxt.Text);
				if (req.Success !=true)
				{
					emailAlert = new UIAlertView("My Mood","There was a problem connecting to the server, please try again.",null,"OK",null);
					emailAlert.Show();

				}
				else
				{
					string msg = string.Format("A report request will be sent to {0}",emailTxt.Text);
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

