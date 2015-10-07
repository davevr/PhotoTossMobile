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


namespace PhotoToss.AndroidApp
{
    public class ProfileFragment : Android.Support.V4.App.Fragment
    {
        public bool IsInitialSignIn = false;

        public MainActivity MainPage { get; set; }
        private ProfilePictureView profileImage;
        private TextView nicknameText;
		private TextView numTotalText;
		private TextView numOriginalsText;
		private TextView numCatchesText;
		private TextView numTossesText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.ProfileFragment, container, false);
            nicknameText = view.FindViewById<TextView>(Resource.Id.txt_nickname);
            profileImage = view.FindViewById<ProfilePictureView>(Resource.Id.profilePicture);
			numTotalText = view.FindViewById<TextView> (Resource.Id.numTotalText);
			numOriginalsText = view.FindViewById<TextView> (Resource.Id.numOriginalsText);
			numCatchesText = view.FindViewById<TextView> (Resource.Id.numCatchesText);
			numTossesText = view.FindViewById<TextView> (Resource.Id.numTossesText);
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

			// update stats
			PhotoTossRest.Instance.GetUserStats ((theStats) => {
				UpdateUserStats (theStats);
			});

        }

		public void UpdateUserStats(UserStatsRecord theStats)
		{
			if (theStats != null) {
				Activity.RunOnUiThread (() => {
					numTossesText.Text = String.Format ("{0}", theStats.numtosses);
					numCatchesText.Text = String.Format ("{0}", theStats.numcatches);
					numOriginalsText.Text = String.Format ("{0}", theStats.numoriginals);
					numTotalText.Text = String.Format ("{0}", theStats.numimages);
				});
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