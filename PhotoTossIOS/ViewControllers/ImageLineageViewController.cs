
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using System.Collections.Generic;

namespace PhotoToss.iOSApp
{
	public partial class ImageLineageViewController : UIViewController
	{
		public PhotoRecord CurrentMarkerRecord;

		public ImageLineageViewController () : base ("ImageLineageViewController", null)
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
			LineageTable.RegisterNibForCellReuse(UINib.FromName(ImageLineageCell.Key, NSBundle.MainBundle), ImageLineageCell.Key);
			LineageTable.RowHeight = 440;

			LoadLineage ();
		}

		private void LoadLineage()
		{
			PhotoTossRest.Instance.GetImageLineage (CurrentMarkerRecord.id, (parents) => {

				UpdateLineage (parents);
			});
		}

		private void UpdateLineage(List<PhotoRecord> parents)
		{
			LineageDataSource dataSource = new LineageDataSource();
			parents.Insert (0, CurrentMarkerRecord);
			InvokeOnMainThread(() => {
				dataSource.photoList = parents;
				LineageTable.DataSource = dataSource;
				LineageTable.ReloadData();
			});

		}
	}

	public class LineageDataSource : UITableViewDataSource
	{
		public List<PhotoRecord> photoList;

		public LineageDataSource ()
		{
		}
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (ImageLineageCell)tableView.DequeueReusableCell (new NSString(ImageLineageCell.Key), indexPath);
			PhotoRecord curPhoto = photoList [(int)indexPath.Item];

			cell.ConformToRecord (curPhoto, indexPath.Row);


			return cell;
		}


		public override nint NumberOfSections (UITableView collectionView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return photoList.Count;
		}



		public PhotoRecord GetItem(NSIndexPath indexPath)
		{
			return photoList [(int)indexPath.Item];
		}

	}
}

