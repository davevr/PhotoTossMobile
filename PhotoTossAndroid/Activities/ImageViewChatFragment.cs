
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using PhotoToss.Core;
using ServiceStack.Text;
using PubNubMessaging.Core;
using Android.Views.InputMethods;


namespace PhotoToss.AndroidApp
{
	public class ImageViewChatFragment : Android.Support.V4.App.Fragment
	{
		private long lastSpeaker = 0;
		private List<ChatTurn> turnList = new List<ChatTurn>();
		private TextView statusText;
		private EditText turnTextField;
		private ListView chatHistoryView;
		private Button sendTurnBtn;
		private ChatHistoryAdapter adapter;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View fragment = inflater.Inflate(Resource.Layout.ImageViewChatFragment, null);


			statusText = fragment.FindViewById<TextView> (Resource.Id.statusText);
			turnTextField = fragment.FindViewById<EditText> (Resource.Id.turnText);
			chatHistoryView = fragment.FindViewById<ListView> (Resource.Id.chatHistory);
			sendTurnBtn = fragment.FindViewById<Button> (Resource.Id.sendTurnBtn);
			adapter = new ChatHistoryAdapter (this.Activity, turnList);
			chatHistoryView.Adapter = adapter;
			chatHistoryView.DividerHeight = 0;// disable dividers
			//turnTextField.SetImeActionLabel("send", Android.Views.InputMethods.ImeAction.Done);
			turnTextField.EditorAction += (object sender, TextView.EditorActionEventArgs e) => 
			{
				if (true) {//(e.ActionId == Android.Views.InputMethods.ImeAction.Done) {
					SubmitTextTurn();
				}
			};

			sendTurnBtn.Click += (object sender, EventArgs e) => 
			{
				SubmitTextTurn();
			};

			RefreshListView ();
			return fragment;
		}

		public void Update()
		{

		}

		private void SubmitTextTurn()
		{
			InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
			imm.HideSoftInputFromWindow(turnTextField.WindowToken, 0);

			string turnText = turnTextField.Text;
			if (!string.IsNullOrEmpty(turnText)) {
				PublishMessage(turnText);

				turnTextField.Text = "";
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
			if (this.View != null) {
				Activity.RunOnUiThread (() => {
					adapter.NotifyDataSetChanged ();
					chatHistoryView.InvalidateViews ();
					chatHistoryView.SmoothScrollToPosition (turnList.Count - 1);
				});
			}
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
				adapter.allItems = turnList;
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

			MainActivity.pubnub.Publish<ChatTurn>(ImageViewActivity.ChannelName, turn, DisplayPublishReturnMessage, DisplayErrorMessage);
		}

		public void PublishImage(string imageUrl)
		{
			ChatTurn turn = new ChatTurn ();
			turn.text = null;
			turn.image = imageUrl;
			turn.userid = PhotoTossRest.Instance.CurrentUser.id;
			turn.userimage = PhotoTossRest.Instance.GetUserProfileImage (PhotoTossRest.Instance.CurrentUser.username);

			MainActivity.pubnub.Publish<ChatTurn>(ImageViewActivity.ChannelName, turn, DisplayPublishReturnMessage, DisplayErrorMessage);
		}

		public void UpdateCount(int newCount)
		{
			Activity.RunOnUiThread (() => {

				statusText.Text = string.Format("{0} tossers in chat", newCount);
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

	public class ChatHistoryAdapter : BaseAdapter<ChatTurn> {
		public List<ChatTurn>	allItems;
		Activity context;

		public ChatHistoryAdapter(Activity context, List<ChatTurn> theItems) : base() {
			this.context = context;
			this.allItems = theItems;
		}
		public override long GetItemId(int position)
		{
			return position;
		}
		public override ChatTurn this[int position] {  
			get { return allItems[position]; }
		}
		public override int Count {
			get { return allItems.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) {
				view = context.LayoutInflater.Inflate (Resource.Layout.ChatHistoryCell, null);
			}
			var imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			var chatView = view.FindViewById<TextView> (Resource.Id.chatTextView);
			var userImageView = view.FindViewById<ImageView> (Resource.Id.userImageView);
			var ownImageView = view.FindViewById<ImageView> (Resource.Id.ownImageView);

			ChatTurn curItem = allItems [position];
			bool	showImage = true;

			ownImageView.Visibility = ViewStates.Gone;
			userImageView.Visibility = ViewStates.Gone;

			if (curItem.sameUser) {
				showImage = false;
			} 

			if (curItem.userid == PhotoTossRest.Instance.CurrentUser.id) {
				// current user - show to the reight
				chatView.TextAlignment = TextAlignment.TextEnd;
				chatView.Gravity = GravityFlags.Right;
				if (showImage) {
					ownImageView.Visibility = ViewStates.Visible;
					Koush.UrlImageViewHelper.SetUrlDrawable (ownImageView, curItem.userimage, Resource.Drawable.unknown_octopus);
				}
			} else {
				// some other user - show to the left
				chatView.TextAlignment = TextAlignment.TextStart;
				chatView.Gravity = GravityFlags.Left;
				if (showImage) {
					userImageView.Visibility = ViewStates.Visible;
					Koush.UrlImageViewHelper.SetUrlDrawable (userImageView, curItem.userimage, Resource.Drawable.unknown_octopus);
				}
			}


			if (!String.IsNullOrEmpty(curItem.image)) {
				// image turn
				imageView.Visibility = ViewStates.Visible;
				Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curItem.image + "=s256", Resource.Drawable.ic_camera);
				chatView.Visibility = ViewStates.Gone;
			} else {
				// text turn
				imageView.Visibility = ViewStates.Gone;
				imageView.SetImageBitmap (null);
				chatView.Visibility = ViewStates.Visible;
				chatView.Text = curItem.text;
			}


			return view;
		}
	}
}

