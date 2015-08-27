using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net;
using Android.Locations;
using PhotoToss.Core;
using Android.Graphics;
using Android.Media;

using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace PhotoToss.AndroidApp
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class UploadActivity : Activity
    {
        private ImageView imageView;
        private EditText captionText;
        private TextView tagField;
        private EditText newTagText;
        private Button addTagBtn;
        private Button uploadBtn;
		private int MAX_IMAGE_SIZE = 2048;
		private Bitmap scaledBitmap;
		private ProgressDialog progressDlg;

        protected override void OnCreate(Bundle bundle)
        {
			RequestWindowFeature (WindowFeatures.NoTitle);
			Window.SetFlags (WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UploadActivity);

            imageView = FindViewById<ImageView>(Resource.Id.imageView);
            captionText = FindViewById<EditText>(Resource.Id.captionText);
            tagField = FindViewById<TextView>(Resource.Id.tagsField);
            newTagText = FindViewById<EditText>(Resource.Id.NewTagText);
            addTagBtn = FindViewById<Button>(Resource.Id.AddTagBtn);
            uploadBtn = FindViewById<Button>(Resource.Id.UploadBtn);

			scaledBitmap = BitmapHelper.LoadAndResizeBitmap (MainActivity._file.AbsolutePath, MAX_IMAGE_SIZE);
			imageView.SetImageBitmap (scaledBitmap);


            uploadBtn.Click += uploadBtn_Click;

			progressDlg = new ProgressDialog(this);
			progressDlg.SetProgressStyle(ProgressDialogStyle.Spinner);
        }

		protected override void OnStop ()
		{
			progressDlg.Dismiss ();
			base.OnStop ();
		}

        void uploadBtn_Click(object sender, EventArgs e)
        {
			progressDlg.SetMessage("uploading image...");
			progressDlg.Show();

			using (System.IO.MemoryStream photoStream = new System.IO.MemoryStream ()) {
				scaledBitmap.Compress (Bitmap.CompressFormat.Jpeg, 90, photoStream);
				photoStream.Flush ();

				string caption = captionText.Text;
				double longitude = MainActivity._lastLocation.Longitude;
				double latitude = MainActivity._lastLocation.Latitude;


				PhotoTossRest.Instance.UploadImage (photoStream, caption, longitude, latitude, (newRec) => {

					if (newRec != null) {
                        // now we upload a thumbnail immediately
                        PhotoTossRest.Instance.GetUploadURL((uploadStr) =>
                            {
                                Bitmap thumbnail = ThumbnailUtils.ExtractThumbnail(scaledBitmap, 64, 64);

                                using (System.IO.MemoryStream thumbStream = new System.IO.MemoryStream())
                                {
                                    thumbnail.Compress(Bitmap.CompressFormat.Png, 100, thumbStream);
                                    thumbStream.Flush();

                                    PhotoTossRest.Instance.UploadImageThumb(thumbStream, newRec.id, (theStr) =>
                                        {
                                            if (!String.IsNullOrEmpty(theStr))
                                                newRec.thumbnailurl = theStr;
                                            MainActivity._file.Delete();
                                            MainActivity._uploadPhotoRecord = newRec;
                                            SetResult(Result.Ok, new Intent());
                                            Finish();
                                        });
                                }
                            });
					} else {
						RunOnUiThread (() => {
							progressDlg.Hide();
							Toast.MakeText (this, "Image upload failed, please try again", ToastLength.Long).Show ();
						});
					}
				});
			}
        }
    }
}