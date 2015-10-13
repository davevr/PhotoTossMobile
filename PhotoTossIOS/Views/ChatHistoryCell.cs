
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;

namespace PhotoToss.iOSApp
{
	public partial class ChatHistoryCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("ChatHistoryCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("ChatHistoryCell");

		public ChatHistoryCell (IntPtr handle) : base (handle)
		{
		}

		public static ChatHistoryCell Create ()
		{
			return (ChatHistoryCell)Nib.Instantiate (null, null) [0];
		}

		public void ConformToRecord (ChatTurn curItem, int index)
		{

		}
	}
}

