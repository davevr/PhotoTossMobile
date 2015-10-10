using System;
using CoreGraphics;

using UIKit;
using Foundation;
using PhotoToss.Core;
using System.Collections.Generic;


namespace PhotoToss.iOSApp
{
	
	public class LeaderboardDataSource : UITableViewDataSource
	{
		public List<PhotoRecord> photoList;

		public LeaderboardDataSource ()
		{
		}
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (LeaderboardCell)tableView.DequeueReusableCell (new NSString(LeaderboardCell.Key), indexPath);
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

