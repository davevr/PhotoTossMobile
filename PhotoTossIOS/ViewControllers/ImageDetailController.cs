
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
		public UIImage CurrentImage { get; set; }
		UIImageView LargeImageView {get; set;}

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

			LargeImageView = new UIImageView ();

			ImageScroller.AddSubview (LargeImageView);

			if ((LargeImageView != null) && (HomeViewController.CurrentPhotoRecord != null)) {
				LargeImageView.SetImage(new NSUrl (HomeViewController.CurrentPhotoRecord.imageUrl + "=s1024"),
					UIImage.FromBundle("placeholder"), ImageLoadComplete);

			}
		}

		private void ImageLoadComplete(UIImage image, NSError theErr, SDImageCacheType cacheType, NSUrl theURL )
		{
			//ResizeImage();
			CurrentImage = image;
			ImageScroller.ContentSize = LargeImageView.Image.Size;
			//Set the zoom properties:
			ImageScroller.MaximumZoomScale = 3f;
			ImageScroller.MinimumZoomScale = .1f;
			ImageScroller.ViewForZoomingInScrollView += (UIScrollView sv) => { return LargeImageView; };
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			var contr = this.NavigationController;
			if (contr != null) {
				contr.NavigationBar.Translucent = false;
				contr.NavigationBar.BarTintColor = UIColor.FromRGB (9, 171, 161);
				contr.NavigationBar.TintColor = UIColor.FromRGB (213,88,2);
				Title = "Tossed Image";

			}
		}

		/*
		private void ResizeImage ()
		{
			CGSize imageSize = LargeImageView.Image.Size;
			nfloat scale = imageSize.Height / imageSize.Width;
			nfloat desiredHeight = LargeImageView.Bounds.Width * scale;
			ImageHeightConstraint.Constant = desiredHeight;
			ImageScroller.ContentSize = new CGSize( LargeImageView.Bounds.Width, desiredHeight);
		}
		*/

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			//ResizeImage ();
		}


	}
}

