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
	[Register ("ImageLineageCell")]
	partial class ImageLineageCell
	{
		[Outlet]
		UIKit.UITextField CaptionField { get; set; }

		[Outlet]
		UIKit.UILabel CaptionLabel { get; set; }

		[Outlet]
		UIKit.UIScrollView ImageScroller { get; set; }

		[Outlet]
		UIKit.UIButton SendBtn { get; set; }

		[Outlet]
		UIKit.UILabel TossDateLabel { get; set; }

		[Outlet]
		UIKit.UIImageView UserImageView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (UserImageView != null) {
				UserImageView.Dispose ();
				UserImageView = null;
			}

			if (ImageScroller != null) {
				ImageScroller.Dispose ();
				ImageScroller = null;
			}

			if (TossDateLabel != null) {
				TossDateLabel.Dispose ();
				TossDateLabel = null;
			}

			if (SendBtn != null) {
				SendBtn.Dispose ();
				SendBtn = null;
			}

			if (CaptionLabel != null) {
				CaptionLabel.Dispose ();
				CaptionLabel = null;
			}

			if (CaptionField != null) {
				CaptionField.Dispose ();
				CaptionField = null;
			}
		}
	}
}
