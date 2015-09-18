using System;
using System.Collections.Generic;
using PhotoToss.Core;
using CoreGraphics;

using UIKit;
using Foundation;
namespace PhotoToss.iOSApp
{
	public class TossStatusDataSource : UICollectionViewDataSource
	{
		public List<PhotoRecord> photoList;

		public TossStatusDataSource ()
		{
		}

		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = (TossProgressCell)collectionView.DequeueReusableCell (new NSString(TossViewController.kTossCellName), indexPath);
			PhotoRecord curPhoto = photoList [(int)indexPath.Item];

			cell.ConformToRecord (curPhoto, TossViewController.kTossCellName, indexPath);


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

