
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;

namespace PhotoToss.iOSApp
{
	public partial class ImageViewController : UITabBarController
	{
		private ImageDetailController tab1;
		private ImageSpreadViewController tab2;
		private ImageStatsViewController tab3;

		public ImageViewController () : base ("ImageViewController", null)
		{
			tab1 = new ImageDetailController();
			tab1.TabBarItem = new UITabBarItem ("Image", UIImage.FromBundle ("CameraIcon"), 0);

		
			tab2 = new ImageSpreadViewController();
			tab2.TabBarItem = new UITabBarItem ("Spread", UIImage.FromBundle ("SpreadIcon"), 1);
			//tab2.View.BackgroundColor = UIColor.Orange;

			tab3 = new ImageStatsViewController();
			tab3.TabBarItem = new UITabBarItem ("Stats", UIImage.FromBundle ("StatsIcon"), 2);
			tab3.View.BackgroundColor = UIColor.Red;

			var tabs = new UIViewController[] {
				tab1, tab2, tab3
			};

			ViewControllers = tabs;

		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			var tossBtn = new UIBarButtonItem(UIImage.FromBundle("TossIcon"),UIBarButtonItemStyle.Plain,  (sender, e) => {

				TossViewController tossViewer = new TossViewController();
				tossViewer.ModalInPopover = true;
				tossViewer.ModalPresentationStyle = UIModalPresentationStyle.Popover;
				tossViewer.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
				if (tossViewer != null) {
					this.NavigationController.PresentViewController(tossViewer, true, () =>
						{
							// init the controller if needed...

						});
				}
			});

			NavigationItem.RightBarButtonItem = tossBtn;


		}


		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

	}
}

