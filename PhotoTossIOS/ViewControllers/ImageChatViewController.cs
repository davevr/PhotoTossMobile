
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
		ChatHistoryDataSource dataSource;

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
			ChatHistoryTableView.RegisterNibForCellReuse(UINib.FromName(ChatHistoryCell.Key, NSBundle.MainBundle), ChatHistoryCell.Key);
			dataSource = new ChatHistoryDataSource ();
			dataSource.chatList = turnList;
			ChatHistoryTableView.DataSource = dataSource;
			ChatHistoryTableView.RowHeight = UITableView.AutomaticDimension;
			ChatHistoryTableView.EstimatedRowHeight = 58;
			ChatHistoryTableView.ReloadData ();

			SendBtn.TouchUpInside += (object sender, EventArgs e) => {
				SubmitTextTurn ();
			};
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		private void SubmitTextTurn()
		{
			string turnText = ChatTurnField.Text;
			if (!string.IsNullOrEmpty(turnText)) {
				PublishMessage(turnText);

				ChatTurnField.Text = "";
			}	
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
			if (ChatHistoryTableView != null) {
				InvokeOnMainThread (() => {
					ChatHistoryTableView.ReloadData ();
					ChatHistoryTableView.ScrollToRow (NSIndexPath.FromRowSection (turnList.Count - 1, 0), UITableViewScrollPosition.Bottom, true);

				});
			}

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
				dataSource.chatList = turnList;
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

	public class ChatHistoryDataSource : UITableViewDataSource
	{
		public List<ChatTurn> chatList;

		public ChatHistoryDataSource ()
		{
		}
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (ChatHistoryCell)tableView.DequeueReusableCell (new NSString(ChatHistoryCell.Key), indexPath);
			ChatTurn curTurn = chatList [(int)indexPath.Item];

			cell.ConformToRecord (curTurn, indexPath.Row);


			return cell;
		}


		public override nint NumberOfSections (UITableView collectionView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return chatList.Count;
		}



		public ChatTurn GetItem(NSIndexPath indexPath)
		{
			return chatList [(int)indexPath.Item];
		}

	}
}

