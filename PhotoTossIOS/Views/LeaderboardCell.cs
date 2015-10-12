
using System;

using Foundation;
using UIKit;
using SDWebImage;
using PhotoToss.Core;

namespace PhotoToss.iOSApp
{
	public partial class LeaderboardCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("LeaderboardCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("LeaderboardCell");
		private PhotoRecord photoRecord;

		public LeaderboardCell (IntPtr handle) : base (handle)
		{
		}

		public static LeaderboardCell Create ()
		{
			return (LeaderboardCell)Nib.Instantiate (null, null) [0];
		}

		public void ConformToRecord (PhotoRecord thePhoto, int index)
		{
			photoRecord = thePhoto;

			IndexLabel.Text = (index + 1).ToString ();
			string thumbnailURL = thePhoto.imageUrl + "=s128-c";
			ImageThumbnail.SetImage(new NSUrl(thumbnailURL), UIImage.FromBundle("placeholder"));
			string statsStr = string.Format ("shared {0} times", thePhoto.totalshares);
			ShareCountLabel.Text = statsStr;
			UserImage.SetImage (new NSUrl(PhotoTossRest.Instance.GetUserProfileImage (thePhoto.ownername)), UIImage.FromBundle ("unknownperson"));

		}
	}
}

