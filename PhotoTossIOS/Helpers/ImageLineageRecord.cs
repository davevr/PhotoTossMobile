using System;
using PhotoToss.Core;
using UIKit;
using CoreGraphics;
using CoreAnimation;

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

		

					CATransform3D innerMatrix = RectToQuad (new CGRect (new CGPoint (0, 0), new CGSize(outerMap.Size.Width, outerMap.Size.Width)),
						                            barLoc.topleft, barLoc.topright, barLoc.bottomleft, barLoc.bottomright);

					CGPoint oldPt1 = new CGPoint (0, 0);
					CGPoint oldPt2 = new CGPoint (outerMap.Size.Width, 0);
					CGPoint oldPt3 = new CGPoint (0, scaledHeight);
					CGPoint oldPt4 = new CGPoint (outerMap.Size.Width, scaledHeight);
					CGAffineTransform ptTrans = CATransform3D.GetAffine (innerMatrix);

					CGPoint newPt1 = ptTrans.TransformPoint (oldPt1);
					CGPoint newPt2 = ptTrans.TransformPoint (oldPt2);
					CGPoint newPt3 = ptTrans.TransformPoint (oldPt3);
					CGPoint newPt4 = ptTrans.TransformPoint (oldPt4);

					innerSmall = new BarcodeLocation ();
					innerSmall.topleft = new BarcodePoint ((float)newPt1.X, (float)newPt1.Y);
					innerSmall.topright = new BarcodePoint ((float)newPt2.X, (float)newPt2.Y);
					innerSmall.bottomleft = new BarcodePoint ((float)newPt3.X, (float)newPt3.Y);
					innerSmall.bottomright = new BarcodePoint ((float)newPt4.X, (float)newPt4.Y);

					CATransform3D outerMatrix = innerMatrix.Invert (innerMatrix);
					oldPt1 = new CGPoint (0, 0);
					oldPt2 = new CGPoint (nextRec.cachedMap.Size.Width, 0);
					oldPt3 = new CGPoint (0, nextRec.cachedMap.Size.Height);
					oldPt4 = new CGPoint (nextRec.cachedMap.Size.Width, nextRec.cachedMap.Size.Height);
					ptTrans = CATransform3D.GetAffine (outerMatrix);

					newPt1 = ptTrans.TransformPoint (oldPt1);
					newPt2 = ptTrans.TransformPoint (oldPt2);
					newPt3 = ptTrans.TransformPoint (oldPt3);
					newPt4 = ptTrans.TransformPoint (oldPt4);

					nextRec.outerLarge = new BarcodeLocation ();
					nextRec.outerLarge.topleft = new BarcodePoint ((float)newPt1.X, (float)newPt1.Y);
					nextRec.outerLarge.topright = new BarcodePoint ((float)newPt2.X, (float)newPt2.Y);
					nextRec.outerLarge.bottomleft = new BarcodePoint ((float)newPt3.X, (float)newPt3.Y);
					nextRec.outerLarge.bottomright = new BarcodePoint ((float)newPt4.X, (float)newPt4.Y);


				}

			}
		}

		public static CATransform3D RectToQuad(CGRect rect, BarcodePoint topLeft, BarcodePoint topRight, BarcodePoint bottomLeft, BarcodePoint bottomRight)
		{
			return RectToQuad (rect, topLeft.x, topLeft.y, topRight.x, topRight.y, bottomLeft.x, bottomLeft.y, bottomRight.x, bottomRight.y);
		}


		public static CATransform3D RectToQuad(CGRect rect, CGPoint topLeft, CGPoint topRight, CGPoint bottomLeft, CGPoint bottomRight)
		{
			return RectToQuad (rect, topLeft.X, topLeft.Y, topRight.X, topRight.Y, bottomLeft.X, bottomLeft.Y, bottomRight.X, bottomRight.Y);
		}

		public static CATransform3D RectToQuad(CGRect rect, nfloat x1a, nfloat y1a, nfloat x2a, nfloat y2a, nfloat x3a, nfloat y3a, nfloat x4a, nfloat y4a)
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

