using System;
using CoreGraphics;

using UIKit;
using Foundation;
using PhotoToss.Core;
using System.Collections.Generic;


namespace PhotoToss.iOSApp
{
	public class TossedImageDataSource : UICollectionViewDataSource
	{
		public List<PhotoRecord> photoList;

		public TossedImageDataSource ()
		{
		}

		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = (TossedImageCell)collectionView.DequeueReusableCell (new NSString(HomeViewController.kTossCellName), indexPath);
			PhotoRecord curPhoto = photoList [(int)indexPath.Item];

			cell.ConformToRecord (curPhoto, HomeViewController.kTossCellName, indexPath);


			return cell;
		}

		public override nint NumberOfSections (UICollectionView collectionView)
		{
			return 1;
		}

		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			return photoList.Count;
		}

		public PhotoRecord GetItem(NSIndexPath indexPath)
		{
			return photoList [(int)indexPath.Item];
		}

	}
}

