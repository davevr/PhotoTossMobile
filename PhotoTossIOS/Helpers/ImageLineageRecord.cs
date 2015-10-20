using System;
using PhotoToss.Core;
using UIKit;

namespace PhotoToss.iOSApp
{
	public class ImageLineageRecord
	{
		public PhotoRecord photoRec { get; set; }
		public UIImage cachedMap {get; set;}
		public ImageLineageRecord prevRec {get; set;}
		public ImageLineageRecord nextRec {get; set;}
		public BarcodeLocation innerLarge {get; set;} 
		public BarcodeLocation innerSmall {get; set;} 
		public BarcodeLocation outerLarge {get; set;} 
		public BarcodeLocation outerSmall {get; set;} 
		public BarcodeLocation curLoc {get; set;} 
		public string imageUrl {get; set;}
		public bool loaded {get; set;}
		public bool loadRequested = false;


		public void DisposeRecord()
		{
			if (cachedMap != null)
				cachedMap.Dispose();
			cachedMap = null;
		}

		public void InitRecord (UIImage theBitmap) 
		{
			cachedMap = theBitmap;
			outerSmall = new BarcodeLocation ();
			outerSmall.topleft = new BarcodePoint (0, 0);
			outerSmall.topright = new BarcodePoint ((float)cachedMap.Size.Width, 0);
			outerSmall.bottomleft = new BarcodePoint (0, (float)cachedMap.Size.Height);
			outerSmall.bottomright = new BarcodePoint ((float)cachedMap.Size.Width, (float)cachedMap.Size.Height);

			if ((nextRec != null) && nextRec.loaded) {
				InitFromNext ();
			}

			if ((prevRec != null) && prevRec.loaded) {
				prevRec.InitFromNext ();
			}

			loaded = true;
		}

		private void InitFromNext()
		{
			if ((nextRec != null) && (innerSmall == null)) {
				UIImage outerMap = nextRec.cachedMap;
				BarcodeLocation barLoc = nextRec.photoRec.barcodelocation;

				if (barLoc != null) {
					float scaledHeight = (float)cachedMap.Size.Height * ((float)outerMap.Size.Width / (float)cachedMap.Size.Width);

					innerLarge = new BarcodeLocation ();
					innerLarge.topleft = new BarcodePoint (0, 0);
					innerLarge.topright = new BarcodePoint ((float)outerMap.Size.Width, 0);
					innerLarge.bottomleft = new BarcodePoint (0, scaledHeight);
					innerLarge.bottomright = new BarcodePoint ((float)outerMap.Size.Width, scaledHeight);

					innerSmall = new BarcodeLocation ();

					// temp for testing
					innerSmall = barLoc.Copy();
					nextRec.outerLarge = new BarcodeLocation ();
					nextRec.outerLarge.topleft = new BarcodePoint (-100, -100);
					nextRec.outerLarge.topright = new BarcodePoint (nextRec.outerSmall.topright.x + 100, -100);
					nextRec.outerLarge.bottomleft = new BarcodePoint (-100, nextRec.outerSmall.bottomleft.y + 100);
					nextRec.outerLarge.bottomright = new BarcodePoint (nextRec.outerSmall.topright.x + 100, nextRec.outerSmall.topright.y + 100);
					/*
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

					// draw inner graphic into outer graphic...
					System.GC.Collect();
					Bitmap newOuterMap = outerMap.Copy (outerMap.GetConfig (), true);
					outerMap.Dispose ();
					nextRec.cachedMap = newOuterMap;
					outerMap = newOuterMap;
					System.GC.Collect ();

					Canvas largeCanvas = new Canvas(outerMap);
					Paint thePaint = new Paint(PaintFlags.AntiAlias);

					Matrix innerMatrix = new Matrix ();
					innerMatrix.SetPolyToPoly (new float[] {
						0,
						0,
						cachedMap.Width,
						0,
						cachedMap.Width,
						cachedMap.Height,
						0,
						cachedMap.Height
					}, 0,
						new float[] {innerSmall.topleft.x, innerSmall.topleft.y, innerSmall.topright.x, innerSmall.topright.y,
							innerSmall.bottomright.x, innerSmall.bottomright.y, innerSmall.bottomleft.x, innerSmall.bottomleft.y
						}, 0, 4);
					largeCanvas.DrawBitmap (cachedMap, innerMatrix, thePaint);
				*/
				}

			}
		}

	}
}

