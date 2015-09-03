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
		UIKit.UIImageView LargeImageView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LargeImageView != null) {
				LargeImageView.Dispose ();
				LargeImageView = null;
			}
		}
	}
}
