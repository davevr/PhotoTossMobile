
using System;

using Foundation;
using UIKit;

namespace PhotoToss.iOSApp
{
	public partial class LeaderboardCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("LeaderboardCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("LeaderboardCell");

		public LeaderboardCell (IntPtr handle) : base (handle)
		{
		}

		public static LeaderboardCell Create ()
		{
			return (LeaderboardCell)Nib.Instantiate (null, null) [0];
		}
	}
}

