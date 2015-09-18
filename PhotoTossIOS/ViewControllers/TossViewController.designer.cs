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
	[Register ("TossViewController")]
	partial class TossViewController
	{
		[Outlet]
		UIKit.UICollectionView CatchCollection { get; set; }

		[Outlet]
		UIKit.UIButton DoneBtn { get; set; }

		[Outlet]
		UIKit.UIImageView TossImageView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CatchCollection != null) {
				CatchCollection.Dispose ();
				CatchCollection = null;
			}

			if (DoneBtn != null) {
				DoneBtn.Dispose ();
				DoneBtn = null;
			}

			if (TossImageView != null) {
				TossImageView.Dispose ();
				TossImageView = null;
			}
		}
	}
}
