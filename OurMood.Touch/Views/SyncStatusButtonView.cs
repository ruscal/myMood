using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.DL;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

using MyMood.Services;
using OurMood.Touch;

namespace OurMood
{
	public partial class SyncStatusButtonView : UIView
	{
		UIImageView backgroundImage;
		
		UIPopoverController supportPopup;
		DateTime? startTouch = null;
		
		public SyncStatusButtonView (RectangleF frame) 
			: base (frame)
		{
			backgroundImage = new UIImageView(this.Bounds);
			backgroundImage.Image = Resources.SyncIconGrey;
			this.Add(backgroundImage);
			
			var popupContent = new SupportPopupViewController();
			popupContent.Close += (object sender, EventArgs e)=>{
				this.supportPopup.Dismiss(true);
			};
			this.supportPopup = new UIPopoverController(popupContent);
			this.supportPopup.SetPopoverContentSize(new SizeF( 350, 200), false);
			
			UIButton showStatusBtn = new UIButton(this.Bounds);
			
			showStatusBtn.TouchDown += (object sender, EventArgs e) => {
				startTouch = DateTime.Now;
			};
			
			showStatusBtn.TouchUpInside += (object sender, EventArgs e) => {
				if(startTouch != null && DateTime.Now.Subtract(startTouch.Value).Seconds > 1){
					supportPopup.PresentFromRect(this.Frame, this.Superview, UIPopoverArrowDirection.Up, true);
				}else{
					var lastPush = ApplicationState.Current.LastSuccessfulGlobalReportRequest.HasValue 
						? ApplicationState.Current.LastSuccessfulGlobalReportRequest.Value.ToLocalTime().ToString()
							: "Never";
					UIAlertView alert = new UIAlertView ("Last Successful Sync", lastPush, null, "OK", null);
					alert.Show ();
				}
			};
			
			
			
			this.Add(showStatusBtn);
			
			
			
		}
		
		public void ShowSupportpopup ()
		{
			
		}
		
		public void ShowSyncing ()
		{
			backgroundImage.Image = Resources.SyncIconGreen;
			
		}
		
		public void ShowStatus (SyncSuccessLevel status)
		{
			switch (status) {
			case SyncSuccessLevel.FailedSevere:
				backgroundImage.Image = Resources.SyncIconAmber;
				break;
			case SyncSuccessLevel.FailedWarning:
				backgroundImage.Image = Resources.SyncIconAmber;
				break;
			default:
				backgroundImage.Image = Resources.SyncIconGrey;
				break;
				
			}
			
		}
	}
}
