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
	[Register ("ChatHistoryCell")]
	partial class ChatHistoryCell
	{
		[Outlet]
		UIKit.NSLayoutConstraint ChatLeftConstraint { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ChatRightConstraint { get; set; }

		[Outlet]
		UIKit.UILabel ChatTurnLabel { get; set; }

		[Outlet]
		UIKit.UIView ChatTurnWrapper { get; set; }

		[Outlet]
		UIKit.UIImageView MyImage { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint MyImageHeight { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint OtherPersonHeight { get; set; }

		[Outlet]
		UIKit.UIImageView OtherPersonImage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChatLeftConstraint != null) {
				ChatLeftConstraint.Dispose ();
				ChatLeftConstraint = null;
			}

			if (ChatRightConstraint != null) {
				ChatRightConstraint.Dispose ();
				ChatRightConstraint = null;
			}

			if (ChatTurnLabel != null) {
				ChatTurnLabel.Dispose ();
				ChatTurnLabel = null;
			}

			if (MyImage != null) {
				MyImage.Dispose ();
				MyImage = null;
			}

			if (OtherPersonImage != null) {
				OtherPersonImage.Dispose ();
				OtherPersonImage = null;
			}

			if (ChatTurnWrapper != null) {
				ChatTurnWrapper.Dispose ();
				ChatTurnWrapper = null;
			}

			if (OtherPersonHeight != null) {
				OtherPersonHeight.Dispose ();
				OtherPersonHeight = null;
			}

			if (MyImageHeight != null) {
				MyImageHeight.Dispose ();
				MyImageHeight = null;
			}
		}
	}
}
