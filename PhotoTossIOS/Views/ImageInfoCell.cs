
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
		private ImageSpreadViewController controller;
		private PhotoRecord photoRecord;

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

		protected void HandleBtnTouch(object sender, EventArgs e)
		{
			controller.ExpandImageRecord (photoRecord);
		}

		public void ConformToRecord (PhotoRecord thePhoto, ImageSpreadViewController theCont, bool expandable)
		{
			photoRecord = thePhoto;
			controller = theCont;

			var df = new NSDateFormatter ();
			df.DateStyle = NSDateFormatterStyle.Medium;
			df.TimeStyle = NSDateFormatterStyle.Medium;

			string catchURL;
			string tosserName;
			string dateStr = "", tossStr = "";

			if (thePhoto.tossid == 0) {
				catchURL = thePhoto.imageUrl;
				tosserName = thePhoto.ownername; 
				dateStr = "Original taken " + df.StringFor (DateTimeToNSDate(thePhoto.created));

				TypeIcon.Image = UIImage.FromBundle ("CameraIcon");
			} else {
				catchURL = thePhoto.catchUrl;
				dateStr = "Caught " + df.StringFor (DateTimeToNSDate(thePhoto.received));

				tosserName = thePhoto.tossername;
				TypeIcon.Image = UIImage.FromBundle ("CatchIcon");
			}

			string thumbnailURL = catchURL + "=s320-c";
			MainImage.SetImage(new NSUrl(thumbnailURL), UIImage.FromBundle("placeholder"));
			ShowTossesBtn.TouchUpInside -= HandleBtnTouch;

			if (expandable) {
				TossCountLabel.Hidden = false;
				if (photoRecord.tossList == null) {
					ShowTossesBtn.Hidden = false;
					ShowTossesBtn.TouchUpInside += HandleBtnTouch;
					tossStr = "";//thePhoto.tossCount.ToString () + " toss(es)";
				}
				else {
					ShowTossesBtn.Hidden = true;
					tossStr = photoRecord.tossList.Count.ToString() + " toss(es)";
				}
			} else {
				ShowTossesBtn.Hidden = true;
				TossCountLabel.Hidden = true;
			}


			if ((thePhoto.tosserid != 0) && (thePhoto.tosserid != PhotoTossRest.Instance.CurrentUser.id)) {
				PersonImage.Hidden = false;
				PersonImage.Layer.CornerRadius = PersonImage.Bounds.Width / 2;
				PersonImage.Layer.MasksToBounds = true;
				PersonImage.SetImage (new NSUrl(PhotoTossRest.Instance.GetUserProfileImage (tosserName)), UIImage.FromBundle ("unknownperson"));
			} else {
				PersonImage.Hidden = true;
			}

			DateTitle.Text = dateStr;
			TossCountLabel.Text = tossStr;

			CGRect boundsRect = this.Bounds;
			boundsRect.Height = 370;
			this.Bounds = boundsRect;
		}


		public static NSDate DateTimeToNSDate(DateTime date) 
		{ 
			DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime(2001, 1, 1, 0, 0, 0) ); 
			return NSDate.FromTimeIntervalSinceReferenceDate( (date - reference).TotalSeconds); 
		}

	}
}

