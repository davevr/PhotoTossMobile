
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
		private TextView totalImageText;
		private TextView imageLineageText;
		private TextView imageTossesText;
		private TextView imageCatchesText;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View fragment = inflater.Inflate(Resource.Layout.ImageViewStatsFragment, null);

			totalImageText = fragment.FindViewById<TextView> (Resource.Id.totalImageText);
			imageLineageText = fragment.FindViewById<TextView> (Resource.Id.imageLineageText);
			imageTossesText = fragment.FindViewById<TextView> (Resource.Id.imageTossesText);
			imageCatchesText = fragment.FindViewById<TextView> (Resource.Id.imageCatchesText);

			return fragment;
		}


		public void Update()
		{
			PhotoTossRest.Instance.GetImageStats(PhotoTossRest.Instance.CurrentImage.id, (theStats) => {
				UpdateStats(theStats);

			});
		}

		private void UpdateStats(ImageStatsRecord theStats) 
		{
			Activity.RunOnUiThread (() => {
				totalImageText.Text = theStats.numcopies.ToString();
				imageLineageText.Text = theStats.numparents.ToString();
				imageTossesText.Text = theStats.numtosses.ToString();
				imageCatchesText.Text = theStats.numchildren.ToString();
			});

		}
	}
}