using System;

using UIKit;
using Foundation;
using CoreGraphics;

namespace PhotoToss.iOSApp
{
	public static class UIImageHelper
	{
		private const int kMaxResolution = 1024;

		public static UIImage ImageFromUrl(string uri)
		{
			using(var url = new NSUrl(uri))
			{
				using(var data = NSData.FromUrl(url))
				{
					return UIImage.LoadFromData(data);
				}
			}
		}

		public static UIImage ScaleAndRotateImage(UIImage image)
		{
			CGImage imgRef = image.CGImage;
			return ScaleAndRotateImage (imgRef, image.Orientation);
		}

		public static UIImage ScaleAndRotateImage(CGImage imgRef, UIImageOrientation orient = UIImageOrientation.Up)
        {
            float width = imgRef.Width;
            float height = imgRef.Height;
            CGAffineTransform transform = CGAffineTransform.MakeIdentity();
            CGRect  bounds = new CGRect(0,0,width, height);

            if (width > kMaxResolution || height > kMaxResolution)
            {
                float ratio = width / height;

                if (ratio > 1)
                {
                    bounds.Width = kMaxResolution;
                    bounds.Height = bounds.Width / ratio;
                }
                else
                {
                    bounds.Height = kMaxResolution;
                    bounds.Width = bounds.Height * ratio;
                }
            }

            float scaleRatio = (float)bounds.Width / width;
            CGSize imageSize = new CGSize(width, height);
            nfloat boundHeight;

            switch (orient)
            {
                case UIImageOrientation.Up: //EXIF = 1
                    transform = CGAffineTransform.MakeIdentity();
                    break;

                case UIImageOrientation.UpMirrored: //EXIF = 2
                    transform = CGAffineTransform.MakeTranslation(imageSize.Width, 0.0f);
                    transform = CGAffineTransform.Scale(transform, -1.0f, 1.0f);
                    break;

                case UIImageOrientation.Down: //EXIF = 3
                    transform = CGAffineTransform.MakeTranslation(imageSize.Width, imageSize.Height);
                    transform = CGAffineTransform.Rotate(transform, (float)Math.PI);
                    break;

                case UIImageOrientation.DownMirrored: //EXIF = 4
                    transform = CGAffineTransform.MakeTranslation(0.0f, imageSize.Height);
                    transform = CGAffineTransform.Scale(transform, 1.0f, -1.0f);
                    break;

                case UIImageOrientation.LeftMirrored: //EXIF = 5
                    boundHeight = bounds.Height;
                    bounds.Height = bounds.Width;
                    bounds.Width = boundHeight;
                    transform = CGAffineTransform.MakeTranslation(imageSize.Height, imageSize.Width);
                    transform = CGAffineTransform.Scale(transform, -1.0f, 1.0f);
                    transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
                    break;

                case UIImageOrientation.Left: //EXIF = 6
                    boundHeight = bounds.Height;
                    bounds.Height = bounds.Width;
                    bounds.Width = boundHeight;
                    transform = CGAffineTransform.MakeTranslation(0.0f, imageSize.Width);
                    transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
                    break;

                case UIImageOrientation.RightMirrored: //EXIF = 7
                    boundHeight = bounds.Height;
                    bounds.Height = bounds.Width;
                    bounds.Width = boundHeight;
                    transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
                    transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
                    break;

                case UIImageOrientation.Right: //EXIF = 8
                    boundHeight = bounds.Height;
                    bounds.Height = bounds.Width;
                    bounds.Width = boundHeight;
                    transform = CGAffineTransform.MakeTranslation(imageSize.Height, 0.0f);
                    transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
                    break;
            }

            UIGraphics.BeginImageContext (bounds.Size);
            CGContext context = UIGraphics.GetCurrentContext ();

            if (orient == UIImageOrientation.Right || orient == UIImageOrientation.Left) 
            {
                context.ScaleCTM(-scaleRatio, scaleRatio);
                context.TranslateCTM (-height, 0.0f);
            }
            else 
            {
                context.ScaleCTM (scaleRatio, -scaleRatio);
                context.TranslateCTM (0.0f, -height);
            }

            context.ConcatCTM (transform);

            context.DrawImage (new CGRect(0,0, width, height), imgRef);
            UIImage imageCopy = UIGraphics.GetImageFromCurrentImageContext ();
            UIGraphics.EndImageContext ();

            return imageCopy;

        }
	}
}

