
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
using PhotoToss.Core;
using System.Collections.Generic;
using Android.Content.PM;
using ImageViews.Photo;
using Android.Graphics;


namespace PhotoToss.AndroidApp
{
	[Activity(Theme = "@style/AppSubTheme", ScreenOrientation = ScreenOrientation.Portrait)]		
	public class ImageFlowActivity : Android.Support.V7.App.AppCompatActivity, Koush.IUrlImageViewCallback
	{
		private ImageView imageView { get; set; }
		private SeekBar slider { get; set; }
		private Bitmap innerMap = null;
		private Bitmap outerMap = null;
		private Bitmap canvasMap = null;
		private List<ImageLineageRecord> imageList;
		private PhotoViewAttacher attacher;
		private float curProgress;
		private BarcodeLocation innerLarge, innerSmall, innerCurrent;
		private BarcodeLocation outerLarge, outerSmall, outerCurrent;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			SetContentView (Resource.Layout.ImageFlowActivity);
			imageView = FindViewById<ImageView> (Resource.Id.imageView);
			slider = FindViewById<SeekBar> (Resource.Id.slider);

			slider.Progress = 0;
			slider.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) => {
				HandleProgressiveRender ((float)e.Progress / 100);
			};

			Refresh ();
		}

		private void Refresh()
		{
			if (ImageViewSpreadFragment.CurrentMarkerRecord != null) {
				PhotoTossRest.Instance.GetImageLineage (ImageViewSpreadFragment.CurrentMarkerRecord.id, (theParents) => {

					UpdateList (theParents);
				});
			}
		} 

		private void UpdateList(List<PhotoRecord> theParents)
		{
			imageList = new List<ImageLineageRecord> ();
			theParents.Insert (0, ImageViewSpreadFragment.CurrentMarkerRecord);
			theParents.Reverse ();
			ImageLineageRecord prevRec = null;
			foreach (PhotoRecord curRec in theParents) {
				ImageLineageRecord newRec = new ImageLineageRecord ();
				newRec.photoRec = curRec;
				if (!string.IsNullOrEmpty (curRec.catchUrl))
					newRec.imageUrl = curRec.catchUrl;
				else
					newRec.imageUrl = curRec.imageUrl;
				newRec.loaded = false;
				newRec.prevRec = prevRec;
				if (prevRec != null)
					prevRec.nextRec = newRec;
				imageList.Add (newRec);
				prevRec = newRec;
			}

			slider.Max = 100 * (imageList.Count - 1);
			slider.KeyProgressIncrement = 100;

			UpdatePoints ();
		}

		public void OnLoaded(ImageView theImage, Bitmap theBitmap, string theURL, bool p4)
		{
			ImageLineageRecord curRec = imageList.Find (rec => rec.imageUrl.CompareTo(theURL) == 0);

			if (curRec != null) {
				curRec.InitRecord (theBitmap);

				RunOnUiThread (() => {
					UpdatePoints ();
				});

			}
		}




		private void HandleProgressiveRender(float curStatus)
		{
			curProgress = curStatus;
			UpdatePoints ();

		}



		private void UpdatePoints()
		{
			int curInnerFrame = (int)Math.Truncate (curProgress);
			float percent = 1 - (curProgress - curInnerFrame);

			ImageLineageRecord innerRec = imageList [curInnerFrame];
			ImageLineageRecord outerRec = innerRec.nextRec;

			if (innerRec.loaded)
				innerRec.curLoc = innerRec.innerLarge.Lerp (innerRec.innerSmall, percent);
			else
				Koush.UrlImageViewHelper.LoadUrlDrawable (this, innerRec.imageUrl + "=s2048", this);

			if (outerRec != null) {
				if (outerRec.loaded)
					outerRec.curLoc = outerRec.outerLarge.Lerp (outerRec.outerSmall, percent);
				else
					Koush.UrlImageViewHelper.LoadUrlDrawable (this, outerRec.imageUrl + "=s2048", this);

			}

			RenderImageFrame ();
		}

		private void RenderImageFrame()
		{
			Canvas newCanvas = new Canvas (canvasMap);
			Paint thePaint = new Paint (PaintFlags.AntiAlias);
			int curInnerFrame = (int)Math.Truncate (curProgress);
			float percent = curProgress - curInnerFrame;
			ImageLineageRecord innerRec = imageList [curInnerFrame];
			ImageLineageRecord outerRec = innerRec.nextRec;


			if ((outerRec != null) && (outerRec.loaded)) {
				if (outerRec.curLoc != null) {
					Matrix outerMatrix = new Matrix ();
					outerMatrix.SetPolyToPoly (new float[] {
						0,
						0,
						outerRec.cachedMap.Width,
						0,
						outerRec.cachedMap.Width,
						outerRec.cachedMap.Height,
						0,
						outerRec.cachedMap.Height
					}, 0,
						new float[] {outerRec.curLoc.topleft.x, outerRec.curLoc.topleft.y, outerRec.curLoc.topright.x, outerRec.curLoc.topright.y,
							outerRec.curLoc.bottomright.x, outerRec.curLoc.bottomright.y, outerRec.curLoc.bottomleft.x, outerRec.curLoc.bottomleft.y
						}, 0, 4);

					newCanvas.DrawBitmap (outerRec.cachedMap, outerMatrix, thePaint);
				} else {
					// not initted yet
					Paint blackPaint = new Paint();
					blackPaint.Color = Color.Black;
					newCanvas.DrawRect(new Rect(0,0,canvasMap.Width, canvasMap.Height), blackPaint);
				}
			}


			if ((innerRec != null) && (innerRec.loaded)) {
				if (innerRec.curLoc != null) {
					Matrix innerMatrix = new Matrix ();
					innerMatrix.SetPolyToPoly (new float[] {
						0,
						0,
						innerRec.cachedMap.Width,
						0,
						innerRec.cachedMap.Width,
						innerRec.cachedMap.Height,
						0,
						innerRec.cachedMap.Height
					}, 0,
						new float[] {innerRec.curLoc.topleft.x, innerRec.curLoc.topleft.y, innerRec.curLoc.topright.x, innerRec.curLoc.topright.y,
							innerRec.curLoc.bottomright.x, innerRec.curLoc.bottomright.y, innerRec.curLoc.bottomleft.x, innerRec.curLoc.bottomleft.y
						}, 0, 4);
					newCanvas.DrawBitmap (innerRec.cachedMap, innerMatrix, thePaint);
				} else {
					// not inited - just draw fullscreen
					newCanvas.DrawBitmap (innerRec.cachedMap, 0, 0, thePaint);
				}
			}


		}
	}

	public class ImageLineageRecord
	{
		public PhotoRecord photoRec { get; set; }
		public Bitmap cachedMap {get; set;}
		public ImageLineageRecord prevRec {get; set;}
		public ImageLineageRecord nextRec {get; set;}
		public BarcodeLocation innerLarge {get; set;} 
		public BarcodeLocation innerSmall {get; set;} 
		public BarcodeLocation outerLarge {get; set;} 
		public BarcodeLocation outerSmall {get; set;} 
		public BarcodeLocation curLoc {get; set;} 
		public string imageUrl {get; set;}
		public bool loaded {get; set;}


		public void InitRecord (Bitmap theBitmap) 
		{
			cachedMap = theBitmap;
			outerSmall = new BarcodeLocation ();
			outerSmall.topleft = new BarcodePoint (0, 0);
			outerSmall.topright = new BarcodePoint (cachedMap.Width, 0);
			outerSmall.bottomleft = new BarcodePoint (0, cachedMap.Height);
			outerSmall.bottomright = new BarcodePoint (cachedMap.Width, cachedMap.Height);

			if ((nextRec != null) && nextRec.loaded) {
				InitFromNext ();
			}

			if ((prevRec != null) && (!prevRec.loaded)) {
				prevRec.InitFromNext ();
			}

			loaded = true;
		}

		private void InitFromNext()
		{
			if ((nextRec != null) && (innerSmall == null)) {
				Bitmap outerMap = nextRec.cachedMap;
				BarcodeLocation barLoc = nextRec.photoRec.barcodelocation;

				if (barLoc != null) {
					float scaledHeight = cachedMap.Height * ((float)outerMap.Width / (float)cachedMap.Width);

					innerLarge = new BarcodeLocation ();
					innerLarge.topleft = new BarcodePoint (0, 0);
					innerLarge.topright = new BarcodePoint (outerMap.Width, 0);
					innerLarge.bottomleft = new BarcodePoint (0, scaledHeight);
					innerLarge.bottomright = new BarcodePoint (outerMap.Width, scaledHeight);

					innerSmall = new BarcodeLocation ();

					Matrix baseMatrix = new Matrix ();
					baseMatrix.SetPolyToPoly (new float[] {
						0,
						0,
						outerMap.Width,
						0,
						outerMap.Width,
						outerMap.Width,
						0,
						outerMap.Width
					}, 0,
						new float[] {barLoc.topleft.x, barLoc.topleft.y, barLoc.topright.x, barLoc.topright.y,
							barLoc.bottomright.x, barLoc.bottomright.y, barLoc.bottomleft.x, barLoc.bottomleft.y
						}, 0, 4);

					float[] innerPtList = innerLarge.GetPts ();
					baseMatrix.MapPoints (innerPtList);
					innerSmall = BarcodeLocation.AllocFromPts (innerPtList);

					Matrix expandedMatrix = new Matrix ();
					baseMatrix.Invert (expandedMatrix);
					float[] outerPtList = nextRec.outerSmall.GetPts ();
					expandedMatrix.MapPoints (outerPtList);
					nextRec.outerLarge = BarcodeLocation.AllocFromPts (outerPtList);
				}

			}
		}

	}
}

