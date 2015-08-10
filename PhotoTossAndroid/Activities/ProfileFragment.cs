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
using Android.Support.V4.App;
using Android.Provider;
using Android.Graphics;

using PhotoToss.Core;

using Java.IO;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace PhotoToss.AndroidApp
{
    public class ProfileFragment : Android.Support.V4.App.Fragment
    {
        public bool IsInitialSignIn = false;
        private ImageView profileImageView;

        public MainActivity MainPage { get; set; }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.ProfileFragment, container, false);
			var registerButton = view.FindViewById<Button> (Resource.Id.registerButton);
			var unregisterButton = view.FindViewById<Button> (Resource.Id.unregisterButton);
            var profileImgBtn = view.FindViewById<Button>(Resource.Id.btn_profileimage);
            profileImageView = view.FindViewById<ImageView>(Resource.Id.img_profileImage);
            var nicknameText = view.FindViewById<TextView>(Resource.Id.txt_nickname);
            var nicknameField = view.FindViewById<EditText>(Resource.Id.field_nickname);
            var finishBtn = view.FindViewById<Button>(Resource.Id.btn_finish);

			registerButton.Click += delegate {
				const string senders = "865065760693";
				var intent = new Intent("com.google.android.c2dm.intent.REGISTER");
				intent.SetPackage("com.google.android.gsf");
				intent.PutExtra("app", PendingIntent.GetBroadcast(MainPage, 0, new Intent(), 0));
				intent.PutExtra("userid", PhotoToss.Core.PhotoTossRest.Instance.CurrentUser.id.ToString());
				intent.PutExtra("sender", senders);
				MainPage.StartService(intent);
			};

			unregisterButton.Click += delegate {
				var intent = new Intent("com.google.android.c2dm.intent.UNREGISTER");
				intent.SetPackage("com.google.android.gsf");
				intent.PutExtra("app", PendingIntent.GetBroadcast(MainPage, 0, new Intent(), 0));
				MainPage.StartService(intent);
			};

            profileImgBtn.Click += delegate
            {
                Intent intent = new Intent(MediaStore.ActionImageCapture);

                MainActivity._file = new File(MainActivity._dir, String.Format("PhotoTossPhoto_{0}.jpg", Guid.NewGuid()));

                intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(MainActivity._file));

                MainPage.StartActivityForResult(intent, Utilities.PROFILEIMAGE_CAPTURE_EVENT);
            };

            finishBtn.Click += delegate
            {
                ((FirstRunActivity)Activity).FinishSignin();
            };

            ConfigureViewState();
            UpdateUserImage();

            return view;
        }

        public void Refresh()
        {

        }

        public void UpdateUserImage()
        {
            if (PhotoTossRest.Instance.CurrentUser != null)
            {
                string imageUrl = PhotoTossRest.Instance.CurrentUser.imageurl;

                if (!String.IsNullOrEmpty(imageUrl))
                {
                    Bitmap userBitMap = BitmapHelper.GetImageBitmapFromUrl(imageUrl);
                    profileImageView.SetImageDrawable(new CircleDrawable(userBitMap));
                }
                else profileImageView.SetImageResource(Resource.Drawable.unknown_octopus);
            }
        }

        private void ConfigureViewState()
        {
            if (IsInitialSignIn)
            {
                // show the skip button, hide management buttons

            }
            else
            {
                // hide the skip button - we are in the rotation.  
                // instead, show the management buttons
            }
        }
    }
}