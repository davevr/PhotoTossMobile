
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.App;
using Android.Support.V4.View;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using com.refractored;
using Android.Text;
using Android.Text.Style;
using Android.Content.PM;

using PhotoToss.Core;
using PubNubMessaging.Core;
using ServiceStack.Text;


namespace PhotoToss.AndroidApp
{
	[Activity(Theme = "@style/AppSubTheme", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustPan)]	
	public class ImageViewActivity : Android.Support.V7.App.AppCompatActivity, ViewPager.IOnPageChangeListener
	{
		private Android.Support.V7.Widget.Toolbar toolbar = null;
		public static ImageViewDetailFragment detailFragment;
		public static ImageViewSpreadFragment spreadFragment;
		public static ImageViewStatsFragment statsFragment;
		public static ImageViewChatFragment chatFragment;
		public static int NewMessageCount {get; set;}
		public static string ChannelName {get; set;}


		private Android.Support.V7.Widget.ShareActionProvider actionProvider = null;

		private PagerSlidingTabStrip tabs;

		private ViewPager pager;

		public class ImageViewPageAdapter : FragmentPagerAdapter, ICustomTabProvider
		{
			private string[] Titles = { "Image", "Spread", "Chat" ,"Stats", };
			Android.Support.V7.App.AppCompatActivity activity;

			public ImageViewPageAdapter(Android.Support.V4.App.FragmentManager fm, Android.Support.V7.App.AppCompatActivity theActivity)
				: base(fm)
			{
				activity = theActivity;
			}

			public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
			{
				return new Java.Lang.String(Titles[position]);
			}

			public override int Count
			{
				get
				{
					return Titles.Length;
				}
			}

			 

			public View GetCustomTabView (ViewGroup parent, int position)
			{
				var tabView = activity.LayoutInflater.Inflate (Resource.Layout.NotifyTabView, null);

				var counter = tabView.FindViewById<TextView> (Resource.Id.counter);

				if ((position != 2) || (ImageViewActivity.NewMessageCount == 0))
					counter.Visibility = ViewStates.Gone;
				else
					counter.Text = ImageViewActivity.NewMessageCount.ToString ();

				return tabView;
			}

			public override Android.Support.V4.App.Fragment GetItem(int position)
			{
				Android.Support.V4.App.Fragment theItem = null;
				switch (position)
				{
				case 0:
					theItem = ImageViewActivity.detailFragment;
					break;

				case 1:
					theItem = ImageViewActivity.spreadFragment;
					break;

				case 2:
					theItem = ImageViewActivity.chatFragment;
					break;

				case 3:
					theItem = ImageViewActivity.statsFragment;
					break;




				}
				return theItem;
			}
		}


		protected override void OnCreate (Bundle bundle)
		{
			//Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
			NewMessageCount = 0;

            base.OnCreate (bundle);

			SetContentView (Resource.Layout.ImageViewActivity);

			toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tool_bar);
			SetSupportActionBar(toolbar);

			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(false);
			SupportActionBar.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable( Resources.GetColor(Resource.Color.PT_light_teal)));


			this.Title =  "Photo";


			detailFragment = new ImageViewDetailFragment();
			spreadFragment = new ImageViewSpreadFragment();
			statsFragment = new ImageViewStatsFragment();
			chatFragment = new ImageViewChatFragment ();

			pager = FindViewById<ViewPager>(Resource.Id.post_pager);
			pager.Adapter = new ImageViewPageAdapter(this.SupportFragmentManager, this);
			pager.AddOnPageChangeListener (this);
			tabs = FindViewById<PagerSlidingTabStrip>(Resource.Id.tabs);
			tabs.SetViewPager(pager);
			tabs.IndicatorColor = Resources.GetColor(Resource.Color.PT_light_orange);
			//tabs.TabTextColor = Resources.GetColorStateList(Resource.Color.tabtextcolor);
			tabs.TabTextColorSelected = Resources.GetColorStateList(Resource.Color.PT_white);
			tabs.IndicatorHeight = Resources.GetDimensionPixelSize(Resource.Dimension.tab_indicator_height);
			tabs.UnderlineColor = Resources.GetColor(Resource.Color.PT_dark_orange);
			tabs.TabPaddingLeftRight = Resources.GetDimensionPixelSize(Resource.Dimension.tab_padding);
			tabs.OnPageChangeListener = this;
			//tabs.ShouldExpand = true;

