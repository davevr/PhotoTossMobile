using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace PhotoToss.AndroidApp
{
	public class NonSwipeViewPager : ViewPager
	{
		private bool enabled;

		public NonSwipeViewPager (Context context, IAttributeSet attrs) : base (context, attrs)
		{
			enabled = true;
		}

		public override bool OnTouchEvent (Android.Views.MotionEvent e)
		{
			if (this.enabled)
				return base.OnTouchEvent (e);
			else
				return false;
		}

		public override bool OnInterceptTouchEvent (Android.Views.MotionEvent ev)
		{
			if (this.enabled)
				return base.OnInterceptTouchEvent (ev);
			else
				return false;
		}

		public bool TouchEnabled 
		{
			get { return enabled; }
			set { enabled = value; }
		}

	}
}
