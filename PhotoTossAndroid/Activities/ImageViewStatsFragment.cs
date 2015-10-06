
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


namespace PhotoToss.AndroidApp
{
	public class ImageViewStatsFragment : Android.Support.V4.App.Fragment
	{
		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View fragment = inflater.Inflate(Resource.Layout.ImageViewStatsFragment, null);

			return fragment;
		}


		public void Update()
		{
			PhotoTossRest.Instance.GetImageStats (PhotoTossRest.Instance.CurrentImage.id, (theStats) => {

				Console.WriteLine("Stats loaded!");
			});
		}
	}
}