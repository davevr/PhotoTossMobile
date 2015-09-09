
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;
using CoreGraphics;

namespace PhotoToss.iOSApp
{
	public partial class ImageInfoCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("ImageInfoCell");
		public static readonly UINib Nib;

		static ImageInfoCell ()
		{
			Nib = UINib.FromName ("ImageInfoCell", NSBundle.MainBundle);
		}

		public ImageInfoCell (IntPtr handle) : base (handle)
		{
		}

		public static ImageInfoCell Create ()
		{
			return (ImageInfoCell)Nib.Instantiate (null, null) [0];
		}

		public void ConformToRecord (PhotoRecord thePhoto)
		{
			var df = new NSDateFormatter ();
			df.DateStyle = NSDateFormatterStyle.Medium;
			df.TimeStyle = NSDateFormatterStyle.Medium;

			DateTitle.Text = df.StringFor (DateTimeToNSDate(thePhoto.received));

			string thumbnailURL = thePhoto.imageUrl + "=320-c";
			MainImage.SetImage(new NSUrl(thumbnailURL), UIImage.FromBundle("placeholder"));

			if ((thePhoto.tosserid != 0) && (thePhoto.tosserid != PhotoTossRest.Instance.CurrentUser.id)) {
				PersonImage.Hidden = false;
				PersonImage.Layer.CornerRadius = PersonImage.Bounds.Width / 2;
				PersonImage.Layer.MasksToBounds = true;
				PersonImage.SetImage (new NSUrl(PhotoTossRest.Instance.GetUserProfileImage (thePhoto.tossername)), UIImage.FromBundle ("unknownperson"));
			} else {
				PersonImage.Hidden = true;
			}
			CGRect boundsRect = this.Bounds;
			boundsRect.Height = 320 + 32 + 8;
			this.Bounds = boundsRect;
		}


		public static NSDate DateTimeToNSDate(DateTime date) 
		{ 
			DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime(2001, 1, 1, 0, 0, 0) ); 
			return NSDate.FromTimeIntervalSinceReferenceDate( (date - reference).TotalSeconds); 
		}

	}
}

