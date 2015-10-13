
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using PubNubMessaging.Core;
using ServiceStack.Text;
using System.Collections.Generic;
using CoreAnimation;

namespace PhotoToss.iOSApp
{
	public partial class ImageViewController : UITabBarController
	{
		public delegate void ImageDeletedDelegate(PhotoRecord theImage);
		public event ImageDeletedDelegate ImageDeleted;

		private ImageDetailController detailTab;
		private ImageSpreadViewController spreadTab;
		private ImageChatViewController chatTab;
		private ImageStatsViewController statsTab;
		public static int NewMessageCount {get; set;}
		public static string ChannelName {get; set;}

		public ImageViewController () : base ("ImageViewController", null)
		{
			detailTab = new ImageDetailController();
			detailTab.TabBarItem = new UITabBarItem ("Image", UIImage.FromBundle ("CameraIcon"), 0);

		
			spreadTab = new ImageSpreadViewController();
			spreadTab.TabBarItem = new UITabBarItem ("Spread", UIImage.FromBundle ("SpreadIcon"), 1);

			chatTab = new ImageChatViewController();
			chatTab.TabBarItem = new UITabBarItem ("Chat", UIImage.FromBundle ("ChatIcon"), 2);

			statsTab = new ImageStatsViewController();
			statsTab.TabBarItem = new UITabBarItem ("Stats", UIImage.FromBundle ("StatsIcon"), 3);

			var tabs = new UIViewController[] {
				detailTab, spreadTab, chatTab, statsTab
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
					NSUrl newURL = new NSUrl(PhotoTossRest.Instance.CurrentImage.ShareURL);
					var activityViewController = new UIActivityViewController(new NSObject[] {newURL }, null)
					{

					};
					PresentViewController(activityViewController, true, null);
				});

			tossBtn.TintColor = UIColor.Black;
			deleteBtn.TintColor = UIColor.Black;
			shareBtn.TintColor = UIColor.Black;
			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] {shareBtn, deleteBtn, tossBtn};
			NewMessageCount = 0;
			SubscribeToImageChannel ();

			this.View.AddGestureRecognizer (new UISwipeGestureRecognizer (() => {
				DoSwipeLeft ();
			}) {
				Direction = UISwipeGestureRecognizerDirection.Left
			});

