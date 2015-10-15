
using System;

using Foundation;
using UIKit;
using SDWebImage;
using PhotoToss.Core;
using CoreGraphics;
using CoreAnimation;

namespace PhotoToss.iOSApp
{
	public partial class ImageLineageCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("ImageLineageCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("ImageLineageCell");
		private PhotoRecord photoRecord;
		private UIImageView newImageView;
		private nfloat nativeScale = 1;
		public UIImage CurrentImage { get; set; }
		private bool drawOnImage;

		public ImageLineageCell (IntPtr handle) : base (handle)
		{
		}

		public static ImageLineageCell Create ()
		{
			var newCell =  (ImageLineageCell)Nib.Instantiate (null, null) [0];

			return newCell;
		}

	

		public void ConformToRecord (PhotoRecord curItem, int index)
		{
			photoRecord = curItem;
			string imageUrl, dateString, userUrl;
			drawOnImage = false;

			if (!String.IsNullOrEmpty (curItem.catchUrl)) {
				// caught image
				imageUrl = curItem.catchUrl;
				DateTime photoDate = curItem.received.ToLocalTime();
				dateString = string.Format("caught {0} {1}", photoDate.ToShortDateString(), photoDate.ToShortTimeString());
				userUrl = PhotoTossRest.Instance.GetUserProfileImage(curItem.ownername);
				drawOnImage = true;

			}
			else {
				// original image
				imageUrl = curItem.imageUrl;
				DateTime photoDate = curItem.created.ToLocalTime();
				dateString = string.Format("originally taken {0} {1}", photoDate.ToShortDateString(), photoDate.ToShortTimeString());
				userUrl = PhotoTossRest.Instance.GetUserProfileImage(curItem.ownername);
			}

			if (newImageView == null) {
				InitImageView ();
			}

			SDWebImageManager.SharedManager.Download (
				url: new NSUrl (imageUrl + "=s2048"), 
				options: SDWebImageOptions.CacheMemoryOnly,
				progressHandler: (recievedSize, expectedSize) => {
					// Track progress...
				},
				completedHandler: ImageLoadComplete);

			//dateString = string.Format ("{0:F} miles from home", curItem.DistanceFromHome);
			TossDateLabel.Text = dateString;
			UserImageView.SetImage(new NSUrl(userUrl), UIImage.FromBundle("unknownperson"));

			if (curItem.ownerid == PhotoTossRest.Instance.CurrentUser.id) {
				// I can edit this image
				CaptionField.Enabled = true;
				CaptionField.Hidden = false;
				SendBtn.Hidden = false;
				CaptionLabel.Hidden = true;
				CaptionField.Text = curItem.caption;
				CaptionField.Placeholder = "enter a caption";
			} else if (!string.IsNullOrEmpty (curItem.caption)) {
				// just static text
				CaptionField.Hidden = true;
				SendBtn.Hidden = true;
				CaptionLabel.Hidden = false;
				CaptionLabel.Text = curItem.caption;
			} else {
				CaptionField.Hidden = true;
				SendBtn.Hidden = true;
				CaptionLabel.Hidden = true;
			}
		}
			

		private void InitImageView()
		{
			UITapGestureRecognizer doubletap = new UITapGestureRecognizer(OnDoubleTap) {
				NumberOfTapsRequired = 2 // double tap
			};
			ImageScroller.AddGestureRecognizer(doubletap); 
			SendBtn.TouchUpInside += (object sender, EventArgs e) => 
			{
				UpdateCaptionText();
			};

		}

		private void UpdateCaptionText()
		{
			if (photoRecord.ownerid == PhotoTossRest.Instance.CurrentUser.id) {
				var newText = CaptionField.Text;
				if (newText.CompareTo (photoRecord.caption) != 0) {
					SendBtn.Enabled = false;
					CaptionField.Enabled = false;
					PhotoTossRest.Instance.SetImageCaption (photoRecord.id, newText, (theRec) => {
						InvokeOnMainThread(() => {
							photoRecord.caption = theRec.caption;
							CaptionField.Text = theRec.caption;
							SendBtn.Enabled = true;
							CaptionField.Enabled = true;
						});

					});
				}
			}
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

		private void ImageLoadComplete(UIImage image, NSError theErr, SDImageCacheType cacheType, bool finished, NSUrl theUrl )
		{
			System.Console.Out.WriteLine("image loaded");
			InvokeOnMainThread (() => {
				//ResizeImage();
				CurrentImage = image;

				CGRect bounds = new CGRect(new CGPoint(0,0), image.Size);
				if (newImageView == null) {
					newImageView = new UIImageView(bounds);

					newImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
					ImageScroller.AddSubview(newImageView);
					ImageScroller.ViewForZoomingInScrollView += (UIScrollView sv) => {
						return newImageView;
					};
				}

				if (drawOnImage && (photoRecord.barcodelocation != null)) {
					newImageView.Image = AlteredImage(image);

				}
				else
					newImageView.Image = image;
				ImageScroller.ContentSize = image.Size;
				//Set the zoom properties:
				ImageScroller.MaximumZoomScale = 3f;
				ImageScroller.MinimumZoomScale = .1f;
				CGSize scrollSize = ImageScroller.Bounds.Size;

				nfloat hScale = scrollSize.Width / image.Size.Width;
				nfloat vScale = scrollSize.Height / image.Size.Height;
				nativeScale = hScale < vScale ? hScale : vScale;// (nfloat)Math.Min(hScale, vScale);
				// compute initial scale
				ImageScroller.SetZoomScale(nativeScale, true);
			});
		}

		private UIImage AlteredImage(UIImage sourceImage)
		{
			BarcodeLocation barLoc = photoRecord.barcodelocation;
			UIGraphics.BeginImageContext(sourceImage.Size);
			BarcodePoint bottomLeftExt;
			BarcodePoint bottomRightExt; 
			bottomLeftExt.x = barLoc.topleft.x + (barLoc.bottomleft.x - barLoc.topleft.x) * 2;
			bottomLeftExt.y = barLoc.topleft.y + (barLoc.bottomleft.y - barLoc.topleft.y) * 2;
			bottomRightExt.x = barLoc.topright.x + (barLoc.bottomright.x - barLoc.topright.x) * 2;
			bottomRightExt.y = barLoc.topright.y + (barLoc.bottomright.y - barLoc.topright.y) * 2;
			sourceImage.Draw(CGPoint.Empty);

			CGContext ctx = UIGraphics.GetCurrentContext();

			//ctx.ConcatCTM (transform);
			sourceImage.Draw(CGPoint.Empty);

			ctx.SetStrokeColor(UIColor.Red.CGColor);
			ctx.SetLineWidth(2);
			ctx.MoveTo(barLoc.topleft.x, barLoc.topleft.y);
			ctx.AddLineToPoint(barLoc.topright.x, barLoc.topright.y);
			ctx.StrokePath();

			ctx.SetStrokeColor(UIColor.Green.CGColor);
			ctx.MoveTo(barLoc.topright.x, barLoc.topright.y);
			ctx.AddLineToPoint(bottomRightExt.x, bottomRightExt.y);
			ctx.StrokePath();

			ctx.SetStrokeColor(UIColor.Blue.CGColor);
			ctx.MoveTo(bottomRightExt.x, bottomRightExt.y);
			ctx.AddLineToPoint(bottomLeftExt.x, bottomLeftExt.y);
			ctx.StrokePath();

			ctx.SetStrokeColor(UIColor.Yellow.CGColor);
			ctx.MoveTo(bottomLeftExt.x, bottomLeftExt.y);
			ctx.AddLineToPoint(barLoc.topleft.x, barLoc.topleft.y);
			ctx.StrokePath();




			UIImage retImage = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return retImage;

		}

		CATransform3D RectToQuad(CGRect rect, BarcodePoint topLeft, BarcodePoint topRight, BarcodePoint bottomLeft, BarcodePoint bottomRight)
		{
			return RectToQuad (rect, topLeft.x, topLeft.y, topRight.x, topRight.y, bottomLeft.x, bottomLeft.y, bottomRight.x, bottomRight.y);
		}


		CATransform3D RectToQuad(CGRect rect, CGPoint topLeft, CGPoint topRight, CGPoint bottomLeft, CGPoint bottomRight)
		{
			return RectToQuad (rect, topLeft.X, topLeft.Y, topRight.X, topRight.Y, bottomLeft.X, bottomLeft.Y, bottomRight.X, bottomRight.Y);
		}

		CATransform3D RectToQuad(CGRect rect, nfloat x1a, nfloat y1a, nfloat x2a, nfloat y2a, nfloat x3a, nfloat y3a, nfloat x4a, nfloat y4a)
		{
			nfloat X = rect.X;
			nfloat Y = rect.Y;
			nfloat W = rect.Width;
			nfloat H = rect.Height;

			nfloat y21 = y2a - y1a;
			nfloat y32 = y3a - y2a;
			nfloat y43 = y4a - y3a;
			nfloat y14 = y1a - y4a;
			nfloat y31 = y3a - y1a;
			nfloat y42 = y4a - y2a;

			nfloat a = -H*(x2a*x3a*y14 + x2a*x4a*y31 - x1a*x4a*y32 + x1a*x3a*y42);
			nfloat b = W*(x2a*x3a*y14 + x3a*x4a*y21 + x1a*x4a*y32 + x1a*x2a*y43);
			nfloat c = H*X*(x2a*x3a*y14 + x2a*x4a*y31 - x1a*x4a*y32 + x1a*x3a*y42) - H*W*x1a*(x4a*y32 - x3a*y42 + x2a*y43) - W*Y*(x2a*x3a*y14 + x3a*x4a*y21 + x1a*x4a*y32 + x1a*x2a*y43);

			nfloat d = H*(-x4a*y21*y3a + x2a*y1a*y43 - x1a*y2a*y43 - x3a*y1a*y4a + x3a*y2a*y4a);
			nfloat e = W*(x4a*y2a*y31 - x3a*y1a*y42 - x2a*y31*y4a + x1a*y3a*y42);
			nfloat f = -(W*(x4a*(Y*y2a*y31 + H*y1a*y32) - x3a*(H + Y)*y1a*y42 + H*x2a*y1a*y43 + x2a*Y*(y1a - y3a)*y4a + x1a*Y*y3a*(-y2a + y4a)) - H*X*(x4a*y21*y3a - x2a*y1a*y43 + x3a*(y1a - y2a)*y4a + x1a*y2a*(-y3a + y4a)));

			nfloat g = H*(x3a*y21 - x4a*y21 + (-x1a + x2a)*y43);
			nfloat h = W*(-x2a*y31 + x4a*y31 + (x1a - x3a)*y42);
			nfloat i = W*Y*(x2a*y31 - x4a*y31 - x1a*y42 + x3a*y42) + H*(X*(-(x3a*y21) + x4a*y21 + x1a*y43 - x2a*y43) + W*(-(x3a*y2a) + x4a*y2a + x2a*y3a - x4a*y3a - x2a*y4a + x3a*y4a));

			nfloat kEpsilon = (nfloat)0.0001;

			if(Math.Abs(i) < kEpsilon)
			{
				i = kEpsilon * (i > 0 ? (nfloat)1 : (nfloat)(-1));
			}

			CATransform3D transform = new CATransform3D ();

			transform.m11 = a/i;
			transform.m12 = d/i;
			transform.m13 = 0;
			transform.m14 = g/i;

			transform.m21 = b/i;
			transform.m22 = e/i;
			transform.m23 = 0;
			transform.m24 = h/i;

			transform.m31 = 0;
			transform.m32 = 0;
			transform.m33 = 1;
			transform.m34 = 0;

			transform.m41 = c/i;
			transform.m42 = f/i;
			transform.m43 = 0;
			transform.m44 = 1;
			return transform;
		}

	}
}

