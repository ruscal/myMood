
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace MyMood
{
	public partial class SupportPopupViewController : DialogViewController
	{
		public event EventHandler<EventArgs> Close;

		EntryElement password;
		CheckboxElement showScreen;
		StyledStringElement confirm;
		string pass = "my3mood!";

		public SupportPopupViewController () : base (UITableViewStyle.Grouped, null)
		{
			password = new EntryElement ("Password", "Support password", String.Empty, true);
			showScreen = new CheckboxElement("Show support screen", false);
			confirm = new StyledStringElement("Confirm", delegate { Confirm(); });
			confirm.BackgroundColor = UIColor.DarkGray;
			confirm.TextColor = UIColor.White;
			Root = new RootElement ("SupportPopupViewController") {
				new Section ("Support"){
					password,
					showScreen,
					confirm
				},
			};
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public void Confirm ()
		{
			if (showScreen.Value && password.Value != pass) {
				UIAlertView alert = new UIAlertView("Invalid password!", "Wrong password, please check and try again.", null, "OK", null);
				alert.Show();
			} else if (showScreen.Value) {
				NSNotificationCenter.DefaultCenter.PostNotificationName("ShowSupportScreen",null);
				password.Value = "";
				if(Close != null) Close(this, new EventArgs());
	
			} else {
				NSNotificationCenter.DefaultCenter.PostNotificationName("HideSupportScreen",null);
				if(Close != null) Close(this, new EventArgs());
		
			}
		}
	}
}
