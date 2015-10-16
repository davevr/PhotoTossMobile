
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

			if (curRec) {
				curRec.InitRecord (theBitmap);
				int curIndex = imageList.IndexOf (curRec);




			}
		}




		private void HandleProgressiveRender(float curStatus)
		{
			curProgress = curStatus;
			UpdatePoints ();

		}

		private void ConfigurePoints()
		{
			BarcodeLocation barLoc = parents [0].barcodelocation;

			float innerHeight = innerMap.Height * ((float)outerMap.Width / (float)innerMap.Width);
			innerLarge = new BarcodeLocation ();
			innerLarge.topleft = new BarcodePoint (0, 0);
			innerLarge.topright = new BarcodePoint (outerMap.Width, 0);
			innerLarge.bottomleft = new BarcodePoint (0, innerHeight);
			innerLarge.bottomright = new BarcodePoint (outerMap.Width, innerHeight);

			outerSmall = new BarcodeLocation ();
			outerSmall.topleft = new BarcodePoint (0, 0);
			outerSmall.topright = new BarcodePoint (outerMap.Width, 0);
			outerSmall.bottomleft = new BarcodePoint (0, outerMap.Height);
			outerSmall.bottomright = new BarcodePoint (outerMap.Width, outerMap.Height);

			Matrix baseMatrix = new Matrix ();
			baseMatrix.SetPolyToPoly (new float[] { 0, 0, outerMap.Width, 0, outerMap.Width, outerMap.Width, 0, outerMap.Width }, 0,
				new float[] {barLoc.topleft.x, barLoc.topleft.y, barLoc.topright.x, barLoc.topright.y,
					barLoc.bottomright.x, barLoc.bottomright.y, barLoc.bottomleft.x, barLoc.bottomleft.y
				}, 0, 4);

			float[] innerPtList = innerLarge.GetPts ();
			baseMatrix.MapPoints (innerPtList);
			innerSmall = BarcodeLocation.AllocFromPts (innerPtList);

			Matrix expandedMatrix = new Matrix ();
			baseMatrix.Invert (expandedMatrix);
			float[] outerPtList = outerSmall.GetPts ();
			expandedMatrix.MapPoints (outerPtList);
			outerLarge = BarcodeLocation.AllocFromPts (outerPtList);
			
			UpdatePoints ();
		}

		private void UpdatePoints()
		{
			innerCurrent = innerLarge.Lerp (innerSmall, curProgress);
			outerCurrent = outerLarge.Lerp (outerSmall, curProgress);
			RenderImageFrame ();
		}

		private void RenderImageFrame()
		{
			Canvas newCanvas = new Canvas (canvasMap);
			Paint thePaint = new Paint (PaintFlags.AntiAlias);

			Matrix innerMatrix = new Matrix ();
			innerMatrix.SetPolyToPoly (new float[] {
				0,
				0,
				innerMap.Width,
				0,
				innerMap.Width,
				innerMap.Height,
				0,
				innerMap.Height
			}, 0,
				new float[] {innerCurrent.topleft.x, innerCurrent.topleft.y, innerCurrent.topright.x, innerCurrent.topright.y,
					innerCurrent.bottomright.x, innerCurrent.bottomright.y, innerCurrent.bottomleft.x, innerCurrent.bottomleft.y
				}, 0, 4);

			Matrix outerMatrix = new Matrix ();
			outerMatrix.SetPolyToPoly (new float[] {
				0,
				0,
				outerMap.Width,
				0,
				outerMap.Width,
				outerMap.Height,
				0,
				outerMap.Height
			}, 0,
				new float[] {outerCurrent.topleft.x, outerCurrent.topleft.y, outerCurrent.topright.x, outerCurrent.topright.y,
					outerCurrent.bottomright.x, outerCurrent.bottomright.y, outerCurrent.bottomleft.x, outerCurrent.bottomleft.y
				}, 0, 4);
			
			//outerMatrix.Reset ();
			newCanvas.DrawBitmap (outerMap, outerMatrix, thePaint);
			newCanvas.DrawBitmap (innerMap, innerMatrix, thePaint);

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

			if ((prevRec != null) && prevRec.loaded) {
				InitFromPrev ();
			}

			if ((nextRec != null) && nextRec.loaded) {
				nextRec.InitFromPrev ();
			}

			loaded = true;
		}

		private void InitFromPrev()
		{
			if ((prevRec != null) && (innerSmall == null)) {
				Bitmap outerMap = prevRec.cachedMap;
				BarcodeLocation barLoc = prevRec.photoRec.barcodelocation;

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
					float[] outerPtList = outerSmall.GetPts ();
					expandedMatrix.MapPoints (outerPtList);
					outerLarge = BarcodeLocation.AllocFromPts (outerPtList);
				}

			}
		}

	}
}

