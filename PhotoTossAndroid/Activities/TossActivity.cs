using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using ZXing;
using ZXing.Rendering;
using PhotoToss.Core;


//using SatelliteMenu;


namespace PhotoToss.AndroidApp
{
	[Activity(ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
    public class TossActivity : Activity
    {
        Button tossBtn;
		ImageView imageView;
		public static int itemWidth = 320;
		Android.Util.DisplayMetrics	metrics;


        protected override void OnCreate(Bundle bundle)
        {
			RequestWindowFeature (WindowFeatures.NoTitle);
			Window.SetFlags (WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
			Window.SetFlags (WindowManagerFlags.Secure, WindowManagerFlags.Secure);
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.TossFragment);

			PhotoRecord curRec = PhotoTossRest.Instance.CurrentImage;
			metrics = Resources.DisplayMetrics;
			int screenWidth = metrics.WidthPixels;
			int screenHeight = (int)(metrics.HeightPixels * .8);
			itemWidth = Math.Max (screenWidth, screenHeight);


            tossBtn = FindViewById<Button>(Resource.Id.tossButton);
			imageView = FindViewById<ImageView>(Resource.Id.aztekView);


            tossBtn.Click +=  delegate
            {
				Finish();


            };
        }

		protected override void OnStart ()
		{
			base.OnStart ();
			double myLong = MainActivity._lastLocation.Longitude;
			double myLat = MainActivity._lastLocation.Latitude;
			int gameType = 0;
			PhotoRecord curRec = PhotoTossRest.Instance.CurrentImage;
			long imageId = curRec.id;
			PhotoTossRest.Instance.StartToss(imageId, gameType, myLong, myLat, PresentTossResult);
		}

		private void PresentTossResult(TossRecord theToss)
		{
			RunOnUiThread (() => {
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