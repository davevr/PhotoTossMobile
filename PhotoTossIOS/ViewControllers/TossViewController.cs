
using System;

using Foundation;
using UIKit;
using CoreGraphics;
using PhotoToss.Core;
using ZXing.Mobile;
using System.Timers;
using System.Collections.Generic;

namespace PhotoToss.iOSApp
{
	public partial class TossViewController : UIViewController
	{
		Timer	tossTimer;
		int secondsLeft;
		int lastCount;
		long currentTossId;
		public static string kTossCellName = "TossProgressCell";

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
				EndToss();

			};

			CatchCollection.RegisterNibForCell(UINib.FromName("TossProgressCell", NSBundle.MainBundle), kTossCellName);
			CatchCollection.SetCollectionViewLayout (new UICollectionViewFlowLayout () {
				SectionInset = new UIEdgeInsets (8,8,8,8),
				ItemSize = new CGSize(50,50),
				ScrollDirection = UICollectionViewScrollDirection.Vertical,
				MinimumInteritemSpacing = 4, // minimum spacing between cells
				MinimumLineSpacing = 4 // minimum spacing between rows if ScrollDirection is Vertical or between columns if Horizontal
			}, true);

			TossStatusDataSource dataSource = new TossStatusDataSource();
			dataSource.photoList = new List<PhotoRecord>();
			CatchCollection.DataSource = dataSource;
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			long imageId = HomeViewController.CurrentPhotoRecord.id;
			LocationHelper.StartLocationManager (CoreLocation.CLLocation.AccuracyBest);
			LocationHelper.LocationResult curLoc = LocationHelper.GetLocationResult ();
			LocationHelper.StopLocationManager ();


			PhotoTossRest.Instance.StartToss (imageId, 0, curLoc.Longitude, curLoc.Latitude, (theToss) => {
				var writer = new BarcodeWriter {
					Format = ZXing.BarcodeFormat.AZTEC,
					Options = new ZXing.Common.EncodingOptions {
						Width = 240,
						Height = 240,
						Margin = 1
					}
				};
				currentTossId = theToss.id;
				string guid = currentTossId.ToString ();
				string url = "http://phototoss.com/toss/" + guid;


				var bitMap = writer.Write (url);
				lastCount = 0;
				InvokeOnMainThread(() => 
					{
						TossImageView.Image = bitMap;
						StartTossTimer();
					});
			});
		}

		private void StartTossTimer()
		{
			tossTimer = new Timer ();
			tossTimer.Interval = 1000;
			tossTimer.AutoReset = true;
			tossTimer.Elapsed += HandleTossTimerTick;
			secondsLeft = 60;
			tossTimer.Start ();
		}

		private void HandleTossTimerTick(object sender, ElapsedEventArgs e)
		{
			secondsLeft--;
			if (secondsLeft < 0) {
				EndToss ();
			} else {
				PhotoTossRest.Instance.GetTossCatches (currentTossId, (catchList) => {
				//PhotoTossRest.Instance.GetTossStatus (currentTossId, (catchList) => {
					
					InvokeOnMainThread (() => {
						if ((catchList != null) && (catchList.Count != lastCount))
							UpdateTosses(catchList);
						DoneBtn.SetTitle(String.Format ("Done (ending in {0} seconds)", secondsLeft), UIControlState.Normal);
					});
				});
			}
		}

		private void UpdateTosses(List<PhotoRecord> catchList)
		{
			((TossStatusDataSource)CatchCollection.DataSource).photoList = catchList; 
			CatchCollection.ReloadData ();
			lastCount = catchList.Count;
		}

		private void EndToss()
		{
			StopTossTimer ();
			DismissViewController(true, () => {
				// do nothing for now
			});
		}

		private void StopTossTimer()
		{
			tossTimer.Stop ();
		
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}
	}
}

