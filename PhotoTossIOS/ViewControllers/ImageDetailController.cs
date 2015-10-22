
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;
using CoreGraphics;
using System.Collections.Generic;
using CoreAnimation;

namespace PhotoToss.iOSApp
{
	public partial class ImageDetailController : UIViewController
	{
		private nfloat scroll_amount = 0.0f;    // amount to scroll 
		private nfloat bottom = 0.0f;           // bottom point
		private nfloat offset = 10.0f;          // extra offset
		private bool moveViewUp = false;  
		private NSObject hideObserver, showObserver;
		private nfloat nativeScale = 1;
		private List<ImageLineageRecord> imageList;
		private float curProgress;
		private string imageSizeStr = "=s2048";
		private UIView frameView;
		private UIImageView innerImage;
		private UIImageView outerImage;
		private int lastFrame = -1;
		private bool scrollInited = false;
		private bool isOuterSet, isInnerSet;
		private UIView activeView;

		public ImageDetailController () : base ("ImageDetailController", null)
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

			UITapGestureRecognizer doubletap = new UITapGestureRecognizer(OnDoubleTap) {
				NumberOfTapsRequired = 2 // double tap
			};
			ImageScroller.AddGestureRecognizer(doubletap); 


			CaptionTextField.Text = PhotoTossRest.Instance.CurrentImage.caption;

			SendBtn.TouchUpInside += (object sender, EventArgs e) => {
				UpdateCaptionText();
			};

			showObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidShowNotification, KeyboardUpNotify);
			hideObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidHideNotification, KeyboardDownNotify);

			slider.Value = 0;
			slider.ValueChanged += (object sender, EventArgs e) => {
				HandleProgressiveRender (slider.Value);
			};

			frameView = new UIView (ImageScroller.Bounds);
			frameView.BackgroundColor = UIColor.LightGray;
			outerImage = new UIImageView (new CGRect (new CGPoint (20, 20), new CGSize (300, 300)));
			outerImage.BackgroundColor = UIColor.LightGray;
			frameView.AddSubview (outerImage);
			innerImage = new UIImageView (new CGRect (new CGPoint (100, 100), new CGSize (200, 200)));
			innerImage.BackgroundColor = UIColor.LightGray;
			frameView.AddSubview (innerImage);

			ImageScroller.AddSubview (frameView);
			frameView.Layer.Position = new CGPoint (0, 0);
			frameView.Layer.AnchorPoint = new CGPoint (0, 0);
			ImageScroller.BackgroundColor = UIColor.LightGray;
		}

		public override void ViewDidUnload ()
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(hideObserver);
			NSNotificationCenter.DefaultCenter.RemoveObserver(showObserver);
			base.ViewDidUnload ();
		}

		private void KeyboardUpNotify(NSNotification notification)
		{
			activeView = null;
			// get the keyboard size
			CGRect r = UIKeyboard.BoundsFromNotification (notification);

			// Find what opened the keyboard
			foreach (UIView view in this.View.Subviews) {
				if (view.IsFirstResponder)
					activeView = CaptionTextField;
			}
			if (activeView != null) {
				// Bottom of the controller = initial position + height + offset      
				bottom = (activeView.Frame.Y + activeView.Frame.Height + offset);

				// Calculate how far we need to scroll
				scroll_amount = (r.Height - (View.Frame.Size.Height - bottom));

				// Perform the scrolling
				if (scroll_amount > 0) {
					moveViewUp = true;
					ScrollTheView (moveViewUp);
				} else {
					moveViewUp = false;
				}
			}

		}

		private void KeyboardDownNotify(NSNotification notification)
		{
			if (activeView != null) {
				if (moveViewUp) {
					ScrollTheView (false);
				}
			}
		}


		private void ScrollTheView(bool move)
		{

			// scroll the view up or down
			UIView.BeginAnimations (string.Empty, System.IntPtr.Zero);
			UIView.SetAnimationDuration (0.3);

			CGRect frame = View.Frame;

			if (move) {
				frame.Y -= scroll_amount;
			} else {
				frame.Y += scroll_amount;
				scroll_amount = 0;
			}

			View.Frame = frame;
			UIView.CommitAnimations();
		}



		private void OnDoubleTap (UIGestureRecognizer gesture) {
			nfloat oldScale = ImageScroller.ZoomScale;
			nfloat newScale = 1;

			if (oldScale > 1)
				newScale = 1;
			else if (oldScale != nativeScale)
				newScale = nativeScale;
			else
				newScale = 1;

			ImageScroller.SetZoomScale(newScale, true);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			Refresh();
		}

		private void UpdateCaptionText()
		{
			if (string.Compare (CaptionTextField.Text, PhotoTossRest.Instance.CurrentImage.caption) != 0) {
				SendBtn.Enabled = false;
				CaptionTextField.Enabled = false;

				PhotoTossRest.Instance.SetImageCaption (PhotoTossRest.Instance.CurrentImage.id, CaptionTextField.Text, (newRec) => {
					if (newRec != null) {
						InvokeOnMainThread (() => {
							PhotoTossRest.Instance.CurrentImage.caption = newRec.caption;
							SendBtn.Enabled = true;
							CaptionTextField.Enabled = true;
						});
					}
				});
			}
		}




		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			var contr = this.NavigationController;
			if (contr != null) {
				contr.NavigationBar.Translucent = false;
				contr.NavigationBar.BarTintColor = UIColor.FromRGB (9, 171, 161);
				Title = "Tossed Image";

			}
		}



		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
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
			if (imageList != null)
				return;

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
					MaybeProcessBitmap(outerRec);
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
			string trimmedURL = theUrl.AbsoluteString.Substring (0, theUrl.AbsoluteString.LastIndexOf ("=s"));
			ImageLineageRecord curRec = imageList.Find (rec => rec.imageUrl.CompareTo(trimmedURL) == 0);


			if (curRec != null) {
				curRec.InitRecord (image);

				InvokeOnMainThread (() => {
					if (!scrollInited) {

						CGRect bounds = new CGRect (new CGPoint (0, 0), image.Size);
						frameView.Frame = bounds;
						frameView.Bounds = bounds;
						ImageScroller.MaximumZoomScale = 3f;
						ImageScroller.MinimumZoomScale = .1f;
						ImageScroller.ViewForZoomingInScrollView += (UIScrollView sv) => {
							return frameView;
						};

						ImageScroller.ContentSize = image.Size;
						CGSize scrollSize = ImageScroller.Bounds.Size;

						nfloat hScale = scrollSize.Width / image.Size.Width;
						nfloat vScale = scrollSize.Height / image.Size.Height;
						nativeScale = hScale < vScale ? hScale : vScale;// (nfloat)Math.Min(hScale, vScale);
						// compute initial scale
						ImageScroller.SetZoomScale (nativeScale, false);
						ImageScroller.SetContentOffset (CGPoint.Empty, false);
						scrollInited = true;

					}

					UpdatePoints ();

				});

			}
		}

		private void MaybeProcessBitmap(ImageLineageRecord outerRec) {
			ImageLineageRecord innerRec = outerRec.prevRec;
			if (outerRec.needsMapProcessed && (innerRec != null) && (innerRec.cachedMap != null) && (innerRec.innerSmall != null)) {
				CGRect bounds = new CGRect (CGPoint.Empty, outerRec.cachedMap.Size);

				isInnerSet = isOuterSet = false;
				SetOuterImage (outerRec);
				SetInnerImage (innerRec);
				frameView.Frame = bounds;
				frameView.Bounds = bounds;
				frameView.Layer.Transform = CATransform3D.Identity;

				outerImage.Layer.AnchorPoint = CGPoint.Empty;
				outerImage.Layer.Position = CGPoint.Empty;
				outerImage.Layer.Transform = CATransform3D.Identity;


				// set inner tranform
				CATransform3D innerMatrix = ImageLineageRecord.RectToQuad (new CGRect (CGPoint.Empty, new CGSize(innerRec.cachedMap.Size.Width, innerRec.cachedMap.Size.Height)),
					innerRec.innerSmall.topleft, innerRec.innerSmall.topright, innerRec.innerSmall.bottomleft, innerRec.innerSmall.bottomright);

				innerImage.Layer.AnchorPoint = CGPoint.Empty;
				innerImage.Layer.Position = CGPoint.Empty;
				innerImage.Layer.Transform = innerMatrix;

				UIGraphics.BeginImageContextWithOptions (bounds.Size, false, 0f);
				//frameView.DrawViewHierarchy (frameView.Frame, false);
				frameView.Layer.RenderInContext (UIGraphics.GetCurrentContext ());
				UIImage newImage = UIGraphics.GetImageFromCurrentImageContext ();
				UIGraphics.EndImageContext ();
				outerRec.cachedMap = newImage;

				outerRec.needsMapProcessed = false;
				isInnerSet = isOuterSet = false;

				// reset the scroll frame
				int curInnerFrame = (int)Math.Truncate (curProgress);
				ImageLineageRecord frameRec = imageList [curInnerFrame];
				SetScrollToFrame (frameRec);
			}

		}

		private void SetScrollToFrame(ImageLineageRecord innerRec)
		{
			if ((innerRec != null) && (innerRec.innerLarge != null)) {
				CGRect bounds = new CGRect (CGPoint.Empty, new CGSize (innerRec.innerLarge.bottomright.x, innerRec.innerLarge.bottomright.y));
				frameView.Frame = bounds;
				frameView.Bounds = bounds;
				ImageScroller.ContentSize = bounds.Size;
				CGSize scrollSize = ImageScroller.Bounds.Size;

				nfloat hScale = scrollSize.Width / bounds.Size.Width;
				nfloat vScale = scrollSize.Height / bounds.Size.Height;
				nativeScale = hScale < vScale ? hScale : vScale;// (nfloat)Math.Min(hScale, vScale);
				// compute initial scale
				ImageScroller.SetZoomScale (nativeScale, false);
				ImageScroller.SetContentOffset (CGPoint.Empty, false);

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
				SetScrollToFrame (innerRec);
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



	}
}

