// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace PhotoToss.iOSApp
{
	[Register ("ImageSpreadViewController")]
	partial class ImageSpreadViewController
	{
		[Outlet]
		UIKit.UITableView HistoryTable { get; set; }

		[Outlet]
		MapKit.MKMapView MapView { get; set; }

		[Outlet]
		UIKit.UIButton ShowAllBtn { get; set; }

		[Outlet]
		UIKit.UIButton ShowMeBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (HistoryTable != null) {
				HistoryTable.Dispose ();
				HistoryTable = null;
			}

			if (MapView != null) {
				MapView.Dispose ();
				MapView = null;
			}

			if (ShowMeBtn != null) {
				ShowMeBtn.Dispose ();
				ShowMeBtn = null;
			}

			if (ShowAllBtn != null) {
				ShowAllBtn.Dispose ();
				ShowAllBtn = null;
			}
		}
	}
}
