
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;


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

			// Perform any additional setup after loading the view, typically from a nib.
			if ((LargeImageView != null) && (HomeViewController.CurrentPhotoRecord != null))
				LargeImageView.SetImage(new NSUrl(HomeViewController.CurrentPhotoRecord.imageUrl + "=s1024"));

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

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}


	}
}

