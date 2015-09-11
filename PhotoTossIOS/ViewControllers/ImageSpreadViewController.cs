
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
		private static List<PhotoRecord> parentList;
		private static List<object> spreadList;
		public static string kImageInfoCellName = "ImageInfoCell";
		public static string kTossInfoCellName = "TossInfoCell";
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

			HistoryTable.RegisterNibForCellReuse (UINib.FromName ("ImageInfoCell", NSBundle.MainBundle), kImageInfoCellName);
			HistoryTable.RegisterNibForCellReuse (UINib.FromName ("TossInfoCell", NSBundle.MainBundle), kTossInfoCellName);
			HistoryTable.RowHeight = UITableView.AutomaticDimension;
			HistoryTable.EstimatedRowHeight = (nfloat)320.0;
			HistoryTable.Source = new ImageSpreadTableSource (this);


			ShowAllBtn.TouchUpInside += (object sender, EventArgs e) => {
				ShowAnnotations();
			};

			ShowMeBtn.TouchUpInside += (object sender, EventArgs e) => {
				LocationHelper.StartLocationManager (CoreLocation.CLLocation.AccuracyBest);
				LocationHelper.LocationResult curLoc = LocationHelper.GetLocationResult ();
				LocationHelper.StopLocationManager ();
				ScrollToLoc(curLoc.Latitude, curLoc.Longitude);
			};
			LoadParents ();

		}

		public void ExpandTossRecord(TossRecord theRec)
		{
			LoadImagesForToss (theRec);
		}

		public void ExpandImageRecord(PhotoRecord theRec)
		{
			LoadTossesForImage (theRec);

		}

		private void ShowAnnotations()
		{
			InvokeOnMainThread (() => {
				MapView.ShowAnnotations (MapView.Annotations, true);
			});
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			UpdateSizes ();
		}

		private nfloat GetCellSizes()
		{
			nfloat totalSize = 0;
			for (nint section = 0; section < HistoryTable.Source.NumberOfSections(HistoryTable); section++) {
				for (nint row = 0; row < HistoryTable.Source.RowsInSection (HistoryTable, section); row++) {
					NSIndexPath newPath = NSIndexPath.FromItemSection (row, section);
					totalSize += HistoryTable.Source.GetHeightForRow (HistoryTable, newPath);
				}
			}

			return totalSize;
		}

		private void UpdateSizes()
		{
			/*
			nfloat cellSizes = GetCellSizes ();
			InvokeOnMainThread (() => {
				nfloat cellCount = 128; // header height * 2
				cellCount += cellSizes;
				CGRect boundsRect = HistoryTable.Bounds;
				boundsRect.Height = cellCount + 320;
				HistoryTable.Bounds = boundsRect;

			});
	*/
		}

		public void ScrollToLoc(double latLoc, double longLoc)
		{
			if (latLoc < -90)
			latLoc = 0;
			else if (latLoc > 90)
				latLoc = 0;

			if (longLoc < -180)
				longLoc = -180;
			else if (longLoc > 0)
				longLoc = 0;
			
			CLLocationCoordinate2D theLoc = new CLLocationCoordinate2D (latLoc, longLoc);
			MKCoordinateRegion region;
			MKCoordinateSpan span;

			span.LatitudeDelta=0.01;
			span.LongitudeDelta=0.01; 

			region.Span=span;
			region.Center=theLoc;

			InvokeOnMainThread (() => {
				MapView.SetRegion (region, true);
				MapView.RegionThatFits (region);
			});

		}

		private void LoadParents()
		{
			parentList = new List<PhotoRecord> ();
			parentList.Add (HomeViewController.CurrentPhotoRecord);
			PhotoTossRest.Instance.GetImageLineage(HomeViewController.CurrentPhotoRecord.id, (resultList) =>
				{
					if (resultList != null) {
						parentList.InsertRange(1,resultList);
					}

					InvokeOnMainThread(() => 
						{
							HistoryTable.ReloadData();
							UpdateSizes();
							// add pins
							if (resultList != null) 
							{
								int parentCount = resultList.Count;

								foreach (PhotoRecord curRec in resultList)
								{
									double latLoc = curRec.createdlat;
									double longLoc = curRec.createdlong;
									if (latLoc < -90)
										latLoc = 0;
									else if (latLoc > 90)
										latLoc = 0;

									if (longLoc < -180)
										longLoc = -180;
									else if (longLoc > 0)
										longLoc = 0;
									
									CLLocationCoordinate2D theLoc = new CLLocationCoordinate2D (latLoc, longLoc);
									MapView.AddAnnotations (new MKPointAnnotation (){
										Title = parentCount == 1 ? "Original Toss" : "Toss #" + (parentCount - 1).ToString(),
										Subtitle = "date/time",
										Coordinate = theLoc
									});
									parentCount--;

								}

								ShowAnnotations();

							}
						});

					LoadChildren();

				});
		}

		private void LoadChildren()
		{
			spreadList = new List<object> ();
			LoadTossesForImage (HomeViewController.CurrentPhotoRecord);
		}

		private void LoadTossesForImage(PhotoRecord curImage)
		{
			PhotoTossRest.Instance.GetImageTosses(curImage.id, (resultList) =>
				{
					if ((resultList != null) && (resultList.Count > 0)) {
						int curLoc = spreadList.IndexOf(curImage) + 1;
						spreadList.InsertRange(curLoc, resultList);
						curImage.tossList = resultList;

						InvokeOnMainThread(() => 
							{
								HistoryTable.ReloadData();
								UpdateSizes();
								// add pins
								int tossCount = 1;
								foreach (TossRecord curRec in resultList)
								{
									double latLoc = curRec.shareLat;
									double longLoc = curRec.shareLong;
									if (latLoc < -90)
										latLoc = 0;
									else if (latLoc > 90)
										latLoc = 0;

									if (longLoc < -180)
										longLoc = -180;
									else if (longLoc > 0)
										longLoc = 0;
									
									CLLocationCoordinate2D theLoc = new CLLocationCoordinate2D (latLoc, longLoc);
									MapView.AddAnnotations (new MKPointAnnotation (){
										Title = "Toss #" + tossCount.ToString(),
										Subtitle = "date/time",
										Coordinate = theLoc
									});
									tossCount++;

								}

								ShowAnnotations();
							});
					}
					else {
						curImage.tossList = new List<TossRecord>();
						InvokeOnMainThread(() => 
							{
								HistoryTable.ReloadData();
								UpdateSizes();
							});
					}

				});
		}

		private void LoadImagesForToss(TossRecord curToss)
		{
			PhotoTossRest.Instance.GetTossCatches(curToss.id, (resultList) =>
				{
					if ((resultList != null) && (resultList.Count > 0)) {
						int curLoc = spreadList.IndexOf(curToss) + 1;
						spreadList.InsertRange(curLoc, resultList);
						curToss.catchList = resultList;

						InvokeOnMainThread(() => 
							{
								HistoryTable.ReloadData();
								UpdateSizes();
								// add pins
								int tossCount = 1;
								foreach (PhotoRecord curRec in resultList)
								{
									double latLoc = curRec.receivedlat;
									double longLoc = curRec.receivedlong;
									if (latLoc < -90)
										latLoc = 0;
									else if (latLoc > 90)
										latLoc = 0;

									if (longLoc < -180)
										longLoc = -180;
									else if (longLoc > 0)
										longLoc = 0;

									CLLocationCoordinate2D theLoc = new CLLocationCoordinate2D (latLoc, longLoc);
									MapView.AddAnnotations (new MKPointAnnotation (){
										Title = "Toss #" + tossCount.ToString(),
										Subtitle = "date/time",
										Coordinate = theLoc
									});
									tossCount++;

								}

								ShowAnnotations();
							});
					}
					else {
						curToss.catchList = new List<PhotoRecord>();
						InvokeOnMainThread(() => 
							{
								HistoryTable.ReloadData();
								UpdateSizes();
							});
					}

				});
		}



		private void ExpandItem(int theItem)
		{
			object curItem = spreadList [theItem];

			if (curItem is PhotoRecord) {

			} else if (curItem is TossRecord) {

			}

		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public class ImageSpreadTableSource : UITableViewSource
		{
			ImageSpreadViewController viewController = null;

			public ImageSpreadTableSource (ImageSpreadViewController controller)
			{
				viewController = controller;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				switch (indexPath.Section) {
				case 0:
					HandleParentSelected (tableView, indexPath.Item);
					break;

				case 1:
					HandleSpreadSelected (tableView, indexPath.Item);
					break;
				}
			}

			private void HandleParentSelected(UITableView tableView, nint index)
			{
				PhotoRecord curRec = parentList[(int)index];
				if (curRec.tossid == 0)
					viewController.ScrollToLoc (curRec.createdlat, curRec.createdlong);
				else
					viewController.ScrollToLoc (curRec.receivedlat, curRec.createdlong);

			}

			private void HandleSpreadSelected(UITableView tableView, nint index)
			{
				object curRec = spreadList[(int)index];
				if (curRec is PhotoRecord) {
					PhotoRecord curPhotoRec = (PhotoRecord)curRec;
					if (curPhotoRec.tossid == 0)
						viewController.ScrollToLoc (curPhotoRec.createdlat, curPhotoRec.createdlong);
					else
						viewController.ScrollToLoc (curPhotoRec.receivedlat, curPhotoRec.createdlong);
				} else {
					TossRecord curTossRec = (TossRecord)curRec;
					viewController.ScrollToLoc (curTossRec.shareLat, curTossRec.shareLong);
				}

			}


			public override nint NumberOfSections (UITableView tableView)
			{
				return 2;
			}

			public override string TitleForHeader (UITableView tableView, nint section)
			{
				switch (section) {
				case 0:
					return "Where it came from";
					break;
				case 1:
					return "Where it went";
					break;
				}

				return null;
			}



			public override nint RowsInSection (UITableView tableview, nint section)
			{
				nint count = 0;
				switch (section)
				{
				case 0:
					if (parentList != null)
						count = parentList.Count;
					break;
				case 1:
					if (spreadList != null)
						count = spreadList.Count;
					break;
				}

				return count;

			}

			private nint DeepTossCount(List<TossRecord> theTossList)
			{
				nint theCount = 0;

				if (theTossList != null)
					theCount = theTossList.Count;

				if (theCount > 0) {
					foreach (TossRecord curToss in theTossList) {

						if (curToss.catchList != null)
							theCount += DeepImageCount (curToss.catchList);
					}

				}

				return theCount;

			}

			private nint DeepImageCount(List<PhotoRecord> theImageList)
			{
				nint theCount = 0;

				if (theImageList != null)
					theCount = theImageList.Count;

				if (theCount > 0) {
					foreach (PhotoRecord curImage in theImageList) {

						if (curImage.tossList != null)
							theCount += DeepTossCount (curImage.tossList);
					}

				}

				return theCount;

			}


			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = null;

				switch (indexPath.Section) {
				case 0:
					cell = GetLineageCell (tableView, indexPath);
					break;

				case 1:
					cell = GetSpreadCell (tableView, indexPath);
					break;
				}

				return cell;
			}

			private UITableViewCell GetLineageCell(UITableView tableView, NSIndexPath indexPath)
			{
				ImageInfoCell cell = (ImageInfoCell)tableView.DequeueReusableCell (kImageInfoCellName);


				//---- if there are no cells to reuse, create a new one
				if (cell == null) { 
					cell = ImageInfoCell.Create (); // new  UITableViewCell(UITableViewCellStyle.Default, kImageInfoCellName); }
				}

				PhotoRecord item = (PhotoRecord)parentList [(int)indexPath.Item];

				cell.ConformToRecord (item, viewController, false);

				return cell;
			}

			private UITableViewCell GetSpreadCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = null;
				object curItem = spreadList [(int)indexPath.Item];

				if (curItem is PhotoRecord) {
					cell = GetSpreadPhotoCell (tableView, indexPath);
				} else if (curItem is TossRecord) {
					cell = GetSpreadTossCell (tableView, indexPath);
				}

				return cell;
			}

			private UITableViewCell GetSpreadPhotoCell(UITableView tableView, NSIndexPath indexPath)
			{
				ImageInfoCell cell = (ImageInfoCell)tableView.DequeueReusableCell (kImageInfoCellName);


				//---- if there are no cells to reuse, create a new one
				if (cell == null) { 
					cell = ImageInfoCell.Create (); // new  UITableViewCell(UITableViewCellStyle.Default, kImageInfoCellName); }
				}

				PhotoRecord item = (PhotoRecord)spreadList [(int)indexPath.Item];

				cell.ConformToRecord (item, viewController, true);

				return cell;
			}

			private UITableViewCell GetSpreadTossCell(UITableView tableView, NSIndexPath indexPath)
			{
				TossInfoCell cell = (TossInfoCell)tableView.DequeueReusableCell (kTossInfoCellName);


				//---- if there are no cells to reuse, create a new one
				if (cell == null) { 
					cell = TossInfoCell.Create (); // new  UITableViewCell(UITableViewCellStyle.Default, kImageInfoCellName); }
				}

				TossRecord item = (TossRecord)spreadList [(int)indexPath.Item];

				cell.ConformToRecord (item, viewController);

				return cell;
			}

			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				nfloat theHeight = 32;

				switch (indexPath.Section) {
				case 0:
					theHeight = 370;
					break;
				case 1:
					object theObj = spreadList [(int)indexPath.Item];
					if (theObj is PhotoRecord)
						theHeight = 370;
					else
						theHeight = 44;
					break;
				}

				return theHeight;
			}


		}
	}
}

