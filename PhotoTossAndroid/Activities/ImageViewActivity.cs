
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


namespace PhotoToss.AndroidApp
{
	[Activity(Theme = "@style/AppSubTheme", ScreenOrientation = ScreenOrientation.Portrait)]	
	public class ImageViewActivity : Android.Support.V7.App.AppCompatActivity, ViewPager.IOnPageChangeListener
	{
		private Android.Support.V7.Widget.Toolbar toolbar = null;
		public static ImageViewDetailFragment detailFragment;
		public static ImageViewSpreadFragment spreadFragment;
		public static ImageViewStatsFragment statsFragment;

		private PagerSlidingTabStrip tabs;

		private ViewPager pager;

		public class ImageViewPageAdapter : FragmentPagerAdapter
		{
			private string[] Titles = { "Image", "Spread", "Stats" };
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
					theItem = ImageViewActivity.statsFragment;
					break;

				}
				return theItem;
			}
		}


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.ImageViewActivity);

			toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tool_bar);
			SetSupportActionBar(toolbar);

			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(false);
			SupportActionBar.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable( Resources.GetColor(Resource.Color.PT_light_teal)));


			this.Title =  "Image Details";


			detailFragment = new ImageViewDetailFragment();
			spreadFragment = new ImageViewSpreadFragment();
			statsFragment = new ImageViewStatsFragment();

			pager = FindViewById<ViewPager>(Resource.Id.post_pager);
			pager.Adapter = new ImageViewPageAdapter(this.SupportFragmentManager, this);

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
				break;

			case 1:
				//Spread
				break;

			case 2:
				// Stats
				break;

			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.ImageDetailMenu, menu);


			return true;

		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Resource.Id.TossButton:
				this.StartActivity(typeof(TossActivity));
				break;
			case Resource.Id.deleteBtn:
				Toast.MakeText (this, "Here is where delete would go...", ToastLength.Long).Show ();
				break;
			case 16908332:// the back button apparently...
				{
					Finish();
				}
				break;

			}
			return base.OnOptionsItemSelected(item);
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
		}
	}
}

