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
	[Register ("ProfileViewController")]
	partial class ProfileViewController
	{
		[Outlet]
		UIKit.UILabel CatchesCount { get; set; }

		[Outlet]
		UIKit.UILabel CollectedCount { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint FakeheaderHeight { get; set; }

		[Outlet]
		UIKit.UIView FakeHeaderView { get; set; }

		[Outlet]
		UIKit.UILabel ProfileNameLabel { get; set; }

		[Outlet]
		UIKit.UILabel TakenCount { get; set; }

		[Outlet]
		UIKit.UILabel TossesCount { get; set; }

		[Outlet]
		UIKit.UILabel ViewTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FakeHeaderView != null) {
				FakeHeaderView.Dispose ();
				FakeHeaderView = null;
			}

			if (ViewTitle != null) {
				ViewTitle.Dispose ();
				ViewTitle = null;
			}

			if (FakeheaderHeight != null) {
				FakeheaderHeight.Dispose ();
				FakeheaderHeight = null;
			}

			if (ProfileNameLabel != null) {
				ProfileNameLabel.Dispose ();
				ProfileNameLabel = null;
			}

			if (TossesCount != null) {
				TossesCount.Dispose ();
				TossesCount = null;
			}

			if (CatchesCount != null) {
				CatchesCount.Dispose ();
				CatchesCount = null;
			}

			if (TakenCount != null) {
				TakenCount.Dispose ();
				TakenCount = null;
			}

			if (CollectedCount != null) {
				CollectedCount.Dispose ();
				CollectedCount = null;
			}
		}
	}
}
