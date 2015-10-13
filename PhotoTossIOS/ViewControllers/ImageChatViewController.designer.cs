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
	[Register ("ImageChatViewController")]
	partial class ImageChatViewController
	{
		[Outlet]
		UIKit.UILabel ChatCountLabel { get; set; }

		[Outlet]
		UIKit.UITableView ChatHistoryTableView { get; set; }

		[Outlet]
		UIKit.UITextField ChatTurnField { get; set; }

		[Outlet]
		UIKit.UIButton SendBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChatCountLabel != null) {
				ChatCountLabel.Dispose ();
				ChatCountLabel = null;
			}

			if (SendBtn != null) {
				SendBtn.Dispose ();
				SendBtn = null;
			}

			if (ChatHistoryTableView != null) {
				ChatHistoryTableView.Dispose ();
				ChatHistoryTableView = null;
			}

			if (ChatTurnField != null) {
				ChatTurnField.Dispose ();
				ChatTurnField = null;
			}
		}
	}
}
