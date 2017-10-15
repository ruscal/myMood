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
	public class RequestReportDialogView : DialogBaseView
	{

		public RequestReportDialogView (RectangleF frame)
			:base(frame)
		{
			backgroundImage.Image = Resources.ReportRequest;

		}

		public override void request()
		{
			if (isValidEmail(emailTxt.Text))
			{
				emailTxt.ResignFirstResponder();
				//ServiceRequestStatus req =  MyMoodService.Current.s  .RequestPersonalMoodReport(emailTxt.Text);
				//req.service
				ServiceRequestStatus req = MyMoodService.Current.RequestPersonalMoodReport(emailTxt.Text);
				if (req.Success !=true)
				{
					emailAlert = new UIAlertView("myMood","Could not send request - please check that you are connected to the network",null,"OK",null);
					emailAlert.Show();
					
				}
				else
				{
					string msg = string.Format("Your mood report has been sent to {0}",emailTxt.Text);
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
