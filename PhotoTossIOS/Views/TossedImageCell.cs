
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;
using CoreGraphics;

namespace PhotoToss.iOSApp
{
	public partial class TossedImageCell : UICollectionViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("TossedImageCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("TossedImageCell");
		public static Random rnd = new Random();

		public TossedImageCell (IntPtr handle) : base (handle)
		{
		}

		public static TossedImageCell Create ()
		{
			return (TossedImageCell)Nib.Instantiate (null, null) [0];
		}

		public void ConformToRecord(PhotoRecord curPhoto, string id, NSIndexPath indexPath)
		{
			float rotDeg = ((float)(50 - rnd.Next (100)))/ 10.0f;
			float rotation = (float)(Math.PI * 2) * (rotDeg / 360);
			Transform = CGAffineTransform.MakeRotation(rotation);
			MainImageView.SetImage(new NSUrl(curPhoto.imageUrl), UIImage.FromBundle("placeholder"));

			if ((curPhoto.tosserid != 0) && (curPhoto.tosserid != PhotoTossRest.Instance.CurrentUser.id)) {
				ThumbnailView.Hidden = false;
				ThumbnailView.SetImage (new NSUrl(PhotoTossRest.Instance.GetUserProfileImage (curPhoto.tossername)), UIImage.FromBundle ("unknownperson"));
			} else {
				ThumbnailView.Hidden = true;
			}

		}
	}
}

