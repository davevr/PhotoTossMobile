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

namespace PhotoToss.AndroidApp
{
    public class BrowseFragment : Android.Support.V4.App.Fragment
    {
		private ListView leaderList;
		private LeaderBoardAdapter adapter;

        public MainActivity MainPage { get; set; }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.BrowseFragment, container, false);
			leaderList = view.FindViewById<ListView> (Resource.Id.leaderList);
			adapter = new LeaderBoardAdapter (this.Activity, new List<PhotoRecord> ());
			leaderList.Adapter = adapter;
			Refresh ();

            return view;
        }

        public void Refresh()
        {
			PhotoTossRest.Instance.GetGlobalStats ((leaders) => {

				UpdateStats(leaders);
			});
        }

		private void UpdateStats(List<PhotoRecord> leaders)
		{
			if (leaders != null) {
				Activity.RunOnUiThread (() => {
					adapter.allItems = leaders;
					adapter.NotifyDataSetChanged ();
					leaderList.InvalidateViews ();
				});
			}
		}
    }

	public class LeaderBoardAdapter : BaseAdapter<PhotoRecord> {
		public List<PhotoRecord>	allItems;
		Activity context;

		public LeaderBoardAdapter(Activity context, List<PhotoRecord> theItems) : base() {
			this.context = context;
			this.allItems = theItems;
		}
		public override long GetItemId(int position)
		{
			return position;
		}
		public override PhotoRecord this[int position] {  
			get { return allItems[position]; }
		}
		public override int Count {
			get { return allItems.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) {
				view = context.LayoutInflater.Inflate (Resource.Layout.LeaderboardCell, null);
			}
			var rankView = view.FindViewById<TextView> (Resource.Id.rankItem);
			var imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			var countView = view.FindViewById<TextView> (Resource.Id.sharedCountText);
			var userImageView = view.FindViewById<ImageView> (Resource.Id.userImageView);
			var userNameView = view.FindViewById<TextView> (Resource.Id.userNameText);
			PhotoRecord curItem = allItems [position];
			string imageUrl = PhotoTossRest.Instance.GetUserProfileImage(curItem.ownername);

			rankView.Text = string.Format ("{0}", position + 1);
			Koush.UrlImageViewHelper.SetUrlDrawable (imageView, curItem.imageUrl + "=s128-c", Resource.Drawable.ic_camera);
			countView.Text = string.Format ("shared {0} times", curItem.totalshares);
			Koush.UrlImageViewHelper.SetUrlDrawable (userImageView, imageUrl, Resource.Drawable.unknown_octopus);
			userNameView.Text = "created by ";

			return view;
		}
	}
}