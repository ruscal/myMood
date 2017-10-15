// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace OurMood.Touch
{
	[Register ("OurMoodViewController")]
	partial class OurMoodViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIView ChartLayer { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView ChartContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChartLayer != null) {
				ChartLayer.Dispose ();
				ChartLayer = null;
			}

			if (ChartContainer != null) {
				ChartContainer.Dispose ();
				ChartContainer = null;
			}
		}
	}
}
