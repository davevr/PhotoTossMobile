
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;
using CoreGraphics;

namespace PhotoToss.iOSApp
{
	public partial class TossProgressCell : UICollectionViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("TossProgressCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("TossProgressCell");

		public TossProgressCell (IntPtr handle) : base (handle)
		{
		}

		public static TossProgressCell Create ()
		{
			return (TossProgressCell)Nib.Instantiate (null, null) [0];
		}

		public void ConformToRecord(PhotoRecord curPhoto, string id, NSIndexPath indexPath)
		{
			ThumbnailView.SetImage (new NSUrl(PhotoTossRest.Instance.GetUserProfileImage (curPhoto.ownername)), UIImage.FromBundle ("unknownperson"));


			UIBezierPath shadowPath = UIBezierPath.FromRect (Bounds);
			Layer.MasksToBounds = false;
			Layer.ShadowColor = new CGColor (0, 0, 0);
			Layer.ShadowOffset = new CGSize (1, 5);
			Layer.ShadowOpacity = 0.5f;
			Layer.ShadowPath = shadowPath.CGPath;

		}
	}
}

