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
using Xamarin.Facebook.Login.Widget;
using PhotoToss.Core;
using Xamarin.Facebook;

using Java.IO;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace PhotoToss.AndroidApp
{
    public class ProfileFragment : Android.Support.V4.App.Fragment
    {
        public bool IsInitialSignIn = false;

        public MainActivity MainPage { get; set; }
        private ProfilePictureView profileImage;
        private TextView nicknameText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.ProfileFragment, container, false);
			var registerButton = view.FindViewById<Button> (Resource.Id.registerButton);
			var unregisterButton = view.FindViewById<Button> (Resource.Id.unregisterButton);
            nicknameText = view.FindViewById<TextView>(Resource.Id.txt_nickname);
            profileImage = view.FindViewById<ProfilePictureView>(Resource.Id.profilePicture);

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

            Refresh();

            return view;
        }

        public void Refresh()
        {
            if (Profile.CurrentProfile != null)
            {
                profileImage.ProfileId = Profile.CurrentProfile.Id;
                nicknameText.Text = Profile.CurrentProfile.Name;
            }
            else
            {
                profileImage.ProfileId = null;
                nicknameText.Text = "";
            }

        }

        public override void OnHiddenChanged(bool hidden)
        {
            base.OnHiddenChanged(hidden);
            if (!hidden)
                Refresh();
        }
    }
}