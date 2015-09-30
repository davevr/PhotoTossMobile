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
		UIKit.UIView FakeHeader { get; set; }

		[Outlet]
		UIKit.UICollectionView TossedImageCollectionView { get; set; }

		[Outlet]
		UIKit.UILabel TossTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FakeHeader != null) {
				FakeHeader.Dispose ();
				FakeHeader = null;
			}

			if (TossedImageCollectionView != null) {
				TossedImageCollectionView.Dispose ();
				TossedImageCollectionView = null;
			}

			if (TossTitle != null) {
				TossTitle.Dispose ();
				TossTitle = null;
			}
		}
	}
}
