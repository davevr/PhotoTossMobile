
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
			Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curRec.imageUrl + "=s2058", Resource.Drawable.ic_camera, this);


			captionText = fragment.FindViewById<EditText> (Resource.Id.turnText);
			sendBtn = fragment.FindViewById<Button> (Resource.Id.sendTurnBtn);

			sendBtn.Click += (object sender, EventArgs e) => {
				UpdateCaptionText();
			};
			captionText.Text = curRec.caption;

			return fragment;
		}

		public void OnLoaded(ImageView theImage, Bitmap theBitmap, string theURL, bool p4)
		{
			var attacher = new  PhotoViewAttacher(imageView);
			attacher.MaximumScale = 5;
			attacher.Update();
		}
			
		public void Update()
		{

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
	



	}


}

