
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using CoreGraphics;

namespace PhotoToss.iOSApp
{
	public partial class TossInfoCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("TossInfoCell");
		public static readonly UINib Nib;

		public TossRecord tossRec { get; set;}
		public ImageSpreadViewController controller { get; set;}

		static TossInfoCell ()
		{
			Nib = UINib.FromName ("TossInfoCell", NSBundle.MainBundle);
		}

		public TossInfoCell (IntPtr handle) : base (handle)
		{

		}

		public static TossInfoCell Create ()
		{
			TossInfoCell theCell =  (TossInfoCell)Nib.Instantiate (null, null) [0];


			return theCell;
		}


		protected void HandleBtnTouch(object sender, EventArgs e)
		{
			//controller.ExpandTossRecord (tossRec);
		}

		public void ConformToRecord(TossRecord theRec, ImageSpreadViewController theCont)
		{
			tossRec = theRec;
			controller = theCont;
			var df = new NSDateFormatter ();
			df.DateStyle = NSDateFormatterStyle.Medium;
			df.TimeStyle = NSDateFormatterStyle.Medium;
			string dateStr = df.StringFor (DateTimeToNSDate(theRec.shareTime));
			ShowCatchesButton.TouchUpInside -= HandleBtnTouch;
			if (theRec.catchList == null) {
				ShowCatchesButton.Hidden = false;
				ShowCatchesButton.TouchUpInside += HandleBtnTouch;
			}
			else {
				ShowCatchesButton.Hidden = true;
				dateStr += "\n(" + theRec.catchList.Count.ToString() + " catches)";
			}

			TossLabel.Text = dateStr;
		}

		public static NSDate DateTimeToNSDate(DateTime date) 
		{ 
			DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime(2001, 1, 1, 0, 0, 0) ); 
			return NSDate.FromTimeIntervalSinceReferenceDate( (date - reference).TotalSeconds); 
		}

	}
}

