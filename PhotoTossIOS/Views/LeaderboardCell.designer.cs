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
	[Register ("LeaderboardCell")]
	partial class LeaderboardCell
	{
		[Outlet]
		UIKit.UIImageView ImageThumbnail { get; set; }

		[Outlet]
		UIKit.UILabel IndexLabel { get; set; }

		[Outlet]
		UIKit.UILabel ShareCountLabel { get; set; }

		[Outlet]
		UIKit.UIImageView UserImage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (IndexLabel != null) {
				IndexLabel.Dispose ();
				IndexLabel = null;
			}

			if (ShareCountLabel != null) {
				ShareCountLabel.Dispose ();
				ShareCountLabel = null;
			}

			if (ImageThumbnail != null) {
				ImageThumbnail.Dispose ();
				ImageThumbnail = null;
			}

			if (UserImage != null) {
				UserImage.Dispose ();
				UserImage = null;
			}
		}
	}
}
