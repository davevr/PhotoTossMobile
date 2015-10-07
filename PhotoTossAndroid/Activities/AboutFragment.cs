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

namespace PhotoToss.AndroidApp
{
	public class AboutFragment : Android.Support.V4.App.Fragment
	{

		public MainActivity MainPage { get; set; }
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.AboutFragment, container, false);

			var feedbackBtn = view.FindViewById<Button> (Resource.Id.reportBtn);
			var reviewBtn = view.FindViewById<Button> (Resource.Id.reviewBtn);
			var shareBtn = view.FindViewById<Button> (Resource.Id.shareBtn);

			feedbackBtn.Click += DoSendFeedback;
			reviewBtn.Click += DoSendReview;
			shareBtn.Click += DoShareApp;


			return view;
		}

		public void Refresh()
		{

		}

		private void DoSendFeedback(object o, EventArgs a)
		{
			var email = new Intent (Android.Content.Intent.ActionSend);
			email.PutExtra (Android.Content.Intent.ExtraEmail,
				new string[]{"phototoss@eweware.com"} );

			email.PutExtra (Android.Content.Intent.ExtraSubject, "Thoughts on PhotoToss");

			email.PutExtra (Android.Content.Intent.ExtraText,
				"Here are some thoughts on PhotoToss");
			email.SetType ("message/rfc822");

			StartActivity (email);

		}

		private void DoSendReview(object o, EventArgs a)
		{
			Android.Net.Uri uri = Android.Net.Uri.Parse ("market://details?id=" + this.Activity.PackageName);
			Intent goToMarket = new Intent (Intent.ActionView, uri);
			this.Activity.Intent.AddFlags (ActivityFlags.NoHistory |
			ActivityFlags.ClearWhenTaskReset |
			ActivityFlags.MultipleTask);

			try {
				this.Activity.StartActivity (goToMarket);
			} catch (Exception e) {
				uri = Android.Net.Uri.Parse ("http://play.google.com/store/apps/details?id=" + this.Activity.PackageName);
				Intent goToWeb = new Intent (Intent.ActionView, uri);
				StartActivity (goToWeb);
			}
		}
			

		private void DoShareApp(object o, EventArgs a)
		{
			var shareIntent = new Intent(Intent.ActionSend);
			shareIntent.SetType("text/plain");
			this.Activity.Intent.AddFlags(ActivityFlags.ClearWhenTaskReset);
			shareIntent.PutExtra(Intent.ExtraTitle, "PhotoToss:  physically mediated photo sharing!");
			shareIntent.PutExtra(Intent.ExtraText, "http://www.phototoss.com");

			StartActivity(Intent.CreateChooser(shareIntent, "Share PhotoToss with friends!"));

		}
	}
}