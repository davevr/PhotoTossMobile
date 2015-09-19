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
using System.Timers;


namespace PhotoToss.AndroidApp
{
	[Activity(ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
    public class TossActivity : Activity
    {
        Button tossBtn;
		ImageView imageView;
		GridView CatchView;

		public static int itemWidth = 320;
		Android.Util.DisplayMetrics	metrics;
		Timer	tossTimer;
		int secondsLeft;
		int lastCount;
		long currentTossId;


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
			CatchView = FindViewById<GridView> (Resource.Id.catchView);
			CatchView.Adapter = new TossStatusAdapter(this);

            tossBtn.Click +=  delegate
            {
				EndToss();


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
                
				currentTossId = theToss.id;
				string guid = currentTossId.ToString ();
				string url = "http://phototoss.com/toss/" + guid;


				var bitMap = writer.Write(url);
				lastCount = 0;
				imageView.SetImageBitmap(bitMap);
				StartTossTimer();
			});


		}

		private void StartTossTimer()
		{
			tossTimer = new Timer ();
			tossTimer.Interval = 1000;
			tossTimer.AutoReset = true;
			tossTimer.Elapsed += HandleTossTimerTick;
			secondsLeft = 60;
			tossTimer.Start ();
		}

		private void HandleTossTimerTick(object sender, ElapsedEventArgs e)
		{
			secondsLeft--;
			if (secondsLeft < 0) {
				EndToss ();
			} else {
				PhotoTossRest.Instance.GetTossCatches (currentTossId, (catchList) => {
					RunOnUiThread(() => 
						{
							if ((catchList != null) && (catchList.Count != lastCount))
								UpdateTosses(catchList);
							tossBtn.Text = String.Format ("Done (ending in {0} seconds)", secondsLeft);
						});
				});
			}
		}

		private void UpdateTosses(List<PhotoRecord> catchList)
		{
			((TossStatusAdapter)CatchView.Adapter).PhotoRecordList = catchList; 
			((TossStatusAdapter)CatchView.Adapter).NotifyDataSetChanged();
			CatchView.InvalidateViews ();
			lastCount = catchList.Count;
		}

		private void EndToss()
		{
			StopTossTimer ();
			Finish();
		}

		private void StopTossTimer()
		{
			tossTimer.Stop ();

		}
        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }


		public class TossStatusAdapter : BaseAdapter
		{
			private readonly Context context;
			private int profileWidth = 64;
			public  List<PhotoRecord> PhotoRecordList { get; set; }

			public TossStatusAdapter(Context c)
			{
				context = c;

			}

			public override int Count
			{
				get 
				{
					if (PhotoRecordList != null)
						return PhotoRecordList.Count; 
					else
						return 0;
				}
			}

			public override Java.Lang.Object GetItem(int position)
			{
				return null;
			}

			public override long GetItemId(int position)
			{
				return 0;
			}


			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				View curView;
				PhotoRecord curRec = PhotoRecordList[position];
				ImageView imageView;

				if (convertView == null)
				{
					curView = ((TossActivity)context).LayoutInflater.Inflate(Resource.Layout.TossProgressCell,null);
				}
				else
				{
					curView = convertView;
				}
				imageView = curView.FindViewById<ImageView>(Resource.Id.imageView);
				Koush.UrlImageViewHelper.SetUrlDrawable (imageView, PhotoTossRest.Instance.GetUserProfileImage (curRec.ownername));


				return curView;
			}




		}
    }
}