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
	[Register ("LeaderboardViewController")]
	partial class LeaderboardViewController
	{
		[Outlet]
		UIKit.NSLayoutConstraint FakeHeaderHeight { get; set; }

		[Outlet]
		UIKit.UIView FakeHeaderView { get; set; }

		[Outlet]
		UIKit.UITableView LeaderboardTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FakeHeaderView != null) {
				FakeHeaderView.Dispose ();
				FakeHeaderView = null;
			}

			if (FakeHeaderHeight != null) {
				FakeHeaderHeight.Dispose ();
				FakeHeaderHeight = null;
			}

			if (LeaderboardTable != null) {
				LeaderboardTable.Dispose ();
				LeaderboardTable = null;
			}
		}
	}
}
