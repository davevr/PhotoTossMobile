
using System;

using Foundation;
using UIKit;
using CoreGraphics;
using PhotoToss.Core;
using ZXing.Mobile;

namespace PhotoToss.iOSApp
{
	public partial class TossViewController : UIViewController
	{
		public TossViewController () : base ("TossViewController", null)
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
			DoneBtn.TouchUpInside += (object sender, EventArgs e) => 
			{
				DismissViewController(true, () => {
					// do nothing for now
				});

			};
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			long imageId = HomeViewController.CurrentPhotoRecord.id;
			LocationHelper.StartLocationManager (CoreLocation.CLLocation.AccuracyBest);
			LocationHelper.LocationResult curLoc = LocationHelper.GetLocationResult ();
			LocationHelper.StopLocationManager ();


			PhotoTossRest.Instance.StartToss (imageId, 0, 100, 100, (theToss) => {
				var writer = new BarcodeWriter {
					Format = ZXing.BarcodeFormat.AZTEC,
					Options = new ZXing.Common.EncodingOptions {
						Width = 240,
						Height = 240,
						Margin = 1
					}
				};

				string baseURL = "http://phototoss.com/share/";
				string guid = theToss.id.ToString ();
				string url = baseURL + guid;
				url = "http://phototoss.com/toss/" + guid;


				var bitMap = writer.Write (url);
				InvokeOnMainThread(() => 
					{
						TossImageView.Image = bitMap;
					});
			});
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}
	}
}

