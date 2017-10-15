using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MyMood.Services;
using System.Text.RegularExpressions;

namespace MyMood
{
	public class RegisterInterestDialogView : DialogBaseView
	{
		
		public RegisterInterestDialogView (RectangleF frame)
			:base(frame)
		{
			backgroundImage.Image = Resources.InterestRequest;
			requestButton.SetTitle("Register",UIControlState.Normal);
		}

		
		public override void request()
		{
			if (isValidEmail(emailTxt.Text))
			{
				emailTxt.ResignFirstResponder();
				ServiceRequestStatus req = MyMoodService.Current.RegisterInterestInApp(emailTxt.Text);
				if (req.Success !=true)
				{
					emailAlert = new UIAlertView("myMood","Could not register your interest - please check that you are connected to the network",null,"OK",null);
					emailAlert.Show();
					
				}
				else
				{
					string msg = "Your interest has been registered - thank you!";
					emailAlert = new UIAlertView("myMood",msg,null,"OK",null);
					Close();
					emailAlert.Show();
				}
			}
			else
			{
				emailAlert = new UIAlertView("myMood","Email address not recognised, please check it and try again",null,"OK",null);
				emailAlert.Show();
				
			}
		}

		
		
	}
}

