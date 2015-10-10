
using System;

using Foundation;
using UIKit;

namespace PhotoToss.iOSApp
{
	public partial class ImageLineageCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("ImageLineageCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("ImageLineageCell");

		public ImageLineageCell (IntPtr handle) : base (handle)
		{
		}

		public static ImageLineageCell Create ()
		{
			return (ImageLineageCell)Nib.Instantiate (null, null) [0];
		}
	}
}

