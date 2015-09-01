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
	[Register ("HomeViewController")]
	partial class HomeViewController
	{
		[Outlet]
		UIKit.UIBarButtonItem CameraBtn { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem CatchBtn { get; set; }

		[Outlet]
		UIKit.UICollectionView TossedImageCollectionView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CameraBtn != null) {
				CameraBtn.Dispose ();
				CameraBtn = null;
			}

			if (CatchBtn != null) {
				CatchBtn.Dispose ();
				CatchBtn = null;
			}

			if (TossedImageCollectionView != null) {
				TossedImageCollectionView.Dispose ();
				TossedImageCollectionView = null;
			}
		}
	}
}
