
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
using ImageViews.Photo;
using Android.Graphics;

namespace PhotoToss.AndroidApp
{
	public class ImageViewDetailFragment : Android.Support.V4.App.Fragment,  Koush.IUrlImageViewCallback
	{
		private ImageView imageView;
		private SeekBar slider {get; set;}
		private Bitmap canvasMap = null;
		private Bitmap curMap = null;
		private List<ImageLineageRecord> imageList;
		private PhotoViewAttacher attacher;
		private float curProgress;
		private string imageSizeStr = "=s2048";
		private EditText captionText;
		private Button sendBtn;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View fragment = inflater.Inflate(Resource.Layout.ImageViewDetailFragment, null);

			PhotoRecord curRec = PhotoTossRest.Instance.CurrentImage;


			imageView = fragment.FindViewById<ImageView>(Resource.Id.imageView);
			slider = fragment.FindViewById<SeekBar> (Resource.Id.slider);

			slider.Progress = 0;
			slider.Max = 0;




			captionText = fragment.FindViewById<EditText> (Resource.Id.turnText);
			sendBtn = fragment.FindViewById<Button> (Resource.Id.sendTurnBtn);

			sendBtn.Click += (object sender, EventArgs e) => {
				UpdateCaptionText();
			};
			captionText.Text = curRec.caption;
			Refresh ();
			return fragment;
		}

		private void Refresh()
		{
			if (PhotoTossRest.Instance.CurrentImage != null) {
				PhotoTossRest.Instance.GetImageLineage (PhotoTossRest.Instance.CurrentImage.id, (theParents) => {

					UpdateList (theParents);
				});
			}
		} 


		private void UpdateList(List<PhotoRecord> theParents)
		{
			if (imageList == null) {
				imageList = new List<ImageLineageRecord> ();
				theParents.Insert (0, PhotoTossRest.Instance.CurrentImage);
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

				if (theParents.Count == 1) {
					slider.Visibility = ViewStates.Gone;
				} else {
					slider.Max = 100 * (imageList.Count - 1);
					slider.KeyProgressIncrement = 100;
				}

				UpdatePoints ();
				slider.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) => {
					HandleProgressiveRender ((float)e.Progress / 100);
				};

			}
		}



		public void OnLoaded(ImageView theImage, Bitmap theBitmap, string theURL, bool p4)
		{
			bool updateAttacher = false;
			string trimmedURL = theURL.Substring (0, theURL.LastIndexOf ("=s"));
			ImageLineageRecord curRec = imageList.Find (rec => rec.imageUrl.CompareTo(trimmedURL) == 0);
			if (canvasMap == null) {
				curMap = theBitmap;
				System.GC.Collect ();
				canvasMap = curMap.Copy (curMap.GetConfig (), true);
				updateAttacher = true;
			}
			if (curRec != null) {
				curRec.InitRecord (theBitmap);

				this.Activity.RunOnUiThread (() => {
					if (updateAttacher) {
						attacher = new PhotoViewAttacher (imageView);
						imageView.SetImageBitmap (canvasMap);

						attacher.MaximumScale = 5;
						attacher.Update();
						Matrix	transMatrix = new Matrix (attacher.DisplayMatrix);
						transMatrix.SetTranslate (-(attacher.DisplayRect.Left), -(attacher.DisplayRect.Top));
						attacher.SetDisplayMatrix (transMatrix);
					}
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
			float percent = (curProgress - curInnerFrame);

			ImageLineageRecord innerRec = imageList [curInnerFrame];
			ImageLineageRecord outerRec = innerRec.nextRec;

			if (innerRec.loaded) {
				if (innerRec.innerLarge != null)
					innerRec.curLoc = innerRec.innerLarge.Lerp (innerRec.innerSmall, percent);
				else
					innerRec.curLoc = innerRec.outerSmall.Copy ();
			} else if (!innerRec.loadRequested) {
				innerRec.loadRequested = true;
				Koush.UrlImageViewHelper.LoadUrlDrawable (this.Activity, innerRec.imageUrl + imageSizeStr, this);
			}

			if (outerRec != null) {
				if (outerRec.loaded) {
					if (outerRec.outerLarge != null)
						outerRec.curLoc = outerRec.outerLarge.Lerp (outerRec.outerSmall, percent);
					else
						outerRec.curLoc = outerRec.outerSmall.Copy ();
				} else if (!outerRec.loadRequested) {
					outerRec.loadRequested = true;
					Koush.UrlImageViewHelper.LoadUrlDrawable (this.Activity, outerRec.imageUrl + imageSizeStr, this);
				}

			}

			RenderImageFrame ();
		}



		private void RenderImageFrame()
		{
			int curInnerFrame = (int)Math.Truncate (curProgress);
			ImageLineageRecord innerRec = imageList [curInnerFrame];
			ImageLineageRecord outerRec = innerRec.nextRec;

			if ((outerRec != null) && (outerRec.loaded) && (outerRec.cachedMap != curMap)) {
				// potentially recompute the map
				if (canvasMap != null) {
					canvasMap.Dispose ();
					System.GC.Collect ();
				}

				curMap = outerRec.cachedMap;
				canvasMap = curMap.Copy (curMap.GetConfig (), true);
				imageView.SetImageBitmap (canvasMap);

				if (attacher != null) {
					attacher.Update ();

					Matrix	transMatrix = new Matrix (attacher.DisplayMatrix);
					transMatrix.SetTranslate (-(attacher.DisplayRect.Left), -(attacher.DisplayRect.Top));
					attacher.SetDisplayMatrix (transMatrix);
				}
			}
			if (canvasMap == null)
				return;
			Canvas newCanvas = new Canvas (canvasMap);
			Paint thePaint = new Paint (PaintFlags.AntiAlias);

			Paint blackPaint = new Paint();
			blackPaint.Color = Color.Black;
			newCanvas.DrawRect (new Rect (0, 0, canvasMap.Width, canvasMap.Height), blackPaint);

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

					//newCanvas.DrawRect(new Rect(0,0,canvasMap.Width, canvasMap.Height), blackPaint);
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

		public override void OnDestroyView ()
		{
			if (imageList != null) {
				foreach (ImageLineageRecord curRec in imageList) {
					curRec.DisposeRecord ();
				}
				imageList.Clear ();
			}
			if (canvasMap != null)
				canvasMap.Dispose();
			imageList = null;
			canvasMap = null;
			curMap = null;
			System.GC.Collect ();
			base.OnDestroy ();
		}



		public void UpdateCaptionText()
		{
			string newText = captionText.Text;

			if (string.Compare (newText, PhotoTossRest.Instance.CurrentImage.caption) != 0) {
				sendBtn.Enabled = false;
				captionText.Enabled = false;
				PhotoTossRest.Instance.SetImageCaption (PhotoTossRest.Instance.CurrentImage.id, newText, (newRec) => {
					Activity.RunOnUiThread(() =>
						{
							sendBtn.Enabled = true;
							captionText.Enabled = true;
						});
				});
			}
		}

		public void Update()
		{

		}
	}
}

