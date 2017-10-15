// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace MyMood
{
	[Register ("MyMoodViewController")]
	partial class MyMoodViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton UpdateBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton SupportBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView columnView { get; set; }

		[Outlet]
		MyMood.TimeLineTableView tableView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnInfo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnReport { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnInterest { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (UpdateBtn != null) {
				UpdateBtn.Dispose ();
				UpdateBtn = null;
			}

			if (SupportBtn != null) {
				SupportBtn.Dispose ();
				SupportBtn = null;
			}

			if (columnView != null) {
				columnView.Dispose ();
				columnView = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}

			if (btnInfo != null) {
				btnInfo.Dispose ();
				btnInfo = null;
			}

			if (btnReport != null) {
				btnReport.Dispose ();
				btnReport = null;
			}

			if (btnInterest != null) {
				btnInterest.Dispose ();
				btnInterest = null;
			}
		}
	}
}