			tabs.SetTabTextColor(Color.White);


			int page = Intent.GetIntExtra("Page", 0);

			pager.CurrentItem = page;

		}


		protected override void OnStart ()
		{
			base.OnStart ();
			SubscribeToImageChannel ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			SubscribeToImageChannel ();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			UnsubscribeFromImageChannel ();
		}

		protected override void OnStop ()
		{
			base.OnStop ();
			UnsubscribeFromImageChannel ();
		}



		public void SubscribeToImageChannel()
		{
			long imageId = PhotoTossRest.Instance.CurrentImage.originid;
			if (imageId == 0)
				imageId = PhotoTossRest.Instance.CurrentImage.id;
			ChannelName = "image" + imageId.ToString ();
			MainActivity.pubnub.Subscribe<string>(ChannelName, DisplaySubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplayErrorMessage);
			MainActivity.pubnub.Presence<string>(ChannelName, DisplayPresenceReturnMessage, DisplayPresenceConnectStatusMessage, DisplayErrorMessage);
			GetHistory ();
		}



		public void GetHistory()
		{
			MainActivity.pubnub.DetailedHistory<string>(ChannelName, 20, HistoryReturnMessage, DisplayErrorMessage);


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
				if (historyList != null) {
					chatFragment.InsertHistory(historyList);
					ClearMessageCount();
				}
			} catch (Exception exp) {
				Console.WriteLine ("[pubnub] history parse error: " + theMsg);
			}
			MainActivity.pubnub.HereNow<string>(ChannelName, DisplayPresenceReturnMessage, DisplayErrorMessage);

		}

		private void DisplaySubscribeReturnMessage(string theMsg)
		{
			try {
				string	jsonMsg = theMsg.Substring (theMsg.IndexOf ("{"), theMsg.IndexOf ("}"));
				//string reparse = jsonMsg.FromJson<string>();
				ChatTurn theTurn = jsonMsg.FromJson<ChatTurn> ();

				if ((theTurn != null) && (chatFragment != null)) {
					chatFragment.ShowTurn(theTurn);
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
				if ((msg != null) && (chatFragment != null))
					chatFragment.UpdateCount(msg.occupancy);
					
			}
			catch (Exception exp) {
				Console.WriteLine ("[pubnub] presence err: " + theMsg);
			}
		}

		public void IncrementMessageCount(int numMessages)
		{
			if (pager.CurrentItem != 2) {
				NewMessageCount += numMessages;
				UpdateMessageCountIndicator();
			} else
				NewMessageCount = 0;
		}

		public void ClearMessageCount()
		{
			NewMessageCount = 0;
			UpdateMessageCountIndicator ();
		}

		private void UpdateMessageCountIndicator()
		{
			LinearLayout tabHolder = tabs.GetChildAt(0) as LinearLayout;
			LinearLayout tab = tabHolder.GetChildAt (2) as LinearLayout;
			if (tab != null) {
				RunOnUiThread (() => {
					
					var counter = tab.FindViewById<TextView>(Resource.Id.counter);
					if (NewMessageCount == 0)
						counter.Visibility = ViewStates.Gone;
					else {
						counter.Visibility = ViewStates.Visible;
						counter.Text = NewMessageCount.ToString();
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

			MainActivity.pubnub.Unsubscribe<string> (ChannelName, DisplayUnsubscribeReturnMessage, DisplaySubscribeConnectStatusMessage, DisplaySubscribeDisconnectStatusMessage, DisplayErrorMessage);
			MainActivity.pubnub.PresenceUnsubscribe<string>(ChannelName, DisplayUnsubscribeReturnMessage, DisplayPresenceConnectStatusMessage, DisplayPresenceDisconnectStatusMessage, DisplayErrorMessage);

		}

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{

		}

		public void OnPageScrollStateChanged(int state)
		{

		}

		public void OnPageSelected(int position)
		{
			switch (position)
			{
			case 0:
				// Images
				detailFragment.Update();
				break;

			case 1:
				//Spread
				spreadFragment.Update();
				break;

			case 2:
				// Stats
				chatFragment.Update ();
				ClearMessageCount ();
				break;

			case 3:
				// Stats
				statsFragment.Update();
				break;


			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.ImageDetailMenu, menu);


			return true;

		}

		public override bool OnPrepareOptionsMenu (IMenu menu)
		{
			if (actionProvider == null)
			{
				PhotoRecord curImage = PhotoTossRest.Instance.CurrentImage;
				string blahURL = curImage.ShareURL;
				var shareItem = menu.FindItem(Resource.Id.shareBtn);
				var nativeAction = MenuItemCompat.GetActionProvider(shareItem);
				actionProvider = nativeAction.JavaCast<Android.Support.V7.Widget.ShareActionProvider>();
				var intent = new Intent(Intent.ActionSend);
				intent.SetType("text/plain");
				intent.AddFlags(ActivityFlags.ClearWhenTaskReset);
				intent.PutExtra(Intent.ExtraTitle, "Shared from PhotoToss");
				intent.PutExtra(Intent.ExtraText, blahURL);
				actionProvider.SetShareIntent(intent);

			}

			return base.OnPrepareOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Resource.Id.TossButton:
				this.StartActivity(typeof(TossActivity));
				break;
			case Resource.Id.delete_mine:
				RemovePhotoFromMe ();
				break;
			case Resource.Id.delete_all:
				RemovePhotoFromAll ();
				break;
			case 16908332:// the back button apparently...
				{
					Finish();
				}
				break;

			}
			return base.OnOptionsItemSelected(item);
		}

		private void RemovePhotoFromMe()
		{
			PhotoTossRest.Instance.RemoveImage (PhotoTossRest.Instance.CurrentImage.id, false, (theResult) => {
				Intent myIntent = new Intent (this, typeof(MainActivity));
				myIntent.PutExtra ("dead", PhotoTossRest.Instance.CurrentImage.id);
				SetResult (Result.Ok, myIntent);
				PhotoTossRest.Instance.CurrentImage = null;
				Finish();
			});
		}

		private void RemovePhotoFromAll()
		{
			PhotoTossRest.Instance.RemoveImage (PhotoTossRest.Instance.CurrentImage.id, true, (theResult) => {
				Intent myIntent = new Intent (this, typeof(MainActivity));
				myIntent.PutExtra ("dead", PhotoTossRest.Instance.CurrentImage.id);
				SetResult (Result.Ok, myIntent);
				PhotoTossRest.Instance.CurrentImage = null;
				Finish();
			});
		}

		protected override void OnTitleChanged(Java.Lang.ICharSequence title, Color color)
		{
			this.SupportActionBar.Title = title.ToString();

			SpannableString s = new SpannableString(title);
			s.SetSpan(new TypefaceSpan(this, "RammettoOne-Regular.ttf"), 0, s.Length(), SpanTypes.ExclusiveExclusive);
			s.SetSpan(new ForegroundColorSpan(Resources.GetColor(Resource.Color.PT_dark_orange)), 0, s.Length(), SpanTypes.ExclusiveExclusive);

			this.SupportActionBar.TitleFormatted = s;


		}

		private void btn_right_Click(object sender, EventArgs e)
		{
			/*
			if (detailFragment != null) // that means it is active
			{
				Finish();
				//commentsFragment.triggerCreateBlock();
			}
			if (spreadFragment != null)
			{
				Finish ();
			}
			if (statsFragment != null) // that means it is active
			{
				Finish();
				//commentsFragment.triggerCreateBlock();
			}

			if (chatFragment != null) // that means it is active
			{
				Finish();
				//commentsFragment.triggerCreateBlock();
			}
			*/
			Finish ();
		}
	}
}

