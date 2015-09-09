
using System;

using Foundation;
using UIKit;

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
	}
}

