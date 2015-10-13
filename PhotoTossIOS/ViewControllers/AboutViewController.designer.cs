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
	[Register ("AboutViewController")]
	partial class AboutViewController
	{
		[Outlet]
		UIKit.UIView FakeHeaderView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint HeaderHeight { get; set; }

		[Outlet]
		UIKit.UIButton ReviewInStoreBtn { get; set; }

		[Outlet]
		UIKit.UIButton SendFeedbackBtn { get; set; }

		[Outlet]
		UIKit.UIButton ShareBtn { get; set; }

		[Outlet]
		UIKit.UILabel ViewTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FakeHeaderView != null) {
				FakeHeaderView.Dispose ();
				FakeHeaderView = null;
			}

			if (HeaderHeight != null) {
				HeaderHeight.Dispose ();
				HeaderHeight = null;
			}

			if (ViewTitle != null) {
				ViewTitle.Dispose ();
				ViewTitle = null;
			}

			if (SendFeedbackBtn != null) {
				SendFeedbackBtn.Dispose ();
				SendFeedbackBtn = null;
			}

			if (ReviewInStoreBtn != null) {
				ReviewInStoreBtn.Dispose ();
				ReviewInStoreBtn = null;
			}

			if (ShareBtn != null) {
				ShareBtn.Dispose ();
				ShareBtn = null;
			}
		}
	}
}
