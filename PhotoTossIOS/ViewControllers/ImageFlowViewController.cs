
using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using PhotoToss.Core;
using SDWebImage;
using CoreGraphics;
using CoreAnimation;

namespace PhotoToss.iOSApp
{
	public partial class ImageFlowViewController : UIViewController
	{
		private nfloat nativeScale = 1;
		public PhotoRecord CurrentMarkerRecord {get; set;}
		private List<ImageLineageRecord> imageList;
		private float curProgress;
		private string imageSizeStr = "=s2048";
		private UIView frameView;
		private UIImageView innerImage;
		private UIImageView outerImage;
		private int lastFrame = -1;
		private bool scrollInited = false;
		private bool isOuterSet, isInnerSet;
		public ImageFlowViewController () : base ("ImageFlowViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			UITapGestureRecognizer doubletap = new UITapGestureRecognizer(OnDoubleTap) {
				NumberOfTapsRequired = 2 // double tap
			};
			scrollView.AddGestureRecognizer(doubletap); 

			slider.Value = 0;
			slider.ValueChanged += (object sender, EventArgs e) => {
				HandleProgressiveRender (slider.Value);
			};

			frameView = new UIView (scrollView.Bounds);
			frameView.BackgroundColor = UIColor.Green;
			outerImage = new UIImageView (new CGRect (new CGPoint (20, 20), new CGSize (300, 300)));
			outerImage.BackgroundColor = UIColor.Red;
			frameView.AddSubview (outerImage);
			innerImage = new UIImageView (new CGRect (new CGPoint (100, 100), new CGSize (200, 200)));
			innerImage.BackgroundColor = UIColor.Yellow;
			frameView.AddSubview (innerImage);

			scrollView.AddSubview (frameView);
			frameView.Layer.Position = new CGPoint (0, 0);
			frameView.Layer.AnchorPoint = new CGPoint (0, 0);
			scrollView.BackgroundColor = UIColor.Blue;
		}

		private void OnDoubleTap (UIGestureRecognizer gesture) {
			nfloat oldScale = scrollView.ZoomScale;
			nfloat newScale = 1;

			if (oldScale > 1)
				newScale = 1;
			else if (oldScale != nativeScale)
				newScale = nativeScale;
			else
				newScale = 1;

			scrollView.SetZoomScale(newScale, true);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			Refresh();
		}

		private void Refresh()
		{
			if (CurrentMarkerRecord != null) {
				PhotoTossRest.Instance.GetImageLineage (CurrentMarkerRecord.id, (theParents) => {

					UpdateList (theParents);
				});
			}
		} 

		private void UpdateList(List<PhotoRecord> theParents)
		{
			if (imageList != null)
				return;
			
			imageList = new List<ImageLineageRecord> ();
			theParents.Insert (0, CurrentMarkerRecord);
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

			InvokeOnMainThread (() => {
				if (theParents.Count == 1) {
						slider.Hidden = true;
				} else {
					slider.MaxValue = imageList.Count - 1;
				}

				UpdatePoints ();
				});
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
				SDWebImageManager.SharedManager.Download (
					url: new NSUrl (innerRec.imageUrl + "=s2048"), 
					options: SDWebImageOptions.CacheMemoryOnly,
					progressHandler: (recievedSize, expectedSize) => {
						// Track progress...
					},
					completedHandler: ImageLoadComplete);
			}

			if (outerRec != null) {
				if (outerRec.loaded) {
					if (outerRec.outerLarge != null)
						outerRec.curLoc = outerRec.outerLarge.Lerp (outerRec.outerSmall, percent);
					else
						outerRec.curLoc = outerRec.outerSmall.Copy ();
				} else if (!outerRec.loadRequested) {
					outerRec.loadRequested = true;
					SDWebImageManager.SharedManager.Download (
						url: new NSUrl (outerRec.imageUrl + "=s2048"), 
						options: SDWebImageOptions.CacheMemoryOnly,
						progressHandler: (recievedSize, expectedSize) => {
							// Track progress...
						},
						completedHandler: ImageLoadComplete);
				}

			}

