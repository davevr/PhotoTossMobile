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
	[Register ("ImageInfoCell")]
	partial class ImageInfoCell
	{
		[Outlet]
		UIKit.UILabel DateTitle { get; set; }

		[Outlet]
		UIKit.UIImageView MainImage { get; set; }

		[Outlet]
		UIKit.UIImageView PersonImage { get; set; }

		[Outlet]
		UIKit.UIImageView TypeIcon { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DateTitle != null) {
				DateTitle.Dispose ();
				DateTitle = null;
			}

			if (MainImage != null) {
				MainImage.Dispose ();
				MainImage = null;
			}

			if (PersonImage != null) {
				PersonImage.Dispose ();
				PersonImage = null;
			}

			if (TypeIcon != null) {
				TypeIcon.Dispose ();
				TypeIcon = null;
			}
		}
	}
}
