
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;

namespace PhotoToss.iOSApp
{
	public partial class ImageViewController : UITabBarController
	{
		public delegate void ImageDeletedDelegate(PhotoRecord theImage);
		public event ImageDeletedDelegate ImageDeleted;

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
			UIBarButtonItem deleteBtn = new UIBarButtonItem (UIImage.FromBundle ("DeleteIcon"), UIBarButtonItemStyle.Plain, (sender, e) => {
				// to do - delete the item
				var alert = UIAlertController.Create ("Remove Photo", "How do you want to remove this photo?", UIAlertControllerStyle.Alert);

				alert.AddAction (UIAlertAction.Create ("just remove mine", UIAlertActionStyle.Destructive, action => 
					{
						this.RemoveImage();
					}));
				alert.AddAction (UIAlertAction.Create ("all tosses as well", UIAlertActionStyle.Destructive, action => 
					{
						this.RemoveAllTosses();
					}));
				alert.AddAction (UIAlertAction.Create("cancel", UIAlertActionStyle.Cancel, null));
				PresentViewController (alert, animated: true, completionHandler: null);

			});

			UIBarButtonItem shareBtn = new UIBarButtonItem (UIBarButtonSystemItem.Action, (sender, eventArg) => 
				{
					NSUrl newURL = new NSUrl(string.Format("http://phototoss-server-01.appspot.com/image/{0}", HomeViewController.CurrentPhotoRecord.id));
					var activityViewController = new UIActivityViewController(new NSObject[] {newURL, tab1.CurrentImage }, null)
					{

					};
					PresentViewController(activityViewController, true, null);
				});



			//NavigationItem.RightBarButtonItem = tossBtn;
			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] {shareBtn, deleteBtn, tossBtn};

		}


		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public void RemoveImage()
		{
			PhotoRecord deadImage = HomeViewController.CurrentPhotoRecord;
			PhotoTossRest.Instance.RemoveImage (HomeViewController.CurrentPhotoRecord.id, false, (theResult) => {
				HomeViewController.CurrentPhotoRecord = null;
				InvokeOnMainThread(() => {
					NavigationController.PopViewController (true);
					if (ImageDeleted != null)
						ImageDeleted(deadImage);
				});
			});
		}

		public void RemoveAllTosses()
		{
			PhotoRecord deadImage = HomeViewController.CurrentPhotoRecord;
			PhotoTossRest.Instance.RemoveImage(HomeViewController.CurrentPhotoRecord.id, true, (theResult) => {
				HomeViewController.CurrentPhotoRecord = null;
				InvokeOnMainThread(() => {
					NavigationController.PopViewController (true);
					if (ImageDeleted != null)
						ImageDeleted(deadImage);
				});

			});
		}

	}
}

