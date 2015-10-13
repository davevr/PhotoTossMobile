
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;
using UIKit;


namespace PhotoToss.iOSApp
{
	public partial class ChatHistoryCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("ChatHistoryCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("ChatHistoryCell");
		private NSLayoutConstraint ChatConstraint;

		public ChatHistoryCell (IntPtr handle) : base (handle)
		{
		}

		public static ChatHistoryCell Create ()
		{
			return (ChatHistoryCell)Nib.Instantiate (null, null) [0];
		}

		public void ConformToRecord (ChatTurn curItem, int index)
		{
			bool	showImage = true;

			MyImage.Hidden = true;
			OtherPersonImage.Hidden = true;

			if (curItem.sameUser) {
				showImage = false;
			} 

			if (ChatConstraint != null) {
				RemoveConstraint(ChatConstraint);
				ChatConstraint = null;
			}
				
		
			if (curItem.userid == PhotoTossRest.Instance.CurrentUser.id) {
				// current user - show to the reight
				ChatTurnLabel.TextAlignment = UITextAlignment.Right;
				ChatTurnLabel.BackgroundColor = UIColor.FromRGB (1,124,112);
				ChatConstraint = NSLayoutConstraint.Create (this, NSLayoutAttribute.Trailing,
					NSLayoutRelation.Equal, ChatTurnLabel, NSLayoutAttribute.Trailing, 1, 64);
				this.AddConstraint (ChatConstraint);
		
				if (showImage) {
					MyImage.Hidden = false;
					MyImage.SetImage (new NSUrl (curItem.userimage), UIImage.FromBundle ("unknownperson"));
				}
			} else {
				// some other user - show to the left
				ChatTurnLabel.TextAlignment = UITextAlignment.Left;
				ChatTurnLabel.BackgroundColor = UIColor.FromRGB (213, 88, 2);
				ChatConstraint = NSLayoutConstraint.Create (ChatTurnLabel, NSLayoutAttribute.Leading,
					NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 64);
				this.AddConstraint (ChatConstraint);

				if (showImage) {
					OtherPersonImage.Hidden = false;
					OtherPersonImage.SetImage (new NSUrl (curItem.userimage), UIImage.FromBundle ("unknownperson"));
				}
			}


			if (!String.IsNullOrEmpty(curItem.image)) {
				// image turn

			} else {
				// text turn
				ChatTurnLabel.Hidden = false;
				ChatTurnLabel.Text = curItem.text;
			}

		}
	}
}

