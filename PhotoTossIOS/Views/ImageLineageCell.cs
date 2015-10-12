
using System;

using Foundation;
using UIKit;
using SDWebImage;
using PhotoToss.Core;
using CoreGraphics;

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

			if (!String.IsNullOrEmpty (curItem.catchUrl)) {
				// caught image
				imageUrl = curItem.catchUrl;
				DateTime photoDate = curItem.received.ToLocalTime();
				dateString = string.Format("caught {0} {1}", photoDate.ToShortDateString(), photoDate.ToShortTimeString());
				userUrl = PhotoTossRest.Instance.GetUserProfileImage(curItem.ownername);

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
				url: new NSUrl (imageUrl + "=s1024"), 
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

	}
}

