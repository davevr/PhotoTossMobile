// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace PhotoToss.iOSApp
{
	[Register ("HomeViewController")]
	partial class HomeViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		TossCollectionView TossCollectionView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (TossCollectionView != null) {
				TossCollectionView.Dispose ();
				TossCollectionView = null;
			}
		}
	}
}
