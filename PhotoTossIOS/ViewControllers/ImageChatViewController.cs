
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using PubNubMessaging.Core;
using System.Collections.Generic;
using CoreGraphics;

namespace PhotoToss.iOSApp
{
	public partial class ImageChatViewController : UIViewController
	{
		private long lastSpeaker = 0;
		private List<ChatTurn> turnList = new List<ChatTurn>();
		ChatHistoryDataSource dataSource;
		private int lastCount = 1;
		private UIView activeView;
		private nfloat scroll_amount = 0.0f;    // amount to scroll 
		private nfloat bottom = 0.0f;           // bottom point
		private nfloat offset = 10.0f;          // extra offset
		private bool moveViewUp = false;  
		private NSObject hideObserver, showObserver;

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
			ChatHistoryTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			ChatHistoryTableView.ReloadData ();
			UpdateCount (lastCount);
			SendBtn.TouchUpInside += (object sender, EventArgs e) => {
				SubmitTextTurn ();
			};

			ChatTurnField.ShouldReturn += (theField) => {
				theField.ResignFirstResponder();
				SubmitTextTurn();
				return false;
			};

			NoChatMessage.Layer.CornerRadius = 15;

			showObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, KeyboardUpNotify);
			hideObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidHideNotification, KeyboardDownNotify);

		}

		public override void ViewDidUnload ()
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(hideObserver);
			NSNotificationCenter.DefaultCenter.RemoveObserver(showObserver);
			base.ViewDidUnload ();
		}

		private void KeyboardUpNotify(NSNotification notification)
		{
			// get the keyboard size
			CGRect r = UIKeyboard.BoundsFromNotification (notification);

			// Find what opened the keyboard
			foreach (UIView view in this.View.Subviews) {
				if (view.IsFirstResponder)
					activeView = view;
			}

			// Bottom of the controller = initial position + height + offset      
			bottom = (activeView.Frame.Y + activeView.Frame.Height + offset);

			// Calculate how far we need to scroll
			scroll_amount = (r.Height - (View.Frame.Size.Height - bottom)) ;

			// Perform the scrolling
			if (scroll_amount > 0) {
				moveViewUp = true;
				ScrollTheView (moveViewUp);
			} else {
				moveViewUp = false;
			}

		}

		private void KeyboardDownNotify(NSNotification notification)
		{
			if(moveViewUp){ScrollTheView(false);}
		}


		private void ScrollTheView(bool move)
		{

			// scroll the view up or down
			UIView.BeginAnimations (string.Empty, System.IntPtr.Zero);
			UIView.SetAnimationDuration (0.3);

			CGRect frame = View.Frame;

			if (move) {
				frame.Y -= scroll_amount;
			} else {
				frame.Y += scroll_amount;
				scroll_amount = 0;
			}

			View.Frame = frame;
			UIView.CommitAnimations();
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		private void SubmitTextTurn()
		{
			string turnText = ChatTurnField.Text;
			if (!string.IsNullOrEmpty(turnText)) {
				SendBtn.Enabled = false;
				ChatTurnField.Enabled = false;
				PublishMessage(turnText);

				ChatTurnField.Text = "";
				SendBtn.Enabled = true;
				ChatTurnField.Enabled = true;
				NoChatMessage.Hidden = true;
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
					if (turnList.Count > 0) {
						NoChatMessage.Hidden = true;
						ChatHistoryTableView.ScrollToRow (NSIndexPath.FromRowSection (turnList.Count - 1, 0), UITableViewScrollPosition.None, true);
					}
				});
			}

		}

		private void ScrollToEnd()
		{
			InvokeOnMainThread (() => {
				if (turnList.Count > 0)
					ChatHistoryTableView.ScrollToRow (NSIndexPath.FromRowSection (turnList.Count - 1, 0), UITableViewScrollPosition.None, true);
			});
		}

		public override void ViewDidAppear (bool animated)
		{
			this.TabBarItem.BadgeValue = null;
			if (turnList.Count > 0) {
				NoChatMessage.Hidden = true;
			} else {
				NoChatMessage.Hidden = false;
			}

			base.ViewDidAppear (animated);
			ScrollToEnd ();
		}

		public void InsertHistory(List<ChatTurn> historyList)
		{
			lastSpeaker = 0;
			foreach (ChatTurn curTurn in historyList) {
				curTurn.sameUser = (curTurn.userid == lastSpeaker);
				lastSpeaker = curTurn.userid;
			}

			turnList = historyList;
			InvokeOnMainThread (() => {
				if (this.dataSource != null) {
					dataSource.chatList = turnList;
					RefreshListView ();
				}
			});
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
			lastCount = newCount;
			if (ChatCountLabel != null) {
				InvokeOnMainThread (() => {
					ChatCountLabel.Text = string.Format ("{0} tossers in chat", newCount);
				});
			}
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

