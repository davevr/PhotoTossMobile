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
	[Register ("ImageDetailController")]
	partial class ImageDetailController
	{
		[Outlet]
		UIKit.UITextField CaptionTextField { get; set; }

		[Outlet]
		UIKit.UIScrollView ImageScroller { get; set; }

		[Outlet]
		UIKit.UIButton SendBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CaptionTextField != null) {
				CaptionTextField.Dispose ();
				CaptionTextField = null;
			}

			if (ImageScroller != null) {
				ImageScroller.Dispose ();
				ImageScroller = null;
			}

			if (SendBtn != null) {
				SendBtn.Dispose ();
				SendBtn = null;
			}
		}
	}
}