			this.View.AddGestureRecognizer (new UISwipeGestureRecognizer (() => {
				DoSwipeRight ();
			}) {
				Direction = UISwipeGestureRecognizerDirection.Right
			});
				

		}

		private void DoSwipeLeft()
		{
			if (this.SelectedIndex < 3) {
				this.SelectedIndex++;
				CATransition anim = new CATransition ();
				anim.Type = CATransition.TransitionPush;
				anim.Subtype = CATransition.TransitionFromRight;
				anim.Duration = .5;
				anim.TimingFunction = CAMediaTimingFunction.FromName (CAMediaTimingFunction.EaseIn);
				this.View.Layer.AddAnimation (anim, "fadetransition");
			}
		}

		private void DoSwipeRight()
		{
			if (this.SelectedIndex > 0) {
				this.SelectedIndex--;
				CATransition anim = new CATransition ();
				anim.Type = CATransition.TransitionPush;
				anim.Subtype = CATransition.TransitionFromLeft;
				anim.Duration = .5;
				anim.TimingFunction = CAMediaTimingFunction.FromName (CAMediaTimingFunction.EaseIn);
				this.View.Layer.AddAnimation (anim, "fadetransition");
			}
		}


		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public void RemoveImage()
		{
			PhotoRecord deadImage = PhotoTossRest.Instance.CurrentImage;
			PhotoTossRest.Instance.RemoveImage (PhotoTossRest.Instance.CurrentImage.id, false, (theResult) => {
				PhotoTossRest.Instance.CurrentImage = null;
				InvokeOnMainThread(() => {
					NavigationController.PopViewController (true);
					if (ImageDeleted != null)
						ImageDeleted(deadImage);
				});
			});
		}

		public void RemoveAllTosses()
		{
			PhotoRecord deadImage = PhotoTossRest.Instance.CurrentImage;
			PhotoTossRest.Instance.RemoveImage(PhotoTossRest.Instance.CurrentImage.id, true, (theResult) => {
				PhotoTossRest.Instance.CurrentImage = null;
				InvokeOnMainThread(() => {
					NavigationController.PopViewController (true);
					if (ImageDeleted != null)
						ImageDeleted(deadImage);
				});

			});
		}

		public override void ViewDidUnload ()
		{
			UnsubscribeFromImageChannel ();
			base.ViewDidUnload ();
		}

		public void SubscribeToImageChannel()
		{
			if (PhotoTossRest.Instance.CurrentImage != null) {
				long imageId = PhotoTossRest.Instance.CurrentImage.originid;
				if (imageId == 0)
					imageId = PhotoTossRest.Instance.CurrentImage.id;
				ChannelName = "image" + imageId.ToString ();
				AppDelegate.pubnub.Subscribe<string> (ChannelName, DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);
				AppDelegate.pubnub.Presence<string> (ChannelName, DisplayPresenceReturnMessage, DisplayPresenceConnectStatusMessage, DisplayErrorMessage);
			}
		}



		public void GetHistory()
		{
			AppDelegate.pubnub.DetailedHistory<string>(ChannelName, 20, HistoryReturnMessage, DisplayErrorMessage);


		}

		public override void ViewWillAppear (bool animated)
		{
			GetHistory ();
			base.ViewWillAppear (animated);

		}

		private void HistoryReturnMessage(string theMsg)
		{
			try {
				int startLoc = theMsg.IndexOf("[", 1);
				int endLoc = theMsg.LastIndexOf("]",theMsg.Length - 2);
				int strLen = (endLoc + 1) - startLoc;
				string historyJSON = theMsg.Substring(startLoc, strLen);
				Console.WriteLine ("[pubnub] history: " + historyJSON);
				List<ChatTurn>	historyList = historyJSON.FromJson<List<ChatTurn>>();
				if ((chatTab != null) && (historyList != null)) {
					chatTab.InsertHistory(historyList);
					ClearMessageCount();
				}
			} catch (Exception exp) {
				Console.WriteLine ("[pubnub] history parse error: " + theMsg);
			}
			AppDelegate.pubnub.HereNow<string>(ChannelName, DisplayPresenceReturnMessage, DisplayErrorMessage);

		}

		private void DisplaySubscribeReturnMessage(string theMsg)
		{
			try {
				string	jsonMsg = theMsg.Substring (theMsg.IndexOf ("{"), theMsg.IndexOf ("}"));
				//string reparse = jsonMsg.FromJson<string>();
				ChatTurn theTurn = jsonMsg.FromJson<ChatTurn> ();

				if ((theTurn != null) && (chatTab != null)) {
					chatTab.ShowTurn(theTurn);
					IncrementMessageCount(1);
				}
			}
			catch (Exception exp)
			{
				Console.WriteLine ("[pubnub] subscribe: invalid ChatTurn " + theMsg);
			}
		}

		private void DisplayUnsubscribeReturnMessage(string theMsg)
		{
			Console.WriteLine ("[pubnub] unsubscribe: " + theMsg);
		}


		private void DisplayPresenceReturnMessage(string theMsg)
		{
			try {
				string	jsonMsg = theMsg.Substring (theMsg.IndexOf ("{"), theMsg.IndexOf ("}"));
				PresenceMessage msg = jsonMsg.FromJson<PresenceMessage>();
				if ((msg != null) && (chatTab != null))
					chatTab.UpdateCount(msg.occupancy);

			}
			catch (Exception exp) {
				Console.WriteLine ("[pubnub] presence err: " + theMsg);
			}
		}

		public void IncrementMessageCount(int numMessages)
		{
			InvokeOnMainThread (() => {
				if (this.SelectedIndex != 2) {
					NewMessageCount += numMessages;
					UpdateMessageCountIndicator ();
				} else
					NewMessageCount = 0;
			});
		}

		public void ClearMessageCount()
		{
			NewMessageCount = 0;
			UpdateMessageCountIndicator ();
		}

		private void UpdateMessageCountIndicator()
		{
			if (chatTab != null) {
				InvokeOnMainThread (() => {
					if (NewMessageCount == 0)
						chatTab.TabBarItem.BadgeValue = null;
					else {
						chatTab.TabBarItem.BadgeValue = NewMessageCount.ToString();
					}

				});
			}
		}

		private void DisplayPresenceConnectStatusMessage(string theMsg)
		{
			Console.WriteLine ("[pubnub] presence connect: " + theMsg);
		}

		private void DisplayPresenceDisconnectStatusMessage(string theMsg)
		{
			Console.WriteLine ("[pubnub] presence disconnect: " + theMsg);
		}


		private void DisplaySubscribeConnectStatusMessage(string theMsg)
		{
			Console.WriteLine ("[pubnub] sub connect: " + theMsg);
		}

		private void DisplaySubscribeDisconnectStatusMessage(string theMsg)
		{
			Console.WriteLine ("sub disconnect: " + theMsg);
		}


		private void DisplayErrorMessage(PubnubClientError pubnubError)
		{
			Console.WriteLine ("[pubnub] Error: " + pubnubError.Message);
		}

		public void UnsubscribeFromImageChannel()
		{
			if (PhotoTossRest.Instance.CurrentImage != null) {
				AppDelegate.pubnub.Unsubscribe<string> (ChannelName, DisplayUnsubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplaySubscribeDisconnectStatusMessage, DisplayErrorMessage);
				AppDelegate.pubnub.PresenceUnsubscribe<string> (ChannelName, DisplayUnsubscribeReturnMessage, DisplayPresenceConnectStatusMessage, DisplayPresenceDisconnectStatusMessage, DisplayErrorMessage);
			}

		}

	}
}

