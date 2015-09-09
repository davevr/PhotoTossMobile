
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;
using CoreGraphics;


namespace PhotoToss.iOSApp
{
	public partial class ImageDetailController : UIViewController
	{
		public ImageDetailController () : base ("ImageDetailController", null)
		{
		}




		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			LargeImageView.ContentMode = UIViewContentMode.ScaleToFill;
			// Perform any additional setup after loading the view, typically from a nib.
			if ((LargeImageView != null) && (HomeViewController.CurrentPhotoRecord != null)) {
				LargeImageView.SetImage(new NSUrl (HomeViewController.CurrentPhotoRecord.imageUrl + "=s1024"),
					UIImage.FromBundle("placeholder"), ImageLoadComplete);

			}
		}

		private void ImageLoadComplete(UIImage image, NSError theErr, SDImageCacheType cacheType, NSUrl theURL )
		{
			ResizeImage();
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			var contr = this.NavigationController;
			if (contr != null) {
				contr.NavigationBar.Translucent = false;
				contr.NavigationBar.BarTintColor = UIColor.FromRGB (9, 171, 161);
				contr.NavigationBar.TintColor = UIColor.FromRGB (231,140,11);
				Title = "Tossed Image";

			}
		}

		private void ResizeImage ()
		{
			CGSize imageSize = LargeImageView.Image.Size;
			nfloat scale = imageSize.Height / imageSize.Width;
			nfloat desiredHeight = LargeImageView.Bounds.Width * scale;
			ImageHeightConstraint.Constant = desiredHeight;
			ImageScroller.ContentSize = new CGSize( LargeImageView.Bounds.Width, desiredHeight);
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			ResizeImage ();
		}


	}
}

