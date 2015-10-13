
using System;

using Foundation;
using UIKit;
using PhotoToss.Core;
using SDWebImage;
using CoreGraphics;


namespace PhotoToss.iOSApp
{
	public partial class ImageDetailController : UIViewController
	{
		public UIImage CurrentImage { get; set; }
		private UIImageView newImageView { get; set;}
		private UIView activeView;
		private nfloat scroll_amount = 0.0f;    // amount to scroll 
		private nfloat bottom = 0.0f;           // bottom point
		private nfloat offset = 10.0f;          // extra offset
		private bool moveViewUp = false;  
		private NSObject hideObserver, showObserver;
		private nfloat nativeScale = 1;

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
			if (PhotoTossRest.Instance.CurrentImage != null) {
				SDWebImageManager.SharedManager.Download (
					url: new NSUrl (PhotoTossRest.Instance.CurrentImage.imageUrl + "=s2048"), 
					options: SDWebImageOptions.CacheMemoryOnly,
					progressHandler: (recievedSize, expectedSize) => {
						// Track progress...
					},
					completedHandler: ImageLoadComplete);


			}
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

		private void ImageLoadComplete(UIImage image, NSError theErr, SDImageCacheType cacheType, bool finished, NSUrl theUrl )
		{
			System.Console.Out.WriteLine("image loaded");
			InvokeOnMainThread (() => {
				//ResizeImage();
				CurrentImage = image;

				CGRect bounds = new CGRect(new CGPoint(0,0), image.Size);
				newImageView = new UIImageView(bounds);



				newImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
				ImageScroller.AddSubview(newImageView);
				newImageView.Image = image;
				ImageScroller.ContentSize = image.Size;
				//Set the zoom properties:
				ImageScroller.MaximumZoomScale = 3f;
				ImageScroller.MinimumZoomScale = .1f;
				ImageScroller.ViewForZoomingInScrollView += (UIScrollView sv) => {
					return newImageView;
				};
				CGSize scrollSize = ImageScroller.Bounds.Size;

				nfloat hScale = scrollSize.Width / image.Size.Width;
				nfloat vScale = scrollSize.Height / image.Size.Height;
				nativeScale = hScale < vScale ? hScale : vScale;// (nfloat)Math.Min(hScale, vScale);
				// compute initial scale
				ImageScroller.SetZoomScale(nativeScale, true);
			});
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			var contr = this.NavigationController;
			if (contr != null) {
				contr.NavigationBar.Translucent = false;
				contr.NavigationBar.BarTintColor = UIColor.FromRGB (9, 171, 161);
				contr.NavigationBar.TintColor = UIColor.FromRGB (213,88,2);
				Title = "Tossed Image";

			}
		}

		/*
		private void ResizeImage ()
		{
			CGSize imageSize = LargeImageView.Image.Size;
			nfloat scale = imageSize.Height / imageSize.Width;
			nfloat desiredHeight = LargeImageView.Bounds.Width * scale;
			ImageHeightConstraint.Constant = desiredHeight;
			ImageScroller.ContentSize = new CGSize( LargeImageView.Bounds.Width, desiredHeight);
		}
		*/

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			//ResizeImage ();
		}


	}
}

