
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using PubNubMessaging.Core;
using System.Collections.Generic;

namespace PhotoToss.iOSApp
{
	public partial class ImageChatViewController : UIViewController
	{
		private long lastSpeaker = 0;
		private List<ChatTurn> turnList = new List<ChatTurn>();

		public ImageChatViewController () : base ("ImageChatViewController", null)
		{
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
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		private void SubmitTextTurn()
		{
			/*
			InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
			imm.HideSoftInputFromWindow(turnTextField.WindowToken, 0);

			string turnText = turnTextField.Text;
			if (!string.IsNullOrEmpty(turnText)) {
				PublishMessage(turnText);

				turnTextField.Text = "";
			}	
*/
		}

		public void ShowTurn(ChatTurn theTurn)
		{
			theTurn.sameUser = (theTurn.userid == lastSpeaker);
			lastSpeaker = theTurn.userid;
			turnList.Add (theTurn);
			RefreshListView ();
		}

		private void RefreshListView()
		{
			/*
			if (this.View != null) {
				Activity.RunOnUiThread (() => {
					adapter.NotifyDataSetChanged ();
					chatHistoryView.InvalidateViews ();
					chatHistoryView.SmoothScrollToPosition (turnList.Count - 1);
				});
			}
			*/
		}

		public override void ViewDidAppear (bool animated)
		{
			this.TabBarItem.BadgeValue = null;
			base.ViewDidAppear (animated);
		}

		public void InsertHistory(List<ChatTurn> historyList)
		{
			lastSpeaker = 0;
			foreach (ChatTurn curTurn in historyList) {
				curTurn.sameUser = (curTurn.userid == lastSpeaker);
				lastSpeaker = curTurn.userid;
			}

			turnList = historyList;
			if (this.View != null) {
				//adapter.allItems = turnList;
				RefreshListView ();
			}
		}

		public void PublishMessage(string message)
		{
			ChatTurn turn = new ChatTurn ();
			turn.text = message;
			turn.image = null;
			turn.userid = PhotoTossRest.Instance.CurrentUser.id;
			turn.userimage = PhotoTossRest.Instance.GetUserProfileImage (PhotoTossRest.Instance.CurrentUser.username);

			AppDelegate.pubnub.Publish<ChatTurn>(ImageViewController.ChannelName, turn, DisplayPublishReturnMessage, DisplayErrorMessage);
		}

		public void PublishImage(string imageUrl)
		{
			ChatTurn turn = new ChatTurn ();
			turn.text = null;
			turn.image = imageUrl;
			turn.userid = PhotoTossRest.Instance.CurrentUser.id;
			turn.userimage = PhotoTossRest.Instance.GetUserProfileImage (PhotoTossRest.Instance.CurrentUser.username);

			AppDelegate.pubnub.Publish<ChatTurn>(ImageViewController.ChannelName, turn, DisplayPublishReturnMessage, DisplayErrorMessage);
		}

		public void UpdateCount(int newCount)
		{
			InvokeOnMainThread (() => {

				//statusText.Text = string.Format("{0} tossers in chat", newCount);
			});
		}

		private void DisplayPublishReturnMessage(ChatTurn theMsg)
		{
			Console.WriteLine ("[pubnub] publish: " + theMsg);
		}

		private void DisplayErrorMessage(PubnubClientError pubnubError)
		{
			Console.WriteLine ("[pubnub] Error: " + pubnubError.Message);
		}
	}
}

