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
using Android.Graphics;
using PhotoToss.Core;
using Android.Support.V4.App;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Xamarin.Facebook.AppEvents;
using Android.Content.PM;

namespace PhotoToss.AndroidApp
{
    public class SignInFragment : Android.Support.V4.App.Fragment
    {
        private ProgressDialog progressDlg;
		ICallbackManager callbackManager;
		ProfilePictureView profilePictureView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.SignInLayout, container, false);
			FacebookSdk.SdkInitialize (this.Activity.ApplicationContext);
			callbackManager = CallbackManagerFactory.Create ();

            view.FindViewById<TextView>(Resource.Id.textView1).SetTypeface(MainActivity.bodyFace, TypefaceStyle.Normal);
			profilePictureView = view.FindViewById <ProfilePictureView> (Resource.Id.profilePicture);
           
			var loginCallback = new FacebookCallback<LoginResult> {
				HandleSuccess = loginResult => {
					UpdateUI ();
				},
				HandleCancel = () => {
						ShowAlert (
							GetString (Resource.String.cancelled),
							GetString (Resource.String.permission_not_granted));


					UpdateUI ();                        
				},
				HandleError = loginError => {
					if (loginError is FacebookAuthorizationException) {
						ShowAlert (
							GetString (Resource.String.cancelled),
							GetString (Resource.String.permission_not_granted));
					}
					UpdateUI ();
				}
			};

			LoginManager.Instance.RegisterCallback (callbackManager, loginCallback);
			UpdateUI ();

            return view;

        }

		private void UpdateUI ()
		{
			var enableButtons = AccessToken.CurrentAccessToken != null;


			var profile = Profile.CurrentProfile;

			if (enableButtons && profile != null) {
				profilePictureView.ProfileId = profile.Id;
				//((FirstRunActivity)Activity).FinishSignin();
			} else {
				profilePictureView.ProfileId = null;
			}

		
		}
       
		public override void OnResume()
		{
			base.OnResume();
			AppEventsLogger.ActivateApp (this.Activity);
		}

		public override void OnPause()
		{
			base.OnPause();
			AppEventsLogger.DeactivateApp (this.Activity);
		}

		void ShowAlert (string title, string msg, string buttonText = null)
		{
			new AlertDialog.Builder (this.Activity)
				.SetTitle (title)
				.SetMessage (msg)
				.SetPositiveButton (buttonText, (s2, e2) => { })
				.Show ();
		}



       
        public override void OnStop()
        {
			if (progressDlg != null)
				progressDlg.Dismiss();
            base.OnStop();
        }

		public override void OnActivityResult (int requestCode, int resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
		}



		class FacebookCallback<TResult> : Java.Lang.Object, IFacebookCallback where TResult : Java.Lang.Object
		{
			public Action HandleCancel { get; set; }
			public Action<FacebookException> HandleError { get; set; }
			public Action<TResult> HandleSuccess { get; set; }

			public void OnCancel ()
			{
				var c = HandleCancel;
				if (c != null)
					c ();
			}

			public void OnError (FacebookException error)
			{
				var c = HandleError;
				if (c != null)
					c (error);
			}

			public void OnSuccess (Java.Lang.Object result)
			{
				var c = HandleSuccess;
				if (c != null)
					c (result.JavaCast<TResult> ());
			}
		}

    }
}