
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
		ImageView imageView;
		public static int itemWidth = 320;
		Android.Util.DisplayMetrics	metrics;


		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View fragment = inflater.Inflate(Resource.Layout.ImageViewDetailFragment, null);

			PhotoRecord curRec = PhotoTossRest.Instance.CurrentImage;
			metrics = Resources.DisplayMetrics;
			int screenWidth = metrics.WidthPixels;
			int screenHeight = metrics.HeightPixels;
			itemWidth = Math.Max (screenWidth, screenHeight);

			imageView = fragment.FindViewById<ImageView>(Resource.Id.imageView);
			Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curRec.imageUrl + "=s" + itemWidth.ToString(), Resource.Drawable.ic_camera, this);

			imageView.Click += (object sender, EventArgs e) => 
			{
				//OpenImageFullScreen();
			};


			return fragment;
		}

		public void OnLoaded(ImageView theImage, Bitmap theBitmap, string theURL, bool p4)
		{
			var attacher = new  PhotoViewAttacher(imageView);
			attacher.MaximumScale = 5;
			attacher.Update();
		}

		private void OpenImageFullScreen()
		{
			this.Activity.StartActivity (typeof(FullScreenImageView));

		}

			
		public void Update()
		{

		}

	



	}


}

