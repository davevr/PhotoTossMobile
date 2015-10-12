
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;


namespace PhotoToss.iOSApp
{
	public partial class ImageStatsViewController : UIViewController
	{
		public ImageStatsViewController () : base ("ImageStatsViewController", null)
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
			UpdateStats();
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		private void UpdateStats()
		{
			PhotoTossRest.Instance.GetImageStats(HomeViewController.CurrentPhotoRecord.id, DrawStats);
		}

		private void DrawStats(ImageStatsRecord theStats)
		{
			InvokeOnMainThread (() => {
				if (theStats != null) {
					TotalImageText.Text = theStats.numcopies.ToString();
					ImageLineageText.Text = theStats.numparents.ToString();
					ImageTossesText.Text = theStats.numtosses.ToString();
					ImageCatchesText.Text =theStats.numchildren.ToString();
				} else {
					TotalImageText.Text = "--";
					ImageLineageText.Text = "--";
					ImageTossesText.Text = "--";
					ImageCatchesText.Text = "--";
				}
			});

		}
	}
}

