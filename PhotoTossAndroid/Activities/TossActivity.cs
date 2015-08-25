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

			// get the actual menu object from the layout
			/*
			var menu = FindViewById<SatelliteMenuButton>(Resource.Id.menu);

			// register for the menu item selection event here
			menu.MenuItemClick += delegate(object sender, SatelliteMenuItemEventArgs e)
			{
				// parse the enum value from int back to the enum here
				string mit = e.MenuItem.Tag.ToString();

				// just show the menu item selected toast, in the app we would probably fire new activity or similar
				Toast.MakeText(this, string.Format("Menu item selected: {0}", mit), ToastLength.Short).Show();
			};

			// array of items
			var items = new List<SatelliteMenuButtonItem>();

			// just add one by one
			items.Add(new SatelliteMenuButtonItem((int) 1, Resource.Drawable.ic_help));
			items.Add(new SatelliteMenuButtonItem((int) 2, Resource.Drawable.ic_settings));
			items.Add(new SatelliteMenuButtonItem((int) 3, Resource.Drawable.ic_star_rate_black_48dp));
			items.Add(new SatelliteMenuButtonItem((int) 4, Resource.Drawable.ic_delete_black_48dp));
			items.Add(new SatelliteMenuButtonItem((int) 5, Resource.Drawable.ic_camera));
			items.Add(new SatelliteMenuButtonItem((int) 6, Resource.Drawable.ic_catch));

			// now add all to the menus
			menu.AddItems(items.ToArray());
			*/

			PhotoRecord curRec = PhotoTossRest.Instance.CurrentImage;
			metrics = Resources.DisplayMetrics;
			int screenWidth = metrics.WidthPixels;
			int screenHeight = (int)(metrics.HeightPixels * .8);
			itemWidth = Math.Max (screenWidth, screenHeight);


            tossBtn = FindViewById<Button>(Resource.Id.tossButton);
			imageView = FindViewById<ImageView>(Resource.Id.aztekView);
			Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curRec.imageUrl + "=s" + itemWidth.ToString(), Resource.Drawable.ic_camera);




            tossBtn.Click +=  delegate
            {
                tossBtn.Text = "cancel toss";
				double myLong = MainActivity._lastLocation.Longitude;
				double myLat = MainActivity._lastLocation.Latitude;
				int gameType = 0;
				long imageId = curRec.id;

				PhotoTossRest.Instance.StartToss(imageId, gameType, myLong, myLat, PresentTossResult);

                


            };
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