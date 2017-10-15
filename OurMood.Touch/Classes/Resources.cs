using System;
using MonoTouch.UIKit;
using MyMood.DL;
using System.Collections.Generic;

namespace OurMood.Touch
{
	public  class Resources
	{
		public static UIImage SnapshotIcon {
			get {
				return UIImage.FromFile ("Images/Buttons/SnapshotSmall.png");
			}
		}

		public static UIImage SnapshotSliderBtn {
			get {
				return UIImage.FromFile ("Images/Buttons/SnapshotSlider.png");
			}
		}

		public static UIImage SnapshotSwitchOff {
			get {
				return UIImage.FromFile ("Images/Buttons/SnapshotOff.png");
			}
		}

		public static UIImage SnapshotSwitchOn {
			get {
				return UIImage.FromFile ("Images/Buttons/SnapshotOn.png");
			}
		}

		public static UIImage SnapshotPanelLeft {
			get {
				return UIImage.FromFile ("Images/Backgrounds/SnapshotPanelLeft.png");
			}
		}

		public static UIImage SnapshotPanelRight {
			get {
				return UIImage.FromFile ("Images/Backgrounds/SnapshotPanelRight.png");
			}
		}

		public static UIImage DragButton {
			get {
				return UIImage.FromFile ("Images/Buttons/Action_Button.png");
			}
		}

		public static UIImage Loading {
			get {
				return UIImage.FromFile ("Images/Backgrounds/Loading.gif");
			}
		}

		public static IEnumerable<UIImage> LoadingImages {
			get {
				List<UIImage> images = new List<UIImage>();
				for(var i=1;i<=24;i++){
					images.Add(UIImage.FromFile (string.Format("Images/Loading/loading_{0}.png", i)));
				}
				return images;
			}
		}

		public static UIImage MoodMapKey {
			get {
				return UIImage.FromFile ("Images/MoodMap/Colour-Key.png");
			}
		}

		public static UIImage MoodMapWindow {
			get {
				return UIImage.FromFile ("Images/MoodMap/MoodMapWindow3.png");
			}
		}

		public static UIImage MoodMapWindow2 {
			get {
				return UIImage.FromFile ("Images/MoodMap/MoodMapWindow2.png");
			}
		}

		public static UIImage SyncIconAmber {
			get {
				return UIImage.FromFile ("Images/Buttons/SyncIcon-Amber.png");
			}
		}
		
		public static UIImage SyncIconGreen {
			get {
				return UIImage.FromFile ("Images/Buttons/SyncIcon-Green.png");
			}
		}
		
		public static UIImage SyncIconGrey {
			get {
				return UIImage.FromFile ("Images/Buttons/SyncIcon-Grey.png");
			}
		}
		
		public static UIImage SyncIconPurple {
			get {
				return UIImage.FromFile ("Images/Buttons/SyncIcon-Purple.png");
			}
		}

		public static UIImage EventSwitchOn {
			get {
				return UIImage.FromFile ("Images/Buttons/EventIcon_On.png");
			}
		}

		public static UIImage EventSwitchOff {
			get {
				return UIImage.FromFile ("Images/Buttons/EventIcon_Off.png");
			}
		}

		public static UIImage EventMarker {
			get {
				return UIImage.FromFile ("Images/Buttons/EventMarker.png");
			}
		}

		public static UIImage PromptSwitchOn {
			get {
				return UIImage.FromFile ("Images/Buttons/PromptIcon_On.png");
			}
		}
		
		public static UIImage PromptSwitchOff {
			get {
				return UIImage.FromFile ("Images/Buttons/PromptIcon_Off.png");
			}
		}

		public static UIImage PromptMarker {
			get {
				return UIImage.FromFile ("Images/Buttons/PromptMarker.png");
			}
		}

		public static UIImage RefreshButton {
			get {
				return UIImage.FromFile ("Images/Buttons/RefreshButton.png");
			}
		}
	}
}

