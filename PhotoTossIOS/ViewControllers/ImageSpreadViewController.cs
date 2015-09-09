
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
			HistoryTable.Source = new ImageSpreadTableSource ();
			/*
			HistoryTable.AddObserver (@"contentSize", NSKeyValueObservingOptions.New, (observed) => {
				InvokeOnMainThread(() => 
					{
						this.HistoryTable.SizeToFit();
					});
			});
			*/

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
			LoadParents ();
		}

		private void UpdateSizes()
		{
			InvokeOnMainThread (() => {
				int cellCount = 128; // header height * 2
				if (parentList != null)
					cellCount += (parentList.Count * 360);
				if (spreadList != null)
					cellCount += (spreadList.Count * 360);
				CGRect boundsRect = HistoryTable.Bounds;
				boundsRect.Height = cellCount;
				HistoryTable.Bounds = boundsRect;
				ScrollViewer.ContentSize = new CGSize(boundsRect.Width, boundsRect.Height + MapView.Bounds.Height);
			});

		}

		private void LoadParents()
		{
			PhotoTossRest.Instance.GetImageLineage(HomeViewController.CurrentPhotoRecord.id, (resultList) =>
				{
					if (resultList != null) {
						parentList = resultList;
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
									CLLocationCoordinate2D theLoc = new CLLocationCoordinate2D (curRec.createdlat, curRec.createdlong);
									MapView.AddAnnotations (new MKPointAnnotation (){
										Title = parentCount == 1 ? "Original Toss" : "Toss #" + (parentCount - 1).ToString(),
										Subtitle = "date/time",
										Coordinate = theLoc
									});

								}

								ShowAnnotations();

							}
						});

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
			public ImageSpreadTableSource ()
			{
				
			}

			public override nint NumberOfSections (UITableView tableView)
			{
				return 2;
			}

			public override string TitleForHeader (UITableView tableView, nint section)
			{
				switch (section) {
				case 0:
					return "Lineage";
					break;
				case 1:
					return "Tosses";
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

				cell.ConformToRecord (item);

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

				cell.ConformToRecord (item);

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

				cell.TextLabel.Text = item.ownerId.ToString();

				return cell;
			}

			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				nfloat theHeight = 32;

				switch (indexPath.Section) {
				case 0:
					theHeight = 360;
					break;
				case 1:
					theHeight = 360;
					break;
				}

				return theHeight;
			}


		}
	}
}

