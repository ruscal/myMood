// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace MonoTestApngPlayer
{
	[Register ("MonoTestApngPlayerViewController")]
	partial class MonoTestApngPlayerViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton btnPlay { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnPlay2 { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnPlay3 { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnPlay != null) {
				btnPlay.Dispose ();
				btnPlay = null;
			}

			if (btnPlay2 != null) {
				btnPlay2.Dispose ();
				btnPlay2 = null;
			}

			if (btnPlay3 != null) {
				btnPlay3.Dispose ();
				btnPlay3 = null;
			}
		}
	}
}
