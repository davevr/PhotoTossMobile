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
	[Register ("ImageStatsViewController")]
	partial class ImageStatsViewController
	{
		[Outlet]
		UIKit.UILabel ImageCatchesText { get; set; }

		[Outlet]
		UIKit.UILabel ImageLineageText { get; set; }

		[Outlet]
		UIKit.UILabel ImageTossesText { get; set; }

		[Outlet]
		UIKit.UILabel TotalImageText { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TotalImageText != null) {
				TotalImageText.Dispose ();
				TotalImageText = null;
			}

			if (ImageLineageText != null) {
				ImageLineageText.Dispose ();
				ImageLineageText = null;
			}

			if (ImageTossesText != null) {
				ImageTossesText.Dispose ();
				ImageTossesText = null;
			}

			if (ImageCatchesText != null) {
				ImageCatchesText.Dispose ();
				ImageCatchesText = null;
			}
		}
	}
}
