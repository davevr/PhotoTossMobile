// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace PhotoToss.iOSApp
{
	[Register ("TossInfoCell")]
	partial class TossInfoCell
	{
		[Outlet]
		UIKit.UIButton ShowCatchesButton { get; set; }

		[Outlet]
		UIKit.UILabel TossLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ShowCatchesButton != null) {
				ShowCatchesButton.Dispose ();
				ShowCatchesButton = null;
			}

			if (TossLabel != null) {
				TossLabel.Dispose ();
				TossLabel = null;
			}
		}
	}
}
