// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace MyMood
{
	[Register ("UpdateRequiredViewController")]
	partial class UpdateRequiredViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIImageView BackgroundImage { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton CancelBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BackgroundImage != null) {
				BackgroundImage.Dispose ();
				BackgroundImage = null;
			}

			if (CancelBtn != null) {
				CancelBtn.Dispose ();
				CancelBtn = null;
			}
		}
	}
}
