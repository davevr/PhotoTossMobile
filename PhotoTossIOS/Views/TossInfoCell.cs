
using System;

using Foundation;
using UIKit;

namespace PhotoToss.iOSApp
{
	public partial class TossInfoCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("TossInfoCell");
		public static readonly UINib Nib;

		static TossInfoCell ()
		{
			Nib = UINib.FromName ("TossInfoCell", NSBundle.MainBundle);
		}

		public TossInfoCell (IntPtr handle) : base (handle)
		{
		}

		public static TossInfoCell Create ()
		{
			return (TossInfoCell)Nib.Instantiate (null, null) [0];
		}
	}
}

