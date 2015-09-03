
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
		public double Rotation { get; set;}
		public double RotationSpeed { get; set;}


		public TossedImageCell (IntPtr handle) : base (handle)
		{
		}

		public static TossedImageCell Create ()
		{
			return (TossedImageCell)Nib.Instantiate (null, null) [0];
		}

		public void ConformToRecord(PhotoRecord curPhoto, string id, NSIndexPath indexPath)
		{
			double rotDeg = ((45 - rnd.Next (90)))/ 10.0;
			Rotation = (Math.PI * 2) * (rotDeg / 360);
			Layer.AnchorPoint = new CGPoint (.5, 0);
			Transform = CGAffineTransform.MakeRotation((nfloat)Rotation);
			string thumbnailURL = curPhoto.imageUrl + "=s256-c";
			MainImageView.SetImage(new NSUrl(thumbnailURL), UIImage.FromBundle("placeholder"));

			if ((curPhoto.tosserid != 0) && (curPhoto.tosserid != PhotoTossRest.Instance.CurrentUser.id)) {
				ThumbnailView.Hidden = false;
				ThumbnailView.Layer.CornerRadius = ThumbnailView.Bounds.Width / 2;
				ThumbnailView.Layer.MasksToBounds = true;
				ThumbnailView.SetImage (new NSUrl(PhotoTossRest.Instance.GetUserProfileImage (curPhoto.tossername)), UIImage.FromBundle ("unknownperson"));
			} else {
				ThumbnailView.Hidden = true;
			}
				
			UIBezierPath shadowPath = UIBezierPath.FromRect (Bounds);
			Layer.MasksToBounds = false;
			Layer.ShadowColor = new CGColor (0, 0, 0);
			Layer.ShadowOffset = new CGSize (1, 5);
			Layer.ShadowOpacity = 0.5f;
			Layer.ShadowPath = shadowPath.CGPath;

		}
	}
}