			RenderImageFrame ();
		}


		private void ImageLoadComplete(UIImage image, NSError theErr, SDImageCacheType cacheType, bool finished, NSUrl theUrl )
		{
			bool updateAttacher = false;
			string trimmedURL = theUrl.AbsoluteString.Substring (0, theUrl.AbsoluteString.LastIndexOf ("=s"));
			ImageLineageRecord curRec = imageList.Find (rec => rec.imageUrl.CompareTo(trimmedURL) == 0);


			if (curRec != null) {
				curRec.InitRecord (image);

				InvokeOnMainThread (() => {
					if (!scrollInited) {

						CGRect bounds = new CGRect (new CGPoint (0, 0), image.Size);
						frameView.Frame = bounds;
						frameView.Bounds = bounds;
						scrollView.MaximumZoomScale = 3f;
						scrollView.MinimumZoomScale = .1f;
						scrollView.ViewForZoomingInScrollView += (UIScrollView sv) => {
							return frameView;
						};

						scrollView.ContentSize = image.Size;
						CGSize scrollSize = scrollView.Bounds.Size;

						nfloat hScale = scrollSize.Width / image.Size.Width;
						nfloat vScale = scrollSize.Height / image.Size.Height;
						nativeScale = hScale < vScale ? hScale : vScale;// (nfloat)Math.Min(hScale, vScale);
						// compute initial scale
						scrollView.SetZoomScale (nativeScale, true);
						scrollInited = true;

					}
					UpdatePoints ();

				});
			
			}
		}

		private void SetInnerImage(ImageLineageRecord innerRec)
		{
			if ((!isInnerSet) && (innerRec != null) && (innerRec.cachedMap != null)) {
				// set inner bitmap
				CGRect bRect = new CGRect (new CGPoint (0, 0), innerRec.cachedMap.Size);
				innerImage.Frame = bRect;
				innerImage.Bounds = bRect;
				innerImage.Image = innerRec.cachedMap;
				isInnerSet = true;
			}
		}

		private void SetOuterImage(ImageLineageRecord outerRec)
		{
			if ((!isOuterSet) && (outerRec != null) && (outerRec.cachedMap != null)) {
				// set outer bitmap
				CGRect bRect = new CGRect (new CGPoint (0, 0), outerRec.cachedMap.Size);
				outerImage.Frame = bRect;
				outerImage.Bounds = bRect;
				outerImage.Image = outerRec.cachedMap;
				isOuterSet = true;

			}
		}

		private void RenderImageFrame()
		{
			int curInnerFrame = (int)Math.Truncate (curProgress);
			float percent = curProgress - curInnerFrame;
			ImageLineageRecord innerRec = imageList [curInnerFrame];
			ImageLineageRecord outerRec = innerRec.nextRec;

			if (curInnerFrame != lastFrame) {
				// set us up!
				isInnerSet = isOuterSet = false;

				lastFrame = curInnerFrame;
			}

			SetOuterImage (outerRec);
			SetInnerImage (innerRec);

			if ((outerRec != null) && (outerRec.cachedMap != null)) {
				// set outer transform
				CATransform3D outerMatrix = ImageLineageRecord.RectToQuad (new CGRect (new CGPoint (0, 0), new CGSize(outerRec.cachedMap.Size.Width, outerRec.cachedMap.Size.Height)),
					outerRec.curLoc.topleft, outerRec.curLoc.topright, outerRec.curLoc.bottomleft, outerRec.curLoc.bottomright);

				CGPoint newLoc = new CGPoint (0,0);
				outerImage.Layer.AnchorPoint = newLoc;
				outerImage.Layer.Position = newLoc;
				outerImage.Layer.Transform = outerMatrix;

			}

			if ((innerRec != null) && (innerRec.cachedMap != null)) {
				// set inner tranform
				CATransform3D innerMatrix = ImageLineageRecord.RectToQuad (new CGRect (new CGPoint (0, 0), new CGSize(innerRec.cachedMap.Size.Width, innerRec.cachedMap.Size.Height)),
					innerRec.curLoc.topleft, innerRec.curLoc.topright, innerRec.curLoc.bottomleft, innerRec.curLoc.bottomright);

				CGPoint newLoc = new CGPoint (0,0);
				innerImage.Layer.AnchorPoint = newLoc;
				innerImage.Layer.Position = newLoc;
				innerImage.Layer.Transform = innerMatrix;
			}


		}

		private void HandleProgressiveRender(float curStatus)
		{
			curProgress = curStatus;
			UpdatePoints ();

		}


		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}



	}



}

