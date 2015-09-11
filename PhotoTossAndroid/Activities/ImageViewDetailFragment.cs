
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
	public class ImageViewDetailFragment : Android.Support.V4.App.Fragment
	{
		Button tossBtn;
		ImageView imageView;
		public static int itemWidth = 320;
		Android.Util.DisplayMetrics	metrics;
		bool isTossing = false;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View fragment = inflater.Inflate(Resource.Layout.ImageViewDetailFragment, null);

			PhotoRecord curRec = PhotoTossRest.Instance.CurrentImage;
			metrics = Resources.DisplayMetrics;
			int screenWidth = metrics.WidthPixels;
			int screenHeight = (int)(metrics.HeightPixels * .8);
			itemWidth = Math.Max (screenWidth, screenHeight);


			tossBtn = fragment.FindViewById<Button>(Resource.Id.tossButton);
			imageView = fragment.FindViewById<ImageView>(Resource.Id.aztekView);
			Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curRec.imageUrl + "=s" + itemWidth.ToString(), Resource.Drawable.ic_camera);

			tossBtn.Click += delegate {
				if (isTossing) {
					isTossing = false;
					tossBtn.Text = "Start Toss";
					Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curRec.imageUrl + "=s" + itemWidth.ToString (), Resource.Drawable.ic_camera);

				} else {
					isTossing = true;
					tossBtn.Text = "cancel toss";
					double myLong = MainActivity._lastLocation.Longitude;
					double myLat = MainActivity._lastLocation.Latitude;
					int gameType = 0;
					long imageId = curRec.id;

					PhotoTossRest.Instance.StartToss (imageId, gameType, myLong, myLat, PresentTossResult);
				}
			};

			return fragment;
		}

		private void PresentTossResult(TossRecord theToss)
		{
			Activity.RunOnUiThread (() => {
				var writer = new ZXing.Mobile.BarcodeWriter
				{
					Format = ZXing.BarcodeFormat.AZTEC,
					Options = new ZXing.Common.EncodingOptions
					{
						Width = 240,
						Height = 240,
						Margin = 1
					}
				};

				string baseURL = "http://phototoss.com/share/";
				string guid = theToss.id.ToString();
				string url = baseURL + guid;
				url = "http://phototoss.com/toss/" + guid;


				var bitMap = writer.Write(url);

				imageView.SetImageBitmap(bitMap);
			});


		}


		private int ConvertPixelsToDp(float pixelValue)
		{
			var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
			return dp;
		}
	}
}

