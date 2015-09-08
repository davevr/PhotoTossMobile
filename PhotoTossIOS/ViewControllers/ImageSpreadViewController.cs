
using System;
using System.Collections.Generic;

using Foundation;
using UIKit;
using MapKit;
using CoreLocation;
using CoreGraphics;
using PhotoToss.Core;

namespace PhotoToss.iOSApp
{
	public partial class ImageSpreadViewController : UIViewController
	{
		private List<PhotoRecord> parentList;
		private List<TossRecord> tossList;

		public ImageSpreadViewController () : base ("ImageSpreadViewController", null)
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
			//CGRect bounds = new CGRect (0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width);
			//MapView = new MKMapView (bounds);
			//View.AddSubview (MapView);
			
			// Perform any additional setup after loading the view, typically from a nib.
			if (HomeViewController.CurrentPhotoRecord != null) {
				CLLocationCoordinate2D theLoc = new CLLocationCoordinate2D (HomeViewController.CurrentPhotoRecord.createdlat, HomeViewController.CurrentPhotoRecord.createdlong);
				MapView.SetCenterCoordinate (theLoc, true);
				MapView.AddAnnotations (new MKPointAnnotation (){
					Title="Current Loc",
					Coordinate = theLoc
				});

			}
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}
	}
}

