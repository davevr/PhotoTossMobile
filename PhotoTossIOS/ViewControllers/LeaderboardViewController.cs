
using System;

using Foundation;
using UIKit;
using JVMenuPopover;
using PhotoToss.Core;
using CoreGraphics;
using System.Collections.Generic;


namespace PhotoToss.iOSApp
{
	public partial class LeaderboardViewController : JVMenuViewController
	{
		public LeaderboardViewController () : base ()
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

			LeaderboardTable.RegisterNibForCellReuse(UINib.FromName(LeaderboardCell.Key, NSBundle.MainBundle), LeaderboardCell.Key);
			LeaderboardTable.RowHeight = 144;
			FakeHeaderView.Layer.ShadowOffset = new CGSize (1, 5);
			FakeHeaderView.Layer.ShadowColor = new CGColor (0, 0, 0);
			FakeHeaderView.Layer.ShadowOpacity = 0.5f;

			TossTitle.AttributedText = new NSAttributedString("Leaderboard", UIFont.FromName("RammettoOne-Regular", 17),
				UIColor.FromRGB(255,121,0));

			// Perform any additional setup after loading the view, typically from a nib.
			LoadStats();
		}

		private void LoadStats()
		{
			PhotoTossRest.Instance.GetGlobalStats ((leaders) => {

				UpdateStats (leaders);
			});
		}

		private void UpdateStats(List<PhotoRecord> leaders)
		{
			LeaderboardDataSource dataSource = new LeaderboardDataSource();
			dataSource.photoList = leaders;
			InvokeOnMainThread(() => {
				LeaderboardTable.DataSource = dataSource;
				LeaderboardTable.ReloadData();
			});

		}
	}

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

